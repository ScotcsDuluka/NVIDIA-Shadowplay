// ─────────────────────────────────────────────────────────────
// APP MODULE 162
// role       : directive nvOscTopBar
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
  }), t.nvOscTopBar = void 0;
  var o = n(1),
    r = n(319),
    a = i(r);
  n(161), n(5);
  var l = o.ngMainModule.directive("nvOscTopBar", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "MainTopBarController",
      controllerAs: "mainTopBarCtrl"
    }
  });
  t.nvOscTopBar = l
}
