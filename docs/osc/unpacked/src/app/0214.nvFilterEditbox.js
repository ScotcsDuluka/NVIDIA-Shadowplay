// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 214
// directive nvFilterEditbox
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
  }), exports.filterEditbox = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(341),
    a = i(r);
  require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("nvFilterEditbox", function() {
    return {
      restrict: "E",
      scope: {
        aControl: "=filterEditbox"
      },
      template: a.default,
      controller: function() {
        var e = this;
        e.onKeyDown = function(e) {
          if (e && e.key) {
            var t = !isNaN(e.key),
              n = t || "." === e.key || "Backspace" === e.key || "Delete" === e.key || e.key
              .startsWith("Arrow");
            n || (e.preventDefault(), e.stopPropagation());
          }
        };
      },
      controllerAs: "ctrl"
    };
  });
  exports.filterEditbox = l;
}
