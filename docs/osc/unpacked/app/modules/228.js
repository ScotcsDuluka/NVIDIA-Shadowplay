// ─────────────────────────────────────────────────────────────
// APP MODULE 228
// role       : controller OsdShadowplayStatusController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.OsdShadowplayStatusController = void 0;
  var i = n(1);
  n(35), n(4);
  var o = i.ngMainModule.controller("OsdShadowplayStatusController", ["$log", "$scope", "shadowPlayService",
    "eventAggregator", "osdService", "COMMON_EVENTS", "SHADOWPLAY_EVENTS", "HOTKEY_EVENTS",
    function(e, t, n, i, o, r, a, l) {
      function s() {
        m.showStatus = o.anythingCapturing, m.showStatus || (m.showHighlights = !1), m.showAny = m.showStatus
      }

      function d() {
        n.getMicCount().then(function(e) {
          n.getMicMode().then(function(t) {
            m.micMode = t, m.showMic = e > 0 && "alwayson" == t
          })
        })
      }

      function c() {
        "ptt" === m.micMode && (m.showMic = !0)
      }

      function u() {
        "ptt" === m.micMode && (m.showMic = !1)
      }

      function f(e) {
        e || e === !1 ? m.showHighlights = e : n.isHighlightsSessionActive().then(function(e) {
          m.showHighlights = e
        })
      }
      var m = this;
      m.showAny = !1, m.showStatus = !1, m.showMic = !1, m.micMode = "off", m.showHighlights = !1;
      e.getInstance("osc/ShadowplayStatusController");
      i.on(r.OSD_SETTINGS_CHANGED, s), i.on(a.MIC_STATUS_CHANGE, d), i.on(l.MIC_PTT_DOWN, c), i.on(l.MIC_PTT_UP, u),
        i.on(a.HIGHLIGHTS_STATUS_CHANGE, f), t.$on("$destroy", function() {
          i.off(r.OSD_SETTINGS_CHANGED, s), i.off(a.MIC_STATUS_CHANGE, d), i.off(l.MIC_PTT_DOWN, c), i.off(l
            .MIC_PTT_UP, u), i.off(a.HIGHLIGHTS_STATUS_CHANGE, f)
        }), d(), s(), f()
    }
  ]);
  t.OsdShadowplayStatusController = o
}
