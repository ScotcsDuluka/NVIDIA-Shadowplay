// launcher_util.h - shared primitives of the Launcher CEF lane: UTF-8
// helpers, layout paths (AppLayout.vb root contract), a small line logger,
// Win32 process plumbing and the config.json read/patch API (the C++
// counterpart of Common\AppConfigShared.vb WriteBool — read-modify-write
// on the CURRENT file content so keys owned by other processes survive).
#ifndef LAUNCHER_UTIL_H_
#define LAUNCHER_UTIL_H_

#include <string>

#include "launcher_json.h"

namespace launcherutil {

// ── strings / paths ─────────────────────────────────────────────────────
std::string WideToUtf8(const std::wstring& in);
std::wstring Utf8ToWide(const std::string& in);
std::string LowerAscii(std::string s);
std::wstring GetExeDir();      // directory of the running .exe (process-wide)
std::wstring GetDllDir();      // directory of Launcher.dll (host body owner)
// Product layout root: the folder holding NvConfig\, NvOverlay\, Flags\...
// The bootstrap exe sits at the root; the body DLL sits in
// NvOverlay\Cef\ ("ลงที่เดียวกับ osc" — shared CEF runtime slot). Both
// resolve to the same root.
std::wstring GetRootDir();
std::wstring JoinPath(const std::wstring& a, const std::wstring& b);
std::string WidePathUtf8(const std::wstring& p);
bool FileExists(const std::wstring& path);
void EnsureParentDir(const std::wstring& path);

// ── logging ─────────────────────────────────────────────────────────────
// Appends one timestamped line to <root>\Logs\launcher-cef.log.
// Safe on any thread; never throws; a logging failure is silently dropped.
void LogInit();
void LogLine(const std::string& line);

// ── processes ───────────────────────────────────────────────────────────
bool ProcessRunning(const wchar_t* name);  // name WITHOUT .exe
// True when a process named |name| runs FROM |exe_path| (path dedupe —
// Main.vb StartCefOverlay contract: the WinForm overlay is also named
// "NVIDIA Share"; only the exact NvOverlay\Cef exe counts).
bool ProcessRunningFromPath(const wchar_t* name, const std::wstring& exe_path);
bool StartProcess(const std::wstring& exe_path, const std::wstring& args);
// TerminateProcess on every process named |name| (except our own PID).
void KillProcessByName(const wchar_t* name);
// TerminateProcess only on instances of |name| running FROM |exe_path|
// (exact image-path match, StartCefOverlay dedupe contract).
void KillProcessFromPath(const wchar_t* name, const std::wstring& exe_path);
bool WaitForTcpPort(unsigned port, unsigned timeout_ms);
// One-shot loopback send to the NVIDIA API hub (:5001). Frame parity with
// TcpClientHelper.Send: "[Send] <app>|<cmd>\r\n". Best effort.
bool SendHubLine(const std::string& line);

// ── config.json (AppConfigShared.vb contract) ───────────────────────────
bool ReadConfigBool(const wchar_t* section, const wchar_t* key,
                    bool fallback);
bool WriteConfigBool(const wchar_t* section, const wchar_t* key, bool value);

}  // namespace launcherutil

#endif  // LAUNCHER_UTIL_H_
