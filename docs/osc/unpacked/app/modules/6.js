// ─────────────────────────────────────────────────────────────
// APP MODULE 6
// role       : directive nvOscTile
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
  }), t.nvOscTile = void 0;
  var o = n(2),
    r = n(314),
    a = i(r),
    l = o.ngMainCommonModule.directive("nvOscTile", [function() {
      return {
        scope: {
          title: "@",
          iconPath: "@",
          status: "@",
          statusBrush: "@",
          iconFont: "@"
        },
        replace: !1,
        restrict: "E",
        template: a.default
      }
    }]);
  t.nvOscTile = l
}
