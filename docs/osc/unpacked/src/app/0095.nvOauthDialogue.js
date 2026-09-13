// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 95
// directive nvOauthDialogue
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvOauthDialogue = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(165) /* app/165 — NvOauthDialogueController (controller) */;
  var o = i.ngMainModule.directive("nvOauthDialogue", function() {
    return {
      restrict: "E",
      controller: "NvOauthDialogueController",
      scope: {
        dialogueParams: "=",
        dialogueClosedCallback: "&?"
      }
    };
  });
  exports.nvOauthDialogue = o;
}
