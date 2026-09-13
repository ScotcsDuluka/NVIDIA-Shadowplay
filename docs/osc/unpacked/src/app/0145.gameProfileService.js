// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 145
// service gameProfileService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.gameProfileService = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.service("gameProfileService", ["$q", "$timeout", "eventAggregator",
      "shadowPlayEndpoints",
      function(e, t, n, i) {
        var o = this;
        o.DRSName = "", o.DRSProfileName = "";
        var r = Date.now(),
          a = 100;
        o.init = function() {
          var e = "broadcastSessionEventNotifier";
          n.on(e, o.broadcastSessionEventNotifier);
        }, o.updateDRSProfileInfo = function() {
          var t = Date.now();
          if (r + a > t) return e.when(null);
          r = Date.now();
          var n = 4294967295,
            l = i.getCaptureProcessInfo(n);
          return l().then(function(e) {
            o.DRSName = e.data.drsName || "", o.DRSProfileName = e.data.profileName || "";
          }, function(e) {
            o.DRSName = "", o.DRSProfileName = "";
          });
        }, o.getDRSInfo = function() {
          return {
            DRSName: o.DRSName,
            DRSProfileName: o.DRSProfileName
          };
        }, o.broadcastSessionEventNotifier = function(e) {
          if ("updateTitle" === e.sessionEvent) {
            if (r + a > Date.now()) return;
            t(o.updateDRSProfileInfo, a);
          }
        };
      }
    ]);
  exports.gameProfileService = o;
}
