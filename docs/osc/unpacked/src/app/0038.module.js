// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 38
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t() {
      for (var e, t = 0, i = arguments.length, o = {}; t < i; ++t) {
        if (!(e = arguments[t] + "") || e in o || /[\s.]/.test(e)) throw new Error("illegal type: " + e);
        o[e] = [];
      }
      return new n(o);
    }

    function n(e) {
      this._ = e;
    }

    function i(e, t) {
      return e.trim().split(/^|\s+/).map(function(e) {
        var n = "",
          i = e.indexOf(".");
        if (i >= 0 && (n = e.slice(i + 1), e = e.slice(0, i)), e && !t.hasOwnProperty(e))
        throw new Error("unknown type: " + e);
        return {
          type: e,
          name: n
        };
      });
    }

    function o(e, t) {
      for (var n, i = 0, o = e.length; i < o; ++i)
        if ((n = e[i]).name === t) return n.value;
    }

    function r(e, t, n) {
      for (var i = 0, o = e.length; i < o; ++i)
        if (e[i].name === t) {
          e[i] = a, e = e.slice(0, i).concat(e.slice(i + 1));
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
          l = i(e + "", a),
          s = -1,
          d = l.length;
        {
          if (!(arguments.length < 2)) {
            if (null != t && "function" != typeof t) throw new Error("invalid callback: " + t);
            for (; ++s < d;)
              if (n = (e = l[s]).type) a[n] = r(a[n], e.name, t);
              else if (null == t)
              for (n in a) a[n] = r(a[n], e.name, null);
            return this;
          }
          for (; ++s < d;)
            if ((n = (e = l[s]).type) && (n = o(a[n], e.name))) return n;
        }
      },
      copy: function() {
        var e = {},
          t = this._;
        for (var i in t) e[i] = t[i].slice();
        return new n(e);
      },
      call: function(e, t) {
        if ((n = arguments.length - 2) > 0)
          for (var n, i, o = new Array(n), r = 0; r < n; ++r) o[r] = arguments[r + 2];
        if (!this._.hasOwnProperty(e)) throw new Error("unknown type: " + e);
        for (i = this._[e], r = 0, n = i.length; r < n; ++r) i[r].value.apply(t, o);
      },
      apply: function(e, t, n) {
        if (!this._.hasOwnProperty(e)) throw new Error("unknown type: " + e);
        for (var i = this._[e], o = 0, r = i.length; o < r; ++o) i[o].value.apply(t, n);
      }
    }, e.dispatch = t, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
