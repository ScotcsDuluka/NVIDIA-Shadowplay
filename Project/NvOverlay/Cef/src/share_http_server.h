// share_http_server.h - loopback static server for the OSC web UI.
// Mirrors the proven production model (OscControllerServer.vb):
// 127.0.0.1 ephemeral port, serves the osc bundle, extensionless/REST
// paths get a deterministic 503 so page services degrade cleanly instead
// of hanging (backend proxying stays engine-lane territory).
#ifndef SHARE_HTTP_SERVER_H_
#define SHARE_HTTP_SERVER_H_

#include <atomic>
#include <string>
#include <thread>
#include <winsock2.h>
#include <ws2tcpip.h>

class OscHttpServer {
 public:
  OscHttpServer();
  ~OscHttpServer();

  // Binds 127.0.0.1:0, starts the accept loop. Returns false with |err|
  // filled on failure. |osc_root| must be an absolute UTF-8 path.
  bool Start(const std::string& osc_root, std::string* err);

  int port() const { return port_.load(); }
  const std::string& osc_root() const { return osc_root_; }

  void Stop();

 private:
  void AcceptLoop();
  void HandleConnection(SOCKET s);

  std::string osc_root_;
  SOCKET listen_sock_ = INVALID_SOCKET;
  std::thread accept_thread_;
  std::atomic<int> port_;
  std::atomic<bool> stop_;
};

#endif  // SHARE_HTTP_SERVER_H_
