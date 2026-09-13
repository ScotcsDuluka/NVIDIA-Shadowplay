// ─────────────────────────────────────────────────────────────
// APP MODULE 12
// role       : service oscDisplayService
// requires   : (none)
// cefQuery   : QUERY_FULLSCREEN_STATE, QUERY_OSC_REGISTER_CLOSE_EVENT, QUERY_OSC_SET_DISPLAY_RECTS
// channels   : /ShadowPlay/v.1.0/WindowState, /ShadowPlay/v.1.0/DisplayOscPreferences, /ShadowPlay/v.1.0/DisplayOscState
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
  }), t.oscDisplayService = void 0;
  var o = n(110),
    r = i(o),
    a = n(8),
    l = i(a),
    s = n(2);
  n(26), n(20);
  var d = s.ngMainCommonModule.service("oscDisplayService", ["$state", "$stateParams", "$log", "$timeout", "$rootScope",
    "$window", "cefService", "socketService", "eventAggregator", "telemetryService", "highlightsEndpoints",
    "shadowPlayEndpoints", "HOTKEY_EVENTS", "WINDOW_EVENTS", "TELEMETRY_OSC_EVENT_NAMES", "PERFTOOL_EVENTS",
    "OSC_EVENTS", "TELEMETRY_OSC_PERF_ID", "CONFIRMATION_TYPE", "COMMON_EVENTS", "NOTIFIER_SELECTIONS",
    function(e, t, n, i, o, a, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S) {
      function E() {
        return s.query({
          command: "QUERY_FULLSCREEN_STATE"
        })
      }

      function k(e) {
        var e = 4294967295,
          t = m.getCaptureProcessInfo(e);
        return t().then(function(e) {
          return V.info("getAppInFocus: success", e), M.appInFocus = e.data, !0
        }, function(e) {
          return V.error("getAppInFocus: error", e), !1
        })
      }

      function _(t, n) {
        void 0 !== t && t || (t = "main.main-menu", "main.main-menu" === e.current.name && (G = !0, t = "base", V
          .info("need transition workaround"))), P || (z = !0, P = !0, D = !0, c.trigger(x.OSC_STATE, "open"), u
          .push(h.OSC_HOTKEY_TOGGLE), u.startNavigation(h.OSC_NAVIGATION_TIME)), E().then(function(e) {
          e && (e = JSON.parse(e), e && (F = !e.fullscreen, c.trigger(S.HDR_ENABLED, !!e.hdractive), c.trigger(x
            .DESKTOP_STATE, F)))
        }), e.go(t, n)
      }

      function T() {
        z || (P ? M.closeOSC() : M.openOSC())
      }

      function C() {
        M.openOSC("main.preferences")
      }

      function O(e) {
        "preferences" === e.state ? M.openOSC("main.preferences") : "gallery" === e.state ? e.params ? M.openOSC(
          "main.gallery.upload", {
            file: e.params
          }) : M.openOSC("main.gallery.files") : V.error("unknown state ", e.state)
      }

      function A(e) {
        V.info("Window event: ", e.windowMsg), "dismiss" === e.windowMsg ? M.closeOSC() : "fullscreenTransition" ===
          e.windowMsg ? null !== F && (R || P) && E().then(function(e) {
            if (e) {
              e = JSON.parse(e);
              var t = !e.fullscreen;
              c.trigger(S.HDR_ENABLED, !!e.hdractive), e && (t !== F || !t && e.borderlessMode !== U) && (F = t,
                U = e.borderlessMode, V.info("new fs state: ", e.fullscreen, e.borderlessMode, e), c.trigger(x
                  .DESKTOP_STATE, F), M.transitionDisplayFullscreen(), V.info(
                  "closing OSC because of state change"))
            }
          }) : "overlayToggle" === e.windowMsg ? T() : "showHotkeyMessage" === e.windowMsg && c.trigger(p
            .DISPLAY_HOTKEY, null)
      }

      function I() {
        a.cefQuery ? a.cefQuery({
          request: (0, l.default)({
            command: "QUERY_OSC_REGISTER_CLOSE_EVENT"
          }),
          persistent: !0,
          onSuccess: function(e) {
            V.info("got close message from cef"), M.closeOSC()
          },
          onFailure: function(e, t) {
            V.info(t)
          }
        }) : V.info("No cefQuery.")
      }
      var M = this,
        R = 0,
        P = !1,
        D = !1,
        N = !0,
        L = null,
        F = null,
        U = null,
        z = !1,
        G = !1;
      M.appInFocus = {};
      var V = n.getInstance("osc/displayService");
      M.getLastActiveWindow = function() {
        return L
      }, M.setDisplayRects = function(e) {
        return V.info("openWithInput:", P), V.info("openNoInput:", R), e.length > 0 && (P || R > 1) ? void V.info(
          "ignoring setDisplayRects:", e) : (V.info("calling setDisplayRects:", e), s.query({
          command: "QUERY_OSC_SET_DISPLAY_RECTS",
          displayRects: e
        }))
      }, M.openOSC = function(e, t) {
        M.setDisplayRects([]), z || k().then(function() {
          u.startWarm(), _(e, t)
        })
      }, M.closeOSC = function() {
        P && !z && (M.appInFocus = {}, u.startPerf(v.closeOSC), P = !1, e.go("base"), c.trigger(x.OSC_STATE,
          "closed"), V.info("Sending event - closeOSC"), c.trigger(b.PERF_OVERLAY_VISIBILITY_CHANGED), u.push(
          h.OSC_HOTKEY_TOGGLE), u.endNavigation(h.OSC_NAVIGATION_TIME), M.notifyOverlayState(!1), R > 0 ? (V
          .info("Open OSC without input"), s.openOSC(!1)) : (V.info("Close OSC"), s.closeOSC(), u
          .endPerfAfterDigest(v.closeOSC)))
      }, M.openOSCForNotification = function(t) {
        t && M.setDisplayRects([]), R += 1, E().then(function(e) {
          e && (e = JSON.parse(e), e && (F = !e.fullscreen, c.trigger(x.DESKTOP_STATE, F)))
        }), P || ("base" !== e.current.name && (V.info("forcing notification to base"), e.go("base")), V.info(
          "Open OSC without input"), s.openOSC(!1))
      }, M.openInDisplay = function(t) {
        R += 1, P || (e.go(t), V.info("Open OSD without input"), s.openOSC(!1))
      }, M.closeOSCForNotification = function() {
        0 != R && (R -= 1, R < 0 && (R = 0), i(function() {
          V.info("Sending event from - closeOSCForNotification"), c.trigger(b
            .PERF_OVERLAY_VISIBILITY_CHANGED), 0 !== R || P || (V.info("Close OSC"), s.closeOSC())
        }, 5e3))
      }, M.closeForDisplay = function() {
        R -= 1, R < 0 && (R = 0), 0 !== R || P || (V.info("Close OSC"), e.go("base"), s.closeOSC())
      }, M.transitionDisplayFullscreen = function() {
        R <= 0 && P <= 0 || (s.closeOSC(), i(function() {
          V.info("Reopening OSC after state change"), s.openOSC(P > 0), c.trigger(w.OSD_SETTINGS_CHANGED)
        }, 1e3))
      }, M.notifyOverlayState = function(n) {
        var i = "main";
        return "main.confirmation" === e.current.name && t.confirmationType === y.PERMISSION ? i = "permission" :
          "main.gallery.upload" === e.current.name && void 0 !== t.moments && (i = "highlightsSummary"), f
          .notifyOverlayState({}, {
            open: n,
            state: i
          }).then(function(e) {
            return !0
          }, function(e) {
            return V.error("notifyOverlayState endpoint error: ", e), !1
          })
      }, M.setLocalStorage = function(e, t) {
        a.localStorage.setItem(e, t), M.writeSharedStorage(e, t)
      }, M.writeSharedStorage = function(e, t) {
        var n = e.split("/"),
          i = JSON.parse(t);
        return "ModsEnableStatus" === e && (i = {}, i.value = t), s.writeSharedStorage(n, i).then(function(t) {
          V.info("writeSharedStorage for ", e, "successful: ", t)
        }, function(t) {
          V.error("writeSharedStorage for ", e, "failed: ", t)
        })
      }, M.init = function() {
        var t = "/ShadowPlay/v.1.0/WindowState",
          n = "/ShadowPlay/v.1.0/DisplayOscPreferences",
          i = "/ShadowPlay/v.1.0/DisplayOscState",
          a = "windowStateEvent",
          l = "displaySettingsEvent",
          f = "displayStateEvent";
        return d.register(t, a), c.on(a, A), d.register(n, l), c.on(l, C), d.register(i, f), c.on(f, O), c.on(g
          .OSC_TOGGLE, T), I(), o.$on("$stateChangeStart", function(e, t, n, i) {
          "base" !== t.name && (s.allowOSCPainting(!1), N = !1), D && (D = !1, s.openOSC(!0).then(function(
          e) {
            void 0 !== ("undefined" == typeof e ? "undefined" : (0, r.default)(e)) && "noWindow" !==
              e && (L = e), M.notifyOverlayState(!0), u.endWarm()
          }))
        }), o.$on("$viewContentLoaded", function() {
          G ? (G = !1, e.go("main.main-menu"), V.info("going to main now")) : N || (s.allowOSCPainting(!0, z),
            N = !0, z = !1)
        }), !0
      }
    }
  ]);
  t.oscDisplayService = d
}
