// ─────────────────────────────────────────────────────────────
// APP MODULE 172
// role       : controller CoPlayGuestControlsController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.CoPlayGuestControlsController = void 0;
  var i = n(1);
  n(33);
  var o = i.ngMainModule.controller("CoPlayGuestControlsController", ["$scope", "$state", "eventAggregator",
    "coplayService", "KEYBOARD_EVENTS", "OSC_KEYBOARD", "COPLAY_CONTROLLER_MAPPING",
    function(e, t, n, i, o, r, a) {
      var l = this;
      l.title = "l10n.stream", l.icon = "icon-stream", l.status = "l10n.guest", l.tiles = [{
        name: a.BLOCKED,
        title: "l10n.watchesMePlay",
        icon: "icon-guest_watch_me",
        initialFocus: !0
      }, {
        name: a.MIRRORED,
        title: "l10n.playsAsMe",
        icon: "icon-guest_play_as_me",
        initialFocus: !1
      }, {
        name: a.EXCLUSIVE_LOCAL_PRIORITY,
        title: "l10n.playsAlongsideMe",
        icon: "icon-guest_play_alongside_me",
        initialFocus: !1
      }], l.done = function() {
        i.setControllerMapping(l.selection), t.go("main.main-menu")
      }, l.back = function() {
        t.go("main.main-menu")
      }, l.keyUp = function(e, t) {
        e.keyCode === r.ENTER && (l.selection = t.name)
      }, n.on(o.ESCAPE, l.back), l.selection = i.getControllerMapping(), e.$on("$destroy", function() {
        n.off(o.ESCAPE, l.back)
      })
    }
  ]);
  t.CoPlayGuestControlsController = o
}
