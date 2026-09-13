// ─────────────────────────────────────────────────────────────
// APP MODULE 234
// role       : controller PreferencesAudioController
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
  var o = n(107),
    r = i(o),
    a = n(8),
    l = i(a),
    s = n(1);
  n(4), n(26);
  s.ngMainModule.controller("PreferencesAudioController", ["$log", "$scope", "$state", "$timeout", "$filter",
    "shadowPlayService", "eventAggregator", "$document", "telemetryService", "KEYBOARD_EVENTS", "OSC_KEYBOARD",
    "AUDIO_STATE", "TELEMETRY_OSC_EVENT_NAMES",
    function(e, t, n, i, o, a, s, d, c, u, f, m, g) {
      var p = this;
      p.disableBoost = !1, p.disableMicSelect = !1, p.settingsChanged = !1, p.disableTrackChange = !1, p
        .throttleTime = 333, p.displayWarningMessage = !1, p.numOfMics = 0, p.disableMics = !0, p.Items = [], p
        .item = null, p.ItemsUponEntry = [], p.initialIndex = 0, p.audioSettings = {}, p
        .audioSettingsUponEntry = {}, p.singleTrack = !0, p.undoCount = 0, p.title = "l10n.settings", p.icon =
        "icon-settings", p.status = "", t.textVolume = "l10n.volume", t.textSource = "l10n.source", t.textBoost =
        "l10n.boost", t.leftWidth = "", t.rightWidth = "";
      var h = e.getInstance("main.preferences.audio"),
        b = !1;
      p.noMic = {
        index: 0,
        name: o("translate")("l10n.noMic"),
        id: "",
        muted: !0,
        volumePercent: 0,
        boostPercent: 0
      }, p.setupDisplay = function() {
        p.sliders = {
          min: 0,
          max: 100,
          step: 1,
          ticks: [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100]
        }, p.tracks = {
          single: {
            name: "single",
            title: "l10n.singleTrack",
            value: !0,
            margin: "mta-track-button1"
          },
          multi: {
            name: "multi",
            title: "l10n.separateTrack",
            value: !1,
            margin: "mta-track-button2"
          }
        }
      }, p.return = function() {
        p.fromCustomizeEntry() ? n.go("main.main-menu") : n.go("main.preferences")
      }, p.save = function() {
        return h.info("Save clicked"), p.saveSettings(!0).then(function() {
          return p.saveAudioSettings(!0)
        }).then(function() {
          p.settingsChanged = !1, p.return()
        })
      }, p.back = function() {
        p.fromCustomizeEntry() && p.undoSettings(), p.return()
      }, p.enableEscapeEvent = function(e) {
        var t = e;
        i(function() {
          t === !0 ? s.on(u.ESCAPE, p.back) : s.off(u.ESCAPE, p.back)
        }, 200)
      }, p.fromCustomizeEntry = function() {
        return a.getAudioState() === m.MAIN
      }, p.saveSettings = function(e) {
        return h.info("Save Mic Settings: ", p.item), a.setMicrophoneSettings(p.item).then(function() {
          p.settingsChanged = e, h.info("Save mic settings done")
        })
      }, p.saveAudioSettings = function(e) {
        return h.info("Save Audio Settings: ", p.audioSettings), a.setAudioSettings(p.audioSettings).then(
          function() {
            p.settingsChanged = e, h.info("Save audio settings done")
          })
      }, p.undoSettings = function() {
        if (0 === p.undoCount && (p.undoCount++, h.info("undoSettings Entered!"), p.settingsChanged === !0)) {
          h.info("undoing Settings!");
          var e = null;
          _.forEach(p.ItemsUponEntry, function(t) {
            p.initialIndex !== t.index ? (p.item = t, p.saveSettings(!1)) : e = t
          }), p.item = e, p.saveSettings(!1), p.audioSettings = p.audioSettingsUponEntry, p.saveAudioSettings(!
            1).then(function() {
            p.settingsChanged = !1, h.info("Undo settings done")
          })
        }
      }, p.audioTrackSelect = function() {
        p.audioSettings.separateTracks = !p.singleTrack, h.info("audioSettings: ", p.audioSettings), p
          .saveAudioSettings(!0)
      }, p.mouseEnter = function() {
        p.displayWarningMessage = p.disableTrackChange
      }, p.mouseLeave = function() {
        p.displayWarningMessage = !1
      }, p.keyUp = function(e, t) {
        e.keyCode === f.ENTER && p.audioTrackSelect()
      }, p.selectionOpen = function() {
        h.info("SelectionOpen item: ", p.item), p.enableEscapeEvent(!1)
      }, p.selectionClose = function() {
        h.info("SelectionClose item: ", p.item), p.checkBoost(), p.saveSettings(!0), b === !0 && p
          .enableEscapeEvent(!0)
      }, p.checkBoost = function() {
        p.disableBoost = p.item.volumePercent < 100
      }, p.checkMicSelect = function() {
        b === !1 ? p.disableMicSelect = !0 : p.disableMicSelect = p.numOfMics < 2
      }, p.volumeChanged = _.throttle(function() {
        h.info("VolumeChanged to: ", p.item.volumePercent), p.checkBoost(), p.saveSettings(!0)
      }, p.throttleTime, {
        leading: !1
      }), p.systemVolChanged = _.throttle(function() {
        h.info("SystemVolChanged to: ", p.audioSettings.systemVolumePercent), p.saveAudioSettings(!0)
      }, p.throttleTime, {
        leading: !1
      }), p.boostChanged = _.throttle(function() {
        h.info("BoostChanged to: ", p.item.boostPercent), p.saveSettings(!0)
      }, p.throttleTime, {
        leading: !1
      }), p.checkRunState = function() {
        if (p.disableTrackChange !== !1) {
          var e = a.runState;
          p.singleTrack === !0 ? e.highlights ? a.appCaptureProcessInfo().then(function(e) {
              p.message = o("translate")("l10n.warningAudioHlT1", {
                arg1: e.profileName
              })
            }) : e.instantReplay ? p.message = o("translate")("l10n.warningAudioIrT1") : e.manualRecord ? p
            .message = o("translate")("l10n.warningAudioMrT1") : e.broadcast && (p.message = o("translate")(
              "l10n.warningAudioBrT1")) : e.highlights ? a.appCaptureProcessInfo().then(function(e) {
              p.message = o("translate")("l10n.warningAudioHlT2", {
                arg1: e.profileName
              })
            }) : e.instantReplay ? p.message = o("translate")("l10n.warningAudioIrT2") : e.manualRecord ? p
            .message = o("translate")("l10n.warningAudioMrT2") : e.broadcast && (p.message = o("translate")(
              "l10n.warningAudioBrT2"))
        }
      }, p.initialize = function() {
        h.info("Initialize Audio Preferences"), a.getMicrophoneSettingsAll().then(function(e) {
          p.numOfMics = e.length, p.Items = e, p.ItemsUponEntry = JSON.parse((0, l.default)(e)), p
            .disableMics = 0 === p.numOfMics
        }).then(function() {
          return a.getMicrophoneSettings()
        }).then(function(e) {
          return p.item = p.disableMics ? p.noMic : e, p.initialIndex = p.item.index, a.getAudioSettings()
        }).then(function(e) {
          return p.audioSettings = e, p.audioSettingsUponEntry = (0, r.default)({}, p.audioSettings), a
            .checkCustomize(!1)
        }).then(function(e) {
          h.info("MTA Track buttons enabled: ", !e), p.disableTrackChange = !e, p.singleTrack = !p
            .audioSettings.separateTracks, p.checkRunState(), p.enableEscapeEvent(!0), p.setupDisplay(),
            angular.element(d[0].getElementById("firstSlider")).focus(), b = !0, p.checkMicSelect(), p
            .checkBoost(), h.info("Microphone initialized")
        }, function(e) {
          h.error("Microphones did not init properly ", e), p.disableMics = !0, p.checkMicSelect(), p
            .checkBoost()
        })
      }, p.initialize(), t.$on("$destroy", function() {
        p.enableEscapeEvent(!1), p.fromCustomizeEntry() && p.undoSettings(), b === !0 && (c.push(g
          .OSC_MTA_SETTINGS, {
            systemVolume: p.audioSettings.systemVolumePercent,
            micVolume: p.item.volumePercent,
            micBoost: p.item.boostPercent,
            micSrc: p.item.name,
            isMultiTrack: c.toTelemetryBoolean(p.audioSettings.separateTracks)
          }), b = !1)
      })
    }
  ])
}
