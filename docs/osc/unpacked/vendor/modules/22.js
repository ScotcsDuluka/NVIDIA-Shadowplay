// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 22
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  function n(e) {
    if (e) return r(e)
  }

  function r(e) {
    for (var t in n.prototype) e[t] = n.prototype[t];
    return e
  }
  e.exports = n, n.prototype.on = n.prototype.addEventListener = function(e, t) {
      return this._callbacks = this._callbacks || {}, (this._callbacks[e] = this._callbacks[e] || []).push(t), this
    }, n.prototype.once = function(e, t) {
      function n() {
        r.off(e, n), t.apply(this, arguments)
      }
      var r = this;
      return this._callbacks = this._callbacks || {}, n.fn = t, this.on(e, n), this
    }, n.prototype.off = n.prototype.removeListener = n.prototype.removeAllListeners = n.prototype.removeEventListener =
    function(e, t) {
      if (this._callbacks = this._callbacks || {}, 0 == arguments.length) return this._callbacks = {}, this;
      var n = this._callbacks[e];
      if (!n) return this;
      if (1 == arguments.length) return delete this._callbacks[e], this;
      for (var r, i = 0; i < n.length; i++)
        if (r = n[i], r === t || r.fn === t) {
          n.splice(i, 1);
          break
        } return this
    }, n.prototype.emit = function(e) {
      this._callbacks = this._callbacks || {};
      var t = [].slice.call(arguments, 1),
        n = this._callbacks[e];
      if (n) {
        n = n.slice(0);
        for (var r = 0, i = n.length; r < i; ++r) n[r].apply(this, t)
      }
      return this
    }, n.prototype.listeners = function(e) {
      return this._callbacks = this._callbacks || {}, this._callbacks[e] || []
    }, n.prototype.hasListeners = function(e) {
      return !!this.listeners(e).length
    }
}
