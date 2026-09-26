// share_app.h - CefApp for both process types: browser-process
// OnContextInitialized creates the host window + ShareClient + browser;
// render-process handlers create the renderer-side message router and
// inject the augmentation script.
#ifndef SHARE_APP_H_
#define SHARE_APP_H_

#include "include/cef_app.h"
#include "include/wrapper/cef_message_router.h"

class ShareApp : public CefApp,
                 public CefBrowserProcessHandler,
                 public CefRenderProcessHandler {
 public:
  ShareApp();

  // CefApp
  CefRefPtr<CefBrowserProcessHandler> GetBrowserProcessHandler() override {
    return this;
  }
  CefRefPtr<CefRenderProcessHandler> GetRenderProcessHandler() override {
    return this;
  }
  void OnBeforeCommandLineProcessing(
      const CefString& browser_type,
      CefRefPtr<CefCommandLine> command_line) override;

  // CefBrowserProcessHandler
  void OnContextInitialized() override;

  // CefRenderProcessHandler
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

  IMPLEMENT_REFCOUNTING(ShareApp);
  DISALLOW_COPY_AND_ASSIGN(ShareApp);
};

#endif  // SHARE_APP_H_
