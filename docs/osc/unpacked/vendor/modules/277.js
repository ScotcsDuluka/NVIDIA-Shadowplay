// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 277
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  var n =
    /^(?:(?![^:@]+:[^:@\/]*@)(http|https|ws|wss):\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?((?:[a-f0-9]{0,4}:){2,7}[a-f0-9]{0,4}|[^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/,
    r = ["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path",
      "directory", "file", "query", "anchor"
    ];
  e.exports = function(e) {
    var t = e,
      i = e.indexOf("["),
      o = e.indexOf("]");
    i != -1 && o != -1 && (e = e.substring(0, i) + e.substring(i, o).replace(/:/g, ";") + e.substring(o, e.length));
    for (var a = n.exec(e || ""), s = {}, c = 14; c--;) s[r[c]] = a[c] || "";
    return i != -1 && o != -1 && (s.source = t, s.host = s.host.substring(1, s.host.length - 1).replace(/;/g, ":"), s
      .authority = s.authority.replace("[", "").replace("]", "").replace(/;/g, ":"), s.ipv6uri = !0), s
  }
}
