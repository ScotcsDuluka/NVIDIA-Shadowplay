// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 67
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, r) {
    r(t, n(31), n(20), n(66), n(21), n(44), n(100))
  }(this, function(e, t, n, r, i, o, a) {
    "use strict";

    function s(e, t, n, r, i, o) {
      var a = e.__transition;
      if (a) {
        if (n in a) return
      } else e.__transition = {};
      d(e, n, {
        name: t,
        index: r,
        group: i,
        on: ye,
        tween: be,
        time: o.time,
        delay: o.delay,
        duration: o.duration,
        ease: o.ease,
        timer: null,
        state: Ee
      })
    }

    function c(e, t) {
      var n = l(e, t);
      if (n.state > Ee) throw new Error("too late; already scheduled");
      return n
    }

    function u(e, t) {
      var n = l(e, t);
      if (n.state > we) throw new Error("too late; already running");
      return n
    }

    function l(e, t) {
      var n = e.__transition;
      if (!n || !(n = n[t])) throw new Error("transition not found");
      return n
    }

    function d(e, t, n) {
      function i(e) {
        n.state = _e, n.timer.restart(o, n.delay, n.time), n.delay <= e && o(e - n.delay)
      }

      function o(i) {
        var l, d, f, h;
        if (n.state !== _e) return s();
        for (l in u)
          if (h = u[l], h.name === n.name) {
            if (h.state === we) return r.timeout(o);
            h.state === Te ? (h.state = xe, h.timer.stop(), h.on.call("interrupt", e, e.__data__, h.index, h.group),
              delete u[l]) : +l < t && (h.state = xe, h.timer.stop(), h.on.call("cancel", e, e.__data__, h.index, h
              .group), delete u[l])
          } if (r.timeout(function() {
            n.state === we && (n.state = Te, n.timer.restart(a, n.delay, n.time), a(i))
          }), n.state = $e, n.on.call("start", e, e.__data__, n.index, n.group), n.state === $e) {
          for (n.state = we, c = new Array(f = n.tween.length), l = 0, d = -1; l < f; ++l)(h = n.tween[l].value.call(
            e, e.__data__, n.index, n.group)) && (c[++d] = h);
          c.length = d + 1
        }
      }

      function a(t) {
        for (var r = t < n.duration ? n.ease.call(null, t / n.duration) : (n.timer.restart(s), n.state = Ce, 1), i = -
            1, o = c.length; ++i < o;) c[i].call(e, r);
        n.state === Ce && (n.on.call("end", e, e.__data__, n.index, n.group), s())
      }

      function s() {
        n.state = xe, n.timer.stop(), delete u[t];
        for (var r in u) return;
        delete e.__transition
      }
      var c, u = e.__transition;
      u[t] = n, n.timer = r.timer(i, 0, n.time)
    }

    function f(e, t) {
      var n, r, i, o = e.__transition,
        a = !0;
      if (o) {
        t = null == t ? null : t + "";
        for (i in o)(n = o[i]).name === t ? (r = n.state > $e && n.state < Ce, n.state = xe, n.timer.stop(), n.on
          .call(r ? "interrupt" : "cancel", e, e.__data__, n.index, n.group), delete o[i]) : a = !1;
        a && delete e.__transition
      }
    }

    function h(e) {
      return this.each(function() {
        f(this, e)
      })
    }

    function p(e, t) {
      var n, r;
      return function() {
        var i = u(this, e),
          o = i.tween;
        if (o !== n) {
          r = n = o;
          for (var a = 0, s = r.length; a < s; ++a)
            if (r[a].name === t) {
              r = r.slice(), r.splice(a, 1);
              break
            }
        }
        i.tween = r
      }
    }

    function m(e, t, n) {
      var r, i;
      if ("function" != typeof n) throw new Error;
      return function() {
        var o = u(this, e),
          a = o.tween;
        if (a !== r) {
          i = (r = a).slice();
          for (var s = {
              name: t,
              value: n
            }, c = 0, l = i.length; c < l; ++c)
            if (i[c].name === t) {
              i[c] = s;
              break
            } c === l && i.push(s)
        }
        o.tween = i
      }
    }

    function v(e, t) {
      var n = this._id;
      if (e += "", arguments.length < 2) {
        for (var r, i = l(this.node(), n).tween, o = 0, a = i.length; o < a; ++o)
          if ((r = i[o]).name === e) return r.value;
        return null
      }
      return this.each((null == t ? p : m)(n, e, t))
    }

    function g(e, t, n) {
      var r = e._id;
      return e.each(function() {
          var e = u(this, r);
          (e.value || (e.value = {}))[t] = n.apply(this, arguments)
        }),
        function(e) {
          return l(e, r).value[t]
        }
    }

    function y(e, t) {
      var n;
      return ("number" == typeof t ? i.interpolateNumber : t instanceof o.color ? i.interpolateRgb : (n = o.color(
        t)) ? (t = n, i.interpolateRgb) : i.interpolateString)(e, t)
    }

    function b(e) {
      return function() {
        this.removeAttribute(e)
      }
    }

    function E(e) {
      return function() {
        this.removeAttributeNS(e.space, e.local)
      }
    }

    function _(e, t, n) {
      var r, i, o = n + "";
      return function() {
        var a = this.getAttribute(e);
        return a === o ? null : a === r ? i : i = t(r = a, n)
      }
    }

    function $(e, t, n) {
      var r, i, o = n + "";
      return function() {
        var a = this.getAttributeNS(e.space, e.local);
        return a === o ? null : a === r ? i : i = t(r = a, n)
      }
    }

    function w(e, t, n) {
      var r, i, o;
      return function() {
        var a, s, c = n(this);
        return null == c ? void this.removeAttribute(e) : (a = this.getAttribute(e), s = c + "", a === s ? null :
          a === r && s === i ? o : (i = s, o = t(r = a, c)))
      }
    }

    function T(e, t, n) {
      var r, i, o;
      return function() {
        var a, s, c = n(this);
        return null == c ? void this.removeAttributeNS(e.space, e.local) : (a = this.getAttributeNS(e.space, e
          .local), s = c + "", a === s ? null : a === r && s === i ? o : (i = s, o = t(r = a, c)))
      }
    }

    function C(e, n) {
      var r = t.namespace(e),
        o = "transform" === r ? i.interpolateTransformSvg : y;
      return this.attrTween(e, "function" == typeof n ? (r.local ? T : w)(r, o, g(this, "attr." + e, n)) : null == n ?
        (r.local ? E : b)(r) : (r.local ? $ : _)(r, o, n))
    }

    function x(e, t) {
      return function(n) {
        this.setAttribute(e, t.call(this, n))
      }
    }

    function S(e, t) {
      return function(n) {
        this.setAttributeNS(e.space, e.local, t.call(this, n))
      }
    }

    function A(e, t) {
      function n() {
        var n = t.apply(this, arguments);
        return n !== i && (r = (i = n) && S(e, n)), r
      }
      var r, i;
      return n._value = t, n
    }

    function M(e, t) {
      function n() {
        var n = t.apply(this, arguments);
        return n !== i && (r = (i = n) && x(e, n)), r
      }
      var r, i;
      return n._value = t, n
    }

    function k(e, n) {
      var r = "attr." + e;
      if (arguments.length < 2) return (r = this.tween(r)) && r._value;
      if (null == n) return this.tween(r, null);
      if ("function" != typeof n) throw new Error;
      var i = t.namespace(e);
      return this.tween(r, (i.local ? A : M)(i, n))
    }

    function N(e, t) {
      return function() {
        c(this, e).delay = +t.apply(this, arguments)
      }
    }

    function I(e, t) {
      return t = +t,
        function() {
          c(this, e).delay = t
        }
    }

    function O(e) {
      var t = this._id;
      return arguments.length ? this.each(("function" == typeof e ? N : I)(t, e)) : l(this.node(), t).delay
    }

    function D(e, t) {
      return function() {
        u(this, e).duration = +t.apply(this, arguments)
      }
    }

    function R(e, t) {
      return t = +t,
        function() {
          u(this, e).duration = t
        }
    }

    function P(e) {
      var t = this._id;
      return arguments.length ? this.each(("function" == typeof e ? D : R)(t, e)) : l(this.node(), t).duration
    }

    function L(e, t) {
      if ("function" != typeof t) throw new Error;
      return function() {
        u(this, e).ease = t
      }
    }

    function U(e) {
      var t = this._id;
      return arguments.length ? this.each(L(t, e)) : l(this.node(), t).ease
    }

    function F(e) {
      "function" != typeof e && (e = t.matcher(e));
      for (var n = this._groups, r = n.length, i = new Array(r), o = 0; o < r; ++o)
        for (var a, s = n[o], c = s.length, u = i[o] = [], l = 0; l < c; ++l)(a = s[l]) && e.call(a, a.__data__, l,
          s) && u.push(a);
      return new fe(i, this._parents, this._name, this._id)
    }

    function j(e) {
      if (e._id !== this._id) throw new Error;
      for (var t = this._groups, n = e._groups, r = t.length, i = n.length, o = Math.min(r, i), a = new Array(r), s =
          0; s < o; ++s)
        for (var c, u = t[s], l = n[s], d = u.length, f = a[s] = new Array(d), h = 0; h < d; ++h)(c = u[h] || l[h]) &&
          (f[h] = c);
      for (; s < r; ++s) a[s] = t[s];
      return new fe(a, this._parents, this._name, this._id)
    }

    function H(e) {
      return (e + "").trim().split(/^|\s+/).every(function(e) {
        var t = e.indexOf(".");
        return t >= 0 && (e = e.slice(0, t)), !e || "start" === e
      })
    }

    function B(e, t, n) {
      var r, i, o = H(t) ? c : u;
      return function() {
        var a = o(this, e),
          s = a.on;
        s !== r && (i = (r = s).copy()).on(t, n), a.on = i
      }
    }

    function z(e, t) {
      var n = this._id;
      return arguments.length < 2 ? l(this.node(), n).on.on(e) : this.each(B(n, e, t))
    }

    function q(e) {
      return function() {
        var t = this.parentNode;
        for (var n in this.__transition)
          if (+n !== e) return;
        t && t.removeChild(this)
      }
    }

    function G() {
      return this.on("end.remove", q(this._id))
    }

    function V(e) {
      var n = this._name,
        r = this._id;
      "function" != typeof e && (e = t.selector(e));
      for (var i = this._groups, o = i.length, a = new Array(o), c = 0; c < o; ++c)
        for (var u, d, f = i[c], h = f.length, p = a[c] = new Array(h), m = 0; m < h; ++m)(u = f[m]) && (d = e.call(u,
          u.__data__, m, f)) && ("__data__" in u && (d.__data__ = u.__data__), p[m] = d, s(p[m], n, r, m, p, l(u,
          r)));
      return new fe(a, this._parents, n, r)
    }

    function W(e) {
      var n = this._name,
        r = this._id;
      "function" != typeof e && (e = t.selectorAll(e));
      for (var i = this._groups, o = i.length, a = [], c = [], u = 0; u < o; ++u)
        for (var d, f = i[u], h = f.length, p = 0; p < h; ++p)
          if (d = f[p]) {
            for (var m, v = e.call(d, d.__data__, p, f), g = l(d, r), y = 0, b = v.length; y < b; ++y)(m = v[y]) && s(
              m, n, r, y, v, g);
            a.push(v), c.push(d)
          } return new fe(a, c, n, r)
    }

    function Y() {
      return new Se(this._groups, this._parents)
    }

    function K(e, n) {
      var r, i, o;
      return function() {
        var a = t.style(this, e),
          s = (this.style.removeProperty(e), t.style(this, e));
        return a === s ? null : a === r && s === i ? o : o = n(r = a, i = s)
      }
    }

    function X(e) {
      return function() {
        this.style.removeProperty(e)
      }
    }

    function Q(e, n, r) {
      var i, o, a = r + "";
      return function() {
        var s = t.style(this, e);
        return s === a ? null : s === i ? o : o = n(i = s, r)
      }
    }

    function J(e, n, r) {
      var i, o, a;
      return function() {
        var s = t.style(this, e),
          c = r(this),
          u = c + "";
        return null == c && (this.style.removeProperty(e), u = c = t.style(this, e)), s === u ? null : s === i &&
          u === o ? a : (o = u, a = n(i = s, c))
      }
    }

    function Z(e, t) {
      var n, r, i, o, a = "style." + t,
        s = "end." + a;
      return function() {
        var c = u(this, e),
          l = c.on,
          d = null == c.value[a] ? o || (o = X(t)) : void 0;
        l === n && i === d || (r = (n = l).copy()).on(s, i = d), c.on = r
      }
    }

    function ee(e, t, n) {
      var r = "transform" == (e += "") ? i.interpolateTransformCss : y;
      return null == t ? this.styleTween(e, K(e, r)).on("end.style." + e, X(e)) : "function" == typeof t ? this
        .styleTween(e, J(e, r, g(this, "style." + e, t))).each(Z(this._id, e)) : this.styleTween(e, Q(e, r, t), n).on(
          "end.style." + e, null)
    }

    function te(e, t, n) {
      return function(r) {
        this.style.setProperty(e, t.call(this, r), n)
      }
    }

    function ne(e, t, n) {
      function r() {
        var r = t.apply(this, arguments);
        return r !== o && (i = (o = r) && te(e, r, n)), i
      }
      var i, o;
      return r._value = t, r
    }

    function re(e, t, n) {
      var r = "style." + (e += "");
      if (arguments.length < 2) return (r = this.tween(r)) && r._value;
      if (null == t) return this.tween(r, null);
      if ("function" != typeof t) throw new Error;
      return this.tween(r, ne(e, t, null == n ? "" : n))
    }

    function ie(e) {
      return function() {
        this.textContent = e
      }
    }

    function oe(e) {
      return function() {
        var t = e(this);
        this.textContent = null == t ? "" : t
      }
    }

    function ae(e) {
      return this.tween("text", "function" == typeof e ? oe(g(this, "text", e)) : ie(null == e ? "" : e + ""))
    }

    function se(e) {
      return function(t) {
        this.textContent = e.call(this, t)
      }
    }

    function ce(e) {
      function t() {
        var t = e.apply(this, arguments);
        return t !== r && (n = (r = t) && se(t)), n
      }
      var n, r;
      return t._value = e, t
    }

    function ue(e) {
      var t = "text";
      if (arguments.length < 1) return (t = this.tween(t)) && t._value;
      if (null == e) return this.tween(t, null);
      if ("function" != typeof e) throw new Error;
      return this.tween(t, ce(e))
    }

    function le() {
      for (var e = this._name, t = this._id, n = pe(), r = this._groups, i = r.length, o = 0; o < i; ++o)
        for (var a, c = r[o], u = c.length, d = 0; d < u; ++d)
          if (a = c[d]) {
            var f = l(a, t);
            s(a, e, n, d, c, {
              time: f.time + f.delay + f.duration,
              delay: 0,
              duration: f.duration,
              ease: f.ease
            })
          } return new fe(r, this._parents, e, n)
    }

    function de() {
      var e, t, n = this,
        r = n._id,
        i = n.size();
      return new Promise(function(o, a) {
        var s = {
            value: a
          },
          c = {
            value: function() {
              0 === --i && o()
            }
          };
        n.each(function() {
          var n = u(this, r),
            i = n.on;
          i !== e && (t = (e = i).copy(), t._.cancel.push(s), t._.interrupt.push(s), t._.end.push(c)), n.on =
            t
        })
      })
    }

    function fe(e, t, n, r) {
      this._groups = e, this._parents = t, this._name = n, this._id = r
    }

    function he(e) {
      return t.selection().transition(e)
    }

    function pe() {
      return ++Ae
    }

    function me(e, t) {
      for (var n; !(n = e.__transition) || !(n = n[t]);)
        if (!(e = e.parentNode)) return ke.time = r.now(), ke;
      return n
    }

    function ve(e) {
      var t, n;
      e instanceof fe ? (t = e._id, e = e._name) : (t = pe(), (n = ke).time = r.now(), e = null == e ? null : e + "");
      for (var i = this._groups, o = i.length, a = 0; a < o; ++a)
        for (var c, u = i[a], l = u.length, d = 0; d < l; ++d)(c = u[d]) && s(c, e, t, d, u, n || me(c, t));
      return new fe(i, this._parents, e, t)
    }

    function ge(e, t) {
      var n, r, i = e.__transition;
      if (i) {
        t = null == t ? null : t + "";
        for (r in i)
          if ((n = i[r]).state > _e && n.name === t) return new fe([
            [e]
          ], Ne, t, +r)
      }
      return null
    }
    var ye = n.dispatch("start", "end", "cancel", "interrupt"),
      be = [],
      Ee = 0,
      _e = 1,
      $e = 2,
      we = 3,
      Te = 4,
      Ce = 5,
      xe = 6,
      Se = t.selection.prototype.constructor,
      Ae = 0,
      Me = t.selection.prototype;
    fe.prototype = he.prototype = {
      constructor: fe,
      select: V,
      selectAll: W,
      filter: F,
      merge: j,
      selection: Y,
      transition: le,
      call: Me.call,
      nodes: Me.nodes,
      node: Me.node,
      size: Me.size,
      empty: Me.empty,
      each: Me.each,
      on: z,
      attr: C,
      attrTween: k,
      style: ee,
      styleTween: re,
      text: ae,
      textTween: ue,
      remove: G,
      tween: v,
      delay: O,
      duration: P,
      ease: U,
      end: de
    };
    var ke = {
      time: null,
      delay: 0,
      duration: 250,
      ease: a.easeCubicInOut
    };
    t.selection.prototype.interrupt = h, t.selection.prototype.transition = ve;
    var Ne = [null];
    e.active = ge, e.interrupt = f, e.transition = he, Object.defineProperty(e, "__esModule", {
      value: !0
    })
  })
}
