// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 202
// provider hardwareEndpoints | defines angular.module("main.localSdk.hardwareSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.hardwareSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("hardwareEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o = n.newLocalEndpointFactory("HardwareInformation", e);
        return i = o.createEndpoint({
          url: "",
          method: "GET"
        }), {
          getFullHardwareUrl: o.generateFullUrl,
          get: i
        };
      }]
    };
  }]), exports.ngHardwareSdkModule = n;
}
