# 04 — Status query + lifecycle order

## 4.1 QueryShadowPlayDdiShimStatus [P]

Body RVA 0x41930. Prologue tests **both** `rcx` and `rdx` for null and exits
via `"QueryShadowPlayDdiShimStatus: Error - Invalid args"`; the success path
logs `"pUseDdiShim = %d, pIsShadowPlayAllowed = %d"`:

```c
HRESULT QueryShadowPlayDdiShimStatus(BOOL* pUseDdiShim, BOOL* pIsShadowPlayAllowed);
```

* Both pointers required; `E_INVALIDARG`-class failure when either is NULL. **[P]**
* The allowed-flag is read from global state (byte compares against globals
  and a struct field `[rax+0x55]`) — i.e. the answer reflects the ShadowPlay
  enable state, not the caller's. **[P]**
* Same function also contains the `GfeSDK.dll` /
  `NVGSDK_EscapeUWPSandboxMessageBus` lookup (strings at 0x41A23-0x41B35):
  it checks whether the process is a GfeSDK-integrated UWP game and can
  report the message-bus address. **[O]** observed; behavior **[H]**; not
  needed for our v1 (we answer `FALSE/FALSE` unconditionally).

## 4.2 DDI call lifecycle (order implied by the slot set) [P-set]

The DDI slot set (doc 02) is a complete adapter→device→resource→present
lifecycle; per-method logs fix the direction of each half:

```
DdiOpenAdapter
  DdiCreateDevice                ("OUT, CShadowPlayProxyShim instance = ...")
    CreateSession                (creates CShadowPlayProxyShim instance)
      CreateResource             ("Surface format set to [%d]")
        PreSetDisplayMode / PostSetDisplayMode
        DdiPrePresent  -> DdiPostPresent     (per frame)
        DdiBlt
        [VER_2+] DdiPrePresent_V2 (9 params, replaces/augments PrePresent)
      DestroyResource
    DestroySession
  DdiDestroyDevice
ReleaseInterface  [VER_3+]
DdiCreateCommandQueue / DdiDestroyCommandQueue  [VER_6]
```

**[P]** each named step exists with those logs; **[H]** the exact pairing
rules (e.g. whether PrePresent_V2 *replaces* PrePresent for a given flip
state — the logs `"currentFlipState=%d"`, `"DWM not doing uiFlip"` suggest
runtime selection) — not relied upon by our shim, which declines all DDI
methods.

## 4.3 Callback/event lifecycle [P + H]

* Registration is object-scoped: `ShimRegisterCallback`/
  `ShimUnregisterCallback` on the proxy object, guarded by the object's
  critical section; non-null argument required. **[P]**
* Event delivery is driven from the present path (`ShimPrePresent` /
  `ShimPostPresent` and the DDI present slots); with no active capture
  session the object idles (`"Exiting due to virtual stop"`). **[P]** for the
  strings; **[H]** for the exact event payload — our shim never registers
  itself into any present path, so it never delivers events.

## 4.4 Enable-state inputs [O]

* Registry: `HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience\ShadowPlay`
  values `ShadowPlayCfg2`, `ShadowPlayCfg3` (strings present in nvspcap64.dll
  *and* in nvsphelper64.exe). **[O]**
* Shared state: `CSPShareStateManager` reads a memory-mapped *server* state
  blob (`ReadDynamicServerDataFromMMF`, procInfo slots, mutex-guarded) and an
  OSC hotkey MMF; `\ShadowPlay` object-namespace prefix. **[O]**
* Our v1 reads **none** of these: it answers status `FALSE/FALSE` and leaves
  all enable plumbing to the real service (doc 06).
