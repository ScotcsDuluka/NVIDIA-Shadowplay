// nvhook.cpp — NVIDIA Share in-game hook (native DLL, C++).
//
// Injected into a CONSENTED whitelisted game (Minecraft Dungeons = first
// target). Hooks IDXGISwapChain::Present (vtable swap) and draws the osc
// overlay frame — streamed by NVIDIA Share.exe through the shared memory
// "NVIDIA_Share_Overlay_Frame_v1" (BGRA32, header: magic,w,h,frameId,
// overlayVisible @ +16, live counter @ +20, engine PID @ +24,
// controller port @ +28, controller secret @ +32, data @ +64) — as an
// alpha-blended fullscreen quad INSIDE the game's own Present.
//
// Toggle: the overlay draw follows header.overlayVisible (written by the
// controller when Alt+Z opens/closes the page).
//
// Input: while the overlay is visible, a polling thread reads the cursor
// and left button and POSTs JSON events to the controller
// (/ShadowPlay/v.1.0/Hook/Input, cookie from header +32) — the engine
// replays them as synthetic DOM events on the osc page. Mouse ONLY by
// design: the keyboard stays with the game (anti keylogger posture).
//
// Build (x64): cl /LD /EHsc /O2 nvhook.cpp /link user32.lib gdi32.lib
//   dxgi.lib d3d11.lib d3dcompiler.lib winhttp.lib

#include <windows.h>
#include <d3d11.h>
#include <dxgi.h>
#include <dxgi1_6.h>
#include <d3dcompiler.h>
#include <cstdint>
#include <cstdio>
#include <d3d12.h>
#include <cstdarg>
#include <winhttp.h>

#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "winhttp.lib")

static void NLog(const char *fmt, ...) {
    FILE *f = nullptr;
    fopen_s(&f, "C:/My Project/NVIDIA-Shadowplay/GameHook/NativeHook/nvhook.log", "a");
    if (!f) return;
    SYSTEMTIME st; GetLocalTime(&st);
    fprintf(f, "[%02d:%02d:%03d] ", st.wHour, st.wMinute, st.wMilliseconds);
    va_list a; va_start(a, fmt);
    vfprintf(f, fmt, a);
    va_end(a);
    fprintf(f, "\n");
    fclose(f);
}

static HMODULE g_self = nullptr;
static volatile LONG g_live = 0;          // dll init done
static volatile LONG g_presentHooked = 0; // vtable patched
static volatile LONG g_apiChecked = 0;
static HWND g_gameWindow = nullptr;

// original Present (saved from the swapchain vtable)
typedef HRESULT(STDMETHODCALLTYPE *Present_t)(void *swapChain, UINT sync, UINT flags);
static Present_t g_origPresent = nullptr;
static void **g_vtableSlot = nullptr;      // address of the vtable entry we patched
static DWORD g_vtableOldProtect = 0;

// per-swapchain resources (MVP: single swap chain)
struct HookRes {
    ID3D11Device *device = nullptr;
    ID3D11DeviceContext *ctx = nullptr;
    ID3D11Texture2D *tex = nullptr;       // BGRA frame from the MMF
    ID3D11ShaderResourceView *srv = nullptr;
    ID3D11BlendState *blend = nullptr;
    ID3D11Buffer *vb = nullptr;
    ID3D11VertexShader *vs = nullptr;
    ID3D11PixelShader *ps = nullptr;
    ID3D11InputLayout *layout = nullptr;
    ID3D11RasterizerState *raster = nullptr;
    ID3D11SamplerState *sampler = nullptr;
    UINT frameW = 0, frameH = 0;
    int lastFrameId = -1;
    bool ready = false;
};
static HookRes g_res;

// ── shared memory frame ────────────────────────────────────
static HANDLE g_mmf = nullptr;
static const wchar_t *MMF_NAME = L"NVIDIA_Share_Overlay_Frame_v1";
static const int HEADER_BYTES = 64;
static const int MAGIC = 0x4C50534E;      // "NSPL"
static DWORD g_cachedEpoch = 0;           // header +56 (engine boot stamp)
static DWORD g_lastSectionCheck = 0;

