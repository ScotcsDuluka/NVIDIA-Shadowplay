// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 82
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(38), require(45));
  }(this, function(e, t, n) {
    "use strict";

    function i() {
      n.event.stopImmediatePropagation();
    }

    function o() {
      n.event.preventDefault(), n.event.stopImmediatePropagation();
    }

    function r(e) {
      var t = e.document.documentElement,
        i = n.select(e).on("dragstart.drag", o, !0);
      "onselectstart" in t ? i.on("selectstart.drag", o, !0) : (t.__noselect = t.style.MozUserSelect, t
        .style.MozUserSelect = "none");
    }

    function a(e, t) {
      var i = e.document.documentElement,
        r = n.select(e).on("dragstart.drag", null);
      t && (r.on("click.drag", o, !0), setTimeout(function() {
        r.on("click.drag", null);
      }, 0)), "onselectstart" in i ? r.on("selectstart.drag", null) : (i.style.MozUserSelect = i
        .__noselect, delete i.__noselect);
    }

    function l(e) {
      return function() {
        return e;
      };
    }

    function s(e, t, n, i, o, r, a, l, s, d) {
      this.target = e, this.type = t, this.subject = n, this.identifier = i, this.active = o, this.x = r,
        this.y = a, this.dx = l, this.dy = s, this._ = d;
    }

    function d() {
      return !n.event.ctrlKey && !n.event.button;
    }

    function c() {
      return this.parentNode;
    }

    function u(e) {
      return null == e ? {
        x: n.event.x,
        y: n.event.y
      } : e;
    }

    function f() {
      return navigator.maxTouchPoints || "ontouchstart" in this;
    }

    function m() {
      function e(e) {
        e.on("mousedown.drag", m).filter(C).on("touchstart.drag", h).on("touchmove.drag", b).on(
          "touchend.drag touchcancel.drag", x).style("touch-action", "none").style(
          "-webkit-tap-highlight-color", "rgba(0,0,0,0)");
      }

      function m() {
        if (!E && k.apply(this, arguments)) {
          var e = v("mouse", _.apply(this, arguments), n.mouse, this, arguments);
          e && (n.select(n.event.view).on("mousemove.drag", g, !0).on("mouseup.drag", p, !0), r(n.event
            .view), i(), S = !1, y = n.event.clientX, w = n.event.clientY, e("start"));
        }
      }

      function g() {
        if (o(), !S) {
          var e = n.event.clientX - y,
            t = n.event.clientY - w;
          S = e * e + t * t > M;
        }
        O.mouse("drag");
      }

      function p() {
        n.select(n.event.view).on("mousemove.drag mouseup.drag", null), a(n.event.view, S), o(), O.mouse(
          "end");
      }

      function h() {
        if (k.apply(this, arguments)) {
          var e,
            t,
            o = n.event.changedTouches,
            r = _.apply(this, arguments),
            a = o.length;
          for (e = 0; e < a; ++e)(t = v(o[e].identifier, r, n.touch, this, arguments)) && (i(), t("start"));
        }
      }

      function b() {
        var e,
          t,
          i = n.event.changedTouches,
          r = i.length;
        for (e = 0; e < r; ++e)(t = O[i[e].identifier]) && (o(), t("drag"));
      }

      function x() {
        var e,
          t,
          o = n.event.changedTouches,
          r = o.length;
        for (E && clearTimeout(E), E = setTimeout(function() {
            E = null;
          }, 500), e = 0; e < r; ++e)(t = O[o[e].identifier]) && (i(), t("end"));
      }

      function v(t, i, o, r, a) {
        var l,
          d,
          c,
          u = o(i, t),
          f = A.copy();
        if (n.customEvent(new s(e, "beforestart", l, t, I, u[0], u[1], 0, 0, f), function() {
            return null != (n.event.subject = l = T.apply(r, a)) && (d = l.x - u[0] || 0, c = l.y - u[
              1] || 0, !0);
          })) return function m(g) {
          var p,
            h = u;
          switch (g) {
            case "start":
              O[t] = m, p = I++;
              break;
            case "end":
              delete O[t], --I;
            case "drag":
              u = o(i, t), p = I;
          }
          n.customEvent(new s(e, g, l, t, p, u[0] + d, u[1] + c, u[0] - h[0], u[1] - h[1], f), f.apply,
            f, [g, r, a]);
        };
      }
      var y,
        w,
        S,
        E,
        k = d,
        _ = c,
        T = u,
        C = f,
        O = {},
        A = t.dispatch("start", "drag", "end"),
        I = 0,
        M = 0;
      return e.filter = function(t) {
        return arguments.length ? (k = "function" == typeof t ? t : l(!!t), e) : k;
      }, e.container = function(t) {
        return arguments.length ? (_ = "function" == typeof t ? t : l(t), e) : _;
      }, e.subject = function(t) {
        return arguments.length ? (T = "function" == typeof t ? t : l(t), e) : T;
      }, e.touchable = function(t) {
        return arguments.length ? (C = "function" == typeof t ? t : l(!!t), e) : C;
      }, e.on = function() {
        var t = A.on.apply(A, arguments);
        return t === A ? e : t;
      }, e.clickDistance = function(t) {
        return arguments.length ? (M = (t = +t) * t, e) : Math.sqrt(M);
      }, e;
    }
    s.prototype.on = function() {
      var e = this._.on.apply(this._, arguments);
      return e === this._ ? this : e;
    }, e.drag = m, e.dragDisable = r, e.dragEnable = a, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
