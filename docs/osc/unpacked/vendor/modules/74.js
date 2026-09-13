// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 74
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";

  function n() {
    return "undefined" != typeof indexedDB ? indexedDB : "undefined" != typeof webkitIndexedDB ? webkitIndexedDB :
      "undefined" != typeof mozIndexedDB ? mozIndexedDB : "undefined" != typeof OIndexedDB ? OIndexedDB : "undefined" !=
      typeof msIndexedDB ? msIndexedDB : void 0
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var r = n();
  t.default = r
}
