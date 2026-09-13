// ─────────────────────────────────────────────────────────────
// APP MODULE 229
// role       : directive nvShadowplayStatus
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
  }), t.nvShadowplayStatus = void 0;
  var o = n(1),
    r = n(349),
    a = i(r);
  n(228);
  var l = o.ngMainModule.directive("nvShadowplayStatus", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdShadowplayStatusController",
      controllerAs: "cont"
    }
  });
  t.nvShadowplayStatus = l
}
