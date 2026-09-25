// launcher_json.h - minimal JSON parse/serialize for the Launcher CEF lane.
// Parser side is a 1:1 copy of the proven Overlay.Cef share_json.h
// (order-preserving objects — the property config.json read-modify-write
// depends on). The serializer is the lane addition: config.json is
// rewritten by LAUNCHER_SET_* and every OTHER key owned by other processes
// must survive byte-for-byte in VALUE (formatting normalizes to 2-space
// indent, the System.Text.Json WriteIndented shape AppConfigShared.vb
// writes).
//
// Number rendering: integral doubles print without a decimal point
// (20 stays 20, never 20.0); non-integral doubles print with %.17g so
// every double System.Text.Json wrote round-trips exactly.
#ifndef LAUNCHER_JSON_H_
#define LAUNCHER_JSON_H_

#include <math.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <utility>
#include <vector>

namespace launcherjson {

struct JVal {
  enum Type { kNull, kBool, kNum, kStr, kArr, kObj };
  Type type = kNull;
  bool b = false;
  double num = 0.0;
  std::string str;
  std::vector<JVal> arr;
  std::vector<std::pair<std::string, JVal> > obj;

  const JVal* Find(const char* key) const {
    if (type != kObj) return 0;
    for (size_t i = 0; i < obj.size(); ++i) {
      if (obj[i].first == key) return &obj[i].second;
    }
    return 0;
  }
  // Case-insensitive member lookup — AppConfigShared.vb contract
  // (hand-edited or re-cased config.json files must still resolve).
  const JVal* FindNoCase(const char* key) const {
    if (type != kObj) return 0;
    for (size_t i = 0; i < obj.size(); ++i) {
      if (IEquals(obj[i].first, key)) return &obj[i].second;
    }
    return 0;
  }
  std::string AsString(const char* def = "") const {
    return (type == kStr) ? str : def;
  }
  bool AsBool() const { return (type == kBool) ? b : false; }
  int AsInt() const {
    if (type != kNum) return 0;
    return static_cast<int>(num);
  }

  static bool IEquals(const std::string& a, const char* b) {
    size_t bl = 0;
    while (b[bl]) ++bl;
    if (a.size() != bl) return false;
    for (size_t i = 0; i < bl; ++i) {
      char x = a[i], y = b[i];
      if (x >= 'A' && x <= 'Z') x += 32;
      if (y >= 'A' && y <= 'Z') y += 32;
      if (x != y) return false;
    }
    return true;
  }
};

// UTF-8 decode of a \uXXXX escape (with surrogate pairs) appended to |out|.
inline void AppendCodepoint(unsigned int cp, std::string* out) {
  if (cp < 0x80) {
    out->push_back(static_cast<char>(cp));
  } else if (cp < 0x800) {
    out->push_back(static_cast<char>(0xC0 | (cp >> 6)));
    out->push_back(static_cast<char>(0x80 | (cp & 0x3F)));
  } else if (cp < 0x10000) {
    out->push_back(static_cast<char>(0xE0 | (cp >> 12)));
    out->push_back(static_cast<char>(0x80 | ((cp >> 6) & 0x3F)));
    out->push_back(static_cast<char>(0x80 | (cp & 0x3F)));
  } else {
    out->push_back(static_cast<char>(0xF0 | (cp >> 18)));
    out->push_back(static_cast<char>(0x80 | ((cp >> 12) & 0x3F)));
    out->push_back(static_cast<char>(0x80 | ((cp >> 6) & 0x3F)));
    out->push_back(static_cast<char>(0x80 | (cp & 0x3F)));
  }
}

inline int ParseHex4(const char* s) {
  int v = 0;
  for (int i = 0; i < 4; ++i) {
    char c = s[i];
    v <<= 4;
    if (c >= '0' && c <= '9') v += c - '0';
    else if (c >= 'a' && c <= 'f') v += c - 'a' + 10;
    else if (c >= 'A' && c <= 'Z') v += c - 'A' + 10;
    else return -1;
  }
  return v;
}

class Parser {
 public:
  explicit Parser(const std::string& in) : s_(in), i_(0) {}
  bool Parse(JVal* out) {
    SkipWs();
    if (!ParseValue(out)) return false;
    SkipWs();
    return i_ >= s_.size();
  }

