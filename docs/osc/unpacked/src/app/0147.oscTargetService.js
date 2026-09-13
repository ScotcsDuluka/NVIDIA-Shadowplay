// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 147
// service oscTargetService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.oscTargetService = void 0;
  var o = require(3),
    r = i(o),
    a = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(26) /* app/26 — telemetryService (service) */, require(133);
  var l = a.ngMainCommonModule.service("oscTargetService", ["$log", "$q", "telemetryService", "targetService",
    "eventAggregator", "dbCacheService", "$filter", "jarvisService", "socketService", "abHubEndpoints",
    "AB_HUB_SOCKET_EVENTS", "AB_HUB_MESSAGE_TYPES", "AB_STORE", "OSC_BUILD_INFO", "OSC_CONFIG",
    "COMMON_EVENTS",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h) {
      function b() {
        R = t.defer(), P = R.promise;
      }

      function x(e) {
        O.event("AB Context updated", e), e.messageType === f.SYNC && n.syncExperienceControlInfo(e
          .experiments);
      }

      function v(e) {
        M = e, O.event("Ab Hub client status", M), M && (b(), R.resolve());
      }

      function y(e) {
        var n = S(A, e),
          i = S(I, e),
          o = t.defer();
        return t.all([n, i]).then(function(t) {
          var n = t[0],
            i = t[1],
            r = void 0;
          if (n) {
            var a = A.activeExperiments || [];
            r = E(a, e);
          } else if (i) {
            var l = I.activeExperiments || [];
            r = E(l, e);
          }
          o.resolve(r);
        }), o.promise;
      }

      function w(e) {
        var n = S(A, e.activity.id),
          i = S(I, e.activity.id),
          o = e.activity,
          r = e.variant,
          a = [];
        t.all([n, i]).then(function(t) {
          var n = t[0],
            i = t[1];
          if (n || i) {
            if (n) {
              a = A.activeExperiments;
              var l = E(a, o.id);
              l.variant = r, A.persist_();
            } else {
              a = I.activeExperiments;
              var s = E(a, o.id);
              s.variant = r, I.persist_();
            }
          } else A && A.activeExperiments ? (a = A.activeExperiments || [], a.push(e), A.persist_(),
            addExperimentDeferred.resolve(!0)) : (a = I.activeExperiments || [], a.push(e), I
            .persist_());
          k("add", f.ADD, e);
        });
      }

      function S(e, n) {
        var i = t.defer();
        return e ? e.sync_().then(function() {
          var t = e.activeExperiments || [],
            o = E(t, n);
          i.resolve(!!o);
        }).catch(function() {
          O.error("Failed to sync db while finding experiment"), i.resolve(!1);
        }) : i.resolve(!1), i.promise;
      }

      function E(e, t) {
        return r.find(e, function(e) {
          return e.activity.id == t;
        });
      }

      function k(e, t, n) {
        M || (O.info("AB Hub not ready, checking status"), c.status().then(function(e) {
          O.info("AB Hub Ready status", e), M = !0, R.resolve(M);
        }).catch(function(e) {
          O.error("AB Hub status API failed", e), R.reject(e);
        })), P.then(function() {
          O.event("Going to call ab hub endpoint", e, t, n, D), c[e]({}, {
            messageType: t,
            userId: D.userId,
            clientName: D.clientName,
            clientVer: D.clientVer,
            experiments: [n]
          });
        }).catch(function(e) {
          O.error("Failed to call Ab hub endpoint", e);
        });
      }

      function _(e, n) {
        var i = S(A, e),
          o = S(I, e),
          r = t.defer();
        return t.all([i, o]).then(function(t) {
          var n = t[0],
            i = t[1],
            o = {},
            a = [];
          n || i ? (n ? (a = A.activeExperiments, o = E(a, e), A.activeExperiments = T(a, e), A
            .persist_()) : (a = I.activeExperiments, o = E(a, e), I.activeExperiments = T(a, e), I
            .persist_()), r.resolve(!!o), k("remove", f.DELETE, o)) : r.resolve(!1);
        }), r.promise;
      }

      function T(e, t) {
        return l("filter")(e, function(e) {
          return e.activity.id !== t;
        });
      }
      var C = this,
        O = e.getInstance("osc/OscTargetService"),
        A = null,
        I = null,
        M = !1,
        R = null,
        P = null,
        D = {
          userId: "",
          clientName: p.windowName,
          clientVer: g.oscPackageVersion
        };
      C.updateLoginStatus = function() {
        var e = s.getLoggedInUser();
        e && e.userId ? (O.info("Setting user to " + e.userId), D.userId = e.userId, a.syncUser(m
          .AB_STORE_NAME, e.userId), A = a.getCachedUserItem(e.userId, m.AB_STORE_NAME, m
          .USER_AB_STATE)) : A = null, C.setMboxThirdPartyId(e && e.userId);
      }, C.init = function() {
        i.setExperimentMbox(p.windowName), C.updateLoginStatus(), I = a.getCachedGlobalItem(m
          .AB_STORE_NAME, m.USER_AB_STATE), i.setQaConfig(p.experienceControlQa), b(), d.register(u
          .AB_CONTEXT_UPDATED, h.AB_CONTEXT_UPDATED), o.on(h.AB_CONTEXT_UPDATED, x), d.register(u
          .AB_HUB_STATUS_UPDATED, h.AB_HUB_STATUS_UPDATED), o.on(h.AB_HUB_STATUS_UPDATED, v);
      }, C.setMboxThirdPartyId = function(e) {
        var t = e ? e : n.deviceId ? n.deviceId : n.sessionId;
        i.setMboxThirdPartyId(t);
      }, C.getVariant = function(e, n) {
        var o = t.defer(),
          r = e.activityId;
        Date.now();
        return i.getVariant(e).then(function(e) {
          var t = e.activity,
            i = e.variant;
          t && t.id && t.name && i && i.name ? (e.disableContext ? O.info(
            "Telemetry context disabled for activity id: ", r) : w(e), n && O.info(
            "Tracking success telemetry for activity id: ", r), o.resolve(i.data)) : (O.info(
            "No telemetry for legacy config experiments"), o.resolve(e));
        }).catch(function(e) {
          e && (e.apiFailure ? (n && O.info("Tracking failure telemetry for activity id: ", r), y(r)
            .then(function(t) {
              t ? (O.info("api failure for get variant, resolving saved experiment data", e),
                o.resolve(t.data)) : (O.info(
                "api failure for get variant, no saved data found"), o.reject(e));
            })) : (_(r, !0), o.reject(e)));
        }), o.promise;
      }, C.trackConversion = i.trackConversion, C.getActiveExperiments = function() {
        var e = [],
          n = t.defer();
        return A ? A.sync_().then(function() {
          e = e.concat(A.activeExperiments), I.sync_().then(function() {
            e = e.concat(I.activeExperiments), n.resolve(e);
          }).catch(function() {
            O.error("Failed to sync global Db for active experiments"), n.resolve(e);
          });
        }).catch(function() {
          O.error("Failed to sync user Db for active experiments"), n.resolve(e);
        }) : I.sync_().then(function() {
          e = e.concat(I.activeExperiments), n.resolve(e);
        }).catch(function() {
          O.error("Failed to sync global Db for active experiments"), n.resolve(e);
        }), n.promise;
      };
    }
  ]);
  exports.oscTargetService = l;
}
