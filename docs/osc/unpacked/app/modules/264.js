// ─────────────────────────────────────────────────────────────
// APP MODULE 264
// role       : directive nvPreferencesVideo
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
  }), t.nvPreferencesVideo = void 0;
  var o = n(1),
    r = n(367),
    a = i(r);
  n(263), n(5), n(6), n(177);
  var l = o.ngMainModule.directive("nvPreferencesVideo", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesVideoController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesVideo = l
}
