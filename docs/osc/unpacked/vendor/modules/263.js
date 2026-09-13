// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 263
// role       : directive angularStats
// defines    : angular.module("angularStats")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  //! ng-stats version 2.5.4 built with ♥ by Kent C. Dodds <kent@doddsfamily.us> (http://kent.doddsfamily.us), Viper Bailey <jinxidoru@gmail.com> (http://jinxidoru.blogspot.com), Daniel Lamb <dlamb.open.source@gmail.com> (http://daniellmb.com) (ó ì_í)=óò=(ì_í ò)
  ! function(t, r) {
    e.exports = r(n(24))
  }(this, function(e) {
    return function(e) {
      function t(r) {
        if (n[r]) return n[r].exports;
        var i = n[r] = {
          exports: {},
          id: r,
          loaded: !1
        };
        return e[r].call(i.exports, i, i.exports, t), i.loaded = !0, i.exports
      }
      var n = {};
      return t.m = e, t.c = n, t.p = "", t(0)
    }([function(e, t, n) {
      "use strict";

      function r(e) {
        return e && e.__esModule ? e : {
          default: e
        }
      }

      function i() {
        if (!R) {
          R = !0;
          var e = Object.getPrototypeOf(u()),
            t = e.$digest;
          e.$digest = function() {
            var e = M();
            t.apply(this, arguments);
            var n = M() - e;
            p(l(), n)
          }
        }
      }

      function o() {
        return "undefined" != typeof chrome && "undefined" != typeof chrome.storage && "undefined" !=
          typeof chrome.storage.local
      }

      function a(e) {
        window.self.angular && u() ? c(e) : setTimeout(function() {
          a(e)
        }, 200)
      }

      function s(e) {
        if (e !== !1 && e.autoload || (sessionStorage.removeItem(S), localStorage.removeItem(S), e !== !1))
          return e.position = e.position || "top-left", e = C.extend({
            htmlId: null,
            rootScope: void 0,
            digestTimeThreshold: 16,
            watchCountThreshold: 2e3,
            autoload: !1,
            trackDigest: !1,
            trackWatches: !1,
            logDigest: !1,
            logWatches: !1,
            styles: {
              position: "fixed",
              background: "black",
              borderBottom: "1px solid #666",
              borderRight: "1px solid #666",
              color: "#666",
              fontFamily: "Courier",
              width: 130,
              zIndex: 9999,
              textAlign: "right",
              top: e.position.indexOf("top") === -1 ? null : 0,
              bottom: e.position.indexOf("bottom") === -1 ? null : 0,
              right: e.position.indexOf("right") === -1 ? null : 0,
              left: e.position.indexOf("left") === -1 ? null : 0
            }
          }, e || {}), e.rootScope && (x = e.rootScope), e
      }

      function c(e) {
        function t(t, n, r) {
          var i = t.charAt(0).toUpperCase() + t.slice(1);
          e["track" + i] && (u[t] = [], n["track + capThingToTrack"] = function(e) {
            r && u[t][u.length - 1] === e || (u[t][u.length - 1] = e, u[t].push(e))
          })
        }

        function n(t, n, r) {
          var i = t.charAt(0).toUpperCase() + t.slice(1);
          if (e["log" + i]) {
            var a;
            n["log" + i] = function(e) {
              if (!r || a !== e) {
                a = e;
                var n = o(t, e);
                n ? console.log("%c" + t + ":", n, e) : console.log(t + ":", e)
              }
            }
          }
        }

        function r(e, t) {
          return e > t ? "red" : e > .7 * t ? "orange" : "green"
        }

        function o(t, n) {
          var i;
          return "digest" === t ? i = "color:" + r(n, e.digestTimeThreshold) : "watches" === t && (i =
            "color:" + r(n, e.watchCountThreshold)), i
        }

        function a(t, n) {
          var i = n || O,
            o = r(i, e.digestTimeThreshold);
          I = m(t) ? I : t;
          var a = r(I, e.watchCountThreshold);
          if (O = m(n) ? O : n, p.text(I).css({
              color: a
            }), v.text(O.toFixed(2)).css({
              color: o
            }), n) {
            var s = y.getContext("2d");
            f > 0 && (f = 0, s.fillStyle = "#333", s.fillRect(g.width - 1, 0, 1, g.height)), s.fillStyle = o, s
              .fillRect(g.width - 1, Math.max(0, g.height - i), 2, 2)
          }
        }

        function c() {
          if (l.active) {
            setTimeout(c, 250);
            var e = y.getContext("2d"),
              t = e.getImageData(1, 0, g.width - 1, g.height);
            e.putImageData(t, 0, 0), e.fillStyle = f++ > 2 ? "black" : "#333", e.fillRect(g.width - 1, 0, 1, g
              .height)
          }
        }
        e = void 0 !== e ? e : {};
        var u = {
          listeners: P
        };
        if (A && (A.$el && A.$el.remove(), A.active = !1, A = null), e = s(e)) {
          i();
          var l = A = {
            active: !0
          };
          if (e.autoload)
            if ("localStorage" === e.autoload) localStorage.setItem(S, JSON.stringify(e));
            else {
              if ("sessionStorage" !== e.autoload && "boolean" != typeof e.autoload) throw new Error(
                "Invalid value for autoload: " + e.autoload +
                ' can only be "localStorage" "sessionStorage" or boolean.');
              sessionStorage.setItem(S, JSON.stringify(e))
            } var d = C.element(document.body),
            f = 0,
            h = e.htmlId ? ' id="' + e.htmlId + '"' : "";
          l.$el = C.element("<div" + h + "><canvas></canvas><div><span></span> | <span></span></div></div>")
            .css(e.styles), d.append(l.$el);
          var p = l.$el.find("span"),
            v = p.next(),
            g = {
              width: 130,
              height: 40
            },
            y = l.$el.find("canvas").attr(g)[0];
          return P.digestLength.ngStatsAddToCanvas = function(e) {
            a(null, e)
          }, P.watchCount.ngStatsAddToCanvas = function(e) {
            a(e)
          }, t("digest", P.digestLength), t("watches", P.watchCount, !0), n("digest", P.digestLength), n(
            "watches", P.watchCount, !0), c(), x.$$phase || x.$digest(), u
        }
      }

      function u() {
        if (x) return x;
        var e = document.querySelector(D);
        return e ? x = C.element(e).scope().$root : null
      }

      function l() {
        clearTimeout(N);
        var e = M();
        return e - k > 300 ? (k = e, I = v()) : N = setTimeout(function() {
          p(l())
        }, 350), I
      }

      function d(e) {
        var t = f(e);
        return v(t)
      }

      function f(e) {
        e = C.element(e);
        var t = e.scope();
        return t || (e = C.element(e.querySelector(D)), t = e.scope()), t
      }

      function h(e) {
        return e && e.$$watchers ? e.$$watchers : []
      }

      function p(e, t) {
        m(e) || C.forEach(P.watchCount, function(t) {
          t(e)
        }), m(t) || C.forEach(P.digestLength, function(e) {
          e(t)
        })
      }

      function m(e) {
        return null === e || void 0 === e
      }

      function v(e) {
        var t = 0;
        return g(e, function(e) {
          t += h(e).length
        }), t
      }

      function g(e, t) {
        if ("function" == typeof e && (t = e, e = null), e = e || u(), e = _(e)) {
          var n = t(e);
          return n === !1 ? n : b(e, t)
        }
      }

      function y(e, t) {
        for (var n;
          (e = e.$$nextSibling) && (n = t(e), n !== !1) && (n = b(e, t), n !== !1););
        return n
      }

      function b(e, t) {
        for (var n;
          (e = e.$$childHead) && (n = t(e), n !== !1) && (n = y(e, t), n !== !1););
        return n
      }

      function E(e) {
        var t = null;
        return g(function(n) {
          if (n.$id === e) return t = n, !1
        }), t
      }

      function _(e) {
        return $(e) && (e = E(e)), e
      }

      function $(e) {
        return "string" == typeof e || "number" == typeof e
      }
      Object.defineProperty(t, "__esModule", {
        value: !0
      });
      var w = n(1),
        T = r(w),
        C = T.default;
      C.version || (C = window.angular), t.default = c;
      var x, S = "showAngularStats_autoload",
        A = null,
        M = window.self.performance && window.self.performance.now ? function() {
          return window.self.performance.now()
        } : function() {
          return Date.now()
        },
        k = M(),
        N = null,
        I = l() || 0,
        O = 0,
        D = ".ng-scope, .ng-isolate-scope",
        R = !1,
        P = {
          watchCount: {},
          digestLength: {}
        },
        L = sessionStorage[S] || !o() && localStorage[S];
      L && a(JSON.parse(L)), C.module("angularStats", []).directive("angularStats", function() {
        function e(e) {
          for (var t = e[0]; t.parentElement;) t = t.parentElement;
          return t
        }
        var t = 1;
        return {
          scope: {
            digestLength: "@",
            watchCount: "@",
            watchCountRoot: "@",
            onDigestLengthUpdate: "&?",
            onWatchCountUpdate: "&?"
          },
          link: function(n, r, o) {
            function a() {
              if (o.hasOwnProperty("digestLength")) {
                var e = r;
                o.digestLength && (e = C.element(r[0].querySelector(o.digestLength))), P.digestLength[
                  "ngStatsDirective" + f] = function(t) {
                  window.dirDigestNode = e[0], e.text((t || 0).toFixed(2))
                }
              }
            }

            function s() {
              if (o.hasOwnProperty("watchCount")) {
                var t, i = r;
                if (n.watchCount && (i = C.element(r[0].querySelector(o.watchCount))), n.watchCountRoot)
                  if ("this" === n.watchCountRoot) t = r;
                  else {
                    var a;
                    if (a = o.hasOwnProperty("watchCountOfChild") ? r[0] : e(r), t = C.element(a
                        .querySelector(n.watchCountRoot)), !t.length) throw new Error(
                      "no element at selector: " + n.watchCountRoot)
                  } P.watchCount["ngStatsDirective" + f] = function(e) {
                  var n = e;
                  t && (n = d(t)), i.text(n)
                }
              }
            }

            function c() {
              o.hasOwnProperty("onWatchCountUpdate") && (P.watchCount["ngStatsDirectiveUpdate" + f] =
                function(e) {
                  n.onWatchCountUpdate({
                    watchCount: e
                  })
                })
            }

            function u() {
              o.hasOwnProperty("onDigestLengthUpdate") && (P.digestLength["ngStatsDirectiveUpdate" + f] =
                function(e) {
                  n.onDigestLengthUpdate({
                    digestLength: e
                  })
                })
            }

            function l() {
              delete P.digestLength["ngStatsDirectiveUpdate" + f], delete P.watchCount[
                  "ngStatsDirectiveUpdate" + f], delete P.digestLength["ngStatsDirective" + f], delete P
                .watchCount["ngStatsDirective" + f]
            }
            i();
            var f = t++;
            a(), s(), c(), u(), n.$on("$destroy", l)
          }
        }
      }), e.exports = t.default
    }, function(t, n) {
      t.exports = e
    }])
  })
}
