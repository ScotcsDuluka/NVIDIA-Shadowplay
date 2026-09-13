// ─────────────────────────────────────────────────────────────
// APP MODULE 131
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, i) {
    i(t, n(84))
  }(this, function(e, t) {
    "use strict";

    function n(e) {
      if (0 <= e.y && e.y < 100) {
        var t = new Date(-1, e.m, e.d, e.H, e.M, e.S, e.L);
        return t.setFullYear(e.y), t
      }
      return new Date(e.y, e.m, e.d, e.H, e.M, e.S, e.L)
    }

    function i(e) {
      if (0 <= e.y && e.y < 100) {
        var t = new Date(Date.UTC(-1, e.m, e.d, e.H, e.M, e.S, e.L));
        return t.setUTCFullYear(e.y), t
      }
      return new Date(Date.UTC(e.y, e.m, e.d, e.H, e.M, e.S, e.L))
    }

    function o(e, t, n) {
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

    function r(e) {
      function r(e, t) {
        return function(n) {
          var i, o, r, a = [],
            l = -1,
            s = 0,
            d = e.length;
          for (n instanceof Date || (n = new Date(+n)); ++l < d;) 37 === e.charCodeAt(l) && (a.push(e.slice(s, l)),
            null != (o = Ee[i = e.charAt(++l)]) ? i = e.charAt(++l) : o = "e" === i ? " " : "0", (r = t[i]) && (
              i = r(n, o)), a.push(i), s = l + 1);
          return a.push(e.slice(s, l)), a.join("")
        }
      }

      function a(e, r) {
        return function(a) {
          var s, d, c = o(1900, void 0, 1),
            u = l(c, e, a += "", 0);
          if (u != a.length) return null;
          if ("Q" in c) return new Date(c.Q);
          if ("s" in c) return new Date(1e3 * c.s + ("L" in c ? c.L : 0));
          if (!r || "Z" in c || (c.Z = 0), "p" in c && (c.H = c.H % 12 + 12 * c.p), void 0 === c.m && (c.m = "q" in
              c ? c.q : 0), "V" in c) {
            if (c.V < 1 || c.V > 53) return null;
            "w" in c || (c.w = 1), "Z" in c ? (s = i(o(c.y, 0, 1)), d = s.getUTCDay(), s = d > 4 || 0 === d ? t
              .utcMonday.ceil(s) : t.utcMonday(s), s = t.utcDay.offset(s, 7 * (c.V - 1)), c.y = s
            .getUTCFullYear(), c.m = s.getUTCMonth(), c.d = s.getUTCDate() + (c.w + 6) % 7) : (s = n(o(c.y, 0,
              1)), d = s.getDay(), s = d > 4 || 0 === d ? t.timeMonday.ceil(s) : t.timeMonday(s), s = t.timeDay
              .offset(s, 7 * (c.V - 1)), c.y = s.getFullYear(), c.m = s.getMonth(), c.d = s.getDate() + (c.w +
              6) % 7)
          } else("W" in c || "U" in c) && ("w" in c || (c.w = "u" in c ? c.u % 7 : "W" in c ? 1 : 0), d = "Z" in c ?
            i(o(c.y, 0, 1)).getUTCDay() : n(o(c.y, 0, 1)).getDay(), c.m = 0, c.d = "W" in c ? (c.w + 6) % 7 + 7 *
            c.W - (d + 5) % 7 : c.w + 7 * c.U - (d + 6) % 7);
          return "Z" in c ? (c.H += c.Z / 100 | 0, c.M += c.Z % 100, i(c)) : n(c)
        }
      }

      function l(e, t, n, i) {
        for (var o, r, a = 0, l = t.length, s = n.length; a < l;) {
          if (i >= s) return -1;
          if (o = t.charCodeAt(a++), 37 === o) {
            if (o = t.charAt(a++), r = ot[o in Ee ? t.charAt(a++) : o], !r || (i = r(e, n, i)) < 0) return -1
          } else if (o != n.charCodeAt(i++)) return -1
        }
        return i
      }

      function V(e, t, n) {
        var i = We.exec(t.slice(n));
        return i ? (e.p = je[i[0].toLowerCase()], n + i[0].length) : -1
      }

      function le(e, t, n) {
        var i = Xe.exec(t.slice(n));
        return i ? (e.w = Ze[i[0].toLowerCase()], n + i[0].length) : -1
      }

      function ve(e, t, n) {
        var i = Ke.exec(t.slice(n));
        return i ? (e.w = qe[i[0].toLowerCase()], n + i[0].length) : -1
      }

      function ye(e, t, n) {
        var i = et.exec(t.slice(n));
        return i ? (e.m = tt[i[0].toLowerCase()], n + i[0].length) : -1
      }

      function we(e, t, n) {
        var i = Qe.exec(t.slice(n));
        return i ? (e.m = Je[i[0].toLowerCase()], n + i[0].length) : -1
      }

      function Se(e, t, n) {
        return l(e, Ue, t, n)
      }

      function ke(e, t, n) {
        return l(e, ze, t, n)
      }

      function _e(e, t, n) {
        return l(e, Ge, t, n)
      }

      function Te(e) {
        return Be[e.getDay()]
      }

      function Ce(e) {
        return He[e.getDay()]
      }

      function Oe(e) {
        return $e[e.getMonth()]
      }

      function Ae(e) {
        return Ye[e.getMonth()]
      }

      function Ie(e) {
        return Ve[+(e.getHours() >= 12)]
      }

      function Me(e) {
        return 1 + ~~(e.getMonth() / 3)
      }

      function Re(e) {
        return Be[e.getUTCDay()]
      }

      function Pe(e) {
        return He[e.getUTCDay()]
      }

      function De(e) {
        return $e[e.getUTCMonth()]
      }

      function Ne(e) {
        return Ye[e.getUTCMonth()]
      }

      function Le(e) {
        return Ve[+(e.getUTCHours() >= 12)]
      }

      function Fe(e) {
        return 1 + ~~(e.getUTCMonth() / 3)
      }
      var Ue = e.dateTime,
        ze = e.date,
        Ge = e.time,
        Ve = e.periods,
        He = e.days,
        Be = e.shortDays,
        Ye = e.months,
        $e = e.shortMonths,
        We = s(Ve),
        je = d(Ve),
        Ke = s(He),
        qe = d(He),
        Xe = s(Be),
        Ze = d(Be),
        Qe = s(Ye),
        Je = d(Ye),
        et = s($e),
        tt = d($e),
        nt = {
          a: Te,
          A: Ce,
          b: Oe,
          B: Ae,
          c: null,
          d: I,
          e: I,
          f: N,
          g: W,
          G: K,
          H: M,
          I: R,
          j: P,
          L: D,
          m: L,
          M: F,
          p: Ie,
          q: Me,
          Q: be,
          s: xe,
          S: U,
          u: z,
          U: G,
          V: H,
          w: B,
          W: Y,
          x: null,
          X: null,
          y: $,
          Y: j,
          Z: q,
          "%": he
        },
        it = {
          a: Re,
          A: Pe,
          b: De,
          B: Ne,
          c: null,
          d: X,
          e: X,
          f: te,
          g: fe,
          G: ge,
          H: Z,
          I: Q,
          j: J,
          L: ee,
          m: ne,
          M: ie,
          p: Le,
          q: Fe,
          Q: be,
          s: xe,
          S: oe,
          u: re,
          U: ae,
          V: se,
          w: de,
          W: ce,
          x: null,
          X: null,
          y: ue,
          Y: me,
          Z: pe,
          "%": he
        },
        ot = {
          a: le,
          A: ve,
          b: ye,
          B: we,
          c: Se,
          d: y,
          e: y,
          f: T,
          g: h,
          G: p,
          H: S,
          I: S,
          j: w,
          L: _,
          m: v,
          M: E,
          p: V,
          q: x,
          Q: O,
          s: A,
          S: k,
          u: u,
          U: f,
          V: m,
          w: c,
          W: g,
          x: ke,
          X: _e,
          y: h,
          Y: p,
          Z: b,
          "%": C
        };
      return nt.x = r(ze, nt), nt.X = r(Ge, nt), nt.c = r(Ue, nt), it.x = r(ze, it), it.X = r(Ge, it), it.c = r(Ue,
        it), {
        format: function(e) {
          var t = r(e += "", nt);
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
          var t = r(e += "", it);
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
      var i = e < 0 ? "-" : "",
        o = (i ? -e : e) + "",
        r = o.length;
      return i + (r < n ? new Array(n - r + 1).join(t) + o : o)
    }

    function l(e) {
      return e.replace(Te, "\\$&")
    }

    function s(e) {
      return new RegExp("^(?:" + e.map(l).join("|") + ")", "i")
    }

    function d(e) {
      for (var t = {}, n = -1, i = e.length; ++n < i;) t[e[n].toLowerCase()] = n;
      return t
    }

    function c(e, t, n) {
      var i = ke.exec(t.slice(n, n + 1));
      return i ? (e.w = +i[0], n + i[0].length) : -1
    }

    function u(e, t, n) {
      var i = ke.exec(t.slice(n, n + 1));
      return i ? (e.u = +i[0], n + i[0].length) : -1
    }

    function f(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.U = +i[0], n + i[0].length) : -1
    }

    function m(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.V = +i[0], n + i[0].length) : -1
    }

    function g(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.W = +i[0], n + i[0].length) : -1
    }

    function p(e, t, n) {
      var i = ke.exec(t.slice(n, n + 4));
      return i ? (e.y = +i[0], n + i[0].length) : -1
    }

    function h(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.y = +i[0] + (+i[0] > 68 ? 1900 : 2e3), n + i[0].length) : -1
    }

    function b(e, t, n) {
      var i = /^(Z)|([+-]\d\d)(?::?(\d\d))?/.exec(t.slice(n, n + 6));
      return i ? (e.Z = i[1] ? 0 : -(i[2] + (i[3] || "00")), n + i[0].length) : -1
    }

    function x(e, t, n) {
      var i = ke.exec(t.slice(n, n + 1));
      return i ? (e.q = 3 * i[0] - 3, n + i[0].length) : -1
    }

    function v(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.m = i[0] - 1, n + i[0].length) : -1
    }

    function y(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.d = +i[0], n + i[0].length) : -1
    }

    function w(e, t, n) {
      var i = ke.exec(t.slice(n, n + 3));
      return i ? (e.m = 0, e.d = +i[0], n + i[0].length) : -1
    }

    function S(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.H = +i[0], n + i[0].length) : -1
    }

    function E(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.M = +i[0], n + i[0].length) : -1
    }

    function k(e, t, n) {
      var i = ke.exec(t.slice(n, n + 2));
      return i ? (e.S = +i[0], n + i[0].length) : -1
    }

    function _(e, t, n) {
      var i = ke.exec(t.slice(n, n + 3));
      return i ? (e.L = +i[0], n + i[0].length) : -1
    }

    function T(e, t, n) {
      var i = ke.exec(t.slice(n, n + 6));
      return i ? (e.L = Math.floor(i[0] / 1e3), n + i[0].length) : -1
    }

    function C(e, t, n) {
      var i = _e.exec(t.slice(n, n + 1));
      return i ? n + i[0].length : -1
    }

    function O(e, t, n) {
      var i = ke.exec(t.slice(n));
      return i ? (e.Q = +i[0], n + i[0].length) : -1
    }

    function A(e, t, n) {
      var i = ke.exec(t.slice(n));
      return i ? (e.s = +i[0], n + i[0].length) : -1
    }

    function I(e, t) {
      return a(e.getDate(), t, 2)
    }

    function M(e, t) {
      return a(e.getHours(), t, 2)
    }

    function R(e, t) {
      return a(e.getHours() % 12 || 12, t, 2)
    }

    function P(e, n) {
      return a(1 + t.timeDay.count(t.timeYear(e), e), n, 3)
    }

    function D(e, t) {
      return a(e.getMilliseconds(), t, 3)
    }

    function N(e, t) {
      return D(e, t) + "000"
    }

    function L(e, t) {
      return a(e.getMonth() + 1, t, 2)
    }

    function F(e, t) {
      return a(e.getMinutes(), t, 2)
    }

    function U(e, t) {
      return a(e.getSeconds(), t, 2)
    }

    function z(e) {
      var t = e.getDay();
      return 0 === t ? 7 : t
    }

    function G(e, n) {
      return a(t.timeSunday.count(t.timeYear(e) - 1, e), n, 2)
    }

    function V(e) {
      var n = e.getDay();
      return n >= 4 || 0 === n ? t.timeThursday(e) : t.timeThursday.ceil(e)
    }

    function H(e, n) {
      return e = V(e), a(t.timeThursday.count(t.timeYear(e), e) + (4 === t.timeYear(e).getDay()), n, 2)
    }

    function B(e) {
      return e.getDay()
    }

    function Y(e, n) {
      return a(t.timeMonday.count(t.timeYear(e) - 1, e), n, 2)
    }

    function $(e, t) {
      return a(e.getFullYear() % 100, t, 2)
    }

    function W(e, t) {
      return e = V(e), a(e.getFullYear() % 100, t, 2)
    }

    function j(e, t) {
      return a(e.getFullYear() % 1e4, t, 4)
    }

    function K(e, n) {
      var i = e.getDay();
      return e = i >= 4 || 0 === i ? t.timeThursday(e) : t.timeThursday.ceil(e), a(e.getFullYear() % 1e4, n, 4)
    }

    function q(e) {
      var t = e.getTimezoneOffset();
      return (t > 0 ? "-" : (t *= -1, "+")) + a(t / 60 | 0, "0", 2) + a(t % 60, "0", 2)
    }

    function X(e, t) {
      return a(e.getUTCDate(), t, 2)
    }

    function Z(e, t) {
      return a(e.getUTCHours(), t, 2)
    }

    function Q(e, t) {
      return a(e.getUTCHours() % 12 || 12, t, 2)
    }

    function J(e, n) {
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

    function ie(e, t) {
      return a(e.getUTCMinutes(), t, 2)
    }

    function oe(e, t) {
      return a(e.getUTCSeconds(), t, 2)
    }

    function re(e) {
      var t = e.getUTCDay();
      return 0 === t ? 7 : t
    }

    function ae(e, n) {
      return a(t.utcSunday.count(t.utcYear(e) - 1, e), n, 2)
    }

    function le(e) {
      var n = e.getUTCDay();
      return n >= 4 || 0 === n ? t.utcThursday(e) : t.utcThursday.ceil(e)
    }

    function se(e, n) {
      return e = le(e), a(t.utcThursday.count(t.utcYear(e), e) + (4 === t.utcYear(e).getUTCDay()), n, 2)
    }

    function de(e) {
      return e.getUTCDay()
    }

    function ce(e, n) {
      return a(t.utcMonday.count(t.utcYear(e) - 1, e), n, 2)
    }

    function ue(e, t) {
      return a(e.getUTCFullYear() % 100, t, 2)
    }

    function fe(e, t) {
      return e = le(e), a(e.getUTCFullYear() % 100, t, 2)
    }

    function me(e, t) {
      return a(e.getUTCFullYear() % 1e4, t, 4)
    }

    function ge(e, n) {
      var i = e.getUTCDay();
      return e = i >= 4 || 0 === i ? t.utcThursday(e) : t.utcThursday.ceil(e), a(e.getUTCFullYear() % 1e4, n, 4)
    }

    function pe() {
      return "+0000"
    }

    function he() {
      return "%"
    }

    function be(e) {
      return +e
    }

    function xe(e) {
      return Math.floor(+e / 1e3)
    }

    function ve(t) {
      return Se = r(t), e.timeFormat = Se.format, e.timeParse = Se.parse, e.utcFormat = Se.utcFormat, e.utcParse = Se
        .utcParse, Se
    }

    function ye(e) {
      return e.toISOString()
    }

    function we(e) {
      var t = new Date(e);
      return isNaN(t) ? null : t
    }
    var Se, Ee = {
        "-": "",
        _: " ",
        0: "0"
      },
      ke = /^\s*\d+/,
      _e = /^%/,
      Te = /[\\^$*+?|[\]().{}]/g;
    ve({
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
    var Ce = "%Y-%m-%dT%H:%M:%S.%LZ",
      Oe = Date.prototype.toISOString ? ye : e.utcFormat(Ce),
      Ae = +new Date("2000-01-01T00:00:00.000Z") ? we : e.utcParse(Ce);
    e.isoFormat = Oe, e.isoParse = Ae, e.timeFormatDefaultLocale = ve, e.timeFormatLocale = r, Object.defineProperty(
      e, "__esModule", {
        value: !0
      })
  })
}
