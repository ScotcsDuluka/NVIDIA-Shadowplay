// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 443
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(38), require(82), require(39), require(45), require(86));
  }(this, function(e, t, n, i, o, r) {
    "use strict";

    function a(e) {
      return function() {
        return e;
      };
    }

    function l(e, t, n) {
      this.target = e, this.type = t, this.transform = n;
    }

    function s(e, t, n) {
      this.k = e, this.x = t, this.y = n;
    }

    function d(e) {
      for (; !e.__zoom;)
        if (!(e = e.parentNode)) return v;
      return e.__zoom;
    }

    function c() {
      o.event.stopImmediatePropagation();
    }

    function u() {
      o.event.preventDefault(), o.event.stopImmediatePropagation();
    }

    function f() {
      return !o.event.ctrlKey && !o.event.button;
    }

    function m() {
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

    function g() {
      return this.__zoom || v;
    }

    function p() {
      return -o.event.deltaY * (1 === o.event.deltaMode ? .05 : o.event.deltaMode ? 1 : .002);
    }

    function h() {
      return navigator.maxTouchPoints || "ontouchstart" in this;
    }

    function b(e, t, n) {
      var i = e.invertX(t[0][0]) - n[0][0],
        o = e.invertX(t[1][0]) - n[1][0],
        r = e.invertY(t[0][1]) - n[0][1],
        a = e.invertY(t[1][1]) - n[1][1];
      return e.translate(o > i ? (i + o) / 2 : Math.min(0, i) || Math.max(0, o), a > r ? (r + a) / 2 : Math
        .min(0, r) || Math.max(0, a));
    }

    function x() {
      function e(e) {
        e.property("__zoom", g).on("wheel.zoom", k).on("mousedown.zoom", _).on("dblclick.zoom", T).filter(L)
          .on("touchstart.zoom", C).on("touchmove.zoom", O).on("touchend.zoom touchcancel.zoom", A).style(
            "touch-action", "none").style("-webkit-tap-highlight-color", "rgba(0,0,0,0)");
      }

      function d(e, t) {
        return t = Math.max(F[0], Math.min(F[1], t)), t === e.k ? e : new s(t, e.x, e.y);
      }

      function x(e, t, n) {
        var i = t[0] - n[0] * e.k,
          o = t[1] - n[1] * e.k;
        return i === e.x && o === e.y ? e : new s(e.k, i, o);
      }

      function y(e) {
        return [(+e[0][0] + +e[1][0]) / 2, (+e[0][1] + +e[1][1]) / 2];
      }

      function w(e, t, n) {
        e.on("start.zoom", function() {
          S(this, arguments).start();
        }).on("interrupt.zoom end.zoom", function() {
          S(this, arguments).end();
        }).tween("zoom", function() {
          var e = this,
            i = arguments,
            o = S(e, i),
            r = P.apply(e, i),
            a = null == n ? y(r) : "function" == typeof n ? n.apply(e, i) : n,
            l = Math.max(r[1][0] - r[0][0], r[1][1] - r[0][1]),
            d = e.__zoom,
            c = "function" == typeof t ? t.apply(e, i) : t,
            u = G(d.invert(a).concat(l / d.k), c.invert(a).concat(l / c.k));
          return function(e) {
            if (1 === e) e = c;
            else {
              var t = u(e),
                n = l / t[2];
              e = new s(n, a[0] - t[0] * n, a[1] - t[1] * n);
            }
            o.zoom(null, e);
          };
        });
      }

      function S(e, t, n) {
        return !n && e.__zooming || new E(e, t);
      }

      function E(e, t) {
        this.that = e, this.args = t, this.active = 0, this.extent = P.apply(e, t), this.taps = 0;
      }

      function k() {
        function e() {
          t.wheel = null, t.end();
        }
        if (R.apply(this, arguments)) {
          var t = S(this, arguments),
            n = this.__zoom,
            i = Math.max(F[0], Math.min(F[1], n.k * Math.pow(2, N.apply(this, arguments)))),
            a = o.mouse(this);
          if (t.wheel) t.mouse[0][0] === a[0] && t.mouse[0][1] === a[1] || (t.mouse[1] = n.invert(t.mouse[
            0] = a)), clearTimeout(t.wheel);
          else {
            if (n.k === i) return;
            t.mouse = [a, n.invert(a)], r.interrupt(this), t.start();
          }
          u(), t.wheel = setTimeout(e, B), t.zoom("mouse", D(x(d(n, i), t.mouse[0], t.mouse[1]), t.extent,
            U));
        }
      }

      function _() {
        function e() {
          if (u(), !i.moved) {
            var e = o.event.clientX - s,
              t = o.event.clientY - d;
            i.moved = e * e + t * t > Y;
          }
          i.zoom("mouse", D(x(i.that.__zoom, i.mouse[0] = o.mouse(i.that), i.mouse[1]), i.extent, U));
        }

        function t() {
          a.on("mousemove.zoom mouseup.zoom", null), n.dragEnable(o.event.view, i.moved), u(), i.end();
        }
        if (!M && R.apply(this, arguments)) {
          var i = S(this, arguments, !0),
            a = o.select(o.event.view).on("mousemove.zoom", e, !0).on("mouseup.zoom", t, !0),
            l = o.mouse(this),
            s = o.event.clientX,
            d = o.event.clientY;
          n.dragDisable(o.event.view), c(), i.mouse = [l, this.__zoom.invert(l)], r.interrupt(this), i
            .start();
        }
      }

      function T() {
        if (R.apply(this, arguments)) {
          var t = this.__zoom,
            n = o.mouse(this),
            i = t.invert(n),
            r = t.k * (o.event.shiftKey ? .5 : 2),
            a = D(x(d(t, r), n, i), P.apply(this, arguments), U);
          u(), z > 0 ? o.select(this).transition().duration(z).call(w, a, n) : o.select(this).call(e
            .transform, a);
        }
      }

      function C() {
        if (R.apply(this, arguments)) {
          var e,
            t,
            n,
            i,
            a = o.event.touches,
            l = a.length,
            s = S(this, arguments, o.event.changedTouches.length === l);
          for (c(), t = 0; t < l; ++t) n = a[t], i = o.touch(this, a, n.identifier), i = [i, this.__zoom
            .invert(i), n.identifier
          ], s.touch0 ? s.touch1 || s.touch0[2] === i[2] || (s.touch1 = i, s.taps = 0) : (s.touch0 = i,
            e = !0, s.taps = 1 + !!I);
          I && (I = clearTimeout(I)), e && (s.taps < 2 && (I = setTimeout(function() {
            I = null;
          }, H)), r.interrupt(this), s.start());
        }
      }

      function O() {
        if (this.__zooming) {
          var e,
            t,
            n,
            i,
            r = S(this, arguments),
            a = o.event.changedTouches,
            l = a.length;
          for (u(), I && (I = clearTimeout(I)), r.taps = 0, e = 0; e < l; ++e) t = a[e], n = o.touch(this,
              a, t.identifier), r.touch0 && r.touch0[2] === t.identifier ? r.touch0[0] = n : r.touch1 && r
            .touch1[2] === t.identifier && (r.touch1[0] = n);
          if (t = r.that.__zoom, r.touch1) {
            var s = r.touch0[0],
              c = r.touch0[1],
              f = r.touch1[0],
              m = r.touch1[1],
              g = (g = f[0] - s[0]) * g + (g = f[1] - s[1]) * g,
              p = (p = m[0] - c[0]) * p + (p = m[1] - c[1]) * p;
            t = d(t, Math.sqrt(g / p)), n = [(s[0] + f[0]) / 2, (s[1] + f[1]) / 2], i = [(c[0] + m[0]) / 2,
              (c[1] + m[1]) / 2
            ];
          } else {
            if (!r.touch0) return;
            n = r.touch0[0], i = r.touch0[1];
          }
          r.zoom("touch", D(x(t, n, i), r.extent, U));
        }
      }

      function A() {
        if (this.__zooming) {
          var e,
            t,
            n = S(this, arguments),
            i = o.event.changedTouches,
            r = i.length;
          for (c(), M && clearTimeout(M), M = setTimeout(function() {
              M = null;
            }, H), e = 0; e < r; ++e) t = i[e], n.touch0 && n.touch0[2] === t.identifier ? delete n.touch0 :
            n.touch1 && n.touch1[2] === t.identifier && delete n.touch1;
          if (n.touch1 && !n.touch0 && (n.touch0 = n.touch1, delete n.touch1), n.touch0) n.touch0[1] = this
            .__zoom.invert(n.touch0[0]);
          else if (n.end(), 2 === n.taps) {
            var a = o.select(this).on("dblclick.zoom");
            a && a.apply(this, arguments);
          }
        }
      }
      var I,
        M,
        R = f,
        P = m,
        D = b,
        N = p,
        L = h,
        F = [0, 1 / 0],
        U = [
          [-(1 / 0), -(1 / 0)],
          [1 / 0, 1 / 0]
        ],
        z = 250,
        G = i.interpolateZoom,
        V = t.dispatch("start", "zoom", "end"),
        H = 500,
        B = 150,
        Y = 0;
      return e.transform = function(e, t, n) {
        var i = e.selection ? e.selection() : e;
        i.property("__zoom", g), e !== i ? w(e, t, n) : i.interrupt().each(function() {
          S(this, arguments).start().zoom(null, "function" == typeof t ? t.apply(this, arguments) : t)
            .end();
        });
      }, e.scaleBy = function(t, n, i) {
        e.scaleTo(t, function() {
          var e = this.__zoom.k,
            t = "function" == typeof n ? n.apply(this, arguments) : n;
          return e * t;
        }, i);
      }, e.scaleTo = function(t, n, i) {
        e.transform(t, function() {
          var e = P.apply(this, arguments),
            t = this.__zoom,
            o = null == i ? y(e) : "function" == typeof i ? i.apply(this, arguments) : i,
            r = t.invert(o),
            a = "function" == typeof n ? n.apply(this, arguments) : n;
          return D(x(d(t, a), o, r), e, U);
        }, i);
      }, e.translateBy = function(t, n, i) {
        e.transform(t, function() {
          return D(this.__zoom.translate("function" == typeof n ? n.apply(this, arguments) : n,
            "function" == typeof i ? i.apply(this, arguments) : i), P.apply(this, arguments), U);
        });
      }, e.translateTo = function(t, n, i, o) {
        e.transform(t, function() {
          var e = P.apply(this, arguments),
            t = this.__zoom,
            r = null == o ? y(e) : "function" == typeof o ? o.apply(this, arguments) : o;
          return D(v.translate(r[0], r[1]).scale(t.k).translate("function" == typeof n ? -n.apply(
            this, arguments) : -n, "function" == typeof i ? -i.apply(this, arguments) : -i), e, U);
        }, o);
      }, E.prototype = {
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
          o.customEvent(new l(e, t, this.that.__zoom), V.apply, V, [t, this.that, this.args]);
        }
      }, e.wheelDelta = function(t) {
        return arguments.length ? (N = "function" == typeof t ? t : a(+t), e) : N;
      }, e.filter = function(t) {
        return arguments.length ? (R = "function" == typeof t ? t : a(!!t), e) : R;
      }, e.touchable = function(t) {
        return arguments.length ? (L = "function" == typeof t ? t : a(!!t), e) : L;
      }, e.extent = function(t) {
        return arguments.length ? (P = "function" == typeof t ? t : a([
          [+t[0][0], +t[0][1]],
          [+t[1][0], +t[1][1]]
        ]), e) : P;
      }, e.scaleExtent = function(t) {
        return arguments.length ? (F[0] = +t[0], F[1] = +t[1], e) : [F[0], F[1]];
      }, e.translateExtent = function(t) {
        return arguments.length ? (U[0][0] = +t[0][0], U[1][0] = +t[1][0], U[0][1] = +t[0][1], U[1][1] = +
          t[1][1], e) : [
          [U[0][0], U[0][1]],
          [U[1][0], U[1][1]]
        ];
      }, e.constrain = function(t) {
        return arguments.length ? (D = t, e) : D;
      }, e.duration = function(t) {
        return arguments.length ? (z = +t, e) : z;
      }, e.interpolate = function(t) {
        return arguments.length ? (G = t, e) : G;
      }, e.on = function() {
        var t = V.on.apply(V, arguments);
        return t === V ? e : t;
      }, e.clickDistance = function(t) {
        return arguments.length ? (Y = (t = +t) * t, e) : Math.sqrt(Y);
      }, e;
    }
    s.prototype = {
      constructor: s,
      scale: function(e) {
        return 1 === e ? this : new s(this.k * e, this.x, this.y);
      },
      translate: function(e, t) {
        return 0 === e & 0 === t ? this : new s(this.k, this.x + this.k * e, this.y + this.k * t);
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
    var v = new s(1, 0, 0);
    d.prototype = s.prototype, e.zoom = x, e.zoomIdentity = v, e.zoomTransform = d, Object.defineProperty(e,
      "__esModule", {
        value: !0
      });
  });
}
