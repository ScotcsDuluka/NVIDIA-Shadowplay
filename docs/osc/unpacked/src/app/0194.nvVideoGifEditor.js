// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 194
// directive nvVideoGifEditor
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
  }), exports.nvVideoGifEditor = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(335),
    a = i(r);
  require(193) /* app/193 — VideoEditorGifController (controller) */, require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("nvVideoGifEditor", function() {
    return {
      restrict: "E",
      scope: {
        nvSrc: "@",
        nvFileSize: "=",
        nvHighlight: "@",
        nvOutputType: "@",
        nvGifMaxDuration: "@",
        nvAddMeme: "=",
        nvSaveMeme: "=",
        nvCancelMeme: "=",
        nvUploadGifService: "=",
        onContentLoaded: "&"
      },
      template: a.default,
      controller: "VideoEditorGifController",
      controllerAs: "videoEditor"
    };
  });
  exports.nvVideoGifEditor = l;
}
