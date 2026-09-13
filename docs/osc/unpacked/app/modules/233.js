// ─────────────────────────────────────────────────────────────
// APP MODULE 233
// role       : directive nvOsdWebcam
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
  }), t.nvOsdWebcam = void 0;
  var o = n(1),
    r = n(351),
    a = i(r);
  n(232);
  var l = o.ngMainModule.directive("nvOsdWebcam", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdWebcamController",
      controllerAs: "cont"
    }
  });
  t.nvOsdWebcam = l
}
