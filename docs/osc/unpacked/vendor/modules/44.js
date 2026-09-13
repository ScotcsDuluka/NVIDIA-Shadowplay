// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 44
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e, t, n) {
      e.prototype = t.prototype = n, n.constructor = e
    }

    function n(e, t) {
      var n = Object.create(e.prototype);
      for (var r in t) n[r] = t[r];
      return n
    }

    function r() {}

    function i() {
      return this.rgb().formatHex()
    }

    function o() {
      return g(this).formatHsl()
    }

    function a() {
      return this.rgb().formatRgb()
    }

    function s(e) {
      var t, n;
      return e = (e + "").trim().toLowerCase(), (t = B.exec(e)) ? (n = t[1].length, t = parseInt(t[1], 16), 6 === n ?
          c(t) : 3 === n ? new f(t >> 8 & 15 | t >> 4 & 240, t >> 4 & 15 | 240 & t, (15 & t) << 4 | 15 & t, 1) : 8 ===
          n ? u(t >> 24 & 255, t >> 16 & 255, t >> 8 & 255, (255 & t) / 255) : 4 === n ? u(t >> 12 & 15 | t >> 8 &
            240, t >> 8 & 15 | t >> 4 & 240, t >> 4 & 15 | 240 & t, ((15 & t) << 4 | 15 & t) / 255) : null) : (t = z
          .exec(e)) ? new f(t[1], t[2], t[3], 1) : (t = q.exec(e)) ? new f(255 * t[1] / 100, 255 * t[2] / 100, 255 *
          t[3] / 100, 1) : (t = G.exec(e)) ? u(t[1], t[2], t[3], t[4]) : (t = V.exec(e)) ? u(255 * t[1] / 100, 255 *
          t[2] / 100, 255 * t[3] / 100, t[4]) : (t = W.exec(e)) ? v(t[1], t[2] / 100, t[3] / 100, 1) : (t = Y.exec(
        e)) ? v(t[1], t[2] / 100, t[3] / 100, t[4]) : K.hasOwnProperty(e) ? c(K[e]) : "transparent" === e ? new f(NaN,
          NaN, NaN, 0) : null
    }

    function c(e) {
      return new f(e >> 16 & 255, e >> 8 & 255, 255 & e, 1)
    }

    function u(e, t, n, r) {
      return r <= 0 && (e = t = n = NaN), new f(e, t, n, r)
    }

    function l(e) {
      return e instanceof r || (e = s(e)), e ? (e = e.rgb(), new f(e.r, e.g, e.b, e.opacity)) : new f
    }

    function d(e, t, n, r) {
      return 1 === arguments.length ? l(e) : new f(e, t, n, null == r ? 1 : r)
    }

    function f(e, t, n, r) {
      this.r = +e, this.g = +t, this.b = +n, this.opacity = +r
    }

    function h() {
      return "#" + m(this.r) + m(this.g) + m(this.b)
    }

    function p() {
      var e = this.opacity;
      return e = isNaN(e) ? 1 : Math.max(0, Math.min(1, e)), (1 === e ? "rgb(" : "rgba(") + Math.max(0, Math.min(255,
        Math.round(this.r) || 0)) + ", " + Math.max(0, Math.min(255, Math.round(this.g) || 0)) + ", " + Math.max(0,
        Math.min(255, Math.round(this.b) || 0)) + (1 === e ? ")" : ", " + e + ")")
    }

    function m(e) {
      return e = Math.max(0, Math.min(255, Math.round(e) || 0)), (e < 16 ? "0" : "") + e.toString(16)
    }

    function v(e, t, n, r) {
      return r <= 0 ? e = t = n = NaN : n <= 0 || n >= 1 ? e = t = NaN : t <= 0 && (e = NaN), new b(e, t, n, r)
    }

    function g(e) {
      if (e instanceof b) return new b(e.h, e.s, e.l, e.opacity);
      if (e instanceof r || (e = s(e)), !e) return new b;
      if (e instanceof b) return e;
      e = e.rgb();
      var t = e.r / 255,
        n = e.g / 255,
        i = e.b / 255,
        o = Math.min(t, n, i),
        a = Math.max(t, n, i),
        c = NaN,
        u = a - o,
        l = (a + o) / 2;
      return u ? (c = t === a ? (n - i) / u + 6 * (n < i) : n === a ? (i - t) / u + 2 : (t - n) / u + 4, u /= l < .5 ?
        a + o : 2 - a - o, c *= 60) : u = l > 0 && l < 1 ? 0 : c, new b(c, u, l, e.opacity)
    }

    function y(e, t, n, r) {
      return 1 === arguments.length ? g(e) : new b(e, t, n, null == r ? 1 : r)
    }

    function b(e, t, n, r) {
      this.h = +e, this.s = +t, this.l = +n, this.opacity = +r
    }

    function E(e, t, n) {
      return 255 * (e < 60 ? t + (n - t) * e / 60 : e < 180 ? n : e < 240 ? t + (n - t) * (240 - e) / 60 : t)
    }

    function _(e) {
      if (e instanceof T) return new T(e.l, e.a, e.b, e.opacity);
      if (e instanceof I) return O(e);
      e instanceof f || (e = l(e));
      var t, n, r = A(e.r),
        i = A(e.g),
        o = A(e.b),
        a = C((.2225045 * r + .7168786 * i + .0606169 * o) / ee);
      return r === i && i === o ? t = n = a : (t = C((.4360747 * r + .3850649 * i + .1430804 * o) / Z), n = C((
        .0139322 * r + .0971045 * i + .7141733 * o) / te)), new T(116 * a - 16, 500 * (t - a), 200 * (a - n), e
        .opacity)
    }

    function $(e, t) {
      return new T(e, 0, 0, null == t ? 1 : t)
    }

    function w(e, t, n, r) {
      return 1 === arguments.length ? _(e) : new T(e, t, n, null == r ? 1 : r)
    }

    function T(e, t, n, r) {
      this.l = +e, this.a = +t, this.b = +n, this.opacity = +r
    }

    function C(e) {
      return e > oe ? Math.pow(e, 1 / 3) : e / ie + ne
    }

    function x(e) {
      return e > re ? e * e * e : ie * (e - ne)
    }

    function S(e) {
      return 255 * (e <= .0031308 ? 12.92 * e : 1.055 * Math.pow(e, 1 / 2.4) - .055)
    }

    function A(e) {
      return (e /= 255) <= .04045 ? e / 12.92 : Math.pow((e + .055) / 1.055, 2.4)
    }

    function M(e) {
      if (e instanceof I) return new I(e.h, e.c, e.l, e.opacity);
      if (e instanceof T || (e = _(e)), 0 === e.a && 0 === e.b) return new I(NaN, 0 < e.l && e.l < 100 ? 0 : NaN, e.l,
        e.opacity);
      var t = Math.atan2(e.b, e.a) * Q;
      return new I(t < 0 ? t + 360 : t, Math.sqrt(e.a * e.a + e.b * e.b), e.l, e.opacity)
    }

    function k(e, t, n, r) {
      return 1 === arguments.length ? M(e) : new I(n, t, e, null == r ? 1 : r)
    }

    function N(e, t, n, r) {
      return 1 === arguments.length ? M(e) : new I(e, t, n, null == r ? 1 : r)
    }

    function I(e, t, n, r) {
      this.h = +e, this.c = +t, this.l = +n, this.opacity = +r
    }

    function O(e) {
      if (isNaN(e.h)) return new T(e.l, 0, 0, e.opacity);
      var t = e.h * X;
      return new T(e.l, Math.cos(t) * e.c, Math.sin(t) * e.c, e.opacity)
    }

    function D(e) {
      if (e instanceof P) return new P(e.h, e.s, e.l, e.opacity);
      e instanceof f || (e = l(e));
      var t = e.r / 255,
        n = e.g / 255,
        r = e.b / 255,
        i = (he * r + de * t - fe * n) / (he + de - fe),
        o = r - i,
        a = (le * (n - i) - ce * o) / ue,
        s = Math.sqrt(a * a + o * o) / (le * i * (1 - i)),
        c = s ? Math.atan2(a, o) * Q - 120 : NaN;
      return new P(c < 0 ? c + 360 : c, s, i, e.opacity)
    }

    function R(e, t, n, r) {
      return 1 === arguments.length ? D(e) : new P(e, t, n, null == r ? 1 : r)
    }

    function P(e, t, n, r) {
      this.h = +e, this.s = +t, this.l = +n, this.opacity = +r
    }
    var L = .7,
      U = 1 / L,
      F = "\\s*([+-]?\\d+)\\s*",
      j = "\\s*([+-]?\\d*\\.?\\d+(?:[eE][+-]?\\d+)?)\\s*",
      H = "\\s*([+-]?\\d*\\.?\\d+(?:[eE][+-]?\\d+)?)%\\s*",
      B = /^#([0-9a-f]{3,8})$/,
      z = new RegExp("^rgb\\(" + [F, F, F] + "\\)$"),
      q = new RegExp("^rgb\\(" + [H, H, H] + "\\)$"),
      G = new RegExp("^rgba\\(" + [F, F, F, j] + "\\)$"),
      V = new RegExp("^rgba\\(" + [H, H, H, j] + "\\)$"),
      W = new RegExp("^hsl\\(" + [j, H, H] + "\\)$"),
      Y = new RegExp("^hsla\\(" + [j, H, H, j] + "\\)$"),
      K = {
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
    t(r, s, {
      copy: function(e) {
        return Object.assign(new this.constructor, this, e)
      },
      displayable: function() {
        return this.rgb().displayable()
      },
      hex: i,
      formatHex: i,
      formatHsl: o,
      formatRgb: a,
      toString: a
    }), t(f, d, n(r, {
      brighter: function(e) {
        return e = null == e ? U : Math.pow(U, e), new f(this.r * e, this.g * e, this.b * e, this.opacity)
      },
      darker: function(e) {
        return e = null == e ? L : Math.pow(L, e), new f(this.r * e, this.g * e, this.b * e, this.opacity)
      },
      rgb: function() {
        return this
      },
      displayable: function() {
        return -.5 <= this.r && this.r < 255.5 && -.5 <= this.g && this.g < 255.5 && -.5 <= this.b && this.b <
          255.5 && 0 <= this.opacity && this.opacity <= 1
      },
      hex: h,
      formatHex: h,
      formatRgb: p,
      toString: p
    })), t(b, y, n(r, {
      brighter: function(e) {
        return e = null == e ? U : Math.pow(U, e), new b(this.h, this.s, this.l * e, this.opacity)
      },
      darker: function(e) {
        return e = null == e ? L : Math.pow(L, e), new b(this.h, this.s, this.l * e, this.opacity)
      },
      rgb: function() {
        var e = this.h % 360 + 360 * (this.h < 0),
          t = isNaN(e) || isNaN(this.s) ? 0 : this.s,
          n = this.l,
          r = n + (n < .5 ? n : 1 - n) * t,
          i = 2 * n - r;
        return new f(E(e >= 240 ? e - 240 : e + 120, i, r), E(e, i, r), E(e < 120 ? e + 240 : e - 120, i, r),
          this.opacity)
      },
      displayable: function() {
        return (0 <= this.s && this.s <= 1 || isNaN(this.s)) && 0 <= this.l && this.l <= 1 && 0 <= this
          .opacity && this.opacity <= 1
      },
      formatHsl: function() {
        var e = this.opacity;
        return e = isNaN(e) ? 1 : Math.max(0, Math.min(1, e)), (1 === e ? "hsl(" : "hsla(") + (this.h || 0) +
          ", " + 100 * (this.s || 0) + "%, " + 100 * (this.l || 0) + "%" + (1 === e ? ")" : ", " + e + ")")
      }
    }));
    var X = Math.PI / 180,
      Q = 180 / Math.PI,
      J = 18,
      Z = .96422,
      ee = 1,
      te = .82521,
      ne = 4 / 29,
      re = 6 / 29,
      ie = 3 * re * re,
      oe = re * re * re;
    t(T, w, n(r, {
      brighter: function(e) {
        return new T(this.l + J * (null == e ? 1 : e), this.a, this.b, this.opacity)
      },
      darker: function(e) {
        return new T(this.l - J * (null == e ? 1 : e), this.a, this.b, this.opacity)
      },
      rgb: function() {
        var e = (this.l + 16) / 116,
          t = isNaN(this.a) ? e : e + this.a / 500,
          n = isNaN(this.b) ? e : e - this.b / 200;
        return t = Z * x(t), e = ee * x(e), n = te * x(n), new f(S(3.1338561 * t - 1.6168667 * e - .4906146 *
          n), S(-.9787684 * t + 1.9161415 * e + .033454 * n), S(.0719453 * t - .2289914 * e + 1.4052427 *
          n), this.opacity)
      }
    })), t(I, N, n(r, {
      brighter: function(e) {
        return new I(this.h, this.c, this.l + J * (null == e ? 1 : e), this.opacity)
      },
      darker: function(e) {
        return new I(this.h, this.c, this.l - J * (null == e ? 1 : e), this.opacity)
      },
      rgb: function() {
        return O(this).rgb()
      }
    }));
    var ae = -.14861,
      se = 1.78277,
      ce = -.29227,
      ue = -.90649,
      le = 1.97294,
      de = le * ue,
      fe = le * se,
      he = se * ce - ue * ae;
    t(P, R, n(r, {
        brighter: function(e) {
          return e = null == e ? U : Math.pow(U, e), new P(this.h, this.s, this.l * e, this.opacity)
        },
        darker: function(e) {
          return e = null == e ? L : Math.pow(L, e), new P(this.h, this.s, this.l * e, this.opacity)
        },
        rgb: function() {
          var e = isNaN(this.h) ? 0 : (this.h + 120) * X,
            t = +this.l,
            n = isNaN(this.s) ? 0 : this.s * t * (1 - t),
            r = Math.cos(e),
            i = Math.sin(e);
          return new f(255 * (t + n * (ae * r + se * i)), 255 * (t + n * (ce * r + ue * i)), 255 * (t + n * (
            le * r)), this.opacity)
        }
      })), e.color = s, e.cubehelix = R, e.gray = $, e.hcl = N, e.hsl = y, e.lab = w, e.lch = k, e.rgb = d, Object
      .defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
