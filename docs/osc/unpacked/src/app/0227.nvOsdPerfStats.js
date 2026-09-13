// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 227
// directive nvOsdPerfStats
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
  }), exports.nvOsdPerfStats = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(348),
    a = i(r);
  require(226) /* app/226 — OsdPerfStatsController (controller) */;
  var l = o.ngMainModule.directive("nvOsdPerfStats", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdPerfStatsController",
      controllerAs: "cont"
    };
  });
  exports.nvOsdPerfStats = l;
}
