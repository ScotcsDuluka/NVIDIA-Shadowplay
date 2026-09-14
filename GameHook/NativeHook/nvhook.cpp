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
#include <tlhelp32.h>
#include <GL/gl.h>
#include <vector>

#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "opengl32.lib")

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
typedef BOOL (WINAPI *WglSwapBuffers_t)(HDC hdc);
static WglSwapBuffers_t g_origWglSwapBuffers = nullptr;
static void *g_wglTrampoline = nullptr;
static volatile LONG g_wglExportHooked = 0;
static volatile LONG g_wglFrameLogged = 0;
static volatile LONG g_gdiSwapCount = 0;
static volatile LONG g_wglInteropLogged = 0;
static HGLRC g_glContext = nullptr;
static GLuint g_glTexture = 0;
static int g_glTextureW = 0;
static int g_glTextureH = 0;
static thread_local bool g_inGlOverlay = false;
static WglSwapBuffers_t g_origGdiSwapBuffers = nullptr;
typedef FARPROC (WINAPI *GetProcAddress_t)(HMODULE, LPCSTR);
static GetProcAddress_t g_origGetProcAddress = nullptr;
typedef PROC (WINAPI *WglGetProcAddress_t)(LPCSTR);
static WglGetProcAddress_t g_origWglGetProcAddress = nullptr;
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
static const uint8_t *g_mmfView = nullptr;
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
    // accept the fresh section when ours is gone OR its epoch differs
    if (!g_mmf || (ep != 0 && ep != g_cachedEpoch)) {
        if (g_mmfView) { UnmapViewOfFile(g_mmfView); g_mmfView = nullptr; }
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

    if (!g_mmfView) {
        g_mmfView = (const uint8_t *)MapViewOfFile(g_mmf, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }

    if (!g_mmfView) {
        // the controller restarted (its MMF died with the old process) —
        // drop the stale handle so the next Present re-opens the new one
        CloseHandle(g_mmf); g_mmf = nullptr;
        return f;
    }
    // the controller writes its PID at +24 — a change means the controller
    // restarted. Update the cache EVEN on reset, otherwise the next Present
    // resets again forever (live-counter null loop, measured).
    {
        static DWORD cachedCtrlPid = 0;
        DWORD ctrlPid = *(const DWORD *)(g_mmfView + 24);
        if (cachedCtrlPid != 0 && ctrlPid != cachedCtrlPid) {
            cachedCtrlPid = ctrlPid;
            if (g_mmfView) { UnmapViewOfFile(g_mmfView); g_mmfView = nullptr; }
            CloseHandle(g_mmf); g_mmf = nullptr;
            return f;   // next Present re-opens the fresh section
        }
        cachedCtrlPid = ctrlPid;
    }
    int magic = *(const int *)(g_mmfView);
    if (magic != MAGIC) return f;
    f.w = *(const int *)(g_mmfView + 4);
    f.h = *(const int *)(g_mmfView + 8);
    f.frameId = *(const int *)(g_mmfView + 12);
    f.visible = *(const int *)(g_mmfView + 16);
    f.pixels = g_mmfView + HEADER_BYTES;
    return f;
}

static void DrawOpenGlFrame()
{
    HGLRC ctx = wglGetCurrentContext();
    if (!ctx || g_inGlOverlay) return;
    MappedFrame f = ReadFrame();
    static LONG drawChecks = 0;
    LONG check = InterlockedIncrement(&drawChecks);
    if (check <= 5) {
        NLog("OpenGL draw check ctx=%p frame=%d visible=%d size=%dx%d pixels=%p",
             (void *)ctx, f.frameId, f.visible, f.w, f.h, (void *)f.pixels);
    }
    if (!f.pixels || !f.visible || f.w <= 0 || f.h <= 0 ||
        f.w > 8192 || f.h > 8192 ||
        (size_t)f.w * (size_t)f.h > (size_t)8192 * 8192) return;
    g_inGlOverlay = true;
    const size_t bytes = (size_t)f.w * (size_t)f.h * 4;
    std::vector<uint8_t> pixels(bytes);
    memcpy(pixels.data(), f.pixels, bytes);

    GLint viewport[4] = {};
    GLint oldTexture = 0, oldMatrixMode = GL_MODELVIEW;
    GLboolean oldBlend = glIsEnabled(GL_BLEND);
    GLboolean oldDepth = glIsEnabled(GL_DEPTH_TEST);
    GLboolean oldScissor = glIsEnabled(GL_SCISSOR_TEST);
    glGetIntegerv(GL_VIEWPORT, viewport);
    glGetIntegerv(GL_TEXTURE_BINDING_2D, &oldTexture);
    glGetIntegerv(GL_MATRIX_MODE, &oldMatrixMode);
    if (viewport[2] <= 0 || viewport[3] <= 0) { g_inGlOverlay = false; return; }

    if (g_glContext != ctx) {
        g_glContext = ctx;
        g_glTexture = 0;
        g_glTextureW = 0;
        g_glTextureH = 0;
    }
    if (!g_glTexture) glGenTextures(1, &g_glTexture);
    glBindTexture(GL_TEXTURE_2D, g_glTexture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP);
    glPixelStorei(GL_UNPACK_ALIGNMENT, 4);
    if (g_glTextureW != f.w || g_glTextureH != f.h) {
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, f.w, f.h, 0, GL_BGRA_EXT, GL_UNSIGNED_BYTE, pixels.data());
        g_glTextureW = f.w;
        g_glTextureH = f.h;
    } else {
        glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, f.w, f.h, GL_BGRA_EXT, GL_UNSIGNED_BYTE, pixels.data());
    }
    glDisable(GL_DEPTH_TEST);
    glDisable(GL_SCISSOR_TEST);
    glEnable(GL_TEXTURE_2D);
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    glViewport(0, 0, viewport[2], viewport[3]);
    glMatrixMode(GL_PROJECTION);
    glPushMatrix();
    glLoadIdentity();
    glOrtho(0, viewport[2], viewport[3], 0, -1, 1);
    glMatrixMode(GL_MODELVIEW);
    glPushMatrix();
    glLoadIdentity();
    glColor4f(1, 1, 1, 1);
    glBegin(GL_QUADS);
    glTexCoord2f(0, 0); glVertex2f(0, 0);
    glTexCoord2f(1, 0); glVertex2f((GLfloat)viewport[2], 0);
    glTexCoord2f(1, 1); glVertex2f((GLfloat)viewport[2], (GLfloat)viewport[3]);
    glTexCoord2f(0, 1); glVertex2f(0, (GLfloat)viewport[3]);
    glEnd();
    glPopMatrix();
    glMatrixMode(GL_PROJECTION);
    glPopMatrix();
    glMatrixMode(oldMatrixMode);
    glBindTexture(GL_TEXTURE_2D, (GLuint)oldTexture);
    if (!oldBlend) glDisable(GL_BLEND);
    if (oldDepth) glEnable(GL_DEPTH_TEST);
    if (oldScissor) glEnable(GL_SCISSOR_TEST);
    g_inGlOverlay = false;
}

