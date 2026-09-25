// share_win.cpp - see share_win.h.
#define _CRT_RAND_S 1  // must precede <stdlib.h> for rand_s
#include <winsock2.h>    // MUST precede windows.h (pulled by share_win.h)
#include <ws2tcpip.h>
#include "share_win.h"

#include <stdio.h>
#include <stdlib.h>

#include <string>

#include "share_client.h"
#include "share_proof.h"

// windowsx.h's macros (GetNextSibling etc.) break the CEF headers — define
// only what the WndProc needs.
#ifndef GET_X_LPARAM
#define GET_X_LPARAM(l) ((int)(short)LOWORD(l))
#define GET_Y_LPARAM(l) ((int)(short)HIWORD(l))
#endif

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

// ShadowPlay hotkey plumbing (the native-listener stand-in): Alt+Z is
// caught by RegisterHotKey; the fire goes to the node's shim listener
// (port 59002) which drives the GENUINE HotkeyCallback ->
// /ShadowPlay/v.1.0/Hotkey socket event -> the page toggles itself.
static void FireHotkey(const char* name) {
  WSADATA wsa;
  if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return;
  SOCKET s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
  if (s != INVALID_SOCKET) {
    sockaddr_in a;
    ZeroMemory(&a, sizeof(a));
    a.sin_family = AF_INET;
    a.sin_port = htons(59002);
    a.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
    if (connect(s, reinterpret_cast<sockaddr*>(&a), sizeof(a)) == 0) {
      char req[160];
      lstrcpyA(req, "GET /fire?hk=");
      lstrcatA(req, name);
      lstrcatA(req, " HTTP/1.1\r\nHost: 127.0.0.1\r\nConnection: close\r\n\r\n");
      send(s, req, lstrlenA(req), 0);
    }
    closesocket(s);
  }
  WSACleanup();
}

LRESULT CALLBACK HostWndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  switch (msg) {
    case WM_HOTKEY:
      // Alt+Z — routed through the ShadowPlay hotkey system.
      if (wparam == 1) FireHotkey("OpenShare");
      break;
    case WM_TIMER:
      // Per-pixel hit test (30ms): read the last OSR frame's alpha under
      // the cursor — opaque = the window takes the click and forwards it
      // to the page; transparent = WS_EX_TRANSPARENT lets it fall through
      // to the game/desktop. This IS the overlay input contract.
      if (wparam == 1 && ShareClient::active_client_ && IsWindowVisible(hwnd)) {
        POINT pt;
        GetCursorPos(&pt);
        RECT r;
        GetWindowRect(hwnd, &r);
        int x = pt.x - r.left, y = pt.y - r.top;
        bool opaque = ShareClient::active_client_->IsPixelOpaque(x, y);
        LONG ex = GetWindowLongW(hwnd, GWL_EXSTYLE);
        bool transparent_now = (ex & WS_EX_TRANSPARENT) != 0;
        if (opaque == transparent_now) {
          SetWindowLongW(hwnd, GWL_EXSTYLE,
                         transparent_now ? (ex & ~WS_EX_TRANSPARENT)
                                         : (ex | WS_EX_TRANSPARENT));
        }
        if (opaque) ShareClient::active_client_->ForwardMouseMove(x, y, false);
      }
      break;
    case WM_LBUTTONDOWN:
    case WM_LBUTTONUP:
    case WM_RBUTTONDOWN:
    case WM_RBUTTONUP:
    case WM_MOUSEMOVE:
      if (ShareClient::active_client_) {
        int x = GET_X_LPARAM(lparam), y = GET_Y_LPARAM(lparam);
        if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN)
          SetFocus(hwnd);
        if (msg == WM_MOUSEMOVE)
          ShareClient::active_client_->ForwardMouseMove(x, y, false);
        else
          ShareClient::active_client_->ForwardMouseButton(
              x, y, msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN,
              msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP);
      }
      break;
    case WM_KEYDOWN:
    case WM_KEYUP:
    case WM_SYSKEYDOWN:
    case WM_SYSKEYUP:
    case WM_CHAR:
      if (ShareClient::active_client_)
        ShareClient::active_client_->ForwardKey(hwnd, msg, wparam, lparam);
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
  return 0;
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
  // Dark brush instead of COLOR_WINDOW/NULL: the Launcher.Cef lane adopted
  // this first ("Share lane lesson") — with no brush the pre-CEF surface is
  // undefined garbage that reads as a giant black box until the child view
  // paints its first frame.
  wc.hbrBackground = CreateSolidBrush(RGB(0x16, 0x17, 0x19));
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
  DWORD ex_style = WS_EX_TOPMOST | WS_EX_LAYERED;
  int w = GetSystemMetrics(SM_CXSCREEN);
  int h = GetSystemMetrics(SM_CYSCREEN);
  HWND hwnd = CreateWindowExW(ex_style, kHostWindowClass, title, style, 0, 0,
                              w, h, NULL, NULL, hinstance, NULL);
  if (hwnd && show) {
    ShowWindow(hwnd, SW_SHOW);
    UpdateWindow(hwnd);
  }
  // Per-pixel hit-test tick: frame alpha under the cursor decides
  // click-through (WS_EX_TRANSPARENT) vs page input forwarding.
  if (hwnd) SetTimer(hwnd, 1, 30, NULL);
  // ShadowPlay overlay toggle hotkey: Alt+Z (routed through the node's
  // hotkey system — FireHotkey on WM_HOTKEY).
  if (hwnd) RegisterHotKey(hwnd, 1, MOD_ALT | MOD_NOREPEAT, 'Z');
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
