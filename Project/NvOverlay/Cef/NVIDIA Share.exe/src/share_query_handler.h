// share_query_handler.h - browser-side CEF message-router handler.
// A faithful port of the PROVEN cefQuery contract of
// Overlay.Engine\CefQueryBridge.vb (missions 3 + phase 5) onto the native
// CEF message router — which is exactly the mechanism the production
// NVIDIA Share.exe used (window.cefQuery / window.cefQueryCancel are the
// router's default JS integration functions).
//
// Page-visible contract (identical to production):
//   window.cefQuery({request: <JSON STRING>, persistent: <bool>,
//                    onSuccess: (responseString) => ...,
//                    onFailure: (errorCode, errorMessage) => ...})
//   -> handle { id, cancel() }
// Response semantics: success resolves the string; failure rejects with
// (errorCode, errorMessage); -1 = not_implemented / unsupported,
// -3 = malformed request, 204-class cancel handled by the router's
// OnQueryCanceled (no callbacks fire, matching the crimson wrapper's
// isCancelled detection).
#ifndef SHARE_QUERY_HANDLER_H_
#define SHARE_QUERY_HANDLER_H_

#include <string>
#include <vector>
#include <windows.h>

#include "include/cef_base.h"
#include "include/cef_browser.h"
#include "include/wrapper/cef_message_router.h"

class ShareStorage;

class ShareQueryHandler : public CefMessageRouterBrowserSide::Handler,
                          public CefBaseRefCounted {
 public:
  // |host_wnd| receives open/close/clipboard window ops.
  // |show_window|: whether QUERY_WIN_OPEN_OSC shows the host window.
  // |backend_port|: >0 = NvBackend origin mode — QUERY_WIN_NODE_INFO
  // reports it (page builds backend URLs there; Backend serves the osc
  // frontend same-origin). 0 = standalone (static server port).
  ShareQueryHandler(int http_port, int backend_port, const std::string& secret,
                    ShareStorage* storage, HWND host_wnd, bool show_window);
  ~ShareQueryHandler() override;

  bool OnQuery(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
               int64 query_id, const CefString& request, bool persistent,
               CefRefPtr<Callback> callback) override;

  void OnQueryCanceled(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                       int64 query_id) override;

  // Pushes the close event through the persistent
  // QUERY_OSC_REGISTER_CLOSE_EVENT channel (parity with
  // CefQueryBridge.RequestCloseFromPage). Unused by the proof run;
  // provided for the hotkey-toggle integration round.
  void PushCloseFromHost();

  // Proof pipeline hook: invoked on the UI thread after the
  // __PROOF_ECHO round-trip closes (plain function pointer — the handler
  // must never own client references).
  void SetProofDoneCallback(void (*cb)(void)) { proof_done_cb_ = cb; }

 private:
  int http_port_;
  int backend_port_;
  std::string secret_;
  ShareStorage* storage_;
  HWND host_wnd_;
  bool show_window_;

  // Open/close display-state flip on <html> (parity with the engine
  // host's OpenOsc/CloseOsc handlers, OscHostForm.vb:732-790).
  void FlipOscDisplayState(bool open);

  // Persistent close-event registration (QUERY_OSC_REGISTER_CLOSE_EVENT).
  int64 close_query_id_;
  CefRefPtr<Callback> close_callback_;

  // QUERY_OSC_SET_DISPLAY_RECTS / QUERY_OSC_SET_PAINTING state (UI thread).
  std::vector<RECT> display_rects_;
  bool painting_enabled_;

  // Main-frame reference for display-state flips (set on each OnQuery;
  // UI thread only).
  CefRefPtr<CefBrowser> browser_for_flip_;

  // Organic query tracking for the proof pipeline.
  bool organic_recorded_;

  void (*proof_done_cb_)(void);

  IMPLEMENT_REFCOUNTING(ShareQueryHandler);
};

#endif  // SHARE_QUERY_HANDLER_H_
