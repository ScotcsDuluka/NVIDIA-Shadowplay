// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 244
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(99));
  }(this, function(e, t) {
    "use strict";

    function n(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      return e.blob();
    }

    function r(e, t) {
      return fetch(e, t).then(n);
    }

    function i(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      return e.arrayBuffer();
    }

    function o(e, t) {
      return fetch(e, t).then(i);
    }

    function a(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      return e.text();
    }

    function s(e, t) {
      return fetch(e, t).then(a);
    }

    function c(e) {
      return function(t, n, r) {
        return 2 === arguments.length && "function" == typeof n && (r = n, n = void 0), s(t, n).then(
          function(t) {
            return e(t, r);
          });
      };
    }

    function u(e, n, r, i) {
      3 === arguments.length && "function" == typeof r && (i = r, r = void 0);
      var o = t.dsvFormat(e);
      return s(n, r).then(function(e) {
        return o.parse(e, i);
      });
    }

    function l(e, t) {
      return new Promise(function(n, r) {
        var i = new Image();
        for (var o in t) i[o] = t[o];
        i.onerror = r, i.onload = function() {
          n(i);
        }, i.src = e;
      });
    }

    function d(e) {
      if (!e.ok) throw new Error(e.status + " " + e.statusText);
      if (204 !== e.status && 205 !== e.status) return e.json();
    }

    function f(e, t) {
      return fetch(e, t).then(d);
    }

    function h(e) {
      return function(t, n) {
        return s(t, n).then(function(t) {
          return new DOMParser().parseFromString(t, e);
        });
      };
    }
    var p = c(t.csvParse),
      m = c(t.tsvParse),
      v = h("application/xml"),
      g = h("text/html"),
      y = h("image/svg+xml");
    e.blob = r, e.buffer = o, e.csv = p, e.dsv = u, e.html = g, e.image = l, e.json = f, e.svg = y, e.text =
      s, e.tsv = m, e.xml = v, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
