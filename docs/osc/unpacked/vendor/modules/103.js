// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 103
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, r) {
    r(t, n(65))
  }(this, function(e, t) {
    "use strict";

    function n(e) {
      if (0 <= e.y && e.y < 100) {
        var t = new Date(-1, e.m, e.d, e.H, e.M, e.S, e.L);
        return t.setFullYear(e.y), t
      }
      return new Date(e.y, e.m, e.d, e.H, e.M, e.S, e.L)
    }

    function r(e) {
      if (0 <= e.y && e.y < 100) {
        var t = new Date(Date.UTC(-1, e.m, e.d, e.H, e.M, e.S, e.L));
        return t.setUTCFullYear(e.y), t
      }
      return new Date(Date.UTC(e.y, e.m, e.d, e.H, e.M, e.S, e.L))
    }

    function i(e, t, n) {
      return {
        y: e,
        m: t,
        d: n,
        H: 0,
        M: 0,
        S: 0,
        L: 0
      }
    }

    function o(e) {
      function o(e, t) {
        return function(n) {
          var r, i, o, a = [],
            s = -1,
            c = 0,
            u = e.length;
          for (n instanceof Date || (n = new Date(+n)); ++s < u;) 37 === e.charCodeAt(s) && (a.push(e.slice(c, s)),
            null != (i = we[r = e.charAt(++s)]) ? r = e.charAt(++s) : i = "e" === r ? " " : "0", (o = t[r]) && (
              r = o(n, i)), a.push(r), c = s + 1);
          return a.push(e.slice(c, s)), a.join("")
        }
      }

      function a(e, o) {
        return function(a) {
          var c, u, l = i(1900, void 0, 1),
            d = s(l, e, a += "", 0);
          if (d != a.length) return null;
          if ("Q" in l) return new Date(l.Q);
          if ("s" in l) return new Date(1e3 * l.s + ("L" in l ? l.L : 0));
          if (!o || "Z" in l || (l.Z = 0), "p" in l && (l.H = l.H % 12 + 12 * l.p), void 0 === l.m && (l.m = "q" in
              l ? l.q : 0), "V" in l) {
            if (l.V < 1 || l.V > 53) return null;
            "w" in l || (l.w = 1), "Z" in l ? (c = r(i(l.y, 0, 1)), u = c.getUTCDay(), c = u > 4 || 0 === u ? t
              .utcMonday.ceil(c) : t.utcMonday(c), c = t.utcDay.offset(c, 7 * (l.V - 1)), l.y = c
            .getUTCFullYear(), l.m = c.getUTCMonth(), l.d = c.getUTCDate() + (l.w + 6) % 7) : (c = n(i(l.y, 0,
              1)), u = c.getDay(), c = u > 4 || 0 === u ? t.timeMonday.ceil(c) : t.timeMonday(c), c = t.timeDay
              .offset(c, 7 * (l.V - 1)), l.y = c.getFullYear(), l.m = c.getMonth(), l.d = c.getDate() + (l.w +
              6) % 7)
          } else("W" in l || "U" in l) && ("w" in l || (l.w = "u" in l ? l.u % 7 : "W" in l ? 1 : 0), u = "Z" in l ?
            r(i(l.y, 0, 1)).getUTCDay() : n(i(l.y, 0, 1)).getDay(), l.m = 0, l.d = "W" in l ? (l.w + 6) % 7 + 7 *
            l.W - (u + 5) % 7 : l.w + 7 * l.U - (u + 6) % 7);
          return "Z" in l ? (l.H += l.Z / 100 | 0, l.M += l.Z % 100, r(l)) : n(l)
        }
      }

      function s(e, t, n, r) {
        for (var i, o, a = 0, s = t.length, c = n.length; a < s;) {
          if (r >= c) return -1;
          if (i = t.charCodeAt(a++), 37 === i) {
            if (i = t.charAt(a++), o = it[i in we ? t.charAt(a++) : i], !o || (r = o(e, n, r)) < 0) return -1
          } else if (i != n.charCodeAt(r++)) return -1
        }
        return r
      }

      function H(e, t, n) {
        var r = Ve.exec(t.slice(n));
        return r ? (e.p = We[r[0].toLowerCase()], n + r[0].length) : -1
      }

      function se(e, t, n) {
        var r = Xe.exec(t.slice(n));
        return r ? (e.w = Qe[r[0].toLowerCase()], n + r[0].length) : -1
      }

      function be(e, t, n) {
        var r = Ye.exec(t.slice(n));
        return r ? (e.w = Ke[r[0].toLowerCase()], n + r[0].length) : -1
      }

      function Ee(e, t, n) {
        var r = et.exec(t.slice(n));
        return r ? (e.m = tt[r[0].toLowerCase()], n + r[0].length) : -1
      }

      function _e(e, t, n) {
        var r = Je.exec(t.slice(n));
        return r ? (e.m = Ze[r[0].toLowerCase()], n + r[0].length) : -1
      }

      function $e(e, t, n) {
        return s(e, Ue, t, n)
      }

      function Te(e, t, n) {
        return s(e, Fe, t, n)
      }

      function Ce(e, t, n) {
        return s(e, je, t, n)
      }

      function xe(e) {
        return ze[e.getDay()]
      }

      function Se(e) {
        return Be[e.getDay()]
      }

      function Ae(e) {
        return Ge[e.getMonth()]
      }

      function Me(e) {
        return qe[e.getMonth()]
      }

      function ke(e) {
        return He[+(e.getHours() >= 12)]
      }

      function Ne(e) {
        return 1 + ~~(e.getMonth() / 3)
      }

      function Ie(e) {
        return ze[e.getUTCDay()]
      }

      function Oe(e) {
        return Be[e.getUTCDay()]
      }

      function De(e) {
        return Ge[e.getUTCMonth()]
      }

      function Re(e) {
        return qe[e.getUTCMonth()]
      }

      function Pe(e) {
        return He[+(e.getUTCHours() >= 12)]
      }

      function Le(e) {
        return 1 + ~~(e.getUTCMonth() / 3)
      }
      var Ue = e.dateTime,
        Fe = e.date,
        je = e.time,
        He = e.periods,
        Be = e.days,
        ze = e.shortDays,
        qe = e.months,
        Ge = e.shortMonths,
        Ve = c(He),
        We = u(He),
        Ye = c(Be),
        Ke = u(Be),
        Xe = c(ze),
        Qe = u(ze),
        Je = c(qe),
        Ze = u(qe),
        et = c(Ge),
        tt = u(Ge),
        nt = {
          a: xe,
          A: Se,
          b: Ae,
          B: Me,
          c: null,
          d: k,
          e: k,
          f: R,
          g: V,
          G: Y,
          H: N,
          I: I,
          j: O,
          L: D,
          m: P,
          M: L,
          p: ke,
          q: Ne,
          Q: ge,
          s: ye,
          S: U,
          u: F,
          U: j,
          V: B,
          w: z,
          W: q,
          x: null,
          X: null,
          y: G,
          Y: W,
          Z: K,
          "%": ve
        },
        rt = {
          a: Ie,
          A: Oe,
          b: De,
          B: Re,
          c: null,
          d: X,
          e: X,
          f: te,
          g: fe,
          G: pe,
          H: Q,
          I: J,
          j: Z,
          L: ee,
          m: ne,
          M: re,
          p: Pe,
          q: Le,
          Q: ge,
          s: ye,
          S: ie,
          u: oe,
          U: ae,
          V: ce,
          w: ue,
          W: le,
          x: null,
          X: null,
          y: de,
          Y: he,
          Z: me,
          "%": ve
        },
        it = {
          a: se,
          A: be,
          b: Ee,
          B: _e,
          c: $e,
          d: E,
          e: E,
          f: x,
          g: v,
          G: m,
          H: $,
          I: $,
          j: _,
          L: C,
          m: b,
          M: w,
          p: H,
          q: y,
          Q: A,
          s: M,
          S: T,
          u: d,
          U: f,
          V: h,
          w: l,
          W: p,
          x: Te,
          X: Ce,
          y: v,
          Y: m,
          Z: g,
          "%": S
        };
      return nt.x = o(Fe, nt), nt.X = o(je, nt), nt.c = o(Ue, nt), rt.x = o(Fe, rt), rt.X = o(je, rt), rt.c = o(Ue,
        rt), {
        format: function(e) {
          var t = o(e += "", nt);
          return t.toString = function() {
            return e
          }, t
        },
        parse: function(e) {
          var t = a(e += "", !1);
          return t.toString = function() {
            return e
          }, t
        },
        utcFormat: function(e) {
          var t = o(e += "", rt);
          return t.toString = function() {
            return e
          }, t
        },
        utcParse: function(e) {
          var t = a(e += "", !0);
          return t.toString = function() {
            return e
          }, t
        }
      }
    }

    function a(e, t, n) {
      var r = e < 0 ? "-" : "",
        i = (r ? -e : e) + "",
        o = i.length;
      return r + (o < n ? new Array(n - o + 1).join(t) + i : i)
    }

    function s(e) {
      return e.replace(xe, "\\$&")
    }

    function c(e) {
      return new RegExp("^(?:" + e.map(s).join("|") + ")", "i")
    }

    function u(e) {
      for (var t = {}, n = -1, r = e.length; ++n < r;) t[e[n].toLowerCase()] = n;
      return t
    }

    function l(e, t, n) {
      var r = Te.exec(t.slice(n, n + 1));
      return r ? (e.w = +r[0], n + r[0].length) : -1
    }

    function d(e, t, n) {
      var r = Te.exec(t.slice(n, n + 1));
      return r ? (e.u = +r[0], n + r[0].length) : -1
    }

    function f(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.U = +r[0], n + r[0].length) : -1
    }

    function h(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.V = +r[0], n + r[0].length) : -1
    }

    function p(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.W = +r[0], n + r[0].length) : -1
    }

    function m(e, t, n) {
      var r = Te.exec(t.slice(n, n + 4));
      return r ? (e.y = +r[0], n + r[0].length) : -1
    }

    function v(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.y = +r[0] + (+r[0] > 68 ? 1900 : 2e3), n + r[0].length) : -1
    }

    function g(e, t, n) {
      var r = /^(Z)|([+-]\d\d)(?::?(\d\d))?/.exec(t.slice(n, n + 6));
      return r ? (e.Z = r[1] ? 0 : -(r[2] + (r[3] || "00")), n + r[0].length) : -1
    }

    function y(e, t, n) {
      var r = Te.exec(t.slice(n, n + 1));
      return r ? (e.q = 3 * r[0] - 3, n + r[0].length) : -1
    }

    function b(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.m = r[0] - 1, n + r[0].length) : -1
    }

    function E(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.d = +r[0], n + r[0].length) : -1
    }

    function _(e, t, n) {
      var r = Te.exec(t.slice(n, n + 3));
      return r ? (e.m = 0, e.d = +r[0], n + r[0].length) : -1
    }

    function $(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.H = +r[0], n + r[0].length) : -1
    }

    function w(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.M = +r[0], n + r[0].length) : -1
    }

    function T(e, t, n) {
      var r = Te.exec(t.slice(n, n + 2));
      return r ? (e.S = +r[0], n + r[0].length) : -1
    }

    function C(e, t, n) {
      var r = Te.exec(t.slice(n, n + 3));
      return r ? (e.L = +r[0], n + r[0].length) : -1
    }

    function x(e, t, n) {
      var r = Te.exec(t.slice(n, n + 6));
      return r ? (e.L = Math.floor(r[0] / 1e3), n + r[0].length) : -1
    }

    function S(e, t, n) {
      var r = Ce.exec(t.slice(n, n + 1));
      return r ? n + r[0].length : -1
    }

    function A(e, t, n) {
      var r = Te.exec(t.slice(n));
      return r ? (e.Q = +r[0], n + r[0].length) : -1
    }

    function M(e, t, n) {
      var r = Te.exec(t.slice(n));
      return r ? (e.s = +r[0], n + r[0].length) : -1
    }

    function k(e, t) {
      return a(e.getDate(), t, 2)
    }

    function N(e, t) {
      return a(e.getHours(), t, 2)
    }

    function I(e, t) {
      return a(e.getHours() % 12 || 12, t, 2)
    }

    function O(e, n) {
      return a(1 + t.timeDay.count(t.timeYear(e), e), n, 3)
    }

    function D(e, t) {
      return a(e.getMilliseconds(), t, 3)
    }

    function R(e, t) {
      return D(e, t) + "000"
    }

    function P(e, t) {
      return a(e.getMonth() + 1, t, 2)
    }

    function L(e, t) {
      return a(e.getMinutes(), t, 2)
    }

    function U(e, t) {
      return a(e.getSeconds(), t, 2)
    }

    function F(e) {
      var t = e.getDay();
      return 0 === t ? 7 : t
    }

    function j(e, n) {
      return a(t.timeSunday.count(t.timeYear(e) - 1, e), n, 2)
    }

    function H(e) {
      var n = e.getDay();
      return n >= 4 || 0 === n ? t.timeThursday(e) : t.timeThursday.ceil(e)
    }

    function B(e, n) {
      return e = H(e), a(t.timeThursday.count(t.timeYear(e), e) + (4 === t.timeYear(e).getDay()), n, 2)
    }

    function z(e) {
      return e.getDay()
    }

    function q(e, n) {
      return a(t.timeMonday.count(t.timeYear(e) - 1, e), n, 2)
    }

    function G(e, t) {
      return a(e.getFullYear() % 100, t, 2)
    }

    function V(e, t) {
      return e = H(e), a(e.getFullYear() % 100, t, 2)
    }

    function W(e, t) {
      return a(e.getFullYear() % 1e4, t, 4)
    }

    function Y(e, n) {
      var r = e.getDay();
      return e = r >= 4 || 0 === r ? t.timeThursday(e) : t.timeThursday.ceil(e), a(e.getFullYear() % 1e4, n, 4)
    }

    function K(e) {
      var t = e.getTimezoneOffset();
      return (t > 0 ? "-" : (t *= -1, "+")) + a(t / 60 | 0, "0", 2) + a(t % 60, "0", 2)
    }

    function X(e, t) {
      return a(e.getUTCDate(), t, 2)
    }

    function Q(e, t) {
      return a(e.getUTCHours(), t, 2)
    }

    function J(e, t) {
      return a(e.getUTCHours() % 12 || 12, t, 2)
    }

    function Z(e, n) {
      return a(1 + t.utcDay.count(t.utcYear(e), e), n, 3)
    }

    function ee(e, t) {
      return a(e.getUTCMilliseconds(), t, 3)
    }

    function te(e, t) {
      return ee(e, t) + "000"
    }

    function ne(e, t) {
      return a(e.getUTCMonth() + 1, t, 2)
    }

    function re(e, t) {
      return a(e.getUTCMinutes(), t, 2)
    }

    function ie(e, t) {
      return a(e.getUTCSeconds(), t, 2)
    }

    function oe(e) {
      var t = e.getUTCDay();
      return 0 === t ? 7 : t
    }

    function ae(e, n) {
      return a(t.utcSunday.count(t.utcYear(e) - 1, e), n, 2)
    }

    function se(e) {
      var n = e.getUTCDay();
      return n >= 4 || 0 === n ? t.utcThursday(e) : t.utcThursday.ceil(e)
    }

    function ce(e, n) {
      return e = se(e), a(t.utcThursday.count(t.utcYear(e), e) + (4 === t.utcYear(e).getUTCDay()), n, 2)
    }

    function ue(e) {
      return e.getUTCDay()
    }

    function le(e, n) {
      return a(t.utcMonday.count(t.utcYear(e) - 1, e), n, 2)
    }

    function de(e, t) {
      return a(e.getUTCFullYear() % 100, t, 2)
    }

    function fe(e, t) {
      return e = se(e), a(e.getUTCFullYear() % 100, t, 2)
    }

    function he(e, t) {
      return a(e.getUTCFullYear() % 1e4, t, 4)
    }

    function pe(e, n) {
      var r = e.getUTCDay();
      return e = r >= 4 || 0 === r ? t.utcThursday(e) : t.utcThursday.ceil(e), a(e.getUTCFullYear() % 1e4, n, 4)
    }

    function me() {
      return "+0000"
    }

    function ve() {
      return "%"
    }

    function ge(e) {
      return +e
    }

    function ye(e) {
      return Math.floor(+e / 1e3)
    }

    function be(t) {
      return $e = o(t), e.timeFormat = $e.format, e.timeParse = $e.parse, e.utcFormat = $e.utcFormat, e.utcParse = $e
        .utcParse, $e
    }

    function Ee(e) {
      return e.toISOString()
    }

    function _e(e) {
      var t = new Date(e);
      return isNaN(t) ? null : t
    }
    var $e, we = {
        "-": "",
        _: " ",
        0: "0"
      },
      Te = /^\s*\d+/,
      Ce = /^%/,
      xe = /[\\^$*+?|[\]().{}]/g;
    be({
      dateTime: "%x, %X",
      date: "%-m/%-d/%Y",
      time: "%-I:%M:%S %p",
      periods: ["AM", "PM"],
      days: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
      shortDays: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
      months: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
        "November", "December"
      ],
      shortMonths: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
    });
    var Se = "%Y-%m-%dT%H:%M:%S.%LZ",
      Ae = Date.prototype.toISOString ? Ee : e.utcFormat(Se),
      Me = +new Date("2000-01-01T00:00:00.000Z") ? _e : e.utcParse(Se);
    e.isoFormat = Ae, e.isoParse = Me, e.timeFormatDefaultLocale = be, e.timeFormatLocale = o, Object.defineProperty(
      e, "__esModule", {
        value: !0
      })
  })
}
