// share_client.h - browser-process CefClient: lifespan/load/display
// handlers + the browser-side message router owning ShareQueryHandler.
#ifndef SHARE_CLIENT_H_
#define SHARE_CLIENT_H_

#include <string>
#include <windows.h>

#include "include/cef_client.h"
#include "include/wrapper/cef_message_router.h"

class ShareStorage;
class ShareQueryHandler;
struct ShareLaunchParams;

class ShareClient : public CefClient,
                    public CefLifeSpanHandler,
                    public CefLoadHandler,
                    public CefDisplayHandler {
 public:
  ShareClient(const ShareLaunchParams* params, HWND host_wnd, int http_port,
              const std::string& secret, ShareStorage* storage);
  ~ShareClient() override;

  // Exposed for ShareApp::OnContextInitialized wiring.
  ShareQueryHandler* query_handler() { return query_handler_.get(); }

  // CefClient
  CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override { return this; }
  CefRefPtr<CefLoadHandler> GetLoadHandler() override { return this; }
  CefRefPtr<CefDisplayHandler> GetDisplayHandler() override { return this; }
  bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                CefProcessId source_process,
                                CefRefPtr<CefProcessMessage> message) override;

  // CefLifeSpanHandler
  void OnAfterCreated(CefRefPtr<CefBrowser> browser) override;
  bool DoClose(CefRefPtr<CefBrowser> browser) override;
  void OnBeforeClose(CefRefPtr<CefBrowser> browser) override;

  // CefLoadHandler
  void OnLoadStart(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                   TransitionType transition_type) override;
  void OnLoadEnd(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                 int httpStatusCode) override;
  void OnLoadError(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                   ErrorCode errorCode, const CefString& errorText,
                   const CefString& failedUrl) override;

  // CefDisplayHandler
  void OnTitleChange(CefRefPtr<CefBrowser> browser,
                     const CefString& title) override;

  // Close requested from the host window (WM_CLOSE forwarded by
  // share_win's close-request callback).
  void RequestClose();

  // Proof pipeline (UI thread).
  void RunProofDriver();
  void OnProofRoundTripComplete();
  void FinishNow(const std::string& reason);
  void EnsureFinish(const std::string& reason);

  // Static trampoline used by posted tasks and the host window proc
  // (maintained on the UI thread; never captured by tasks).
  static ShareClient* active_client_;

 private:
  void PostFinish(const std::string& reason);

  const ShareLaunchParams* params_;
  HWND host_wnd_;
  CefRefPtr<CefMessageRouterBrowserSide> router_;
  CefRefPtr<ShareQueryHandler> query_handler_;
  CefRefPtr<CefBrowser> browser_;
  bool finishing_;   // finish scheduled (round-trip observed)
  bool finished_;    // FinishNow executed

  IMPLEMENT_REFCOUNTING(ShareClient);
};

#endif  // SHARE_CLIENT_H_
