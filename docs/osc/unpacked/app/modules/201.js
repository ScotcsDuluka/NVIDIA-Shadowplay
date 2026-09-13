// ─────────────────────────────────────────────────────────────
// APP MODULE 201
// role       : provider gfwslEndpoints
// defines    : angular.module("main.localSdk.gfwslSdk")
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.ngGfwslSdkModule = void 0;
  var o = n(8),
    r = i(o),
    a = angular.module("main.localSdk.gfwslSdk", ["nvAngularHttpEndpoint", "main.common"]);
  a.provider("gfwslEndpoints", [function() {
    var e, t;
    return {
      setConfig: function(n) {
        e = n.server, t = n.defaultTimeout
      },
      $get: ["NvEndpointFactory", function(n) {
        var i = new n,
          o =
          "nvidia_web_services/controller.gfeclientcontent.NG.php/com.nvidia.services.GFEClientContent_NG.";
        i.setUrlGenerator(function(t, n) {
          var i;
          return i = e + o + t.methodName + "/", i += (0, r.default)(t.gfwslParams)
        }), i.setDefaultTimeout(t);
        var a = function(e, t) {
            return i.createEndpoint({
              methodName: e,
              method: "GET",
              gfwslParams: t
            })
          },
          l = function(t) {
            e = t
          };
        return {
          getFullUrl: i.generateFullUrl,
          get: a,
          setServer: l
        }
      }]
    }
  }]), t.ngGfwslSdkModule = a
}