 private:
  void SkipWs() {
    while (i_ < s_.size()) {
      char c = s_[i_];
      if (c == ' ' || c == '\t' || c == '\r' || c == '\n') ++i_;
      else break;
    }
  }
  bool ParseValue(JVal* out) {
    if (i_ >= s_.size()) return false;
    char c = s_[i_];
    if (c == '{') return ParseObj(out);
    if (c == '[') return ParseArr(out);
    if (c == '"') { out->type = JVal::kStr; return ParseStr(&out->str); }
    if (c == 't') { return Lit("true") && (out->type = JVal::kBool, out->b = true, true); }
    if (c == 'f') { return Lit("false") && (out->type = JVal::kBool, out->b = false, true); }
    if (c == 'n') { return Lit("null") && (out->type = JVal::kNull, true); }
    return ParseNum(out);
  }
  bool Lit(const char* lit) {
    size_t n = 0;
    while (lit[n]) ++n;
    if (s_.compare(i_, n, lit) != 0) return false;
    i_ += n;
    return true;
  }
  bool ParseNum(JVal* out) {
    size_t start = i_;
    if (i_ < s_.size() && (s_[i_] == '-' || s_[i_] == '+')) ++i_;
    bool digits = false;
    while (i_ < s_.size() && s_[i_] >= '0' && s_[i_] <= '9') { ++i_; digits = true; }
    if (i_ < s_.size() && s_[i_] == '.') {
      ++i_;
      while (i_ < s_.size() && s_[i_] >= '0' && s_[i_] <= '9') { ++i_; digits = true; }
    }
    if (i_ < s_.size() && (s_[i_] == 'e' || s_[i_] == 'E')) {
      ++i_;
      if (i_ < s_.size() && s_[i_] == '-' || s_[i_] == '+') ++i_;
      while (i_ < s_.size() && s_[i_] >= '0' && s_[i_] <= '9') ++i_;
    }
    if (!digits) return false;
    out->type = JVal::kNum;
    out->num = strtod(s_.c_str() + start, 0);
    return true;
  }
  bool ParseStr(std::string* out) {
    ++i_;  // opening quote
    while (i_ < s_.size()) {
      char c = s_[i_];
      if (c == '"') { ++i_; return true; }
      if (c == '\\') {
        ++i_;
        if (i_ >= s_.size()) return false;
        char e = s_[i_++];
        switch (e) {
          case '"': out->push_back('"'); break;
          case '\\': out->push_back('\\'); break;
          case '/': out->push_back('/'); break;
          case 'b': out->push_back('\b'); break;
          case 'f': out->push_back('\f'); break;
          case 'n': out->push_back('\n'); break;
          case 'r': out->push_back('\r'); break;
          case 't': out->push_back('\t'); break;
          case 'u': {
            if (i_ + 4 > s_.size()) return false;
            int cp = ParseHex4(s_.c_str() + i_);
            if (cp < 0) return false;
            i_ += 4;
            if (cp >= 0xD800 && cp <= 0xDBFF && i_ + 6 <= s_.size() &&
                s_[i_] == '\\' && s_[i_ + 1] == 'u') {
              int lo = ParseHex4(s_.c_str() + i_ + 2);
              if (lo >= 0xDC00 && lo <= 0xDFFF) {
                cp = 0x10000 + ((cp - 0xD800) << 10) + (lo - 0xDC00);
                i_ += 6;
              }
            }
            AppendCodepoint(static_cast<unsigned int>(cp), out);
            break;
          }
          default: return false;
        }
      } else {
        out->push_back(c);
        ++i_;
      }
    }
    return false;  // unterminated
  }
  bool ParseObj(JVal* out) {
    ++i_;
    out->type = JVal::kObj;
    SkipWs();
    if (i_ < s_.size() && s_[i_] == '}') { ++i_; return true; }
    for (;;) {
      SkipWs();
      if (i_ >= s_.size() || s_[i_] != '"') return false;
      std::string key;
      if (!ParseStr(&key)) return false;
      SkipWs();
      if (i_ >= s_.size() || s_[i_] != ':') return false;
      ++i_;
      SkipWs();
      JVal val;
      if (!ParseValue(&val)) return false;
      out->obj.push_back(std::make_pair(key, val));
      SkipWs();
      if (i_ < s_.size() && s_[i_] == ',') { ++i_; continue; }
      if (i_ < s_.size() && s_[i_] == '}') { ++i_; return true; }
      return false;
    }
  }
  bool ParseArr(JVal* out) {
    ++i_;
    out->type = JVal::kArr;
    SkipWs();
    if (i_ < s_.size() && s_[i_] == ']') { ++i_; return true; }
    for (;;) {
      SkipWs();
      JVal val;
      if (!ParseValue(&val)) return false;
      out->arr.push_back(val);
      SkipWs();
      if (i_ < s_.size() && s_[i_] == ',') { ++i_; continue; }
      if (i_ < s_.size() && s_[i_] == ']') { ++i_; return true; }
      return false;
    }
  }
  const std::string& s_;
  size_t i_;
};

inline bool Parse(const std::string& in, JVal* out) {
  return Parser(in).Parse(out);
}

inline void EscapeInto(const std::string& in, std::string* out) {
  out->push_back('"');
  for (size_t i = 0; i < in.size(); ++i) {
    unsigned char c = static_cast<unsigned char>(in[i]);
    switch (c) {
      case '"': out->append("\\\""); break;
      case '\\': out->append("\\\\"); break;
      case '\b': out->append("\\b"); break;
      case '\f': out->append("\\f"); break;
      case '\n': out->append("\\n"); break;
      case '\r': out->append("\\r"); break;
      case '\t': out->append("\\t"); break;
      default:
        if (c < 0x20) {
          char buf[8];
          sprintf(buf, "\\u%04x", c);
          out->append(buf);
        } else {
          out->push_back(static_cast<char>(c));
        }
    }
  }
  out->push_back('"');
}

inline std::string Escape(const std::string& in) {
  std::string out;
  EscapeInto(in, &out);
  return out;
}

inline void SerializeNum(double num, std::string* out) {
  // Integral values in the JS-safe range print without a decimal point so
  // System.Text.Json-written integers (bitrates, counters) stay integers.
  double integral;
  if (modf(num, &integral) == 0.0 &&
      num >= -9007199254740992.0 && num <= 9007199254740992.0) {
    char buf[32];
    sprintf(buf, "%lld", (long long)num);
    out->append(buf);
    return;
  }
  char buf[40];
  sprintf(buf, "%.17g", num);
  out->append(buf);
}

inline void SerializeValue(const JVal& v, int indent, std::string* out) {
  std::string pad(indent * 2, ' ');
  std::string pad_in((indent + 1) * 2, ' ');
  switch (v.type) {
    case JVal::kNull: out->append("null"); break;
    case JVal::kBool: out->append(v.b ? "true" : "false"); break;
    case JVal::kNum: SerializeNum(v.num, out); break;
    case JVal::kStr: EscapeInto(v.str, out); break;
    case JVal::kArr: {
      if (v.arr.empty()) { out->append("[]"); break; }
      out->append("[\n");
      for (size_t i = 0; i < v.arr.size(); ++i) {
        out->append(pad_in);
        SerializeValue(v.arr[i], indent + 1, out);
        if (i + 1 < v.arr.size()) out->push_back(',');
        out->push_back('\n');
      }
      out->append(pad);
      out->push_back(']');
      break;
    }
    case JVal::kObj: {
      if (v.obj.empty()) { out->append("{}"); break; }
      out->append("{\n");
      for (size_t i = 0; i < v.obj.size(); ++i) {
        out->append(pad_in);
        EscapeInto(v.obj[i].first, out);
        out->append(": ");
        SerializeValue(v.obj[i].second, indent + 1, out);
        if (i + 1 < v.obj.size()) out->push_back(',');
        out->push_back('\n');
      }
      out->append(pad);
      out->push_back('}');
      break;
    }
  }
}

inline std::string Serialize(const JVal& v) {
  std::string out;
  SerializeValue(v, 0, &out);
  return out;
}

// Case-insensitive object member access with default (mirrors the
// GetStr/GetBool/GetInt helpers of CefQueryBridge.vb: absent = ""/false/0).
inline std::string GetStr(const JVal& v, const char* key) {
  const JVal* f = v.Find(key);
  return f ? f->AsString("") : std::string();
}
inline bool GetBool(const JVal& v, const char* key) {
  const JVal* f = v.Find(key);
  return f ? f->AsBool() : false;
}
inline int GetInt(const JVal& v, const char* key) {
  const JVal* f = v.Find(key);
  return f ? f->AsInt() : 0;
}

// Read-modify-write helper for one boolean config key. Returns false when
// the root/section had to be created in a tree that cannot hold it
// (root not an object). Mutation is case-insensitive on the section and
// key names so the existing spelling in the file always wins.
inline bool SetBoolInObject(JVal* root, const char* section, const char* key,
                            bool value) {
  if (root->type == JVal::kNull) {
    root->type = JVal::kObj;
  }
  if (root->type != JVal::kObj) return false;
  JVal* sec = 0;
  for (size_t i = 0; i < root->obj.size(); ++i) {
    if (JVal::IEquals(root->obj[i].first, section)) {
      sec = &root->obj[i].second;
      break;
    }
  }
  if (!sec) {
    root->obj.push_back(std::make_pair(std::string(section), JVal()));
    sec = &root->obj.back().second;
  }
  if (sec->type == JVal::kNull) sec->type = JVal::kObj;
  if (sec->type != JVal::kObj) return false;
  for (size_t i = 0; i < sec->obj.size(); ++i) {
    if (JVal::IEquals(sec->obj[i].first, key)) {
      sec->obj[i].second.type = JVal::kBool;
      sec->obj[i].second.b = value;
      return true;
    }
  }
  JVal v;
  v.type = JVal::kBool;
  v.b = value;
  sec->obj.push_back(std::make_pair(std::string(key), v));
  return true;
}

}  // namespace launcherjson

#endif  // LAUNCHER_JSON_H_
