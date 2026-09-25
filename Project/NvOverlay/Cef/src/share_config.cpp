// share_config.cpp - see share_config.h.
#include "share_config.h"

#include <fstream>
#include <sstream>

#include "share_json.h"
#include "share_proof.h"
#include "share_win.h"

void ShareConfig::LoadFromExeDir() {
  std::wstring path = sharewin::GetExeDir() + L"\\NVIDIA Share.json";
  std::ifstream f(path.c_str(), std::ios::binary);
  if (!f.is_open()) {
    shareproof::LogLine("NVIDIA Share.json not found - using reference defaults");
    return;
  }
  std::ostringstream buf;
  buf << f.rdbuf();
  sharejson::JVal root;
  if (!sharejson::Parse(buf.str(), &root) || root.type != sharejson::JVal::kObj) {
    shareproof::LogLine("NVIDIA Share.json unparsable - using reference defaults");
    return;
  }
  loaded_from_file = true;
  nv_osc = sharejson::GetBool(root, "nv-osc");
  nv_gpu_accel = sharejson::GetBool(root, "nv-gpu-accel");
  nv_url_relative = sharejson::GetStr(root, "nv-url-relative");
  nv_node_app = sharejson::GetStr(root, "nv-node-app");
  nv_plugin_folder_relative =
      sharejson::GetStr(root, "nv-plugin-folder-relative");
  nv_plugin_dependencies_relative =
      sharejson::GetStr(root, "nv-plugin-dependencies-relative");
}

std::string ShareConfig::Describe() const {
  std::ostringstream o;
  o << "nv-osc=" << (nv_osc ? "true" : "false")
    << " nv-gpu-accel=" << (nv_gpu_accel ? "true" : "false")
    << " nv-url-relative=" << nv_url_relative;
  return o.str();
}
