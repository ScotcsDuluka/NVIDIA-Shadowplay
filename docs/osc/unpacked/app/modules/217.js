// ─────────────────────────────────────────────────────────────
// APP MODULE 217
// role       : directive modsMenu
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
  }), t.modsMenu = void 0;
  var o = n(1),
    r = n(343),
    a = i(r);
  n(216), n(100);
  var l = o.ngMainModule.directive("modsMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "ModsMenuController",
      controllerAs: "cont"
    }
  });
  t.modsMenu = l
}
