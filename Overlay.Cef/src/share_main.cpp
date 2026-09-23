// share_main.cpp - DLL entry surface for NVIDIA Share.dll.
// Exports NvShareCefMain: the full CEF process split (subprocess relaunch
// -> CefExecuteProcess; browser process -> config + loopback OSC server +
// CefInitialize + message loop). NVIDIA Share.exe loads this DLL and
// calls the export, mirroring the owner drawing "NvOverlay\CEF\
// NVIDIA Share.exe + NVIDIA Share.dll".
#include <stdio.h>

#include <string>

#include "include/cef_app.h"
#include "include/cef_browser.h"
#include "include/cef_command_line.h"
#include "include/cef_sandbox_win.h"

#include "share_app.h"
#include "share_client.h"
#include "share_config.h"
#include "share_http_server.h"
#include "share_json.h"
#include "share_launch.h"
#include "share_proof.h"
#include "share_storage.h"
#include "share_win.h"

namespace {

ShareLaunchParams* Params() {
  static ShareLaunchParams* p = new ShareLaunchParams();
  return p;
}

ShareHostContext* HostCtx() {
  static ShareHostContext* c = new ShareHostContext();
  return c;
}

// Resolves the OSC bundle root: --osc-root=... > <exeDir>\Resources\osc >
// <exeDir>\osc. Returns "" when none exists (logged).
std::string ResolveOscRoot(CefRefPtr<CefCommandLine> cl) {
  std::vector<std::wstring> candidates;
  if (cl->HasSwitch("osc-root")) {
    candidates.push_back(
        sharewin::Utf8ToWide(cl->GetSwitchValue("osc-root").ToString()));
  }
  const std::wstring exe_dir = sharewin::GetExeDir();
  candidates.push_back(exe_dir + L"\\Resources\\osc");
  candidates.push_back(exe_dir + L"\\osc");
  for (size_t i = 0; i < candidates.size(); ++i) {
    DWORD attr = GetFileAttributesW(candidates[i].c_str());
    if (attr != INVALID_FILE_ATTRIBUTES && (attr & FILE_ATTRIBUTE_DIRECTORY)) {
      return sharewin::WideToUtf8(candidates[i]);
    }
  }
  return "";
}

unsigned ParseUnsigned(const std::string& s, unsigned def) {
  if (s.empty()) return def;
  unsigned v = def;
  if (sscanf(s.c_str(), "%u", &v) == 1) return v;
  return def;
}

}  // namespace

ShareLaunchParams* GetLaunchParams() { return Params(); }
ShareHostContext* GetHostContext() { return HostCtx(); }

