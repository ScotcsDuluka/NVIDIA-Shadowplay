// ─────────────────────────────────────────────────────────────
// APP MODULE 241
// role       : directive nvPreferencesHangout
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
  }), t.nvPreferencesHangout = void 0;
  var o = n(1),
    r = n(355),
    a = i(r);
  n(240), n(6);
  var l = o.ngMainModule.directive("nvPreferencesHangout", [function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesHangoutController",
      controllerAs: "preferencesHangout"
    }
  }]);
  t.nvPreferencesHangout = l
}
