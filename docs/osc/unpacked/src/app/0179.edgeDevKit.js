// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 179
// directive edgeDevKit
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
  }), exports.edgeDevKit = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(327),
    a = i(r);
  require(178) /* app/178 — EdgeDevKitController (controller) */;
  var l = o.ngMainModule.directive("edgeDevKit", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "EdgeDevKitController",
      controllerAs: "cont"
    };
  });
  exports.edgeDevKit = l;
}
