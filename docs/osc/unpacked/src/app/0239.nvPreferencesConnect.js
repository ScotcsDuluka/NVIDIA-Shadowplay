// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 239
// directive nvPreferencesConnect
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
  }), exports.nvPreferencesConnect = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(354),
    a = i(r);
  require(238) /* app/238 — PreferencesConnectController (controller) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */, require(95) /* app/95 — nvOauthDialogue (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesConnect", [function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesConnectController",
      controllerAs: "preferencesConnect"
    };
  }]);
  exports.nvPreferencesConnect = l;
}
