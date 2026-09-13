// ─────────────────────────────────────────────────────────────
// APP MODULE 48
// role       : provider oscService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.oscService = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.provider("oscService", [function() {
      return {
        $get: ["$log", "$window", function(e, t) {
          var n = this;
          e.getInstance("osc/oscService");
          return n.onlineState = {
            online: !0
          }, n.setOnline = function(e) {
            n.onlineState.online = e
          }, n
        }]
      }
    }]);
  t.oscService = o
}
