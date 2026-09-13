// ─────────────────────────────────────────────────────────────
// APP MODULE 189
// role       : directive nvGalleryRemoveMenu
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
  }), t.nvGalleryRemoveMenu = void 0;
  var o = n(1),
    r = n(333),
    a = i(r);
  n(188), n(5), n(6);
  var l = o.ngMainModule.directive("nvGalleryRemoveMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "GalleryRemoveMenuController",
      controllerAs: "removeMenu"
    }
  });
  t.nvGalleryRemoveMenu = l
}
