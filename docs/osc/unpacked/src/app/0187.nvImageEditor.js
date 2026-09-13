// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 187
// directive nvImageEditor | directive imageonload
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
  }), exports.nvImageEditor = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(332),
    a = i(r);
  require(186) /* app/186 — ImageEditorController (controller) */;
  var l = o.ngMainModule.directive("nvImageEditor", function() {
    return {
      restrict: "E",
      scope: {
        nvSrc: "@",
        onContentLoaded: "&"
      },
      template: a.default,
      controller: "ImageEditorController",
      controllerAs: "imageEditor"
    };
  });
  o.ngMainModule.directive("imageonload", function() {
    return {
      restrict: "A",
      link: function(e, t, n) {
        t.bind("load", function() {
          e && e.imageEditor && e.imageEditor.onLoad && e.imageEditor.onLoad();
        });
      }
    };
  });
  exports.nvImageEditor = l;
}
