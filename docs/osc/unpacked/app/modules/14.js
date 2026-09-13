// ─────────────────────────────────────────────────────────────
// APP MODULE 14
// role       : directive nvSlider
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvSlider = void 0;
  var i = n(1),
    o = i.ngMainModule.directive("nvSlider", ["$$rAF", "$window", "$mdAria", "$mdUtil", "$mdConstant", "$mdGesture",
      "$parse", "$log", "$timeout", "OSC_KEYBOARD",
      function(e, t, n, i, o, r, a, l, s, d) {
        function c(n, c, u, f) {
          function m(e) {
            return Math.max(0, Math.min(e || 0, 1))
          }

          function g(e) {
            return Math.max(0, Math.min(1, (e - ce.left) / ce.width))
          }

          function p(e) {
            return q + e * (X - q)
          }

          function h(e) {
            return (e - q) / (X - q) || 0
          }

          function b(e) {
            if (ge || (Q = q, J = X), angular.isNumber(e)) return Math.max(Q, Math.min(J, e))
          }

          function x(e) {
            if (angular.isNumber(e)) {
              var t = Math.round((e - q) / ee) * ee + q;
              return Math.round(1e3 * t) / 1e3
            }
          }

          function v(e) {
            e = m(e);
            var t = te[0].offsetWidth;
            if (t <= 0) return e;
            var n = t / 2 / ce.width;
            return e < n ? e = n : e > 1 - n && (e = 1 - n), e
          }

          function y(e) {
            e = he ? v(e) : m(e);
            var t = 100 * e + "%";
            if (ne.css("left", t), !ge || me) return void oe.css("width", "0");
            var n = m((Q - q) / (X - q)) || 0,
              i = 100 * n + "%",
              o = m((J - Q) / (X - q)) || 0,
              r = 100 * o + "%";
            oe.css("left", i), oe.css("width", r)
          }

          function w(e, t) {
            if (f.$setViewValue(b(x(e))), t = t || !1, be && !t && angular.isDefined(Z) && angular.isDefined(X) &&
              angular.isDefined(q)) {
              if (Z === q || Z === X) return;
              var n = (X - q) / 20;
              f.$viewValue > Z - n && f.$viewValue < Z + n && f.$setViewValue(b(x(Z)))
            }
          }

          function S() {
            f.$viewValue = f.$modelValue;
            var e = (f.$viewValue - q) / (X - q) || 0;
            n.modelValue = f.$viewValue, c.attr("aria-valuenow", f.$viewValue), y(e)
          }

          function E(e) {
            n.$evalAsync(function() {
              w(p(g(e)))
            })
          }

          function k(e) {
            var t = p(g(e));
            b(x(t));
            y(g(e))
          }

          function _(e) {
            fe ? k(e.pointer.x) : E(e.pointer.x)
          }

          function T() {
            ce = ie[0].getBoundingClientRect()
          }

          function C() {
            return de(), ce
          }

          function O() {
            if ((angular.isDefined(u.mdDiscrete) || angular.isDefined(u.ticks)) && angular.isDefined(X) && angular
              .isDefined(q) && angular.isDefined(ee) && "" !== u.ticks) {
              if (ee <= 0) {
                var e = "Slider step value must be greater than zero when in discrete mode";
                throw l.error(e), new Error(e)
              }
              var n = Math.floor((X - q) / ee);
              ae || (ae = angular.element("<canvas>"), re.append(ae), le = ae[0].getContext("2d"));
              var i = C();
              if (0 === i.width) return void s(I, 5);
              ae[0].width = i.width, ae[0].height = i.height;
              var o = t.getComputedStyle(re[0]);
              se = parseInt(o.lineHeight, 10) || 14, angular.isDefined(u.ticks) && (ae[0].height += 2 * se), le.font =
                o.font || "12px Segoe UI";
              for (var r, a = 0; a <= n; a++)
                if (r = Math.ceil(i.width * (a / n)), a === n && (r = Math.floor(i.width)), angular.isDefined(u
                    .mdDiscrete) && (le.fillStyle = o.backgroundColor || "black", le.fillRect(r - 1, 0, 2, i.height)),
                  angular.isDefined(u.ticks)) {
                  var d = JSON.parse(u.ticks),
                    c = a * ee + q;
                  if (d.indexOf(c) < 0) continue;
                  0 === a ? le.textAlign = "left" : a === n ? le.textAlign = "right" : le.textAlign = "center";
                  var f = 0;
                  le.fillStyle = o.color || "white", le.textBaseline = "top", le.fillText("|", r, f), le.fillText(c,
                    r, f + se)
                }
            }
          }

          function A() {
            q && X && (Q && (Q = Math.max(q, Math.min(X, Q))), J && (J = Math.max(q, Math.min(X, J))), Q > f
              .$viewValue && w(Q), J < f.$viewValue && w(J))
          }

          function I() {
            A(), T(), S(), O()
          }

          function M(e) {
            q = parseFloat(e), c.attr("aria-valuemin", e), I()
          }

          function R(e) {
            X = parseFloat(e), c.attr("aria-valuemax", e), I()
          }

          function P(e) {
            ee = parseFloat(e), O()
          }

          function D() {
            O()
          }

          function N(e) {
            Z = parseFloat(e), I()
          }

          function L(e) {
            be = !!e
          }

          function F(e) {
            xe = !!e
          }

          function U(e) {
            Q = parseFloat(e), I()
          }

          function z(e) {
            J = parseFloat(e), I()
          }

          function G(e) {
            me = "true" === e, I()
          }

          function V(e) {
            c.attr("aria-disabled", !!e)
          }

          function H(e) {
            if (!ve()) {
              if (e.keyCode === d.PERIOD && angular.isDefined(u.defaultValue)) return f.$viewValue = Z, e
                .preventDefault(), e.stopPropagation(), void n.$evalAsync(function() {
                  w(f.$viewValue)
                });
              var t;
              e.keyCode === o.KEY_CODE.LEFT_ARROW ? t = -ee : e.keyCode === o.KEY_CODE.RIGHT_ARROW && (t = ee), t && (
                (e.metaKey || e.ctrlKey || e.altKey) && (t *= 4), e.preventDefault(), e.stopPropagation(), n
                .$evalAsync(function() {
                  w(f.$viewValue + t, xe)
                }))
            }
          }

          function B() {
            ve() || c[0].focus()
          }

          function Y(e) {
            if (!ve()) {
              c.addClass("nv-active"), c[0].focus(), T();
              var t = p(g(e.pointer.x)),
                i = b(x(t));
              n.$apply(function() {
                w(i), y(h(i))
              })
            }
          }

          function $(e) {
            if (!ve()) {
              c.removeClass("nv-dragging nv-active");
              var t = p(g(e.pointer.x)),
                i = b(x(t));
              n.$apply(function() {
                w(i), S()
              })
            }
          }

          function W(e) {
            ve() || (ue = !0, e.stopPropagation(), c.addClass("nv-dragging"), _(e))
          }

          function j(e) {
            ue && (e.stopPropagation(), _(e))
          }

          function K(e) {
            ue && (e.stopPropagation(), ue = !1)
          }
          var q, X, Z, Q, J, ee, te, ne, ie, oe, re, ae, le, se, de, ce = {},
            ue = !1,
            fe = angular.isDefined(u.mdDiscrete),
            me = !1,
            ge = !1,
            pe = !1,
            he = !0,
            be = void 0,
            xe = void 0;
          l.getInstance("nvSlider");
          f = f || {
            $setViewValue: function(e) {
              this.$viewValue = e, this.$viewChangeListeners.forEach(function(e) {
                e()
              })
            },
            $parsers: [],
            $formatters: [],
            $viewChangeListeners: []
          };
          var ve = angular.noop;
          null != u.disabled ? ve = function() {
              return !0
            } : u.ngDisabled && (ve = angular.bind(null, a(u.ngDisabled), n.$parent)), te = angular.element(c[0]
              .querySelector(".nv-thumb")), ne = te.parent(), ie = angular.element(c[0].querySelector(
              ".nv-track-container")), oe = angular.element(c[0].querySelector(".nv-track-fill")), re = angular
            .element(c[0].querySelector(".nv-track-ticks")), de = i.throttle(T, 5e3), angular.isDefined(u
              .activeTrackHidden) ? u.$observe("activeTrackHidden", G) : G("false"), angular.isDefined(u.min) ? u
            .$observe("min", M) : M(0), angular.isDefined(u.max) ? u.$observe("max", R) : R(100), angular.isDefined(u
              .step) ? u.$observe("step", P) : P(1), angular.isDefined(u.ticks) && u.$observe("ticks", D), angular
            .isDefined(u.defaultValue) && u.$observe("defaultValue", N), angular.isDefined(u.snapToDefault) && u
            .$observe("snapToDefault", L), angular.isDefined(u.skipSnapWhenKbInput) && u.$observe(
              "skipSnapWhenKbInput", F), angular.isDefined(u.rangeMin) ? (u.$observe("rangeMin", U), ge = !0) : Q = q,
            angular.isDefined(u.rangeMax) ? u.$observe("rangeMax", z) : J = X, angular.isDefined(u.onlyThumbs) && (
              pe = !0), angular.isDefined(u.dontClampThumb) && (he = !1);
          var ye = angular.noop;
          u.ngDisabled && (ye = n.$parent.$watch(u.ngDisabled, V)), r.register(c, "drag"), c.on("keydown", H).on(
            "$md.dragstart", W).on("$md.drag", j).on("$md.dragend", K), pe ? te.on("mouseenter", B).on(
            "$md.pressdown", Y).on("$md.pressup", $) : c.on("mouseenter", B).on("$md.pressdown", Y).on(
            "$md.pressup", $), s(I, 0);
          var we = e.throttle(I);
          angular.element(t).on("resize", we), n.$on("$destroy", function() {
            angular.element(t).off("resize", we), ye()
          }), f.$render = S, f.$viewChangeListeners.push(S), f.$formatters.push(x), T()
        }

        function u(e, t) {
          return t.tabindex || e.attr("tabindex", 0), e.attr("role", "slider"), n.expect(e, "aria-label"), c
        }
        return {
          scope: {},
          require: "?ngModel",
          template: '<div class="nv-slider-wrapper">  <div class="nv-track-container">    <div class="nv-track"></div>    <div class="nv-track nv-track-fill"></div>    <div class="nv-track-ticks"></div>  </div>  <div class="nv-thumb-container">    <div class="nv-thumb"></div>  </div></div>',
          compile: u
        }
      }
    ]);
  /*!
   * Angular Material Design
   * https://github.com/angular/material
   * @license MIT
   * v1.0.5
   */
  t.nvSlider = o
}
