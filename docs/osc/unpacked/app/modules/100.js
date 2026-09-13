// ─────────────────────────────────────────────────────────────
// APP MODULE 100
// role       : directive nvFilterControl
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
  }), t.filterControl = void 0;
  var o = n(1),
    r = n(340),
    a = i(r);
  n(214), n(215), n(14);
  var l = o.ngMainModule.directive("nvFilterControl", function() {
    return {
      restrict: "E",
      scope: {
        aControl: "=filterControl"
      },
      template: a.default
    }
  });
  t.filterControl = l
}
