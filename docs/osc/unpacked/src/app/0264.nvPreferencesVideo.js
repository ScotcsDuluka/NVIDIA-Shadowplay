// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 264
// directive nvPreferencesVideo
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
  }), exports.nvPreferencesVideo = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(367),
    a = i(r);
  require(263) /* app/263 — PreferencesVideoController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(177) /* app/177 — settingsCustomize (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesVideo", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesVideoController",
      controllerAs: "controller"
    };
  });
  exports.nvPreferencesVideo = l;
}
