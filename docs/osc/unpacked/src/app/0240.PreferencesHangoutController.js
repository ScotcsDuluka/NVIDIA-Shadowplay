// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 240
// controller PreferencesHangoutController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  require(1) /* app/1 — main (module) */;
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
      e.go("main.preferences");
    };
  }]);
}
