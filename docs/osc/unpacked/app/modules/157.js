// ─────────────────────────────────────────────────────────────
// APP MODULE 157
// role       : directive nvConfirmation
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
  }), t.nvConfirmation = void 0;
  var o = n(1),
    r = n(316),
    a = i(r);
  n(156), n(6);
  var l = o.ngMainModule.directive("nvConfirmation", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "confirmationController",
      controllerAs: "confirmation"
    }
  });
  t.nvConfirmation = l
}
