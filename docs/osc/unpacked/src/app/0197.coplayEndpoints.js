// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 197
// provider coplayEndpoints | defines angular.module("main.localSdk.coplaySdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.coplaySdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("coplayEndpoints", [function() {
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
          s = n.newLocalEndpointFactory("GameShare", e);
        return i = s.createEndpoint({
          url: "/CreateSession",
          method: "POST",
          data: {
            activeWindow: "",
            displayName: "",
            inviteMode: "",
            emailId: ""
          }
        }), o = s.createEndpoint({
          url: "/ConfigureControllerMapping",
          method: "PUT",
          data: {
            mode: ""
          }
        }), r = s.createEndpoint({
          url: "/ModifySession/:id",
          method: "PUT",
          params: {
            id: ""
          },
          data: {
            action: ""
          }
        }), a = s.createEndpoint({
          url: "/Session/:id",
          method: "DELETE",
          params: {
            id: ""
          }
        }), l = s.createEndpoint({
          url: "/FullScreenProcessId/:activeWindow",
          method: "GET",
          timeout: 500,
          params: {
            activeWindow: ""
          }
        }), {
          createSession: i,
          configureControllerMapping: o,
          modifySession: r,
          deleteSession: a,
          getFullscreenPid: l
        };
      }]
    };
  }]), exports.ngCoplaySdkModule = n;
}
