// share_config.h - NVIDIA Share.json loader.
// Mirrors the real GFE 3.28 "NVIDIA Share.json" switch surface
// (reference: C:\My Project\GFE\GeForce_Experience_v3.28.0.412\GFExperience\NVIDIA Share.json):
//   nv-osc=true, nv-gpu-accel=true, nv-url-relative=osc/index.html,
//   nv-node-app, nv-plugin-folder-relative, nv-plugin-dependencies-relative
// Missing file / missing keys fall back to the reference defaults.
#ifndef SHARE_CONFIG_H_
#define SHARE_CONFIG_H_

#include <string>

struct ShareConfig {
  bool nv_osc = true;
  bool nv_gpu_accel = true;
  std::string nv_url_relative = "Resources/osc/index.html";
  std::string nv_node_app;             // recorded, engine lane owns NvNode
  std::string nv_plugin_folder_relative;
  std::string nv_plugin_dependencies_relative;
  bool loaded_from_file = false;

  // Reads <exeDir>\NVIDIA Share.json (missing file = all defaults, logged).
  void LoadFromExeDir();
  // One-line summary for the host log.
  std::string Describe() const;
};

#endif  // SHARE_CONFIG_H_
