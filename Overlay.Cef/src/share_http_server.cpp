// share_http_server.cpp - see share_http_server.h.
#include "share_http_server.h"

#include <stdio.h>

#include <sstream>
#include <vector>

#include "share_proof.h"
#include "share_win.h"

namespace {

struct WsaInit {
  WsaInit() {
    WSADATA data;
    WSAStartup(MAKEWORD(2, 2), &data);
  }
  ~WsaInit() { WSACleanup(); }
};

const char* MimeFor(const std::string& path_lower) {
  size_t dot = path_lower.find_last_of('.');
  if (dot == std::string::npos) return "application/octet-stream";
  const std::string ext = path_lower.substr(dot);
  if (ext == ".html" || ext == ".htm") return "text/html; charset=utf-8";
  if (ext == ".js") return "application/javascript; charset=utf-8";
  if (ext == ".json" || ext == ".map") return "application/json";
  if (ext == ".css") return "text/css; charset=utf-8";
  if (ext == ".png") return "image/png";
  if (ext == ".jpg" || ext == ".jpeg") return "image/jpeg";
  if (ext == ".gif") return "image/gif";
  if (ext == ".webp") return "image/webp";
  if (ext == ".svg") return "image/svg+xml";
  if (ext == ".woff") return "font/woff";
  if (ext == ".woff2") return "font/woff2";
  if (ext == ".ttf") return "font/ttf";
  if (ext == ".otf") return "font/otf";
  if (ext == ".ico") return "image/x-icon";
  if (ext == ".txt") return "text/plain; charset=utf-8";
  if (ext == ".mp3") return "audio/mpeg";
  if (ext == ".mp4") return "video/mp4";
  if (ext == ".webm") return "video/webm";
  return "application/octet-stream";
}

int StatusReason(int code, std::string* reason) {
  switch (code) {
    case 200: *reason = "OK"; return 200;
    case 400: *reason = "Bad Request"; return 400;
    case 404: *reason = "Not Found"; return 404;
    case 405: *reason = "Method Not Allowed"; return 405;
    default: *reason = "Service Unavailable"; return 503;
  }
}

const std::string& kNotFoundBody() {
  static const std::string* body = new std::string("not found");
  return *body;
}

// URL-decodes %xx (+ left as-is: paths don't use form encoding).
std::string UrlDecode(const std::string& in) {
  std::string out;
  out.reserve(in.size());
  for (size_t i = 0; i < in.size(); ++i) {
    if (in[i] == '%' && i + 2 < in.size()) {
      int hi = in[i + 1], lo = in[i + 2];
      auto hv = [](int c) -> int {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        return -1;
      };
      int h = hv(hi), l = hv(lo);
      if (h >= 0 && l >= 0) {
        out.push_back(static_cast<char>((h << 4) | l));
        i += 2;
        continue;
      }
    }
    out.push_back(in[i]);
  }
  return out;
}

void SendAll(SOCKET s, const char* data, int len) {
  int sent = 0;
  while (sent < len) {
    int n = send(s, data + sent, len - sent, 0);
    if (n <= 0) return;
    sent += n;
  }
}

void SendResponse(SOCKET s, int code, const std::string& content_type,
                  const std::string* body_text, const wchar_t* file_path) {
  std::string reason;
  StatusReason(code, &reason);
  std::ostringstream head;
  head << "HTTP/1.1 " << code << " " << reason << "\r\n"
       << "Content-Type: " << content_type << "\r\n"
       << "Connection: close\r\n"
       << "Cache-Control: no-cache\r\n";
  if (body_text) {
    head << "Content-Length: " << body_text->size() << "\r\n\r\n";
    std::string h = head.str();
    SendAll(s, h.data(), (int)h.size());
    SendAll(s, body_text->data(), (int)body_text->size());
  } else {
    HANDLE f = CreateFileW(file_path, GENERIC_READ, FILE_SHARE_READ, NULL,
                           OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (f == INVALID_HANDLE_VALUE) {
      std::string body = "{\"error\":\"file_open_failed\"}";
      head << "Content-Length: " << body.size() << "\r\n\r\n";
      std::string h = head.str();
      SendAll(s, h.data(), (int)h.size());
      SendAll(s, body.data(), (int)body.size());
      return;
    }
    LARGE_INTEGER sz;
    GetFileSizeEx(f, &sz);
    head << "Content-Length: " << sz.QuadPart << "\r\n\r\n";
    std::string h = head.str();
    SendAll(s, h.data(), (int)h.size());
    char buf[65536];
    DWORD read = 0;
    LARGE_INTEGER off;
    off.QuadPart = 0;
    SetFilePointer(f, 0, NULL, FILE_BEGIN);
    while (ReadFile(f, buf, sizeof(buf), &read, NULL) && read > 0) {
      SendAll(s, buf, (int)read);
    }
    CloseHandle(f);
  }
}

}  // namespace

OscHttpServer::OscHttpServer() : port_(0), stop_(false) {}

OscHttpServer::~OscHttpServer() { Stop(); }

bool OscHttpServer::Start(const std::string& osc_root, std::string* err) {
  static WsaInit wsa_init;  // process-wide, main thread before threads spawn
  osc_root_ = osc_root;

  listen_sock_ = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
  if (listen_sock_ == INVALID_SOCKET) {
    *err = "socket() failed: " + std::to_string(WSAGetLastError());
    return false;
  }
  BOOL reuse = TRUE;
  setsockopt(listen_sock_, SOL_SOCKET, SO_REUSEADDR, (const char*)&reuse,
             sizeof(reuse));
  sockaddr_in addr;
  memset(&addr, 0, sizeof(addr));
  addr.sin_family = AF_INET;
  addr.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
  addr.sin_port = 0;  // ephemeral, mirrors OscControllerServer.FindFreePort
  if (bind(listen_sock_, (sockaddr*)&addr, sizeof(addr)) == SOCKET_ERROR) {
    *err = "bind() failed: " + std::to_string(WSAGetLastError());
    return false;
  }
  if (listen(listen_sock_, 64) == SOCKET_ERROR) {
    *err = "listen() failed: " + std::to_string(WSAGetLastError());
    return false;
  }
  sockaddr_in bound;
  int blen = sizeof(bound);
  getsockname(listen_sock_, (sockaddr*)&bound, &blen);
  port_.store(ntohs(bound.sin_port));

  stop_.store(false);
  accept_thread_ = std::thread(&OscHttpServer::AcceptLoop, this);
  return true;
}

void OscHttpServer::AcceptLoop() {
  while (!stop_.load()) {
    SOCKET c = accept(listen_sock_, NULL, NULL);
    if (c == INVALID_SOCKET) {
      if (stop_.load()) break;
      continue;
    }
    // One thread per connection; connections are short-lived and local.
    std::thread(&OscHttpServer::HandleConnection, this, c).detach();
  }
}

void OscHttpServer::HandleConnection(SOCKET s) {
  std::string request;
  char buf[8192];
  // Read until the header block ends (requests carry no bodies).
  for (;;) {
    int n = recv(s, buf, sizeof(buf), 0);
    if (n <= 0) break;
    request.append(buf, n);
    if (request.size() > 32 * 1024) break;
    if (request.find("\r\n\r\n") != std::string::npos) break;
  }
  size_t line_end = request.find("\r\n");
  std::string line =
      line_end == std::string::npos ? request : request.substr(0, line_end);
  std::string method, target;
  {
    size_t sp1 = line.find(' ');
    size_t sp2 = sp1 == std::string::npos ? std::string::npos
                                          : line.find(' ', sp1 + 1);
    if (sp1 != std::string::npos && sp2 != std::string::npos) {
      method = line.substr(0, sp1);
      target = line.substr(sp1 + 1, sp2 - sp1 - 1);
    }
  }

  if (method != "GET") {
    std::string body = "method not allowed";
    SendResponse(s, 405, "text/plain", &body, NULL);
    closesocket(s);
    return;
  }

  std::string path = target;
  size_t q = path.find('?');
  if (q != std::string::npos) path = path.substr(0, q);
  path = UrlDecode(path);
  if (path.empty() || path[0] != '/' || path.find("..") != std::string::npos ||
      path.find('\\') != std::string::npos || path.find('\0') != std::string::npos) {
    shareproof::LogLine("http rejected path=" + path);
    std::string body = "bad path";
    SendResponse(s, 400, "text/plain", &body, NULL);
    closesocket(s);
    return;
  }
  if (path == "/") path = "/index.html";

  // Rooted, separator-normalized path under the osc root.
  std::string rel = path.substr(1);
  for (size_t i = 0; i < rel.size(); ++i) {
    if (rel[i] == '/') rel[i] = '\\';
  }
  std::string root_lower = osc_root_, rel_lower = rel;
  for (size_t i = 0; i < root_lower.size(); ++i)
    if (root_lower[i] == '/') root_lower[i] = '\\';
  std::string full = root_lower + (root_lower[root_lower.size() - 1] == '\\'
                                       ? "" : "\\") + rel;
  std::string full_lower = full;
  for (size_t i = 0; i < full_lower.size(); ++i) {
    char& c = full_lower[i];
    if (c >= 'A' && c <= 'Z') c += 32;
  }
  for (size_t i = 0; i < root_lower.size(); ++i) {
    char& c = root_lower[i];
    if (c >= 'A' && c <= 'Z') c += 32;
  }

  std::string seg = rel_lower;
  size_t bslash = seg.find_last_of('\\');
  if (bslash != std::string::npos) seg = seg.substr(bslash + 1);
  bool looks_like_file = seg.find('.') != std::string::npos;

  if (looks_like_file) {
    std::wstring wide = sharewin::Utf8ToWide(full);
    DWORD attr = GetFileAttributesW(wide.c_str());
    if (attr != INVALID_FILE_ATTRIBUTES && !(attr & FILE_ATTRIBUTE_DIRECTORY)) {
      SendResponse(s, 200, MimeFor(full_lower), NULL, wide.c_str());
      shareproof::LogLine("http GET " + path + " -> 200");
      closesocket(s);
      return;
    }
    SendResponse(s, 404, "text/plain", &kNotFoundBody(), NULL);
    shareproof::LogLine("http GET " + path + " -> 404");
    closesocket(s);
    return;
  }

  // Extensionless (REST / socket.io / backend proxy paths): the CEF host
  // serves the OSC bundle only; backend proxying stays with the engine's
  // OscControllerServer. Deterministic 503 keeps services degraded, not
  // hung (parity with engine behavior when the Web Helper is absent).
  std::string body = "{\"error\":\"backend_unavailable_in_cef_host\"}";
  SendResponse(s, 503, "application/json", &body, NULL);
  shareproof::LogLine("http GET " + path + " -> 503 (backend path)");
  closesocket(s);
}

void OscHttpServer::Stop() {
  bool expected = false;
  if (stop_.compare_exchange_strong(expected, true)) {
    if (listen_sock_ != INVALID_SOCKET) {
      closesocket(listen_sock_);
      listen_sock_ = INVALID_SOCKET;
    }
    if (accept_thread_.joinable()) accept_thread_.join();
  }
}
