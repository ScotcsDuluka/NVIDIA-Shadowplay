// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 143
// role       : factory $swipe | directive ngClick | provider $touch
// defines    : angular.module("ngTouch")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  /**
   * @license AngularJS v1.5.5
   * (c) 2010-2016 Google, Inc. http://angularjs.org
   * License: MIT
   */
  ! function(e, t) {
    "use strict";

    function n(e) {
      return t.lowercase(e.nodeName || e[0] && e[0].nodeName)
    }

    function r(e, n) {
      var r = !1,
        i = !1;
      this.ngClickOverrideEnabled = function(o) {
        return t.isDefined(o) ? (o && !i && (i = !0, a.$$moduleName = "ngTouch", n.directive("ngClick", a), e
          .decorator("ngClickDirective", ["$delegate", function(e) {
            if (r) e.shift();
            else
              for (var t = e.length - 1; t >= 0;) {
                if ("ngTouch" === e[t].$$moduleName) {
                  e.splice(t, 1);
                  break
                }
                t--
              }
            return e
          }])), r = o, this) : r
      }, this.$get = function() {
        return {
          ngClickOverrideEnabled: function() {
            return r
          }
        }
      }
    }

    function i(e, n, r) {
      o.directive(e, ["$parse", "$swipe", function(i, o) {
        var a = 75,
          s = .3,
          c = 30;
        return function(u, l, d) {
          function f(e) {
            if (!h) return !1;
            var t = Math.abs(e.y - h.y),
              r = (e.x - h.x) * n;
            return p && t < a && r > 0 && r > c && t / r < s
          }
          var h, p, m = i(d[e]),
            v = ["touch"];
          t.isDefined(d.ngSwipeDisableMouse) || v.push("mouse"), o.bind(l, {
            start: function(e, t) {
              h = e, p = !0
            },
            cancel: function(e) {
              p = !1
            },
            end: function(e, t) {
              f(e) && u.$apply(function() {
                l.triggerHandler(r), m(u, {
                  $event: t
                })
              })
            }
          }, v)
        }
      }])
    }
    var o = t.module("ngTouch", []);
    o.provider("$touch", r), r.$inject = ["$provide", "$compileProvider"], o.factory("$swipe", [function() {
      function e(e) {
        var t = e.originalEvent || e,
          n = t.touches && t.touches.length ? t.touches : [t],
          r = t.changedTouches && t.changedTouches[0] || n[0];
        return {
          x: r.clientX,
          y: r.clientY
        }
      }

      function n(e, n) {
        var r = [];
        return t.forEach(e, function(e) {
          var t = i[e][n];
          t && r.push(t)
        }), r.join(" ")
      }
      var r = 10,
        i = {
          mouse: {
            start: "mousedown",
            move: "mousemove",
            end: "mouseup"
          },
          touch: {
            start: "touchstart",
            move: "touchmove",
            end: "touchend",
            cancel: "touchcancel"
          }
        };
      return {
        bind: function(t, i, o) {
          var a, s, c, u, l = !1;
          o = o || ["mouse", "touch"], t.on(n(o, "start"), function(t) {
            c = e(t), l = !0, a = 0, s = 0, u = c, i.start && i.start(c, t)
          });
          var d = n(o, "cancel");
          d && t.on(d, function(e) {
            l = !1, i.cancel && i.cancel(e)
          }), t.on(n(o, "move"), function(t) {
            if (l && c) {
              var n = e(t);
              if (a += Math.abs(n.x - u.x), s += Math.abs(n.y - u.y), u = n, !(a < r && s < r)) return s >
                a ? (l = !1, void(i.cancel && i.cancel(t))) : (t.preventDefault(), void(i.move && i.move(
                  n, t)))
            }
          }), t.on(n(o, "end"), function(t) {
            l && (l = !1, i.end && i.end(e(t), t))
          })
        }
      }
    }]);
    var a = ["$parse", "$timeout", "$rootElement", function(e, r, i) {
      function o(e, t, n, r) {
        return Math.abs(e - n) < v && Math.abs(t - r) < v
      }

      function a(e, t, n) {
        for (var r = 0; r < e.length; r += 2)
          if (o(e[r], e[r + 1], t, n)) return e.splice(r, r + 2), !0;
        return !1
      }

      function s(e) {
        if (!(Date.now() - l > m)) {
          var t = e.touches && e.touches.length ? e.touches : [e],
            r = t[0].clientX,
            i = t[0].clientY;
          r < 1 && i < 1 || f && f[0] === r && f[1] === i || (f && (f = null), "label" === n(e.target) && (f = [r,
            i]), a(d, r, i) || (e.stopPropagation(), e.preventDefault(), e.target && e.target.blur && e.target
            .blur()))
        }
      }

      function c(e) {
        var t = e.touches && e.touches.length ? e.touches : [e],
          n = t[0].clientX,
          i = t[0].clientY;
        d.push(n, i), r(function() {
          for (var e = 0; e < d.length; e += 2)
            if (d[e] == n && d[e + 1] == i) return void d.splice(e, e + 2)
        }, m, !1)
      }

      function u(e, t) {
        d || (i[0].addEventListener("click", s, !0), i[0].addEventListener("touchstart", c, !0), d = []), l = Date
          .now(), a(d, e, t)
      }
      var l, d, f, h = 750,
        p = 12,
        m = 2500,
        v = 25,
        g = "ng-click-active";
      return function(n, r, i) {
        function o() {
          f = !1, r.removeClass(g)
        }
        var a, s, c, l, d = e(i.ngClick),
          f = !1;
        r.on("touchstart", function(e) {
          f = !0, a = e.target ? e.target : e.srcElement, 3 == a.nodeType && (a = a.parentNode), r.addClass(
            g), s = Date.now();
          var t = e.originalEvent || e,
            n = t.touches && t.touches.length ? t.touches : [t],
            i = n[0];
          c = i.clientX, l = i.clientY
        }), r.on("touchcancel", function(e) {
          o()
        }), r.on("touchend", function(e) {
          var n = Date.now() - s,
            d = e.originalEvent || e,
            m = d.changedTouches && d.changedTouches.length ? d.changedTouches : d.touches && d.touches
            .length ? d.touches : [d],
            v = m[0],
            g = v.clientX,
            y = v.clientY,
            b = Math.sqrt(Math.pow(g - c, 2) + Math.pow(y - l, 2));
          f && n < h && b < p && (u(g, y), a && a.blur(), t.isDefined(i.disabled) && i.disabled !== !1 || r
            .triggerHandler("click", [e])), o()
        }), r.onclick = function(e) {}, r.on("click", function(e, t) {
          n.$apply(function() {
            d(n, {
              $event: t || e
            })
          })
        }), r.on("mousedown", function(e) {
          r.addClass(g)
        }), r.on("mousemove mouseup", function(e) {
          r.removeClass(g)
        })
      }
    }];
    i("ngSwipeLeft", -1, "swipeleft"), i("ngSwipeRight", 1, "swiperight")
  }(window, window.angular)
}
