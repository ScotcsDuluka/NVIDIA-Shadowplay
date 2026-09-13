// ─────────────────────────────────────────────────────────────
// APP MODULE 191
// role       : directive nvGalleryUploadMenu
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
  }), t.nvGalleryUploadMenu = void 0;
  var o = n(1),
    r = n(334),
    a = i(r);
  n(190), n(5), n(6), n(195), n(194), n(187), n(97);
  var l = o.ngMainModule.directive("nvGalleryUploadMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "GalleryUploadMenuController",
      controllerAs: "uploadMenu"
    }
  });
  t.nvGalleryUploadMenu = l
}
