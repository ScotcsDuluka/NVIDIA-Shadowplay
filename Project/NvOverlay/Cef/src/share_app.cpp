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
  // The osc page loads from file:// (genuine Share.exe model — the backend
  // is API-only) and pulls its templates + l10n JSON via XHR relative to
  // that file origin. Chromium blocks file:// XHR by default, which left
  // the ui-view container empty (full-size, zero content) — the "black
  // screen". The genuine host runs with file access allowed.
  command_line->AppendSwitch("allow-file-access-from-files");
}

void ShareApp::OnContextInitialized() {
  CEF_REQUIRE_UI_THREAD();

  ShareLaunchParams* params = GetLaunchParams();
  ShareHostContext* ctx = GetHostContext();

  // Overlay contract (genuine GFE): the host window is created HIDDEN —
  // a visible window at boot paints a fullscreen black cover (the page
  // parks on the hidden base state until the menu opens). The page shows
  // it via cefQuery QUERY_WIN_OPEN_OSC / hides via QUERY_WIN_CLOSE_OSC.
  // Proof runs keep the old visible-at-boot behavior (visible evidence;
  // --hidden suppresses there too).
  HWND hwnd = sharewin::CreateHostWindow(GetModuleHandleW(NULL),
                                         params->proof_mode && params->show_window,
                                         1280, 800,
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
      new ShareClient(params, hwnd, ctx->http_port, ctx->backend_port,
                      ctx->secret, ctx->storage);
  client->query_handler()->SetProofDoneCallback([]() {
    if (ShareClient::active_client_) {
      ShareClient::active_client_->OnProofRoundTripComplete();
    }
  });

  CefWindowInfo info;
  // Child bounds MUST match the host window's actual client area — the
  // host is a full-screen overlay (CreateHostWindow overrides the passed
  // width/height with SM_CXSCREEN/SM_CYSCREEN), so a hardcoded 1280x800
  // child left the rest of the overlay window unpainted.
  RECT bounds;
  if (!GetClientRect(hwnd, &bounds)) {
    bounds = RECT{0, 0, 1280, 800};
  }
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
