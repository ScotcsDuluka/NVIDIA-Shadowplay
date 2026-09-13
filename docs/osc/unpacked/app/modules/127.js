// ─────────────────────────────────────────────────────────────
// APP MODULE 127
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e) {
      return new Function("d", "return {" + e.map(function(e, t) {
        return JSON.stringify(e) + ": d[" + t + '] || ""'
      }).join(",") + "}")
    }

    function n(e, n) {
      var i = t(e);
      return function(t, o) {
        return n(i(t), o, e)
      }
    }

    function i(e) {
      var t = Object.create(null),
        n = [];
      return e.forEach(function(e) {
        for (var i in e) i in t || n.push(t[i] = i)
      }), n
    }

    function o(e, t) {
      var n = e + "",
        i = n.length;
      return i < t ? new Array(t - i + 1).join(0) + n : n
    }

    function r(e) {
      return e < 0 ? "-" + o(-e, 6) : e > 9999 ? "+" + o(e, 6) : o(e, 4)
    }

    function a(e) {
      var t = e.getUTCHours(),
        n = e.getUTCMinutes(),
        i = e.getUTCSeconds(),
        a = e.getUTCMilliseconds();
      return isNaN(e) ? "Invalid Date" : r(e.getUTCFullYear()) + "-" + o(e.getUTCMonth() + 1, 2) + "-" + o(e
        .getUTCDate(), 2) + (a ? "T" + o(t, 2) + ":" + o(n, 2) + ":" + o(i, 2) + "." + o(a, 3) + "Z" : i ? "T" + o(
        t, 2) + ":" + o(n, 2) + ":" + o(i, 2) + "Z" : n || t ? "T" + o(t, 2) + ":" + o(n, 2) + "Z" : "")
    }

    function l(e) {
      function o(e, i) {
        var o, a, l = r(e, function(e, r) {
          return o ? o(e, r - 1) : (a = e, void(o = i ? n(e, i) : t(e)))
        });
        return l.columns = a || [], l
      }

      function r(e, t) {
        function n() {
          if (s) return c;
          if (g) return g = !1, d;
          var t, n, i = a;
          if (e.charCodeAt(i) === u) {
            for (; a++ < r && e.charCodeAt(a) !== u || e.charCodeAt(++a) === u;);
            return (t = a) >= r ? s = !0 : (n = e.charCodeAt(a++)) === f ? g = !0 : n === m && (g = !0, e.charCodeAt(
              a) === f && ++a), e.slice(i + 1, t - 1).replace(/""/g, '"')
          }
          for (; a < r;) {
            if ((n = e.charCodeAt(t = a++)) === f) g = !0;
            else if (n === m) g = !0, e.charCodeAt(a) === f && ++a;
            else if (n !== v) continue;
            return e.slice(i, t)
          }
          return s = !0, e.slice(i, r)
        }
        var i, o = [],
          r = e.length,
          a = 0,
          l = 0,
          s = r <= 0,
          g = !1;
        for (e.charCodeAt(r - 1) === f && --r, e.charCodeAt(r - 1) === m && --r;
          (i = n()) !== c;) {
          for (var p = []; i !== d && i !== c;) p.push(i), i = n();
          t && null == (p = t(p, l++)) || o.push(p)
        }
        return o
      }

      function l(t, n) {
        return t.map(function(t) {
          return n.map(function(e) {
            return b(t[e])
          }).join(e)
        })
      }

      function s(t, n) {
        return null == n && (n = i(t)), [n.map(b).join(e)].concat(l(t, n)).join("\n")
      }

      function g(e, t) {
        return null == t && (t = i(e)), l(e, t).join("\n")
      }

      function p(e) {
        return e.map(h).join("\n")
      }

      function h(t) {
        return t.map(b).join(e)
      }

      function b(e) {
        return null == e ? "" : e instanceof Date ? a(e) : x.test(e += "") ? '"' + e.replace(/"/g, '""') + '"' : e
      }
      var x = new RegExp('["' + e + "\n\r]"),
        v = e.charCodeAt(0);
      return {
        parse: o,
        parseRows: r,
        format: s,
        formatBody: g,
        formatRows: p,
        formatRow: h,
        formatValue: b
      }
    }

    function s(e) {
      for (var t in e) {
        var n, i, o = e[t].trim();
        if (o)
          if ("true" === o) o = !0;
          else if ("false" === o) o = !1;
        else if ("NaN" === o) o = NaN;
        else if (isNaN(n = +o)) {
          if (!(i = o.match(
              /^([-+]\d{2})?\d{4}(-\d{2}(-\d{2})?)?(T\d{2}:\d{2}(:\d{2}(\.\d{3})?)?(Z|[-+]\d{2}:\d{2})?)?$/)))
            continue;
          I && i[4] && !i[7] && (o = o.replace(/-/g, "/").replace(/T/, " ")), o = new Date(o)
        } else o = n;
        else o = null;
        e[t] = o
      }
      return e
    }
    var d = {},
      c = {},
      u = 34,
      f = 10,
      m = 13,
      g = l(","),
      p = g.parse,
      h = g.parseRows,
      b = g.format,
      x = g.formatBody,
      v = g.formatRows,
      y = g.formatRow,
      w = g.formatValue,
      S = l("\t"),
      E = S.parse,
      k = S.parseRows,
      _ = S.format,
      T = S.formatBody,
      C = S.formatRows,
      O = S.formatRow,
      A = S.formatValue,
      I = new Date("2019-01-01T00:00").getHours() || new Date("2019-07-01T00:00").getHours();
    e.autoType = s, e.csvFormat = b, e.csvFormatBody = x, e.csvFormatRow = y, e.csvFormatRows = v, e.csvFormatValue =
      w, e.csvParse = p, e.csvParseRows = h, e.dsvFormat = l, e.tsvFormat = _, e.tsvFormatBody = T, e.tsvFormatRow =
      O, e.tsvFormatRows = C, e.tsvFormatValue = A, e.tsvParse = E, e.tsvParseRows = k, Object.defineProperty(e,
        "__esModule", {
          value: !0
        })
  })
}
