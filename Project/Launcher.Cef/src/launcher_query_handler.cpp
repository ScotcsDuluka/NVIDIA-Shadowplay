// launcher_query_handler.cpp - see launcher_query_handler.h.
#include "launcher_query_handler.h"

#include <shellapi.h>

#include <sstream>
#include <thread>

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
    // Power toggle for the complete selected overlay engine. The WinForm
    // hub or NvContainer owns starting/stopping its engine's service family.
    const bool value = launcherjson::GetBool(req, "value");
    const bool ok = launcherutil::WriteConfigBool(
        L"Overlay", L"UseOverlayEnabled", value);
    if (ok && supervisor_ &&
        launcherutil::ReadConfigBool(L"Overlay", L"EngineOverlayMode", false)) {
      if (value) {
        supervisor_->StartEngineOverlayChainAsync();
      } else {
        supervisor_->StopCefOverlay();
      }
    }
    RespondOk(callback, ok ? "{\"ok\":true}" : "{\"ok\":false}");
    return true;
  }

  if (cmd == "LAUNCHER_SET_ENGINE_OVERLAY") {
    // Overlay Mode switch: false = WINFORM lane (hub-managed family),
    // true = CEF lane (NvContainer -> Web Helper :59011 -> NVIDIA OSC,
    // toggle :59013). NvContainer owns the helper and presenter; mode
    // changes prepare services but never show the overlay.
    const bool value = launcherjson::GetBool(req, "value");
    const bool ok = launcherutil::WriteConfigBool(
        L"Overlay", L"EngineOverlayMode", value);
    if (ok && supervisor_) {
      if (value &&
          launcherutil::ReadConfigBool(L"Overlay", L"UseOverlayEnabled", false)) {
        // Power is already on: prepare the newly selected engine without
        // showing the overlay. Power-off mode selection starts no services.
        supervisor_->StartEngineOverlayChainAsync();
      } else if (!value) {
        supervisor_->StopCefOverlay();
      }
    }
    RespondOk(callback, ok ? "{\"ok\":true}" : "{\"ok\":false}");
    return true;
  }

  if (cmd == "LAUNCHER_OPEN_OVERLAY") {
    auto* supervisor = supervisor_;
    std::thread([supervisor, callback]() {
      const bool ok = supervisor && supervisor->SendOpenOverlay();
      RespondOk(callback, ok ? "{\"ok\":true}" : "{\"ok\":false}");
    }).detach();
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
