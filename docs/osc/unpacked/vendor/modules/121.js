// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 121
// role       : service nvAccountService | provider nvAccountEndpoints
// defines    : angular.module("nvAngularAccountSdk")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  angular.module("nvAngularAccountSdk", ["nvAngularHttpEndpoint"]), angular.module("nvAngularAccountSdk").service(
    "nvAccountService", ["nvAccountEndpoints", function(e) {
      var t = this;
      t.storeJarvisUserToken = function(n) {
        return e.setUserToken({}, {
          userToken: n.userToken,
          userInfo: n.userInfo
        }).then(function() {
          if (n.userInfo && n.userInfo.dataTracking) return t.setUserTelemetryConsent(n.userInfo.userId, n
            .userInfo.dataTracking)
        })
      }, t.deleteJarvisUserToken = function() {
        return e.setUserToken({}, {
          userToken: "",
          userInfo: "{}"
        })
      }, t.getJarvisUserToken = function() {
        return e.getUserToken().then(function(e) {
          return e ? e.data : $q.reject("NvAccount UserToken endpoint returned empty response!")
        })
      }, t.setUserTelemetryConsent = function(t, n) {
        return e.setUserTelemetryConsent({}, {
          userId: t,
          consentSettings: n
        })
      }, t.getUserTelemetryConsent = function(t) {
        return e.getUserTelemetryConsent({
          userId: t
        }).then(function(e) {
          return e ? e.data : $q.reject("NvAccount User PrivacySettings endpoint returned empty response!")
        })
      }, t.setClientTelemetryConsent = function(t, n) {
        return e.setClientTelemetryConsent({}, {
          clientId: t,
          consentSettings: n
        })
      }, t.setDefaultTelemetryConsent = function(e, n) {
        return t.setClientTelemetryConsent(n || "0", e)
      }, t.getClientTelemetryConsent = function(t) {
        return e.getClientTelemetryConsent({
          clientId: t
        }).then(function(e) {
          return e ? e.data : $q.reject("NvAccount Client PrivacySettings endpoint returned empty response!")
        })
      }
    }]), angular.module("nvAngularAccountSdk").provider("nvAccountEndpoints", [function() {
    var e, t, n = "Account";
    return {
      setConfig: function(n) {
        t = n.version, e = n
      },
      $get: ["NvEndpointFactory", function(r) {
        function i(t) {
          e.port = t.port, e.commonHeaders.X_LOCAL_SECURITY_COOKIE = t.secret, e.active = t.active
        }
        var o, a, s, c, u, l, d = new r;
        return d.setUrlGenerator(function(r, i) {
          return e.server + e.port + "/" + n + "/" + t + r.url
        }), d.setHeaderGenerator(function(t, n) {
          var r = angular.merge({}, t.headers, e.commonHeaders);
          return r
        }), o = d.createEndpoint({
          url: "/UserToken",
          method: "GET"
        }), a = d.createEndpoint({
          url: "/UserToken",
          method: "POST",
          data: {
            userToken: "",
            userInfo: ""
          }
        }), s = d.createEndpoint({
          url: "/PrivacySettings",
          method: "POST",
          data: {
            userId: "",
            consentSettings: ""
          }
        }), c = d.createEndpoint({
          url: "/PrivacySettings",
          method: "GET",
          params: {
            userId: ""
          }
        }), u = d.createEndpoint({
          url: "/PrivacySettings",
          method: "POST",
          data: {
            clientId: "",
            consentSettings: ""
          }
        }), l = d.createEndpoint({
          url: "/PrivacySettings",
          method: "GET",
          params: {
            clientId: ""
          }
        }), {
          updateNodeInfo: i,
          getFullAccountUrl: d.generateFullUrl,
          getUserToken: o,
          setUserToken: a,
          setUserTelemetryConsent: s,
          getUserTelemetryConsent: c,
          setClientTelemetryConsent: u,
          getClientTelemetryConsent: l
        }
      }]
    }
  }])
}
