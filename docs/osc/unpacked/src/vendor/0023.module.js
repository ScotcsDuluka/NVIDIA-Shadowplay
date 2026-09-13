// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 23
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  (function(e) {
    function r(e, n) {
      var r = "b" + exports.packets[e.type] + e.data.data;
      return n(r);
    }

    function i(e, n, r) {
      if (!n) return exports.encodeBase64Packet(e, r);
      var i = e.data,
        o = new Uint8Array(i),
        a = new Uint8Array(1 + i.byteLength);
      a[0] = g[e.type];
      for (var s = 0; s < o.length; s++) a[s + 1] = o[s];
      return r(a.buffer);
    }

    function o(e, n, r) {
      if (!n) return exports.encodeBase64Packet(e, r);
      var i = new FileReader();
      return i.onload = function() {
        e.data = i.result, exports.encodePacket(e, n, !0, r);
      }, i.readAsArrayBuffer(e.data);
    }

    function a(e, n, r) {
      if (!n) return exports.encodeBase64Packet(e, r);
      if (v) return o(e, n, r);
      var i = new Uint8Array(1);
      i[0] = g[e.type];
      var a = new E([i.buffer, e.data]);
      return r(a);
    }

    function s(e, t, n) {
      for (var r = new Array(e.length), i = f(e.length, n), o = function(e, n, i) {
          t(n, function(t, n) {
            r[e] = n, i(t, r);
          });
        }, a = 0; a < e.length; a++) o(a, e[a], i);
    }
    var c = require(278),
      u = require(111),
      l = require(150),
      d = require(269),
      f = require(268),
      h = require(288),
      p = navigator.userAgent.match(/Android/i),
      m = /PhantomJS/i.test(navigator.userAgent),
      v = p || m;
    exports.protocol = 3;
    var g = exports.packets = {
        open: 0,
        close: 1,
        ping: 2,
        pong: 3,
        message: 4,
        upgrade: 5,
        noop: 6
      },
      y = c(g),
      b = {
        type: "error",
        data: "parser error"
      },
      E = require(164);
    exports.encodePacket = function(t, n, o, s) {
      "function" == typeof n && (s = n, n = !1), "function" == typeof o && (s = o, o = null);
      var c = void 0 === t.data ? void 0 : t.data.buffer || t.data;
      if (e.ArrayBuffer && c instanceof ArrayBuffer) return i(t, n, s);
      if (E && c instanceof e.Blob) return a(t, n, s);
      if (c && c.base64) return r(t, s);
      var u = g[t.type];
      return void 0 !== t.data && (u += o ? h.encode(String(t.data)) : String(t.data)), s("" + u);
    }, exports.encodeBase64Packet = function(n, r) {
      var i = "b" + exports.packets[n.type];
      if (E && n.data instanceof E) {
        var o = new FileReader();
        return o.onload = function() {
          var e = o.result.split(",")[1];
          r(i + e);
        }, o.readAsDataURL(n.data);
      }
      var a;
      try {
        a = String.fromCharCode.apply(null, new Uint8Array(n.data));
      } catch (e) {
        for (var s = new Uint8Array(n.data), c = new Array(s.length), u = 0; u < s.length; u++) c[u] = s[
          u];
        a = String.fromCharCode.apply(null, c);
      }
      return i += e.btoa(a), r(i);
    }, exports.decodePacket = function(e, n, r) {
      if ("string" == typeof e || void 0 === e) {
        if ("b" == e.charAt(0)) return exports.decodeBase64Packet(e.substr(1), n);
        if (r) try {
          e = h.decode(e);
        } catch (e) {
          return b;
        }
        var i = e.charAt(0);
        return Number(i) == i && y[i] ? e.length > 1 ? {
          type: y[i],
          data: e.substring(1)
        } : {
          type: y[i]
        } : b;
      }
      var o = new Uint8Array(e),
        i = o[0],
        a = l(e, 1);
      return E && "blob" === n && (a = new E([a])), {
        type: y[i],
        data: a
      };
    }, exports.decodeBase64Packet = function(t, n) {
      var r = y[t.charAt(0)];
      if (!e.ArrayBuffer) return {
        type: r,
        data: {
          base64: !0,
          data: t.substr(1)
        }
      };
      var i = d.decode(t.substr(1));
      return "blob" === n && E && (i = new E([i])), {
        type: r,
        data: i
      };
    }, exports.encodePayload = function(e, n, r) {
      function i(e) {
        return e.length + ":" + e;
      }

      function o(e, r) {
        exports.encodePacket(e, !!a && n, !0, function(e) {
          r(null, i(e));
        });
      }
      "function" == typeof n && (r = n, n = null);
      var a = u(e);
      return n && a ? E && !v ? exports.encodePayloadAsBlob(e, r) : exports.encodePayloadAsArrayBuffer(e,
        r) : e.length ? void s(e, o, function(e, t) {
        return r(t.join(""));
      }) : r("0:");
    }, exports.decodePayload = function(e, n, r) {
      if ("string" != typeof e) return exports.decodePayloadAsBinary(e, n, r);
      "function" == typeof n && (r = n, n = null);
      var i;
      if ("" == e) return r(b, 0, 1);
      for (var o, a, s = "", c = 0, u = e.length; c < u; c++) {
        var l = e.charAt(c);
        if (":" != l) s += l;
        else {
          if ("" == s || s != (o = Number(s))) return r(b, 0, 1);
          if (a = e.substr(c + 1, o), s != a.length) return r(b, 0, 1);
          if (a.length) {
            if (i = exports.decodePacket(a, n, !0), b.type == i.type && b.data == i.data) return r(b, 0,
              1);
            var d = r(i, c + o, u);
            if (!1 === d) return;
          }
          c += o, s = "";
        }
      }
      return "" != s ? r(b, 0, 1) : void 0;
    }, exports.encodePayloadAsArrayBuffer = function(e, n) {
      function r(e, n) {
        exports.encodePacket(e, !0, !0, function(e) {
          return n(null, e);
        });
      }
      return e.length ? void s(e, r, function(e, t) {
        var r = t.reduce(function(e, t) {
            var n;
            return n = "string" == typeof t ? t.length : t.byteLength, e + n.toString().length + n +
              2;
          }, 0),
          i = new Uint8Array(r),
          o = 0;
        return t.forEach(function(e) {
          var t = "string" == typeof e,
            n = e;
          if (t) {
            for (var r = new Uint8Array(e.length), a = 0; a < e.length; a++) r[a] = e.charCodeAt(
              a);
            n = r.buffer;
          }
          t ? i[o++] = 0 : i[o++] = 1;
          for (var s = n.byteLength.toString(), a = 0; a < s.length; a++) i[o++] = parseInt(s[a]);
          i[o++] = 255;
          for (var r = new Uint8Array(n), a = 0; a < r.length; a++) i[o++] = r[a];
        }), n(i.buffer);
      }) : n(new ArrayBuffer(0));
    }, exports.encodePayloadAsBlob = function(e, n) {
      function r(e, n) {
        exports.encodePacket(e, !0, !0, function(e) {
          var t = new Uint8Array(1);
          if (t[0] = 1, "string" == typeof e) {
            for (var r = new Uint8Array(e.length), i = 0; i < e.length; i++) r[i] = e.charCodeAt(i);
            e = r.buffer, t[0] = 0;
          }
          for (var o = e instanceof ArrayBuffer ? e.byteLength : e.size, a = o.toString(), s =
              new Uint8Array(a.length + 1), i = 0; i < a.length; i++) s[i] = parseInt(a[i]);
          if (s[a.length] = 255, E) {
            var c = new E([t.buffer, s.buffer, e]);
            n(null, c);
          }
        });
      }
      s(e, r, function(e, t) {
        return n(new E(t));
      });
    }, exports.decodePayloadAsBinary = function(e, n, r) {
      "function" == typeof n && (r = n, n = null);
      for (var i = e, o = [], a = !1; i.byteLength > 0;) {
        for (var s = new Uint8Array(i), c = 0 === s[0], u = "", d = 1; 255 != s[d]; d++) {
          if (u.length > 310) {
            a = !0;
            break;
          }
          u += s[d];
        }
        if (a) return r(b, 0, 1);
        i = l(i, 2 + u.length), u = parseInt(u);
        var f = l(i, 0, u);
        if (c) try {
          f = String.fromCharCode.apply(null, new Uint8Array(f));
        } catch (e) {
          var h = new Uint8Array(f);
          f = "";
          for (var d = 0; d < h.length; d++) f += String.fromCharCode(h[d]);
        }
        o.push(f), i = l(i, u);
      }
      var p = o.length;
      o.forEach(function(e, i) {
        r(exports.decodePacket(e, n, !0), i, p);
      });
    };
  }).call(exports, function() {
    return this;
  }());
}
