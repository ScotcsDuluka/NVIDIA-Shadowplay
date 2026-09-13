// ─────────────────────────────────────────────────────────────
// APP MODULE 230
// role       : controller OsdViewerCountController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.OsdViewerCountController = void 0;
  var i = n(1);
  n(35);
  var o = i.ngMainModule.controller("OsdViewerCountController", ["$log", "$scope", "$element", "$timeout",
    "eventAggregator", "osdService", "shadowPlayEndpoints", "COMMON_EVENTS", "SHADOWPLAY_EVENTS", "CONNECT_EVENTS",
    function(e, t, n, i, o, r, a, l, s, d) {
      function c() {
        var e = document.createElement("CANVAS"),
          t = e.getContext("2d"),
          i = n[0].parentElement.offsetWidth - 5,
          o = n[0].parentElement.offsetHeight - 5;
        if (!(i === b.savedWidth || i < 0)) {
          b.savedWidth = i, i * o > 6e3 && (i = 200, o = 30), t.clearRect(0, 0, i, o);
          var r = t.getImageData(0, 0, i, o),
            l = new Blob([r.data]),
            s = new FileReader;
          s.addEventListener("loadend", function() {
            var e = String.fromCharCode.apply(null, new Uint8Array(s.result));
            a.updateBroadcastViewerCountImage({}, {
              ViewerCountImage: e,
              ImageHeight: o,
              ImageWidth: i
            }), l = null, s = null
          }), s.readAsArrayBuffer(l)
        }
      }

      function u() {
        b.visible && i(c)
      }

      function f() {
        b.visible || (b.reactionCount = 0, b.commentCount = 0, b.viewerCount = 0, b.savedWidth = 0), b.visible = r
          .anythingBroadcasting, u()
      }

      function m(e) {
        b.viewerCount = e, u()
      }

      function g(e) {
        if (b.reactionCount = 0, b.reactions = [], e && e.NONE) {
          b.reactionCount = e.NONE;
          var t = _.keys(e).sort(function(t, n) {
            return e[n] - e[t]
          });
          b.reactions = [];
          for (var n = 0; n < 3; n++) {
            for (var i = t.shift(); i && ("NONE" === i || "THANKFUL" === i);) i = t.shift();
            if (!i || e[i] <= 0) break;
            b.reactions.unshift("fbReaction:" + i.toLowerCase())
          }
          u()
        }
      }

      function p(e) {
        e && e.summary && e.summary.total_count && (b.commentCount = e.summary.total_count), u()
      }

      function h() {
        var e = r.getViewerCount();
        0 !== e && m(e);
        var t = r.getBroadcastComments();
        void 0 !== t && p(t);
        var n = r.getReactions();
        void 0 !== n && g(n), f()
      }
      var b = this;
      b.visible = !1, b.reactionCount = 0, b.reactions = [], b.commentCount = 0, b.savedWidth = 0, b.viewerCount =
      0;
      e.getInstance("osc/ViewerCountController");
      o.on(l.OSD_SETTINGS_CHANGED, f), o.on(s.VIEWER_COUNT_UPDATE, m), o.on(d.FACEBOOK_REACTIONS, g), o.on(d
        .FACEBOOK_COMMENTS, p), h(), t.$on("$destroy", function() {
        o.off(l.OSD_SETTINGS_CHANGED, f), o.off(s.VIEWER_COUNT_UPDATE, m), o.off(d.FACEBOOK_REACTIONS, g), o
          .off(d.FACEBOOK_COMMENTS, p)
      })
    }
  ]);
  t.OsdViewerCountController = o
}
