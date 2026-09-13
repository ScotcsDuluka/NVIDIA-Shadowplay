// ─────────────────────────────────────────────────────────────
// APP MODULE 1
// role       : utility
// defines    : angular.module("main")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.ngMainModule = void 0;
  var i = n(209),
    o = n(2),
    r = angular.module("main", [i.ngMainConstantsModule.name, o.ngMainCommonModule.name, "ngMaterial", "ngResource",
      "pascalprecht.translate", "ui.router", "ngAnimate"
    ]);
  t.ngMainModule = r
}
