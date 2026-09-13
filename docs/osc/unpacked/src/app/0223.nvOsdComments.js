// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 223
// directive nvOsdComments
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
  }), exports.nvOsdComments = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(346),
    a = i(r);
  require(222) /* app/222 — OsdCommentsController (controller) */;
  var l = o.ngMainModule.directive("nvOsdComments", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdCommentsController",
      controllerAs: "cont"
    };
  });
  exports.nvOsdComments = l;
}
