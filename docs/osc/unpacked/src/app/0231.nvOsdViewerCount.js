// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 231
// directive nvOsdViewerCount
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
  }), exports.nvOsdViewerCount = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(350),
    a = i(r);
  require(230) /* app/230 — OsdViewerCountController (controller) */;
  var l = o.ngMainModule.directive("nvOsdViewerCount", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdViewerCountController",
      controllerAs: "cont"
    };
  });
  exports.nvOsdViewerCount = l;
}
