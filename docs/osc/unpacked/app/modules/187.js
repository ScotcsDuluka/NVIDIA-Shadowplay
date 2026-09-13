// ─────────────────────────────────────────────────────────────
// APP MODULE 187
// role       : directive nvImageEditor | directive imageonload
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
  }), t.nvImageEditor = void 0;
  var o = n(1),
    r = n(332),
    a = i(r);
  n(186);
  var l = o.ngMainModule.directive("nvImageEditor", function() {
    return {
      restrict: "E",
      scope: {
        nvSrc: "@",
        onContentLoaded: "&"
      },
      template: a.default,
      controller: "ImageEditorController",
      controllerAs: "imageEditor"
    }
  });
  o.ngMainModule.directive("imageonload", function() {
    return {
      restrict: "A",
      link: function(e, t, n) {
        t.bind("load", function() {
          e && e.imageEditor && e.imageEditor.onLoad && e.imageEditor.onLoad()
        })
      }
    }
  });
  t.nvImageEditor = l
}
