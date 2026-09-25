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

// Fade-in state (old WinForm launcher had an Opacity fade; borderless
// native windows pop in with none). One host window per browser process,
// so a file-scope counter is safe. While the fade runs the page has not
// painted yet (first paint ~1.3s), so layering only affects the dark
// frame — it cannot clip CEF content. The WS_EX_LAYERED bit is REMOVED
// when the fade completes so CEF GPU compositing is never layered.
BYTE g_fade_alpha = 0;

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  switch (msg) {
    case WM_NCCALCSIZE:
      // Borderless-with-shadow contract: WS_THICKFRAME keeps the DWM
      // shadow + Win11 rounded corners, and returning 0 (when wParam is
      // TRUE) removes the visible frame area so the client covers the
      // whole window — the page paints edge to edge.
      if (wparam) return 0;
      break;
    case WM_TIMER:
      if (wparam == 1) {
        g_fade_alpha = (g_fade_alpha > 229) ? 255 : (BYTE)(g_fade_alpha + 26);
        SetLayeredWindowAttributes(hwnd, 0, g_fade_alpha, LWA_ALPHA);
        if (g_fade_alpha >= 255) {
          KillTimer(hwnd, 1);
          SetWindowLongPtrW(hwnd, GWL_EXSTYLE,
                            GetWindowLongPtrW(hwnd, GWL_EXSTYLE) &
                                ~WS_EX_LAYERED);
        }
        return 0;
      }
      if (wparam == 2) {
        // Restore animation done — drop the borrowed caption (if still
        // present; Minimize() cancels this timer when re-minimizing).
        // NEVER restyle inside WM_SIZE itself: a frame change there can
        // re-enter WM_SIZE and starve the message pump (the
        // "กดอะไรไม่ได้เลย" freeze). Deferring via this timer fixes it.
        KillTimer(hwnd, 2);
        if (GetWindowLongPtrW(hwnd, GWL_STYLE) & WS_CAPTION) {
          SetWindowLongPtrW(hwnd, GWL_STYLE,
                            GetWindowLongPtrW(hwnd, GWL_STYLE) & ~WS_CAPTION);
          SetWindowPos(hwnd, NULL, 0, 0, 0, 0,
                       SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE |
                           SWP_NOZORDER | SWP_NOACTIVATE);
        }
        return 0;
      }
      break;
    case WM_SIZE:
      if (wparam == SIZE_RESTORED) {
        // Restored from an animated minimize — schedule the caption
        // strip (see WM_TIMER id 2).
        SetTimer(hwnd, 2, 350, NULL);
      }
      break;
    case WM_CLOSE:
      // Forward to CEF (CloseBrowser) when a callback is registered —
      // destroying the window directly would bypass CEF teardown.
      if (g_close_request_cb) {
        PrepareCloseFrame(hwnd);
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

void PrepareCloseFrame(HWND hwnd) {
  // Borderless (WS_POPUP, no caption) windows get NO DWM close/minimize
  // animation. Borrow WS_CAPTION for the duration of the animation —
  // NCCALCSIZE still strips the visible frame, so nothing shifts; DWM
  // just plays the standard zoom-fade / minimize-to-taskbar. The caption
  // is dropped again on WM_SIZE(SIZE_RESTORED).
  launcherutil::LogLine("borrow caption frame for window animation");
  SetWindowLongPtrW(hwnd, GWL_STYLE,
                    GetWindowLongPtrW(hwnd, GWL_STYLE) | WS_CAPTION);
  SetWindowPos(hwnd, NULL, 0, 0, 0, 0,
               SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
                   SWP_NOACTIVATE);
}

HWND CreateLauncherWindow(HINSTANCE hinstance, bool show, int width,
                          int height, const wchar_t* title) {
  WNDCLASSEXW wc = {0};
  wc.cbSize = sizeof(wc);
  // No CS_HREDRAW/CS_VREDRAW: the client is fully covered by the CEF
  // child (fixed-size window), and forced full repaints are what made
  // the open/minimize animations flicker ("animation แปลกๆ").
  wc.style = 0;
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
  // corners). NO WS_CAPTION — on some Win11 builds the caption survives
  // WM_NCCALCSIZE=0 and stacks a second titlebar over the page nav
  // ("มันซ้อน" — owner report 2026-09-25). WS_THICKFRAME alone still
  // supplies the DWM shadow + rounding, WM_NCCALCSIZE removes its edge.
  // WS_EX_APPWINDOW keeps taskbar presence; WS_SYSMENU + WS_MINIMIZEBOX
  // give the taskbar right-click contract of the old FormBorderStyle.None
  // form.
  DWORD style = WS_POPUP | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX;
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
      // Layered fade-in (~160ms, 16ms steps) — the open-animation
      // contract of the old WinForm launcher. Layered bit is dropped
      // once opaque (see WM_TIMER).
      g_fade_alpha = 0;
      SetWindowLongPtrW(hwnd, GWL_EXSTYLE,
                        GetWindowLongPtrW(hwnd, GWL_EXSTYLE) |
                            WS_EX_LAYERED);
      SetLayeredWindowAttributes(hwnd, 0, 0, LWA_ALPHA);
      ShowWindow(hwnd, SW_SHOW);
      SetTimer(hwnd, 1, 16, NULL);
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
  if (!hwnd) return;
  if (GetWindowLongPtrW(hwnd, GWL_STYLE) & WS_CAPTION) {
    // A pending restore-strip timer (id 2) would fight the re-borrow.
    KillTimer(hwnd, 2);
  }
  PrepareCloseFrame(hwnd);  // borrow the caption frame for the animation
  ShowWindow(hwnd, SW_MINIMIZE);
}

}  // namespace launcherwin
