// bootstrap.cpp - Launcher.exe: thin CEF bootstrap at the product root
// slot (the old WinForm Launcher.exe double-click entry). Loads the host
// body NvOverlay\Cef\Launcher.dll ("ลงที่เดียวกับ osc" — the shared CEF
// runtime slot) by ABSOLUTE path and calls its NvLauncherCefMain export,
// which implements the whole CEF process split (browser process +
// subprocess relaunches of this same exe).
#include <windows.h>

#include <stdio.h>
#include <string>

namespace {

// Absolute path of NvOverlay\Cef\Launcher.dll next to this exe's root.
bool ResolveBodyPath(std::wstring* out) {
  wchar_t exe[MAX_PATH];
  if (!GetModuleFileNameW(NULL, exe, MAX_PATH)) return false;
  std::wstring dir(exe);
  size_t slash = dir.find_last_of(L'\\');
  if (slash == std::wstring::npos) return false;
  dir.resize(slash);
  *out = dir + L"\\NvOverlay\\Cef\\Launcher.dll";
  return true;
}

}  // namespace

int APIENTRY wWinMain(HINSTANCE, HINSTANCE, LPWSTR, int) {
  std::wstring body_path;
  if (!ResolveBodyPath(&body_path)) {
    MessageBoxW(NULL, L"Launcher.dll path could not be resolved.",
                L"NVIDIA ShadowPlay", MB_ICONERROR);
    return 1;
  }

  // The body DLL statically imports libcef.dll — it lives in the SAME
  // directory (NvOverlay\Cef). SetDllDirectory inserts that directory into
  // the loader search order (position 2, right after the app dir) BEFORE
  // LoadLibrary, so the import resolves from the shared osc runtime slot.
  std::wstring body_dir = body_path;
  {
    size_t slash = body_dir.find_last_of(L'\\');
    if (slash != std::wstring::npos) body_dir.resize(slash);
    SetDllDirectoryW(body_dir.c_str());
  }

  HMODULE host = LoadLibraryW(body_path.c_str());
  if (!host) {
    wchar_t msg[1024];
    swprintf(msg, 1024,
             L"NvOverlay\\Cef\\Launcher.dll could not be loaded (error %lu).\n\n"
             L"The launcher host body must sit in the shared CEF runtime "
             L"slot beside NVIDIA Share.exe:\n%s",
             GetLastError(), body_path.c_str());
    MessageBoxW(NULL, msg, L"NVIDIA ShadowPlay", MB_ICONERROR);
    return 1;
  }
  typedef int (*NvLauncherCefMainFn)(void);
  NvLauncherCefMainFn main_fn =
      reinterpret_cast<NvLauncherCefMainFn>(GetProcAddress(host,
                                                           "NvLauncherCefMain"));
  if (!main_fn) {
    MessageBoxW(NULL,
                L"Launcher.dll is missing the NvLauncherCefMain export.",
                L"NVIDIA ShadowPlay", MB_ICONERROR);
    return 1;
  }
  return main_fn();
}
