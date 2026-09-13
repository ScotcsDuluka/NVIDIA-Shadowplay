// ─────────────────────────────────────────────────────────────
// APP MODULE 154
// role       : controller BaseController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.BaseController = void 0;
  var i = n(1),
    o = i.ngMainModule.controller("BaseController", ["$document", "eventAggregator", "NOTIFIER_SELECTIONS", function(e,
      t, n) {
      function i(e) {
        o.hdrActive = e
      }
      var o = this;
      o.hdrActive = !1, e.on("keydown", function(e) {
        if (8 === e.which) {
          var t = e.target.nodeName.toLowerCase();
          ("input" !== t || "text" !== e.target.type && "password" !== e.target.type && "search" !== e.target
            .type) && "textarea" !== t && e.preventDefault()
        }
      }), t.on(n.HDR_ENABLED, i)
    }]);
  t.BaseController = o
}
