// ─────────────────────────────────────────────────────────────
// APP MODULE 239
// role       : directive nvPreferencesConnect
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
  }), t.nvPreferencesConnect = void 0;
  var o = n(1),
    r = n(354),
    a = i(r);
  n(238), n(5), n(6), n(95);
  var l = o.ngMainModule.directive("nvPreferencesConnect", [function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesConnectController",
      controllerAs: "preferencesConnect"
    }
  }]);
  t.nvPreferencesConnect = l
}
