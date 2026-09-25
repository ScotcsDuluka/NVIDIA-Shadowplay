// launcher_client.h - CefClient of the launcher lane. Owns the browser
// object, the message router and the host-push bridge that delivers
// supervisor state snapshots to the page.
#ifndef LAUNCHER_CLIENT_H_
#define LAUNCHER_CLIENT_H_

#include "include/cef_browser.h"
#include "include/cef_client.h"
#include "include/wrapper/cef_message_router.h"

#include "launcher_query_handler.h"

struct LauncherLaunchParams;

// Poll-thread entry for supervisor state pushes: posts a UI-thread task
// that re-fetches the active client (never touches CEF from this thread).
void LauncherPushStateAnyThread(const std::string& json);

class LauncherClient : public CefClient,
                       public CefLifeSpanHandler,
                       public CefLoadHandler {
 public:
  // UI-thread-only static (Share lane FnTask pattern: queued tasks re-fetch
  // this instead of capturing `this`, so a dying client can't be touched).
  static LauncherClient* active_client_;

  LauncherClient(const LauncherLaunchParams* params, HWND host_wnd);
  ~LauncherClient() override;

  CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override { return this; }
  CefRefPtr<CefLoadHandler> GetLoadHandler() override { return this; }

  bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                CefProcessId source_process,
                                CefRefPtr<CefProcessMessage> message) override;

  void OnAfterCreated(CefRefPtr<CefBrowser> browser) override;
  bool DoClose(CefRefPtr<CefBrowser> browser) override;
  void OnBeforeClose(CefRefPtr<CefBrowser> browser) override;

  void OnLoadEnd(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                 int httpStatusCode) override;
  void OnLoadError(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                   CefLoadHandler::ErrorCode errorCode,
                   const CefString& errorText,
                   const CefString& failedUrl) override;

  LauncherQueryHandler* query_handler() { return query_handler_.get(); }
  void RequestClose();

  // UI thread: execute the state-push JS against the current page.
  void PushState(const std::string& json);

 private:
  const LauncherLaunchParams* params_;
  HWND host_wnd_;
  CefRefPtr<CefBrowser> browser_;
  CefRefPtr<CefMessageRouterBrowserSide> router_;
  CefRefPtr<LauncherQueryHandler> query_handler_;

  IMPLEMENT_REFCOUNTING(LauncherClient);
  DISALLOW_COPY_AND_ASSIGN(LauncherClient);
};

#endif  // LAUNCHER_CLIENT_H_
