// ─────────────────────────────────────────────────────────────
// APP MODULE 101
// role       : directive nvPreferencesRecordingsFolderBrowser
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvPreferencesRecordingsFolderBrowser = void 0;
  var o = n(1),
    r = n(365),
    a = i(r);
  n(260), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesRecordingsFolderBrowser", function() {
    return {
      restrict: "E",
      scope: {
        folderParams: "=folderParams"
      },
      template: a.default,
      controller: "FolderBrowserController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesRecordingsFolderBrowser = l
}
