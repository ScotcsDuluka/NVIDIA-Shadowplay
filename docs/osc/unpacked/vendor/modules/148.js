// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 148
// role       : service $resolve | service $templateFactory | directive uiView | directive uiSref | directive uiSrefActive | directive uiSrefActiveEq
// defines    : angular.module("ui.router.util")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  /**
   * State-based routing for AngularJS
   * @version v0.2.15
   * @link http://angular-ui.github.com/
   * @license MIT License, http://www.opensource.org/licenses/MIT
   */
  "undefined" != typeof e && "undefined" != typeof t && e.exports === t && (e.exports = "ui.router"),
    function(e, t, n) {
      "use strict";

      function r(e, t) {
        return F(new(F(function() {}, {
          prototype: e
        })), t)
      }

      function i(e) {
        return U(arguments, function(t) {
          t !== e && U(t, function(t, n) {
            e.hasOwnProperty(n) || (e[n] = t)
          })
        }), e
      }

      function o(e, t) {
        var n = [];
        for (var r in e.path) {
          if (e.path[r] !== t.path[r]) break;
          n.push(e.path[r])
        }
        return n
      }

      function a(e) {
        if (Object.keys) return Object.keys(e);
        var t = [];
        return U(e, function(e, n) {
          t.push(n)
        }), t
      }

      function s(e, t) {
        if (Array.prototype.indexOf) return e.indexOf(t, Number(arguments[2]) || 0);
        var n = e.length >>> 0,
          r = Number(arguments[2]) || 0;
        for (r = r < 0 ? Math.ceil(r) : Math.floor(r), r < 0 && (r += n); r < n; r++)
          if (r in e && e[r] === t) return r;
        return -1
      }

      function c(e, t, n, r) {
        var i, c = o(n, r),
          u = {},
          l = [];
        for (var d in c)
          if (c[d].params && (i = a(c[d].params), i.length))
            for (var f in i) s(l, i[f]) >= 0 || (l.push(i[f]), u[i[f]] = e[i[f]]);
        return F({}, u, t)
      }

      function u(e, t, n) {
        if (!n) {
          n = [];
          for (var r in e) n.push(r)
        }
        for (var i = 0; i < n.length; i++) {
          var o = n[i];
          if (e[o] != t[o]) return !1
        }
        return !0
      }

      function l(e, t) {
        var n = {};
        return U(e, function(e) {
          n[e] = t[e]
        }), n
      }

      function d(e) {
        var t = {},
          n = Array.prototype.concat.apply(Array.prototype, Array.prototype.slice.call(arguments, 1));
        return U(n, function(n) {
          n in e && (t[n] = e[n])
        }), t
      }

      function f(e) {
        var t = {},
          n = Array.prototype.concat.apply(Array.prototype, Array.prototype.slice.call(arguments, 1));
        for (var r in e) s(n, r) == -1 && (t[r] = e[r]);
        return t
      }

      function h(e, t) {
        var n = L(e),
          r = n ? [] : {};
        return U(e, function(e, i) {
          t(e, i) && (r[n ? r.length : i] = e)
        }), r
      }

      function p(e, t) {
        var n = L(e) ? [] : {};
        return U(e, function(e, r) {
          n[r] = t(e, r)
        }), n
      }

      function m(e, t) {
        var r = 1,
          o = 2,
          c = {},
          u = [],
          l = c,
          d = F(e.when(c), {
            $$promises: c,
            $$values: c
          });
        this.study = function(c) {
          function h(e, n) {
            if (y[n] !== o) {
              if (g.push(n), y[n] === r) throw g.splice(0, s(g, n)), new Error("Cyclic dependency: " + g.join(
              " -> "));
              if (y[n] = r, R(e)) v.push(n, [function() {
                return t.get(e)
              }], u);
              else {
                var i = t.annotate(e);
                U(i, function(e) {
                  e !== n && c.hasOwnProperty(e) && h(c[e], e)
                }), v.push(n, e, i)
              }
              g.pop(), y[n] = o
            }
          }

          function p(e) {
            return P(e) && e.then && e.$$promises
          }
          if (!P(c)) throw new Error("'invocables' must be an object");
          var m = a(c || {}),
            v = [],
            g = [],
            y = {};
          return U(c, h), c = g = y = null,
            function(r, o, a) {
              function s() {
                --E || (_ || i(b, o.$$values), g.$$values = b, g.$$promises = g.$$promises || !0, delete g
                  .$$inheritedValues, h.resolve(b))
              }

              function c(e) {
                g.$$failure = e, h.reject(e)
              }

              function u(n, i, o) {
                function u(e) {
                  d.reject(e), c(e)
                }

                function l() {
                  if (!O(g.$$failure)) try {
                    d.resolve(t.invoke(i, a, b)), d.promise.then(function(e) {
                      b[n] = e, s()
                    }, u)
                  } catch (e) {
                    u(e)
                  }
                }
                var d = e.defer(),
                  f = 0;
                U(o, function(e) {
                  y.hasOwnProperty(e) && !r.hasOwnProperty(e) && (f++, y[e].then(function(t) {
                    b[e] = t, --f || l()
                  }, u))
                }), f || l(), y[n] = d.promise
              }
              if (p(r) && a === n && (a = o, o = r, r = null), r) {
                if (!P(r)) throw new Error("'locals' must be an object")
              } else r = l;
              if (o) {
                if (!p(o)) throw new Error("'parent' must be a promise returned by $resolve.resolve()")
              } else o = d;
              var h = e.defer(),
                g = h.promise,
                y = g.$$promises = {},
                b = F({}, r),
                E = 1 + v.length / 3,
                _ = !1;
              if (O(o.$$failure)) return c(o.$$failure), g;
              o.$$inheritedValues && i(b, f(o.$$inheritedValues, m)), F(y, o.$$promises), o.$$values ? (_ = i(b, f(o
                .$$values, m)), g.$$inheritedValues = f(o.$$values, m), s()) : (o.$$inheritedValues && (g
                .$$inheritedValues = f(o.$$inheritedValues, m)), o.then(s, c));
              for (var $ = 0, w = v.length; $ < w; $ += 3) r.hasOwnProperty(v[$]) ? s() : u(v[$], v[$ + 1], v[$ + 2]);
              return g
            }
        }, this.resolve = function(e, t, n, r) {
          return this.study(e)(t, n, r)
        }
      }

      function v(e, t, n) {
        this.fromConfig = function(e, t, n) {
          return O(e.template) ? this.fromString(e.template, t) : O(e.templateUrl) ? this.fromUrl(e.templateUrl, t) :
            O(e.templateProvider) ? this.fromProvider(e.templateProvider, t, n) : null
        }, this.fromString = function(e, t) {
          return D(e) ? e(t) : e
        }, this.fromUrl = function(n, r) {
          return D(n) && (n = n(r)), null == n ? null : e.get(n, {
            cache: t,
            headers: {
              Accept: "text/html"
            }
          }).then(function(e) {
            return e.data
          })
        }, this.fromProvider = function(e, t, r) {
          return n.invoke(e, null, r || {
            params: t
          })
        }
      }

      function g(e, t, i) {
        function o(t, n, r, i) {
          if (v.push(t), p[t]) return p[t];
          if (!/^\w+(-+\w+)*(?:\[\])?$/.test(t)) throw new Error("Invalid parameter name '" + t + "' in pattern '" + e +
            "'");
          if (m[t]) throw new Error("Duplicate parameter name '" + t + "' in pattern '" + e + "'");
          return m[t] = new H.Param(t, n, r, i), m[t]
        }

        function a(e, t, n, r) {
          var i = ["", ""],
            o = e.replace(/[\\\[\]\^$*+?.()|{}]/g, "\\$&");
          if (!t) return o;
          switch (n) {
            case !1:
              i = ["(", ")" + (r ? "?" : "")];
              break;
            case !0:
              i = ["?(", ")?"];
              break;
            default:
              i = ["(" + n + "|", ")?"]
          }
          return o + i[0] + t + i[1]
        }

        function s(i, o) {
          var a, s, c, u, l;
          return a = i[2] || i[3], l = t.params[a], c = e.substring(f, i.index), s = o ? i[4] : i[4] || ("*" == i[1] ?
            ".*" : null), u = H.type(s || "string") || r(H.type("string"), {
            pattern: new RegExp(s, t.caseInsensitive ? "i" : n)
          }), {
            id: a,
            regexp: s,
            segment: c,
            type: u,
            cfg: l
          }
        }
        t = F({
          params: {}
        }, P(t) ? t : {});
        var c, u = /([:*])([\w\[\]]+)|\{([\w\[\]]+)(?:\:((?:[^{}\\]+|\\.|\{(?:[^{}\\]+|\\.)*\})+))?\}/g,
          l = /([:]?)([\w\[\]-]+)|\{([\w\[\]-]+)(?:\:((?:[^{}\\]+|\\.|\{(?:[^{}\\]+|\\.)*\})+))?\}/g,
          d = "^",
          f = 0,
          h = this.segments = [],
          p = i ? i.params : {},
          m = this.params = i ? i.params.$$new() : new H.ParamSet,
          v = [];
        this.source = e;
        for (var g, y, b;
          (c = u.exec(e)) && (g = s(c, !1), !(g.segment.indexOf("?") >= 0));) y = o(g.id, g.type, g.cfg, "path"), d +=
          a(g.segment, y.type.pattern.source, y.squash, y.isOptional), h.push(g.segment), f = u.lastIndex;
        b = e.substring(f);
        var E = b.indexOf("?");
        if (E >= 0) {
          var _ = this.sourceSearch = b.substring(E);
          if (b = b.substring(0, E), this.sourcePath = e.substring(0, f + E), _.length > 0)
            for (f = 0; c = l.exec(_);) g = s(c, !0), y = o(g.id, g.type, g.cfg, "search"), f = u.lastIndex
        } else this.sourcePath = e, this.sourceSearch = "";
        d += a(b) + (t.strict === !1 ? "/?" : "") + "$", h.push(b), this.regexp = new RegExp(d, t.caseInsensitive ?
          "i" : n), this.prefix = h[0], this.$$paramNames = v
      }

      function y(e) {
        F(this, e)
      }

      function b() {
        function e(e) {
          return null != e ? e.toString().replace(/\//g, "%2F") : e
        }

        function i(e) {
          return null != e ? e.toString().replace(/%2F/g, "/") : e
        }

        function o() {
          return {
            strict: m,
            caseInsensitive: f
          }
        }

        function c(e) {
          return D(e) || L(e) && D(e[e.length - 1])
        }

        function u() {
          for (; $.length;) {
            var e = $.shift();
            if (e.pattern) throw new Error("You cannot override a type's .pattern at runtime.");
            t.extend(E[e.name], d.invoke(e.def))
          }
        }

        function l(e) {
          F(this, e || {})
        }
        H = this;
        var d, f = !1,
          m = !0,
          v = !1,
          E = {},
          _ = !0,
          $ = [],
          w = {
            string: {
              encode: e,
              decode: i,
              is: function(e) {
                return null == e || !O(e) || "string" == typeof e
              },
              pattern: /[^\/]*/
            },
            int: {
              encode: e,
              decode: function(e) {
                return parseInt(e, 10)
              },
              is: function(e) {
                return O(e) && this.decode(e.toString()) === e
              },
              pattern: /\d+/
            },
            bool: {
              encode: function(e) {
                return e ? 1 : 0
              },
              decode: function(e) {
                return 0 !== parseInt(e, 10)
              },
              is: function(e) {
                return e === !0 || e === !1
              },
              pattern: /0|1/
            },
            date: {
              encode: function(e) {
                return this.is(e) ? [e.getFullYear(), ("0" + (e.getMonth() + 1)).slice(-2), ("0" + e.getDate()).slice(
                  -2)].join("-") : n
              },
              decode: function(e) {
                if (this.is(e)) return e;
                var t = this.capture.exec(e);
                return t ? new Date(t[1], t[2] - 1, t[3]) : n
              },
              is: function(e) {
                return e instanceof Date && !isNaN(e.valueOf())
              },
              equals: function(e, t) {
                return this.is(e) && this.is(t) && e.toISOString() === t.toISOString()
              },
              pattern: /[0-9]{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[1-2][0-9]|3[0-1])/,
              capture: /([0-9]{4})-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1])/
            },
            json: {
              encode: t.toJson,
              decode: t.fromJson,
              is: t.isObject,
              equals: t.equals,
              pattern: /[^\/]*/
            },
            any: {
              encode: t.identity,
              decode: t.identity,
              equals: t.equals,
              pattern: /.*/
            }
          };
        b.$$getDefaultValue = function(e) {
          if (!c(e.value)) return e.value;
          if (!d) throw new Error("Injectable functions cannot be called at configuration time");
          return d.invoke(e.value)
        }, this.caseInsensitive = function(e) {
          return O(e) && (f = e), f
        }, this.strictMode = function(e) {
          return O(e) && (m = e), m
        }, this.defaultSquashPolicy = function(e) {
          if (!O(e)) return v;
          if (e !== !0 && e !== !1 && !R(e)) throw new Error("Invalid squash policy: " + e +
            ". Valid policies: false, true, arbitrary-string");
          return v = e, e
        }, this.compile = function(e, t) {
          return new g(e, F(o(), t))
        }, this.isMatcher = function(e) {
          if (!P(e)) return !1;
          var t = !0;
          return U(g.prototype, function(n, r) {
            D(n) && (t = t && O(e[r]) && D(e[r]))
          }), t
        }, this.type = function(e, t, n) {
          if (!O(t)) return E[e];
          if (E.hasOwnProperty(e)) throw new Error("A type named '" + e + "' has already been defined.");
          return E[e] = new y(F({
            name: e
          }, t)), n && ($.push({
            name: e,
            def: n
          }), _ || u()), this
        }, U(w, function(e, t) {
          E[t] = new y(F({
            name: t
          }, e))
        }), E = r(E, {}), this.$get = ["$injector", function(e) {
          return d = e, _ = !1, u(), U(w, function(e, t) {
            E[t] || (E[t] = new y(e))
          }), this
        }], this.Param = function(e, t, r, i) {
          function o(e) {
            var t = P(e) ? a(e) : [],
              n = s(t, "value") === -1 && s(t, "type") === -1 && s(t, "squash") === -1 && s(t, "array") === -1;
            return n && (e = {
              value: e
            }), e.$$fn = c(e.value) ? e.value : function() {
              return e.value
            }, e
          }

          function u(t, n, r) {
            if (t.type && n) throw new Error("Param '" + e + "' has two type configurations.");
            return n ? n : t.type ? t.type instanceof y ? t.type : new y(t.type) : "config" === r ? E.any : E.string
          }

          function l() {
            var t = {
                array: "search" === i && "auto"
              },
              n = e.match(/\[\]$/) ? {
                array: !0
              } : {};
            return F(t, n, r).array
          }

          function f(e, t) {
            var n = e.squash;
            if (!t || n === !1) return !1;
            if (!O(n) || null == n) return v;
            if (n === !0 || R(n)) return n;
            throw new Error("Invalid squash policy: '" + n + "'. Valid policies: false, true, or arbitrary string")
          }

          function m(e, t, r, i) {
            var o, a, c = [{
              from: "",
              to: r || t ? n : ""
            }, {
              from: null,
              to: r || t ? n : ""
            }];
            return o = L(e.replace) ? e.replace : [], R(i) && o.push({
              from: i,
              to: n
            }), a = p(o, function(e) {
              return e.from
            }), h(c, function(e) {
              return s(a, e.from) === -1
            }).concat(o)
          }

          function g() {
            if (!d) throw new Error("Injectable functions cannot be called at configuration time");
            var e = d.invoke(r.$$fn);
            if (null !== e && e !== n && !$.type.is(e)) throw new Error("Default value (" + e + ") for parameter '" +
              $.id + "' is not an instance of Type (" + $.type.name + ")");
            return e
          }

          function b(e) {
            function t(e) {
              return function(t) {
                return t.from === e
              }
            }

            function n(e) {
              var n = p(h($.replace, t(e)), function(e) {
                return e.to
              });
              return n.length ? n[0] : e
            }
            return e = n(e), O(e) ? $.type.$normalize(e) : g()
          }

          function _() {
            return "{Param:" + e + " " + t + " squash: '" + C + "' optional: " + T + "}"
          }
          var $ = this;
          r = o(r), t = u(r, t, i);
          var w = l();
          t = w ? t.$asArray(w, "search" === i) : t, "string" !== t.name || w || "path" !== i || r.value !== n || (r
            .value = "");
          var T = r.value !== n,
            C = f(r, T),
            x = m(r, w, T, C);
          F(this, {
            id: e,
            type: t,
            location: i,
            array: w,
            squash: C,
            replace: x,
            isOptional: T,
            value: b,
            dynamic: n,
            config: r,
            toString: _
          })
        }, l.prototype = {
          $$new: function() {
            return r(this, F(new l, {
              $$parent: this
            }))
          },
          $$keys: function() {
            for (var e = [], t = [], n = this, r = a(l.prototype); n;) t.push(n), n = n.$$parent;
            return t.reverse(), U(t, function(t) {
              U(a(t), function(t) {
                s(e, t) === -1 && s(r, t) === -1 && e.push(t)
              })
            }), e
          },
          $$values: function(e) {
            var t = {},
              n = this;
            return U(n.$$keys(), function(r) {
              t[r] = n[r].value(e && e[r])
            }), t
          },
          $$equals: function(e, t) {
            var n = !0,
              r = this;
            return U(r.$$keys(), function(i) {
              var o = e && e[i],
                a = t && t[i];
              r[i].type.equals(o, a) || (n = !1)
            }), n
          },
          $$validates: function(e) {
            var r, i, o, a, s, c = this.$$keys();
            for (r = 0; r < c.length && (i = this[c[r]], o = e[c[r]], o !== n && null !== o || !i
              .isOptional); r++) {
              if (a = i.type.$normalize(o), !i.type.is(a)) return !1;
              if (s = i.type.encode(a), t.isString(s) && !i.type.pattern.exec(s)) return !1
            }
            return !0
          },
          $$parent: n
        }, this.ParamSet = l
      }

      function E(e, r) {
        function i(e) {
          var t = /^\^((?:\\[^a-zA-Z0-9]|[^\\\[\]\^$*+?.()|{}]+)*)/.exec(e.source);
          return null != t ? t[1].replace(/\\(.)/g, "$1") : ""
        }

        function o(e, t) {
          return e.replace(/\$(\$|\d{1,2})/, function(e, n) {
            return t["$" === n ? 0 : Number(n)]
          })
        }

        function a(e, t, n) {
          if (!n) return !1;
          var r = e.invoke(t, t, {
            $match: n
          });
          return !O(r) || r
        }

        function s(r, i, o, a) {
          function s(e, t, n) {
            return "/" === m ? e : t ? m.slice(0, -1) + e : n ? m.slice(1) + e : e
          }

          function f(e) {
            function t(e) {
              var t = e(o, r);
              return !!t && (R(t) && r.replace().url(t), !0)
            }
            if (!e || !e.defaultPrevented) {
              p && r.url() === p;
              p = n;
              var i, a = u.length;
              for (i = 0; i < a; i++)
                if (t(u[i])) return;
              l && t(l)
            }
          }

          function h() {
            return c = c || i.$on("$locationChangeSuccess", f)
          }
          var p, m = a.baseHref(),
            v = r.url();
          return d || h(), {
            sync: function() {
              f()
            },
            listen: function() {
              return h()
            },
            update: function(e) {
              return e ? void(v = r.url()) : void(r.url() !== v && (r.url(v), r.replace()))
            },
            push: function(e, t, i) {
              var o = e.format(t || {});
              null !== o && t && t["#"] && (o += "#" + t["#"]), r.url(o), p = i && i.$$avoidResync ? r.url() : n,
                i && i.replace && r.replace()
            },
            href: function(n, i, o) {
              if (!n.validates(i)) return null;
              var a = e.html5Mode();
              t.isObject(a) && (a = a.enabled);
              var c = n.format(i);
              if (o = o || {}, a || null === c || (c = "#" + e.hashPrefix() + c), null !== c && i && i["#"] && (c +=
                  "#" + i["#"]), c = s(c, a, o.absolute), !o.absolute || !c) return c;
              var u = !a && c ? "/" : "",
                l = r.port();
              return l = 80 === l || 443 === l ? "" : ":" + l, [r.protocol(), "://", r.host(), l, u, c].join("")
            }
          }
        }
        var c, u = [],
          l = null,
          d = !1;
        this.rule = function(e) {
          if (!D(e)) throw new Error("'rule' must be a function");
          return u.push(e), this
        }, this.otherwise = function(e) {
          if (R(e)) {
            var t = e;
            e = function() {
              return t
            }
          } else if (!D(e)) throw new Error("'rule' must be a function");
          return l = e, this
        }, this.when = function(e, t) {
          var n, s = R(t);
          if (R(e) && (e = r.compile(e)), !s && !D(t) && !L(t)) throw new Error("invalid 'handler' in when()");
          var c = {
              matcher: function(e, t) {
                return s && (n = r.compile(t), t = ["$match", function(e) {
                  return n.format(e)
                }]), F(function(n, r) {
                  return a(n, t, e.exec(r.path(), r.search()))
                }, {
                  prefix: R(e.prefix) ? e.prefix : ""
                })
              },
              regex: function(e, t) {
                if (e.global || e.sticky) throw new Error("when() RegExp must not be global or sticky");
                return s && (n = t, t = ["$match", function(e) {
                  return o(n, e)
                }]), F(function(n, r) {
                  return a(n, t, e.exec(r.path()))
                }, {
                  prefix: i(e)
                })
              }
            },
            u = {
              matcher: r.isMatcher(e),
              regex: e instanceof RegExp
            };
          for (var l in u)
            if (u[l]) return this.rule(c[l](e, t));
          throw new Error("invalid 'what' in when()")
        }, this.deferIntercept = function(e) {
          e === n && (e = !0), d = e
        }, this.$get = s, s.$inject = ["$location", "$rootScope", "$injector", "$browser"]
      }

      function _(e, i) {
        function o(e) {
          return 0 === e.indexOf(".") || 0 === e.indexOf("^")
        }

        function f(e, t) {
          if (!e) return n;
          var r = R(e),
            i = r ? e : e.name,
            a = o(i);
          if (a) {
            if (!t) throw new Error("No reference point given for path '" + i + "'");
            t = f(t);
            for (var s = i.split("."), c = 0, u = s.length, l = t; c < u; c++)
              if ("" !== s[c] || 0 !== c) {
                if ("^" !== s[c]) break;
                if (!l.parent) throw new Error("Path '" + i + "' not valid for state '" + t.name + "'");
                l = l.parent
              } else l = t;
            s = s.slice(c).join("."), i = l.name + (l.name && s ? "." : "") + s
          }
          var d = C[i];
          return !d || !r && (r || d !== e && d.self !== e) ? n : d
        }

        function h(e, t) {
          x[e] || (x[e] = []), x[e].push(t)
        }

        function m(e) {
          for (var t = x[e] || []; t.length;) v(t.shift())
        }

        function v(t) {
          t = r(t, {
            self: t,
            resolve: t.resolve || {},
            toString: function() {
              return this.name
            }
          });
          var n = t.name;
          if (!R(n) || n.indexOf("@") >= 0) throw new Error("State must have a valid name");
          if (C.hasOwnProperty(n)) throw new Error("State '" + n + "'' is already defined");
          var i = n.indexOf(".") !== -1 ? n.substring(0, n.lastIndexOf(".")) : R(t.parent) ? t.parent : P(t.parent) &&
            R(t.parent.name) ? t.parent.name : "";
          if (i && !C[i]) return h(i, t.self);
          for (var o in A) D(A[o]) && (t[o] = A[o](t, A.$delegates[o]));
          return C[n] = t, !t[S] && t.url && e.when(t.url, ["$match", "$stateParams", function(e, n) {
            T.$current.navigable == t && u(e, n) || T.transitionTo(t, e, {
              inherit: !0,
              location: !1
            })
          }]), m(n), t
        }

        function g(e) {
          return e.indexOf("*") > -1
        }

        function y(e) {
          for (var t = e.split("."), n = T.$current.name.split("."), r = 0, i = t.length; r < i; r++) "*" === t[r] && (
            n[r] = "*");
          return "**" === t[0] && (n = n.slice(s(n, t[1])), n.unshift("**")), "**" === t[t.length - 1] && (n.splice(s(n,
            t[t.length - 2]) + 1, Number.MAX_VALUE), n.push("**")), t.length == n.length && n.join("") === t.join("")
        }

        function b(e, t) {
          return R(e) && !O(t) ? A[e] : D(t) && R(e) ? (A[e] && !A.$delegates[e] && (A.$delegates[e] = A[e]), A[e] = t,
            this) : this
        }

        function E(e, t) {
          return P(e) ? t = e : t.name = e, v(t), this
        }

        function _(e, i, o, s, d, h, m, v, b) {
          function E(t, n, r, o) {
            var a = e.$broadcast("$stateNotFound", t, n, r);
            if (a.defaultPrevented) return m.update(), M;
            if (!a.retry) return null;
            if (o.$retry) return m.update(), k;
            var s = T.transition = i.when(a.retry);
            return s.then(function() {
              return s !== T.transition ? x : (t.options.$retry = !0, T.transitionTo(t.to, t.toParams, t.options))
            }, function() {
              return M
            }), m.update(), s
          }

          function _(e, n, r, a, c, u) {
            function f() {
              var n = [];
              return U(e.views, function(r, i) {
                var a = r.resolve && r.resolve !== e.resolve ? r.resolve : {};
                a.$template = [function() {
                  return o.load(i, {
                    view: r,
                    locals: c.globals,
                    params: h,
                    notify: u.notify
                  }) || ""
                }], n.push(d.resolve(a, c.globals, c.resolve, e).then(function(n) {
                  if (D(r.controllerProvider) || L(r.controllerProvider)) {
                    var o = t.extend({}, a, c.globals);
                    n.$$controller = s.invoke(r.controllerProvider, null, o)
                  } else n.$$controller = r.controller;
                  n.$$state = e, n.$$controllerAs = r.controllerAs, c[i] = n
                }))
              }), i.all(n).then(function() {
                return c.globals
              })
            }
            var h = r ? n : l(e.params.$$keys(), n),
              p = {
                $stateParams: h
              };
            c.resolve = d.resolve(e.resolve, p, c.resolve, e);
            var m = [c.resolve.then(function(e) {
              c.globals = e
            })];
            return a && m.push(a), i.all(m).then(f).then(function(e) {
              return c
            })
          }
          var x = i.reject(new Error("transition superseded")),
            A = i.reject(new Error("transition prevented")),
            M = i.reject(new Error("transition aborted")),
            k = i.reject(new Error("transition failed"));
          return w.locals = {
            resolve: null,
            globals: {
              $stateParams: {}
            }
          }, T = {
            params: {},
            current: w.self,
            $current: w,
            transition: null
          }, T.reload = function(e) {
            return T.transitionTo(T.current, h, {
              reload: e || !0,
              inherit: !1,
              notify: !0
            })
          }, T.go = function(e, t, n) {
            return T.transitionTo(e, t, F({
              inherit: !0,
              relative: T.$current
            }, n))
          }, T.transitionTo = function(t, n, o) {
            n = n || {}, o = F({
              location: !0,
              inherit: !1,
              relative: null,
              notify: !0,
              reload: !1,
              $retry: !1
            }, o || {});
            var a, u = T.$current,
              d = T.params,
              p = u.path,
              v = f(t, o.relative),
              g = n["#"];
            if (!O(v)) {
              var y = {
                  to: t,
                  toParams: n,
                  options: o
                },
                b = E(y, u.self, d, o);
              if (b) return b;
              if (t = y.to, n = y.toParams, o = y.options, v = f(t, o.relative), !O(v)) {
                if (!o.relative) throw new Error("No such state '" + t + "'");
                throw new Error("Could not resolve '" + t + "' from state '" + o.relative + "'")
              }
            }
            if (v[S]) throw new Error("Cannot transition to abstract state '" + t + "'");
            if (o.inherit && (n = c(h, n || {}, T.$current, v)), !v.params.$$validates(n)) return k;
            n = v.params.$$values(n), t = v;
            var C = t.path,
              M = 0,
              N = C[M],
              I = w.locals,
              D = [];
            if (o.reload) {
              if (R(o.reload) || P(o.reload)) {
                if (P(o.reload) && !o.reload.name) throw new Error("Invalid reload state object");
                var L = o.reload === !0 ? p[0] : f(o.reload);
                if (o.reload && !L) throw new Error("No such reload state '" + (R(o.reload) ? o.reload : o.reload
                  .name) + "'");
                for (; N && N === p[M] && N !== L;) I = D[M] = N.locals, M++, N = C[M]
              }
            } else
              for (; N && N === p[M] && N.ownParams.$$equals(n, d);) I = D[M] = N.locals, M++, N = C[M];
            if ($(t, n, u, d, I, o)) return g && (n["#"] = g), T.params = n, j(T.params, h), o.location && t
              .navigable && t.navigable.url && (m.push(t.navigable.url, n, {
                $$avoidResync: !0,
                replace: "replace" === o.location
              }), m.update(!0)), T.transition = null, i.when(T.current);
            if (n = l(t.params.$$keys(), n || {}), o.notify && e.$broadcast("$stateChangeStart", t.self, n, u.self, d)
              .defaultPrevented) return e.$broadcast("$stateChangeCancel", t.self, n, u.self, d), m.update(), A;
            for (var U = i.when(I), H = M; H < C.length; H++, N = C[H]) I = D[H] = r(I), U = _(N, n, N === t, U, I,
            o);
            var B = T.transition = U.then(function() {
              var r, i, a;
              if (T.transition !== B) return x;
              for (r = p.length - 1; r >= M; r--) a = p[r], a.self.onExit && s.invoke(a.self.onExit, a.self, a
                .locals.globals), a.locals = null;
              for (r = M; r < C.length; r++) i = C[r], i.locals = D[r], i.self.onEnter && s.invoke(i.self.onEnter,
                i.self, i.locals.globals);
              return g && (n["#"] = g), T.transition !== B ? x : (T.$current = t, T.current = t.self, T.params =
                n, j(T.params, h), T.transition = null, o.location && t.navigable && m.push(t.navigable.url, t
                  .navigable.locals.globals.$stateParams, {
                    $$avoidResync: !0,
                    replace: "replace" === o.location
                  }), o.notify && e.$broadcast("$stateChangeSuccess", t.self, n, u.self, d), m.update(!0), T
                .current)
            }, function(r) {
              return T.transition !== B ? x : (T.transition = null, a = e.$broadcast("$stateChangeError", t.self,
                n, u.self, d, r), a.defaultPrevented || m.update(), i.reject(r))
            });
            return B
          }, T.is = function(e, t, r) {
            r = F({
              relative: T.$current
            }, r || {});
            var i = f(e, r.relative);
            return O(i) ? T.$current === i && (!t || u(i.params.$$values(t), h)) : n
          }, T.includes = function(e, t, r) {
            if (r = F({
                relative: T.$current
              }, r || {}), R(e) && g(e)) {
              if (!y(e)) return !1;
              e = T.$current.name
            }
            var i = f(e, r.relative);
            return O(i) ? !!O(T.$current.includes[i.name]) && (!t || u(i.params.$$values(t), h, a(t))) : n
          }, T.href = function(e, t, r) {
            r = F({
              lossy: !0,
              inherit: !0,
              absolute: !1,
              relative: T.$current
            }, r || {});
            var i = f(e, r.relative);
            if (!O(i)) return null;
            r.inherit && (t = c(h, t || {}, T.$current, i));
            var o = i && r.lossy ? i.navigable : i;
            return o && o.url !== n && null !== o.url ? m.href(o.url, l(i.params.$$keys().concat("#"), t || {}), {
              absolute: r.absolute
            }) : null
          }, T.get = function(e, t) {
            if (0 === arguments.length) return p(a(C), function(e) {
              return C[e].self
            });
            var n = f(e, t || T.$current);
            return n && n.self ? n.self : null
          }, T
        }

        function $(e, t, n, r, i, o) {
          function a(e, t, n) {
            function r(t) {
              return "search" != e.params[t].location
            }
            var i = e.params.$$keys().filter(r),
              o = d.apply({}, [e.params].concat(i)),
              a = new H.ParamSet(o);
            return a.$$equals(t, n)
          }
          if (!o.reload && e === n && (i === n.locals || e.self.reloadOnSearch === !1 && a(n, r, t))) return !0
        }
        var w, T, C = {},
          x = {},
          S = "abstract",
          A = {
            parent: function(e) {
              if (O(e.parent) && e.parent) return f(e.parent);
              var t = /^(.+)\.[^.]+$/.exec(e.name);
              return t ? f(t[1]) : w
            },
            data: function(e) {
              return e.parent && e.parent.data && (e.data = e.self.data = F({}, e.parent.data, e.data)), e.data
            },
            url: function(e) {
              var t = e.url,
                n = {
                  params: e.params || {}
                };
              if (R(t)) return "^" == t.charAt(0) ? i.compile(t.substring(1), n) : (e.parent.navigable || w).url
                .concat(t, n);
              if (!t || i.isMatcher(t)) return t;
              throw new Error("Invalid url '" + t + "' in state '" + e + "'")
            },
            navigable: function(e) {
              return e.url ? e : e.parent ? e.parent.navigable : null
            },
            ownParams: function(e) {
              var t = e.url && e.url.params || new H.ParamSet;
              return U(e.params || {}, function(e, n) {
                t[n] || (t[n] = new H.Param(n, null, e, "config"))
              }), t
            },
            params: function(e) {
              return e.parent && e.parent.params ? F(e.parent.params.$$new(), e.ownParams) : new H.ParamSet
            },
            views: function(e) {
              var t = {};
              return U(O(e.views) ? e.views : {
                "": e
              }, function(n, r) {
                r.indexOf("@") < 0 && (r += "@" + e.parent.name), t[r] = n
              }), t
            },
            path: function(e) {
              return e.parent ? e.parent.path.concat(e) : []
            },
            includes: function(e) {
              var t = e.parent ? F({}, e.parent.includes) : {};
              return t[e.name] = !0, t
            },
            $delegates: {}
          };
        w = v({
          name: "",
          url: "^",
          views: null,
          abstract: !0
        }), w.navigable = null, this.decorator = b, this.state = E, this.$get = _, _.$inject = ["$rootScope", "$q",
          "$view", "$injector", "$resolve", "$stateParams", "$urlRouter", "$location", "$urlMatcherFactory"
        ]
      }

      function $() {
        function e(e, t) {
          return {
            load: function(n, r) {
              var i, o = {
                template: null,
                controller: null,
                view: null,
                locals: null,
                notify: !0,
                async: !0,
                params: {}
              };
              return r = F(o, r), r.view && (i = t.fromConfig(r.view, r.params, r.locals)), i && r.notify && e
                .$broadcast("$viewContentLoading", r), i
            }
          }
        }
        this.$get = e, e.$inject = ["$rootScope", "$templateFactory"]
      }

      function w() {
        var e = !1;
        this.useAnchorScroll = function() {
          e = !0
        }, this.$get = ["$anchorScroll", "$timeout", function(t, n) {
          return e ? t : function(e) {
            return n(function() {
              e[0].scrollIntoView()
            }, 0, !1)
          }
        }]
      }

      function T(e, n, r, i) {
        function o() {
          return n.has ? function(e) {
            return n.has(e) ? n.get(e) : null
          } : function(e) {
            try {
              return n.get(e)
            } catch (e) {
              return null
            }
          }
        }

        function a(e, t) {
          var n = function() {
            return {
              enter: function(e, t, n) {
                t.after(e), n()
              },
              leave: function(e, t) {
                e.remove(), t()
              }
            }
          };
          if (u) return {
            enter: function(e, t, n) {
              var r = u.enter(e, null, t, n);
              r && r.then && r.then(n)
            },
            leave: function(e, t) {
              var n = u.leave(e, t);
              n && n.then && n.then(t)
            }
          };
          if (c) {
            var r = c && c(t, e);
            return {
              enter: function(e, t, n) {
                r.enter(e, null, t), n()
              },
              leave: function(e, t) {
                r.leave(e), t()
              }
            }
          }
          return n()
        }
        var s = o(),
          c = s("$animator"),
          u = s("$animate"),
          l = {
            restrict: "ECA",
            terminal: !0,
            priority: 400,
            transclude: "element",
            compile: function(n, o, s) {
              return function(n, o, c) {
                function u() {
                  d && (d.remove(), d = null), h && (h.$destroy(), h = null), f && (g.leave(f, function() {
                    d = null
                  }), d = f, f = null)
                }

                function l(a) {
                  var l, d = x(n, c, o, i),
                    y = d && e.$current && e.$current.locals[d];
                  if (a || y !== p) {
                    l = n.$new(), p = e.$current.locals[d];
                    var b = s(l, function(e) {
                      g.enter(e, o, function() {
                        h && h.$emit("$viewContentAnimationEnded"), (t.isDefined(v) && !v || n.$eval(v)) &&
                          r(e)
                      }), u()
                    });
                    f = b, h = l, h.$emit("$viewContentLoaded"), h.$eval(m)
                  }
                }
                var d, f, h, p, m = c.onload || "",
                  v = c.autoscroll,
                  g = a(c, n);
                n.$on("$stateChangeSuccess", function() {
                  l(!1)
                }), n.$on("$viewContentLoading", function() {
                  l(!1)
                }), l(!0)
              }
            }
          };
        return l
      }

      function C(e, t, n, r) {
        return {
          restrict: "ECA",
          priority: -400,
          compile: function(i) {
            var o = i.html();
            return function(i, a, s) {
              var c = n.$current,
                u = x(i, s, a, r),
                l = c && c.locals[u];
              if (l) {
                a.data("$uiView", {
                  name: u,
                  state: l.$$state
                }), a.html(l.$template ? l.$template : o);
                var d = e(a.contents());
                if (l.$$controller) {
                  l.$scope = i, l.$element = a;
                  var f = t(l.$$controller, l);
                  l.$$controllerAs && (i[l.$$controllerAs] = f), a.data("$ngControllerController", f), a.children()
                    .data("$ngControllerController", f)
                }
                d(i)
              }
            }
          }
        }
      }

      function x(e, t, n, r) {
        var i = r(t.uiView || t.name || "")(e),
          o = n.inheritedData("$uiView");
        return i.indexOf("@") >= 0 ? i : i + "@" + (o ? o.state.name : "")
      }

      function S(e, t) {
        var n, r = e.match(/^\s*({[^}]*})\s*$/);
        if (r && (e = t + "(" + r[1] + ")"), n = e.replace(/\n/g, " ").match(/^([^(]+?)\s*(\((.*)\))?$/), !n || 4 !== n
          .length) throw new Error("Invalid state ref '" + e + "'");
        return {
          state: n[1],
          paramExpr: n[3] || null
        }
      }

      function A(e) {
        var t = e.parent().inheritedData("$uiView");
        if (t && t.state && t.state.name) return t.state
      }

      function M(e, n) {
        var r = ["location", "inherit", "reload", "absolute"];
        return {
          restrict: "A",
          require: ["?^uiSrefActive", "?^uiSrefActiveEq"],
          link: function(i, o, a, s) {
            var c = S(a.uiSref, e.current.name),
              u = null,
              l = A(o) || e.$current,
              d = "[object SVGAnimatedString]" === Object.prototype.toString.call(o.prop("href")) ? "xlink:href" :
              "href",
              f = null,
              h = "A" === o.prop("tagName").toUpperCase(),
              p = "FORM" === o[0].nodeName,
              m = p ? "action" : d,
              v = !0,
              g = {
                relative: l,
                inherit: !0
              },
              y = i.$eval(a.uiSrefOpts) || {};
            t.forEach(r, function(e) {
              e in y && (g[e] = y[e])
            });
            var b = function(n) {
              if (n && (u = t.copy(n)), v) {
                f = e.href(c.state, u, g);
                var r = s[1] || s[0];
                return r && r.$$addStateInfo(c.state, u), null === f ? (v = !1, !1) : void a.$set(m, f)
              }
            };
            c.paramExpr && (i.$watch(c.paramExpr, function(e, t) {
              e !== u && b(e)
            }, !0), u = t.copy(i.$eval(c.paramExpr))), b(), p || o.bind("click", function(t) {
              var r = t.which || t.button;
              if (!(r > 1 || t.ctrlKey || t.metaKey || t.shiftKey || o.attr("target"))) {
                var i = n(function() {
                  e.go(c.state, u, g)
                });
                t.preventDefault();
                var a = h && !f ? 1 : 0;
                t.preventDefault = function() {
                  a-- <= 0 && n.cancel(i)
                }
              }
            })
          }
        }
      }

      function k(e, t, n) {
        return {
          restrict: "A",
          controller: ["$scope", "$element", "$attrs", function(t, r, i) {
            function o() {
              a() ? r.addClass(c) : r.removeClass(c)
            }

            function a() {
              for (var e = 0; e < u.length; e++)
                if (s(u[e].state, u[e].params)) return !0;
              return !1
            }

            function s(t, n) {
              return "undefined" != typeof i.uiSrefActiveEq ? e.is(t.name, n) : e.includes(t.name, n)
            }
            var c, u = [];
            c = n(i.uiSrefActiveEq || i.uiSrefActive || "", !1)(t), this.$$addStateInfo = function(t, n) {
              var i = e.get(t, A(r));
              u.push({
                state: i || {
                  name: t
                },
                params: n
              }), o()
            }, t.$on("$stateChangeSuccess", o)
          }]
        }
      }

      function N(e) {
        var t = function(t) {
          return e.is(t)
        };
        return t.$stateful = !0, t
      }

      function I(e) {
        var t = function(t) {
          return e.includes(t)
        };
        return t.$stateful = !0, t
      }
      var O = t.isDefined,
        D = t.isFunction,
        R = t.isString,
        P = t.isObject,
        L = t.isArray,
        U = t.forEach,
        F = t.extend,
        j = t.copy;
      t.module("ui.router.util", ["ng"]), t.module("ui.router.router", ["ui.router.util"]), t.module("ui.router.state",
        ["ui.router.router", "ui.router.util"]), t.module("ui.router", ["ui.router.state"]), t.module(
        "ui.router.compat", ["ui.router"]), m.$inject = ["$q", "$injector"], t.module("ui.router.util").service(
        "$resolve", m), v.$inject = ["$http", "$templateCache", "$injector"], t.module("ui.router.util").service(
        "$templateFactory", v);
      var H;
      g.prototype.concat = function(e, t) {
          var n = {
            caseInsensitive: H.caseInsensitive(),
            strict: H.strictMode(),
            squash: H.defaultSquashPolicy()
          };
          return new g(this.sourcePath + e + this.sourceSearch, F(n, t), this)
        }, g.prototype.toString = function() {
          return this.source
        }, g.prototype.exec = function(e, t) {
          function n(e) {
            function t(e) {
              return e.split("").reverse().join("")
            }

            function n(e) {
              return e.replace(/\\-/g, "-")
            }
            var r = t(e).split(/-(?!\\)/),
              i = p(r, t);
            return p(i, n).reverse()
          }
          var r = this.regexp.exec(e);
          if (!r) return null;
          t = t || {};
          var i, o, a, s = this.parameters(),
            c = s.length,
            u = this.segments.length - 1,
            l = {};
          if (u !== r.length - 1) throw new Error("Unbalanced capture group in route '" + this.source + "'");
          for (i = 0; i < u; i++) {
            a = s[i];
            var d = this.params[a],
              f = r[i + 1];
            for (o = 0; o < d.replace; o++) d.replace[o].from === f && (f = d.replace[o].to);
            f && d.array === !0 && (f = n(f)), l[a] = d.value(f)
          }
          for (; i < c; i++) a = s[i], l[a] = this.params[a].value(t[a]);
          return l
        }, g.prototype.parameters = function(e) {
          return O(e) ? this.params[e] || null : this.$$paramNames
        }, g.prototype.validates = function(e) {
          return this.params.$$validates(e)
        }, g.prototype.format = function(e) {
          function t(e) {
            return encodeURIComponent(e).replace(/-/g, function(e) {
              return "%5C%" + e.charCodeAt(0).toString(16).toUpperCase()
            })
          }
          e = e || {};
          var n = this.segments,
            r = this.parameters(),
            i = this.params;
          if (!this.validates(e)) return null;
          var o, a = !1,
            s = n.length - 1,
            c = r.length,
            u = n[0];
          for (o = 0; o < c; o++) {
            var l = o < s,
              d = r[o],
              f = i[d],
              h = f.value(e[d]),
              m = f.isOptional && f.type.equals(f.value(), h),
              v = !!m && f.squash,
              g = f.type.encode(h);
            if (l) {
              var y = n[o + 1];
              if (v === !1) null != g && (u += L(g) ? p(g, t).join("-") : encodeURIComponent(g)), u += y;
              else if (v === !0) {
                var b = u.match(/\/$/) ? /\/?(.*)/ : /(.*)/;
                u += y.match(b)[1]
              } else R(v) && (u += v + y)
            } else {
              if (null == g || m && v !== !1) continue;
              L(g) || (g = [g]), g = p(g, encodeURIComponent).join("&" + d + "="), u += (a ? "&" : "?") + (d + "=" + g),
                a = !0
            }
          }
          return u
        }, y.prototype.is = function(e, t) {
          return !0
        }, y.prototype.encode = function(e, t) {
          return e
        }, y.prototype.decode = function(e, t) {
          return e
        }, y.prototype.equals = function(e, t) {
          return e == t
        }, y.prototype.$subPattern = function() {
          var e = this.pattern.toString();
          return e.substr(1, e.length - 2)
        }, y.prototype.pattern = /.*/, y.prototype.toString = function() {
          return "{Type:" + this.name + "}"
        }, y.prototype.$normalize = function(e) {
          return this.is(e) ? e : this.decode(e)
        }, y.prototype.$asArray = function(e, t) {
          function r(e, t) {
            function r(e, t) {
              return function() {
                return e[t].apply(e, arguments)
              }
            }

            function i(e) {
              return L(e) ? e : O(e) ? [e] : []
            }

            function o(e) {
              switch (e.length) {
                case 0:
                  return n;
                case 1:
                  return "auto" === t ? e[0] : e;
                default:
                  return e
              }
            }

            function a(e) {
              return !e
            }

            function s(e, t) {
              return function(n) {
                n = i(n);
                var r = p(n, e);
                return t === !0 ? 0 === h(r, a).length : o(r)
              }
            }

            function c(e) {
              return function(t, n) {
                var r = i(t),
                  o = i(n);
                if (r.length !== o.length) return !1;
                for (var a = 0; a < r.length; a++)
                  if (!e(r[a], o[a])) return !1;
                return !0
              }
            }
            this.encode = s(r(e, "encode")), this.decode = s(r(e, "decode")), this.is = s(r(e, "is"), !0), this.equals =
              c(r(e, "equals")), this.pattern = e.pattern, this.$normalize = s(r(e, "$normalize")), this.name = e.name,
              this.$arrayMode = t
          }
          if (!e) return this;
          if ("auto" === e && !t) throw new Error("'auto' array mode is for query parameters only");
          return new r(this, e)
        }, t.module("ui.router.util").provider("$urlMatcherFactory", b), t.module("ui.router.util").run([
          "$urlMatcherFactory",
          function(e) {}
        ]), E.$inject = ["$locationProvider", "$urlMatcherFactoryProvider"], t.module("ui.router.router").provider(
          "$urlRouter", E), _.$inject = ["$urlRouterProvider", "$urlMatcherFactoryProvider"], t.module(
          "ui.router.state").value("$stateParams", {}).provider("$state", _), $.$inject = [], t.module(
          "ui.router.state").provider("$view", $), t.module("ui.router.state").provider("$uiViewScroll", w), T
        .$inject = ["$state", "$injector", "$uiViewScroll", "$interpolate"], C.$inject = ["$compile", "$controller",
          "$state", "$interpolate"
        ], t.module("ui.router.state").directive("uiView", T), t.module("ui.router.state").directive("uiView", C), M
        .$inject = ["$state", "$timeout"], k.$inject = ["$state", "$stateParams", "$interpolate"], t.module(
          "ui.router.state").directive("uiSref", M).directive("uiSrefActive", k).directive("uiSrefActiveEq", k), N
        .$inject = ["$state"], I.$inject = ["$state"], t.module("ui.router.state").filter("isState", N).filter(
          "includedByState", I)
    }(window, window.angular)
}
