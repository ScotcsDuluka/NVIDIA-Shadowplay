// ─────────────────────────────────────────────────────────────
// APP MODULE 83
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t() {
      this._x0 = this._y0 = this._x1 = this._y1 = null, this._ = ""
    }

    function n() {
      return new t
    }
    var i = Math.PI,
      o = 2 * i,
      r = 1e-6,
      a = o - r;
    t.prototype = n.prototype = {
      constructor: t,
      moveTo: function(e, t) {
        this._ += "M" + (this._x0 = this._x1 = +e) + "," + (this._y0 = this._y1 = +t)
      },
      closePath: function() {
        null !== this._x1 && (this._x1 = this._x0, this._y1 = this._y0, this._ += "Z")
      },
      lineTo: function(e, t) {
        this._ += "L" + (this._x1 = +e) + "," + (this._y1 = +t)
      },
      quadraticCurveTo: function(e, t, n, i) {
        this._ += "Q" + +e + "," + +t + "," + (this._x1 = +n) + "," + (this._y1 = +i)
      },
      bezierCurveTo: function(e, t, n, i, o, r) {
        this._ += "C" + +e + "," + +t + "," + +n + "," + +i + "," + (this._x1 = +o) + "," + (this._y1 = +r)
      },
      arcTo: function(e, t, n, o, a) {
        e = +e, t = +t, n = +n, o = +o, a = +a;
        var l = this._x1,
          s = this._y1,
          d = n - e,
          c = o - t,
          u = l - e,
          f = s - t,
          m = u * u + f * f;
        if (a < 0) throw new Error("negative radius: " + a);
        if (null === this._x1) this._ += "M" + (this._x1 = e) + "," + (this._y1 = t);
        else if (m > r)
          if (Math.abs(f * d - c * u) > r && a) {
            var g = n - l,
              p = o - s,
              h = d * d + c * c,
              b = g * g + p * p,
              x = Math.sqrt(h),
              v = Math.sqrt(m),
              y = a * Math.tan((i - Math.acos((h + m - b) / (2 * x * v))) / 2),
              w = y / v,
              S = y / x;
            Math.abs(w - 1) > r && (this._ += "L" + (e + w * u) + "," + (t + w * f)), this._ += "A" + a + "," +
              a + ",0,0," + +(f * g > u * p) + "," + (this._x1 = e + S * d) + "," + (this._y1 = t + S * c)
          } else this._ += "L" + (this._x1 = e) + "," + (this._y1 = t);
        else;
      },
      arc: function(e, t, n, l, s, d) {
        e = +e, t = +t, n = +n, d = !!d;
        var c = n * Math.cos(l),
          u = n * Math.sin(l),
          f = e + c,
          m = t + u,
          g = 1 ^ d,
          p = d ? l - s : s - l;
        if (n < 0) throw new Error("negative radius: " + n);
        null === this._x1 ? this._ += "M" + f + "," + m : (Math.abs(this._x1 - f) > r || Math.abs(this._y1 - m) >
          r) && (this._ += "L" + f + "," + m), n && (p < 0 && (p = p % o + o), p > a ? this._ += "A" + n + "," +
          n + ",0,1," + g + "," + (e - c) + "," + (t - u) + "A" + n + "," + n + ",0,1," + g + "," + (this._x1 =
            f) + "," + (this._y1 = m) : p > r && (this._ += "A" + n + "," + n + ",0," + +(p >= i) + "," + g +
            "," + (this._x1 = e + n * Math.cos(s)) + "," + (this._y1 = t + n * Math.sin(s))))
      },
      rect: function(e, t, n, i) {
        this._ += "M" + (this._x0 = this._x1 = +e) + "," + (this._y0 = this._y1 = +t) + "h" + +n + "v" + +i +
          "h" + -n + "Z"
      },
      toString: function() {
        return this._
      }
    }, e.path = n, Object.defineProperty(e, "__esModule", {
      value: !0
    })
  })
}
