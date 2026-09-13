// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 243
// directive nvPreferencesHighlights
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
  }), exports.nvPreferencesHighlights = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(356),
    a = i(r);
  require(242) /* app/242 — PreferencesHighlightsController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(14) /* app/14 — nvSlider (directive) */, require(96) /* app/96 — nvProgressIndicator (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesHighlights", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesHighlightsController",
      controllerAs: "controller"
    };
  });
  exports.nvPreferencesHighlights = l;
}
