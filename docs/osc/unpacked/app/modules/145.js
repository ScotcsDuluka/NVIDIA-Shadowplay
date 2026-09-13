// ─────────────────────────────────────────────────────────────
// APP MODULE 145
// role       : service gameProfileService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.gameProfileService = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.service("gameProfileService", ["$q", "$timeout", "eventAggregator", "shadowPlayEndpoints",
      function(e, t, n, i) {
        var o = this;
        o.DRSName = "", o.DRSProfileName = "";
        var r = Date.now(),
          a = 100;
        o.init = function() {
          var e = "broadcastSessionEventNotifier";
          n.on(e, o.broadcastSessionEventNotifier)
        }, o.updateDRSProfileInfo = function() {
          var t = Date.now();
          if (r + a > t) return e.when(null);
          r = Date.now();
          var n = 4294967295,
            l = i.getCaptureProcessInfo(n);
          return l().then(function(e) {
            o.DRSName = e.data.drsName || "", o.DRSProfileName = e.data.profileName || ""
          }, function(e) {
            o.DRSName = "", o.DRSProfileName = ""
          })
        }, o.getDRSInfo = function() {
          return {
            DRSName: o.DRSName,
            DRSProfileName: o.DRSProfileName
          }
        }, o.broadcastSessionEventNotifier = function(e) {
          if ("updateTitle" === e.sessionEvent) {
            if (r + a > Date.now()) return;
            t(o.updateDRSProfileInfo, a)
          }
        }
      }
    ]);
  t.gameProfileService = o
}
