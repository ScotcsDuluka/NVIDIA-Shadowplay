// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 233
// directive nvOsdWebcam
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
  }), exports.nvOsdWebcam = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(351),
    a = i(r);
  require(232) /* app/232 — OsdWebcamController (controller) */;
  var l = o.ngMainModule.directive("nvOsdWebcam", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdWebcamController",
      controllerAs: "cont"
    };
  });
  exports.nvOsdWebcam = l;
}
