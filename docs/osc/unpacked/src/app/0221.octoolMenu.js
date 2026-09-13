// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 221
// directive octoolMenu
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
  }), exports.octoolMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(345),
    a = i(r);
  require(220) /* app/220 — OctoolMenuController (controller) */, require(14) /* app/14 — nvSlider (directive) */, require(5) /* app/5 — hoverFocus (directive) */, require(164) /* app/164 — nvChevron (directive) */;
  var l = o.ngMainModule.directive("octoolMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OctoolMenuController",
      controllerAs: "octool"
    };
  });
  exports.octoolMenu = l;
}
