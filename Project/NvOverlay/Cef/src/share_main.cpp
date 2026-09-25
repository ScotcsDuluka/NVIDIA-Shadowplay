// share_main.cpp - DLL entry surface for NVIDIA Share.dll.
// Exports NvShareCefMain: the full CEF process split (subprocess relaunch
// -> CefExecuteProcess; browser process -> config + loopback OSC server +
// CefInitialize + message loop). NVIDIA Share.exe loads this DLL and
// calls the export, mirroring the owner drawing "NvOverlay\CEF\
// NVIDIA Share.exe + NVIDIA Share.dll".
// winsock2.h MUST precede any windows.h include (cef headers pull it in)
// so the older winsock.h is suppressed by _WINSOCKAPI_.
#include <winsock2.h>
#include <ws2tcpip.h>

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

// Production Web Helper port (NvNode loopback) — the same default the
// backend config carries; --backend-port overrides.
constexpr unsigned kDefaultBackendPort = 59001;

// TCP probe: is the production backend accepting on the loopback port?
// A refused loopback connect returns immediately, so this stays cheap.
bool PortAlive(unsigned port) {
  WSADATA wsa;
  if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return false;
  bool alive = false;
  SOCKET s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
  if (s != INVALID_SOCKET) {
    sockaddr_in a{};
    a.sin_family = AF_INET;
    a.sin_port = htons(static_cast<u_short>(port));
    a.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
    if (connect(s, reinterpret_cast<sockaddr*>(&a), sizeof(a)) == 0) {
      alive = true;
    }
    closesocket(s);
  }
  WSACleanup();
  return alive;
}

// Minimal HTTP GET: does <path> answer 200 on this loopback origin? Used to
// tell OUR backend (serves the osc bundle same-origin, /index.html -> 200)
// from the GENUINE Web Helper (API-only — every static path 404s; verified
// via CDP 2026-09-25, docs/osc/25).
bool HttpPathOk(unsigned port, const std::string& path) {
  WSADATA wsa;
  if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return false;
  bool ok = false;
  SOCKET s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
  if (s != INVALID_SOCKET) {
    sockaddr_in a{};
    a.sin_family = AF_INET;
    a.sin_port = htons(static_cast<u_short>(port));
    a.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
    if (connect(s, reinterpret_cast<sockaddr*>(&a), sizeof(a)) == 0) {
      std::string req = "GET " + path + " HTTP/1.0\r\n"
                        "Host: 127.0.0.1\r\nConnection: close\r\n\r\n";
      if (send(s, req.c_str(), (int)req.size(), 0) > 0) {
        char buf[512] = {};
        int n = recv(s, buf, sizeof(buf) - 1, 0);
        if (n > 0) {
          buf[n] = 0;
          ok = strncmp(buf, "HTTP/1.", 7) == 0 &&
               strstr(buf, " 200 ") != NULL;
        }
      }
    }
    closesocket(s);
  }
  WSACleanup();
  return ok;
}

