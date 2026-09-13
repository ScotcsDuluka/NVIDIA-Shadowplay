// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 201
// provider gfwslEndpoints | defines angular.module("main.localSdk.gfwslSdk")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.ngGfwslSdkModule = void 0;
  var o = require(8),
    r = i(o),
    a = angular.module("main.localSdk.gfwslSdk", ["nvAngularHttpEndpoint", "main.common"]);
  a.provider("gfwslEndpoints", [function() {
    var e, t;
    return {
      setConfig: function(n) {
        e = n.server, t = n.defaultTimeout;
      },
      $get: ["NvEndpointFactory", function(n) {
        var i = new n(),
          o =
          "nvidia_web_services/controller.gfeclientcontent.NG.php/com.nvidia.services.GFEClientContent_NG.";
        i.setUrlGenerator(function(t, n) {
          var i;
          return i = e + o + t.methodName + "/", i += (0, r.default)(t.gfwslParams);
        }), i.setDefaultTimeout(t);
        var a = function(e, t) {
            return i.createEndpoint({
              methodName: e,
              method: "GET",
              gfwslParams: t
            });
          },
          l = function(t) {
            e = t;
          };
        return {
          getFullUrl: i.generateFullUrl,
          get: a,
          setServer: l
        };
      }]
    };
  }]), exports.ngGfwslSdkModule = a;
}
