// ─────────────────────────────────────────────────────────────
// APP MODULE 95
// role       : directive nvOauthDialogue
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvOauthDialogue = void 0;
  var i = n(1);
  n(165);
  var o = i.ngMainModule.directive("nvOauthDialogue", function() {
    return {
      restrict: "E",
      controller: "NvOauthDialogueController",
      scope: {
        dialogueParams: "=",
        dialogueClosedCallback: "&?"
      }
    }
  });
  t.nvOauthDialogue = o
}
