// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 166
// controller NvOauthMenuController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.NvOauthMenuController = void 0;
  var i = require(1) /* app/1 — main (module) */,
    o = i.ngMainModule.controller("NvOauthMenuController", ["$scope", "$stateParams", "$log",
      "eventAggregator", "oscDisplayService", "KEYBOARD_EVENTS", "CONNECT_EVENTS",
      function(e, t, n, i, o, r, a) {
        var l = this;
        l.title = "l10n.settings", l.icon = "icon-settings", l.status = "", l.oauthDialogueParams = {};
        n.getInstance("osc/nvOauthMenuController");
        l.onPopupClose = function() {
          var e = t.lastParams;
          angular.isUndefined(e) && (e = {
            service: t.service
          }), o.openOSC(t.lastState, e);
        }, l.cancel = function() {
          l.oauthDialogueParams.openPopup = !1;
        }, l.initialize = function() {
          l.oauthDialogueParams = {
            serviceName: t.service.name,
            openPopup: !0
          }, angular.isDefined(t.oscTileParams) && (l.title = t.oscTileParams.title, l.icon = t
            .oscTileParams.icon, l.status = t.oscTileParams.status);
        }, i.on(r.ESCAPE, l.cancel), i.on(a.LOGIN_BLOCKED, l.cancel), e.$on("$destroy", function() {
          i.off(r.ESCAPE, l.cancel), i.off(a.LOGIN_BLOCKED, l.cancel);
        }), l.initialize();
      }
    ]);
  exports.NvOauthMenuController = o;
}
