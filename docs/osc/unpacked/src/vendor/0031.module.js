// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 31
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
      return n >= 0 && "xmlns" !== (t = e.slice(0, n)) && (e = e.slice(n + 1)), Ke.hasOwnProperty(t) ? {
        space: Ke[t],
        local: e
      } : e;
    }

    function n(e) {
      return function() {
        var t = this.ownerDocument,
          n = this.namespaceURI;
        return n === Ye && t.documentElement.namespaceURI === Ye ? t.createElement(e) : t.createElementNS(
          n, e);
      };
    }

    function r(e) {
      return function() {
        return this.ownerDocument.createElementNS(e.space, e.local);
      };
    }

    function i(e) {
      var i = t(e);
      return (i.local ? r : n)(i);
    }

    function o() {}

    function a(e) {
      return null == e ? o : function() {
        return this.querySelector(e);
      };
    }

    function s(e) {
      "function" != typeof e && (e = a(e));
      for (var t = this._groups, n = t.length, r = new Array(n), i = 0; i < n; ++i)
        for (var o, s, c = t[i], u = c.length, l = r[i] = new Array(u), d = 0; d < u; ++d)(o = c[d]) && (s =
          e.call(o, o.__data__, d, c)) && ("__data__" in o && (s.__data__ = o.__data__), l[d] = s);
      return new Pe(r, this._parents);
    }

    function c() {
      return [];
    }

    function u(e) {
      return null == e ? c : function() {
        return this.querySelectorAll(e);
      };
    }

    function l(e) {
      "function" != typeof e && (e = u(e));
      for (var t = this._groups, n = t.length, r = [], i = [], o = 0; o < n; ++o)
        for (var a, s = t[o], c = s.length, l = 0; l < c; ++l)(a = s[l]) && (r.push(e.call(a, a.__data__, l,
          s)), i.push(a));
      return new Pe(r, i);
    }

    function d(e) {
      return function() {
        return this.matches(e);
      };
    }

    function f(e) {
      "function" != typeof e && (e = d(e));
      for (var t = this._groups, n = t.length, r = new Array(n), i = 0; i < n; ++i)
        for (var o, a = t[i], s = a.length, c = r[i] = [], u = 0; u < s; ++u)(o = a[u]) && e.call(o, o
          .__data__, u, a) && c.push(o);
      return new Pe(r, this._parents);
    }

    function h(e) {
      return new Array(e.length);
    }

    function p() {
      return new Pe(this._enter || this._groups.map(h), this._parents);
    }

    function m(e, t) {
      this.ownerDocument = e.ownerDocument, this.namespaceURI = e.namespaceURI, this._next = null, this
        ._parent = e, this.__data__ = t;
    }

    function v(e) {
      return function() {
        return e;
      };
    }

    function g(e, t, n, r, i, o) {
      for (var a, s = 0, c = t.length, u = o.length; s < u; ++s)(a = t[s]) ? (a.__data__ = o[s], r[s] = a) :
        n[s] = new m(e, o[s]);
      for (; s < c; ++s)(a = t[s]) && (i[s] = a);
    }

    function y(e, t, n, r, i, o, a) {
      var s,
        c,
        u,
        l = {},
        d = t.length,
        f = o.length,
        h = new Array(d);
      for (s = 0; s < d; ++s)(c = t[s]) && (h[s] = u = Xe + a.call(c, c.__data__, s, t), u in l ? i[s] = c :
        l[u] = c);
      for (s = 0; s < f; ++s) u = Xe + a.call(e, o[s], s, o), (c = l[u]) ? (r[s] = c, c.__data__ = o[s], l[
        u] = null) : n[s] = new m(e, o[s]);
      for (s = 0; s < d; ++s)(c = t[s]) && l[h[s]] === c && (i[s] = c);
    }

    function b(e, t) {
      if (!e) return h = new Array(this.size()), u = -1, this.each(function(e) {
        h[++u] = e;
      }), h;
      var n = t ? y : g,
        r = this._parents,
        i = this._groups;
      "function" != typeof e && (e = v(e));
      for (var o = i.length, a = new Array(o), s = new Array(o), c = new Array(o), u = 0; u < o; ++u) {
        var l = r[u],
          d = i[u],
          f = d.length,
          h = e.call(l, l && l.__data__, u, r),
          p = h.length,
          m = s[u] = new Array(p),
          b = a[u] = new Array(p),
          E = c[u] = new Array(f);
        n(l, d, m, b, E, h, t);
        for (var _, $, w = 0, T = 0; w < p; ++w)
          if (_ = m[w]) {
            for (w >= T && (T = w + 1); !($ = b[T]) && ++T < p;);
            _._next = $ || null;
          }
      }
      return a = new Pe(a, r), a._enter = s, a._exit = c, a;
    }

    function E() {
      return new Pe(this._exit || this._groups.map(h), this._parents);
    }

    function _(e, t, n) {
      var r = this.enter(),
        i = this,
        o = this.exit();
      return r = "function" == typeof e ? e(r) : r.append(e + ""), null != t && (i = t(i)), null == n ? o
        .remove() : n(o), r && i ? r.merge(i).order() : i;
    }

    function $(e) {
      for (var t = this._groups, n = e._groups, r = t.length, i = n.length, o = Math.min(r, i), a =
          new Array(r), s = 0; s < o; ++s)
        for (var c, u = t[s], l = n[s], d = u.length, f = a[s] = new Array(d), h = 0; h < d; ++h)(c = u[
          h] || l[h]) && (f[h] = c);
      for (; s < r; ++s) a[s] = t[s];
      return new Pe(a, this._parents);
    }

    function w() {
      for (var e = this._groups, t = -1, n = e.length; ++t < n;)
        for (var r, i = e[t], o = i.length - 1, a = i[o]; --o >= 0;)(r = i[o]) && (a && 4 ^ r
          .compareDocumentPosition(a) && a.parentNode.insertBefore(r, a), a = r);
      return this;
    }

    function T(e) {
      function t(t, n) {
        return t && n ? e(t.__data__, n.__data__) : !t - !n;
      }
      e || (e = C);
      for (var n = this._groups, r = n.length, i = new Array(r), o = 0; o < r; ++o) {
        for (var a, s = n[o], c = s.length, u = i[o] = new Array(c), l = 0; l < c; ++l)(a = s[l]) && (u[l] =
          a);
        u.sort(t);
      }
      return new Pe(i, this._parents).order();
    }

    function C(e, t) {
      return e < t ? -1 : e > t ? 1 : e >= t ? 0 : NaN;
    }

    function x() {
      var e = arguments[0];
      return arguments[0] = this, e.apply(null, arguments), this;
    }

    function S() {
      var e = new Array(this.size()),
        t = -1;
      return this.each(function() {
        e[++t] = this;
      }), e;
    }

    function A() {
      for (var e = this._groups, t = 0, n = e.length; t < n; ++t)
        for (var r = e[t], i = 0, o = r.length; i < o; ++i) {
          var a = r[i];
          if (a) return a;
        }
      return null;
    }

    function M() {
      var e = 0;
      return this.each(function() {
        ++e;
      }), e;
    }

    function k() {
      return !this.node();
    }

    function N(e) {
      for (var t = this._groups, n = 0, r = t.length; n < r; ++n)
        for (var i, o = t[n], a = 0, s = o.length; a < s; ++a)(i = o[a]) && e.call(i, i.__data__, a, o);
      return this;
    }

    function I(e) {
      return function() {
        this.removeAttribute(e);
      };
    }

    function O(e) {
      return function() {
        this.removeAttributeNS(e.space, e.local);
      };
    }

    function D(e, t) {
      return function() {
        this.setAttribute(e, t);
      };
    }

    function R(e, t) {
      return function() {
        this.setAttributeNS(e.space, e.local, t);
      };
    }

    function P(e, t) {
      return function() {
        var n = t.apply(this, arguments);
        null == n ? this.removeAttribute(e) : this.setAttribute(e, n);
      };
    }

    function L(e, t) {
      return function() {
        var n = t.apply(this, arguments);
        null == n ? this.removeAttributeNS(e.space, e.local) : this.setAttributeNS(e.space, e.local, n);
      };
    }

    function U(e, n) {
      var r = t(e);
      if (arguments.length < 2) {
        var i = this.node();
        return r.local ? i.getAttributeNS(r.space, r.local) : i.getAttribute(r);
      }
      return this.each((null == n ? r.local ? O : I : "function" == typeof n ? r.local ? L : P : r.local ?
        R : D)(r, n));
    }

    function F(e) {
      return e.ownerDocument && e.ownerDocument.defaultView || e.document && e || e.defaultView;
    }

    function j(e) {
      return function() {
        this.style.removeProperty(e);
      };
    }

    function H(e, t, n) {
      return function() {
        this.style.setProperty(e, t, n);
      };
    }

    function B(e, t, n) {
      return function() {
        var r = t.apply(this, arguments);
        null == r ? this.style.removeProperty(e) : this.style.setProperty(e, r, n);
      };
    }

    function z(e, t, n) {
      return arguments.length > 1 ? this.each((null == t ? j : "function" == typeof t ? B : H)(e, t, null ==
        n ? "" : n)) : q(this.node(), e);
    }

    function q(e, t) {
      return e.style.getPropertyValue(t) || F(e).getComputedStyle(e, null).getPropertyValue(t);
    }

    function G(e) {
      return function() {
        delete this[e];
      };
    }

    function V(e, t) {
      return function() {
        this[e] = t;
      };
    }

    function W(e, t) {
      return function() {
        var n = t.apply(this, arguments);
        null == n ? delete this[e] : this[e] = n;
      };
    }

    function Y(e, t) {
      return arguments.length > 1 ? this.each((null == t ? G : "function" == typeof t ? W : V)(e, t)) : this
        .node()[e];
    }

    function K(e) {
      return e.trim().split(/^|\s+/);
    }

    function X(e) {
      return e.classList || new Q(e);
    }

    function Q(e) {
      this._node = e, this._names = K(e.getAttribute("class") || "");
    }

    function J(e, t) {
      for (var n = X(e), r = -1, i = t.length; ++r < i;) n.add(t[r]);
    }

    function Z(e, t) {
      for (var n = X(e), r = -1, i = t.length; ++r < i;) n.remove(t[r]);
    }

    function ee(e) {
      return function() {
        J(this, e);
      };
    }

    function te(e) {
      return function() {
        Z(this, e);
      };
    }

    function ne(e, t) {
      return function() {
        (t.apply(this, arguments) ? J : Z)(this, e);
      };
    }

    function re(e, t) {
      var n = K(e + "");
      if (arguments.length < 2) {
        for (var r = X(this.node()), i = -1, o = n.length; ++i < o;)
          if (!r.contains(n[i])) return !1;
        return !0;
      }
      return this.each(("function" == typeof t ? ne : t ? ee : te)(n, t));
    }

    function ie() {
      this.textContent = "";
    }

    function oe(e) {
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

    function se(e) {
      return arguments.length ? this.each(null == e ? ie : ("function" == typeof e ? ae : oe)(e)) : this
        .node().textContent;
    }

    function ce() {
      this.innerHTML = "";
    }

    function ue(e) {
      return function() {
        this.innerHTML = e;
      };
    }

    function le(e) {
      return function() {
        var t = e.apply(this, arguments);
        this.innerHTML = null == t ? "" : t;
      };
    }

    function de(e) {
      return arguments.length ? this.each(null == e ? ce : ("function" == typeof e ? le : ue)(e)) : this
        .node().innerHTML;
    }

    function fe() {
      this.nextSibling && this.parentNode.appendChild(this);
    }

    function he() {
      return this.each(fe);
    }

    function pe() {
      this.previousSibling && this.parentNode.insertBefore(this, this.parentNode.firstChild);
    }

    function me() {
      return this.each(pe);
    }

    function ve(e) {
      var t = "function" == typeof e ? e : i(e);
      return this.select(function() {
        return this.appendChild(t.apply(this, arguments));
      });
    }

    function ge() {
      return null;
    }

    function ye(e, t) {
      var n = "function" == typeof e ? e : i(e),
        r = null == t ? ge : "function" == typeof t ? t : a(t);
      return this.select(function() {
        return this.insertBefore(n.apply(this, arguments), r.apply(this, arguments) || null);
      });
    }

    function be() {
      var e = this.parentNode;
      e && e.removeChild(this);
    }

    function Ee() {
      return this.each(be);
    }

    function _e() {
      var e = this.cloneNode(!1),
        t = this.parentNode;
      return t ? t.insertBefore(e, this.nextSibling) : e;
    }

    function $e() {
      var e = this.cloneNode(!0),
        t = this.parentNode;
      return t ? t.insertBefore(e, this.nextSibling) : e;
    }

    function we(e) {
      return this.select(e ? $e : _e);
    }

    function Te(e) {
      return arguments.length ? this.property("__data__", e) : this.node().__data__;
    }

    function Ce(e, t, n) {
      return e = xe(e, t, n),
        function(t) {
          var n = t.relatedTarget;
          n && (n === this || 8 & n.compareDocumentPosition(this)) || e.call(this, t);
        };
    }

    function xe(t, n, r) {
      return function(i) {
        var o = e.event;
        e.event = i;
        try {
          t.call(this, this.__data__, n, r);
        } finally {
          e.event = o;
        }
      };
    }

    function Se(e) {
      return e.trim().split(/^|\s+/).map(function(e) {
        var t = "",
          n = e.indexOf(".");
        return n >= 0 && (t = e.slice(n + 1), e = e.slice(0, n)), {
          type: e,
          name: t
        };
      });
    }

    function Ae(e) {
      return function() {
        var t = this.__on;
        if (t) {
          for (var n, r = 0, i = -1, o = t.length; r < o; ++r) n = t[r], e.type && n.type !== e.type || n
            .name !== e.name ? t[++i] = n : this.removeEventListener(n.type, n.listener, n.capture);
          ++i ? t.length = i : delete this.__on;
        }
      };
    }

    function Me(e, t, n) {
      var r = Qe.hasOwnProperty(e.type) ? Ce : xe;
      return function(i, o, a) {
        var s,
          c = this.__on,
          u = r(t, o, a);
        if (c)
          for (var l = 0, d = c.length; l < d; ++l)
            if ((s = c[l]).type === e.type && s.name === e.name) return this.removeEventListener(s.type, s
                .listener, s.capture), this.addEventListener(s.type, s.listener = u, s.capture = n),
              void(s.value = t);
        this.addEventListener(e.type, u, n), s = {
          type: e.type,
          name: e.name,
          value: t,
          listener: u,
          capture: n
        }, c ? c.push(s) : this.__on = [s];
      };
    }

    function ke(e, t, n) {
      var r,
        i,
        o = Se(e + ""),
        a = o.length;
      {
        if (!(arguments.length < 2)) {
          for (s = t ? Me : Ae, null == n && (n = !1), r = 0; r < a; ++r) this.each(s(o[r], t, n));
          return this;
        }
        var s = this.node().__on;
        if (s)
          for (var c, u = 0, l = s.length; u < l; ++u)
            for (r = 0, c = s[u]; r < a; ++r)
              if ((i = o[r]).type === c.type && i.name === c.name) return c.value;
      }
    }

    function Ne(t, n, r, i) {
      var o = e.event;
      t.sourceEvent = e.event, e.event = t;
      try {
        return n.apply(r, i);
      } finally {
        e.event = o;
      }
    }

    function Ie(e, t, n) {
      var r = F(e),
        i = r.CustomEvent;
      "function" == typeof i ? i = new i(t, n) : (i = r.document.createEvent("Event"), n ? (i.initEvent(t, n
        .bubbles, n.cancelable), i.detail = n.detail) : i.initEvent(t, !1, !1)), e.dispatchEvent(i);
    }

    function Oe(e, t) {
      return function() {
        return Ie(this, e, t);
      };
    }

    function De(e, t) {
      return function() {
        return Ie(this, e, t.apply(this, arguments));
      };
    }

    function Re(e, t) {
      return this.each(("function" == typeof t ? De : Oe)(e, t));
    }

    function Pe(e, t) {
      this._groups = e, this._parents = t;
    }

    function Le() {
      return new Pe([
        [document.documentElement]
      ], Ze);
    }

    function Ue(e) {
      return "string" == typeof e ? new Pe([
        [document.querySelector(e)]
      ], [document.documentElement]) : new Pe([
        [e]
      ], Ze);
    }

    function Fe(e) {
      return Ue(i(e).call(document.documentElement));
    }

    function je() {
      return new He();
    }

    function He() {
      this._ = "@" + (++et).toString(36);
    }

    function Be() {
      for (var t, n = e.event; t = n.sourceEvent;) n = t;
      return n;
    }

    function ze(e, t) {
      var n = e.ownerSVGElement || e;
      if (n.createSVGPoint) {
        var r = n.createSVGPoint();
        return r.x = t.clientX, r.y = t.clientY, r = r.matrixTransform(e.getScreenCTM().inverse()), [r.x, r
          .y
        ];
      }
      var i = e.getBoundingClientRect();
      return [t.clientX - i.left - e.clientLeft, t.clientY - i.top - e.clientTop];
    }

    function qe(e) {
      var t = Be();
      return t.changedTouches && (t = t.changedTouches[0]), ze(e, t);
    }

    function Ge(e) {
      return "string" == typeof e ? new Pe([document.querySelectorAll(e)], [document.documentElement]) :
        new Pe([null == e ? [] : e], Ze);
    }

    function Ve(e, t, n) {
      arguments.length < 3 && (n = t, t = Be().changedTouches);
      for (var r, i = 0, o = t ? t.length : 0; i < o; ++i)
        if ((r = t[i]).identifier === n) return ze(e, r);
      return null;
    }

    function We(e, t) {
      null == t && (t = Be().touches);
      for (var n = 0, r = t ? t.length : 0, i = new Array(r); n < r; ++n) i[n] = ze(e, t[n]);
      return i;
    }
    var Ye = "http://www.w3.org/1999/xhtml",
      Ke = {
        svg: "http://www.w3.org/2000/svg",
        xhtml: Ye,
        xlink: "http://www.w3.org/1999/xlink",
        xml: "http://www.w3.org/XML/1998/namespace",
        xmlns: "http://www.w3.org/2000/xmlns/"
      };
    m.prototype = {
      constructor: m,
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
    Q.prototype = {
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
    var Qe = {};
    if (e.event = null, "undefined" != typeof document) {
      var Je = document.documentElement;
      "onmouseenter" in Je || (Qe = {
        mouseenter: "mouseover",
        mouseleave: "mouseout"
      });
    }
    var Ze = [null];
    Pe.prototype = Le.prototype = {
      constructor: Pe,
      select: s,
      selectAll: l,
      filter: f,
      data: b,
      enter: p,
      exit: E,
      join: _,
      merge: $,
      order: w,
      sort: T,
      call: x,
      nodes: S,
      node: A,
      size: M,
      empty: k,
      each: N,
      attr: U,
      style: z,
      property: Y,
      classed: re,
      text: se,
      html: de,
      raise: he,
      lower: me,
      append: ve,
      insert: ye,
      remove: Ee,
      clone: we,
      datum: Te,
      on: ke,
      dispatch: Re
    };
    var et = 0;
    He.prototype = je.prototype = {
        constructor: He,
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
      }, e.clientPoint = ze, e.create = Fe, e.creator = i, e.customEvent = Ne, e.local = je, e.matcher = d,
      e.mouse = qe, e.namespace = t, e.namespaces = Ke, e.select = Ue, e.selectAll = Ge, e.selection = Le, e
      .selector = a, e.selectorAll = u, e.style = q, e.touch = Ve, e.touches = We, e.window = F, Object
      .defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
