// ─────────────────────────────────────────────────────────────
// APP MODULE 182
// role       : directive nvGalleryFilesMenu
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
  }), t.nvGalleryFilesMenu = void 0;
  var o = n(1),
    r = n(329),
    a = i(r);
  n(181), n(5), n(6), n(151), n(98);
  var l = o.ngMainModule.directive("nvGalleryFilesMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "GalleryFilesMenuController",
      controllerAs: "filesMenu"
    }
  });
  t.nvGalleryFilesMenu = l
}
