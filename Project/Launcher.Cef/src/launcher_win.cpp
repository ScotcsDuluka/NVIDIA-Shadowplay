// launcher_win.cpp - see launcher_win.h.
#include "launcher_win.h"

#include "launcher_util.h"

// version.rc resource id of the NVIDIA ShadowPlay icon.
#define IDI_ICON1 101

namespace launcherwin {

namespace {

const wchar_t* kWindowClass = L"NvLauncherHostWindow";

void (*g_close_request_cb)(void) = NULL;

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  switch (msg) {
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
}

HBRUSH DarkBrush() {
  static HBRUSH brush = CreateSolidBrush(RGB(0x16, 0x17, 0x19));
  return brush;
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

  // Borderless app window, centered, taskbar-present (WS_EX_APPWINDOW),
  // minimizable from the taskbar (WS_SYSMENU + WS_MINIMIZEBOX give the
  // taskbar right-click contract of the old FormBorderStyle.None form).
  DWORD style = WS_POPUP | WS_SYSMENU | WS_MINIMIZEBOX;
  DWORD ex_style = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;

  int screen_w = GetSystemMetrics(SM_CXSCREEN);
  int screen_h = GetSystemMetrics(SM_CYSCREEN);
  // WS_POPUP has no non-client area: client size == window size.
  int x = (screen_w - width) / 2;
  int y = (screen_h - height) / 2;
  if (x < 0) x = 0;
  if (y < 0) y = 0;

  HWND hwnd = CreateWindowExW(ex_style, kWindowClass, title, style, x, y,
                              width, height, NULL, NULL, hinstance, NULL);
  if (hwnd && show) {
    ShowWindow(hwnd, SW_SHOW);
    UpdateWindow(hwnd);
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
