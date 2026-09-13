// ─────────────────────────────────────────────────────────────
// APP MODULE 231
// role       : directive nvOsdViewerCount
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
  }), t.nvOsdViewerCount = void 0;
  var o = n(1),
    r = n(350),
    a = i(r);
  n(230);
  var l = o.ngMainModule.directive("nvOsdViewerCount", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdViewerCountController",
      controllerAs: "cont"
    }
  });
  t.nvOsdViewerCount = l
}
