// ─────────────────────────────────────────────────────────────
// APP MODULE 202
// role       : provider hardwareEndpoints
// defines    : angular.module("main.localSdk.hardwareSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.hardwareSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("hardwareEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o = n.newLocalEndpointFactory("HardwareInformation", e);
        return i = o.createEndpoint({
          url: "",
          method: "GET"
        }), {
          getFullHardwareUrl: o.generateFullUrl,
          get: i
        }
      }]
    }
  }]), t.ngHardwareSdkModule = n
}
