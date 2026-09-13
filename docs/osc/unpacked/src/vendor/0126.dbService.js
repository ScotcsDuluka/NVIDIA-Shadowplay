// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 126
// provider dbService | provider dbCacheService | constant DB_SERVICE_EVENTS | defines angular.module("nvDbService")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  angular.module("nvDbService", ["crimson", "ngEventAggregator"]), angular.module("nvDbService").constant(
    "DB_SERVICE_EVENTS", {
      DATABASE_ITEM_CHANGE: "nvDbService.dbItemChange"
    }), angular.module("nvDbService").provider("dbService", [function() {
    function e(e, t) {
      if (!e.storeName) throw new Error("Error: must specify storeName in config argument");
      if (e.storeName in t) throw new Error("Error: store name already exists", e.storeName);
      return e.name = f, e.driver = c, t[e.storeName] = {
        config: e,
        upgrades: {}
      }, this;
    }

    function t(e, t, n, r) {
      return r[e].upgrades[t] = n, r[e].upgradeTargetVersion = t, this;
    }

    function n(e) {
      return f = e, this;
    }

    function r(t) {
      return e.call(this, t, l);
    }

    function i(t) {
      return e.call(this, t, d);
    }

    function o(e, n, r) {
      return t.call(this, e, n, r, l);
    }

    function a(e, n, r) {
      return t.call(this, e, n, r, d);
    }
    var s = "_version",
      c = "asyncStorage",
      u = {},
      l = {},
      d = {},
      f = "";
    return {
      init: n,
      defineGlobalStore: r,
      upgradeGlobalStore: o,
      defineUserStore: i,
      upgradeUserStore: a,
      $get: ["$window", "$log", "$q", "eventAggregator", "DB_SERVICE_EVENTS", function(e, t, n, r, i) {
        function o(e, t, r, i) {
          return t.getItem(s).then(function(o) {
            var a = o || 0;
            if (a > i) return n.reject(e, "Database downgrade not supported dbVersion=" + a +
              "target=" + i);
            var c = n.resolve();
            return angular.forEach(r, function(r, i) {
              i > a && (c = c.then(function() {
                return v.info(e, "Attempt upgrade from version " + a + " to " + i), r(
                  t).then(function() {
                  return v.info(e, "Successfully upgraded to " + i), a = i, t
                    .setItem(s, a).catch(function(t) {
                      return n.reject(e, "Failed to persist new db version " +
                        a, t);
                    });
                }).catch(function(t) {
                  return n.reject(e, "Failed to upgrade to db version from " + a +
                    " to " + i, t);
                });
              }));
            }), c.then(function() {
              return v.info("Using datastore " + e + " version " + a), t;
            });
          });
        }

        function a(e, t) {
          var r = Array.prototype.slice.call(arguments, 2);
          return n.when(e).then(function(e) {
            return e[t].apply(e, r).then(function(e) {
              return e;
            });
          });
        }

        function h(e) {
          function t() {
            var t;
            l.name = e.storeName, l.dbName = e.storeDbName, l.userId = e.userId, e.storeDef ? (t = _
              .extend({}, e.storeDef.config, {
                storeName: e.storeDbName
              }), s = g.createInstance(t), u = o(e.storeName, s, e.storeDef.upgrades, e.storeDef
                .upgradeTargetVersion)) : (s = g.createInstance({
              name: f,
              storeName: e.storeDbName,
              driver: c
            }), u = n.resolve(s));
          }
          var s,
            u,
            l = this;
          t(), angular.forEach(["getItem", "setItem", "removeItem", "clear", "length", "keys",
            "iterate"
          ], function(e) {
            l[e] = a.bind(l, u, e);
          }), e.storeDef && (l.setItem = function(e, t) {
            return _.isObject(t) && (t.ts = Date.now()), a.call(l, u, "setItem", e, t).then(
              function() {
                r.trigger(i.DATABASE_ITEM_CHANGE, {
                  storeName: l.name,
                  userId: l.userId,
                  key: e
                });
              });
          }), l.connected = function() {
            return u.then(function(e) {
              return !0;
            }, function() {
              return !1;
            });
          };
        }

        function p(e) {
          return u[e.storeDbName] || (t.info("Create datastore instance", e.storeDbName), u[e
            .storeDbName] = new h(e)), u[e.storeDbName];
        }
        var m = {},
          v = t.getInstance("nvDbService/dbService"),
          g = e.localforage;
        return m.getGlobalStore = function(e) {
          return p({
            storeName: e,
            storeDbName: e,
            storeDef: l[e]
          });
        }, m.getUserStore = function(e, t) {
          var n = e + "_" + t;
          return p({
            storeName: t,
            storeDbName: n,
            userId: e,
            storeDef: d[t]
          });
        }, m.getStore = function(e) {
          return p({
            storeName: e,
            storeDbName: e
          });
        }, m;
      }]
    };
  }]), angular.module("nvDbService").provider("dbCacheService", [function() {
    function e(e, t) {
      return e ? n.getUserStore(e, t) : n.getGlobalStore(t);
    }

    function t(t, n, i) {
      function o(e) {
        var t = s.status_;
        s.status_ = e, a && t === c && s.status_ !== c && a.resolve();
      }
      var a,
        s = this,
        d = {};
      s.status_ = c, s.persist_ = function() {
        var r;
        o(u), r = e(i, t);
        var a = _.omit(s, ["status_", "persist_", "sync_", "observe_", "unobserve_", "wait_"]);
        return r.setItem(n, a).then(function() {
          o(l);
        });
      }, s.sync_ = function() {
        e(i, t);
        return e(i, t).getItem(n).then(function(e) {
          if (e = e || {}, !_.isObject(e)) throw new Error(
            "Error: Tried to cache plain old data. Only objects supported");
          return angular.merge(s, e), o(l), _.each(d, function(e) {
            e();
          }), e;
        });
      }, s.wait_ = function() {
        return a || (a = r.defer(), s.status_ !== c && a.resolve()), a.promise;
      }, s.observe_ = function(e) {
        d[e] = e, s.status_ !== c && e();
      }, s.unobserve_ = function(e) {
        delete d[e];
      };
    }
    var n,
      r,
      i = {},
      o = [],
      a = {},
      s = [],
      c = "loading",
      u = "dirty",
      l = "saved",
      d = "notFound";
    return {
      loadGlobalKey: function(e, n) {
        o.push({
          storeName: e,
          key: n
        }), i[e] = i[e] || {}, i[e][n] = new t(e, n);
      },
      loadUserKey: function(e, t) {
        s.push({
          storeName: e,
          key: t
        });
      },
      $get: ["$q", "$log", "dbService", "eventAggregator", "DB_SERVICE_EVENTS", function(e, c, u, l,
      f) {
        function h(e) {
          a[e] = {}, _.each(s, function(n) {
            a[e][n.storeName] = a[e][n.storeName] || {}, a[e][n.storeName][n.key] = new t(n
              .storeName, n.key, e);
          });
        }
        var p = {},
          m = c.getInstance("nvDbService/dbCacheService");
        return n = u, r = e, l.on(f.DATABASE_ITEM_CHANGE, function(e) {
          var t;
          e.userId ? a[e.userId] && a[e.userId][e.storeName] && (t = a[e.userId][e.storeName][e
            .key
          ]) : i[e.storeName] && (t = i[e.storeName][e.key]), t && t.sync_();
        }), p.syncGlobal = function(e) {
          var t = _.where(o, {
            storeName: e
          });
          n.getGlobalStore(e);
          return r.all(_.map(t, function(e) {
            return i[e.storeName][e.key].sync_();
          })).then(function(t) {
            return m.info("Cached DB store " + e + " into memory"), t;
          });
        }, p.getCachedGlobalItem = function(e, t) {
          return e in i ? i[e][t] : {
            status_: d
          };
        }, p.getOrCreateCachedGlobalItem = function(e, n) {
          return i[e] && i[e][n] || (o.push({
            storeName: e,
            key: n
          }), i[e] = i[e] || {}, i[e][n] = new t(e, n)), i[e][n];
        }, p.syncUser = function(e, t) {
          var i = _.where(s, {
            storeName: e
          });
          n.getUserStore(t, e);
          return a[t] || h(t), r.all(_.map(i, function(e) {
            return a[t][e.storeName][e.key].sync_();
          })).then(function(n) {
            m.info("Cached DB user store " + e + " into memory for user " + t);
          });
        }, p.getCachedUserItem = function(e, t, n) {
          return a[e] || h(e), a[e][t][n];
        }, p;
      }]
    };
  }]);
}
