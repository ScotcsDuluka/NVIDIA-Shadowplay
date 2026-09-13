// ─────────────────────────────────────────────────────────────
// APP MODULE 200
// role       : provider gfeUpdatesEndpoints
// defines    : angular.module("main.localSdk.gfeUpdatesSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.gfeUpdatesSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("gfeUpdatesEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(e, t) {
        var n, i = new e;
        return i.setUrlGenerator(function(e, n) {
          var i = t.getNodeConfig();
          return i.server + i.port + "/gfeupdate/" + e.url
        }), i.setHeaderGenerator(function(e, n) {
          var i = t.getNodeConfig(),
            o = angular.merge({}, e.headers, i.commonHeaders);
          return o
        }), n = i.createEndpoint({
          url: "autoGFEDownload/autoGFEbeta",
          method: "GET"
        }), {
          getFullGfeUpdatesUrl: i.generateFullUrl,
          getBetaSetting: n
        }
      }]
    }
  }]), t.ngGfeUpdatesSdkModule = n
}
