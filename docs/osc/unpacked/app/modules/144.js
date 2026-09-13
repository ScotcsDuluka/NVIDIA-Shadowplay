// ─────────────────────────────────────────────────────────────
// APP MODULE 144
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.exceptionConfig = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.config(["$provide", function(e) {
      e.decorator("$exceptionHandler", ["$log", "$delegate", "$injector", function(e, t, n) {
        return function(e, i) {
          var o = n.get("exceptionService");
          o.logException(e, i), t(e, i)
        }
      }])
    }]);
  t.exceptionConfig = o
}
