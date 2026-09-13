// ─────────────────────────────────────────────────────────────
// APP MODULE 214
// role       : directive nvFilterEditbox
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
  }), t.filterEditbox = void 0;
  var o = n(1),
    r = n(341),
    a = i(r);
  n(14);
  var l = o.ngMainModule.directive("nvFilterEditbox", function() {
    return {
      restrict: "E",
      scope: {
        aControl: "=filterEditbox"
      },
      template: a.default,
      controller: function() {
        var e = this;
        e.onKeyDown = function(e) {
          if (e && e.key) {
            var t = !isNaN(e.key),
              n = t || "." === e.key || "Backspace" === e.key || "Delete" === e.key || e.key.startsWith(
              "Arrow");
            n || (e.preventDefault(), e.stopPropagation())
          }
        }
      },
      controllerAs: "ctrl"
    }
  });
  t.filterEditbox = l
}
