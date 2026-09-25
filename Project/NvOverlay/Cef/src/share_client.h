// share_client.h - browser-process CefClient: lifespan/load/display
// handlers + the browser-side message router owning ShareQueryHandler.
#ifndef SHARE_CLIENT_H_
#define SHARE_CLIENT_H_

#include <string>
#include <vector>
#include <windows.h>

#include "include/cef_client.h"
#include "include/wrapper/cef_message_router.h"

class ShareStorage;
class ShareQueryHandler;
struct ShareLaunchParams;

class ShareClient : public CefClient,
                    public CefLifeSpanHandler,
                    public CefLoadHandler,
                    public CefDisplayHandler,
                    public CefRenderHandler,
                    public CefContextMenuHandler {
 public:
  ShareClient(const ShareLaunchParams* params, HWND host_wnd, int http_port,
              int backend_port, const std::string& secret,
              ShareStorage* storage);
  ~ShareClient() override;

  // Exposed for ShareApp::OnContextInitialized wiring.
  ShareQueryHandler* query_handler() { return query_handler_.get(); }

  // CefClient
  CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override { return this; }
  CefRefPtr<CefLoadHandler> GetLoadHandler() override { return this; }
  CefRefPtr<CefDisplayHandler> GetDisplayHandler() override { return this; }
  CefRefPtr<CefRenderHandler> GetRenderHandler() override { return this; }
  CefRefPtr<CefContextMenuHandler> GetContextMenuHandler() override {
    return this;
  }
  // Suppress the default browser context menu (Back/Forward/Print...) —
  // meaningless inside an overlay (owner 2026-09-26). CEF73 contract:
  // returning true from RunContextMenu = the app owns the menu; we own
  // nothing, so nothing is shown.
  bool RunContextMenu(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                      CefRefPtr<CefContextMenuParams> params,
                      CefRefPtr<CefMenuModel> model,
                      CefRefPtr<CefRunContextMenuCallback> callback) override {
    return true;
  }
  bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                CefProcessId source_process,
                                CefRefPtr<CefProcessMessage> message) override;

  // CefRenderHandler — OSR: the host window is a LAYERED overlay (WS_EX_
  // LAYERED + UpdateLayeredWindow). Transparent pixels are invisible AND
  // click-through; the frame alpha drives the per-pixel hit test.
  void GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) override;
  void OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type,
               const RectList& dirtyRects, const void* buffer, int width,
               int height) override;

  // OSR input/present bridge used by share_win's window proc and timer.
  bool IsPixelOpaque(int x, int y);
  void ForwardMouseMove(int x, int y, bool leave);
  void ForwardMouseButton(int x, int y, bool down, bool left_button);
  void ForwardKey(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam);
  // Alt+Z toggle (WM_HOTKEY): closed -> main.main-menu + openOSC; open ->
  // back to base. The visibility bridge mirrors the state to the window.
  void ToggleOverlay();

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
  void PresentOsrFrame();

  const ShareLaunchParams* params_;
  HWND host_wnd_;
  CefRefPtr<CefMessageRouterBrowserSide> router_;
  CefRefPtr<ShareQueryHandler> query_handler_;
  CefRefPtr<CefBrowser> browser_;
  bool finishing_;   // finish scheduled (round-trip observed)
  bool finished_;    // FinishNow executed
  // OSR frame + layered-window presentation surface.
  std::vector<unsigned char> osr_frame_;
  int osr_w_ = 0;
  int osr_h_ = 0;
  HDC osr_dc_ = NULL;
  HBITMAP osr_bmp_ = NULL;
  void* osr_bits_ = NULL;

  IMPLEMENT_REFCOUNTING(ShareClient);
};

#endif  // SHARE_CLIENT_H_
