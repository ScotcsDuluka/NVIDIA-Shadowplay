# 02 — SHADOWPLAY_DDISHIM interface (QueryShadowPlayDdiShimInterface)

Export body: RVA 0x412B0. Signature **[P]** from the prologue
(`mov ebx, ecx` = version, `mov [rdx], rax` = out pointer, OUT log
`"...version = 0x%x, res = 0x%x"`):

```c
HRESULT QueryShadowPlayDdiShimInterface(UINT ver, void** ppOut);
```

## 2.1 Version negotiation [P]

The body is a compare-chain; each hit populates a **caller-visible function
pointer table in `.data`** and stores its address into `*ppOut`
(`lea rax,[rip+D]; mov [rdx],rax`). Version codes and their log tags
(strings at RVA 0x1FFD20/0x1FFD80/0x1FFDE0/0x1FFE40):

| code | log tag | slots (`code & 0xFFFF / 8`) | table RVA (.data, zero in file, filled at query time) |
|---|---|---|---|
| 0x10078 | `SHADOWPLAY_DDISHIM_INTERFACE_VER_1` | 15 | 0x283DB0 |
| 0x10088 | `SHADOWPLAY_DDISHIM_INTERFACE_VER_2` | 17 | 0x283E40 |
| 0x40090 | `SHADOWPLAY_DDISHIM_INTERFACE_VER_3-5` | 18 | 0x283EF0 |
| 0x400A0 | `SHADOWPLAY_DDISHIM_INTERFACE_VER_6` | 20 | 0x283FA0 |

**[P]** Low 16 bits of the version code = struct byte size (120/136/144/160 =
exactly 15/17/18/20 qwords, matching the observed pointer-fill counts).
**[H]** high word is an interface family/generation marker (0x0001 for V1-2,
0x0004 for V3-6) — not needed for compatibility, never relied on.
Unknown codes fall through to the common `OUT, version = 0x%x, res = 0x%x`
exit **[P]**.

## 2.2 Slot map (proven)

Slot labels are **[P]**: each table entry is an ILT thunk → real function, and
each function's first string cross-reference (through `.pdata`-bounded
functions) is its own log tag. Full dump: `dumps/ddi_slot_map.txt`.

| slot | VER_1 | VER_2 | VER_3-5 | VER_6 | semantics |
|---|---|---|---|---|---|
| 0 | CheckStatus | = | = | = | `CheckStatus` |
| 1 | CreateSession | = | = | = | `CreateSession` — creates the `CShadowPlayProxyShim` instance (`"CreateSession: ProxyShim instance(0x%X), hDevice..."`) |
| 2 | DestroySession | = | = | = | `DestroySession` |
| 3 | GetSessionParam | = | = | = | `GetSessionParam` |
| 4 | SetSessionParam | = | = | = | `SetSessionParam` |
| 5 | DdiOpenAdapter | = | = | = | `DdiOpenAdapter` |
| 6 | DdiCreateDevice | = | = | = | `DdiCreateDevice` |
| 7 | DdiDestroyDevice | = | = | = | `DdiDestroyDevice` |
| 8 | CreateResource | = | = | = | `ShimCreateResource` (`"Surface format set to [%d]"`) |
| 9 | DestroyResource | = | = | = | `DdiDestroyResource` |
| 10 | PreSetDisplayMode | = | = | = | `DdiPreSetDisplayMode` |
| 11 | PostSetDisplayMode | = | = | = | `DdiPostSetDisplayMode` |
| 12 | DdiPrePresent | = | = | = | `DdiPrePresent` |
| 13 | DdiPostPresent | = | = | = | `DdiPostPresent` |
| 14 | DdiBlt | = | = | = | `DdiBlt` |
| 15 | — | ✓ | = | = | fn 0x3017 (first xref `"NVIDIA Share.exe"` — semantics **[H]** pending) |
| 16 | — | ✓ | = | = | `DdiPrePresent_V2` — 9 logged params: `instance, ver, hDevice, width, height, hResource, isFullScreen, setFullScreenFlag, SrcSubResourceIndex` **[P]** |
| 17 | — | — | ✓ | = | `ReleaseInterface` |
| 18 | — | — | — | ✓ | `DdiCreateCommandQueue` |
| 19 | — | — | — | ✓ | `DdiDestroyCommandQueue` |

**[P]** Each version is a strict **superset** of the previous one (slots 0-14
byte-identical across all four tables). Down-level callers are safe; a
compatibility implementation can support VER_1 alone and decline the rest.

## 2.3 Relationship to the proxy shim (task item 4)

**[P]** DDI slot 1 (`CreateSession`) instantiates `CShadowPlayProxyShim`
(`CShadowPlayProxyShim::CreateInstance`, fn 0x3A630, log
`"CreateSession: ProxyShim instance(0x%X), hDevice(0x%X), hRenderer(0x%X)"`).
So the DDI table (consumed by the driver, doc 05) is the *upper* half and the
ProxyShim object (doc 03) is the *session instance* the DDI side drives.
`DdiPrePresent` logs `"DWM not doing uiFlip"` / surface-format tracking —
consistent with driver-callback present-path interception, not DXGI hooking.

## 2.4 Unknown-version behavior [P]

For codes outside the four known ones, the chain falls through to the shared
exit (logs `OUT, version = ...`) **without storing into `*ppOut`** — the out
pointer is only written on a matched version. Our implementation mirrors this
(decline + leave `*ppOut` untouched, but we also pre-null it defensively and
document the delta in doc 06).
