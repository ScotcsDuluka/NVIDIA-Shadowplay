// ─────────────────────────────────────────────────────────────
// APP MODULE 261
// role       : controller PreferencesStreamController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(1);
  n(4), n(33);
  i.ngMainModule.controller("PreferencesStreamController", ["$log", "$scope", "$state", "shadowPlayService",
    "eventAggregator", "coplayService", "KEYBOARD_EVENTS", "OSC_KEYBOARD", "COPLAY_STATE",
    function(e, t, n, i, o, r, a, l, s) {
      var d = this;
      d.title = "l10n.settings", d.icon = "icon-settings", d.status = "";
      var c = e.getInstance("main.preferences.stream/preferencesstreamcontroller");
      d.entries = {
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
      }, d.streamStatus = !1, d.oldStreamStatus = !1, d.settingsDisabled = !1, d.setStreamStatus = function() {
        var e = d.streamStatus;
        d.oldStreamStatus !== e && (c.info("setting: ", e), d.oldStreamStatus = d.streamStatus, r
          .setLocalCoplayFlag(e), i.setCoplayEnabled(e).then(function(t) {
            c.info("Setting stream enabled: " + e), d.streamStatus = e, d.oldStreamStatus = e
          }, function(e) {
            c.error("ERROR setting stream enabled: ", e)
          }))
      }, d.isCoplayActive = function() {
        var e = r.getState();
        d.settingsDisabled = e !== s.OFF
      }, d.initPreferencesStream = function() {
        i.getCoplaySupported().then(function(e) {
          return c.info("Coplay supported: ", e), i.getCoplayEnabled()
        }).then(function(e) {
          c.info("Coplay enabled: ", e), d.streamStatus = e, d.oldStreamStatus = e, d.isCoplayActive()
        }, function(e) {
          c.error("ERROR retrieving coplay enabled: ", e), d.streamStatus = !1, d.oldStreamStatus = !1
        })
      }, d.done = function() {
        n.go("main.preferences")
      }, d.keyUp = function(e, t) {
        e.keyCode === l.ENTER && (d.streamStatus = 1 !== t, d.setStreamStatus(d.streamStatus))
      }, o.on(a.ESCAPE, d.done), d.initPreferencesStream(), t.$on("$destroy", function() {
        o.off(a.ESCAPE, d.done)
      })
    }
  ])
}
