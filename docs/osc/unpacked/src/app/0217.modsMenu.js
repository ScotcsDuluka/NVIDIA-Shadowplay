// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 217
// directive modsMenu
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
  }), exports.modsMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(343),
    a = i(r);
  require(216) /* app/216 — ModsMenuController (controller) */, require(100) /* app/100 — nvFilterControl (directive) */;
  var l = o.ngMainModule.directive("modsMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "ModsMenuController",
      controllerAs: "cont"
    };
  });
  exports.modsMenu = l;
}
