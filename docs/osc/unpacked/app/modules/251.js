// ─────────────────────────────────────────────────────────────
// APP MODULE 251
// role       : directive nvPreferencesNotifications
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvPreferencesNotifications = void 0;
  var o = n(1),
    r = n(360),
    a = i(r);
  n(250), n(5), n(6);
  var l = o.ngMainModule.directive("nvPreferencesNotifications", function() {
    return {
      restrict: "E",
      template: a.default,
      controller: "PreferencesNotificationsController",
      controllerAs: "controller"
    }
  });
  t.nvPreferencesNotifications = l
}
