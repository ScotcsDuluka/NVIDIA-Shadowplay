// ─────────────────────────────────────────────────────────────
// APP MODULE 23
// role       : service oscNotificationService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.oscNotificationService = void 0;
  var r = n(8),
    a = o(r),
    l = n(3),
    s = i(l),
    d = n(2),
    c = d.ngMainCommonModule.service("oscNotificationService", ["$window", "$log", "eventAggregator",
      "oscDisplayService", "NOTIFICATION_EVENT", "COMMON_EVENTS", "piplConfigService",
      function(e, t, n, i, o, r, l) {
        function d(e, t, i, r) {
          var a = !1;
          if (s.each(g, function(t) {
              s.indexOf(t.notifs, e) >= 0 && (t.enabled || (a = !0))
            }), !a) {
            var l = {
              selection: e,
              arg1: i,
              arg2: r,
              flip: t
            };
            n.trigger(o, l)
          }
        }

        function c(e) {
          g.BRStarted && (g.BRStarted.available = e), g.BRPaused && (g.BRPaused.available = e)
        }

        function u(e) {
          f.info("Connect status:", e.isConnectEnabled), c(e.isConnectEnabled)
        }
        var f = t.getInstance("osc/notificationService"),
          m = "notification-settings",
          g = {
            openHeader: {
              header: "l10n.general",
              viewHeader: !0
            },
            openShare: {
              name: "l10n.openShare",
              notifs: ["OPEN_SHARE"],
              enabled: !0,
              available: !0
            },
            whisperModeSettings: {
              name: "l10n.enabledWhisperModeSettings",
              notifs: ["WHISPER_MODE_ENABLED", "WHISPER_MODE_DISABLED", "WHISPER_MODE_ENABLED_GAMESTART"],
              enabled: !0,
              available: !0
            },
            anselHeader: {
              header: "l10n.anselNotifHeader",
              viewHeader: !0
            },
            anselReadyAppStarted: {
              name: "l10n.openAnsel",
              notifs: ["ANSEL_READY_APP_STARTED"],
              enabled: !0,
              available: !0
            },
            savedHeader: {
              header: "l10n.gallery",
              viewHeader: !0
            },
            savedIR: {
              name: "l10n.savedLastRecordToGallery",
              notifs: ["INSTANT_REPLAY_SAVED", "INSTANT_REPLAY_SAVED_TO_GALLERY"],
              enabled: !0,
              available: !0
            },
            savedMR: {
              name: "l10n.notificationManualRecordStoppedAndSavedToGallery",
              notifs: ["RECORD_STOPPED", "RECORD_STOPPED_AND_SAVED_TO_GALLERY"],
              enabled: !0,
              available: !0
            },
            savedSS: {
              name: "l10n.notificationScreenshotSavedToGallery",
              notifs: ["SCREENSHOT_SAVED", "SCREENSHOT_SAVED_TO_GALLERY", "PHOTOGRAPHIC_SCREENSHOT_SAVED_TO_GALLERY"],
              enabled: !0,
              available: !0
            },
            statusHeader: {
              header: "l10n.statusNotifications",
              viewHeader: !0
            },
            IROnOff: {
              name: "l10n.instantReplayOnOff",
              notifs: ["INSTANT_REPLAY_STARTED", "INSTANT_REPLAY_STOPPED"],
              enabled: !0,
              available: !0
            },
            MRStarted: {
              name: "l10n.notificationManualRecordStarted",
              notifs: ["RECORD_STARTED"],
              enabled: !0,
              available: !0
            },
            BRStarted: {
              name: "l10n.broadcastStarted",
              notifs: ["BROADCAST_STARTED", "BROADCAST_STOPPED"],
              enabled: !0,
              available: !0
            },
            BRPaused: {
              name: "l10n.broadcastPaused",
              notifs: ["BROADCAST_PAUSED", "BROADCAST_RESUMED"],
              enabled: !0,
              available: !0
            },
            highlightsHeader: {
              header: "l10n.highlights",
              viewHeader: !0
            },
            savedHL: {
              name: "l10n.notificationHighlightSaved",
              notifs: ["HIGHLIGHTS_SAVED"],
              enabled: !0,
              available: !0
            },
            screenshotHDRError: {
              name: "l10n.ScreenshotHDRError",
              notifs: ["HDR_ERROR_SCREENSHOT"],
              enabled: !0,
              available: !0
            },
            recordHDRError: {
              name: "l10n.RecordHDRError",
              notifs: ["HDR_ERROR_RECORD"],
              enabled: !0,
              available: !0
            },
            broadcastHDRError: {
              name: "l10n.BroadcastHDRError",
              notifs: ["HDR_ERROR_BROADCAST"],
              enabled: !0,
              available: !0
            },
            hlHDRError: {
              name: "l10n.HighlightsHDRError",
              notifs: ["HDR_ERROR_HL"],
              enabled: !0,
              available: !0
            },
            performanceMonitoringHeader: {
              header: "l10n.perfmonoc.performanceMonitoring",
              viewHeader: !0
            },
            performanceMonitoringRectAlignSupport: {
              name: "l10n.perfmonoc.autoRectAlignmentSupport",
              notifs: ["PERFMON_RECTALIGNMENT_SUPPORT"],
              enabled: !0,
              available: !0
            },
            performanceMonitoringFlashIndicatorSupport: {
              name: "l10n.perfmonoc.rfiOptionSupport",
              notifs: ["PERFMON_RFI_SUPPORT"],
              enabled: !0,
              available: !0
            }
          };
        this.show = function(e, t, n) {
          d(e, !1, t, n)
        }, this.flipTo = function(e, t, n) {
          d(e, !0, t, n)
        }, this.getConfigs = function() {
          return g
        }, this.setConfigs = function(e) {
          g = e;
          var t = {};
          s.each(g, function(e, n) {
            s.isUndefined(e.enabled) || (t[n] = e.enabled)
          }), i.setLocalStorage(m, (0, a.default)(t))
        }, this.init = function() {
          l.isConnectEnabled().then(function(t) {
            c(t), n.on(r.PIPL_CONFIG_UPDATED, u);
            try {
              var i = e.localStorage.getItem(m);
              if (i) {
                var o = JSON.parse(i);
                s.each(g, function(e, t) {
                  s.has(o, t) && (e.enabled = o[t])
                })
              }
            } catch (e) {
              f.error("init() failed: ", e)
            }
          })
        }
      }
    ]);
  t.oscNotificationService = c
}