static void ProbeWglDxInterop()
{
    if (InterlockedCompareExchange(&g_wglInteropLogged, 1, 0) != 0) return;
    HGLRC ctx = wglGetCurrentContext();
    PROC open = ctx ? wglGetProcAddress("wglDXOpenDeviceNV") : nullptr;
    PROC registerObject = ctx ? wglGetProcAddress("wglDXRegisterObjectNV") : nullptr;
    PROC lockObject = ctx ? wglGetProcAddress("wglDXLockObjectsNV") : nullptr;
    NLog("WGL_NV_DX_interop probe: context=%p open=%p register=%p lock=%p",
         (void *)ctx, (void *)open, (void *)registerObject, (void *)lockObject);
}

static BOOL WINAPI HookedWglSwapBuffers(HDC hdc)
{
    __try {
        if (g_origWglSwapBuffers && g_mmfView) {
            auto *wv = (int *)g_mmfView;
            wv[5] = wv[5] + 1;
            HWND hwnd = WindowFromDC(hdc);
            RECT r = {};
            bool fullscreen = false;
            if (hwnd && GetWindowRect(hwnd, &r)) {
                HMONITOR mon = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                MONITORINFO mi = { sizeof(mi) };
                LONG style = GetWindowLongW(hwnd, GWL_STYLE);
                if (mon && GetMonitorInfoW(mon, &mi)) {
                    fullscreen = (style & WS_CAPTION) == 0 &&
                        r.left <= mi.rcMonitor.left && r.top <= mi.rcMonitor.top &&
                        r.right >= mi.rcMonitor.right && r.bottom >= mi.rcMonitor.bottom;
                }

            }
            wv[15] = fullscreen ? 1 : 0; // OpenGL fullscreen telemetry
        }
        if (InterlockedCompareExchange(&g_wglFrameLogged, 1, 0) == 0) {
            NLog("OpenGL wglSwapBuffers reached");
        }
    } __except (EXCEPTION_EXECUTE_HANDLER) {
        NLog("OpenGL overlay frame fault 0x%08X - skipped", GetExceptionCode());
    }
    return g_origWglSwapBuffers ? g_origWglSwapBuffers(hdc) : FALSE;
}

