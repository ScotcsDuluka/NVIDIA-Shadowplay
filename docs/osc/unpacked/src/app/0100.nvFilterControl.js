// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 100
// directive nvFilterControl
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
  }), exports.filterControl = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(340),
    a = i(r);
  require(214) /* app/214 — nvFilterEditbox (directive) */, require(215) /* app/215 — nvFilterSlider (directive) */, require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("nvFilterControl", function() {
    return {
      restrict: "E",
      scope: {
        aControl: "=filterControl"
      },
      template: a.default
    };
  });
  exports.filterControl = l;
}
