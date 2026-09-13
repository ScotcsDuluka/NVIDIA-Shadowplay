// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 178
// controller EdgeDevKitController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  require(1) /* app/1 — main (module) */;
  require(89) /* app/89 — edgeDevKitService (service) */, angular.module("main").controller("EdgeDevKitController", ["$scope", "$log",
    "edgeDevKitService",
    function(e, t, n) {
      var i = this,
        o = t.getInstance("main.edge/EdgeDevKitController");
      i.initialize = function() {
        n.enableUI(!0);
      }, i.destroy = function() {
        n.enableUI(!1);
      }, e.$on("$destroy", function() {
        o.info("onDestroy"), i.destroy();
      }), i.initialize();
    }
  ]);
}
