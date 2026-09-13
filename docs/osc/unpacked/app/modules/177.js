// ─────────────────────────────────────────────────────────────
// APP MODULE 177
// role       : directive settingsCustomize
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
  }), t.settingsCustomize = void 0;
  var o = n(1),
    r = n(326),
    a = i(r);
  n(176), n(5), n(6), n(14);
  var l = o.ngMainModule.directive("settingsCustomize", function() {
    return {
      restrict: "E",
      scope: {
        nvChangeSettings: "=",
        onCustomizeCloseComplete: "&"
      },
      template: a.default,
      controller: "SettingsCustomizeController",
      controllerAs: "customizeMenu"
    }
  });
  t.settingsCustomize = l
}
