// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 101
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      return Math.abs(e = Math.round(e)) >= 1e21 ? e.toLocaleString("en").replace(/,/g, "") : e.toString(
      10);
    }

    function n(e, t) {
      if ((n = (e = t ? e.toExponential(t - 1) : e.toExponential()).indexOf("e")) < 0) return null;
      var n,
        r = e.slice(0, n);
      return [r.length > 1 ? r[0] + r.slice(2) : r, +e.slice(n + 1)];
    }

    function r(e) {
      return e = n(Math.abs(e)), e ? e[1] : NaN;
    }

    function i(e, t) {
      return function(n, r) {
        for (var i = n.length, o = [], a = 0, s = e[0], c = 0; i > 0 && s > 0 && (c + s + 1 > r && (s =
            Math.max(1, r - c)), o.push(n.substring(i -= s, i + s)), !((c += s + 1) > r));) s = e[a = (a +
          1) % e.length];
        return o.reverse().join(t);
      };
    }

    function o(e) {
      return function(t) {
        return t.replace(/[0-9]/g, function(t) {
          return e[+t];
        });
      };
    }

    function a(e) {
      if (!(t = g.exec(e))) throw new Error("invalid format: " + e);
      var t;
      return new s({
        fill: t[1],
        align: t[2],
        sign: t[3],
        symbol: t[4],
        zero: t[5],
        width: t[6],
        comma: t[7],
        precision: t[8] && t[8].slice(1),
        trim: t[9],
        type: t[10]
      });
    }

    function s(e) {
      this.fill = void 0 === e.fill ? " " : e.fill + "", this.align = void 0 === e.align ? ">" : e.align +
        "", this.sign = void 0 === e.sign ? "-" : e.sign + "", this.symbol = void 0 === e.symbol ? "" : e
        .symbol + "", this.zero = !!e.zero, this.width = void 0 === e.width ? void 0 : +e.width, this
        .comma = !!e.comma, this.precision = void 0 === e.precision ? void 0 : +e.precision, this.trim = !!e
        .trim, this.type = void 0 === e.type ? "" : e.type + "";
    }

    function c(e) {
      e: for (var t, n = e.length, r = 1, i = -1; r < n; ++r) switch (e[r]) {
        case ".":
          i = t = r;
          break;
        case "0":
          0 === i && (i = r), t = r;
          break;
        default:
          if (!+e[r]) break e;
          i > 0 && (i = 0);
      }
      return i > 0 ? e.slice(0, i) + e.slice(t + 1) : e;
    }

    function u(e, t) {
      var r = n(e, t);
      if (!r) return e + "";
      var i = r[0],
        o = r[1],
        a = o - (y = 3 * Math.max(-8, Math.min(8, Math.floor(o / 3)))) + 1,
        s = i.length;
      return a === s ? i : a > s ? i + new Array(a - s + 1).join("0") : a > 0 ? i.slice(0, a) + "." + i
        .slice(a) : "0." + new Array(1 - a).join("0") + n(e, Math.max(0, t + a - 1))[0];
    }

    function l(e, t) {
      var r = n(e, t);
      if (!r) return e + "";
      var i = r[0],
        o = r[1];
      return o < 0 ? "0." + new Array(-o).join("0") + i : i.length > o + 1 ? i.slice(0, o + 1) + "." + i
        .slice(o + 1) : i + new Array(o - i.length + 2).join("0");
    }

    function d(e) {
      return e;
    }

    function f(e) {
      function t(e) {
        function t(e) {
          var t,
            o,
            a,
            u = C,
            l = x;
          if ("c" === T) l = S(e) + l, e = "";
          else {
            e = +e;
            var p = e < 0 || 1 / e < 0;
            if (e = isNaN(e) ? v : S(Math.abs(e), _), w && (e = c(e)), p && 0 === +e && "+" !== i && (p = !
                1), u = (p ? "(" === i ? i : m : "-" === i || "(" === i ? "" : i) + u, l = ("s" === T ? $[
                8 + y / 3] : "") + l + (p && "(" === i ? ")" : ""), A)
              for (t = -1, o = e.length; ++t < o;)
                if (a = e.charCodeAt(t), 48 > a || a > 57) {
                  l = (46 === a ? f + e.slice(t + 1) : e.slice(t)) + l, e = e.slice(0, t);
                  break;
                }
          }
          b && !d && (e = s(e, 1 / 0));
          var E = u.length + e.length + l.length,
            M = E < g ? new Array(g - E + 1).join(n) : "";
          switch (b && d && (e = s(M + e, M.length ? g - l.length : 1 / 0), M = ""), r) {
            case "<":
              e = u + e + l + M;
              break;
            case "=":
              e = u + M + e + l;
              break;
            case "^":
              e = M.slice(0, E = M.length >> 1) + u + e + l + M.slice(E);
              break;
            default:
              e = M + u + e + l;
          }
          return h(e);
        }
        e = a(e);
        var n = e.fill,
          r = e.align,
          i = e.sign,
          o = e.symbol,
          d = e.zero,
          g = e.width,
          b = e.comma,
          _ = e.precision,
          w = e.trim,
          T = e.type;
        "n" === T ? (b = !0, T = "g") : E[T] || (void 0 === _ && (_ = 12), w = !0, T = "g"), (d || "0" ===
          n && "=" === r) && (d = !0, n = "0", r = "=");
        var C = "$" === o ? u : "#" === o && /[boxX]/.test(T) ? "0" + T.toLowerCase() : "",
          x = "$" === o ? l : /[%p]/.test(T) ? p : "",
          S = E[T],
          A = /[defgprs%]/.test(T);
        return _ = void 0 === _ ? 6 : /[gprs]/.test(T) ? Math.max(1, Math.min(21, _)) : Math.max(0, Math
          .min(20, _)), t.toString = function() {
          return e + "";
        }, t;
      }

      function n(e, n) {
        var i = t((e = a(e), e.type = "f", e)),
          o = 3 * Math.max(-8, Math.min(8, Math.floor(r(n) / 3))),
          s = Math.pow(10, -o),
          c = $[8 + o / 3];
        return function(e) {
          return i(s * e) + c;
        };
      }
      var s = void 0 === e.grouping || void 0 === e.thousands ? d : i(_.call(e.grouping, Number), e
          .thousands + ""),
        u = void 0 === e.currency ? "" : e.currency[0] + "",
        l = void 0 === e.currency ? "" : e.currency[1] + "",
        f = void 0 === e.decimal ? "." : e.decimal + "",
        h = void 0 === e.numerals ? d : o(_.call(e.numerals, String)),
        p = void 0 === e.percent ? "%" : e.percent + "",
        m = void 0 === e.minus ? "-" : e.minus + "",
        v = void 0 === e.nan ? "NaN" : e.nan + "";
      return {
        format: t,
        formatPrefix: n
      };
    }

    function h(t) {
      return b = f(t), e.format = b.format, e.formatPrefix = b.formatPrefix, b;
    }

    function p(e) {
      return Math.max(0, -r(Math.abs(e)));
    }

    function m(e, t) {
      return Math.max(0, 3 * Math.max(-8, Math.min(8, Math.floor(r(t) / 3))) - r(Math.abs(e)));
    }

    function v(e, t) {
      return e = Math.abs(e), t = Math.abs(t) - e, Math.max(0, r(t) - r(e)) + 1;
    }
    var g = /^(?:(.)?([<>=^]))?([+\-( ])?([$#])?(0)?(\d+)?(,)?(\.\d+)?(~)?([a-z%])?$/i;
    a.prototype = s.prototype, s.prototype.toString = function() {
      return this.fill + this.align + this.sign + this.symbol + (this.zero ? "0" : "") + (void 0 === this
        .width ? "" : Math.max(1, 0 | this.width)) + (this.comma ? "," : "") + (void 0 === this
        .precision ? "" : "." + Math.max(0, 0 | this.precision)) + (this.trim ? "~" : "") + this.type;
    };
    var y,
      b,
      E = {
        "%": function(e, t) {
          return (100 * e).toFixed(t);
        },
        b: function(e) {
          return Math.round(e).toString(2);
        },
        c: function(e) {
          return e + "";
        },
        d: t,
        e: function(e, t) {
          return e.toExponential(t);
        },
        f: function(e, t) {
          return e.toFixed(t);
        },
        g: function(e, t) {
          return e.toPrecision(t);
        },
        o: function(e) {
          return Math.round(e).toString(8);
        },
        p: function(e, t) {
          return l(100 * e, t);
        },
        r: l,
        s: u,
        X: function(e) {
          return Math.round(e).toString(16).toUpperCase();
        },
        x: function(e) {
          return Math.round(e).toString(16);
        }
      },
      _ = Array.prototype.map,
      $ = ["y", "z", "a", "f", "p", "n", "µ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y"];
    h({
        decimal: ".",
        thousands: ",",
        grouping: [3],
        currency: ["$", ""],
        minus: "-"
      }), e.FormatSpecifier = s, e.formatDefaultLocale = h, e.formatLocale = f, e.formatSpecifier = a, e
      .precisionFixed = p, e.precisionPrefix = m, e.precisionRound = v, Object.defineProperty(e,
        "__esModule", {
          value: !0
        });
  });
}
