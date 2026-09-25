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

// The TCP hub ("NVIDIA API") is staged as NvBackend\NvBackend.exe in the
// current tree (Nv<App>.exe rename wave). The historical name was
// "NVIDIA Backend" — treat BOTH as the hub lane for status/start/kill.
const wchar_t* kHub = L"NvBackend";
const wchar_t* kHubLegacy = L"NVIDIA Backend";

bool HubRunning() {
  return launcherutil::ProcessRunning(kHub) ||
         launcherutil::ProcessRunning(kHubLegacy);
}

// Installer-exit kill list — Main.vb RadioButton2 order + the staged hub
// name (NvBackend.exe — the "NVIDIA Backend" slot of the current tree).
const wchar_t* kKillList[] = {
    L"NVIDIA Notifier.exe", L"NVIDIA ShadowPlay.exe", L"nvsphelper64.exe",
    L"NvContainer.exe",     L"NvBackend.exe",         L"NVIDIA Backend.exe",
    L"NVIDIA Capture.exe",
};

void StartIfMissing(const wchar_t* name, const std::wstring& exe_path,
                    const std::wstring& args = L"") {
  if (!launcherutil::FileExists(exe_path)) return;  // logged by StartProcess
  if (launcherutil::ProcessRunning(name)) {
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
  st.container = launcherutil::ProcessRunning(kContainer);
  st.web_helper = launcherutil::ProcessRunning(kWebHelper);
  st.cef_overlay = launcherutil::ProcessRunning(kCefOverlay);
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
      if (!launcherutil::ProcessRunning(kContainer)) {
        LogLine("NvContainer lost - restarting");
        StartIfMissing(kContainer, RootP(L"NvContainer", L"NvContainer.exe"));
      }
      if (!HubRunning()) {
        LogLine("hub lost - restarting");
        launcherutil::StartProcess(ResolveHubExe(), L"");
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
  StartIfMissing(kWebHelper, RootP(L"NvBackend", L"NVIDIA Web Helper.exe"));
  // Wait for the node backend (:59001) before the CEF overlay, so the
  // page and the ShadowPlay v1.0 REST surface come up same-origin.
  launcherutil::WaitForTcpPort(59001, 10000);
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
    LogLine("engine overlay chain already running");
    return;
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
  // Overlay Mode -> WINFORM: bring the CEF lane down (only the NVIDIA
  // Share.exe running from NvOverlay\Cef — other instances of that name
  // belong to different lanes and stay). The WinForm family itself is
  // hub-managed (Overlay.UseOverlayEnabled), untouched here.
  std::wstring share_exe = RootP(L"NvOverlay\\Cef", L"NVIDIA Share.exe");
  launcherutil::KillProcessFromPath(kCefOverlay, share_exe);
}

bool LauncherSupervisor::SendOpenOverlay() {
  // TcpClientHelper.Send("open_overlay") wire parity:
  //   "[Send] NVIDIA  APP|open_overlay\r\n" to 127.0.0.1:5001.
  return launcherutil::SendHubLine("[Send] NVIDIA  APP|open_overlay");
}

void LauncherSupervisor::InstallerExit() {
  // Single-source config: clear the overlay switch (Main.vb RadioButton2).
  launcherutil::WriteConfigBool(L"Overlay", L"UseOverlayEnabled", false);
  for (size_t i = 0; i < sizeof(kKillList) / sizeof(kKillList[0]); ++i) {
    launcherutil::KillProcessByName(kKillList[i]);
  }
  LogLine("installer exit complete");
}
