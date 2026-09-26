# Overlay.Cef — CEF lane (NvOverlay\CEF owner, NVIDIA Share.exe)

Real CEF-based OSC host, separated from the WinForm lane and from the
WebView2 engine host. Production target name: **NVIDIA Share.exe** (+ host
body **NVIDIA Share.dll**), owner slot **NvOverlay\CEF\** per
`Logical Architecture\Layout.txt` v2.

Runtime: pinned CEF **73.1.12+gee4b49f+chromium-73.0.3683.75** (windows64
minimal from cef-builds.spotifycdn.com, sha1-verified) — the same CEF
commit family as the real GFE 3.28 `libcef.dll`, and the exact Chromium of
the OSC bundle's era. No WebView2 anywhere in this lane.

## Build

```
"C:\Visual Studio\MSBuild\Current\Bin\MSBuild.exe" Overlay.Cef\NVIDIA Share.vcxproj -p:Configuration=Release -p:Platform=x64
"C:\Visual Studio\MSBuild\Current\Bin\MSBuild.exe" Overlay.Cef\NVIDIA Share Bootstrap.vcxproj -p:Configuration=Release -p:Platform=x64
```
(the DLL project references the wrapper project; both build in one shot via
the DLL project). SDK path override: `/p:CefSdk=C:\path\to\cef73\`.

## Stage + prove

```
powershell -File Overlay.Cef\deploy-cef-owner.ps1      # stage NvOverlay\CEF owner slot
powershell -File Overlay.Cef\run-proof-of-life.ps1     # PASS/FAIL proof driver
```

## Key facts

- `src\share_query_handler.cpp` = 1:1 port of the proven
  `Overlay.Engine\CefQueryBridge.vb` dispatch onto the native CEF message
  router — `PROTOCOL-PARITY.md` maps every command line-by-line.
- `src\share_http_server.cpp` = loopback static server for the OSC bundle
  (production model: page loads over localhost HTTP; extensionless paths
  503 so page services degrade, never hang).
- `--proof-of-life --self-exit-ms=20000` runs the proof chain
  (start → CEF init → page load → cefQuery round-trip) and writes
  `Logs\proof-of-life-<pid>.json`; `--stay-open` keeps the UI up for
  visual capture; `--hidden` suppresses the window.
- Evidence dossier: `EVIDENCE.md` · runtime manifest: staged
  `CEF-RUNTIME-MANIFEST.txt`.
