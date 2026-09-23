// share_app.cpp - see share_app.h.
#include "share_app.h"

#include "include/cef_browser.h"
#include "include/cef_command_line.h"
#include "include/cef_frame.h"
#include "include/cef_v8.h"
#include "include/wrapper/cef_helpers.h"

#include "share_client.h"
#include "share_query_handler.h"
#include "share_launch.h"
#include "share_proof.h"
#include "share_script.h"
#include "share_storage.h"
#include "share_win.h"

ShareApp::ShareApp() {
  // The production page contract (vendor.js crimson.cefService) calls
  // window.cefQuery / window.cefQueryCancel — the router defaults. Set
  // explicitly so the contract is visible in source.
  router_config_.js_query_function = "cefQuery";
  router_config_.js_cancel_function = "cefQueryCancel";
}

void ShareApp::OnBeforeCommandLineProcessing(
    const CefString& browser_type,
    CefRefPtr<CefCommandLine> command_line) {
  // NVIDIA Share.json nv-gpu-accel=false parity (reference GFE 3.28
  // default was true — keep GPU acceleration unless configured off).
  if (!GetLaunchParams()->gpu_accel) {
    command_line->AppendSwitch("disable-gpu");
    shareproof::LogLine("nv-gpu-accel=false -> --disable-gpu");
  }
}

void ShareApp::OnContextInitialized() {
  CEF_REQUIRE_UI_THREAD();

  ShareLaunchParams* params = GetLaunchParams();
  ShareHostContext* ctx = GetHostContext();

  HWND hwnd = sharewin::CreateHostWindow(GetModuleHandleW(NULL),
                                         params->show_window, 1280, 800,
                                         L"NVIDIA Share");
  if (!hwnd) {
    shareproof::LogLine("CreateHostWindow failed");
  }
  // WM_CLOSE -> CloseBrowser (never direct DestroyWindow).
  sharewin::SetHostCloseRequestCallback([]() {
    if (ShareClient::active_client_) {
      ShareClient::active_client_->RequestClose();
    }
  });

  CefRefPtr<ShareClient> client =
      new ShareClient(params, hwnd, ctx->http_port, ctx->secret, ctx->storage);
  client->query_handler()->SetProofDoneCallback([]() {
    if (ShareClient::active_client_) {
      ShareClient::active_client_->OnProofRoundTripComplete();
    }
  });

  CefWindowInfo info;
  RECT bounds = {0, 0, 1280, 800};
  info.SetAsChild(hwnd, bounds);
  CefBrowserSettings settings;
  CefBrowserHost::CreateBrowser(info, client.get(), params->url, settings,
                                NULL);
}

void ShareApp::OnWebKitInitialized() {
  CEF_REQUIRE_RENDERER_THREAD();
  renderer_router_ = CefMessageRouterRendererSide::Create(router_config_);
}

void ShareApp::OnContextCreated(CefRefPtr<CefBrowser> browser,
                                CefRefPtr<CefFrame> frame,
                                CefRefPtr<CefV8Context> context) {
  CEF_REQUIRE_RENDERER_THREAD();
  if (renderer_router_.get()) {
    renderer_router_->OnContextCreated(browser, frame, context);
  }
  // Augmentation runs in every frame at context creation (document-start
  // equivalent of the engine's AddScriptToExecuteOnDocumentCreated).
  frame->ExecuteJavaScript(AugmentationSource(), "nvidia-share://augment", 0);
}

void ShareApp::OnContextReleased(CefRefPtr<CefBrowser> browser,
                                 CefRefPtr<CefFrame> frame,
                                 CefRefPtr<CefV8Context> context) {
  CEF_REQUIRE_RENDERER_THREAD();
  if (renderer_router_.get()) {
    renderer_router_->OnContextReleased(browser, frame, context);
  }
}

bool ShareApp::OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                        CefProcessId source_process,
                                        CefRefPtr<CefProcessMessage> message) {
  CEF_REQUIRE_RENDERER_THREAD();
  if (renderer_router_.get()) {
    return renderer_router_->OnProcessMessageReceived(browser, source_process,
                                                      message);
  }
  return false;
}
