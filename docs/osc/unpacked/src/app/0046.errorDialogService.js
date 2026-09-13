// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 46
// service errorDialogService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.errorDialogService = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(12) /* app/12 — oscDisplayService (service) */;
  var o = i.ngMainCommonModule.service("errorDialogService", ["$state", "oscDisplayService", function(e, t) {
    this.show = function(n, i, o, r) {
      var a = {
        error: n,
        details: i,
        lastState: e.current.name,
        lastParams: e.params,
        arg1: o,
        arg2: r
      };
      t.openOSC("main.error-dialog", a);
    };
  }]);
  exports.errorDialogService = o;
}
