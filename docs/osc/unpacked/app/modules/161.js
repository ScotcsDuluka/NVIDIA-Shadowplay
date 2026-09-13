// ─────────────────────────────────────────────────────────────
// APP MODULE 161
// role       : controller MainTopBarController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.MainTopBarController = void 0;
  var i = n(1);
  n(12);
  var o = i.ngMainModule.controller("MainTopBarController", ["oscDisplayService", function(e) {
    var t = this;
    t.closeButtonClick = function() {
      e.closeOSC()
    }
  }]);
  t.MainTopBarController = o
}
