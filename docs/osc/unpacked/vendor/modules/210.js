// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 210
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var r = n(4),
    i = n(10),
    o = n(6),
    a = n(8),
    s = n(91),
    c = n(190).KEY,
    u = n(15),
    l = n(55),
    d = n(38),
    f = n(40),
    h = n(5),
    p = n(59),
    m = n(58),
    v = n(181),
    g = n(185),
    y = n(7),
    b = n(12),
    E = n(39),
    _ = n(13),
    $ = n(57),
    w = n(37),
    T = n(84),
    C = n(85),
    x = n(194),
    S = n(53),
    A = n(9),
    M = n(17),
    k = x.f,
    N = A.f,
    I = C.f,
    O = r.Symbol,
    D = r.JSON,
    R = D && D.stringify,
    P = "prototype",
    L = h("_hidden"),
    U = h("toPrimitive"),
    F = {}.propertyIsEnumerable,
    j = l("symbol-registry"),
    H = l("symbols"),
    B = l("op-symbols"),
    z = Object[P],
    q = "function" == typeof O && !!S.f,
    G = r.QObject,
    V = !G || !G[P] || !G[P].findChild,
    W = o && u(function() {
      return 7 != T(N({}, "a", {
        get: function() {
          return N(this, "a", {
            value: 7
          }).a
        }
      })).a
    }) ? function(e, t, n) {
      var r = k(z, t);
      r && delete z[t], N(e, t, n), r && e !== z && N(z, t, r)
    } : N,
    Y = function(e) {
      var t = H[e] = T(O[P]);
      return t._k = e, t
    },
    K = q && "symbol" == typeof O.iterator ? function(e) {
      return "symbol" == typeof e
    } : function(e) {
      return e instanceof O
    },
    X = function(e, t, n) {
      return e === z && X(B, t, n), y(e), t = $(t, !0), y(n), i(H, t) ? (n.enumerable ? (i(e, L) && e[L][t] && (e[L][
        t] = !1), n = T(n, {
        enumerable: w(0, !1)
      })) : (i(e, L) || N(e, L, w(1, {})), e[L][t] = !0), W(e, t, n)) : N(e, t, n)
    },
    Q = function(e, t) {
      y(e);
      for (var n, r = v(t = _(t)), i = 0, o = r.length; o > i;) X(e, n = r[i++], t[n]);
      return e
    },
    J = function(e, t) {
      return void 0 === t ? T(e) : Q(T(e), t)
    },
    Z = function(e) {
      var t = F.call(this, e = $(e, !0));
      return !(this === z && i(H, e) && !i(B, e)) && (!(t || !i(this, e) || !i(H, e) || i(this, L) && this[L][e]) || t)
    },
    ee = function(e, t) {
      if (e = _(e), t = $(t, !0), e !== z || !i(H, t) || i(B, t)) {
        var n = k(e, t);
        return !n || !i(H, t) || i(e, L) && e[L][t] || (n.enumerable = !0),
          n
      }
    },
    te = function(e) {
      for (var t, n = I(_(e)), r = [], o = 0; n.length > o;) i(H, t = n[o++]) || t == L || t == c || r.push(t);
      return r
    },
    ne = function(e) {
      for (var t, n = e === z, r = I(n ? B : _(e)), o = [], a = 0; r.length > a;) !i(H, t = r[a++]) || n && !i(z, t) ||
        o.push(H[t]);
      return o
    };
  q || (O = function() {
    if (this instanceof O) throw TypeError("Symbol is not a constructor!");
    var e = f(arguments.length > 0 ? arguments[0] : void 0),
      t = function(n) {
        this === z && t.call(B, n), i(this, L) && i(this[L], e) && (this[L][e] = !1), W(this, e, w(1, n))
      };
    return o && V && W(z, e, {
      configurable: !0,
      set: t
    }), Y(e)
  }, s(O[P], "toString", function() {
    return this._k
  }), x.f = ee, A.f = X, n(86).f = C.f = te, n(29).f = Z, S.f = ne, o && !n(28) && s(z, "propertyIsEnumerable", Z, !
    0), p.f = function(e) {
    return Y(h(e))
  }), a(a.G + a.W + a.F * !q, {
    Symbol: O
  });
  for (var re =
      "hasInstance,isConcatSpreadable,iterator,match,replace,search,species,split,toPrimitive,toStringTag,unscopables"
      .split(","), ie = 0; re.length > ie;) h(re[ie++]);
  for (var oe = M(h.store), ae = 0; oe.length > ae;) m(oe[ae++]);
  a(a.S + a.F * !q, "Symbol", {
    for: function(e) {
      return i(j, e += "") ? j[e] : j[e] = O(e)
    },
    keyFor: function(e) {
      if (!K(e)) throw TypeError(e + " is not a symbol!");
      for (var t in j)
        if (j[t] === e) return t
    },
    useSetter: function() {
      V = !0
    },
    useSimple: function() {
      V = !1
    }
  }), a(a.S + a.F * !q, "Object", {
    create: J,
    defineProperty: X,
    defineProperties: Q,
    getOwnPropertyDescriptor: ee,
    getOwnPropertyNames: te,
    getOwnPropertySymbols: ne
  });
  var se = u(function() {
    S.f(1)
  });
  a(a.S + a.F * se, "Object", {
    getOwnPropertySymbols: function(e) {
      return S.f(E(e))
    }
  }), D && a(a.S + a.F * (!q || u(function() {
    var e = O();
    return "[null]" != R([e]) || "{}" != R({
      a: e
    }) || "{}" != R(Object(e))
  })), "JSON", {
    stringify: function(e) {
      for (var t, n, r = [e], i = 1; arguments.length > i;) r.push(arguments[i++]);
      if (n = t = r[1], (b(t) || void 0 !== e) && !K(e)) return g(t) || (t = function(e, t) {
        if ("function" == typeof n && (t = n.call(this, e, t)), !K(t)) return t
      }), r[1] = t, R.apply(D, r)
    }
  }), O[P][U] || n(11)(O[P], U, O[P].valueOf), d(O, "Symbol"), d(Math, "Math", !0), d(r.JSON, "JSON", !0)
}
