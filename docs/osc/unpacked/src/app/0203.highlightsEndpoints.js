// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 203
// provider highlightsEndpoints | defines angular.module("main.localSdk.highlightsSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.highlightsSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("highlightsEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o,
          r,
          a,
          l,
          s,
          d,
          c,
          u,
          f,
          m,
          g,
          p = n.newLocalEndpointFactory("SDK", e);
        return i = p.createEndpoint({
          url: "/Highlights/Active",
          method: "GET"
        }), o = p.createEndpoint({
          url: "/Highlights/RecoverSpace",
          method: "POST"
        }), r = p.createEndpoint({
          url: "/Highlights/GetConfig",
          method: "POST",
          data: {
            shortName: ""
          }
        }), a = p.createEndpoint({
          url: "/Highlights/SetConfig",
          method: "POST",
          data: {
            enabled: ""
          }
        }), l = p.createEndpoint({
          url: "/GetPermissions",
          method: "POST",
          data: {
            shortName: ""
          }
        }), s = p.createEndpoint({
          url: "/SetPermissions",
          method: "POST",
          data: {
            shortName: "",
            permissions: ""
          }
        }), d = p.createEndpoint({
          url: "/Highlights/Enable",
          method: "GET"
        }), c = p.createEndpoint({
          url: "/Highlights/Enable",
          method: "POST",
          data: {
            enabled: ""
          }
        }), u = p.createEndpoint({
          url: "/NotifyOverlayState",
          method: "POST",
          data: {
            open: "",
            state: ""
          }
        }), f = p.createEndpoint({
          url: "/Highlights/GetRecent",
          method: "POST",
          data: {
            game: "",
            maxItems: ""
          }
        }), m = p.createEndpoint({
          url: "/Highlights/GetGamesConfig",
          method: "GET"
        }), g = p.createEndpoint({
          url: "/Highlights/GetHighlights",
          method: "POST",
          data: {
            gameName: ""
          }
        }), {
          getFullShadowPlayUrl: p.generateFullUrl,
          getActive: i,
          recoverSpace: o,
          getConfig: r,
          setConfig: a,
          getPermissions: l,
          setPermissions: s,
          getHighlightsEnabled: d,
          setHighlightsEnabled: c,
          notifyOverlayState: u,
          getRecent: f,
          getGamesConfig: m,
          getGameHighlights: g
        };
      }]
    };
  }]), exports.ngHighlightsSdkModule = n;
}
