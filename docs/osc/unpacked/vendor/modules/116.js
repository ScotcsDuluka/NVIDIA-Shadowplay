// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 116
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }

  function i(e, t) {
    e[t] = function() {
      var n = arguments;
      return e.ready().then(function() {
        return e[t].apply(e, n)
      })
    }
  }

  function o() {
    for (var e = 1; e < arguments.length; e++) {
      var t = arguments[e];
      if (t)
        for (var n in t) t.hasOwnProperty(n) && (L(t[n]) ? arguments[0][n] = t[n].slice() : arguments[0][n] = t[n])
    }
    return arguments[0]
  }

  function a(e) {
    for (var t in I)
      if (I.hasOwnProperty(t) && I[t] === e) return !0;
    return !1
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var s = n(26),
    c = r(s),
    u = n(159),
    l = r(u),
    d = n(160),
    f = r(d),
    h = n(118),
    p = r(h),
    m = n(120),
    v = r(m),
    g = n(119),
    y = r(g),
    b = n(113),
    E = r(b),
    _ = n(115),
    $ = r(_),
    w = n(114),
    T = r(w),
    C = n(47),
    x = r(C),
    S = n(33),
    A = r(S),
    M = n(117),
    k = r(M),
    N = {},
    I = {
      INDEXEDDB: "asyncStorage",
      LOCALSTORAGE: "localStorageWrapper",
      WEBSQL: "webSQLStorage"
    },
    O = [I.INDEXEDDB, I.WEBSQL, I.LOCALSTORAGE],
    D = ["clear", "getItem", "iterate", "key", "keys", "length", "removeItem", "setItem"],
    R = {
      description: "",
      driver: O.slice(),
      name: "localforage",
      size: 4980736,
      storeName: "keyvaluepairs",
      version: 1
    },
    P = {};
  P[I.INDEXEDDB] = (0, p.default)(), P[I.WEBSQL] = (0, v.default)(), P[I.LOCALSTORAGE] = (0, y.default)();
  var L = Array.isArray || function(e) {
      return "[object Array]" === Object.prototype.toString.call(e)
    },
    U = function() {
      function e(t) {
        (0, l.default)(this, e), this.INDEXEDDB = I.INDEXEDDB, this.LOCALSTORAGE = I.LOCALSTORAGE, this.WEBSQL = I
          .WEBSQL, this._defaultConfig = o({}, R), this._config = o({}, this._defaultConfig, t), this._driverSet = null,
          this._initDriver = null, this._ready = !1, this._dbInfo = null, this._wrapLibraryMethodsWithReady(), this
          .setDriver(this._config.driver)
      }
      return (0, f.default)(e, [{
        key: "config",
        value: function(e) {
          if ("object" === ("undefined" == typeof e ? "undefined" : (0, c.default)(e))) {
            if (this._ready) return new Error("Can't call config() after localforage has been used.");
            for (var t in e) "storeName" === t && (e[t] = e[t].replace(/\W/g, "_")), this._config[t] = e[t];
            return "driver" in e && e.driver && this.setDriver(this._config.driver), !0
          }
          return "string" == typeof e ? this._config[e] : this._config
        }
      }, {
        key: "defineDriver",
        value: function(e, t, n) {
          var r = new A.default(function(t, n) {
            try {
              var r = e._driver,
                i = new Error(
                  "Custom driver not compliant; see https://mozilla.github.io/localForage/#definedriver"),
                o = new Error("Custom driver name already in use: " + e._driver);
              if (!e._driver) return void n(i);
              if (a(e._driver)) return void n(o);
              for (var s = D.concat("_initStorage"), c = 0; c < s.length; c++) {
                var u = s[c];
                if (!u || !e[u] || "function" != typeof e[u]) return void n(i)
              }
              var l = A.default.resolve(!0);
              "_support" in e && (l = e._support && "function" == typeof e._support ? e._support() : A
                .default.resolve(!!e._support)), l.then(function(n) {
                P[r] = n, N[r] = e, t()
              }, n)
            } catch (e) {
              n(e)
            }
          });
          return (0, k.default)(r, t, n), r
        }
      }, {
        key: "driver",
        value: function() {
          return this._driver || null
        }
      }, {
        key: "getDriver",
        value: function(e, t, n) {
          var r = this,
            i = A.default.resolve().then(function() {
              if (!a(e)) {
                if (N[e]) return N[e];
                throw new Error("Driver not found.")
              }
              switch (e) {
                case r.INDEXEDDB:
                  return E.default;
                case r.LOCALSTORAGE:
                  return T.default;
                case r.WEBSQL:
                  return $.default
              }
            });
          return (0, k.default)(i, t, n), i
        }
      }, {
        key: "getSerializer",
        value: function(e) {
          var t = A.default.resolve(x.default);
          return (0, k.default)(t, e), t
        }
      }, {
        key: "ready",
        value: function(e) {
          var t = this,
            n = t._driverSet.then(function() {
              return null === t._ready && (t._ready = t._initDriver()), t._ready
            });
          return (0, k.default)(n, e, e), n
        }
      }, {
        key: "setDriver",
        value: function(e, t, n) {
          function r() {
            o._config.driver = o.driver()
          }

          function i(e) {
            return function() {
              function t() {
                for (; n < e.length;) {
                  var i = e[n];
                  return n++, o._dbInfo = null, o._ready = null, o.getDriver(i).then(function(e) {
                    return o._extend(e), r(), o._ready = o._initStorage(o._config), o._ready
                  }).catch(t)
                }
                r();
                var a = new Error("No available storage method found.");
                return o._driverSet = A.default.reject(a), o._driverSet
              }
              var n = 0;
              return t()
            }
          }
          var o = this;
          L(e) || (e = [e]);
          var a = this._getSupportedDrivers(e),
            s = null !== this._driverSet ? this._driverSet.catch(function() {
              return A.default.resolve()
            }) : A.default.resolve();
          return this._driverSet = s.then(function() {
            var e = a[0];
            return o._dbInfo = null, o._ready = null, o.getDriver(e).then(function(e) {
              o._driver = e._driver, r(), o._wrapLibraryMethodsWithReady(), o._initDriver = i(a)
            })
          }).catch(function() {
            r();
            var e = new Error("No available storage method found.");
            return o._driverSet = A.default.reject(e), o._driverSet
          }), (0, k.default)(this._driverSet, t, n), this._driverSet
        }
      }, {
        key: "supports",
        value: function(e) {
          return !!P[e]
        }
      }, {
        key: "_extend",
        value: function(e) {
          o(this, e)
        }
      }, {
        key: "_getSupportedDrivers",
        value: function(e) {
          for (var t = [], n = 0, r = e.length; n < r; n++) {
            var i = e[n];
            this.supports(i) && t.push(i)
          }
          return t
        }
      }, {
        key: "_wrapLibraryMethodsWithReady",
        value: function() {
          for (var e = 0; e < D.length; e++) i(this, D[e])
        }
      }, {
        key: "createInstance",
        value: function(t) {
          return new e(t)
        }
      }]), e
    }();
  t.default = new U
}
