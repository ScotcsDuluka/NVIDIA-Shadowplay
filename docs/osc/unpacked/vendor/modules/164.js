// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 164
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  (function(t) {
    function n(e) {
      for (var t = 0; t < e.length; t++) {
        var n = e[t];
        if (n.buffer instanceof ArrayBuffer) {
          var r = n.buffer;
          if (n.byteLength !== r.byteLength) {
            var i = new Uint8Array(n.byteLength);
            i.set(new Uint8Array(r, n.byteOffset, n.byteLength)), r = i.buffer
          }
          e[t] = r
        }
      }
    }

    function r(e, t) {
      t = t || {};
      var r = new o;
      n(e);
      for (var i = 0; i < e.length; i++) r.append(e[i]);
      return t.type ? r.getBlob(t.type) : r.getBlob()
    }

    function i(e, t) {
      return n(e), new Blob(e, t || {})
    }
    var o = t.BlobBuilder || t.WebKitBlobBuilder || t.MSBlobBuilder || t.MozBlobBuilder,
      a = function() {
        try {
          var e = new Blob(["hi"]);
          return 2 === e.size
        } catch (e) {
          return !1
        }
      }(),
      s = a && function() {
        try {
          var e = new Blob([new Uint8Array([1, 2])]);
          return 2 === e.size
        } catch (e) {
          return !1
        }
      }(),
      c = o && o.prototype.append && o.prototype.getBlob;
    e.exports = function() {
      return a ? s ? t.Blob : i : c ? r : void 0
    }()
  }).call(t, function() {
    return this
  }())
}
