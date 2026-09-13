// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 165
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  (function(e) {
    /*!
     * The buffer module from node.js, for the browser.
     *
     * @author   Feross Aboukhadijeh <http://feross.org>
     * @license  MIT
     */
    "use strict";

    function r() {
      try {
        var e = new Uint8Array(1);
        return e.__proto__ = {
          __proto__: Uint8Array.prototype,
          foo: function() {
            return 42;
          }
        }, 42 === e.foo() && "function" == typeof e.subarray && 0 === e.subarray(1, 1).byteLength;
      } catch (e) {
        return !1;
      }
    }

    function i() {
      return a.TYPED_ARRAY_SUPPORT ? 2147483647 : 1073741823;
    }

    function o(e, t) {
      if (i() < t) throw new RangeError("Invalid typed array length");
      return a.TYPED_ARRAY_SUPPORT ? (e = new Uint8Array(t), e.__proto__ = a.prototype) : (null === e && (
        e = new a(t)), e.length = t), e;
    }

    function a(e, t, n) {
      if (!(a.TYPED_ARRAY_SUPPORT || this instanceof a)) return new a(e, t, n);
      if ("number" == typeof e) {
        if ("string" == typeof t) throw new Error(
          "If encoding is specified then the first argument must be a string");
        return l(this, e);
      }
      return s(this, e, t, n);
    }

    function s(e, t, n, r) {
      if ("number" == typeof t) throw new TypeError('"value" argument must not be a number');
      return "undefined" != typeof ArrayBuffer && t instanceof ArrayBuffer ? h(e, t, n, r) : "string" ==
        typeof t ? d(e, t, n) : p(e, t);
    }

    function c(e) {
      if ("number" != typeof e) throw new TypeError('"size" argument must be a number');
      if (e < 0) throw new RangeError('"size" argument must not be negative');
    }

    function u(e, t, n, r) {
      return c(t), t <= 0 ? o(e, t) : void 0 !== n ? "string" == typeof r ? o(e, t).fill(n, r) : o(e, t)
        .fill(n) : o(e, t);
    }

    function l(e, t) {
      if (c(t), e = o(e, t < 0 ? 0 : 0 | m(t)), !a.TYPED_ARRAY_SUPPORT)
        for (var n = 0; n < t; ++n) e[n] = 0;
      return e;
    }

    function d(e, t, n) {
      if ("string" == typeof n && "" !== n || (n = "utf8"), !a.isEncoding(n)) throw new TypeError(
        '"encoding" must be a valid string encoding');
      var r = 0 | g(t, n);
      e = o(e, r);
      var i = e.write(t, n);
      return i !== r && (e = e.slice(0, i)), e;
    }

    function f(e, t) {
      var n = t.length < 0 ? 0 : 0 | m(t.length);
      e = o(e, n);
      for (var r = 0; r < n; r += 1) e[r] = 255 & t[r];
      return e;
    }

    function h(e, t, n, r) {
      if (t.byteLength, n < 0 || t.byteLength < n) throw new RangeError("'offset' is out of bounds");
      if (t.byteLength < n + (r || 0)) throw new RangeError("'length' is out of bounds");
      return t = void 0 === n && void 0 === r ? new Uint8Array(t) : void 0 === r ? new Uint8Array(t, n) :
        new Uint8Array(t, n, r), a.TYPED_ARRAY_SUPPORT ? (e = t, e.__proto__ = a.prototype) : e = f(e, t),
        e;
    }

    function p(e, t) {
      if (a.isBuffer(t)) {
        var n = 0 | m(t.length);
        return e = o(e, n), 0 === e.length ? e : (t.copy(e, 0, 0, n), e);
      }
      if (t) {
        if ("undefined" != typeof ArrayBuffer && t.buffer instanceof ArrayBuffer || "length" in t)
        return "number" != typeof t.length || X(t.length) ? o(e, 0) : f(e, t);
        if ("Buffer" === t.type && Z(t.data)) return f(e, t.data);
      }
      throw new TypeError(
        "First argument must be a string, Buffer, ArrayBuffer, Array, or array-like object.");
    }

    function m(e) {
      if (e >= i()) throw new RangeError("Attempt to allocate Buffer larger than maximum size: 0x" + i()
        .toString(16) + " bytes");
      return 0 | e;
    }

    function v(e) {
      return +e != e && (e = 0), a.alloc(+e);
    }

    function g(e, t) {
      if (a.isBuffer(e)) return e.length;
      if ("undefined" != typeof ArrayBuffer && "function" == typeof ArrayBuffer.isView && (ArrayBuffer
          .isView(e) || e instanceof ArrayBuffer)) return e.byteLength;
      "string" != typeof e && (e = "" + e);
      var n = e.length;
      if (0 === n) return 0;
      for (var r = !1;;) switch (t) {
        case "ascii":
        case "latin1":
        case "binary":
          return n;
        case "utf8":
        case "utf-8":
        case void 0:
          return G(e).length;
        case "ucs2":
        case "ucs-2":
        case "utf16le":
        case "utf-16le":
          return 2 * n;
        case "hex":
          return n >>> 1;
        case "base64":
          return Y(e).length;
        default:
          if (r) return G(e).length;
          t = ("" + t).toLowerCase(), r = !0;
      }
    }

    function y(e, t, n) {
      var r = !1;
      if ((void 0 === t || t < 0) && (t = 0), t > this.length) return "";
      if ((void 0 === n || n > this.length) && (n = this.length), n <= 0) return "";
      if (n >>>= 0, t >>>= 0, n <= t) return "";
      for (e || (e = "utf8");;) switch (e) {
        case "hex":
          return O(this, t, n);
        case "utf8":
        case "utf-8":
          return M(this, t, n);
        case "ascii":
          return N(this, t, n);
        case "latin1":
        case "binary":
          return I(this, t, n);
        case "base64":
          return A(this, t, n);
        case "ucs2":
        case "ucs-2":
        case "utf16le":
        case "utf-16le":
          return D(this, t, n);
        default:
          if (r) throw new TypeError("Unknown encoding: " + e);
          e = (e + "").toLowerCase(), r = !0;
      }
    }

    function b(e, t, n) {
      var r = e[t];
      e[t] = e[n], e[n] = r;
    }

    function E(e, t, n, r, i) {
      if (0 === e.length) return -1;
      if ("string" == typeof n ? (r = n, n = 0) : n > 2147483647 ? n = 2147483647 : n < -2147483648 && (
          n = -2147483648), n = +n, isNaN(n) && (n = i ? 0 : e.length - 1), n < 0 && (n = e.length + n),
        n >= e.length) {
        if (i) return -1;
        n = e.length - 1;
      } else if (n < 0) {
        if (!i) return -1;
        n = 0;
      }
      if ("string" == typeof t && (t = a.from(t, r)), a.isBuffer(t)) return 0 === t.length ? -1 : _(e, t, n,
        r, i);
      if ("number" == typeof t) return t &= 255, a.TYPED_ARRAY_SUPPORT && "function" == typeof Uint8Array
        .prototype.indexOf ? i ? Uint8Array.prototype.indexOf.call(e, t, n) : Uint8Array.prototype
        .lastIndexOf.call(e, t, n) : _(e, [t], n, r, i);
      throw new TypeError("val must be string, number or Buffer");
    }

    function _(e, t, n, r, i) {
      function o(e, t) {
        return 1 === a ? e[t] : e.readUInt16BE(t * a);
      }
      var a = 1,
        s = e.length,
        c = t.length;
      if (void 0 !== r && (r = String(r).toLowerCase(), "ucs2" === r || "ucs-2" === r || "utf16le" === r ||
          "utf-16le" === r)) {
        if (e.length < 2 || t.length < 2) return -1;
        a = 2, s /= 2, c /= 2, n /= 2;
      }
      var u;
      if (i) {
        var l = -1;
        for (u = n; u < s; u++)
          if (o(e, u) === o(t, l === -1 ? 0 : u - l)) {
            if (l === -1 && (l = u), u - l + 1 === c) return l * a;
          } else l !== -1 && (u -= u - l), l = -1;
      } else
        for (n + c > s && (n = s - c), u = n; u >= 0; u--) {
          for (var d = !0, f = 0; f < c; f++)
            if (o(e, u + f) !== o(t, f)) {
              d = !1;
              break;
            }
          if (d) return u;
        }
      return -1;
    }

    function $(e, t, n, r) {
      n = Number(n) || 0;
      var i = e.length - n;
      r ? (r = Number(r), r > i && (r = i)) : r = i;
      var o = t.length;
      if (o % 2 !== 0) throw new TypeError("Invalid hex string");
      r > o / 2 && (r = o / 2);
      for (var a = 0; a < r; ++a) {
        var s = parseInt(t.substr(2 * a, 2), 16);
        if (isNaN(s)) return a;
        e[n + a] = s;
      }
      return a;
    }

    function w(e, t, n, r) {
      return K(G(t, e.length - n), e, n, r);
    }

    function T(e, t, n, r) {
      return K(V(t), e, n, r);
    }

    function C(e, t, n, r) {
      return T(e, t, n, r);
    }

    function x(e, t, n, r) {
      return K(Y(t), e, n, r);
    }

    function S(e, t, n, r) {
      return K(W(t, e.length - n), e, n, r);
    }

    function A(e, t, n) {
      return 0 === t && n === e.length ? Q.fromByteArray(e) : Q.fromByteArray(e.slice(t, n));
    }

    function M(e, t, n) {
      n = Math.min(e.length, n);
      for (var r = [], i = t; i < n;) {
        var o = e[i],
          a = null,
          s = o > 239 ? 4 : o > 223 ? 3 : o > 191 ? 2 : 1;
        if (i + s <= n) {
          var c, u, l, d;
          switch (s) {
            case 1:
              o < 128 && (a = o);
              break;
            case 2:
              c = e[i + 1], 128 === (192 & c) && (d = (31 & o) << 6 | 63 & c, d > 127 && (a = d));
              break;
            case 3:
              c = e[i + 1], u = e[i + 2], 128 === (192 & c) && 128 === (192 & u) && (d = (15 & o) << 12 | (
                63 & c) << 6 | 63 & u, d > 2047 && (d < 55296 || d > 57343) && (a = d));
              break;
            case 4:
              c = e[i + 1], u = e[i + 2], l = e[i + 3], 128 === (192 & c) && 128 === (192 & u) && 128 === (
                192 & l) && (d = (15 & o) << 18 | (63 & c) << 12 | (63 & u) << 6 | 63 & l, d > 65535 &&
                d < 1114112 && (a = d));
          }
        }
        null === a ? (a = 65533, s = 1) : a > 65535 && (a -= 65536, r.push(a >>> 10 & 1023 | 55296), a =
          56320 | 1023 & a), r.push(a), i += s;
      }
      return k(r);
    }

    function k(e) {
      var t = e.length;
      if (t <= ee) return String.fromCharCode.apply(String, e);
      for (var n = "", r = 0; r < t;) n += String.fromCharCode.apply(String, e.slice(r, r += ee));
      return n;
    }

    function N(e, t, n) {
      var r = "";
      n = Math.min(e.length, n);
      for (var i = t; i < n; ++i) r += String.fromCharCode(127 & e[i]);
      return r;
    }

    function I(e, t, n) {
      var r = "";
      n = Math.min(e.length, n);
      for (var i = t; i < n; ++i) r += String.fromCharCode(e[i]);
      return r;
    }

    function O(e, t, n) {
      var r = e.length;
      (!t || t < 0) && (t = 0), (!n || n < 0 || n > r) && (n = r);
      for (var i = "", o = t; o < n; ++o) i += q(e[o]);
      return i;
    }

    function D(e, t, n) {
      for (var r = e.slice(t, n), i = "", o = 0; o < r.length; o += 2) i += String.fromCharCode(r[o] + 256 *
        r[o + 1]);
      return i;
    }

    function R(e, t, n) {
      if (e % 1 !== 0 || e < 0) throw new RangeError("offset is not uint");
      if (e + t > n) throw new RangeError("Trying to access beyond buffer length");
    }

    function P(e, t, n, r, i, o) {
      if (!a.isBuffer(e)) throw new TypeError('"buffer" argument must be a Buffer instance');
      if (t > i || t < o) throw new RangeError('"value" argument is out of bounds');
      if (n + r > e.length) throw new RangeError("Index out of range");
    }

    function L(e, t, n, r) {
      t < 0 && (t = 65535 + t + 1);
      for (var i = 0, o = Math.min(e.length - n, 2); i < o; ++i) e[n + i] = (t & 255 << 8 * (r ? i : 1 -
        i)) >>> 8 * (r ? i : 1 - i);
    }

    function U(e, t, n, r) {
      t < 0 && (t = 4294967295 + t + 1);
      for (var i = 0, o = Math.min(e.length - n, 4); i < o; ++i) e[n + i] = t >>> 8 * (r ? i : 3 - i) & 255;
    }

    function F(e, t, n, r, i, o) {
      if (n + r > e.length) throw new RangeError("Index out of range");
      if (n < 0) throw new RangeError("Index out of range");
    }

    function j(e, t, n, r, i) {
      return i || F(e, t, n, 4, 3.4028234663852886e38, -3.4028234663852886e38), J.write(e, t, n, r, 23, 4),
        n + 4;
    }

    function H(e, t, n, r, i) {
      return i || F(e, t, n, 8, 1.7976931348623157e308, -1.7976931348623157e308), J.write(e, t, n, r, 52,
        8), n + 8;
    }

    function B(e) {
      if (e = z(e).replace(te, ""), e.length < 2) return "";
      for (; e.length % 4 !== 0;) e += "=";
      return e;
    }

    function z(e) {
      return e.trim ? e.trim() : e.replace(/^\s+|\s+$/g, "");
    }

    function q(e) {
      return e < 16 ? "0" + e.toString(16) : e.toString(16);
    }

    function G(e, t) {
      t = t || 1 / 0;
      for (var n, r = e.length, i = null, o = [], a = 0; a < r; ++a) {
        if (n = e.charCodeAt(a), n > 55295 && n < 57344) {
          if (!i) {
            if (n > 56319) {
              (t -= 3) > -1 && o.push(239, 191, 189);
              continue;
            }
            if (a + 1 === r) {
              (t -= 3) > -1 && o.push(239, 191, 189);
              continue;
            }
            i = n;
            continue;
          }
          if (n < 56320) {
            (t -= 3) > -1 && o.push(239, 191, 189), i = n;
            continue;
          }
          n = (i - 55296 << 10 | n - 56320) + 65536;
        } else i && (t -= 3) > -1 && o.push(239, 191, 189);
        if (i = null, n < 128) {
          if ((t -= 1) < 0) break;
          o.push(n);
        } else if (n < 2048) {
          if ((t -= 2) < 0) break;
          o.push(n >> 6 | 192, 63 & n | 128);
        } else if (n < 65536) {
          if ((t -= 3) < 0) break;
          o.push(n >> 12 | 224, n >> 6 & 63 | 128, 63 & n | 128);
        } else {
          if (!(n < 1114112)) throw new Error("Invalid code point");
          if ((t -= 4) < 0) break;
          o.push(n >> 18 | 240, n >> 12 & 63 | 128, n >> 6 & 63 | 128, 63 & n | 128);
        }
      }
      return o;
    }

    function V(e) {
      for (var t = [], n = 0; n < e.length; ++n) t.push(255 & e.charCodeAt(n));
      return t;
    }

    function W(e, t) {
      for (var n, r, i, o = [], a = 0; a < e.length && !((t -= 2) < 0); ++a) n = e.charCodeAt(a), r = n >>
        8, i = n % 256, o.push(i), o.push(r);
      return o;
    }

    function Y(e) {
      return Q.toByteArray(B(e));
    }

    function K(e, t, n, r) {
      for (var i = 0; i < r && !(i + n >= t.length || i >= e.length); ++i) t[i + n] = e[i];
      return i;
    }

    function X(e) {
      return e !== e;
    }
    var Q = require(163),
      J = require(258),
      Z = require(166);
    exports.Buffer = a, exports.SlowBuffer = v, exports.INSPECT_MAX_BYTES = 50, a.TYPED_ARRAY_SUPPORT =
      void 0 !== e.TYPED_ARRAY_SUPPORT ? e.TYPED_ARRAY_SUPPORT : r(), exports.kMaxLength = i(), a.poolSize =
      8192, a._augment = function(e) {
        return e.__proto__ = a.prototype, e;
      }, a.from = function(e, t, n) {
        return s(null, e, t, n);
      }, a.TYPED_ARRAY_SUPPORT && (a.prototype.__proto__ = Uint8Array.prototype, a.__proto__ = Uint8Array,
        "undefined" != typeof Symbol && Symbol.species && a[Symbol.species] === a && Object.defineProperty(
          a, Symbol.species, {
            value: null,
            configurable: !0
          })), a.alloc = function(e, t, n) {
        return u(null, e, t, n);
      }, a.allocUnsafe = function(e) {
        return l(null, e);
      }, a.allocUnsafeSlow = function(e) {
        return l(null, e);
      }, a.isBuffer = function(e) {
        return !(null == e || !e._isBuffer);
      }, a.compare = function(e, t) {
        if (!a.isBuffer(e) || !a.isBuffer(t)) throw new TypeError("Arguments must be Buffers");
        if (e === t) return 0;
        for (var n = e.length, r = t.length, i = 0, o = Math.min(n, r); i < o; ++i)
          if (e[i] !== t[i]) {
            n = e[i], r = t[i];
            break;
          }
        return n < r ? -1 : r < n ? 1 : 0;
      }, a.isEncoding = function(e) {
        switch (String(e).toLowerCase()) {
          case "hex":
          case "utf8":
          case "utf-8":
          case "ascii":
          case "latin1":
          case "binary":
          case "base64":
          case "ucs2":
          case "ucs-2":
          case "utf16le":
          case "utf-16le":
            return !0;
          default:
            return !1;
        }
      }, a.concat = function(e, t) {
        if (!Z(e)) throw new TypeError('"list" argument must be an Array of Buffers');
        if (0 === e.length) return a.alloc(0);
        var n;
        if (void 0 === t)
          for (t = 0, n = 0; n < e.length; ++n) t += e[n].length;
        var r = a.allocUnsafe(t),
          i = 0;
        for (n = 0; n < e.length; ++n) {
          var o = e[n];
          if (!a.isBuffer(o)) throw new TypeError('"list" argument must be an Array of Buffers');
          o.copy(r, i), i += o.length;
        }
        return r;
      }, a.byteLength = g, a.prototype._isBuffer = !0, a.prototype.swap16 = function() {
        var e = this.length;
        if (e % 2 !== 0) throw new RangeError("Buffer size must be a multiple of 16-bits");
        for (var t = 0; t < e; t += 2) b(this, t, t + 1);
        return this;
      }, a.prototype.swap32 = function() {
        var e = this.length;
        if (e % 4 !== 0) throw new RangeError("Buffer size must be a multiple of 32-bits");
        for (var t = 0; t < e; t += 4) b(this, t, t + 3), b(this, t + 1, t + 2);
        return this;
      }, a.prototype.swap64 = function() {
        var e = this.length;
        if (e % 8 !== 0) throw new RangeError("Buffer size must be a multiple of 64-bits");
        for (var t = 0; t < e; t += 8) b(this, t, t + 7), b(this, t + 1, t + 6), b(this, t + 2, t + 5), b(
          this, t + 3, t + 4);
        return this;
      }, a.prototype.toString = function() {
        var e = 0 | this.length;
        return 0 === e ? "" : 0 === arguments.length ? M(this, 0, e) : y.apply(this, arguments);
      }, a.prototype.equals = function(e) {
        if (!a.isBuffer(e)) throw new TypeError("Argument must be a Buffer");
        return this === e || 0 === a.compare(this, e);
      }, a.prototype.inspect = function() {
        var e = "",
          n = exports.INSPECT_MAX_BYTES;
        return this.length > 0 && (e = this.toString("hex", 0, n).match(/.{2}/g).join(" "), this.length >
          n && (e += " ... ")), "<Buffer " + e + ">";
      }, a.prototype.compare = function(e, t, n, r, i) {
        if (!a.isBuffer(e)) throw new TypeError("Argument must be a Buffer");
        if (void 0 === t && (t = 0), void 0 === n && (n = e ? e.length : 0), void 0 === r && (r = 0),
          void 0 === i && (i = this.length), t < 0 || n > e.length || r < 0 || i > this.length)
        throw new RangeError("out of range index");
        if (r >= i && t >= n) return 0;
        if (r >= i) return -1;
        if (t >= n) return 1;
        if (t >>>= 0, n >>>= 0, r >>>= 0, i >>>= 0, this === e) return 0;
        for (var o = i - r, s = n - t, c = Math.min(o, s), u = this.slice(r, i), l = e.slice(t, n), d =
          0; d < c; ++d)
          if (u[d] !== l[d]) {
            o = u[d], s = l[d];
            break;
          }
        return o < s ? -1 : s < o ? 1 : 0;
      }, a.prototype.includes = function(e, t, n) {
        return this.indexOf(e, t, n) !== -1;
      }, a.prototype.indexOf = function(e, t, n) {
        return E(this, e, t, n, !0);
      }, a.prototype.lastIndexOf = function(e, t, n) {
        return E(this, e, t, n, !1);
      }, a.prototype.write = function(e, t, n, r) {
        if (void 0 === t) r = "utf8", n = this.length, t = 0;
        else if (void 0 === n && "string" == typeof t) r = t, n = this.length, t = 0;
        else {
          if (!isFinite(t)) throw new Error(
            "Buffer.write(string, encoding, offset[, length]) is no longer supported");
          t |= 0, isFinite(n) ? (n |= 0, void 0 === r && (r = "utf8")) : (r = n, n = void 0);
        }
        var i = this.length - t;
        if ((void 0 === n || n > i) && (n = i), e.length > 0 && (n < 0 || t < 0) || t > this.length)
        throw new RangeError("Attempt to write outside buffer bounds");
        r || (r = "utf8");
        for (var o = !1;;) switch (r) {
          case "hex":
            return $(this, e, t, n);
          case "utf8":
          case "utf-8":
            return w(this, e, t, n);
          case "ascii":
            return T(this, e, t, n);
          case "latin1":
          case "binary":
            return C(this, e, t, n);
          case "base64":
            return x(this, e, t, n);
          case "ucs2":
          case "ucs-2":
          case "utf16le":
          case "utf-16le":
            return S(this, e, t, n);
          default:
            if (o) throw new TypeError("Unknown encoding: " + r);
            r = ("" + r).toLowerCase(), o = !0;
        }
      }, a.prototype.toJSON = function() {
        return {
          type: "Buffer",
          data: Array.prototype.slice.call(this._arr || this, 0)
        };
      };
    var ee = 4096;
    a.prototype.slice = function(e, t) {
      var n = this.length;
      e = ~~e, t = void 0 === t ? n : ~~t, e < 0 ? (e += n, e < 0 && (e = 0)) : e > n && (e = n), t < 0 ?
        (t += n, t < 0 && (t = 0)) : t > n && (t = n), t < e && (t = e);
      var r;
      if (a.TYPED_ARRAY_SUPPORT) r = this.subarray(e, t), r.__proto__ = a.prototype;
      else {
        var i = t - e;
        r = new a(i, void 0);
        for (var o = 0; o < i; ++o) r[o] = this[o + e];
      }
      return r;
    }, a.prototype.readUIntLE = function(e, t, n) {
      e |= 0, t |= 0, n || R(e, t, this.length);
      for (var r = this[e], i = 1, o = 0; ++o < t && (i *= 256);) r += this[e + o] * i;
      return r;
    }, a.prototype.readUIntBE = function(e, t, n) {
      e |= 0, t |= 0, n || R(e, t, this.length);
      for (var r = this[e + --t], i = 1; t > 0 && (i *= 256);) r += this[e + --t] * i;
      return r;
    }, a.prototype.readUInt8 = function(e, t) {
      return t || R(e, 1, this.length), this[e];
    }, a.prototype.readUInt16LE = function(e, t) {
      return t || R(e, 2, this.length), this[e] | this[e + 1] << 8;
    }, a.prototype.readUInt16BE = function(e, t) {
      return t || R(e, 2, this.length), this[e] << 8 | this[e + 1];
    }, a.prototype.readUInt32LE = function(e, t) {
      return t || R(e, 4, this.length), (this[e] | this[e + 1] << 8 | this[e + 2] << 16) + 16777216 *
        this[e + 3];
    }, a.prototype.readUInt32BE = function(e, t) {
      return t || R(e, 4, this.length), 16777216 * this[e] + (this[e + 1] << 16 | this[e + 2] << 8 | this[
        e + 3]);
    }, a.prototype.readIntLE = function(e, t, n) {
      e |= 0, t |= 0, n || R(e, t, this.length);
      for (var r = this[e], i = 1, o = 0; ++o < t && (i *= 256);) r += this[e + o] * i;
      return i *= 128, r >= i && (r -= Math.pow(2, 8 * t)), r;
    }, a.prototype.readIntBE = function(e, t, n) {
      e |= 0, t |= 0, n || R(e, t, this.length);
      for (var r = t, i = 1, o = this[e + --r]; r > 0 && (i *= 256);) o += this[e + --r] * i;
      return i *= 128, o >= i && (o -= Math.pow(2, 8 * t)), o;
    }, a.prototype.readInt8 = function(e, t) {
      return t || R(e, 1, this.length), 128 & this[e] ? (255 - this[e] + 1) * -1 : this[e];
    }, a.prototype.readInt16LE = function(e, t) {
      t || R(e, 2, this.length);
      var n = this[e] | this[e + 1] << 8;
      return 32768 & n ? 4294901760 | n : n;
    }, a.prototype.readInt16BE = function(e, t) {
      t || R(e, 2, this.length);
      var n = this[e + 1] | this[e] << 8;
      return 32768 & n ? 4294901760 | n : n;
    }, a.prototype.readInt32LE = function(e, t) {
      return t || R(e, 4, this.length), this[e] | this[e + 1] << 8 | this[e + 2] << 16 | this[e + 3] <<
      24;
    }, a.prototype.readInt32BE = function(e, t) {
      return t || R(e, 4, this.length), this[e] << 24 | this[e + 1] << 16 | this[e + 2] << 8 | this[e +
      3];
    }, a.prototype.readFloatLE = function(e, t) {
      return t || R(e, 4, this.length), J.read(this, e, !0, 23, 4);
    }, a.prototype.readFloatBE = function(e, t) {
      return t || R(e, 4, this.length), J.read(this, e, !1, 23, 4);
    }, a.prototype.readDoubleLE = function(e, t) {
      return t || R(e, 8, this.length), J.read(this, e, !0, 52, 8);
    }, a.prototype.readDoubleBE = function(e, t) {
      return t || R(e, 8, this.length), J.read(this, e, !1, 52, 8);
    }, a.prototype.writeUIntLE = function(e, t, n, r) {
      if (e = +e, t |= 0, n |= 0, !r) {
        var i = Math.pow(2, 8 * n) - 1;
        P(this, e, t, n, i, 0);
      }
      var o = 1,
        a = 0;
      for (this[t] = 255 & e; ++a < n && (o *= 256);) this[t + a] = e / o & 255;
      return t + n;
    }, a.prototype.writeUIntBE = function(e, t, n, r) {
      if (e = +e, t |= 0, n |= 0, !r) {
        var i = Math.pow(2, 8 * n) - 1;
        P(this, e, t, n, i, 0);
      }
      var o = n - 1,
        a = 1;
      for (this[t + o] = 255 & e; --o >= 0 && (a *= 256);) this[t + o] = e / a & 255;
      return t + n;
    }, a.prototype.writeUInt8 = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 1, 255, 0), a.TYPED_ARRAY_SUPPORT || (e = Math.floor(e)),
        this[t] = 255 & e, t + 1;
    }, a.prototype.writeUInt16LE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 2, 65535, 0), a.TYPED_ARRAY_SUPPORT ? (this[t] = 255 & e,
        this[t + 1] = e >>> 8) : L(this, e, t, !0), t + 2;
    }, a.prototype.writeUInt16BE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 2, 65535, 0), a.TYPED_ARRAY_SUPPORT ? (this[t] = e >>> 8,
        this[t + 1] = 255 & e) : L(this, e, t, !1), t + 2;
    }, a.prototype.writeUInt32LE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 4, 4294967295, 0), a.TYPED_ARRAY_SUPPORT ? (this[t + 3] =
          e >>> 24, this[t + 2] = e >>> 16, this[t + 1] = e >>> 8, this[t] = 255 & e) : U(this, e, t, !0),
        t + 4;
    }, a.prototype.writeUInt32BE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 4, 4294967295, 0), a.TYPED_ARRAY_SUPPORT ? (this[t] =
        e >>> 24, this[t + 1] = e >>> 16, this[t + 2] = e >>> 8, this[t + 3] = 255 & e) : U(this, e, t,
        !1), t + 4;
    }, a.prototype.writeIntLE = function(e, t, n, r) {
      if (e = +e, t |= 0, !r) {
        var i = Math.pow(2, 8 * n - 1);
        P(this, e, t, n, i - 1, -i);
      }
      var o = 0,
        a = 1,
        s = 0;
      for (this[t] = 255 & e; ++o < n && (a *= 256);) e < 0 && 0 === s && 0 !== this[t + o - 1] && (s =
        1), this[t + o] = (e / a >> 0) - s & 255;
      return t + n;
    }, a.prototype.writeIntBE = function(e, t, n, r) {
      if (e = +e, t |= 0, !r) {
        var i = Math.pow(2, 8 * n - 1);
        P(this, e, t, n, i - 1, -i);
      }
      var o = n - 1,
        a = 1,
        s = 0;
      for (this[t + o] = 255 & e; --o >= 0 && (a *= 256);) e < 0 && 0 === s && 0 !== this[t + o + 1] && (
        s = 1), this[t + o] = (e / a >> 0) - s & 255;
      return t + n;
    }, a.prototype.writeInt8 = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 1, 127, -128), a.TYPED_ARRAY_SUPPORT || (e = Math.floor(
        e)), e < 0 && (e = 255 + e + 1), this[t] = 255 & e, t + 1;
    }, a.prototype.writeInt16LE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 2, 32767, -32768), a.TYPED_ARRAY_SUPPORT ? (this[t] =
        255 & e, this[t + 1] = e >>> 8) : L(this, e, t, !0), t + 2;
    }, a.prototype.writeInt16BE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 2, 32767, -32768), a.TYPED_ARRAY_SUPPORT ? (this[t] =
        e >>> 8, this[t + 1] = 255 & e) : L(this, e, t, !1), t + 2;
    }, a.prototype.writeInt32LE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 4, 2147483647, -2147483648), a.TYPED_ARRAY_SUPPORT ? (
        this[t] = 255 & e, this[t + 1] = e >>> 8, this[t + 2] = e >>> 16, this[t + 3] = e >>> 24) : U(
        this, e, t, !0), t + 4;
    }, a.prototype.writeInt32BE = function(e, t, n) {
      return e = +e, t |= 0, n || P(this, e, t, 4, 2147483647, -2147483648), e < 0 && (e = 4294967295 +
        e + 1), a.TYPED_ARRAY_SUPPORT ? (this[t] = e >>> 24, this[t + 1] = e >>> 16, this[t + 2] = e >>>
        8, this[t + 3] = 255 & e) : U(this, e, t, !1), t + 4;
    }, a.prototype.writeFloatLE = function(e, t, n) {
      return j(this, e, t, !0, n);
    }, a.prototype.writeFloatBE = function(e, t, n) {
      return j(this, e, t, !1, n);
    }, a.prototype.writeDoubleLE = function(e, t, n) {
      return H(this, e, t, !0, n);
    }, a.prototype.writeDoubleBE = function(e, t, n) {
      return H(this, e, t, !1, n);
    }, a.prototype.copy = function(e, t, n, r) {
      if (n || (n = 0), r || 0 === r || (r = this.length), t >= e.length && (t = e.length), t || (t = 0),
        r > 0 && r < n && (r = n), r === n) return 0;
      if (0 === e.length || 0 === this.length) return 0;
      if (t < 0) throw new RangeError("targetStart out of bounds");
      if (n < 0 || n >= this.length) throw new RangeError("sourceStart out of bounds");
      if (r < 0) throw new RangeError("sourceEnd out of bounds");
      r > this.length && (r = this.length), e.length - t < r - n && (r = e.length - t + n);
      var i,
        o = r - n;
      if (this === e && n < t && t < r)
        for (i = o - 1; i >= 0; --i) e[i + t] = this[i + n];
      else if (o < 1e3 || !a.TYPED_ARRAY_SUPPORT)
        for (i = 0; i < o; ++i) e[i + t] = this[i + n];
      else Uint8Array.prototype.set.call(e, this.subarray(n, n + o), t);
      return o;
    }, a.prototype.fill = function(e, t, n, r) {
      if ("string" == typeof e) {
        if ("string" == typeof t ? (r = t, t = 0, n = this.length) : "string" == typeof n && (r = n, n =
            this.length), 1 === e.length) {
          var i = e.charCodeAt(0);
          i < 256 && (e = i);
        }
        if (void 0 !== r && "string" != typeof r) throw new TypeError("encoding must be a string");
        if ("string" == typeof r && !a.isEncoding(r)) throw new TypeError("Unknown encoding: " + r);
      } else "number" == typeof e && (e &= 255);
      if (t < 0 || this.length < t || this.length < n) throw new RangeError("Out of range index");
      if (n <= t) return this;
      t >>>= 0, n = void 0 === n ? this.length : n >>> 0, e || (e = 0);
      var o;
      if ("number" == typeof e)
        for (o = t; o < n; ++o) this[o] = e;
      else {
        var s = a.isBuffer(e) ? e : G(new a(e, r).toString()),
          c = s.length;
        for (o = 0; o < n - t; ++o) this[o + t] = s[o % c];
      }
      return this;
    };
    var te = /[^+\/0-9A-Za-z-_]/g;
  }).call(exports, function() {
    return this;
  }());
}
