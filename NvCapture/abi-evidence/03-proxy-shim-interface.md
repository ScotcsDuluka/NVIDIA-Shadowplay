# 03 — CShadowPlayProxyShim interface (CreateShadowPlayProxyShimInterface)

Export body: RVA 0x30450.

## 3.1 Signature and args-struct [P]

Prologue (`dumps/proxy_create.txt`):

```asm
mov  rbx, rcx                  ; pArgs
...
test rbx, rbx                  ; null check
je   <fail>
mov  rcx, [rbx + 0x20]         ; out-slot of the args struct
test rcx, rcx
je   <fail>
mov  rax, [rip + 0x2530ae]     ; proxy shim singleton (object pointer)
mov  [rcx], rax                ; *(pArgs+0x20) = singleton
xor  eax, eax                  ; S_OK
```

So: `HRESULT CreateShadowPlayProxyShimInterface(void* pArgs)` where the
caller-owned args struct carries an out-slot `void** ppOut` at **offset
0x20**; on success the function stores a **proxy object pointer** (whose
first qword is the vtable) into it and returns `S_OK`. Struct size is at
least 0x28 bytes; the meaning of fields 0x00-0x1F is **[H]** (not probed, not
relied upon).

**[O]** No module in the ShadowPlay folder, and no NVIDIA module we scanned
(driver UMDs, helper, server, API DLLs), contains the string
`CreateShadowPlayProxyShimInterface` — except nvspcap itself (its export
table). The caller therefore addresses this export by **ordinal 3**
(`GetProcAddress(hMod, MAKEINTRESOURCEA(3))`), which needs no name string.
**[H]** caller identity: service-side component (doc 05).

## 3.2 The live vtable — 12 slots [P]

Vtable at RVA **0x1FDD08** (found by scanning `.rdata` for runs of text
pointers and matching the methods found via string xrefs, `dumps/`):

| slot | method (first string xref in body) | fn RVA |
|---|---|---|
| 0 | `ShimReleaseInterface` | 0x3E460 |
| 1 | `ShimRegisterCallback` | 0x3E3B0 |
| 2 | `ShimUnregisterCallback` | 0x3E5A0 |
| 3 | `ShimGetStatus` | 0x3CFE0 |
| 4 | `ShimCreateSession` (`"Creating session for Proxy version %d"`) | 0x3C490 |
| 5 | `ShimDestroySession` | 0x3CD40 |
| 6 | *(no string xref)* — lock-guarded accessor on `this+0x60` | 0x3CF50 |
| 7 | *(no string xref)* — lock-guarded accessor on `this+0x60` | 0x3E510 |
| 8 | `ShimPrePresent` (`"Resume from virtual stop"`) | 0x3D1F0 |
| 9 | `ShimPostPresent` | 0x3D0C0 |
| 10 | `ShimCreateCommandQueue` | 0x3C3E0 |
| 11 | `ShimDestroyCommandQueue` | 0x3CC90 |

Slots 6/7 **[P]** shape: `(this, void* pOut)` → `EnterCriticalSection(this+0x60)`
→ work → `LeaveCriticalSection` → return `S_OK`/`E_INVALIDARG`. Their exact
semantics are **[H]** (positional symmetry with the DDI table suggests
session-param accessors) — we do not implement them as anything.

## 3.3 Callback lifecycle (task item 3)

`ShimRegisterCallback` body **[P]** (`dumps/`, fn 0x3E3B0):

```c
HRESULT ShimRegisterCallback(ProxyShim* this, void* pCallback);
//  - pCallback == NULL  -> E_INVALIDARG (0x80070057), no registration
//  - pCallback != NULL  -> internal registration via fn @0x180008922,
//                          S_OK; guarded by the object's CRITICAL_SECTION
```

`ShimUnregisterCallback` mirrors it (slot 2). **[H]** the callback object/
function layout is not recovered; our shim accepts the pointer, stores
nothing callable, and never fires callbacks (we do not drive a present path,
so firing anything would be fake evidence). Symmetric
`Register`→(session lifetime)→`Unregister` ordering is **[P]** by method
pairing and the OUT logs; the *event stream* behind the callback is **[H]**.

## 3.4 The no-op vtable — NVIDIA's own "decline" state [P]

Second 12-slot vtable at RVA **0x1FD040** (same class region as the live
table): all 12 slots point at the *same* stub (fn 0x15DB90). This is the
proxy shim's dead/inactive state: every method callable, everything declines. This is the
in-binary precedent for our compatibility strategy (doc 06): a
structurally-valid interface whose methods decline is a legitimate,
crash-free state, and is what we ship in v1.

## 3.5 Object layout facts [P]

* `this + 0x00` — vtable pointer.
* `this + 0x60` — `CRITICAL_SECTION` used by register/unregister and slots 6/7
  (Enter/LeaveCriticalSection imports resolved at 0x2B3440/0x2B3448).
* Lifecycle strings: `CShadowPlayProxyShim::CShadowPlayProxyShim: OUT`,
  `::~CShadowPlayProxyShim`, `::CreateInstance`, `::DestroyInstance`,
  `::DestroyNoDeviceModules` — construct/destroy is explicit, not refcount-
  magic beyond slot 0 `ShimReleaseInterface` (semantics **[H]**).
