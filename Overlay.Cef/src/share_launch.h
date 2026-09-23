// share_launch.h - launch parameters and host context handed over before
// CefInitialize (single-threaded window of time, read afterwards on the
// browser process UI thread).
#ifndef SHARE_LAUNCH_H_
#define SHARE_LAUNCH_H_

#include <string>

class ShareStorage;

struct ShareLaunchParams {
  std::string url;               // page to load (http://127.0.0.1:<port>/index.html)
  bool show_window = false;      // --show (visual evidence runs)
  bool proof_mode = false;       // --proof-of-life
  bool stay_open = false;        // --stay-open: record proof, keep the UI up
  unsigned self_exit_ms = 25000; // --self-exit-ms watchdog (proof mode)
  bool gpu_accel = true;         // NVIDIA Share.json nv-gpu-accel
};

// Function-local static: set before CefInitialize spawns worker threads,
// read-only afterwards.
ShareLaunchParams* GetLaunchParams();

struct ShareHostContext {
  int http_port = 0;
  std::string secret;            // QUERY_WIN_NODE_INFO auth secret
  ShareStorage* storage = 0;     // owned by share_main
};

ShareHostContext* GetHostContext();

#endif  // SHARE_LAUNCH_H_
