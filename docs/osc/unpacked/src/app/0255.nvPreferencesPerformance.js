// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 255
// directive nvPreferencesPerformance
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
  }), exports.nvPreferencePerformance = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(362),
    a = i(r);
  require(254) /* app/254 — PreferencesPerformanceController (controller) */;
  var l = o.ngMainModule.directive("nvPreferencesPerformance", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesPerformanceController",
      controllerAs: "controller"
    };
  });
  exports.nvPreferencePerformance = l;
}
