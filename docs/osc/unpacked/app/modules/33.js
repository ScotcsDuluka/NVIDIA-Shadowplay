// ─────────────────────────────────────────────────────────────
// APP MODULE 33
// role       : service coplayService
// requires   : (none)
// channels   : /GameShare/v.1.0/SessionUpdate, /GameShare/v.1.0/CreateSession
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
  }), t.coplayService = void 0;
  var o = n(8),
    r = i(o),
    a = n(110),
    l = i(a),
    s = n(2);
  n(17), n(23), n(46), n(94), n(4), n(20), n(61);
  var d = s.ngMainCommonModule.service("coplayService", ["$window", "$filter", "$timeout", "$log", "$q",
    "coplayEndpoints", "socketService", "eventAggregator", "oscDisplayService", "oscNotificationService",
    "errorDialogService", "cefService", "webrtcP2PService", "shadowPlayService", "telemetryService", "COPLAY_STATE",
    "COPLAY_EVENTS", "COPLAY_CONTROLLER_MAPPING", "OSC_EVENTS", "NOTIFIER_SELECTIONS", "SHADOWPLAY_EVENTS",
    "HOTKEY_EVENTS", "TELEMETRY_OSC_EVENT_NAMES",
    function(e, t, n, i, o, a, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S, E, k) {
      function _() {
        W.info("webrtc hangup received")
      }

      function T() {
        W.info("webrtc connected")
      }

      function C(e) {
        g.setMicrophoneMute(se, e)
      }

      function O() {
        de = !0, g.setVerbose(se, !0), C(!("alwayson" === ce)), g.startCall(se, {
          proxyAddress: j.serverAddress,
          proxySessionId: j.sessionId,
          videoOn: !1,
          audioOn: !0
        }), h.push(k.OSC_GAMESHARE_STARTED)
      }

      function A() {
        de = !1, g.hangup(se)
      }

      function I() {
        de && "ptt" === ce && C(!1)
      }

      function M() {
        de && "ptt" === ce && C(!0)
      }

      function R(e) {
        ce = e, de && C(!("alwayson" === ce))
      }

      function P(e) {
        e !== Q && (Q = e, d.trigger(x.COPLAY_STATE_CHANGED), de && e === b.OFF ? A() : de || e !== b.ON && e !== b
          .PAUSED || O(), p.setCoplayState(Q))
      }

      function D(e) {
        void 0 === ("undefined" == typeof e ? "undefined" : (0, l.default)(e)) && f.show("l10n.notice",
            "l10n.notificationCoplayInviteFailed"), P(b.OFF), j.sessionId = "", $.deleteSession(), n.cancel(J), J =
          null
      }

      function N(e) {
        return Q !== b.OFF ? (W.info("coplay create session not in off state"), o.when(!1)) : (W.info(
          "create session start: ", e), P(b.CREATINGINVITE), a.createSession({}, e).then(function() {
          W.info("CreateSession start"), J = n(D, ee)
        }))
      }

      function L() {
        P(b.OFF), te = !1, oe && (n.cancel(oe), n.cancel(re), n.cancel(ae), n.cancel(le)), j.sessionId = "", j
          .serverAddress = "", h.push(k.OSC_GAMESHARE_STOPPED)
      }

      function F(e) {
        if (0 === j.sessionId.length) {
          var t = o.defer();
          return t.resolve(!1), t.promise
        }
        te = e;
        return a.modifySession({
          id: j.sessionId
        }, {
          action: e ? "pause" : "resume"
        }).then(function() {
          W.info("coplay state set to: ", e)
        })
      }

      function U(e) {
        return Q !== b.CREATINGINVITE ? (j.sessionId = e.sessionInfo.sessionId, void $.deleteSession()) : (P(b
            .INVITESENT), j.sessionId = e.sessionInfo.sessionId, j.state = e.sessionInfo.state, j.title = e
          .sessionInfo.gameTitle, j.exePath = e.sessionInfo.gameExePath, j.serverAddress = e.sessionInfo
          .serverAddress, j.processId = e.sessionInfo.processId, j.serverAddress.indexOf("://") === -1 && (j
            .serverAddress = "wss://" + e.sessionInfo.serverAddress), void("invitePrepared" === e.status && m
            .setClipboardData(e.invitationLink)))
      }

      function z(e) {
        if (n.cancel(J), J = null, W.info("create session callback: ", e), "failed" === e.status) {
          var i = t("translate")("l10n.aGame");
          switch (e.curSession && e.curSession.title && (i = e.curSession.title), h.push(k
              .OSC_GAMESHARE_ERROR_CANNOT_START, e.error), e.error) {
            case "processBlacklisted":
              f.show("l10n.notSupported", "l10n.notificationCoplayInviteBlacklisted", i);
              break;
            case "processNotSupported":
              f.show("l10n.systemRequirement", "l10n.notificationWarningGameRequired");
              break;
            case "processRunningOnIGPU":
              f.show("l10n.systemRequirement", "l10n.notificationWarningNvidiaGpuRequired");
              break;
            case "InvalidToAddress":
              f.show("l10n.notice", "l10n.notificationCoplayInvalidTo", K);
              break;
            case "inviteError":
              f.show("l10n.systemRequirement", "l10n.notificationCoplayNetworkUnavailable");
              break;
            case "processBlacklistInvalid":
            case "processNameInvalid":
            case "sessionAlreadyExists":
            case "internalError":
            case "unknown":
            default:
              f.show("l10n.notice", "l10n.notificationCoplayInviteFailed")
          }
          D(!0)
        } else "inviteSent" === e.status || "invitePrepared" === e.status ? U(e) : "sendingInvite" === e.status ||
          "preparingInvite" === e.status ? W.info("preparing invite") : W.info("unknown response from coplay: ", e)
      }

      function G(e) {
        if (W.info("session update: ", e), e.state)
          if ("stopped" === e.state) {
            if (Q === b.OFF) return;
            u.show(w.COPLAY_DONE, K), L()
          } else if ("streaming" === e.state) {
          if (Q === b.CREATINGINVITE) return;
          P(b.ON), oe = n(function() {
            u.show(w.COPLAY_5_MINUTES)
          }, 60 * (ie - 5) * 1e3), re = n(function() {
            u.show(w.COPLAY_1_MINUTE)
          }, 60 * (ie - 1) * 1e3), ae = n(function() {
            u.show(w.COPLAY_EXPIRED)
          }, 1e3 * (60 * ie - 15)), le = n(function() {
            $.deleteSession()
          }, 1e3 * (60 * ie - 15)), u.show(w.COPLAY_NOW_PLAYING, K), $.setControllerMapping(), ne && !te && F(ne)
        }
        e.deviceFriendlyName && (W.info("Device changed, using device ", e.deviceFriendlyName), g
          .setPreferredAudioDevice(se, e.deviceFriendlyName))
      }

      function V() {
        p.setCoplayEnabled(!1)
      }

      function H() {
        ue = !0;
        try {
          var t = e.localStorage.getItem(X);
          t && (ue = JSON.parse(t))
        } catch (t) {
          e.localStorage.setItem(X, (0, r.default)(ue))
        }
      }

      function B(e) {
        if (ne = "open" === e, ne && V(), Q === b.ON) return $.getFullscreenPid().then(function(e) {
          e === j.processId && (!ne && te || ne) && F(ne)
        })
      }

      function Y() {
        var e = "/GameShare/v.1.0/SessionUpdate",
          t = "/GameShare/v.1.0/CreateSession",
          n = "coplaySessionUpdate",
          i = "coplaySessionCreate";
        W.info("Registering coplay notification events"), s.register(e, n), s.register(t, i), d.on(n, G), d.on(i,
          z), d.on(y.OSC_STATE, B), d.on(E.MIC_PTT_DOWN, I), d.on(E.MIC_PTT_UP, M), d.on(S.MIC_STATUS_CHANGE, R)
      }
      var $ = this,
        W = i.getInstance("osc/coplayService"),
        j = {
          sessionId: "",
          state: "",
          title: "",
          exePath: "",
          serverAddress: "",
          processId: ""
        },
        K = "",
        q = "coplay-controller-mapping",
        X = "coplay-enabled",
        Z = null,
        Q = b.OFF,
        J = null,
        ee = 15e3,
        te = !1,
        ne = !1,
        ie = 59,
        oe = null,
        re = null,
        ae = null,
        le = null,
        se = "defaultSession",
        de = !1,
        ce = "off",
        ue = !0;
      $.getInviteName = function() {
        return K
      }, $.getState = function() {
        return Q
      }, $.getFullscreenPid = function() {
        return a.getFullscreenPid({
          activeWindow: c.getLastActiveWindow()
        }).then(function(e) {
          return void 0 !== (0, l.default)(e.data.processId) ? e.data.processId : (W.info(
            "bad response from coplay node: ", e), -1)
        }, function(e) {
          return W.info("coplay/node error", e), -1
        })
      }, $.getControllerMapping = function() {
        if (!Z) try {
          var t = e.localStorage.getItem(q);
          t && (Z = JSON.parse(t)), t && "undefined" !== Z || (Z = v.BLOCKED)
        } catch (t) {
          Z = v.BLOCKED, e.localStorage.setItem(q, (0, r.default)(Z))
        }
        return Z
      }, $.setControllerMapping = function(t) {
        return "undefined" != typeof t && (Z = t, e.localStorage.setItem(q, (0, r.default)(t))), a
          .configureControllerMapping({}, {
            mode: Z
          })
      }, $.createLinkSession = function() {
        var e = t("translate")("l10n.copyFromDisplayName");
        K = e, h.push(k.OSC_GAMESHARE_INVITE_COPY);
        var n = {
          activeWindow: c.getLastActiveWindow(),
          displayName: e,
          inviteMode: "invitationLink"
        };
        return N(n)
      }, $.createEmailSession = function(e, n) {
        var i = t("translate")("l10n.copyFromDisplayName");
        "undefined" != typeof n && (i = n), h.push(k.OSC_GAMESHARE_INVITE_EMAIL), u.show(w.COPLAY_SENDING_INVITE,
          e), K = e;
        var o = {
          activeWindow: c.getLastActiveWindow(),
          displayName: i,
          inviteMode: "email",
          emailId: e
        };
        return N(o)
      }, $.deleteSession = function() {
        var e = j.sessionId;
        L(), e.length > 0 && a.deleteSession({
          id: e
        }).then(function() {
          W.info("deleting coplay session")
        })
      }, $.setPause = function(e) {
        if (0 === j.sessionId.length) {
          var t = o.defer();
          return t.resolve(!1), t.promise
        }
        return $.getFullscreenPid().then(function(t) {
          return t !== j.processId ? (u.show(w.WARNING_FULLSCREEN_GAME_REQUIRED), !1) : (h.push(k
            .OSC_GAMESHARE_PAUSED, String(e)), P(e ? b.PAUSED : b.ON), !0)
        })
      }, $.shouldShowCoplaySetting = function() {
        return !1
      }, $.setLocalCoplayFlag = function(t) {
        "undefined" != typeof t && (ue = t, e.localStorage.setItem(X, (0, r.default)(t)))
      }, $.init = function() {
        W.info("Initialize CoplayService"), p.getMicMode().then(function(e) {
          ce = e
        }), $.getControllerMapping(), H(), V(), g.registerRemoteVideoCallback(se, T), g.registerHangupCallback(
          se, _), Y()
      }
    }
  ]);
  t.coplayService = d
}
