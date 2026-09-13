// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 159
// directive nvErrorDialog
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
  }), exports.nvErrorDialog = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(317),
    a = i(r);
  require(158) /* app/158 — ErrorDialogController (controller) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvErrorDialog", function() {
    return {
      restrict: "E",
      scope: {
        errorDialougeParam: "=errorDialougeParam"
      },
      template: a.default,
      controller: "ErrorDialogController",
      controllerAs: "errorDialog"
    };
  });
  exports.nvErrorDialog = l;
}
