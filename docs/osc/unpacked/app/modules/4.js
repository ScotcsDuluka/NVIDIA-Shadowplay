// ─────────────────────────────────────────────────────────────
// APP MODULE 4
// role       : service shadowPlayService
// requires   : (none)
// channels   : /ShadowPlay/v.1.0/InstantReplay/Enable, /ShadowPlay/v.1.0/InstantReplay/Started, /ShadowPlay/v.1.0/InstantReplay/Save, /ShadowPlay/v.1.0/InstantReplay/Upload, /ShadowPlay/v.1.0/Record/Enable, /ShadowPlay/v.1.0/Notification, /ShadowPlay/v.1.0/DisplayOscNotification
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
  }), t.shadowPlayService = void 0;
  var r = n(108),
    a = o(r),
    l = n(8),
    s = o(l),
    d = n(64),
    c = o(d),
    u = n(3),
    f = i(u),
    m = n(2);
  n(23), n(17), n(20), n(11), n(34), n(49), n(93);
  var g = m.ngMainCommonModule.service("shadowPlayService", ["$q", "$log", "$document", "$filter", "$state",
    "shadowPlayEndpoints", "oscNotificationService", "socketService", "cefService", "galleryService",
    "oscGalleryService", "eventAggregator", "keyboardService", "telemetryService", "hardwareService", "$timeout",
    "ugcService", "oscDisplayService", "sdkService", "gameProfileService", "quietMode2Service", "OSC_CONFIG",
    "DEFAULT_VIDEO_TAGS", "NOTIFIER_SELECTIONS", "OSC_MODE", "HOTKEY_EVENTS", "SHADOWPLAY_EVENTS", "WINDOW_EVENTS",
    "COPLAY_STATE", "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_PERF_ID", "TELEMETRY_OSC_CAPTURE_TYPE",
    "NVCAMERA_EVENTS", "TELEMETRY_CAPTURE_ACTION", "TELEMETRY_OVERLAY_LOCATION", "TELEMETRY_OSC_AVAILABLE_STATUS",
    "TELEMETRY_OSC_MENU_TYPE", "NGX_NOTIFICATIONS",
    function(e, t, n, i, o, r, l, d, u, m, g, p, h, b, x, v, y, w, S, E, k, _, T, C, O, A, I, M, R, P, D, N, L, F,
      U, z, G, V) {
      function H() {
        we.shortcuts = {
          saveRecord: {
            hotkeyId: we.HotkeyShortcuts.RECORDSAVE,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          toggleRecord: {
            hotkeyId: we.HotkeyShortcuts.RECORDTOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          toggleBroadcast: {
            hotkeyId: we.HotkeyShortcuts.BROADCASTTOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          toggleBroadcastPause: {
            hotkeyId: we.HotkeyShortcuts.BROADCASTPAUSETOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          toggleCamera: {
            hotkeyId: we.HotkeyShortcuts.CAMERATOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          toggleDVR: {
            hotkeyId: we.HotkeyShortcuts.DVRTOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          screenshot: {
            hotkeyId: we.HotkeyShortcuts.SCREENSHOT,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          anselOpen: {
            hotkeyId: we.HotkeyShortcuts.NVCAMERAUI,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          modsOpen: {
            hotkeyId: we.HotkeyShortcuts.MODSUI,
            hotkeyKeys: {},
            hotkeyStr: ""
          },
          ocToolMenuToggle: {
            hotkeyId: we.HotkeyShortcuts.OCTOOLUITOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          }
        }
      }

      function B(e) {
        var t = z.no;
        if (we.getModsActiveStatus() && (t = z.yes), e === F.saveInstantReplay || e === F.saveManualCapture) {
          var n = {
            action: e,
            freestyleActive: t
          };
          we.addOverlayTelemetryData(n), we.getMicMode().then(function(e) {
            n.micMode = e, b.push(P.OSC_CAPTURE_EVENT, n)
          }, function() {
            n.micMode = "unknown", b.push(P.OSC_CAPTURE_EVENT, n)
          })
        } else b.push(P.OSC_CAPTURE_EVENT, {
          action: e,
          micMode: "unknown",
          fpsOverlayPos: "NA",
          cameraOverlayPos: "NA",
          statusOverlayPos: "NA",
          freestyleActive: t
        })
      }

      function Y() {
        return r.getCaptureControlPIDMode().then(function(e) {
          return Se.info('"Is Capture Control PID mode valid: ', e.data.valid), e.data.valid
        })
      }

      function $() {
        we.isIREnabled().then(function(e) {
          e ? we.stopInstantReplay() : we.startInstantReplay()
        })
      }

      function W(e, t) {
        var n = Math.floor((new Date).getTime() / 1e3),
          i = n - e;
        "ir" === t && i >= Ae && (i = Ae);
        var o = Math.floor(i / 3600),
          r = Math.floor((i - 3600 * o) / 60),
          a = Math.floor(i - 3600 * o - 60 * r);
        return o < 10 && (o = "0" + o), r < 10 && (r = "0" + r), a < 10 && (a = "0" + a), "mr" === t ? o + ":" + r +
          ":" + a : r + ":" + a
      }

      function j(e) {
        b.startPerf(D.uploadScreen), o.go("main.gallery.upload", {
          callback: function(e) {
            b.endPerfAfterDigest(D.uploadScreen, e)
          }
        })
      }

      function K(e, t) {
        return n.on("dragover", function(e) {
          e.stopPropagation(), e.preventDefault(), e.dataTransfer.dropEffect = "copy"
        }), n.on("drop", function(i) {
          if (i.stopPropagation(), i.preventDefault(), !i.dataTransfer.files || !i.dataTransfer.files[0])
          return void Se.info("bad drop, no file list");
          var o = i.dataTransfer.files[0];
          o.name === e ? t(o) : Se.info("bad drag file: " + e + " " + o.name), n.off("dragover"), n.off("drop")
        }), u.oscCreateDropUrl(e, 0, 0)
      }

      function q(e) {
        var t = e.fullFilename.replace(/\\/g, "/");
        K(t, function(t) {
          x.getSystemDescription().then(function(n) {
            var o = [];
            o = T, o.push("#" + e.folder.replace(/\s+/g, ""));
            var r = i("translate")("l10n.uploadVideoMarketingSuffixNew", {
                arg1: e.folder
              }),
              a = {
                file: t,
                filepath: e.fullFilename,
                title: r,
                description: n,
                tags: o,
                categoryId: "20"
              };
            Se.info(a);
            var l = Ee.getPreferredServiceOrDefault(Ee.serviceTypes.VIDEO_UPLOAD);
            Ee.endpoints[l.providerName].uploadVideo(a, l)
          })
        })
      }

      function X(e, t) {
        var n = e.fullFilename,
          i = n.lastIndexOf(".");
        i >= 0 && (n = n.slice(0, i)), n += "-" + Math.floor(t.startMs / 1e3) + "-" + Math.floor(t.durationMs /
          1e3) + ".mp4", we.trimVideo(e.fullFilename, n, t.startMs, t.durationMs).then(function() {
            e.fullFilename = n, g.resetVideoParams(), q(e)
          }, function(e) {
            f.isUndefined(e) || f.isNull(e) || Se.info("Video trim failure: " + e.data), g.resetVideoParams();
            var t = Ee.getPreferredServiceOrDefault(Ee.serviceTypes.VIDEO_UPLOAD);
            l.show(C.UPLOAD_FAILED, t.name)
          })
      }

      function Z(e) {
        var t = e.split(":");
        return Date.UTC(1970, 0, 1, t[0], t[1], t[2])
      }

      function Q(e) {
        return Se.info("uploadFile"), Ie(e, _.autoUploadMs).then(function() {
          if (Se.info("Recording uploading"), _.autoUploadMs > 0) {
            var e = Z(g.fileToProcess.duration);
            if (Se.info(e), e > _.autoUploadMs) {
              var t = {};
              t.startMs = e - _.autoUploadMs, t.durationMs = _.autoUploadMs, t.trimmed = !0, Se.info(t), X(g
                .fileToProcess, t)
            } else q(g.fileToProcess)
          }
          p.off(I.RECORDING_SAVED, Q)
        }, function(e) {
          Se.info("Recording cannot be uploaded"), p.off(I.RECORDING_SAVED, Q), we.handleNodeError(e.data)
        })
      }

      function J() {
        return we.isMRActive().then(function(e) {
          return e ? we.stopAndSaveManualRecord() : we.tryStartManualRecord(!0)
        })
      }

      function ee() {
        if ("ptt" === Me) return we.setMicOnOff("on")
      }

      function te() {
        if ("ptt" === Me) return we.setMicOnOff("off")
      }

      function ne() {
        var e = "off" === Me ? "alwayson" : "off";
        we.setMicMode(e)
      }

      function ie(e, t) {
        t && (t.enable ? we.cachedOverlayPositions[e] = t.position : we.cachedOverlayPositions[e] = U.nowhere)
      }

      function oe(e, t) {
        f.each(we.shortcuts, function(n) {
          if (n.hotkeyId === e) return n.hotkeyKeys = t, n.hotkeyStr = h.shortcutToStr(t), void(n.hotkeyStr =
            "None" === n.hotkeyStr ? "" : n.hotkeyStr)
        })
      }

      function re(e) {
        if (e === _e.OSC_ERR_NO_VALID_GAME_RUNNING && we.DTCaptureSupported === !1 && we.notebookCoProc === !0)
          return "PID" === we.captureState ? l.show(C.WARNING_SUPPORTED_GAME_REQUIRED) : l.show(C
            .WARNING_FULLSCREEN_GAME_REQUIRED)
      }

      function ae() {
        we.getHotkeyShortcut(we.HotkeyShortcuts.NVCAMERAUI).then(function(e) {
          p.trigger(L.CHECK_AND_INIT_ACTION, h.shortcutToStr(e.keys))
        })
      }

      function le(e) {
        e && e.notifier && e.notifier === C.OPEN_SHARE && we.getHotkeyShortcut(we.HotkeyShortcuts.OPENSHARE).then(
          function(t) {
            if (!f.isUndefined(t.keys)) {
              var n = h.shortcutToStr(t.keys);
              l.show(e.notifier, n), k.getStateInfo(!0).then(function(e) {
                Se.info("Quiet Mode2 getStateInfo success: ", e);
                var t = e;
                t.supported && t.enabled ? v(function() {
                  l.flipTo(C.WHISPER_MODE_ENABLED_GAMESTART, t.fanVolumeMode, t.baseFrameRate), v(
                    function() {
                      ae()
                    }, 3500)
                }, 3500) : ae()
              })
            }
          })
      }

      function se(e) {
        e = {}, e.notifier = C.OPEN_SHARE, le(e)
      }

      function de(e) {
        e.status === !0 ? (Se.info("instantReplayNotifier (true): ", e), l.show(C.INSTANT_REPLAY_STARTED), we
          .mainMenuData.instantReplayEnabled = !0, p.trigger(I.STATUS_CHANGE_RECORD), b.endPerf(D.providerTrigger,
            N.instantReplay)) : e.status === !1 ? (Se.info("instantReplayNotifier (false): ", e), l.show(C
            .INSTANT_REPLAY_STOPPED), we.mainMenuData.instantReplayEnabled = !1, p.trigger(I
          .STATUS_CHANGE_RECORD), b.endPerf(D.providerStop, N.instantReplay)) : Se.error(
          "instantReplayNotifier (other): ", e)
      }

      function ce(e) {
        E.updateDRSProfileInfo(), void 0 !== e.started && e.started === !0 || void 0 !== e.restarted && e
          .restarted === !0 ? (Se.info("instantReplayRecordingNotifier (true): ", e), we.mainMenuData
            .instantReplayRunning = !0, p.trigger(I.IR_RECORDING_STATE_CHANGED), p.trigger(I.STATUS_CHANGE_RECORD)
            ) : void 0 !== e.started && e.started === !1 ? (Se.info("instantReplayRecordingNotifier (false): ", e),
            we.mainMenuData.instantReplayRunning = !1, p.trigger(I.IR_RECORDING_STATE_CHANGED), p.trigger(I
              .STATUS_CHANGE_RECORD)) : Se.error("instantReplayRecordingNotifier (other): ", e)
      }

      function ue(e) {
        r.getInstantReplayBufferLength().then(function(t) {
          var n;
          n = _.autoUploadMs > 0 ? _.autoUploadMs / 1e3 : t.data.lengthSeconds;
          var i = Math.floor(n / 60);
          n = Math.floor(n % 60), Se.info("minutes: ", i, " seconds: ", n), l.show(e, i, n)
        })
      }

      function fe(e) {
        ue(C.INSTANT_REPLAY_SAVED)
      }

      function me(e) {
        ue(C.INSTANT_REPLAY_SAVED_TO_GALLERY)
      }

      function ge(e) {
        e.status === !0 ? (Se.info("recordNotifier (true): ", e), l.show(C.RECORD_STARTED), we.mainMenuData
          .manualRecordEnabled = !0, p.trigger(I.STATUS_CHANGE_RECORD), b.endPerf(D.providerTrigger, N
            .manualRecord)) : e.status === !1 ? (Se.info("recordNotifier (false): ", e), l.show(C.RECORD_STOPPED),
          we.mainMenuData.manualRecordEnabled = !1, p.trigger(I.STATUS_CHANGE_RECORD)) : Se.error(
          "recordNotifier (other): ", e)
      }

      function pe(e) {
        if (e.notification === ke.SCREENSHOT) {
          if (Se.info("processNotification: Screenshot; result = ", e.result), we.onScreenShotCapturedCallback)
            return we.onScreenShotCapturedCallback(e), void(we.onScreenShotCapturedCallback = void 0);
          e.result === _e.OSC_SUCCESS ? (l.show(C.SCREENSHOT_SAVED_TO_GALLERY), B(F.takeScreenshot)) : (e.result ===
            _e.OSC_ERR_NO_VALID_GAME_RUNNING ? we.tryStartScreenshot() : re(e.result), b.push(P
              .OSC_SHADOWPLAY_CAPTURE_ERROR, {
                action: F.takeScreenshot,
                errorDetail: (0, s.default)(e.result)
              }))
        } else if (e.notification === ke.RECORDING_SAVED) Se.info("processNotification: Recording saved; result = ",
          e.result), e.result === _e.OSC_SUCCESS ? (p.trigger(I.RECORDING_SAVED, e.file), we.expectIRSave ? (we
          .expectIRSave = !1, b.endPerf(D.providerPause, N.instantReplay)) : (we.expectManualSave || b
          .startPerf(D.providerStop), we.expectManualSave = !1, b.endPerf(D.providerStop, N.manualRecord))) : (e
          .result !== _e.OSC_ERR_NO_VALID_GAME_RUNNING && re(e.result), b.push(P.OSC_SHADOWPLAY_CAPTURE_ERROR, {
            action: we.expectIRSave ? F.saveInstantReplay : F.saveManualCapture,
            errorDetail: (0, s.default)(e.result)
          }), we.expectIRSave = !1);
        else if (e.notification === ke.HIGHLIGHT_SESSION) p.trigger(I.HIGHLIGHTS_STATUS_CHANGE, e.active);
        else if (e.notification === ke.GAME_STARTED) p.trigger(I.GAME_STARTED, e);
        else if (e.notification === ke.GAME_EXITED) p.trigger(I.GAME_EXITED, e);
        else if (e.notification === V.AI_SUPER_RES_STARTED) we.onNGXShotCapturedCallback && we
          .onNGXShotCapturedCallback(e);
        else if (e.notification === V.AI_SUPER_RES_DONE || e.notification === V.AI_SUPER_RES_FAILED) {
          if (we.onNGXShotCapturedCallback) return we.onNGXShotCapturedCallback(e), void(we
            .onNGXShotCapturedCallback = void 0)
        } else e.notification === V.AI_SUPER_RES_PROGRESS ? void 0 !== e.progress && we.onNGXShotCapturedCallback &&
          we.onNGXShotCapturedCallback(e) : e.notification === ke.RECORD_HDR_ERROR ? l.show(C.HDR_ERROR_RECORD) : e
          .notification === ke.BROADCAST_HDR_ERROR ? l.show(C.HDR_ERROR_BROADCAST) : e.notification === ke
          .SCREENSHOT_HDR_ERROR ? p.trigger(I.HDR_SCREENSHOT) : e.notification === ke.HL_HDR_ERROR ? l.show(C
            .HDR_ERROR_HL) : Se.error("processNotification: Notification = " + e.notification + "; result = " + e
            .result)
      }

      function he(e) {
        switch (e) {
          case we.HotkeyShortcuts.NVCAMERAUI:
            return G.ansel;
          case we.HotkeyShortcuts.MODSUI:
          case we.HotkeyShortcuts.MODSTOGGLE:
          case we.HotkeyShortcuts.MODSPRESETCYCLE:
            return G.freestyle;
          default:
            return G.mainMenu
        }
      }

      function be(e) {
        return we.getHotkeyShortcut(e.hotkeyId).then(function(t) {
          e.hotkeyKeys = t.keys, f.isUndefined(t.keys) ? e.hotkeyStr = "" : (e.hotkeyStr = h.shortcutToStr(t
            .keys), e.hotkeyStr = "None" === e.hotkeyStr ? "" : e.hotkeyStr)
        }, function(t) {
          Se.error("ERROR in retrieving shortcut " + e.hotkeyId), e.hotkeyKeys = "", e.hotkeyStr = ""
        })
      }

      function xe() {
        var t = [];
        return f.each(we.shortcuts, function(e) {
          t.push(be(e))
        }), e.all(t)
      }

      function ve() {
        var e = "/ShadowPlay/v.1.0/InstantReplay/Enable",
          t = "/ShadowPlay/v.1.0/InstantReplay/Started",
          n = "/ShadowPlay/v.1.0/InstantReplay/Save",
          i = "/ShadowPlay/v.1.0/InstantReplay/Upload",
          o = "/ShadowPlay/v.1.0/Record/Enable",
          r = "/ShadowPlay/v.1.0/Notification",
          a = "/ShadowPlay/v.1.0/DisplayOscNotification",
          l = "instantReplayNotifier",
          s = "instantReplayRecordingNotifier",
          c = "instantReplaySaveNotifier",
          u = "instantReplayUploadNotifier",
          f = "recordNotifier",
          m = "processNotification",
          g = "processShowNotification";
        Se.info("Registering shadowplay notification events"), d.register(e, l), d.register(t, s), d.register(n, c),
          d.register(i, u), d.register(o, f), d.register(r, m), d.register(a, g), p.on(l, de), p.on(s, ce), p.on(c,
            fe), p.on(u, me), p.on(f, ge), p.on(m, pe), p.on(g, le), p.on(I.FILE_READY_TO_UPLOAD, j), p.on(A.MANUAL,
            J), _.autoUploadMs > 0 ? p.on(A.DVR, we.uploadInstantReplay) : p.on(A.DVR, we.saveInstantReplay), p.on(A
            .CAMERA, we.toggleWebcam), p.on(A.MIC_PTT_DOWN, ee), p.on(A.MIC_PTT_UP, te), p.on(A.MIC_TOGGLE, ne), p
          .on(A.DVR_TOGGLE, $), p.on(A.SCREENSHOT, we.checkAndCaptureScreenshot), p.on(M.DISPLAY_HOTKEY, se)
      }
      var ye, we = this,
        Se = t.getInstance("osc/shadowplayService"),
        Ee = y.connectService;
      we.isBroadcastStartActive = !1, we.expectIRSave = !1, we.expectManualSave = !1, we
        .cachedOverlayPositions = {}, we.twitchPreferredIngestServer = null;
      var ke = (ye = {
          SCREENSHOT: "screenshot",
          SP_RESTART: "shadowplayrestart",
          RECORDING_SAVED: "recordingSaved",
          HIGHLIGHT_SESSION: "highlightSession",
          GAME_STARTED: "gameAppStarted",
          GAME_EXITED: "gameAppExit"
        }, (0, c.default)(ye, "GAME_STARTED", "gameAppStarted"), (0, c.default)(ye, "SCREENSHOT_HDR_ERROR",
          "screenshotHDRError"), (0, c.default)(ye, "RECORD_HDR_ERROR", "recordHDRError"), (0, c.default)(ye,
          "BROADCAST_HDR_ERROR", "broadcastHDRError"), (0, c.default)(ye, "HL_HDR_ERROR", "hlHDRError"), ye),
        _e = {
          OSC_SUCCESS: 0,
          ET_NO_ERROR: 1,
          ET_GENERIC: 2,
          ET_INVALID_ARGUMENT: 3,
          ET_INVALID_DATA: 4,
          ET_INTERFACE_UNAVAILABLE: 5,
          OSC_ERR_GENERIC: -1,
          OSC_ERR_INVALID_VER: -2,
          OSC_ERR_CLIENT_UNINITIALIZED: -3,
          OSC_ERR_SERVER_NOT_CONNECTED: -4,
          OSC_ERR_SERVER_TIME_OUT: -5,
          OSC_ERR_SERVER_CANNOT_CONNECT: -6,
          OSC_ERR_STREAM_NOT_FOUND: -7,
          OSC_ERR_INVALID_FORMAT: -8,
          OSC_ERR_INVALID_PARAMETERS: -9,
          OSC_ERR_DX: -10,
          OSC_ERR_MMF: -11,
          OSC_ERR_INSUFFICIENT_BUFFER: -12,
          OSC_ERR_NO_IMPLEMENTATION: -13,
          OSC_ERR_STREAM_LOCKED: -14,
          OSC_ERR_ALREADY_REGISTERED: -15,
          OSC_ERR_ALREADY_CREATED: -16,
          OSC_ERR_PROTOBUF: -17,
          OSC_ERR_NOTAVAILABLE: -18,
          OSC_ERR_OUT_OF_MEMORY: -19,
          OSC_ERR_ABANDONED: -20,
          OSC_ERR_INVALID_CALL: -21,
          OSC_ERR_NO_VALID_GAME_RUNNING: -22
        };
      we.SHADOWPLAY_ERRORCODES = _e, we.getPreferredTwitchIngestServer = function() {
        return we.twitchPreferredIngestServer ? e.when(we.twitchPreferredIngestServer) : r
          .getBroadcastIngestServer().then(function(e) {
            return e.data.ingestserver
          })
      }, we.setPreferredTwitchIngestServer = function(t) {
        return we.twitchPreferredIngestServer === t ? e.when(we.twitchPreferredIngestServer) : r
          .setBroadcastIngestServer({}, {
            ingestserver: t
          }).then(function() {
            we.twitchPreferredIngestServer = t
          })
      }, we.run = function() {
        return r.launch({}, {
          launch: !0
        }).then(function(e) {
          Se.info("ShadowPlay started successfully")
        })
      }, we.shutdown = function() {
        return r.launch({}, {
          launch: !1
        }).then(function(e) {
          Se.info("ShadowPlay shutdown success")
        })
      }, we.spRunning = null, we.isSPRunning = function() {
        return we.spRunning ? e.when(we.spRunning) : r.isRunning().then(function(e) {
          return we.spRunning = e.data.launch, Se.info("Is ShadowPlay running: ", we.spRunning), we.spRunning
        })
      }, we.isIREnabled = function() {
        return r.getIREnableStatus().then(function(e) {
          return Se.info("InstantReplay Enable Status: ", e.data.status), we.mainMenuData
            .instantReplayEnabled = e.data.status, e.data.status
        })
      }, we.isIRActive = function() {
        return r.getIRRunningStatus().then(function(e) {
          return Se.info("InstantReplay Running Status: ", e.data.running), we.mainMenuData
            .instantReplayRunning = e.data.status, e.data.running
        })
      }, we.setInstantReplayRecording = function(e) {
        return r.setInstantReplayRecording({}, {
          status: e
        }).then(function(t) {
          return Se.info("Set InstantReplay recording: ", e), e
        })
      }, we.isMREnabled = function() {
        return r.getMREnableStatus().then(function(e) {
          return Se.info("Manual recording Enable Status: ", e.data.status), e.data.status
        })
      }, we.isMRActive = function() {
        return r.getMRRunningStatus().then(function(e) {
          return Se.info("Manual recording Running Status: ", e.data.running), e.data.running
        })
      }, we.setManualRecording = function(e) {
        return r.setManualRecording({}, {
          status: e
        }).then(function(t) {
          return Se.info("Set Manual recording: ", e), e
        })
      }, we.isBroadcastActive = function() {
        return r.getBroadcastSessionStatus().then(function(e) {
          return Se.info("Broadcast Running Status: ", e.data.status), e.data.status
        })
      }, we.isDesktopCaptureSettingShown = function() {
        return r.getDesktopCaptureSupportReason().then(function(e) {
          if (!e.data.support) {
            var t = e.data.unsupportReason;
            if (t.indexOf("notebookDriver") >= 0 || t.indexOf("notebookCoProc") >= 0 || t.indexOf(
                "notebookDGpu") >= 0 || t.indexOf("hideCheckboxAOSP") >= 0) return !1
          }
          return !0
        }, function(e) {
          return Se.info("Desktop capture support reason failure"), !1
        })
      }, we.isBroadcastRecordSupported = function() {
        return we.DTCaptureSupported === !0 ? e.when(!0) : we.notebookCoProc === !0 ? u.isInDesktopMode().then(
          function(e) {
            return Se.info("isInDesktopMode :", e), Se.info("CaptureState :", we.captureState), "PID" === we
              .captureState || !e || (Se.info("For notebooks, record is supported in fullscreen only"), l.show(C
                .WARNING_FULLSCREEN_GAME_REQUIRED), !1)
          }) : e.when(!1)
      }, we.isBroadcastBlocking = function() {
        return r.getRecordBroadcastConcurrencySupport().then(function(e) {
          Se.info("Record and Broadcast Concurrency Support: ", e.data.support);
          var t = e.data.support;
          return !t && we.isBroadcastActive().then(function(e) {
            Se.info("Broadcast is active: ", e);
            var t = e;
            return t === !0 ? (l.show(C.WARNING_BROADCAST_STOP_TO_USE_FEATURE), Se.info(
              "isBroadcastBlocking: broadcast acvtive"), !0) : !!we.isBroadcastStartActive && (Se.info(
              "isBroadcastBlocking: start active"), !0)
          })
        })
      }, we.isHDRBlocking = function() {
        return r.getHDRActiveState().then(function(e) {
          Se.info("HDR Active: ", e.data.active);
          var t = e.data.active;
          return t === !0 && (Se.info("sending disbale hdr notification"), l.show(C.HDR_ERROR_BROADCAST), !0)
        })
      }, we.isRecordBlocking = function() {
        return r.getRecordBroadcastConcurrencySupport().then(function(t) {
          Se.info("Record and Broadcast Concurrency Support: ", t.data.support);
          var n = t.data.support;
          return !n && e.all([we.isIREnabled(), we.isIRActive(), we.isMRActive(), S.isHighlightsActive()])
            .then(function(e) {
              var t = e[0],
                n = e[1],
                i = e[2],
                o = e[3];
              return t || n ? (l.show(C.WARNING_INSTANT_REPLAY_STOP_TO_USE_FEATURE), !0) : i ? (l.show(C
                .WARNING_RECORDING_STOP_TO_USE_FEATURE), !0) : !!o && (we.appCaptureProcessInfo().then(
                function(e) {
                  l.show(C.WARNING_HIGHLIGHTS_STOP_TO_USE_BROADCAST, e.profileName)
                }), !0)
            })
        })
      }, we.isCoplayBlocked = function() {
        return r.getRecordGamestreamConcurrencySupport().then(function(t) {
          var n = t.data.support;
          return !n && e.all([we.isIREnabled(), we.isIRActive(), we.isMRActive(), we.isBroadcastActive()])
            .then(function(e) {
              var t = e[0],
                n = e[1],
                i = e[2],
                o = e[3];
              return t || n ? (l.show(C.WARNING_INSTANT_REPLAY_STOP_TO_USE_FEATURE), !0) : i ? (l.show(C
                .WARNING_RECORDING_STOP_TO_USE_FEATURE), !0) : !(!o && !we.isBroadcastStartActive) && (l
                .show(C.WARNING_BROADCAST_STOP_TO_USE_FEATURE), !0)
            })
        })
      };
      var Te = R.OFF;
      we.setCoplayState = function(e) {
        Te = e
      }, we.isCoplayBlocking = function() {
        return r.getRecordGamestreamConcurrencySupport().then(function(e) {
          Se.info("Record and Gamestream Concurrency Support: ", e.data.support);
          var t = e.data.support;
          return !t && (Te !== R.OFF && (l.show(C.WARNING_COPLAY_STOP_TO_USE_FEATURE), !0))
        })
      }, we.getDesktopCaptureEnabled = function() {
        return r.getDesktopCaptureEnabled().then(function(e) {
          return Se.info("Is Desktop Capture Enabled: ", e.data.enable), e.data.enable
        }, function(e) {
          return Se.error("ERROR, Desktop Capture Enabled request failed: ", e), !1
        })
      }, we.setDesktopCaptureEnabled = function(e) {
        return r.setDesktopCaptureEnabled({}, {
          enable: e
        }).then(function(t) {
          Se.info("Set Desktop Capture Enabled: ", e)
        }, function(e) {
          Se.error("ERROR, Desktop Capture Enabled request failed: ", e)
        })
      }, we.getDesktopCaptureSupported = function() {
        return r.getDesktopCaptureSupported().then(function(e) {
          return Se.info("Get Desktop Capture Supported: ", e.data), e.data.support
        }, function(e) {
          return Se.error("ERROR, Desktop Capture Supported request failed: ", e), !1
        })
      }, we.getCoplayEnabled = function() {
        return r.getCoplayEnabled().then(function(e) {
          return Se.info("Get Coplay Enabled: ", e.data.enable), e.data.enable
        }, function(e) {
          return Se.error("ERROR, Coplay Enabled request failed: ", e), !1
        })
      }, we.setCoplayEnabled = function(e) {
        return r.setCoplayEnabled({}, {
          enable: e
        }).then(function(t) {
          Se.info("Set Coplay Enabled: ", e), we.mainMenuData.coplayEnabled = e, p.trigger(I
            .COPLAY_ENABLED_CHANGED)
        }, function(e) {
          Se.error("ERROR, Coplay Enabled request failed: ", e)
        })
      }, we.getCoplaySupported = function() {
        return r.getCoplaySupported().then(function(e) {
          return Se.info("Get Coplay Supported: ", e.data.support), e.data.support
        }, function(e) {
          return Se.error("ERROR, Coplay Supported request failed: ", e), !1
        })
      }, we.isWindowedModeAllowed = function() {
        return e.all([u.isInDesktopMode(), we.getDesktopCaptureEnabled()]).then(function(t) {
          var n = t[0],
            i = t[1],
            o = !1;
          return Se.info("DesktopMode, dtCaptureEnabled : ", n, i), !(!n || i) && Y().then(function(t) {
            return o = t, !o && (we.notebookCoProc !== !0 || ("PID" === we.captureState ? (l.show(C
              .WARNING_SUPPORTED_GAME_REQUIRED), Se.info(
              "isWindowedModeAllowed: supported game required")) : (l.show(C
              .WARNING_FULLSCREEN_GAME_REQUIRED), Se.info(
              "isWindowedModeAllowed: fullscreen game required")), e.reject(!1)))
          })
        })
      }, we.startInstantReplay = function() {
        return B(F.turnOnInstantReplay), b.startPerf(D.providerTrigger), e.all([we.isBroadcastRecordSupported(),
          we.isBroadcastBlocking(), we.isCoplayBlocking()
        ]).then(function(e) {
          var t = e[0],
            n = e[1] || we.isBroadcastStartActive,
            i = e[2];
          if (t && !n && !i) return u.isInDesktopMode().then(function(e) {
            e ? we.telemetryDVRFSMode = P.OSC_CAPTURE_DVR_SESSION_DURATION_DT : we.telemetryDVRFSMode =
              P.OSC_CAPTURE_DVR_SESSION_DURATION_FS
          }), we.setInstantReplayRecording(!0).then(function() {
            Se.info("Instant Replay Started")
          }).then(null, function(e) {
            Se.info("Instant Replay cannot be started"), we.handleNodeError(e.data)
          })
        })
      }, we.stopInstantReplay = function() {
        return B(F.turnOffInstantReplay), b.startPerf(D.providerStop), we.setInstantReplayRecording(!1).then(
          function() {
            Se.info("Instant Replay Stopped")
          }).then(null, function(e) {
          Se.info("Instant Replay cannot be stopped"), we.handleNodeError(e.data)
        })
      }, we.saveInstantReplay = function() {
        return we.expectIRSave = !0, B(F.saveInstantReplay), b.startPerf(D.providerPause), r.saveInstantReplay()
          .then(function(e) {
            return Se.info("Save Instant Replay recording: ", e.data), e.data.status
          }).then(null, function(e) {
            Se.info("Instant Replay recording cannot be saved"), we.handleNodeError(e.data)
          })
      };
      var Ce = 0,
        Oe = 0,
        Ae = 15;
      we.getIRRecordTime = function() {
        return W(Ce, "ir")
      }, we.waitForIRStartToComplete = function() {
        return 0 !== Ce
      }, we.getMRRecordTime = function() {
        return W(Oe, "mr")
      }, we.usersDirectory = null, we.getusersDirectory = function() {
        return we.usersDirectory ? e.when(we.usersDirectory) : r.getCustomOverlayDefaultPath().then(function(e) {
          return we.usersDirectory = e.data.defaultPath, we.usersDirectory
        })
      };
      var Ie = function(e, t) {
        var n = e.replace(/\\/g, "/"),
          i = n.replace(/\/$/, "").split("/"),
          o = i[i.length - 2],
          r = i[i.length - 1];
        Se.info(o, r);
        var a = [],
          l = {
            name: r,
            type: "video",
            subtype: ""
          };
        return m.getMetaData(e, o, l, 0, a, !1, !0).then(function(e) {
          return void 0 !== a[0] ? (g.setFileToProcess(a[0]), g.setParentGalleryState(!1), 0 === t && p
              .trigger(I.FILE_READY_TO_UPLOAD, Ee.isUploadTargetServiceKnown(g.fileToProcess.file.type)), !0
              ) : (Se.error("uploadRecordedFile getMetaData failed!"), !1)
        })
      };
      we.uploadInstantReplay = function() {
        return p.on(I.RECORDING_SAVED, Q), we.saveInstantReplay().then({}, function(e) {
          Se.info("Instant Replay recording cannot be uploaded"), p.off(I.RECORDING_SAVED, Q), we
            .handleNodeError(e.data)
        })
      }, we.uploadManualRecord = function() {
        return p.on(I.RECORDING_SAVED, Q), we.stopAndSaveManualRecord().then({}, function(e) {
          Se.info("Manual recording cannot be uploaded"), p.off(I.RECORDING_SAVED, Q), we.handleNodeError(e
            .data)
        })
      }, we.startManualRecord = function() {
        return B(F.startManualCapture), b.startPerf(D.providerTrigger), e.all([we.isBroadcastRecordSupported(), we
          .isBroadcastBlocking(), we.isCoplayBlocking()
        ]).then(function(e) {
          var t = e[0],
            n = e[1] || we.isBroadcastStartActive,
            i = e[2];
          if (t && !n && !i) return we.setManualRecording(!0).then(function(e) {
            Se.info("Manual Record started")
          }).then(null, function(e) {
            Se.error("Manual Record cannot be started"), we.handleNodeError(e.data), b.push(P
              .OSC_SHADOWPLAY_CAPTURE_ERROR, {
                action: F.startManualCapture,
                errorDetail: (0, s.default)(e.data)
              })
          })
        })
      }, we.stopAndSaveManualRecord = function() {
        return we.expectManualSave = !0, B(F.saveManualCapture), b.startPerf(D.providerStop), we
          .setManualRecording(!1).then(function() {
            Se.info("Manual Record saved and stopped")
          }).then(null, function(e) {
            Se.info("Manual Record cannot be stopped"), we.handleNodeError(e.data), b.push(P
              .OSC_SHADOWPLAY_CAPTURE_ERROR, {
                action: F.saveManualCapture,
                errorDetail: (0, s.default)(e.data)
              })
          })
      }, we.checkCustomize = function(t) {
        return e.all([we.isBroadcastActive(), we.isIRActive(), we.isIREnabled(), we.isMRActive(), S
          .isHighlightsActive()
        ]).then(function(e) {
          return we.runState = {
              broadcast: e[0],
              instantReplay: e[1] || e[2],
              manualRecord: e[3],
              highlights: e[4]
            }, we.runState.broadcast === !0 ? (t === !0 && l.show(C.WARNING_BROADCAST_STOP_TO_CUSTOMIZE), !
            1) : we.runState.instantReplay === !0 || we.runState.manualRecord === !0 ? (t === !0 && l.show(C
              .WARNING_RECORDING_STOP_TO_CUSTOMIZE), !1) : we.runState.highlights !== !0 || (t === !0 && we
              .appCaptureProcessInfo().then(function(e) {
                l.show(C.WARNING_HIGHLIGHTS_STOP_TO_CUSTOMIZE, e.profileName)
              }), !1)
        })
      }, we.getDefaultResolution = function(e) {
        return r.getDefaultResolution({
          quality: e
        }).then(function(t) {
          return Se.info("Default Resolution for : " + e, t.data.resolution), t.data.resolution
        })
      }, we.getDefaultFramerate = function(e) {
        return r.getDefaultFramerate({
          quality: e
        }).then(function(t) {
          return Se.info("Default Framerate for : " + e, t.data.framerate), t.data.framerate
        })
      }, we.getBitrateRange = function(e, t) {
        return r.getBitrateRange({
          quality: e,
          resolution: t
        }).then(function(t) {
          return Se.info("Bitrate Range for : " + e, t.data), t.data
        })
      }, we.getSupportedResolutions = function() {
        return r.getSupportedResolutions().then(function(e) {
          return Se.info("Supported Resolutions : ", e.data.resolutions), e.data.resolutions
        })
      }, we.getSupportedFramerates = function() {
        return r.getSupportedFramerates().then(function(e) {
          return Se.info("Supported Framerates : ", e.data.framerates), e.data.framerates
        })
      }, we.get4KSupport = function() {
        return r.get4KSupport().then(function(e) {
          return Se.info("Supported  : ", e.data.support), e.data.support
        })
      }, we.get8K60Support = function() {
        return Se.info("get8K60Support :", we.supported8K60), we.supported8K60
      }, we.getCurrentSettingsIR = function() {
        return r.getInstantReplaySettings().then(function(e) {
          return Se.info("Current IR Settings : ", e.data), e.data
        })
      }, we.getCurrentSettingsMR = function() {
        return r.getManualRecordSettings().then(function(e) {
          return Se.info("Current Manual Settings : ", e.data), e.data
        })
      }, we.getCurrentSettingsBR = function(e) {
        return r.setBroadcastSessionParamV1({
          type: "config"
        }, {
          provider: e
        }).then(function() {
          return r.getBroadcastSettings().then(function(e) {
            return Se.info("Current Broadcast Settings : ", e.data), e.data
          })
        })
      }, we.setQualitySettings = function(e, t, n) {
        if ("Custom" !== e) {
          if (t === O.INSTANTREPLAY) return r.setInstantReplayPresetSettings({}, {
            replayLengthSeconds: n.replayLengthSeconds,
            quality: n.quality
          }).then(function() {
            Se.info("Set IR Settings for: " + e)
          });
          if (t === O.MANUALRECORD) return r.setManualRecordPresetSettings({}, {
            quality: n.quality
          }).then(function() {
            Se.info("Set ManualRecord Settings for: " + e)
          });
          if (t === O.BROADCAST) return r.setBroadcastPresetSettings({}, {
            quality: n.quality,
            provider: n.provider
          }).then(function() {
            Se.info("Set Broadcast Settings for portal: " + n.provider + "quality: " + e)
          })
        } else {
          if (t === O.INSTANTREPLAY) return r.setInstantReplaySettings({}, {
            replayLengthSeconds: n.replayLengthSeconds,
            quality: n.quality,
            resolution: n.resolution,
            framerate: n.framerate,
            bitrateBps: n.bitrate
          }).then(function() {
            Se.info("Set Custom IR Settings : ", n)
          });
          if (t === O.MANUALRECORD) return r.setManualRecordSettings({}, {
            quality: n.quality,
            resolution: n.resolution,
            framerate: n.framerate,
            bitrateBps: n.bitrate
          }).then(function() {
            Se.info("Set Custom Manual Record Settings : ", n)
          });
          if (t === O.BROADCAST) return r.setBroadcastSettings({}, {
            provider: n.provider,
            quality: n.quality,
            resolution: n.resolution,
            framerate: n.framerate,
            bitrateBps: n.bitrate
          }).then(function() {
            Se.info("Set Custom Broadcast Settings : ", n)
          })
        }
      }, we.getMicCount = function() {
        return r.getMicrophoneCount().then(function(e) {
          return Se.info("Number of microphones : ", e.data.present), e.data.present
        })
      };
      var Me = "off";
      we.getMicMode = function() {
        return r.getAudioMode().then(function(e) {
          return "both" === e.data.mode ? r.getMicrophoneMode().then(function(e) {
            return Se.info("Microphone mode : ", e.data.mode), Me = e.data.mode, e.data.mode
          }) : (Me = "off", "off")
        }, function(e) {
          return Se.info("error getting mic mode: ", e), "off"
        })
      }, we.setMicMode = function(e) {
        var t = "both";
        return "off" === e && (t = "game"), we.mainMenuData.audioMode = t, r.setAudioMode({}, {
          mode: t
        }).then(function() {
          return r.setMicrophoneMode({}, {
            mode: e
          })
        }).then(function() {
          var t = "off";
          return "alwayson" === e && (t = "on"), r.setMicrophoneOnOff({}, {
            mode: t
          })
        }).then(function() {
          Se.info("Set microphone mode : ", e), Me = e, we.mainMenuData.micMode = e, p.trigger(I
            .MIC_STATUS_CHANGE, Me)
        }, function(t) {
          Se.info("error setting microphone mode: ", t), Me = "off", we.mainMenuData.micMode = e, p.trigger(I
            .MIC_STATUS_CHANGE, Me)
        })
      }, we.setMicOnOff = function(e) {
        return r.setMicrophoneOnOff({}, {
          mode: e
        })
      }, we.getMicrophoneSettings = function() {
        return r.getMicrophoneSelectedSettings().then(function(e) {
          return Se.info("Current microphone : ", e.data), e.data
        })
      }, we.micList = [], we.getMicrophoneIndexSettings = function(e) {
        return r.getMicrophoneSettings({
          index: e
        }).then(function(t) {
          t.data.index === e && we.micList.push(t.data)
        }, function(e) {
          Se.error("Failed to get MicSettings: ", e)
        })
      }, we.getMicrophoneSettingsAll = function() {
        return we.getMicCount().then(function(t) {
          var n = [];
          we.micList.length = 0;
          for (var i = 0; i < t; i++) n.push(we.getMicrophoneIndexSettings(i));
          return e.all(n).then(function() {
            return Se.info("MicList: ", we.micList), we.micList
          })
        })
      }, we.setMicrophoneSettings = function(e) {
        return r.setMicrophoneSettings({
          index: e.index
        }, {
          muted: e.muted,
          volumePercent: e.volumePercent,
          boostPercent: e.boostPercent
        }).then(function(e) {
          Se.info("Microphone updated!")
        }, function(e) {
          Se.error("Error saving mic settings: ", e)
        })
      }, we.audioSettings = {}, we.getAudioSettings = function() {
        return e.when(we.audioSettings)
      }, we.getAudioSettingsInit = function() {
        return r.getAudioSettings().then(function(e) {
          return Se.info("Audio settings: ", e.data), we.audioSettings = e.data, we.audioSettings
        }, function(e) {
          return Se.error("Error getting audio settings: ", e), we.audioSettings.systemVolumePercent = 100, we
            .audioSettings.separateTracks = !1, we.audioSettings
        })
      }, we.setAudioSettings = function(e) {
        return r.setAudioSettings({}, {
          systemVolumePercent: e.systemVolumePercent,
          separateTracks: e.separateTracks
        }).then(function(t) {
          Se.info("Audio settings updated"), we.audioSettings = e
        }, function(e) {
          Se.error("Error saving audio settings: ", e)
        })
      }, we.isMTAAvailCached = !1, we.isMTAAvail = !1, we.isMultiTrackAudioAvailable = function() {
        return we.isMTAAvailCached === !0 ? e.when(we.isMTAAvail) : r.getAudioSettings().then(function() {
          return Se.info("Multi-Track Audio is available"), we.isMTAAvail = !0, we.isMTAAvailCached = !0, we
            .isMTAAvail
        }, function(e) {
          return Se.info("Multi-Track Audio is NOT available"), we.isMTAAvail = !1, we.isMTAAvailCached = !0,
            we.isMTAAvail
        })
      }, we.getAudioState = function() {
        return we.audioState
      }, we.setAudioState = function(e) {
        we.audioState = e
      }, we.getVideoState = function() {
        return we.videoState
      }, we.setVideoState = function(e) {
        we.videoState = e
      }, we.getWebcamPresent = function() {
        return r.getWebcamPresent().then(function(e) {
          return Se.info("Webcam present : ", e.data.present), we.mainMenuData.webcamPresent = e.data.present,
            e.data.present
        })
      }, we.getWebcamShown = function() {
        return r.getWebcamShown().then(function(e) {
          return Se.info("Webcam showing : ", e.data.shown), we.mainMenuData.webcamShown = e.data.shown, e
            .data.shown
        })
      }, we.getWebcamMode = function() {
        return r.getWebcamMode().then(function(e) {
          return Se.info("Webcam mode : ", e.data.status), e.data.status
        })
      }, we.setWebcamMode = function(e) {
        return r.setWebcamMode({}, {
          status: e
        }).then(function() {
          Se.info("Set webcam mode : ", e), p.trigger(I.WEBCAM_STATUS_CHANGE)
        })
      }, we.toggleWebcamDisplay = function() {
        return r.toggleWebcamDisplay({}, {}).then(function(e) {
          Se.info("Toggle Webcam Display success"), p.trigger(I.WEBCAM_STATUS_CHANGE)
        })
      }, we.toggleWebcam = function() {
        return b.startPerf(D.cameraPreviewToggle), we.getWebcamPresent().then(function(e) {
          if (e) return we.toggleWebcamDisplay().then(function() {
            b.endPerf(D.cameraPreviewToggle)
          })
        }).then(null, function(e) {
          Se.info("Cannot toggle Webcam"), we.handleNodeError(e.data)
        })
      }, we.addOverlayTelemetryData = function(e, t) {
        e = e || {}, t && (e.viewersOverlayPos = we.cachedOverlayPositions.viewer), e.fpsOverlayPos = we
          .cachedOverlayPositions.fps, e.cameraOverlayPos = we.cachedOverlayPositions.webcam, e.statusOverlayPos =
          we.cachedOverlayPositions.record
      }, we.getWebcamOverlaySettings = function() {
        return r.getWebcamOverlaySettings().then(function(e) {
          return Se.info("Webcam overlay settings : ", e.data), ie("webcam", e.data), e.data
        })
      }, we.setWebcamOverlaySettings = function(e) {
        return ie("webcam", e), r.setWebcamOverlaySettings({}, {
          enable: e.enable,
          position: e.position,
          size: e.size
        }).then(function() {
          Se.info("Set webcam overlay settings : ", e)
        })
      }, we.IndicatorOverlayEnum = {
        INDICATOR_OVERLAY_RECORD: "record",
        INDICATOR_OVERLAY_GAMECAST: "gamecast",
        INDICATOR_OVERLAY_FPS: "fps",
        INDICATOR_OVERLAY_VIEWER: "viewer",
        INDICATOR_OVERLAY_RIG: "rig"
      }, a.default && (0, a.default)(we.IndicatorOverlayEnum), we.getIndicatorOverlaySupported = function(e) {
        return r.getIndicatorOverlaySupported({
          id: e
        }).then(function(t) {
          return Se.info("Get " + e + " overlay supported : ", t.data.support), t.data.support
        })
      }, we.getIndicatorOverlaySettings = function(e) {
        return r.getIndicatorOverlaySettings({
          id: e
        }).then(function(t) {
          return Se.info("Get " + e + " overlay settings : ", t.data), ie(e, t.data), t.data
        })
      }, we.setIndicatorOverlaySettings = function(e, t) {
        return ie(e, t), r.setIndicatorOverlaySettings({
          id: e
        }, {
          enable: t.enable,
          position: t.position
        }).then(function() {
          Se.info("Set " + e + " overlay settings : ", t), "viewer" === e && p.trigger(I
            .BROADCAST_DISPLAY_VIEWER_COUNT, t.enable)
        })
      }, we.getRecordingPaths = function() {
        return r.getRecordingPaths().then(function(e) {
          return Se.info("Get Recording paths: ", e.data), e.data
        }, function(e) {
          Se.error("ERROR, problem when getting recording paths:")
        })
      }, we.setRecordingPaths = function(e, t) {
        return r.setRecordingPaths({}, {
          videos: e,
          tempFiles: t
        }).then(function(n) {
          Se.info("Set Recording paths: videos=" + e + ", tempFiles=" + t)
        }, function(n) {
          Se.error("ERROR, problem when setting recording paths: videos=" + e + ", tempFiles=" + t)
        })
      }, we.broadcast2KEnabled = null, we.isBroadcast2KEnabled = function() {
        return we.isBroadcast2KSupported() ? f.isNull(we.broadcast2KEnabled) ? r.getBroadcast2KEnabled().then(
          function(e) {
            return we.broadcast2KEnabled = e.data.enable, Se.info("getBroadcast2KEnabled: ", e.data.enable), e
              .data.enable
          }) : e.when(we.broadcast2KEnabled) : e.when(!1)
      }, we.isBroadcast2KSupported = function() {
        return Se.info("Is broadcast2K supported :", we.broadcast2KSupported), we.broadcast2KSupported
      }, we.HotkeyShortcuts = {
        OPENSHARE: "OpenShare",
        PTT: "PTT",
        FPS: "FPS",
        SCREENSHOT: "Screenshot",
        RECORDSAVE: "RecordSave",
        RECORDTOGGLE: "RecordToggle",
        BROADCASTTOGGLE: "BroadcastToggle",
        BROADCASTPAUSETOGGLE: "BroadcastPauseToggle",
        CAMERATOGGLE: "CameraToggle",
        OVERLAYTOGGLE: "OverlayToggle",
        NVCAMERAUI: "NvCameraUI",
        OVERLAYASWITCH: "OverlayASwitch",
        OVERLAYBSWITCH: "OverlayBSwitch",
        OVERLAYCSWITCH: "OverlayCSwitch",
        COMMENTSTOGGLE: "CommentsToggle",
        DVRTOGGLE: "DVRToggle",
        MICTOGGLE: "MicToggle",
        MODSTOGGLE: "ModsToggle",
        MODSPRESETCYCLE: "ModsPresetCycle",
        MODSUI: "ModsUI",
        MODSPRESET1: "ModsPreset1",
        MODSPRESET2: "ModsPreset2",
        MODSPRESET3: "ModsPreset3",
        OCTOOLUITOGGLE: "pmocsidebar",
        PERFOVERLAYTOGGLE: "pmocoverlay",
        PERFOVERLAYCYCLE: "pmocoverlaycycle",
        RESETAVERAGES: "pmocresetaveragemetrics",
        TOGGLELOGGING: "pmocloggingtoggle"
      }, a.default && (0, a.default)(we.HotkeyShortcutEnum), we.getHotkeyShortcut = function(t) {
        return f.indexOf(f.values(we.HotkeyShortcuts), t) < 0 ? (Se.error(
          "ERROR, invalid hotkey shortcut identifier"), e.reject("")) : r.getHotkeyShortcut({
          hk: t
        }).then(function(e) {
          return Se.info("Get hotkey shortcut for " + t + ": ", e.data), e.data
        }, function(e) {
          return Se.error("ERROR, problem when getting keys for hotkey shortcut " + t), ""
        })
      }, we.setHotkeyShortcut = function(t, n) {
        return !f.indexOf(f.values(we.HotkeyShortcuts), t) < 0 ? (Se.error(
          "ERROR, invalid hotkey shortcut identifier"), e.reject("")) : r.setHotkeyShortcut({
          hk: t
        }, {
          keys: n
        }).then(function() {
          Se.info("Set hotkey shortcut for " + t + ": ", n), oe(t, n)
        }, function(n) {
          var i = "(no message)";
          return n && n.data && (i = n.data.message), Se.error(
            "ERROR, problem when setting keys for hotkey shortcut " + t + ": " + i), e.reject(n)
        })
      }, we.getHotkeyMonitoringEnabled = function() {
        return r.getHotkeyMonitoringEnabled().then(function(e) {
          return Se.info("Is Hotkey Monitoring Enabled: ", e.data.enable), e.data
        }, function(e) {
          return Se.error("ERROR, Hotkey Monitoring Enabled request failed: ", e), !1
        })
      }, we.setHotkeyMonitoringEnabled = function(e) {
        return r.setHotkeyMonitoringEnabled({}, {
          enable: e
        }).then(function(t) {
          Se.info("Set Hotkey Monitoring Enabled: ", e)
        }, function(e) {
          Se.error("ERROR, Hotkey Monitoring Enabled request failed: ", e)
        })
      }, we.getScreenshotSupported = function() {
        return r.getScreenshotSupported().then(function(e) {
          return Se.info("Is Screenshot Feature supported: ", e.data.support), e.data.support
        }, function(e) {
          Se.error("ERROR, Screenshot support request failed: ", e)
        })
      }, we.checkAndCaptureScreenshot = function() {
        return we.captureScreenshot().catch(function(e) {
          e.data.code === _e.OSC_ERR_NO_VALID_GAME_RUNNING && we.tryStartScreenshot({
            closeOSC: !1,
            lastState: "main.main-menu",
            topActionArgs: {
              fromOscUI: !0
            }
          })
        })
      }, we.onScreenShotCapturedCallback = void 0, we.captureScreenshot = function(e, t, n) {
        return we.onScreenShotCapturedCallback = e, r.captureScreenshot({}, {
          scale: t,
          effect: n
        }).then(function(e) {
          Se.info("Screenshot capture from Main-Menu successful. " + (0, s.default)(e))
        })
      }, we.onNGXShotCapturedCallback = void 0, we.captureNGXShot = function(e, t, n) {
        return we.onNGXShotCapturedCallback = e, r.captureNGXShot({}, {
          scale: t,
          path: n
        }).then(function(e) {
          Se.info("NGX capture started successful. " + (0, s.default)(e))
        }).catch(function(e) {
          Se.info("NGX capture failed to start. " + (0, s.default)(e));
          var t = {};
          if (t.notification = V.AI_SUPER_RES_FAILED, we.onNGXShotCapturedCallback) return we
            .onNGXShotCapturedCallback(t), void(we.onNGXShotCapturedCallback = void 0)
        })
      }, we.cancelNGXShot = function() {
        return Se.info("cancel called"), r.cancelNGXShot().then(function(e) {
          Se.info("NGX cancelShot done")
        }, function(e) {
          Se.info("NGX cancelShot failed")
        })
      }, we.handleNodeError = function(e) {
        if (void 0 === e || null === e) return void Se.info("handleNodeError Undefined error");
        Se.info("handleNodeError Error: ", e);
        var t = angular.fromJson(e);
        return void 0 !== t ? void re(t.code) : void 0
      }, we.setOscReady = function() {
        return Se.info("set osc ready"), r.setOscReady({}, {
          ready: !0
        })
      }, we.trimVideo = function(e, t, n, i) {
        return r.videoTrim({}, {
          input: e,
          output: t,
          headTrimMs: n,
          lengthMs: i
        })
      }, we.setInputRedirection = function(e, t, n) {
        return r.setInputRedirection({}, {
          redirect: e,
          type: n,
          hid: t
        })
      }, we.mainMenuData = {}, we.getMainViewData = function() {
        return r.getMainViewData({}, {
          fetchPartial: !0
        }).then(function(e) {
          return we.mainMenuData.webcamPresent = e.data.webcamPresent, we.mainMenuData.webcamShown = e.data
            .webcamShown, we.mainMenuData.micPresentCount = e.data.micPresentCount, we.mainMenuData
        }, function(e) {
          Se.error("ERROR, problem getting partial MainView data: ", e)
        })
      }, we.getMainViewDataInit = function() {
        return r.getMainViewData({}, {
          fetchPartial: !1
        }).then(function(e) {
          return we.mainMenuData = e.data, e.data
        }, function(e) {
          Se.error("ERROR, problem getting full MainView data: ", e)
        })
      }, we.setBroadcastPreference = function(e) {
        we.mainMenuData.broadcastProvider = e
      }, we.shouldWeAskUserToTurnOnPrivacyControl = function() {
        var e = !1,
          t = !1,
          n = !1;
        return we.isWindowedModeAllowed().then(function(t) {
          return e = !!t, Se.info("winModeSupported: ", e), we.getDesktopCaptureSupported()
        }).then(function(e) {
          return t = e, Se.info("dtCaptureSupported: ", t), we.getDesktopCaptureEnabled()
        }).then(function(i) {
          n = i, Se.info("dtCaptureEnabled: ", n);
          var o = e && t && !n;
          return o
        })
      }, we.tryStartManualRecord = function(e) {
        we.shouldWeAskUserToTurnOnPrivacyControl().then(function(t) {
          if (t === !1) return we.startManualRecord();
          var n = {
              title: "l10n.manualRecord",
              icon: "icon-record",
              question: "l10n.captureRecording",
              footnote: "l10n.undoPrivacySettings",
              topButton: "l10n.yes",
              bottomButton: "l10n.no",
              topAction: we.enableDTandStartManualRecord,
              bottomAction: "",
              closeOSC: !!e,
              lastState: e ? "" : "main.main-menu",
              topActionArgs: e
            },
            i = "main.confirmation";
          w.openOSC(i, n)
        }, function(e) {
          Se.info("tryStartManualRecord: Desktop Record not supported on MSHybrids")
        })
      }, we.enableDTandStartManualRecord = function(e) {
        return we.setDesktopCaptureEnabled(!0).then(function(t) {
          we.startManualRecord(), e === !1 && o.go("main.main-menu")
        }, function(e) {
          Se.error("setting privacy control enabled: ", e)
        })
      }, we.tryStartScreenshot = function(e) {
        we.shouldWeAskUserToTurnOnPrivacyControl().then(function(t) {
          if (t === !0) {
            e = e || {};
            var n = {
                title: "l10n.screenshot",
                icon: "icon-screenshot",
                question: "l10n.captureScreenshot",
                footnote: "l10n.undoPrivacySettings",
                topButton: "l10n.yes",
                bottomButton: "l10n.no",
                topAction: we.enableDTandStartScreenshot,
                bottomAction: "",
                closeOSC: !angular.isDefined(e.closeOSC) || e.closeOSC,
                lastState: angular.isDefined(e.lastState) ? e.lastState : "",
                topActionArgs: angular.isDefined(e.topActionArgs) ? e.topActionArgs : ""
              },
              i = "main.confirmation";
            w.openOSC(i, n)
          }
        }, function(e) {
          Se.info("tryStartScreenshot : Desktop screenshot not supported on MSHybrids")
        })
      }, we.enableDTandStartScreenshot = function(e) {
        return we.setDesktopCaptureEnabled(!0).then(function(t) {
          e.fromOscUI === !0 && w.openOSC("main.main-menu")
        }, function(e) {
          Se.error("setting privacy control enabled: ", e)
        })
      }, we.appInFocusTitle = function() {
        var e = "";
        return r.getBroadcastTitle().then(function(t) {
          return e = t.data.title, (f.isUndefined(e) || f.isNull(e)) && (e = ""), e
        }, function(t) {
          return e
        })
      }, we.appCaptureProcessInfo = function(e) {
        var t = r.getCaptureProcessInfo(e);
        return t().then(function(e) {
          return e.data.profileName || (e.data.profileName = ""), e.data
        })
      }, we.getLastAppID = function() {
        var e = 4294967293;
        return we.appCaptureProcessInfo(e).then(function(e) {
          return Se.info("drsInfo: success", e), e
        }, function(e) {
          return Se.error("getAppInFocus: error", e), !1
        })
      }, we.isHighlightsSessionActive = function() {
        return r.getHighlightsActive().then(function(e) {
          return e.active
        }, function(e) {
          return !1
        })
      };
      var Re = !1;
      we.setModsActiveStatus = function(e) {
          Re = e, S.setModsActiveStatus(e), Se.info("setModsActiveStatus: ", Re)
        }, we.getModsActiveStatus = function() {
          return Re
        }, we.getMainMenuShortcuts = function() {
          return e.when(we.shortcuts)
        }, we.dynamicHotkeyToggle = function(e, t, n) {
          return n = n || 0, v(function() {
            r.dynamicHotkeyToggle({}, {
              enable: t,
              hotkeyNames: e
            }).catch(function(e) {
              Se.info("dynamicHotkeyToggle failed : ", e)
            })
          }, n)
        }, we.sendHotkeyTelemetry = function(e) {
          if (f.indexOf(f.values(we.HotkeyShortcuts), e) < 0) return void Se.error(
          "ERROR, invalid hotkey shortcut");
          var t = {
            hotkeyId: e,
            hotkeyKeys: {},
            hotkeyStr: ""
          };
          be(t).then(function() {
            var n = {
              menuName: he(e),
              hotkeyID: e,
              hotkey: t.hotkeyStr
            };
            b.push(P.OSC_HOTKEY_SETTINGS_EVENT, n)
          })
        }, we.getAnselHotKeyData = function() {
          var t = [];
          return we.anselHotkeys = [{
            hotkeyId: we.HotkeyShortcuts.MODSTOGGLE,
            hotkeyKeys: {},
            hotkeyStr: ""
          }, {
            hotkeyId: we.HotkeyShortcuts.MODSPRESET1,
            hotkeyKeys: {},
            hotkeyStr: ""
          }, {
            hotkeyId: we.HotkeyShortcuts.MODSPRESET2,
            hotkeyKeys: {},
            hotkeyStr: ""
          }, {
            hotkeyId: we.HotkeyShortcuts.MODSPRESET3,
            hotkeyKeys: {},
            hotkeyStr: ""
          }], f.each(we.anselHotkeys, function(e) {
            t.push(be(e))
          }), e.all(t)
        }, we.DTCaptureSupported = !1, we.notebookCoProc = !1, we.broadcast2KSupported = !1, we.supported8K60 = !1,
        we.captureState = "", we.init = function() {
          return Se.info("Initialize ShadowPlayService"), ve(), we.isSPRunning().then(function(e) {
            return e ? r.getCaptureState() : we.run().then(function() {
              return r.getCaptureState()
            })
          }).then(function(e) {
            return Se.info("Capture State: ", e.data.state), we.captureState = e.data.state, r
              .getDesktopCaptureSupportReason()
          }).then(function(e) {
            if (Se.info("Desktop Support Reason: ", e.data), we.DTCaptureSupported = e.data.support, !we
              .DTCaptureSupported) {
              var t = e.data.unsupportReason;
              (t.indexOf("notebookDriver") >= 0 || t.indexOf("notebookCoProc") >= 0 || t.indexOf(
                "notebookDGpu") >= 0 || t.indexOf("hideCheckboxAOSP") >= 0) && (we.notebookCoProc = !0)
            }
            return r.getBroadcast2KSupported()
          }).then(function(e) {
            return Se.info("Broadcast2K Support : ", e.data), we.broadcast2KSupported = e.data.support, we
              .getMainViewDataInit()
          }).then(function() {
            return we.getAudioSettingsInit()
          }).then(function() {
            return H(), xe()
          }).then(function() {
            return r.get8K60Support()
          }).then(function(e) {
            Se.info("8K60 Support : ", e.data.support), we.supported8K60 = e.data.support
          }).then(function() {})
        }
    }
  ]);
  t.shadowPlayService = g
}
