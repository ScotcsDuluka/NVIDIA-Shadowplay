// ─────────────────────────────────────────────────────────────
// APP MODULE 225
// role       : directive nvOsd
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
  }), t.nvOsd = void 0;
  var o = n(1),
    r = n(347),
    a = i(r);
  n(224), n(231), n(229), n(233), n(223), n(227);
  var l = o.ngMainModule.directive("nvOsd", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "osdController",
      controllerAs: "osd"
    }
  });
  t.nvOsd = l
}
