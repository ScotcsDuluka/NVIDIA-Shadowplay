// ─────────────────────────────────────────────────────────────
// APP MODULE 224
// role       : controller osdController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.osdController = void 0;
  var i = n(1);
  n(12), n(35), n(22);
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
            .isPerfOverlayVisible()), o.isPerfEnabledbyFileLogging && (c.visibility.PERFORMANCE = f, a = !0, u.info(
            "File logging is enabled and selected quandrant is ", f)), c.anythingVisible = a, e != c
          .anythingVisible && (c.anythingVisible ? i.openOSCForNotification() : i.closeOSCForNotification()), c
          .scaleFactor = 1 / (t.screen.availWidth / 1920), u.info("OSD scale factor is: " + c.scaleFactor)
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
      c.scaleFactor = 1, r.on(a.OSD_SETTINGS_CHANGED, d), r.on(l.PERF_OVERLAY_VISIBILITY_CHANGED, d)
    }
  ]);
  t.osdController = o
}
