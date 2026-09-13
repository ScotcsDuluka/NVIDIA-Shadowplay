// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 96
// directive nvProgressIndicator
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }

  function o(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvProgressIndicator = void 0;
  var r = require(3),
    a = o(r),
    l = require(1) /* app/1 — main (module) */,
    s = require(323),
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
            t.empty(), t.remove(), t = null;
          }
          e.hideButton = a.isUndefined(e.nvButton), e.$on("$destroy", n), t.on("$destroy", function() {
            e.$destroy();
          });
        }
      };
    });
  exports.nvProgressIndicator = c;
}
