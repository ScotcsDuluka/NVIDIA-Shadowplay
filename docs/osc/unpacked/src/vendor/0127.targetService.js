// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 127
// service targetService | constant ADOBE_TARGET_MBOXES | constant ADOBE_TARGET_CONFIGURATION | constant CRIMSON_WINDOW_NAMES | defines angular.module("nvExperienceControl")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  var i,
    o,
    a = require(153),
    s = r(a),
    c = require(26);
  r(c);
  ! function(r, a) {
    i = [require(24)], o = function(e) {
      return a(e);
    }.apply(exports, i), !(void 0 !== o && (module.exports = o));
  }(void 0, function(e) {
    e.module("nvExperienceControl", ["crimson"]), e.module("nvExperienceControl").constant(
      "ADOBE_TARGET_MBOXES", {
        GFE_EXPERIMENT_MBOX: "gfe-app-mbox",
        GFN_EXPERIMENT_MBOX: "gfn-app-mbox",
        OSC_EXPERIMENT_MBOX: "osc-app-mbox",
        CONVERSION_MBOX: "mboxClickTrack"
      }).constant("ADOBE_TARGET_CONFIGURATION", {
      DEFAULT_TIMEOUT: 3e4
    }).constant("CRIMSON_WINDOW_NAMES", {
      GFE: "gfeclient",
      GFN: "gfnclient",
      OSC: "shareclient"
    }), e.module("nvExperienceControl").service("targetService", ["$window", "$q", "$log",
      "ADOBE_TARGET_MBOXES", "ADOBE_TARGET_CONFIGURATION", "CRIMSON_WINDOW_NAMES",
      function(e, t, n, r, i, o) {
        "use strict";

        function a(e) {
          return e.activityId ? (p[e.activityId] || (p[e.activityId] = l), p[e.activityId]) : (p[e
            .activityName] || (p[e.activityName] = l), p[e.activityName]);
        }
        var c = this,
          u = n.getInstance("nvExperienceControl/targetService"),
          l = void 0,
          d = void 0,
          f = i.DEFAULT_TIMEOUT,
          h = null,
          p = {};
        c.initialize = function(e, t, n) {
          c.setMboxThirdPartyId(e), c.setDefaultTimeout(t), c.setExperimentMbox(n);
        }, c.setMboxThirdPartyId = function(e) {
          l = e || void 0;
        }, c.setDefaultTimeout = function(e) {
          f = e ? e : f;
        }, c.setExperimentMbox = function(e) {
          switch (e) {
            case o.GFE:
              d = r.GFE_EXPERIMENT_MBOX;
              break;
            case o.GFN:
              d = r.GFN_EXPERIMENT_MBOX;
              break;
            case o.OSC:
              d = r.OSC_EXPERIMENT_MBOX;
              break;
            default:
              d = void 0;
          }
        }, c.setQaConfig = function(e) {
          h = e;
        }, c.getVariant = function(n) {
          var r = t.defer();
          if (!n || !n.activityId && !n.activityName) {
            var i = "Invalid request params to get variant";
            return u.error(i), r.reject({
              apiFailure: !1,
              message: i
            }), r.promise;
          }
          if (h && h[n.activityId]) return u.debug("Resolving fake response for", n.activityId, h[n
            .activityId]), r.resolve(h[n.activityId]), r.promise;
          if (e.adobe && e.adobe.target) {
            u.info("Getting variant for", n);
            var o = (0, s.default)({}, n);
            if (o.mbox3rdPartyId = a(o), d) e.adobe.target.getOffer({
              mbox: d,
              params: o,
              success: function(e) {
                if (u.info("Got adobe target offer, response: ", e), e && e[0] && e[0].content)
                  try {
                    var t = JSON.parse(e[0].content);
                    r.resolve(t);
                  } catch (t) {
                    u.error("Warning, failed to parse response offer, relaying response"), r
                      .resolve({
                        variant: {
                          data: e[0].content
                        }
                      });
                  } else r.reject({
                    apiFailure: !1,
                    message: "undefined offer content"
                  });
              },
              error: function(e, t) {
                u.error("Failed to get adobe target offer: ", t), r.reject({
                  apiFailure: !0,
                  message: t,
                  status: e
                });
              },
              timeout: f
            });
            else {
              var i = "Configuration error, invalid experiment mbox";
              u.error(i), r.reject({
                apiFailure: !1,
                message: i
              });
            }
          } else {
            var i = "Setup error, Adobe target object not found on $window";
            u.error(i), r.reject({
              apiFailure: !1,
              message: i
            });
          }
          return r.promise;
        }, c.trackConversion = function(n) {
          var i = t.defer();
          if (!n || !n.activityId && !n.activityName) {
            var o = "Invalid request params to track conversion";
            return u.error(o), i.reject({
              apiFailure: !1,
              message: o
            }), i.promise;
          }
          if (h && h[n.activityId]) return u.debug("Tracking fake conversion for", n.activityId), i
            .resolve({}), i.promise;
          if (e.adobe && e.adobe.target) {
            u.info("Tracking conversion");
            var c = (0, s.default)({}, n);
            c.mbox3rdPartyId = a(c), e.adobe.target.getOffer({
              mbox: r.CONVERSION_MBOX,
              params: c,
              success: function(e) {
                u.info("Conversion tracked, response: ", e), i.resolve(e);
              },
              error: function(e, t) {
                u.error("Tracking conversion failed: ", t), i.reject({
                  apiFailure: !0,
                  message: t,
                  status: e
                });
              },
              timeout: f
            });
          } else {
            var o = "Setup error, Adobe target object not found on $window";
            u.error(o), i.reject({
              apiFailure: !1,
              message: o
            });
          }
          return i.promise;
        };
      }
    ]);
  });
}
