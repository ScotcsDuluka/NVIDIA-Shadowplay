// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 124
// factory _ | defines angular.module("underscore")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  var n = angular.module("underscore", []);
  n.factory("_", ["$window", function(e) {
    return e._;
  }]);
}
