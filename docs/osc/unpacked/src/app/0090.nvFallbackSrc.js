// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 90
// directive nvFallbackSrc
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvFallbackSrc = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.directive("nvFallbackSrc", function() {
      var e = {
        restrict: "A",
        scope: {
          nvFallbackSrc: "@",
          nvOnImageLoaded: "&",
          nvHideOnError: "@"
        },
        link: function(e, t, n) {
          function i() {
            l && angular.element(this).attr("style", "display:none"), void 0 !== a && a !== angular
              .element(this).attr("src") && angular.element(this).attr("src", a), o(!1);
          }

          function o() {
            var t = !(arguments.length > 0 && void 0 !== arguments[0]) || arguments[0];
            t && l && void 0 !== angular.element(this).attr("style") && angular.element(this)
              .removeAttr("style"), _.isUndefined(e.nvOnImageLoaded) || e.nvOnImageLoaded({
                success: t,
                url: n.nvFallbackSrc
              });
          }

          function r() {
            t.off("load", o), t.off("error", i);
          }
          t.on("load", o), t.on("error", i);
          var a = n.nvFallbackSrc,
            l = n.nvHideOnError || !1;
          e.$on("$destroy", r);
        }
      };
      return e;
    });
  exports.nvFallbackSrc = o;
}
