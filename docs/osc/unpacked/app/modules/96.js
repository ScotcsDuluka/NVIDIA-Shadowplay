// ─────────────────────────────────────────────────────────────
// APP MODULE 96
// role       : directive nvProgressIndicator
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

  function o(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvProgressIndicator = void 0;
  var r = n(3),
    a = o(r),
    l = n(1),
    s = n(323),
    d = i(s),
    c = l.ngMainModule.directive("nvProgressIndicator", function() {
      return {
        transclude: !0,
        restrict: "E",
        scope: {
          nvMessage: "@",
          nvValue: "@",
          nvButton: "@",
          nvProgressCanceled: "=",
          onButtonClick: "&onClick"
        },
        template: d.default,
        link: function(e, t) {
          function n() {
            t.empty(), t.remove(), t = null
          }
          e.hideButton = a.isUndefined(e.nvButton), e.$on("$destroy", n), t.on("$destroy", function() {
            e.$destroy()
          })
        }
      }
    });
  t.nvProgressIndicator = c
}
