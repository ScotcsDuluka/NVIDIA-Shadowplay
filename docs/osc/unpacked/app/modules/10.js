// ─────────────────────────────────────────────────────────────
// APP MODULE 10
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function i(e, t) {
    for (var n = 0; n < e.length; n++) {
      var i = e[n],
        o = m[i.id];
      if (o) {
        o.refs++;
        for (var r = 0; r < o.parts.length; r++) o.parts[r](i.parts[r]);
        for (; r < i.parts.length; r++) o.parts.push(d(i.parts[r], t))
      } else {
        for (var a = [], r = 0; r < i.parts.length; r++) a.push(d(i.parts[r], t));
        m[i.id] = {
          id: i.id,
          refs: 1,
          parts: a
        }
      }
    }
  }

  function o(e) {
    for (var t = [], n = {}, i = 0; i < e.length; i++) {
      var o = e[i],
        r = o[0],
        a = o[1],
        l = o[2],
        s = o[3],
        d = {
          css: a,
          media: l,
          sourceMap: s
        };
      n[r] ? n[r].parts.push(d) : t.push(n[r] = {
        id: r,
        parts: [d]
      })
    }
    return t
  }

  function r(e, t) {
    var n = h(),
      i = v[v.length - 1];
    if ("top" === e.insertAt) i ? i.nextSibling ? n.insertBefore(t, i.nextSibling) : n.appendChild(t) : n.insertBefore(
      t, n.firstChild), v.push(t);
    else {
      if ("bottom" !== e.insertAt) throw new Error(
      "Invalid value for parameter 'insertAt'. Must be 'top' or 'bottom'.");
      n.appendChild(t)
    }
  }

  function a(e) {
    e.parentNode.removeChild(e);
    var t = v.indexOf(e);
    t >= 0 && v.splice(t, 1)
  }

  function l(e) {
    var t = document.createElement("style");
    return t.type = "text/css", r(e, t), t
  }

  function s(e) {
    var t = document.createElement("link");
    return t.rel = "stylesheet", r(e, t), t
  }

  function d(e, t) {
    var n, i, o;
    if (t.singleton) {
      var r = x++;
      n = b || (b = l(t)), i = c.bind(null, n, r, !1), o = c.bind(null, n, r, !0)
    } else e.sourceMap && "function" == typeof URL && "function" == typeof URL.createObjectURL && "function" ==
      typeof URL.revokeObjectURL && "function" == typeof Blob && "function" == typeof btoa ? (n = s(t), i = f.bind(null,
        n), o = function() {
        a(n), n.href && URL.revokeObjectURL(n.href)
      }) : (n = l(t), i = u.bind(null, n), o = function() {
        a(n)
      });
    return i(e),
      function(t) {
        if (t) {
          if (t.css === e.css && t.media === e.media && t.sourceMap === e.sourceMap) return;
          i(e = t)
        } else o()
      }
  }

  function c(e, t, n, i) {
    var o = n ? "" : i.css;
    if (e.styleSheet) e.styleSheet.cssText = y(t, o);
    else {
      var r = document.createTextNode(o),
        a = e.childNodes;
      a[t] && e.removeChild(a[t]), a.length ? e.insertBefore(r, a[t]) : e.appendChild(r)
    }
  }

  function u(e, t) {
    var n = t.css,
      i = t.media;
    if (i && e.setAttribute("media", i), e.styleSheet) e.styleSheet.cssText = n;
    else {
      for (; e.firstChild;) e.removeChild(e.firstChild);
      e.appendChild(document.createTextNode(n))
    }
  }

  function f(e, t) {
    var n = t.css,
      i = t.sourceMap;
    i && (n += "\n/*# sourceMappingURL=data:application/json;base64," + btoa(unescape(encodeURIComponent(JSON.stringify(
      i)))) + " */");
    var o = new Blob([n], {
        type: "text/css"
      }),
      r = e.href;
    e.href = URL.createObjectURL(o), r && URL.revokeObjectURL(r)
  }
  var m = {},
    g = function(e) {
      var t;
      return function() {
        return "undefined" == typeof t && (t = e.apply(this, arguments)), t
      }
    },
    p = g(function() {
      return /msie [6-9]\b/.test(self.navigator.userAgent.toLowerCase())
    }),
    h = g(function() {
      return document.head || document.getElementsByTagName("head")[0]
    }),
    b = null,
    x = 0,
    v = [];
  e.exports = function(e, t) {
    t = t || {}, "undefined" == typeof t.singleton && (t.singleton = p()), "undefined" == typeof t.insertAt && (t
      .insertAt = "bottom");
    var n = o(e);
    return i(n, t),
      function(e) {
        for (var r = [], a = 0; a < n.length; a++) {
          var l = n[a],
            s = m[l.id];
          s.refs--, r.push(s)
        }
        if (e) {
          var d = o(e);
          i(d, t)
        }
        for (var a = 0; a < r.length; a++) {
          var s = r[a];
          if (0 === s.refs) {
            for (var c = 0; c < s.parts.length; c++) s.parts[c]();
            delete m[s.id]
          }
        }
      }
  };
  var y = function() {
    var e = [];
    return function(t, n) {
      return e[t] = n, e.filter(Boolean).join("\n")
    }
  }()
}
