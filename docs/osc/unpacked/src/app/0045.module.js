// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 45
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      var t = e += "",
        n = t.indexOf(":");
      return n >= 0 && "xmlns" !== (t = e.slice(0, n)) && (e = e.slice(n + 1)), qe.hasOwnProperty(t) ? {
        space: qe[t],
        local: e
      } : e;
    }

    function n(e) {
      return function() {
        var t = this.ownerDocument,
          n = this.namespaceURI;
        return n === Ke && t.documentElement.namespaceURI === Ke ? t.createElement(e) : t.createElementNS(
          n, e);
      };
    }

    function i(e) {
      return function() {
        return this.ownerDocument.createElementNS(e.space, e.local);
      };
    }

    function o(e) {
      var o = t(e);
      return (o.local ? i : n)(o);
    }

    function r() {}

    function a(e) {
      return null == e ? r : function() {
        return this.querySelector(e);
      };
    }

    function l(e) {
      "function" != typeof e && (e = a(e));
      for (var t = this._groups, n = t.length, i = new Array(n), o = 0; o < n; ++o)
        for (var r, l, s = t[o], d = s.length, c = i[o] = new Array(d), u = 0; u < d; ++u)(r = s[u]) && (l =
          e.call(r, r.__data__, u, s)) && ("__data__" in r && (l.__data__ = r.__data__), c[u] = l);
      return new Le(i, this._parents);
    }

    function s() {
      return [];
    }

    function d(e) {
      return null == e ? s : function() {
        return this.querySelectorAll(e);
      };
    }

    function c(e) {
      "function" != typeof e && (e = d(e));
      for (var t = this._groups, n = t.length, i = [], o = [], r = 0; r < n; ++r)
        for (var a, l = t[r], s = l.length, c = 0; c < s; ++c)(a = l[c]) && (i.push(e.call(a, a.__data__, c,
          l)), o.push(a));
      return new Le(i, o);
    }

    function u(e) {
      return function() {
        return this.matches(e);
      };
    }

    function f(e) {
      "function" != typeof e && (e = u(e));
      for (var t = this._groups, n = t.length, i = new Array(n), o = 0; o < n; ++o)
        for (var r, a = t[o], l = a.length, s = i[o] = [], d = 0; d < l; ++d)(r = a[d]) && e.call(r, r
          .__data__, d, a) && s.push(r);
      return new Le(i, this._parents);
    }

    function m(e) {
      return new Array(e.length);
    }

    function g() {
      return new Le(this._enter || this._groups.map(m), this._parents);
    }

    function p(e, t) {
      this.ownerDocument = e.ownerDocument, this.namespaceURI = e.namespaceURI, this._next = null, this
        ._parent = e, this.__data__ = t;
    }

    function h(e) {
      return function() {
        return e;
      };
    }

    function b(e, t, n, i, o, r) {
      for (var a, l = 0, s = t.length, d = r.length; l < d; ++l)(a = t[l]) ? (a.__data__ = r[l], i[l] = a) :
        n[l] = new p(e, r[l]);
      for (; l < s; ++l)(a = t[l]) && (o[l] = a);
    }

    function x(e, t, n, i, o, r, a) {
      var l,
        s,
        d,
        c = {},
        u = t.length,
        f = r.length,
        m = new Array(u);
      for (l = 0; l < u; ++l)(s = t[l]) && (m[l] = d = Xe + a.call(s, s.__data__, l, t), d in c ? o[l] = s :
        c[d] = s);
      for (l = 0; l < f; ++l) d = Xe + a.call(e, r[l], l, r), (s = c[d]) ? (i[l] = s, s.__data__ = r[l], c[
        d] = null) : n[l] = new p(e, r[l]);
      for (l = 0; l < u; ++l)(s = t[l]) && c[m[l]] === s && (o[l] = s);
    }

    function v(e, t) {
      if (!e) return m = new Array(this.size()), d = -1, this.each(function(e) {
        m[++d] = e;
      }), m;
      var n = t ? x : b,
        i = this._parents,
        o = this._groups;
      "function" != typeof e && (e = h(e));
      for (var r = o.length, a = new Array(r), l = new Array(r), s = new Array(r), d = 0; d < r; ++d) {
        var c = i[d],
          u = o[d],
          f = u.length,
          m = e.call(c, c && c.__data__, d, i),
          g = m.length,
          p = l[d] = new Array(g),
          v = a[d] = new Array(g),
          y = s[d] = new Array(f);
        n(c, u, p, v, y, m, t);
        for (var w, S, E = 0, k = 0; E < g; ++E)
          if (w = p[E]) {
            for (E >= k && (k = E + 1); !(S = v[k]) && ++k < g;);
            w._next = S || null;
          }
      }
      return a = new Le(a, i), a._enter = l, a._exit = s, a;
    }

    function y() {
      return new Le(this._exit || this._groups.map(m), this._parents);
    }

    function w(e, t, n) {
      var i = this.enter(),
        o = this,
        r = this.exit();
      return i = "function" == typeof e ? e(i) : i.append(e + ""), null != t && (o = t(o)), null == n ? r
        .remove() : n(r), i && o ? i.merge(o).order() : o;
    }

    function S(e) {
      for (var t = this._groups, n = e._groups, i = t.length, o = n.length, r = Math.min(i, o), a =
          new Array(i), l = 0; l < r; ++l)
        for (var s, d = t[l], c = n[l], u = d.length, f = a[l] = new Array(u), m = 0; m < u; ++m)(s = d[
          m] || c[m]) && (f[m] = s);
      for (; l < i; ++l) a[l] = t[l];
      return new Le(a, this._parents);
    }

    function E() {
      for (var e = this._groups, t = -1, n = e.length; ++t < n;)
        for (var i, o = e[t], r = o.length - 1, a = o[r]; --r >= 0;)(i = o[r]) && (a && 4 ^ i
          .compareDocumentPosition(a) && a.parentNode.insertBefore(i, a), a = i);
      return this;
    }

    function k(e) {
      function t(t, n) {
        return t && n ? e(t.__data__, n.__data__) : !t - !n;
      }
      e || (e = _);
      for (var n = this._groups, i = n.length, o = new Array(i), r = 0; r < i; ++r) {
        for (var a, l = n[r], s = l.length, d = o[r] = new Array(s), c = 0; c < s; ++c)(a = l[c]) && (d[c] =
          a);
        d.sort(t);
      }
      return new Le(o, this._parents).order();
    }

    function _(e, t) {
      return e < t ? -1 : e > t ? 1 : e >= t ? 0 : NaN;
    }

    function T() {
      var e = arguments[0];
      return arguments[0] = this, e.apply(null, arguments), this;
    }

    function C() {
      var e = new Array(this.size()),
        t = -1;
      return this.each(function() {
        e[++t] = this;
      }), e;
    }

    function O() {
      for (var e = this._groups, t = 0, n = e.length; t < n; ++t)
        for (var i = e[t], o = 0, r = i.length; o < r; ++o) {
          var a = i[o];
          if (a) return a;
        }
      return null;
    }

    function A() {
      var e = 0;
      return this.each(function() {
        ++e;
      }), e;
    }

    function I() {
      return !this.node();
    }

    function M(e) {
      for (var t = this._groups, n = 0, i = t.length; n < i; ++n)
        for (var o, r = t[n], a = 0, l = r.length; a < l; ++a)(o = r[a]) && e.call(o, o.__data__, a, r);
      return this;
    }

    function R(e) {
      return function() {
        this.removeAttribute(e);
      };
    }

    function P(e) {
      return function() {
        this.removeAttributeNS(e.space, e.local);
      };
    }

    function D(e, t) {
      return function() {
        this.setAttribute(e, t);
      };
    }

    function N(e, t) {
      return function() {
        this.setAttributeNS(e.space, e.local, t);
      };
    }

    function L(e, t) {
      return function() {
        var n = t.apply(this, arguments);
        null == n ? this.removeAttribute(e) : this.setAttribute(e, n);
      };
    }

    function F(e, t) {
      return function() {
        var n = t.apply(this, arguments);
        null == n ? this.removeAttributeNS(e.space, e.local) : this.setAttributeNS(e.space, e.local, n);
      };
    }

    function U(e, n) {
      var i = t(e);
      if (arguments.length < 2) {
        var o = this.node();
        return i.local ? o.getAttributeNS(i.space, i.local) : o.getAttribute(i);
      }
      return this.each((null == n ? i.local ? P : R : "function" == typeof n ? i.local ? F : L : i.local ?
        N : D)(i, n));
    }

    function z(e) {
      return e.ownerDocument && e.ownerDocument.defaultView || e.document && e || e.defaultView;
    }

    function G(e) {
      return function() {
        this.style.removeProperty(e);
      };
    }

    function V(e, t, n) {
      return function() {
        this.style.setProperty(e, t, n);
      };
    }

    function H(e, t, n) {
      return function() {
        var i = t.apply(this, arguments);
        null == i ? this.style.removeProperty(e) : this.style.setProperty(e, i, n);
      };
    }

    function B(e, t, n) {
      return arguments.length > 1 ? this.each((null == t ? G : "function" == typeof t ? H : V)(e, t, null ==
        n ? "" : n)) : Y(this.node(), e);
    }

    function Y(e, t) {
      return e.style.getPropertyValue(t) || z(e).getComputedStyle(e, null).getPropertyValue(t);
    }

    function $(e) {
      return function() {
        delete this[e];
      };
    }

    function W(e, t) {
      return function() {
        this[e] = t;
      };
    }

    function j(e, t) {
      return function() {
        var n = t.apply(this, arguments);
        null == n ? delete this[e] : this[e] = n;
      };
    }

    function K(e, t) {
      return arguments.length > 1 ? this.each((null == t ? $ : "function" == typeof t ? j : W)(e, t)) : this
        .node()[e];
    }

    function q(e) {
      return e.trim().split(/^|\s+/);
    }

    function X(e) {
      return e.classList || new Z(e);
    }

    function Z(e) {
      this._node = e, this._names = q(e.getAttribute("class") || "");
    }

    function Q(e, t) {
      for (var n = X(e), i = -1, o = t.length; ++i < o;) n.add(t[i]);
    }

    function J(e, t) {
      for (var n = X(e), i = -1, o = t.length; ++i < o;) n.remove(t[i]);
    }

    function ee(e) {
      return function() {
        Q(this, e);
      };
    }

    function te(e) {
      return function() {
        J(this, e);
      };
    }

    function ne(e, t) {
      return function() {
        (t.apply(this, arguments) ? Q : J)(this, e);
      };
    }

    function ie(e, t) {
      var n = q(e + "");
      if (arguments.length < 2) {
        for (var i = X(this.node()), o = -1, r = n.length; ++o < r;)
          if (!i.contains(n[o])) return !1;
        return !0;
      }
      return this.each(("function" == typeof t ? ne : t ? ee : te)(n, t));
    }

    function oe() {
      this.textContent = "";
    }

    function re(e) {
      return function() {
        this.textContent = e;
      };
    }

    function ae(e) {
      return function() {
        var t = e.apply(this, arguments);
        this.textContent = null == t ? "" : t;
      };
    }

    function le(e) {
      return arguments.length ? this.each(null == e ? oe : ("function" == typeof e ? ae : re)(e)) : this
        .node().textContent;
    }

    function se() {
      this.innerHTML = "";
    }

    function de(e) {
      return function() {
        this.innerHTML = e;
      };
    }

    function ce(e) {
      return function() {
        var t = e.apply(this, arguments);
        this.innerHTML = null == t ? "" : t;
      };
    }

    function ue(e) {
      return arguments.length ? this.each(null == e ? se : ("function" == typeof e ? ce : de)(e)) : this
        .node().innerHTML;
    }

    function fe() {
      this.nextSibling && this.parentNode.appendChild(this);
    }

    function me() {
      return this.each(fe);
    }

    function ge() {
      this.previousSibling && this.parentNode.insertBefore(this, this.parentNode.firstChild);
    }

    function pe() {
      return this.each(ge);
    }

    function he(e) {
      var t = "function" == typeof e ? e : o(e);
      return this.select(function() {
        return this.appendChild(t.apply(this, arguments));
      });
    }

    function be() {
      return null;
    }

    function xe(e, t) {
      var n = "function" == typeof e ? e : o(e),
        i = null == t ? be : "function" == typeof t ? t : a(t);
      return this.select(function() {
        return this.insertBefore(n.apply(this, arguments), i.apply(this, arguments) || null);
      });
    }

    function ve() {
      var e = this.parentNode;
      e && e.removeChild(this);
    }

    function ye() {
      return this.each(ve);
    }

    function we() {
      var e = this.cloneNode(!1),
        t = this.parentNode;
      return t ? t.insertBefore(e, this.nextSibling) : e;
    }

    function Se() {
      var e = this.cloneNode(!0),
        t = this.parentNode;
      return t ? t.insertBefore(e, this.nextSibling) : e;
    }

    function Ee(e) {
      return this.select(e ? Se : we);
    }

    function ke(e) {
      return arguments.length ? this.property("__data__", e) : this.node().__data__;
    }

    function _e(e, t, n) {
      return e = Te(e, t, n),
        function(t) {
          var n = t.relatedTarget;
          n && (n === this || 8 & n.compareDocumentPosition(this)) || e.call(this, t);
        };
    }

    function Te(t, n, i) {
      return function(o) {
        var r = e.event;
        e.event = o;
        try {
          t.call(this, this.__data__, n, i);
        } finally {
          e.event = r;
        }
      };
    }

    function Ce(e) {
      return e.trim().split(/^|\s+/).map(function(e) {
        var t = "",
          n = e.indexOf(".");
        return n >= 0 && (t = e.slice(n + 1), e = e.slice(0, n)), {
          type: e,
          name: t
        };
      });
    }

    function Oe(e) {
      return function() {
        var t = this.__on;
        if (t) {
          for (var n, i = 0, o = -1, r = t.length; i < r; ++i) n = t[i], e.type && n.type !== e.type || n
            .name !== e.name ? t[++o] = n : this.removeEventListener(n.type, n.listener, n.capture);
          ++o ? t.length = o : delete this.__on;
        }
      };
    }

    function Ae(e, t, n) {
      var i = Ze.hasOwnProperty(e.type) ? _e : Te;
      return function(o, r, a) {
        var l,
          s = this.__on,
          d = i(t, r, a);
        if (s)
          for (var c = 0, u = s.length; c < u; ++c)
            if ((l = s[c]).type === e.type && l.name === e.name) return this.removeEventListener(l.type, l
                .listener, l.capture), this.addEventListener(l.type, l.listener = d, l.capture = n),
              void(l.value = t);
        this.addEventListener(e.type, d, n), l = {
          type: e.type,
          name: e.name,
          value: t,
          listener: d,
          capture: n
        }, s ? s.push(l) : this.__on = [l];
      };
    }

    function Ie(e, t, n) {
      var i,
        o,
        r = Ce(e + ""),
        a = r.length;
      {
        if (!(arguments.length < 2)) {
          for (l = t ? Ae : Oe, null == n && (n = !1), i = 0; i < a; ++i) this.each(l(r[i], t, n));
          return this;
        }
        var l = this.node().__on;
        if (l)
          for (var s, d = 0, c = l.length; d < c; ++d)
            for (i = 0, s = l[d]; i < a; ++i)
              if ((o = r[i]).type === s.type && o.name === s.name) return s.value;
      }
    }

    function Me(t, n, i, o) {
      var r = e.event;
      t.sourceEvent = e.event, e.event = t;
      try {
        return n.apply(i, o);
      } finally {
        e.event = r;
      }
    }

    function Re(e, t, n) {
      var i = z(e),
        o = i.CustomEvent;
      "function" == typeof o ? o = new o(t, n) : (o = i.document.createEvent("Event"), n ? (o.initEvent(t, n
        .bubbles, n.cancelable), o.detail = n.detail) : o.initEvent(t, !1, !1)), e.dispatchEvent(o);
    }

    function Pe(e, t) {
      return function() {
        return Re(this, e, t);
      };
    }

    function De(e, t) {
      return function() {
        return Re(this, e, t.apply(this, arguments));
      };
    }

    function Ne(e, t) {
      return this.each(("function" == typeof t ? De : Pe)(e, t));
    }

    function Le(e, t) {
      this._groups = e, this._parents = t;
    }

    function Fe() {
      return new Le([
        [document.documentElement]
      ], Je);
    }

    function Ue(e) {
      return "string" == typeof e ? new Le([
        [document.querySelector(e)]
      ], [document.documentElement]) : new Le([
        [e]
      ], Je);
    }

    function ze(e) {
      return Ue(o(e).call(document.documentElement));
    }

    function Ge() {
      return new Ve();
    }

    function Ve() {
      this._ = "@" + (++et).toString(36);
    }

    function He() {
      for (var t, n = e.event; t = n.sourceEvent;) n = t;
      return n;
    }

    function Be(e, t) {
      var n = e.ownerSVGElement || e;
      if (n.createSVGPoint) {
        var i = n.createSVGPoint();
        return i.x = t.clientX, i.y = t.clientY, i = i.matrixTransform(e.getScreenCTM().inverse()), [i.x, i
          .y
        ];
      }
      var o = e.getBoundingClientRect();
      return [t.clientX - o.left - e.clientLeft, t.clientY - o.top - e.clientTop];
    }

    function Ye(e) {
      var t = He();
      return t.changedTouches && (t = t.changedTouches[0]), Be(e, t);
    }

    function $e(e) {
      return "string" == typeof e ? new Le([document.querySelectorAll(e)], [document.documentElement]) :
        new Le([null == e ? [] : e], Je);
    }

    function We(e, t, n) {
      arguments.length < 3 && (n = t, t = He().changedTouches);
      for (var i, o = 0, r = t ? t.length : 0; o < r; ++o)
        if ((i = t[o]).identifier === n) return Be(e, i);
      return null;
    }

    function je(e, t) {
      null == t && (t = He().touches);
      for (var n = 0, i = t ? t.length : 0, o = new Array(i); n < i; ++n) o[n] = Be(e, t[n]);
      return o;
    }
    var Ke = "http://www.w3.org/1999/xhtml",
      qe = {
        svg: "http://www.w3.org/2000/svg",
        xhtml: Ke,
        xlink: "http://www.w3.org/1999/xlink",
        xml: "http://www.w3.org/XML/1998/namespace",
        xmlns: "http://www.w3.org/2000/xmlns/"
      };
    p.prototype = {
      constructor: p,
      appendChild: function(e) {
        return this._parent.insertBefore(e, this._next);
      },
      insertBefore: function(e, t) {
        return this._parent.insertBefore(e, t);
      },
      querySelector: function(e) {
        return this._parent.querySelector(e);
      },
      querySelectorAll: function(e) {
        return this._parent.querySelectorAll(e);
      }
    };
    var Xe = "$";
    Z.prototype = {
      add: function(e) {
        var t = this._names.indexOf(e);
        t < 0 && (this._names.push(e), this._node.setAttribute("class", this._names.join(" ")));
      },
      remove: function(e) {
        var t = this._names.indexOf(e);
        t >= 0 && (this._names.splice(t, 1), this._node.setAttribute("class", this._names.join(" ")));
      },
      contains: function(e) {
        return this._names.indexOf(e) >= 0;
      }
    };
    var Ze = {};
    if (e.event = null, "undefined" != typeof document) {
      var Qe = document.documentElement;
      "onmouseenter" in Qe || (Ze = {
        mouseenter: "mouseover",
        mouseleave: "mouseout"
      });
    }
    var Je = [null];
    Le.prototype = Fe.prototype = {
      constructor: Le,
      select: l,
      selectAll: c,
      filter: f,
      data: v,
      enter: g,
      exit: y,
      join: w,
      merge: S,
      order: E,
      sort: k,
      call: T,
      nodes: C,
      node: O,
      size: A,
      empty: I,
      each: M,
      attr: U,
      style: B,
      property: K,
      classed: ie,
      text: le,
      html: ue,
      raise: me,
      lower: pe,
      append: he,
      insert: xe,
      remove: ye,
      clone: Ee,
      datum: ke,
      on: Ie,
      dispatch: Ne
    };
    var et = 0;
    Ve.prototype = Ge.prototype = {
        constructor: Ve,
        get: function(e) {
          for (var t = this._; !(t in e);)
            if (!(e = e.parentNode)) return;
          return e[t];
        },
        set: function(e, t) {
          return e[this._] = t;
        },
        remove: function(e) {
          return this._ in e && delete e[this._];
        },
        toString: function() {
          return this._;
        }
      }, e.clientPoint = Be, e.create = ze, e.creator = o, e.customEvent = Me, e.local = Ge, e.matcher = u,
      e.mouse = Ye, e.namespace = t, e.namespaces = qe, e.select = Ue, e.selectAll = $e, e.selection = Fe, e
      .selector = a, e.selectorAll = d, e.style = Y, e.touch = We, e.touches = je, e.window = z, Object
      .defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
