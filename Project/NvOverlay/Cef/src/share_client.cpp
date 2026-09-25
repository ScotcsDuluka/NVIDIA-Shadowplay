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
                         int http_port, int backend_port,
                         const std::string& secret, ShareStorage* storage)
    : params_(params), host_wnd_(host_wnd), finishing_(false), finished_(false) {
  CefMessageRouterConfig config;
  config.js_query_function = "cefQuery";        // production contract
  config.js_cancel_function = "cefQueryCancel"; // (router defaults, explicit)
  router_ = CefMessageRouterBrowserSide::Create(config);
  query_handler_ = new ShareQueryHandler(http_port, backend_port, secret,
                                         storage, host_wnd,
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

// ── OSR render + layered-window presentation ────────────────────────────
// The overlay contract (genuine host parity): transparent pixels are
// invisible AND click-through; only the rendered menu UI is opaque and
// interactive. The page is composited offscreen and presented through
// UpdateLayeredWindow on the layered host window.

void ShareClient::GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) {
  rect = CefRect(0, 0, GetSystemMetrics(SM_CXSCREEN),
                 GetSystemMetrics(SM_CYSCREEN));
}

void ShareClient::OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type,
                          const RectList& dirtyRects, const void* buffer,
                          int width, int height) {
  if (type != PET_VIEW || width <= 0 || height <= 0 || !buffer) return;
  const size_t needed = static_cast<size_t>(width) * height * 4;
  if (osr_w_ != width || osr_h_ != height) {
    osr_frame_.assign(static_cast<const unsigned char*>(buffer),
                      static_cast<const unsigned char*>(buffer) + needed);
    osr_w_ = width;
    osr_h_ = height;
    if (osr_bmp_) {
      DeleteObject(osr_bmp_);
      osr_bmp_ = NULL;
      osr_dc_ = NULL;
    }
  } else {
    memcpy(osr_frame_.data(), buffer, needed);
  }
  PresentOsrFrame();
}

void ShareClient::PresentOsrFrame() {
  if (!host_wnd_ || osr_w_ <= 0 || osr_h_ <= 0 || osr_frame_.empty()) return;
  HDC wdc = GetDC(host_wnd_);
  if (!wdc) return;
  if (!osr_dc_) {
    BITMAPINFO bi;
    ZeroMemory(&bi, sizeof(bi));
    bi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bi.bmiHeader.biWidth = osr_w_;
    bi.bmiHeader.biHeight = -osr_h_;  // top-down
    bi.bmiHeader.biPlanes = 1;
    bi.bmiHeader.biBitCount = 32;
    bi.bmiHeader.biCompression = BI_RGB;
    osr_dc_ = CreateCompatibleDC(wdc);
    osr_bmp_ = CreateDIBSection(wdc, &bi, DIB_RGB_COLORS, &osr_bits_, NULL, 0);
    if (osr_bmp_) SelectObject(osr_dc_, osr_bmp_);
  }
  if (osr_dc_ && osr_bits_) {
    memcpy(osr_bits_, osr_frame_.data(),
           static_cast<size_t>(osr_w_) * osr_h_ * 4);
    POINT src = {0, 0};
    SIZE sz = {osr_w_, osr_h_};
    BLENDFUNCTION bf = {AC_SRC_OVER, 0, 255, AC_SRC_ALPHA};
    UpdateLayeredWindow(host_wnd_, wdc, NULL, &sz, osr_dc_, &src, 0, &bf,
                        ULW_ALPHA);
  }
  ReleaseDC(host_wnd_, wdc);
}

bool ShareClient::IsPixelOpaque(int x, int y) {
  if (x < 0 || y < 0 || x >= osr_w_ || y >= osr_h_) return false;
  const unsigned char* px =
      osr_frame_.data() + (static_cast<size_t>(y) * osr_w_ + x) * 4;
  return px[3] > 8;  // alpha threshold: below = click-through
}

void ShareClient::ForwardMouseMove(int x, int y, bool leave) {
  if (!browser_) return;
  CefMouseEvent e;
  e.x = x;
  e.y = y;
  e.modifiers = 0;
  browser_->GetHost()->SendMouseMoveEvent(e, leave);
}

void ShareClient::ForwardMouseButton(int x, int y, bool down,
                                     bool left_button) {
  if (!browser_) return;
  CefMouseEvent e;
  e.x = x;
  e.y = y;
  e.modifiers = 0;
  browser_->GetHost()->SendMouseClickEvent(
      e, (down && left_button) ? MBT_LEFT : MBT_RIGHT, !down, 1);
}

void ShareClient::ForwardKey(HWND hwnd, UINT msg, WPARAM wparam,
                             LPARAM lparam) {
  if (!browser_) return;
  CefKeyEvent e;
  e.windows_key_code = static_cast<int>(wparam);
  e.native_key_code = static_cast<int>(lparam);
  if (msg == WM_CHAR) {
    e.type = KEYEVENT_CHAR;
  } else if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN) {
    e.type = KEYEVENT_KEYDOWN;
  } else {
    e.type = KEYEVENT_KEYUP;
  }
  e.is_system_key = (msg == WM_SYSKEYDOWN || msg == WM_SYSKEYUP);
  browser_->GetHost()->SendKeyEvent(e);
}

void ShareClient::ToggleOverlay() {
  // Alt+Z delivery: the host drives ITS OWN page through CEF IPC (the
  // genuine native half does the same via its renderer channel). The page
  // navigates itself and the visibility bridge mirrors the state to the
  // window — no dependency on the socket event delivery.
  if (!browser_) return;
  browser_->GetMainFrame()->ExecuteJavaScript(
      "(function(){try{var inj=window.angular.element(document.body).injector();"
      "var st=inj.get('$state');"
      "console.log('[hk-toggle] state='+st.current.name);"
      "if(st.current.name.indexOf('main')===0){st.go('base');"
      "console.log('[hk-toggle] going base');}"
      "else{st.go('main.main-menu');"
      "console.log('[hk-toggle] going main');"
      "if(window.__oscOpen){window.__oscOpen();}}"
      "}catch(e){console.log('[hk-toggle] ERR '+e.message);}})();",
      "nvidia-share://hotkey-toggle", 0);
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
