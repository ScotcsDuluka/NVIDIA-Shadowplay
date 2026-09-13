// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 47
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }

  function i(e) {
    var t,
      n,
      r,
      i,
      o,
      a = .75 * e.length,
      s = e.length,
      c = 0;
    "=" === e[e.length - 1] && (a--, "=" === e[e.length - 2] && a--);
    var u = new ArrayBuffer(a),
      l = new Uint8Array(u);
    for (t = 0; t < s; t += 4) n = f.indexOf(e[t]), r = f.indexOf(e[t + 1]), i = f.indexOf(e[t + 2]), o = f
      .indexOf(e[t + 3]), l[c++] = n << 2 | r >> 4, l[c++] = (15 & r) << 4 | i >> 2, l[c++] = (3 & i) << 6 |
      63 & o;
    return u;
  }

  function o(e) {
    var t,
      n = new Uint8Array(e),
      r = "";
    for (t = 0; t < n.length; t += 3) r += f[n[t] >> 2], r += f[(3 & n[t]) << 4 | n[t + 1] >> 4], r += f[(15 &
      n[t + 1]) << 2 | n[t + 2] >> 6], r += f[63 & n[t + 2]];
    return n.length % 3 === 2 ? r = r.substring(0, r.length - 1) + "=" : n.length % 3 === 1 && (r = r
      .substring(0, r.length - 2) + "=="), r;
  }

  function a(e, t) {
    var n = "";
    if (e && (n = e.toString()), e && ("[object ArrayBuffer]" === e.toString() || e.buffer &&
        "[object ArrayBuffer]" === e.buffer.toString())) {
      var r,
        i = m;
      e instanceof ArrayBuffer ? (r = e, i += g) : (r = e.buffer, "[object Int8Array]" === n ? i += b :
        "[object Uint8Array]" === n ? i += E : "[object Uint8ClampedArray]" === n ? i += _ :
        "[object Int16Array]" === n ? i += $ : "[object Uint16Array]" === n ? i += T :
        "[object Int32Array]" === n ? i += w : "[object Uint32Array]" === n ? i += C :
        "[object Float32Array]" === n ? i += x : "[object Float64Array]" === n ? i += S : t(new Error(
          "Failed to get type for BinaryArray"))), t(i + o(r));
    } else if ("[object Blob]" === n) {
      var a = new FileReader();
      a.onload = function() {
        var n = h + e.type + "~" + o(this.result);
        t(m + y + n);
      }, a.readAsArrayBuffer(e);
    } else try {
      t((0, u.default)(e));
    } catch (n) {
      console.error("Couldn't convert value into a JSON string: ", e), t(null, n);
    }
  }

  function s(e) {
    if (e.substring(0, v) !== m) return JSON.parse(e);
    var t,
      n = e.substring(A),
      r = e.substring(v, A);
    if (r === y && p.test(n)) {
      var o = n.match(p);
      t = o[1], n = n.substring(o[0].length);
    }
    var a = i(n);
    switch (r) {
      case g:
        return a;
      case y:
        return (0, d.default)([a], {
          type: t
        });
      case b:
        return new Int8Array(a);
      case E:
        return new Uint8Array(a);
      case _:
        return new Uint8ClampedArray(a);
      case $:
        return new Int16Array(a);
      case T:
        return new Uint16Array(a);
      case w:
        return new Int32Array(a);
      case C:
        return new Uint32Array(a);
      case x:
        return new Float32Array(a);
      case S:
        return new Float64Array(a);
      default:
        throw new Error("Unkown type: " + r);
    }
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var c = require(25),
    u = r(c),
    l = require(73),
    d = r(l),
    f = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/",
    h = "~~local_forage_type~",
    p = /^~~local_forage_type~([^~]+)~/,
    m = "__lfsc__:",
    v = m.length,
    g = "arbf",
    y = "blob",
    b = "si08",
    E = "ui08",
    _ = "uic8",
    $ = "si16",
    w = "si32",
    T = "ur16",
    C = "ui32",
    x = "fl32",
    S = "fl64",
    A = v + g.length,
    M = {
      serialize: a,
      deserialize: s,
      stringToBuffer: i,
      bufferToString: o
    };
  exports.default = M;
}
