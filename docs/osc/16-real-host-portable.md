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


---

# INTEL VALIDATION ADDENDUM (validated on HUAWEI-PC, Intel UHD, no NVIDIA)

## RESULT: overlay opens on a machine with ZERO NVIDIA hardware.
Full pipeline: Alt+Z -> listener -> POST /ShadowPlay/v.1.0/Hotkey/Toggle ->
socket emit WindowState{overlayToggle} -> page opens via QUERY_WIN_OPEN_OSC.
Settings pages navigate; hardware data flows from per-machine floor.

## HOTKEY WIRE-THROUGH (who owns Alt+Z)
- Share.exe: NO hotkey code at all (no overlayToggle/RegisterHotKey strings).
- nvsphelperplugin64.dll: the real owner (RegisterHotKey + OpenShare inside)
  - spawns only when the capture stack is ready = driver-bound
  - on non-NVIDIA machines it NEVER appears -> hotkey is silent by nature
- Solution: hotkey-listener.ps1 (RegisterHotKey Alt+Z itself, autostart
  HKCU Run) -> POST /ShadowPlay/v.1.0/Hotkey/Toggle -> backend emits
  WindowState{overlayToggle} (same frame the real stack sends).
  On NVIDIA machines the listener FAILS to register (owner exists) and
  withdraws itself - zero conflict by design.

## FLOOR SHAPES (what the page reads - all values are STRINGS)
- GET /HardwareInformation/v.0.2  (v.0.2! not v.1.0; no subpath)
  fields the page reads: GPU[].LongGPUName, CPUName, PhysicalMemoryCapacity,
  CurrentResolution, OSName + MoboType/BIOSVersion/JarvisDeviceId/
  TelemetryDeviceId/UserDefaultUILanguage/ProcessorArchitecture/OSVersion/
  OSBuildNumber/TotalPhysicalMemory/PCName/DriverVersion/IsDCHDriverInstalled/
  DriverType/SLISupported/HasActiveSLITopology/ActiveTopologyGPUCount/IsOptimus
  GPU[]: LongGPUName/ActualVRAMSize/GPURAMType/VBIOSVersion/IsQuadro/DeviceId/
  VendorId/SubSystemId/SubVendorId/SystemType/BrandType/PhysicalGPUHandle/
  GPUArchitecture("" for Intel)/GPUArchRevision/GPUArchVersion/
  GPUArchImplementation/IsPrimary
  -> gfwsl validates DeviceId+VendorId (NOT "DID" - wrong name made it spam)
- GET /FramerateLimiter/v.0.1/state = {"enabled":false,"value":0}
  (v.0.1 has NO "supported" field - that is v.1.0/2)
- GET /ShadowPlay/v.1.0/Resolutions = {"resolutions":["In-game","2160p 4K",...]} (strings!)
- GET /ShadowPlay/v.1.0/FrameRates = {"framerates":[60,30]}
- POST /ShadowPlay/v1.0/OSC/GetCustomize/{Record|InstantReplay|Broadcast}
  body MUST contain quality+resolution+framerate+bitrateBps (missing one =
  500 "Argument doesn't have 'X' property" - error names the missing field)
  response: resolutions[] + framerates[] + quality/resolution/framerate +
  bitrate{current,min,max,default}  <- slider bounds for the video page
- GetCustomize is REST POST, NOT socket. HardwareInformation data also
  flows to the page via systemInfoUpdated socket event.
- install-host.ps1 now generates hardware-floor.json per machine via WMI.

## PAGE BEHAVIOR NOTES (observed on Intel)
- Click-outside dismisses the overlay (normal) - synthetic clicks that land
  outside the overlay rect close it; not a bug.
- GPU process "Exiting GPU process due to errors during initialization"
  appears on both machines - CEF falls back to software/dx9; normal.
- Socket emits over engine.io v3 POLLING transport deliver on the client's
  next poll (up to ~25s) - wait a full poll cycle before assuming death.
- Page boot crashes if ANY init-chain endpoint returns a thin shape:
  unsupportReason.indexOf(), info.GPU[0], octool overlayViews[0] - every
  floor must match the real shape or the whole chain dies before
  oscDisplayService.init (the /WindowState handler) registers.
- osc page logs flow through user-config.js console bridge -> Debug/PageLog.

## GIT MERGE NOTE
Both machines hold doc-16 versions (1080 Ti blueprint + Intel addendum).
This file = the merged master. Intel-side stash (their addendum edits)
should be re-applied only for anything missing here.
