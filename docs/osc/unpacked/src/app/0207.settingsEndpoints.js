// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 207
// provider settingsEndpoints | defines angular.module("main.localSdk.settingsSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.settingsSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("settingsEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o,
          r,
          a = n.newLocalEndpointFactory("Settings", e),
          l = new t();
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
          return i.server + i.port + e.url;
        }), l.setHeaderGenerator(function(e, t) {
          var i = n.getNodeConfig(),
            o = angular.merge({}, e.headers, i.commonHeaders);
          return o;
        }), r = l.createEndpoint({
          url: "/beta",
          method: "GET"
        }), {
          getFullSettingsUrl: a.generateFullUrl,
          getLanguage: i,
          setLanguage: o,
          getBetaPackage: r
        };
      }]
    };
  }]), exports.ngSettingsSdkModule = n;
}
