// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 64
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t() {
      this._x0 = this._y0 = this._x1 = this._y1 = null, this._ = "";
    }

    function n() {
      return new t();
    }
    var r = Math.PI,
      i = 2 * r,
      o = 1e-6,
      a = i - o;
    t.prototype = n.prototype = {
      constructor: t,
      moveTo: function(e, t) {
        this._ += "M" + (this._x0 = this._x1 = +e) + "," + (this._y0 = this._y1 = +t);
      },
      closePath: function() {
        null !== this._x1 && (this._x1 = this._x0, this._y1 = this._y0, this._ += "Z");
      },
      lineTo: function(e, t) {
        this._ += "L" + (this._x1 = +e) + "," + (this._y1 = +t);
      },
      quadraticCurveTo: function(e, t, n, r) {
        this._ += "Q" + +e + "," + +t + "," + (this._x1 = +n) + "," + (this._y1 = +r);
      },
      bezierCurveTo: function(e, t, n, r, i, o) {
        this._ += "C" + +e + "," + +t + "," + +n + "," + +r + "," + (this._x1 = +i) + "," + (this
          ._y1 = +o);
      },
      arcTo: function(e, t, n, i, a) {
        e = +e, t = +t, n = +n, i = +i, a = +a;
        var s = this._x1,
          c = this._y1,
          u = n - e,
          l = i - t,
          d = s - e,
          f = c - t,
          h = d * d + f * f;
        if (a < 0) throw new Error("negative radius: " + a);
        if (null === this._x1) this._ += "M" + (this._x1 = e) + "," + (this._y1 = t);
        else if (h > o) {
          if (Math.abs(f * u - l * d) > o && a) {
            var p = n - s,
              m = i - c,
              v = u * u + l * l,
              g = p * p + m * m,
              y = Math.sqrt(v),
              b = Math.sqrt(h),
              E = a * Math.tan((r - Math.acos((v + h - g) / (2 * y * b))) / 2),
              _ = E / b,
              $ = E / y;
            Math.abs(_ - 1) > o && (this._ += "L" + (e + _ * d) + "," + (t + _ * f)), this._ += "A" +
              a + "," + a + ",0,0," + +(f * p > d * m) + "," + (this._x1 = e + $ * u) + "," + (this
                ._y1 = t + $ * l);
          } else this._ += "L" + (this._x1 = e) + "," + (this._y1 = t);
        } else;
      },
      arc: function(e, t, n, s, c, u) {
        e = +e, t = +t, n = +n, u = !!u;
        var l = n * Math.cos(s),
          d = n * Math.sin(s),
          f = e + l,
          h = t + d,
          p = 1 ^ u,
          m = u ? s - c : c - s;
        if (n < 0) throw new Error("negative radius: " + n);
        null === this._x1 ? this._ += "M" + f + "," + h : (Math.abs(this._x1 - f) > o || Math.abs(this
          ._y1 - h) > o) && (this._ += "L" + f + "," + h), n && (m < 0 && (m = m % i + i), m > a ?
          this._ += "A" + n + "," + n + ",0,1," + p + "," + (e - l) + "," + (t - d) + "A" + n + "," +
          n + ",0,1," + p + "," + (this._x1 = f) + "," + (this._y1 = h) : m > o && (this._ += "A" +
            n + "," + n + ",0," + +(m >= r) + "," + p + "," + (this._x1 = e + n * Math.cos(c)) + "," +
            (this._y1 = t + n * Math.sin(c))));
      },
      rect: function(e, t, n, r) {
        this._ += "M" + (this._x0 = this._x1 = +e) + "," + (this._y0 = this._y1 = +t) + "h" + +n + "v" +
          +r + "h" + -n + "Z";
      },
      toString: function() {
        return this._;
      }
    }, e.path = n, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
