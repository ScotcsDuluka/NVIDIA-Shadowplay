// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 177
// directive settingsCustomize
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
  }), exports.settingsCustomize = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(326),
    a = i(r);
  require(176) /* app/176 — SettingsCustomizeController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("settingsCustomize", function() {
    return {
      restrict: "E",
      scope: {
        nvChangeSettings: "=",
        onCustomizeCloseComplete: "&"
      },
      template: a.default,
      controller: "SettingsCustomizeController",
      controllerAs: "customizeMenu"
    };
  });
  exports.settingsCustomize = l;
}
