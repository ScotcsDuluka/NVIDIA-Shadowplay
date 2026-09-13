// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 204
// provider nisEndpoints | defines angular.module("main.localSdk.nisSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.nisSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("nisEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o,
          r = n.newLocalEndpointFactory("Nis2", e);
        return i = r.createEndpoint({
          url: "/:cmsId/state",
          method: "GET",
          data: {
            cmsId: ""
          }
        }), o = r.createEndpoint({
          url: "/state",
          method: "POST",
          data: {
            enabled: "",
            sharpen: "",
            selectedResolutionIndex: "",
            cmsId: ""
          }
        }), {
          getFullNisUrl: r.generateFullUrl,
          getState: i,
          setState: o
        };
      }]
    };
  }]), exports.ngNisSdkModule = n;
}
