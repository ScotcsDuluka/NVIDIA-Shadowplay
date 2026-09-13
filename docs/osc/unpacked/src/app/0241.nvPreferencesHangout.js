// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 241
// directive nvPreferencesHangout
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
  }), exports.nvPreferencesHangout = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(355),
    a = i(r);
  require(240) /* app/240 — PreferencesHangoutController (controller) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesHangout", [function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesHangoutController",
      controllerAs: "preferencesHangout"
    };
  }]);
  exports.nvPreferencesHangout = l;
}
