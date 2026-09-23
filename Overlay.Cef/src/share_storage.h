// share_storage.h - backing store for the cefQuery QUERY_READ/WRITE_
// SHARED_STORAGE pair. Contract parity with Overlay.Engine's
// SharedStorageStore.vb: JSON file under the product Data\ folder
// (Data\osc-shared-storage.json), missing key reads "", corrupt content
// reads as empty, every access re-loads under a lock.
#ifndef SHARE_STORAGE_H_
#define SHARE_STORAGE_H_

#include <string>
#include <windows.h>

class ShareStorage {
 public:
  explicit ShareStorage(const std::wstring& file_path);

  std::string Read(const std::string& key);
  void Write(const std::string& key, const std::string& value);

 private:
  std::wstring path_;
  CRITICAL_SECTION lock_;
};

#endif  // SHARE_STORAGE_H_
