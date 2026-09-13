// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 148
// directive nvOverlay
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvOverlay = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.directive("nvOverlay", [function() {
      function e(e, t) {
        function n() {
          t.empty(), t.remove(), t = null;
        }
        if (e.nvPosition) {
          var i = {},
            o = e.nvPosition.split(" ");
          angular.forEach(o, function(e) {
            switch (e) {
              case "bottom":
                i.top = "auto", i.bottom = 0;
                break;
              case "right":
                i.left = "auto";
            }
          }), t.css(i);
        }
        e.$on("$destroy", n), t.on("$destroy", function() {
          e.$destroy();
        });
      }
      return {
        restrict: "E",
        transclude: !0,
        scope: {
          nvPosition: "@"
        },
        template: "<ng-transclude flex layout layout-fill></ng-transclude>",
        link: e
      };
    }]);
  exports.nvOverlay = o;
}
