// ─────────────────────────────────────────────────────────────
// APP MODULE 244
// role       : controller PreferencesKeyboardShortcutsController
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
  }), t.PreferencesKeyboardShortcutsController = void 0;
  var r = n(109),
    a = o(r),
    l = n(375),
    s = o(l),
    d = n(3),
    c = i(d),
    u = n(1);
  n(4), n(34), n(32), n(12), n(22);
  var f = u.ngMainModule.controller("PreferencesKeyboardShortcutsController", ["$q", "$log", "$scope", "$state",
    "$timeout", "$filter", "eventAggregator", "shadowPlayService", "$stateParams", "broadcastService",
    "keyboardService", "nvCameraService", "oscDisplayService", "octoolService", "KEYBOARD_EVENTS", "OSC_KEYBOARD",
    "NVCAMERA_EVENTS", "COMMON_EVENTS", "piplConfigService",
    function(e, t, n, i, o, r, l, d, u, f, m, g, p, h, b, x, v, y, w) {
      function S(e) {
        return d.getHotkeyShortcut(e.hotkeyId).then(function(t) {
          e.hotkeyKeys = t.keys, c.isUndefined(t.keys) ? e.hotkeyStr = "" : e.hotkeyStr = m.shortcutToStr(t
            .keys)
        }, function(t) {
          I.error("ERROR in retrieving shortcut " + e.hotkeyId), e.hotkeyKeys = "", e.hotkeyStr = ""
        })
      }

      function E(e, t) {
        if (!e || !t || e.length !== t.length) return !1;
        for (var n = 0; n < e.length; n++) {
          var i = [].concat((0, s.default)(e)),
            o = [].concat((0, s.default)(t));
          if (i.sort(), o.sort(), i[n] !== o[n]) return !1
        }
        return !0
      }

      function k(e) {
        var t = c.findIndex(A.shortcuts, function(t) {
          return E(t.hotkeyKeys, e)
        });
        return t
      }

      function _(e) {
        return 1 === e.length && [x.BACKSPACE, x.SPACEBAR, x.DELETE].indexOf(e[0]) !== -1
      }

      function T(e) {
        return 1 === e.length && e[0] === x.ESCAPE
      }

      function C() {
        A.selectedShortCut = void 0, R = !1, M = []
      }

      function O(e) {
        I.info("Connect status:", e.isConnectEnabled), A.isConnectEnabled = e.isConnectEnabled, A.categories
          .Broadcast.visible = A.isConnectEnabled, f.getCustomOverlaySupportType().then(function(e) {
            2 === e && (A.categories.BroadcastOverlays.visible = A.isConnectEnabled, c.each(A.shortcuts, function(
              e) {
              "l10n.toggleOverlay" === e.name && (e.catId = "BroadcastOverlays"), "BroadcastOverlays" === e
                .catId && (e.visible = A.isConnectEnabled)
            }))
          })
      }
      var A = this;
      A.title = "l10n.settings", A.icon = "icon-settings", A.status = "", A.showDuplicateShortcutMessage = !1, A
        .errorTxt = "", A.errorTimeout = null, A.minutesToSave = 5, A.errorDisplayTime = 3e3, A.isConnectEnabled = !
        1;
      var I = t.getInstance("osc/preferencesKeyboardShortcutsController"),
        M = [],
        R = !1;
      A.selectedShortCut = void 0;
      var P = g.isOn(),
        D = g.isModsOn(),
        N = h.getIsFeatureAvailable();
      A.categories = {
        General: {
          name: "l10n.general",
          visible: !0
        },
        Capture: {
          name: "l10n.capture",
          visible: !0
        },
        Mods: {
          name: "l10n.mods",
          visible: D
        },
        Record: {
          name: "l10n.manualRecord",
          visible: !0
        },
        Broadcast: {
          name: "l10n.broadcastLive",
          visible: A.isConnectEnabled
        },
        BroadcastOverlays: {
          name: "l10n.broadcastOverlays",
          visible: !1
        },
        Performance: {
          name: "l10n.perfmonoc.performance",
          visible: N
        }
      }, A.shortcuts = [{
        catId: "General",
        name: "l10n.openShare",
        hotkeyId: d.HotkeyShortcuts.OPENSHARE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 90]
      }, {
        catId: "General",
        name: "l10n.activatePushToTalk",
        hotkeyId: d.HotkeyShortcuts.PTT,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [192]
      }, {
        catId: "General",
        name: "l10n.toggleMic",
        hotkeyId: d.HotkeyShortcuts.MICTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [17, 18, 77]
      }, {
        catId: "General",
        name: "l10n.toggleFPS",
        hotkeyId: d.HotkeyShortcuts.FPS,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !N,
        default: [18, 123]
      }, {
        catId: "Capture",
        name: "l10n.saveScreenshot",
        hotkeyId: d.HotkeyShortcuts.SCREENSHOT,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 112]
      }, {
        catId: "Capture",
        name: "l10n.photographTheScene",
        hotkeyId: d.HotkeyShortcuts.NVCAMERAUI,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: P,
        default: [18, 113]
      }, {
        catId: "Mods",
        name: "l10n.openMods",
        hotkeyId: d.HotkeyShortcuts.MODSUI,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: D,
        default: [18, 114]
      }, {
        catId: "Mods",
        name: "l10n.toggleMods",
        hotkeyId: d.HotkeyShortcuts.MODSTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: D,
        default: [0]
      }, {
        catId: "Mods",
        name: "l10n.cycleMods",
        hotkeyId: d.HotkeyShortcuts.MODSPRESETCYCLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: D,
        default: [0]
      }, {
        catId: "Record",
        name: "l10n.toggleIR",
        hotkeyId: d.HotkeyShortcuts.DVRTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 16, 121]
      }, {
        catId: "Record",
        name: "l10n.saveLastNMins",
        hotkeyId: d.HotkeyShortcuts.RECORDSAVE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 121]
      }, {
        catId: "Record",
        name: "l10n.toggleRecording",
        hotkeyId: d.HotkeyShortcuts.RECORDTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 120]
      }, {
        catId: "Broadcast",
        name: "l10n.toggleBroadcasting",
        hotkeyId: d.HotkeyShortcuts.BROADCASTTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 119]
      }, {
        catId: "Broadcast",
        name: "l10n.pauseResume",
        hotkeyId: d.HotkeyShortcuts.BROADCASTPAUSETOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 118]
      }, {
        catId: "Broadcast",
        name: "l10n.toggleCamera",
        hotkeyId: d.HotkeyShortcuts.CAMERATOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 117]
      }, {
        catId: "Broadcast",
        name: "l10n.commentHotkey",
        hotkeyId: d.HotkeyShortcuts.COMMENTSTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 122]
      }, {
        catId: "Broadcast",
        name: "l10n.toggleOverlay",
        hotkeyId: d.HotkeyShortcuts.OVERLAYTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !0,
        default: [18, 116]
      }, {
        catId: "BroadcastOverlays",
        name: r("translate")("l10n.switchOverlay", {
          overlayNumber: 1
        }),
        hotkeyId: d.HotkeyShortcuts.OVERLAYASWITCH,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !1,
        default: [0]
      }, {
        catId: "BroadcastOverlays",
        name: r("translate")("l10n.switchOverlay", {
          overlayNumber: 2
        }),
        hotkeyId: d.HotkeyShortcuts.OVERLAYBSWITCH,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !1,
        default: [0]
      }, {
        catId: "BroadcastOverlays",
        name: r("translate")("l10n.switchOverlay", {
          overlayNumber: 3
        }),
        hotkeyId: d.HotkeyShortcuts.OVERLAYCSWITCH,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: !1,
        default: [0]
      }, {
        catId: "Performance",
        name: "l10n.perfmonoc.togglePerfMenu",
        hotkeyId: d.HotkeyShortcuts.OCTOOLUITOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: N,
        default: [0]
      }, {
        catId: "Performance",
        name: "l10n.perfmonoc.togglePerfOverlay",
        hotkeyId: d.HotkeyShortcuts.PERFOVERLAYTOGGLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: N,
        default: [18, 82]
      }, {
        catId: "Performance",
        name: "l10n.perfmonoc.cyclePerfOverlay",
        hotkeyId: d.HotkeyShortcuts.PERFOVERLAYCYCLE,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: N,
        default: [0]
      }, {
        catId: "Performance",
        name: "l10n.perfmonoc.resetAverages",
        hotkeyId: d.HotkeyShortcuts.RESETAVERAGES,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: N && h.enableReflexEnhancements,
        default: [0]
      }, {
        catId: "Performance",
        name: "l10n.perfmonoc.toggleLogging",
        hotkeyId: d.HotkeyShortcuts.TOGGLELOGGING,
        hotkeyKeys: {},
        hotkeyStr: "",
        visible: N && h.enableReflexEnhancements,
        default: [0]
      }], A.updateNvCameraVisibility = function() {
        P = g.isOn(), D = g.isModsOn(), c.each(A.shortcuts, function(e) {
          e.visible && S(e)
        })
      }, A.initPreferencesKeyboardShortcuts = function() {
        w.isConnectEnabled().then(function(e) {
          A.isConnectEnabled = e, A.categories.Broadcast.visible = A.isConnectEnabled
        }).then(function() {
          l.on(y.PIPL_CONFIG_UPDATED, O);
          var t = [];
          t.push(d.setHotkeyMonitoringEnabled(!1).then(function(e) {}, function(e) {
            I.error("ERROR in disabling hotkey monitoring")
          })), t.push(f.getCustomOverlaySupportType().then(function(t) {
            2 === t && (A.categories.BroadcastOverlays.visible = A.isConnectEnabled, c.each(A.shortcuts,
              function(e) {
                "l10n.toggleOverlay" === e.name && (e.catId = "BroadcastOverlays"),
                  "BroadcastOverlays" === e.catId && (e.visible = A.isConnectEnabled)
              }));
            var n = ((0, a.default)(A.shortcuts).length, []);
            return c.each(A.shortcuts, function(e) {
              n.push(S(e))
            }), e.all(n)
          })), A.enableEscapeEvent(!0), t.push(d.getCurrentSettingsIR().then(function(e) {
            return A.minutesToSave = (e.replayLengthSeconds / 60).toFixed(1)
          })), e.all(t).then(function() {
            u && u.callback && u.callback()
          })
        })
      }, A.modifyShortcuts = function(t, n) {
        function i() {
          n && c.each(A.shortcuts, function(e) {
            S(e)
          }), M = [], o.resolve(!0)
        }
        var o = e.defer(),
          r = [];
        return M[0] = 0, c.each(A.shortcuts, function(e) {
          var n = t && "" === e.hotkeyStr,
            i = c.isEqual(e.hotkeyKeys, e.default),
            o = 0 === e.hotkeyKeys.length && 0 === e.default[0],
            a = e.visible === !1 && "None" === e.hotkeyStr,
            l = n || i || o || a;
          e == c.first(A.shortcuts) || l || r.push(d.setHotkeyShortcut(e.hotkeyId, t ? M : e.default))
        }), e.all(r).then(i, function(e) {
          I.error("Error while resetting hotkeys: ", e), o.resolve(!1)
        }), o.promise
      }, A.setDefaultShortcutForShare = function() {
        var t = c.first(A.shortcuts);
        return c.isEqual(t.hotkeyKeys, t.default) ? e.when() : void d.setHotkeyShortcut(t.hotkeyId, t.default)
          .then(function(e) {
            S(t)
          }, function(e) {
            I.error("ERROR in saving shortcut " + shortcut.hotkeyId), S(t)
          })
      }, A.resetShortcuts = function() {
        A.disableButtons = !0;
        var e = {
            title: "l10n.settings",
            icon: "icon-settings",
            status: "l10n.keyboardShortcuts",
            question: "l10n.resetMessage",
            footnote: "",
            topButton: "l10n.resetAll",
            bottomButton: "l10n.back",
            topAction: A.resetAllShortcuts,
            bottomAction: "",
            closeOSC: !1,
            lastState: "main.preferences.keyboard-shortcuts"
          },
          t = "main.confirmation";
        p.openOSC(t, e)
      }, A.resetAllShortcuts = function() {
        return d.setHotkeyMonitoringEnabled(!1).then(function(e) {
          return A.modifyShortcuts(!0, !1)
        }).then(function(e) {
          return A.setDefaultShortcutForShare()
        }).then(function() {
          return A.modifyShortcuts(!1, !0)
        }).then(function() {
          A.disableButtons = !1, i.go("main.preferences.keyboard-shortcuts")
        })
      }, A.isShortcutSelected = function(e) {
        return e === A.selectedShortCut
      }, A.selectShortcut = function(e) {
        A.selectedShortCut = e
      }, A.processKeyDownEvent = function(e) {
        if (A.selectedShortCut && !e.repeat && e.keyCode !== x.ENTER) {
          e.preventDefault(), e.stopImmediatePropagation(), e.stopPropagation(), A.errorTimeout && (o.cancel(A
            .errorTimeout), A.errorTimeout = null);
          var t = m.normalizeModifier(e.keyCode);
          M.indexOf(t) === -1 && M.push(t), I.debug("keyCode: " + e.keyCode + "(0x" + e.keyCode.toString(16) +
            "), key: " + e.key + ", normalized key: " + t + ", code: " + e.code + ", ctrlKey: " + e.ctrlKey +
            ", altKey: " + e.altKey + ", shiftKey: " + e.shiftKey + ", metaKey: " + e.metaKey + ", repeat: " + e
            .repeat + ", isModifierOnly: " + m.isModifierOnly(e))
        }
      }, A.processKeyUpEvent = function(e) {
        if (!R && A.selectedShortCut && e.keyCode !== x.ENTER) {
          R = !0, e.preventDefault(), e.stopImmediatePropagation(), e.stopPropagation();
          var t = m.normalizeModifier(e.keyCode);
          if (M.indexOf(t) === -1 && M.push(t), _(M)) M[0] = 0;
          else if (T(M)) return void C();
          if (A.errorTimeout && (o.cancel(A.errorTimeout), A.errorTimeout = null), A
            .showDuplicateShortcutMessage = !1, 0 !== M[0]) {
            var n = k(M);
            if (n !== -1 && A.shortcuts[n] === A.selectedShortCut) return void C();
            if (n !== -1) return C(), A.showDuplicateShortcutMessage = !0, A.errorTxt =
              "l10n.keyboardShortcutDuplicate", void(A.errorTimeout = o(function() {
                A.showDuplicateShortcutMessage = !1, A.errorTimeout = null
              }, A.errorDisplayTime))
          }
          var i = A.selectedShortCut;
          d.setHotkeyShortcut(i.hotkeyId, M).then(function(e) {
            S(i), C()
          }, function(e) {
            I.error("ERROR in saving shortcut " + i.hotkeyId, e && e.data ? e.data.message : e), e.data
              .code === d.SHADOWPLAY_ERRORCODES.ET_INVALID_DATA && (A.showDuplicateShortcutMessage = !0, A
                .errorTxt = "l10n.keyboardShortcutUsedByGFN", A.errorTimeout = o(function() {
                  A.showDuplicateShortcutMessage = !1, A.errorTimeout = null
                }, A.errorDisplayTime)), S(i), C()
          })
        }
      }, A.done = function() {
        i.go("main.preferences")
      }, A.enableEscapeEvent = function(e) {
        var t = e;
        o(function() {
          t === !0 ? l.on(b.ESCAPE, A.done) : l.off(b.ESCAPE, A.done)
        }, 200)
      }, g.hasVariableAvailability() && (A.updateNvCameraVisibility(), l.on(v.AVAILABILITY_CHANGED, A
        .updateNvCameraVisibility)), n.$on("$destroy", function() {
        A.errorTimeout && (o.cancel(A.errorTimeout), A.errorTimeout = null), A.enableEscapeEvent(!1), l.off(v
            .AVAILABILITY_CHANGED, A.updateNvCameraVisibility), l.off(y.PIPL_CONFIG_UPDATED, O), d
          .setHotkeyMonitoringEnabled(!0).then(function(e) {}, function(e) {
            I.error("ERROR in enabling hotkey monitoring")
          })
      })
    }
  ]);
  t.PreferencesKeyboardShortcutsController = f
}
