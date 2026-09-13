// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 139
// provider $resource | defines angular.module("ngResource")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  /**
   * @license AngularJS v1.5.5
   * (c) 2010-2016 Google, Inc. http://angularjs.org
   * License: MIT
   */
  ! function(e, t) {
    "use strict";

    function n(e) {
      return null != e && "" !== e && "hasOwnProperty" !== e && a.test("." + e);
    }

    function r(e, r) {
      if (!n(r)) throw o("badmember", 'Dotted member path "@{0}" is invalid.', r);
      for (var i = r.split("."), a = 0, s = i.length; a < s && t.isDefined(e); a++) {
        var c = i[a];
        e = null !== e ? e[c] : void 0;
      }
      return e;
    }

    function i(e, n) {
      n = n || {}, t.forEach(n, function(e, t) {
        delete n[t];
      });
      for (var r in e) !e.hasOwnProperty(r) || "$" === r.charAt(0) && "$" === r.charAt(1) || (n[r] = e[r]);
      return n;
    }
    var o = t.$$minErr("$resource"),
      a = /^(\.[a-zA-Z_$@][0-9a-zA-Z_$@]*)+$/;
    t.module("ngResource", ["ng"]).provider("$resource", function() {
      var e = /^https?:\/\/[^\/]*/,
        n = this;
      this.defaults = {
        stripTrailingSlashes: !0,
        actions: {
          get: {
            method: "GET"
          },
          save: {
            method: "POST"
          },
          query: {
            method: "GET",
            isArray: !0
          },
          remove: {
            method: "DELETE"
          },
          delete: {
            method: "DELETE"
          }
        }
      }, this.$get = ["$http", "$log", "$q", "$timeout", function(a, s, c, u) {
        function l(e) {
          return d(e, !0).replace(/%26/gi, "&").replace(/%3D/gi, "=").replace(/%2B/gi, "+");
        }

        function d(e, t) {
          return encodeURIComponent(e).replace(/%40/gi, "@").replace(/%3A/gi, ":").replace(/%24/g,
            "$").replace(/%2C/gi, ",").replace(/%20/g, t ? "%20" : "+");
        }

        function f(e, t) {
          this.template = e, this.defaults = v({}, n.defaults, t), this.urlParams = {};
        }

        function h(e, l, d, b) {
          function E(e, t) {
            var n = {};
            return t = v({}, l, t), m(t, function(t, i) {
              y(t) && (t = t()), n[i] = t && t.charAt && "@" == t.charAt(0) ? r(e, t.substr(1)) :
                t;
            }), n;
          }

          function _(e) {
            return e.resource;
          }

          function $(e) {
            i(e || {}, this);
          }
          var w = new f(e, b);
          return d = v({}, n.defaults.actions, d), $.prototype.toJSON = function() {
            var e = v({}, this);
            return delete e.$promise, delete e.$resolved, e;
          }, m(d, function(e, r) {
            var l = /^(POST|PUT|PATCH)$/i.test(e.method),
              d = e.timeout,
              f = t.isDefined(e.cancellable) ? e.cancellable : b && t.isDefined(b.cancellable) ? b
              .cancellable : n.defaults.cancellable;
            d && !t.isNumber(d) && (s.debug(
              "ngResource:\n  Only numeric values are allowed as `timeout`.\n  Promises are not supported in $resource, because the same value would be used for multiple requests. If you are looking for a way to cancel requests, you should use the `cancellable` option."
              ), delete e.timeout, d = null), $[r] = function(n, s, h, b) {
              var T,
                C,
                x,
                S = {};
              switch (arguments.length) {
                case 4:
                  x = b, C = h;
                case 3:
                case 2:
                  if (!y(s)) {
                    S = n, T = s, C = h;
                    break;
                  }
                  if (y(n)) {
                    C = n, x = s;
                    break;
                  }
                  C = s, x = h;
                case 1:
                  y(n) ? C = n : l ? T = n : S = n;
                  break;
                case 0:
                  break;
                default:
                  throw o("badargs",
                    "Expected up to 4 arguments [params, data, success, error], got {0} arguments",
                    arguments.length);
              }
              var A,
                M,
                k = this instanceof $,
                N = k ? T : e.isArray ? [] : new $(T),
                I = {},
                O = e.interceptor && e.interceptor.response || _,
                D = e.interceptor && e.interceptor.responseError || void 0;
              m(e, function(e, t) {
                switch (t) {
                  default:
                    I[t] = g(e);
                    break;
                  case "params":
                  case "isArray":
                  case "interceptor":
                  case "cancellable":
                }
              }), !k && f && (A = c.defer(), I.timeout = A.promise, d && (M = u(A.resolve,
                d))), l && (I.data = T), w.setUrlParams(I, v({}, E(T, e.params || {}), S), e
                .url);
              var R = a(I).then(function(n) {
                var a = n.data;
                if (a) {
                  if (t.isArray(a) !== !!e.isArray) throw o("badcfg",
                    "Error in resource configuration for action `{0}`. Expected response to contain an {1} but got an {2} (Request: {3} {4})",
                    r, e.isArray ? "array" : "object", t.isArray(a) ? "array" :
                    "object", I.method, I.url);
                  if (e.isArray) N.length = 0, m(a, function(e) {
                    "object" == typeof e ? N.push(new $(e)) : N.push(e);
                  });
                  else {
                    var s = N.$promise;
                    i(a, N), N.$promise = s;
                  }
                }
                return n.resource = N, n;
              }, function(e) {
                return (x || p)(e), c.reject(e);
              });
              return R.finally(function() {
                N.$resolved = !0, !k && f && (N.$cancelRequest = t.noop, u.cancel(M), A =
                  M = I.timeout = null);
              }), R = R.then(function(e) {
                var t = O(e);
                return (C || p)(t, e.headers), t;
              }, D), k ? R : (N.$promise = R, N.$resolved = !1, f && (N.$cancelRequest = A
                .resolve), N);
            }, $.prototype["$" + r] = function(e, t, n) {
              y(e) && (n = t, t = e, e = {});
              var i = $[r].call(this, e, this, t, n);
              return i.$promise || i;
            };
          }), $.bind = function(t) {
            return h(e, v({}, l, t), d);
          }, $;
        }
        var p = t.noop,
          m = t.forEach,
          v = t.extend,
          g = t.copy,
          y = t.isFunction;
        return f.prototype = {
          setUrlParams: function(n, r, i) {
            var a,
              s,
              c = this,
              u = i || c.template,
              f = "",
              h = c.urlParams = {};
            m(u.split(/\W/), function(e) {
                if ("hasOwnProperty" === e) throw o("badname",
                  "hasOwnProperty is not a valid parameter name.");
                !new RegExp("^\\d+$").test(e) && e && new RegExp("(^|[^\\\\]):" + e + "(\\W|$)")
                  .test(u) && (h[e] = {
                    isQueryParamValue: new RegExp("\\?.*=:" + e + "(?:\\W|$)").test(u)
                  });
              }), u = u.replace(/\\:/g, ":"), u = u.replace(e, function(e) {
                return f = e, "";
              }), r = r || {}, m(c.urlParams, function(e, n) {
                a = r.hasOwnProperty(n) ? r[n] : c.defaults[n], t.isDefined(a) && null !== a ? (
                  s = e.isQueryParamValue ? d(a, !0) : l(a), u = u.replace(new RegExp(":" +
                    n + "(\\W|$)", "g"), function(e, t) {
                    return s + t;
                  })) : u = u.replace(new RegExp("(/?):" + n + "(\\W|$)", "g"), function(e, t,
                  n) {
                  return "/" == n.charAt(0) ? n : t + n;
                });
              }), c.defaults.stripTrailingSlashes && (u = u.replace(/\/+$/, "") || "/"), u = u
              .replace(/\/\.(?=\w+($|\?))/, "."), n.url = f + u.replace(/\/\\\./, "/."), m(r,
                function(e, t) {
                  c.urlParams[t] || (n.params = n.params || {}, n.params[t] = e);
                });
          }
        }, h;
      }];
    });
  }(window, window.angular);
}
