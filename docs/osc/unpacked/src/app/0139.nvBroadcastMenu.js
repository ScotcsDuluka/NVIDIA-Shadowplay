// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 139
// directive nvBroadcastMenu
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
  }), exports.nvBroadcastMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(313),
    a = i(r);
  require(138) /* app/138 — BroadcastMenuController (controller) */, require(6) /* app/6 — nvOscTile (directive) */, require(97) /* app/97 — destinationPicker (directive) */;
  var l = o.ngMainModule.directive("nvBroadcastMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "BroadcastMenuController",
      controllerAs: "broadcastMenu"
    };
  });
  exports.nvBroadcastMenu = l;
}
