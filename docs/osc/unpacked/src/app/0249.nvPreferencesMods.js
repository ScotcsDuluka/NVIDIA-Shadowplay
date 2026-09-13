// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 249
// directive nvPreferencesMods
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
  }), exports.nvPreferencesMods = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(359),
    a = i(r);
  require(248) /* app/248 — PreferencesModsController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesMods", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesModsController",
      controllerAs: "controller"
    };
  });
  exports.nvPreferencesMods = l;
}
