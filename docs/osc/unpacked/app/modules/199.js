// ─────────────────────────────────────────────────────────────
// APP MODULE 199
// role       : provider feedbackEndpoints
// defines    : angular.module("main.localSdk.feedbackSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.feedbackSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("feedbackEndpoints", [function() {
    var e;
    return {
      setConfig: function(t) {
        e = t.version
      },
      $get: ["NvEndpointFactory", "localSdk", function(t, n) {
        var i, o = n.newLocalEndpointFactory("Feedback", e);
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
        }
      }]
    }
  }]), t.ngFeedbackSdkModule = n
}