extern "C" __declspec(dllexport)
int NvShareCefMain(void) {
  // CEF requires CefExecuteProcess before any other CEF call; logging
  // first is fine (shareproof touches no CEF API).
  shareproof::Init(false);
  shareproof::Checkpoint("PROC_START", "NvShareCefMain entered");
  shareproof::LogLine("exe=" + sharewin::WideToUtf8(sharewin::GetExeDir()));

  CefMainArgs args(GetModuleHandleW(NULL));
  CefRefPtr<ShareApp> app(new ShareApp());

  int exit_code = CefExecuteProcess(args, app.get(), NULL);
  if (exit_code >= 0) {
    // Subprocess relaunch (renderer/gpu/utility) handled and finished.
    shareproof::LogLine("subprocess exit code " + std::to_string(exit_code));
    return exit_code;
  }
  shareproof::Checkpoint("BROWSER_PROCESS",
                         "CefExecuteProcess returned -1 (browser path)");

  // ── browser process only from here on ────────────────────────────────
  CefRefPtr<CefCommandLine> cl = CefCommandLine::CreateCommandLine();
  cl->InitFromString(GetCommandLineW());

  ShareLaunchParams* params = Params();
  params->proof_mode = cl->HasSwitch("proof-of-life");
  params->stay_open = cl->HasSwitch("stay-open");
  // Proof runs show the window by default (visible evidence + avoids the
  // hidden-window GPU/compositor stall measured on the first runs);
  // --hidden suppresses.
  params->show_window = !cl->HasSwitch("hidden");
  params->self_exit_ms =
      ParseUnsigned(cl->GetSwitchValue("self-exit-ms").ToString(), 25000);

  // Proof recording on (QPC timeline from PROC_START is preserved).
  if (params->proof_mode) {
    shareproof::EnableProofMode();
    shareproof::LogLine("proof-of-life mode, self-exit-ms=" +
                        std::to_string(params->self_exit_ms));
  }

  ShareConfig cfg;
  cfg.LoadFromExeDir();
  shareproof::LogLine("config: " + cfg.Describe());
  params->gpu_accel = cfg.nv_gpu_accel;
  shareproof::SetField("nvOsc", cfg.nv_osc ? "true" : "false");
  shareproof::SetField("nvGpuAccel", cfg.nv_gpu_accel ? "true" : "false");
  shareproof::SetField("nvUrlRelative",
                       sharejson::Escape(cfg.nv_url_relative));

  // OSC bundle + loopback server (production model: the page loads over
  // localhost HTTP, never file:// — OscControllerServer.vb:1553 parity).
  std::string osc_root = ResolveOscRoot(cl);
  static OscHttpServer server;
  if (osc_root.empty()) {
    shareproof::Checkpoint("OSC_BUNDLE_MISSING",
                           "no osc root found (tried --osc-root, "
                           "<exeDir>\\Resources\\osc, <exeDir>\\osc)");
  } else {
    std::string err;
    if (server.Start(osc_root, &err)) {
      shareproof::Checkpoint(
          "HTTP_SERVER_UP",
          "http://127.0.0.1:" + std::to_string(server.port()) +
              "/ serving " + osc_root);
    } else {
      shareproof::Checkpoint("HTTP_SERVER_FAILED", err);
    }
  }

  HostCtx()->http_port = server.port();
  HostCtx()->secret = sharewin::RandomHex(16);
  shareproof::SetField("httpPort", std::to_string(server.port()));
  shareproof::SetField("secretLength",
                       std::to_string(HostCtx()->secret.size()));
  shareproof::SetField("oscRoot", sharejson::Escape(osc_root));

  // URL: --url= override, else the nv-url-relative basename on our
  // server. Host is 127.0.0.1 (the engine's OscHostForm navigates
  // "localhost" via WebView2; measured on this machine, Chromium 73
  // stalls ~100s resolving "localhost" before the first navigation, so
  // the CEF lane pins the loopback literal).
  if (cl->HasSwitch("url")) {
    params->url = cl->GetSwitchValue("url").ToString();
  } else {
    std::string page = "index.html";
    size_t slash = cfg.nv_url_relative.find_last_of('/');
    if (slash != std::string::npos && slash + 1 < cfg.nv_url_relative.size()) {
      page = cfg.nv_url_relative.substr(slash + 1);
    }
    params->url = "http://127.0.0.1:" + std::to_string(server.port()) + "/" +
                  page;
  }
  shareproof::SetField("url", sharejson::Escape(params->url));
  shareproof::LogLine("navigating to " + params->url);

  // Storage under the exe's Data\ (SharedStorageStore.vb parity:
  // Data\osc-shared-storage.json).
  const std::wstring exe_dir = sharewin::GetExeDir();
  CreateDirectoryW((exe_dir + L"\\Data").c_str(), NULL);
  static ShareStorage storage(exe_dir + L"\\Data\\osc-shared-storage.json");
  HostCtx()->storage = &storage;

  CefSettings settings;
  settings.size = sizeof(CefSettings);
  settings.no_sandbox = 1;  // sandbox not linked (no cef_sandbox.lib)
  settings.multi_threaded_message_loop = false;
  settings.windowless_rendering_enabled = false;
  settings.log_severity = LOGSEVERITY_INFO;
  CefString(&settings.log_file).FromString(
      sharewin::WideToUtf8(exe_dir + L"\\Logs\\cef-debug.log"));
  // GFE 3.28 reference layout: cef.pak + locales\ sit in the CEF root
  // beside NVIDIA Share.exe (NOT under Resources\ — the crash of the
  // first proof run measured this: "Could not load cef.pak").
  CefString(&settings.resources_dir_path).FromString(
      sharewin::WideToUtf8(exe_dir));
  CefString(&settings.locales_dir_path).FromString(
      sharewin::WideToUtf8(exe_dir + L"\\locales"));
  CefString(&settings.cache_path).FromString(
      sharewin::WideToUtf8(exe_dir + L"\\Data\\cef-cache"));
  CefString(&settings.user_data_path).FromString(
      sharewin::WideToUtf8(exe_dir + L"\\Data\\cef-user-data"));

  if (!CefInitialize(args, settings, app.get(), NULL)) {
    shareproof::Checkpoint("CEF_INIT_FAILED", "CefInitialize returned false");
    shareproof::WriteProof();
    return 3;
  }
  shareproof::Checkpoint("CEF_INIT_OK", "CefInitialize returned true");

  CefRunMessageLoop();
  CefShutdown();
  server.Stop();

  const int code = shareproof::ExitCode();
  shareproof::LogLine("host exit code " + std::to_string(code));
  return code;
}
