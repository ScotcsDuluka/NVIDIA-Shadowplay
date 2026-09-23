// nvspcap.cpp — NvCapture minimal-safe ABI compatibility shim (v1)
//
// Contract evidence: NvCapture/abi-evidence/*.md (evidence > hypothesis).
// Implements ONLY the proven surface of NVIDIA's nvspcap(64).dll:
//   @1 QueryShadowPlayDdiShimInterface(UINT ver, void** ppOut)          [02]
//   @2 QueryShadowPlayDdiShimStatus(BOOL* pUseDdiShim, BOOL* pAllowed)   [04]
//   @3 CreateShadowPlayProxyShimInterface(void* pArgs), out-slot at +0x20  [03]
// Every interface method is the same decline stub (an all-stub vtable is the
// reference's own inactive-state pattern, abi-evidence 03.4). No DDI
// interception, no threads, no files, no MMFs; logging is debug-channel only.
//
// Freestanding build: no CRT and no Windows SDK headers (this dev box has
// none — see build.cmd), so the few Win32 pieces are declared by hand and
// resolved through the minimal import library generated from kernel32_min.def.

typedef long HRESULT;
typedef int BOOL;
typedef unsigned long DWORD;
typedef void* HANDLE;
typedef void* HINSTANCE;
typedef void* LPVOID;
typedef const void* LPCVOID;

#ifdef _WIN64
typedef unsigned __int64 UPTR;
#else
typedef unsigned int UPTR;
#endif

#define WINAPI __stdcall

#ifndef NULL
#define NULL ((void*)0)
#endif
#define DLL_PROCESS_ATTACH 1

#ifndef S_OK
#define S_OK         ((HRESULT)0L)
#endif
#define E_NOTIMPL    ((HRESULT)0x80004001L)
#define E_POINTER    ((HRESULT)0x80004003L)
#define E_INVALIDARG ((HRESULT)0x80070057L)

extern "C" {
__declspec(dllimport) void WINAPI OutputDebugStringA(const char* lpOutputString);
__declspec(dllimport) BOOL WINAPI DisableThreadLibraryCalls(HINSTANCE hLibModule);
__declspec(dllimport) BOOL WINAPI IsBadReadPtr(LPCVOID lp, UPTR ucb);
__declspec(dllimport) BOOL WINAPI IsBadWritePtr(LPVOID lp, UPTR ucb);
}

// ---------------------------------------------------------------- logging
static void LogHex(const char* tag, unsigned long long v)
{
    char buf[160];
    static const char* hexd = "0123456789ABCDEF";
    char hex[17];
    hex[16] = 0;
    for (int i = 15; i >= 0; --i) { hex[i] = hexd[v & 0xF]; v >>= 4; }
    size_t n = 0;
    while (tag[n] && n < 120) { buf[n] = tag[n]; ++n; }
    if (n < 135) { buf[n++] = '0'; buf[n++] = 'x'; }
    for (int i = 0; i < 16 && n < 155; ++i) buf[n++] = hex[i];
    buf[n] = 0;
    OutputDebugStringA(buf);
}

// ----------------------------------------------- shared decline stub type
// x64: caller passes as many register/stack args as the real slot needs;
// the stub reads none of them and the caller cleans the stack. Returning
// E_NOTIMPL without touching any argument is crash-proof by construction.
typedef HRESULT (*ShimFn)(void*, void*, void*, void*, void*, void*, void*,
                          void*, void*, void*);

static HRESULT DdiDeclineStub(void*, void*, void*, void*, void*, void*,
                              void*, void*, void*, void*)
{
    return E_NOTIMPL;
}

// ----------------------------------------------- DDI shim table (VER_1)
// abi-evidence 02: VER_1 = 0x10078, 15 slots, strict-superset root of all
// versions. All slots decline: the driver caller treats the interface as
// unavailable (status query reports "not in use").
static ShimFn g_ddiTableV1[15] = {
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
};

// -------------------------------------------------- proxy shim interface
// abi-evidence 03: 12-slot vtable; object's first qword is the vtable
// pointer. Callers observe HRESULTs only; we hold no state, so the
// reference's CRITICAL_SECTION at +0x60 has nothing to guard here.
static ShimFn g_proxyVtable[12] = {
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
    DdiDeclineStub, DdiDeclineStub, DdiDeclineStub, DdiDeclineStub,
};

static struct ProxyObject {
    void* vtbl;
} g_proxyObject = { (void*)g_proxyVtable };

extern "C" {

// @1 — abi-evidence 02.1: (UINT ver, void** ppOut); VER_1 (0x10078) only;
// unknown versions decline like the reference fall-through (we null the out
// slot first — documented, strictly-safer delta in 06.3).
__declspec(noinline)
HRESULT QueryShadowPlayDdiShimInterface(unsigned int ver, void** ppOut)
{
    LogHex("NvCapture.QueryShadowPlayDdiShimInterface ver=", ver);
    if (!ppOut)
        return E_POINTER;
    *ppOut = NULL;
    if (ver == 0x10078u)             // SHADOWPLAY_DDISHIM_INTERFACE_VER_1
    {
        *ppOut = (void*)g_ddiTableV1;
        return S_OK;
    }
    return E_NOTIMPL;                // VER_2/3-5/6 reserved (doc 06.3)
}

// @2 — abi-evidence 04.1: both out-pointers required; we truthfully report
// "not in use / not allowed" because this shim drives no present path.
__declspec(noinline)
HRESULT QueryShadowPlayDdiShimStatus(int* pUseDdiShim, int* pIsShadowPlayAllowed)
{
    if (!pUseDdiShim || !pIsShadowPlayAllowed)
        return E_POINTER;
    *pUseDdiShim = 0;
    *pIsShadowPlayAllowed = 0;
    return S_OK;
}

// @3 — abi-evidence 03.1: pArgs carries the destination address at +0x20
// (reference loads [pArgs+0x20], null-checks it, stores the singleton into
// that address, returns S_OK). Reads/writes are range-validated with IsBad*
// (SEH-free equivalent of the guard noted in the evidence).
__declspec(noinline)
HRESULT CreateShadowPlayProxyShimInterface(void* pArgs)
{
    void* dest;
    if (!pArgs)
        return E_POINTER;
    if (IsBadReadPtr(pArgs, 0x28))   // args struct is at least 0x28 bytes
        return E_POINTER;
    dest = *(void**)((unsigned char*)pArgs + 0x20);
    if (!dest)
        return E_INVALIDARG;
    if (IsBadWritePtr(dest, sizeof(void*)))
        return E_POINTER;
    *(void**)dest = (void*)&g_proxyObject;
    LogHex("NvCapture.CreateShadowPlayProxyShimInterface obj=",
           (unsigned long long)(UPTR)&g_proxyObject);
    return S_OK;
}

int WINAPI DllMain(HINSTANCE hInst, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
        DisableThreadLibraryCalls(hInst);
    return 1;
}

} // extern "C"
