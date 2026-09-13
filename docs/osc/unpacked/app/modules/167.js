// ─────────────────────────────────────────────────────────────
// APP MODULE 167
// role       : directive nvOauthMenu
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
  }), t.nvOauthMenu = void 0;
  var o = n(1),
    r = n(321),
    a = i(r);
  n(166), n(6), n(95);
  var l = o.ngMainModule.directive("nvOauthMenu", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "NvOauthMenuController",
      controllerAs: "nvOauthMenu"
    }
  });
  t.nvOauthMenu = l
}
