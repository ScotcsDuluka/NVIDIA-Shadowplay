// ─────────────────────────────────────────────────────────────
// APP MODULE 185
// role       : directive nvGalleryHistoryMenu
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
  }), t.nvGalleryHistoryMenu = void 0;
  var o = n(1),
    r = n(331),
    a = i(r);
  n(184), n(5), n(6), n(98);
  var l = o.ngMainModule.directive("nvGalleryHistoryMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "nvGalleryHistoryMenu",
      controllerAs: "historyMenu"
    }
  });
  t.nvGalleryHistoryMenu = l
}
