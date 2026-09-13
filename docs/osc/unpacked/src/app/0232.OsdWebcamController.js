// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 232
// controller OsdWebcamController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.OsdWebcamController = void 0;
  var o = require(106),
    r = i(o),
    a = require(1) /* app/1 — main (module) */,
    l = a.ngMainModule.controller("OsdWebcamController", ["$log", "$scope", "$window", "osdService",
      "eventAggregator", "COMMON_EVENTS",
      function(e, t, n, i, o, a) {
        function l() {
          var e = i.overlaySettings.Camera,
            t = e.size,
            o = 1;
          "Small" === t ? o = 12 : "Medium" === t ? o = 8 : "Large" === t && (o = 4), s.width = (0, r
            .default)(n.screen.availWidth / o), s.height = (0, r.default)(3 * s.width / 4);
        }
        var s = this;
        e.getInstance("osc/WebcamController");
        s.width = 0, s.height = 0, s.getSize = function() {
          return {
            width: s.width + "px",
            height: s.height + "px"
          };
        }, o.on(a.OSD_SETTINGS_CHANGED, l), t.$on("$destroy", function() {
          o.off(a.OSD_SETTINGS_CHANGED, l);
        }), l();
      }
    ]);
  exports.OsdWebcamController = l;
}
