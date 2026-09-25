// launcher_win.h - host window for the launcher CEF child view.
// Borderless NVIDIA-style app window (dark, centered, taskbar-present,
// NOT topmost — this is a desktop app, unlike the fullscreen overlay
// host). Drag/minimize/close are driven by the page over cefQuery.
#ifndef LAUNCHER_WIN_H_
#define LAUNCHER_WIN_H_

#include <windows.h>

namespace launcherwin {

// WM_CLOSE -> registered callback (CEF CloseBrowser path — destroying the
// window directly would bypass CEF teardown).
void SetCloseRequestCallback(void (*cb)());

HWND CreateLauncherWindow(HINSTANCE hinstance, bool show, int width,
                          int height, const wchar_t* title);

// ReleaseCapture + WM_NCLBUTTONDOWN/HTCAPTION (Main.vb BOX_LOGO_MouseDown
// contract) so the page can drag the borderless window from its top bar.
void BeginWindowDrag(HWND hwnd);

// Borrow WS_CAPTION so DWM plays the standard close animation when the
// borderless window is destroyed (dropped again on restore).
void PrepareCloseFrame(HWND hwnd);

void Minimize(HWND hwnd);

}  // namespace launcherwin

#endif  // LAUNCHER_WIN_H_
