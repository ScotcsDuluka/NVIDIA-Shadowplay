// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 186
// controller ImageEditorController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.ImageEditorController = void 0;
  var i = require(1) /* app/1 — main (module) */,
    o = i.ngMainModule.controller("ImageEditorController", ["$scope", "$sce", "$log", function(e, t, n) {
      var i = this;
      i.nvSrc = e.nvSrc;
      var o = n.getInstance("osc/ImageEditorController");
      i.imageSrc = t.trustAsResourceUrl(i.nvSrc), i.onLoad = function() {
        e.onContentLoaded();
      }, e.$watch("nvSrc", function() {
        o.info("ImageEditor Directive: nvSrc:", i.nvSrc), i.nvSrc !== e.nvSrc && (i.nvSrc = e.nvSrc, i
          .imageSrc = t.trustAsResourceUrl(i.nvSrc));
      }, !0);
    }]);
  exports.ImageEditorController = o;
}
