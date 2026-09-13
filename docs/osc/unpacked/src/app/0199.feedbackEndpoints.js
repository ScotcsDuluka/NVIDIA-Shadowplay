// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 199
// provider feedbackEndpoints | defines angular.module("main.localSdk.feedbackSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.feedbackSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("feedbackEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i,
          o = n.newLocalEndpointFactory("Feedback", e);
        return i = o.createEndpoint({
          url: "",
          method: "POST",
          data: {
            category: "",
            message: "",
            email: "",
            relatedApplications: "",
            relatedFiles: ""
          }
        }), {
          getFeedbackUrl: o.generateFullUrl,
          postFeedback: i
        };
      }]
    };
  }]), exports.ngFeedbackSdkModule = n;
}
