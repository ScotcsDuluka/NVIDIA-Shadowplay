// share_proof.cpp - see share_proof.h.
// Required checkpoint set mirrors the mission proof-of-life chain:
//   PROC_START -> HTTP_SERVER_UP -> CEF_INIT_OK -> BROWSER_CREATED ->
//   PAGE_LOADED -> PROOF_ECHO_VERIFIED
// ORGANIC_QUERY / DOM_PROBE are recorded when the real page drives them
// and are evidence, not requirements (the page may take longer paths).
#include "share_proof.h"

#include <stdio.h>
#include <windows.h>

#include <fstream>
#include <map>
#include <mutex>
#include <sstream>
#include <vector>

#include "share_json.h"
#include "share_win.h"

namespace shareproof {
namespace {

const char* kRequired[] = {
    "PROC_START", "HTTP_SERVER_UP", "CEF_INIT_OK",
    "BROWSER_CREATED", "PAGE_LOADED", "PROOF_ECHO_VERIFIED",
};

struct CheckpointRec {
  std::string name;
  double ms;
  std::string utc;
  std::string detail;
};

struct ProofState {
  std::mutex lock;
  bool proof_mode = false;
  std::wstring exe_dir;
  std::wstring pid_tag;
  std::wstring log_path;
  LARGE_INTEGER qpc_freq;
  LARGE_INTEGER qpc_start;
  std::string start_utc;
  std::map<std::string, std::string> fields;   // key -> raw JSON value
  std::vector<CheckpointRec> checkpoints;
  std::map<std::string, bool> seen;
};

ProofState& State() {
  static ProofState* s = new ProofState();
  return *s;
}

std::string NowUtcIso() {
  SYSTEMTIME st;
  GetSystemTime(&st);
  char buf[64];
  sprintf(buf, "%04u-%02u-%02uT%02u:%02u:%02u.%03uZ",
          st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond,
          st.wMilliseconds);
  return buf;
}

double MsSinceStart(const ProofState& st) {
  LARGE_INTEGER now;
  QueryPerformanceCounter(&now);
  return static_cast<double>(now.QuadPart - st.qpc_start.QuadPart) *
         1000.0 / static_cast<double>(st.qpc_freq.QuadPart);
}

std::string MakeLogHeader() {
  std::ostringstream o;
  o << "==== NVIDIA Share (CEF OSC host) run " << NowUtcIso() << " ====\n";
  return o.str();
}

void AppendLog(const std::string& line) {
  ProofState& st = State();
  std::ofstream f(st.log_path.c_str(), std::ios::app);
  if (f.is_open()) {
    f << line;
    f.flush();
  }
}

}  // namespace

void Init(bool proof_mode) {
  ProofState& st = State();
  std::lock_guard<std::mutex> g(st.lock);
  st.proof_mode = proof_mode;
  QueryPerformanceFrequency(&st.qpc_freq);
  QueryPerformanceCounter(&st.qpc_start);
  st.start_utc = NowUtcIso();

  wchar_t exe[MAX_PATH];
  GetModuleFileNameW(NULL, exe, MAX_PATH);
  std::wstring dir(exe);
  size_t slash = dir.find_last_of(L'\\');
  if (slash != std::wstring::npos) dir.resize(slash);
  st.exe_dir = dir;
  // PID-tagged files: concurrent instances (browser + subprocess relaunches
  // of earlier runs) must never interleave evidence.
  wchar_t pid_tag[32];
  swprintf(pid_tag, 32, L"-%lu", (unsigned long)GetCurrentProcessId());
  st.pid_tag = pid_tag;
  st.log_path = dir + L"\\Logs\\nvidia-share-host" + st.pid_tag + L".log";

  CreateDirectoryW((dir + L"\\Logs").c_str(), NULL);

  AppendLog(MakeLogHeader());
}

void EnableProofMode() {
  ProofState& st = State();
  std::lock_guard<std::mutex> g(st.lock);
  if (st.proof_mode) return;
  st.proof_mode = true;
  AppendLog("proof-of-life mode enabled (browser-process args)\n");
}

void LogLine(const std::string& line) {
  ProofState& st = State();
  std::ostringstream o;
  o << "[" << NowUtcIso() << "] " << line << "\n";
  OutputDebugStringA(o.str().c_str());
  std::lock_guard<std::mutex> g(st.lock);
  AppendLog(o.str());
}

void Checkpoint(const char* name, const std::string& detail) {
  ProofState& st = State();
  std::lock_guard<std::mutex> g(st.lock);
  if (st.seen[name]) {
    AppendLog("checkpoint (dup, ignored): " + std::string(name) + "\n");
    return;
  }
  st.seen[name] = true;
  CheckpointRec cp;
  cp.name = name;
  cp.ms = MsSinceStart(st);
  cp.utc = NowUtcIso();
  cp.detail = detail;
  std::ostringstream log;
  log << "[checkpoint] " << name << " at " << cp.ms << " ms";
  if (!detail.empty()) log << " (" << detail << ")";
  log << "\n";
  OutputDebugStringA(log.str().c_str());
  AppendLog(log.str());
  st.checkpoints.push_back(cp);
  if (st.proof_mode) WriteProof();
}

void SetField(const char* key, const std::string& json_value) {
  ProofState& st = State();
  std::lock_guard<std::mutex> g(st.lock);
  st.fields[key] = json_value;
  if (st.proof_mode) WriteProof();
}

void WriteProof() {
  ProofState& st = State();
  // caller holds st.lock (Checkpoint/SetField) — file write under lock is
  // cheap relative to CEF work and keeps the proof file crash-consistent.
  std::ostringstream o;
  o << "{\n  \"run\": {\"startUtc\": " << sharejson::Escape(st.start_utc)
    << ", \"mode\": " << (st.proof_mode ? "\"proof-of-life\"" : "\"normal\"")
    << ", \"exeDir\": " << sharejson::Escape(sharewin::WideToUtf8(st.exe_dir))
    << "},\n";
  o << "  \"fields\": {";
  bool first = true;
  for (std::map<std::string, std::string>::const_iterator it = st.fields.begin();
       it != st.fields.end(); ++it) {
    if (!first) o << ", ";
    first = false;
    o << sharejson::Escape(it->first) << ": " << it->second;
  }
  o << "},\n";
  o << "  \"checkpoints\": [\n";
  for (size_t i = 0; i < st.checkpoints.size(); ++i) {
    const CheckpointRec& c = st.checkpoints[i];
    o << "    {\"name\": " << sharejson::Escape(c.name)
      << ", \"ms\": " << c.ms
      << ", \"utc\": " << sharejson::Escape(c.utc)
      << ", \"detail\": " << sharejson::Escape(c.detail) << "}";
    if (i + 1 < st.checkpoints.size()) o << ",";
    o << "\n";
  }
  o << "  ]\n}\n";

  std::ofstream f((st.exe_dir + L"\\Logs\\proof-of-life" + st.pid_tag +
                   L".json").c_str(),
                  std::ios::binary | std::ios::trunc);
  if (f.is_open()) {
    f << o.str();
    f.flush();
  }
}

int ExitCode() {
  ProofState& st = State();
  std::lock_guard<std::mutex> g(st.lock);
  if (!st.proof_mode) return 0;
  for (size_t i = 0; i < sizeof(kRequired) / sizeof(kRequired[0]); ++i) {
    if (!st.seen[kRequired[i]]) {
      AppendLog(std::string("exit code 2: missing checkpoint ") + kRequired[i] + "\n");
      return 2;
    }
  }
  return 0;
}

}  // namespace shareproof
