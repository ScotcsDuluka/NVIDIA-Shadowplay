// share_client.cpp - see share_client.h.
// Posted-task safety: tasks never capture `this` (the client is owned by
// CEF refcounting and can die while a task is queued). Tasks re-fetch the
// static active_client_, which is maintained on the UI thread only.
#include "share_client.h"

#include <functional>
#include <sstream>

#include "include/cef_app.h"
#include "include/cef_browser.h"
#include "include/cef_frame.h"
#include "include/cef_task.h"
#include "include/wrapper/cef_helpers.h"

#include "share_launch.h"
#include "share_proof.h"
#include "share_query_handler.h"
#include "share_script.h"
#include "share_storage.h"
#include "share_win.h"

ShareClient* ShareClient::active_client_ = NULL;

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

}  // namespace

ShareClient::ShareClient(const ShareLaunchParams* params, HWND host_wnd,
                         int http_port, const std::string& secret,
                         ShareStorage* storage)
    : params_(params), host_wnd_(host_wnd), finishing_(false), finished_(false) {
  CefMessageRouterConfig config;
  config.js_query_function = "cefQuery";        // production contract
  config.js_cancel_function = "cefQueryCancel"; // (router defaults, explicit)
  router_ = CefMessageRouterBrowserSide::Create(config);
  query_handler_ = new ShareQueryHandler(http_port, secret, storage, host_wnd,
                                         params->show_window);
  router_->AddHandler(query_handler_.get(), false);
}

ShareClient::~ShareClient() {}

bool ShareClient::OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                           CefProcessId source_process,
                                           CefRefPtr<CefProcessMessage> message) {
  return router_->OnProcessMessageReceived(browser, source_process, message);
}

void ShareClient::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
  CEF_REQUIRE_UI_THREAD();
  browser_ = browser;
  active_client_ = this;
  shareproof::Checkpoint("BROWSER_CREATED", "browser object created");

  if (params_->proof_mode) {
    // Watchdog: the automated proof run always terminates and always
    // leaves a proof file behind.
    const int64 delay = static_cast<int64>(params_->self_exit_ms);
    PostUiTask([]() {
      if (ShareClient::active_client_) {
        ShareClient::active_client_->FinishNow("watchdog-timeout");
      }
    }, delay);
  }
}

bool ShareClient::DoClose(CefRefPtr<CefBrowser> browser) {
  CEF_REQUIRE_UI_THREAD();
  // Windowed child browser (SetAsChild): CEF requires the application to
  // destroy its own top-level window — OnBeforeClose only fires after the
  // OS window is gone (cef_life_span_handler.h close examples). Deferred
  // by one loop iteration to stay out of CEF's close re-entry.
  if (host_wnd_ && IsWindow(host_wnd_)) {
    const HWND hwnd = host_wnd_;
    PostUiTask([hwnd]() {
      if (IsWindow(hwnd)) DestroyWindow(hwnd);
    });
  }
  return false;  // allow the close
}

void ShareClient::OnBeforeClose(CefRefPtr<CefBrowser> browser) {
  CEF_REQUIRE_UI_THREAD();
  router_->OnBeforeClose(browser);
  if (browser_.get() && browser_->IsSame(browser)) {
    browser_ = NULL;
  }
  if (active_client_ == this) active_client_ = NULL;
  shareproof::LogLine("browser closed - quitting message loop");
  CefQuitMessageLoop();
}

void ShareClient::OnLoadStart(CefRefPtr<CefBrowser> browser,
                              CefRefPtr<CefFrame> frame,
                              CefLoadHandler::TransitionType transition_type) {
  CEF_REQUIRE_UI_THREAD();
  if (frame->IsMain()) {
    shareproof::Checkpoint("PAGE_LOAD_START", frame->GetURL().ToString());
  }
}

void ShareClient::OnLoadEnd(CefRefPtr<CefBrowser> browser,
                            CefRefPtr<CefFrame> frame, int httpStatusCode) {
  CEF_REQUIRE_UI_THREAD();
  if (!frame->IsMain()) return;
  std::ostringstream o;
  o << "url=" << frame->GetURL().ToString() << " httpStatus=" << httpStatusCode;
  shareproof::Checkpoint("PAGE_LOADED", o.str());
  if (params_->proof_mode) {
    // Drive the proof: DOM probe + synthetic cefQuery round-trip. The
    // driver's onSuccess echoes the observed response back through a
    // __PROOF_ECHO query, which lands in ShareQueryHandler::OnQuery and
    // calls OnProofRoundTripComplete.
    PostUiTask([]() {
      if (ShareClient::active_client_) {
        ShareClient::active_client_->RunProofDriver();
      }
    }, 250);
  }
}

void ShareClient::OnLoadError(CefRefPtr<CefBrowser> browser,
                              CefRefPtr<CefFrame> frame,
                              CefLoadHandler::ErrorCode errorCode,
                              const CefString& errorText,
                              const CefString& failedUrl) {
  CEF_REQUIRE_UI_THREAD();
  if (errorCode == ERR_ABORTED) return;
  shareproof::LogLine("load error: " + errorText.ToString() + " url=" +
                      failedUrl.ToString());
}

void ShareClient::OnTitleChange(CefRefPtr<CefBrowser> browser,
                                const CefString& title) {
  CEF_REQUIRE_UI_THREAD();
  if (host_wnd_) {
    const std::wstring t = sharewin::Utf8ToWide(title.ToString());
    SetWindowTextW(host_wnd_, t.c_str());
  }
}

void ShareClient::RequestClose() {
  if (browser_.get()) browser_->GetHost()->CloseBrowser(false);
}

void ShareClient::RunProofDriver() {
  CEF_REQUIRE_UI_THREAD();
  if (browser_.get()) {
    browser_->GetMainFrame()->ExecuteJavaScript(
        ProofDriverSource(), "nvidia-share://proof-driver", 0);
  }
}

void ShareClient::OnProofRoundTripComplete() {
  CEF_REQUIRE_UI_THREAD();
  if (finished_) {
    // Watchdog already fired (page load was stalled then); the browser is
    // still alive — retry the close now that the round-trip completed.
    EnsureFinish("proof-round-trip-complete");
    return;
  }
  PostFinish("proof-round-trip-complete");
}

void ShareClient::PostFinish(const std::string& reason) {
  if (finishing_ || finished_) return;
  finishing_ = true;
  PostUiTask([]() {
    if (ShareClient::active_client_) {
      ShareClient::active_client_->EnsureFinish("proof-round-trip-complete");
    }
  }, 750);
}

void ShareClient::FinishNow(const std::string& reason) {
  EnsureFinish(reason);
}

void ShareClient::EnsureFinish(const std::string& reason) {
  if (!finished_) {
    finished_ = true;
    if (reason == "watchdog-timeout" && !finishing_) {
      shareproof::Checkpoint(
          "WATCHDOG_FIRED", "self-exit-ms elapsed without a complete round-trip");
    }
    shareproof::Checkpoint("PROOF_FINISH", reason);
  }
  // --stay-open (visual evidence runs): proof recorded, UI stays up; the
  // runner terminates the process after capturing.
  if (params_->stay_open) return;
  // Retry-safe: CloseBrowser may be a no-op on a browser whose load never
  // started (measured in the first runs); a later EnsureFinish closes it.
  if (browser_.get()) {
    browser_->GetHost()->CloseBrowser(true);
  } else {
    CefQuitMessageLoop();
  }
}
