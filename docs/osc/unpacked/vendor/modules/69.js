// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 69
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function r(e) {
    this.path = e.path, this.hostname = e.hostname, this.port = e.port, this.secure = e.secure, this.query = e.query,
      this.timestampParam = e.timestampParam, this.timestampRequests = e.timestampRequests, this.readyState = "", this
      .agent = e.agent || !1, this.socket = e.socket, this.enablesXDR = e.enablesXDR, this.pfx = e.pfx, this.key = e
      .key, this.passphrase = e.passphrase, this.cert = e.cert, this.ca = e.ca, this.ciphers = e.ciphers, this
      .rejectUnauthorized = e.rejectUnauthorized
  }
  var i = n(23),
    o = n(22);
  e.exports = r, o(r.prototype), r.timestamps = 0, r.prototype.onError = function(e, t) {
    var n = new Error(e);
    return n.type = "TransportError", n.description = t, this.emit("error", n), this
  }, r.prototype.open = function() {
    return "closed" != this.readyState && "" != this.readyState || (this.readyState = "opening", this.doOpen()), this
  }, r.prototype.close = function() {
    return "opening" != this.readyState && "open" != this.readyState || (this.doClose(), this.onClose()), this
  }, r.prototype.send = function(e) {
    if ("open" != this.readyState) throw new Error("Transport not open");
    this.write(e)
  }, r.prototype.onOpen = function() {
    this.readyState = "open", this.writable = !0, this.emit("open")
  }, r.prototype.onData = function(e) {
    var t = i.decodePacket(e, this.socket.binaryType);
    this.onPacket(t)
  }, r.prototype.onPacket = function(e) {
    this.emit("packet", e)
  }, r.prototype.onClose = function() {
    this.readyState = "closed", this.emit("close")
  }
}
