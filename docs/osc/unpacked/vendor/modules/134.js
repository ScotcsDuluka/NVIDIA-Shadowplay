// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 134
// role       : factory eventAggregator
// defines    : angular.module("ngEventAggregator")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  ! function(e, t, n) {
    "use strict";
    t.module("ngEventAggregator", []).factory("eventAggregator", [function() {
      function e(e, t) {
        var n = i[e] || (i[e] = []);
        n.push(t)
      }

      function n(e, n) {
        var r = i[e];
        if (!t.isUndefined(r)) {
          for (var o = r.length - 1; o >= 0; --o) r[o] === n && r.splice(o, 1);
          0 === r.length && delete i[e]
        }
      }

      function r(e, n) {
        var r = i[e];
        if (t.isDefined(r))
          for (var o = 0; o < r.length; o++) r[o](n)
      }
      var i = {};
      return {
        on: e,
        off: n,
        trigger: r
      }
    }])
  }(window, angular)
}
