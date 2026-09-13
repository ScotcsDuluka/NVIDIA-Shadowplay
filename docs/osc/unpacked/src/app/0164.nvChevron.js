// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 164
// directive nvChevron
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
  }), exports.nvChevron = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(320),
    a = i(r);
  require(163) /* app/163 — nvChevronController (controller) */, require(5) /* app/5 — hoverFocus (directive) */;
  var l = o.ngMainModule.directive("nvChevron", function() {
    return {
      restrict: "E",
      scope: {
        input: "=",
        onSelected: "&"
      },
      template: a.default,
      controller: "nvChevronController",
      controllerAs: "nvChevron"
    };
  });
  exports.nvChevron = l;
}
