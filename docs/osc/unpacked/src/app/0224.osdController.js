// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 224
// controller osdController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.osdController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(12) /* app/12 — oscDisplayService (service) */, require(35) /* app/35 — osdService (service) */, require(22) /* app/22 — octoolService (service) */;
  var o = i.ngMainModule.controller("osdController", ["$log", "$window", "osdService", "oscDisplayService",
    "octoolService", "eventAggregator", "COMMON_EVENTS", "PERFTOOL_EVENTS", "OSC_CONFIG",
    function(e, t, n, i, o, r, a, l, s) {
      function d() {
        c.visibility = {};
        var e = c.anythingVisible,
          a = !1,
          s = n.overlaySettings.Status;
        s.enabled && (c.visibility.STATUS = s.position);
        var d = n.overlaySettings.FPS;
        d.enabled && (c.visibility.FPS = d.position);
        var m = n.overlaySettings.Viewers;
        m.enabled && (c.visibility.VIEWERS = m.position, a = !0);
        var g = n.overlaySettings.Camera;
        g.enabled && (c.visibility.WEBCAM = g.position);
        var p = n.overlaySettings.Comments;
        p.enabled && (c.visibility.COMMENTS = p.position, a = !0), a = a && n.anythingBroadcasting && n
          .broadcastHasOSD, o.isAnyOtherOSDVisible = a, u.info(
            "Sending PERF_OVERLAY_OTHER_OSD_VISIBILITY_CHANGED from - OSD controller"), r.trigger(l
            .PERF_OVERLAY_OTHER_OSD_VISIBILITY_CHANGED);
        var h = n.overlaySettings.Performance;
        h.enabled && (c.visibility.PERFORMANCE = h.position, f = c.visibility.PERFORMANCE, a = a || o
            .isPerfOverlayVisible()), o.isPerfEnabledbyFileLogging && (c.visibility.PERFORMANCE = f, a = !
            0, u.info("File logging is enabled and selected quandrant is ", f)), c.anythingVisible = a,
          e != c.anythingVisible && (c.anythingVisible ? i.openOSCForNotification() : i
            .closeOSCForNotification()), c.scaleFactor = 1 / (t.screen.availWidth / 1920), u.info(
            "OSD scale factor is: " + c.scaleFactor);
      }
      var c = this,
        u = e.getInstance("osc/OsdController");
      c.visibility = {}, c.anythingVisible = !1, c.enabled = s.osd || !1, c.quadrants = {
        LeftTop: "osd-topleft",
        RightTop: "osd-topright",
        LeftBottom: "osd-bottomleft",
        RightBottom: "osd-bottomright"
      };
      var f = "RightTop";
      c.scaleFactor = 1, r.on(a.OSD_SETTINGS_CHANGED, d), r.on(l.PERF_OVERLAY_VISIBILITY_CHANGED, d);
    }
  ]);
  exports.osdController = o;
}
