// launcher_query_handler.h - browser-side cefQuery dispatch for the
// launcher page. Commands are namespaced LAUNCHER_* (no overlap with the
// osc QUERY_* namespace — the two lanes never share a page).
#ifndef LAUNCHER_QUERY_HANDLER_H_
#define LAUNCHER_QUERY_HANDLER_H_

#include "include/cef_browser.h"
#include "include/base/cef_ref_counted.h"
#include "include/wrapper/cef_message_router.h"

class LauncherSupervisor;

class LauncherQueryHandler
    : public CefMessageRouterBrowserSide::Handler,
      public CefBaseRefCounted {
 public:
  explicit LauncherQueryHandler(HWND host_wnd);
  ~LauncherQueryHandler() override;

  void SetSupervisor(LauncherSupervisor* supervisor) {
    supervisor_ = supervisor;
  }
  void SetRequestCloseCallback(void (*cb)()) { request_close_cb_ = cb; }

  // Called by the client when the page has loaded — pushes the first
  // state snapshot so the UI never sits on stale defaults.
  void SetInitialPushCallback(void (*cb)(const std::string&)) {
    initial_push_cb_ = cb;
  }
  void FireInitialPush();

  bool OnQuery(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
               int64 query_id, const CefString& request, bool persistent,
               CefRefPtr<Callback> callback) override;

 private:
  HWND host_wnd_;
  LauncherSupervisor* supervisor_ = NULL;
  void (*request_close_cb_)() = NULL;
  void (*initial_push_cb_)(const std::string&) = NULL;

  IMPLEMENT_REFCOUNTING(LauncherQueryHandler);
  DISALLOW_COPY_AND_ASSIGN(LauncherQueryHandler);
};

#endif  // LAUNCHER_QUERY_HANDLER_H_
