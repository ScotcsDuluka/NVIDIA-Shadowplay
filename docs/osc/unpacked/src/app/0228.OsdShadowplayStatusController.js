// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 228
// controller OsdShadowplayStatusController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.OsdShadowplayStatusController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(35) /* app/35 — osdService (service) */, require(4) /* app/4 — shadowPlayService (service) */;
  var o = i.ngMainModule.controller("OsdShadowplayStatusController", ["$log", "$scope", "shadowPlayService",
    "eventAggregator", "osdService", "COMMON_EVENTS", "SHADOWPLAY_EVENTS", "HOTKEY_EVENTS",
    function(e, t, n, i, o, r, a, l) {
      function s() {
        m.showStatus = o.anythingCapturing, m.showStatus || (m.showHighlights = !1), m.showAny = m
          .showStatus;
      }

      function d() {
        n.getMicCount().then(function(e) {
          n.getMicMode().then(function(t) {
            m.micMode = t, m.showMic = e > 0 && "alwayson" == t;
          });
        });
      }

      function c() {
        "ptt" === m.micMode && (m.showMic = !0);
      }

      function u() {
        "ptt" === m.micMode && (m.showMic = !1);
      }

      function f(e) {
        e || e === !1 ? m.showHighlights = e : n.isHighlightsSessionActive().then(function(e) {
          m.showHighlights = e;
        });
      }
      var m = this;
      m.showAny = !1, m.showStatus = !1, m.showMic = !1, m.micMode = "off", m.showHighlights = !1;
      e.getInstance("osc/ShadowplayStatusController");
      i.on(r.OSD_SETTINGS_CHANGED, s), i.on(a.MIC_STATUS_CHANGE, d), i.on(l.MIC_PTT_DOWN, c), i.on(l
        .MIC_PTT_UP, u), i.on(a.HIGHLIGHTS_STATUS_CHANGE, f), t.$on("$destroy", function() {
        i.off(r.OSD_SETTINGS_CHANGED, s), i.off(a.MIC_STATUS_CHANGE, d), i.off(l.MIC_PTT_DOWN, c), i
          .off(l.MIC_PTT_UP, u), i.off(a.HIGHLIGHTS_STATUS_CHANGE, f);
      }), d(), s(), f();
    }
  ]);
  exports.OsdShadowplayStatusController = o;
}
