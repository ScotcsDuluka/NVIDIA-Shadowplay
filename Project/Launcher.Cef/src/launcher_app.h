// launcher_app.h - CefApp for the launcher lane: browser-side message
// router + renderer-side router (window.cefQuery / window.cefQueryCancel
// — the osc page contract, kept identical so the RPC wiring is one shape
// across both CEF lanes).
#ifndef LAUNCHER_APP_H_
#define LAUNCHER_APP_H_

#include "include/cef_app.h"
#include "include/wrapper/cef_message_router.h"

class LauncherApp : public CefApp,
                    public CefBrowserProcessHandler,
                    public CefRenderProcessHandler {
 public:
  LauncherApp();

  CefRefPtr<CefBrowserProcessHandler> GetBrowserProcessHandler() override {
    return this;
  }
  CefRefPtr<CefRenderProcessHandler> GetRenderProcessHandler() override {
    return this;
  }

  void OnBeforeCommandLineProcessing(
      const CefString& browser_type,
      CefRefPtr<CefCommandLine> command_line) override;

  void OnContextInitialized() override;

  void OnWebKitInitialized() override;

  void OnContextCreated(CefRefPtr<CefBrowser> browser,
                        CefRefPtr<CefFrame> frame,
                        CefRefPtr<CefV8Context> context) override;

  void OnContextReleased(CefRefPtr<CefBrowser> browser,
                         CefRefPtr<CefFrame> frame,
                         CefRefPtr<CefV8Context> context) override;

  bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                CefProcessId source_process,
                                CefRefPtr<CefProcessMessage> message) override;

 private:
  CefMessageRouterConfig router_config_;
  CefRefPtr<CefMessageRouterRendererSide> renderer_router_;

  IMPLEMENT_REFCOUNTING(LauncherApp);
  DISALLOW_COPY_AND_ASSIGN(LauncherApp);
};

#endif  // LAUNCHER_APP_H_
