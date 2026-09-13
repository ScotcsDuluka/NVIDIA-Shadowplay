// ─────────────────────────────────────────────────────────────
// APP MODULE 195
// role       : directive nvVideoEditor
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
  }), t.nvVideoEditor = void 0;
  var o = n(1),
    r = n(336),
    a = i(r);
  n(192), n(14);
  var l = o.ngMainModule.directive("nvVideoEditor", function() {
    return {
      restrict: "E",
      scope: {
        nvSrc: "@",
        nvFileSize: "=",
        nvHighlight: "@",
        onContentLoaded: "&"
      },
      template: a.default,
      controller: "VideoEditorController",
      controllerAs: "videoEditor"
    }
  });
  t.nvVideoEditor = l
}
