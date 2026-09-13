// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 144
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.exceptionConfig = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.config(["$provide", function(e) {
      e.decorator("$exceptionHandler", ["$log", "$delegate", "$injector", function(e, t, n) {
        return function(e, i) {
          var o = n.get("exceptionService");
          o.logException(e, i), t(e, i);
        };
      }]);
    }]);
  exports.exceptionConfig = o;
}
