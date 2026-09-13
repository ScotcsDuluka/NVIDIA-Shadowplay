// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 1
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(n, r) {
    e.exports = t = r()
  }(this, function() {
    var e = e || function(e, t) {
      var n = Object.create || function() {
          function e() {}
          return function(t) {
            var n;
            return e.prototype = t, n = new e, e.prototype = null, n
          }
        }(),
        r = {},
        i = r.lib = {},
        o = i.Base = function() {
          return {
            extend: function(e) {
              var t = n(this);
              return e && t.mixIn(e), t.hasOwnProperty("init") && this.init !== t.init || (t.init = function() {
                t.$super.init.apply(this, arguments)
              }), t.init.prototype = t, t.$super = this, t
            },
            create: function() {
              var e = this.extend();
              return e.init.apply(e, arguments), e
            },
            init: function() {},
            mixIn: function(e) {
              for (var t in e) e.hasOwnProperty(t) && (this[t] = e[t]);
              e.hasOwnProperty("toString") && (this.toString = e.toString)
            },
            clone: function() {
              return this.init.prototype.extend(this)
            }
          }
        }(),
        a = i.WordArray = o.extend({
          init: function(e, n) {
            e = this.words = e || [], n != t ? this.sigBytes = n : this.sigBytes = 4 * e.length
          },
          toString: function(e) {
            return (e || c).stringify(this)
          },
          concat: function(e) {
            var t = this.words,
              n = e.words,
              r = this.sigBytes,
              i = e.sigBytes;
            if (this.clamp(), r % 4)
              for (var o = 0; o < i; o++) {
                var a = n[o >>> 2] >>> 24 - o % 4 * 8 & 255;
                t[r + o >>> 2] |= a << 24 - (r + o) % 4 * 8
              } else
                for (var o = 0; o < i; o += 4) t[r + o >>> 2] = n[o >>> 2];
            return this.sigBytes += i, this
          },
          clamp: function() {
            var t = this.words,
              n = this.sigBytes;
            t[n >>> 2] &= 4294967295 << 32 - n % 4 * 8, t.length = e.ceil(n / 4)
          },
          clone: function() {
            var e = o.clone.call(this);
            return e.words = this.words.slice(0), e
          },
          random: function(t) {
            for (var n, r = [], i = function(t) {
                var t = t,
                  n = 987654321,
                  r = 4294967295;
                return function() {
                  n = 36969 * (65535 & n) + (n >> 16) & r, t = 18e3 * (65535 & t) + (t >> 16) & r;
                  var i = (n << 16) + t & r;
                  return i /= 4294967296, i += .5, i * (e.random() > .5 ? 1 : -1)
                }
              }, o = 0; o < t; o += 4) {
              var s = i(4294967296 * (n || e.random()));
              n = 987654071 * s(), r.push(4294967296 * s() | 0)
            }
            return new a.init(r, t)
          }
        }),
        s = r.enc = {},
        c = s.Hex = {
          stringify: function(e) {
            for (var t = e.words, n = e.sigBytes, r = [], i = 0; i < n; i++) {
              var o = t[i >>> 2] >>> 24 - i % 4 * 8 & 255;
              r.push((o >>> 4).toString(16)), r.push((15 & o).toString(16))
            }
            return r.join("")
          },
          parse: function(e) {
            for (var t = e.length, n = [], r = 0; r < t; r += 2) n[r >>> 3] |= parseInt(e.substr(r, 2), 16) <<
              24 - r % 8 * 4;
            return new a.init(n, t / 2)
          }
        },
        u = s.Latin1 = {
          stringify: function(e) {
            for (var t = e.words, n = e.sigBytes, r = [], i = 0; i < n; i++) {
              var o = t[i >>> 2] >>> 24 - i % 4 * 8 & 255;
              r.push(String.fromCharCode(o))
            }
            return r.join("")
          },
          parse: function(e) {
            for (var t = e.length, n = [], r = 0; r < t; r++) n[r >>> 2] |= (255 & e.charCodeAt(r)) << 24 - r %
              4 * 8;
            return new a.init(n, t)
          }
        },
        l = s.Utf8 = {
          stringify: function(e) {
            try {
              return decodeURIComponent(escape(u.stringify(e)))
            } catch (e) {
              throw new Error("Malformed UTF-8 data")
            }
          },
          parse: function(e) {
            return u.parse(unescape(encodeURIComponent(e)))
          }
        },
        d = i.BufferedBlockAlgorithm = o.extend({
          reset: function() {
            this._data = new a.init, this._nDataBytes = 0
          },
          _append: function(e) {
            "string" == typeof e && (e = l.parse(e)), this._data.concat(e), this._nDataBytes += e.sigBytes
          },
          _process: function(t) {
            var n = this._data,
              r = n.words,
              i = n.sigBytes,
              o = this.blockSize,
              s = 4 * o,
              c = i / s;
            c = t ? e.ceil(c) : e.max((0 | c) - this._minBufferSize, 0);
            var u = c * o,
              l = e.min(4 * u, i);
            if (u) {
              for (var d = 0; d < u; d += o) this._doProcessBlock(r, d);
              var f = r.splice(0, u);
              n.sigBytes -= l
            }
            return new a.init(f, l)
          },
          clone: function() {
            var e = o.clone.call(this);
            return e._data = this._data.clone(), e
          },
          _minBufferSize: 0
        }),
        f = (i.Hasher = d.extend({
          cfg: o.extend(),
          init: function(e) {
            this.cfg = this.cfg.extend(e), this.reset()
          },
          reset: function() {
            d.reset.call(this), this._doReset()
          },
          update: function(e) {
            return this._append(e), this._process(), this
          },
          finalize: function(e) {
            e && this._append(e);
            var t = this._doFinalize();
            return t
          },
          blockSize: 16,
          _createHelper: function(e) {
            return function(t, n) {
              return new e.init(n).finalize(t)
            }
          },
          _createHmacHelper: function(e) {
            return function(t, n) {
              return new f.HMAC.init(e, n).finalize(t)
            }
          }
        }), r.algo = {});
      return r
    }(Math);
    return e
  })
}
