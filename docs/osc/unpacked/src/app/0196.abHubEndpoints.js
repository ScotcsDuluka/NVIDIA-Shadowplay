// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 196
// provider abHubEndpoints | defines angular.module("main.localSdk.abHubSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.abHubSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("abHubEndpoints", [function() {
    var e = void 0;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i = void 0,
          o = void 0,
          r = void 0,
          a = void 0,
          l = n.newLocalEndpointFactory("abHubAPI", e);
        return i = l.createEndpoint({
          url: "/Post",
          method: "POST",
          data: {
            messageType: "GET",
            userId: "",
            clientName: "",
            clientVer: "",
            experiments: []
          }
        }), o = l.createEndpoint({
          url: "/Add",
          method: "POST",
          data: {
            messageType: "ADD",
            userId: "",
            clientName: "",
            clientVer: "",
            experiments: []
          }
        }), r = l.createEndpoint({
          url: "/Delete",
          method: "POST",
          data: {
            messageType: "DELETE",
            userId: "",
            clientName: "",
            clientVer: "",
            experiments: []
          }
        }), a = l.createEndpoint({
          url: "/Status",
          method: "GET"
        }), {
          getFullAbHubUrl: l.generateFullUrl,
          post: i,
          add: o,
          remove: r,
          status: a
        };
      }]
    };
  }]), exports.ngAbHubSdkModule = n;
}
