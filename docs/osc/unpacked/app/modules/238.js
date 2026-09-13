// ─────────────────────────────────────────────────────────────
// APP MODULE 238
// role       : controller PreferencesConnectController
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
  }), t.PreferencesConnectController = void 0;
  var o = n(3),
    r = i(o),
    a = n(1);
  n(4), n(11), n(48), n(46), n(90);
  var l = a.ngMainModule.controller("PreferencesConnectController", ["$scope", "$state", "$stateParams", "$log",
    "$filter", "oscNotificationService", "connectService", "oscService", "errorDialogService", "eventAggregator",
    "shadowPlayService", "broadcastService", "ugcService", "KEYBOARD_EVENTS", "CONNECT_EVENTS", "OSC_KEYBOARD",
    "NOTIFIER_SELECTIONS",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b) {
      function x(e) {
        return u.isBroadcastActive().then(function(t) {
          if (t === !0) {
            var n = f._currentPortalName;
            E.info("serviceName: " + e + " portal: " + n), e === n && (w.disableLogout = !0)
          }
        })
      }

      function v(e) {
        w.updateStatus(e.serviceName)
      }

      function y() {
        if (w.shouldShowLoginButton(w.selected)) {
          if (s.onlineState && s.onlineState.online === !1) return E.info("No Internet connection"), void d.show(
            "l10n.systemRequirement", "l10n.notificationCoplayNetworkUnavailable");
          var e = !1;
          if (S);
          else if (angular.isDefined(w.selected) && null !== w.selected)
            if (w.isLoggedIntoService(w.selected)) E.info("Logout clicked for service ", w.selected), l.logout(w
              .selected).then(function(e) {
              E.info("Logout successful from service ", w.selected)
            }, function(e) {
              E.error("Logout NOT successful from service ", w.selected, " error:", e)
            });
            else if (E.info("Login clicked for service ", w.selected), m.checkJarvisLoginRequirement(w.selected)) a
            .show(b.CONNECT_LOGIN_TO_GFE);
          else if (m.checkIfBrowserLogin(w.selected)) {
            var n = {
              selectedServiceName: w.selected
            };
            m.logInFromBrowser(w.selected, t.current.name, n)
          } else e = !0;
          else E.info("Login button clicked but no service selected.");
          angular.isDefined(e) && (w.oauthDialogueParams = {
            serviceName: w.selected,
            openPopup: e
          }, S = e), w.updateStatus(w.selected)
        }
      }
      var w = this;
      w.title = "l10n.settings", w.icon = "icon-settings", w.status = "", w.actionText = "l10n.connectLogin", w
        .showBackButton = !0, w.services = l.services, w.sortedServices = r.sortBy(w.services, "name"), w
        .oauthDialogueParams = {}, w.disableLogout = !1;
      var S, E = i.getInstance("main.preferences/preferencesconnectcontroller"),
        k = function(e) {
          w.services[e].userLoginTimestamp = o("date")(l.getLoginTimestamp(e), "medium"), l.getUserName(e).then(
            function(t) {
              w.services[e].userName = t
            }).then(function() {
            l.getAvatarUri(e).then(function(t) {
              w.services[e].userAvatarUri = t
            })
          })
        };
      w.updateStatus = function(e) {
        e = e || w.selected;
        var t, n = e === w.selected;
        w.disableLogout = !1, S ? (w.showBackButton = !1, t = "l10n.cancel") : w.isLoggedIntoService(e) ? (w
          .showBackButton = !0, t = "l10n.connectLogout", x(e)) : (w.showBackButton = !0, t =
          "l10n.connectLogin"), n && (w.actionText = t), w.isLoggedIntoService(e) && k(e)
      }, w.select = function(e) {
        w.selected = e, w.updateStatus(e)
      }, w.isActive = function(e) {
        return w.selected === e.name
      }, w.initPreferencesConnect = function() {
        angular.forEach(w.services, function(e) {
          k(e.name), l.doubleCheckConnection(e.name)
        });
        var e = r.findWhere(w.services, {
          name: n.selectedServiceName
        }) || w.sortedServices[0];
        w.selected = e.name, w.updateStatus(w.selected), S = !1
      }, w.action = function(e) {
        return angular.isDefined(e) ? x(e).then(function() {
          w.disableLogout || y()
        }) : void y()
      }, w.back = function() {
        t.go("main.preferences")
      }, w.keyUp = function(e, t) {
        e.keyCode === h.ENTER && w.select(w.sortedServices[t].name)
      }, w.shouldShowLoginButton = function(e) {
        return !(!e && !l.canLoginToService(w.selected)) && !(e && !l.canLoginToService(e))
      }, w.isLoggedIntoService = function(e) {
        return angular.isUndefined(e) && (e = w.selected), void 0 !== e && null !== e && l.isLoggedIntoService(e)
      }, w.onOauthDialogueClosed = function() {
        S = !1, w.updateStatus(w.selected)
      }, w.getBannerText = function(e) {
        return w.shouldShowBrowserIcon(e) ? o("translate")("l10n.loginBrowserRequired", {
          arg1: o("translate")(e.title)
        }) : o("translate")("l10n.connectNotLoggedIn")
      }, w.shouldShowBrowserIcon = function(e) {
        return m.checkIfBrowserLogin(e.name)
      }, w.onLoginBlocked = function() {
        w.oauthDialogueParams.openPopup = !1, w.onOauthDialogueClosed()
      };
      var _ = function() {
        S ? w.oauthDialogueParams.openPopup = !1 : w.back()
      };
      c.on(g.ESCAPE, _), c.on(p.USER_LOGGED_IN, v), c.on(p.USER_LOGGED_OUT, w.updateStatus), c.on(p.LOGIN_BLOCKED, w
        .onLoginBlocked), e.$on("$destroy", function() {
        c.off(g.ESCAPE, _), c.off(p.USER_LOGGED_IN, v), c.off(p.USER_LOGGED_OUT, w.updateStatus), c.off(p
          .LOGIN_BLOCKED, w.onLoginBlocked)
      })
    }
  ]);
  t.PreferencesConnectController = l
}
