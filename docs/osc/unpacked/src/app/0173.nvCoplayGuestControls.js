// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 173
// directive nvCoplayGuestControls
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
  }), exports.nvCoplayGuestControls = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(324),
    a = i(r);
  require(172) /* app/172 — CoPlayGuestControlsController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvCoplayGuestControls", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "CoPlayGuestControlsController",
      controllerAs: "coplayGuestControls"
    };
  });
  exports.nvCoplayGuestControls = l;
}
