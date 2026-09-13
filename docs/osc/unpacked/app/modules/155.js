// ─────────────────────────────────────────────────────────────
// APP MODULE 155
// role       : directive nvBase
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
  }), t.nvBase = void 0;
  var o = n(1),
    r = n(315),
    a = i(r);
  n(154), n(170), n(171), n(225);
  var l = o.ngMainModule.directive("nvBase", function() {
    return {
      restrict: "E",
      scope: {},
      template: a.default,
      controller: "BaseController",
      controllerAs: "base"
    }
  });
  t.nvBase = l
}
