// launcher_win.cpp - see launcher_win.h.
#include "launcher_win.h"

#include <dwmapi.h>

#include "launcher_util.h"

// version.rc resource id of the NVIDIA ShadowPlay icon.
#define IDI_ICON1 101

#ifndef DWMWA_USE_IMMERSIVE_DARK_MODE
#define DWMWA_USE_IMMERSIVE_DARK_MODE 20
#endif
#ifndef DWMWA_WINDOW_CORNER_PREFERENCE
#define DWMWA_WINDOW_CORNER_PREFERENCE 33
#endif
#ifndef DWMWCP_ROUND
#define DWMWCP_ROUND 2
#endif

namespace launcherwin {

namespace {

const wchar_t* kWindowClass = L"NvLauncherHostWindow";

void (*g_close_request_cb)(void) = NULL;

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  switch (msg) {
    case WM_NCCALCSIZE:
      // Borderless-with-shadow contract: WS_THICKFRAME keeps the DWM
      // shadow + Win11 rounded corners, and returning 0 (when wParam is
      // TRUE) removes the visible frame area so the client covers the
      // whole window — the page paints edge to edge.
      if (wparam) return 0;
      break;
    case WM_CLOSE:
      // Forward to CEF (CloseBrowser) when a callback is registered —
      // destroying the window directly would bypass CEF teardown.
      if (g_close_request_cb) {
        g_close_request_cb();
        return 0;
      }
      DestroyWindow(hwnd);
      return 0;
    default:
      return DefWindowProcW(hwnd, msg, wparam, lparam);
  }
  return DefWindowProcW(hwnd, msg, wparam, lparam);
}

HBRUSH DarkBrush() {
  static HBRUSH brush = CreateSolidBrush(RGB(0x16, 0x17, 0x19));
  return brush;
}

// DWM polish so the borderless window never reads as a raw
// FormBorderStyle.None surface: dark frame, rounded corners (Win11),
// DWM shadow via WS_THICKFRAME. Attribute calls fail harmlessly pre-Win11.
void ApplyDwmPolish(HWND hwnd) {
  BOOL dark = TRUE;
  DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &dark,
                        sizeof(dark));
  UINT pref = DWMWCP_ROUND;
  DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, &pref,
                        sizeof(pref));
}

}  // namespace

void SetCloseRequestCallback(void (*cb)()) { g_close_request_cb = cb; }

HWND CreateLauncherWindow(HINSTANCE hinstance, bool show, int width,
                          int height, const wchar_t* title) {
  WNDCLASSEXW wc = {0};
  wc.cbSize = sizeof(wc);
  wc.style = CS_HREDRAW | CS_VREDRAW;
  wc.lpfnWndProc = WndProc;
  wc.hInstance = hinstance;
  wc.hIcon = LoadIconW(hinstance, MAKEINTRESOURCEW(IDI_ICON1));
  wc.hIconSm = LoadIconW(hinstance, MAKEINTRESOURCEW(IDI_ICON1));
  wc.hCursor = LoadCursor(NULL, IDC_ARROW);
  // Dark brush instead of COLOR_WINDOW: no white flash before the CEF
  // child view attaches (Share lane lesson).
  wc.hbrBackground = DarkBrush();
  wc.lpszClassName = kWindowClass;
  RegisterClassExW(&wc);

  // Borderless app window WITH the DWM frame contract (shadow + rounded
  // corners): WS_THICKFRAME supplies the frame metrics that DWM needs,
  // WM_NCCALCSIZE removes the visible frame. WS_EX_APPWINDOW keeps the
  // taskbar presence; WS_SYSMENU + WS_MINIMIZEBOX give the taskbar
  // right-click contract of the old FormBorderStyle.None form.
  DWORD style =
      WS_POPUP | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_CAPTION;
  DWORD ex_style = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;

  int screen_w = GetSystemMetrics(SM_CXSCREEN);
  int screen_h = GetSystemMetrics(SM_CYSCREEN);
  // WM_NCCALCSIZE (return 0) removes the frame area, so the CLIENT rect
  // equals the WINDOW rect — pass width/height as the window size
  // directly (AdjustWindowRect would double-count the removed frame).
  int x = (screen_w - width) / 2;
  int y = (screen_h - height) / 2;
  if (x < 0) x = 0;
  if (y < 0) y = 0;

  HWND hwnd = CreateWindowExW(ex_style, kWindowClass, title, style, x, y,
                              width, height, NULL, NULL, hinstance, NULL);
  if (hwnd) {
    ApplyDwmPolish(hwnd);
    if (show) {
      ShowWindow(hwnd, SW_SHOW);
      UpdateWindow(hwnd);
    }
  }
  return hwnd;
}

void BeginWindowDrag(HWND hwnd) {
  if (!hwnd) return;
  ReleaseCapture();
  SendMessageW(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
}

void Minimize(HWND hwnd) {
  if (hwnd) ShowWindow(hwnd, SW_MINIMIZE);
}

}  // namespace launcherwin
