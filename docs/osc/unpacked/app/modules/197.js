// ─────────────────────────────────────────────────────────────
// APP MODULE 197
// role       : provider coplayEndpoints
// defines    : angular.module("main.localSdk.coplaySdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.coplaySdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("coplayEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o, r, a, l, s = n.newLocalEndpointFactory("GameShare", e);
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
        }
      }]
    }
  }]), t.ngCoplaySdkModule = n
}
