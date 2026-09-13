// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 128
// role       : service jsEventsService | provider jsEventsEndpoints | constant EVENTS_DATA | constant EVENTS_DB_NAMES | constant EVENTS_RETURN_CODE | constant GDPR_LEVEL
// defines    : angular.module("nvJsEvents")
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
  var i = n(26),
    o = r(i),
    a = n(77),
    s = r(a),
    c = n(25),
    u = r(c);
  angular.module("nvJsEvents", ["nvAngularHttpEndpoint", "crimson", "underscore", "nvDbService", "ngEventAggregator"]),
    angular.module("nvJsEvents").constant("EVENTS_DATA", {
      EVENT_COMMON: {
        clientId: "{CLIENTID}",
        clientVer: "{CLIENTVER}",
        eventSchemaVer: "{EVENTSCHEMAVER}",
        eventSysVer: "0.7.11",
        deviceId: "{DEVICEID}",
        userId: "{USERID}",
        sessionId: "{SESSIONID}",
        sentTs: "",
        events: "[]"
      },
      EXPERIENCE_CONTROL_EXPERIMENT_CONTEXT: {
        id: "",
        group: "",
        name: ""
      },
      EXPERIENCE_CONTROL_EXPERIMENT: {
        activity: {
          id: "",
          type: "",
          name: ""
        },
        variant: {
          type: "",
          name: "",
          data: {}
        }
      }
    }).constant("EVENTS_DB_NAMES", {
      EVENTS_COMMON_STORE: "eventsCommonStore",
      EVENTS_DETAIL_STORE: "eventsDetailStore",
      EVENTS_DETAIL_STORE_TECHNICAL: "eventsDetailStoreTechnical",
      EVENTS_DETAIL_STORE_BEHAVIORAL: "eventsDetailStoreBehavioral",
      EXPERIENCE_CONTROL_STORE: "experienceControlStore",
      USER_DATA_CONSENT_STORE: "userDataConsentStore"
    }).constant("EVENTS_RETURN_CODE", {
      OK: "OK",
      INVALID_INFO_FOR_EVENT_TYPE: "INVALID_INFO_FOR_EVENT_TYPE",
      UNKNOWN_EVENT_TYPE: "UNKNOWN_EVENT_TYPE",
      UNPROCESSED: "UNPROCESSED",
      INVALID_EXPERIMENT_FORMAT: "INVALID_EXPERIMENT_FORMAT",
      EXPERIMENT_ALREADY_ACTIVE: "EXPERIMENT_ALREADY_ACTIVE",
      FUNCTIONAL_CONSENT_NOT_RECEIVED: "FUNCTIONAL_CONSENT_NOT_RECEIVED",
      SWITCH_AND_PURGE_IN_PROGRESS: "SWITCH_AND_PURGE_IN_PROGRESS"
    }).constant("GDPR_LEVEL", {
      FUNCTIONAL: "functional",
      TECHNICAL: "technical",
      BEHAVIORAL: "behavioral"
    }).constant("USER_CONSENT_LEVEL", {
      FULL: "Full",
      NONE: "None"
    }).constant("GDPR_CONSENT", {
      NONE: {
        functional: "None",
        technical: "None",
        behavioral: "None"
      },
      DEFAULT: {
        functional: "Full",
        technical: "None",
        behavioral: "None"
      }
    }).constant("PLATFORM_TYPE", {
      MAC: "Mac",
      WINDOWS: "Win",
      ANDROID: "Android"
    }), angular.module("nvJsEvents").provider("jsEventsEndpoints", [function() {
      var e, t, n = {
          "Accept-Language": "en-US",
          "X-Event-Protocol": "1.1"
        },
        r = 2,
        i = 5e3,
        o = 1e3;
      return {
        setConfig: function(n) {
          e = n.version, t = n.server, i = n.defaultTimeout || i, r = n.defaultRetries || r, o = n
            .defaultTimeBetweenRetries || o
        },
        $get: ["NvEndpointFactory", "$q", "$http", "$log", function(a, s, c, u) {
          function l(e) {
            var n = e || t;
            return n && "" !== n ? n.endsWith(".cn") ? "china" : "global" : "unknown"
          }

          function d(e) {
            t = e
          }
          var f = new a,
            h = u.getInstance("nvJsEvents/jsEventsEndpoints");
          h.info("jsEventsEndpoints created"), f.setUrlGenerator(function(n) {
            return h.info("server" + t), n.url.startsWith("/dev/") ? t + n.url : t + "/" + e + n.url
          }), f.setHeaderGenerator(function(e) {
            var t = angular.merge({}, e.headers, n);
            return t
          }), f.setEndpointConfigFunc(function(e, t) {
            var n = {};
            return null != t && hasOwnProperty.call(t, "sync") && (t.sync === !0 && (n.sync = !0), delete t
              .sync), n
          }), f.setHttpFunc(function(e) {
            function t(e) {
              n.reject({})
            }
            var n = s.defer();
            if (e.sync) {
              try {
                var r = new XMLHttpRequest;
                r.open("POST", e.url, !1), r.setRequestHeader("Content-Type",
                    "application/json;charset=UTF-8"), r.setRequestHeader("X-Event-Protocol", "1.1"), r
                  .onload = function(e) {
                    n.resolve({})
                  }, r.onabort = t, r.onerror = t, r.ontimeout = t, r.send(e.data)
              } catch (e) {
                t(e)
              }
              return n.promise
            }
            return c(e)
          }), f.setDefaultTimeout(i), f.setDefaultRetries(r), f.setDefaultTimeBetweenRetries(o);
          var p = f.createEndpoint({
            url: "/events/json",
            method: "POST"
          });
          return {
            getFullJsEventsUrl: f.generateFullUrl,
            sendEvent: p,
            getServerLocale: l,
            updateServer: d
          }
        }]
      }
    }]), angular.module("nvJsEvents").service("jsEventsService", ["jsEventsEndpoints", "$log", "EVENTS_DATA", "_",
      "dbService", "EVENTS_DB_NAMES", "dbCacheService", "EVENTS_RETURN_CODE", "$interval", "$window", "GDPR_LEVEL",
      "USER_CONSENT_LEVEL", "GDPR_CONSENT", "PLATFORM_TYPE", "$q",
      function(e, t, n, r, i, a, c, l, d, f, h, p, m, v, g) {
        function y(e, t) {
          return JSON.parse(f.localStorage.getItem(e + t))
        }

        function b(e, t, n) {
          return f.localStorage.setItem(e + t, (0, u.default)(n))
        }

        function E(e, t) {
          return f.localStorage.removeItem(e + t)
        }

        function _() {
          pe.timerPromise || (pe.timerPromise = d(he, re), fe.event("send interval started"))
        }

        function $() {
          pe.timerPromise && d.cancel(pe.timerPromise), pe.timerPromise = null, fe.event("send interval stopped")
        }

        function w(t) {
          var n = t.detailData.events.slice(0);
          for (t.eventsToBeSent = t.detailData.events.length; 0 !== t.eventsToBeSent;) {
            fe.event("eventsToBeSent", t.eventsToBeSent.toString());
            var r = [];
            t.eventsToBeSent > ie ? (r = n.slice(0, ie), n.splice(0, ie)) : r = n, t.eventsToBeSent = t
              .eventsToBeSent - r.length;
            var i = (new Date).toISOString(),
              o = t.commonData.common;
            o.events = r, o.sentTs = i;
            var a = (0, u.default)(o);
            fe.event("events request", a), e.sendEvent({}, a).then(t.sendEventCallback).catch(t
              .sendEventCatchCallback)
          }
        }

        function T(e, t, n, r) {
          fe.event("attempting to send backlog events for dbName:", t, " userconsent: ", e, "key: ", n);
          var o = g.defer();
          return i.getGlobalStore(t).getItem(n).then(function(a) {
            var s = {},
              c = function(e) {
                if (fe.event("jsEvents backlog response", e), 0 === s.eventsToBeSent) return i.getGlobalStore(t)
                  .removeItem(n), o.resolve()
              },
              u = function(e) {
                return fe.error("Failed to send backlog events for :", n, t, ".ErrorInfo: status", e.status), o
                  .reject("error")
              };
            return e === p.FULL && r && a && a.events && a.events.length > 0 ? (s = {
              commonData: angular.merge({}, r),
              detailData: angular.merge({}, a),
              eventsToBeSent: 0,
              sendEventCallback: c,
              sendEventCatchCallback: u
            }, void w(s)) : (e !== p.FULL && fe.event("cannot send for backlog key", n, e, a), i.getGlobalStore(
              t).removeItem(n), o.resolve())
          }).catch(function(e) {
            return fe.event("detail data not available for backlog key", n), i.getGlobalStore(t).removeItem(n), o
              .resolve()
          }), o.promise
        }

        function C(e, t, n, r) {
          fe.event("attempting to send wls backlog events for dbName:", t, " userconsent: ", e, "key: ", n);
          var i = g.defer(),
            o = y(t, (0, u.default)(n)),
            s = {},
            c = function(e) {
              if (fe.event("jsEvents wls backlog response", e), 0 === s.eventsToBeSent) return E(t, (0, u.default)(
                n)), i.resolve()
            },
            l = function(e) {
              return fe.error("Failed to send wls backlog events for :", n, ".ErrorInfo: status", e.status), i
              .reject()
            };
          return e === p.FULL && r && o && o.events && o.events.length > 0 ? (s = {
            commonData: angular.merge({}, r),
            detailData: angular.merge({}, o),
            eventsToBeSent: 0,
            sendEventCallback: c,
            sendEventCatchCallback: l
          }, w(s), i.promise) : (fe.event("nothing to send for backlog key", n, e), E(a.EVENTS_DETAIL_STORE, (0, u
            .default)(n)), E(a.EVENTS_DETAIL_STORE_TECHNICAL, (0, u.default)(n)), E(a
            .EVENTS_DETAIL_STORE_BEHAVIORAL, (0, u.default)(n)), i.resolve())
        }

        function x(e, t) {
          var n = g.defer(),
            r = e || "undefined",
            i = ae,
            o = c.getOrCreateCachedGlobalItem(a.USER_DATA_CONSENT_STORE, r);
          return o ? o.sync_().then(function() {
            t === !0 && (oe = o && o.userConsent || i, se = !0), n.resolve(o && o.userConsent || i)
          }) : n.resolve(i), n.promise
        }

        function S(e) {
          i.getGlobalStore(a.EVENTS_COMMON_STORE).getItem(e).then(function(t) {
            t && t.common ? x(e.userId || "undefined", !1).then(function(n) {
              var r, o, s = null;
              t.common.gdprTechOptIn = pe.getString(n.technical), t.common.gdprBehOptIn = pe.getString(n
                  .behavioral), t.common.deviceGdprTechOptIn = pe.getString(ae.technical), t.common
                .deviceGdprBehOptIn = pe.getString(ae.behavioral), r = T(n.functional, a.EVENTS_DETAIL_STORE,
                  e, t), o = T(n.technical, a.EVENTS_DETAIL_STORE_TECHNICAL, e, t), s = T(n.behavioral, a
                  .EVENTS_DETAIL_STORE_BEHAVIORAL, e, t), g.all([r, o, s]).then(function() {
                  i.getGlobalStore(a.EVENTS_COMMON_STORE).removeItem(e)
                })
            }) : (fe.event("common data not available for backlog key", e), i.getGlobalStore(a
              .EVENTS_COMMON_STORE).removeItem(e))
          }).catch(function(t) {
            fe.event("common data not available for backlog key", e), i.getGlobalStore(a.EVENTS_COMMON_STORE)
              .removeItem(e)
          })
        }

        function A(e) {
          var t = y(a.EVENTS_COMMON_STORE, (0, u.default)(e));
          t ? x(e.userId || "undefined", !1).then(function(n) {
            var r, i, o = null,
              s = {
                common: t
              };
            s.common.gdprTechOptIn = pe.getString(n.technical), s.common.gdprBehOptIn = pe.getString(n
              .behavioral), s.common.deviceGdprTechOptIn = pe.getString(ae.technical), s.common
              .deviceGdprBehOptIn = pe.getString(ae.behavioral), r = C(n.functional, a.EVENTS_DETAIL_STORE, e, s),
              i = C(n.technical, a.EVENTS_DETAIL_STORE_TECHNICAL, e, s), o = C(n.behavioral, a
                .EVENTS_DETAIL_STORE_BEHAVIORAL, e, s), g.all([r, i, o]).then(function() {
                E(a.EVENTS_COMMON_STORE, (0, u.default)(e))
              })
          }) : (fe.event("nothing to send for wls backlog key", e), E(a.EVENTS_COMMON_STORE, (0, u.default)(e)))
        }

        function M() {
          var e = {
            sessionId: te || "undefined",
            userId: ee || "undefined"
          };
          fe.event("cleanUpDbStore excluding session for", e), i.getGlobalStore(a.EVENTS_COMMON_STORE).iterate(
            function(t, n, r) {
              if (fe.event("Key#", r, "key ", n), "_version" === n) fe.event("_version to be skipped", n);
              else {
                var i = JSON.parse(n);
                "" === i.sessionId || i.sessionId === e.sessionId ? fe.event("key not to be processed", i) : S(n)
              }
            }).then(function() {
            fe.event("eventsCommonStore items enumeration completed")
          }).catch(function(e) {
            fe.event("eventsCommonStore items enumeration error", e)
          })
        }

        function k() {
          var e = {
            sessionId: te || "undefined",
            userId: ee || "undefined"
          };
          fe.event("cleanUpWlsStore excluding session for", e);
          var t, n = 0;
          for (n = 0; n < f.localStorage.length; ++n)
            if (t = f.localStorage.key(n), t.match("^" + a.EVENTS_COMMON_STORE)) {
              fe.event("Key#", n, "key ", t);
              var r = JSON.parse(t.replace(a.EVENTS_COMMON_STORE, ""));
              "" === r.sessionId || r.sessionId === e.sessionId ? fe.event("key not to be processed", r) : A(r)
            }
        }

        function N(e, t) {
          var n = t || {},
            i = e || {},
            a = (0, s.default)(n),
            c = (0, s.default)(i);
          if (c.length !== a.length) return !1;
          for (var u in n) {
            var l = n[u];
            if ("object" === ("undefined" == typeof l ? "undefined" : (0, o.default)(l))) {
              if ("object" !== (0, o.default)(i[u])) return !1;
              if (!N(i[u], l)) return !1
            }
            if (!r.has(i, u)) return !1;
            if (i[u] !== n[u]) return !1
          }
          return !0
        }

        function I() {
          var e = g.defer();
          try {
            f.localStorage.clear();
            var t = i.getGlobalStore(a.EVENTS_DETAIL_STORE).clear(),
              n = i.getGlobalStore(a.EVENTS_DETAIL_STORE_TECHNICAL).clear(),
              r = i.getGlobalStore(a.EVENTS_DETAIL_STORE_BEHAVIORAL).clear(),
              o = i.getGlobalStore(a.EVENTS_COMMON_STORE).clear();
            g.all([t, n, r, o]).then(function() {
              e.resolve()
            }).catch(function(t) {
              fe.error("IndexedDb purge error", t), e.resolve()
            })
          } catch (t) {
            fe.error("exception occurred, resolving promise"), e.resolve()
          }
          return e.promise
        }

        function O(e, t, n) {
          fe.event("cleanUpSentEvents for", n);
          var i = c.getCachedGlobalItem(t, n);
          r.forEach(e, function(e) {
            if (i && i.events.length > 0) {
              var t = r.findWhere(i.events, {
                ts: e.ts
              });
              if (t && N(t.parameters, e.parameters)) {
                var n = i.events.indexOf(t);
                fe.event("matching event found", e), i.events.splice(n, 1), i.persist_()
              } else fe.event("matching event not found")
            } else fe.event("events seem to be already cleared")
          }), i.persist_()
        }

        function D(e, t, n, r) {
          fe.event("attempting to send batched events for dbName:", t, " userconsent: ", e, "key: ", n);
          var i = c.getCachedGlobalItem(t, n),
            o = {},
            a = function(e) {
              0 === o.eventsToBeSent && null === pe.timerPromise && _(), fe.event("jsEvents batched response", e);
              var r = JSON.parse(e.config.data);
              O(r.events, t, n)
            },
            s = function(e) {
              0 === o.eventsToBeSent && null === pe.timerPromise && _(), fe.error("Failed to send events for :", n,
                ".ErrorInfo: status", e.status)
            };
          e === p.FULL && r && i && i.events && i.events.length > 0 ? (o = {
              commonData: angular.merge({}, r),
              detailData: angular.merge({}, i),
              eventsToBeSent: 0,
              sendEventCallback: a,
              sendEventCatchCallback: s
            }, $(), w(o)) : e !== p.FULL && (ce === !0 || "undefined" === ee) && i && i.events && i.events.length >
            0 && (fe.event("deleting events as userConsent is", e), i.events.splice(0, i.events.length), i.persist_())
        }

        function R(e) {
          var t = ["Windows", "Win16", "Win32"],
            n = ["Macintosh", "MacIntel", "MacPPC", "Mac68K"],
            i = ["Android"],
            o = r.findWhere(t, e);
          return o ? v.WINDOWS : (o = r.findWhere(n, e)) ? v.MAC : (o = r.findWhere(i, e), o ? v.ANDROID : v.WINDOWS)
        }

        function P() {
          var e = angular.merge({}, n.EVENT_COMMON);
          return e.clientId = e.clientId.replace("{CLIENTID}", Q), e.clientVer = e.clientVer.replace("{CLIENTVER}",
            J), e.deviceId = e.deviceId.replace("{DEVICEID}", Z), e.userId = e.userId.replace("{USERID}", ee), e
            .sessionId = e.sessionId.replace("{SESSIONID}", te), e.eventSchemaVer = e.eventSchemaVer.replace(
              "{EVENTSCHEMAVER}", ne), e.platform = R(f.navigator.platform), null != e.platform && (e.eventProtocol =
              "1.1"), e
        }

        function L(e, t, n) {
          var i = (0, u.default)(e),
            a = JSON.parse(i);
          e = a || {};
          var c = (0, s.default)(t),
            l = (0, s.default)(e);
          return !(c.length !== l.length && !n) && r.every(t, function(t, i) {
            return "object" === ("undefined" == typeof t ? "undefined" : (0, o.default)(t)) ? "object" === (0, o
              .default)(e[i]) && L(e[i], t, n) : r.has(e, i)
          })
        }

        function U(e) {
          var t = angular.merge({}, n.EXPERIENCE_CONTROL_EXPERIMENT_CONTEXT);
          return L(e, t, !1) === !1 ? (fe.event("mismatch", t, e), l.INVALID_EXPERIMENT_FORMAT) : (angular.merge(t,
            e), t)
        }

        function F(e) {
          var t = angular.merge({}, n.EXPERIENCE_CONTROL_EXPERIMENT);
          return L(e, t, !0) === !1 ? (fe.event("mismatch", t, e), l.INVALID_EXPERIMENT_FORMAT) : (angular.merge(t,
            e), t)
        }

        function j(e) {
          var t, n = [],
            i = c.getCachedGlobalItem(a.EXPERIENCE_CONTROL_STORE, e);
          if (i && i.experiments && i.experiments.length > 0) {
            var o = i.experiments;
            r.forEach(o, function(e) {
              var t = angular.merge({}, r.omit(e, "name"));
              n.push(t)
            })
          }
          return fe.event("experiments for key", n, e), n.length > 0 && (t = n), fe.event("activeExperiments", t), t
        }

        function H() {
          var e = Z,
            t = c.getOrCreateCachedGlobalItem(a.EXPERIENCE_CONTROL_STORE, e);
          t && t.sync_()
        }

        function B() {
          pe.offline || angular.equals(ae, m.NONE) || "unknown" === le ? fe.info(
            "Device offline or No Functional consent. No clean job happened.", ae) : (_(), M(), k(), ue = !1)
        }

        function z(e) {
          return e = r.omit(e, "gdprLevel")
        }

        function q(e, t) {
          var n = P();
          t && t.anonymize && (n.deviceId = "undefined", n.userId = "undefined", n.sessionId = "undefined");
          var r = Z;
          e.experiments = j(r), e = z(e), fe.event("eventDetail", e);
          var i = [];
          i.push(e), n.gdprTechOptIn = oe.technical, n.gdprBehOptIn = oe.behavioral, n.deviceGdprTechOptIn = pe
            .getString(ae.technical), n.deviceGdprBehOptIn = pe.getString(ae.behavioral), n.events = i;
          var o = (new Date).toISOString();
          return n.sentTs = o, (0, u.default)(n)
        }

        function G(e, t, n) {
          var i = (0, u.default)({
              sessionId: te || "undefined",
              userId: ee || "undefined"
            }),
            o = Z,
            s = a.EVENTS_DETAIL_STORE;
          e.gdprLevel === h.TECHNICAL && (s = a.EVENTS_DETAIL_STORE_TECHNICAL), e.gdprLevel === h.BEHAVIORAL && (s = a
            .EVENTS_DETAIL_STORE_BEHAVIORAL);
          var l;
          if (t) l = y(s, i) || {}, l.events = l.events || [], e.experiments = j(o), e = z(e), fe.event(
              "eventDetail wls", e), l.events.push(e), fe.event("details list wls", l.events), b(s, i, l), r
            .isFunction(n) && n();
          else {
            l = c.getOrCreateCachedGlobalItem(s, i);
            var d = l.events;
            if (e.experiments = j(o), e = z(e), fe.event("eventDetail IDb", e), d) d.push(e);
            else {
              var f = [];
              f.push(e), d = f
            }
            fe.event("details list indexed db", d), l.events = d, l.persist_().finally(function() {
              r.isFunction(n) && n()
            })
          }
        }

        function V(e) {
          if (se === !0) {
            if (e === h.FUNCTIONAL) return oe.functional;
            if (e === h.TECHNICAL) return oe.technical;
            if (e === h.BEHAVIORAL) return oe.behavioral
          }
          return p.NONE
        }

        function W(e, t, n) {
          r.has(n, "afterDone") ? G(e, t, n.afterDone) : G(e, t)
        }

        function Y() {
          fe.info("jsEvents turns into offline"), pe.offline = !0, $()
        }

        function K() {
          fe.info("jsEvents turns into online"), pe.offline = !1, angular.equals(ae, m.NONE) || _()
        }

        function X() {
          f.navigator.onLine !== !0 && Y()
        }
        var Q = "",
          J = "",
          Z = "",
          ee = "undefined",
          te = "",
          ne = "1.0",
          re = 5e3,
          ie = 128,
          oe = m.NONE,
          ae = m.NONE,
          se = !1,
          ce = !1,
          ue = !1,
          le = e.getServerLocale(),
          de = !1,
          fe = t.getInstance("nvJsEvents/jsEventsService");
        fe.info("jsEventsService created");
        var he, pe = this;
        pe.offline = !1, pe.defaultErrorCallback = function(e) {
          fe.error("message:", e ? e : "Unknown error")
        }, pe.registeredErrorCallback = pe.defaultErrorCallback, pe.setErrorCallback = function(e) {
          pe.registeredErrorCallback = e
        }, pe.timerPromise = null, this.getString = function(e) {
          return e && "string" != typeof e ? (0, u.default)(e) : e || ""
        }, this.updateServer = function(t) {
          var n = e.getServerLocale(t);
          if ("unknown" !== le && le !== n) {
            var r = !1;
            null != pe.timerPromise && (r = !0, $()), de = !0, I().then(function() {
              r === !0 && (_(), r = !1), de = !1
            })
          }
          e.updateServer(t), le = n
        }, he = function() {
          var e = (0, u.default)({
              sessionId: te || "undefined",
              userId: ee || "undefined"
            }),
            t = c.getCachedGlobalItem(a.EVENTS_COMMON_STORE, e);
          if (t && t.common) x(ee || "undefined", !0).then(function(n) {
            t.common.gdprTechOptIn = pe.getString(n.technical), t.common.gdprBehOptIn = pe.getString(n
                .behavioral), t.common.deviceGdprTechOptIn = pe.getString(ae.technical), t.common
              .deviceGdprBehOptIn = pe.getString(ae.behavioral), D(n.functional, a.EVENTS_DETAIL_STORE, e, t),
              D(n.technical, a.EVENTS_DETAIL_STORE_TECHNICAL, e, t), D(n.behavioral, a
                .EVENTS_DETAIL_STORE_BEHAVIORAL, e, t)
          });
          else if (fe.event("common data not available for key", e), t && r.isUndefined(t.common)) {
            try {
              pe.registeredErrorCallback("common data not set before starting to send events")
            } catch (e) {
              fe.error(e), pe.defaultErrorCallback(
                "Invalid callback. Original Error: common data not set before starting to send events")
            }
            $()
          }
        }, this.setBatchModeSettings = function(e) {
          e && (re = e.msInterval || re, ie = e.maxEvents || ie)
        }, this.syncExperienceControlInfo = function(e) {
          var t = [],
            n = [];
          e.forEach(function(e) {
            var n = F(e);
            n !== l.INVALID_EXPERIMENT_FORMAT && t.push({
              id: n.activity.id,
              group: n.variant.name,
              name: n.activity.name
            })
          });
          var r = Z;
          n = c.getOrCreateCachedGlobalItem(a.EXPERIENCE_CONTROL_STORE, r), fe.event(
            "previously cached experiments data", n), n.experiments = t, n.persist_(), fe.event(
            "updated experiments", t)
        }, this.setExperienceControlInfo = function(e) {
          var t, n = U(e);
          if (n === l.INVALID_EXPERIMENT_FORMAT) return n;
          var i = (0, u.default)({
            deviceId: Z || "undefined",
            userId: ee || "undefined"
          });
          t = c.getOrCreateCachedGlobalItem(a.EXPERIENCE_CONTROL_STORE, i);
          var o = t.experiments;
          if (fe.event("experienceControlInfo IDb", n), o) {
            var s = r.findWhere(o, {
              id: n.id
            });
            if (s) {
              var d = o.indexOf(s);
              if (d !== -1) return fe.event("experienceControlInfo already exist"), l.EXPERIMENT_ALREADY_ACTIVE
            }
            o.push(n)
          } else {
            var f = [];
            f.push(n), o = f
          }
          fe.event("experienceControlInfo list indexed db", o), t.experiments = o, t.persist_()
        }, this.resetExperienceControlInfo = function(e) {
          var t = (0, u.default)({
              deviceId: Z || "undefined",
              userId: ee || "undefined"
            }),
            n = c.getCachedGlobalItem(a.EXPERIENCE_CONTROL_STORE, t);
          if (n && n.experiments && n.experiments.length > 0) {
            var i = r.findWhere(n.experiments, {
              id: e
            });
            if (i) {
              var o = n.experiments.indexOf(i);
              o !== -1 && (fe.event("matching experiment found"), n.experiments.splice(o, 1), n.persist_())
            } else fe.event("matching experiment not found")
          } else fe.event("experiment list is empty for key", t)
        }, this.setDefaultConsent = function(e) {
          ae = e, ("undefined" === ee || r.isUndefined(ee)) && (oe = ae, se = !0), B()
        }, this.syncUserConsentInfo = function(e) {
          r.forEach(e, function(e) {
            var t = e.userId;
            t === ee && (oe = e.userConsent, se = !0);
            var n = c.getOrCreateCachedGlobalItem(a.USER_DATA_CONSENT_STORE, t);
            fe.event("previously cached consent", n.userConsent), n.userConsent = e.userConsent, n.persist_(),
              fe.event("updated consent", e.userConsent)
          }), ce = !0, B()
        }, this.setEventsCommonData = function(e) {
          if (ue = !1, !e) return void fe.error("Undefined common data sent by client");
          !te && e.clientSessionId && (ue = !0), Q = e.clientProductId, J = e.clientProductVer, te = e
            .clientSessionId, Z = e.clientDeviceId, ee = e.clientUserId, se = !1, ne = e.eventSchemaVer, Z && fe
            .info("Device Id:", Z, "set for client:", Q), H(), x(ee, !0);
          var t = (0, u.default)({
              sessionId: te || "undefined",
              userId: ee || "undefined"
            }),
            n = P(),
            r = c.getOrCreateCachedGlobalItem(a.EVENTS_COMMON_STORE, t);
          r.common = n, r.persist_();
          var i = y(a.EVENTS_COMMON_STORE, t) || {};
          i = n, b(a.EVENTS_COMMON_STORE, t, i), pe.offline || angular.equals(ae, m.NONE) || "unknown" === le ? fe
            .info("Device offline or No Functional consent. No clean job happened.", ae) : (_(), ue && (M(), k(),
              ue = !1))
        }, this.sendEventDetail = function(t, n, i) {
          if (de === !0) return l.SWITCH_AND_PURGE_IN_PROGRESS;
          if (t && n) {
            if (i = i || {}, i && !i.anonymize && angular.equals(ae, m.NONE) || n && r.isUndefined(n.gdprLevel))
              return fe.info("Functional consent not received, discarding event", t, n.gdprLevel), r.isFunction(i
                .afterDone) && i.afterDone(), l.FUNCTIONAL_CONSENT_NOT_RECEIVED;
            var o = i.appExit === !0;
            if ("unknown" !== le && t.immediateRequest === !0 && pe.offline === !1) {
              var a = p.NONE;
              if (a = i && i.anonymize ? p.FULL : V(n.gdprLevel), fe.event(
                  "attempting to send immediate event for userId:", ee, " userconsent: ", oe), a !== p.FULL)
              return ee && "undefined" !== ee && se === !1 ? (W(n, o, i), fe.event(
                  "Event stored as userConsent not synced yet", a, se)) : (r.isFunction(i.afterDone) && i
                  .afterDone(), fe.error("Event cannot be sent due to reason:", a)), a;
              var s = q(n, i);
              fe.event("jsonString request to send:", s);
              var c = {
                sync: !1
              };
              i.appExit === !0 && (c.sync = !0), e.sendEvent(c, s).then(function(e) {
                fe.event("jsevents response", e), r.isFunction(i.afterDone) && i.afterDone()
              }).catch(function(e) {
                fe.error("Failed to send eventdata for:", t.name, ".ErrorInfo: status", e.status, ", data", e
                  .config && e.config.data), W(n, o, i)
              })
            } else W(n, o, i);
            return l.OK
          }
          return fe.error("eventName undefined "), l.UNPROCESSED
        }, f.addEventListener("online", K), f.addEventListener("offline", Y), X()
      }
    ])
}
