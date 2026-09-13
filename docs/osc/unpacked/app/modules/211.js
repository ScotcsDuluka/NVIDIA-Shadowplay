// ─────────────────────────────────────────────────────────────
// APP MODULE 211
// role       : directive nvMainMenu | directive mdMenu
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
  }), t.mdMenuOverride = t.nvMainMenu = void 0;
  var o = n(1),
    r = n(338),
    a = i(r);
  n(210), n(5), n(6), n(148);
  var l = o.ngMainModule.directive("nvMainMenu", function() {
      return {
        restrict: "E",
        template: a.default,
        controller: "MainMenuController",
        controllerAs: "mainMenu"
      }
    }),
    s = o.ngMainModule.directive("mdMenu", function() {
      return {
        require: "^mdMenu",
        link: function(e, t, n, i) {
          e.$mdCloseMenu = i.close
        }
      }
    });
  t.nvMainMenu = l, t.mdMenuOverride = s
}
