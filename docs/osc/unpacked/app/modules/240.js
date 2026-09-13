// ─────────────────────────────────────────────────────────────
// APP MODULE 240
// role       : controller PreferencesHangoutController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  n(1);
  angular.module("main").controller("PreferencesHangoutController", ["$state", "$log", function(e, t) {
    var n = this;
    t.getInstance("main.preferences/preferenceshangoutcontroller");
    gapi.hangout.render("hangout-div", {
      render: "createhangout",
      hangout_type: "normal",
      initial_apps: [{
        app_id: "184219133185",
        start_data: "dQw4w9WgXcQ",
        app_type: "ROOM_APP"
      }],
      widget_size: 175
    }), gapi.hangout.render("hangoutonair-div", {
      render: "createhangout",
      hangout_type: "onair",
      initial_apps: [{
        app_id: "184219133185",
        start_data: "dQw4w9WgXcQ",
        app_type: "ROOM_APP"
      }],
      widget_size: 175
    }), n.done = function() {
      e.go("main.preferences")
    }
  }])
}
