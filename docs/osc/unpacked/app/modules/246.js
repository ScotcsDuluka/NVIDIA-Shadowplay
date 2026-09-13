// ─────────────────────────────────────────────────────────────
// APP MODULE 246
// role       : controller PreferencesMenuController
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
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.PreferencesMenuController = void 0;
  var o = n(3),
    r = i(o),
    a = n(1);
  n(4), n(33), n(23);
  var l = a.ngMainModule.controller("PreferencesMenuController", ["$scope", "$state", "$log", "$stateParams",
    "eventAggregator", "COMMON_EVENTS", "shadowPlayService", "oscService", "errorDialogService", "coplayService",
    "telemetryService", "nvCameraService", "octoolService", "piplConfigService", "KEYBOARD_EVENTS", "OSC_CONFIG",
    "OSC_KEYBOARD", "AUDIO_STATE", "VIDEO_STATE", "TELEMETRY_OSC_PERF_ID", "TELEMETRY_OSC_EVENT_NAMES",
    "TELEMETRY_OSC_CAPTURE_TYPE",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S) {
      function E(e) {
        _.info("Connect status:", e.isConnectEnabled), k.isConnectEnabled = e.isConnectEnabled;
        var t = k.getTileIndex("connect-tile");
        k.tiles[t].visible = k.isConnectEnabled;
        var n = k.getTileIndex("broadcast-tile");
        k.tiles[n].visible = k.isConnectEnabled
      }
      var k = this,
        _ = n.getInstance("osc/PreferencesMenuController");
      k.title = "l10n.settings", k.icon = "icon-settings", k.status = "", k.tileIndex = 0, k.tiles = [], k
        .multiTrackAudioAvailable = !1, k.isConnectEnabled = !1, k.initPreferencesMenu = function() {
          g.isConnectEnabled().then(function(e) {
            return k.isConnectEnabled = e, o.on(a.PIPL_CONFIG_UPDATED, E), l.isMultiTrackAudioAvailable().then(
              function(e) {
                k.multiTrackAudioAvailable = e, k.initializePreferencesMenu()
              })
          })
        }, k.initializePreferencesMenu = function() {
          k.tiles = [{
            name: "connect-tile",
            title: "l10n.connect",
            icon: "icon-connect",
            state: "main.preferences.connect",
            visible: k.isConnectEnabled,
            precondition: function() {
              return s.onlineState.online
            },
            fallback: function() {
              _.info("No Internet connection"), d.show("l10n.systemRequirement",
                "l10n.notificationCoplayNetworkUnavailable")
            }
          }, {
            name: "hangout-tile",
            title: "l10n.hangout",
            icon: "icon-connect",
            state: "main.preferences.hangout",
            visible: !0
          }, {
            name: "overlays-tile",
            title: "l10n.hudLayout",
            icon: "icon-pref_overlays",
            state: "main.preferences.overlays",
            visible: !0
          }, {
            name: "keyboard-shortcuts-tile",
            title: "l10n.keyboardShortcuts",
            icon: "icon-pref_keyboard",
            state: "main.preferences.keyboard-shortcuts",
            visible: !0
          }, {
            name: "recordings-tile",
            title: "l10n.recordings",
            icon: "icon-pref_recordings",
            state: "main.preferences.recordings",
            visible: !0
          }, {
            name: "stream-tile",
            title: "l10n.stream",
            subtitle: "l10n.experimental",
            icon: "icon-stream",
            state: "main.preferences.stream",
            visible: !1
          }, {
            name: "broadcast-tile",
            title: "l10n.broadcastLive",
            icon: "icon-broadcast",
            state: "main.preferences.broadcast",
            visible: k.isConnectEnabled
          }, {
            name: "highlights-tile",
            title: "l10n.highlights",
            icon: "icon-highlights",
            state: "main.preferences.highlights",
            visible: !0
          }, {
            name: "mods-tile",
            title: "l10n.anselMods",
            icon: "icon-freestyle",
            state: "main.preferences.mods",
            visible: f.isModsOn() && !f.hasVariableAvailability()
          }, {
            name: "audio-tile",
            title: "l10n.audio",
            icon: "icon-audio_mixer",
            state: "main.preferences.audio",
            visible: k.multiTrackAudioAvailable
          }, {
            name: "video-tile",
            title: "l10n.videoCapture",
            subtitle: "l10n.videoCaptureText",
            icon: "icon-video_capture",
            state: "main.preferences.video",
            visible: !0
          }, {
            name: "notifications-tile",
            title: "l10n.notifications",
            icon: "icon-pref_notifications",
            state: "main.preferences.notifications",
            visible: !0
          }, {
            name: "privacy-control-tile",
            title: "l10n.privacyControl",
            icon: "icon-pref_privacy",
            state: "main.preferences.privacy-control",
            visible: !1
          }, {
            name: "perf-monitor",
            title: "l10n.perfmonoc.performanceMonitoring",
            icon: "icon-performance",
            state: "main.preferences.perfsettings",
            visible: m.getIsFeatureAvailable() && m.enableReflexEnhancements
          }];
          var e = l.isDesktopCaptureSettingShown().then(function(e) {
              var t = k.getTileIndex("privacy-control-tile");
              k.tiles[t].visible = e
            }),
            t = 0;
          t = k.getTileIndex("stream-tile"), k.tiles[t].visible = c.shouldShowCoplaySetting(), t = k.getTileIndex(
            "hangout-tile"), k.tiles[t].visible = h.hangouts, e.then(function() {
            i && i.callback && i.callback()
          })
        }, k.getTileIndex = function(e) {
          return r.findIndex(k.tiles, function(t) {
            return t.name === e
          })
        }, k.goToPreferenceState = function(e) {
          if (u.startPerf(y.keyboardShortcutScreen), !e.precondition || e.precondition && e.precondition()) {
            var n = null;
            "main.preferences.keyboard-shortcuts" === e.state && (n = function() {
                u.endPerfAfterDigest(y.keyboardShortcutScreen)
              }), "main.preferences.audio" === e.state && l.setAudioState(x.PREFERENCES),
              "main.preferences.video" !== e.state && "main.preferences.broadcast" !== e.state || (u.startPerf(y
                .customizeScreen), l.setVideoState(v.PREFERENCES), n = function(e) {
                u.endPerfAfterDigest(y.customizeScreen, e)
              }, "main.preferences.video" === e.state && (u.push(w.OSC_CAPTURE_DVR_CUSTOMIZE), u.push(w
                .OSC_CAPTURE_CUSTOMIZE, {
                  provider: S.instantReplay
                }))), t.go(e.state, {
                callback: n
              })
          } else e.fallback && e.fallback()
        }, k.back = function() {
          t.go("main.main-menu")
        }, k.mouseOver = function(e, t) {
          t.currentTarget.focus(), k.tileIndex = e
        }, k.isActiveTile = function(e) {
          return k.tileIndex === e
        }, k.incDown = function(e) {
          do {
            if (!(k.tileIndex < k.tiles.length - 1)) {
              e && (k.tileIndex = k.tiles.length);
              break
            }
            k.tileIndex++
          } while (k.tiles[k.tileIndex].visible === !1)
        }, k.incUp = function(e, t) {
          do {
            if (!(k.tileIndex > 0)) {
              t && (k.tileIndex = -1);
              break
            }
            k.tileIndex--
          } while (k.tiles[k.tileIndex].visible === !1)
        }, k.keyDown = function(e, t) {
          k.tileIndex = e, t.keyCode === b.TAB && t.shiftKey === !1 ? k.incDown(e, !0) : t.keyCode === b
            .DOWN_ARROW ? k.incDown(e, !1) : t.keyCode === b.UP_ARROW ? k.incUp(e, !1) : t.keyCode === b.TAB && t
            .shiftKey === !0 && k.incUp(e, !0)
        }, o.on(p.ESCAPE, k.back), e.$on("$destroy", function() {
          o.off(p.ESCAPE, k.back), o.off(a.PIPL_CONFIG_UPDATED, E)
        })
    }
  ]);
  t.PreferencesMenuController = l
}
