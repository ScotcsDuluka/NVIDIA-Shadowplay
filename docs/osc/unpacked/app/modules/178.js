// ─────────────────────────────────────────────────────────────
// APP MODULE 178
// role       : controller EdgeDevKitController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  n(1);
  n(89), angular.module("main").controller("EdgeDevKitController", ["$scope", "$log", "edgeDevKitService", function(e,
    t, n) {
    var i = this,
      o = t.getInstance("main.edge/EdgeDevKitController");
    i.initialize = function() {
      n.enableUI(!0)
    }, i.destroy = function() {
      n.enableUI(!1)
    }, e.$on("$destroy", function() {
      o.info("onDestroy"), i.destroy()
    }), i.initialize()
  }])
}
