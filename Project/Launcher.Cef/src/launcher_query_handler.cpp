// launcher_query_handler.cpp - see launcher_query_handler.h.
#include "launcher_query_handler.h"

#include <shellapi.h>

#include <sstream>

#include "include/cef_browser.h"
#include "include/cef_frame.h"

#include "launcher_json.h"
#include "launcher_supervisor.h"
#include "launcher_util.h"
#include "launcher_win.h"

namespace {

void RespondOk(CefRefPtr<CefMessageRouterBrowserSide::Callback> callback,
               const std::string& response) {
  callback->Success(response);
}

void RespondFail(CefRefPtr<CefMessageRouterBrowserSide::Callback> callback,
                 int code, const std::string& message) {
  callback->Failure(code, message);
}

}  // namespace

LauncherQueryHandler::LauncherQueryHandler(HWND host_wnd)
    : host_wnd_(host_wnd) {}

LauncherQueryHandler::~LauncherQueryHandler() {}

void LauncherQueryHandler::FireInitialPush() {
  if (initial_push_cb_ && supervisor_) {
    initial_push_cb_(supervisor_->Snapshot().ToJson());
  }
}

bool LauncherQueryHandler::OnQuery(CefRefPtr<CefBrowser> browser,
                                   CefRefPtr<CefFrame> frame, int64 query_id,
                                   const CefString& request, bool persistent,
                                   CefRefPtr<Callback> callback) {
  const std::string request_str = request.ToString();

  // Malformed-request ladder — parity with ShareQueryHandler::OnQuery.
  launcherjson::JVal req;
  if (!launcherjson::Parse(request_str, &req)) {
    RespondFail(callback, -3, "bad request json");
    return true;
  }
  if (req.type != launcherjson::JVal::kObj) {
    RespondFail(callback, -3, "request not object");
    return true;
  }
  const std::string cmd = launcherjson::GetStr(req, "command");
  if (cmd.empty()) {
    RespondFail(callback, -3, "no command");
    return true;
  }

  if (cmd == "LAUNCHER_GET_STATE") {
    RespondOk(callback, supervisor_ ? supervisor_->Snapshot().ToJson()
                                    : std::string("{}"));
    return true;
  }

  if (cmd == "LAUNCHER_SET_OVERLAY") {
    // USER toggle only — config.json is the single source of truth; the
    // NVIDIA API hub enforces the value every second (Main.vb contract:
    // the launcher toggle itself never starts/kills the overlay stack).
    const bool value = launcherjson::GetBool(req, "value");
    const bool ok = launcherutil::WriteConfigBool(
        L"Overlay", L"UseOverlayEnabled", value);
    RespondOk(callback, ok ? "{\"ok\":true}" : "{\"ok\":false}");
    return true;
  }

  if (cmd == "LAUNCHER_SET_ENGINE_OVERLAY") {
    const bool value = launcherjson::GetBool(req, "value");
    const bool ok = launcherutil::WriteConfigBool(
        L"Overlay", L"EngineOverlayMode", value);
    if (ok && value && supervisor_) {
      // ON = bring up the real chain (NvContainer -> Web Helper ->
      // :59001 -> NvOverlay\Cef NVIDIA Share.exe). Every step idempotent;
      // the supervisor runs it on its own thread (the :59001 wait must
      // never sit inside the CEF UI thread).
      supervisor_->StartEngineOverlayChainAsync();
    }
    RespondOk(callback, ok ? "{\"ok\":true}" : "{\"ok\":false}");
    return true;
  }

  if (cmd == "LAUNCHER_OPEN_OVERLAY") {
    const bool ok = supervisor_ && supervisor_->SendOpenOverlay();
    std::ostringstream o;
    o << "{\"ok\":" << (ok ? "true" : "false") << "}";
    RespondOk(callback, o.str());
    return true;
  }

  if (cmd == "LAUNCHER_OPEN_OBT3") {
    // Legacy OBT3 banner link (Main.vb OBT3_Click).
    HINSTANCE r = ShellExecuteW(NULL, L"open",
                                L"https://scotcsduluka.github.io/"
                                L"NVIDIA-Shadowplay/obt3.html",
                                NULL, NULL, SW_SHOWNORMAL);
    RespondOk(callback, reinterpret_cast<INT_PTR>(r) > 32 ? "{\"ok\":true}"
                                                          : "{\"ok\":false}");
    return true;
  }

  if (cmd == "LAUNCHER_DRAG") {
    launcherwin::BeginWindowDrag(host_wnd_);
    RespondOk(callback, "{\"ok\":true}");
    return true;
  }

  if (cmd == "LAUNCHER_MINIMIZE") {
    launcherwin::Minimize(host_wnd_);
    RespondOk(callback, "{\"ok\":true}");
    return true;
  }

  if (cmd == "LAUNCHER_CLOSE") {
    // Plain close: processes keep running (Main.vb RadioButton1 contract).
    RespondOk(callback, "{\"ok\":true}");
    if (request_close_cb_) request_close_cb_();
    return true;
  }

  if (cmd == "LAUNCHER_EXIT_ALL") {
    // Installer-mode exit: reset the overlay switch, kill the family,
    // then close the launcher.
    RespondOk(callback, "{\"ok\":true}");
    if (supervisor_) supervisor_->InstallerExit();
    if (request_close_cb_) request_close_cb_();
    return true;
  }

  RespondFail(callback, -1, "unknown command: " + cmd);
  return true;
}
