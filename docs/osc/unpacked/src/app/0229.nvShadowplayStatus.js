// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 229
// directive nvShadowplayStatus
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
  }), exports.nvShadowplayStatus = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(349),
    a = i(r);
  require(228) /* app/228 — OsdShadowplayStatusController (controller) */;
  var l = o.ngMainModule.directive("nvShadowplayStatus", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdShadowplayStatusController",
      controllerAs: "cont"
    };
  });
  exports.nvShadowplayStatus = l;
}
