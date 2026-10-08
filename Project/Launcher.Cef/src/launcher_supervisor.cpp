// launcher_supervisor.cpp - see launcher_supervisor.h.
#include "launcher_supervisor.h"

#include <stdio.h>

#include <windows.h>

#include "launcher_util.h"

using launcherutil::LogLine;

namespace {

// Process image names WITHOUT .exe — Process.GetProcessesByName parity.
const wchar_t* kShadowPlay = L"NVIDIA ShadowPlay";
const wchar_t* kNotifier = L"NVIDIA Notifier";
const wchar_t* kContainer = L"NvContainer";
const wchar_t* kWebHelper = L"NVIDIA Web Helper";
const wchar_t* kCefOverlay = L"NVIDIA Share";

// The TCP hub ("NVIDIA API") uses NvBackend.exe in the current tree; the
// older NVIDIA Backend.exe path remains a compatibility candidate.
const wchar_t* kHub = L"NvBackend";

std::wstring RootP(const wchar_t* folder, const wchar_t* file);

bool HubRunning() {
  const std::wstring candidates[] = {
      RootP(L"NvBackend", L"NvBackend.exe"),
      RootP(L"NvBackend", L"NVIDIA Backend.exe"),
      RootP(L"", L"NVIDIA Backend.exe"),
  };
  for (size_t i = 0; i < sizeof(candidates) / sizeof(candidates[0]); ++i) {
    const std::wstring& path = candidates[i];
    const size_t slash = path.find_last_of(L'\\');
    const size_t dot = path.find_last_of(L'.');
    const std::wstring name = path.substr(
        slash == std::wstring::npos ? 0 : slash + 1,
        (dot == std::wstring::npos ? path.size() : dot) -
            (slash == std::wstring::npos ? 0 : slash + 1));
    if (launcherutil::ProcessRunningFromPath(name.c_str(), path)) return true;
  }
  return false;
}

void StartIfMissing(const wchar_t* name, const std::wstring& exe_path,
                    const std::wstring& args = L"") {
  if (!launcherutil::FileExists(exe_path)) {
    LogLine(std::string("service executable missing: ") +
            launcherutil::WideToUtf8(exe_path));
    return;
  }
  const size_t slash = exe_path.find_last_of(L'\\');
  const size_t dot = exe_path.find_last_of(L'.');
  const std::wstring image_name = exe_path.substr(
      slash == std::wstring::npos ? 0 : slash + 1,
      (dot == std::wstring::npos ? exe_path.size() : dot) -
          (slash == std::wstring::npos ? 0 : slash + 1));
  if (launcherutil::ProcessRunningFromPath(image_name.c_str(), exe_path)) {
    LogLine(std::string("adopt running: ") + launcherutil::WideToUtf8(name));
    return;
  }
  launcherutil::StartProcess(exe_path, args);
}

std::wstring RootP(const wchar_t* folder, const wchar_t* file) {
  return launcherutil::JoinPath(
      launcherutil::JoinPath(launcherutil::GetRootDir(), folder), file);
}

// First existing hub exe wins: staged name, legacy name, then the layout
// root (dev bin contract).
std::wstring ResolveHubExe() {
  const std::wstring candidates[] = {
      RootP(L"NvBackend", L"NvBackend.exe"),
      RootP(L"NvBackend", L"NVIDIA Backend.exe"),
      RootP(L"", L"NVIDIA Backend.exe"),
  };
  for (size_t i = 0; i < 3; ++i) {
    if (launcherutil::FileExists(candidates[i])) return candidates[i];
  }
  return candidates[0];
}

}  // namespace

std::string LauncherState::ToJson() const {
  // Overlay API label contract (Main.vb IF_APP_Tick):
  //   !shadowplay || overlay_ready -> "OVERLAY API" else "Loading..."
  const char* overlay_label =
      (!shadowplay || overlay_ready) ? "OVERLAY API" : "LOADING";
  std::string o = "{";
  o += "\"overlayApi\":{\"running\":" + std::string(shadowplay ? "true" : "false") +
       ",\"ready\":" + std::string(overlay_ready ? "true" : "false") +
       ",\"label\":\"" + overlay_label + "\"}";
  o += ",\"notifierApi\":{\"running\":" + std::string(notifier ? "true" : "false") + "}";
  o += ",\"nvApi\":{\"running\":" + std::string(nv_api ? "true" : "false") + "}";
  o += ",\"lanes\":{\"container\":" + std::string(container ? "true" : "false") +
       ",\"webHelper\":" + std::string(web_helper ? "true" : "false") +
       ",\"cefOverlay\":" + std::string(cef_overlay ? "true" : "false") + "}";
  o += ",\"overlayEnabled\":" + std::string(overlay_enabled ? "true" : "false");
  o += ",\"engineOverlay\":" + std::string(engine_overlay ? "true" : "false");
  o += "}";
  return o;
}

LauncherSupervisor* LauncherSupervisor::Get() {
  static LauncherSupervisor* s = new LauncherSupervisor();
  return s;
}

