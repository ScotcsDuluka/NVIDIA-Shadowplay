// ─────────────────────────────────────────────────────────────
// APP MODULE 248
// role       : controller PreferencesModsController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(1);
  n(40);
  i.ngMainModule.controller("PreferencesModsController", ["$log", "$scope", "$state", "eventAggregator",
    "nvCameraService", "telemetryService", "KEYBOARD_EVENTS", "OSC_KEYBOARD", "TELEMETRY_OSC_EVENT_NAMES",
    "TELEMETRY_OSC_MENU_TYPE", "OSC_SETTINGS_CONTROL_TYPES", "TELEMETRY_OSC_TOGGLE_STATE",
    function(e, t, n, i, o, r, a, l, s, d, c, u) {
      var f = this;
      f.title = "l10n.settings", f.icon = "icon-settings", f.status = "";
      var m = e.getInstance("preferencesModsController");
      f.entries = {
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
      }, f.modsStatus = !1, f.oldModsStatus = !1, f.setModsStatus = function() {
        var e = f.modsStatus;
        f.oldModsStatus !== e && (m.info("setModsStatus: ", e), f.oldModsStatus = f.modsStatus, o
          .setModsStatusToNvcamera(e), f.sendGlobalOnOffState(e))
      }, f.initPreferencesMods = function() {
        f.modsStatus = f.oldModsStatus = o.getModsEnableStatus(), m.info("initPreferencesMods modsStatus = ", f
          .modsStatus)
      }, f.done = function() {
        n.go("main.preferences")
      }, f.keyUp = function(e, t) {
        e.keyCode === l.ENTER && (f.modsStatus = 1 !== t, f.setModsStatus(f.modsStatus))
      }, f.sendGlobalOnOffState = function(e) {
        r.push(s.OSC_SETTINGS_EVENT, {
          menuName: d.freestyle,
          controlType: c.globalonoff,
          toggleState: e ? u.on : u.off
        })
      }, i.on(a.ESCAPE, f.done), f.initPreferencesMods(), t.$on("$destroy", function() {
        i.off(a.ESCAPE, f.done)
      })
    }
  ])
}