// A dead engine's section survives as an UNNAMED orphan as long as we hold
// a handle — OpenFileMapping under the same name then resolves to the NEW
// engine's section. Poll the name and switch when the epoch (@+56) differs.
static void ReopenIfEngineRestarted()
{
    DWORD now = GetTickCount();
    if (g_mmf && now - g_lastSectionCheck < 500) return;
    g_lastSectionCheck = now;
    HANDLE h2 = OpenFileMappingW(FILE_MAP_READ | FILE_MAP_WRITE, FALSE, MMF_NAME);
    if (!h2) return;
    if (h2 == g_mmf) { CloseHandle(h2); return; }
    auto *nv = (const uint8_t *)MapViewOfFile(h2, FILE_MAP_READ, 0, 0, 0);
    if (!nv) { CloseHandle(h2); return; }
    DWORD ep = *(const DWORD *)(nv + 56);
    UnmapViewOfFile(nv);
    if (ep != 0 && ep != g_cachedEpoch) {
        if (g_mmf) CloseHandle(g_mmf);
        g_mmf = h2;
        g_cachedEpoch = ep;
        NLog("switched to fresh engine section (epoch=%u)", ep);
    } else {
        CloseHandle(h2);
    }
}

struct MappedFrame {
    int w = 0, h = 0, frameId = -1, visible = 0;
    const uint8_t *pixels = nullptr;      // points into the mapping (BGRA)
};

static MappedFrame ReadFrame()
{
    MappedFrame f;
    ReopenIfEngineRestarted();
    if (!g_mmf) {
        g_mmf = OpenFileMappingW(FILE_MAP_READ | FILE_MAP_WRITE, FALSE, MMF_NAME);
        NLog("OpenFileMapping -> %p (err=%u)", (void *)g_mmf, GetLastError());
        if (!g_mmf) return f;
    }
    auto *view = (const uint8_t *)MapViewOfFile(g_mmf, FILE_MAP_READ, 0, 0, 0);
    if (!view) {
        // the controller restarted (its MMF died with the old process) —
        // drop the stale handle so the next Present re-opens the new one
        CloseHandle(g_mmf); g_mmf = nullptr;
        return f;
    }
    // the controller writes its PID at +24 — a change means the controller
    // restarted and created a NEW section; our old handle points at the
    // ORPHANED section (kept alive by our own handle) → re-open
    {
        static DWORD cachedCtrlPid = 0;
        DWORD ctrlPid = *(const DWORD *)(view + 24);
        if (cachedCtrlPid != 0 && ctrlPid != cachedCtrlPid) {
            CloseHandle(g_mmf); g_mmf = nullptr;
            UnmapViewOfFile(view);
            return f;   // next Present re-opens the fresh section
        }
        cachedCtrlPid = ctrlPid;
    }
    int magic = *(const int *)(view);
    if (magic != MAGIC) { UnmapViewOfFile(view); return f; }
    f.w = *(const int *)(view + 4);
    f.h = *(const int *)(view + 8);
    f.frameId = *(const int *)(view + 12);
    f.visible = *(const int *)(view + 16);
    f.pixels = view + HEADER_BYTES;
    return f;
}

// ── hook input → controller REST ───────────────────────────
static HINTERNET g_httpSes = nullptr, g_httpCon = nullptr;
static DWORD g_httpPort = 0;
static char g_httpSecret[40] = {};
static volatile LONG g_inputPosts = 0;

