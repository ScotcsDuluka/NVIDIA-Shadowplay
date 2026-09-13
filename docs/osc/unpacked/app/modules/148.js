// ─────────────────────────────────────────────────────────────
// APP MODULE 148
// role       : directive nvOverlay
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvOverlay = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.directive("nvOverlay", [function() {
      function e(e, t) {
        function n() {
          t.empty(), t.remove(), t = null
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
                i.left = "auto"
            }
          }), t.css(i)
        }
        e.$on("$destroy", n), t.on("$destroy", function() {
          e.$destroy()
        })
      }
      return {
        restrict: "E",
        transclude: !0,
        scope: {
          nvPosition: "@"
        },
        template: "<ng-transclude flex layout layout-fill></ng-transclude>",
        link: e
      }
    }]);
  t.nvOverlay = o
}
