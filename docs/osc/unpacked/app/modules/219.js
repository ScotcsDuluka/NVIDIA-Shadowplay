// ─────────────────────────────────────────────────────────────
// APP MODULE 219
// role       : directive nvCameraMenu
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
  }), t.nvCameraMenu = void 0;
  var o = n(1),
    r = n(344),
    a = i(r);
  n(218), n(100), n(141), n(14), n(96), n(101);
  var l = o.ngMainModule.directive("nvCameraMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "NvCameraMenuController",
      controllerAs: "nvCameraMenu"
    }
  });
  t.nvCameraMenu = l
}
