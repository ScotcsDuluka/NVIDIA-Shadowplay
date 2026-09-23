// abi_harness.cpp — NvCapture ABI validity / no-crash harness (freestanding)
//
// Loads OUR NvCapture/nvspcap/build/nvspcap64.dll (never the NVIDIA binary)
// and verifies the contract documented in NvCapture/abi-evidence/:
//   01  export contract: 3 exports, names pinned to ordinals 1/2/3
//   04.1 status query signature + decline semantics
//   02   DDI interface: VER_1 (0x10078) hands out a readable 15-slot table;
//        all other versions decline
//   03   proxy interface: out-slot at +0x20, 12-slot vtable
// Every foreign call is SEH-wrapped (x64 __C_specific_handler via ntdll):
// a crash is a FAIL, not a dead harness. No CRT: output via WriteFile.
//
// Usage: abi_harness.exe [path-to-nvspcap64.dll]

typedef long HRESULT;
typedef int BOOL;
typedef unsigned long DWORD;
typedef unsigned int UINT;
typedef void* HANDLE;
typedef void* HMODULE;
typedef void* LPVOID;
typedef const void* LPCVOID;
typedef void* FARPROC;

#ifdef _WIN64
typedef unsigned __int64 UPTR;
#else
typedef unsigned int UPTR;
#endif

#define WINAPI __stdcall
#define S_OK        ((HRESULT)0L)
#define STD_OUTPUT_HANDLE ((DWORD)-11L)

#ifndef NULL
#define NULL ((void*)0)
#endif

extern "C" {
__declspec(dllimport) HANDLE WINAPI GetStdHandle(DWORD nStdHandle);
__declspec(dllimport) BOOL WINAPI WriteFile(HANDLE hFile, LPCVOID lpBuffer,
    DWORD nNumberOfBytesToWrite, DWORD* lpNumberOfBytesWritten, LPVOID lpOverlapped);
__declspec(dllimport) void WINAPI ExitProcess(UINT uExitCode);
__declspec(dllimport) HMODULE WINAPI LoadLibraryA(const char* lpLibFileName);
__declspec(dllimport) FARPROC WINAPI GetProcAddress(HMODULE hModule, const char* lpProcName);
__declspec(dllimport) const char* WINAPI GetCommandLineA(void);
__declspec(dllimport) BOOL WINAPI GetModuleHandleExW(DWORD dwFlags, const wchar_t* lpModuleName,
    HMODULE* phModule);
__declspec(dllimport) BOOL WINAPI IsBadReadPtr(LPCVOID lp, UPTR ucb);
}

// ---------------------------------------------------------------- output
static int StrLen(const char* s) { int n = 0; while (s[n]) ++n; return n; }

static void Out(const char* s)
{
    DWORD written = 0;
    HANDLE h = GetStdHandle(STD_OUTPUT_HANDLE);
    if (h) WriteFile(h, s, (DWORD)StrLen(s), &written, NULL);
}

static void OutDec(int v)
{
    char buf[16]; int i = 15; buf[i] = 0;
    unsigned int u = v < 0 ? (unsigned int)(-(long long)v) : (unsigned int)v;
    if (u == 0) buf[--i] = '0';
    while (u) { buf[--i] = (char)('0' + u % 10); u /= 10; }
    if (v < 0) buf[--i] = '-';
    Out(&buf[i]);
}

static void OutHex(unsigned long long v)
{
    static const char* hexd = "0123456789ABCDEF";
    char buf[19]; buf[0] = '0'; buf[1] = 'x';
    for (int i = 0; i < 16; ++i) buf[2 + i] = hexd[(v >> (60 - 4 * i)) & 0xF];
    buf[18] = 0;
    Out(buf);
}

// ---------------------------------------------------------------- checks
static int g_pass = 0, g_fail = 0;

static void Check(int id, const char* what, int ok)
{
    Out(ok ? "[PASS] " : "[FAIL] ");
    if (id < 10) Out("0");
    OutDec(id);
    Out("  ");
    Out(what);
    Out("\n");
    if (ok) ++g_pass; else ++g_fail;
}

