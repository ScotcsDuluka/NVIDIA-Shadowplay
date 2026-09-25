// share_query_handler.cpp - port of Overlay.Engine\CefQueryBridge.vb
// Dispatch onto the native CEF message router. Every command name, request
// field and response string is copied verbatim from the proven VB bridge
// (see Overlay.Cef\PROTOCOL-PARITY.md for the line-by-line mapping).
// Lane-only additions are namespaced __PROOF_* so they cannot collide with
// the production QUERY_* command namespace.
#include "share_query_handler.h"

#include <stdio.h>

#include <sstream>

#include "include/cef_browser.h"
#include "include/cef_frame.h"

#include "share_json.h"
#include "share_proof.h"
#include "share_storage.h"
#include "share_win.h"

namespace {

std::string CefToStd(const CefString& s) { return s.ToString(); }

// Response builders — exact parity with CefQueryBridge.OkResponse /
// FailResponse. On the native router the RESPONSE STRING itself is the
// payload (no {__cefResponse} envelope — that envelope exists only on the
// WebView2 polyfill wire, CefQueryBridge.vb:11-12).
void RespondOk(CefRefPtr<CefMessageRouterBrowserSide::Callback> callback,
               const std::string& response) {
  callback->Success(response);
}

void RespondFail(CefRefPtr<CefMessageRouterBrowserSide::Callback> callback,
                 int code, const std::string& message) {
  callback->Failure(code, message);
}

// Port of OscProtocol.ParseDisplayRects: entries are [x,y,width,height]
// arrays or {x, y, width|w, height|h} objects; malformed entries skipped.
void ParseDisplayRects(const sharejson::JVal& arr, std::vector<RECT>* out) {
  if (arr.type != sharejson::JVal::kArr) return;
  for (size_t i = 0; i < arr.arr.size(); ++i) {
    const sharejson::JVal& el = arr.arr[i];
    RECT r = {0, 0, 0, 0};
    if (el.type == sharejson::JVal::kArr && el.arr.size() >= 4) {
      r.left = el.arr[0].AsInt();
      r.top = el.arr[1].AsInt();
      r.right = r.left + el.arr[2].AsInt();
      r.bottom = r.top + el.arr[3].AsInt();
    } else if (el.type == sharejson::JVal::kObj) {
      r.left = sharejson::GetInt(el, "x");
      r.top = sharejson::GetInt(el, "y");
      const sharejson::JVal* w = el.Find("width");
      if (!w) w = el.Find("w");
      const sharejson::JVal* h = el.Find("height");
      if (!h) h = el.Find("h");
      r.right = r.left + (w ? w->AsInt() : 0);
      r.bottom = r.top + (h ? h->AsInt() : 0);
    } else {
      continue;  // skip malformed entry
    }
    out->push_back(r);
  }
}

std::string RectsSummary(const std::vector<RECT>& rects) {
  std::ostringstream o;
  o << rects.size() << " rect(s)";
  for (size_t i = 0; i < rects.size() && i < 4; ++i) {
    char buf[96];
    sprintf(buf, " [%ld,%ld %ldx%ld]", rects[i].left, rects[i].top,
            rects[i].right - rects[i].left, rects[i].bottom - rects[i].top);
    o << buf;
  }
  return o.str();
}

}  // namespace

ShareQueryHandler::ShareQueryHandler(int http_port, int backend_port,
                                     const std::string& secret,
                                     ShareStorage* storage, HWND host_wnd,
                                     bool show_window)
    : http_port_(http_port),
      backend_port_(backend_port),
      secret_(secret),
      storage_(storage),
      host_wnd_(host_wnd),
      show_window_(show_window),
      close_query_id_(-1),
      painting_enabled_(false),
      organic_recorded_(false),
      proof_done_cb_(NULL) {}

ShareQueryHandler::~ShareQueryHandler() {}

