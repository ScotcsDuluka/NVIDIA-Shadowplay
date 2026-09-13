// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 258
// controller PreferencesRecordingsController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.PreferencesRecordingsController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(4) /* app/4 — shadowPlayService (service) */;
  var o = i.ngMainModule.controller("PreferencesRecordingsController", ["$log", "$scope", "$state", "$q",
    "shadowPlayService", "sdkService", "eventAggregator", "RECORDING_PATH_TYPES", "KEYBOARD_EVENTS",
    function(e, t, n, i, o, r, a, l, s) {
      var d = this;
      d.title = "l10n.settings", d.icon = "icon-settings", d.status = "", d.settingsDisabled = !1;
      var c = e.getInstance("osc/preferencesrecordingscontroller");
      d.videosPath = "", d.tempFilesPath = "";
      var u = function(e) {
        return i.all([o.isMRActive(), o.isIREnabled(), r.isHighlightsActive()]);
      };
      d.initPreferencesRecordings = function() {
        return u().then(function(e) {
          return d.settingsDisabled = e[0] || e[1] || e[2], o.getRecordingPaths();
        }).then(function(e) {
          d.videosPath = e.videos, d.tempFilesPath = e.tempFiles;
        }, function(e) {
          c.error("ERROR, cannot get recording paths");
        });
      }, d.getVideosPath = function() {
        d.settingsDisabled !== !0 && n.go("main.preferences.recordings.folder-browser", {
          parentView: "main.preferences.recordings",
          heading: "l10n.folderBrowserTitleVideos",
          currentPath: d.videosPath,
          pathType: l.VIDEOS
        });
      }, d.getTempFilesPath = function() {
        d.settingsDisabled !== !0 && n.go("main.preferences.recordings.folder-browser", {
          parentView: "main.preferences.recordings",
          heading: "l10n.folderBrowserTitleTempFiles",
          currentPath: d.tempFilesPath,
          pathType: l.TEMP_FILES
        });
      }, d.done = function() {
        n.go("main.preferences");
      }, a.on(s.ESCAPE, d.done), t.$on("$destroy", function() {
        a.off(s.ESCAPE, d.done);
      });
    }
  ]);
  exports.PreferencesRecordingsController = o;
}
