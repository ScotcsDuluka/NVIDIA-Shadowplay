// ─────────────────────────────────────────────────────────────
// APP MODULE 17
// role       : provider localSdk
// defines    : angular.module("main.localSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.ngLocalSdkModule = void 0;
  var i = n(208);
  n(11);
  var o = n(202),
    r = n(197),
    a = n(207),
    l = n(205),
    s = n(199),
    d = n(200),
    c = n(203),
    u = n(201),
    f = n(196),
    m = n(99),
    g = n(204),
    p = n(198),
    h = n(206),
    b = angular.module("main.localSdk", [i.ngShadowplaySdkModule.name, o.ngHardwareSdkModule.name, r.ngCoplaySdkModule
      .name, a.ngSettingsSdkModule.name, l.ngNvCameraSdkModule.name, s.ngFeedbackSdkModule.name, d
      .ngGfeUpdatesSdkModule.name, c.ngHighlightsSdkModule.name, u.ngGfwslSdkModule.name, f.ngAbHubSdkModule.name, m
      .ngQuietMode2SdkModule.name, g.ngNisSdkModule.name, p.ngDvcSdkModule.name, h.ngPiplConfigSdkModule.name
    ]);
  b.provider("localSdk", [function() {
    var e;
    return {
      setNodeConfig: function(t) {
        e = t
      },
      $get: ["$log", "NvEndpointFactory", function(t, n) {
        function i(t) {
          e.port = t.port, e.commonHeaders.X_LOCAL_SECURITY_COOKIE = t.secret
        }

        function o() {
          return e
        }

        function r(t, i) {
          var o = new n;
          return o.setUrlGenerator(function(n, o) {
            return e.server + e.port + "/" + t + "/" + i + n.url
          }), o.setHeaderGenerator(function(t, n) {
            var i = angular.merge({}, t.headers, e.commonHeaders);
            return i
          }), o
        }
        return {
          updateNodeInfo: i,
          newLocalEndpointFactory: r,
          getNodeConfig: o
        }
      }]
    }
  }]), t.ngLocalSdkModule = b
}
