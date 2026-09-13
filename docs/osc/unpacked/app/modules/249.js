// ─────────────────────────────────────────────────────────────
// APP MODULE 249
// role       : directive nvPreferencesMods
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
  }), t.nvPreferencesMods = void 0;
  var o = n(1),
    r = n(359),
    a = i(r);
  n(248), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesMods", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesModsController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesMods = l
}
