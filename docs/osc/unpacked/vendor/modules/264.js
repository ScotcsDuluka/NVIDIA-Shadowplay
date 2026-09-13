// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 264
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  var n = Object.prototype.hasOwnProperty;
  t.keys = Object.keys || function(e) {
    var t = [];
    for (var r in e) n.call(e, r) && t.push(r);
    return t
  }, t.values = function(e) {
    var t = [];
    for (var r in e) n.call(e, r) && t.push(e[r]);
    return t
  }, t.merge = function(e, t) {
    for (var r in t) n.call(t, r) && (e[r] = t[r]);
    return e
  }, t.length = function(e) {
    return t.keys(e).length
  }, t.isEmpty = function(e) {
    return 0 == t.length(e)
  }
}
