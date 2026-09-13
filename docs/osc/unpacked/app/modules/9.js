// ─────────────────────────────────────────────────────────────
// APP MODULE 9
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports = function() {
    var e = [];
    return e.toString = function() {
      for (var e = [], t = 0; t < this.length; t++) {
        var n = this[t];
        n[2] ? e.push("@media " + n[2] + "{" + n[1] + "}") : e.push(n[1])
      }
      return e.join("")
    }, e.i = function(t, n) {
      "string" == typeof t && (t = [
        [null, t, ""]
      ]);
      for (var i = {}, o = 0; o < this.length; o++) {
        var r = this[o][0];
        "number" == typeof r && (i[r] = !0)
      }
      for (o = 0; o < t.length; o++) {
        var a = t[o];
        "number" == typeof a[0] && i[a[0]] || (n && !a[2] ? a[2] = n : n && (a[2] = "(" + a[2] + ") and (" + n +
          ")"), e.push(a))
      }
    }, e
  }
}
