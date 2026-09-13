// ─────────────────────────────────────────────────────────────
// APP MODULE 223
// role       : directive nvOsdComments
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
  }), t.nvOsdComments = void 0;
  var o = n(1),
    r = n(346),
    a = i(r);
  n(222);
  var l = o.ngMainModule.directive("nvOsdComments", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "OsdCommentsController",
      controllerAs: "cont"
    }
  });
  t.nvOsdComments = l
}
