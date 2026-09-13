// ─────────────────────────────────────────────────────────────
// APP MODULE 164
// role       : directive nvChevron
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
  }), t.nvChevron = void 0;
  var o = n(1),
    r = n(320),
    a = i(r);
  n(163), n(5);
  var l = o.ngMainModule.directive("nvChevron", function() {
    return {
      restrict: "E",
      scope: {
        input: "=",
        onSelected: "&"
      },
      template: a.default,
      controller: "nvChevronController",
      controllerAs: "nvChevron"
    }
  });
  t.nvChevron = l
}
