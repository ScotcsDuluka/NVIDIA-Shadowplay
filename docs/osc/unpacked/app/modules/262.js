// ─────────────────────────────────────────────────────────────
// APP MODULE 262
// role       : directive nvPreferencesStream
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
  }), t.nvPreferencesStream = void 0;
  var o = n(1),
    r = n(366),
    a = i(r);
  n(261), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesStream", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesStreamController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesStream = l
}
