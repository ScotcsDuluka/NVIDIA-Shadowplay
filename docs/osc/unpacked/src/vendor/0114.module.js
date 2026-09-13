// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 114
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }

  function i(e) {
    var t = this,
      n = {};
    if (e)
      for (var r in e) n[r] = e[r];
    return n.keyPrefix = n.name + "/", n.storeName !== t._defaultConfig.storeName && (n.keyPrefix += n
      .storeName + "/"), t._dbInfo = n, n.serializer = p.default, v.default.resolve();
  }

  function o(e) {
    var t = this,
      n = t.ready().then(function() {
        for (var e = t._dbInfo.keyPrefix, n = localStorage.length - 1; n >= 0; n--) {
          var r = localStorage.key(n);
          0 === r.indexOf(e) && localStorage.removeItem(r);
        }
      });
    return (0, y.default)(n, e), n;
  }

  function a(e, t) {
    var n = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var r = n.ready().then(function() {
      var t = n._dbInfo,
        r = localStorage.getItem(t.keyPrefix + e);
      return r && (r = t.serializer.deserialize(r)), r;
    });
    return (0, y.default)(r, t), r;
  }

  function s(e, t) {
    var n = this,
      r = n.ready().then(function() {
        for (var t = n._dbInfo, r = t.keyPrefix, i = r.length, o = localStorage.length, a = 1, s = 0; s <
          o; s++) {
          var c = localStorage.key(s);
          if (0 === c.indexOf(r)) {
            var u = localStorage.getItem(c);
            if (u && (u = t.serializer.deserialize(u)), u = e(u, c.substring(i), a++), void 0 !== u)
            return u;
          }
        }
      });
    return (0, y.default)(r, t), r;
  }

  function c(e, t) {
    var n = this,
      r = n.ready().then(function() {
        var t,
          r = n._dbInfo;
        try {
          t = localStorage.key(e);
        } catch (e) {
          t = null;
        }
        return t && (t = t.substring(r.keyPrefix.length)), t;
      });
    return (0, y.default)(r, t), r;
  }

  function u(e) {
    var t = this,
      n = t.ready().then(function() {
        for (var e = t._dbInfo, n = localStorage.length, r = [], i = 0; i < n; i++) 0 === localStorage.key(
          i).indexOf(e.keyPrefix) && r.push(localStorage.key(i).substring(e.keyPrefix.length));
        return r;
      });
    return (0, y.default)(n, e), n;
  }

  function l(e) {
    var t = this,
      n = t.keys().then(function(e) {
        return e.length;
      });
    return (0, y.default)(n, e), n;
  }

  function d(e, t) {
    var n = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var r = n.ready().then(function() {
      var t = n._dbInfo;
      localStorage.removeItem(t.keyPrefix + e);
    });
    return (0, y.default)(r, t), r;
  }

  function f(e, t, n) {
    var r = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var i = r.ready().then(function() {
      void 0 === t && (t = null);
      var n = t;
      return new v.default(function(i, o) {
        var a = r._dbInfo;
        a.serializer.serialize(t, function(t, r) {
          if (r) o(r);
          else try {
            localStorage.setItem(a.keyPrefix + e, t), i(n);
          } catch (e) {
            "QuotaExceededError" !== e.name && "NS_ERROR_DOM_QUOTA_REACHED" !== e.name || o(e),
              o(e);
          }
        });
      });
    });
    return (0, y.default)(i, n), i;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var h = require(47),
    p = r(h),
    m = require(33),
    v = r(m),
    g = require(46),
    y = r(g),
    b = {
      _driver: "localStorageWrapper",
      _initStorage: i,
      iterate: s,
      getItem: a,
      setItem: f,
      removeItem: d,
      clear: o,
      length: l,
      key: c,
      keys: u
    };
  exports.default = b;
}
