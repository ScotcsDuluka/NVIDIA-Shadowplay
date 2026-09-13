// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 250
// controller PreferencesNotificationsController
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
  var r = require(8),
    a = o(r),
    l = require(3),
    s = i(l),
    d = require(1) /* app/1 — main (module) */;
  require(93) /* app/93 — quietMode2Service (service) */;
  d.ngMainModule.controller("PreferencesNotificationsController", ["$log", "$scope", "$state",
    "eventAggregator", "oscNotificationService", "telemetryService", "nvCameraService", "KEYBOARD_EVENTS",
    "TELEMETRY_OSC_EVENT_NAMES", "quietMode2Service", "piplConfigService", "COMMON_EVENTS",
    function(e, t, n, i, o, r, l, d, c, u, f, m) {
      function g() {
        var e = !1,
          t = !1;
        s.forEach(b.notifiers, function(n) {
          s.isUndefined(n.viewHeader) && n.available && (n.enabled ? t = !0 : s.isUndefined(n
            .enabled) || s.isUndefined(n.name) || (e = !0));
        }), !e && t ? b.globalToggle = !0 : e && (b.globalToggle = !1);
      }

      function p() {
        u.getStateInfo().then(function(e) {
          x.info("Quiet Mode2 getSupportInfo success: ", e), e.supported === !1 ? b.notifiers
            .whisperModeSettings.name = void 0 : b.notifiers.whisperModeSettings.name =
            "l10n.enabledWhisperModeSettings";
        }, function(e) {
          x.error("Quiet Mode getSupportInfo2 failed with error: ", e);
        });
      }

      function h(e) {
        x.info("Connect status:", e.isConnectEnabled), b.isConnectEnabled = e.isConnectEnabled, b
          .notifiers = o.getConfigs();
      }
      var b = this;
      b.title = "l10n.settings", b.icon = "icon-settings", b.status = "", b.globalToggle = !1, b
        .notifiers = {};
      var x = e.getInstance("main.preferences.stream/preferencesnotificationscontroller");
      b.checkExperimentalFeatures = function() {
        return l.isGfeAnselSupported().then(function(e) {
          x.info("GFE-IPC-Ansel Supported? " + e), e === !1 ? b.notifiers.anselReadyAppStarted
            .name = void 0 : b.notifiers.anselReadyAppStarted.name = "l10n.openAnsel", b.notifiers
            .anselHeader.viewHeader = e;
        }, function(e) {
          x.error("call to isGfeAnselSupported() returned with message " + (0, a.default)(e));
        });
      }, b.initPreferencesNotifications = function() {
        f.isConnectEnabled().then(function(e) {
          return b.isConnectEnabled = e;
        }).then(function() {
          i.on(m.PIPL_CONFIG_UPDATED, h), b.notifiers = o.getConfigs(), b
          .checkExperimentalFeatures().then(function() {
            g();
          }), p();
        });
      }, b.globalChanged = function() {
        var e = b.globalToggle === !0;
        s.forEach(b.notifiers, function(t) {
          t.enabled = e;
        });
      }, b.notifChanged = function() {
        g();
      }, b.done = function() {
        n.go("main.preferences");
      }, i.on(d.ESCAPE, b.done), b.initPreferencesNotifications(), t.$on("$destroy", function() {
        var e = {
          openShare: r.bool2Toggle(b.notifiers.openShare.enabled),
          savedIR: r.bool2Toggle(b.notifiers.savedIR.enabled),
          savedMR: r.bool2Toggle(b.notifiers.savedMR.enabled),
          savedSS: r.bool2Toggle(b.notifiers.savedSS.enabled),
          irOnOff: r.bool2Toggle(b.notifiers.IROnOff.enabled),
          mrStarted: r.bool2Toggle(b.notifiers.MRStarted.enabled),
          brStarted: r.bool2Toggle(b.notifiers.BRStarted.enabled),
          brPaused: r.bool2Toggle(b.notifiers.BRPaused.enabled),
          savedHL: r.bool2Toggle(b.notifiers.savedHL.enabled)
        };
        r.push(c.OSC_PREFERENCES_NOTIFICATIONS_CHANGED, e), o.setConfigs(b.notifiers), i.off(d.ESCAPE,
          b.done), i.off(m.PIPL_CONFIG_UPDATED, h);
      });
    }
  ]);
}
