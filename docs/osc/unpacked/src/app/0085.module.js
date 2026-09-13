// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 85
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t() {
      return v || (S(n), v = w.now() + y);
    }

    function n() {
      v = 0;
    }

    function i() {
      this._call = this._time = this._next = null;
    }

    function o(e, t, n) {
      var o = new i();
      return o.restart(e, t, n), o;
    }

    function r() {
      t(), ++g;
      for (var e, n = f; n;)(e = v - n._time) >= 0 && n._call.call(null, e), n = n._next;
      --g;
    }

    function a() {
      v = (x = w.now()) + y, g = p = 0;
      try {
        r();
      } finally {
        g = 0, s(), v = 0;
      }
    }

    function l() {
      var e = w.now(),
        t = e - x;
      t > b && (y -= t, x = e);
    }

    function s() {
      for (var e, t, n = f, i = 1 / 0; n;) n._call ? (i > n._time && (i = n._time), e = n, n = n._next) : (
        t = n._next, n._next = null, n = e ? e._next = t : f = t);
      m = e, d(i);
    }

    function d(e) {
      if (!g) {
        p && (p = clearTimeout(p));
        var t = e - v;
        t > 24 ? (e < 1 / 0 && (p = setTimeout(a, e - w.now() - y)), h && (h = clearInterval(h))) : (h || (
          x = w.now(), h = setInterval(l, b)), g = 1, S(a));
      }
    }

    function c(e, t, n) {
      var o = new i();
      return t = null == t ? 0 : +t, o.restart(function(n) {
        o.stop(), e(n + t);
      }, t, n), o;
    }

    function u(e, n, o) {
      var r = new i(),
        a = n;
      return null == n ? (r.restart(e, n, o), r) : (n = +n, o = null == o ? t() : +o, r.restart(function t(
        i) {
        i += a, r.restart(t, a += n, o), e(i);
      }, n, o), r);
    }
    var f,
      m,
      g = 0,
      p = 0,
      h = 0,
      b = 1e3,
      x = 0,
      v = 0,
      y = 0,
      w = "object" == typeof performance && performance.now ? performance : Date,
      S = "object" == typeof window && window.requestAnimationFrame ? window.requestAnimationFrame.bind(
        window) : function(e) {
        setTimeout(e, 17);
      };
    i.prototype = o.prototype = {
      constructor: i,
      restart: function(e, n, i) {
        if ("function" != typeof e) throw new TypeError("callback is not a function");
        i = (null == i ? t() : +i) + (null == n ? 0 : +n), this._next || m === this || (m ? m._next =
          this : f = this, m = this), this._call = e, this._time = i, d();
      },
      stop: function() {
        this._call && (this._call = null, this._time = 1 / 0, d());
      }
    }, e.interval = u, e.now = t, e.timeout = c, e.timer = o, e.timerFlush = r, Object.defineProperty(e,
      "__esModule", {
        value: !0
      });
  });
}
