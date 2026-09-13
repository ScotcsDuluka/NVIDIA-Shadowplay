// ─────────────────────────────────────────────────────────────
// APP MODULE 160
// role       : directive nvMain
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
  }), t.nvMain = void 0;
  var o = n(1),
    r = n(318),
    a = i(r);
  n(162);
  var l = o.ngMainModule.directive("nvMain", function() {
    return {
      restrict: "E",
      scope: {},
      template: a.default
    }
  });
  t.nvMain = l
}
