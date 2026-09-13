// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 286
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r, i, o;
  ! function(n, a) {
    "use strict";
    i = [], r = a, o = "function" == typeof r ? r.apply(t, i) : r, !(void 0 !== o && (e.exports = o))
  }(this, function() {
    "use strict";

    function e(e) {
      return !isNaN(parseFloat(e)) && isFinite(e)
    }

    function t(e, t, n, r, i, o) {
      void 0 !== e && this.setFunctionName(e), void 0 !== t && this.setArgs(t), void 0 !== n && this.setFileName(n),
        void 0 !== r && this.setLineNumber(r), void 0 !== i && this.setColumnNumber(i), void 0 !== o && this
        .setSource(o)
    }
    return t.prototype = {
      getFunctionName: function() {
        return this.functionName
      },
      setFunctionName: function(e) {
        this.functionName = String(e)
      },
      getArgs: function() {
        return this.args
      },
      setArgs: function(e) {
        if ("[object Array]" !== Object.prototype.toString.call(e)) throw new TypeError("Args must be an Array");
        this.args = e
      },
      getFileName: function() {
        return this.fileName
      },
      setFileName: function(e) {
        this.fileName = String(e)
      },
      getLineNumber: function() {
        return this.lineNumber
      },
      setLineNumber: function(t) {
        if (!e(t)) throw new TypeError("Line Number must be a Number");
        this.lineNumber = Number(t)
      },
      getColumnNumber: function() {
        return this.columnNumber
      },
      setColumnNumber: function(t) {
        if (!e(t)) throw new TypeError("Column Number must be a Number");
        this.columnNumber = Number(t)
      },
      getSource: function() {
        return this.source
      },
      setSource: function(e) {
        this.source = String(e)
      },
      toString: function() {
        var t = this.getFunctionName() || "{anonymous}",
          n = "(" + (this.getArgs() || []).join(",") + ")",
          r = this.getFileName() ? "@" + this.getFileName() : "",
          i = e(this.getLineNumber()) ? ":" + this.getLineNumber() : "",
          o = e(this.getColumnNumber()) ? ":" + this.getColumnNumber() : "";
        return t + n + r + i + o
      }
    }, t
  })
}
