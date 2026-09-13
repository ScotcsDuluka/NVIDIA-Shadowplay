// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 200
// provider gfeUpdatesEndpoints | defines angular.module("main.localSdk.gfeUpdatesSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.gfeUpdatesSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("gfeUpdatesEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(e, t) {
        var n,
          i = new e();
        return i.setUrlGenerator(function(e, n) {
          var i = t.getNodeConfig();
          return i.server + i.port + "/gfeupdate/" + e.url;
        }), i.setHeaderGenerator(function(e, n) {
          var i = t.getNodeConfig(),
            o = angular.merge({}, e.headers, i.commonHeaders);
          return o;
        }), n = i.createEndpoint({
          url: "autoGFEDownload/autoGFEbeta",
          method: "GET"
        }), {
          getFullGfeUpdatesUrl: i.generateFullUrl,
          getBetaSetting: n
        };
      }]
    };
  }]), exports.ngGfeUpdatesSdkModule = n;
}
