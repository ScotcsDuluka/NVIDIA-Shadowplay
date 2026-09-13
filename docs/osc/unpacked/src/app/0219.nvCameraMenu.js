// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 219
// directive nvCameraMenu
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
  }), exports.nvCameraMenu = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(344),
    a = i(r);
  require(218) /* app/218 — NvCameraMenuController (controller) */, require(100) /* app/100 — nvFilterControl (directive) */, require(141) /* app/141 — nvAccordion (directive) */, require(14) /* app/14 — nvSlider (directive) */, require(96) /* app/96 — nvProgressIndicator (directive) */, require(101) /* app/101 — nvPreferencesRecordingsFolderBrowser (directive) */;
  var l = o.ngMainModule.directive("nvCameraMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "NvCameraMenuController",
      controllerAs: "nvCameraMenu"
    };
  });
  exports.nvCameraMenu = l;
}