static void InstallWglExportHook()
{
    if (InterlockedCompareExchange(&g_wglExportHooked, 1, 0) != 0) return;
    HMODULE gl = GetModuleHandleW(L"opengl32.dll");
    BYTE *target = gl ? (BYTE *)GetProcAddress(gl, "wglSwapBuffers") : nullptr;
    if (!target) { InterlockedExchange(&g_wglExportHooked, 0); return; }
    // wglSwapBuffers begins with:
    // 40 55 | 57 | 48 83 EC 58 | 48 8B 05 <rip-rel32>
    // Keep complete instructions and relocate the RIP-relative load in the
    // trampoline before jumping back to the original function.
    const SIZE_T stolen = 14;
    BYTE *tramp = (BYTE *)VirtualAlloc(nullptr, stolen + 12,
        MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if (!tramp) { InterlockedExchange(&g_wglExportHooked, 0); return; }
    memcpy(tramp, target, stolen);
    INT32 oldDisp = *(INT32 *)(target + 10);
    BYTE *absolute = target + 14 + oldDisp;
    *(INT32 *)(tramp + 10) = (INT32)(absolute - (tramp + 14));
    BYTE *j = tramp + stolen;
    j[0] = 0x48; j[1] = 0xB8; *(void **)(j + 2) = target + stolen;
    j[10] = 0xFF; j[11] = 0xE0;
    DWORD oldp = 0;
    if (!VirtualProtect(target, stolen, PAGE_EXECUTE_READWRITE, &oldp)) {
        VirtualFree(tramp, 0, MEM_RELEASE);
        InterlockedExchange(&g_wglExportHooked, 0);
        return;
    }
    target[0] = 0x48; target[1] = 0xB8;
    *(void **)(target + 2) = (void *)&HookedWglSwapBuffers;
    target[10] = 0xFF; target[11] = 0xE0;
    for (SIZE_T i = 12; i < stolen; i++) target[i] = 0x90;
    VirtualProtect(target, stolen, oldp, &oldp);
    FlushInstructionCache(GetCurrentProcess(), target, stolen);
    g_wglTrampoline = tramp;
    g_origWglSwapBuffers = (WglSwapBuffers_t)tramp;
    NLog("OpenGL export wglSwapBuffers detour installed");
}

static PROC WINAPI HookedWglGetProcAddress(LPCSTR name);
static BOOL WINAPI HookedGdiSwapBuffers(HDC hdc);
static WglSwapBuffers_t g_origWglSwapLayer = nullptr;
static BOOL WINAPI HookedWglSwapLayer(HDC hdc)
{
    LONG count = InterlockedIncrement(&g_gdiSwapCount);
    if (count == 1 || (count % 60) == 0) {
        static DWORD last = 0;
        DWORD now = GetTickCount();
        NLog("LAYER swap #%d (dt=%ums)", count, last ? now - last : 0);
        last = now;
    }
    return g_origWglSwapLayer ? g_origWglSwapLayer(hdc) : FALSE;
}

static FARPROC WINAPI HookedGetProcAddress(HMODULE module, LPCSTR name)
{
    FARPROC proc = g_origGetProcAddress ? g_origGetProcAddress(module, name) : nullptr;
    if (name && (strstr(name, "Swap") || strstr(name, "wgl"))) {
        NLog("GetProcAddress request: %s", name);
    }
    if (name && _stricmp(name, "wglSwapBuffers") == 0 &&
        module == GetModuleHandleW(L"opengl32.dll")) {
        g_origWglSwapBuffers = (WglSwapBuffers_t)proc;
        NLog("OpenGL wglSwapBuffers resolved; redirecting");
        return (FARPROC)&HookedWglSwapBuffers;
    }
    if (name && _stricmp(name, "wglGetProcAddress") == 0 &&
        module == GetModuleHandleW(L"opengl32.dll")) {
        g_origWglGetProcAddress = (WglGetProcAddress_t)proc;
        return (FARPROC)&HookedWglGetProcAddress;
    }
    if (name && _stricmp(name, "SwapBuffers") == 0 &&
        module == GetModuleHandleW(L"gdi32.dll")) {
        g_origGdiSwapBuffers = (WglSwapBuffers_t)proc;
        NLog("GDI SwapBuffers resolved; redirecting");
        return (FARPROC)&HookedGdiSwapBuffers;
    }
    return proc;
}

static PROC WINAPI HookedWglGetProcAddress(LPCSTR name)
{
    PROC proc = g_origWglGetProcAddress ? g_origWglGetProcAddress(name) : nullptr;
    if (name && _stricmp(name, "wglSwapBuffers") == 0) {
        g_origWglSwapBuffers = (WglSwapBuffers_t)proc;
        NLog("OpenGL wglSwapBuffers resolved via wglGetProcAddress; redirecting");
        return (PROC)&HookedWglSwapBuffers;
    }

    return proc;
}

static BOOL WINAPI HookedGdiSwapBuffers(HDC hdc)
{
    LONG count = InterlockedIncrement(&g_gdiSwapCount);
    if (count == 1 || (count % 60) == 0) {
        static DWORD last = 0;
        DWORD now = GetTickCount();
        NLog("GDI swap #%d (dt=%ums)", count, last ? now - last : 0);
        last = now;
    }
    __try { DrawOpenGlFrame(); }
    __except (EXCEPTION_EXECUTE_HANDLER) {
        NLog("OpenGL GDI overlay fault 0x%08X - skipped", GetExceptionCode());
    }
    return g_origGdiSwapBuffers ? g_origGdiSwapBuffers(hdc) : FALSE;
}

// ── hook input → shared-memory ring ────────────────────────
// The game's online-fix layer intercepts network APIs inside the game
// process (WinHTTP worked exactly once per process, then 12029 forever)
// — so input travels through a second shared-memory ring instead.
// Layout: +0 magic "NSIN", +4 writeIdx, +8 readIdx, +16 events[512x16B]
// event: {int type; int a; int b; int c;} 1=move 2=down 3=up 4=kd 5=ku
static HANDLE g_inMmf = nullptr;
static int *g_inView = nullptr;
static volatile LONG g_inWriteIdx = 0;
static SHORT (WINAPI *g_origGetAsyncKeyState)(int) = nullptr;
static SHORT (WINAPI *g_origGetKeyState)(int) = nullptr;
static BOOL (WINAPI *g_origGetKeyboardState)(PBYTE) = nullptr;
static UINT (WINAPI *g_origGetRawInputData)(HRAWINPUT, UINT, LPVOID, PUINT, UINT) = nullptr;

static void HookEnqueue(int type, int a, int b, int c)
{
    if (!g_inMmf) {
        g_inMmf = OpenFileMappingW(FILE_MAP_READ | FILE_MAP_WRITE, FALSE,
                                   L"NVIDIA_Share_Overlay_Input_v1");
        if (!g_inMmf) return;
    }
    if (!g_inView) g_inView = (int *)MapViewOfFile(g_inMmf, FILE_MAP_WRITE, 0, 0, 0);
    if (!g_inView) return;
    if (g_inView[0] != 0x4E49534E) return;   // "NSIN"
    int wi = InterlockedIncrement(&g_inWriteIdx) - 1;
    int slot = wi % 512;
    int *ev = g_inView + (4 + slot * 4);
    ev[0] = type; ev[1] = a; ev[2] = b; ev[3] = c;
    g_inView[1] = wi + 1;                  // publish writeIdx
}

static BYTE g_keyState[256] = {};

static DWORD WINAPI InputThread(LPVOID)
{
    NLog("input thread start");
    DWORD lastX = 0xFFFFFFFF, lastY = 0xFFFFFFFF;
    bool lastDown = false;
    DWORD lastPostTick = GetTickCount();
    while (true) {
        __try {
        Sleep(33);
        if (!g_mmf) continue;
        if (!g_mmfView) continue;
        int visible = *(const int *)(g_mmfView + 16);
        DWORD port = *(const DWORD *)(g_mmfView + 28);
        char sec[40] = {};
        memcpy(sec, g_mmfView + 32, 31);
        if (!visible || !port || !sec[0]) {
            lastX = 0xFFFFFFFF; lastY = 0xFFFFFFFF;
            memset(g_keyState, 0, sizeof(g_keyState));   // no stuck keys
            continue;
        }
        if (!visible) {
            lastX = 0xFFFFFFFF; lastY = 0xFFFFFFFF;
            memset(g_keyState, 0, sizeof(g_keyState));
            continue;
        }
        POINT p;
        if (!GetCursorPos(&p)) continue;
        POINT c = p;
        HWND fw = GetForegroundWindow();
        if (fw) ScreenToClient(fw, &c);
        auto realGetAsync = g_origGetAsyncKeyState ? g_origGetAsyncKeyState : GetAsyncKeyState;
        auto realGetKey = g_origGetKeyState ? g_origGetKeyState : GetKeyState;
        bool down = (realGetAsync(VK_LBUTTON) & 0x8000) != 0;

        if ((DWORD)c.x != lastX || (DWORD)c.y != lastY) {
            lastX = (DWORD)c.x; lastY = (DWORD)c.y;
            HookEnqueue(1, c.x, c.y, 0);
            lastPostTick = GetTickCount();
        }
        if (down != lastDown) {
            lastDown = down;
            HookEnqueue(down ? 2 : 3, c.x, c.y, 0);
            lastPostTick = GetTickCount();
        }
        // keyboard: transitions only, never logged (privacy: the key
        // values go straight to the controller, nothing touches disk)
        for (int vk = 0x08; vk <= 0xFE; vk++) {
            if (vk == VK_LBUTTON || vk == VK_RBUTTON || vk == VK_MBUTTON ||
                vk == VK_XBUTTON1 || vk == VK_XBUTTON2) continue;
            bool kd = (realGetAsync(vk) & 0x8000) != 0;
            bool was = g_keyState[vk] != 0;
            if (kd != was) {
                g_keyState[vk] = kd ? 1 : 0;
                int mods = ((realGetKey(VK_SHIFT) & 0x8000) ? 1 : 0) |
                           ((realGetKey(VK_MENU) & 0x8000) ? 2 : 0);
                HookEnqueue(kd ? 4 : 5, vk, mods,
                            (realGetKey(VK_CONTROL) & 0x8000) ? 1 : 0);
                lastPostTick = GetTickCount();
            }
        }
        } __except (EXCEPTION_EXECUTE_HANDLER) {
            NLog("input thread SEH caught 0x%08X — recovering", GetExceptionCode());
            memset(g_keyState, 0, sizeof(g_keyState));
            if (g_inView) { UnmapViewOfFile(g_inView); g_inView = nullptr; }
            g_inMmf = nullptr;
            Sleep(1000);
        }
    }
    return 0;
}

// ── input suppression while the overlay is open ────────────
// Subclass the game's window: while header.overlayVisible, mouse and
// keyboard MESSAGES are swallowed so the game does not also react to
// menu interaction (GFE parity). GetAsyncKeyState (our input poll) reads
// system state and is unaffected by this.
static bool OverlayVisibleCached()
{
    return g_mmfView && *(const int *)(g_mmfView + 16) == 1;
}

static WNDPROC g_origWndProc = nullptr;
static LRESULT CALLBACK HookedWndProc(HWND h, UINT msg, WPARAM w, LPARAM l)
{
    if (OverlayVisibleCached()) {
        // mouse: whole range (move + clicks + wheel + X buttons)
        if ((msg >= WM_MOUSEFIRST && msg <= WM_MOUSELAST) ||
            msg == WM_INPUT || msg == WM_MOUSEWHEEL || msg == WM_MOUSEHWHEEL) {
            return 0;
        }
        // keyboard
        if (msg == WM_KEYDOWN || msg == WM_KEYUP || msg == WM_CHAR ||
            msg == WM_SYSKEYDOWN || msg == WM_SYSKEYUP || msg == WM_SYSCHAR ||
            msg == WM_UNICHAR) {
            return 0;
        }
    }
    return CallWindowProcW(g_origWndProc, h, msg, w, l);
}

static void InstallWndProcHook()
{
    DWORD myPid = GetCurrentProcessId();
    struct Ctx { DWORD pid; HWND found; } ctx = { myPid, nullptr };
    EnumWindows([](HWND h, LPARAM lp) -> BOOL {
        auto *c = (Ctx *)lp;
        DWORD pid = 0; GetWindowThreadProcessId(h, &pid);
        if (pid == c->pid && IsWindowVisible(h) && GetWindow(h, GW_OWNER) == nullptr) {
            c->found = h; return FALSE;
        }
        return TRUE;
    }, (LPARAM)&ctx);
    if (!ctx.found) { NLog("WndProc: no game window yet"); return; }
    g_origWndProc = (WNDPROC)SetWindowLongPtrW(ctx.found, GWLP_WNDPROC, (LONG_PTR)&HookedWndProc);
    if (g_origWndProc) NLog("WndProc hook installed on game window %p", (void *)ctx.found);
    else NLog("SetWindowLongPtrW FAILED err=%u", GetLastError());
}

// ── IAT-level input suppression ────────────────────────────
// WndProc swallowing covers window MESSAGES, but engines often poll input
// APIs directly (UE: GetRawInputData in the pump, GetAsyncKeyState in
// ticks). While the overlay is open we redirect those imports to stubs
// that report "nothing pressed" so the game is fully blocked.
static SHORT WINAPI HookGetAsyncKeyState(int vk)
{
    if (OverlayVisibleCached()) return 0;
    return g_origGetAsyncKeyState ? g_origGetAsyncKeyState(vk) : 0;
}
static SHORT WINAPI HookGetKeyState(int vk)
{
    if (OverlayVisibleCached()) return 0;
    return g_origGetKeyState ? g_origGetKeyState(vk) : 0;
}
static BOOL WINAPI HookGetKeyboardState(PBYTE kb)
{
    if (OverlayVisibleCached() && kb) { memset(kb, 0, 256); return TRUE; }
    return g_origGetKeyboardState ? g_origGetKeyboardState(kb) : FALSE;
}
static UINT WINAPI HookGetRawInputData(HRAWINPUT h, UINT cmd, LPVOID data, PUINT size, UINT headerSize)
{
    if (OverlayVisibleCached()) { if (size) *size = 0; return (UINT)-1; }
    return g_origGetRawInputData ? g_origGetRawInputData(h, cmd, data, size, headerSize) : (UINT)-1;
}

// Patch one import across every loaded module's IAT (delay-load NOT
// covered — UE imports these statically, verified via dumpbin-equivalent
// behavior; re-run on later DLL loads is a future hardening step).
static void PatchIatAll(const char *importDll, const char *funcName, void *hook, void **orig)
{
    if (*orig) return;   // already patched
    HMODULE mods[1024];
    HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, GetCurrentProcessId());
    if (snap == INVALID_HANDLE_VALUE) return;
    MODULEENTRY32W me = { sizeof(me) };
    int patched = 0;
    if (Module32FirstW(snap, &me)) {
        do {
            // NEVER patch ourselves — our input-forwarding poll needs the
            // real GetAsyncKeyState (patching it here would cut our own
            // input stream while the overlay is open)
            if ((uint8_t *)me.modBaseAddr == (uint8_t *)g_self) continue;
            // parse PE imports
            __try {
                auto *base = (uint8_t *)me.modBaseAddr;
                auto *dos = (IMAGE_DOS_HEADER *)base;
                if (dos->e_magic != IMAGE_DOS_SIGNATURE) continue;
                auto *nt = (IMAGE_NT_HEADERS *)(base + dos->e_lfanew);
                if (nt->Signature != IMAGE_NT_SIGNATURE) continue;
                auto dir = nt->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
                if (!dir.VirtualAddress) continue;
                auto *imp = (IMAGE_IMPORT_DESCRIPTOR *)(base + dir.VirtualAddress);
                for (; imp->Name; imp++) {
                    const char *dll = (const char *)(base + imp->Name);
                    if (_stricmp(dll, importDll) != 0) continue;
                    auto *thunk = (IMAGE_THUNK_DATA *)(base +
                        (imp->OriginalFirstThunk ? imp->OriginalFirstThunk : imp->FirstThunk));
                    auto *iat = (IMAGE_THUNK_DATA *)(base + imp->FirstThunk);
                    for (; thunk->u1.AddressOfData; thunk++, iat++) {
                        if (thunk->u1.Ordinal & IMAGE_ORDINAL_FLAG) continue;
                        auto *fn = (IMAGE_IMPORT_BY_NAME *)(base + thunk->u1.AddressOfData);
                        if (strcmp((const char *)fn->Name, funcName) != 0) continue;
                        if (!*orig) *orig = (void *)(uintptr_t)iat->u1.Function;
                        DWORD oldp;
                        VirtualProtect(&iat->u1.Function, sizeof(void *), PAGE_EXECUTE_READWRITE, &oldp);
                        iat->u1.Function = (ULONG_PTR)hook;
                        VirtualProtect(&iat->u1.Function, sizeof(void *), oldp, &oldp);
                        patched++;
                    }
                }
            } __except (EXCEPTION_EXECUTE_HANDLER) {
                // malformed module — skip it
            }
        } while (Module32NextW(snap, &me));
    }
    CloseHandle(snap);
    if (patched) NLog("IAT %s!%s patched x%d", importDll, funcName, patched);
}

