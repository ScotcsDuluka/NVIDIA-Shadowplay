// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 287
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r, i;
  (function() {
    function n(e) {
      function t(t, n, r, i, o, a) {
        for (; o >= 0 && o < a; o += e) {
          var s = i ? i[o] : o;
          r = n(r, t[s], s, t);
        }
        return r;
      }
      return function(n, r, i, o) {
        r = w(r, o, 4);
        var a = !k(n) && $.keys(n),
          s = (a || n).length,
          c = e > 0 ? 0 : s - 1;
        return arguments.length < 3 && (i = n[a ? a[c] : c], c += e), t(n, r, i, a, c, s);
      };
    }

    function o(e) {
      return function(t, n, r) {
        n = T(n, r);
        for (var i = M(t), o = e > 0 ? 0 : i - 1; o >= 0 && o < i; o += e)
          if (n(t[o], o, t)) return o;
        return -1;
      };
    }

    function a(e, t, n) {
      return function(r, i, o) {
        var a = 0,
          s = M(r);
        if ("number" == typeof o) e > 0 ? a = o >= 0 ? o : Math.max(o + s, a) : s = o >= 0 ? Math.min(o +
          1, s) : o + s + 1;
        else if (n && o && s) return o = n(r, i), r[o] === i ? o : -1;
        if (i !== i) return o = t(p.call(r, a, s), $.isNaN), o >= 0 ? o + a : -1;
        for (o = e > 0 ? a : s - 1; o >= 0 && o < s; o += e)
          if (r[o] === i) return o;
        return -1;
      };
    }

    function s(e, t) {
      var n = R.length,
        r = e.constructor,
        i = $.isFunction(r) && r.prototype || d,
        o = "constructor";
      for ($.has(e, o) && !$.contains(t, o) && t.push(o); n--;) o = R[n], o in e && e[o] !== i[o] && !$
        .contains(t, o) && t.push(o);
    }
    var c = this,
      u = c._,
      l = Array.prototype,
      d = Object.prototype,
      f = Function.prototype,
      h = l.push,
      p = l.slice,
      m = d.toString,
      v = d.hasOwnProperty,
      g = Array.isArray,
      y = Object.keys,
      b = f.bind,
      E = Object.create,
      _ = function() {},
      $ = function(e) {
        return e instanceof $ ? e : this instanceof $ ? void(this._wrapped = e) : new $(e);
      };
    "undefined" != typeof module && module.exports && (exports = module.exports = $), exports._ = $, $
      .VERSION = "1.8.3";
    var w = function(e, t, n) {
        if (void 0 === t) return e;
        switch (null == n ? 3 : n) {
          case 1:
            return function(n) {
              return e.call(t, n);
            };
          case 2:
            return function(n, r) {
              return e.call(t, n, r);
            };
          case 3:
            return function(n, r, i) {
              return e.call(t, n, r, i);
            };
          case 4:
            return function(n, r, i, o) {
              return e.call(t, n, r, i, o);
            };
        }
        return function() {
          return e.apply(t, arguments);
        };
      },
      T = function(e, t, n) {
        return null == e ? $.identity : $.isFunction(e) ? w(e, t, n) : $.isObject(e) ? $.matcher(e) : $
          .property(e);
      };
    $.iteratee = function(e, t) {
      return T(e, t, 1 / 0);
    };
    var C = function(e, t) {
        return function(n) {
          var r = arguments.length;
          if (r < 2 || null == n) return n;
          for (var i = 1; i < r; i++)
            for (var o = arguments[i], a = e(o), s = a.length, c = 0; c < s; c++) {
              var u = a[c];
              t && void 0 !== n[u] || (n[u] = o[u]);
            }
          return n;
        };
      },
      x = function(e) {
        if (!$.isObject(e)) return {};
        if (E) return E(e);
        _.prototype = e;
        var t = new _();
        return _.prototype = null, t;
      },
      S = function(e) {
        return function(t) {
          return null == t ? void 0 : t[e];
        };
      },
      A = Math.pow(2, 53) - 1,
      M = S("length"),
      k = function(e) {
        var t = M(e);
        return "number" == typeof t && t >= 0 && t <= A;
      };
    $.each = $.forEach = function(e, t, n) {
        t = w(t, n);
        var r, i;
        if (k(e))
          for (r = 0, i = e.length; r < i; r++) t(e[r], r, e);
        else {
          var o = $.keys(e);
          for (r = 0, i = o.length; r < i; r++) t(e[o[r]], o[r], e);
        }
        return e;
      }, $.map = $.collect = function(e, t, n) {
        t = T(t, n);
        for (var r = !k(e) && $.keys(e), i = (r || e).length, o = Array(i), a = 0; a < i; a++) {
          var s = r ? r[a] : a;
          o[a] = t(e[s], s, e);
        }
        return o;
      }, $.reduce = $.foldl = $.inject = n(1), $.reduceRight = $.foldr = n(-1), $.find = $.detect =
      function(e, t, n) {
        var r;
        if (r = k(e) ? $.findIndex(e, t, n) : $.findKey(e, t, n), void 0 !== r && r !== -1) return e[r];
      }, $.filter = $.select = function(e, t, n) {
        var r = [];
        return t = T(t, n), $.each(e, function(e, n, i) {
          t(e, n, i) && r.push(e);
        }), r;
      }, $.reject = function(e, t, n) {
        return $.filter(e, $.negate(T(t)), n);
      }, $.every = $.all = function(e, t, n) {
        t = T(t, n);
        for (var r = !k(e) && $.keys(e), i = (r || e).length, o = 0; o < i; o++) {
          var a = r ? r[o] : o;
          if (!t(e[a], a, e)) return !1;
        }
        return !0;
      }, $.some = $.any = function(e, t, n) {
        t = T(t, n);
        for (var r = !k(e) && $.keys(e), i = (r || e).length, o = 0; o < i; o++) {
          var a = r ? r[o] : o;
          if (t(e[a], a, e)) return !0;
        }
        return !1;
      }, $.contains = $.includes = $.include = function(e, t, n, r) {
        return k(e) || (e = $.values(e)), ("number" != typeof n || r) && (n = 0), $.indexOf(e, t, n) >= 0;
      }, $.invoke = function(e, t) {
        var n = p.call(arguments, 2),
          r = $.isFunction(t);
        return $.map(e, function(e) {
          var i = r ? t : e[t];
          return null == i ? i : i.apply(e, n);
        });
      }, $.pluck = function(e, t) {
        return $.map(e, $.property(t));
      }, $.where = function(e, t) {
        return $.filter(e, $.matcher(t));
      }, $.findWhere = function(e, t) {
        return $.find(e, $.matcher(t));
      }, $.max = function(e, t, n) {
        var r,
          i,
          o = -(1 / 0),
          a = -(1 / 0);
        if (null == t && null != e) {
          e = k(e) ? e : $.values(e);
          for (var s = 0, c = e.length; s < c; s++) r = e[s], r > o && (o = r);
        } else t = T(t, n), $.each(e, function(e, n, r) {
          i = t(e, n, r), (i > a || i === -(1 / 0) && o === -(1 / 0)) && (o = e, a = i);
        });
        return o;
      }, $.min = function(e, t, n) {
        var r,
          i,
          o = 1 / 0,
          a = 1 / 0;
        if (null == t && null != e) {
          e = k(e) ? e : $.values(e);
          for (var s = 0, c = e.length; s < c; s++) r = e[s], r < o && (o = r);
        } else t = T(t, n), $.each(e, function(e, n, r) {
          i = t(e, n, r), (i < a || i === 1 / 0 && o === 1 / 0) && (o = e, a = i);
        });
        return o;
      }, $.shuffle = function(e) {
        for (var t, n = k(e) ? e : $.values(e), r = n.length, i = Array(r), o = 0; o < r; o++) t = $.random(
          0, o), t !== o && (i[o] = i[t]), i[t] = n[o];
        return i;
      }, $.sample = function(e, t, n) {
        return null == t || n ? (k(e) || (e = $.values(e)), e[$.random(e.length - 1)]) : $.shuffle(e).slice(
          0, Math.max(0, t));
      }, $.sortBy = function(e, t, n) {
        return t = T(t, n), $.pluck($.map(e, function(e, n, r) {
          return {
            value: e,
            index: n,
            criteria: t(e, n, r)
          };
        }).sort(function(e, t) {
          var n = e.criteria,
            r = t.criteria;
          if (n !== r) {
            if (n > r || void 0 === n) return 1;
            if (n < r || void 0 === r) return -1;
          }
          return e.index - t.index;
        }), "value");
      };
    var N = function(e) {
      return function(t, n, r) {
        var i = {};
        return n = T(n, r), $.each(t, function(r, o) {
          var a = n(r, o, t);
          e(i, r, a);
        }), i;
      };
    };
    $.groupBy = N(function(e, t, n) {
      $.has(e, n) ? e[n].push(t) : e[n] = [t];
    }), $.indexBy = N(function(e, t, n) {
      e[n] = t;
    }), $.countBy = N(function(e, t, n) {
      $.has(e, n) ? e[n]++ : e[n] = 1;
    }), $.toArray = function(e) {
      return e ? $.isArray(e) ? p.call(e) : k(e) ? $.map(e, $.identity) : $.values(e) : [];
    }, $.size = function(e) {
      return null == e ? 0 : k(e) ? e.length : $.keys(e).length;
    }, $.partition = function(e, t, n) {
      t = T(t, n);
      var r = [],
        i = [];
      return $.each(e, function(e, n, o) {
        (t(e, n, o) ? r : i).push(e);
      }), [r, i];
    }, $.first = $.head = $.take = function(e, t, n) {
      if (null != e) return null == t || n ? e[0] : $.initial(e, e.length - t);
    }, $.initial = function(e, t, n) {
      return p.call(e, 0, Math.max(0, e.length - (null == t || n ? 1 : t)));
    }, $.last = function(e, t, n) {
      if (null != e) return null == t || n ? e[e.length - 1] : $.rest(e, Math.max(0, e.length - t));
    }, $.rest = $.tail = $.drop = function(e, t, n) {
      return p.call(e, null == t || n ? 1 : t);
    }, $.compact = function(e) {
      return $.filter(e, $.identity);
    };
    var I = function(e, t, n, r) {
      for (var i = [], o = 0, a = r || 0, s = M(e); a < s; a++) {
        var c = e[a];
        if (k(c) && ($.isArray(c) || $.isArguments(c))) {
          t || (c = I(c, t, n));
          var u = 0,
            l = c.length;
          for (i.length += l; u < l;) i[o++] = c[u++];
        } else n || (i[o++] = c);
      }
      return i;
    };
    $.flatten = function(e, t) {
        return I(e, t, !1);
      }, $.without = function(e) {
        return $.difference(e, p.call(arguments, 1));
      }, $.uniq = $.unique = function(e, t, n, r) {
        $.isBoolean(t) || (r = n, n = t, t = !1), null != n && (n = T(n, r));
        for (var i = [], o = [], a = 0, s = M(e); a < s; a++) {
          var c = e[a],
            u = n ? n(c, a, e) : c;
          t ? (a && o === u || i.push(c), o = u) : n ? $.contains(o, u) || (o.push(u), i.push(c)) : $
            .contains(i, c) || i.push(c);
        }
        return i;
      }, $.union = function() {
        return $.uniq(I(arguments, !0, !0));
      }, $.intersection = function(e) {
        for (var t = [], n = arguments.length, r = 0, i = M(e); r < i; r++) {
          var o = e[r];
          if (!$.contains(t, o)) {
            for (var a = 1; a < n && $.contains(arguments[a], o); a++);
            a === n && t.push(o);
          }
        }
        return t;
      }, $.difference = function(e) {
        var t = I(arguments, !0, !0, 1);
        return $.filter(e, function(e) {
          return !$.contains(t, e);
        });
      }, $.zip = function() {
        return $.unzip(arguments);
      }, $.unzip = function(e) {
        for (var t = e && $.max(e, M).length || 0, n = Array(t), r = 0; r < t; r++) n[r] = $.pluck(e, r);
        return n;
      }, $.object = function(e, t) {
        for (var n = {}, r = 0, i = M(e); r < i; r++) t ? n[e[r]] = t[r] : n[e[r][0]] = e[r][1];
        return n;
      }, $.findIndex = o(1), $.findLastIndex = o(-1), $.sortedIndex = function(e, t, n, r) {
        n = T(n, r, 1);
        for (var i = n(t), o = 0, a = M(e); o < a;) {
          var s = Math.floor((o + a) / 2);
          n(e[s]) < i ? o = s + 1 : a = s;
        }
        return o;
      }, $.indexOf = a(1, $.findIndex, $.sortedIndex), $.lastIndexOf = a(-1, $.findLastIndex), $.range =
      function(e, t, n) {
        null == t && (t = e || 0, e = 0), n = n || 1;
        for (var r = Math.max(Math.ceil((t - e) / n), 0), i = Array(r), o = 0; o < r; o++, e += n) i[o] = e;
        return i;
      };
    var O = function(e, t, n, r, i) {
      if (!(r instanceof t)) return e.apply(n, i);
      var o = x(e.prototype),
        a = e.apply(o, i);
      return $.isObject(a) ? a : o;
    };
    $.bind = function(e, t) {
      if (b && e.bind === b) return b.apply(e, p.call(arguments, 1));
      if (!$.isFunction(e)) throw new TypeError("Bind must be called on a function");
      var n = p.call(arguments, 2),
        r = function() {
          return O(e, r, t, this, n.concat(p.call(arguments)));
        };
      return r;
    }, $.partial = function(e) {
      var t = p.call(arguments, 1),
        n = function() {
          for (var r = 0, i = t.length, o = Array(i), a = 0; a < i; a++) o[a] = t[a] === $ ? arguments[
            r++] : t[a];
          for (; r < arguments.length;) o.push(arguments[r++]);
          return O(e, n, this, this, o);
        };
      return n;
    }, $.bindAll = function(e) {
      var t,
        n,
        r = arguments.length;
      if (r <= 1) throw new Error("bindAll must be passed function names");
      for (t = 1; t < r; t++) n = arguments[t], e[n] = $.bind(e[n], e);
      return e;
    }, $.memoize = function(e, t) {
      var n = function(r) {
        var i = n.cache,
          o = "" + (t ? t.apply(this, arguments) : r);
        return $.has(i, o) || (i[o] = e.apply(this, arguments)), i[o];
      };
      return n.cache = {}, n;
    }, $.delay = function(e, t) {
      var n = p.call(arguments, 2);
      return setTimeout(function() {
        return e.apply(null, n);
      }, t);
    }, $.defer = $.partial($.delay, $, 1), $.throttle = function(e, t, n) {
      var r,
        i,
        o,
        a = null,
        s = 0;
      n || (n = {});
      var c = function() {
        s = n.leading === !1 ? 0 : $.now(), a = null, o = e.apply(r, i), a || (r = i = null);
      };
      return function() {
        var u = $.now();
        s || n.leading !== !1 || (s = u);
        var l = t - (u - s);
        return r = this, i = arguments, l <= 0 || l > t ? (a && (clearTimeout(a), a = null), s = u, o =
          e.apply(r, i), a || (r = i = null)) : a || n.trailing === !1 || (a = setTimeout(c, l)), o;
      };
    }, $.debounce = function(e, t, n) {
      var r,
        i,
        o,
        a,
        s,
        c = function() {
          var u = $.now() - a;
          u < t && u >= 0 ? r = setTimeout(c, t - u) : (r = null, n || (s = e.apply(o, i), r || (o = i =
            null)));
        };
      return function() {
        o = this, i = arguments, a = $.now();
        var u = n && !r;
        return r || (r = setTimeout(c, t)), u && (s = e.apply(o, i), o = i = null), s;
      };
    }, $.wrap = function(e, t) {
      return $.partial(t, e);
    }, $.negate = function(e) {
      return function() {
        return !e.apply(this, arguments);
      };
    }, $.compose = function() {
      var e = arguments,
        t = e.length - 1;
      return function() {
        for (var n = t, r = e[t].apply(this, arguments); n--;) r = e[n].call(this, r);
        return r;
      };
    }, $.after = function(e, t) {
      return function() {
        if (--e < 1) return t.apply(this, arguments);
      };
    }, $.before = function(e, t) {
      var n;
      return function() {
        return --e > 0 && (n = t.apply(this, arguments)), e <= 1 && (t = null), n;
      };
    }, $.once = $.partial($.before, 2);
    var D = !{
        toString: null
      }.propertyIsEnumerable("toString"),
      R = ["valueOf", "isPrototypeOf", "toString", "propertyIsEnumerable", "hasOwnProperty",
        "toLocaleString"
      ];
    $.keys = function(e) {
      if (!$.isObject(e)) return [];
      if (y) return y(e);
      var t = [];
      for (var n in e) $.has(e, n) && t.push(n);
      return D && s(e, t), t;
    }, $.allKeys = function(e) {
      if (!$.isObject(e)) return [];
      var t = [];
      for (var n in e) t.push(n);
      return D && s(e, t), t;
    }, $.values = function(e) {
      for (var t = $.keys(e), n = t.length, r = Array(n), i = 0; i < n; i++) r[i] = e[t[i]];
      return r;
    }, $.mapObject = function(e, t, n) {
      t = T(t, n);
      for (var r, i = $.keys(e), o = i.length, a = {}, s = 0; s < o; s++) r = i[s], a[r] = t(e[r], r, e);
      return a;
    }, $.pairs = function(e) {
      for (var t = $.keys(e), n = t.length, r = Array(n), i = 0; i < n; i++) r[i] = [t[i], e[t[i]]];
      return r;
    }, $.invert = function(e) {
      for (var t = {}, n = $.keys(e), r = 0, i = n.length; r < i; r++) t[e[n[r]]] = n[r];
      return t;
    }, $.functions = $.methods = function(e) {
      var t = [];
      for (var n in e) $.isFunction(e[n]) && t.push(n);
      return t.sort();
    }, $.extend = C($.allKeys), $.extendOwn = $.assign = C($.keys), $.findKey = function(e, t, n) {
      t = T(t, n);
      for (var r, i = $.keys(e), o = 0, a = i.length; o < a; o++)
        if (r = i[o], t(e[r], r, e)) return r;
    }, $.pick = function(e, t, n) {
      var r,
        i,
        o = {},
        a = e;
      if (null == a) return o;
      $.isFunction(t) ? (i = $.allKeys(a), r = w(t, n)) : (i = I(arguments, !1, !1, 1), r = function(e, t,
        n) {
        return t in n;
      }, a = Object(a));
      for (var s = 0, c = i.length; s < c; s++) {
        var u = i[s],
          l = a[u];
        r(l, u, a) && (o[u] = l);
      }
      return o;
    }, $.omit = function(e, t, n) {
      if ($.isFunction(t)) t = $.negate(t);
      else {
        var r = $.map(I(arguments, !1, !1, 1), String);
        t = function(e, t) {
          return !$.contains(r, t);
        };
      }
      return $.pick(e, t, n);
    }, $.defaults = C($.allKeys, !0), $.create = function(e, t) {
      var n = x(e);
      return t && $.extendOwn(n, t), n;
    }, $.clone = function(e) {
      return $.isObject(e) ? $.isArray(e) ? e.slice() : $.extend({}, e) : e;
    }, $.tap = function(e, t) {
      return t(e), e;
    }, $.isMatch = function(e, t) {
      var n = $.keys(t),
        r = n.length;
      if (null == e) return !r;
      for (var i = Object(e), o = 0; o < r; o++) {
        var a = n[o];
        if (t[a] !== i[a] || !(a in i)) return !1;
      }
      return !0;
    };
    var P = function(e, t, n, r) {
      if (e === t) return 0 !== e || 1 / e === 1 / t;
      if (null == e || null == t) return e === t;
      e instanceof $ && (e = e._wrapped), t instanceof $ && (t = t._wrapped);
      var i = m.call(e);
      if (i !== m.call(t)) return !1;
      switch (i) {
        case "[object RegExp]":
        case "[object String]":
          return "" + e == "" + t;
        case "[object Number]":
          return +e !== +e ? +t !== +t : 0 === +e ? 1 / +e === 1 / t : +e === +t;
        case "[object Date]":
        case "[object Boolean]":
          return +e === +t;
      }
      var o = "[object Array]" === i;
      if (!o) {
        if ("object" != typeof e || "object" != typeof t) return !1;
        var a = e.constructor,
          s = t.constructor;
        if (a !== s && !($.isFunction(a) && a instanceof a && $.isFunction(s) && s instanceof s) &&
          "constructor" in e && "constructor" in t) return !1;
      }
      n = n || [], r = r || [];
      for (var c = n.length; c--;)
        if (n[c] === e) return r[c] === t;
      if (n.push(e), r.push(t), o) {
        if (c = e.length, c !== t.length) return !1;
        for (; c--;)
          if (!P(e[c], t[c], n, r)) return !1;
      } else {
        var u,
          l = $.keys(e);
        if (c = l.length, $.keys(t).length !== c) return !1;
        for (; c--;)
          if (u = l[c], !$.has(t, u) || !P(e[u], t[u], n, r)) return !1;
      }
      return n.pop(), r.pop(), !0;
    };
    $.isEqual = function(e, t) {
      return P(e, t);
    }, $.isEmpty = function(e) {
      return null == e || (k(e) && ($.isArray(e) || $.isString(e) || $.isArguments(e)) ? 0 === e.length :
        0 === $.keys(e).length);
    }, $.isElement = function(e) {
      return !(!e || 1 !== e.nodeType);
    }, $.isArray = g || function(e) {
      return "[object Array]" === m.call(e);
    }, $.isObject = function(e) {
      var t = typeof e;
      return "function" === t || "object" === t && !!e;
    }, $.each(["Arguments", "Function", "String", "Number", "Date", "RegExp", "Error"], function(e) {
      $["is" + e] = function(t) {
        return m.call(t) === "[object " + e + "]";
      };
    }), $.isArguments(arguments) || ($.isArguments = function(e) {
      return $.has(e, "callee");
    }), "function" != typeof /./ && "object" != typeof Int8Array && ($.isFunction = function(e) {
      return "function" == typeof e || !1;
    }), $.isFinite = function(e) {
      return isFinite(e) && !isNaN(parseFloat(e));
    }, $.isNaN = function(e) {
      return $.isNumber(e) && e !== +e;
    }, $.isBoolean = function(e) {
      return e === !0 || e === !1 || "[object Boolean]" === m.call(e);
    }, $.isNull = function(e) {
      return null === e;
    }, $.isUndefined = function(e) {
      return void 0 === e;
    }, $.has = function(e, t) {
      return null != e && v.call(e, t);
    }, $.noConflict = function() {
      return c._ = u, this;
    }, $.identity = function(e) {
      return e;
    }, $.constant = function(e) {
      return function() {
        return e;
      };
    }, $.noop = function() {}, $.property = S, $.propertyOf = function(e) {
      return null == e ? function() {} : function(t) {
        return e[t];
      };
    }, $.matcher = $.matches = function(e) {
      return e = $.extendOwn({}, e),
        function(t) {
          return $.isMatch(t, e);
        };
    }, $.times = function(e, t, n) {
      var r = Array(Math.max(0, e));
      t = w(t, n, 1);
      for (var i = 0; i < e; i++) r[i] = t(i);
      return r;
    }, $.random = function(e, t) {
      return null == t && (t = e, e = 0), e + Math.floor(Math.random() * (t - e + 1));
    }, $.now = Date.now || function() {
      return new Date().getTime();
    };
    var L = {
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#x27;",
        "`": "&#x60;"
      },
      U = $.invert(L),
      F = function(e) {
        var t = function(t) {
            return e[t];
          },
          n = "(?:" + $.keys(e).join("|") + ")",
          r = RegExp(n),
          i = RegExp(n, "g");
        return function(e) {
          return e = null == e ? "" : "" + e, r.test(e) ? e.replace(i, t) : e;
        };
      };
    $.escape = F(L), $.unescape = F(U), $.result = function(e, t, n) {
      var r = null == e ? void 0 : e[t];
      return void 0 === r && (r = n), $.isFunction(r) ? r.call(e) : r;
    };
    var j = 0;
    $.uniqueId = function(e) {
      var t = ++j + "";
      return e ? e + t : t;
    }, $.templateSettings = {
      evaluate: /<%([\s\S]+?)%>/g,
      interpolate: /<%=([\s\S]+?)%>/g,
      escape: /<%-([\s\S]+?)%>/g
    };
    var H = /(.)^/,
      B = {
        "'": "'",
        "\\": "\\",
        "\r": "r",
        "\n": "n",
        "\u2028": "u2028",
        "\u2029": "u2029"
      },
      z = /\\|'|\r|\n|\u2028|\u2029/g,
      q = function(e) {
        return "\\" + B[e];
      };
    $.template = function(e, t, n) {
      !t && n && (t = n), t = $.defaults({}, t, $.templateSettings);
      var r = RegExp([(t.escape || H).source, (t.interpolate || H).source, (t.evaluate || H).source].join(
          "|") + "|$", "g"),
        i = 0,
        o = "__p+='";
      e.replace(r, function(t, n, r, a, s) {
          return o += e.slice(i, s).replace(z, q), i = s + t.length, n ? o += "'+\n((__t=(" + n +
            "))==null?'':_.escape(__t))+\n'" : r ? o += "'+\n((__t=(" + r + "))==null?'':__t)+\n'" :
            a && (o += "';\n" + a + "\n__p+='"), t;
        }), o += "';\n", t.variable || (o = "with(obj||{}){\n" + o + "}\n"), o =
        "var __t,__p='',__j=Array.prototype.join,print=function(){__p+=__j.call(arguments,'');};\n" + o +
        "return __p;\n";
      try {
        var a = new Function(t.variable || "obj", "_", o);
      } catch (e) {
        throw e.source = o, e;
      }
      var s = function(e) {
          return a.call(this, e, $);
        },
        c = t.variable || "obj";
      return s.source = "function(" + c + "){\n" + o + "}", s;
    }, $.chain = function(e) {
      var t = $(e);
      return t._chain = !0, t;
    };
    var G = function(e, t) {
      return e._chain ? $(t).chain() : t;
    };
    $.mixin = function(e) {
      $.each($.functions(e), function(t) {
        var n = $[t] = e[t];
        $.prototype[t] = function() {
          var e = [this._wrapped];
          return h.apply(e, arguments), G(this, n.apply($, e));
        };
      });
    }, $.mixin($), $.each(["pop", "push", "reverse", "shift", "sort", "splice", "unshift"], function(e) {
      var t = l[e];
      $.prototype[e] = function() {
        var n = this._wrapped;
        return t.apply(n, arguments), "shift" !== e && "splice" !== e || 0 !== n.length || delete n[
          0], G(this, n);
      };
    }), $.each(["concat", "join", "slice"], function(e) {
      var t = l[e];
      $.prototype[e] = function() {
        return G(this, t.apply(this._wrapped, arguments));
      };
    }), $.prototype.value = function() {
      return this._wrapped;
    }, $.prototype.valueOf = $.prototype.toJSON = $.prototype.value, $.prototype.toString = function() {
      return "" + this._wrapped;
    }, r = [], i = function() {
      return $;
    }.apply(exports, r), !(void 0 !== i && (module.exports = i));
  }).call(this);
}
