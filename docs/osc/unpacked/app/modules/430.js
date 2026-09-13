// ─────────────────────────────────────────────────────────────
// APP MODULE 430
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, i) {
    i(t, n(38), n(82), n(39), n(45), n(86))
  }(this, function(e, t, n, i, o, r) {
    "use strict";

    function a(e) {
      return function() {
        return e
      }
    }

    function l(e, t, n) {
      this.target = e, this.type = t, this.selection = n
    }

    function s() {
      o.event.stopImmediatePropagation()
    }

    function d() {
      o.event.preventDefault(), o.event.stopImmediatePropagation()
    }

    function c(e) {
      return [+e[0], +e[1]]
    }

    function u(e) {
      return [c(e[0]), c(e[1])]
    }

    function f(e) {
      return function(t) {
        return o.touch(t, o.event.touches, e)
      }
    }

    function m(e) {
      return {
        type: e
      }
    }

    function g() {
      return !o.event.ctrlKey && !o.event.button
    }

    function p() {
      var e = this.ownerSVGElement || this;
      return e.hasAttribute("viewBox") ? (e = e.viewBox.baseVal, [
        [e.x, e.y],
        [e.x + e.width, e.y + e.height]
      ]) : [
        [0, 0],
        [e.width.baseVal.value, e.height.baseVal.value]
      ]
    }

    function h() {
      return navigator.maxTouchPoints || "ontouchstart" in this
    }

    function b(e) {
      for (; !e.__brush;)
        if (!(e = e.parentNode)) return;
      return e.__brush
    }

    function x(e) {
      return e[0][0] === e[1][0] || e[0][1] === e[1][1]
    }

    function v(e) {
      var t = e.__brush;
      return t ? t.dim.output(t.selection) : null
    }

    function y() {
      return E(O)
    }

    function w() {
      return E(A)
    }

    function S() {
      return E(I)
    }

    function E(e) {
      function c(t) {
        var n = t.property("__brush", L).selectAll(".overlay").data([m("overlay")]);
        n.enter().append("rect").attr("class", "overlay").attr("pointer-events", "all").attr("cursor", M.overlay)
          .merge(n).each(function() {
            var e = b(this).extent;
            o.select(this).attr("x", e[0][0]).attr("y", e[0][1]).attr("width", e[1][0] - e[0][0]).attr("height", e[
              1][1] - e[0][1])
          }), t.selectAll(".selection").data([m("selection")]).enter().append("rect").attr("class", "selection").attr(
            "cursor", M.selection).attr("fill", "#777").attr("fill-opacity", .3).attr("stroke", "#fff").attr(
            "shape-rendering", "crispEdges");
        var i = t.selectAll(".handle").data(e.handles, function(e) {
          return e.type
        });
        i.exit().remove(), i.enter().append("rect").attr("class", function(e) {
          return "handle handle--" + e.type
        }).attr("cursor", function(e) {
          return M[e.type]
        }), t.each(v).attr("fill", "none").attr("pointer-events", "all").on("mousedown.brush", S).filter(G).on(
          "touchstart.brush", S).on("touchmove.brush", E).on("touchend.brush touchcancel.brush", I).style(
          "touch-action", "none").style("-webkit-tap-highlight-color", "rgba(0,0,0,0)")
      }

      function v() {
        var e = o.select(this),
          t = b(this).selection;
        t ? (e.selectAll(".selection").style("display", null).attr("x", t[0][0]).attr("y", t[0][1]).attr("width", t[1]
          [0] - t[0][0]).attr("height", t[1][1] - t[0][1]), e.selectAll(".handle").style("display", null).attr(
          "x",
          function(e) {
            return "e" === e.type[e.type.length - 1] ? t[1][0] - B / 2 : t[0][0] - B / 2
          }).attr("y", function(e) {
          return "s" === e.type[0] ? t[1][1] - B / 2 : t[0][1] - B / 2
        }).attr("width", function(e) {
          return "n" === e.type || "s" === e.type ? t[1][0] - t[0][0] + B : B
        }).attr("height", function(e) {
          return "e" === e.type || "w" === e.type ? t[1][1] - t[0][1] + B : B
        })) : e.selectAll(".selection,.handle").style("display", "none").attr("x", null).attr("y", null).attr(
          "width", null).attr("height", null)
      }

      function y(e, t, n) {
        var i = e.__brush.emitter;
        return !i || n && i.clean ? new w(e, t, n) : i
      }

      function w(e, t, n) {
        this.that = e, this.args = t, this.state = e.__brush, this.active = 0, this.clean = n
      }

      function S() {
        function t() {
          var e = ne(G);
          !te || L || U || (Math.abs(e[0] - oe[0]) > Math.abs(e[1] - oe[1]) ? U = !0 : L = !0), oe = e, I = !0, d(),
            i()
        }

        function i() {
          var e;
          switch (J = oe[0] - ie[0], ee = oe[1] - ie[1], B) {
            case _:
            case k:
              Y && (J = Math.max(q - u, Math.min(Z - h, J)), m = u + J, w = h + J), $ && (ee = Math.max(X - g, Math
                .min(Q - S, ee)), p = g + ee, E = S + ee);
              break;
            case T:
              Y < 0 ? (J = Math.max(q - u, Math.min(Z - u, J)), m = u + J, w = h) : Y > 0 && (J = Math.max(q - h, Math
                .min(Z - h, J)), m = u, w = h + J), $ < 0 ? (ee = Math.max(X - g, Math.min(Q - g, ee)), p = g + ee,
                E = S) : $ > 0 && (ee = Math.max(X - S, Math.min(Q - S, ee)), p = g, E = S + ee);
              break;
            case C:
              Y && (m = Math.max(q, Math.min(Z, u - J * Y)), w = Math.max(q, Math.min(Z, h + J * Y))), $ && (p = Math
                .max(X, Math.min(Q, g - ee * $)), E = Math.max(X, Math.min(Q, S + ee * $)))
          }
          w < m && (Y *= -1, e = u, u = h, h = e, e = m, m = w, w = e, H in R && le.attr("cursor", M[H = R[H]])), E <
            p && ($ *= -1, e = g, g = S, S = e, e = p, p = E, E = e, H in P && le.attr("cursor", M[H = P[H]])), W
            .selection && (K = W.selection), L && (m = K[0][0], w = K[1][0]), U && (p = K[0][1], E = K[1][1]), K[0][
            0] === m && K[0][1] === p && K[1][0] === w && K[1][1] === E || (W.selection = [
              [m, p],
              [w, E]
            ], v.call(G), re.brush())
        }

        function a() {
          if (s(), o.event.touches) {
            if (o.event.touches.length) return;
            F && clearTimeout(F), F = setTimeout(function() {
              F = null
            }, 500)
          } else n.dragEnable(o.event.view, I), se.on("keydown.brush keyup.brush mousemove.brush mouseup.brush",
          null);
          ae.attr("pointer-events", "all"), le.attr("cursor", M.overlay), W.selection && (K = W.selection), x(K) && (W
            .selection = null, v.call(G)), re.end()
        }

        function l() {
          switch (o.event.keyCode) {
            case 16:
              te = Y && $;
              break;
            case 18:
              B === T && (Y && (h = w - J * Y, u = m + J * Y), $ && (S = E - ee * $, g = p + ee * $), B = C, i());
              break;
            case 32:
              B !== T && B !== C || (Y < 0 ? h = w - J : Y > 0 && (u = m - J), $ < 0 ? S = E - ee : $ > 0 && (g = p -
                ee), B = _, le.attr("cursor", M.selection), i());
              break;
            default:
              return
          }
          d()
        }

        function c() {
          switch (o.event.keyCode) {
            case 16:
              te && (L = U = te = !1, i());
              break;
            case 18:
              B === C && (Y < 0 ? h = w : Y > 0 && (u = m), $ < 0 ? S = E : $ > 0 && (g = p), B = T, i());
              break;
            case 32:
              B === _ && (o.event.altKey ? (Y && (h = w - J * Y, u = m + J * Y), $ && (S = E - ee * $, g = p + ee *
                $), B = C) : (Y < 0 ? h = w : Y > 0 && (u = m), $ < 0 ? S = E : $ > 0 && (g = p), B = T), le.attr(
                "cursor", M[H]), i());
              break;
            default:
              return
          }
          d()
        }
        if ((!F || o.event.touches) && z.apply(this, arguments)) {
          var u, m, g, p, h, w, S, E, I, L, U, G = this,
            H = o.event.target.__data__.type,
            B = "selection" === (V && o.event.metaKey ? H = "overlay" : H) ? k : V && o.event.altKey ? C : T,
            Y = e === A ? null : D[H],
            $ = e === O ? null : N[H],
            W = b(G),
            j = W.extent,
            K = W.selection,
            q = j[0][0],
            X = j[0][1],
            Z = j[1][0],
            Q = j[1][1],
            J = 0,
            ee = 0,
            te = Y && $ && V && o.event.shiftKey,
            ne = o.event.touches ? f(o.event.changedTouches[0].identifier) : o.mouse,
            ie = ne(G),
            oe = ie,
            re = y(G, arguments, !0).beforestart();
          "overlay" === H ? (K && (I = !0), W.selection = K = [
            [u = e === A ? q : ie[0], g = e === O ? X : ie[1]],
            [h = e === A ? Z : u, S = e === O ? Q : g]
          ]) : (u = K[0][0], g = K[0][1], h = K[1][0], S = K[1][1]), m = u, p = g, w = h, E = S;
          var ae = o.select(G).attr("pointer-events", "none"),
            le = ae.selectAll(".overlay").attr("cursor", M[H]);
          if (o.event.touches) re.moved = t, re.ended = a;
          else {
            var se = o.select(o.event.view).on("mousemove.brush", t, !0).on("mouseup.brush", a, !0);
            V && se.on("keydown.brush", l, !0).on("keyup.brush", c, !0), n.dragDisable(o.event.view)
          }
          s(), r.interrupt(G), v.call(G), re.start()
        }
      }

      function E() {
        y(this, arguments).moved()
      }

      function I() {
        y(this, arguments).ended()
      }

      function L() {
        var t = this.__brush || {
          selection: null
        };
        return t.extent = u(U.apply(this, arguments)), t.dim = e, t
      }
      var F, U = p,
        z = g,
        G = h,
        V = !0,
        H = t.dispatch("start", "brush", "end"),
        B = 6;
      return c.move = function(t, n) {
        t.selection ? t.on("start.brush", function() {
          y(this, arguments).beforestart().start()
        }).on("interrupt.brush end.brush", function() {
          y(this, arguments).end()
        }).tween("brush", function() {
          function t(e) {
            r.selection = 1 === e && null === s ? null : d(e), v.call(o), a.brush()
          }
          var o = this,
            r = o.__brush,
            a = y(o, arguments),
            l = r.selection,
            s = e.input("function" == typeof n ? n.apply(this, arguments) : n, r.extent),
            d = i.interpolate(l, s);
          return null !== l && null !== s ? t : t(1)
        }) : t.each(function() {
          var t = this,
            i = arguments,
            o = t.__brush,
            a = e.input("function" == typeof n ? n.apply(t, i) : n, o.extent),
            l = y(t, i).beforestart();
          r.interrupt(t), o.selection = null === a ? null : a, v.call(t), l.start().brush().end()
        })
      }, c.clear = function(e) {
        c.move(e, null)
      }, w.prototype = {
        beforestart: function() {
          return 1 === ++this.active && (this.state.emitter = this, this.starting = !0), this
        },
        start: function() {
          return this.starting ? (this.starting = !1, this.emit("start")) : this.emit("brush"), this
        },
        brush: function() {
          return this.emit("brush"), this
        },
        end: function() {
          return 0 === --this.active && (delete this.state.emitter, this.emit("end")), this
        },
        emit: function(t) {
          o.customEvent(new l(c, t, e.output(this.state.selection)), H.apply, H, [t, this.that, this.args])
        }
      }, c.extent = function(e) {
        return arguments.length ? (U = "function" == typeof e ? e : a(u(e)), c) : U
      }, c.filter = function(e) {
        return arguments.length ? (z = "function" == typeof e ? e : a(!!e), c) : z
      }, c.touchable = function(e) {
        return arguments.length ? (G = "function" == typeof e ? e : a(!!e), c) : G
      }, c.handleSize = function(e) {
        return arguments.length ? (B = +e, c) : B
      }, c.keyModifiers = function(e) {
        return arguments.length ? (V = !!e, c) : V
      }, c.on = function() {
        var e = H.on.apply(H, arguments);
        return e === H ? c : e
      }, c
    }
    var k = {
        name: "drag"
      },
      _ = {
        name: "space"
      },
      T = {
        name: "handle"
      },
      C = {
        name: "center"
      },
      O = {
        name: "x",
        handles: ["w", "e"].map(m),
        input: function(e, t) {
          return null == e ? null : [
            [+e[0], t[0][1]],
            [+e[1], t[1][1]]
          ]
        },
        output: function(e) {
          return e && [e[0][0], e[1][0]]
        }
      },
      A = {
        name: "y",
        handles: ["n", "s"].map(m),
        input: function(e, t) {
          return null == e ? null : [
            [t[0][0], +e[0]],
            [t[1][0], +e[1]]
          ]
        },
        output: function(e) {
          return e && [e[0][1], e[1][1]]
        }
      },
      I = {
        name: "xy",
        handles: ["n", "w", "e", "s", "nw", "ne", "sw", "se"].map(m),
        input: function(e) {
          return null == e ? null : u(e)
        },
        output: function(e) {
          return e
        }
      },
      M = {
        overlay: "crosshair",
        selection: "move",
        n: "ns-resize",
        e: "ew-resize",
        s: "ns-resize",
        w: "ew-resize",
        nw: "nwse-resize",
        ne: "nesw-resize",
        se: "nwse-resize",
        sw: "nesw-resize"
      },
      R = {
        e: "w",
        w: "e",
        nw: "ne",
        ne: "nw",
        se: "sw",
        sw: "se"
      },
      P = {
        n: "s",
        s: "n",
        nw: "sw",
        ne: "se",
        se: "ne",
        sw: "nw"
      },
      D = {
        overlay: 1,
        selection: 1,
        n: null,
        e: 1,
        s: null,
        w: -1,
        nw: -1,
        ne: 1,
        se: 1,
        sw: -1
      },
      N = {
        overlay: 1,
        selection: 1,
        n: -1,
        e: null,
        s: 1,
        w: null,
        nw: -1,
        ne: -1,
        se: 1,
        sw: 1
      };
    e.brush = S, e.brushSelection = v, e.brushX = y, e.brushY = w, Object.defineProperty(e, "__esModule", {
      value: !0
    })
  })
}
