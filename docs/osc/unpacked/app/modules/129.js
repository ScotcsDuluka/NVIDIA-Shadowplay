// ─────────────────────────────────────────────────────────────
// APP MODULE 129
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e) {
      return Math.abs(e = Math.round(e)) >= 1e21 ? e.toLocaleString("en").replace(/,/g, "") : e.toString(10)
    }

    function n(e, t) {
      if ((n = (e = t ? e.toExponential(t - 1) : e.toExponential()).indexOf("e")) < 0) return null;
      var n, i = e.slice(0, n);
      return [i.length > 1 ? i[0] + i.slice(2) : i, +e.slice(n + 1)]
    }

    function i(e) {
      return e = n(Math.abs(e)), e ? e[1] : NaN
    }

    function o(e, t) {
      return function(n, i) {
        for (var o = n.length, r = [], a = 0, l = e[0], s = 0; o > 0 && l > 0 && (s + l + 1 > i && (l = Math.max(1,
            i - s)), r.push(n.substring(o -= l, o + l)), !((s += l + 1) > i));) l = e[a = (a + 1) % e.length];
        return r.reverse().join(t)
      }
    }

    function r(e) {
      return function(t) {
        return t.replace(/[0-9]/g, function(t) {
          return e[+t]
        })
      }
    }

    function a(e) {
      if (!(t = b.exec(e))) throw new Error("invalid format: " + e);
      var t;
      return new l({
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
      })
    }

    function l(e) {
      this.fill = void 0 === e.fill ? " " : e.fill + "", this.align = void 0 === e.align ? ">" : e.align + "", this
        .sign = void 0 === e.sign ? "-" : e.sign + "", this.symbol = void 0 === e.symbol ? "" : e.symbol + "", this
        .zero = !!e.zero, this.width = void 0 === e.width ? void 0 : +e.width, this.comma = !!e.comma, this
        .precision = void 0 === e.precision ? void 0 : +e.precision, this.trim = !!e.trim, this.type = void 0 === e
        .type ? "" : e.type + ""
    }

    function s(e) {
      e: for (var t, n = e.length, i = 1, o = -1; i < n; ++i) switch (e[i]) {
        case ".":
          o = t = i;
          break;
        case "0":
          0 === o && (o = i), t = i;
          break;
        default:
          if (!+e[i]) break e;
          o > 0 && (o = 0)
      }
      return o > 0 ? e.slice(0, o) + e.slice(t + 1) : e
    }

    function d(e, t) {
      var i = n(e, t);
      if (!i) return e + "";
      var o = i[0],
        r = i[1],
        a = r - (x = 3 * Math.max(-8, Math.min(8, Math.floor(r / 3)))) + 1,
        l = o.length;
      return a === l ? o : a > l ? o + new Array(a - l + 1).join("0") : a > 0 ? o.slice(0, a) + "." + o.slice(a) :
        "0." + new Array(1 - a).join("0") + n(e, Math.max(0, t + a - 1))[0]
    }

    function c(e, t) {
      var i = n(e, t);
      if (!i) return e + "";
      var o = i[0],
        r = i[1];
      return r < 0 ? "0." + new Array(-r).join("0") + o : o.length > r + 1 ? o.slice(0, r + 1) + "." + o.slice(r +
        1) : o + new Array(r - o.length + 2).join("0")
    }

    function u(e) {
      return e
    }

    function f(e) {
      function t(e) {
        function t(e) {
          var t, r, a, d = _,
            c = T;
          if ("c" === k) c = C(e) + c, e = "";
          else {
            e = +e;
            var g = e < 0 || 1 / e < 0;
            if (e = isNaN(e) ? h : C(Math.abs(e), w), E && (e = s(e)), g && 0 === +e && "+" !== o && (g = !1), d = (
                g ? "(" === o ? o : p : "-" === o || "(" === o ? "" : o) + d, c = ("s" === k ? S[8 + x / 3] : "") +
              c + (g && "(" === o ? ")" : ""), O)
              for (t = -1, r = e.length; ++t < r;)
                if (a = e.charCodeAt(t), 48 > a || a > 57) {
                  c = (46 === a ? f + e.slice(t + 1) : e.slice(t)) + c, e = e.slice(0, t);
                  break
                }
          }
          v && !u && (e = l(e, 1 / 0));
          var y = d.length + e.length + c.length,
            A = y < b ? new Array(b - y + 1).join(n) : "";
          switch (v && u && (e = l(A + e, A.length ? b - c.length : 1 / 0), A = ""), i) {
            case "<":
              e = d + e + c + A;
              break;
            case "=":
              e = d + A + e + c;
              break;
            case "^":
              e = A.slice(0, y = A.length >> 1) + d + e + c + A.slice(y);
              break;
            default:
              e = A + d + e + c
          }
          return m(e)
        }
        e = a(e);
        var n = e.fill,
          i = e.align,
          o = e.sign,
          r = e.symbol,
          u = e.zero,
          b = e.width,
          v = e.comma,
          w = e.precision,
          E = e.trim,
          k = e.type;
        "n" === k ? (v = !0, k = "g") : y[k] || (void 0 === w && (w = 12), E = !0, k = "g"), (u || "0" === n &&
          "=" === i) && (u = !0, n = "0", i = "=");
        var _ = "$" === r ? d : "#" === r && /[boxX]/.test(k) ? "0" + k.toLowerCase() : "",
          T = "$" === r ? c : /[%p]/.test(k) ? g : "",
          C = y[k],
          O = /[defgprs%]/.test(k);
        return w = void 0 === w ? 6 : /[gprs]/.test(k) ? Math.max(1, Math.min(21, w)) : Math.max(0, Math.min(20, w)),
          t.toString = function() {
            return e + ""
          }, t
      }

      function n(e, n) {
        var o = t((e = a(e), e.type = "f", e)),
          r = 3 * Math.max(-8, Math.min(8, Math.floor(i(n) / 3))),
          l = Math.pow(10, -r),
          s = S[8 + r / 3];
        return function(e) {
          return o(l * e) + s
        }
      }
      var l = void 0 === e.grouping || void 0 === e.thousands ? u : o(w.call(e.grouping, Number), e.thousands + ""),
        d = void 0 === e.currency ? "" : e.currency[0] + "",
        c = void 0 === e.currency ? "" : e.currency[1] + "",
        f = void 0 === e.decimal ? "." : e.decimal + "",
        m = void 0 === e.numerals ? u : r(w.call(e.numerals, String)),
        g = void 0 === e.percent ? "%" : e.percent + "",
        p = void 0 === e.minus ? "-" : e.minus + "",
        h = void 0 === e.nan ? "NaN" : e.nan + "";
      return {
        format: t,
        formatPrefix: n
      }
    }

    function m(t) {
      return v = f(t), e.format = v.format, e.formatPrefix = v.formatPrefix, v
    }

    function g(e) {
      return Math.max(0, -i(Math.abs(e)))
    }

    function p(e, t) {
      return Math.max(0, 3 * Math.max(-8, Math.min(8, Math.floor(i(t) / 3))) - i(Math.abs(e)))
    }

    function h(e, t) {
      return e = Math.abs(e), t = Math.abs(t) - e, Math.max(0, i(t) - i(e)) + 1
    }
    var b = /^(?:(.)?([<>=^]))?([+\-( ])?([$#])?(0)?(\d+)?(,)?(\.\d+)?(~)?([a-z%])?$/i;
    a.prototype = l.prototype, l.prototype.toString = function() {
      return this.fill + this.align + this.sign + this.symbol + (this.zero ? "0" : "") + (void 0 === this.width ?
        "" : Math.max(1, 0 | this.width)) + (this.comma ? "," : "") + (void 0 === this.precision ? "" : "." + Math
        .max(0, 0 | this.precision)) + (this.trim ? "~" : "") + this.type
    };
    var x, v, y = {
        "%": function(e, t) {
          return (100 * e).toFixed(t)
        },
        b: function(e) {
          return Math.round(e).toString(2)
        },
        c: function(e) {
          return e + ""
        },
        d: t,
        e: function(e, t) {
          return e.toExponential(t)
        },
        f: function(e, t) {
          return e.toFixed(t)
        },
        g: function(e, t) {
          return e.toPrecision(t)
        },
        o: function(e) {
          return Math.round(e).toString(8)
        },
        p: function(e, t) {
          return c(100 * e, t)
        },
        r: c,
        s: d,
        X: function(e) {
          return Math.round(e).toString(16).toUpperCase()
        },
        x: function(e) {
          return Math.round(e).toString(16)
        }
      },
      w = Array.prototype.map,
      S = ["y", "z", "a", "f", "p", "n", "µ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y"];
    m({
        decimal: ".",
        thousands: ",",
        grouping: [3],
        currency: ["$", ""],
        minus: "-"
      }), e.FormatSpecifier = l, e.formatDefaultLocale = m, e.formatLocale = f, e.formatSpecifier = a, e
      .precisionFixed = g, e.precisionPrefix = p, e.precisionRound = h, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
