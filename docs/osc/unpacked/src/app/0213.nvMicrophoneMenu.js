// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 213
// directive nvMicrophoneMenu
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
  }), exports.nvMicrophoneMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(339),
    a = i(r);
  require(212) /* app/212 — MicrophoneMenuController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(14) /* app/14 — nvSlider (directive) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvMicrophoneMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "MicrophoneMenuController",
      controllerAs: "microphoneMenu"
    };
  });
  exports.nvMicrophoneMenu = l;
}
