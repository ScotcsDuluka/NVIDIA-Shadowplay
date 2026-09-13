// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 110
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r(e) {
    var t = e && e.forceBase64;
    u && !t || (this.supportsBinary = !1), i.call(this, e);
  }
  var i = require(69),
    o = require(71),
    a = require(23),
    s = require(34),
    c = require(45)("engine.io-client:polling");
  module.exports = r;
  var u = function() {
    var e = require(70),
      t = new e({
        xdomain: !1
      });
    return null != t.responseType;
  }();
  s(r, i), r.prototype.name = "polling", r.prototype.doOpen = function() {
    this.poll();
  }, r.prototype.pause = function(e) {
    function t() {
      c("paused"), n.readyState = "paused", e();
    }
    var n = this;
    if (this.readyState = "pausing", this.polling || !this.writable) {
      var r = 0;
      this.polling && (c("we are currently polling - waiting to pause"), r++, this.once("pollComplete",
        function() {
          c("pre-pause polling complete"), --r || t();
        })), this.writable || (c("we are currently writing - waiting to pause"), r++, this.once("drain",
        function() {
          c("pre-pause writing complete"), --r || t();
        }));
    } else t();
  }, r.prototype.poll = function() {
    c("polling"), this.polling = !0, this.doPoll(), this.emit("poll");
  }, r.prototype.onData = function(e) {
    var t = this;
    c("polling got data %s", e);
    var n = function(e, n, r) {
      return "opening" == t.readyState && t.onOpen(), "close" == e.type ? (t.onClose(), !1) : void t
        .onPacket(e);
    };
    a.decodePayload(e, this.socket.binaryType, n), "closed" != this.readyState && (this.polling = !1, this
      .emit("pollComplete"), "open" == this.readyState ? this.poll() : c(
        'ignoring poll - transport state "%s"', this.readyState));
  }, r.prototype.doClose = function() {
    function e() {
      c("writing close packet"), t.write([{
        type: "close"
      }]);
    }
    var t = this;
    "open" == this.readyState ? (c("transport open - closing"), e()) : (c(
      "transport not open - deferring close"), this.once("open", e));
  }, r.prototype.write = function(e) {
    var t = this;
    this.writable = !1;
    var n = function() {
        t.writable = !0, t.emit("drain");
      },
      t = this;
    a.encodePayload(e, this.supportsBinary, function(e) {
      t.doWrite(e, n);
    });
  }, r.prototype.uri = function() {
    var e = this.query || {},
      t = this.secure ? "https" : "http",
      n = "";
    return !1 !== this.timestampRequests && (e[this.timestampParam] = +new Date() + "-" + i.timestamps++),
      this.supportsBinary || e.sid || (e.b64 = 1), e = o.encode(e), this.port && ("https" == t && 443 !=
        this.port || "http" == t && 80 != this.port) && (n = ":" + this.port), e.length && (e = "?" + e),
      t + "://" + this.hostname + n + this.path + e;
  };
}
