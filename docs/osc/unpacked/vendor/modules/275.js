// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 275
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function r(e) {
    var t = e && e.forceBase64;
    t && (this.supportsBinary = !1), i.call(this, e)
  }
  var i = n(69),
    o = n(23),
    a = n(71),
    s = n(34),
    c = n(45)("engine.io-client:websocket"),
    u = n(285);
  e.exports = r, s(r, i), r.prototype.name = "websocket", r.prototype.supportsBinary = !0, r.prototype.doOpen =
    function() {
      if (this.check()) {
        var e = this.uri(),
          t = void 0,
          n = {
            agent: this.agent
          };
        n.pfx = this.pfx, n.key = this.key, n.passphrase = this.passphrase, n.cert = this.cert, n.ca = this.ca, n
          .ciphers = this.ciphers, n.rejectUnauthorized = this.rejectUnauthorized, this.ws = new u(e, t, n), void 0 ===
          this.ws.binaryType && (this.supportsBinary = !1), this.ws.binaryType = "arraybuffer", this.addEventListeners()
      }
    }, r.prototype.addEventListeners = function() {
      var e = this;
      this.ws.onopen = function() {
        e.onOpen()
      }, this.ws.onclose = function() {
        e.onClose()
      }, this.ws.onmessage = function(t) {
        e.onData(t.data)
      }, this.ws.onerror = function(t) {
        e.onError("websocket error", t)
      }
    }, "undefined" != typeof navigator && /iPad|iPhone|iPod/i.test(navigator.userAgent) && (r.prototype.onData =
      function(e) {
        var t = this;
        setTimeout(function() {
          i.prototype.onData.call(t, e)
        }, 0)
      }), r.prototype.write = function(e) {
      function t() {
        n.writable = !0, n.emit("drain")
      }
      var n = this;
      this.writable = !1;
      for (var r = 0, i = e.length; r < i; r++) o.encodePacket(e[r], this.supportsBinary, function(e) {
        try {
          n.ws.send(e)
        } catch (e) {
          c("websocket closed before onclose event")
        }
      });
      setTimeout(t, 0)
    }, r.prototype.onClose = function() {
      i.prototype.onClose.call(this)
    }, r.prototype.doClose = function() {
      "undefined" != typeof this.ws && this.ws.close()
    }, r.prototype.uri = function() {
      var e = this.query || {},
        t = this.secure ? "wss" : "ws",
        n = "";
      return this.port && ("wss" == t && 443 != this.port || "ws" == t && 80 != this.port) && (n = ":" + this.port),
        this.timestampRequests && (e[this.timestampParam] = +new Date), this.supportsBinary || (e.b64 = 1), e = a
        .encode(e), e.length && (e = "?" + e), t + "://" + this.hostname + n + this.path + e
    }, r.prototype.check = function() {
      return !(!u || "__initialize" in u && this.name === r.prototype.name)
    }
}
