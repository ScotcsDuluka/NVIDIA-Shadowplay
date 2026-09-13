// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 209
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var r, i, o, a, s = n(28),
    c = n(4),
    u = n(36),
    l = n(48),
    d = n(8),
    f = n(12),
    h = n(35),
    p = n(179),
    m = n(182),
    v = n(92),
    g = n(93).set,
    y = n(191)(),
    b = n(52),
    E = n(89),
    _ = n(201),
    $ = n(90),
    w = "Promise",
    T = c.TypeError,
    C = c.process,
    x = C && C.versions,
    S = x && x.v8 || "",
    A = c[w],
    M = "process" == l(C),
    k = function() {},
    N = i = b.f,
    I = !! function() {
      try {
        var e = A.resolve(1),
          t = (e.constructor = {})[n(5)("species")] = function(e) {
            e(k, k)
          };
        return (M || "function" == typeof PromiseRejectionEvent) && e.then(k) instanceof t && 0 !== S.indexOf("6.6") &&
          _.indexOf("Chrome/66") === -1
      } catch (e) {}
    }(),
    O = function(e) {
      var t;
      return !(!f(e) || "function" != typeof(t = e.then)) && t
    },
    D = function(e, t) {
      if (!e._n) {
        e._n = !0;
        var n = e._c;
        y(function() {
          for (var r = e._v, i = 1 == e._s, o = 0, a = function(t) {
              var n, o, a, s = i ? t.ok : t.fail,
                c = t.resolve,
                u = t.reject,
                l = t.domain;
              try {
                s ? (i || (2 == e._h && L(e), e._h = 1), s === !0 ? n = r : (l && l.enter(), n = s(r), l && (l
                  .exit(), a = !0)), n === t.promise ? u(T("Promise-chain cycle")) : (o = O(n)) ? o.call(n, c,
                  u) : c(n)) : u(r)
              } catch (e) {
                l && !a && l.exit(), u(e)
              }
            }; n.length > o;) a(n[o++]);
          e._c = [], e._n = !1, t && !e._h && R(e)
        })
      }
    },
    R = function(e) {
      g.call(c, function() {
        var t, n, r, i = e._v,
          o = P(e);
        if (o && (t = E(function() {
            M ? C.emit("unhandledRejection", i, e) : (n = c.onunhandledrejection) ? n({
              promise: e,
              reason: i
            }) : (r = c.console) && r.error && r.error("Unhandled promise rejection", i)
          }), e._h = M || P(e) ? 2 : 1), e._a = void 0, o && t.e) throw t.v
      })
    },
    P = function(e) {
      return 1 !== e._h && 0 === (e._a || e._c).length
    },
    L = function(e) {
      g.call(c, function() {
        var t;
        M ? C.emit("rejectionHandled", e) : (t = c.onrejectionhandled) && t({
          promise: e,
          reason: e._v
        })
      })
    },
    U = function(e) {
      var t = this;
      t._d || (t._d = !0, t = t._w || t, t._v = e, t._s = 2, t._a || (t._a = t._c.slice()), D(t, !0))
    },
    F = function(e) {
      var t, n = this;
      if (!n._d) {
        n._d = !0, n = n._w || n;
        try {
          if (n === e) throw T("Promise can't be resolved itself");
          (t = O(e)) ? y(function() {
            var r = {
              _w: n,
              _d: !1
            };
            try {
              t.call(e, u(F, r, 1), u(U, r, 1))
            } catch (e) {
              U.call(r, e)
            }
          }): (n._v = e, n._s = 1, D(n, !1))
        } catch (e) {
          U.call({
            _w: n,
            _d: !1
          }, e)
        }
      }
    };
  I || (A = function(e) {
    p(this, A, w, "_h"), h(e), r.call(this);
    try {
      e(u(F, this, 1), u(U, this, 1))
    } catch (e) {
      U.call(this, e)
    }
  }, r = function(e) {
    this._c = [], this._a = void 0, this._s = 0, this._d = !1, this._v = void 0, this._h = 0, this._n = !1
  }, r.prototype = n(197)(A.prototype, {
    then: function(e, t) {
      var n = N(v(this, A));
      return n.ok = "function" != typeof e || e, n.fail = "function" == typeof t && t, n.domain = M ? C.domain :
        void 0, this._c.push(n), this._a && this._a.push(n), this._s && D(this, !1), n.promise
    },
    catch: function(e) {
      return this.then(void 0, e)
    }
  }), o = function() {
    var e = new r;
    this.promise = e, this.resolve = u(F, e, 1), this.reject = u(U, e, 1)
  }, b.f = N = function(e) {
    return e === A || e === a ? new o(e) : i(e)
  }), d(d.G + d.W + d.F * !I, {
    Promise: A
  }), n(38)(A, w), n(198)(w), a = n(2)[w], d(d.S + d.F * !I, w, {
    reject: function(e) {
      var t = N(this),
        n = t.reject;
      return n(e), t.promise
    }
  }), d(d.S + d.F * (s || !I), w, {
    resolve: function(e) {
      return $(s && this === a ? A : this, e)
    }
  }), d(d.S + d.F * !(I && n(188)(function(e) {
    A.all(e).catch(k)
  })), w, {
    all: function(e) {
      var t = this,
        n = N(t),
        r = n.resolve,
        i = n.reject,
        o = E(function() {
          var n = [],
            o = 0,
            a = 1;
          m(e, !1, function(e) {
            var s = o++,
              c = !1;
            n.push(void 0), a++, t.resolve(e).then(function(e) {
              c || (c = !0, n[s] = e, --a || r(n))
            }, i)
          }), --a || r(n)
        });
      return o.e && i(o.v), n.promise
    },
    race: function(e) {
      var t = this,
        n = N(t),
        r = n.reject,
        i = E(function() {
          m(e, !1, function(e) {
            t.resolve(e).then(n.resolve, r)
          })
        });
      return i.e && r(i.v), n.promise
    }
  })
}
