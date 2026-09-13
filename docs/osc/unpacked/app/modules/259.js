// ─────────────────────────────────────────────────────────────
// APP MODULE 259
// role       : directive nvPreferencesRecordings
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
  }), t.nvPreferencesRecordings = void 0;
  var o = n(1),
    r = n(364),
    a = i(r);
  n(258), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesRecordings", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesRecordingsController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesRecordings = l
}
