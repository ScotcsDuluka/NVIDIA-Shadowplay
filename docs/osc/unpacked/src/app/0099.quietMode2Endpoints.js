// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 99
// provider quietMode2Endpoints | defines angular.module("main.localSdk.quietMode2Sdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.quietMode2Sdk", ["nvAngularHttpEndpoint"]);
  n.provider("quietMode2Endpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o,
          r,
          a = n.newLocalEndpointFactory("QuietMode2", e);
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
        };
      }]
    };
  }]), exports.ngQuietMode2SdkModule = n;
}
