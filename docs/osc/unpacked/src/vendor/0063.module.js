// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 63
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(20), require(31));
  }(this, function(e, t, n) {
    "use strict";

    function r() {
      n.event.stopImmediatePropagation();
    }

    function i() {
      n.event.preventDefault(), n.event.stopImmediatePropagation();
    }

    function o(e) {
      var t = e.document.documentElement,
        r = n.select(e).on("dragstart.drag", i, !0);
      "onselectstart" in t ? r.on("selectstart.drag", i, !0) : (t.__noselect = t.style.MozUserSelect, t
        .style.MozUserSelect = "none");
    }

    function a(e, t) {
      var r = e.document.documentElement,
        o = n.select(e).on("dragstart.drag", null);
      t && (o.on("click.drag", i, !0), setTimeout(function() {
        o.on("click.drag", null);
      }, 0)), "onselectstart" in r ? o.on("selectstart.drag", null) : (r.style.MozUserSelect = r
        .__noselect, delete r.__noselect);
    }

    function s(e) {
      return function() {
        return e;
      };
    }

    function c(e, t, n, r, i, o, a, s, c, u) {
      this.target = e, this.type = t, this.subject = n, this.identifier = r, this.active = i, this.x = o,
        this.y = a, this.dx = s, this.dy = c, this._ = u;
    }

    function u() {
      return !n.event.ctrlKey && !n.event.button;
    }

    function l() {
      return this.parentNode;
    }

    function d(e) {
      return null == e ? {
        x: n.event.x,
        y: n.event.y
      } : e;
    }

    function f() {
      return navigator.maxTouchPoints || "ontouchstart" in this;
    }

    function h() {
      function e(e) {
        e.on("mousedown.drag", h).filter(S).on("touchstart.drag", v).on("touchmove.drag", g).on(
          "touchend.drag touchcancel.drag", y).style("touch-action", "none").style(
          "-webkit-tap-highlight-color", "rgba(0,0,0,0)");
      }

      function h() {
        if (!w && T.apply(this, arguments)) {
          var e = b("mouse", C.apply(this, arguments), n.mouse, this, arguments);
          e && (n.select(n.event.view).on("mousemove.drag", p, !0).on("mouseup.drag", m, !0), o(n.event
            .view), r(), $ = !1, E = n.event.clientX, _ = n.event.clientY, e("start"));
        }
      }

      function p() {
        if (i(), !$) {
          var e = n.event.clientX - E,
            t = n.event.clientY - _;
          $ = e * e + t * t > N;
        }
        A.mouse("drag");
      }

      function m() {
        n.select(n.event.view).on("mousemove.drag mouseup.drag", null), a(n.event.view, $), i(), A.mouse(
          "end");
      }

      function v() {
        if (T.apply(this, arguments)) {
          var e,
            t,
            i = n.event.changedTouches,
            o = C.apply(this, arguments),
            a = i.length;
          for (e = 0; e < a; ++e)(t = b(i[e].identifier, o, n.touch, this, arguments)) && (r(), t("start"));
        }
      }

      function g() {
        var e,
          t,
          r = n.event.changedTouches,
          o = r.length;
        for (e = 0; e < o; ++e)(t = A[r[e].identifier]) && (i(), t("drag"));
      }

      function y() {
        var e,
          t,
          i = n.event.changedTouches,
          o = i.length;
        for (w && clearTimeout(w), w = setTimeout(function() {
            w = null;
          }, 500), e = 0; e < o; ++e)(t = A[i[e].identifier]) && (r(), t("end"));
      }

      function b(t, r, i, o, a) {
        var s,
          u,
          l,
          d = i(r, t),
          f = M.copy();
        if (n.customEvent(new c(e, "beforestart", s, t, k, d[0], d[1], 0, 0, f), function() {
            return null != (n.event.subject = s = x.apply(o, a)) && (u = s.x - d[0] || 0, l = s.y - d[
              1] || 0, !0);
          })) return function h(p) {
          var m,
            v = d;
          switch (p) {
            case "start":
              A[t] = h, m = k++;
              break;
            case "end":
              delete A[t], --k;
            case "drag":
              d = i(r, t), m = k;
          }
          n.customEvent(new c(e, p, s, t, m, d[0] + u, d[1] + l, d[0] - v[0], d[1] - v[1], f), f.apply,
            f, [p, o, a]);
        };
      }
      var E,
        _,
        $,
        w,
        T = u,
        C = l,
        x = d,
        S = f,
        A = {},
        M = t.dispatch("start", "drag", "end"),
        k = 0,
        N = 0;
      return e.filter = function(t) {
        return arguments.length ? (T = "function" == typeof t ? t : s(!!t), e) : T;
      }, e.container = function(t) {
        return arguments.length ? (C = "function" == typeof t ? t : s(t), e) : C;
      }, e.subject = function(t) {
        return arguments.length ? (x = "function" == typeof t ? t : s(t), e) : x;
      }, e.touchable = function(t) {
        return arguments.length ? (S = "function" == typeof t ? t : s(!!t), e) : S;
      }, e.on = function() {
        var t = M.on.apply(M, arguments);
        return t === M ? e : t;
      }, e.clickDistance = function(t) {
        return arguments.length ? (N = (t = +t) * t, e) : Math.sqrt(N);
      }, e;
    }
    c.prototype.on = function() {
      var e = this._.on.apply(this._, arguments);
      return e === this._ ? this : e;
    }, e.drag = h, e.dragDisable = o, e.dragEnable = a, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