static HMODULE g_mod = NULL;

static BOOL IsInOurModule(void* p)
{
    HMODULE h = NULL;
    if (!p || !GetModuleHandleExW(0x02 /*FROM_ADDRESS*/ | 0x04 /*UNCHANGED_REFCOUNT*/,
                                  (const wchar_t*)p, &h))
        return 0;
    return h == g_mod;
}

// SEH-call a slot with 10 null args; returns 1 if the call completed.
static int CallSlot(void* slot, HRESULT* outHr)
{
    typedef HRESULT (*ShimFn)(void*, void*, void*, void*, void*, void*,
                              void*, void*, void*, void*);
    __try
    {
        ShimFn fn = (ShimFn)slot;
        *outHr = fn(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
        return 1;
    }
    __except (1 /*EXCEPTION_EXECUTE_HANDLER*/)
    {
        return 0;
    }
}

// optional arg: skip argv[0] honoring quotes, take next token into a buffer
static const char* ArgDllPath(void)
{
    static char buf[520];
    unsigned int i = 0;
    const char* p = GetCommandLineA();
    while (*p == ' ') ++p;
    if (*p == '"') { ++p; while (*p && *p != '"') ++p; if (*p) ++p; }
    else while (*p && *p != ' ') ++p;
    while (*p == ' ') ++p;
    if (*p == '"')
    {
        ++p;
        while (*p && *p != '"' && i < sizeof(buf) - 1) buf[i++] = *p++;
    }
    else
    {
        while (*p && *p != ' ' && i < sizeof(buf) - 1) buf[i++] = *p++;
    }
    buf[i] = 0;
    return buf;
}

extern "C" int entryProc(void*)
{
    Out("NvCapture ABI harness - target: ");
    Out(ArgDllPath());
    Out("\n");

    // 01 load
    g_mod = LoadLibraryA(ArgDllPath());
    Check(1, "LoadLibrary(our nvspcap64.dll)", g_mod != NULL);
    if (!g_mod) { Out("RESULT: FAIL\n"); ExitProcess(1); }

    // 02/03 export contract: names + ordinals
    typedef HRESULT (*PfnQueryDdi)(unsigned int, void**);
    typedef HRESULT (*PfnQueryStatus)(int*, int*);
    typedef HRESULT (*PfnCreateProxy)(void*);
    PfnQueryDdi    qDdi  = (PfnQueryDdi)GetProcAddress(g_mod, "QueryShadowPlayDdiShimInterface");
    PfnQueryStatus qStat = (PfnQueryStatus)GetProcAddress(g_mod, "QueryShadowPlayDdiShimStatus");
    PfnCreateProxy cProxy= (PfnCreateProxy)GetProcAddress(g_mod, "CreateShadowPlayProxyShimInterface");
    Check(2, "exports by name (3/3)", qDdi && qStat && cProxy);

    FARPROC o1 = GetProcAddress(g_mod, (const char*)1);
    FARPROC o2 = GetProcAddress(g_mod, (const char*)2);
    FARPROC o3 = GetProcAddress(g_mod, (const char*)3);
    Check(3, "ordinal map {1:QueryDdi, 2:QueryStatus, 3:CreateProxy}",
          o1 == (FARPROC)qDdi && o2 == (FARPROC)qStat && o3 == (FARPROC)cProxy);

    // 04/05 status query
    int a = -1, b = -1;
    int ok04 = 0, ok05 = 0;
    __try { ok04 = (qStat((int*)NULL, &b) < 0); } __except (1) { ok04 = 0; }
    Check(4, "QueryStatus(NULL,&b) fails without crash", ok04);
    __try { a = -1; b = -1; ok05 = (qStat(&a, &b) == S_OK && a == 0 && b == 0); } __except (1) { ok05 = 0; }
    Check(5, "QueryStatus(&a,&b) == S_OK and (FALSE,FALSE)", ok05);

    // 06 DDI VER_1
    void* tbl = NULL;
    int ok06 = 0, inMod = 0;
    __try {
        if (qDdi(0x10078u, &tbl) == S_OK && tbl && !IsBadReadPtr(tbl, 15 * sizeof(void*))) {
            ok06 = 1;
            inMod = 1;
            for (int i = 0; inMod && i < 15; ++i)
                inMod = IsInOurModule(((void**)tbl)[i]);
        }
    } __except (1) { ok06 = 0; }
    Check(6, "QueryDdi(VER_1 0x10078) -> S_OK, 15 readable slots in-module", ok06 && inMod);

    // 07 DDI slots decline without crash
    int slotsDecline = 1;
    if (ok06) {
        for (int i = 0; slotsDecline && i < 15; ++i) {
            HRESULT h2 = 0;
            if (!CallSlot(((void**)tbl)[i], &h2) || h2 >= 0)
                slotsDecline = 0;
        }
    }
    Check(7, "DDI slots 0..14 callable (nulls) -> negative HRESULT, no crash",
          ok06 && slotsDecline);

    // 08/09 unknown versions + null out
    int declined = 1, ok09 = 0;
    unsigned int badVers[4] = { 0x10088u, 0x40090u, 0x400A0u, 0xDEADBEEFu };
    __try {
        for (int i = 0; declined && i < 4; ++i) {
            void* p = (void*)1;
            HRESULT h3 = qDdi(badVers[i], &p);
            if (!(h3 < 0 && p == NULL)) declined = 0;
        }
        ok09 = (qDdi(0x10078u, (void**)NULL) < 0);
    } __except (1) { declined = 0; ok09 = 0; }
    Check(8, "VER_2/3-5/6/unknown declined, *ppOut NULL, no crash", declined);
    Check(9, "QueryDdi(ver, NULL) fails without crash", ok09);

    // 10 proxy create: pArgs+0x20 holds the DESTINATION ADDRESS (doc 03.1:
    // reference loads [pArgs+0x20], null-checks it, stores the object into it)
    struct Args { void* pad[4]; void* out; } args;
    void* obj = (void*)0;
    for (int i = 0; i < 4; ++i) args.pad[i] = NULL;
    args.out = (void*)&obj;          // storage address, must be non-null
    int ok10 = 0, vtblOk = 0;
    void** vtbl = (void**)0;
    __try {
        if (cProxy(&args) == S_OK && obj && !IsBadReadPtr(obj, sizeof(void*))) {
            vtbl = *(void***)obj;
            if (vtbl && !IsBadReadPtr(vtbl, 12 * sizeof(void*)))
                { ok10 = 1; vtblOk = 1; }
        }
    } __except (1) { ok10 = 0; }
    Check(10, "CreateProxy(args.out@+0x20) -> S_OK, object with vtable", ok10 && vtblOk);

    int vInMod = 1, vDecline = 1;
    if (ok10) {
        for (int i = 0; vInMod && i < 12; ++i)
            vInMod = IsInOurModule(vtbl[i]);
        for (int i = 0; vDecline && i < 12; ++i) {
            HRESULT h4 = 0;
            if (!CallSlot(vtbl[i], &h4))
                vDecline = 0;   // decline value may be any HRESULT; crash is fail
        }
    }
    Check(11, "proxy vtable 12 slots in-module", ok10 && vInMod);
    Check(12, "proxy slots 0..11 callable (nulls), no crash", ok10 && vDecline);

    int ok13 = 0;
    __try { ok13 = (cProxy(NULL) < 0); } __except (1) { ok13 = 0; }
    Check(13, "CreateProxy(NULL) fails without crash", ok13);

    Out("SUMMARY: pass=");
    OutDec(g_pass);
    Out(" fail=");
    OutDec(g_fail);
    Out("\n");
    Out(g_fail == 0 ? "RESULT: PASS\n" : "RESULT: FAIL\n");
    ExitProcess(g_fail == 0 ? 0 : 2);
    return 0;
}
