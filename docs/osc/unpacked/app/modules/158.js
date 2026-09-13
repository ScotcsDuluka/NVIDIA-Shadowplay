// ─────────────────────────────────────────────────────────────
// APP MODULE 158
// role       : controller ErrorDialogController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.ErrorDialogController = void 0;
  var i = n(1);
  n(12);
  var o = i.ngMainModule.controller("ErrorDialogController", ["$scope", "$state", "$stateParams", "$window", "$filter",
    "OSC_CONFIG", "oscDisplayService", "eventAggregator", "KEYBOARD_EVENTS",
    function(e, t, n, i, o, r, a, l, s) {
      var d = this;
      d.title = "l10n.error", d.icon = "icon-notify_warning", d.initialize = function(e) {
        e ? (d.error = e.error, d.details = o("translate")(e.details, {
          arg1: e.arg1,
          arg2: e.arg2
        }), d.isCustomError = e.isCustomError) : "undefined" !== n.error && (d.error = n.error, d.details = o(
          "translate")(n.details, {
          arg1: n.arg1,
          arg2: n.arg2
        }), d.isCustomError = !1)
      }, d.troubleshoot = function() {
        a.closeOSC(), i.open(r.redirect.server + "ENU&page=osc_tsguide")
      }, d.close = function() {
        d.isCustomError ? e.errorDialougeParam.isCustomError = !1 : "base" === n.lastState ||
          "main.error-dialog" === n.lastState ? a.closeOSC() : t.go(n.lastState, n.lastParams)
      }, l.on(s.ESCAPE, d.close), e.$on("$destroy", function() {
        l.off(s.ESCAPE, d.close)
      })
    }
  ]);
  t.ErrorDialogController = o
}
