// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 211
// directive nvMainMenu | directive mdMenu
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
  }), exports.mdMenuOverride = exports.nvMainMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(338),
    a = i(r);
  require(210) /* app/210 — MainMenuController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(148) /* app/148 — nvOverlay (directive) */;
  var l = o.ngMainModule.directive("nvMainMenu", function() {
      return {
        restrict: "E",
        template: a.default,
        controller: "MainMenuController",
        controllerAs: "mainMenu"
      };
    }),
    s = o.ngMainModule.directive("mdMenu", function() {
      return {
        require: "^mdMenu",
        link: function(e, t, n, i) {
          e.$mdCloseMenu = i.close;
        }
      };
    });
  exports.nvMainMenu = l, exports.mdMenuOverride = s;
}
