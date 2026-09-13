// ─────────────────────────────────────────────────────────────
// APP MODULE 256
// role       : controller PreferencesPrivacyControlController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.PreferencesPrivacyControlController = void 0;
  var i = n(1);
  n(4);
  var o = i.ngMainModule.controller("PreferencesPrivacyControlController", ["$log", "$scope", "$state", "$q",
    "shadowPlayService", "broadcastService", "sdkService", "eventAggregator", "KEYBOARD_EVENTS", "OSC_KEYBOARD",
    "BROADCAST_STATES",
    function(e, t, n, i, o, r, a, l, s, d, c) {
      var u = this;
      u.title = "l10n.settings", u.icon = "icon-settings", u.status = "", u.settingsDisabled = !1;
      var f = e.getInstance("main.preferences.privacy-control/preferencesprivacycontrolcontroller");
      u.entries = {
        on: {
          name: "on",
          title: "l10n.yes",
          value: !0,
          initialFocus: !0
        },
        off: {
          name: "off",
          title: "l10n.no",
          value: !1,
          initialFocus: !1
        }
      }, u.privacyControlStatus = !1, u.oldPrivacyControlStatus = !1, u.setPrivacyControlStatus = function() {
        var e = u.privacyControlStatus;
        u.oldPrivacyControlStatus !== e && (f.info("setting: ", e), u.oldPrivacyControlStatus = u
          .privacyControlStatus, o.setDesktopCaptureEnabled(e).then(function(t) {
            f.info("Setting privacy control enabled: " + e), u.oldPrivacyControlStatus = e
          }, function(e) {
            f.error("ERROR setting privacy control enabled: ", e)
          }))
      };
      var m = function(e) {
        return i.all([o.isMRActive(), o.isIREnabled(), r.getBroadcastState(), a.isHighlightsActive()])
      };
      u.initPreferencesPrivacyControl = function() {
        return m().then(function(e) {
          return u.settingsDisabled = e[0] || e[1] || e[3] || e[2] === c.ACTIVE || e[2] === c.PAUSED, o
            .getDesktopCaptureSupported()
        }).then(function(e) {
          return f.info("Privacy control supported: ", e), o.getDesktopCaptureEnabled()
        }).then(function(e) {
          f.info("Privacy control enabled: ", e), u.privacyControlStatus = e, u.oldPrivacyControlStatus = e
        }, function(e) {
          f.error("ERROR retrieving privacy control enabled: ", e), u.privacyControlStatus = !1, u
            .oldPrivacyControlStatus = !1
        })
      }, u.done = function() {
        n.go("main.preferences")
      }, u.keyUp = function(e, t) {
        e.keyCode === d.ENTER && (u.privacyControlStatus = 1 !== t, u.setPrivacyControlStatus())
      }, l.on(s.ESCAPE, u.done), u.initPreferencesPrivacyControl(), t.$on("$destroy", function() {
        l.off(s.ESCAPE, u.done)
      })
    }
  ]);
  t.PreferencesPrivacyControlController = o
}
