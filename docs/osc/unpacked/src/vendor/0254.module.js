// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 254
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
      this.target = e, this.type = t, this.transform = n;
    }

    function c(e, t, n) {
      this.k = e, this.x = t, this.y = n;
    }

    function u(e) {
      for (; !e.__zoom;)
        if (!(e = e.parentNode)) return b;
      return e.__zoom;
    }

    function l() {
      i.event.stopImmediatePropagation();
    }

    function d() {
      i.event.preventDefault(), i.event.stopImmediatePropagation();
    }

    function f() {
      return !i.event.ctrlKey && !i.event.button;
    }

    function h() {
      var e = this;
      return e instanceof SVGElement ? (e = e.ownerSVGElement || e, e.hasAttribute("viewBox") ? (e = e
        .viewBox.baseVal, [
          [e.x, e.y],
          [e.x + e.width, e.y + e.height]
        ]) : [
        [0, 0],
        [e.width.baseVal.value, e.height.baseVal.value]
      ]) : [
        [0, 0],
        [e.clientWidth, e.clientHeight]
      ];
    }

    function p() {
      return this.__zoom || b;
    }

    function m() {
      return -i.event.deltaY * (1 === i.event.deltaMode ? .05 : i.event.deltaMode ? 1 : .002);
    }

    function v() {
      return navigator.maxTouchPoints || "ontouchstart" in this;
    }

    function g(e, t, n) {
      var r = e.invertX(t[0][0]) - n[0][0],
        i = e.invertX(t[1][0]) - n[1][0],
        o = e.invertY(t[0][1]) - n[0][1],
        a = e.invertY(t[1][1]) - n[1][1];
      return e.translate(i > r ? (r + i) / 2 : Math.min(0, r) || Math.max(0, i), a > o ? (o + a) / 2 : Math
        .min(0, o) || Math.max(0, a));
    }

    function y() {
      function e(e) {
        e.property("__zoom", p).on("wheel.zoom", T).on("mousedown.zoom", C).on("dblclick.zoom", x).filter(P)
          .on("touchstart.zoom", S).on("touchmove.zoom", A).on("touchend.zoom touchcancel.zoom", M).style(
            "touch-action", "none").style("-webkit-tap-highlight-color", "rgba(0,0,0,0)");
      }

      function u(e, t) {
        return t = Math.max(L[0], Math.min(L[1], t)), t === e.k ? e : new c(t, e.x, e.y);
      }

      function y(e, t, n) {
        var r = t[0] - n[0] * e.k,
          i = t[1] - n[1] * e.k;
        return r === e.x && i === e.y ? e : new c(e.k, r, i);
      }

      function E(e) {
        return [(+e[0][0] + +e[1][0]) / 2, (+e[0][1] + +e[1][1]) / 2];
      }

      function _(e, t, n) {
        e.on("start.zoom", function() {
          $(this, arguments).start();
        }).on("interrupt.zoom end.zoom", function() {
          $(this, arguments).end();
        }).tween("zoom", function() {
          var e = this,
            r = arguments,
            i = $(e, r),
            o = O.apply(e, r),
            a = null == n ? E(o) : "function" == typeof n ? n.apply(e, r) : n,
            s = Math.max(o[1][0] - o[0][0], o[1][1] - o[0][1]),
            u = e.__zoom,
            l = "function" == typeof t ? t.apply(e, r) : t,
            d = j(u.invert(a).concat(s / u.k), l.invert(a).concat(s / l.k));
          return function(e) {
            if (1 === e) e = l;
            else {
              var t = d(e),
                n = s / t[2];
              e = new c(n, a[0] - t[0] * n, a[1] - t[1] * n);
            }
            i.zoom(null, e);
          };
        });
      }

      function $(e, t, n) {
        return !n && e.__zooming || new w(e, t);
      }

      function w(e, t) {
        this.that = e, this.args = t, this.active = 0, this.extent = O.apply(e, t), this.taps = 0;
      }

      function T() {
        function e() {
          t.wheel = null, t.end();
        }
        if (I.apply(this, arguments)) {
          var t = $(this, arguments),
            n = this.__zoom,
            r = Math.max(L[0], Math.min(L[1], n.k * Math.pow(2, R.apply(this, arguments)))),
            a = i.mouse(this);
          if (t.wheel) t.mouse[0][0] === a[0] && t.mouse[0][1] === a[1] || (t.mouse[1] = n.invert(t.mouse[
            0] = a)), clearTimeout(t.wheel);
          else {
            if (n.k === r) return;
            t.mouse = [a, n.invert(a)], o.interrupt(this), t.start();
          }
          d(), t.wheel = setTimeout(e, z), t.zoom("mouse", D(y(u(n, r), t.mouse[0], t.mouse[1]), t.extent,
            U));
        }
      }

      function C() {
        function e() {
          if (d(), !r.moved) {
            var e = i.event.clientX - c,
              t = i.event.clientY - u;
            r.moved = e * e + t * t > q;
          }
          r.zoom("mouse", D(y(r.that.__zoom, r.mouse[0] = i.mouse(r.that), r.mouse[1]), r.extent, U));
        }

        function t() {
          a.on("mousemove.zoom mouseup.zoom", null), n.dragEnable(i.event.view, r.moved), d(), r.end();
        }
        if (!N && I.apply(this, arguments)) {
          var r = $(this, arguments, !0),
            a = i.select(i.event.view).on("mousemove.zoom", e, !0).on("mouseup.zoom", t, !0),
            s = i.mouse(this),
            c = i.event.clientX,
            u = i.event.clientY;
          n.dragDisable(i.event.view), l(), r.mouse = [s, this.__zoom.invert(s)], o.interrupt(this), r
            .start();
        }
      }

      function x() {
        if (I.apply(this, arguments)) {
          var t = this.__zoom,
            n = i.mouse(this),
            r = t.invert(n),
            o = t.k * (i.event.shiftKey ? .5 : 2),
            a = D(y(u(t, o), n, r), O.apply(this, arguments), U);
          d(), F > 0 ? i.select(this).transition().duration(F).call(_, a, n) : i.select(this).call(e
            .transform, a);
        }
      }

      function S() {
        if (I.apply(this, arguments)) {
          var e,
            t,
            n,
            r,
            a = i.event.touches,
            s = a.length,
            c = $(this, arguments, i.event.changedTouches.length === s);
          for (l(), t = 0; t < s; ++t) n = a[t], r = i.touch(this, a, n.identifier), r = [r, this.__zoom
            .invert(r), n.identifier
          ], c.touch0 ? c.touch1 || c.touch0[2] === r[2] || (c.touch1 = r, c.taps = 0) : (c.touch0 = r,
            e = !0, c.taps = 1 + !!k);
          k && (k = clearTimeout(k)), e && (c.taps < 2 && (k = setTimeout(function() {
            k = null;
          }, B)), o.interrupt(this), c.start());
        }
      }

      function A() {
        if (this.__zooming) {
          var e,
            t,
            n,
            r,
            o = $(this, arguments),
            a = i.event.changedTouches,
            s = a.length;
          for (d(), k && (k = clearTimeout(k)), o.taps = 0, e = 0; e < s; ++e) t = a[e], n = i.touch(this,
              a, t.identifier), o.touch0 && o.touch0[2] === t.identifier ? o.touch0[0] = n : o.touch1 && o
            .touch1[2] === t.identifier && (o.touch1[0] = n);
          if (t = o.that.__zoom, o.touch1) {
            var c = o.touch0[0],
              l = o.touch0[1],
              f = o.touch1[0],
              h = o.touch1[1],
              p = (p = f[0] - c[0]) * p + (p = f[1] - c[1]) * p,
              m = (m = h[0] - l[0]) * m + (m = h[1] - l[1]) * m;
            t = u(t, Math.sqrt(p / m)), n = [(c[0] + f[0]) / 2, (c[1] + f[1]) / 2], r = [(l[0] + h[0]) / 2,
              (l[1] + h[1]) / 2
            ];
          } else {
            if (!o.touch0) return;
            n = o.touch0[0], r = o.touch0[1];
          }
          o.zoom("touch", D(y(t, n, r), o.extent, U));
        }
      }

      function M() {
        if (this.__zooming) {
          var e,
            t,
            n = $(this, arguments),
            r = i.event.changedTouches,
            o = r.length;
          for (l(), N && clearTimeout(N), N = setTimeout(function() {
              N = null;
            }, B), e = 0; e < o; ++e) t = r[e], n.touch0 && n.touch0[2] === t.identifier ? delete n.touch0 :
            n.touch1 && n.touch1[2] === t.identifier && delete n.touch1;
          if (n.touch1 && !n.touch0 && (n.touch0 = n.touch1, delete n.touch1), n.touch0) n.touch0[1] = this
            .__zoom.invert(n.touch0[0]);
          else if (n.end(), 2 === n.taps) {
            var a = i.select(this).on("dblclick.zoom");
            a && a.apply(this, arguments);
          }
        }
      }
      var k,
        N,
        I = f,
        O = h,
        D = g,
        R = m,
        P = v,
        L = [0, 1 / 0],
        U = [
          [-(1 / 0), -(1 / 0)],
          [1 / 0, 1 / 0]
        ],
        F = 250,
        j = r.interpolateZoom,
        H = t.dispatch("start", "zoom", "end"),
        B = 500,
        z = 150,
        q = 0;
      return e.transform = function(e, t, n) {
        var r = e.selection ? e.selection() : e;
        r.property("__zoom", p), e !== r ? _(e, t, n) : r.interrupt().each(function() {
          $(this, arguments).start().zoom(null, "function" == typeof t ? t.apply(this, arguments) : t)
            .end();
        });
      }, e.scaleBy = function(t, n, r) {
        e.scaleTo(t, function() {
          var e = this.__zoom.k,
            t = "function" == typeof n ? n.apply(this, arguments) : n;
          return e * t;
        }, r);
      }, e.scaleTo = function(t, n, r) {
        e.transform(t, function() {
          var e = O.apply(this, arguments),
            t = this.__zoom,
            i = null == r ? E(e) : "function" == typeof r ? r.apply(this, arguments) : r,
            o = t.invert(i),
            a = "function" == typeof n ? n.apply(this, arguments) : n;
          return D(y(u(t, a), i, o), e, U);
        }, r);
      }, e.translateBy = function(t, n, r) {
        e.transform(t, function() {
          return D(this.__zoom.translate("function" == typeof n ? n.apply(this, arguments) : n,
            "function" == typeof r ? r.apply(this, arguments) : r), O.apply(this, arguments), U);
        });
      }, e.translateTo = function(t, n, r, i) {
        e.transform(t, function() {
          var e = O.apply(this, arguments),
            t = this.__zoom,
            o = null == i ? E(e) : "function" == typeof i ? i.apply(this, arguments) : i;
          return D(b.translate(o[0], o[1]).scale(t.k).translate("function" == typeof n ? -n.apply(
            this, arguments) : -n, "function" == typeof r ? -r.apply(this, arguments) : -r), e, U);
        }, i);
      }, w.prototype = {
        start: function() {
          return 1 === ++this.active && (this.that.__zooming = this, this.emit("start")), this;
        },
        zoom: function(e, t) {
          return this.mouse && "mouse" !== e && (this.mouse[1] = t.invert(this.mouse[0])), this
            .touch0 && "touch" !== e && (this.touch0[1] = t.invert(this.touch0[0])), this.touch1 &&
            "touch" !== e && (this.touch1[1] = t.invert(this.touch1[0])), this.that.__zoom = t, this
            .emit("zoom"), this;
        },
        end: function() {
          return 0 === --this.active && (delete this.that.__zooming, this.emit("end")), this;
        },
        emit: function(t) {
          i.customEvent(new s(e, t, this.that.__zoom), H.apply, H, [t, this.that, this.args]);
        }
      }, e.wheelDelta = function(t) {
        return arguments.length ? (R = "function" == typeof t ? t : a(+t), e) : R;
      }, e.filter = function(t) {
        return arguments.length ? (I = "function" == typeof t ? t : a(!!t), e) : I;
      }, e.touchable = function(t) {
        return arguments.length ? (P = "function" == typeof t ? t : a(!!t), e) : P;
      }, e.extent = function(t) {
        return arguments.length ? (O = "function" == typeof t ? t : a([
          [+t[0][0], +t[0][1]],
          [+t[1][0], +t[1][1]]
        ]), e) : O;
      }, e.scaleExtent = function(t) {
        return arguments.length ? (L[0] = +t[0], L[1] = +t[1], e) : [L[0], L[1]];
      }, e.translateExtent = function(t) {
        return arguments.length ? (U[0][0] = +t[0][0], U[1][0] = +t[1][0], U[0][1] = +t[0][1], U[1][1] = +
          t[1][1], e) : [
          [U[0][0], U[0][1]],
          [U[1][0], U[1][1]]
        ];
      }, e.constrain = function(t) {
        return arguments.length ? (D = t, e) : D;
      }, e.duration = function(t) {
        return arguments.length ? (F = +t, e) : F;
      }, e.interpolate = function(t) {
        return arguments.length ? (j = t, e) : j;
      }, e.on = function() {
        var t = H.on.apply(H, arguments);
        return t === H ? e : t;
      }, e.clickDistance = function(t) {
        return arguments.length ? (q = (t = +t) * t, e) : Math.sqrt(q);
      }, e;
    }
    c.prototype = {
      constructor: c,
      scale: function(e) {
        return 1 === e ? this : new c(this.k * e, this.x, this.y);
      },
      translate: function(e, t) {
        return 0 === e & 0 === t ? this : new c(this.k, this.x + this.k * e, this.y + this.k * t);
      },
      apply: function(e) {
        return [e[0] * this.k + this.x, e[1] * this.k + this.y];
      },
      applyX: function(e) {
        return e * this.k + this.x;
      },
      applyY: function(e) {
        return e * this.k + this.y;
      },
      invert: function(e) {
        return [(e[0] - this.x) / this.k, (e[1] - this.y) / this.k];
      },
      invertX: function(e) {
        return (e - this.x) / this.k;
      },
      invertY: function(e) {
        return (e - this.y) / this.k;
      },
      rescaleX: function(e) {
        return e.copy().domain(e.range().map(this.invertX, this).map(e.invert, e));
      },
      rescaleY: function(e) {
        return e.copy().domain(e.range().map(this.invertY, this).map(e.invert, e));
      },
      toString: function() {
        return "translate(" + this.x + "," + this.y + ") scale(" + this.k + ")";
      }
    };
    var b = new c(1, 0, 0);
    u.prototype = c.prototype, e.zoom = y, e.zoomIdentity = b, e.zoomTransform = u, Object.defineProperty(e,
      "__esModule", {
        value: !0
      });
  });
}
