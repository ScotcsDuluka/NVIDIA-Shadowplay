// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 107
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function r(e, t) {
    this.io = e, this.nsp = t, this.json = this, this.ids = 0, this.acks = {}, this.io.autoConnect && this.open(), this
      .receiveBuffer = [], this.sendBuffer = [], this.connected = !1, this.disconnected = !0
  }
  var i = n(72),
    o = n(22),
    a = n(284),
    s = n(106),
    c = n(79),
    u = n(32)("socket.io-client:socket"),
    l = n(111);
  e.exports = t = r;
  var d = {
      connect: 1,
      connect_error: 1,
      connect_timeout: 1,
      disconnect: 1,
      error: 1,
      reconnect: 1,
      reconnect_attempt: 1,
      reconnect_failed: 1,
      reconnect_error: 1,
      reconnecting: 1
    },
    f = o.prototype.emit;
  o(r.prototype), r.prototype.subEvents = function() {
    if (!this.subs) {
      var e = this.io;
      this.subs = [s(e, "open", c(this, "onopen")), s(e, "packet", c(this, "onpacket")), s(e, "close", c(this,
        "onclose"))]
    }
  }, r.prototype.open = r.prototype.connect = function() {
    return this.connected ? this : (this.subEvents(), this.io.open(), "open" == this.io.readyState && this.onopen(),
      this)
  }, r.prototype.send = function() {
    var e = a(arguments);
    return e.unshift("message"), this.emit.apply(this, e), this
  }, r.prototype.emit = function(e) {
    if (d.hasOwnProperty(e)) return f.apply(this, arguments), this;
    var t = a(arguments),
      n = i.EVENT;
    l(t) && (n = i.BINARY_EVENT);
    var r = {
      type: n,
      data: t
    };
    return "function" == typeof t[t.length - 1] && (u("emitting packet with ack id %d", this.ids), this.acks[this
      .ids] = t.pop(), r.id = this.ids++), this.connected ? this.packet(r) : this.sendBuffer.push(r), this
  }, r.prototype.packet = function(e) {
    e.nsp = this.nsp, this.io.packet(e)
  }, r.prototype.onopen = function() {
    u("transport is open - connecting"), "/" != this.nsp && this.packet({
      type: i.CONNECT
    })
  }, r.prototype.onclose = function(e) {
    u("close (%s)", e), this.connected = !1, this.disconnected = !0, delete this.id, this.emit("disconnect", e)
  }, r.prototype.onpacket = function(e) {
    if (e.nsp == this.nsp) switch (e.type) {
      case i.CONNECT:
        this.onconnect();
        break;
      case i.EVENT:
        this.onevent(e);
        break;
      case i.BINARY_EVENT:
        this.onevent(e);
        break;
      case i.ACK:
        this.onack(e);
        break;
      case i.BINARY_ACK:
        this.onack(e);
        break;
      case i.DISCONNECT:
        this.ondisconnect();
        break;
      case i.ERROR:
        this.emit("error", e.data)
    }
  }, r.prototype.onevent = function(e) {
    var t = e.data || [];
    u("emitting event %j", t), null != e.id && (u("attaching ack callback to event"), t.push(this.ack(e.id))), this
      .connected ? f.apply(this, t) : this.receiveBuffer.push(t)
  }, r.prototype.ack = function(e) {
    var t = this,
      n = !1;
    return function() {
      if (!n) {
        n = !0;
        var r = a(arguments);
        u("sending ack %j", r);
        var o = l(r) ? i.BINARY_ACK : i.ACK;
        t.packet({
          type: o,
          id: e,
          data: r
        })
      }
    }
  }, r.prototype.onack = function(e) {
    u("calling ack %s with %j", e.id, e.data);
    var t = this.acks[e.id];
    t.apply(this, e.data), delete this.acks[e.id]
  }, r.prototype.onconnect = function() {
    this.connected = !0, this.disconnected = !1, this.emit("connect"), this.emitBuffered()
  }, r.prototype.emitBuffered = function() {
    var e;
    for (e = 0; e < this.receiveBuffer.length; e++) f.apply(this, this.receiveBuffer[e]);
    for (this.receiveBuffer = [], e = 0; e < this.sendBuffer.length; e++) this.packet(this.sendBuffer[e]);
    this.sendBuffer = []
  }, r.prototype.ondisconnect = function() {
    u("server disconnect (%s)", this.nsp), this.destroy(), this.onclose("io server disconnect")
  }, r.prototype.destroy = function() {
    if (this.subs) {
      for (var e = 0; e < this.subs.length; e++) this.subs[e].destroy();
      this.subs = null
    }
    this.io.destroy(this)
  }, r.prototype.close = r.prototype.disconnect = function() {
    return this.connected && (u("performing disconnect (%s)", this.nsp), this.packet({
      type: i.DISCONNECT
    })), this.destroy(), this.connected && this.onclose("io client disconnect"), this
  }
}
