// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 225
// directive nvOsd
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
  }), exports.nvOsd = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(347),
    a = i(r);
  require(224) /* app/224 — osdController (controller) */, require(231) /* app/231 — nvOsdViewerCount (directive) */, require(229) /* app/229 — nvShadowplayStatus (directive) */, require(233) /* app/233 — nvOsdWebcam (directive) */, require(223) /* app/223 — nvOsdComments (directive) */, require(227) /* app/227 — nvOsdPerfStats (directive) */;
  var l = o.ngMainModule.directive("nvOsd", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "osdController",
      controllerAs: "osd"
    };
  });
  exports.nvOsd = l;
}
