// ─────────────────────────────────────────────────────────────
// APP MODULE 186
// role       : controller ImageEditorController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.ImageEditorController = void 0;
  var i = n(1),
    o = i.ngMainModule.controller("ImageEditorController", ["$scope", "$sce", "$log", function(e, t, n) {
      var i = this;
      i.nvSrc = e.nvSrc;
      var o = n.getInstance("osc/ImageEditorController");
      i.imageSrc = t.trustAsResourceUrl(i.nvSrc), i.onLoad = function() {
        e.onContentLoaded()
      }, e.$watch("nvSrc", function() {
        o.info("ImageEditor Directive: nvSrc:", i.nvSrc), i.nvSrc !== e.nvSrc && (i.nvSrc = e.nvSrc, i
          .imageSrc = t.trustAsResourceUrl(i.nvSrc))
      }, !0)
    }]);
  t.ImageEditorController = o
}
