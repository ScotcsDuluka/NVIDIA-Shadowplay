// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 162
// directive nvOscTopBar
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
  }), exports.nvOscTopBar = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(319),
    a = i(r);
  require(161) /* app/161 — MainTopBarController (controller) */, require(5) /* app/5 — hoverFocus (directive) */;
  var l = o.ngMainModule.directive("nvOscTopBar", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "MainTopBarController",
      controllerAs: "mainTopBarCtrl"
    };
  });
  exports.nvOscTopBar = l;
}
