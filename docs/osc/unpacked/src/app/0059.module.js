// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 59
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e, t, n) {
      e.prototype = t.prototype = n, n.constructor = e;
    }

    function n(e, t) {
      var n = Object.create(e.prototype);
      for (var i in t) n[i] = t[i];
      return n;
    }

    function i() {}

    function o() {
      return this.rgb().formatHex();
    }

    function r() {
      return b(this).formatHsl();
    }

    function a() {
      return this.rgb().formatRgb();
    }

    function l(e) {
      var t, n;
      return e = (e + "").trim().toLowerCase(), (t = H.exec(e)) ? (n = t[1].length, t = parseInt(t[1], 16),
          6 === n ? s(t) : 3 === n ? new f(t >> 8 & 15 | t >> 4 & 240, t >> 4 & 15 | 240 & t, (15 & t) <<
            4 | 15 & t, 1) : 8 === n ? d(t >> 24 & 255, t >> 16 & 255, t >> 8 & 255, (255 & t) / 255) :
          4 === n ? d(t >> 12 & 15 | t >> 8 & 240, t >> 8 & 15 | t >> 4 & 240, t >> 4 & 15 | 240 & t, ((15 &
            t) << 4 | 15 & t) / 255) : null) : (t = B.exec(e)) ? new f(t[1], t[2], t[3], 1) : (t = Y.exec(
          e)) ? new f(255 * t[1] / 100, 255 * t[2] / 100, 255 * t[3] / 100, 1) : (t = $.exec(e)) ? d(t[1],
          t[2], t[3], t[4]) : (t = W.exec(e)) ? d(255 * t[1] / 100, 255 * t[2] / 100, 255 * t[3] / 100, t[
          4]) : (t = j.exec(e)) ? h(t[1], t[2] / 100, t[3] / 100, 1) : (t = K.exec(e)) ? h(t[1], t[2] / 100,
          t[3] / 100, t[4]) : q.hasOwnProperty(e) ? s(q[e]) : "transparent" === e ? new f(NaN, NaN, NaN,
        0) : null;
    }

    function s(e) {
      return new f(e >> 16 & 255, e >> 8 & 255, 255 & e, 1);
    }

    function d(e, t, n, i) {
      return i <= 0 && (e = t = n = NaN), new f(e, t, n, i);
    }

    function c(e) {
      return e instanceof i || (e = l(e)), e ? (e = e.rgb(), new f(e.r, e.g, e.b, e.opacity)) : new f();
    }

    function u(e, t, n, i) {
      return 1 === arguments.length ? c(e) : new f(e, t, n, null == i ? 1 : i);
    }

    function f(e, t, n, i) {
      this.r = +e, this.g = +t, this.b = +n, this.opacity = +i;
    }

    function m() {
      return "#" + p(this.r) + p(this.g) + p(this.b);
    }

    function g() {
      var e = this.opacity;
      return e = isNaN(e) ? 1 : Math.max(0, Math.min(1, e)), (1 === e ? "rgb(" : "rgba(") + Math.max(0, Math
          .min(255, Math.round(this.r) || 0)) + ", " + Math.max(0, Math.min(255, Math.round(this.g) || 0)) +
        ", " + Math.max(0, Math.min(255, Math.round(this.b) || 0)) + (1 === e ? ")" : ", " + e + ")");
    }

    function p(e) {
      return e = Math.max(0, Math.min(255, Math.round(e) || 0)), (e < 16 ? "0" : "") + e.toString(16);
    }

    function h(e, t, n, i) {
      return i <= 0 ? e = t = n = NaN : n <= 0 || n >= 1 ? e = t = NaN : t <= 0 && (e = NaN), new v(e, t, n,
        i);
    }

    function b(e) {
      if (e instanceof v) return new v(e.h, e.s, e.l, e.opacity);
      if (e instanceof i || (e = l(e)), !e) return new v();
      if (e instanceof v) return e;
      e = e.rgb();
      var t = e.r / 255,
        n = e.g / 255,
        o = e.b / 255,
        r = Math.min(t, n, o),
        a = Math.max(t, n, o),
        s = NaN,
        d = a - r,
        c = (a + r) / 2;
      return d ? (s = t === a ? (n - o) / d + 6 * (n < o) : n === a ? (o - t) / d + 2 : (t - n) / d + 4,
        d /= c < .5 ? a + r : 2 - a - r, s *= 60) : d = c > 0 && c < 1 ? 0 : s, new v(s, d, c, e.opacity);
    }

    function x(e, t, n, i) {
      return 1 === arguments.length ? b(e) : new v(e, t, n, null == i ? 1 : i);
    }

    function v(e, t, n, i) {
      this.h = +e, this.s = +t, this.l = +n, this.opacity = +i;
    }

    function y(e, t, n) {
      return 255 * (e < 60 ? t + (n - t) * e / 60 : e < 180 ? n : e < 240 ? t + (n - t) * (240 - e) / 60 :
        t);
    }

    function w(e) {
      if (e instanceof k) return new k(e.l, e.a, e.b, e.opacity);
      if (e instanceof R) return P(e);
      e instanceof f || (e = c(e));
      var t,
        n,
        i = O(e.r),
        o = O(e.g),
        r = O(e.b),
        a = _((.2225045 * i + .7168786 * o + .0606169 * r) / ee);
      return i === o && o === r ? t = n = a : (t = _((.4360747 * i + .3850649 * o + .1430804 * r) / J), n =
        _((.0139322 * i + .0971045 * o + .7141733 * r) / te)), new k(116 * a - 16, 500 * (t - a), 200 * (
        a - n), e.opacity);
    }

    function S(e, t) {
      return new k(e, 0, 0, null == t ? 1 : t);
    }

    function E(e, t, n, i) {
      return 1 === arguments.length ? w(e) : new k(e, t, n, null == i ? 1 : i);
    }

    function k(e, t, n, i) {
      this.l = +e, this.a = +t, this.b = +n, this.opacity = +i;
    }

    function _(e) {
      return e > re ? Math.pow(e, 1 / 3) : e / oe + ne;
    }

    function T(e) {
      return e > ie ? e * e * e : oe * (e - ne);
    }

    function C(e) {
      return 255 * (e <= .0031308 ? 12.92 * e : 1.055 * Math.pow(e, 1 / 2.4) - .055);
    }

    function O(e) {
      return (e /= 255) <= .04045 ? e / 12.92 : Math.pow((e + .055) / 1.055, 2.4);
    }

    function A(e) {
      if (e instanceof R) return new R(e.h, e.c, e.l, e.opacity);
      if (e instanceof k || (e = w(e)), 0 === e.a && 0 === e.b) return new R(NaN, 0 < e.l && e.l < 100 ? 0 :
        NaN, e.l, e.opacity);
      var t = Math.atan2(e.b, e.a) * Z;
      return new R(t < 0 ? t + 360 : t, Math.sqrt(e.a * e.a + e.b * e.b), e.l, e.opacity);
    }

    function I(e, t, n, i) {
      return 1 === arguments.length ? A(e) : new R(n, t, e, null == i ? 1 : i);
    }

    function M(e, t, n, i) {
      return 1 === arguments.length ? A(e) : new R(e, t, n, null == i ? 1 : i);
    }

    function R(e, t, n, i) {
      this.h = +e, this.c = +t, this.l = +n, this.opacity = +i;
    }

    function P(e) {
      if (isNaN(e.h)) return new k(e.l, 0, 0, e.opacity);
      var t = e.h * X;
      return new k(e.l, Math.cos(t) * e.c, Math.sin(t) * e.c, e.opacity);
    }

    function D(e) {
      if (e instanceof L) return new L(e.h, e.s, e.l, e.opacity);
      e instanceof f || (e = c(e));
      var t = e.r / 255,
        n = e.g / 255,
        i = e.b / 255,
        o = (me * i + ue * t - fe * n) / (me + ue - fe),
        r = i - o,
        a = (ce * (n - o) - se * r) / de,
        l = Math.sqrt(a * a + r * r) / (ce * o * (1 - o)),
        s = l ? Math.atan2(a, r) * Z - 120 : NaN;
      return new L(s < 0 ? s + 360 : s, l, o, e.opacity);
    }

    function N(e, t, n, i) {
      return 1 === arguments.length ? D(e) : new L(e, t, n, null == i ? 1 : i);
    }

    function L(e, t, n, i) {
      this.h = +e, this.s = +t, this.l = +n, this.opacity = +i;
    }
    var F = .7,
      U = 1 / F,
      z = "\\s*([+-]?\\d+)\\s*",
      G = "\\s*([+-]?\\d*\\.?\\d+(?:[eE][+-]?\\d+)?)\\s*",
      V = "\\s*([+-]?\\d*\\.?\\d+(?:[eE][+-]?\\d+)?)%\\s*",
      H = /^#([0-9a-f]{3,8})$/,
      B = new RegExp("^rgb\\(" + [z, z, z] + "\\)$"),
      Y = new RegExp("^rgb\\(" + [V, V, V] + "\\)$"),
      $ = new RegExp("^rgba\\(" + [z, z, z, G] + "\\)$"),
      W = new RegExp("^rgba\\(" + [V, V, V, G] + "\\)$"),
      j = new RegExp("^hsl\\(" + [G, V, V] + "\\)$"),
      K = new RegExp("^hsla\\(" + [G, V, V, G] + "\\)$"),
      q = {
        aliceblue: 15792383,
        antiquewhite: 16444375,
        aqua: 65535,
        aquamarine: 8388564,
        azure: 15794175,
        beige: 16119260,
        bisque: 16770244,
        black: 0,
        blanchedalmond: 16772045,
        blue: 255,
        blueviolet: 9055202,
        brown: 10824234,
        burlywood: 14596231,
        cadetblue: 6266528,
        chartreuse: 8388352,
        chocolate: 13789470,
        coral: 16744272,
        cornflowerblue: 6591981,
        cornsilk: 16775388,
        crimson: 14423100,
        cyan: 65535,
        darkblue: 139,
        darkcyan: 35723,
        darkgoldenrod: 12092939,
        darkgray: 11119017,
        darkgreen: 25600,
        darkgrey: 11119017,
        darkkhaki: 12433259,
        darkmagenta: 9109643,
        darkolivegreen: 5597999,
        darkorange: 16747520,
        darkorchid: 10040012,
        darkred: 9109504,
        darksalmon: 15308410,
        darkseagreen: 9419919,
        darkslateblue: 4734347,
        darkslategray: 3100495,
        darkslategrey: 3100495,
        darkturquoise: 52945,
        darkviolet: 9699539,
        deeppink: 16716947,
        deepskyblue: 49151,
        dimgray: 6908265,
        dimgrey: 6908265,
        dodgerblue: 2003199,
        firebrick: 11674146,
        floralwhite: 16775920,
        forestgreen: 2263842,
        fuchsia: 16711935,
        gainsboro: 14474460,
        ghostwhite: 16316671,
        gold: 16766720,
        goldenrod: 14329120,
        gray: 8421504,
        green: 32768,
        greenyellow: 11403055,
        grey: 8421504,
        honeydew: 15794160,
        hotpink: 16738740,
        indianred: 13458524,
        indigo: 4915330,
        ivory: 16777200,
        khaki: 15787660,
        lavender: 15132410,
        lavenderblush: 16773365,
        lawngreen: 8190976,
        lemonchiffon: 16775885,
        lightblue: 11393254,
        lightcoral: 15761536,
        lightcyan: 14745599,
        lightgoldenrodyellow: 16448210,
        lightgray: 13882323,
        lightgreen: 9498256,
        lightgrey: 13882323,
        lightpink: 16758465,
        lightsalmon: 16752762,
        lightseagreen: 2142890,
        lightskyblue: 8900346,
        lightslategray: 7833753,
        lightslategrey: 7833753,
        lightsteelblue: 11584734,
        lightyellow: 16777184,
        lime: 65280,
        limegreen: 3329330,
        linen: 16445670,
        magenta: 16711935,
        maroon: 8388608,
        mediumaquamarine: 6737322,
        mediumblue: 205,
        mediumorchid: 12211667,
        mediumpurple: 9662683,
        mediumseagreen: 3978097,
        mediumslateblue: 8087790,
        mediumspringgreen: 64154,
        mediumturquoise: 4772300,
        mediumvioletred: 13047173,
        midnightblue: 1644912,
        mintcream: 16121850,
        mistyrose: 16770273,
        moccasin: 16770229,
        navajowhite: 16768685,
        navy: 128,
        oldlace: 16643558,
        olive: 8421376,
        olivedrab: 7048739,
        orange: 16753920,
        orangered: 16729344,
        orchid: 14315734,
        palegoldenrod: 15657130,
        palegreen: 10025880,
        paleturquoise: 11529966,
        palevioletred: 14381203,
        papayawhip: 16773077,
        peachpuff: 16767673,
        peru: 13468991,
        pink: 16761035,
        plum: 14524637,
        powderblue: 11591910,
        purple: 8388736,
        rebeccapurple: 6697881,
        red: 16711680,
        rosybrown: 12357519,
        royalblue: 4286945,
        saddlebrown: 9127187,
        salmon: 16416882,
        sandybrown: 16032864,
        seagreen: 3050327,
        seashell: 16774638,
        sienna: 10506797,
        silver: 12632256,
        skyblue: 8900331,
        slateblue: 6970061,
        slategray: 7372944,
        slategrey: 7372944,
        snow: 16775930,
        springgreen: 65407,
        steelblue: 4620980,
        tan: 13808780,
        teal: 32896,
        thistle: 14204888,
        tomato: 16737095,
        turquoise: 4251856,
        violet: 15631086,
        wheat: 16113331,
        white: 16777215,
        whitesmoke: 16119285,
        yellow: 16776960,
        yellowgreen: 10145074
      };
    t(i, l, {
      copy: function(e) {
        return Object.assign(new this.constructor(), this, e);
      },
      displayable: function() {
        return this.rgb().displayable();
      },
      hex: o,
      formatHex: o,
      formatHsl: r,
      formatRgb: a,
      toString: a
    }), t(f, u, n(i, {
      brighter: function(e) {
        return e = null == e ? U : Math.pow(U, e), new f(this.r * e, this.g * e, this.b * e, this
          .opacity);
      },
      darker: function(e) {
        return e = null == e ? F : Math.pow(F, e), new f(this.r * e, this.g * e, this.b * e, this
          .opacity);
      },
      rgb: function() {
        return this;
      },
      displayable: function() {
        return -.5 <= this.r && this.r < 255.5 && -.5 <= this.g && this.g < 255.5 && -.5 <= this
          .b && this.b < 255.5 && 0 <= this.opacity && this.opacity <= 1;
      },
      hex: m,
      formatHex: m,
      formatRgb: g,
      toString: g
    })), t(v, x, n(i, {
      brighter: function(e) {
        return e = null == e ? U : Math.pow(U, e), new v(this.h, this.s, this.l * e, this.opacity);
      },
      darker: function(e) {
        return e = null == e ? F : Math.pow(F, e), new v(this.h, this.s, this.l * e, this.opacity);
      },
      rgb: function() {
        var e = this.h % 360 + 360 * (this.h < 0),
          t = isNaN(e) || isNaN(this.s) ? 0 : this.s,
          n = this.l,
          i = n + (n < .5 ? n : 1 - n) * t,
          o = 2 * n - i;
        return new f(y(e >= 240 ? e - 240 : e + 120, o, i), y(e, o, i), y(e < 120 ? e + 240 : e -
          120, o, i), this.opacity);
      },
      displayable: function() {
        return (0 <= this.s && this.s <= 1 || isNaN(this.s)) && 0 <= this.l && this.l <= 1 && 0 <=
          this.opacity && this.opacity <= 1;
      },
      formatHsl: function() {
        var e = this.opacity;
        return e = isNaN(e) ? 1 : Math.max(0, Math.min(1, e)), (1 === e ? "hsl(" : "hsla(") + (this
          .h || 0) + ", " + 100 * (this.s || 0) + "%, " + 100 * (this.l || 0) + "%" + (1 === e ?
          ")" : ", " + e + ")");
      }
    }));
    var X = Math.PI / 180,
      Z = 180 / Math.PI,
      Q = 18,
      J = .96422,
      ee = 1,
      te = .82521,
      ne = 4 / 29,
      ie = 6 / 29,
      oe = 3 * ie * ie,
      re = ie * ie * ie;
    t(k, E, n(i, {
      brighter: function(e) {
        return new k(this.l + Q * (null == e ? 1 : e), this.a, this.b, this.opacity);
      },
      darker: function(e) {
        return new k(this.l - Q * (null == e ? 1 : e), this.a, this.b, this.opacity);
      },
      rgb: function() {
        var e = (this.l + 16) / 116,
          t = isNaN(this.a) ? e : e + this.a / 500,
          n = isNaN(this.b) ? e : e - this.b / 200;
        return t = J * T(t), e = ee * T(e), n = te * T(n), new f(C(3.1338561 * t - 1.6168667 * e -
          .4906146 * n), C(-.9787684 * t + 1.9161415 * e + .033454 * n), C(.0719453 * t -
          .2289914 * e + 1.4052427 * n), this.opacity);
      }
    })), t(R, M, n(i, {
      brighter: function(e) {
        return new R(this.h, this.c, this.l + Q * (null == e ? 1 : e), this.opacity);
      },
      darker: function(e) {
        return new R(this.h, this.c, this.l - Q * (null == e ? 1 : e), this.opacity);
      },
      rgb: function() {
        return P(this).rgb();
      }
    }));
    var ae = -.14861,
      le = 1.78277,
      se = -.29227,
      de = -.90649,
      ce = 1.97294,
      ue = ce * de,
      fe = ce * le,
      me = le * se - de * ae;
    t(L, N, n(i, {
        brighter: function(e) {
          return e = null == e ? U : Math.pow(U, e), new L(this.h, this.s, this.l * e, this.opacity);
        },
        darker: function(e) {
          return e = null == e ? F : Math.pow(F, e), new L(this.h, this.s, this.l * e, this.opacity);
        },
        rgb: function() {
          var e = isNaN(this.h) ? 0 : (this.h + 120) * X,
            t = +this.l,
            n = isNaN(this.s) ? 0 : this.s * t * (1 - t),
            i = Math.cos(e),
            o = Math.sin(e);
          return new f(255 * (t + n * (ae * i + le * o)), 255 * (t + n * (se * i + de * o)), 255 * (
            t + n * (ce * i)), this.opacity);
        }
      })), e.color = l, e.cubehelix = N, e.gray = S, e.hcl = M, e.hsl = x, e.lab = E, e.lch = I, e.rgb = u,
      Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
