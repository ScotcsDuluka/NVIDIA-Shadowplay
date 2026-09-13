// ─────────────────────────────────────────────────────────────
// APP MODULE 47
// role       : service hotkeyService
// requires   : (none)
// channels   : /ShadowPlay/v.1.0/Hotkey
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.hotkeyService = void 0;
  var i = n(2);
  n(20);
  var o = i.ngMainCommonModule.service("hotkeyService", ["$log", "socketService", "eventAggregator", "HOTKEY_EVENTS",
    "OSC_CONFIG", "shadowPlayService", "OSC_KEYBOARD", "$document", "$q",
    function(e, t, n, i, o, r, a, l, s) {
      function d(e) {
        var t = null;
        return g.info("Hotkey: ", e.hotkeyId), o.enableEDGEDevKit && ("DVR" === e.hotkeyId ? t = i.EDGE_OSD :
          "Manual" === e.hotkeyId && (t = i.EDGE_OSC), t) ? void n.trigger(t, null) : ("DVR" === e.hotkeyId ? t =
          i.DVR : "Manual" === e.hotkeyId ? t = i.MANUAL : "GameCast" === e.hotkeyId ? t = i.GAMECAST :
          "MicPTT" === e.hotkeyId ? "Down" === e.hotkeyState ? t = i.MIC_PTT_DOWN : "Up" === e.hotkeyState && (t =
            i.MIC_PTT_UP) : "Camera" === e.hotkeyId ? t = i.CAMERA : "PauseResume" === e.hotkeyId ? t = i
          .PAUSERESUME : "FPS" === e.hotkeyId ? t = i.FPS : "OSC" === e.hotkeyId ? t = i.OSC_TOGGLE :
          "Screenshot" === e.hotkeyId ? t = i.SCREENSHOT : "CustomOverlay" === e.hotkeyId ? t = i.CUSTOMOVERLAY :
          "NvCameraUI" === e.hotkeyId ? t = i.NVCAMERAUI : "CustomOverlayA" === e.hotkeyId ? t = i
          .CUSTOMOVERLAYA : "CustomOverlayB" === e.hotkeyId ? t = i.CUSTOMOVERLAYB : "CustomOverlayC" === e
          .hotkeyId ? t = i.CUSTOMOVERLAYC : "CommentsToggle" === e.hotkeyId ? t = i.COMMENTS_TOGGLE :
          "DVRToggle" === e.hotkeyId ? t = i.DVR_TOGGLE : "MicToggle" === e.hotkeyId ? t = i.MIC_TOGGLE :
          "ModsToggle" === e.hotkeyId ? t = i.MODS_TOGGLE : "ModsPresetCycle" === e.hotkeyId ? t = i.MODS_CYCLE :
          "ModsUI" === e.hotkeyId ? t = i.MODS_SHOWUI : "ModsPreset1" === e.hotkeyId ? t = i.MODS_PRESET1 :
          "ModsPreset2" === e.hotkeyId ? t = i.MODS_PRESET2 : "ModsPreset3" === e.hotkeyId ? t = i.MODS_PRESET3 :
          "PMOCSidebar" === e.hotkeyId ? t = i.OCTOOLUI_TOGGLE : "PMOCOverlay" === e.hotkeyId ? t = i
          .PERFOVERLAY_TOGGLE : "PMOCOverlayCycle" === e.hotkeyId ? t = i.PERFOVERLAY_CYCLE :
          "PMOCResetAverageMetrics" === e.hotkeyId ? t = i.RESET_AVERAGES : "PMOCLoggingToggle" === e.hotkeyId &&
          (t = i.TOGGLE_LOGGING), void(t && n.trigger(t, null)))
      }

      function c(e, t) {
        this.hotKey = [], this.hotKeyCount = 0, this.keyMatchCount = 0, this.invalidSequence = !1, this.totrigger =
          t, this.hotKeyName = e;
        var i = this;
        e in u.nonSPHK ? (i.hotKey = u.nonSPHK[e], i.hotKeyCount = i.hotKey.length) : r.getHotkeyShortcut(e).then(
          function(e) {
            _.isUndefined(e.keys) || (i.hotKey = e.keys), i.hotKeyCount = i.hotKey.length
          }), this.onKeyDownEvent = function(e) {
          var t = !1;
          t = i.monitorHotKey(e, !0), t && (g.info("HotKey detected", i.hotKeyName), n.trigger(i.totrigger, null))
        }, this.onKeyUpEvent = function(e) {
          i.monitorHotKey(e, !1)
        }, this.stopHotKeyDetection = function() {
          return l[0].removeEventListener("keyup", i.onKeyUpEvent), l[0].removeEventListener("keydown", i
            .onKeyDownEvent), u.hotKeyMapping[i.hotKeyName] ? r.dynamicHotkeyToggle([u.hotKeyMapping[i
            .hotKeyName]], !0, 100) : s.when(!0)
        }, this.startHotkeyDetection = function() {
          var e;
          return e = u.hotKeyMapping[i.hotKeyName] ? r.dynamicHotkeyToggle([u.hotKeyMapping[i.hotKeyName]], !1,
            0) : s.when(!0), e.then(function() {
              l[0].addEventListener("keyup", i.onKeyUpEvent), l[0].addEventListener("keydown", i.onKeyDownEvent)
            })
        }, this.monitorHotKey = function(e, t) {
          if (0 == this.hotKeyCount || 0 == this.hotKey.length) return !1;
          var n = !1,
            i = e.keyCode;
          switch (t) {
            case !0:
              if (18 != i && 164 != i && 165 != i || (i = 18), 17 != i && 162 != i && 163 != i || (i = 17), 16 !=
                i && 160 != i && 161 != i || (i = 16), !this.invalidSequence && !this.keyMatchCount && this
                .hotKey[this.hotKeyCount - 1] == i)
                for (var o = 0; o < this.hotKeyCount - 1; o++) 18 == this.hotKey[o] && e.altKey && this
                  .keyMatchCount++, 17 == this.hotKey[o] && e.ctrlKey && this.keyMatchCount++, 16 == this.hotKey[
                    o] && e.shiftKey && this.keyMatchCount++;
              if (this.hotKey[this.keyMatchCount] == i) {
                this.keyMatchCount++, this.keyMatchCount == this.hotKeyCount && (n = !0, this.keyMatchCount = 0,
                  this.invalidSequence = !1);
                break
              }
              if (this.keyMatchCount) {
                for (var r = !1, o = 0; o < this.keyMatchCount; o++) r = this.hotKey[o] == i;
                if (r) break
              }
              this.invalidSequence = !0;
            case !1:
              this.keyMatchCount = 0
          }
          return n
        }
      }
      var u = this,
        f = "/ShadowPlay/v.1.0/Hotkey",
        m = "hotkeyEvent",
        g = e.getInstance("osc/hotkeyService");
      u.init = function() {
        return t.register(f, m), n.on(m, d), !0
      }, u.hotKeyMapping = {
        OpenShare: "openshare",
        PTT: "ptt",
        FPS: "fps",
        Screenshot: "screenshot",
        RecordSave: "recordsave",
        RecordToggle: "recordtoggle",
        BroadcastToggle: "broadcasttoggle",
        BroadcastPauseToggle: "broadcastpausetoggle",
        CameraToggle: "cameratoggle",
        OverlayToggle: "overlaytoggle",
        NvCameraUI: "nvcameraui",
        OverlayASwitch: "overlayaswitch",
        OverlayBSwitch: "overlaybswitch",
        OverlayCSwitch: "overlaycswitch",
        CommentsToggle: "commentstoggle",
        DVRToggle: "dvrtoggle",
        MicToggle: "mictoggle",
        ModsToggle: "modstoggle",
        ModsPresetCycle: "modspresetcycle",
        ModsUI: "modsui",
        ModsPreset1: "modspreset1",
        ModsPreset2: "modspreset2",
        ModsPreset3: "modspreset3",
        OcToolUI: "pmocsidebar",
        PerfOverlayToggle: "pmocoverlay",
        PerfOverlayCycle: "pmocoverlaycycle",
        ResetAverages: "pmocresetaveragemetrics",
        ToggleLogging: "pmocloggingtoggle"
      }, u.nonSPHK = {
        anselHideUIHK: [a.INSERT]
      }, u.createHotKey = function(e, t) {
        return new c(e, t)
      }
    }
  ]);
  t.hotkeyService = o
}