// file:/// URL for a local path (spaces percent-encoded — "NVIDIA ShadowPlay"
// and "My Project" both contain them).
std::string FileUrl(const std::wstring& path) {
  std::string u = sharewin::WideToUtf8(path);
  for (size_t i = 0; i < u.size(); ++i) {
    if (u[i] == '\\') u[i] = '/';
  }
  std::string enc;
  enc.reserve(u.size() + 8);
  for (size_t i = 0; i < u.size(); ++i) {
    unsigned char c = static_cast<unsigned char>(u[i]);
    if (c == ' ') enc += "%20";
    else enc += static_cast<char>(c);
  }
  return "file:///" + enc;
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
  // NvBackend origin mode: QUERY_WIN_NODE_INFO reports the BACKEND port so
  // the page builds backend URLs against it (parity with the engine's
  // single-port controller server; Backend/index.js serves the osc
  // frontend same-origin on 59001).
  if (cl->HasSwitch("backend-port")) {
    params->backend_port =
        ParseUnsigned(cl->GetSwitchValue("backend-port").ToString(), 0);
  } else if (PortAlive(kDefaultBackendPort)) {
    // Wire to the REAL system by default (owner call 2026-09-24): when the
    // Web Helper backend is up, serve the page same-origin from it (REST +
    // osc + ShadowPlay v1.0 surface). The static server below stays up as
    // a fallback surface; --backend-port still overrides everything.
    params->backend_port = kDefaultBackendPort;
    shareproof::LogLine("backend auto-detected on 59001 — same-origin mode");
  }

  // Proof recording on (QPC timeline from PROC_START is preserved).
  if (params->proof_mode) {
    shareproof::EnableProofMode();
    shareproof::LogLine("proof-of-life mode, self-exit-ms=" +
                        std::to_string(params->self_exit_ms));
  }

  // ── single-instance guard (browser process, non-proof launches) ──────
  // GFE intentionally runs several Share instances from different folders
  // (the 1..4 rule), but relaunching the SAME exe must not stack another
  // full CEF stack on top of the running one (extra window + extra
  // loopback server per double-click). One named mutex per exe path gives
  // exactly that scope; subprocess relaunches exit earlier inside
  // CefExecuteProcess and never reach this point, and proof-of-life runs
  // are exempt above so the evidence harness can run as needed. The
  // handle is deliberately never released — the kernel destroys the mutex
  // when this process dies, so a crashed host frees its slot.
  if (!params->proof_mode) {
    static HANDLE s_instance_mutex = NULL;
    s_instance_mutex = CreateMutexW(
        NULL, TRUE,
        (L"Local\\NvShare-Browser-" + sharewin::GetExeDir()).c_str());
    if (s_instance_mutex && GetLastError() == ERROR_ALREADY_EXISTS) {
      shareproof::Checkpoint(
          "ALREADY_RUNNING",
          "browser process for this exe path is already active");
      shareproof::LogLine("second launch for " +
                          sharewin::WideToUtf8(sharewin::GetExeDir()) +
                          " — exiting, existing instance stays up");
      return 0;
    }
    // NULL handle (creation failed): fail open, proceed unguarded.
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
  HostCtx()->backend_port = params->backend_port;
  HostCtx()->secret = sharewin::RandomHex(16);
  shareproof::SetField("httpPort", std::to_string(server.port()));
  shareproof::SetField("backendPort", std::to_string(params->backend_port));
  shareproof::SetField("secretLength",
                       std::to_string(HostCtx()->secret.size()));
  shareproof::SetField("oscRoot", sharejson::Escape(osc_root));

  // URL: --url= override; else the backend origin when --backend-port is
  // set — BUT only if that backend actually serves the osc bundle (our
  // NvBackend does, /index.html -> 200). The GENUINE Web Helper is API-only
  // (every static path 404s — the original black screen), so fall back to
  // the genuine Share.exe model: file://<exeDir>/<nv-url-relative>, which
  // CDP-proven boots the full osc UI (docs/osc/25).
  if (cl->HasSwitch("url")) {
    params->url = cl->GetSwitchValue("url").ToString();
  } else if (params->backend_port > 0) {
    std::string page = "index.html";
    size_t slash = cfg.nv_url_relative.find_last_of('/');
    if (slash != std::string::npos && slash + 1 < cfg.nv_url_relative.size()) {
      page = cfg.nv_url_relative.substr(slash + 1);
    }
    const std::string origin =
        "http://127.0.0.1:" + std::to_string(params->backend_port);
    if (HttpPathOk(params->backend_port, "/" + page)) {
      params->url = origin + "/" + page;
      shareproof::LogLine("backend serves the osc bundle — same-origin mode");
    } else {
      const std::wstring rel =
          sharewin::Utf8ToWide(cfg.nv_url_relative);
      params->url = FileUrl(sharewin::GetExeDir() + L"\\" + rel);
      shareproof::LogLine(
          "backend is API-only (static 404) — file:// fallback (genuine "
          "Share.exe model), page=" + sharewin::WideToUtf8(rel));
    }
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
  settings.windowless_rendering_enabled = true;
  // Transparent overlay: the OSR frame composites through
  // UpdateLayeredWindow — alpha=0 pixels stay invisible + click-through,
  // so there is no opaque backing surface at all.
  settings.background_color = CefColorSetARGB(0x00, 0x00, 0x00, 0x00);
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
