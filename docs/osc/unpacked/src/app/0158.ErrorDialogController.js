// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 158
// controller ErrorDialogController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.ErrorDialogController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(12) /* app/12 — oscDisplayService (service) */;
  var o = i.ngMainModule.controller("ErrorDialogController", ["$scope", "$state", "$stateParams", "$window",
    "$filter", "OSC_CONFIG", "oscDisplayService", "eventAggregator", "KEYBOARD_EVENTS",
    function(e, t, n, i, o, r, a, l, s) {
      var d = this;
      d.title = "l10n.error", d.icon = "icon-notify_warning", d.initialize = function(e) {
        e ? (d.error = e.error, d.details = o("translate")(e.details, {
          arg1: e.arg1,
          arg2: e.arg2
        }), d.isCustomError = e.isCustomError) : "undefined" !== n.error && (d.error = n.error, d
          .details = o("translate")(n.details, {
            arg1: n.arg1,
            arg2: n.arg2
          }), d.isCustomError = !1);
      }, d.troubleshoot = function() {
        a.closeOSC(), i.open(r.redirect.server + "ENU&page=osc_tsguide");
      }, d.close = function() {
        d.isCustomError ? e.errorDialougeParam.isCustomError = !1 : "base" === n.lastState ||
          "main.error-dialog" === n.lastState ? a.closeOSC() : t.go(n.lastState, n.lastParams);
      }, l.on(s.ESCAPE, d.close), e.$on("$destroy", function() {
        l.off(s.ESCAPE, d.close);
      });
    }
  ]);
  exports.ErrorDialogController = o;
}
