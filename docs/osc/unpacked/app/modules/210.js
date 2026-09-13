// ─────────────────────────────────────────────────────────────
// APP MODULE 210
// role       : controller MainMenuController | controller mdMenuBar
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
  }), t.MainMenuController = void 0;
  var o = n(8),
    r = i(o),
    a = n(1);
  n(11), n(4), n(23), n(12), n(33), n(46), n(26), n(48), n(40), n(60), n(22);
  var l = n(312),
    s = i(l),
    d = n(311),
    c = i(d),
    u = a.ngMainModule.controller("MainMenuController", ["$scope", "$mdMenu", "$state", "$log", "$document", "$timeout",
      "$q", "$filter", "shadowPlayService", "broadcastService", "osdService", "oscNotificationService",
      "oscDisplayService", "eventAggregator", "COMMON_EVENTS", "coplayService", "errorDialogService",
      "telemetryService", "nvCameraService", "octoolService", "quietMode2Service", "SHADOWPLAY_EVENTS",
      "RECORDING_STATES", "AUDIO_STATE", "NVCAMERA_EVENTS", "BROADCAST_STATES", "TILE_STATUS_BRUSH",
      "NOTIFIER_SELECTIONS", "KEYBOARD_EVENTS", "COPLAY_STATE", "COPLAY_EVENTS", "OSC_KEYBOARD",
      "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_CAPTURE_TYPE", "TELEMETRY_OSC_PERF_ID", "VIDEO_STATE",
      "piplConfigService",
      function(e, t, n, i, o, a, l, d, u, f, m, g, p, h, b, x, v, y, w, S, E, k, T, C, O, A, I, M, R, P, D, N, L, F,
        U, z, G) {
        function V() {
          J.micMenu = {
            items: [{
              id: "1",
              name: "l10n.pushToTalk",
              icon: "icon-mic_ptt",
              visible: !0,
              enabled: !0,
              click: function() {
                return J.setMicMode("ptt")
              }
            }, {
              id: "2",
              name: "l10n.alwaysOn",
              icon: "icon-mic_on",
              visible: !0,
              enabled: !0,
              click: function() {
                return J.setMicMode("alwayson")
              }
            }, {
              id: "3",
              name: "l10n.off",
              icon: "icon-mic_off",
              visible: !0,
              enabled: !0,
              click: function() {
                return J.setMicMode("off")
              }
            }, {
              id: "divider"
            }, {
              id: "4",
              name: "l10n.settings",
              visible: !0,
              enabled: !1,
              click: function() {
                return J.setMicCustomize()
              }
            }]
          }, J.tiles = {
            Screenshot: {
              name: "screenshot-tile",
              extraclass: se ? "three-left" : "two-left",
              title: "l10n.screenshot",
              icon: "icon-screenshot",
              initialFocus: "false",
              visible: le,
              enabled: le,
              shortcut: J.screenshotHotkey,
              click: function() {
                J.screenshotOnClick()
              }
            },
            Ansel: {
              name: "ansel-tile",
              extraclass: se ? "three-left" : "two-left",
              title: "l10n.openAnsel",
              icon: "icon-camera",
              initialFocus: "false",
              visible: le,
              enabled: le,
              shortcut: J.anselHotkey,
              click: function() {
                J.anselOnClick()
              }
            },
            Mods: {
              name: "mods-tile",
              extraclass: se ? "three-left" : "two-left",
              title: "l10n.mods",
              icon: "icon-freestyle",
              initialFocus: "false",
              visible: se,
              enabled: se,
              shortcut: J.shortcuts.modsOpen.hotkeyStr,
              click: function() {
                J.modsOnClick()
              }
            },
            InstantReplay: {
              name: "instant-replay-tile",
              title: "l10n.instantReplay",
              currentState: T.NOTRECORDING,
              initialFocus: "true",
              visible: !0,
              enabled: !0,
              state: {
                NotRecording: {
                  icon: "icon-replay",
                  status: "l10n.off",
                  statusBrush: I.NORMAL,
                  items: [{
                    id: "1",
                    name: "l10n.instantReplayStart",
                    icon: "icon-play",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleDVR.hotkeyStr,
                    click: function() {
                      return u.startInstantReplay()
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "2",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.InstantReplay)
                    }
                  }]
                },
                Recording: {
                  icon: "icon-replay icon-highlighted",
                  status: "l10n.on",
                  statusBrush: I.HIGHLIGHTED,
                  items: [{
                    id: "1",
                    name: "l10n.instantReplayStop",
                    icon: "icon-stop",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleDVR.hotkeyStr,
                    click: function() {
                      u.stopInstantReplay()
                    }
                  }, {
                    id: "2",
                    name: "l10n.save",
                    icon: "icon-save",
                    visible: !0,
                    enabled: !1,
                    shortcut: J.shortcuts.saveRecord.hotkeyStr,
                    click: function() {
                      return u.saveInstantReplay()
                    }
                  }, {
                    id: "3",
                    name: "l10n.upload",
                    icon: "icon-share",
                    visible: J.isConnectEnabled,
                    enabled: ae && J.isConnectEnabled,
                    click: function() {
                      return u.uploadInstantReplay()
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "4",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.InstantReplay)
                    }
                  }]
                }
              }
            },
            ManualRecord: {
              name: "manual-record-tile",
              title: "l10n.manualRecord",
              currentState: "NotRecording",
              initialFocus: "false",
              visible: !0,
              enabled: !0,
              state: {
                NotRecording: {
                  name: "NotRecording",
                  icon: "icon-record",
                  status: "l10n.notRecording",
                  statusBrush: I.NORMAL,
                  items: [{
                    id: "1",
                    name: "l10n.start",
                    icon: "icon-play",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleRecord.hotkeyStr,
                    click: function() {
                      return u.tryStartManualRecord(!1)
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "2",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.ManualRecord)
                    }
                  }]
                },
                Recording: {
                  name: "Recording",
                  icon: "icon-record icon-highlighted",
                  status: "l10n.recording",
                  statusBrush: I.HIGHLIGHTED,
                  items: [{
                    id: "1",
                    name: "l10n.stopAndSave",
                    icon: "icon-stop",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleRecord.hotkeyStr,
                    click: function() {
                      return u.stopAndSaveManualRecord()
                    }
                  }, {
                    id: "2",
                    name: "l10n.upload",
                    icon: "icon-share",
                    visible: J.isConnectEnabled,
                    enabled: ae && J.isConnectEnabled,
                    click: function() {
                      return u.uploadManualRecord()
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "3",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.ManualRecord)
                    }
                  }]
                }
              }
            },
            Stream: {
              name: "stream-tile",
              title: "l10n.stream",
              currentState: P.OFF,
              initialFocus: "false",
              visible: !1,
              enabled: !0,
              state: {
                On: {
                  name: "On",
                  icon: "icon-stream",
                  status: "l10n.on",
                  statusBrush: I.HIGHLIGHTED,
                  items: [{
                    id: "1",
                    name: "l10n.stop",
                    icon: "icon-stop",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayStop()
                    }
                  }, {
                    id: "2",
                    name: "l10n.pause",
                    icon: "icon-pause",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayPause()
                    }
                  }, {
                    id: "3",
                    name: "l10n.guest",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayGuest()
                    }
                  }]
                },
                Paused: {
                  name: "Paused",
                  icon: "icon-stream icon-highlighted",
                  status: "l10n.paused",
                  statusBrush: I.HIGHLIGHTED,
                  items: [{
                    id: "1",
                    name: "l10n.stop",
                    icon: "icon-stop",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayStop()
                    }
                  }, {
                    id: "2",
                    name: "l10n.resume",
                    icon: "icon-play",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayResume()
                    }
                  }, {
                    id: "3",
                    name: "l10n.guest",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayGuest()
                    }
                  }]
                },
                Off: {
                  name: "Off",
                  icon: "icon-stream",
                  status: "l10n.off",
                  statusBrush: I.NORMAL,
                  items: [{
                    id: "1",
                    name: "l10n.invite",
                    icon: "icon-invite_send",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayEmailInvite()
                    }
                  }, {
                    id: "2",
                    name: "l10n.copyInvite",
                    icon: "icon-invite_copy_url",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayCopyLink()
                    }
                  }, {
                    id: "3",
                    name: "l10n.guest",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayGuest()
                    }
                  }]
                },
                CreatingInvite: {
                  name: "CreatingInvite",
                  icon: "icon-stream icon-alternate",
                  status: "l10n.off",
                  statusBrush: I.ALTERNATE
                },
                InviteSent: {
                  name: "InviteSent",
                  icon: "icon-stream icon-alternate",
                  status: "l10n.off",
                  statusBrush: I.ALTERNATE,
                  items: [{
                    id: "1",
                    name: "l10n.cancelInvite",
                    icon: "icon-invite_cancel",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayCancelInvite()
                    }
                  }, {
                    id: "2",
                    name: "l10n.guest",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayGuest()
                    }
                  }]
                },
                ClientReady: {
                  name: "ClientReady",
                  icon: "icon-stream icon-alternate",
                  status: "l10n.off",
                  statusBrush: I.ALTERNATE,
                  items: [{
                    id: "1",
                    name: "l10n.cancelInvite",
                    icon: "icon-invite_cancel",
                    visible: !0,
                    enabled: !0
                  }, {
                    id: "2",
                    name: "l10n.guest",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.coplayGuest()
                    }
                  }]
                }
              }
            },
            Broadcast: {
              name: "broadcast-tile",
              title: "l10n.broadcastLive",
              currentState: A.STOPPED,
              initialFocus: "false",
              visible: J.isConnectEnabled,
              enabled: J.isConnectEnabled,
              state: {
                Stopped: {
                  name: "Stopped",
                  icon: "icon-broadcast",
                  status: "l10n.notBroadcasting",
                  statusBrush: I.NORMAL,
                  items: [{
                    id: "1",
                    name: "l10n.start",
                    icon: "icon-play",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleBroadcast.hotkeyStr,
                    click: function() {
                      return f.tryStartBroadcast(!1)
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "2",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.Broadcast)
                    }
                  }]
                },
                Active: {
                  name: "Active",
                  icon: "icon-broadcast icon-highlighted",
                  status: "l10n.broadcasting",
                  statusBrush: I.HIGHLIGHTED,
                  items: [{
                    id: "1",
                    name: "l10n.pause",
                    icon: "icon-pause",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleBroadcastPause.hotkeyStr,
                    click: function() {
                      return f.pauseBroadcast()
                    }
                  }, {
                    id: "2",
                    name: "l10n.stop",
                    icon: "icon-stop",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleBroadcast.hotkeyStr,
                    click: function() {
                      return f.stopBroadcast()
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "3",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.Broadcast)
                    }
                  }]
                },
                Paused: {
                  name: "Paused",
                  icon: "icon-broadcast",
                  status: "l10n.paused",
                  statusBrush: I.HIGHLIGHTED,
                  items: [{
                    id: "1",
                    name: "l10n.resume",
                    icon: "icon-play",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleBroadcastPause.hotkeyStr,
                    click: function() {
                      return f.resumeBroadcast()
                    }
                  }, {
                    id: "2",
                    name: "l10n.stop",
                    icon: "icon-stop",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleBroadcast.hotkeyStr,
                    click: function() {
                      return f.stopBroadcast()
                    }
                  }, {
                    id: "divider"
                  }, {
                    id: "3",
                    name: "l10n.settings",
                    visible: !0,
                    enabled: !0,
                    click: function() {
                      return J.settings(J.tiles.Broadcast)
                    }
                  }]
                }
              }
            },
            Performance: {
              name: "performance-tile",
              extraclass: H(),
              title: "l10n.perfmonoc.performance",
              icon: "icon-audio_mixer",
              initialFocus: "false",
              visible: ce,
              enabled: ce,
              shortcut: J.shortcuts.ocToolMenuToggle.hotkeyStr,
              click: function() {
                S.launchOCToolMenu("uiTrigger")
              }
            },
            Gallery: {
              name: "gallery-tile",
              extraclass: H(),
              title: "l10n.gallery",
              icon: "icon-gallery",
              initialFocus: "false",
              visible: !0,
              enabled: !0,
              click: function() {
                J.galleryOnClick()
              }
            },
            Mic: {
              name: "mic-tile",
              extraclass: ce ? "three-right" : "two-right",
              currentState: "On",
              initialFocus: "false",
              visible: !0,
              enabled: !0,
              state: {
                On: {
                  name: "On",
                  icon: "icon-mic_on",
                  items: J.micMenu.items
                },
                Off: {
                  name: "Off",
                  icon: "icon-mic_off",
                  items: J.micMenu.items
                },
                PTT: {
                  name: "PTT",
                  icon: "icon-mic_ptt",
                  items: J.micMenu.items
                }
              }
            },
            Camera: {
              name: "camera-tile",
              extraclass: ce ? "three-right" : "two-right",
              currentState: "Off",
              initialFocus: "false",
              visible: !0,
              enabled: !0,
              state: {
                On: {
                  name: "On",
                  icon: "icon-webcam_on",
                  items: [{
                    id: "1",
                    name: "l10n.off",
                    icon: "icon-webcam_off",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleCamera.hotkeyStr,
                    click: function() {
                      u.toggleWebcam()
                    }
                  }]
                },
                Off: {
                  name: "Off",
                  icon: "icon-webcam_off",
                  items: [{
                    id: "1",
                    name: "l10n.on",
                    icon: "icon-webcam_on",
                    visible: !0,
                    enabled: !0,
                    shortcut: J.shortcuts.toggleCamera.hotkeyStr,
                    click: function() {
                      u.toggleWebcam()
                    }
                  }]
                }
              }
            },
            Preferences: {
              name: "preferences-tile",
              extraclass: ce ? "three-right" : "two-right",
              icon: "icon-settings",
              initialFocus: "false",
              visible: !0,
              enabled: !0,
              click: function() {
                J.preferencesOnClick()
              }
            },
            QuietMode2: j()
          }
        }

        function H() {
          var e = ce ? "three-right" : "two-right";
          return ue.supported && (e += " extra-wide"), e
        }

        function B() {
          t.hide();
          var e = angular.element(o[0].getElementById("mainMenuBar"));
          e.controller("mdMenuBar").scheduleOpenMenu(fe)
        }

        function Y(e) {
          return u.isCoplayBlocked().then(function(t) {
            if (!t) return x.getFullscreenPid().then(function(t) {
              0 === t ? v.show("l10n.systemRequirement", "l10n.notificationWarningGameRequired") : t < 0 ? v
                .show("l10n.notice", "l10n.notificationCoplayCommunicationError") : e()
            })
          })
        }

        function $() {
          var e = !1;
          return le = se = !1, w.isGfeAnselSupported().then(function(t) {
            return le = e = t === !0, le && (se = w.isModsOn()), ee.info("GFE-IPC-Ansel Supported? " + e), e
          }, function(t) {
            return ee.error("call to isGfeAnselSupported() returned with message " + (0, r.default)(t)), le = e, e
          })
        }

        function W(e) {
          J.setMenuStateRecord({
            instantReplayEnabled: e.instantReplayEnabled,
            instantReplayRunning: e.instantReplayRunning,
            manualRecordEnabled: e.manualRecordEnabled
          }), J.setMenuStateBroadcast({}), J.setBroadcastMenuState({
            broadcastProvider: e.broadcastProvider
          }), J.setCameraState({
            webcamPresent: e.webcamPresent
          }), J.setMicMenuState({
            micMode: e.micMode,
            micPresentCount: e.micPresentCount
          }), J.setCoplayState(), J.setCoplayMenuState(e.coplayEnabled), J.setHotkeyShortcuts({
            screenshotHotkey: e.screenshotHotkey,
            nvCameraHotkey: e.nvCameraHotkey
          })
        }

        function j() {
          return {
            name: "quietmode2-tile",
            extraclass: ce ? "three-right" : "two-right",
            icon: ue.enabled ? s.default : c.default,
            initialFocus: "false",
            visible: ue.supported,
            enabled: ue.supported,
            click: q
          }
        }

        function K(e) {
          X(e).then(function() {
            J.tiles.QuietMode2 = j()
          })
        }

        function q() {
          var e = !ue.enabled;
          ee.info("set quiet mode to: ", e), ue.enabled = e, E.setStateInfo(ue).then(function(e) {
            ee.info("set quiet mode response:", e), K();
            var t = ue.enabled ? M.WHISPER_MODE_ENABLED : M.WHISPER_MODE_DISABLED;
            g.show(t)
          })
        }

        function X(e) {
          return E.getStateInfo(e).then(function(e) {
            ee.info("Quiet Mode2 getStateInfo success: ", e), ue = e
          })
        }

        function Z() {
          h.on(k.WEBCAM_STATUS_CHANGE, J.setCameraState), h.on(k.STATUS_CHANGE_RECORD, J.setMenuStateRecord), h.on(k
              .STATUS_CHANGE_BROADCAST, J.setMenuStateBroadcast), h.on(k.IR_RECORDING_STATE_CHANGED, J
            .setIRMenuState), h.on(k.COPLAY_ENABLED_CHANGED, J.setCoplayMenuState), h.on(D.COPLAY_STATE_CHANGED, J
              .setCoplayState), h.on(k.MIC_STATUS_CHANGE, J.setMicMenuState), h.on(O.AVAILABILITY_CHANGED, J
              .setNvCameraMenuState), J.enableEscapeEvent(!0)
        }

        function Q(e) {
          ee.info("Connect status:", e.isConnectEnabled), J.isConnectEnabled = e.isConnectEnabled, J.tiles.Broadcast
            .visible = J.isConnectEnabled, J.tiles.Broadcast.enabled = J.isConnectEnabled;
          var t = J.tiles.InstantReplay.state.Recording.items.find(function(e) {
            return e.id = "3"
          });
          t && (t.visible = J.isConnectEnabled, t.enabled = ae && J.isConnectEnabled);
          var n = J.tiles.ManualRecord.state.Recording.items.find(function(e) {
            return e.id = "2"
          });
          n && (n.visible = J.isConnectEnabled, n.enabled = ae && J.isConnectEnabled)
        }
        var J = this,
          ee = i.getInstance("main.main-menu/mainmenucontroller"),
          te = !1,
          ne = !1,
          ie = !1,
          oe = !1,
          re = !1,
          ae = !0,
          le = !1,
          se = !1,
          de = "broadcast-tile",
          ce = S.getIsFeatureAvailable(),
          ue = {};
        J.isConnectEnabled = !1, J.showDiscoveryBanner = function() {
          return (!te || te && ie) && !le
        }, J.showScreenshotDiscoveryBanner = function() {
          return ne
        }, J.showNvCameraDiscoveryBanner = function() {
          return w.isOn()
        }, J.showPhotographyBanner = function() {
          return J.showDiscoveryBanner() && J.showScreenshotDiscoveryBanner() && J.showNvCameraDiscoveryBanner() &&
            re
        }, J.showScreenshotBanner = function() {
          return oe
        }, J.select = function(e) {
          J.selected = e
        }, J.isActive = function(e) {
          return J.selected === e
        }, J.openMenu = function(e, t, n, i) {
          return i.click ? void i.click() : void(te === !1 ? e(n) : t(n))
        }, J.closeOSC = function() {
          p.closeOSC()
        }, J.enableEscapeEvent = function(e) {
          var t = e;
          a(function() {
            t === !0 ? h.on(R.ESCAPE, J.closeOSC) : h.off(R.ESCAPE, J.closeOSC)
          }, 200)
        }, e.$on("$mdMenuClose", function(e, t) {
          var n = t.children(".menu-button");
          n.removeClass("menu-open"), te = !1, J.enableEscapeEvent(!0)
        }), e.$on("$mdMenuOpen", function(e, t) {
          var n = t.children(".menu-button");
          n.addClass("menu-open"), te = !0, J.enableEscapeEvent(!1)
        }), J.mouseOver = function(e, t) {
          t.enabled === !0 ? e.currentTarget.focus() : e.currentTarget.blur(), ie = J.isASideTile(t)
        }, J.isASideTile = function(e) {
          return "gallery-tile" === e.name || "mic-tile" === e.name || "camera-tile" === e.name ||
            "preferences-tile" === e.name || "screenshot-tile" === e.name || "ansel-tile" === e.name
        }, J.doNotPropogate = function(e) {
          ee.info("Blocking: ", e.keyCode), e.preventDefault(), e.stopImmediatePropagation(), e.stopPropagation()
        }, J.keyDownMenu = function(e, t) {
          ee.debug("keyDownMenu keyCode: " + t.keyCode + " tile: " + e.name), t.keyCode === N.DOWN_ARROW ? (ie = J
              .isASideTile(e), "gallery-tile" !== e.name && "preferences-tile" !== e.name && "screenshot-tile" !== e
              .name && "ansel-tile" !== e.name || J.doNotPropogate(t)) : t.keyCode === N.RIGHT_ARROW ? e.name ===
            de ? ie = !0 : "ansel-tile" === e.name ? ie = !1 : "preferences-tile" === e.name && te === !0 && J
            .doNotPropogate(t) : t.keyCode === N.LEFT_ARROW && ("instant-replay-tile" === e.name ? ie = !0 :
              "gallery-tile" === e.name && (ie = !1))
        }, J.keyDownSubmenu = function(e, t, n, i) {
          ee.debug("keyDownSubmenu keyCode: " + t.keyCode + " tile: " + e.name), t.keyCode === N.RIGHT_ARROW ? e
            .name === de && (ie = !0) : t.keyCode === N.LEFT_ARROW ? "instant-replay-tile" === e.name && (ie = !0) :
            t.keyCode === N.TAB && te === !0 && i(t)
        }, J.settings = function(e) {
          y.startPerf(U.customizeScreen), J.custTeleParams = {
            callback: function(e) {
              y.endPerfAfterDigest(U.customizeScreen, e)
            }
          }, u.setVideoState(z.MAIN), e === J.tiles.Broadcast ? n.go("main.preferences.broadcast", J
            .custTeleParams) : e === J.tiles.InstantReplay ? (y.push(L.OSC_CAPTURE_DVR_CUSTOMIZE), y.push(L
            .OSC_CAPTURE_CUSTOMIZE, {
              provider: F.instantReplay
            }), n.go("main.preferences.video", J.custTeleParams)) : (y.push(L.OSC_CAPTURE_MANUAL_CUSTOMIZE), y
            .push(L.OSC_CAPTURE_CUSTOMIZE, {
              provider: F.manualRecord
            }), n.go("main.preferences.video", J.custTeleParams))
        };
        var fe = {
          open: function() {}
        };
        J.preferencesOnClick = function() {
          y.startPerf(U.preferencesScreen);
          var e = function() {
            y.endPerfAfterDigest(U.preferencesScreen)
          };
          y.push(L.OSC_PREFERENCES_OPEN), B(), n.go("main.preferences", {
            callback: e
          })
        }, J.galleryOnClick = function() {
          y.push(L.OSC_GALLERY_OPEN), B(), n.go("main.gallery.files")
        }, J.screenshotOnClick = function() {
          B(), u.checkAndCaptureScreenshot()
        }, J.anselOnClick = function() {
          B(), ee.info("Navigating to Ansel UI using nvcameraservice."), w.launchUIForNvCamera("uiTrigger")
        }, J.modsOnClick = function() {
          B(), w.launchUIForMods("uiTrigger")
        }, J.coplayGuest = function() {
          n.go("main.guest-controls")
        }, J.coplayEmailInvite = function() {
          Y(function() {
            n.go("main.coplay-invite")
          })
        }, J.coplayCopyLink = function() {
          Y(function() {
            return g.show(M.COPLAY_COPYING_INVITE), x.createLinkSession()
          })
        }, J.coplayCancelInvite = function() {
          x.deleteSession(), g.show(M.COPLAY_INVITETO_CANCELLED, x.getInviteName())
        }, J.coplayPause = function() {
          x.setPause(!0).then(function(e) {
            e && g.show(M.COPLAY_PAUSED, x.getInviteName())
          })
        }, J.coplayResume = function() {
          x.setPause(!1).then(function(e) {
            e && g.show(M.COPLAY_NOW_PLAYING, x.getInviteName())
          })
        }, J.coplayStop = function() {
          g.show(M.COPLAY_STOPPEDPLAYING, x.getInviteName()), x.deleteSession()
        };
        var me = function(e) {
          return void 0 !== e && void 0 !== e.webcamPresent ? l.all([l.when(e.webcamPresent), u.getWebcamMode()]) :
            l.all([u.getWebcamPresent(), u.getWebcamMode()])
        };
        J.setCameraState = function(e) {
          return me(e).then(function(e) {
            var t = e[0],
              n = e[1];
            t ? (n ? J.tiles.Camera.currentState = "On" : J.tiles.Camera.currentState = "Off", m.overlaySettings
              .Camera.enabled = n, m.saveOverlaySettings("Camera")) : (J.tiles.Camera.currentState = "Off", J
              .tiles.Camera.state.Off.items[0].enabled = !1)
          })
        };
        var ge = function(e) {
          return void 0 !== e && void 0 !== e.micMode && void 0 !== e.micPresentCount ? l.all([l.when(e.micMode), l
            .when(e.micPresentCount)
          ]) : l.all([u.getMicMode(), u.getMicCount()])
        };
        J.setMicMenuState = function(e) {
          return ge(e).then(function(e) {
            var t = e[0],
              n = e[1];
            "ptt" === t ? J.tiles.Mic.currentState = "PTT" : "alwayson" === t ? J.tiles.Mic.currentState =
              "On" : J.tiles.Mic.currentState = "Off", J.micMenu.items.forEach(function(e) {
                e.enabled = 0 !== n
              })
          })
        }, J.setMicMode = function(e) {
          return u.setMicMode(e)
        }, J.setMicCustomize = function() {
          u.isMultiTrackAudioAvailable().then(function(e) {
            u.setAudioState(C.MAIN), e === !0 ? n.go("main.preferences.audio") : n.go("main.microphone")
          })
        };
        var pe = function(e) {
          return void 0 !== e && void 0 !== e.instantReplayEnabled && void 0 !== e.instantReplayRunning ? l.all([l
            .when(e.instantReplayRunning), l.when(e.instantReplayEnabled)
          ]) : l.all([u.isIRActive(), u.isIREnabled()])
        };
        J.setIRMenuState = function(e) {
          return pe(e).then(function(e) {
            var t = e[0],
              n = e[1];
            ee.info("IR Active: " + t + " IR Enabled: " + n), n ? J.tiles.InstantReplay.currentState = T
              .RECORDING : J.tiles.InstantReplay.currentState = T.NOTRECORDING, J.tiles.InstantReplay.state
              .Recording.items[1].enabled = t, J.tiles.InstantReplay.state.Recording.items[2].enabled = t && ae
          })
        };
        var he = function(e) {
          return void 0 !== e && void 0 !== e.manualRecordEnabled ? l.when(e.manualRecordEnabled) : u.isMRActive()
        };
        J.setMRMenuState = function(e) {
          return he(e).then(function(e) {
            ee.info("Rec mode: ", e), e === !0 ? J.tiles.ManualRecord.currentState = T.RECORDING : J.tiles
              .ManualRecord.currentState = T.NOTRECORDING
          })
        }, J.setMenuStateRecord = function(e) {
          J.setIRMenuState(e), J.setMRMenuState(e)
        }, J.setMenuStateBroadcast = function() {
          var e = f.getBroadcastState();
          e === A.ACTIVE || e === A.STOPPED || e === A.PAUSED ? J.tiles.Broadcast.currentState = e : J.tiles
            .Broadcast.currentState = A.STOPPED
        }, J.setCoplayState = function() {
          var e = x.getState();
          J.tiles.Stream.currentState = e;
          var t = "";
          switch (e) {
            case P.ON:
              t = "l10n.playingWith";
              break;
            case P.PAUSED:
              t = "l10n.pausedPlaying";
              break;
            case P.CREATINGINVITE:
              t = "l10n.creatingInviteFor";
              break;
            case P.INVITESENT:
              t = "l10n.inviteSentTo";
              break;
            case P.OFF:
            default:
              t = "l10n.off"
          }
          var n = d("translate")(t, {
            name: x.getInviteName()
          });
          J.tiles.Stream.state[e].status = n
        }, J.setCoplayMenuState = function(e) {
          return void 0 === e ? u.getCoplayEnabled().then(function(e) {
            J.tiles.Stream.visible = e
          }) : void(J.tiles.Stream.visible = e)
        }, J.setNvCameraMenuState = function(e) {
          return $().then(function(e) {
            var t = se ? "three-left" : "two-left";
            J.tiles.Screenshot.visible = e, J.tiles.Screenshot.enabled = e, J.tiles.Screenshot.extraclass = t, J
              .tiles.Ansel.visible = e, J.tiles.Ansel.enabled = e, J.tiles.Ansel.extraclass = t, J.tiles.Mods
              .enabled = se, J.tiles.Mods.visible = se, J.tiles.Mods.extraclass = t
          })
        };
        var be = function(e) {
          return void 0 !== e && void 0 !== e.broadcastProvider ? l.when(e.broadcastProvider) : f
            .getBroadcastPreference()
        };
        J.setBroadcastMenuState = function(e) {
          return J.isConnectEnabled ? be(e).then(function(e) {
            J.broadcastPortalPreference = f.getBroadcastPortalFromName(e), J.broadcastPortalPreference.id < 0 ?
              J.tiles.Broadcast.visible = !1 : J.tiles.Broadcast.visible = !0
          }) : l.when(!1)
        }, J.setHotkeyShortcuts = function(e) {
          var t = J.shortcuts.screenshot,
            n = J.shortcuts.anselOpen;
          return _.isUndefined(t) || _.isUndefined(t.hotkeyKeys) ? (oe = !1, J.screenshotDiscoveryText = "", J
            .screenshotHotkey = "l10n.disabled") : (oe = 1 !== t.hotkeyKeys.length || 0 !== t.hotkeyKeys[0], J
            .screenshotHotkey = t.hotkeyStr, J.screenshotDiscoveryText = d("translate")(
              "l10n.screenshotDiscovery", {
                arg1: J.screenshotHotkey
              })), _.isUndefined(n) || _.isUndefined(n.hotkeyKeys) ? (re = !1, J.inGamePhotographyText = "", J
            .anselHotkey = "l10n.disabled") : (re = 1 !== n.hotkeyKeys.length || 0 !== n.hotkeyKeys[0], J
            .anselHotkey = n.hotkeyStr, J.inGamePhotographyText = d("translate")("l10n.inGamePhotography", {
              arg1: J.anselHotkey
            })), u.getScreenshotSupported().then(function(e) {
            ne = e
          })
        }, J.mainMenuData = {}, J.fetchMainViewData = function() {
          return u.getMainViewData().then(function(e) {
            J.mainMenuData = e || {}
          })
        }, J.fetchAudioSettings = function() {
          return u.getAudioSettings().then(function(e) {
            ae = !e.separateTracks
          })
        }, J.fetchHotKeyData = function() {
          return u.getMainMenuShortcuts().then(function(e) {
            J.shortcuts = e
          })
        }, J.fetchInitializeData = function() {
          function e() {
            t.resolve(!0)
          }
          var t = l.defer(),
            n = [];
          return n.push(J.fetchMainViewData()), n.push(J.fetchAudioSettings()), n.push(J.fetchHotKeyData()), n.push(
            $()), n.push(X()), l.all(n).then(e), t.promise
        }, J.initialize = function() {
          return ee.info("Initialize Enter"), h.on(b.PIPL_CONFIG_UPDATED, Q), J.fetchInitializeData().then(function(
            e) {
            return G.isConnectEnabled()
          }).then(function(e) {
            J.isConnectEnabled = e, V(), Z(), W(J.mainMenuData), K(!0), ee.info("Initialized Data: ", J
              .mainMenuData)
          })
        }, J.initialize(), e.$on("$destroy", function() {
          h.off(k.WEBCAM_STATUS_CHANGE, J.setCameraState), h.off(k.STATUS_CHANGE_RECORD, J.setMenuStateRecord), h
            .off(k.STATUS_CHANGE_BROADCAST, J.setMenuStateBroadcast), h.off(k.IR_RECORDING_STATE_CHANGED, J
              .setIRMenuState), h.off(k.COPLAY_ENABLED_CHANGED, J.setCoplayMenuState), h.off(D
              .COPLAY_STATE_CHANGED, J.setCoplayState), h.off(k.MIC_STATUS_CHANGE, J.setMicMenuState), h.off(O
              .AVAILABILITY_CHANGED, J.setNvCameraMenuState), h.off(b.PIPL_CONFIG_UPDATED, Q), J
            .enableEscapeEvent(!1)
        })
      }
    ]);
  t.MainMenuController = u
}
