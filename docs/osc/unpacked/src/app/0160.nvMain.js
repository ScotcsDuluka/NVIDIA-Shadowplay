// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 160
// directive nvMain
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
  }), exports.nvMain = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(318),
    a = i(r);
  require(162) /* app/162 — nvOscTopBar (directive) */;
  var l = o.ngMainModule.directive("nvMain", function() {
    return {
      restrict: "E",
      scope: {},
      template: a.default
    };
  });
  exports.nvMain = l;
}
