// ─────────────────────────────────────────────────────────────
// APP MODULE 257
// role       : directive nvPreferencesPrivacyControl
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
  }), t.nvPreferencesPrivacyControl = void 0;
  var o = n(1),
    r = n(363),
    a = i(r);
  n(256), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesPrivacyControl", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesPrivacyControlController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesPrivacyControl = l
}
