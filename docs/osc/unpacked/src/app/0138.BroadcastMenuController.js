// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 138
// controller BroadcastMenuController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.BroadcastMenuController = void 0;
  var o = require(3),
    r = i(o),
    a = require(1) /* app/1 — main (module) */;
  require(32) /* app/32 — broadcastService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(49) /* app/49 — ugcService (service) */;
  var l = a.ngMainModule.controller("BroadcastMenuController", ["$scope", "$state", "$log",
    "broadcastService", "ugcService", "shadowPlayService", "oscNotificationService", "eventAggregator",
    "KEYBOARD_EVENTS", "NOTIFIER_SELECTIONS",
    function(e, t, n, i, o, a, l, s, d, c) {
      var u = this,
        f = n.getInstance("osc/BroadcastMenuController");
      u.title = "l10n.broadcastLive", u.icon = "icon-broadcast", u.status = "l10n.start", u.portals = i
        .portals;
      var m = 100;
      u.titleMaxLength = m, u.isLoggedIn = void 0, u.broadcastTitle = void 0, u.selectedPrivacy = void 0,
        u.selectedLocationType = void 0, u.selectedLocation = void 0, u.serviceType = "", u
        .uploadService = "", u.pickerData = "", u.destinationDataChange = function(e) {
          u.isLoggedIn = e.isLoggedIn, u.broadcastTitle = e.uploadTitle, u.selectedPrivacy = e
            .selectedPrivacy, u.selectedLocationType = e.selectedLocationType, u.selectedLocation = e
            .selectedLocation, u.serviceType = e.serviceType, u.uploadService = e.uploadService;
        }, u.login = function(e) {
          if (f.info("Login from broadcast controller: ", e.name), o.checkJarvisLoginRequirement(e.name))
            l.show(c.CONNECT_LOGIN_TO_GFE);
          else if (o.checkIfBrowserLogin(e.name)) a.getDesktopCaptureEnabled().then(function(n) {
            o.logInFromBrowser(e.name, t.current.name, t.params, !n);
          });
          else {
            var n = {
                title: u.title,
                icon: u.icon,
                status: u.status
              },
              i = {
                service: e,
                oscTileParams: n,
                lastState: t.current.name,
                lastParams: t.params
              };
            t.go("main.oauth-menu", i);
          }
        }, u.updatePicker = function(e) {
          u.serviceType = o.connectService.serviceTypes.STREAMING, u.pickerData = {
            fileToUpload: void 0,
            serviceType: u.serviceType
          };
        }, u.broadcastDisabled = function() {
          return !u.isLoggedIn;
        }, u.start = function() {
          if (null != u.uploadService) {
            var e = r.findWhere(i.broadcastPortalPreferences, {
                id: u.uploadService.id
              }),
              n = {};
            n.title = u.broadcastTitle, n.privacy = u.selectedPrivacy, n.destinationType = u
              .selectedLocationType, n.destination = u.selectedLocation, i.setupAndTriggerBroadcast(e, n),
              o.connectService.setLastUsedService(u.serviceType, u.uploadService.name), o.connectService
              .setLastUploadedVideoType(o.connectService.serviceTypes.GIF_UPLOAD), t.go("main.main-menu");
          }
        }, u.back = function() {
          t.go("main.main-menu");
        }, u.initialize = function() {
          u.updatePicker(), s.on(d.ESCAPE, u.back);
        }, u.initialize(), e.$on("$destroy", function() {
          s.off(d.ESCAPE, u.back);
        });
    }
  ]);
  exports.BroadcastMenuController = l;
}
