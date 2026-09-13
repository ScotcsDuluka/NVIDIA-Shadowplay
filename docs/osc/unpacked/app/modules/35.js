// ─────────────────────────────────────────────────────────────
// APP MODULE 35
// role       : service osdService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.osdService = void 0;
  var r = n(8),
    a = o(r),
    l = n(3),
    s = i(l),
    d = n(2);
  n(4), n(32), n(22), n(12), n(11);
  var c = n(290),
    u = o(c),
    f = n(289),
    m = o(f),
    g = n(288),
    p = o(g),
    h = n(294),
    b = o(h),
    x = n(302),
    v = o(x),
    y = n(292),
    w = o(y),
    S = n(295),
    E = o(S),
    k = n(291),
    _ = o(k),
    T = n(309),
    C = o(T),
    O = d.ngMainCommonModule.service("osdService", ["$q", "$log", "$window", "shadowPlayService", "broadcastService",
      "octoolService", "connectService", "oscDisplayService", "eventAggregator", "COMMON_EVENTS", "SHADOWPLAY_EVENTS",
      "BROADCAST_STATES", "CONNECT_EVENTS", "PERFTOOL_EVENTS", "OSC_CONFIG",
      function(e, t, n, i, o, r, l, d, c, f, g, h, x, y, S) {
        function k(e) {
          var t = "Performance";
          e = e || {}, F.overlaySettings[t].supported = !!S.osd && r.getIsFeatureAvailable(), s.isUndefined(e
            .enabled) ? F.overlaySettings[t].enabled = r.defaultOverlayEnabledState : F.overlaySettings[t].enabled = e
            .enabled, F.overlaySettings[t].position = e.position || r.defaultOverlayQuadrant, F.overlaySettings[t]
            .view = e.view || r.defaultOverlayViewName, F.overlaySettings[t].supported && r.updatePerfOverlaySetting(F
              .overlaySettings[t])
        }

        function T() {
          c.trigger(f.OSD_SETTINGS_CHANGED)
        }

        function O() {
          return F.anythingBroadcasting = !1, i.isMRActive().then(function(e) {
            return e ? (F.anythingCapturing = !0, void T()) : i.isIRActive().then(function(e) {
              F.anythingCapturing = e, T()
            })
          })
        }

        function A() {
          var e = o.getBroadcastState();
          F.anythingCapturing = e === h.ACTIVE || e === h.PAUSED, F.anythingBroadcasting = F.anythingCapturing, F
            .broadcastHasOSD = !1, e === h.STOPPED && (G = {}, V = {}, H = 0), F.anythingBroadcasting && o
            .getBroadcastEndpointName() === l.providerNames.FACEBOOK && (F.broadcastHasOSD = !0), T()
        }

        function I(e) {
          G = e
        }

        function M(e) {
          H = e
        }

        function R(e) {
          V = e
        }

        function P() {
          var e = "Performance";
          F.overlaySettings[e].view = r.selectedOverlayViewName, F.saveOverlaySettings(e, !0)
        }

        function D(e) {
          U.info("Feature state change", e), F.loadOverlaySettings("Performance"), F.loadOverlaySettings("FPS")
        }

        function N(t) {
          var i = n.localStorage.getItem(t);
          return i ? d.writeSharedStorage(t, i) : (U.info("No storage found for ", t), e.when(!1))
        }

        function L() {
          U.info("exportLocalstorageData");
          var t = e.when(!0),
            n = ["ModsEnableStatus", "ModsSlotStorage", "ModsDataStorage", "overclockings", "osd-storage",
              "notification-settings"
            ];
          s.each(n, function(e) {
            t = t.then(function() {
              return N(e)
            })
          }), t.then(function() {
            U.info("exportLocalstorageData done")
          })
        }
        var F = this,
          U = t.getInstance("osdService"),
          z = "osd-storage",
          G = {},
          V = {},
          H = 0;
        F.overlaySettings = {
            Camera: {
              id: "Camera",
              text: "l10n.camera",
              icon: {
                Small: u.default,
                Medium: m.default,
                Large: p.default
              },
              supported: !1,
              enabled: !1,
              position: "Nowhere",
              size: "NoSize"
            },
            Status: {
              id: "Status",
              text: "l10n.statusIndicator",
              icon: b.default,
              settingId: i.IndicatorOverlayEnum.INDICATOR_OVERLAY_RECORD,
              supported: !1,
              enabled: !1,
              position: "Nowhere"
            },
            FPS: {
              id: "FPS",
              text: "l10n.fpsCounter",
              icon: v.default,
              settingId: i.IndicatorOverlayEnum.INDICATOR_OVERLAY_FPS,
              supported: !1,
              enabled: !1,
              position: "Nowhere"
            },
            Performance: {
              id: "Performance",
              text: "l10n.perfmonoc.performance",
              icon: C.default,
              supported: !1,
              enabled: !1,
              position: "Nowhere",
              view: r.defaultOverlayViewName
            },
            Viewers: {
              id: "Viewers",
              text: "l10n.viewers",
              icon: w.default,
              settingId: i.IndicatorOverlayEnum.INDICATOR_OVERLAY_VIEWER,
              supported: !1,
              enabled: !1,
              position: "Nowhere"
            },
            MyRig: {
              id: "MyRig",
              text: "l10n.myRigDetails",
              icon: E.default,
              settingId: i.IndicatorOverlayEnum.INDICATOR_OVERLAY_RIG,
              supported: !1,
              enabled: !1,
              position: "Nowhere"
            },
            Comments: {
              id: "Comments",
              text: "l10n.comments",
              icon: _.default,
              supported: !!S.osd,
              enabled: !1,
              position: "Nowhere"
            }
          }, F.stubGetOverlaySupported = function(e) {
            return "Camera" === e ? i.getWebcamPresent() : i.getIndicatorOverlaySupported(F.overlaySettings[e]
              .settingId)
          }, F.stubGetOverlaySettings = function(e) {
            return "Camera" === e ? i.getWebcamOverlaySettings() : i.getIndicatorOverlaySettings(F.overlaySettings[e]
              .settingId)
          }, F.loadOverlaySettings = function(t) {
            if ("MyRig" === t) return e.when(1);
            if ("Comments" === t || "Performance" === t) {
              var i = n.localStorage.getItem(z);
              if (!i) return k(), e.when(1);
              var a = JSON.parse(i);
              return a[t] || (a[t] = {}), "Comments" === t ? (F.overlaySettings[t].enabled = a[t].enabled || !1, F
                  .overlaySettings[t].position = a[t].position || "Nowhere", o.setCommentsOverlayEnabled(F
                    .overlaySettings[t].enabled)) : k(a[t]), F.overlaySettings[t].enabled = F.overlaySettings[t]
                .supported && F.overlaySettings[t].enabled, e.when(1)
            }
            var l = !1;
            return l = "FPS" !== t || (!S.osd || !r.getIsFeatureAvailable()), l ? F.stubGetOverlaySupported(t).then(
              function(e) {
                return F.overlaySettings[t].supported = e || !1, F.stubGetOverlaySettings(t)
              }).then(function(e) {
              return e ? (F.overlaySettings[t].enabled = e.enable || !1, F.overlaySettings[t].position = e
                  .position || "Nowhere", void("Camera" === t && (F.overlaySettings[t].size = e.size || "Small"))
                  ) : void(F.overlaySettings[t].enabled = !1)
            }) : (F.overlaySettings[t].supported = l, F.overlaySettings[t].enabled = l && F.overlaySettings[t]
              .enabled, F.overlaySettings[t].position = "LeftTop", F.saveOverlaySettings(t, !0))
          }, F.saveOverlaySettings = function(t, n) {
            var l = {};
            return n || c.trigger(f.OSD_SETTINGS_CHANGED), l.enable = F.overlaySettings[t].enabled, l.position = F
              .overlaySettings[t].position, "Comments" === t || "Performance" == t ? ("Comments" == t ? o
                .setCommentsOverlayEnabled(l.enable) : "Performance" === t && (n || r.updatePerfOverlaySetting(F
                  .overlaySettings[t], !0)), d.setLocalStorage(z, (0, a.default)(F.overlaySettings)), e.when(1)) :
              "Camera" !== t ? i.setIndicatorOverlaySettings(F.overlaySettings[t].settingId, l).then(function() {
                "Viewers" === t && o.updateViewerCountImage(H)
              }) : (l.size = F.overlaySettings[t].size, i.setWebcamOverlaySettings(l))
          }, F.anythingCapturing = !1, F.anythingBroadcasting = !1, F.broadcastHasOSD = !1, F.getBroadcastComments =
          function() {
            return G
          }, F.getViewerCount = function() {
            return H
          }, F.getReactions = function() {
            return V
          }, F.init = function() {
            var t = [];
            return s.each(F.overlaySettings, function(e) {
              t.push(F.loadOverlaySettings(e.id))
            }), t.push(O()), t.push(A()), c.on(g.STATUS_CHANGE_RECORD, O), c.on(g.STATUS_CHANGE_BROADCAST, A), c.on(
              g.VIEWER_COUNT_UPDATE, M), c.on(x.FACEBOOK_REACTIONS, R), c.on(x.FACEBOOK_COMMENTS, I), c.on(y
              .PERF_OVERLAY_VISIBILITY_CHANGED, P), c.on(y.FEATURE_SUPPORT_STATE_CHANGE, D), U.info(
              "overlay service init"), e.all(t).then(function() {
              c.trigger(f.OSD_SETTINGS_CHANGED), L()
            }, function(e) {
              U.error("Error in overlay service init:", e)
            })
          }
      }
    ]);
  t.osdService = O
}
