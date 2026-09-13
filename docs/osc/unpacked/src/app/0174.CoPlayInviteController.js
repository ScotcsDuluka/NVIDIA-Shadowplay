// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 174
// controller CoPlayInviteController
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
  }), exports.CoPlayInviteController = void 0;
  var o = require(8),
    r = i(o),
    a = require(1) /* app/1 — main (module) */;
  require(33) /* app/33 — coplayService (service) */;
  var l = a.ngMainModule.controller("CoPlayInviteController", ["$scope", "$state", "$window",
    "eventAggregator", "coplayService", "KEYBOARD_EVENTS",
    function(e, t, n, i, o, a) {
      function l() {
        var e = n.localStorage.getItem(u + "emails");
        e && (c.emailList = JSON.parse(e));
        var t = n.localStorage.getItem(u + "name");
        t && (c.name = JSON.parse(t));
      }

      function s() {
        n.localStorage.setItem(u + "emails", (0, r.default)(c.emailList)), n.localStorage.setItem(u +
          "name", (0, r.default)(c.name));
      }

      function d(e) {
        var t = angular.lowercase(e);
        return function(e) {
          var n = angular.lowercase(e);
          return 0 === n.indexOf(t);
        };
      }
      var c = this;
      c.title = "l10n.stream", c.icon = "icon-stream", c.status = "l10n.inviteAFriend", c.name = "", c
        .email = "", c.emailSearch = "", c.emailList = null;
      var u = "coplay-invite-";
      c.querySearch = function(e) {
        if (!c.emailList) return [];
        var t = e ? c.emailList.filter(d(e)) : c.emailList;
        return t;
      }, c.invite = function() {
        if (!c.email) {
          var e = c.querySearch(c.emailSearch);
          "undefined" != typeof e && e && 0 !== e.length || (c.emailList || (c.emailList = []), c
            .emailList.push(c.emailSearch)), c.email = c.emailSearch;
        }
        var n = c.emailList.indexOf(c.email);
        for (n > -1 && c.emailList.splice(n, 1); c.emailList.length > 10;) c.emailList.pop();
        c.emailList.unshift(c.email), s(), o.createEmailSession(c.email, c.name), t.go(
        "main.main-menu");
      }, c.back = function() {
        t.go("main.main-menu");
      }, i.on(a.ESCAPE, c.back), e.$on("$destroy", function() {
        i.off(a.ESCAPE, c.back);
      }), l(), c.emailList && c.emailList[0] && (c.emailSearch = c.emailList[0], c.email = c.emailList[
        0]);
    }
  ]);
  exports.CoPlayInviteController = l;
}
