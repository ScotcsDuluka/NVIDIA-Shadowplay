// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 154
// controller BaseController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.BaseController = void 0;
  var i = require(1) /* app/1 — main (module) */,
    o = i.ngMainModule.controller("BaseController", ["$document", "eventAggregator", "NOTIFIER_SELECTIONS",
      function(e, t, n) {
        function i(e) {
          o.hdrActive = e;
        }
        var o = this;
        o.hdrActive = !1, e.on("keydown", function(e) {
          if (8 === e.which) {
            var t = e.target.nodeName.toLowerCase();
            ("input" !== t || "text" !== e.target.type && "password" !== e.target.type && "search" !== e
              .target.type) && "textarea" !== t && e.preventDefault();
          }
        }), t.on(n.HDR_ENABLED, i);
      }
    ]);
  exports.BaseController = o;
}