static void HttpEnsure(DWORD port, const char *secret)
{
    if (g_httpSes && g_httpPort == port) return;
    if (g_httpCon) { WinHttpCloseHandle(g_httpCon); g_httpCon = nullptr; }
    if (g_httpSes) { WinHttpCloseHandle(g_httpSes); g_httpSes = nullptr; }
    g_httpSes = WinHttpOpen(L"NvShareHook", WINHTTP_ACCESS_TYPE_NO_PROXY,
                            WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
    if (!g_httpSes) { NLog("WinHttpOpen FAILED err=%u", GetLastError()); return; }
    g_httpCon = WinHttpConnect(g_httpSes, L"127.0.0.1", (INTERNET_PORT)port, 0);
    if (!g_httpCon) { NLog("WinHttpConnect FAILED err=%u", GetLastError()); return; }
    g_httpPort = port;
    NLog("hook input endpoint ready: 127.0.0.1:%u", port);
}

static void HookPostInput(const char *json)
{
    if (!g_httpCon) return;
    HINTERNET rq = WinHttpOpenRequest(g_httpCon, L"POST", L"/ShadowPlay/v.1.0/Hook/Input",
                                      nullptr, WINHTTP_NO_REFERER,
                                      WINHTTP_DEFAULT_ACCEPT_TYPES, 0);
    if (!rq) return;
    wchar_t hdr[320];
    swprintf_s(hdr, L"Content-Type: application/json\r\nX_LOCAL_SECURITY_COOKIE: %hs\r\n", g_httpSecret);
    int len = (int)strlen(json);
    if (WinHttpSendRequest(rq, hdr, -1L, (LPVOID)json, len, len, 0)) {
        WinHttpReceiveResponse(rq, nullptr);
        LONG posts = InterlockedIncrement(&g_inputPosts);
        if (posts == 1) NLog("first input POST sent: %s", json);
    } else {
        static int sendErrLogged = 0;
        if (!sendErrLogged) { NLog("SendRequest FAILED err=%u", GetLastError()); sendErrLogged = 1; }
    }
    WinHttpCloseHandle(rq);
}

static DWORD WINAPI InputThread(LPVOID)
{
    NLog("input thread start");
    DWORD lastX = 0xFFFFFFFF, lastY = 0xFFFFFFFF;
    bool lastDown = false;
    while (true) {
        Sleep(16);
        if (!g_mmf) continue;
        auto *v = (const uint8_t *)MapViewOfFile(g_mmf, FILE_MAP_READ, 0, 0, 64);
        if (!v) continue;
        int visible = *(const int *)(v + 16);
        DWORD port = *(const DWORD *)(v + 28);
        char sec[40] = {};
        memcpy(sec, v + 32, 31);
        UnmapViewOfFile(v);
        if (!visible || !port || !sec[0]) { lastX = 0xFFFFFFFF; lastY = 0xFFFFFFFF; continue; }
        strcpy_s(g_httpSecret, sec);
        HttpEnsure(port, sec);
        if (!g_httpCon) continue;

        POINT p;
        if (!GetCursorPos(&p)) continue;
        POINT c = p;
        HWND fw = GetForegroundWindow();
        if (fw) ScreenToClient(fw, &c);
        bool down = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;

        if ((DWORD)c.x != lastX || (DWORD)c.y != lastY) {
            lastX = (DWORD)c.x; lastY = (DWORD)c.y;
            char js[128];
            sprintf_s(js, "{\"type\":\"mousemove\",\"x\":%d,\"y\":%d}", c.x, c.y);
            HookPostInput(js);
        }
        if (down != lastDown) {
            lastDown = down;
            char js[160];
            sprintf_s(js, "{\"type\":\"%s\",\"x\":%d,\"y\":%d,\"button\":0}",
                      down ? "mousedown" : "mouseup", c.x, c.y);
            HookPostInput(js);
        }
    }
}

// ── D3D helpers ────────────────────────────────────────────
static const char *SHADER_HLSL = R"(
struct VSOut { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
VSOut vsMain(uint vid:SV_VertexID){
  // standard oversized triangle covering the whole screen:
  //   (-1,-1), (3,-1), (-1,3) — the previous (3,3),(-1,3),(3,-1) ordering
  //   covered only x+y>2 (a single screen corner) = invisible draw.
  float2 p = float2(vid==0 ? -1.0 : (vid==1 ? 3.0 : -1.0),
                    vid==0 ? -1.0 : (vid==2 ? 3.0 : -1.0));
  VSOut o; o.pos = float4(p,0,1); o.uv = float2((p.x+1)/2, 1-(p.y+1)/2); return o;
}
Texture2D tex0:register(t0); SamplerState s0:register(s0);
float4 psMain(VSOut i):SV_Target { return tex0.Sample(s0, i.uv); }
)";

static bool BuildResources(void *swapChain)
{
    IDXGISwapChain *sc = (IDXGISwapChain *)swapChain;
    if (FAILED(sc->GetDevice(__uuidof(ID3D11Device), (void **)&g_res.device)) || !g_res.device) return false;
    g_res.device->GetImmediateContext(&g_res.ctx);
    if (!g_res.ctx) return false;

    D3D11_TEXTURE2D_DESC td = {};
    td.Width = g_res.frameW; td.Height = g_res.frameH;
    td.MipLevels = 1; td.ArraySize = 1;
    td.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    td.SampleDesc.Count = 1; td.Usage = D3D11_USAGE_DEFAULT;
    td.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    if (FAILED(g_res.device->CreateTexture2D(&td, nullptr, &g_res.tex))) { NLog("CreateTexture2D failed"); return false; }
    if (FAILED(g_res.device->CreateShaderResourceView(g_res.tex, nullptr, &g_res.srv))) return false;

    D3D11_BLEND_DESC bd = {};
    bd.RenderTarget[0].BlendEnable = TRUE;
    bd.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_ALPHA;
    bd.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
    bd.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
    bd.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
    bd.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_INV_SRC_ALPHA;
    bd.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;
    bd.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
    g_res.device->CreateBlendState(&bd, &g_res.blend);

    ID3DBlob *vsb = nullptr, *psb = nullptr, *err = nullptr;
    if (FAILED(D3DCompile(SHADER_HLSL, strlen(SHADER_HLSL), nullptr, nullptr, nullptr,
                          "vsMain", "vs_5_0", 0, 0, &vsb, &err))) {
        NLog("D3DCompile vs FAILED: %s", err ? (const char *)err->GetBufferPointer() : "?");
        return false;
    }
    if (FAILED(g_res.device->CreateVertexShader(vsb->GetBufferPointer(), vsb->GetBufferSize(), nullptr, &g_res.vs))) return false;
    if (FAILED(D3DCompile(SHADER_HLSL, strlen(SHADER_HLSL), nullptr, nullptr, nullptr,
                          "psMain", "ps_5_0", 0, 0, &psb, &err))) {
        NLog("D3DCompile ps FAILED: %s", err ? (const char *)err->GetBufferPointer() : "?");
        return false;
    }
    if (FAILED(g_res.device->CreatePixelShader(psb->GetBufferPointer(), psb->GetBufferSize(), nullptr, &g_res.ps))) return false;

    // screen-covering triangle via SV_VertexID — layout can be null with
    // a input-less vs; create an empty layout to satisfy the runtime
    g_res.device->CreateInputLayout(nullptr, 0, vsb->GetBufferPointer(), vsb->GetBufferSize(), &g_res.layout);

    D3D11_RASTERIZER_DESC rd = {};
    rd.FillMode = D3D11_FILL_SOLID;
    rd.CullMode = D3D11_CULL_NONE;
    rd.ScissorEnable = FALSE;
    g_res.device->CreateRasterizerState(&rd, &g_res.raster);

    D3D11_SAMPLER_DESC sd = {};
    sd.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    sd.AddressU = sd.AddressV = sd.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    sd.MaxLOD = 1000;
    g_res.device->CreateSamplerState(&sd, &g_res.sampler);

    if (vsb) vsb->Release();
    if (psb) psb->Release();
    g_res.ready = true;
    return true;
}

static bool drawLogged = false;
static void DrawFrame(void *swapChain, MappedFrame f)
{
    if (!g_res.ready) {
        IDXGISwapChain *sc = (IDXGISwapChain *)swapChain;
        DXGI_SWAP_CHAIN_DESC d = {};
        sc->GetDesc(&d);
        NLog("swapchain: effect=%d buffers=%d fmt=%d", d.SwapEffect, d.BufferCount, d.BufferDesc.Format);
        g_res.frameW = (UINT)f.w; g_res.frameH = (UINT)f.h;
        if (!BuildResources(swapChain)) { NLog("BuildResources FAILED"); return; }
        NLog("BuildResources ok (%ux%u)", g_res.frameW, g_res.frameH);
    }
    if (!g_res.ready) return;

    // DEFAULT-usage texture: UpdateSubresource (Map+DISCARD is invalid here)
    g_res.ctx->UpdateSubresource(g_res.tex, 0, nullptr, f.pixels,
                                 g_res.frameW * 4, 0);

    UINT vw = 0, vh = 0;
    IDXGISwapChain *sc = (IDXGISwapChain *)swapChain;
    {
        DXGI_SWAP_CHAIN_DESC d = {};
        sc->GetDesc(&d);
        vw = d.BufferDesc.Width; vh = d.BufferDesc.Height;
    }
    D3D11_VIEWPORT vp = {0, 0, (float)vw, (float)vh, 0, 1};
    // bind the swap chain's BACK BUFFER — drawing into the game's currently
    // bound RTV (a scene target that never gets presented) was why the
    // overlay was invisible
    ID3D11Texture2D *back = nullptr;
    if (FAILED(sc->GetBuffer(0, __uuidof(ID3D11Texture2D), (void **)&back))) {
        NLog("GetBuffer FAILED"); return;
    }
    ID3D11RenderTargetView *rtv = nullptr;
    if (FAILED(g_res.device->CreateRenderTargetView(back, nullptr, &rtv))) {
        NLog("CreateRTV FAILED"); back->Release(); return;
    }
    if (!drawLogged) { NLog("GetBuffer+RTV ok"); }
    ID3D11ShaderResourceView *nullSRV = nullptr;
    g_res.ctx->OMSetRenderTargets(1, &rtv, nullptr);
    g_res.ctx->OMSetBlendState(g_res.blend, nullptr, 0xFFFFFFFF);
    g_res.ctx->RSSetState(g_res.raster);          // game's scissor/state must go
    D3D11_RECT sr = {0, 0, (LONG)vw, (LONG)vh};
    g_res.ctx->RSSetScissorRects(1, &sr);
    g_res.ctx->RSSetViewports(1, &vp);
    g_res.ctx->VSSetShader(g_res.vs, nullptr, 0);
    g_res.ctx->PSSetShader(g_res.ps, nullptr, 0);
    g_res.ctx->PSSetShaderResources(0, 1, &g_res.srv);
    g_res.ctx->PSSetSamplers(0, 1, &g_res.sampler);
    g_res.ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    g_res.ctx->IASetInputLayout(nullptr);
    // the game's bound vertex/index buffers + null layout make D3D11 drop
    // the draw silently — unbind them (classic gotcha)
    ID3D11Buffer *nullVB = nullptr;
    UINT zeroStride = 0, zeroOffset = 0;
    g_res.ctx->IASetVertexBuffers(0, 1, &nullVB, &zeroStride, &zeroOffset);
    g_res.ctx->IASetIndexBuffer(nullptr, DXGI_FORMAT_R16_UINT, 0);
    g_res.ctx->GSSetShader(nullptr, nullptr, 0);   // game's stages must not run
    g_res.ctx->HSSetShader(nullptr, nullptr, 0);
    g_res.ctx->DSSetShader(nullptr, nullptr, 0);
    g_res.ctx->SOSetTargets(0, nullptr, nullptr);
    g_res.ctx->Draw(3, 0);
    if (!drawLogged) { NLog("Draw(3,0) executed vw=%u vh=%u", vw, vh); drawLogged = 1; }
    nullSRV = nullptr;
    g_res.ctx->PSSetShaderResources(0, 1, &nullSRV);
    g_res.ctx->OMSetRenderTargets(0, nullptr, nullptr);
    rtv->Release();
    back->Release();
}

// ── Present hook ───────────────────────────────────────────
static volatile LONG g_presentCount = 0;
static volatile LONG g_drawCount = 0;

static void OverlayWork(void *swapChain)
{
    InterlockedIncrement(&g_presentCount);
    if (g_presentHooked && g_mmf) {
        auto *wv = (int *)MapViewOfFile(g_mmf, FILE_MAP_WRITE, 0, 0, 0);
        if (wv) { wv[5] = wv[5] + 1; UnmapViewOfFile(wv); }
    }
    if (!g_apiChecked) {
        IDXGISwapChain *sc = (IDXGISwapChain *)swapChain;
        void *d11 = nullptr, *d12 = nullptr;
        HRESULT hr11 = sc->GetDevice(__uuidof(ID3D11Device), &d11);
        if (SUCCEEDED(hr11) && d11) {
            NLog("swapchain device = D3D11");
            ((ID3D11Device *)d11)->Release();
        } else {
            HRESULT hr12 = sc->GetDevice(__uuidof(ID3D12Device), &d12);
            NLog("swapchain device = D3D12 (hr11=0x%08X hr12=0x%08X)", hr11, hr12);
            if (SUCCEEDED(hr12) && d12) ((ID3D12Device *)d12)->Release();
        }
        NLog("ReadFrame probe: mmf=%p", (void *)g_mmf);
        g_apiChecked = 1;
    }
    MappedFrame f = ReadFrame();
    static int g_frameLogged = 0;
    if (!g_frameLogged) {
        NLog("frame probe: pixels=%p visible=%d w=%d h=%d fid=%d", (void *)f.pixels, f.visible, f.w, f.h, f.frameId);
        if (f.pixels || f.visible) g_frameLogged = 1;
    }
    if (f.pixels && f.visible && f.w > 0) {
        LONG d = InterlockedIncrement(&g_drawCount);
        if (d == 1 || d % 600 == 0) {
            DXGI_SWAP_CHAIN_DESC dsc = {};
            ((IDXGISwapChain *)swapChain)->GetDesc(&dsc);
            NLog("draw #%d desc effect=%d buffers=%d fmt=%d %ux%u",
                 d, dsc.SwapEffect, dsc.BufferCount, dsc.BufferDesc.Format,
                 dsc.BufferDesc.Width, dsc.BufferDesc.Height);
        }
        if (!g_res.ready || g_res.frameW != (UINT)f.w || g_res.frameH != (UINT)f.h) {
            // reset resources on size change
            if (g_res.ready) {
                if (g_res.tex) g_res.tex->Release();
                if (g_res.srv) g_res.srv->Release();
                g_res.tex = nullptr; g_res.srv = nullptr;
                g_res.ready = false;
            }
            g_res.frameW = (UINT)f.w; g_res.frameH = (UINT)f.h;
        }
        DrawFrame(swapChain, f);
    }
}

static HRESULT STDMETHODCALLTYPE HookedPresent(void *swapChain, UINT sync, UINT flags)
{
    OverlayWork(swapChain);
    return g_origPresent(swapChain, sync, flags);
}

// IDXGISwapChain1::Present1 lives in a DIFFERENT vtable (slot 14: the
// base swapchain occupies 0..13) — games presenting through it never hit
// the slot-8 Present hook. Slot 12 is GetFrameStatistics — patching it
// crashes the game (draws mid-frame + wrong signature).
typedef HRESULT(STDMETHODCALLTYPE *Present1_t)(void *swapChain, UINT sync, UINT flags, const void *presentParams);
static Present1_t g_origPresent1 = nullptr;
static void **g_vtableSlot1 = nullptr;
static HRESULT STDMETHODCALLTYPE HookedPresent1(void *swapChain, UINT sync, UINT flags, const void *presentParams)
{
    OverlayWork(swapChain);
    return g_origPresent1(swapChain, sync, flags, presentParams);
}

static void InstallPresentHook()
{
    // dummy device + swapchain to grab the DXGI swapchain vtable
    HWND w = CreateWindowExW(0, L"STATIC", L"", 0, 0, 0, 8, 8, nullptr, nullptr, GetModuleHandleW(nullptr), nullptr);
    DXGI_SWAP_CHAIN_DESC sd = {};
    sd.BufferDesc.Width = 8; sd.BufferDesc.Height = 8;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.SampleDesc.Count = 1;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.BufferCount = 1;
    sd.OutputWindow = w;
    sd.Windowed = TRUE;
    IDXGISwapChain *sc = nullptr;
    ID3D11Device *dev = nullptr;
    ID3D11DeviceContext *ctx = nullptr;
    D3D_FEATURE_LEVEL fl;
    if (FAILED(D3D11CreateDeviceAndSwapChain(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, 0,
                                             nullptr, 0, D3D11_SDK_VERSION, &sd,
                                             &sc, &dev, &fl, &ctx))) { NLog("dummy device FAILED"); return; }
    NLog("dummy device ok, patching vtable");
    void **vtable = *(void ***)sc;         // IDXGISwapChain vtable
    g_vtableSlot = &vtable[8];             // slot 8 = Present
    DWORD oldp;
    VirtualProtect(g_vtableSlot, sizeof(void *), PAGE_EXECUTE_READWRITE, &oldp);
    g_origPresent = (Present_t)*g_vtableSlot;
    *g_vtableSlot = (void *)&HookedPresent;
    VirtualProtect(g_vtableSlot, sizeof(void *), oldp, &oldp);
    g_presentHooked = 1;
    NLog("Present hook installed");
    // The game may hold the swap chain as ANY interface level (SwapChain1..4)
    // and every interface has its OWN vtable memory. Patch Present (slot 8)
    // and Present1 (slot 12) on every vtable we can QueryInterface.
    {
        const IID iids[] = {
            __uuidof(IDXGISwapChain1),
            __uuidof(IDXGISwapChain2),
            __uuidof(IDXGISwapChain3),
            __uuidof(IDXGISwapChain4),
        };
        for (int i = 0; i < 4; i++) {
            void *unk = nullptr;
            if (FAILED(sc->QueryInterface(iids[i], &unk)) || !unk) continue;
            void **vt = *(void ***)unk;
            // slot 8: Present (same signature at every interface level)
            void **slot8 = &vt[8];
            VirtualProtect(slot8, sizeof(void *), PAGE_EXECUTE_READWRITE, &oldp);
            if (!g_origPresent) g_origPresent = (Present_t)*slot8;
            *slot8 = (void *)&HookedPresent;
            VirtualProtect(slot8, sizeof(void *), oldp, &oldp);
            // slot 14: Present1 (base swapchain owns 0..13)
            void **slot14 = &vt[14];
            VirtualProtect(slot14, sizeof(void *), PAGE_EXECUTE_READWRITE, &oldp);
            if (!g_origPresent1) g_origPresent1 = (Present1_t)*slot14;
            *slot14 = (void *)&HookedPresent1;
            VirtualProtect(slot14, sizeof(void *), oldp, &oldp);
            NLog("patched derived interface %d (present+present1)", i + 1);
            ((IUnknown *)unk)->Release();
        }
    }
    // announce liveness to the controller: header[20] = 1 (re-written
    // every Present so staleness is detectable)
    {
        auto *wv = (int *)MapViewOfFile(g_mmf, FILE_MAP_WRITE, 0, 0, 0);
        if (wv) { wv[5] = wv[5] + 1; UnmapViewOfFile(wv); }
    }
    if (ctx) ctx->Release();
    if (sc) sc->Release();
    if (dev) dev->Release();
    DestroyWindow(w);
}

// ── dll lifecycle ──────────────────────────────────────────
static DWORD WINAPI InitThread(LPVOID)
{
    NLog("init thread start");
    // NO window wait — the dummy swapchain hook does not need the game
    // window; install immediately so Alt+Z works seconds after injection.
    InstallPresentHook();
    CreateThread(nullptr, 0, InputThread, nullptr, 0, nullptr);
    g_live = 1;
    return 0;
}

BOOL APIENTRY DllMain(HMODULE self, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH) {
        g_self = self;
        DisableThreadLibraryCalls(self);
        CreateThread(nullptr, 0, InitThread, nullptr, 0, nullptr);
    }
    return TRUE;
}
