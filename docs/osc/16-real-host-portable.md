# 16 — REAL HOST PORTABLE (achieved 2026-09-19)

## RESULT
Real NVIDIA ShadowPlay host (GFE 3.28.0.412) runs 100% natively, installed
WITHOUT the GFE installer. Validated on GTX 1080 Ti machine: Alt+Z overlay,
IR, capture pipeline - all features native and working.

Portable package: `C:\Users\ScotcsDuluka\Downloads\NVIDIA-Host-Portable\`
(install-host.ps1 = one-click, includes every fix below).

## WHY THE INSTALLER FAILED ("Required files are missing")
The pre-extracted distribution was missing 2 files that NvBackend.nvi's
manifest REQUIRES (CopyOrCheck):
    NvBackend\NvTmRep.exe   (1.6MB)
    NvBackend\NvSHIM.exe    (1.4MB)
They ship inside GeForce_Experience_v3.28.0.412.exe (CDN copy also works:
us.download.nvidia.com/GFE/GFEClient/3.28.0.412/...). Extract with:
    GFExperience\7z.exe e <installer>.exe -o<dest> "NvBackend/NvTmRep.exe" "NvBackend/NvSHIM.exe"
The FIRST install of the night only succeeded because NVIDIA App v11.0.5
previously had these files on disk (CopyOrCheck = copy-from-package OR
check-at-destination). Uninstalling removed them -> every later run failed.

## THE FULL PROVISIONING RECIPE (all in install-host.ps1)
1. Files: GFE dir, ShadowPlay dir (incl NVSPCAPS\_nvspcaps64.dll),
   NvContainer (64-bit) -> Program Files; NvContainer x86 binaries +
   NvNode + NvBackend (incl NvTmRep/NvSHIM) -> Program Files (x86).
2. System32\nvspcap64.dll + SysWOW64\nvspcap.dll (in-game hook).
3. Registry BOTH views (HKLM\SOFTWARE + WOW6432Node - NvNode/clients are
   32-bit!):
   - Global\NvNode: port=59001 (DWORD), disableSecurity=1 (DWORD)
   - Global\GFExperience + Global\GeForce Experience: Version/Installed/
     FullPath (GetGFEVersionSync reads the WOW view - boot dies without)
   - Global\NVSCAPS: ShadowPlayMode=Manual
   - NvContainer\MessageBus (both views): LogPath/LogLevel/InstallPath
     (WOW InstallPath MUST point to the x86 NvContainer dir - 32-bit
     MessageBus.dll! Wrong path = MessageBus init fails system-wide =
     SDK + SP proxy dead + osc page boot fails = overlay closes itself)
   - NvContainer\ModuleMap (both views): MessageBus.dll, libprotobuf.dll,
     libcrypto-1_1.dll, libssl-1_1.dll, Poco.dll, PocoInitializer.dll
     (64-bit view -> Program Files NvContainer; WOW view -> x86 NvContainer)
   - Global\NvContainerLocalSystem: PluginFolderPath
   - NvContainer\Watchdog: LogFile
4. Service: New-Service NvContainerLocalSystem (ImagePath MUST quote the
   exe path - spaces!), then sdset DACL granting RP(start) to IU
   (interactive users) - otherwise unelevated Web Helper gets error 5.
5. Watchdog instance SPUserX64: Folder=plugins\SPUser,
   Container=nvcontainer.exe, Parameters(-f SPUser log -d SPUser dir -r -l
   3 -p 30000 -st TelemetryApi.dll), Policy=10/300/5.
   The SPUser container hosts nvspcaps\_nvspcaps64.dll = the capture
   service (ShadowplayServer on the MessageBus).
6. Launch order: service up first, then NVIDIA Share.exe (owns Alt+Z).

## RUNTIME TOPOLOGY (all real)
    Alt+Z -> nvsphelper64 hotkey plugin (MessageBus Hotkey:HotkeyPlugin)
          -> Shadowplay:ServicePlugin (LocalSystem container)
          -> Shadowplay:ShadowplayServer (SPUser container, capture core)
    Share.exe (CEF) <- QUERY_WIN_NODE_INFO <- Web Helper :59001
    NvShadowPlayAPI native <-> nvspapi(32) <-> IpcCommon <-> SP server
    MessageBus broadcast lives inside the LocalSystem container.

## STANDALONE BACKEND (for machines without the full agent chain)
optional-standalone-backend\index.js patches (all proven):
- stub NvBackendAPI (native NvBackendAPINode.node access-violates c0000005
  ~30s after boot when the agent is absent - NEVER load it)
- skip NvCameraAPI (native crash), skip GameStream initialize,
  skip ShadowPlayAPI-detached-wait (hangs without SP server),
  AccountAPI.initialize() MUST have .catch (consent failure otherwise
  leaves Promise.all pending forever = boot never reaches HTTP)
- DulukaAPI.js = drop-in NvAccountAPI replacement (see 13-duluka-account.md)
Deployment quirk: NVIDIA Web Helper.exe IGNORES argv - it always executes
the hardcoded <ProgramFilesX86>\NVIDIA Corporation\NvNode\index.js.

## INTEL / AMD MACHINES - status
- Works: host, Alt+Z, overlay UI, all settings, backend.
- Driver-bound (physically): NVFBC/NVENC capture, in-game compositing via
  nvspcap64. Capture-service init calls NvAPI_Initialize - fails without
  the driver, so native /ShadowPlay/* may 500; if the osc page refuses to
  boot on those machines, serve /ShadowPlay/* from a JS state module
  (same shapes OscControllerServer used) - not yet needed/tested.
- nvspcap64.dll was restored from payload\System32 (driver component).

## ENGINE (ShadowPlay.v1) STATUS
Idle on this machine by design (real host owns everything). Features added
tonight that remain useful: OscControllerServer reverse-proxy to the real
backend (RealBackendUsable + ProxyToWebHelper, /ShadowPlay/* 404->local
fallback) and HookCdpCapture endpoint-file publication
(engine-endpoint.json) for the ShadowPlayBridge concept.