static void InstallIatHooks()
{
    PatchIatAll("user32.dll", "GetAsyncKeyState", (void *)&HookGetAsyncKeyState, (void **)&g_origGetAsyncKeyState);
    PatchIatAll("user32.dll", "GetKeyState", (void *)&HookGetKeyState, (void **)&g_origGetKeyState);
    PatchIatAll("user32.dll", "GetKeyboardState", (void *)&HookGetKeyboardState, (void **)&g_origGetKeyboardState);
    PatchIatAll("user32.dll", "GetRawInputData", (void *)&HookGetRawInputData, (void **)&g_origGetRawInputData);
    // Prefer import/resolver hooks; never patch opengl32.dll's export
    // prologue because Geometry Dash/driver combinations can crash there.
    PatchIatAll("opengl32.dll", "wglSwapBuffers", (void *)&HookedWglSwapBuffers, (void **)&g_origWglSwapBuffers);
    PatchIatAll("gdi32.dll", "SwapBuffers", (void *)&HookedGdiSwapBuffers, (void **)&g_origGdiSwapBuffers);
    PatchIatAll("opengl32.dll", "wglSwapLayerBuffers", (void *)&HookedWglSwapLayer, (void **)&g_origWglSwapLayer);
    PatchIatAll("opengl32.dll", "wglGetProcAddress", (void *)&HookedWglGetProcAddress, (void **)&g_origWglGetProcAddress);
    PatchIatAll("kernel32.dll", "GetProcAddress", (void *)&HookedGetProcAddress, (void **)&g_origGetProcAddress);

}


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

