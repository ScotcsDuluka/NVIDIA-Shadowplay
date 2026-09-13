// ─────────────────────────────────────────────────────────────
// APP MODULE 212
// role       : controller MicrophoneMenuController
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
  }), t.MicrophoneMenuController = void 0;
  var r = n(8),
    a = o(r),
    l = n(3),
    s = i(l),
    d = n(1);
  n(4);
  var c = d.ngMainModule.controller("MicrophoneMenuController", ["$scope", "$state", "$log", "$timeout",
    "eventAggregator", "shadowPlayService", "KEYBOARD_EVENTS",
    function(e, t, n, i, o, r, l) {
      var d = this;
      d.title = "l10n.microphone", d.icon = "icon-mic_on", d.status = "l10n.customize";
      var c = n.getInstance("main.microphone/microphonecontroller"),
        u = !1;
      d.disableBoost = !1, d.disableMicSelect = !1, d.disableVolume = !1, d.settingsChanged = !1, d.throttleTime =
        333, d.numOfMics = 0, d.Items = [], d.item = null, d.ItemsUponEntry = [], d.initialIndex = 0, d.sliders = {
          min: 0,
          max: 100,
          step: 1,
          ticks: [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100]
        }, d.save = function() {
          return c.info("Save clicked"), d.saveSettings().then(function() {
            d.settingsChanged = !1, t.go("main.main-menu")
          })
        }, d.saveSettings = function() {
          return c.info("Save Mic Settings"), r.setMicrophoneSettings(d.item).then(function() {
            d.settingsChanged = !0
          })
        }, d.undoSettings = function() {
          if (c.info("undoSettings Entered!"), d.settingsChanged === !0) {
            d.settingsChanged = !1;
            var e = null;
            s.forEach(d.ItemsUponEntry, function(t) {
              d.initialIndex !== t.index ? (d.item = t, c.info("Undoing index: " + d.item.index + " mic: " + d
                .item.name), d.saveSettings()) : e = t
            }), d.item = e, c.info("Undoing index: " + d.item.index + " mic: " + d.item.name), d.saveSettings()
          }
        }, d.back = function() {
          d.undoSettings(), t.go("main.main-menu")
        }, d.enableEscapeEvent = function(e) {
          var t = e;
          i(function() {
            t === !0 ? o.on(l.ESCAPE, d.back) : o.off(l.ESCAPE, d.back)
          }, 200)
        }, d.selectionOpen = function() {
          c.info("SelectionOpen item: ", d.item), d.enableEscapeEvent(!1)
        }, d.selectionClose = function(e) {
          c.info("SelectionClose item: ", d.item), d.checkBoost(), d.saveSettings(), u === !0 && d
            .enableEscapeEvent(!0)
        }, d.checkVolume = function() {
          u === !1 ? d.disableVolume = !0 : d.disableVolume = !1
        }, d.checkBoost = function() {
          u === !1 ? d.disableBoost = !0 : d.disableBoost = d.item.volumePercent < 100
        }, d.checkMicSelect = function() {
          u === !1 ? d.disableMicSelect = !0 : d.disableMicSelect = d.numOfMics < 2
        }, d.volumeChanged = s.throttle(function() {
          c.info("VolumeChanged to: ", d.item.volumePercent), d.checkBoost(), d.saveSettings()
        }, d.throttleTime, {
          leading: !1
        }), d.boostChanged = s.throttle(function() {
          c.info("BoostChanged to: ", d.item.boostPercent), d.saveSettings()
        }, d.throttleTime, {
          leading: !1
        }), d.initialize = function() {
          c.info("Initialize Microphone VM"), r.getMicrophoneSettingsAll().then(function(e) {
            d.numOfMics = e.length, d.Items = e, d.ItemsUponEntry = JSON.parse((0, a.default)(e))
          }).then(function() {
            return r.getMicrophoneSettings()
          }).then(function(e) {
            d.item = e, d.initialIndex = d.item.index, d.enableEscapeEvent(!0), u = !0, d.checkMicSelect(), d
              .checkBoost(), d.checkVolume(), c.info("Microphone initialized")
          }, function(e) {
            c.error("Microphones did not init properly ", e), d.checkMicSelect(), d.checkBoost(), d
            .checkVolume()
          })
        }, d.initialize(), e.$on("$destroy", function() {
          d.enableEscapeEvent(!1), d.undoSettings(), u = !1
        })
    }
  ]);
  t.MicrophoneMenuController = c
}
