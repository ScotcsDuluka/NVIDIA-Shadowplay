// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 283
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(e) {
    var r = n(68),
      i = n(112);
    t.deconstructPacket = function(e) {
      function t(e) {
        if (!e) return e;
        if (i(e)) {
          var o = {
            _placeholder: !0,
            num: n.length
          };
          return n.push(e), o
        }
        if (r(e)) {
          for (var a = new Array(e.length), s = 0; s < e.length; s++) a[s] = t(e[s]);
          return a
        }
        if ("object" == typeof e && !(e instanceof Date)) {
          var a = {};
          for (var c in e) a[c] = t(e[c]);
          return a
        }
        return e
      }
      var n = [],
        o = e.data,
        a = e;
      return a.data = t(o), a.attachments = n.length, {
        packet: a,
        buffers: n
      }
    }, t.reconstructPacket = function(e, t) {
      function n(e) {
        if (e && e._placeholder) {
          var i = t[e.num];
          return i
        }
        if (r(e)) {
          for (var o = 0; o < e.length; o++) e[o] = n(e[o]);
          return e
        }
        if (e && "object" == typeof e) {
          for (var a in e) e[a] = n(e[a]);
          return e
        }
        return e
      }
      return e.data = n(e.data), e.attachments = void 0, e
    }, t.removeBlobs = function(t, n) {
      function o(t, c, u) {
        if (!t) return t;
        if (e.Blob && t instanceof Blob || e.File && t instanceof File) {
          a++;
          var l = new FileReader;
          l.onload = function() {
            u ? u[c] = this.result : s = this.result, --a || n(s)
          }, l.readAsArrayBuffer(t)
        } else if (r(t))
          for (var d = 0; d < t.length; d++) o(t[d], d, t);
        else if (t && "object" == typeof t && !i(t))
          for (var f in t) o(t[f], f, t)
      }
      var a = 0,
        s = t;
      o(s), a || n(s)
    }
  }).call(t, function() {
    return this
  }())
}
