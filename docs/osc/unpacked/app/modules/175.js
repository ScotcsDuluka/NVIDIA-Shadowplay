// ─────────────────────────────────────────────────────────────
// APP MODULE 175
// role       : directive nvCoplayInvite
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
  }), t.nvCoplayInvite = void 0;
  var o = n(1),
    r = n(325),
    a = i(r);
  n(174), n(5), n(6);
  var l = o.ngMainModule.directive("nvCoplayInvite", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "CoPlayInviteController",
      controllerAs: "coplayInvite"
    }
  });
  t.nvCoplayInvite = l
}
