// ─────────────────────────────────────────────────────────────
// APP MODULE 159
// role       : directive nvErrorDialog
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvErrorDialog = void 0;
  var o = n(1),
    r = n(317),
    a = i(r);
  n(158), n(6);
  var l = o.ngMainModule.directive("nvErrorDialog", function() {
    return {
      restrict: "E",
      scope: {
        errorDialougeParam: "=errorDialougeParam"
      },
      template: a.default,
      controller: "ErrorDialogController",
      controllerAs: "errorDialog"
    }
  });
  t.nvErrorDialog = l
}
