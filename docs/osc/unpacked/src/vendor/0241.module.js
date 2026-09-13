// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 241
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(20), require(63), require(21), require(31), require(67));
  }(this, function(e, t, n, r, i, o) {
    "use strict";

    function a(e) {
      return function() {
        return e;
      };
    }

    function s(e, t, n) {
      this.target = e, this.type = t, this.selection = n;
    }

    function c() {
      i.event.stopImmediatePropagation();
    }

    function u() {
      i.event.preventDefault(), i.event.stopImmediatePropagation();
    }

    function l(e) {
      return [+e[0], +e[1]];
    }

    function d(e) {
      return [l(e[0]), l(e[1])];
    }

    function f(e) {
      return function(t) {
        return i.touch(t, i.event.touches, e);
      };
    }

    function h(e) {
      return {
        type: e
      };
    }

    function p() {
      return !i.event.ctrlKey && !i.event.button;
    }

    function m() {
      var e = this.ownerSVGElement || this;
      return e.hasAttribute("viewBox") ? (e = e.viewBox.baseVal, [
        [e.x, e.y],
        [e.x + e.width, e.y + e.height]
      ]) : [
        [0, 0],
        [e.width.baseVal.value, e.height.baseVal.value]
      ];
    }

    function v() {
      return navigator.maxTouchPoints || "ontouchstart" in this;
    }

    function g(e) {
      for (; !e.__brush;)
        if (!(e = e.parentNode)) return;
      return e.__brush;
    }

    function y(e) {
      return e[0][0] === e[1][0] || e[0][1] === e[1][1];
    }

    function b(e) {
      var t = e.__brush;
      return t ? t.dim.output(t.selection) : null;
    }

    function E() {
      return w(A);
    }

    function _() {
      return w(M);
    }

    function $() {
      return w(k);
    }

    function w(e) {
      function l(t) {
        var n = t.property("__brush", P).selectAll(".overlay").data([h("overlay")]);
        n.enter().append("rect").attr("class", "overlay").attr("pointer-events", "all").attr("cursor", N
          .overlay).merge(n).each(function() {
          var e = g(this).extent;
          i.select(this).attr("x", e[0][0]).attr("y", e[0][1]).attr("width", e[1][0] - e[0][0]).attr(
            "height", e[1][1] - e[0][1]);
        }), t.selectAll(".selection").data([h("selection")]).enter().append("rect").attr("class",
          "selection").attr("cursor", N.selection).attr("fill", "#777").attr("fill-opacity", .3).attr(
          "stroke", "#fff").attr("shape-rendering", "crispEdges");
        var r = t.selectAll(".handle").data(e.handles, function(e) {
          return e.type;
        });
        r.exit().remove(), r.enter().append("rect").attr("class", function(e) {
            return "handle handle--" + e.type;
          }).attr("cursor", function(e) {
            return N[e.type];
          }), t.each(b).attr("fill", "none").attr("pointer-events", "all").on("mousedown.brush", $).filter(
            j).on("touchstart.brush", $).on("touchmove.brush", w).on("touchend.brush touchcancel.brush", k)
          .style("touch-action", "none").style("-webkit-tap-highlight-color", "rgba(0,0,0,0)");
      }

      function b() {
        var e = i.select(this),
          t = g(this).selection;
        t ? (e.selectAll(".selection").style("display", null).attr("x", t[0][0]).attr("y", t[0][1]).attr(
            "width", t[1][0] - t[0][0]).attr("height", t[1][1] - t[0][1]), e.selectAll(".handle").style(
            "display", null).attr("x", function(e) {
            return "e" === e.type[e.type.length - 1] ? t[1][0] - z / 2 : t[0][0] - z / 2;
          }).attr("y", function(e) {
            return "s" === e.type[0] ? t[1][1] - z / 2 : t[0][1] - z / 2;
          }).attr("width", function(e) {
            return "n" === e.type || "s" === e.type ? t[1][0] - t[0][0] + z : z;
          }).attr("height", function(e) {
            return "e" === e.type || "w" === e.type ? t[1][1] - t[0][1] + z : z;
          })) : e.selectAll(".selection,.handle").style("display", "none").attr("x", null).attr("y", null)
          .attr("width", null).attr("height", null);
      }

      function E(e, t, n) {
        var r = e.__brush.emitter;
        return !r || n && r.clean ? new _(e, t, n) : r;
      }

      function _(e, t, n) {
        this.that = e, this.args = t, this.state = e.__brush, this.active = 0, this.clean = n;
      }

      function $() {
        function t() {
          var e = ne(j);
          !te || P || U || (Math.abs(e[0] - ie[0]) > Math.abs(e[1] - ie[1]) ? U = !0 : P = !0), ie = e,
            k = !0, u(), r();
        }

        function r() {
          var e;
          switch (Z = ie[0] - re[0], ee = ie[1] - re[1], z) {
            case C:
            case T:
              q && (Z = Math.max(K - d, Math.min(Q - v, Z)), h = d + Z, _ = v + Z), G && (ee = Math.max(X -
                p, Math.min(J - $, ee)), m = p + ee, w = $ + ee);
              break;
            case x:
              q < 0 ? (Z = Math.max(K - d, Math.min(Q - d, Z)), h = d + Z, _ = v) : q > 0 && (Z = Math.max(
                K - v, Math.min(Q - v, Z)), h = d, _ = v + Z), G < 0 ? (ee = Math.max(X - p, Math.min(J -
                p, ee)), m = p + ee, w = $) : G > 0 && (ee = Math.max(X - $, Math.min(J - $, ee)), m = p,
                w = $ + ee);
              break;
            case S:
              q && (h = Math.max(K, Math.min(Q, d - Z * q)), _ = Math.max(K, Math.min(Q, v + Z * q))), G &&
                (m = Math.max(X, Math.min(J, p - ee * G)), w = Math.max(X, Math.min(J, $ + ee * G)));
          }
          _ < h && (q *= -1, e = d, d = v, v = e, e = h, h = _, _ = e, B in I && se.attr("cursor", N[B = I[
              B]])), w < m && (G *= -1, e = p, p = $, $ = e, e = m, m = w, w = e, B in O && se.attr(
              "cursor", N[B = O[B]])), V.selection && (Y = V.selection), P && (h = Y[0][0], _ = Y[1][0]),
            U && (m = Y[0][1], w = Y[1][1]), Y[0][0] === h && Y[0][1] === m && Y[1][0] === _ && Y[1][1] ===
            w || (V.selection = [
              [h, m],
              [_, w]
            ], b.call(j), oe.brush());
        }

        function a() {
          if (c(), i.event.touches) {
            if (i.event.touches.length) return;
            L && clearTimeout(L), L = setTimeout(function() {
              L = null;
            }, 500);
          } else n.dragEnable(i.event.view, k), ce.on(
            "keydown.brush keyup.brush mousemove.brush mouseup.brush", null);
          ae.attr("pointer-events", "all"), se.attr("cursor", N.overlay), V.selection && (Y = V.selection),
            y(Y) && (V.selection = null, b.call(j)), oe.end();
        }

        function s() {
          switch (i.event.keyCode) {
            case 16:
              te = q && G;
              break;
            case 18:
              z === x && (q && (v = _ - Z * q, d = h + Z * q), G && ($ = w - ee * G, p = m + ee * G), z = S,
                r());
              break;
            case 32:
              z !== x && z !== S || (q < 0 ? v = _ - Z : q > 0 && (d = h - Z), G < 0 ? $ = w - ee : G > 0 &&
                (p = m - ee), z = C, se.attr("cursor", N.selection), r());
              break;
            default:
              return;
          }
          u();
        }

        function l() {
          switch (i.event.keyCode) {
            case 16:
              te && (P = U = te = !1, r());
              break;
            case 18:
              z === S && (q < 0 ? v = _ : q > 0 && (d = h), G < 0 ? $ = w : G > 0 && (p = m), z = x, r());
              break;
            case 32:
              z === C && (i.event.altKey ? (q && (v = _ - Z * q, d = h + Z * q), G && ($ = w - ee * G, p =
                m + ee * G), z = S) : (q < 0 ? v = _ : q > 0 && (d = h), G < 0 ? $ = w : G > 0 && (p =
                m), z = x), se.attr("cursor", N[B]), r());
              break;
            default:
              return;
          }
          u();
        }
        if ((!L || i.event.touches) && F.apply(this, arguments)) {
          var d,
            h,
            p,
            m,
            v,
            _,
            $,
            w,
            k,
            P,
            U,
            j = this,
            B = i.event.target.__data__.type,
            z = "selection" === (H && i.event.metaKey ? B = "overlay" : B) ? T : H && i.event.altKey ? S :
            x,
            q = e === M ? null : D[B],
            G = e === A ? null : R[B],
            V = g(j),
            W = V.extent,
            Y = V.selection,
            K = W[0][0],
            X = W[0][1],
            Q = W[1][0],
            J = W[1][1],
            Z = 0,
            ee = 0,
            te = q && G && H && i.event.shiftKey,
            ne = i.event.touches ? f(i.event.changedTouches[0].identifier) : i.mouse,
            re = ne(j),
            ie = re,
            oe = E(j, arguments, !0).beforestart();
          "overlay" === B ? (Y && (k = !0), V.selection = Y = [
            [d = e === M ? K : re[0], p = e === A ? X : re[1]],
            [v = e === M ? Q : d, $ = e === A ? J : p]
          ]) : (d = Y[0][0], p = Y[0][1], v = Y[1][0], $ = Y[1][1]), h = d, m = p, _ = v, w = $;
          var ae = i.select(j).attr("pointer-events", "none"),
            se = ae.selectAll(".overlay").attr("cursor", N[B]);
          if (i.event.touches) oe.moved = t, oe.ended = a;
          else {
            var ce = i.select(i.event.view).on("mousemove.brush", t, !0).on("mouseup.brush", a, !0);
            H && ce.on("keydown.brush", s, !0).on("keyup.brush", l, !0), n.dragDisable(i.event.view);
          }
          c(), o.interrupt(j), b.call(j), oe.start();
        }
      }

      function w() {
        E(this, arguments).moved();
      }

      function k() {
        E(this, arguments).ended();
      }

      function P() {
        var t = this.__brush || {
          selection: null
        };
        return t.extent = d(U.apply(this, arguments)), t.dim = e, t;
      }
      var L,
        U = m,
        F = p,
        j = v,
        H = !0,
        B = t.dispatch("start", "brush", "end"),
        z = 6;
      return l.move = function(t, n) {
        t.selection ? t.on("start.brush", function() {
          E(this, arguments).beforestart().start();
        }).on("interrupt.brush end.brush", function() {
          E(this, arguments).end();
        }).tween("brush", function() {
          function t(e) {
            o.selection = 1 === e && null === c ? null : u(e), b.call(i), a.brush();
          }
          var i = this,
            o = i.__brush,
            a = E(i, arguments),
            s = o.selection,
            c = e.input("function" == typeof n ? n.apply(this, arguments) : n, o.extent),
            u = r.interpolate(s, c);
          return null !== s && null !== c ? t : t(1);
        }) : t.each(function() {
          var t = this,
            r = arguments,
            i = t.__brush,
            a = e.input("function" == typeof n ? n.apply(t, r) : n, i.extent),
            s = E(t, r).beforestart();
          o.interrupt(t), i.selection = null === a ? null : a, b.call(t), s.start().brush().end();
        });
      }, l.clear = function(e) {
        l.move(e, null);
      }, _.prototype = {
        beforestart: function() {
          return 1 === ++this.active && (this.state.emitter = this, this.starting = !0), this;
        },
        start: function() {
          return this.starting ? (this.starting = !1, this.emit("start")) : this.emit("brush"), this;
        },
        brush: function() {
          return this.emit("brush"), this;
        },
        end: function() {
          return 0 === --this.active && (delete this.state.emitter, this.emit("end")), this;
        },
        emit: function(t) {
          i.customEvent(new s(l, t, e.output(this.state.selection)), B.apply, B, [t, this.that, this
            .args
          ]);
        }
      }, l.extent = function(e) {
        return arguments.length ? (U = "function" == typeof e ? e : a(d(e)), l) : U;
      }, l.filter = function(e) {
        return arguments.length ? (F = "function" == typeof e ? e : a(!!e), l) : F;
      }, l.touchable = function(e) {
        return arguments.length ? (j = "function" == typeof e ? e : a(!!e), l) : j;
      }, l.handleSize = function(e) {
        return arguments.length ? (z = +e, l) : z;
      }, l.keyModifiers = function(e) {
        return arguments.length ? (H = !!e, l) : H;
      }, l.on = function() {
        var e = B.on.apply(B, arguments);
        return e === B ? l : e;
      }, l;
    }
    var T = {
        name: "drag"
      },
      C = {
        name: "space"
      },
      x = {
        name: "handle"
      },
      S = {
        name: "center"
      },
      A = {
        name: "x",
        handles: ["w", "e"].map(h),
        input: function(e, t) {
          return null == e ? null : [
            [+e[0], t[0][1]],
            [+e[1], t[1][1]]
          ];
        },
        output: function(e) {
          return e && [e[0][0], e[1][0]];
        }
      },
      M = {
        name: "y",
        handles: ["n", "s"].map(h),
        input: function(e, t) {
          return null == e ? null : [
            [t[0][0], +e[0]],
            [t[1][0], +e[1]]
          ];
        },
        output: function(e) {
          return e && [e[0][1], e[1][1]];
        }
      },
      k = {
        name: "xy",
        handles: ["n", "w", "e", "s", "nw", "ne", "sw", "se"].map(h),
        input: function(e) {
          return null == e ? null : d(e);
        },
        output: function(e) {
          return e;
        }
      },
      N = {
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
      I = {
        e: "w",
        w: "e",
        nw: "ne",
        ne: "nw",
        se: "sw",
        sw: "se"
      },
      O = {
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
      R = {
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
    e.brush = $, e.brushSelection = b, e.brushX = E, e.brushY = _, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