LauncherState LauncherSupervisor::Snapshot() {
  LauncherState st;
  st.nv_api = HubRunning();
  st.shadowplay = launcherutil::ProcessRunning(kShadowPlay);
  st.overlay_ready =
      launcherutil::FileExists(RootP(L"Flags", L"Ready"));
  st.notifier = launcherutil::ProcessRunning(kNotifier);
  st.container = launcherutil::ProcessRunningFromPath(
      kContainer, RootP(L"NvContainer", L"NvContainer.exe"));
  st.web_helper = launcherutil::ProcessRunningFromPath(
      kWebHelper, RootP(L"NvNode", L"NVIDIA Web Helper.exe"));
  st.cef_overlay = launcherutil::ProcessRunningFromPath(
      kCefOverlay, RootP(L"NvOverlay\\Cef", L"NVIDIA Share.exe"));
  st.overlay_enabled =
      launcherutil::ReadConfigBool(L"Overlay", L"UseOverlayEnabled", false);
  st.engine_overlay =
      launcherutil::ReadConfigBool(L"Overlay", L"EngineOverlayMode", false);
  return st;
}

void LauncherSupervisor::Start(bool supervise, PushFn push) {
  supervise_ = supervise;
  push_ = push;
  stop_ = 0;
  if (supervise_) StartBaseChain();
  // Engine mode persisted as CEF: bring the genuine chain up at boot too
  // (the engine chain otherwise starts only on the mode toggle).
  if (supervise_ &&
      launcherutil::ReadConfigBool(L"Overlay", L"EngineOverlayMode", false)) {
    StartEngineOverlayChainAsync();
  }
  poll_thread_ = CreateThread(NULL, 0, PollTramp, this, 0, NULL);
}

void LauncherSupervisor::Stop() {
  InterlockedExchange(&stop_, 1);
  if (poll_thread_) {
    WaitForSingleObject(poll_thread_, 2500);
    CloseHandle(poll_thread_);
    poll_thread_ = NULL;
  }
  if (chain_thread_) {
    // The chain thread is bounded by the :59001 wait (<=10s); detach-safe.
    CloseHandle(chain_thread_);
    chain_thread_ = NULL;
  }
}

unsigned long LauncherSupervisor::PollTramp(void* self) {
  static_cast<LauncherSupervisor*>(self)->PollLoop();
  return 0;
}

unsigned long LauncherSupervisor::ChainTramp(void* self) {
  static_cast<LauncherSupervisor*>(self)->StartEngineOverlayChain();
  return 0;
}

void LauncherSupervisor::PollLoop() {
  // 1s tick = Main.vb IF_APP.Interval. First snapshot goes out immediately
  // so the page paints real state without a startup blink.
  int tick = 0;
  while (!stop_) {
    LauncherState st = Snapshot();
    std::string json = st.ToJson();
    if (json != last_pushed_ && push_) {
      last_pushed_ = json;
      push_(json);
    }
    // Main.vb parity: if the NOTIFIER lane is down, the Ready marker is
    // stale — remove it (the overlay writes it when its API surface opens).
    if (!st.notifier && st.overlay_ready && supervise_) {
      DeleteFileW(RootP(L"Flags", L"Ready").c_str());
      LogLine("stale Flags\\Ready removed (notifier down)");
    }
    // Slow lane housekeeping (every 5s): keep the base chain alive.
    if (supervise_ && (++tick % 5) == 0) {
      if (!launcherutil::ProcessRunningFromPath(
              kContainer, RootP(L"NvContainer", L"NvContainer.exe"))) {
        LogLine("NvContainer lost - restarting");
        StartIfMissing(kContainer, RootP(L"NvContainer", L"NvContainer.exe"));
      }
      if (!HubRunning()) {
        const std::wstring hub_exe = ResolveHubExe();
        if (launcherutil::FileExists(hub_exe)) {
          LogLine("hub lost - restarting");
          StartIfMissing(kHub, hub_exe);
        }
      }
    }
    for (int i = 0; i < 10 && !stop_; ++i) Sleep(100);
  }
}

void LauncherSupervisor::StartBaseChain() {
  // Supervisor lane runs ALWAYS (owner 2026-09-25): NvContainer starts
  // together with the Launcher and keeps nvsphelper64 alive.
  StartIfMissing(kContainer, RootP(L"NvContainer", L"NvContainer.exe"));
  // TCP hub (WinForm family contract): NvBackend\NvBackend.exe.
  StartIfMissing(kHub, ResolveHubExe());
}

