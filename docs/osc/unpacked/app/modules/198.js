// ─────────────────────────────────────────────────────────────
// APP MODULE 198
// role       : provider dvcEndpoints
// defines    : angular.module("main.localSdk.dvcSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.dvcSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("dvcEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o, r = n.newLocalEndpointFactory("DeepDVC", e);
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
        }
      }]
    }
  }]), t.ngDvcSdkModule = n
}
