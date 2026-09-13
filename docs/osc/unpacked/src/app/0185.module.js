// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 185
// directive nvGalleryHistoryMenu
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
  }), exports.nvGalleryHistoryMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(331),
    a = i(r);
  require(184) /* app/184 — nvGalleryHistoryMenu (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(98) /* app/98 — nvGalleryFilterMenu (directive) */;
  var l = o.ngMainModule.directive("nvGalleryHistoryMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "nvGalleryHistoryMenu",
      controllerAs: "historyMenu"
    };
  });
  exports.nvGalleryHistoryMenu = l;
}