void LauncherSupervisor::StartEngineOverlayChain() {
  LogLine("engine overlay chain: begin");
  StartIfMissing(kContainer, RootP(L"NvContainer", L"NvContainer.exe"));
  // Backend lane (owner 2026-09-26, decisive): the GENUINE NvNode v11 was
  // CUT — its trusted-location loader refuses anything outside the NVIDIA
  // install and its container handshake needs NVIDIA's own service stack.
  // The CEF overlay runs against OUR backend: root NvNode\ = the managed
  // NVIDIA Web Helper.exe hosting Backend\index.js (parity 11/11,
  // same-origin osc on :59001, no cookie in standalone mode).
  StartIfMissing(kWebHelper, RootP(L"NvNode", L"NVIDIA Web Helper.exe"));
  // Wait for the node backend (:59001) before the CEF overlay, so the
  // page and the ShadowPlay v1.0 REST surface come up same-origin.
  launcherutil::WaitForTcpPort(59001, 15000);
  std::wstring share_exe = RootP(L"NvOverlay\\Cef", L"NVIDIA Share.exe");
  if (!launcherutil::ProcessRunningFromPath(kCefOverlay, share_exe)) {
    launcherutil::StartProcess(share_exe, L"--backend-port 59001");
  } else {
    LogLine("CEF overlay already running from NvOverlay\\Cef - adopted");
  }
  LogLine("engine overlay chain: done");
}

void LauncherSupervisor::StartEngineOverlayChainAsync() {
  if (chain_thread_) {
    if (WaitForSingleObject(chain_thread_, 0) == WAIT_OBJECT_0) {
      CloseHandle(chain_thread_);
      chain_thread_ = NULL;
    } else {
      LogLine("engine overlay chain already running");
      return;
    }
  }
  chain_thread_ = CreateThread(NULL, 0, ChainTramp, this, 0, NULL);
  // The thread self-terminates after the chain; the handle is reaped in
  // Stop() or on the next chain request once done.
  if (WaitForSingleObject(chain_thread_, 0) == WAIT_OBJECT_0) {
    CloseHandle(chain_thread_);
    chain_thread_ = NULL;
  }
}

void LauncherSupervisor::StopCefOverlay() {
  // Overlay Mode -> WINFORM: stop only our CEF overlay window. Leave its
  // backend warm; other NVIDIA Web Helper instances may belong to NVIDIA.
  std::wstring share_exe = RootP(L"NvOverlay\\Cef", L"NVIDIA Share.exe");
  launcherutil::KillProcessFromPath(kCefOverlay, share_exe);
}

bool LauncherSupervisor::SendOpenOverlay() {
  if (launcherutil::ReadConfigBool(L"Overlay", L"EngineOverlayMode", false)) {
    // CEF lane: make sure the genuine chain is up first (a Launcher boot
    // with CEF persisted runs the engine chain async, but the port wait
    // may still be in flight — or the lane was stopped externally).
    if (!launcherutil::WaitForTcpPort(59001, 500)) {
      LogLine("OPEN OVERLAY (CEF): chain down - starting");
      StartEngineOverlayChain();
    }
    // Genuine hotkey parity: the node broadcasts WindowState and the
    // genuine page opens. disableSecurity=1 provisioning means no cookie
    // header is needed from outside the chain.
    return launcherutil::HttpPostLocal(
        59001, L"/ShadowPlay/v.1.0/Hotkey/Toggle");
  }
  // WINFORM lane: TcpClientHelper.Send("open_overlay") wire parity:
  //   "[Send] NVIDIA  APP|open_overlay\r\n" to 127.0.0.1:5001.
  return launcherutil::SendHubLine("[Send] NVIDIA  APP|open_overlay");
}

void LauncherSupervisor::InstallerExit() {
  // Single-source config: clear the overlay switch (Main.vb RadioButton2).
  launcherutil::WriteConfigBool(L"Overlay", L"UseOverlayEnabled", false);
  const struct { const wchar_t* name; const wchar_t* path; } owned[] = {
      {L"NVIDIA Notifier", L"NvOverlay\\WinForm\\NVIDIA Notifier.exe"},
      {L"NVIDIA ShadowPlay", L"NvOverlay\\WinForm\\NVIDIA ShadowPlay.exe"},
      {L"nvsphelper64", L"ShadowPlay\\nvsphelper64.exe"},
      {L"nvsphelper64", L"NvContainer\\nvsphelper64.exe"},
      {L"NvContainer", L"NvContainer\\NvContainer.exe"},
      {L"NvBackend", L"NvBackend\\NvBackend.exe"},
      {L"NVIDIA Backend", L"NvBackend\\NVIDIA Backend.exe"},
      {L"NVIDIA Backend", L"NVIDIA Backend.exe"},
      {L"NvCapture", L"NvContainer\\CaptureEngine\\NvCapture.exe"},
      {L"NVIDIA Share", L"NvOverlay\\Cef\\NVIDIA Share.exe"},
      {L"NVIDIA Web Helper", L"NvNode\\NVIDIA Web Helper.exe"},
      {L"nvnodejslauncher", L"NvNode\\nvnodejslauncher.exe"},
  };
  for (size_t i = 0; i < sizeof(owned) / sizeof(owned[0]); ++i) {
    launcherutil::KillProcessFromPath(
        owned[i].name, RootP(L"", owned[i].path));
  }
  LogLine("installer exit complete");
}
