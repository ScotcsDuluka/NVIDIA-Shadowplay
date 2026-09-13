// ─────────────────────────────────────────────────────────────
// APP MODULE 139
// role       : directive nvBroadcastMenu
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
  }), t.nvBroadcastMenu = void 0;
  var o = n(1),
    r = n(313),
    a = i(r);
  n(138), n(6), n(97);
  var l = o.ngMainModule.directive("nvBroadcastMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "BroadcastMenuController",
      controllerAs: "broadcastMenu"
    }
  });
  t.nvBroadcastMenu = l
}
