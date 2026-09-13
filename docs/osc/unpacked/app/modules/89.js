// ─────────────────────────────────────────────────────────────
// APP MODULE 89
// role       : service edgeDevKitService
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
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.edgeDevKitService = void 0;
  var o = n(3),
    r = (i(o), n(2)),
    a = r.ngMainCommonModule.service("edgeDevKitService", ["$log", "eventAggregator", "oscDisplayService", "cefService",
      "OSC_CONFIG", "HOTKEY_EVENTS",
      function(e, t, n, i, o, r) {
        function a(e) {
          "osd" == u ? (u = "off", n.closeForDisplay()) : (n.openInDisplay("main.edge"), "osc" == u && n.closeOSC(),
            u = "osd")
        }

        function l() {
          d.info("Registering EDGE DevKit notification events"), t.on(r.EDGE_OSD, a)
        }
        var s = this,
          d = e.getInstance("osc/edgeDevKitService"),
          c = o.enableEDGEDevKit,
          u = "off";
        s.isOn = function() {
          return c
        }, s.init = function() {
          d.info("Initialize EDGE DevKit Service"), l()
        }, s.enableUI = function(e) {
          e ? i.openCustomLayer() : i.closeCustomLayer()
        }
      }
    ]);
  t.edgeDevKitService = a
}
