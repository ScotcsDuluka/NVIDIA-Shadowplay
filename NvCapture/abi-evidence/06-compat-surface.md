# 06 — Minimal safe compatibility surface (our nvspcap v1)

Deliverable: `NvCapture/nvspcap/nvspcap.cpp` (+ `.def`), built to
`nvspcap64.dll` (x64) and `nvspcap.dll` (x86), plus `NvCapture/harness/abi_harness.exe`.

## 6.1 What we implement — only [P]/[O] surface

| Reference element | Our v1 |
|---|---|
| Exports `QueryShadowPlayDdiShimInterface` @1, `QueryShadowPlayDdiShimStatus` @2, `CreateShadowPlayProxyShimInterface` @3 | exact names + ordinals via `.def` |
| `QueryShadowPlayDdiShimInterface(UINT ver, void** ppOut)` | accepts **VER_1 only** (`0x10078`); fills a 15-slot table; every other version (incl. proven 0x10088/0x40090/0x400A0) declines like the reference fall-through |
| VER_1 slot map (15 slots, doc 02) | structurally exact: 15 function pointers; **all slots = one decline stub** returning `E_NOTIMPL` without touching any argument (mirrors NVIDIA's own all-stub no-op vtable, doc 03.4) |
| `QueryShadowPlayDdiShimStatus(BOOL*, BOOL*)` | both-args-required (`E_POINTER`/`E_INVALIDARG` on null); success returns `FALSE/FALSE` — we never claim an active DDI shim, so a conforming driver caller simply does not drive the DDI path |
| `CreateShadowPlayProxyShimInterface(pArgs)`, out-slot at `+0x20` | validates `pArgs` (SEH-guarded read), writes a static proxy object, `S_OK`; unknown/invalid args decline |
| Proxy vtable, 12 slots (doc 03.2) | structurally exact: 12 slots, all = the same decline stub (`ShimReleaseInterface` included — declining Release is the safe choice while we hold no resources) |
| `ShimRegisterCallback/UnregisterCallback` shape `(this, void* pCallback)` | accepted as calls into the decline stub: non-crashing, no state, no callbacks ever fired |

## 6.2 What we deliberately do NOT do (and why that is correct)

* **No guessed vtables.** Slots whose semantics are [H] (proxy slots 6/7,
  DDI slot 15) are implemented as declines, never as invented behavior.
* **No DDI interception.** We never hook a present path; the DDI table is
  valid and callable but every call declines, and the status query reports
  "not in use" — the driver's own gate (doc 04.1) then skips the shim.
* **No capture, no NvFBC, no MMFs, no registry writes, no files.** The only
  side channel is `OutputDebugStringA` logging (the reference writes
  `CaptureCore.log`; we do not touch disk in a host process).
* **No threads, no TLS callbacks, no CRT startup work beyond `/MT` defaults.**
  `DllMain` is empty. Safe to load in any process for ABI testing.
* **Unknown versions/args** always decline with a negative `HRESULT` and
  never write through caller pointers other than the documented out-slots.

## 6.3 Documented deltas vs reference (all intentional)

| Delta | Reason |
|---|---|
| VER_2/3-5/6 not negotiable | structurally proven (17/18/20 slots) but semantics not exhausted; v1 keeps the minimal proven surface. Slot map is already on file for a v2 (doc 02.2). |
| `QueryShadowPlayDdiShimInterface` pre-nulls `*ppOut` on unknown versions | reference leaves it untouched; we null first *only* because our harness + any defensive caller treats "declined" as `NULL`. Delta is caller-visible but strictly safer. |
| Proxy methods decline instead of maintaining a `CRITICAL_SECTION` at `+0x60` | our object is stateless; no lock needed. Callers never observe the lock, only the `HRESULT`s. |
| Status always `FALSE/FALSE` | truthful: this shim drives nothing. |

## 6.4 Harness acceptance criteria (run before any integration)

`harness/abi_harness.exe` (SEH-wrapped, never dies):

1. Load our DLL by path; resolve 3 exports **by name and by ordinal** — names
   must match ordinal map {1,2,3} exactly (contract check vs doc 01).
2. `QueryShadowPlayDdiShimStatus(NULL,&b)` fails without crash; `(&a,&b)`
   returns `S_OK` with both `FALSE`.
3. `QueryShadowPlayDdiShimInterface(0x10078,&p)` → `S_OK`; `p` readable for
   15 slots; every slot calls (10 null args, SEH-guarded) and returns a
   negative `HRESULT`; no crash.
4. Versions `0x10088`, `0x40090`, `0x400A0`, `0xDEADBEEF` → negative `HRESULT`,
   `*ppOut == NULL`, no crash.
5. `CreateShadowPlayProxyShimInterface` with a ≥0x28-byte args struct whose
   `+0x20` slot holds a **non-null destination address** (doc 03.1: the
   reference null-checks the loaded value, so a zeroed struct is correctly
   declined with a negative `HRESULT`) → `S_OK`, object with readable
   12-slot vtable; every slot called (nulls, SEH-guarded), no crash;
   `pArgs == NULL` → negative `HRESULT`.
6. Overall PASS only if every check above passes and the process exits 0.

Results of the accepted run: `harness/harness-results.txt` — **13/13 PASS**.

## 6.5 Build mode note

Both the shim and the harness are **freestanding**: no CRT and no Windows
SDK (this dev box ships neither). The build generates its own minimal import
libraries from `kernel32_min.def` / `kernel32_min_x86.def` with
`lib.exe /DEF:` and links `/NODEFAULTLIB` (`/ENTRY:DllMain`); the harness's
x64 SEH resolves through the OS-exported `ntdll!__C_specific_handler`
(`ntdll_min.def`). Import surface of the shipped DLL is kernel32.dll only.
