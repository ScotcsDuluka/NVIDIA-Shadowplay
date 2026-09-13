// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 182
// directive nvGalleryFilesMenu
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvGalleryFilesMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(329),
    a = i(r);
  require(181) /* app/181 — GalleryFilesMenuController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(151) /* app/151 — nvVirtualGridList (directive) */, require(98) /* app/98 — nvGalleryFilterMenu (directive) */;
  var l = o.ngMainModule.directive("nvGalleryFilesMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "GalleryFilesMenuController",
      controllerAs: "filesMenu"
    };
  });
  exports.nvGalleryFilesMenu = l;
}
