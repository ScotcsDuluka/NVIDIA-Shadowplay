// ─────────────────────────────────────────────────────────────
// APP MODULE 46
// role       : service errorDialogService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.errorDialogService = void 0;
  var i = n(2);
  n(12);
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
      t.openOSC("main.error-dialog", a)
    }
  }]);
  t.errorDialogService = o
}
