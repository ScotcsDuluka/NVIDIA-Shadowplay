// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 6
// directive nvOscTile
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
  }), exports.nvOscTile = void 0;
  var o = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    r = require(314),
    a = i(r),
    l = o.ngMainCommonModule.directive("nvOscTile", [function() {
      return {
        scope: {
          title: "@",
          iconPath: "@",
          status: "@",
          statusBrush: "@",
          iconFont: "@"
        },
        replace: !1,
        restrict: "E",
        template: a.default
      };
    }]);
  exports.nvOscTile = l;
}
