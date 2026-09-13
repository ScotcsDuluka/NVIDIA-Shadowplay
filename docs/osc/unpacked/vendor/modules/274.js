// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 274
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(t) {
    function r() {}

    function i(e) {
      if (c.call(this, e), t.location) {
        var n = "https:" == location.protocol,
          r = location.port;
        r || (r = n ? 443 : 80), this.xd = e.hostname != t.location.hostname || r != e.port, this.xs = e.secure != n
      }
    }

    function o(e) {
      this.method = e.method || "GET", this.uri = e.uri, this.xd = !!e.xd, this.xs = !!e.xs, this.async = !1 !== e
        .async, this.data = void 0 != e.data ? e.data : null, this.agent = e.agent, this.isBinary = e.isBinary, this
        .supportsBinary = e.supportsBinary, this.enablesXDR = e.enablesXDR, this.pfx = e.pfx, this.key = e.key, this
        .passphrase = e.passphrase, this.cert = e.cert, this.ca = e.ca, this.ciphers = e.ciphers, this
        .rejectUnauthorized = e.rejectUnauthorized, this.create()
    }

    function a() {
      for (var e in o.requests) o.requests.hasOwnProperty(e) && o.requests[e].abort()
    }
    var s = n(70),
      c = n(110),
      u = n(22),
      l = n(34),
      d = n(45)("engine.io-client:polling-xhr");
    e.exports = i, e.exports.Request = o, l(i, c), i.prototype.supportsBinary = !0, i.prototype.request = function(
    e) {
      return e = e || {}, e.uri = this.uri(), e.xd = this.xd, e.xs = this.xs, e.agent = this.agent || !1, e
        .supportsBinary = this.supportsBinary, e.enablesXDR = this.enablesXDR, e.pfx = this.pfx, e.key = this.key, e
        .passphrase = this.passphrase, e.cert = this.cert, e.ca = this.ca, e.ciphers = this.ciphers, e
        .rejectUnauthorized = this.rejectUnauthorized, new o(e)
    }, i.prototype.doWrite = function(e, t) {
      var n = "string" != typeof e && void 0 !== e,
        r = this.request({
          method: "POST",
          data: e,
          isBinary: n
        }),
        i = this;
      r.on("success", t), r.on("error", function(e) {
        i.onError("xhr post error", e)
      }), this.sendXhr = r
    }, i.prototype.doPoll = function() {
      d("xhr poll");
      var e = this.request(),
        t = this;
      e.on("data", function(e) {
        t.onData(e)
      }), e.on("error", function(e) {
        t.onError("xhr poll error", e)
      }), this.pollXhr = e
    }, u(o.prototype), o.prototype.create = function() {
      var e = {
        agent: this.agent,
        xdomain: this.xd,
        xscheme: this.xs,
        enablesXDR: this.enablesXDR
      };
      e.pfx = this.pfx, e.key = this.key, e.passphrase = this.passphrase, e.cert = this.cert, e.ca = this.ca, e
        .ciphers = this.ciphers, e.rejectUnauthorized = this.rejectUnauthorized;
      var n = this.xhr = new s(e),
        r = this;
      try {
        if (d("xhr open %s: %s", this.method, this.uri), n.open(this.method, this.uri, this.async), this
          .supportsBinary && (n.responseType = "arraybuffer"), "POST" == this.method) try {
          this.isBinary ? n.setRequestHeader("Content-type", "application/octet-stream") : n.setRequestHeader(
            "Content-type", "text/plain;charset=UTF-8")
        } catch (e) {}
        "withCredentials" in n && (n.withCredentials = !0), this.hasXDR() ? (n.onload = function() {
          r.onLoad()
        }, n.onerror = function() {
          r.onError(n.responseText)
        }) : n.onreadystatechange = function() {
          4 == n.readyState && (200 == n.status || 1223 == n.status ? r.onLoad() : setTimeout(function() {
            r.onError(n.status)
          }, 0))
        }, d("xhr data %s", this.data), n.send(this.data)
      } catch (e) {
        return void setTimeout(function() {
          r.onError(e)
        }, 0)
      }
      t.document && (this.index = o.requestsCount++, o.requests[this.index] = this)
    }, o.prototype.onSuccess = function() {
      this.emit("success"), this.cleanup()
    }, o.prototype.onData = function(e) {
      this.emit("data", e), this.onSuccess()
    }, o.prototype.onError = function(e) {
      this.emit("error", e), this.cleanup(!0)
    }, o.prototype.cleanup = function(e) {
      if ("undefined" != typeof this.xhr && null !== this.xhr) {
        if (this.hasXDR() ? this.xhr.onload = this.xhr.onerror = r : this.xhr.onreadystatechange = r, e) try {
          this.xhr.abort()
        } catch (e) {}
        t.document && delete o.requests[this.index], this.xhr = null
      }
    }, o.prototype.onLoad = function() {
      var e;
      try {
        var t;
        try {
          t = this.xhr.getResponseHeader("Content-Type").split(";")[0]
        } catch (e) {}
        e = "application/octet-stream" === t ? this.xhr.response : this.supportsBinary ? "ok" : this.xhr
          .responseText
      } catch (e) {
        this.onError(e)
      }
      null != e && this.onData(e)
    }, o.prototype.hasXDR = function() {
      return "undefined" != typeof t.XDomainRequest && !this.xs && this.enablesXDR
    }, o.prototype.abort = function() {
      this.cleanup()
    }, t.document && (o.requestsCount = 0, o.requests = {}, t.attachEvent ? t.attachEvent("onunload", a) : t
      .addEventListener && t.addEventListener("beforeunload", a, !1))
  }).call(t, function() {
    return this
  }())
}
