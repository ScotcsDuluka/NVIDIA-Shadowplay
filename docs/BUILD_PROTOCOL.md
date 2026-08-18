# Build Protocol

> **Critical**: Run this **every time** after `git pull`. Skipping
> clean step is the #1 cause of false-positive bugs (stale DLLs).

## Why

The Engine project is built as `NVIDIA Capture.dll` and copied into
the Overlay's output folder at build time:

```
Engine/NVIDIA Capture.vbproj
  ↓ dotnet build
Engine/bin/Release/net8.0-windows10.0.26100.0/NVIDIA Capture.dll
  ↓ Overlay build (ProjectReference)
Overlay/bin/Release/net8.0-windows10.0.26100.0/NVIDIA Capture.dll  ← runtime
```

When you change Engine source but don't clean, the old `NVIDIA Capture.dll`
in Overlay's bin folder may persist. Stack traces will show the Overlay
path, but the code inside is stale.

## Required Build Sequence (Windows cmd)

```cmd
cd "C:\My Project\NVIDIA-Shadowplay"

:: 1. Pull latest
git fetch origin
git switch Engine-Audio
git pull origin Engine-Audio

:: 2. Clean (CRITICAL — prevents stale DLL)
dotnet clean ".\Engine\NVIDIA Capture.vbproj" -c Release
dotnet clean ".\Overlay\NVIDIA Overlay.vbproj" -c Release
rmdir /s /q Engine\bin
rmdir /s /q Engine\obj
rmdir /s /q Overlay\bin
rmdir /s /q Overlay\obj

:: 3. Build Engine first
dotnet build ".\Engine\NVIDIA Capture.vbproj" -c Release

:: 4. Then build Overlay (depends on Engine via ProjectReference)
dotnet build ".\Overlay\NVIDIA Overlay.vbproj" -c Release
```

## Verify Build Succeeded

After build, check the runtime DLL timestamp:

```cmd
dir "Overlay\bin\Release\net8.0-windows10.0.26100.0\NVIDIA Capture.dll"
```

The timestamp must match "now" — if it shows an old date/time, the
build didn't reach Overlay, or you forgot to clean.

## Quick Sanity Check

Run the app, trigger a record, check the log file for the FFmpeg
command line. It should reflect the latest source code.

If you see old behavior in logs despite source changes → **stale DLL**.
Re-run the clean build sequence above.
