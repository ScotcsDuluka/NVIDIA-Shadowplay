// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 20
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t() {
      for (var e, t = 0, r = arguments.length, i = {}; t < r; ++t) {
        if (!(e = arguments[t] + "") || e in i || /[\s.]/.test(e)) throw new Error("illegal type: " + e);
        i[e] = [];
      }
      return new n(i);
    }

    function n(e) {
      this._ = e;
    }

    function r(e, t) {
      return e.trim().split(/^|\s+/).map(function(e) {
        var n = "",
          r = e.indexOf(".");
        if (r >= 0 && (n = e.slice(r + 1), e = e.slice(0, r)), e && !t.hasOwnProperty(e))
        throw new Error("unknown type: " + e);
        return {
          type: e,
          name: n
        };
      });
    }

    function i(e, t) {
      for (var n, r = 0, i = e.length; r < i; ++r)
        if ((n = e[r]).name === t) return n.value;
    }

    function o(e, t, n) {
      for (var r = 0, i = e.length; r < i; ++r)
        if (e[r].name === t) {
          e[r] = a, e = e.slice(0, r).concat(e.slice(r + 1));
          break;
        }
      return null != n && e.push({
        name: t,
        value: n
      }), e;
    }
    var a = {
      value: function() {}
    };
    n.prototype = t.prototype = {
      constructor: n,
      on: function(e, t) {
        var n,
          a = this._,
          s = r(e + "", a),
          c = -1,
          u = s.length;
        {
          if (!(arguments.length < 2)) {
            if (null != t && "function" != typeof t) throw new Error("invalid callback: " + t);
            for (; ++c < u;)
              if (n = (e = s[c]).type) a[n] = o(a[n], e.name, t);
              else if (null == t)
              for (n in a) a[n] = o(a[n], e.name, null);
            return this;
          }
          for (; ++c < u;)
            if ((n = (e = s[c]).type) && (n = i(a[n], e.name))) return n;
        }
      },
      copy: function() {
        var e = {},
          t = this._;
        for (var r in t) e[r] = t[r].slice();
        return new n(e);
      },
      call: function(e, t) {
        if ((n = arguments.length - 2) > 0)
          for (var n, r, i = new Array(n), o = 0; o < n; ++o) i[o] = arguments[o + 2];
        if (!this._.hasOwnProperty(e)) throw new Error("unknown type: " + e);
        for (r = this._[e], o = 0, n = r.length; o < n; ++o) r[o].value.apply(t, i);
      },
      apply: function(e, t, n) {
        if (!this._.hasOwnProperty(e)) throw new Error("unknown type: " + e);
        for (var r = this._[e], i = 0, o = r.length; i < o; ++i) r[i].value.apply(t, n);
      }
    }, e.dispatch = t, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
