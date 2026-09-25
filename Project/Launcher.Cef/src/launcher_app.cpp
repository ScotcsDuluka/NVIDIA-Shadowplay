// launcher_app.cpp - see launcher_app.h.
#include "launcher_app.h"

#include "include/cef_browser.h"
#include "include/cef_command_line.h"
#include "include/cef_frame.h"
#include "include/cef_v8.h"
#include "include/wrapper/cef_helpers.h"

#include "launcher_client.h"
#include "launcher_main.h"
#include "launcher_script.h"
#include "launcher_supervisor.h"
#include "launcher_util.h"
#include "launcher_win.h"

LauncherApp::LauncherApp() {
  // The production page contract (osc vendor.js crimson.cefService) calls
  // window.cefQuery / window.cefQueryCancel — the router defaults. Set
  // explicitly so the contract is visible in source.
  router_config_.js_query_function = "cefQuery";
  router_config_.js_cancel_function = "cefQueryCancel";
}

void LauncherApp::OnBeforeCommandLineProcessing(
    const CefString& browser_type, CefRefPtr<CefCommandLine> command_line) {
  if (!GetLaunchParams()->gpu_accel) {
    command_line->AppendSwitch("disable-gpu");
    launcherutil::LogLine("nv-gpu-accel=false -> --disable-gpu");
  }
}

void LauncherApp::OnContextInitialized() {
  CEF_REQUIRE_UI_THREAD();

  LauncherLaunchParams* params = GetLaunchParams();

  HWND hwnd = launcherwin::CreateLauncherWindow(
      GetModuleHandleW(NULL), params->show_window, params->width,
      params->height, L"NVIDIA ShadowPlay");
  if (!hwnd) {
    launcherutil::LogLine("CreateLauncherWindow failed");
  }
  launcherwin::SetCloseRequestCallback([]() {
    if (LauncherClient::active_client_) {
      LauncherClient::active_client_->RequestClose();
    }
  });

  CefRefPtr<LauncherClient> client = new LauncherClient(params, hwnd);
  client->query_handler()->SetSupervisor(LauncherSupervisor::Get());

  CefWindowInfo info;
  RECT bounds = {0, 0, params->width, params->height};
  info.SetAsChild(hwnd, bounds);
  CefBrowserSettings settings;
  CefBrowserHost::CreateBrowser(info, client.get(), params->url, settings,
                                NULL);
}

void LauncherApp::OnWebKitInitialized() {
  CEF_REQUIRE_RENDERER_THREAD();
  renderer_router_ = CefMessageRouterRendererSide::Create(router_config_);
}

void LauncherApp::OnContextCreated(CefRefPtr<CefBrowser> browser,
                                   CefRefPtr<CefFrame> frame,
                                   CefRefPtr<CefV8Context> context) {
  CEF_REQUIRE_RENDERER_THREAD();
  if (renderer_router_.get()) {
    renderer_router_->OnContextCreated(browser, frame, context);
  }
  // Augmentation runs in every frame at context creation.
  frame->ExecuteJavaScript(LauncherAugmentationSource(),
                           "nvidia-launcher://augment", 0);
}

void LauncherApp::OnContextReleased(CefRefPtr<CefBrowser> browser,
                                    CefRefPtr<CefFrame> frame,
                                    CefRefPtr<CefV8Context> context) {
  CEF_REQUIRE_RENDERER_THREAD();
  if (renderer_router_.get()) {
    renderer_router_->OnContextReleased(browser, frame, context);
  }
}

bool LauncherApp::OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                           CefProcessId source_process,
                                           CefRefPtr<CefProcessMessage> message) {
  CEF_REQUIRE_RENDERER_THREAD();
  if (renderer_router_.get()) {
    return renderer_router_->OnProcessMessageReceived(browser, source_process,
                                                      message);
  }
  return false;
}
