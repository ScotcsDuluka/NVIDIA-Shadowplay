// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 105
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r(e, t) {
    return this instanceof r ? (e && "object" == typeof e && (t = e, e = void 0), t = t || {}, t.path = t
      .path || "/socket.io", this.nsps = {}, this.subs = [], this.opts = t, this.reconnection(t
        .reconnection !== !1), this.reconnectionAttempts(t.reconnectionAttempts || 1 / 0), this
      .reconnectionDelay(t.reconnectionDelay || 1e3), this.reconnectionDelayMax(t.reconnectionDelayMax ||
        5e3), this.randomizationFactor(t.randomizationFactor || .5), this.backoff = new f({
        min: this.reconnectionDelay(),
        max: this.reconnectionDelayMax(),
        jitter: this.randomizationFactor()
      }), this.timeout(null == t.timeout ? 2e4 : t.timeout), this.readyState = "closed", this.uri = e, this
      .connected = [], this.encoding = !1, this.packetBuffer = [], this.encoder = new s.Encoder(), this
      .decoder = new s.Decoder(), this.autoConnect = t.autoConnect !== !1, void(this.autoConnect && this
        .open())) : new r(e, t);
  }
  var i = (require(108), require(270)),
    o = require(107),
    a = require(22),
    s = require(72),
    c = require(106),
    u = require(79),
    l = (require(264), require(32)("socket.io-client:manager")),
    d = require(104),
    f = require(162);
  module.exports = r, r.prototype.emitAll = function() {
    this.emit.apply(this, arguments);
    for (var e in this.nsps) this.nsps[e].emit.apply(this.nsps[e], arguments);
  }, r.prototype.updateSocketIds = function() {
    for (var e in this.nsps) this.nsps[e].id = this.engine.id;
  }, a(r.prototype), r.prototype.reconnection = function(e) {
    return arguments.length ? (this._reconnection = !!e, this) : this._reconnection;
  }, r.prototype.reconnectionAttempts = function(e) {
    return arguments.length ? (this._reconnectionAttempts = e, this) : this._reconnectionAttempts;
  }, r.prototype.reconnectionDelay = function(e) {
    return arguments.length ? (this._reconnectionDelay = e, this.backoff && this.backoff.setMin(e), this) :
      this._reconnectionDelay;
  }, r.prototype.randomizationFactor = function(e) {
    return arguments.length ? (this._randomizationFactor = e, this.backoff && this.backoff.setJitter(e),
      this) : this._randomizationFactor;
  }, r.prototype.reconnectionDelayMax = function(e) {
    return arguments.length ? (this._reconnectionDelayMax = e, this.backoff && this.backoff.setMax(e),
      this) : this._reconnectionDelayMax;
  }, r.prototype.timeout = function(e) {
    return arguments.length ? (this._timeout = e, this) : this._timeout;
  }, r.prototype.maybeReconnectOnOpen = function() {
    !this.reconnecting && this._reconnection && 0 === this.backoff.attempts && this.reconnect();
  }, r.prototype.open = r.prototype.connect = function(e) {
    if (l("readyState %s", this.readyState), ~this.readyState.indexOf("open")) return this;
    l("opening %s", this.uri), this.engine = i(this.uri, this.opts);
    var t = this.engine,
      n = this;
    this.readyState = "opening", this.skipReconnect = !1;
    var r = c(t, "open", function() {
        n.onopen(), e && e();
      }),
      o = c(t, "error", function(t) {
        if (l("connect_error"), n.cleanup(), n.readyState = "closed", n.emitAll("connect_error", t), e) {
          var r = new Error("Connection error");
          r.data = t, e(r);
        } else n.maybeReconnectOnOpen();
      });
    if (!1 !== this._timeout) {
      var a = this._timeout;
      l("connect attempt will timeout after %d", a);
      var s = setTimeout(function() {
        l("connect attempt timed out after %d", a), r.destroy(), t.close(), t.emit("error", "timeout"),
          n.emitAll("connect_timeout", a);
      }, a);
      this.subs.push({
        destroy: function() {
          clearTimeout(s);
        }
      });
    }
    return this.subs.push(r), this.subs.push(o), this;
  }, r.prototype.onopen = function() {
    l("open"), this.cleanup(), this.readyState = "open", this.emit("open");
    var e = this.engine;
    this.subs.push(c(e, "data", u(this, "ondata"))), this.subs.push(c(this.decoder, "decoded", u(this,
      "ondecoded"))), this.subs.push(c(e, "error", u(this, "onerror"))), this.subs.push(c(e, "close", u(
      this, "onclose")));
  }, r.prototype.ondata = function(e) {
    this.decoder.add(e);
  }, r.prototype.ondecoded = function(e) {
    this.emit("packet", e);
  }, r.prototype.onerror = function(e) {
    l("error", e), this.emitAll("error", e);
  }, r.prototype.socket = function(e) {
    var t = this.nsps[e];
    if (!t) {
      t = new o(this, e), this.nsps[e] = t;
      var n = this;
      t.on("connect", function() {
        t.id = n.engine.id, ~d(n.connected, t) || n.connected.push(t);
      });
    }
    return t;
  }, r.prototype.destroy = function(e) {
    var t = d(this.connected, e);
    ~t && this.connected.splice(t, 1), this.connected.length || this.close();
  }, r.prototype.packet = function(e) {
    l("writing packet %j", e);
    var t = this;
    t.encoding ? t.packetBuffer.push(e) : (t.encoding = !0, this.encoder.encode(e, function(e) {
      for (var n = 0; n < e.length; n++) t.engine.write(e[n]);
      t.encoding = !1, t.processPacketQueue();
    }));
  }, r.prototype.processPacketQueue = function() {
    if (this.packetBuffer.length > 0 && !this.encoding) {
      var e = this.packetBuffer.shift();
      this.packet(e);
    }
  }, r.prototype.cleanup = function() {
    for (var e; e = this.subs.shift();) e.destroy();
    this.packetBuffer = [], this.encoding = !1, this.decoder.destroy();
  }, r.prototype.close = r.prototype.disconnect = function() {
    this.skipReconnect = !0, this.backoff.reset(), this.readyState = "closed", this.engine && this.engine
      .close();
  }, r.prototype.onclose = function(e) {
    l("close"), this.cleanup(), this.backoff.reset(), this.readyState = "closed", this.emit("close", e),
      this._reconnection && !this.skipReconnect && this.reconnect();
  }, r.prototype.reconnect = function() {
    if (this.reconnecting || this.skipReconnect) return this;
    var e = this;
    if (this.backoff.attempts >= this._reconnectionAttempts) l("reconnect failed"), this.backoff.reset(),
      this.emitAll("reconnect_failed"), this.reconnecting = !1;
    else {
      var t = this.backoff.duration();
      l("will wait %dms before reconnect attempt", t), this.reconnecting = !0;
      var n = setTimeout(function() {
        e.skipReconnect || (l("attempting reconnect"), e.emitAll("reconnect_attempt", e.backoff
          .attempts), e.emitAll("reconnecting", e.backoff.attempts), e.skipReconnect || e.open(
          function(t) {
            t ? (l("reconnect attempt error"), e.reconnecting = !1, e.reconnect(), e.emitAll(
              "reconnect_error", t.data)) : (l("reconnect success"), e.onreconnect());
          }));
      }, t);
      this.subs.push({
        destroy: function() {
          clearTimeout(n);
        }
      });
    }
  }, r.prototype.onreconnect = function() {
    var e = this.backoff.attempts;
    this.reconnecting = !1, this.backoff.reset(), this.updateSocketIds(), this.emitAll("reconnect", e);
  };
}
