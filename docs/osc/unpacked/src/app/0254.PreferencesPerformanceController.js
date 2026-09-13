// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 254
// controller PreferencesPerformanceController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.PreferencesPerformanceController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(22) /* app/22 — octoolService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(34) /* app/34 — keyboardService (service) */;
  var o = i.ngMainModule.controller("PreferencesPerformanceController", ["octoolService", "$log", "$scope",
    "$state", "eventAggregator", "$timeout", "KEYBOARD_EVENTS", "shadowPlayService", "keyboardService",
    "RECORDING_PATH_TYPES",
    function(e, t, n, i, o, r, a, l, s, d) {
      function c() {
        l.getHotkeyShortcut(l.HotkeyShortcuts.RESETAVERAGES).then(function(e) {
          e && e.keys ? (u.isHotkeyAvailable = !0, u.hotkeyString = s.shortcutToStr(e.keys)) : u
            .isHotkeyAvailable = !1, "None" === u.hotkeyString && (u.isHotkeyAvailable = !1);
        }), l.getHotkeyShortcut(l.HotkeyShortcuts.TOGGLELOGGING).then(function(e) {
          e && e.keys ? (u.isLoggingHotkeyAvailable = !0, u.loggingHotkeyString = s.shortcutToStr(e
            .keys)) : u.isLoggingHotkeyAvailable = !1, "None" === u.loggingHotkeyString && (u
            .isLoggingHotkeyAvailable = !1);
        });
      }
      var u = this;
      u.title = "l10n.settings", u.icon = "icon-settings", u.status = "", u.flashIndicatorStatus = !1, u
        .rectAlignmentStatus = !0, u.currentCustAvgValue = 20, u.isHotkeyAvailable = !1, u
        .isLoggingHotkeyAvailable = !1, u.loggingPath = "C:\\", u.isRLA = !1, u.isSupportedDD = !1;
      var f = t.getInstance("osc/preferenceperformancecontroller"),
        m = 100;
      u.initPreferencesPerformanceSettings = function() {
        u.fetchCustAvgSampleSize(), u.getDefaultLoggingPath(), u.getRectAlignmentStatus(), u
          .getFlashIndicatorStatus(), u.isRLA = e.isRLAMonitor, u.isSupportedDD = e.isRLASupportedDD,
          c();
      }, u.fetchCustAvgSampleSize = function() {
        u.currentCustAvgValue = e.getCustAvgSampleSize(), f.info(
          "Fetched current custom average sample size - " + u.currentCustAvgValue);
      }, u.getDefaultLoggingPath = function() {
        var t = e.getLoggingPath();
        u.loggingPath = t ? t : u.loggingPath, f.info("Current logging path is: " + u.loggingPath);
      }, u.updateCustAvgSampleSize = function() {
        e.updateCustAvgSampleSize(u.currentCustAvgValue), f.info(
          "User changed custom average sample size - " + u.currentCustAvgValue);
      }, u.done = function() {
        i.go("main.preferences");
      }, u.setPath = function(t) {
        u.loggingPath = t, f.info("New logging path - ", t), e.storeLoggingPath(t);
      }, u.getLoggingPath = function() {
        i.go("main.preferences.recordings.folder-browser", {
          parentView: "main.preferences.perfsettings",
          heading: "l10n.folderBrowserTitleLogging",
          currentPath: u.loggingPath,
          callback: u.setPath,
          pathType: d.FILE_LOGGING
        });
      }, u.goToKeyboardShortcutsScreen = function() {
        i.go("main.preferences.keyboard-shortcuts");
      }, u.alwaysShowFlashIndicator = function() {
        f.info("alwaysShowFlashIndicator");
      }, u.alignRectangle = function() {
        f.info("alignRectangle"), e.updateRectAlignStatus(u.rectAlignmentStatus);
      }, u.getRectAlignmentStatus = function() {
        u.rectAlignmentStatus = e.getRectAlignStatus(), f.info("Fetched Rectangle Alignment Status - " +
          u.rectAlignmentStatus);
      }, u.toggleFlashIndicatorStatus = function() {
        u.isSupportedDD && (e.setFlashIndicatorSize(u.flashIndicatorStatus), f.info(
          "setFlashIndicatorSize"), e.updateFlashIndicatorStatus(u.flashIndicatorStatus), r(
          function() {
            (e.islatestRFISupported || e.islegacyRFISupported) && e.isRLAMonitor || u
              .flashIndicatorStatus || e.setFlashIndicatorVisibility(!u.flashIndicatorStatus);
          }, m));
      }, u.getFlashIndicatorStatus = function() {
        u.flashIndicatorStatus = e.getFlashIndicatorStatus(), f.info(
          "Fetched Flash Indicator Status - " + u.flashIndicatorStatus);
      }, o.on(a.ESCAPE, u.done), n.$on("$destroy", function() {
        o.off(a.ESCAPE, u.done);
      });
    }
  ]);
  exports.PreferencesPerformanceController = o;
}
