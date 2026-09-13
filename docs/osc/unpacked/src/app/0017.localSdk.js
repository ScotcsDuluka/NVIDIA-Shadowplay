// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 17
// provider localSdk | defines angular.module("main.localSdk")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.ngLocalSdkModule = void 0;
  var i = require(208) /* app/208 — shadowPlayEndpoints (provider) */;
  require(11);
  var o = require(202) /* app/202 — hardwareEndpoints (provider) */,
    r = require(197) /* app/197 — coplayEndpoints (provider) */,
    a = require(207) /* app/207 — settingsEndpoints (provider) */,
    l = require(205) /* app/205 — nvCameraEndpoints (provider) */,
    s = require(199) /* app/199 — feedbackEndpoints (provider) */,
    d = require(200) /* app/200 — gfeUpdatesEndpoints (provider) */,
    c = require(203) /* app/203 — highlightsEndpoints (provider) */,
    u = require(201) /* app/201 — gfwslEndpoints (provider) */,
    f = require(196) /* app/196 — abHubEndpoints (provider) */,
    m = require(99) /* app/99 — quietMode2Endpoints (provider) */,
    g = require(204) /* app/204 — nisEndpoints (provider) */,
    p = require(198) /* app/198 — dvcEndpoints (provider) */,
    h = require(206) /* app/206 — piplConfigEndpoints (provider) */,
    b = angular.module("main.localSdk", [i.ngShadowplaySdkModule.name, o.ngHardwareSdkModule.name, r
      .ngCoplaySdkModule.name, a.ngSettingsSdkModule.name, l.ngNvCameraSdkModule.name, s.ngFeedbackSdkModule
      .name, d.ngGfeUpdatesSdkModule.name, c.ngHighlightsSdkModule.name, u.ngGfwslSdkModule.name, f
      .ngAbHubSdkModule.name, m.ngQuietMode2SdkModule.name, g.ngNisSdkModule.name, p.ngDvcSdkModule.name, h
      .ngPiplConfigSdkModule.name
    ]);
  b.provider("localSdk", [function() {
    var e;
    return {
      setNodeConfig: function(t) {
        e = t;
      },
      $get: ["$log", "NvEndpointFactory", function(t, n) {
        function i(t) {
          e.port = t.port, e.commonHeaders.X_LOCAL_SECURITY_COOKIE = t.secret;
        }

        function o() {
          return e;
        }

        function r(t, i) {
          var o = new n();
          return o.setUrlGenerator(function(n, o) {
            return e.server + e.port + "/" + t + "/" + i + n.url;
          }), o.setHeaderGenerator(function(t, n) {
            var i = angular.merge({}, t.headers, e.commonHeaders);
            return i;
          }), o;
        }
        return {
          updateNodeInfo: i,
          newLocalEndpointFactory: r,
          getNodeConfig: o
        };
      }]
    };
  }]), exports.ngLocalSdkModule = b;
}
