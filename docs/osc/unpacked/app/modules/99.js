// ─────────────────────────────────────────────────────────────
// APP MODULE 99
// role       : provider quietMode2Endpoints
// defines    : angular.module("main.localSdk.quietMode2Sdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.quietMode2Sdk", ["nvAngularHttpEndpoint"]);
  n.provider("quietMode2Endpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o, r, a = n.newLocalEndpointFactory("QuietMode2", e);
        return i = a.createEndpoint({
          url: "/support",
          method: "GET"
        }), o = a.createEndpoint({
          url: "/state",
          method: "GET"
        }), r = a.createEndpoint({
          url: "/state",
          method: "POST",
          data: {
            enabled: "",
            baseFrameRate: "",
            fanVolume: ""
          }
        }), {
          getFullQuietModeUrl: a.generateFullUrl,
          support: i,
          state: o,
          setState: r
        }
      }]
    }
  }]), t.ngQuietMode2SdkModule = n
}
