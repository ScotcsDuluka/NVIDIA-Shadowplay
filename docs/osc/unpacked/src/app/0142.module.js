// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 142
// (no Angular registrations — utility module)
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
  }), exports.commonConfig = void 0;
  var o = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(17) /* app/17 — localSdk (provider) */, require(11);
  var r = require(296),
    a = i(r),
    l = require(297),
    s = i(l),
    d = require(298),
    c = i(d),
    u = require(299),
    f = i(u),
    m = require(300),
    g = i(m),
    p = require(301),
    h = i(p),
    b = require(303),
    x = i(b),
    v = o.ngMainCommonModule.config(["shadowPlayEndpointsProvider", "highlightsEndpointsProvider",
      "hardwareEndpointsProvider", "localSdkProvider", "coplayEndpointsProvider",
      "settingsEndpointsProvider", "feedbackEndpointsProvider", "nvCameraEndpointsProvider",
      "gfeUpdatesEndpointsProvider", "ugcLibProvider", "abHubEndpointsProvider", "LOCALHOST_ADDR",
      "LOCALHOST_PORT", "quietMode2EndpointsProvider", "nisEndpointsProvider", "dvcEndpointsProvider",
      function(e, t, n, i, o, r, a, l, s, d, c, u, f, m, g, p) {
        var h = {
          server: u,
          port: f,
          commonHeaders: {
            X_LOCAL_SECURITY_COOKIE: ""
          }
        };
        i.setNodeConfig(h), d.setNodeConfig(h);
        var b = {
          version: "v.1.0"
        };
        e.setConfig(b), d.setLocalConfig(b), o.setConfig(b), r.setConfig(b), l.setConfig(b), s.setConfig(b),
          t.setConfig(b), m.setConfig(b), g.setConfig(b), p.setConfig(b);
        var x = {
          version: "v.0.1"
        };
        n.setConfig(x), a.setConfig(x), c.setConfig(x);
      }
    ]);
  o.ngMainCommonModule.config(["$windowProvider", "jarvisProvider", "OSC_CONFIG", "OSC_BUILD_INFO", function(
    e, t, n, i) {
    var o = angular.merge({}, n.jarvis);
    o.clientDescription = o.clientDescription.replace("{VERSION}", i.gfePackageVersion), t.setConfig(o);
  }]), o.ngMainCommonModule.config(["$windowProvider", "jsEventsEndpointsProvider",
    "gfwslEndpointsProvider", "OSC_CONFIG",
    function(e, t, n, i) {
      t.setConfig(i.jsEvents), n.setConfig(i.gfwsl);
    }
  ]), o.ngMainCommonModule.config(["nvAccountEndpointsProvider", "LOCALHOST_ADDR", "LOCALHOST_PORT",
    function(e, t, n) {
      var i = {
        version: "v.1.0",
        server: t,
        port: n,
        commonHeaders: {
          "Content-Type": "application/json"
        }
      };
      e.setConfig(i);
    }
  ]), o.ngMainCommonModule.config(["loggingServiceProvider", "OSCCLIENT_USER_CONFIG", function(e, t) {
    e.setVerboseLoggingEnabled(t.verboseLoggingEnabled), e.setEventLoggingEnabled(t
      .eventLoggingEnabled), e.setPerformanceLoggingEnabled(t.perfLoggingEnabled);
  }]), o.ngMainCommonModule.config(["$translateProvider", function(e) {
    e.useLoader("$translatePartialLoader", {
      urlTemplate: "{part}/{lang}.json"
    }), e.preferredLanguage("en-US"), e.use("en-US"), e.fallbackLanguage("en-US");
  }]), o.ngMainCommonModule.config(["$mdIconProvider", function(e) {
    e.icon("fbReaction:angry", a.default).icon("fbReaction:haha", s.default).icon("fbReaction:like", c
      .default).icon("fbReaction:love", f.default).icon("fbReaction:sad", g.default).icon(
      "fbReaction:wow", h.default).icon("gfeBranding", x.default);
  }]), o.ngMainCommonModule.config(["$mdThemingProvider", function(e) {
    var t = e.extendPalette("green", {
        500: "76b900",
        A100: "76b900"
      }),
      n = e.extendPalette("grey", {
        contrastDefaultColor: "light",
        200: "757575",
        600: "3a3a3a",
        800: "191919"
      });
    e.extendPalette("grey", {});
    e.definePalette("nvAccentPalette", t), e.definePalette("nvPrimaryPalette", n), e.definePalette(
      "nvBackgroundPalette", n), e.theme("default").primaryPalette("nvPrimaryPalette").accentPalette(
      "nvAccentPalette").backgroundPalette("nvBackgroundPalette").dark();
  }]), o.ngMainCommonModule.config(["dbCacheServiceProvider", "AB_STORE", function(e, t) {
    e.loadUserKey(t.AB_STORE_NAME, t.USER_AB_STATE), e.loadGlobalKey(t.AB_STORE_NAME, t.USER_AB_STATE);
  }]), o.ngMainCommonModule.config(["piplConfigEndpointsProvider", "OSC_CONFIG", function(e, t) {
    e.setConfig(t.pipl);
  }]), exports.commonConfig = v;
}
