// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 191
// directive nvGalleryUploadMenu
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
  }), exports.nvGalleryUploadMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(334),
    a = i(r);
  require(190) /* app/190 — GalleryUploadMenuController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(195) /* app/195 — nvVideoEditor (directive) */, require(194) /* app/194 — nvVideoGifEditor (directive) */, require(187) /* app/187 — nvImageEditor (directive) */, require(97) /* app/97 — destinationPicker (directive) */;
  var l = o.ngMainModule.directive("nvGalleryUploadMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "GalleryUploadMenuController",
      controllerAs: "uploadMenu"
    };
  });
  exports.nvGalleryUploadMenu = l;
}
