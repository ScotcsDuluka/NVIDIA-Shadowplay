// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 206
// provider piplConfigEndpoints | defines angular.module("main.piplConfigSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.piplConfigSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("piplConfigEndpoints", [function() {
    var e = void 0,
      t = void 0,
      n = void 0;
    return {
      setConfig: function(i) {
        e = i.server, t = i.version, n = i.defaultTimeout;
      },
      $get: ["localSdk", function(i) {
        var o = i.newLocalEndpointFactory(e, t);
        o.setDefaultTimeout(n);
        var r = o.createEndpoint({
          url: "/data",
          method: "GET"
        });
        return {
          getFullUrl: o.generateFullUrl,
          getPiplConfig: r
        };
      }]
    };
  }]), exports.ngPiplConfigSdkModule = n;
}
