// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 124
// role       : factory _
// defines    : angular.module("underscore")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  var n = angular.module("underscore", []);
  n.factory("_", ["$window", function(e) {
    return e._
  }])
}
