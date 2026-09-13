// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 195
// directive nvVideoEditor
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
  }), exports.nvVideoEditor = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(336),
    a = i(r);
  require(192) /* app/192 — VideoEditorController (controller) */, require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("nvVideoEditor", function() {
    return {
      restrict: "E",
      scope: {
        nvSrc: "@",
        nvFileSize: "=",
        nvHighlight: "@",
        onContentLoaded: "&"
      },
      template: a.default,
      controller: "VideoEditorController",
      controllerAs: "videoEditor"
    };
  });
  exports.nvVideoEditor = l;
}
