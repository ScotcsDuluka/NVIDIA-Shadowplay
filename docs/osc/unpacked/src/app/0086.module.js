// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 86
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(45), require(38), require(85), require(39), require(59), require(128));
  }(this, function(e, t, n, i, o, r, a) {
    "use strict";

    function l(e, t, n, i, o, r) {
      var a = e.__transition;
      if (a) {
        if (n in a) return;
      } else e.__transition = {};
      u(e, n, {
        name: t,
        index: i,
        group: o,
        on: xe,
        tween: ve,
        time: r.time,
        delay: r.delay,
        duration: r.duration,
        ease: r.ease,
        timer: null,
        state: ye
      });
    }

    function s(e, t) {
      var n = c(e, t);
      if (n.state > ye) throw new Error("too late; already scheduled");
      return n;
    }

    function d(e, t) {
      var n = c(e, t);
      if (n.state > Ee) throw new Error("too late; already running");
      return n;
    }

    function c(e, t) {
      var n = e.__transition;
      if (!n || !(n = n[t])) throw new Error("transition not found");
      return n;
    }

    function u(e, t, n) {
      function o(e) {
        n.state = we, n.timer.restart(r, n.delay, n.time), n.delay <= e && r(e - n.delay);
      }

      function r(o) {
        var c, u, f, m;
        if (n.state !== we) return l();
        for (c in d)
          if (m = d[c], m.name === n.name) {
            if (m.state === Ee) return i.timeout(r);
            m.state === ke ? (m.state = Te, m.timer.stop(), m.on.call("interrupt", e, e.__data__, m.index, m
              .group), delete d[c]) : +c < t && (m.state = Te, m.timer.stop(), m.on.call("cancel", e, e
              .__data__, m.index, m.group), delete d[c]);
          }
        if (i.timeout(function() {
            n.state === Ee && (n.state = ke, n.timer.restart(a, n.delay, n.time), a(o));
          }), n.state = Se, n.on.call("start", e, e.__data__, n.index, n.group), n.state === Se) {
          for (n.state = Ee, s = new Array(f = n.tween.length), c = 0, u = -1; c < f; ++c)(m = n.tween[c]
            .value.call(e, e.__data__, n.index, n.group)) && (s[++u] = m);
          s.length = u + 1;
        }
      }

      function a(t) {
        for (var i = t < n.duration ? n.ease.call(null, t / n.duration) : (n.timer.restart(l), n.state = _e,
            1), o = -1, r = s.length; ++o < r;) s[o].call(e, i);
        n.state === _e && (n.on.call("end", e, e.__data__, n.index, n.group), l());
      }

      function l() {
        n.state = Te, n.timer.stop(), delete d[t];
        for (var i in d) return;
        delete e.__transition;
      }
      var s,
        d = e.__transition;
      d[t] = n, n.timer = i.timer(o, 0, n.time);
    }

    function f(e, t) {
      var n,
        i,
        o,
        r = e.__transition,
        a = !0;
      if (r) {
        t = null == t ? null : t + "";
        for (o in r)(n = r[o]).name === t ? (i = n.state > Se && n.state < _e, n.state = Te, n.timer.stop(),
          n.on.call(i ? "interrupt" : "cancel", e, e.__data__, n.index, n.group), delete r[o]) : a = !1;
        a && delete e.__transition;
      }
    }

    function m(e) {
      return this.each(function() {
        f(this, e);
      });
    }

    function g(e, t) {
      var n, i;
      return function() {
        var o = d(this, e),
          r = o.tween;
        if (r !== n) {
          i = n = r;
          for (var a = 0, l = i.length; a < l; ++a)
            if (i[a].name === t) {
              i = i.slice(), i.splice(a, 1);
              break;
            }
        }
        o.tween = i;
      };
    }

    function p(e, t, n) {
      var i, o;
      if ("function" != typeof n) throw new Error();
      return function() {
        var r = d(this, e),
          a = r.tween;
        if (a !== i) {
          o = (i = a).slice();
          for (var l = {
              name: t,
              value: n
            }, s = 0, c = o.length; s < c; ++s)
            if (o[s].name === t) {
              o[s] = l;
              break;
            }
          s === c && o.push(l);
        }
        r.tween = o;
      };
    }

    function h(e, t) {
      var n = this._id;
      if (e += "", arguments.length < 2) {
        for (var i, o = c(this.node(), n).tween, r = 0, a = o.length; r < a; ++r)
          if ((i = o[r]).name === e) return i.value;
        return null;
      }
      return this.each((null == t ? g : p)(n, e, t));
    }

    function b(e, t, n) {
      var i = e._id;
      return e.each(function() {
          var e = d(this, i);
          (e.value || (e.value = {}))[t] = n.apply(this, arguments);
        }),
        function(e) {
          return c(e, i).value[t];
        };
    }

    function x(e, t) {
      var n;
      return ("number" == typeof t ? o.interpolateNumber : t instanceof r.color ? o.interpolateRgb : (n = r
        .color(t)) ? (t = n, o.interpolateRgb) : o.interpolateString)(e, t);
    }

    function v(e) {
      return function() {
        this.removeAttribute(e);
      };
    }

    function y(e) {
      return function() {
        this.removeAttributeNS(e.space, e.local);
      };
    }

    function w(e, t, n) {
      var i,
        o,
        r = n + "";
      return function() {
        var a = this.getAttribute(e);
        return a === r ? null : a === i ? o : o = t(i = a, n);
      };
    }

    function S(e, t, n) {
      var i,
        o,
        r = n + "";
      return function() {
        var a = this.getAttributeNS(e.space, e.local);
        return a === r ? null : a === i ? o : o = t(i = a, n);
      };
    }

    function E(e, t, n) {
      var i, o, r;
      return function() {
        var a,
          l,
          s = n(this);
        return null == s ? void this.removeAttribute(e) : (a = this.getAttribute(e), l = s + "", a === l ?
          null : a === i && l === o ? r : (o = l, r = t(i = a, s)));
      };
    }

    function k(e, t, n) {
      var i, o, r;
      return function() {
        var a,
          l,
          s = n(this);
        return null == s ? void this.removeAttributeNS(e.space, e.local) : (a = this.getAttributeNS(e
          .space, e.local), l = s + "", a === l ? null : a === i && l === o ? r : (o = l, r = t(i = a,
          s)));
      };
    }

    function _(e, n) {
      var i = t.namespace(e),
        r = "transform" === i ? o.interpolateTransformSvg : x;
      return this.attrTween(e, "function" == typeof n ? (i.local ? k : E)(i, r, b(this, "attr." + e, n)) :
        null == n ? (i.local ? y : v)(i) : (i.local ? S : w)(i, r, n));
    }

    function T(e, t) {
      return function(n) {
        this.setAttribute(e, t.call(this, n));
      };
    }

    function C(e, t) {
      return function(n) {
        this.setAttributeNS(e.space, e.local, t.call(this, n));
      };
    }

    function O(e, t) {
      function n() {
        var n = t.apply(this, arguments);
        return n !== o && (i = (o = n) && C(e, n)), i;
      }
      var i, o;
      return n._value = t, n;
    }

    function A(e, t) {
      function n() {
        var n = t.apply(this, arguments);
        return n !== o && (i = (o = n) && T(e, n)), i;
      }
      var i, o;
      return n._value = t, n;
    }

    function I(e, n) {
      var i = "attr." + e;
      if (arguments.length < 2) return (i = this.tween(i)) && i._value;
      if (null == n) return this.tween(i, null);
      if ("function" != typeof n) throw new Error();
      var o = t.namespace(e);
      return this.tween(i, (o.local ? O : A)(o, n));
    }

    function M(e, t) {
      return function() {
        s(this, e).delay = +t.apply(this, arguments);
      };
    }

    function R(e, t) {
      return t = +t,
        function() {
          s(this, e).delay = t;
        };
    }

    function P(e) {
      var t = this._id;
      return arguments.length ? this.each(("function" == typeof e ? M : R)(t, e)) : c(this.node(), t).delay;
    }

    function D(e, t) {
      return function() {
        d(this, e).duration = +t.apply(this, arguments);
      };
    }

    function N(e, t) {
      return t = +t,
        function() {
          d(this, e).duration = t;
        };
    }

    function L(e) {
      var t = this._id;
      return arguments.length ? this.each(("function" == typeof e ? D : N)(t, e)) : c(this.node(), t)
        .duration;
    }

    function F(e, t) {
      if ("function" != typeof t) throw new Error();
      return function() {
        d(this, e).ease = t;
      };
    }

    function U(e) {
      var t = this._id;
      return arguments.length ? this.each(F(t, e)) : c(this.node(), t).ease;
    }

    function z(e) {
      "function" != typeof e && (e = t.matcher(e));
      for (var n = this._groups, i = n.length, o = new Array(i), r = 0; r < i; ++r)
        for (var a, l = n[r], s = l.length, d = o[r] = [], c = 0; c < s; ++c)(a = l[c]) && e.call(a, a
          .__data__, c, l) && d.push(a);
      return new fe(o, this._parents, this._name, this._id);
    }

    function G(e) {
      if (e._id !== this._id) throw new Error();
      for (var t = this._groups, n = e._groups, i = t.length, o = n.length, r = Math.min(i, o), a =
          new Array(i), l = 0; l < r; ++l)
        for (var s, d = t[l], c = n[l], u = d.length, f = a[l] = new Array(u), m = 0; m < u; ++m)(s = d[
          m] || c[m]) && (f[m] = s);
      for (; l < i; ++l) a[l] = t[l];
      return new fe(a, this._parents, this._name, this._id);
    }

    function V(e) {
      return (e + "").trim().split(/^|\s+/).every(function(e) {
        var t = e.indexOf(".");
        return t >= 0 && (e = e.slice(0, t)), !e || "start" === e;
      });
    }

    function H(e, t, n) {
      var i,
        o,
        r = V(t) ? s : d;
      return function() {
        var a = r(this, e),
          l = a.on;
        l !== i && (o = (i = l).copy()).on(t, n), a.on = o;
      };
    }

    function B(e, t) {
      var n = this._id;
      return arguments.length < 2 ? c(this.node(), n).on.on(e) : this.each(H(n, e, t));
    }

    function Y(e) {
      return function() {
        var t = this.parentNode;
        for (var n in this.__transition)
          if (+n !== e) return;
        t && t.removeChild(this);
      };
    }

    function $() {
      return this.on("end.remove", Y(this._id));
    }

    function W(e) {
      var n = this._name,
        i = this._id;
      "function" != typeof e && (e = t.selector(e));
      for (var o = this._groups, r = o.length, a = new Array(r), s = 0; s < r; ++s)
        for (var d, u, f = o[s], m = f.length, g = a[s] = new Array(m), p = 0; p < m; ++p)(d = f[p]) && (u =
          e.call(d, d.__data__, p, f)) && ("__data__" in d && (u.__data__ = d.__data__), g[p] = u, l(g[p],
          n, i, p, g, c(d, i)));
      return new fe(a, this._parents, n, i);
    }

    function j(e) {
      var n = this._name,
        i = this._id;
      "function" != typeof e && (e = t.selectorAll(e));
      for (var o = this._groups, r = o.length, a = [], s = [], d = 0; d < r; ++d)
        for (var u, f = o[d], m = f.length, g = 0; g < m; ++g)
          if (u = f[g]) {
            for (var p, h = e.call(u, u.__data__, g, f), b = c(u, i), x = 0, v = h.length; x < v; ++x)(p =
              h[x]) && l(p, n, i, x, h, b);
            a.push(h), s.push(u);
          }
      return new fe(a, s, n, i);
    }

    function K() {
      return new Ce(this._groups, this._parents);
    }

    function q(e, n) {
      var i, o, r;
      return function() {
        var a = t.style(this, e),
          l = (this.style.removeProperty(e), t.style(this, e));
        return a === l ? null : a === i && l === o ? r : r = n(i = a, o = l);
      };
    }

    function X(e) {
      return function() {
        this.style.removeProperty(e);
      };
    }

    function Z(e, n, i) {
      var o,
        r,
        a = i + "";
      return function() {
        var l = t.style(this, e);
        return l === a ? null : l === o ? r : r = n(o = l, i);
      };
    }

    function Q(e, n, i) {
      var o, r, a;
      return function() {
        var l = t.style(this, e),
          s = i(this),
          d = s + "";
        return null == s && (this.style.removeProperty(e), d = s = t.style(this, e)), l === d ? null :
          l === o && d === r ? a : (r = d, a = n(o = l, s));
      };
    }

    function J(e, t) {
      var n,
        i,
        o,
        r,
        a = "style." + t,
        l = "end." + a;
      return function() {
        var s = d(this, e),
          c = s.on,
          u = null == s.value[a] ? r || (r = X(t)) : void 0;
        c === n && o === u || (i = (n = c).copy()).on(l, o = u), s.on = i;
      };
    }

    function ee(e, t, n) {
      var i = "transform" == (e += "") ? o.interpolateTransformCss : x;
      return null == t ? this.styleTween(e, q(e, i)).on("end.style." + e, X(e)) : "function" == typeof t ?
        this.styleTween(e, Q(e, i, b(this, "style." + e, t))).each(J(this._id, e)) : this.styleTween(e, Z(e,
          i, t), n).on("end.style." + e, null);
    }

    function te(e, t, n) {
      return function(i) {
        this.style.setProperty(e, t.call(this, i), n);
      };
    }

    function ne(e, t, n) {
      function i() {
        var i = t.apply(this, arguments);
        return i !== r && (o = (r = i) && te(e, i, n)), o;
      }
      var o, r;
      return i._value = t, i;
    }

    function ie(e, t, n) {
      var i = "style." + (e += "");
      if (arguments.length < 2) return (i = this.tween(i)) && i._value;
      if (null == t) return this.tween(i, null);
      if ("function" != typeof t) throw new Error();
      return this.tween(i, ne(e, t, null == n ? "" : n));
    }

    function oe(e) {
      return function() {
        this.textContent = e;
      };
    }

    function re(e) {
      return function() {
        var t = e(this);
        this.textContent = null == t ? "" : t;
      };
    }

    function ae(e) {
      return this.tween("text", "function" == typeof e ? re(b(this, "text", e)) : oe(null == e ? "" : e +
        ""));
    }

    function le(e) {
      return function(t) {
        this.textContent = e.call(this, t);
      };
    }

    function se(e) {
      function t() {
        var t = e.apply(this, arguments);
        return t !== i && (n = (i = t) && le(t)), n;
      }
      var n, i;
      return t._value = e, t;
    }

    function de(e) {
      var t = "text";
      if (arguments.length < 1) return (t = this.tween(t)) && t._value;
      if (null == e) return this.tween(t, null);
      if ("function" != typeof e) throw new Error();
      return this.tween(t, se(e));
    }

    function ce() {
      for (var e = this._name, t = this._id, n = ge(), i = this._groups, o = i.length, r = 0; r < o; ++r)
        for (var a, s = i[r], d = s.length, u = 0; u < d; ++u)
          if (a = s[u]) {
            var f = c(a, t);
            l(a, e, n, u, s, {
              time: f.time + f.delay + f.duration,
              delay: 0,
              duration: f.duration,
              ease: f.ease
            });
          }
      return new fe(i, this._parents, e, n);
    }

    function ue() {
      var e,
        t,
        n = this,
        i = n._id,
        o = n.size();
      return new Promise(function(r, a) {
        var l = {
            value: a
          },
          s = {
            value: function() {
              0 === --o && r();
            }
          };
        n.each(function() {
          var n = d(this, i),
            o = n.on;
          o !== e && (t = (e = o).copy(), t._.cancel.push(l), t._.interrupt.push(l), t._.end.push(
            s)), n.on = t;
        });
      });
    }

    function fe(e, t, n, i) {
      this._groups = e, this._parents = t, this._name = n, this._id = i;
    }

    function me(e) {
      return t.selection().transition(e);
    }

    function ge() {
      return ++Oe;
    }

    function pe(e, t) {
      for (var n; !(n = e.__transition) || !(n = n[t]);)
        if (!(e = e.parentNode)) return Ie.time = i.now(), Ie;
      return n;
    }

    function he(e) {
      var t, n;
      e instanceof fe ? (t = e._id, e = e._name) : (t = ge(), (n = Ie).time = i.now(), e = null == e ?
        null : e + "");
      for (var o = this._groups, r = o.length, a = 0; a < r; ++a)
        for (var s, d = o[a], c = d.length, u = 0; u < c; ++u)(s = d[u]) && l(s, e, t, u, d, n || pe(s, t));
      return new fe(o, this._parents, e, t);
    }

    function be(e, t) {
      var n,
        i,
        o = e.__transition;
      if (o) {
        t = null == t ? null : t + "";
        for (i in o)
          if ((n = o[i]).state > we && n.name === t) return new fe([
            [e]
          ], Me, t, +i);
      }
      return null;
    }
    var xe = n.dispatch("start", "end", "cancel", "interrupt"),
      ve = [],
      ye = 0,
      we = 1,
      Se = 2,
      Ee = 3,
      ke = 4,
      _e = 5,
      Te = 6,
      Ce = t.selection.prototype.constructor,
      Oe = 0,
      Ae = t.selection.prototype;
    fe.prototype = me.prototype = {
      constructor: fe,
      select: W,
      selectAll: j,
      filter: z,
      merge: G,
      selection: K,
      transition: ce,
      call: Ae.call,
      nodes: Ae.nodes,
      node: Ae.node,
      size: Ae.size,
      empty: Ae.empty,
      each: Ae.each,
      on: B,
      attr: _,
      attrTween: I,
      style: ee,
      styleTween: ie,
      text: ae,
      textTween: de,
      remove: $,
      tween: h,
      delay: P,
      duration: L,
      ease: U,
      end: ue
    };
    var Ie = {
      time: null,
      delay: 0,
      duration: 250,
      ease: a.easeCubicInOut
    };
    t.selection.prototype.interrupt = m, t.selection.prototype.transition = he;
    var Me = [null];
    e.active = be, e.interrupt = f, e.transition = me, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
