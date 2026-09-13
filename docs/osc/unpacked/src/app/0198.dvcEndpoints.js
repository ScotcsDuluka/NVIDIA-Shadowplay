// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 198
// provider dvcEndpoints | defines angular.module("main.localSdk.dvcSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.dvcSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("dvcEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o,
          r = n.newLocalEndpointFactory("DeepDVC", e);
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
            cmsId: "",
            enabled: "",
            supported: "",
            vibrance: "",
            saveToDRS: ""
          }
        }), {
          getFullDvcUrl: r.generateFullUrl,
          getState: i,
          setState: o
        };
      }]
    };
  }]), exports.ngDvcSdkModule = n;
}
