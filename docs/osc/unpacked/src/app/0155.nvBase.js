// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 155
// directive nvBase
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
  }), exports.nvBase = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(315),
    a = i(r);
  require(154) /* app/154 — BaseController (controller) */, require(170) /* app/170 — nvOscNotifier (directive) */, require(171) /* app/171 — nvWebrtcChat (directive) */, require(225) /* app/225 — nvOsd (directive) */;
  var l = o.ngMainModule.directive("nvBase", function() {
    return {
      restrict: "E",
      scope: {},
      template: a.default,
      controller: "BaseController",
      controllerAs: "base"
    };
  });
  exports.nvBase = l;
}
