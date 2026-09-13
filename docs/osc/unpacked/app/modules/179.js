// ─────────────────────────────────────────────────────────────
// APP MODULE 179
// role       : directive edgeDevKit
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
  }), t.edgeDevKit = void 0;
  var o = n(1),
    r = n(327),
    a = i(r);
  n(178);
  var l = o.ngMainModule.directive("edgeDevKit", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "EdgeDevKitController",
      controllerAs: "cont"
    }
  });
  t.edgeDevKit = l
}
