// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 1
// defines angular.module("main")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.ngMainModule = void 0;
  var i = require(209) /* app/209 — main.constants (module) */,
    o = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    r = angular.module("main", [i.ngMainConstantsModule.name, o.ngMainCommonModule.name, "ngMaterial",
      "ngResource", "pascalprecht.translate", "ui.router", "ngAnimate"
    ]);
  exports.ngMainModule = r;
}