static void *g_lastSwapChain = nullptr;
static void OverlayWorkInner(void *swapChain)
{
    // exclusive-fullscreen resolution change hands us a NEW swapchain —
    // resources bound to the old device are poison. Full reset on change.
    if (g_lastSwapChain != swapChain) {
        g_lastSwapChain = swapChain;
        if (g_res.ready) {
            if (g_res.tex) g_res.tex->Release();
            if (g_res.srv) g_res.srv->Release();
            if (g_res.vs) g_res.vs->Release();
            if (g_res.ps) g_res.ps->Release();
            if (g_res.layout) g_res.layout->Release();
            if (g_res.blend) g_res.blend->Release();
            if (g_res.raster) g_res.raster->Release();
            if (g_res.sampler) g_res.sampler->Release();
            g_res = HookRes();
            NLog("swapchain changed - resources reset");
        }
    }
    InterlockedIncrement(&g_presentCount);
    if (g_presentHooked && g_mmfView) {
        auto *wv = (int *)g_mmfView;
        wv[5] = wv[5] + 1;
        // Use the swap-chain descriptor instead of GetFullscreenState.
        // GetDesc is already used by the renderer and is safe for UE/DXGI
        // fullscreen transitions; Windowed=false indicates exclusive mode.
        DXGI_SWAP_CHAIN_DESC modeDesc = {};
        if (SUCCEEDED(((IDXGISwapChain *)swapChain)->GetDesc(&modeDesc))) {
            wv[15] = modeDesc.Windowed ? 0 : 1; // header +60
        } else {
            wv[15] = 0;
        }
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
        // Draw the captured osc frame into the game's back buffer. The host
        // window stays behind the game in in-game mode, so disabling this
        // call leaves input blocked while rendering nothing visible.
        DrawFrame(swapChain, f);

    }
}

// SEH: a fault mid-draw (resize/resolution change vs nvspcap64 double-hook)
// must skip the frame, never kill the game
static void OverlayWork(void *swapChain)
{
    __try { OverlayWorkInner(swapChain); }
    __except (EXCEPTION_EXECUTE_HANDLER) {
        NLog("overlay frame fault 0x%08X - skipped", GetExceptionCode());
        g_res.ready = false;
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
    if (g_mmfView) {
        auto *wv = (int *)g_mmfView;
        wv[5] = wv[5] + 1;
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
    // Input and late-loaded graphics imports need a few retries while the
    // game finishes loading its renderer (Geometry Dash delay-loads OpenGL).
    for (int i = 0; i < 60; i++) {
        if (!g_origWndProc) InstallWndProcHook();
        InstallIatHooks();
        Sleep(1000);
    }
    // API-level suppression (engines polling input directly)
    InstallIatHooks();
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
