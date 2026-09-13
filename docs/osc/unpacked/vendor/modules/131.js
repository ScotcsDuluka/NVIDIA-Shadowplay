// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 131
// role       : factory $$rAFScheduler | directive ngAnimateSwap | directive ngAnimateChildren | provider $$animateQueue | provider $$animation | provider $animateCss
// defines    : angular.module("ngAnimate")
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

    function n(e, t, n) {
      if (!e) throw me("areq", "Argument '{0}' is {1}", t || "?", n || "required");
      return e
    }

    function r(e, t) {
      return e || t ? e ? t ? (B(e) && (e = e.join(" ")), B(t) && (t = t.join(" ")), e + " " + t) : e : t : ""
    }

    function i(e) {
      var t = {};
      return e && (e.to || e.from) && (t.to = e.to, t.from = e.from), t
    }

    function o(e, t, n) {
      var r = "";
      return e = B(e) ? e : e && z(e) && e.length ? e.split(/\s+/) : [], H(e, function(e, i) {
        e && e.length > 0 && (r += i > 0 ? " " : "", r += n ? t + e : e + t)
      }), r
    }

    function a(e, t) {
      var n = e.indexOf(t);
      t >= 0 && e.splice(n, 1)
    }

    function s(e) {
      if (e instanceof j) switch (e.length) {
        case 0:
          return [];
        case 1:
          if (e[0].nodeType === K) return e;
          break;
        default:
          return j(c(e))
      }
      if (e.nodeType === K) return j(e)
    }

    function c(e) {
      if (!e[0]) return e;
      for (var t = 0; t < e.length; t++) {
        var n = e[t];
        if (n.nodeType == K) return n
      }
    }

    function u(e, t, n) {
      H(t, function(t) {
        e.addClass(t, n)
      })
    }

    function l(e, t, n) {
      H(t, function(t) {
        e.removeClass(t, n)
      })
    }

    function d(e) {
      return function(t, n) {
        n.addClass && (u(e, t, n.addClass), n.addClass = null), n.removeClass && (l(e, t, n.removeClass), n
          .removeClass = null)
      }
    }

    function f(e) {
      if (e = e || {}, !e.$$prepared) {
        var t = e.domOperation || L;
        e.domOperation = function() {
          e.$$domOperationFired = !0, t(), t = L
        }, e.$$prepared = !0
      }
      return e
    }

    function h(e, t) {
      p(e, t), m(e, t)
    }

    function p(e, t) {
      t.from && (e.css(t.from), t.from = null)
    }

    function m(e, t) {
      t.to && (e.css(t.to), t.to = null)
    }

    function v(e, t, n) {
      var r = t.options || {},
        i = n.options || {},
        o = (r.addClass || "") + " " + (i.addClass || ""),
        a = (r.removeClass || "") + " " + (i.removeClass || ""),
        s = g(e.attr("class"), o, a);
      i.preparationClasses && (r.preparationClasses = T(i.preparationClasses, r.preparationClasses), delete i
        .preparationClasses);
      var c = r.domOperation !== L ? r.domOperation : null;
      return F(r, i), c && (r.domOperation = c), s.addClass ? r.addClass = s.addClass : r.addClass = null, s
        .removeClass ? r.removeClass = s.removeClass : r.removeClass = null, t.addClass = r.addClass, t.removeClass = r
        .removeClass, r
    }

    function g(e, t, n) {
      function r(e) {
        z(e) && (e = e.split(" "));
        var t = {};
        return H(e, function(e) {
          e.length && (t[e] = !0)
        }), t
      }
      var i = 1,
        o = -1,
        a = {};
      e = r(e), t = r(t), H(t, function(e, t) {
        a[t] = i
      }), n = r(n), H(n, function(e, t) {
        a[t] = a[t] === i ? null : o
      });
      var s = {
        addClass: "",
        removeClass: ""
      };
      return H(a, function(t, n) {
        var r, a;
        t === i ? (r = "addClass", a = !e[n]) : t === o && (r = "removeClass", a = e[n]), a && (s[r].length && (s[
          r] += " "), s[r] += n)
      }), s
    }

    function y(e) {
      return e instanceof t.element ? e[0] : e
    }

    function b(e, t, n) {
      var r = "";
      t && (r = o(t, J, !0)), n.addClass && (r = T(r, o(n.addClass, X))), n.removeClass && (r = T(r, o(n.removeClass,
        Q))), r.length && (n.preparationClasses = r, e.addClass(r))
    }

    function E(e, t) {
      t.preparationClasses && (e.removeClass(t.preparationClasses), t.preparationClasses = null), t.activeClasses && (e
        .removeClass(t.activeClasses), t.activeClasses = null)
    }

    function _(e, t) {
      var n = t ? "-" + t + "s" : "";
      return w(e, [he, n]), [he, n]
    }

    function $(e, t) {
      var n = t ? "paused" : "",
        r = R + ue;
      return w(e, [r, n]), [r, n]
    }

    function w(e, t) {
      var n = t[0],
        r = t[1];
      e.style[n] = r
    }

    function T(e, t) {
      return e ? t ? e + " " + t : e : t
    }

    function C(e) {
      return [fe, e + "s"]
    }

    function x(e, t) {
      var n = t ? de : he;
      return [n, e + "s"]
    }

    function S(e, t, n) {
      var r = Object.create(null),
        i = e.getComputedStyle(t) || {};
      return H(n, function(e, t) {
        var n = i[e];
        if (n) {
          var o = n.charAt(0);
          ("-" === o || "+" === o || o >= 0) && (n = A(n)), 0 === n && (n = null), r[t] = n
        }
      }), r
    }

    function A(e) {
      var t = 0,
        n = e.split(/\s*,\s*/);
      return H(n, function(e) {
        "s" == e.charAt(e.length - 1) && (e = e.substring(0, e.length - 1)), e = parseFloat(e) || 0, t = t ? Math
          .max(e, t) : e
      }), t
    }

    function M(e) {
      return 0 === e || null != e
    }

    function k(e, t) {
      var n = O,
        r = e + "s";
      return t ? n += ie : r += " linear all", [n, r]
    }

    function N() {
      var e = Object.create(null);
      return {
        flush: function() {
          e = Object.create(null)
        },
        count: function(t) {
          var n = e[t];
          return n ? n.total : 0
        },
        get: function(t) {
          var n = e[t];
          return n && n.value
        },
        put: function(t, n) {
          e[t] ? e[t].total++ : e[t] = {
            total: 1,
            value: n
          }
        }
      }
    }

    function I(e, t, n) {
      H(n, function(n) {
        e[n] = V(e[n]) ? e[n] : t.style.getPropertyValue(n)
      })
    }
    var O, D, R, P, L = t.noop,
      U = t.copy,
      F = t.extend,
      j = t.element,
      H = t.forEach,
      B = t.isArray,
      z = t.isString,
      q = t.isObject,
      G = t.isUndefined,
      V = t.isDefined,
      W = t.isFunction,
      Y = t.isElement,
      K = 1,
      X = "-add",
      Q = "-remove",
      J = "ng-",
      Z = "-active",
      ee = "-prepare",
      te = "ng-animate",
      ne = "$$ngAnimateChildren",
      re = "";
    G(e.ontransitionend) && V(e.onwebkittransitionend) ? (re = "-webkit-", O = "WebkitTransition", D =
      "webkitTransitionEnd transitionend") : (O = "transition", D = "transitionend"), G(e.onanimationend) && V(e
      .onwebkitanimationend) ? (re = "-webkit-", R = "WebkitAnimation", P = "webkitAnimationEnd animationend") : (R =
      "animation", P = "animationend");
    var ie = "Duration",
      oe = "Property",
      ae = "Delay",
      se = "TimingFunction",
      ce = "IterationCount",
      ue = "PlayState",
      le = 9999,
      de = R + ae,
      fe = R + ie,
      he = O + ae,
      pe = O + ie,
      me = t.$$minErr("ng"),
      ve = ["$$rAF", function(e) {
        function t(e) {
          r = r.concat(e), n()
        }

        function n() {
          if (r.length) {
            for (var t = r.shift(), o = 0; o < t.length; o++) t[o]();
            i || e(function() {
              i || n()
            })
          }
        }
        var r, i;
        return r = t.queue = [], t.waitUntilQuiet = function(t) {
          i && i(), i = e(function() {
            i = null, t(), n()
          })
        }, t
      }],
      ge = ["$interpolate", function(e) {
        return {
          link: function(n, r, i) {
            function o(e) {
              e = "on" === e || "true" === e, r.data(ne, e)
            }
            var a = i.ngAnimateChildren;
            t.isString(a) && 0 === a.length ? r.data(ne, !0) : (o(e(a)(n)), i.$observe("ngAnimateChildren", o))
          }
        }
      }],
      ye = "$$animateCss",
      be = 1e3,
      Ee = 3,
      _e = 1.5,
      $e = {
        transitionDuration: pe,
        transitionDelay: he,
        transitionProperty: O + oe,
        animationDuration: fe,
        animationDelay: de,
        animationIterationCount: R + ce
      },
      we = {
        transitionDuration: pe,
        transitionDelay: he,
        animationDuration: fe,
        animationDelay: de
      },
      Te = ["$animateProvider", function(e) {
        var t = N(),
          n = N();
        this.$get = ["$window", "$$jqLite", "$$AnimateRunner", "$timeout", "$$forceReflow", "$sniffer",
          "$$rAFScheduler", "$$animateQueue",
          function(e, r, s, c, u, l, v, g) {
            function b(e, t) {
              var n = "$$ngAnimateParentKey",
                r = e.parentNode,
                i = r[n] || (r[n] = ++j);
              return i + "-" + e.getAttribute("class") + "-" + t
            }

            function E(n, r, i, o) {
              var a = t.get(i);
              return a || (a = S(e, n, o), "infinite" === a.animationIterationCount && (a.animationIterationCount =
                1)), t.put(i, a), a
            }

            function T(i, a, s, c) {
              var u;
              if (t.count(s) > 0 && (u = n.get(s), !u)) {
                var l = o(a, "-stagger");
                r.addClass(i, l), u = S(e, i, c), u.animationDuration = Math.max(u.animationDuration, 0), u
                  .transitionDuration = Math.max(u.transitionDuration, 0), r.removeClass(i, l), n.put(s, u)
              }
              return u || {}
            }

            function A(e) {
              z.push(e), v.waitUntilQuiet(function() {
                t.flush(), n.flush();
                for (var e = u(), r = 0; r < z.length; r++) z[r](e);
                z.length = 0
              })
            }

            function N(e, t, n) {
              var r = E(e, t, n, $e),
                i = r.animationDelay,
                o = r.transitionDelay;
              return r.maxDelay = i && o ? Math.max(i, o) : i || o, r.maxDuration = Math.max(r.animationDuration * r
                .animationIterationCount, r.transitionDuration), r
            }
            var F = d(r),
              j = 0,
              z = [];
            return function(e, n) {
              function u() {
                v()
              }

              function d() {
                v(!0)
              }

              function v(t) {
                if (!(W || K && Y)) {
                  W = !0, Y = !1, q.$$skipPreparationClasses || r.removeClass(e, $e), r.removeClass(e, Ce), $(V, !
                    1), _(V, !1), H(ue, function(e) {
                    V.style[e[0]] = ""
                  }), F(e, q), h(e, q), Object.keys(G).length && H(G, function(e, t) {
                    e ? V.style.setProperty(t, e) : V.style.removeProperty(t)
                  }), q.onDone && q.onDone(), he && he.length && e.off(he.join(" "), j);
                  var n = e.data(ye);
                  n && (c.cancel(n[0].timer), e.removeData(ye)), ee && ee.complete(!t)
                }
              }

              function E(e) {
                Fe.blockTransition && _(V, e), Fe.blockKeyframeAnimation && $(V, !!e)
              }

              function S() {
                return ee = new s({
                  end: u,
                  cancel: d
                }), A(L), v(), {
                  $$willAnimate: !1,
                  start: function() {
                    return ee
                  },
                  end: u
                }
              }

              function j(e) {
                e.stopPropagation();
                var t = e.originalEvent || e,
                  n = t.$manualTimeStamp || Date.now(),
                  r = parseFloat(t.elapsedTime.toFixed(Ee));
                Math.max(n - ce, 0) >= re && r >= ie && (K = !0, v())
              }

              function z() {
                function t() {
                  if (!W) {
                    if (E(!1), H(ue, function(e) {
                        var t = e[0],
                          n = e[1];
                        V.style[t] = n
                      }), F(e, q), r.addClass(e, Ce), Fe.recalculateTimingStyles) {
                      if (Te = V.className + " " + $e, Ae = b(V, Te), Le = N(V, Te, Ae), Ue = Le.maxDelay, ne =
                        Math.max(Ue, 0), ie = Le.maxDuration, 0 === ie) return void v();
                      Fe.hasTransitions = Le.transitionDuration > 0, Fe.hasAnimations = Le.animationDuration > 0
                    }
                    if (Fe.applyAnimationDelay && (Ue = "boolean" != typeof q.delay && M(q.delay) ? parseFloat(q
                          .delay) : Ue, ne = Math.max(Ue, 0), Le.animationDelay = Ue, je = x(Ue, !0), ue.push(je),
                        V.style[je[0]] = je[1]), re = ne * be, ae = ie * be, q.easing) {
                      var t, i = q.easing;
                      Fe.hasTransitions && (t = O + se, ue.push([t, i]), V.style[t] = i), Fe.hasAnimations && (t =
                        R + se, ue.push([t, i]), V.style[t] = i)
                    }
                    Le.transitionDuration && he.push(D), Le.animationDuration && he.push(P), ce = Date.now();
                    var o = re + _e * ae,
                      a = ce + o,
                      s = e.data(ye) || [],
                      u = !0;
                    if (s.length) {
                      var l = s[0];
                      u = a > l.expectedEndTime, u ? c.cancel(l.timer) : s.push(v)
                    }
                    if (u) {
                      var d = c(n, o, !1);
                      s[0] = {
                        timer: d,
                        expectedEndTime: a
                      }, s.push(v), e.data(ye, s)
                    }
                    he.length && e.on(he.join(" "), j), q.to && (q.cleanupStyles && I(G, V, Object.keys(q.to)), m(
                      e, q))
                  }
                }

                function n() {
                  var t = e.data(ye);
                  if (t) {
                    for (var n = 1; n < t.length; n++) t[n]();
                    e.removeData(ye)
                  }
                }
                if (!W) {
                  if (!V.parentNode) return void v();
                  var i = function(e) {
                      if (K) Y && e && (Y = !1, v());
                      else if (Y = !e, Le.animationDuration) {
                        var t = $(V, Y);
                        Y ? ue.push(t) : a(ue, t)
                      }
                    },
                    o = Re > 0 && (Le.transitionDuration && 0 === Me.transitionDuration || Le.animationDuration &&
                      0 === Me.animationDuration) && Math.max(Me.animationDelay, Me.transitionDelay);
                  o ? c(t, Math.floor(o * Re * be), !1) : t(), te.resume = function() {
                    i(!0)
                  }, te.pause = function() {
                    i(!1)
                  }
                }
              }
              var q = n || {};
              q.$$prepared || (q = f(U(q)));
              var G = {},
                V = y(e);
              if (!V || !V.parentNode || !g.enabled()) return S();
              var W, Y, K, ee, te, ne, re, ie, ae, ce, ue = [],
                de = e.attr("class"),
                fe = i(q),
                he = [];
              if (0 === q.duration || !l.animations && !l.transitions) return S();
              var pe = q.event && B(q.event) ? q.event.join(" ") : q.event,
                me = pe && q.structural,
                ve = "",
                ge = "";
              me ? ve = o(pe, J, !0) : pe && (ve = pe), q.addClass && (ge += o(q.addClass, X)), q.removeClass && (
                ge.length && (ge += " "), ge += o(q.removeClass, Q)), q.applyClassesEarly && ge.length && F(e,
                q);
              var $e = [ve, ge].join(" ").trim(),
                Te = de + " " + $e,
                Ce = o($e, Z),
                xe = fe.to && Object.keys(fe.to).length > 0,
                Se = (q.keyframeStyle || "").length > 0;
              if (!Se && !xe && !$e) return S();
              var Ae, Me;
              if (q.stagger > 0) {
                var ke = parseFloat(q.stagger);
                Me = {
                  transitionDelay: ke,
                  animationDelay: ke,
                  transitionDuration: 0,
                  animationDuration: 0
                }
              } else Ae = b(V, Te), Me = T(V, $e, Ae, we);
              q.$$skipPreparationClasses || r.addClass(e, $e);
              var Ne;
              if (q.transitionStyle) {
                var Ie = [O, q.transitionStyle];
                w(V, Ie), ue.push(Ie)
              }
              if (q.duration >= 0) {
                Ne = V.style[O].length > 0;
                var Oe = k(q.duration, Ne);
                w(V, Oe), ue.push(Oe)
              }
              if (q.keyframeStyle) {
                var De = [R, q.keyframeStyle];
                w(V, De), ue.push(De)
              }
              var Re = Me ? q.staggerIndex >= 0 ? q.staggerIndex : t.count(Ae) : 0,
                Pe = 0 === Re;
              Pe && !q.skipBlocking && _(V, le);
              var Le = N(V, Te, Ae),
                Ue = Le.maxDelay;
              ne = Math.max(Ue, 0), ie = Le.maxDuration;
              var Fe = {};
              if (Fe.hasTransitions = Le.transitionDuration > 0, Fe.hasAnimations = Le.animationDuration > 0, Fe
                .hasTransitionAll = Fe.hasTransitions && "all" == Le.transitionProperty, Fe
                .applyTransitionDuration = xe && (Fe.hasTransitions && !Fe.hasTransitionAll || Fe.hasAnimations &&
                  !Fe.hasTransitions), Fe.applyAnimationDuration = q.duration && Fe.hasAnimations, Fe
                .applyTransitionDelay = M(q.delay) && (Fe.applyTransitionDuration || Fe.hasTransitions), Fe
                .applyAnimationDelay = M(q.delay) && Fe.hasAnimations, Fe.recalculateTimingStyles = ge.length > 0,
                (Fe.applyTransitionDuration || Fe.applyAnimationDuration) && (ie = q.duration ? parseFloat(q
                    .duration) : ie, Fe.applyTransitionDuration && (Fe.hasTransitions = !0, Le
                    .transitionDuration = ie, Ne = V.style[O + oe].length > 0, ue.push(k(ie, Ne))), Fe
                  .applyAnimationDuration && (Fe.hasAnimations = !0, Le.animationDuration = ie, ue.push(C(ie)))),
                0 === ie && !Fe.recalculateTimingStyles) return S();
              if (null != q.delay) {
                var je;
                "boolean" != typeof q.delay && (je = parseFloat(q.delay), ne = Math.max(je, 0)), Fe
                  .applyTransitionDelay && ue.push(x(je)), Fe.applyAnimationDelay && ue.push(x(je, !0))
              }
              return null == q.duration && Le.transitionDuration > 0 && (Fe.recalculateTimingStyles = Fe
                  .recalculateTimingStyles || Pe), re = ne * be, ae = ie * be, q.skipBlocking || (Fe
                  .blockTransition = Le.transitionDuration > 0, Fe.blockKeyframeAnimation = Le.animationDuration >
                  0 && Me.animationDelay > 0 && 0 === Me.animationDuration), q.from && (q.cleanupStyles && I(G, V,
                  Object.keys(q.from)), p(e, q)), Fe.blockTransition || Fe.blockKeyframeAnimation ? E(ie) : q
                .skipBlocking || _(V, !1), {
                  $$willAnimate: !0,
                  end: u,
                  start: function() {
                    if (!W) return te = {
                      end: u,
                      cancel: d,
                      resume: null,
                      pause: null
                    }, ee = new s(te), A(z), ee
                  }
                }
            }
          }
        ]
      }],
      Ce = ["$$animationProvider", function(e) {
        function t(e) {
          return e.parentNode && 11 === e.parentNode.nodeType
        }
        e.drivers.push("$$animateCssDriver");
        var n = "ng-animate-shim",
          r = "ng-anchor",
          i = "ng-anchor-out",
          o = "ng-anchor-in";
        this.$get = ["$animateCss", "$rootScope", "$$AnimateRunner", "$rootElement", "$sniffer", "$$jqLite",
          "$document",
          function(e, a, s, c, u, l, f) {
            function h(e) {
              return e.replace(/\bng-\S+\b/g, "")
            }

            function p(e, t) {
              return z(e) && (e = e.split(" ")), z(t) && (t = t.split(" ")), e.filter(function(e) {
                return t.indexOf(e) === -1
              }).join(" ")
            }

            function m(t, a, c) {
              function u(e) {
                var t = {},
                  n = y(e).getBoundingClientRect();
                return H(["width", "height", "top", "left"], function(e) {
                  var r = n[e];
                  switch (e) {
                    case "top":
                      r += b.scrollTop;
                      break;
                    case "left":
                      r += b.scrollLeft
                  }
                  t[e] = Math.floor(r) + "px"
                }), t
              }

              function l() {
                var t = e(v, {
                  addClass: i,
                  delay: !0,
                  from: u(a)
                });
                return t.$$willAnimate ? t : null
              }

              function d(e) {
                return e.attr("class") || ""
              }

              function f() {
                var t = h(d(c)),
                  n = p(t, g),
                  r = p(g, t),
                  a = e(v, {
                    to: u(c),
                    addClass: o + " " + n,
                    removeClass: i + " " + r,
                    delay: !0
                  });
                return a.$$willAnimate ? a : null
              }

              function m() {
                v.remove(), a.removeClass(n), c.removeClass(n)
              }
              var v = j(y(a).cloneNode(!0)),
                g = h(d(v));
              a.addClass(n), c.addClass(n), v.addClass(r), _.append(v);
              var E, $ = l();
              if (!$ && (E = f(), !E)) return m();
              var w = $ || E;
              return {
                start: function() {
                  function e() {
                    n && n.end()
                  }
                  var t, n = w.start();
                  return n.done(function() {
                    return n = null, !E && (E = f()) ? (n = E.start(), n.done(function() {
                      n = null, m(), t.complete()
                    }), n) : (m(), void t.complete())
                  }), t = new s({
                    end: e,
                    cancel: e
                  })
                }
              }
            }

            function v(e, t, n, r) {
              var i = g(e, L),
                o = g(t, L),
                a = [];
              if (H(r, function(e) {
                  var t = e.out,
                    r = e.in,
                    i = m(n, t, r);
                  i && a.push(i)
                }), i || o || 0 !== a.length) return {
                start: function() {
                  function e() {
                    H(t, function(e) {
                      e.end()
                    })
                  }
                  var t = [];
                  i && t.push(i.start()), o && t.push(o.start()), H(a, function(e) {
                    t.push(e.start())
                  });
                  var n = new s({
                    end: e,
                    cancel: e
                  });
                  return s.all(t, function(e) {
                    n.complete(e)
                  }), n
                }
              }
            }

            function g(t) {
              var n = t.element,
                r = t.options || {};
              t.structural && (r.event = t.event, r.structural = !0, r.applyClassesEarly = !0, "leave" === t
                .event && (r.onDone = r.domOperation)), r.preparationClasses && (r.event = T(r.event, r
                .preparationClasses));
              var i = e(n, r);
              return i.$$willAnimate ? i : null
            }
            if (!u.animations && !u.transitions) return L;
            var b = f[0].body,
              E = y(c),
              _ = j(t(E) || b.contains(E) ? E : b);
            d(l);
            return function(e) {
              return e.from && e.to ? v(e.from, e.to, e.classes, e.anchors) : g(e)
            }
          }
        ]
      }],
      xe = ["$animateProvider", function(e) {
        this.$get = ["$injector", "$$AnimateRunner", "$$jqLite", function(t, n, r) {
          function i(n) {
            n = B(n) ? n : n.split(" ");
            for (var r = [], i = {}, o = 0; o < n.length; o++) {
              var a = n[o],
                s = e.$$registeredAnimations[a];
              s && !i[a] && (r.push(t.get(s)), i[a] = !0)
            }
            return r
          }
          var o = d(r);
          return function(e, t, r, a) {
            function s() {
              a.domOperation(), o(e, a)
            }

            function c() {
              p = !0, s(), h(e, a)
            }

            function u(e, t, r, i, o) {
              var a;
              switch (r) {
                case "animate":
                  a = [t, i.from, i.to, o];
                  break;
                case "setClass":
                  a = [t, g, y, o];
                  break;
                case "addClass":
                  a = [t, g, o];
                  break;
                case "removeClass":
                  a = [t, y, o];
                  break;
                default:
                  a = [t, o]
              }
              a.push(i);
              var s = e.apply(e, a);
              if (s)
                if (W(s.start) && (s = s.start()), s instanceof n) s.done(o);
                else if (W(s)) return s;
              return L
            }

            function l(e, t, r, i, o) {
              var a = [];
              return H(i, function(i) {
                var s = i[o];
                s && a.push(function() {
                  var i, o, a = !1,
                    c = function(e) {
                      a || (a = !0, (o || L)(e), i.complete(!e))
                    };
                  return i = new n({
                    end: function() {
                      c()
                    },
                    cancel: function() {
                      c(!0)
                    }
                  }), o = u(s, e, t, r, function(e) {
                    var t = e === !1;
                    c(t)
                  }), i
                })
              }), a
            }

            function d(e, t, r, i, o) {
              var a = l(e, t, r, i, o);
              if (0 === a.length) {
                var s, c;
                "beforeSetClass" === o ? (s = l(e, "removeClass", r, i, "beforeRemoveClass"), c = l(e,
                  "addClass", r, i, "beforeAddClass")) : "setClass" === o && (s = l(e, "removeClass", r, i,
                  "removeClass"), c = l(e, "addClass", r, i, "addClass")), s && (a = a.concat(s)), c && (a = a
                  .concat(c))
              }
              if (0 !== a.length) return function(e) {
                var t = [];
                return a.length && H(a, function(e) {
                    t.push(e())
                  }), t.length ? n.all(t, e) : e(),
                  function(e) {
                    H(t, function(t) {
                      e ? t.cancel() : t.end()
                    })
                  }
              }
            }
            var p = !1;
            3 === arguments.length && q(r) && (a = r, r = null), a = f(a), r || (r = e.attr("class") || "", a
              .addClass && (r += " " + a.addClass), a.removeClass && (r += " " + a.removeClass));
            var m, v, g = a.addClass,
              y = a.removeClass,
              b = i(r);
            if (b.length) {
              var E, _;
              "leave" == t ? (_ = "leave", E = "afterLeave") : (_ = "before" + t.charAt(0).toUpperCase() + t
                .substr(1), E = t), "enter" !== t && "move" !== t && (m = d(e, t, a, b, _)), v = d(e, t, a, b,
                E)
            }
            if (m || v) {
              var $;
              return {
                $$willAnimate: !0,
                end: function() {
                  return $ ? $.end() : (c(), $ = new n, $.complete(!0)), $
                },
                start: function() {
                  function e(e) {
                    c(e), $.complete(e)
                  }

                  function t(t) {
                    p || ((r || L)(t), e(t))
                  }
                  if ($) return $;
                  $ = new n;
                  var r, i = [];
                  return m && i.push(function(e) {
                    r = m(e)
                  }), i.length ? i.push(function(e) {
                    s(), e(!0)
                  }) : s(), v && i.push(function(e) {
                    r = v(e)
                  }), $.setHost({
                    end: function() {
                      t()
                    },
                    cancel: function() {
                      t(!0)
                    }
                  }), n.chain(i, e), $
                }
              }
            }
          }
        }]
      }],
      Se = ["$$animationProvider", function(e) {
        e.drivers.push("$$animateJsDriver"), this.$get = ["$$animateJs", "$$AnimateRunner", function(e, t) {
          function n(t) {
            var n = t.element,
              r = t.event,
              i = t.options,
              o = t.classes;
            return e(n, r, o, i)
          }
          return function(e) {
            if (e.from && e.to) {
              var r = n(e.from),
                i = n(e.to);
              if (!r && !i) return;
              return {
                start: function() {
                  function e() {
                    return function() {
                      H(o, function(e) {
                        e.end()
                      })
                    }
                  }

                  function n(e) {
                    a.complete(e)
                  }
                  var o = [];
                  r && o.push(r.start()), i && o.push(i.start()), t.all(o, n);
                  var a = new t({
                    end: e(),
                    cancel: e()
                  });
                  return a
                }
              }
            }
            return n(e)
          }
        }]
      }],
      Ae = "data-ng-animate",
      Me = "$ngAnimatePin",
      ke = ["$animateProvider", function(r) {
        function i(e) {
          if (!e) return null;
          var t = e.split(m),
            n = Object.create(null);
          return H(t, function(e) {
            n[e] = !0
          }), n
        }

        function o(e, t) {
          if (e && t) {
            var n = i(t);
            return e.split(m).some(function(e) {
              return n[e]
            })
          }
        }

        function a(e, t, n, r) {
          return g[e].some(function(e) {
            return e(t, n, r)
          })
        }

        function u(e, t) {
          var n = (e.addClass || "").length > 0,
            r = (e.removeClass || "").length > 0;
          return t ? n && r : n || r
        }
        var l = 1,
          p = 2,
          m = " ",
          g = this.rules = {
            skip: [],
            cancel: [],
            join: []
          };
        g.join.push(function(e, t, n) {
          return !t.structural && u(t)
        }), g.skip.push(function(e, t, n) {
          return !t.structural && !u(t)
        }), g.skip.push(function(e, t, n) {
          return "leave" == n.event && t.structural
        }), g.skip.push(function(e, t, n) {
          return n.structural && n.state === p && !t.structural
        }), g.cancel.push(function(e, t, n) {
          return n.structural && t.structural
        }), g.cancel.push(function(e, t, n) {
          return n.state === p && t.structural
        }), g.cancel.push(function(e, t, n) {
          if (n.structural) return !1;
          var r = t.addClass,
            i = t.removeClass,
            a = n.addClass,
            s = n.removeClass;
          return !(G(r) && G(i) || G(a) && G(s)) && (o(r, s) || o(i, a))
        }), this.$get = ["$$rAF", "$rootScope", "$rootElement", "$document", "$$HashMap", "$$animation",
          "$$AnimateRunner", "$templateRequest", "$$jqLite", "$$forceReflow",
          function(i, o, m, g, _, $, w, T, C, x) {
            function S() {
              var e = !1;
              return function(t) {
                e ? t() : o.$$postDigest(function() {
                  e = !0, t()
                })
              }
            }

            function A(e, t) {
              return v(e, t, {})
            }

            function M(e, t, n) {
              var r = y(t),
                i = y(e),
                o = [],
                a = Z[n];
              return a && H(a, function(e) {
                ie.call(e.node, r) ? o.push(e.callback) : "leave" === n && ie.call(e.node, i) && o.push(e
                  .callback)
              }), o
            }

            function k(e, t, n) {
              var r = c(t);
              return e.filter(function(e) {
                var t = e.node === r && (!n || e.callback === n);
                return !t
              })
            }

            function N(e, t) {
              "close" !== e || t[0].parentNode || oe.off(t)
            }

            function I(e, t, n) {
              function r(t, n, r, o) {
                C(function() {
                  var t = M(m, e, n);
                  t.length ? i(function() {
                    H(t, function(t) {
                      t(e, r, o)
                    }), N(r, e)
                  }) : N(r, e)
                }), t.progress(n, r, o)
              }

              function c(t) {
                E(e, _), re(e, _), h(e, _), _.domOperation(), T.complete(!t)
              }
              var d, m, _ = U(n);
              e = s(e), e && (d = y(e), m = e.parent()), _ = f(_);
              var T = new w,
                C = S();
              if (B(_.addClass) && (_.addClass = _.addClass.join(" ")), _.addClass && !z(_.addClass) && (_
                  .addClass = null), B(_.removeClass) && (_.removeClass = _.removeClass.join(" ")), _.removeClass &&
                !z(_.removeClass) && (_.removeClass = null), _.from && !q(_.from) && (_.from = null), _.to && !q(_
                  .to) && (_.to = null), !d) return c(), T;
              var x = [d.className, _.addClass, _.removeClass].join(" ");
              if (!te(x)) return c(), T;
              var k = ["enter", "move", "leave"].indexOf(t) >= 0,
                I = g[0].hidden,
                R = !Q || I || X.get(d),
                F = !R && W.get(d) || {},
                j = !!F.state;
              if (R || j && F.state == l || (R = !P(e, m, t)), R) return I && r(T, t, "start"), c(), I && r(T, t,
                "close"), T;
              k && O(e);
              var G = {
                structural: k,
                element: e,
                event: t,
                addClass: _.addClass,
                removeClass: _.removeClass,
                close: c,
                options: _,
                runner: T
              };
              if (j) {
                var V = a("skip", e, G, F);
                if (V) return F.state === p ? (c(), T) : (v(e, F, G), F.runner);
                var Y = a("cancel", e, G, F);
                if (Y)
                  if (F.state === p) F.runner.end();
                  else {
                    if (!F.structural) return v(e, F, G), F.runner;
                    F.close()
                  }
                else {
                  var K = a("join", e, G, F);
                  if (K) {
                    if (F.state !== p) return b(e, k ? t : null, _), t = G.event = F.event, _ = v(e, F, G), F
                    .runner;
                    A(e, G)
                  }
                }
              } else A(e, G);
              var J = G.structural;
              if (J || (J = "animate" === G.event && Object.keys(G.options.to || {}).length > 0 || u(G)), !J)
              return c(), D(e), T;
              var Z = (F.counter || 0) + 1;
              return G.counter = Z, L(e, l, G), o.$$postDigest(function() {
                var n = W.get(d),
                  i = !n;
                n = n || {};
                var o = e.parent() || [],
                  a = o.length > 0 && ("animate" === n.event || n.structural || u(n));
                if (i || n.counter !== Z || !a) return i && (re(e, _), h(e, _)), (i || k && n.event !== t) && (_
                  .domOperation(), T.end()), void(a || D(e));
                t = !n.structural && u(n, !0) ? "setClass" : n.event, L(e, p);
                var s = $(e, t, n.options);
                T.setHost(s), r(T, t, "start", {}), s.done(function(n) {
                  c(!n);
                  var i = W.get(d);
                  i && i.counter === Z && D(y(e)), r(T, t, "close", {})
                })
              }), T
            }

            function O(e) {
              var t = y(e),
                n = t.querySelectorAll("[" + Ae + "]");
              H(n, function(e) {
                var t = parseInt(e.getAttribute(Ae)),
                  n = W.get(e);
                if (n) switch (t) {
                  case p:
                    n.runner.end();
                  case l:
                    W.remove(e)
                }
              })
            }

            function D(e) {
              var t = y(e);
              t.removeAttribute(Ae), W.remove(t)
            }

            function R(e, t) {
              return y(e) === y(t)
            }

            function P(e, t, n) {
              var r, i = j(g[0].body),
                o = R(e, i) || "HTML" === e[0].nodeName,
                a = R(e, m),
                s = !1,
                c = X.get(y(e)),
                u = j.data(e[0], Me);
              for (u && (t = u), t = y(t); t && (a || (a = R(t, m)), t.nodeType === K);) {
                var l = W.get(t) || {};
                if (!s) {
                  var d = X.get(t);
                  if (d === !0 && c !== !1) {
                    c = !0;
                    break
                  }
                  d === !1 && (c = !1), s = l.structural
                }
                if (G(r) || r === !0) {
                  var f = j.data(t, ne);
                  V(f) && (r = f)
                }
                if (s && r === !1) break;
                if (o || (o = R(t, i)), o && a) break;
                t = a || !(u = j.data(t, Me)) ? t.parentNode : y(u)
              }
              var h = (!s || r) && c !== !0;
              return h && a && o
            }

            function L(e, t, n) {
              n = n || {}, n.state = t;
              var r = y(e);
              r.setAttribute(Ae, t);
              var i = W.get(r),
                o = i ? F(i, n) : n;
              W.put(r, o)
            }
            var W = new _,
              X = new _,
              Q = null,
              J = o.$watch(function() {
                return 0 === T.totalPendingRequests
              }, function(e) {
                e && (J(), o.$$postDigest(function() {
                  o.$$postDigest(function() {
                    null === Q && (Q = !0)
                  })
                }))
              }),
              Z = {},
              ee = r.classNameFilter(),
              te = ee ? function(e) {
                return ee.test(e)
              } : function() {
                return !0
              },
              re = d(C),
              ie = e.Node.prototype.contains || function(e) {
                return this === e || !!(16 & this.compareDocumentPosition(e))
              },
              oe = {
                on: function(e, t, n) {
                  var r = c(t);
                  Z[e] = Z[e] || [], Z[e].push({
                    node: r,
                    callback: n
                  }), j(t).on("$destroy", function() {
                    var i = W.get(r);
                    i || oe.off(e, t, n)
                  })
                },
                off: function(e, n, r) {
                  if (1 !== arguments.length || t.isString(arguments[0])) {
                    var i = Z[e];
                    i && (Z[e] = 1 === arguments.length ? null : k(i, n, r))
                  } else {
                    n = arguments[0];
                    for (var o in Z) Z[o] = k(Z[o], n)
                  }
                },
                pin: function(e, t) {
                  n(Y(e), "element", "not an element"), n(Y(t), "parentElement", "not an element"), e.data(Me, t)
                },
                push: function(e, t, n, r) {
                  return n = n || {}, n.domOperation = r, I(e, t, n)
                },
                enabled: function(e, t) {
                  var n = arguments.length;
                  if (0 === n) t = !!Q;
                  else {
                    var r = Y(e);
                    if (r) {
                      var i = y(e),
                        o = X.get(i);
                      1 === n ? t = !o : X.put(i, !t)
                    } else t = Q = !!e
                  }
                  return t
                }
              };
            return oe
          }
        ]
      }],
      Ne = ["$animateProvider", function(e) {
        function t(e, t) {
          e.data(s, t)
        }

        function n(e) {
          e.removeData(s)
        }

        function i(e) {
          return e.data(s)
        }
        var o = "ng-animate-ref",
          a = this.drivers = [],
          s = "$$animationRunner";
        this.$get = ["$$jqLite", "$rootScope", "$injector", "$$AnimateRunner", "$$HashMap", "$$rAFScheduler",
          function(e, s, c, u, l, p) {
            function m(e) {
              function t(e) {
                if (e.processed) return e;
                e.processed = !0;
                var n = e.domNode,
                  r = n.parentNode;
                o.put(n, e);
                for (var a; r;) {
                  if (a = o.get(r)) {
                    a.processed || (a = t(a));
                    break
                  }
                  r = r.parentNode
                }
                return (a || i).children.push(e), e
              }

              function n(e) {
                var t, n = [],
                  r = [];
                for (t = 0; t < e.children.length; t++) r.push(e.children[t]);
                var i = r.length,
                  o = 0,
                  a = [];
                for (t = 0; t < r.length; t++) {
                  var s = r[t];
                  i <= 0 && (i = o, o = 0, n.push(a), a = []), a.push(s.fn), s.children.forEach(function(e) {
                    o++, r.push(e)
                  }), i--
                }
                return a.length && n.push(a), n
              }
              var r, i = {
                  children: []
                },
                o = new l;
              for (r = 0; r < e.length; r++) {
                var a = e[r];
                o.put(a.domNode, e[r] = {
                  domNode: a.domNode,
                  fn: a.fn,
                  children: []
                })
              }
              for (r = 0; r < e.length; r++) t(e[r]);
              return n(i)
            }
            var v = [],
              g = d(e);
            return function(l, d, b) {
              function E(e) {
                var t = "[" + o + "]",
                  n = e.hasAttribute(o) ? [e] : e.querySelectorAll(t),
                  r = [];
                return H(n, function(e) {
                  var t = e.getAttribute(o);
                  t && t.length && r.push(e)
                }), r
              }

              function _(e) {
                var t = [],
                  n = {};
                H(e, function(e, r) {
                  var i = e.element,
                    a = y(i),
                    s = e.event,
                    c = ["enter", "move"].indexOf(s) >= 0,
                    u = e.structural ? E(a) : [];
                  if (u.length) {
                    var l = c ? "to" : "from";
                    H(u, function(e) {
                      var t = e.getAttribute(o);
                      n[t] = n[t] || {}, n[t][l] = {
                        animationID: r,
                        element: j(e)
                      }
                    })
                  } else t.push(e)
                });
                var r = {},
                  i = {};
                return H(n, function(n, o) {
                  var a = n.from,
                    s = n.to;
                  if (!a || !s) {
                    var c = a ? a.animationID : s.animationID,
                      u = c.toString();
                    return void(r[u] || (r[u] = !0, t.push(e[c])))
                  }
                  var l = e[a.animationID],
                    d = e[s.animationID],
                    f = a.animationID.toString();
                  if (!i[f]) {
                    var h = i[f] = {
                      structural: !0,
                      beforeStart: function() {
                        l.beforeStart(), d.beforeStart()
                      },
                      close: function() {
                        l.close(), d.close()
                      },
                      classes: $(l.classes, d.classes),
                      from: l,
                      to: d,
                      anchors: []
                    };
                    h.classes.length ? t.push(h) : (t.push(l), t.push(d))
                  }
                  i[f].anchors.push({
                    out: a.element,
                    in: s.element
                  })
                }), t
              }

              function $(e, t) {
                e = e.split(" "), t = t.split(" ");
                for (var n = [], r = 0; r < e.length; r++) {
                  var i = e[r];
                  if ("ng-" !== i.substring(0, 3))
                    for (var o = 0; o < t.length; o++)
                      if (i === t[o]) {
                        n.push(i);
                        break
                      }
                }
                return n.join(" ")
              }

              function w(e) {
                for (var t = a.length - 1; t >= 0; t--) {
                  var n = a[t];
                  if (c.has(n)) {
                    var r = c.get(n),
                      i = r(e);
                    if (i) return i
                  }
                }
              }

              function T() {
                l.addClass(te), N && e.addClass(l, N), I && (e.removeClass(l, I), I = null)
              }

              function C(e, t) {
                function n(e) {
                  i(e).setHost(t)
                }
                e.from && e.to ? (n(e.from.element), n(e.to.element)) : n(e.element)
              }

              function x() {
                var e = i(l);
                !e || "leave" === d && b.$$domOperationFired || e.end()
              }

              function S(t) {
                l.off("$destroy", x), n(l), g(l, b), h(l, b), b.domOperation(), N && e.removeClass(l, N), l
                  .removeClass(te), M.complete(!t)
              }
              b = f(b);
              var A = ["enter", "move", "leave"].indexOf(d) >= 0,
                M = new u({
                  end: function() {
                    S()
                  },
                  cancel: function() {
                    S(!0)
                  }
                });
              if (!a.length) return S(), M;
              t(l, M);
              var k = r(l.attr("class"), r(b.addClass, b.removeClass)),
                N = b.tempClasses;
              N && (k += " " + N, b.tempClasses = null);
              var I;
              return A && (I = "ng-" + d + ee, e.addClass(l, I)), v.push({
                element: l,
                classes: k,
                event: d,
                structural: A,
                options: b,
                beforeStart: T,
                close: S
              }), l.on("$destroy", x), v.length > 1 ? M : (s.$$postDigest(function() {
                var e = [];
                H(v, function(t) {
                  i(t.element) ? e.push(t) : t.close()
                }), v.length = 0;
                var t = _(e),
                  n = [];
                H(t, function(e) {
                  n.push({
                    domNode: y(e.from ? e.from.element : e.element),
                    fn: function() {
                      e.beforeStart();
                      var t, n = e.close,
                        r = e.anchors ? e.from.element || e.to.element : e.element;
                      if (i(r)) {
                        var o = w(e);
                        o && (t = o.start)
                      }
                      if (t) {
                        var a = t();
                        a.done(function(e) {
                          n(!e)
                        }), C(e, a)
                      } else n()
                    }
                  })
                }), p(m(n))
              }), M)
            }
          }
        ]
      }],
      Ie = ["$animate", "$rootScope", function(e, t) {
        return {
          restrict: "A",
          transclude: "element",
          terminal: !0,
          priority: 600,
          link: function(t, n, r, i, o) {
            var a, s;
            t.$watchCollection(r.ngAnimateSwap || r.for, function(r) {
              a && e.leave(a), s && (s.$destroy(), s = null), (r || 0 === r) && (s = t.$new(), o(s, function(
              t) {
                a = t, e.enter(t, null, n)
              }))
            })
          }
        }
      }];
    t.module("ngAnimate", []).directive("ngAnimateSwap", Ie).directive("ngAnimateChildren", ge).factory(
        "$$rAFScheduler", ve).provider("$$animateQueue", ke).provider("$$animation", Ne).provider("$animateCss", Te)
      .provider("$$animateCssDriver", Ce).provider("$$animateJs", xe).provider("$$animateJsDriver", Se)
  }(window, window.angular)
}
