// ─────────────────────────────────────────────────────────────
// APP MODULE 415
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(395),
    o = n(125),
    r = "Map";
  e.exports = n(397)(r, function(e) {
    return function() {
      return e(this, arguments.length > 0 ? arguments[0] : void 0)
    }
  }, {
    get: function(e) {
      var t = i.getEntry(o(this, r), e);
      return t && t.v
    },
    set: function(e, t) {
      return i.def(o(this, r), 0 === e ? 0 : e, t)
    }
  }, i, !0)
}
