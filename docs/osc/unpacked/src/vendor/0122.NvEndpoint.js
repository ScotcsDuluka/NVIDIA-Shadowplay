// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 122
// factory NvEndpoint | factory NvEndpointFactory | factory nvHttpStatusInterceptor | constant NV_STATUS | defines angular.module("nvAngularHttpEndpoint")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  var i = require(25),
    o = r(i),
    a = require(26),
    s = r(a);
  angular.module("nvAngularHttpEndpoint", []), angular.module("nvAngularHttpEndpoint").factory("NvEndpoint", [
    "$http", "$timeout", "$q", "NV_STATUS",
    function(e, t, n, r) {
      function i(e, t, n) {
        var r, i;
        for (r in t) i = t[r], n && (i = encodeURIComponent(i)), e.indexOf(":" + r) > -1 && (e = e
          .replace(new RegExp(":" + r, "g"), i), delete t[r]);
        return e;
      }

      function a(e, t, n) {
        var r,
          o = n(e, t),
          a = "";
        o = i(o, t, !0);
        for (r in t) a = a + "&" + encodeURIComponent(r) + "=" + encodeURIComponent(t[r]), delete t[r];
        return a.length > 0 && (o = o + "?" + a.slice(1, a.length)), o;
      }

      function c(e, t) {
        var n,
          r = {};
        r = angular.merge({}, e.params, t);
        for (n in r) "object" === (0, s.default)(r[n]) && (r[n] = (0, o.default)(r[n]));
        return r;
      }

      function u(e, t) {
        var n = e.data;
        return t && "object" !== ("undefined" == typeof t ? "undefined" : (0, s.default)(t)) ? n = t :
          t && (n = angular.merge({}, e.data, t)), n;
      }

      function l(e, t, n) {
        var r, o;
        o = n(e, t);
        for (r in o) o[r] = i(o[r], t);
        return o;
      }

      function d(e, t) {
        return e.timeout || t;
      }

      function f(e, t) {
        return null === e.retries || void 0 === e.retries ? t : e.retries;
      }

      function h(e, t) {
        return null === e.timeBetweenRetries || void 0 === e.timeBetweenRetries ? t : e
        .timeBetweenRetries;
      }

      function p(r, i) {
        function o(e, t) {
          return e = angular.isArray(e) ? e : [e], e.concat(t);
        }

        function s(e, t) {
          var n = c(b, e),
            r = i.endpointConfigFunc(b, n);
          return r.method = b.method, r.data = u(b, t), r.headers = l(b, n, i.headerGenerator), r.url = a(
            b, n, i.urlGenerator), r.params = n, r;
        }

        function p(e, t) {
          e && e.timeoutDeferred && e.timeoutDeferred.resolve && e.timeoutDeferred.resolve(t);
        }

        function m(e) {
          p(e, !1);
        }

        function v(r) {
          var a,
            s = !1,
            c = !1,
            u = d(b, i.defaultTimeout),
            l = h(b, i.defaultTimeBetweenRetries),
            f = n.defer();
          r.timeout = f.promise.then(function(e) {
            e ? c = !0 : s = !0;
          }), r.transformResponse = o(e.defaults.transformResponse, function(e) {
            return c ? "TIMED_OUT" : s ? "CANCELLED" : e;
          });
          var a = i.httpFunc(r);
          return a.timeoutDeferred = f, a.catch(function(e) {
            return !s && r.retriesRemaining > 0 && (e.status === -1 || e.status >= 500 && e.status <
              600) ? (r.retriesRemaining = r.retriesRemaining - 1, t(function() {
              return v(r);
            }, l)) : (c && (e.timeout = !0), s && (e.cancelled = !0), n.reject(e));
          }), u && t(function() {
            p(a, !0);
          }, u), a;
        }

        function g(e, t) {
          var n;
          return n = s(e, t), n.retriesRemaining = f(b, i.defaultRetries), v(n);
        }

        function y(e) {
          var t = s(e);
          return t.url;
        }
        var b = r;
        if (b.method = b.method.toUpperCase(), b.params = b.params || {}, ["PUT", "POST", "GET", "DELETE",
            "HEAD", "JSONP", "PATCH"
          ].indexOf(b.method) < 0) throw new Error("Invalid method");
        return g.applyInput = s, g.config = r, g.getQueryString = y, g.cancel = m, g;
      }
      return p;
    }
  ]), angular.module("nvAngularHttpEndpoint").factory("NvEndpointFactory", ["$http", "NvEndpoint", function(
    e, t) {
    function n() {
      var n = {};
      n.urlGenerator = function(e, t) {
          return e.url;
        }, n.headerGenerator = function(e, t) {
          return angular.merge({}, e.headers || {});
        }, n.httpFunc = function(t) {
          return e(t);
        }, n.endpointConfigFunc = function(e, t) {
          return {};
        }, n.defaultTimeout = null, n.defaultRetries = 0, n.defaultTimeBetweenRetries = 0, this
        .setUrlGenerator = function(e) {
          n.urlGenerator = e;
        }, this.setHeaderGenerator = function(e) {
          n.headerGenerator = e;
        }, this.setHttpFunc = function(e) {
          n.httpFunc = e;
        }, this.setEndpointConfigFunc = function(e) {
          n.endpointConfigFunc = e;
        }, this.setDefaultTimeout = function(e) {
          n.defaultTimeout = e;
        }, this.setDefaultRetries = function(e) {
          n.defaultRetries = e;
        }, this.setDefaultTimeBetweenRetries = function(e) {
          n.defaultTimeBetweenRetries = e;
        }, this.createEndpoint = function(e) {
          return new t(e, n);
        }, this.generateFullUrl = function(e) {
          return n.urlGenerator({
            url: e
          });
        };
    }
    return n;
  }]), angular.module("nvAngularHttpEndpoint").constant("NV_STATUS", {
    TIMED_OUT: "-100",
    CANCELLED: "-101"
  }), angular.module("nvAngularHttpEndpoint").config(["$httpProvider", function(e) {
    e.interceptors.push("nvHttpStatusInterceptor");
  }]), angular.module("nvAngularHttpEndpoint").factory("nvHttpStatusInterceptor", ["$q", "NV_STATUS",
    function(e, t) {
      var n = {
        responseError: function(n) {
          return angular.forEach(t, function(e, t) {
            n.data === t && (n.status = e, n.data = null);
          }), e.reject(n);
        }
      };
      return n;
    }
  ]);
}
