// bootstrap.cpp - NVIDIA Share.exe: thin CEF bootstrap. Loads the host
// body (NVIDIA Share.dll beside this exe, NvOverlay\CEF owner layout) and
// calls its NvShareCefMain export, which handles the whole CEF process
// split (browser process + subprocess relaunches of this same exe).
#include <windows.h>

namespace {
typedef int (*NvShareCefMainFn)(void);
}

int APIENTRY wWinMain(HINSTANCE, HINSTANCE, LPWSTR, int) {
  HMODULE host = LoadLibraryW(L"NVIDIA Share.dll");
  if (!host) {
    MessageBoxW(NULL,
                L"NVIDIA Share.dll could not be loaded.\n\n"
                L"The CEF host body must sit beside this executable "
                L"(NvOverlay\\CEF owner layout: NVIDIA Share.exe + "
                L"NVIDIA Share.dll + libcef.dll + Resources + locales + "
                L"cef.pak).",
                L"NVIDIA Share", MB_ICONERROR);
    return 1;
  }
  NvShareCefMainFn main_fn =
      reinterpret_cast<NvShareCefMainFn>(GetProcAddress(host, "NvShareCefMain"));
  if (!main_fn) {
    MessageBoxW(NULL, L"NVIDIA Share.dll is missing the NvShareCefMain export.",
                L"NVIDIA Share", MB_ICONERROR);
    return 1;
  }
  return main_fn();
}
