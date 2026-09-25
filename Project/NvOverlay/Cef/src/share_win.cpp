// share_win.cpp - see share_win.h.
#define _CRT_RAND_S 1  // must precede <stdlib.h> for rand_s
#include "share_win.h"

#include <stdio.h>
#include <stdlib.h>

#include <string>

#include "share_proof.h"

namespace sharewin {

std::string WideToUtf8(const std::wstring& in) {
  if (in.empty()) return std::string();
  int n = WideCharToMultiByte(CP_UTF8, 0, in.c_str(), (int)in.size(),
                              NULL, 0, NULL, NULL);
  std::string out(n, 0);
  if (n > 0) {
    WideCharToMultiByte(CP_UTF8, 0, in.c_str(), (int)in.size(), &out[0], n,
                        NULL, NULL);
  }
  return out;
}

std::wstring Utf8ToWide(const std::string& in) {
  if (in.empty()) return std::wstring();
  int n = MultiByteToWideChar(CP_UTF8, 0, in.c_str(), (int)in.size(), NULL, 0);
  std::wstring out(n, 0);
  if (n > 0) {
    MultiByteToWideChar(CP_UTF8, 0, in.c_str(), (int)in.size(), &out[0], n);
  }
  return out;
}

std::string RandomHex(size_t bytes) {
  static const char* hex = "0123456789abcdef";
  std::string out;
  out.reserve(bytes * 2);
  for (size_t i = 0; i < bytes; ++i) {
    unsigned int r = 0;
    rand_s(&r);
    out.push_back(hex[(r >> 4) & 0xF]);
    out.push_back(hex[r & 0xF]);
  }
  return out;
}

// Port of CefQueryBridge.vb WinFullscreen.IsFullscreenActive (the decisive
// diagnostics string format matches LastFullscreenProbe).
bool IsFullscreenActive(std::string* probe) {
  struct WinRectT { LONG Left, Top, Right, Bottom; };
  HWND hwnd = GetForegroundWindow();
  if (!hwnd) {
    *probe = "no foreground window";
    return false;
  }
  DWORD pid = 0;
  GetWindowThreadProcessId(hwnd, &pid);
  if (pid == GetCurrentProcessId()) {
    char buf[64];
    sprintf(buf, "own-process pid=%lu", (unsigned long)pid);
    *probe = buf;
    return false;
  }
  const LONG GWL_STYLE_T = GWL_STYLE;
  const LONG WS_CAPTION_T = WS_CAPTION;
  LONG_PTR style = GetWindowLongPtrW(hwnd, GWL_STYLE_T);
  RECT r;
  if (!GetWindowRect(hwnd, &r)) {
    *probe = "GetWindowRect failed";
    return false;
  }
  MONITORINFO mi;
  mi.cbSize = sizeof(mi);
  if (!GetMonitorInfoW(MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST), &mi)) {
    *probe = "GetMonitorInfo failed";
    return false;
  }
  RECT screen = mi.rcMonitor;
  bool covers = r.left <= screen.left && r.top <= screen.top &&
                r.right >= screen.right && r.bottom >= screen.bottom;
  char buf[256];
  sprintf(buf, "hwnd=0x%p pid=%lu style=0x%IX rect=(%ld,%ld)-(%ld,%ld) screen=%ldx%ld covers=%d",
          (void*)hwnd, (unsigned long)pid, style, r.left, r.top, r.right,
          r.bottom, screen.right - screen.left, screen.bottom - screen.top,
          covers ? 1 : 0);
  *probe = buf;
  if (style & WS_CAPTION_T) return false;
  return covers;
}

bool SetClipboardText(HWND owner, const std::string& utf8_text) {
  if (!OpenClipboard(owner)) return false;
  bool ok = false;
  std::wstring wide = Utf8ToWide(utf8_text);
  SIZE_T bytes = (wide.size() + 1) * sizeof(wchar_t);
  HGLOBAL mem = GlobalAlloc(GMEM_MOVEABLE, bytes);
  if (mem) {
    void* p = GlobalLock(mem);
    if (p) {
      memcpy(p, wide.c_str(), bytes);
      GlobalUnlock(mem);
      EmptyClipboard();
      ok = SetClipboardData(CF_UNICODETEXT, mem) != NULL;
      if (!ok) GlobalFree(mem);
    } else {
      GlobalFree(mem);
    }
  }
  CloseClipboard();
  return ok;
}

#define IDI_ICON1 101

namespace {

const wchar_t* kHostWindowClass = L"NvShareHostWindow";

void (*g_close_request_cb)(void) = NULL;

LRESULT CALLBACK HostWndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
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

}  // namespace

void SetHostCloseRequestCallback(void (*cb)(void)) {
  g_close_request_cb = cb;
}

HWND CreateHostWindow(HINSTANCE hinstance, bool show, int width, int height,
                      const wchar_t* title) {
  WNDCLASSEXW wc = {0};
  wc.cbSize = sizeof(wc);
  wc.style = CS_HREDRAW | CS_VREDRAW;
  wc.lpfnWndProc = HostWndProc;
  wc.hInstance = hinstance;
  wc.hIcon = LoadIconW(hinstance, MAKEINTRESOURCEW(IDI_ICON1));
  wc.hIconSm = LoadIconW(hinstance, MAKEINTRESOURCEW(IDI_ICON1));
  wc.hCursor = LoadCursor(NULL, IDC_ARROW);
  // No class background brush: the white COLOR_WINDOW fill is what flashed
  // through before CEF attached its child view. CEF paints its own
  // background (settings.background_color, black) once initialized.
  wc.hbrBackground = NULL;
  wc.lpszClassName = kHostWindowClass;
  RegisterClassExW(&wc);

  // Desktop-overlay shape (Share instance-3 contract): borderless, always
  // on top, covering the PRIMARY screen only (owner call 2026-09-24:
  // คลุม main screen — multi-monitor setups keep their other screens
  // usable). The width/height arguments stay in the signature for callers
  // but the overlay geometry wins — the GFE osc window is never a small
  // framed app window.
  (void)width;
  (void)height;
  DWORD style = WS_POPUP;
  DWORD ex_style = WS_EX_TOPMOST;
  int w = GetSystemMetrics(SM_CXSCREEN);
  int h = GetSystemMetrics(SM_CYSCREEN);
  HWND hwnd = CreateWindowExW(ex_style, kHostWindowClass, title, style, 0, 0,
                              w, h, NULL, NULL, hinstance, NULL);
  if (hwnd && show) {
    ShowWindow(hwnd, SW_SHOW);
    UpdateWindow(hwnd);
  }
  return hwnd;
}

std::wstring GetExeDir() {
  wchar_t exe[MAX_PATH];
  GetModuleFileNameW(NULL, exe, MAX_PATH);
  std::wstring dir(exe);
  size_t slash = dir.find_last_of(L'\\');
  if (slash != std::wstring::npos) dir.resize(slash);
  return dir;
}

}  // namespace sharewin
