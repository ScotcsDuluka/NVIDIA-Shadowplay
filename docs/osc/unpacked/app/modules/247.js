// ─────────────────────────────────────────────────────────────
// APP MODULE 247
// role       : directive nvPreferencesMenu
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
  }), t.nvPreferencesMenu = void 0;
  var o = n(1),
    r = n(358),
    a = i(r);
  n(246), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesMenuController",
      controllerAs: "preferencesMenu"
    }
  });
  t.nvPreferencesMenu = l
}
