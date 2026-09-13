// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 260
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(r) {
    "use strict";

    function i() {}

    function o(e) {
      if ("function" != typeof e) throw new TypeError("resolver must be a function");
      this.state = b, this.queue = [], this.outcome = void 0, r.browser || (this.handled = E), e !== i && u(this, e)
    }

    function a(e, t, n) {
      this.promise = e, "function" == typeof t && (this.onFulfilled = t, this.callFulfilled = this
        .otherCallFulfilled), "function" == typeof n && (this.onRejected = n, this.callRejected = this
          .otherCallRejected)
    }

    function s(e, t, n) {
      m(function() {
        var r;
        try {
          r = t(n)
        } catch (t) {
          return v.reject(e, t)
        }
        r === e ? v.reject(e, new TypeError("Cannot resolve promise with itself")) : v.resolve(e, r)
      })
    }

    function c(e) {
      var t = e && e.then;
      if (e && "object" == typeof e && "function" == typeof t) return function() {
        t.apply(e, arguments)
      }
    }

    function u(e, t) {
      function n(t) {
        o || (o = !0, v.reject(e, t))
      }

      function r(t) {
        o || (o = !0, v.resolve(e, t))
      }

      function i() {
        t(r, n)
      }
      var o = !1,
        a = l(i);
      "error" === a.status && n(a.value)
    }

    function l(e, t) {
      var n = {};
      try {
        n.value = e(t), n.status = "success"
      } catch (e) {
        n.status = "error", n.value = e
      }
      return n
    }

    function d(e) {
      return e instanceof this ? e : v.resolve(new this(i), e)
    }

    function f(e) {
      var t = new this(i);
      return v.reject(t, e)
    }

    function h(e) {
      function t(e, t) {
        function i(e) {
          a[t] = e, ++s !== r || o || (o = !0, v.resolve(u, a))
        }
        n.resolve(e).then(i, function(e) {
          o || (o = !0, v.reject(u, e))
        })
      }
      var n = this;
      if ("[object Array]" !== Object.prototype.toString.call(e)) return this.reject(new TypeError(
        "must be an array"));
      var r = e.length,
        o = !1;
      if (!r) return this.resolve([]);
      for (var a = new Array(r), s = 0, c = -1, u = new this(i); ++c < r;) t(e[c], c);
      return u
    }

    function p(e) {
      function t(e) {
        n.resolve(e).then(function(e) {
          o || (o = !0, v.resolve(s, e))
        }, function(e) {
          o || (o = !0, v.reject(s, e))
        })
      }
      var n = this;
      if ("[object Array]" !== Object.prototype.toString.call(e)) return this.reject(new TypeError(
        "must be an array"));
      var r = e.length,
        o = !1;
      if (!r) return this.resolve([]);
      for (var a = -1, s = new this(i); ++a < r;) t(e[a]);
      return s
    }
    var m = n(259),
      v = {},
      g = ["REJECTED"],
      y = ["FULFILLED"],
      b = ["PENDING"];
    if (!r.browser) var E = ["UNHANDLED"];
    e.exports = t = o, o.prototype.catch = function(e) {
      return this.then(null, e)
    }, o.prototype.then = function(e, t) {
      if ("function" != typeof e && this.state === y || "function" != typeof t && this.state === g) return this;
      var n = new this.constructor(i);
      if (r.browser || this.handled === E && (this.handled = null), this.state !== b) {
        var o = this.state === y ? e : t;
        s(n, o, this.outcome)
      } else this.queue.push(new a(n, e, t));
      return n
    }, a.prototype.callFulfilled = function(e) {
      v.resolve(this.promise, e)
    }, a.prototype.otherCallFulfilled = function(e) {
      s(this.promise, this.onFulfilled, e)
    }, a.prototype.callRejected = function(e) {
      v.reject(this.promise, e)
    }, a.prototype.otherCallRejected = function(e) {
      s(this.promise, this.onRejected, e)
    }, v.resolve = function(e, t) {
      var n = l(c, t);
      if ("error" === n.status) return v.reject(e, n.value);
      var r = n.value;
      if (r) u(e, r);
      else {
        e.state = y, e.outcome = t;
        for (var i = -1, o = e.queue.length; ++i < o;) e.queue[i].callFulfilled(t)
      }
      return e
    }, v.reject = function(e, t) {
      e.state = g, e.outcome = t, r.browser || e.handled === E && m(function() {
        e.handled === E && r.emit("unhandledRejection", t, e)
      });
      for (var n = -1, i = e.queue.length; ++n < i;) e.queue[n].callRejected(t);
      return e
    }, t.resolve = d, t.reject = f, t.all = h, t.race = p
  }).call(t, n(265))
}
