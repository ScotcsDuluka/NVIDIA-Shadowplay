// ─────────────────────────────────────────────────────────────
// APP MODULE 255
// role       : directive nvPreferencesPerformance
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
  }), t.nvPreferencePerformance = void 0;
  var o = n(1),
    r = n(362),
    a = i(r);
  n(254);
  var l = o.ngMainModule.directive("nvPreferencesPerformance", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesPerformanceController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencePerformance = l
}
