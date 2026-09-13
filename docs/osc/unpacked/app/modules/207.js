// ─────────────────────────────────────────────────────────────
// APP MODULE 207
// role       : provider settingsEndpoints
// defines    : angular.module("main.localSdk.settingsSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.settingsSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("settingsEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o, r, a = n.newLocalEndpointFactory("Settings", e),
          l = new t;
        return i = a.createEndpoint({
          url: "/Language",
          method: "GET"
        }), o = a.createEndpoint({
          url: "/Language",
          method: "POST",
          data: {
            language: ""
          }
        }), l.setUrlGenerator(function(e, t) {
          var i = n.getNodeConfig();
          return i.server + i.port + e.url
        }), l.setHeaderGenerator(function(e, t) {
          var i = n.getNodeConfig(),
            o = angular.merge({}, e.headers, i.commonHeaders);
          return o
        }), r = l.createEndpoint({
          url: "/beta",
          method: "GET"
        }), {
          getFullSettingsUrl: a.generateFullUrl,
          getLanguage: i,
          setLanguage: o,
          getBetaPackage: r
        }
      }]
    }
  }]), t.ngSettingsSdkModule = n
}
