// ─────────────────────────────────────────────────────────────
// APP MODULE 173
// role       : directive nvCoplayGuestControls
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
  }), t.nvCoplayGuestControls = void 0;
  var o = n(1),
    r = n(324),
    a = i(r);
  n(172), n(5), n(6);
  var l = o.ngMainModule.directive("nvCoplayGuestControls", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "CoPlayGuestControlsController",
      controllerAs: "coplayGuestControls"
    }
  });
  t.nvCoplayGuestControls = l
}
