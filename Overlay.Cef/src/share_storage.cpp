// share_storage.cpp - see share_storage.h.
#include "share_storage.h"

#include <fstream>
#include <map>
#include <sstream>

#include "share_json.h"
#include "share_proof.h"
#include "share_win.h"

ShareStorage::ShareStorage(const std::wstring& file_path) : path_(file_path) {
  InitializeCriticalSectionAndSpinCount(&lock_, 0x400);
}

std::map<std::string, std::string> LoadMap(const std::wstring& path) {
  std::map<std::string, std::string> dict;
  std::ifstream f(path.c_str(), std::ios::binary);
  if (!f.is_open()) return dict;
  std::ostringstream buf;
  buf << f.rdbuf();
  const std::string text = buf.str();
  if (text.empty()) return dict;
  sharejson::JVal root;
  if (!sharejson::Parse(text, &root) || root.type != sharejson::JVal::kObj) {
    return dict;  // corrupt store reads as empty, next write rebuilds it
  }
  for (size_t i = 0; i < root.obj.size(); ++i) {
    if (root.obj[i].second.type == sharejson::JVal::kStr) {
      dict[root.obj[i].first] = root.obj[i].second.str;
    }
  }
  return dict;
}

void SaveMap(const std::wstring& path,
             const std::map<std::string, std::string>& dict) {
  std::string json = "{";
  bool first = true;
  for (std::map<std::string, std::string>::const_iterator it = dict.begin();
       it != dict.end(); ++it) {
    if (!first) json += ",";
    first = false;
    json += sharejson::Escape(it->first);
    json += ":";
    json += sharejson::Escape(it->second);
  }
  json += "}";
  std::ofstream f(path.c_str(), std::ios::binary | std::ios::trunc);
  if (f.is_open()) {
    f << json;
    f.flush();
  }
}

std::string ShareStorage::Read(const std::string& key) {
  if (key.empty()) return "";
  EnterCriticalSection(&lock_);
  std::map<std::string, std::string> dict = LoadMap(path_);
  LeaveCriticalSection(&lock_);
  std::map<std::string, std::string>::const_iterator it = dict.find(key);
  if (it == dict.end()) return "";
  return it->second;
}

void ShareStorage::Write(const std::string& key, const std::string& value) {
  if (key.empty()) return;
  EnterCriticalSection(&lock_);
  std::map<std::string, std::string> dict = LoadMap(path_);
  dict[key] = value;
  SaveMap(path_, dict);
  LeaveCriticalSection(&lock_);
  shareproof::LogLine("shared-storage write key=" + key);
}
