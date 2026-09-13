// ─────────────────────────────────────────────────────────────
// APP MODULE 32
// role       : service broadcastService
// requires   : (none)
// channels   : /ShadowPlay/v.1.0/Broadcast/Enable, /ShadowPlay/v.1.0/Broadcast/Pause, /ShadowPlay/v.1.0/Broadcast/SessionEvent
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
  }), t.broadcastService = void 0;
  var r = n(8),
    a = o(r),
    l = n(3),
    s = i(l),
    d = n(2);
  n(87), n(11), n(25), n(4), n(23), n(20), n(12);
  var c = n(304),
    u = o(c),
    f = n(305),
    m = o(f),
    g = n(306),
    p = o(g),
    h = d.ngMainCommonModule.service("broadcastService", ["$q", "$log", "$interval", "$timeout", "shadowPlayService",
      "keyboardService", "connectService", "shadowPlayEndpoints", "hardwareService", "oscService",
      "errorDialogService", "oscNotificationService", "socketService", "eventAggregator", "oscDisplayService",
      "telemetryService", "cefService", "NOTIFIER_SELECTIONS", "BROADCAST_STATES", "HOTKEY_EVENTS",
      "SHADOWPLAY_EVENTS", "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_PERF_ID", "TELEMETRY_OSC_TOGGLE_STATE",
      "TELEMETRY_OSC_SCREEN_STATE", "TELEMETRY_OSC_QUALITY_SETTING", "TELEMETRY_OSC_MIC_MODE",
      "TELEMETRY_OSC_BOOLEAN_STATUS", "TELEMETRY_OSC_AVAILABLE_STATUS", "OSC_CONFIG",
      function(e, t, n, i, o, r, l, d, c, f, g, h, b, x, v, y, w, S, E, k, _, T, C, O, A, I, M, R, P, D) {
        function N(e) {
          var t = Z.name === l.providerNames.TWITCH,
            r = Z.name === l.providerNames.GOOGLE,
            a = Z.name === l.providerNames.FACEBOOK,
            s = void 0 !== Z.providerInfo.services[de].broadcastTimeout;
          te.info("broadcastNotifier (data): ", e);
          var d = null,
            c = 0;
          e.status === !0 && (o.isBroadcastStartActive = !1, w.isInDesktopMode().then(function(e) {
            e ? (t ? ae = T.OSC_BROADCAST_TWITCH_SESSION_DURATION_DT : r && (ae = T
              .OSC_BROADCAST_YOUTUBE_SESSION_DURATION_DT), be = A.desktop) : (t ? ae = T
              .OSC_BROADCAST_TWITCH_SESSION_DURATION_FS : r && (ae = T
                .OSC_BROADCAST_YOUTUBE_SESSION_DURATION_FS), be = A.fullscreen), ae && y.startBroadcast(ae)
          }), y.startBroadcast(T.OSC_SESSION), o.getWebcamShown().then(function(e) {
            e ? (t ? y.push(T.OSC_BROADCAST_TWITCH_CAMERA) : r && y.push(T.OSC_BROADCAST_YOUTUBE_CAMERA), xe
              .camera = O.on) : xe.camera = O.off
          }), s === !0 && (pe = Z.providerInfo.services[de].max_duration, te.info(
            "Timeout for this broadcast session : ", pe), a && (ue = i(function() {
            d = S.BROADCAST_TIMEOUT_N_MINUTES, c = 5, h.show(d, de, c)
          }, 60 * (pe - 5) * 1e3), fe = i(function() {
            d = S.BROADCAST_TIMEOUT_N_MINUTES, c = 3, h.show(d, de, c)
          }, 60 * (pe - 3) * 1e3), me = i(function() {
            d = S.BROADCAST_TIMEOUT_1_MINUTE, c = 1, h.show(d, de, c)
          }, 60 * (pe - 1) * 1e3), ge = i(function() {
            le = "broadcastStopTimeout", X.stopBroadcast()
          }, 60 * pe * 1e3)))), e.status === !1 && (oe || (oe = !0, X.stopBroadcast()), j(!1), oe = !1, angular
            .isDefined(Q) && n.cancel(Q), J = !1, ae && y.endBroadcast(ae), y.endBroadcast(T.OSC_SESSION, {
              provider: Z.name,
              screenState: be
            }), ae = null, ue && (i.cancel(ue), i.cancel(fe), i.cancel(me), i.cancel(ge)))
        }

        function L() {
          v.openOSC("main.broadcast-menu")
        }

        function F(e, t) {
          var n = Math.floor((new Date).getTime() / 1e3),
            i = ye;
          t === !1 && (i += n - e);
          var o = Math.floor(i / 3600),
            r = Math.floor((i - 3600 * o) / 60),
            a = Math.floor(i - 3600 * o - 60 * r);
          return o < 10 && (o = "0" + o), r < 10 && (r = "0" + r), a < 10 && (a = "0" + a), o + ":" + r + ":" + a
        }

        function U(e) {
          var t = Z.name === l.providerNames.TWITCH,
            n = Z.name === l.providerNames.GOOGLE,
            i = Z.name === l.providerNames.FACEBOOK;
          te.info("broadcastPauseNotifier (data): ", e);
          var o = null;
          re ? re = !1 : ((t || n || i) && e.status === !0 ? (X._broadcastState = E.PAUSED, o = S.BROADCAST_PAUSED) :
            (t || n || i) && e.status === !1 ? (X._broadcastState = E.ACTIVE, o = S.BROADCAST_RESUMED) : te.error(
              "broadcastPauseNotifier (other): ", e), o && (h.show(o, de), x.trigger(_.STATUS_CHANGE_BROADCAST)))
        }

        function z(e) {
          var t = null;
          "updateTitle" === e.sessionEvent ? se || "useProfiledTitle" !== e.titleSource && "useWindowTitle" !== e
            .titleSource ? se ? te.info("Broadcast title has already been set to 'non-Desktop' value.") : te.info(
              "Update broadcast title (data.titleSource): Unknown.") : d.getBroadcastTitle().then(function(e) {
              t = e.data.title, s.isUndefined(t) || s.isNull(t) || (X.updateBroadcastTitle(t).then(function() {
                te.info("Update broadcast title (new title): ", t), X.setDescription(t), "Desktop" !== t && (
                  se = !0)
              }), xe.gameTitle = t), X.getViewerCount()
            }) : "streamerBroadcastStop" === e.sessionEvent ? (te.info("Streamer stopped the broadcast"), y.push(T
              .OSC_BROADCAST_STOPPED_AUTO_ADAPT, ce), le = "streamerBroadcastStop", X.stopBroadcast()) :
            "spTelemetry" === e.sessionEvent && "broadcastAutoAdaptTriggered" === e.spTelemetryName && y.push(T
              .OSC_BROADCAST_AUTO_ADAPT_TRIGGER)
        }

        function G() {
          X.getBroadcastPreference().then(function(e) {
            var t = X.getBroadcastPortalFromName(e);
            t.id >= 0 && (ne = null, ne = X.getBroadcastSessionStatus(), ne.then(function(e) {
              e === !0 ? (te.info("Hotkey : Stop broadcast"), X.stopBroadcast()) : X.tryStartBroadcast(!0)
            }))
          })
        }

        function V() {
          ne = null, ne = X.getBroadcastSessionStatus(), ne.then(function(e) {
            e === !0 && (ie === !1 ? X.pauseBroadcast() : X.resumeBroadcast())
          })
        }

        function H() {
          X.getBroadcastSessionStatus().then(function(e) {
            e && X.getCustomOverlaySupportType().then(function(e) {
              if (2 == e) {
                var t = s.indexOf(X.isMultipleCustomOverlayEnabled, !0);
                t === -1 && h.show(S.CUSTOMOVERLAY_ALL_SLOT_EMPTY)
              }
            })
          })
        }

        function B() {
          var e = 1;
          W(e)
        }

        function Y() {
          var e = 2;
          W(e)
        }

        function $() {
          var e = 3;
          W(e)
        }

        function W(e) {
          X.getBroadcastSessionStatus().then(function(t) {
            t && X.getCustomOverlaySupportType().then(function(t) {
              if (2 === t) {
                var n, i;
                X.getMultipleCustomOverlayEnabled(e).then(function(t) {
                  t ? X.getCustomOverlayDisplayState().then(function(e) {
                    te.info("Displaying custom overlay : ", e), e || (i = o.HotkeyShortcuts
                      .OVERLAYTOGGLE, o.getHotkeyShortcut(i).then(function(e) {
                        if (!s.isUndefined(e.keys)) {
                          var t = r.shortcutToStr(e.keys);
                          n = S.CUSTOMOVERLAY_TURNED_OFF, h.show(n, t)
                        }
                      }))
                  }) : (n = S.CUSTOMOVERLAY_NOT_ASSIGNED, h.show(n, e))
                })
              }
            })
          })
        }

        function j(e) {
          te.info("Dynamic enable/disable hotkeys: ", e);
          var t = ["overlayaswitch", "overlaybswitch", "overlaycswitch"];
          return d.dynamicHotkeyToggle({}, {
            enable: e,
            hotkeyNames: t
          }).then(function() {
            te.info("enable contextual hotkey")
          })
        }

        function K(e) {
          J = e
        }

        function q() {
          var e = "/ShadowPlay/v.1.0/Broadcast/Enable",
            t = "/ShadowPlay/v.1.0/Broadcast/Pause",
            n = "/ShadowPlay/v.1.0/Broadcast/SessionEvent",
            i = "broadcastNotifier",
            o = "broadcastPauseNotifier",
            r = "broadcastSessionEventNotifier";
          te.info("Registering broadcast notification events"), b.register(e, i), b.register(t, o), b.register(n, r),
            x.on(i, N), x.on(o, U), x.on(r, z), x.on(k.GAMECAST, G), x.on(k.PAUSERESUME, V), x.on(k.CUSTOMOVERLAYA,
            B), x.on(k.CUSTOMOVERLAYB, Y), x.on(k.CUSTOMOVERLAYC, $), x.on(k.CUSTOMOVERLAY, H), Se = !0
        }
        var X = this;
        this._broadcastState = E.STOPPED, this._currentPortalName = "";
        var Z = null,
          Q = null,
          J = !1,
          ee = 0,
          te = t.getInstance("osc/broadcastService"),
          ne = !1,
          ie = !1,
          oe = !1,
          re = !1,
          ae = null,
          le = null,
          se = !1,
          de = null,
          ce = "",
          ue = null,
          fe = null,
          me = null,
          ge = null,
          pe = 0,
          he = !1,
          be = A.fullscreen,
          xe = {
            provider: "",
            customOverlayState: O.off,
            micMode: M.off,
            quality: I.Good,
            resolution: "0x0",
            bitRate: 0,
            fps: 0,
            camera: O.off,
            gameTitle: "",
            privacy: "",
            commentsOn: R.FALSE,
            freestyleActive: P.no
          };
        X.maxViewerCount = 0, X.CustomOverlaySupportTypePreference = [{
          id: 0,
          name: "none"
        }, {
          id: 1,
          name: "single"
        }, {
          id: 2,
          name: "multiple"
        }], X.isAnythingBlocking = function() {
          return o.isBroadcastRecordSupported().then(function(e) {
            return e || te.info("isAnythingBlocking: Broadcast Record Support"), !e || o.isBroadcastBlocking()
          }).then(function(e) {
            return e && te.info("isAnythingBlocking: Broadcast"), !!e || o.isRecordBlocking()
          }).then(function(e) {
            return e && te.info("isAnythingBlocking: Record"), !!e || o.isCoplayBlocking()
          }).then(function(e) {
            return e && te.info("isAnythingBlocking: Coplay"), !!e || o.isHDRBlocking()
          }).then(function(e) {
            return e && te.info("isAnythingBlocking: HDR"), !!e
          }).then(null, function(e) {
            return te.error("isAnythingBlocking error: ", e), !0
          })
        }, X.tryStartBroadcast = function(t) {
          if (f.onlineState && f.onlineState.online === !1) return te.info("No Internet connection"), void g.show(
            "l10n.systemRequirement", "l10n.notificationCoplayNetworkUnavailable");
          var n = !0;
          return X.isAnythingBlocking().then(function(e) {
            return e ? (te.info("isAnythingBlocking to start this session: ", e), n = !1, !1) : o
              .shouldWeAskUserToTurnOnPrivacyControl()
          }).then(function(e) {
            if (e === !0) {
              var i = {
                  title: "l10n.broadcastLive",
                  icon: "icon-broadcast",
                  question: "l10n.captureBroadcasting",
                  footnote: "l10n.undoPrivacySettings",
                  topButton: "l10n.yes",
                  bottomButton: "l10n.no",
                  topAction: X.enableDTandStartBroadcasting,
                  bottomAction: "",
                  closeOSC: !1,
                  lastState: t ? "" : "main.main-menu"
                },
                o = "main.confirmation";
              v.openOSC(o, i)
            } else n === !0 && L()
          }, function(t) {
            return te.info("tryStartBroadcast: Desktop Record not supported on MSHybrids"), e.reject(!1)
          })
        }, X.enableDTandStartBroadcasting = function() {
          return o.setDesktopCaptureEnabled(!0).then(function() {
            L()
          }, function(e) {
            te.error("setting privacy control enabled: ", e)
          })
        }, X.startBroadcast = function(t) {
          if (te.info("startBroadcast (portal): ", t.portal.name), this._currentPortalName = de = t.portal.name, Z =
            l.endpoints[t.portal.providerName], null === Z) return e.reject();
          o.isBroadcastStartActive = !0, le = null, ee = 0, se = !1;
          var n = null,
            i = null,
            r = 1935;
          t.portal.id;
          return X._broadcastState = E.ACTIVE, n = S.BROADCAST_STARTED, xe.customOverlayState = O.off, X
            .maxViewerCount = 0, j(!0), o.getMicMode().then(function(e) {
              t.portal.providerName === l.providerNames.TWITCH ? y.push(T.OSC_BROADCAST_TWITCH, "mic: " + e) : t
                .portal.providerName === l.providerNames.GOOGLE ? y.push(T.OSC_BROADCAST_YOUTUBE, "mic: " + e) :
                r = 80, xe.micMode = e
            }), h.show(n, de), x.trigger(_.STATUS_CHANGE_BROADCAST), oe = !1, Z.getRtmpUrl(t).then(function(e) {
              return i = e, i && i.includes("rtmps") && t.portal.providerName === l.providerNames.FACEBOOK && (r =
                443), te.info("success " + i), te.info("rtmp port:" + r), d.setBroadcastSessionParamV1({
                type: "live"
              }, {
                sessionUrl: i,
                provider: t.portal.id,
                port: r
              })
            }).then(function() {
              return te.info("start broadcast"), d.setBroadcastSessionStatus({}, {
                status: !0
              })
            }).then(function() {
              Z.postRtmpProcess().then(function() {
                te.info("postRtmpProcess completed"), X.stopBroadcast()
              }), x.on(_.BROADCAST_DISPLAY_VIEWER_COUNT, K), d.getIndicatorOverlaySettings({
                id: o.IndicatorOverlayEnum.INDICATOR_OVERLAY_VIEWER
              }).then(function(e) {
                J = e.data.enable, te.info("Viewers overlay enable : ", J), X.startUpdateViewerCountInterval()
              }), X.setDescription()
            }, function(e) {
              if (te.info("broadcast start error:", e), X._broadcastState = E.STOPPED, h.show(S.BROADCAST_FAILED,
                  t.portal.title), x.trigger(_.STATUS_CHANGE_BROADCAST), X.isBroadcastActive().then(function(e) {
                  e && d.setBroadcastSessionStatus({}, {
                    status: !1
                  })
                }), o.isBroadcastStartActive = !1, y.push(T.OSC_BROADCAST_ERROR, {
                  provider: Z.name,
                  error: (0, a.default)(e)
                }), null != e) {
                try {
                  if (!s.isUndefined(e.data.error.errors[0].reason) && !s.isNull(e.data.error.errors[0].reason))
                    return void(0 === e.data.error.errors[0].reason.localeCompare("liveStreamingNotEnabled") && h
                      .show(S.BROADCAST_YOUTUBE_LIVE_STREAMING))
                } catch (e) {}
                if (!s.isUndefined(e.statusText) && !s.isNull(e.statusText)) return void(0 === e.statusText
                  .localeCompare("Bad Request") && h.show(S.BROADCAST_LOGIN, t.portal.title))
              }
            })
        }, X.stopBroadcast = function() {
          var t = null,
            i = null,
            r = Z.name === l.providerNames.TWITCH,
            a = Z.name === l.providerNames.GOOGLE,
            s = Z.name === l.providerNames.FACEBOOK;
          y.startPerf(C.providerStop), o.isBroadcastStartActive || X.isBroadcastActive().then(function(e) {
            if (e && !oe) return ie === !0 ? (te.info("Broadcast is in pause state.Resume before stop"), re = !
              0, X.resumeBroadcast().then(function(e) {
                if (e === !1) return te.info("Stop broadcast after resume is success"), oe = !0, d
                  .setBroadcastSessionStatus({}, {
                    status: !1
                  })
              }, function(e) {
                te.error("failed to resume broadcast prior stop")
              })) : (te.info("broadcast not in paused state.Stop directly"), oe = !0, d
              .setBroadcastSessionStatus({}, {
                status: !1
              }))
          }).then(function() {
            y.endPerf(C.providerStop, Z.name), "streamerBroadcastStop" === le ? (i = S
                .BROADCAST_STREAMER_STOPPED, le = null) : s && "broadcastStopTimeout" === le ? (i = S
                .BROADCAST_ENDED_TIMEOUT, le = null) : i = S.BROADCAST_STOPPED, X._broadcastState = E.STOPPED, h
              .show(i, de), x.trigger(_.STATUS_CHANGE_BROADCAST), Z.stopBroadcast(), angular.isDefined(Q) && n
              .cancel(Q), J = !1, x.off(_.BROADCAST_DISPLAY_VIEWER_COUNT, K), xe.provider = Z.name, o
              .getModsActiveStatus() && (xe.freestyleActive = P.yes), X.overlayCount = 0;
            for (var l = [], d = 1; d <= 3; d++) l.push(X.getMultipleCustomOverlayEnabled(d).then(function(e) {
              e && X.overlayCount++
            }, function() {}));
            e.all(l).then(function() {
              t = " CustomOverlayCount:" + X.overlayCount, r ? y.push(T.OSC_BROADCAST_TWITCH_CUSTOM_OVERLAY,
                t) : a ? y.push(T.OSC_BROADCAST_YOUTUBE_CUSTOM_OVERLAY, t) : s && y.push(T
                .OSC_BROADCAST_FACEBOOK_CUSTOM_OVERLAY, t), X.overlayCount > 0 && (xe.customOverlayState =
                O.on), X.maxViewerCount = 0, o.addOverlayTelemetryData(xe, !0), y.push(T
                .OSC_BROADCAST_END, xe)
            }), r ? y.push(T.OSC_BROADCAST_TWITCH_VIEWER_COUNT) : a && y.push(T
              .OSC_BROADCAST_YOUTUBE_VIEWER_COUNT)
          })
        }, X.setCommentsOverlayEnabled = function(e) {
          xe.commentsOn = e ? R.TRUE : R.FALSE
        }, X.pauseBroadcast = function() {
          return y.startPerf(C.providerPause), d.pauseBroadcastSession({}, {
            status: !0
          }).then(function() {
            return y.endPerf(C.providerPause, Z.name), ie = !0
          })
        }, X.resumeBroadcast = function() {
          return y.startPerf(C.providerResume), d.pauseBroadcastSession({}, {
            status: !1
          }).then(function() {
            return y.endPerf(C.providerResume, Z.name), ie = !1
          })
        };
        var ve = 0,
          ye = 0,
          we = !1;
        X.getBRRecordTime = function() {
            return F(ve, we)
          }, X.getViewerCount = function() {
            s.isNull(Z) || Z.getCurrentViewerCount().then(function(e) {
              null != e && (J === !0 && e != ee && (X.updateViewerCountImage(e), ee = e), X.maxViewerCount < e &&
                (X.maxViewerCount = e, d.setBroadcastViewerCountMax({}, {
                  count: X.maxViewerCount
                })))
            })
          }, X.startUpdateViewerCountInterval = function() {
            J === !0 && (X.updateViewerCountImage(0), ee = 0), Q = n(function() {
              X.getViewerCount()
            }, 1e4)
          }, X.updateViewerCountImage = function(e) {
            if (x.trigger(_.VIEWER_COUNT_UPDATE, e), !D.osd || !Z || Z.name !== l.providerNames.FACEBOOK) {
              var t = document.createElement("CANVAS"),
                n = t.getContext("2d"),
                i = new Image,
                o = new Image,
                r = new Image,
                a = e.toString();
              i.src = u.default, n.clearRect(0, 0, t.width, t.height), i.onload = function() {
                n.drawImage(i, 0, 0), o.src = m.default
              }, o.onload = function() {
                n.drawImage(o, i.width, 0, 10 * a.length, o.height), n.font = "12pt Segoe", n.fillStyle = "white", n
                  .fillText(a, 35, 20), r.src = p.default
              }, r.onload = function() {
                n.drawImage(r, 10 * a.length + i.width, 0);
                var e = 10 * a.length + i.width + r.width,
                  t = n.getImageData(0, 0, e, i.height),
                  o = new Blob([t.data]);
                te.info("creating canvas: ", o.size, " type:", o.type);
                var l = new FileReader;
                l.addEventListener("loadend", function() {
                  d.updateBroadcastViewerCountImage({}, {
                    ViewerCountImage: String.fromCharCode.apply(null, new Uint8Array(l.result)),
                    ImageHeight: i.height,
                    ImageWidth: 10 * a.length + i.width + r.width
                  })
                }), l.readAsArrayBuffer(o)
              }
            }
          }, X.broadcastState = null, X.getBroadcastSessionStatus = function() {
            return d.getBroadcastSessionStatus().then(function(e) {
              return X.broadcastState = e.data.status, te.info("Get Broadcast Session status: ", X
                .broadcastState), X.broadcastState
            })
          }, X.setDescription = function() {
            c.getSystemDescription().then(function(e) {
              s.isNull(Z) || Z.setBroadcastDescription(e)
            }, function(e) {
              te.error("Failed to get Hardware Information from NvBackend ")
            })
          }, X.getBroadcastState = function() {
            return X._broadcastState
          }, X.getBroadcastEndpointName = function() {
            return Z ? Z.name : null
          }, X.updateBroadcastTitle = function(t) {
            return Z ? Z.setBroadcastTitle(t, he).then(function() {
              Z.name === l.providerNames.GOOGLE && X.setDescription()
            }) : e.when(!1)
          }, X.broadcastPreference = null, X.getBroadcastPreference = function() {
            return X.broadcastPreference ? e.when(X.broadcastPreference) : d.getBroadcastProvider().then(function(e) {
              return e.data.provider
            })
          }, X.setBroadcastPreference = function(t) {
            return X.broadcastPreference === t ? e.when(X.broadcastPreference) : d.setBroadcastProvider({}, {
              provider: t
            }).then(function(e) {
              return X.broadcastPreference = t, X.broadcastPreference
            })
          }, X.CustomOverlaySupportType = void 0, X.getCustomOverlaySupportType = function() {
            return void 0 !== X.CustomOverlaySupportType ? e.when(X.CustomOverlaySupportType) : d
              .getCustomOverlaySupportType().then(function(e) {
                var t = s.findIndex(X.CustomOverlaySupportTypePreference, {
                  name: e.data.support
                });
                return X.CustomOverlaySupportType = X.CustomOverlaySupportTypePreference[t].id, X
                  .CustomOverlaySupportType
              })
          }, X.isCustomOverlayEnabled = void 0, X.getCustomOverlayEnabled = function() {
            return void 0 !== X.isCustomOverlayEnabled ? e.when(X.isCustomOverlayEnabled) : d
            .getCustomOverlayEnabled().then(function(e) {
              return X.isCustomOverlayEnabled = e.data.enable, X.isCustomOverlayEnabled
            })
          }, X.isMultipleCustomOverlayEnabled = [void 0, void 0, void 0], X.getMultipleCustomOverlayEnabled =
          function(t) {
            return void 0 !== X.isMultipleCustomOverlayEnabled[t - 1] ? e.when(X.isMultipleCustomOverlayEnabled[t -
              1]) : d.getCustomOverlayEnabledV1({
              index: t
            }).then(function(e) {
              return X.isMultipleCustomOverlayEnabled[t - 1] = e.data.enable, X.isMultipleCustomOverlayEnabled[t -
                1]
            })
          }, X.setCustomOverlayEnabled = function(t) {
            return X.isCustomOverlayEnabled === t ? e.when(X.isCustomOverlayEnabled) : d.setCustomOverlayEnabled({}, {
              enable: t
            }).then(function(e) {
              return X.isCustomOverlayEnabled = t, X.isCustomOverlayEnabled
            })
          }, X.setMultipleCustomOverlayEnabled = function(t, n) {
            return X.isMultipleCustomOverlayEnabled[n - 1] === t ? e.when(X.isMultipleCustomOverlayEnabled[n - 1]) : d
              .setCustomOverlayEnabledV1({
                index: n
              }, {
                enable: t
              }).then(function(e) {
                return X.isMultipleCustomOverlayEnabled[n - 1] = t, X.isMultipleCustomOverlayEnabled[n - 1]
              })
          }, X.customOverlayFilePath = null, X.getCustomOverlayFilePath = function() {
            return X.customOverlayFilePath ? e.when(X.customOverlayFilePath) : d.getCustomOverlayPath().then(function(
              e) {
              return X.customOverlayFilePath = e.data.path, X.customOverlayFilePath
            })
          }, X.multipleCustomOverlayFilePath = [null, null, null], X.getMultipleCustomOverlayFilePath = function(t) {
            return X.multipleCustomOverlayFilePath[t - 1] ? e.when(X.multipleCustomOverlayFilePath[t - 1]) : d
              .getCustomOverlayPathV1({
                index: t
              }).then(function(e) {
                return X.multipleCustomOverlayFilePath[t - 1] = e.data.path, X.multipleCustomOverlayFilePath[t - 1]
              })
          }, X.setCustomOverlayFilePath = function(t) {
            return X.customOverlayFilePath === t ? e.when(X.customOverlayFilePath) : d.setCustomOverlayPath({}, {
              path: t
            }).then(function(e) {
              return X.customOverlayFilePath = t, X.customOverlayFilePath
            })
          }, X.lastSelectedFilePath = "", X.setMultipleCustomOverlayFilePath = function(t, n) {
            return X.lastSelectedFilePath = t, X.multipleCustomOverlayFilePath[n - 1] === t ? e.when(X
              .multipleCustomOverlayFilePath[n - 1]) : d.setCustomOverlayPathV1({
              index: n
            }, {
              path: t
            }).then(function(e) {
              return X.multipleCustomOverlayFilePath[n - 1] = t, X.multipleCustomOverlayFilePath[n - 1]
            })
          }, X.isBroadcastActive = function() {
            return X.getBroadcastSessionStatus().then(function(e) {
              return !!e
            })
          }, X.CustomOverlayDisplayState = !1, X.getCustomOverlayDisplayState = function() {
            return d.getCustomOverlayDisplayState().then(function(e) {
              return X.CustomOverlayDisplayState = e.data.display, X.CustomOverlayDisplayState
            })
          }, X.getBroadcastPortalFromName = function(e) {
            var t = s.findWhere(X.broadcastPortalPreferences, {
              portalIdentifier: e
            });
            return t = t || X.broadcastPortalPreferences[0]
          }, X.setupAndTriggerBroadcast = function(e, t) {
            var n = e.id;
            if (y.startPerf(C.providerTrigger), n > 0) {
              var i = s.findWhere(X.portals, {
                id: n
              });
              if (l.isLoggedIntoService(i.name) === !1) return void h.show(S.BROADCAST_LOGIN, i.title);
              X.isAnythingBlocking().then(function(e) {
                if (te.info("isAnythingBlocking to start this session (trigger)? : ", e), !e) return o
                  .getCurrentSettingsBR(n).then(function(e) {
                    var n = {};
                    ce = "", n.quality = e.quality, n.resolution = e.resolution.split(" ")[0], n.bitrate = e
                      .bitrateBps, n.framerate = e.framerate, n.portal = i, angular.isDefined(t.title) ? (n
                        .title = t.title, he = !0) : he = !1, n.privacy = t.privacy, n.destinationType = t
                      .destinationType, n.destination = t.destination;
                    var o = X.startBroadcast(n);
                    xe.privacy = n.privacy.id, ce = " Quality:" + e.quality, ce += " Resolution:" + e
                      .resolution, ce += " BitRate:" + e.bitrateBps, ce += " FPS:" + e.framerate, 1 === n
                      .portal.id ? y.push(T.OSC_BROADCAST_TWITCH_QUALITY, ce) : 2 === n.portal.id && y.push(T
                        .OSC_BROADCAST_YOUTUBE_QUALITY, ce), xe.quality = e.quality, xe.resolution = e
                      .resolution, xe.bitRate = e.bitrateBps, xe.fps = e.framerate, xe.provider = Z.name, l
                      .setServiceParameters(t, i.name), o.then(function() {
                        y.endPerf(C.providerTrigger, Z.name)
                      })
                  })
              })
            }
          };
        var Se = !1;
        X.init = function() {
          return te.info("Initialize BroadcastService"), q(), X.portals = l.getServices(l.serviceTypes.STREAMING), s
            .each(X.portals, function(e) {
              e.flex = Math.floor(100 / s.size(X.portals))
            }), X.broadcastPortalPreferences = s.toArray(X.portals).concat([{
              id: 0,
              portalIdentifier: "AlwaysAsk",
              title: "l10n.alwaysAskMe",
              flex: "50"
            }, {
              id: -1,
              portalIdentifier: "DoNotBroadcast",
              title: "l10n.doNotBroadcast",
              flex: "50"
            }]), !0
        }
      }
    ]);
  t.broadcastService = h
}
