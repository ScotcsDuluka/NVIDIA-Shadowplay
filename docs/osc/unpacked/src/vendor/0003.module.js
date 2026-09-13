// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 3
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(14));
  }(this, function(e) {
    e.lib.Cipher || function(t) {
      var n = e,
        r = n.lib,
        i = r.Base,
        o = r.WordArray,
        a = r.BufferedBlockAlgorithm,
        s = n.enc,
        c = (s.Utf8, s.Base64),
        u = n.algo,
        l = u.EvpKDF,
        d = r.Cipher = a.extend({
          cfg: i.extend(),
          createEncryptor: function(e, t) {
            return this.create(this._ENC_XFORM_MODE, e, t);
          },
          createDecryptor: function(e, t) {
            return this.create(this._DEC_XFORM_MODE, e, t);
          },
          init: function(e, t, n) {
            this.cfg = this.cfg.extend(n), this._xformMode = e, this._key = t, this.reset();
          },
          reset: function() {
            a.reset.call(this), this._doReset();
          },
          process: function(e) {
            return this._append(e), this._process();
          },
          finalize: function(e) {
            e && this._append(e);
            var t = this._doFinalize();
            return t;
          },
          keySize: 4,
          ivSize: 4,
          _ENC_XFORM_MODE: 1,
          _DEC_XFORM_MODE: 2,
          _createHelper: function() {
            function e(e) {
              return "string" == typeof e ? w : E;
            }
            return function(t) {
              return {
                encrypt: function(n, r, i) {
                  return e(r).encrypt(t, n, r, i);
                },
                decrypt: function(n, r, i) {
                  return e(r).decrypt(t, n, r, i);
                }
              };
            };
          }()
        }),
        f = (r.StreamCipher = d.extend({
          _doFinalize: function() {
            var e = this._process(!0);
            return e;
          },
          blockSize: 1
        }), n.mode = {}),
        h = r.BlockCipherMode = i.extend({
          createEncryptor: function(e, t) {
            return this.Encryptor.create(e, t);
          },
          createDecryptor: function(e, t) {
            return this.Decryptor.create(e, t);
          },
          init: function(e, t) {
            this._cipher = e, this._iv = t;
          }
        }),
        p = f.CBC = function() {
          function e(e, n, r) {
            var i = this._iv;
            if (i) {
              var o = i;
              this._iv = t;
            } else var o = this._prevBlock;
            for (var a = 0; a < r; a++) e[n + a] ^= o[a];
          }
          var n = h.extend();
          return n.Encryptor = n.extend({
            processBlock: function(t, n) {
              var r = this._cipher,
                i = r.blockSize;
              e.call(this, t, n, i), r.encryptBlock(t, n), this._prevBlock = t.slice(n, n + i);
            }
          }), n.Decryptor = n.extend({
            processBlock: function(t, n) {
              var r = this._cipher,
                i = r.blockSize,
                o = t.slice(n, n + i);
              r.decryptBlock(t, n), e.call(this, t, n, i), this._prevBlock = o;
            }
          }), n;
        }(),
        m = n.pad = {},
        v = m.Pkcs7 = {
          pad: function(e, t) {
            for (var n = 4 * t, r = n - e.sigBytes % n, i = r << 24 | r << 16 | r << 8 | r, a = [], s =
                0; s < r; s += 4) a.push(i);
            var c = o.create(a, r);
            e.concat(c);
          },
          unpad: function(e) {
            var t = 255 & e.words[e.sigBytes - 1 >>> 2];
            e.sigBytes -= t;
          }
        },
        g = (r.BlockCipher = d.extend({
          cfg: d.cfg.extend({
            mode: p,
            padding: v
          }),
          reset: function() {
            d.reset.call(this);
            var e = this.cfg,
              t = e.iv,
              n = e.mode;
            if (this._xformMode == this._ENC_XFORM_MODE) var r = n.createEncryptor;
            else {
              var r = n.createDecryptor;
              this._minBufferSize = 1;
            }
            this._mode && this._mode.__creator == r ? this._mode.init(this, t && t.words) : (this
              ._mode = r.call(n, this, t && t.words), this._mode.__creator = r);
          },
          _doProcessBlock: function(e, t) {
            this._mode.processBlock(e, t);
          },
          _doFinalize: function() {
            var e = this.cfg.padding;
            if (this._xformMode == this._ENC_XFORM_MODE) {
              e.pad(this._data, this.blockSize);
              var t = this._process(!0);
            } else {
              var t = this._process(!0);
              e.unpad(t);
            }
            return t;
          },
          blockSize: 4
        }), r.CipherParams = i.extend({
          init: function(e) {
            this.mixIn(e);
          },
          toString: function(e) {
            return (e || this.formatter).stringify(this);
          }
        })),
        y = n.format = {},
        b = y.OpenSSL = {
          stringify: function(e) {
            var t = e.ciphertext,
              n = e.salt;
            if (n) var r = o.create([1398893684, 1701076831]).concat(n).concat(t);
            else var r = t;
            return r.toString(c);
          },
          parse: function(e) {
            var t = c.parse(e),
              n = t.words;
            if (1398893684 == n[0] && 1701076831 == n[1]) {
              var r = o.create(n.slice(2, 4));
              n.splice(0, 4), t.sigBytes -= 16;
            }
            return g.create({
              ciphertext: t,
              salt: r
            });
          }
        },
        E = r.SerializableCipher = i.extend({
          cfg: i.extend({
            format: b
          }),
          encrypt: function(e, t, n, r) {
            r = this.cfg.extend(r);
            var i = e.createEncryptor(n, r),
              o = i.finalize(t),
              a = i.cfg;
            return g.create({
              ciphertext: o,
              key: n,
              iv: a.iv,
              algorithm: e,
              mode: a.mode,
              padding: a.padding,
              blockSize: e.blockSize,
              formatter: r.format
            });
          },
          decrypt: function(e, t, n, r) {
            r = this.cfg.extend(r), t = this._parse(t, r.format);
            var i = e.createDecryptor(n, r).finalize(t.ciphertext);
            return i;
          },
          _parse: function(e, t) {
            return "string" == typeof e ? t.parse(e, this) : e;
          }
        }),
        _ = n.kdf = {},
        $ = _.OpenSSL = {
          execute: function(e, t, n, r) {
            r || (r = o.random(8));
            var i = l.create({
                keySize: t + n
              }).compute(e, r),
              a = o.create(i.words.slice(t), 4 * n);
            return i.sigBytes = 4 * t, g.create({
              key: i,
              iv: a,
              salt: r
            });
          }
        },
        w = r.PasswordBasedCipher = E.extend({
          cfg: E.cfg.extend({
            kdf: $
          }),
          encrypt: function(e, t, n, r) {
            r = this.cfg.extend(r);
            var i = r.kdf.execute(n, e.keySize, e.ivSize);
            r.iv = i.iv;
            var o = E.encrypt.call(this, e, t, i.key, r);
            return o.mixIn(i), o;
          },
          decrypt: function(e, t, n, r) {
            r = this.cfg.extend(r), t = this._parse(t, r.format);
            var i = r.kdf.execute(n, e.keySize, e.ivSize, t.salt);
            r.iv = i.iv;
            var o = E.decrypt.call(this, e, t, i.key, r);
            return o;
          }
        });
    }();
  });
}
