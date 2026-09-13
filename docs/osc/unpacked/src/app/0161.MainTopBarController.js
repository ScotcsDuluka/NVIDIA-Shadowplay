// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 161
// controller MainTopBarController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.MainTopBarController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(12) /* app/12 — oscDisplayService (service) */;
  var o = i.ngMainModule.controller("MainTopBarController", ["oscDisplayService", function(e) {
    var t = this;
    t.closeButtonClick = function() {
      e.closeOSC();
    };
  }]);
  exports.MainTopBarController = o;
}
