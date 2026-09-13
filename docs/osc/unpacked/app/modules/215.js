// ─────────────────────────────────────────────────────────────
// APP MODULE 215
// role       : directive nvFilterSlider
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
  }), t.filterSlider = void 0;
  var o = n(1),
    r = n(342),
    a = i(r);
  n(14);
  var l = o.ngMainModule.directive("nvFilterSlider", function() {
    return {
      restrict: "E",
      scope: {
        aControl: "=filterSlider"
      },
      template: a.default
    }
  });
  t.filterSlider = l
}
