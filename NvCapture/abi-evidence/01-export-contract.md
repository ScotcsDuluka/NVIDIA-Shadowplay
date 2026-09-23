# 01 — Export contract

Targets: `nvspcap64.dll` (x64, GFE 3.28.0.412) and `nvspcap.dll` (x86, same drop).
All RVAs below are for the x64 image (base `0x180000000`) unless noted.

## 1.1 The complete export table

`dumpbin /EXPORTS` (dumps/nvspcap64.exports.txt):

| ordinal | name | RVA (x64 GFE build) | RVA (x86 GFE build) | RVA (x64 driver build, System32) |
|---|---|---|---|---|
| 1 | `QueryShadowPlayDdiShimInterface` | 0x984F | 0x5079 | 0xA9C0* |
| 2 | `QueryShadowPlayDdiShimStatus` | 0xA326 | 0x6AE6 | (see dumps) |
| 3 | `CreateShadowPlayProxyShimInterface` | 0x276B | 0x4430 | (see dumps) |

\* driver build dumps `CreateShadowPlayProxyShimInterface` at 0xA9C0; full table in `dumps/` session notes.

**[O]** This is the *whole* contract — `number of functions = 3, number of names = 3`
on both architectures and on the newer driver-shipped System32 build. The export
surface did not change between the GFE 3.28 drop (2024-04) and driver 32.0.15.8266.

**[O]** The names on the task card (`ShimRegisterCallback`, `DdiPrePresent`,
`CShadowPlayProxyShim`, …) exist in the binary **as internal log-tag strings**,
not exports (see 02/03). Any third-party code that tries to `GetProcAddress`
them will fail; they are reachable only through the two `Query*`/`Create*`
root interfaces.

## 1.2 Export thunks

**[P]** Each export is an `E9` jmp to a real function:

| export | body RVA |
|---|---|
| `CreateShadowPlayProxyShimInterface` | 0x30450 |
| `QueryShadowPlayDdiShimInterface` | 0x412B0 |
| `QueryShadowPlayDdiShimStatus` | 0x41930 |

## 1.3 Import surface [O]

`dumpbin /IMPORTS nvspcap64.dll`: `KERNEL32`, `USER32`, `SHELL32`, `ole32`,
`ADVAPI32`, `SHLWAPI`, `VERSION` — **no d3d11/dxgi/d3d9/nvcuda imports**.
The DLL does not talk to the D3D runtime itself; the DDI path is driven by
the graphics driver calling *into* it (doc 02, doc 05). Capture work is
delegated to `NvFBC` loaded dynamically
(`NvFBCDLL::CheckAndInitialize` log tag, RVA 0x1FBD20) **[P]**.

## 1.4 Naming/build topology [O]

Folder `ShadowPlay/` contains a family: `nvspcap.dll`/`nvspcap64.dll`
(in-process shim), `_nvspcaps64.dll` (ShadowPlay2 *server*; PDB path
`...\shadowplay2\server\win7_amd64_release\_nvspcaps64.pdb` **[P]**),
`nvspapi64.dll` (API), `nvsphelper64.exe`, `capcore64.dll`,
`NvRemux64.dll`, `NvRtmpStreamer64.dll`, `nvspscreenshot64.dll`. The shim
also ships inside the *driver* package (`System32\nvspcap64.dll`), see doc 05.

## 1.5 Consequence for our DLL

Our `NvCapture/nvspcap.dll` must export exactly the three names, with the
reference ordinals (1/2/3) pinned through a `.def` file, and nothing else.
