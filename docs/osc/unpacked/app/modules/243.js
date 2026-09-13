// ─────────────────────────────────────────────────────────────
// APP MODULE 243
// role       : directive nvPreferencesHighlights
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
  }), t.nvPreferencesHighlights = void 0;
  var o = n(1),
    r = n(356),
    a = i(r);
  n(242), n(5), n(6), n(14), n(96);
  var l = o.ngMainModule.directive("nvPreferencesHighlights", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesHighlightsController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesHighlights = l
}
