// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 60
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i) {
    module.exports = exports = i(require(1));
  }(this, function(e) {
    ! function() {
      var t = e,
        n = t.lib,
        r = n.Base,
        i = t.enc,
        o = i.Utf8,
        a = t.algo;
      a.HMAC = r.extend({
        init: function(e, t) {
          e = this._hasher = new e.init(), "string" == typeof t && (t = o.parse(t));
          var n = e.blockSize,
            r = 4 * n;
          t.sigBytes > r && (t = e.finalize(t)), t.clamp();
          for (var i = this._oKey = t.clone(), a = this._iKey = t.clone(), s = i.words, c = a.words,
              u = 0; u < n; u++) s[u] ^= 1549556828, c[u] ^= 909522486;
          i.sigBytes = a.sigBytes = r, this.reset();
        },
        reset: function() {
          var e = this._hasher;
          e.reset(), e.update(this._iKey);
        },
        update: function(e) {
          return this._hasher.update(e), this;
        },
        finalize: function(e) {
          var t = this._hasher,
            n = t.finalize(e);
          t.reset();
          var r = t.finalize(this._oKey.clone().concat(n));
          return r;
        }
      });
    }();
  });
}
