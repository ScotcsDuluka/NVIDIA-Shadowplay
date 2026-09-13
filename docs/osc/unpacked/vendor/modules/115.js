// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 115
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

  function i(e) {
    var t = this,
      n = {
        db: null
      };
    if (e)
      for (var r in e) n[r] = "string" != typeof e[r] ? e[r].toString() : e[r];
    var i = new v.default(function(e, r) {
      try {
        n.db = openDatabase(n.name, String(n.version), n.description, n.size)
      } catch (e) {
        return r(e)
      }
      n.db.transaction(function(i) {
        i.executeSql("CREATE TABLE IF NOT EXISTS " + n.storeName +
          " (id INTEGER PRIMARY KEY, key unique, value)", [],
          function() {
            t._dbInfo = n, e()
          },
          function(e, t) {
            r(t)
          })
      })
    });
    return n.serializer = p.default, i
  }

  function o(e, t) {
    var n = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var r = new v.default(function(t, r) {
      n.ready().then(function() {
        var i = n._dbInfo;
        i.db.transaction(function(n) {
          n.executeSql("SELECT * FROM " + i.storeName + " WHERE key = ? LIMIT 1", [e], function(e, n) {
            var r = n.rows.length ? n.rows.item(0).value : null;
            r && (r = i.serializer.deserialize(r)), t(r)
          }, function(e, t) {
            r(t)
          })
        })
      }).catch(r)
    });
    return (0, y.default)(r, t), r
  }

  function a(e, t) {
    var n = this,
      r = new v.default(function(t, r) {
        n.ready().then(function() {
          var i = n._dbInfo;
          i.db.transaction(function(n) {
            n.executeSql("SELECT * FROM " + i.storeName, [], function(n, r) {
              for (var o = r.rows, a = o.length, s = 0; s < a; s++) {
                var c = o.item(s),
                  u = c.value;
                if (u && (u = i.serializer.deserialize(u)), u = e(u, c.key, s + 1), void 0 !== u)
                return void t(u)
              }
              t()
            }, function(e, t) {
              r(t)
            })
          })
        }).catch(r)
      });
    return (0, y.default)(r, t), r
  }

  function s(e, t, n) {
    var r = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var i = new v.default(function(n, i) {
      r.ready().then(function() {
        void 0 === t && (t = null);
        var o = t,
          a = r._dbInfo;
        a.serializer.serialize(t, function(t, r) {
          r ? i(r) : a.db.transaction(function(r) {
            r.executeSql("INSERT OR REPLACE INTO " + a.storeName + " (key, value) VALUES (?, ?)", [e,
              t],
              function() {
                n(o)
              },
              function(e, t) {
                i(t)
              })
          }, function(e) {
            e.code === e.QUOTA_ERR && i(e)
          })
        })
      }).catch(i)
    });
    return (0, y.default)(i, n), i
  }

  function c(e, t) {
    var n = this;
    "string" != typeof e && (console.warn(e + " used as a key, but it is not a string."), e = String(e));
    var r = new v.default(function(t, r) {
      n.ready().then(function() {
        var i = n._dbInfo;
        i.db.transaction(function(n) {
          n.executeSql("DELETE FROM " + i.storeName + " WHERE key = ?", [e], function() {
            t()
          }, function(e, t) {
            r(t)
          })
        })
      }).catch(r)
    });
    return (0, y.default)(r, t), r
  }

  function u(e) {
    var t = this,
      n = new v.default(function(e, n) {
        t.ready().then(function() {
          var r = t._dbInfo;
          r.db.transaction(function(t) {
            t.executeSql("DELETE FROM " + r.storeName, [], function() {
              e()
            }, function(e, t) {
              n(t)
            })
          })
        }).catch(n)
      });
    return (0, y.default)(n, e), n
  }

  function l(e) {
    var t = this,
      n = new v.default(function(e, n) {
        t.ready().then(function() {
          var r = t._dbInfo;
          r.db.transaction(function(t) {
            t.executeSql("SELECT COUNT(key) as c FROM " + r.storeName, [], function(t, n) {
              var r = n.rows.item(0).c;
              e(r)
            }, function(e, t) {
              n(t)
            })
          })
        }).catch(n)
      });
    return (0, y.default)(n, e), n
  }

  function d(e, t) {
    var n = this,
      r = new v.default(function(t, r) {
        n.ready().then(function() {
          var i = n._dbInfo;
          i.db.transaction(function(n) {
            n.executeSql("SELECT key FROM " + i.storeName + " WHERE id = ? LIMIT 1", [e + 1], function(e, n) {
              var r = n.rows.length ? n.rows.item(0).key : null;
              t(r)
            }, function(e, t) {
              r(t)
            })
          })
        }).catch(r)
      });
    return (0, y.default)(r, t), r
  }

  function f(e) {
    var t = this,
      n = new v.default(function(e, n) {
        t.ready().then(function() {
          var r = t._dbInfo;
          r.db.transaction(function(t) {
            t.executeSql("SELECT key FROM " + r.storeName, [], function(t, n) {
              for (var r = [], i = 0; i < n.rows.length; i++) r.push(n.rows.item(i).key);
              e(r)
            }, function(e, t) {
              n(t)
            })
          })
        }).catch(n)
      });
    return (0, y.default)(n, e), n
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var h = n(47),
    p = r(h),
    m = n(33),
    v = r(m),
    g = n(46),
    y = r(g),
    b = {
      _driver: "webSQLStorage",
      _initStorage: i,
      iterate: a,
      getItem: o,
      setItem: s,
      removeItem: c,
      clear: u,
      length: l,
      key: d,
      keys: f
    };
  t.default = b
}
