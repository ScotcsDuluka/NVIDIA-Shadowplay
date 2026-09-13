// ─────────────────────────────────────────────────────────────
// APP MODULE 245
// role       : directive nvPreferencesKeyboardShortcuts
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
  }), t.nvPreferencesKeyboardShortcuts = void 0;
  var o = n(1),
    r = n(357),
    a = i(r);
  n(244), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesKeyboardShortcuts", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesKeyboardShortcutsController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesKeyboardShortcuts = l
}
