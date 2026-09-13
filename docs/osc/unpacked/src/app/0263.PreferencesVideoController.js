// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 263
// controller PreferencesVideoController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(1) /* app/1 — main (module) */;
  require(4) /* app/4 — shadowPlayService (service) */, require(26) /* app/26 — telemetryService (service) */;
  i.ngMainModule.controller("PreferencesVideoController", ["$log", "$scope", "$state", "$stateParams",
    "$timeout", "$q", "shadowPlayService", "sdkService", "eventAggregator", "KEYBOARD_EVENTS",
    "VIDEO_STATE", "OSC_MODE",
    function(e, t, n, i, o, r, a, l, s, d, c, u) {
      var f = this;
      f.title = "l10n.settings", f.icon = "icon-settings", f.status = "", f.settingsDisabled = !1, f
        .settingsData = {};
      var m = e.getInstance("main.preferences.video");
      f.fromMainMenu = function() {
        return a.getVideoState() === c.MAIN;
      }, f.return = function() {
        f.fromMainMenu() ? n.go("main.main-menu") : n.go("main.preferences");
      }, f.back = function() {
        f.settingsData.executeBack = !0;
      }, f.save = function() {
        f.settingsData.executeSave = !0;
      }, f.customizeCloseComplete = function() {
        m.info("Customize close complete (video)"), f.return();
      }, f.enableEscapeEvent = function(e) {
        var t = e;
        o(function() {
          t === !0 ? s.on(d.ESCAPE, f.back) : s.off(d.ESCAPE, f.back);
        }, 200);
      };
      var g = function(e) {
        return r.all([a.isMRActive(), a.isIREnabled(), l.isHighlightsActive()]);
      };
      f.initialize = function() {
        return m.info("Initialize Video Preferences"), f.enableEscapeEvent(!0), g().then(function(e) {
          f.settingsDisabled = e[0] || e[1] || e[2], f.settingsData = {
            feature: u.INSTANTREPLAY,
            source: a.getVideoState(),
            disabled: f.settingsDisabled,
            executeBack: !1,
            executeSave: !1,
            callback: i.callback
          };
        });
      }, f.initialize(), t.$on("$destroy", function() {
        f.enableEscapeEvent(!1);
      });
    }
  ]);
}
