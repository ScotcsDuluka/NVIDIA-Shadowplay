// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 113
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
    for (var t = e.length, n = new ArrayBuffer(t), r = new Uint8Array(n), i = 0; i < t; i++) r[i] = e
      .charCodeAt(i);
    return n;
  }

  function o(e) {
    return new O.default(function(t) {
      var n = (0, M.default)([""]);
      e.objectStore(P).put(n, "key"), e.onabort = function(e) {
        e.preventDefault(), e.stopPropagation(), t(!1);
      }, e.oncomplete = function() {
        var e = navigator.userAgent.match(/Chrome\/(\d+)/),
          n = navigator.userAgent.match(/Edge\//);
        t(n || !e || parseInt(e[1], 10) >= 43);
      };
    }).catch(function() {
      return !1;
    });
  }

  function a(e) {
    return "boolean" == typeof x ? O.default.resolve(x) : o(e).then(function(e) {
      return x = e;
    });
  }

  function s(e) {
    var t = S[e.name],
      n = {};
    n.promise = new O.default(function(e) {
      n.resolve = e;
    }), t.deferredOperations.push(n), t.dbReady ? t.dbReady = t.dbReady.then(function() {
      return n.promise;
    }) : t.dbReady = n.promise;
  }

  function c(e) {
    var t = S[e.name],
      n = t.deferredOperations.pop();
    n && n.resolve();
  }

  function u(e, t) {
    return new O.default(function(n, r) {
      if (e.db) {
        if (!t) return n(e.db);
        s(e), e.db.close();
      }
      var i = [e.name];
      t && i.push(e.version);
      var o = N.default.open.apply(N.default, i);
      t && (o.onupgradeneeded = function(t) {
        var n = o.result;
        try {
          n.createObjectStore(e.storeName), t.oldVersion <= 1 && n.createObjectStore(P);
        } catch (n) {
          if ("ConstraintError" !== n.name) throw n;
          console.warn('The database "' + e.name + '" has been upgraded from version ' + t
            .oldVersion + " to version " + t.newVersion + ', but the storage "' + e.storeName +
            '" already exists.');
        }
      }), o.onerror = function() {
        r(o.error);
      }, o.onsuccess = function() {
        n(o.result), c(e);
      };
    });
  }

  function l(e) {
    return u(e, !1);
  }

  function d(e) {
    return u(e, !0);
  }

  function f(e, t) {
    if (!e.db) return !0;
    var n = !e.db.objectStoreNames.contains(e.storeName),
      r = e.version < e.db.version,
      i = e.version > e.db.version;
    if (r && (e.version !== t && console.warn('The database "' + e.name +
          "\" can't be downgraded from version " + e.db.version + " to version " + e.version + "."), e
        .version = e.db.version), i || n) {
      if (n) {
        var o = e.db.version + 1;
        o > e.version && (e.version = o);
      }
      return !0;
    }
    return !1;
  }

  function h(e) {
    return new O.default(function(t, n) {
      var r = new FileReader();
      r.onerror = n, r.onloadend = function(n) {
        var r = btoa(n.target.result || "");
        t({
          __local_forage_encoded_blob: !0,
          data: r,
          type: e.type
        });
      }, r.readAsBinaryString(e);
    });
  }

  function p(e) {
    var t = i(atob(e.data));
    return (0, M.default)([t], {
      type: e.type
    });
  }

  function m(e) {
    return e && e.__local_forage_encoded_blob;
  }

  function v(e) {
    var t = this,
      n = t._initReady().then(function() {
        var e = S[t._dbInfo.name];
        if (e && e.dbReady) return e.dbReady;
      });
    return n.then(e, e), n;
  }

  function g(e) {
    function t() {
      return O.default.resolve();
    }
    var n = this,
      r = {
        db: null
      };
    if (e)
      for (var i in e) r[i] = e[i];
    S || (S = {});
    var o = S[r.name];
    o || (o = {
      forages: [],
      db: null,
      dbReady: null,
      deferredOperations: []
    }, S[r.name] = o), o.forages.push(n), n._initReady || (n._initReady = n.ready, n.ready = v);
    for (var a = [], s = 0; s < o.forages.length; s++) {
      var c = o.forages[s];
      c !== n && a.push(c._initReady().catch(t));
    }
    var u = o.forages.slice(0);
    return O.default.all(a).then(function() {
      return r.db = o.db, l(r);
    }).then(function(e) {
      return r.db = e, f(r, n._defaultConfig.version) ? d(r) : e;
    }).then(function(e) {
      r.db = o.db = e, n._dbInfo = r;
      for (var t = 0; t < u.length; t++) {
        var i = u[t];
        i !== n && (i._dbInfo.db = r.db, i._dbInfo.version = r.version);
      }
    });
  }

  function y(e, t) {
    var n = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var r = new O.default(function(t, r) {
      n.ready().then(function() {
        var i = n._dbInfo,
          o = i.db.transaction(i.storeName, "readonly").objectStore(i.storeName),
          a = o.get(e);
        a.onsuccess = function() {
          var e = a.result;
          void 0 === e && (e = null), m(e) && (e = p(e)), t(e);
        }, a.onerror = function() {
          r(a.error);
        };
      }).catch(r);
    });
    return (0, R.default)(r, t), r;
  }

  function b(e, t) {
    var n = this,
      r = new O.default(function(t, r) {
        n.ready().then(function() {
          var i = n._dbInfo,
            o = i.db.transaction(i.storeName, "readonly").objectStore(i.storeName),
            a = o.openCursor(),
            s = 1;
          a.onsuccess = function() {
            var n = a.result;
            if (n) {
              var r = n.value;
              m(r) && (r = p(r));
              var i = e(r, n.key, s++);
              void 0 !== i ? t(i) : n.continue();
            } else t();
          }, a.onerror = function() {
            r(a.error);
          };
        }).catch(r);
      });
    return (0, R.default)(r, t), r;
  }

  function E(e, t, n) {
    var r = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var i = new O.default(function(n, i) {
      var o;
      r.ready().then(function() {
        return o = r._dbInfo, t instanceof Blob ? a(o.db).then(function(e) {
          return e ? t : h(t);
        }) : t;
      }).then(function(t) {
        var r = o.db.transaction(o.storeName, "readwrite"),
          a = r.objectStore(o.storeName);
        null === t && (t = void 0), r.oncomplete = function() {
          void 0 === t && (t = null), n(t);
        }, r.onabort = r.onerror = function() {
          var e = s.error ? s.error : s.transaction.error;
          i(e);
        };
        var s = a.put(t, e);
      }).catch(i);
    });
    return (0, R.default)(i, n), i;
  }

  function _(e, t) {
    var n = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var r = new O.default(function(t, r) {
      n.ready().then(function() {
        var i = n._dbInfo,
          o = i.db.transaction(i.storeName, "readwrite"),
          a = o.objectStore(i.storeName),
          s = a.delete(e);
        o.oncomplete = function() {
          t();
        }, o.onerror = function() {
          r(s.error);
        }, o.onabort = function() {
          var e = s.error ? s.error : s.transaction.error;
          r(e);
        };
      }).catch(r);
    });
    return (0, R.default)(r, t), r;
  }

  function $(e) {
    var t = this,
      n = new O.default(function(e, n) {
        t.ready().then(function() {
          var r = t._dbInfo,
            i = r.db.transaction(r.storeName, "readwrite"),
            o = i.objectStore(r.storeName),
            a = o.clear();
          i.oncomplete = function() {
            e();
          }, i.onabort = i.onerror = function() {
            var e = a.error ? a.error : a.transaction.error;
            n(e);
          };
        }).catch(n);
      });
    return (0, R.default)(n, e), n;
  }

  function w(e) {
    var t = this,
      n = new O.default(function(e, n) {
        t.ready().then(function() {
          var r = t._dbInfo,
            i = r.db.transaction(r.storeName, "readonly").objectStore(r.storeName),
            o = i.count();
          o.onsuccess = function() {
            e(o.result);
          }, o.onerror = function() {
            n(o.error);
          };
        }).catch(n);
      });
    return (0, R.default)(n, e), n;
  }

  function T(e, t) {
    var n = this,
      r = new O.default(function(t, r) {
        return e < 0 ? void t(null) : void n.ready().then(function() {
          var i = n._dbInfo,
            o = i.db.transaction(i.storeName, "readonly").objectStore(i.storeName),
            a = !1,
            s = o.openCursor();
          s.onsuccess = function() {
            var n = s.result;
            return n ? void(0 === e ? t(n.key) : a ? t(n.key) : (a = !0, n.advance(e))) : void t(
            null);
          }, s.onerror = function() {
            r(s.error);
          };
        }).catch(r);
      });
    return (0, R.default)(r, t), r;
  }

  function C(e) {
    var t = this,
      n = new O.default(function(e, n) {
        t.ready().then(function() {
          var r = t._dbInfo,
            i = r.db.transaction(r.storeName, "readonly").objectStore(r.storeName),
            o = i.openCursor(),
            a = [];
          o.onsuccess = function() {
            var t = o.result;
            return t ? (a.push(t.key), void t.continue()) : void e(a);
          }, o.onerror = function() {
            n(o.error);
          };
        }).catch(n);
      });
    return (0, R.default)(n, e), n;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var x,
    S,
    A = require(73),
    M = r(A),
    k = require(74),
    N = r(k),
    I = require(33),
    O = r(I),
    D = require(46),
    R = r(D),
    P = "local-forage-detect-blob-support",
    L = {
      _driver: "asyncStorage",
      _initStorage: g,
      iterate: b,
      getItem: y,
      setItem: E,
      removeItem: _,
      clear: $,
      length: w,
      key: T,
      keys: C
    };
  exports.default = L;
}
