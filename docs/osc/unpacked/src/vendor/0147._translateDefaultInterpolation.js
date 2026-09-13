// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 147
// factory $translateDefaultInterpolation | directive translate | directive translateCloak | provider $translate | constant $STORAGE_KEY | filter translate | defines angular.module("pascalprecht.translate")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  angular.module("pascalprecht.translate", ["ng"]).run(["$translate", function(e) {
      var t = e.storageKey(),
        n = e.storage(),
        r = function() {
          var r = e.preferredLanguage();
          angular.isString(r) ? e.use(r) : n.put(t, e.use());
        };
      n ? n.get(t) ? e.use(n.get(t)).catch(r) : r() : angular.isString(e.preferredLanguage()) && e.use(e
        .preferredLanguage());
    }]), angular.module("pascalprecht.translate").provider("$translate", ["$STORAGE_KEY", "$windowProvider",
      function(e, t) {
        var n,
          r,
          i,
          o,
          a,
          s,
          c,
          u,
          l,
          d,
          f,
          h,
          p,
          m,
          v,
          g = {},
          y = [],
          b = e,
          E = [],
          _ = !1,
          $ = "translate-cloak",
          w = !1,
          T = ".",
          C = 0,
          x = "2.6.1",
          S = function() {
            var e,
              n,
              r = t.$get().navigator,
              i = ["language", "browserLanguage", "systemLanguage", "userLanguage"];
            if (angular.isArray(r.languages))
              for (e = 0; e < r.languages.length; e++)
                if (n = r.languages[e], n && n.length) return n;
            for (e = 0; e < i.length; e++)
              if (n = r[i[e]], n && n.length) return n;
            return null;
          };
        S.displayName = "angular-translate/service: getFirstBrowserLanguage";
        var A = function() {
          return (S() || "").split("-").join("_");
        };
        A.displayName = "angular-translate/service: getLocale";
        var M = function(e, t) {
            for (var n = 0, r = e.length; n < r; n++)
              if (e[n] === t) return n;
            return -1;
          },
          k = function() {
            return this.replace(/^\s+|\s+$/g, "");
          },
          N = function(e) {
            for (var t = [], n = angular.lowercase(e), i = 0, o = y.length; i < o; i++) t.push(angular
              .lowercase(y[i]));
            if (M(t, n) > -1) return e;
            if (r) {
              var a;
              for (var s in r) {
                var c = !1,
                  u = Object.prototype.hasOwnProperty.call(r, s) && angular.lowercase(s) === angular
                  .lowercase(e);
                if ("*" === s.slice(-1) && (c = s.slice(0, -1) === e.slice(0, s.length - 1)), (u || c) && (
                    a = r[s], M(t, angular.lowercase(a)) > -1)) return a;
              }
            }
            var l = e.split("_");
            return l.length > 1 && M(t, angular.lowercase(l[0])) > -1 ? l[0] : e;
          },
          I = function(e, t) {
            if (!e && !t) return g;
            if (e && !t) {
              if (angular.isString(e)) return g[e];
            } else angular.isObject(g[e]) || (g[e] = {}), angular.extend(g[e], O(t));
            return this;
          };
        this.translations = I, this.cloakClassName = function(e) {
          return e ? ($ = e, this) : $;
        };
        var O = function(e, t, n, r) {
          var i, o, a, s;
          t || (t = []), n || (n = {});
          for (i in e) Object.prototype.hasOwnProperty.call(e, i) && (s = e[i], angular.isObject(s) ? O(s,
            t.concat(i), n, i) : (o = t.length ? "" + t.join(T) + T + i : i, t.length && i === r && (
            a = "" + t.join(T), n[a] = "@:" + o), n[o] = s));
          return n;
        };
        this.addInterpolation = function(e) {
          return E.push(e), this;
        }, this.useMessageFormatInterpolation = function() {
          return this.useInterpolation("$translateMessageFormatInterpolation");
        }, this.useInterpolation = function(e) {
          return d = e, this;
        }, this.useSanitizeValueStrategy = function(e) {
          return _ = e, this;
        }, this.preferredLanguage = function(e) {
          return D(e), this;
        };
        var D = function(e) {
          return e && (n = e), n;
        };
        this.translationNotFoundIndicator = function(e) {
          return this.translationNotFoundIndicatorLeft(e), this.translationNotFoundIndicatorRight(e),
          this;
        }, this.translationNotFoundIndicatorLeft = function(e) {
          return e ? (p = e, this) : p;
        }, this.translationNotFoundIndicatorRight = function(e) {
          return e ? (m = e, this) : m;
        }, this.fallbackLanguage = function(e) {
          return R(e), this;
        };
        var R = function(e) {
          return e ? (angular.isString(e) ? (o = !0, i = [e]) : angular.isArray(e) && (o = !1, i = e),
            angular.isString(n) && M(i, n) < 0 && i.push(n), this) : o ? i[0] : i;
        };
        this.use = function(e) {
          if (e) {
            if (!g[e] && !f) throw new Error(
              "$translateProvider couldn't find translationTable for langKey: '" + e + "'");
            return a = e, this;
          }
          return a;
        };
        var P = function(e) {
          return e ? void(b = e) : u ? u + b : b;
        };
        this.storageKey = P, this.useUrlLoader = function(e, t) {
          return this.useLoader("$translateUrlLoader", angular.extend({
            url: e
          }, t));
        }, this.useStaticFilesLoader = function(e) {
          return this.useLoader("$translateStaticFilesLoader", e);
        }, this.useLoader = function(e, t) {
          return f = e, h = t || {}, this;
        }, this.useLocalStorage = function() {
          return this.useStorage("$translateLocalStorage");
        }, this.useCookieStorage = function() {
          return this.useStorage("$translateCookieStorage");
        }, this.useStorage = function(e) {
          return c = e, this;
        }, this.storagePrefix = function(e) {
          return e ? (u = e, this) : e;
        }, this.useMissingTranslationHandlerLog = function() {
          return this.useMissingTranslationHandler("$translateMissingTranslationHandlerLog");
        }, this.useMissingTranslationHandler = function(e) {
          return l = e, this;
        }, this.usePostCompiling = function(e) {
          return w = !!e, this;
        }, this.determinePreferredLanguage = function(e) {
          var t = e && angular.isFunction(e) ? e() : A();
          return n = y.length ? N(t) : t, this;
        }, this.registerAvailableLanguageKeys = function(e, t) {
          return e ? (y = e, t && (r = t), this) : y;
        }, this.useLoaderCache = function(e) {
          return e === !1 ? v = void 0 : e === !0 ? v = !0 : "undefined" == typeof e ? v =
            "$translationCache" : e && (v = e), this;
        }, this.directivePriority = function(e) {
          return void 0 === e ? C : (C = e, this);
        }, this.$get = ["$log", "$injector", "$rootScope", "$q", function(e, t, r, u) {
          var y,
            T,
            S,
            A = t.get(d || "$translateDefaultInterpolation"),
            L = !1,
            U = {},
            F = {},
            j = function(e, t, r, o) {
              if (angular.isArray(e)) {
                var s = function(e) {
                  for (var n = {}, i = [], a = function(e) {
                      var i = u.defer(),
                        a = function(t) {
                          n[e] = t, i.resolve([e, t]);
                        };
                      return j(e, t, r, o).then(a, a), i.promise;
                    }, s = 0, c = e.length; s < c; s++) i.push(a(e[s]));
                  return u.all(i).then(function() {
                    return n;
                  });
                };
                return s(e);
              }
              var l = u.defer();
              e && (e = k.apply(e));
              var d = function() {
                var e = n ? F[n] : F[a];
                if (T = 0, c && !e) {
                  var t = y.get(b);
                  if (e = F[t], i && i.length) {
                    var r = M(i, t);
                    T = 0 === r ? 1 : 0, M(i, n) < 0 && i.push(n);
                  }
                }
                return e;
              }();
              return d ? d.then(function() {
                J(e, t, r, o).then(l.resolve, l.reject);
              }, l.reject) : J(e, t, r, o).then(l.resolve, l.reject), l.promise;
            },
            H = function(e) {
              return p && (e = [p, e].join(" ")), m && (e = [e, m].join(" ")), e;
            },
            B = function(e) {
              a = e, r.$emit("$translateChangeSuccess", {
                language: e
              }), c && y.put(j.storageKey(), a), A.setLocale(a), angular.forEach(U, function(e, t) {
                U[t].setLocale(a);
              }), r.$emit("$translateChangeEnd", {
                language: e
              });
            },
            z = function(e) {
              if (!e) throw "No language key specified for loading.";
              var n = u.defer();
              r.$emit("$translateLoadingStart", {
                language: e
              }), L = !0;
              var i = v;
              "string" == typeof i && (i = t.get(i));
              var o = angular.extend({}, h, {
                key: e,
                $http: angular.extend({}, {
                  cache: i
                }, h.$http)
              });
              return t.get(f)(o).then(function(t) {
                var i = {};
                r.$emit("$translateLoadingSuccess", {
                  language: e
                }), angular.isArray(t) ? angular.forEach(t, function(e) {
                  angular.extend(i, O(e));
                }) : angular.extend(i, O(t)), L = !1, n.resolve({
                  key: e,
                  table: i
                }), r.$emit("$translateLoadingEnd", {
                  language: e
                });
              }, function(e) {
                r.$emit("$translateLoadingError", {
                  language: e
                }), n.reject(e), r.$emit("$translateLoadingEnd", {
                  language: e
                });
              }), n.promise;
            };
          if (c && (y = t.get(c), !y.get || !y.put)) throw new Error("Couldn't use storage '" + c +
            "', missing get() or put() method!");
          angular.isFunction(A.useSanitizeValueStrategy) && A.useSanitizeValueStrategy(_), E.length &&
            angular.forEach(E, function(e) {
              var r = t.get(e);
              r.setLocale(n || a), angular.isFunction(r.useSanitizeValueStrategy) && r
                .useSanitizeValueStrategy(_), U[r.getInterpolationIdentifier()] = r;
            });
          var q = function(e) {
              var t = u.defer();
              return Object.prototype.hasOwnProperty.call(g, e) ? t.resolve(g[e]) : F[e] ? F[e].then(
                function(e) {
                  I(e.key, e.table), t.resolve(e.table);
                }, t.reject) : t.reject(), t.promise;
            },
            G = function(e, t, n, r) {
              var i = u.defer();
              return q(e).then(function(o) {
                if (Object.prototype.hasOwnProperty.call(o, t)) {
                  r.setLocale(e);
                  var s = o[t];
                  "@:" === s.substr(0, 2) ? G(e, s.substr(2), n, r).then(i.resolve, i.reject) : i
                    .resolve(r.interpolate(o[t], n)), r.setLocale(a);
                } else i.reject();
              }, i.reject), i.promise;
            },
            V = function(e, t, n, r) {
              var i,
                o = g[e];
              if (o && Object.prototype.hasOwnProperty.call(o, t)) {
                if (r.setLocale(e), i = r.interpolate(o[t], n), "@:" === i.substr(0, 2)) return V(e, i
                  .substr(2), n, r);
                r.setLocale(a);
              }
              return i;
            },
            W = function(e) {
              if (l) {
                var n = t.get(l)(e, a);
                return void 0 !== n ? n : e;
              }
              return e;
            },
            Y = function(e, t, n, r, o) {
              var a = u.defer();
              if (e < i.length) {
                var s = i[e];
                G(s, t, n, r).then(a.resolve, function() {
                  Y(e + 1, t, n, r, o).then(a.resolve);
                });
              } else o ? a.resolve(o) : a.resolve(W(t));
              return a.promise;
            },
            K = function(e, t, n, r) {
              var o;
              if (e < i.length) {
                var a = i[e];
                o = V(a, t, n, r), o || (o = K(e + 1, t, n, r));
              }
              return o;
            },
            X = function(e, t, n, r) {
              return Y(S > 0 ? S : T, e, t, n, r);
            },
            Q = function(e, t, n) {
              return K(S > 0 ? S : T, e, t, n);
            },
            J = function(e, t, n, r) {
              var o = u.defer(),
                s = a ? g[a] : g,
                c = n ? U[n] : A;
              if (s && Object.prototype.hasOwnProperty.call(s, e)) {
                var d = s[e];
                "@:" === d.substr(0, 2) ? j(d.substr(2), t, n, r).then(o.resolve, o.reject) : o.resolve(
                  c.interpolate(d, t));
              } else {
                var f;
                l && !L && (f = W(e)), a && i && i.length ? X(e, t, c, r).then(function(e) {
                  o.resolve(e);
                }, function(e) {
                  o.reject(H(e));
                }) : l && !L && f ? r ? o.resolve(r) : o.resolve(f) : r ? o.resolve(r) : o.reject(H(
                  e));
              }
              return o.promise;
            },
            Z = function(e, t, n) {
              var r,
                o = a ? g[a] : g,
                s = A;
              if (U && Object.prototype.hasOwnProperty.call(U, n) && (s = U[n]), o && Object.prototype
                .hasOwnProperty.call(o, e)) {
                var c = o[e];
                r = "@:" === c.substr(0, 2) ? Z(c.substr(2), t, n) : s.interpolate(c, t);
              } else {
                var u;
                l && !L && (u = W(e)), a && i && i.length ? (T = 0, r = Q(e, t, s)) : r = l && !L && u ?
                  u : H(e);
              }
              return r;
            };
          if (j.preferredLanguage = function(e) {
              return e && D(e), n;
            }, j.cloakClassName = function() {
              return $;
            }, j.fallbackLanguage = function(e) {
              if (void 0 !== e && null !== e) {
                if (R(e), f && i && i.length)
                  for (var t = 0, n = i.length; t < n; t++) F[i[t]] || (F[i[t]] = z(i[t]));
                j.use(j.use());
              }
              return o ? i[0] : i;
            }, j.useFallbackLanguage = function(e) {
              if (void 0 !== e && null !== e)
                if (e) {
                  var t = M(i, e);
                  t > -1 && (S = t);
                } else S = 0;
            }, j.proposedLanguage = function() {
              return s;
            }, j.storage = function() {
              return y;
            }, j.use = function(e) {
              if (!e) return a;
              var t = u.defer();
              r.$emit("$translateChangeStart", {
                language: e
              });
              var n = N(e);
              return n && (e = n), g[e] || !f || F[e] ? (t.resolve(e), B(e)) : (s = e, F[e] = z(e).then(
                function(n) {
                  return I(n.key, n.table), t.resolve(n.key), B(n.key), s === e && (s = void 0), n;
                },
                function(e) {
                  s === e && (s = void 0), r.$emit("$translateChangeError", {
                    language: e
                  }), t.reject(e), r.$emit("$translateChangeEnd", {
                    language: e
                  });
                })), t.promise;
            }, j.storageKey = function() {
              return P();
            }, j.isPostCompilingEnabled = function() {
              return w;
            }, j.refresh = function(e) {
              function t() {
                o.resolve(), r.$emit("$translateRefreshEnd", {
                  language: e
                });
              }

              function n() {
                o.reject(), r.$emit("$translateRefreshEnd", {
                  language: e
                });
              }
              if (!f) throw new Error("Couldn't refresh translation table, no loader registered!");
              var o = u.defer();
              if (r.$emit("$translateRefreshStart", {
                  language: e
                }), e) g[e] ? z(e).then(function(n) {
                I(n.key, n.table), e === a && B(a), t();
              }, n) : n();
              else {
                var s = [],
                  c = {};
                if (i && i.length)
                  for (var l = 0, d = i.length; l < d; l++) s.push(z(i[l])), c[i[l]] = !0;
                a && !c[a] && s.push(z(a)), u.all(s).then(function(e) {
                  angular.forEach(e, function(e) {
                    g[e.key] && delete g[e.key], I(e.key, e.table);
                  }), a && B(a), t();
                });
              }
              return o.promise;
            }, j.instant = function(e, t, r) {
              if (null === e || angular.isUndefined(e)) return e;
              if (angular.isArray(e)) {
                for (var o = {}, s = 0, c = e.length; s < c; s++) o[e[s]] = j.instant(e[s], t, r);
                return o;
              }
              if (angular.isString(e) && e.length < 1) return e;
              e && (e = k.apply(e));
              var u,
                d = [];
              n && d.push(n), a && d.push(a), i && i.length && (d = d.concat(i));
              for (var f = 0, h = d.length; f < h; f++) {
                var v = d[f];
                if (g[v] && ("undefined" != typeof g[v][e] ? u = Z(e, t, r) : (p || m) && (u = H(e))),
                  "undefined" != typeof u) break;
              }
              return u || "" === u || (u = A.interpolate(e, t), l && !L && (u = W(e))), u;
            }, j.versionInfo = function() {
              return x;
            }, j.loaderCache = function() {
              return v;
            }, j.directivePriority = function() {
              return C;
            }, f && (angular.equals(g, {}) && j.use(j.use()), i && i.length))
            for (var ee = function(e) {
                return I(e.key, e.table), r.$emit("$translateChangeEnd", {
                  language: e.key
                }), e;
              }, te = 0, ne = i.length; te < ne; te++) F[i[te]] = z(i[te]).then(ee);
          return j;
        }];
      }
    ]), angular.module("pascalprecht.translate").factory("$translateDefaultInterpolation", ["$interpolate",
      function(e) {
        var t,
          n = {},
          r = "default",
          i = null,
          o = {
            escaped: function(e) {
              var t = {};
              for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (angular.isNumber(e[n]) ? t[
                n] = e[n] : t[n] = angular.element("<div></div>").text(e[n]).html());
              return t;
            }
          },
          a = function(e) {
            var t;
            return t = angular.isFunction(o[i]) ? o[i](e) : e;
          };
        return n.setLocale = function(e) {
          t = e;
        }, n.getInterpolationIdentifier = function() {
          return r;
        }, n.useSanitizeValueStrategy = function(e) {
          return i = e, this;
        }, n.interpolate = function(t, n) {
          return i && (n = a(n)), e(t)(n || {});
        }, n;
      }
    ]), angular.module("pascalprecht.translate").constant("$STORAGE_KEY", "NG_TRANSLATE_LANG_KEY"), angular
    .module("pascalprecht.translate").directive("translate", ["$translate", "$q", "$interpolate", "$compile",
      "$parse", "$rootScope",
      function(e, t, n, r, i, o) {
        var a = function() {
          return this.replace(/^\s+|\s+$/g, "");
        };
        return {
          restrict: "AE",
          scope: !0,
          priority: e.directivePriority(),
          compile: function(t, s) {
            var c = s.translateValues ? s.translateValues : void 0,
              u = s.translateInterpolation ? s.translateInterpolation : void 0,
              l = t[0].outerHTML.match(/translate-value-+/i),
              d = "^(.*)(" + n.startSymbol() + ".*" + n.endSymbol() + ")(.*)",
              f = "^(.*)" + n.startSymbol() + "(.*)" + n.endSymbol() + "(.*)";
            return function(t, h, p) {
              t.interpolateParams = {}, t.preText = "", t.postText = "";
              var m = {},
                v = function(e) {
                  if (angular.isFunction(v._unwatchOld) && (v._unwatchOld(), v._unwatchOld = void 0),
                    angular.equals(e, "") || !angular.isDefined(e)) {
                    var r = a.apply(h.text()).match(d);
                    if (angular.isArray(r)) {
                      t.preText = r[1], t.postText = r[3], m.translate = n(r[2])(t.$parent);
                      var i = h.text().match(f);
                      angular.isArray(i) && i[2] && i[2].length && (v._unwatchOld = t.$watch(i[2],
                        function(e) {
                          m.translate = e, $();
                        }));
                    } else m.translate = h.text().replace(/^\s+|\s+$/g, "");
                  } else m.translate = e;
                  $();
                },
                g = function(e) {
                  p.$observe(e, function(t) {
                    m[e] = t, $();
                  });
                },
                y = !0;
              p.$observe("translate", function(e) {
                "undefined" == typeof e ? v("") : "" === e && y || (m.translate = e, $()), y = !1;
              });
              for (var b in p) p.hasOwnProperty(b) && "translateAttr" === b.substr(0, 13) && g(b);
              if (p.$observe("translateDefault", function(e) {
                  t.defaultText = e;
                }), c && p.$observe("translateValues", function(e) {
                  e && t.$parent.$watch(function() {
                    angular.extend(t.interpolateParams, i(e)(t.$parent));
                  });
                }), l) {
                var E = function(e) {
                  p.$observe(e, function(n) {
                    var r = angular.lowercase(e.substr(14, 1)) + e.substr(15);
                    t.interpolateParams[r] = n;
                  });
                };
                for (var _ in p) Object.prototype.hasOwnProperty.call(p, _) && "translateValue" === _
                  .substr(0, 14) && "translateValues" !== _ && E(_);
              }
              var $ = function() {
                  for (var e in m) m.hasOwnProperty(e) && w(e, m[e], t, t.interpolateParams, t
                    .defaultText);
                },
                w = function(t, n, r, i, o) {
                  n ? e(n, i, u, o).then(function(e) {
                    T(e, r, !0, t);
                  }, function(e) {
                    T(e, r, !1, t);
                  }) : T(n, r, !1, t);
                },
                T = function(t, n, i, o) {
                  if ("translate" === o) {
                    i || "undefined" == typeof n.defaultText || (t = n.defaultText), h.html(n.preText +
                      t + n.postText);
                    var a = e.isPostCompilingEnabled(),
                      c = "undefined" != typeof s.translateCompile,
                      u = c && "false" !== s.translateCompile;
                    (a && !c || u) && r(h.contents())(n);
                  } else {
                    i || "undefined" == typeof n.defaultText || (t = n.defaultText);
                    var l = p.$attr[o].substr(15);
                    h.attr(l, t);
                  }
                };
              t.$watch("interpolateParams", $, !0);
              var C = o.$on("$translateChangeSuccess", $);
              h.text().length && v(""), $(), t.$on("$destroy", C);
            };
          }
        };
      }
    ]), angular.module("pascalprecht.translate").directive("translateCloak", ["$rootScope", "$translate",
      function(e, t) {
        return {
          compile: function(n) {
            var r = function() {
                n.addClass(t.cloakClassName());
              },
              i = function() {
                n.removeClass(t.cloakClassName());
              },
              o = e.$on("$translateChangeEnd", function() {
                i(), o(), o = null;
              });
            return r(),
              function(e, n, o) {
                o.translateCloak && o.translateCloak.length && o.$observe("translateCloak", function(e) {
                  t(e).then(i, r);
                });
              };
          }
        };
      }
    ]), angular.module("pascalprecht.translate").filter("translate", ["$parse", "$translate", function(e, t) {
      var n = function(n, r, i) {
        return angular.isObject(r) || (r = e(r)(this)), t.instant(n, r, i);
      };
      return n.$stateful = !0, n;
    }]);
}
