// ─────────────────────────────────────────────────────────────
// APP MODULE 206
// role       : provider piplConfigEndpoints
// defines    : angular.module("main.piplConfigSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.piplConfigSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("piplConfigEndpoints", [function() {
    var e = void 0,
      t = void 0,
      n = void 0;
    return {
      setConfig: function(i) {
        e = i.server, t = i.version, n = i.defaultTimeout
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
        }
      }]
    }
  }]), t.ngPiplConfigSdkModule = n
}
