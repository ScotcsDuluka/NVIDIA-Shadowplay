// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 101
// directive nvPreferencesRecordingsFolderBrowser
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvPreferencesRecordingsFolderBrowser = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(365),
    a = i(r);
  require(260) /* app/260 — FolderBrowserController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesRecordingsFolderBrowser", function() {
    return {
      restrict: "E",
      scope: {
        folderParams: "=folderParams"
      },
      template: a.default,
      controller: "FolderBrowserController",
      controllerAs: "controller"
    };
  });
  exports.nvPreferencesRecordingsFolderBrowser = l;
}
