// ─────────────────────────────────────────────────────────────
// APP MODULE 204
// role       : provider nisEndpoints
// defines    : angular.module("main.localSdk.nisSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.nisSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("nisEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o, r = n.newLocalEndpointFactory("Nis2", e);
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
        }
      }]
    }
  }]), t.ngNisSdkModule = n
}
