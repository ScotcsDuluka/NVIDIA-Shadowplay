// launcher_client.cpp - see launcher_client.h.
// Posted-task safety: tasks never capture `this` (the client is owned by
// CEF refcounting and can die while a task is queued). Tasks re-fetch the
// static active_client_, which is maintained on the UI thread only.
#include "launcher_client.h"

#include <functional>

#include "include/cef_app.h"
#include "include/cef_browser.h"
#include "include/cef_frame.h"
#include "include/cef_task.h"
#include "include/wrapper/cef_helpers.h"

#include "launcher_main.h"
#include "launcher_supervisor.h"
#include "launcher_util.h"

LauncherClient* LauncherClient::active_client_ = NULL;

namespace {

class FnTask : public CefTask {
 public:
  explicit FnTask(const std::function<void()>& fn) : fn_(fn) {}
  void Execute() override { fn_(); }
 private:
  std::function<void()> fn_;
  IMPLEMENT_REFCOUNTING(FnTask);
};

void PostUiTask(const std::function<void()>& fn, int64 delay_ms = 0) {
  CefRefPtr<FnTask> task(new FnTask(fn));
  if (delay_ms > 0) {
    CefPostDelayedTask(TID_UI, task.get(), delay_ms);
  } else {
    CefPostTask(TID_UI, task.get());
  }
}

// Poll-thread -> UI-thread push: capture only the JSON string; the task
// re-fetches active_client_ on the UI thread.
void PushStateTrampoline(const std::string& json) {
  PostUiTask([json]() {
    if (LauncherClient::active_client_) {
      LauncherClient::active_client_->PushState(json);
    }
  });
}

}  // namespace

void LauncherPushStateAnyThread(const std::string& json) {
  PushStateTrampoline(json);
}

LauncherClient::LauncherClient(const LauncherLaunchParams* params,
                               HWND host_wnd)
    : params_(params), host_wnd_(host_wnd) {
  CefMessageRouterConfig config;
  config.js_query_function = "cefQuery";        // production contract
  config.js_cancel_function = "cefQueryCancel"; // (router defaults, explicit)
  router_ = CefMessageRouterBrowserSide::Create(config);
  query_handler_ = new LauncherQueryHandler(host_wnd);
  query_handler_->SetRequestCloseCallback([]() {
    if (LauncherClient::active_client_) {
      LauncherClient::active_client_->RequestClose();
    }
  });
  query_handler_->SetInitialPushCallback(&PushStateTrampoline);
  router_->AddHandler(query_handler_.get(), false);
}

LauncherClient::~LauncherClient() {}

bool LauncherClient::OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                              CefProcessId source_process,
                                              CefRefPtr<CefProcessMessage> message) {
  return router_->OnProcessMessageReceived(browser, source_process, message);
}

void LauncherClient::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
  CEF_REQUIRE_UI_THREAD();
  browser_ = browser;
  active_client_ = this;
  launcherutil::LogLine("browser object created");
}

bool LauncherClient::DoClose(CefRefPtr<CefBrowser> browser) {
  CEF_REQUIRE_UI_THREAD();
  // Windowed child browser (SetAsChild): CEF requires the application to
  // destroy its own top-level window — OnBeforeClose only fires after the
  // OS window is gone. Deferred by one loop iteration to stay out of CEF's
  // close re-entry.
  if (host_wnd_ && IsWindow(host_wnd_)) {
    const HWND hwnd = host_wnd_;
    PostUiTask([hwnd]() {
      if (IsWindow(hwnd)) DestroyWindow(hwnd);
    });
  }
  return false;  // allow the close
}

void LauncherClient::OnBeforeClose(CefRefPtr<CefBrowser> browser) {
  CEF_REQUIRE_UI_THREAD();
  router_->OnBeforeClose(browser);
  if (browser_.get() && browser_->IsSame(browser)) {
    browser_ = NULL;
  }
  if (active_client_ == this) active_client_ = NULL;
  launcherutil::LogLine("browser closed - quitting message loop");
  CefQuitMessageLoop();
}

void LauncherClient::OnLoadEnd(CefRefPtr<CefBrowser> browser,
                               CefRefPtr<CefFrame> frame,
                               int httpStatusCode) {
  CEF_REQUIRE_UI_THREAD();
  if (!frame->IsMain()) return;
  launcherutil::LogLine("page loaded url=" + frame->GetURL().ToString() +
                        " httpStatus=" + std::to_string(httpStatusCode));
  if (httpStatusCode != 200 && httpStatusCode != 0) return;
  // First state snapshot right after load; the 1s poll keeps it fresh.
  query_handler_->FireInitialPush();
}

void LauncherClient::OnLoadError(CefRefPtr<CefBrowser> browser,
                                 CefRefPtr<CefFrame> frame,
                                 CefLoadHandler::ErrorCode errorCode,
                                 const CefString& errorText,
                                 const CefString& failedUrl) {
  CEF_REQUIRE_UI_THREAD();
  if (errorCode == ERR_ABORTED) return;
  launcherutil::LogLine("load error: " + errorText.ToString() + " url=" +
                        failedUrl.ToString());
}

void LauncherClient::RequestClose() {
  if (browser_.get()) browser_->GetHost()->CloseBrowser(false);
}

void LauncherClient::PushState(const std::string& json) {
  CEF_REQUIRE_UI_THREAD();
  if (!browser_.get()) return;
  // The JSON snapshot is a valid JS object literal — delivered through the
  // augmentation dispatcher (launcher_script.h).
  browser_->GetMainFrame()->ExecuteJavaScript(
      "window.__LauncherState(" + json + ");", "nvidia-launcher://state", 0);
}
