# NvCapture — nvspcap ABI Evidence Pack

Reverse-engineering evidence for the NVIDIA ShadowPlay in-process shim
(`nvspcap.dll` / `nvspcap64.dll`), produced **before** implementing our own
compatible `NvCapture/nvspcap.dll`.

Reference binary used (READ-ONLY, never redistributed, never copied into
this repo):

```
C:\My Project\GFE\GeForce_Experience_v3.28.0.412\ShadowPlay\nvspcap64.dll
  size 2900520, x64, image base 0x180000000
C:\My Project\GFE\GeForce_Experience_v3.28.0.412\ShadowPlay\nvspcap.dll
  size 2231336, x86 (same 3-export contract)
```

Cross-check binary (driver-shipped, read-only):
`C:\Windows\System32\nvspcap64.dll` (1,456,752 bytes, driver 32.0.15.8266,
same 3 exports at different RVAs → contract stable across builds).

## Evidence conventions

| Tag | Meaning |
|-----|---------|
| **[P]** | **Proven** — direct code evidence: disassembly of the referenced RVA, or a string→function cross-reference through `.pdata`-bounded functions |
| **[O]** | **Observed** — binary metadata fact (export table, imports, PE headers, file presence) |
| **[H]** | **Hypothesis** — explicitly marked inference. **Never implemented as fact**; our shim declines instead of guessing |

Rule of the lane (per task card): *Evidence > Hypothesis*. Nothing in our
implementation relies on an [H] item; every [H] path is implemented as a
graceful decline.

## Documents

| File | Question answered |
|------|-------------------|
| [01-export-contract.md](01-export-contract.md) | (1) export contract |
| [02-ddi-shim-interface.md](02-ddi-shim-interface.md) | (2, 4) DDI shim interface: version negotiation + full slot map |
| [03-proxy-shim-interface.md](03-proxy-shim-interface.md) | (2, 3) CShadowPlayProxyShim vtable + callback surface |
| [04-status-query-and-lifecycle.md](04-status-query-and-lifecycle.md) | (3) status query, lifecycle order |
| [05-load-path.md](05-load-path.md) | (5) who loads nvspcap and who calls what |
| [06-compat-surface.md](06-compat-surface.md) | (6) the minimal safe compatibility surface we implement |

## Tooling (ours, zero NVIDIA content)

* `tools/strings_scan.py` — ASCII/UTF-16 string scan with RVA mapping.
* `tools/abi_probe.py` — capstone disassembly at any RVA with string/IAT/section annotations.
* `tools/map_interface_slots.py` — recovers versioned interface tables from annotated disasm.
* `tools/vtable_scan.py` — `.pdata`-based function index, string xref search, vtable discovery.

Raw dumps (disassembly, export/import tables, string scans) live in
`dumps/` — **gitignored**, never committed (they are derived from the
NVIDIA binary; only distilled facts + RVAs are committed).

## Headline findings

1. The export contract is **3 exports total** (both architectures, both builds): `QueryShadowPlayDdiShimInterface` @1, `QueryShadowPlayDdiShimStatus` @2, `CreateShadowPlayProxyShimInterface` @3. Every other name on the task card (`Shim*`, `Ddi*`, `CShadowPlayProxyShim`) is an internal method tag, not an export. [O]
2. `QueryShadowPlayDdiShimInterface(ver, void** ppOut)` dispatches on version codes where **low 16 bits = struct byte size** (`0x10078`→15 slots, `0x10088`→17, `0x40090`→18, `0x400A0`→20) and fills a caller-visible function-pointer table in `.data`. The full slot→semantics map is **[P]** for 19 of 20 slots. [P]
3. The DDI interface consumer is the **graphics UMD** (`nvd3dumx.dll`, `nvoglv64.dll` and their 32-bit counterparts), which references `nvspcap64.dll` and the `Query*` names as strings — not `nvsphelper64.exe`, which contains no `nvspcap` reference. [P/O]
4. `CreateShadowPlayProxyShimInterface` is referenced **by name nowhere** — its caller must use **ordinal 3**. [O]
5. NVIDIA's own binary contains a **12-slot no-op vtable** (all slots → one stub) used as the dead proxy state — the precedent for our "decline gracefully" compatibility design. [P]
