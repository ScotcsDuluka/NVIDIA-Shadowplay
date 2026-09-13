// ─────────────────────────────────────────────────────────────
// APP MODULE 237
// role       : directive nvPreferencesBroadcast
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvPreferencesBroadcast = void 0;
  var o = n(1),
    r = n(353),
    a = i(r);
  n(236), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesBroadcast", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesBroadcastController",
      controllerAs: "preferencesBroadcast"
    }
  });
  t.nvPreferencesBroadcast = l
}
