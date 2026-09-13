// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 26
// service telemetryService
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

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.telemetryService = void 0;
  var r = require(8),
    a = o(r),
    l = require(107),
    s = o(l),
    d = require(3),
    c = i(d),
    u = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(17) /* app/17 — localSdk (provider) */, require(88), require(134);
  var f = u.ngMainCommonModule.service("telemetryService", ["$q", "$timeout", "$log", "jsEventsService",
    "cefService", "eventsDetailService", "eventAggregator", "nvAccountService", "gameProfileService",
    "socketService", "ACCOUNT_SOCKET_EVENTS", "ACCOUNT_EVENTS", "GDPR_CONSENT", "OSC_CONFIG",
    "OSC_BUILD_INFO", "OSC_EVENTS", "TELEMETRY_OSC_TOGGLE_STATE", "TELEMETRY_OSC_EVENT_NAMES",
    "TELEMETRY_OSC_VALID_PROVIDERS", "TELEMETRY_OSC_SCREEN_STATE", "TELEMETRY_OSC_BOOLEAN_STATUS",
    "EVENTS_DETAIL_DATA", "EVENTS_RETURN_CODE", "CONNECT_EVENTS", "GALLERY_TYPES", "GALLERY_EVENTS",
    function(e, t, n, i, o, r, l, d, u, f, m, g, p, h, b, x, v, y, w, S, E, k, _, T, C, O) {
      function A(e, t, n) {
        if ("" === N.sessionId) {
          switch (z.info("Deferring telemetry event " + t), e) {
            case G.PUSH:
              F.push({
                type: e,
                eventName: t,
                info: n.info
              });
              break;
            case G.TIMER:
              F.push({
                type: e,
                eventName: t,
                info: n.info,
                endTime: n.endTime
              });
              break;
            case G.PERF:
              F.push({
                type: e,
                eventName: t,
                provider: n.provider,
                endTime: n.endTime
              });
              break;
            case G.GALLERY_PERF:
              F.push({
                type: e,
                eventName: t,
                numItems: n.numItems,
                batchType: n.batchType,
                endTime: n.endTime
              });
              break;
            default:
              z.error("Unrecognized type '" + item.type + "' in deferred telemetry");
          }
          return !1;
        }
        return !0;
      }

      function I(e, t, n, i, o) {
        for (var a in t)
          if (void 0 === t[a] || null === t[a]) {
            var l = c.findWhere(n, {
              name: e.type
            });
            for (var a in t) void 0 !== t[a] && null !== t[a] || !l.parameters || (t[a] = l.parameters[
            a]);
            break;
          }
        return i || o ? r.getFormattedDurationEvent(e, t, i, o, n) : r.getFormattedEvent(e, t, n);
      }

      function M() {
        F.length > 0 && z.info("Sending deferred telemetry:"), F.forEach(function(e) {
          switch (z.info(e.eventName), e.type) {
            case G.PUSH:
              N.push(e.eventName, e.info);
              break;
            case G.TIMER:
              N.endTimer(e.eventName, e);
              break;
            case G.PERF:
              N.endPerf(e.eventName, e.provider, e.endTime);
              break;
            case G.GALLERY_PERF:
              N.endGalleryPerf(e.eventName, e.numItems, e.batchType, e.endTime);
              break;
            default:
              z.error("Unrecognized type '" + e.type + "' in deferred telemetry");
          }
        }), F = [];
      }

      function R(e, t, n) {
        if (!n) {
          var i = t.indexOf(c.findWhere(t, {
            name: e.name
          }));
          i !== -1 && t.splice(i, 1);
        }
        return {
          name: e.name,
          startTime: Date.now(),
          endTime: null
        };
      }

      function P(e) {
        return !(!e || e === _.INVALID_INFO_FOR_EVENT_TYPE || e === _.UNKNOWN_EVENT_TYPE || e === _
          .UNPROCESSED);
      }

      function D() {
        var e = Date.now(),
          t = "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(t) {
            var n = (e + 16 * Math.random()) % 16 | 0;
            return e = Math.floor(e / 16), ("x" === t ? n : 3 & n | 8).toString(16);
          });
        return t;
      }
      var N = this,
        L = [],
        F = [],
        U = [y.OSC_BROADCAST_END, y.OSC_ANSEL_NAVIGATION, y.OSC_ANSEL_SCREENSHOT_COMPLETED, y
          .OSC_ANSEL_ERROR, y.OSC_CAPTURE_EVENT, y.OSC_SHADOWPLAY_CAPTURE_ERROR, y
          .OSC_GIF_CONVERSION_ATTEMPT, y.OSC_FREESTYLE_FILTERS_APPLIED, y.OSC_FREESTYLE_FILTERS_ADDED, y
          .OSC_FREESTYLE_FILTERS_SLOT_CHANGED, y.OSC_MENU_LAUNCH, y.OSC_HOTKEY_SETTINGS_EVENT, y
          .OSC_ANSEL_FILTER_CONTROL_SETTINGS, y.OSC_ANSEL_LITE_SCREENSHOT_TAKEN, y
          .OSC_ANSEL_SCREENSHOT_STARTED, y.OSC_ANSEL_SCREENSHOT_CANCELLED, y.OSC_ANSEL_SCREENSHOT_FAILED,
          y.OSC_PERFORMANCE_TOOL_LATENCY_METRICS, y.OSC_PERFORMANCE_TOOL_SAMPLE_SIZE, y
          .OSC_PERFORMANCE_TOOL_RESET_AVERAGE, y.OSC_PERFORMANCE_TOOL_LOGGING_SESSION, y
          .OSC_PERFORMANCE_TOOL_OVERLAY_SESSION, y.OSC_PERFORMANCE_TOOL_SETTINGS
        ];
      N.sessionId = "", N.deviceId = "", N.userId = "", N.clientVersion = b.oscPackageVersion, N
        .clientVersion || (N.clientVersion = b.oscclientVersion + "-" + b.gitHash);
      var z = n.getInstance("main.common/telemetryService");
      z.info("telemetryService created");
      var G = {
        PUSH: "push",
        TIMER: "timer",
        PERF: "perf",
        GALLERY_PERF: "galleryPerf"
      };
      N.push = function(e, t) {
        if (A(G.PUSH, e, {
            info: t
          })) {
          var n = {
            appExit: !1
          };
          if (e) {
            var o;
            if ("TarCon_ClickInfo" === e.type) o = I(e, {
              clickedUrl: t || ""
            }, k);
            else {
              if (c.findWhere(U, e)) {
                var r = u.getDRSInfo();
                t.DRSName = r.DRSName || "", t.DRSProfileName = r.DRSProfileName || "";
              }
              o = I(e, t, k);
            }
            P(o) ? i.sendEventDetail(e, o, n) : z.error("No event detail formatted");
          } else z.error("eventName undefined ");
        }
      }, N.startCold = function() {
        N.startTimer(y.OSC_LAUNCH_TIME_COLD);
      }, N.endCold = function() {
        N.endTimer(y.OSC_LAUNCH_TIME_COLD);
      }, N.startWarm = function() {
        N.startTimer(y.OSC_LAUNCH_TIME_WARM);
      }, N.endWarm = function() {
        N.endTimer(y.OSC_LAUNCH_TIME_WARM);
      }, N.startNavigation = function(e) {
        N.startTimer(e);
      }, N.endNavigation = function(e, t) {
        N.endTimer(e);
      }, N.startTimer = function(e) {
        if (e) {
          if (e.name && e.enumId) {
            var t = R(e, L, e.allowMultiple);
            return L.push(t), t.startTime;
          }
          z.error("Event not tracked for", e);
        } else z.error("EventName undefined ");
      }, N.endTimer = function(e, t) {
        if (t = t || {}, c.findWhere(U, e)) {
          var n = u.getDRSInfo();
          t.info = t.info || {}, t.info.DRSName = n.DRSName || "", t.info.DRSProfileName = n
            .DRSProfileName || "";
        }
        var o = t.endTime || Date.now();
        if (A(G.TIMER, e, t)) {
          var r = {
              appExit: !1,
              negativeDurationLimit: -1e8
            },
            a = {
              name: e.name
            };
          t.startTime && (a.startTime = t.startTime);
          var l = c.findWhere(L, a);
          if (!l) return void z.error("Attempted to end timer '" + e.name + "' without starting it!");
          var s = l.startTime,
            d = I(e, t.info, k, s, r);
          P(d) ? (z.perf(e, s, o), i.sendEventDetail(e, d, r)) : z.error(
            "Telemetry event schema mismatch for ", e, t.info);
        }
      }, N.startBroadcast = function(e) {
        N.startTimer(e);
      }, N.endBroadcast = function(e, t) {
        N.endTimer(e, {
          info: t
        });
      }, N.startPerf = function(e) {
        L.push(R({
          name: e
        }, L));
      }, N.endPerf = function(e, t, n) {
        var o = u.getDRSInfo(),
          r = n || Date.now();
        if (A(G.PERF, e, {
            provider: t,
            endTime: r
          })) {
          if (t = t || w[0], !c.contains(w, t)) return void z.error("Invalid provider " + t +
            "measured");
          var a = {
              appExit: !1,
              negativeDurationLimit: -1e8
            },
            l = (0, s.default)({}, y.OSC_AUTOMATED_UI_PERF);
          l.name = e, N.isInDesktopMode().then(function(n) {
            var s = c.findWhere(L, {
              name: e
            });
            if (!s || !s.startTime) return void z.error(
              "Attempted to end a performance measurement '" + e + "' without starting it!");
            var d = s.startTime;
            s.startTime = null;
            var u = {
                provider: t,
                screenState: n ? S.desktop : S.fullscreen,
                DRSName: o.DRSName,
                DRSProfileName: o.DRSProfileName
              },
              f = I(l, u, k, d, a);
            P(f) ? (f.parameters.id = e, f.parameters.totalMs = r - d, z.perf(e, d, r, u), i
              .sendEventDetail(y.OSC_AUTOMATED_UI_PERF, f, a)) : z.error(
              "Performance data schema mismatch for ", l, t);
          });
        }
      }, N.endGalleryPerf = function(e, n, o, r) {
        t(null, 0, !1).then(function() {
          var t = r || Date.now();
          if (A(G.GALLERY_PERF, e, {
              numItems: n,
              batchType: o,
              endTime: t
            })) {
            var a = c.findWhere(L, {
              name: e
            });
            if (!a) return void z.error("Attempted to end a gallery measurement '" + e +
              "' without starting it!");
            var l = a.startTime,
              d = t - l,
              u = {
                appExit: !1,
                negativeDurationLimit: -1e8
              },
              f = (0, s.default)({}, y.OSC_AUTOMATED_GALLERY_PERF);
            f.name = e, N.isInDesktopMode().then(function(r) {
              var a = {
                  averageMs: Math.round(0 === n ? 0 : d / n),
                  screenState: r ? S.desktop : S.fullscreen,
                  numItems: n,
                  batchType: o
                },
                s = I(f, a, k, l, u);
              P(s) ? (s.parameters.id = e, s.parameters.totalMs = d, z.perf(e, l, t, a), i
                .sendEventDetail(y.OSC_AUTOMATED_GALLERY_PERF, s, u)) : z.error(
                "Performance data schema mismatch for ", f);
            });
          }
        });
      }, N.endPerfAfterDigest = function(e, n) {
        t(null, 0, !1).then(function() {
          N.endPerf(e, n);
        });
      }, N.setEventsCommonData = function(e) {
        N.sessionId = N.sessionId || D(), N.deviceId = N.deviceId || e && e.deviceId, N.userId = N
          .userId || e && e.userId, i.setEventsCommonData({
            clientProductId: h.jsEvents.oscClientId,
            clientProductVer: N.clientVersion,
            clientSessionId: N.sessionId,
            clientDeviceId: N.deviceId,
            clientUserId: N.userId,
            eventSchemaVer: h.jsEvents.schemaVersion
          }), M();
      }, i.setBatchModeSettings({
        msInterval: h.jsEvents.msBetweenSendRequest,
        maxEvents: h.jsEvents.maxEventsPerRequest
      }), N.bool2Toggle = function(e) {
        return e ? v.on : v.off;
      }, N.toTelemetryBoolean = function(e) {
        return e ? E.TRUE : E.FALSE;
      }, N.saveDesktopMode = function(e) {
        N.isInDTMode = e;
      }, N.isInDesktopMode = function() {
        return N.isInDTMode ? e.when(N.isInDTMode) : o.isInDesktopMode().then(function(e) {
          return N.saveDesktopMode(e), e;
        });
      }, N.onServiceLogin = function(e) {
        N.push(y.OSC_LOGIN, {
          provider: e.providerName
        });
      }, N.onBCError = function(e) {
        N.push(y.OSC_BROADCAST_ERROR, {
          provider: e.serviceName,
          error: e.errorTxt
        });
      }, N.onUploadComplete = function(e) {
        var t = e.type !== C.IMAGE,
          n = c.extend({}, y.OSC_UPLOAD_DATA),
          i = {
            info: {
              provider: e.uploadService.providerName,
              fileSize: Math.round(e.fileSize),
              fileType: e.type,
              fileSubType: e.subtype,
              fileSource: e.fileSource,
              hlID: e.highlightDefinitionId,
              containsMeme: e.containsMeme,
              topMemeLength: e.topMemeLength,
              bottomMemeLength: e.bottomMemeLength,
              message: "",
              DRSName: e.DRSName || "",
              DRSProfileName: e.DRSProfileName || "",
              retryAttempts: e.retryCount || 0
            },
            startTime: e.uploadStartTime
          };
        if (e.error) {
          n.id = t ? y.OSC_VIDEO_UPLOAD_ERROR.id : y.OSC_IMAGE_UPLOAD_ERROR.id, n.enumId = t ? y
            .OSC_VIDEO_UPLOAD_ERROR.enumId : y.OSC_IMAGE_UPLOAD_ERROR.enumId;
          var o = "Upload Error: ";
          e.error.status ? (o += "status=" + e.error.status, o += ", statusText=" + e.error
            .statusText) : o += (0, a.default)(e.error), i.info.message = o;
        } else n.id = t ? y.OSC_VIDEO_UPLOAD_DURATION.id : y.OSC_IMAGE_UPLOAD_DURATION.id, n.enumId =
          t ? y.OSC_VIDEO_UPLOAD_DURATION.enumId : y.OSC_IMAGE_UPLOAD_DURATION.enumId;
        N.endTimer(n, i);
      }, N.syncUserConsentInfo = function(e) {
        z.event("User consent settings received", e), i.syncUserConsentInfo(e);
      }, N.disableAllConsent = function() {
        z.event("Functional consent not given, turning off"), i.setDefaultConsent(p.NONE);
      }, N.setFunctionalConsentReceived = function() {
        z.event("Functional consent received, turning on"), i.setDefaultConsent(p.DEFAULT);
      }, this.getDefaultConsent = function() {
        return p.DEFAULT;
      }, N.resetUserConsent = function(e) {
        z.info("Setting all user consent to none for user ", e), N.syncUserConsentInfo([{
          userId: e,
          userConsent: {
            functional: "None",
            technical: "None",
            behavioral: "None"
          }
        }]);
      };
      var V = function(e) {
        return e ? {
          functional: e.trackFunctionalData ? e.trackFunctionalData.level : "None",
          technical: e.trackTechnicalData ? e.trackTechnicalData.level : "None",
          behavioral: e.trackBehavioralData ? e.trackBehavioralData.level : "None"
        } : p.NONE;
      };
      N.updateClientTelemetryConsent = function() {
          return d.getClientTelemetryConsent().then(function(e) {
            if (e) {
              var t = V(e.consentSettings);
              z.info("Setting device telemetry consent setting to: ", t), i.setDefaultConsent(t);
            } else z.error(
              "Unable to get consent data from response, disabling all consent for now."), N
              .disableAllConsent();
          }).catch(function(e) {
            z.error("Unable to retrieve client telemetry consent, disabling all consent for now."), N
              .disableAllConsent();
          });
        }, N.setUserConsent = function(e) {
          if (z.info("Setting user consent settings to: ", e), e.userId) {
            if (e.consentSettings) {
              var t = e.consentSettings;
              N.syncUserConsentInfo([{
                userId: e.userId,
                userConsent: V(t)
              }]);
            } else z.error("Unable to process user consent because user consent data is invalid!");
            N.userId != e.userId && (z.info("Updating telemetry UserId from " + N.userId + " to " + e
              .userId), N.userId = e.userId, N.setEventsCommonData());
          } else z.error("Unable to process user consent because user ID and consent data are invalid!");
          return N.updateClientTelemetryConsent();
        }, N.updateUserTelemetryConsent = function() {
          return d.getJarvisUserToken().then(function(e) {
            return e.userInfo && e.userInfo.userId ? d.getUserTelemetryConsent(e.userInfo.userId)
              .then(function(e) {
                return N.setUserConsent(e);
              }).catch(function(t) {
                z.error("Could not retrieve user telemetry consent settings, resetting all."), N
                  .resetUserConsent(e.userInfo.userId);
              }) : (z.error(
                  "No valid logged in user detected, falling back to client consent setting."), N
                .updateClientTelemetryConsent());
          }).catch(function(e) {
            return z.error(
                "Unable to get Jarvis user token, falling back to client consent setting. Error: ", e
                ), N.updateClientTelemetryConsent();
          });
        }, N.onUserConsentChanged = function(e) {
          e.userId && (z.info(
              "telemetryService received Accounts module user consent changed notification."), N
            .setUserConsent(e));
        }, N.updateServer = function(e) {
          i.updateServer(e);
        }, N.init = function() {
          return z.info("Initialize TelemetryService"), l.on(x.DESKTOP_STATE, N.saveDesktopMode), l.on(T
              .USER_LOGGED_IN, N.onServiceLogin), l.on(T.BROADCAST_ERROR, N.onBCError), l.on(O
              .UPLOAD_COMPLETE, N.onUploadComplete), f.register(m.USER_CONSENT_CHANGED, g
              .USER_CONSENT_CHANGED), l.on(g.USER_CONSENT_CHANGED, N.onUserConsentChanged), N
            .updateUserTelemetryConsent();
        }, N.setExperienceControlInfo = i.setExperienceControlInfo, N.resetExperienceControlInfo = i
        .resetExperienceControlInfo, N.syncExperienceControlInfo = i.syncExperienceControlInfo;
    }
  ]);
  exports.telemetryService = f;
}
