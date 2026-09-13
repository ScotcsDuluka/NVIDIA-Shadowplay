// ─────────────────────────────────────────────────────────────
// APP MODULE 194
// role       : directive nvVideoGifEditor
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
  }), t.nvVideoGifEditor = void 0;
  var o = n(1),
    r = n(335),
    a = i(r);
  n(193), n(14);
  var l = o.ngMainModule.directive("nvVideoGifEditor", function() {
    return {
      restrict: "E",
      scope: {
        nvSrc: "@",
        nvFileSize: "=",
        nvHighlight: "@",
        nvOutputType: "@",
        nvGifMaxDuration: "@",
        nvAddMeme: "=",
        nvSaveMeme: "=",
        nvCancelMeme: "=",
        nvUploadGifService: "=",
        onContentLoaded: "&"
      },
      template: a.default,
      controller: "VideoEditorGifController",
      controllerAs: "videoEditor"
    }
  });
  t.nvVideoGifEditor = l
}
