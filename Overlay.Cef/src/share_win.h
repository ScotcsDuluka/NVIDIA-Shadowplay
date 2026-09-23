// share_win.h - Win32 helpers for the CEF lane host (UTF-8 <-> UTF-16,
// fullscreen probe port of Overlay.Engine's WinFullscreen, clipboard,
// hidden host window).
#ifndef SHARE_WIN_H_
#define SHARE_WIN_H_

#include <string>
#include <windows.h>

namespace sharewin {

std::string WideToUtf8(const std::wstring& in);
std::wstring Utf8ToWide(const std::string& in);

// Cryptographically seeded random hex string (rand_s), |bytes| random
// bytes rendered as 2*|bytes| hex chars. Used for the
// QUERY_WIN_NODE_INFO secret, mirroring the engine's auth secret.
std::string RandomHex(size_t bytes);

// Port of CefQueryBridge.vb WinFullscreen.IsFullscreenActive: foreground
// window covers its monitor and has no WS_CAPTION. Always fills |probe|
// with the decisive diagnostic (same format as LastFullscreenProbe).
bool IsFullscreenActive(std::string* probe);

// CF_UNICODETEXT set via user32 clipboard (parity with
// QUERY_WIN_COPY_TO_CLIPBOARD's Clipboard.SetText).
bool SetClipboardText(HWND owner, const std::string& utf8_text);

// Registers and creates the host window. Returns the HWND (NULL on
// failure); hidden unless |show|.
HWND CreateHostWindow(HINSTANCE hinstance, bool show, int width, int height,
                      const wchar_t* title);

// WM_CLOSE on the host window is forwarded to |cb| (which must call
// CefBrowserHost::CloseBrowser — direct DestroyWindow would bypass CEF
// teardown). Pass NULL to restore direct destruction.
void SetHostCloseRequestCallback(void (*cb)(void));

std::wstring GetExeDir();

}  // namespace sharewin

#endif  // SHARE_WIN_H_
