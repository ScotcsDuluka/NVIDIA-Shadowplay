// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 215
// directive nvFilterSlider
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
  }), exports.filterSlider = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(342),
    a = i(r);
  require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("nvFilterSlider", function() {
    return {
      restrict: "E",
      scope: {
        aControl: "=filterSlider"
      },
      template: a.default
    };
  });
  exports.filterSlider = l;
}
