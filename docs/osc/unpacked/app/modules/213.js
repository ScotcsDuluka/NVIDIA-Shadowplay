// ─────────────────────────────────────────────────────────────
// APP MODULE 213
// role       : directive nvMicrophoneMenu
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
  }), t.nvMicrophoneMenu = void 0;
  var o = n(1),
    r = n(339),
    a = i(r);
  n(212), n(5), n(14), n(6);
  var l = o.ngMainModule.directive("nvMicrophoneMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "MicrophoneMenuController",
      controllerAs: "microphoneMenu"
    }
  });
  t.nvMicrophoneMenu = l
}
