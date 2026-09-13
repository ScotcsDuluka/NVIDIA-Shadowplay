// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 273
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(t) {
    function r() {}

    function i(e) {
      o.call(this, e), this.query = this.query || {}, s || (t.___eio || (t.___eio = []), s = t.___eio), this.index = s
        .length;
      var n = this;
      s.push(function(e) {
        n.onData(e)
      }), this.query.j = this.index, t.document && t.addEventListener && t.addEventListener("beforeunload",
        function() {
          n.script && (n.script.onerror = r)
        }, !1)
    }
    var o = n(110),
      a = n(34);
    e.exports = i;
    var s, c = /\n/g,
      u = /\\n/g;
    a(i, o), i.prototype.supportsBinary = !1, i.prototype.doClose = function() {
      this.script && (this.script.parentNode.removeChild(this.script), this.script = null), this.form && (this.form
        .parentNode.removeChild(this.form), this.form = null, this.iframe = null), o.prototype.doClose.call(this)
    }, i.prototype.doPoll = function() {
      var e = this,
        t = document.createElement("script");
      this.script && (this.script.parentNode.removeChild(this.script), this.script = null), t.async = !0, t.src =
        this.uri(), t.onerror = function(t) {
          e.onError("jsonp poll error", t)
        };
      var n = document.getElementsByTagName("script")[0];
      n.parentNode.insertBefore(t, n), this.script = t;
      var r = "undefined" != typeof navigator && /gecko/i.test(navigator.userAgent);
      r && setTimeout(function() {
        var e = document.createElement("iframe");
        document.body.appendChild(e), document.body.removeChild(e)
      }, 100)
    }, i.prototype.doWrite = function(e, t) {
      function n() {
        r(), t()
      }

      function r() {
        if (i.iframe) try {
          i.form.removeChild(i.iframe)
        } catch (e) {
          i.onError("jsonp polling iframe removal error", e)
        }
        try {
          var e = '<iframe src="javascript:0" name="' + i.iframeId + '">';
          o = document.createElement(e)
        } catch (e) {
          o = document.createElement("iframe"), o.name = i.iframeId, o.src = "javascript:0"
        }
        o.id = i.iframeId, i.form.appendChild(o), i.iframe = o
      }
      var i = this;
      if (!this.form) {
        var o, a = document.createElement("form"),
          s = document.createElement("textarea"),
          l = this.iframeId = "eio_iframe_" + this.index;
        a.className = "socketio", a.style.position = "absolute", a.style.top = "-1000px", a.style.left = "-1000px",
          a.target = l, a.method = "POST", a.setAttribute("accept-charset", "utf-8"), s.name = "d", a.appendChild(
          s), document.body.appendChild(a), this.form = a, this.area = s
      }
      this.form.action = this.uri(), r(), e = e.replace(u, "\\\n"), this.area.value = e.replace(c, "\\n");
      try {
        this.form.submit()
      } catch (e) {}
      this.iframe.attachEvent ? this.iframe.onreadystatechange = function() {
        "complete" == i.iframe.readyState && n()
      } : this.iframe.onload = n
    }
  }).call(t, function() {
    return this
  }())
}
