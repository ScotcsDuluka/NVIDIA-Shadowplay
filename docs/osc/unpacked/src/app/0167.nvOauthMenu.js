// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 167
// directive nvOauthMenu
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
  }), exports.nvOauthMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(321),
    a = i(r);
  require(166) /* app/166 — NvOauthMenuController (controller) */, require(6) /* app/6 — nvOscTile (directive) */, require(95) /* app/95 — nvOauthDialogue (directive) */;
  var l = o.ngMainModule.directive("nvOauthMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "NvOauthMenuController",
      controllerAs: "nvOauthMenu"
    };
  });
  exports.nvOauthMenu = l;
}
