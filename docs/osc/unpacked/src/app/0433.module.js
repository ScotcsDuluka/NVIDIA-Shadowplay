// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 433
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(127));
  }(this, function(e, t) {
    "use strict";

    function n(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      return e.blob();
    }

    function i(e, t) {
      return fetch(e, t).then(n);
    }

    function o(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      return e.arrayBuffer();
    }

    function r(e, t) {
      return fetch(e, t).then(o);
    }

    function a(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      return e.text();
    }

    function l(e, t) {
      return fetch(e, t).then(a);
    }

    function s(e) {
      return function(t, n, i) {
        return 2 === arguments.length && "function" == typeof n && (i = n, n = void 0), l(t, n).then(
          function(t) {
            return e(t, i);
          });
      };
    }

    function d(e, n, i, o) {
      3 === arguments.length && "function" == typeof i && (o = i, i = void 0);
      var r = t.dsvFormat(e);
      return l(n, i).then(function(e) {
        return r.parse(e, o);
      });
    }

    function c(e, t) {
      return new Promise(function(n, i) {
        var o = new Image();
        for (var r in t) o[r] = t[r];
        o.onerror = i, o.onload = function() {
          n(o);
        }, o.src = e;
      });
    }

    function u(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      if (204 !== e.status && 205 !== e.status) return e.json();
    }

    function f(e, t) {
      return fetch(e, t).then(u);
    }

    function m(e) {
      return function(t, n) {
        return l(t, n).then(function(t) {
          return new DOMParser().parseFromString(t, e);
        });
      };
    }
    var g = s(t.csvParse),
      p = s(t.tsvParse),
      h = m("application/xml"),
      b = m("text/html"),
      x = m("image/svg+xml");
    e.blob = i, e.buffer = r, e.csv = g, e.dsv = d, e.html = b, e.image = c, e.json = f, e.svg = x, e.text =
      l, e.tsv = p, e.xml = h, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