bool ShareQueryHandler::OnQuery(CefRefPtr<CefBrowser> browser,
                                CefRefPtr<CefFrame> frame, int64 query_id,
                                const CefString& request, bool persistent,
                                CefRefPtr<Callback> callback) {
  const std::string request_str = CefToStd(request);

  // Malformed-request ladder — exact parity with
  // CefQueryBridge.HandleWebMessage (CefQueryBridge.vb:189-199).
  sharejson::JVal req;
  if (!sharejson::Parse(request_str, &req)) {
    RespondFail(callback, -3, "bad request json");
    return true;
  }
  if (req.type != sharejson::JVal::kObj) {
    RespondFail(callback, -3, "request not object");
    return true;
  }
  const std::string cmd = sharejson::GetStr(req, "command");
  if (cmd.empty()) {
    RespondFail(callback, -3, "no command");
    return true;
  }

  // Main frame reference for host-side display-state flips (UI thread).
  if (browser.get() && browser->GetMainFrame()) {
    browser_for_flip_ = browser;
  }

  std::string response;   // success payload ("" registered per command)
  bool fail = false;
  int fail_code = -1;
  std::string fail_msg;
  std::string log_extra;

  if (cmd == "QUERY_WIN_NODE_INFO") {
    // Boot-critical handshake: controller server port + auth secret
    // (CefQueryBridge.vb:209-213). In NvBackend origin mode the reported
    // port is the BACKEND port — the page builds all backend URLs
    // against it, and the backend serves the osc frontend same-origin.
    const int port = backend_port_ > 0 ? backend_port_ : http_port_;
    std::ostringstream o;
    o << "{\"port\":" << port << ",\"secret\":\"" << secret_ << "\"}";
    response = o.str();
    log_extra = "boot handshake (port=" + std::to_string(port) +
                (backend_port_ > 0 ? ", backend mode" : ", standalone") + ")";
  } else if (cmd == "QUERY_FULLSCREEN_STATE") {
    // ALWAYS answer desktop (fullscreen=false) — the payload is a
    // constant in the proven bridge (CefQueryBridge.vb:215-225); the
    // probe is kept for the log only.
    std::string probe;
    sharewin::IsFullscreenActive(&probe);
    response = "{\"fullscreen\":false,\"hdractive\":false,\"borderlessMode\":null}";
    log_extra = "fullscreen probe: " + probe + " -> forced false";
  } else if (cmd == "QUERY_OSC_DISPLAY_IS_DESKTOP_MODE") {
    // "true" renders the FULL menu instead of the in-game compact
    // sidebar (CefQueryBridge.vb:227-232).
    response = "true";
  } else if (cmd == "QUERY_OSC_SET_DISPLAY_RECTS") {
    const sharejson::JVal* rects = req.Find("displayRects");
    std::vector<RECT> parsed;
    if (rects) ParseDisplayRects(*rects, &parsed);
    display_rects_ = parsed;
    log_extra = "displayRects=" + RectsSummary(display_rects_);
    response = "true";
  } else if (cmd == "QUERY_OSC_SET_PAINTING") {
    painting_enabled_ = sharejson::GetBool(req, "enablePainting");
    log_extra = std::string("painting=") + (painting_enabled_ ? "true" : "false");
    response = "true";
  } else if (cmd == "QUERY_OSC_SET_EXPERIMENTAL") {
    response = "true";
  } else if (cmd == "QUERY_OSC_REGISTER_CLOSE_EVENT") {
    if (persistent) {
      // Persistent query: no immediate response body; pushes happen later
      // via PushCloseFromHost (CefQueryBridge.vb:253-257).
      close_query_id_ = query_id;
      close_callback_ = callback;
      log_extra = "close-event channel registered";
      shareproof::LogLine("cefQuery cmd=" + cmd + " persistent=" +
                          (persistent ? "1" : "0") + " -> " + log_extra);
      return true;  // handled; callback stays pending
    }
    response = "true";
  } else if (cmd == "QUERY_WIN_OPEN_OSC") {
    bool enable_input = sharejson::GetBool(req, "enableInput");
    if (host_wnd_ && show_window_) ShowWindow(host_wnd_, SW_SHOW);
    FlipOscDisplayState(true);
    log_extra = std::string("open osc enableInput=") +
                (enable_input ? "true" : "false");
    response = "true";
  } else if (cmd == "QUERY_WIN_CLOSE_OSC") {
    if (host_wnd_) ShowWindow(host_wnd_, SW_HIDE);
    FlipOscDisplayState(false);
    log_extra = "close osc";
    response = "true";
  } else if (cmd == "QUERY_READ_SHARED_STORAGE") {
    response = storage_->Read(sharejson::GetStr(req, "path"));
  } else if (cmd == "QUERY_WRITE_SHARED_STORAGE") {
    storage_->Write(sharejson::GetStr(req, "path"),
                    sharejson::GetStr(req, "data"));
    response = "true";
  } else if (cmd == "QUERY_LOAD_STRING_TABLE") {
    // Page passes {stringTable:{...}}; ack-only (CefQueryBridge.vb:275-277).
    response = "true";
  } else if (cmd == "QUERY_WIN_COPY_TO_CLIPBOARD") {
    const std::string text = sharejson::GetStr(req, "clipBoardData");
    if (!text.empty() && !sharewin::SetClipboardText(host_wnd_, text)) {
      shareproof::LogLine("clipboard: set failed");
    }
    response = "true";
  } else if (cmd == "QUERY_HTTPSERVER_START") {
    // OAuth loopback capture — out of scope, fail fast (CefQueryBridge.vb:290-293).
    fail = true; fail_code = -1; fail_msg = "oauth_not_implemented";
  } else if (cmd == "QUERY_BROWSE_DIRECTORY") {
    // Parity with the engine host's unwired state (CefQueryBridge.vb:307-309):
    // the native folder picker is a later integration round; failing fast
    // keeps the deterministic no-hang contract.
    log_extra = "browseDirectory name=" + sharejson::GetStr(req, "name") +
                " (picker not wired in CEF lane yet)";
    fail = true; fail_code = -1; fail_msg = "folder_picker_unavailable";
  } else if (cmd == "QUERY_OSC_DROP_URL") {
    // Drag-drop registration ack — response ignored by the page
    // (CefQueryBridge.vb:334-344).
    std::ostringstream o;
    o << "oscDropUrl url=" << sharejson::GetStr(req, "url")
      << " pos=" << sharejson::GetInt(req, "xpos") << ","
      << sharejson::GetInt(req, "ypos");
    log_extra = o.str();
    response = "true";
  } else if (cmd == "QUERY_WIN_KB_MESSAGE") {
    // Gamepad key-message ack — response ignored (CefQueryBridge.vb:346-355).
    std::ostringstream o;
    o << "winKbMessage keycode=" << sharejson::GetInt(req, "keycode")
      << " modifier=" << sharejson::GetInt(req, "keymodifier");
    log_extra = o.str();
    response = "true";
  } else if (cmd == "__PROOF_DOM") {
    // CEF-lane proof observer (not a production command). after-open
    // probes carry their phase inside the probe JSON.
    const std::string probe = sharejson::GetStr(req, "probe");
    if (probe.find("after-open") != std::string::npos) {
      shareproof::SetField("domProbeAfterOpen", probe);
      shareproof::Checkpoint("DOM_PROBE_AFTER_OPEN", probe);
    } else {
      shareproof::SetField("domProbe", probe);
      shareproof::Checkpoint("DOM_PROBE", probe);
    }
    response = "true";
  } else if (cmd == "__PROOF_OPEN_UI") {
    // CEF-lane proof observer: opens the OSC menu through the page's OWN
    // display service (the same openOSC path the engine's Alt+Z uses) AND
    // flips the host-owned oscengine-open class — both halves of the
    // engine host's open path.
    if (browser.get() && browser->GetMainFrame()) {
      browser->GetMainFrame()->ExecuteJavaScript(
          "var t=0;var iv=setInterval(function(){"
          "if(window.__oscOpen){window.__oscOpen();clearInterval(iv);"
          "setTimeout(function(){"
          "var b=document.querySelector('.base');"
          "var p={phase:'after-open',"
          "baseVisibility:(b?getComputedStyle(b).visibility:'no-element'),"
          "baseWidth:(b?Math.round(b.getBoundingClientRect().width):-1)};"
          "window.cefQuery({request:JSON.stringify({command:'__PROOF_DOM',probe:JSON.stringify(p)}),persistent:false,onSuccess:function(){},onFailure:function(e,m){}});"
          "},1500);}"
          "else if(++t>40){clearInterval(iv);}},250);",
          "nvidia-share://proof-open", 0);
      FlipOscDisplayState(true);
    }
    shareproof::Checkpoint("UI_OPEN_TRIGGERED",
                           "openOSC + oscengine-open flip");
    response = "true";
  } else if (cmd == "__PROOF_ECHO") {
    // CEF-lane proof observer: the page's onSuccess already fired (the
    // echo only exists because of it); the echoed value closes the
    // round-trip host->page->host through the native router.
    const std::string value = sharejson::GetStr(req, "value");
    if (value == "true") {
      shareproof::Checkpoint("PROOF_ECHO_VERIFIED", "QUERY_OSC_SET_EXPERIMENTAL round-trip returned \"true\"");
    } else {
      shareproof::Checkpoint("PROOF_ECHO_MISMATCH", "value=" + value);
    }
    response = "ack";
    if (proof_done_cb_) proof_done_cb_();
  } else {
    // Unknown commands FAIL with errorCode -1 so the calling service
    // degrades cleanly instead of hanging (CefQueryBridge.vb:14-17,357-359).
    shareproof::LogLine("cefQuery not implemented: " + cmd);
    fail = true; fail_code = -1; fail_msg = "not_implemented";
  }

  if (!organic_recorded_ && cmd.compare(0, 6, "QUERY_") == 0) {
    organic_recorded_ = true;
    shareproof::Checkpoint("ORGANIC_QUERY", "first page-originated command=" + cmd);
  }

  std::ostringstream log;
  log << "cefQuery cmd=" << cmd << " persistent=" << (persistent ? "1" : "0");
  if (fail) {
    log << " -> fail " << fail_code << " " << fail_msg;
  } else {
    log << " -> ok \"" << response << "\"";
  }
  if (!log_extra.empty()) log << " | " << log_extra;
  shareproof::LogLine(log.str());

  if (fail) {
    RespondFail(callback, fail_code, fail_msg);
  } else {
    RespondOk(callback, response);
  }
  return true;
}

void ShareQueryHandler::OnQueryCanceled(CefRefPtr<CefBrowser> browser,
                                        CefRefPtr<CefFrame> frame,
                                        int64 query_id) {
  if (query_id == close_query_id_) {
    close_query_id_ = -1;
    close_callback_ = NULL;
    shareproof::LogLine("cefQuery close-event channel canceled");
  }
}

void ShareQueryHandler::FlipOscDisplayState(bool open) {
  if (!browser_for_flip_.get()) return;
  std::ostringstream js;
  js << "document.documentElement.classList.remove('oscengine-toast');"
     << "document.documentElement.classList.toggle('oscengine-open',"
     << (open ? "true" : "false") << ");";
  browser_for_flip_->GetMainFrame()->ExecuteJavaScript(
      js.str(), "nvidia-share://display-state", 0);
}

void ShareQueryHandler::PushCloseFromHost() {
  if (close_query_id_ >= 0 && close_callback_.get()) {
    shareproof::LogLine("cefQuery close-event push (RequestCloseFromPage parity)");
    close_callback_->Success("true");
  }
}
