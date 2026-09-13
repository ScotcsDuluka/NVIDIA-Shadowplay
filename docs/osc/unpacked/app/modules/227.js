// ─────────────────────────────────────────────────────────────
// APP MODULE 227
// role       : directive nvOsdPerfStats
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
  }), t.nvOsdPerfStats = void 0;
  var o = n(1),
    r = n(348),
    a = i(r);
  n(226);
  var l = o.ngMainModule.directive("nvOsdPerfStats", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdPerfStatsController",
      controllerAs: "cont"
    }
  });
  t.nvOsdPerfStats = l
}
