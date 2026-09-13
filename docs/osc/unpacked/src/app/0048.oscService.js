// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 48
// provider oscService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.oscService = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.provider("oscService", [function() {
      return {
        $get: ["$log", "$window", function(e, t) {
          var n = this;
          e.getInstance("osc/oscService");
          return n.onlineState = {
            online: !0
          }, n.setOnline = function(e) {
            n.onlineState.online = e;
          }, n;
        }]
      };
    }]);
  exports.oscService = o;
}
