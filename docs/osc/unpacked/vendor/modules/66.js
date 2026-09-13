// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 66
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t() {
      return b || ($(n), b = _.now() + E)
    }

    function n() {
      b = 0
    }

    function r() {
      this._call = this._time = this._next = null
    }

    function i(e, t, n) {
      var i = new r;
      return i.restart(e, t, n), i
    }

    function o() {
      t(), ++p;
      for (var e, n = f; n;)(e = b - n._time) >= 0 && n._call.call(null, e), n = n._next;
      --p
    }

    function a() {
      b = (y = _.now()) + E, p = m = 0;
      try {
        o()
      } finally {
        p = 0, c(), b = 0
      }
    }

    function s() {
      var e = _.now(),
        t = e - y;
      t > g && (E -= t, y = e)
    }

    function c() {
      for (var e, t, n = f, r = 1 / 0; n;) n._call ? (r > n._time && (r = n._time), e = n, n = n._next) : (t = n
        ._next, n._next = null, n = e ? e._next = t : f = t);
      h = e, u(r)
    }

    function u(e) {
      if (!p) {
        m && (m = clearTimeout(m));
        var t = e - b;
        t > 24 ? (e < 1 / 0 && (m = setTimeout(a, e - _.now() - E)), v && (v = clearInterval(v))) : (v || (y = _
        .now(), v = setInterval(s, g)), p = 1, $(a))
      }
    }

    function l(e, t, n) {
      var i = new r;
      return t = null == t ? 0 : +t, i.restart(function(n) {
        i.stop(), e(n + t)
      }, t, n), i
    }

    function d(e, n, i) {
      var o = new r,
        a = n;
      return null == n ? (o.restart(e, n, i), o) : (n = +n, i = null == i ? t() : +i, o.restart(function t(r) {
        r += a, o.restart(t, a += n, i), e(r)
      }, n, i), o)
    }
    var f, h, p = 0,
      m = 0,
      v = 0,
      g = 1e3,
      y = 0,
      b = 0,
      E = 0,
      _ = "object" == typeof performance && performance.now ? performance : Date,
      $ = "object" == typeof window && window.requestAnimationFrame ? window.requestAnimationFrame.bind(window) :
      function(e) {
        setTimeout(e, 17)
      };
    r.prototype = i.prototype = {
      constructor: r,
      restart: function(e, n, r) {
        if ("function" != typeof e) throw new TypeError("callback is not a function");
        r = (null == r ? t() : +r) + (null == n ? 0 : +n), this._next || h === this || (h ? h._next = this : f =
          this, h = this), this._call = e, this._time = r, u()
      },
      stop: function() {
        this._call && (this._call = null, this._time = 1 / 0, u())
      }
    }, e.interval = d, e.now = t, e.timeout = l, e.timer = i, e.timerFlush = o, Object.defineProperty(e,
      "__esModule", {
        value: !0
      })
  })
}
