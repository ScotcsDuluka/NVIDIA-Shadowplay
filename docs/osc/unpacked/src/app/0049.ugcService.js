// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 49
// service ugcService
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
  }), exports.ugcService = void 0;
  var o = require(369),
    r = i(o),
    a = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(11);
  var l = a.ngMainCommonModule.service("ugcService", ["$q", "$log", "$window", "$timeout", "hardwareService",
    "settingsService", "oscDisplayService", "oscNotificationService", "connectService", "jarvisService",
    "oscGalleryService", "socketService", "eventAggregator", "runTimeConfigService",
    "hardwareConfigWrapper", "recordingPathConfigWrapper", "twitchIngestServerConfigWrapper",
    "shotWithGeForceService", "oscTargetService", "cefService", "OSC_CONFIG", "COMMON_EVENTS",
    "ACCOUNT_SOCKET_EVENTS", "ACCOUNT_EVENTS", "CONNECT_EVENTS", "OSC_EVENTS", "NOTIFIER_SELECTIONS",
    "piplConfigService",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S, E, k, _, T, C, O) {
      function A() {
        return D.info("Initializing connect"), a.getLanguage().then(function(e) {
          return d.initialize(e, S.LOCALE_CHANGED);
        }).then(function() {
          return P = !0;
        });
      }

      function I(e) {
        D.info("Connect status:", e.isConnectEnabled), e.isConnectEnabled && !P && A();
      }

      function M(e) {
        "open" === e && (D.info("OSC Opening, resetting gallery service state"), R.resetGalleryState());
      }
      var R = this;
      R.connectService = d, R.jarvisService = c;
      var P = !1,
        D = t.getInstance("main.common/ugcService");
      D.info("ugcService created"), R.setupHwConfig = function() {
        p.init(o.getSystemInfo, o.getSystemDescription);
      }, R.init = function(t) {
        return f.register(E.USER_TOKEN_CHANGED, k.USER_TOKEN_CHANGED), m.on(k.USER_TOKEN_CHANGED, R
            .onUserTokenChanged), m.on(T.OSC_STATE, M), m.on(S.PIPL_CONFIG_UPDATED, I), g.init(w), h
          .init(t.getRecordingPaths), b.init(t.getPreferredTwitchIngestServer), O.isConnectEnabled()
          .then(function(t) {
            return t && !P ? A() : e.when(null);
          });
      }, R.resetGalleryState = function() {
        u.resetGalleryState();
      }, R.onUserTokenChanged = function(e) {
        if (D.info("Received User Token changed notification."), e && e.userInfo) {
          var t = x.serviceNames.SHOT_WITH_GEFORCE;
          e.userInfo.userId ? (c.hasSession() || c.loginFromDatabase().then(function(e) {
            c.startSession(e).then(function() {
              v.updateLoginStatus();
            });
          }), m.trigger(_.USER_LOGGED_IN, {
            serviceName: t,
            providerName: x.name,
            reason: k.USER_TOKEN_CHANGED
          })) : (x.storedToken = void 0, d.logout(t), m.trigger(_.USER_LOGGED_OUT, t));
        }
      }, R.checkJarvisLoginRequirement = function(e) {
        var t = d.providers[d.services[e].providerName].useOauthProxy,
          n = c.hasSession();
        return t && !n;
      }, R.checkIfBrowserLogin = function(e) {
        return d.providers[d.services[e].providerName].embeddedRestricted;
      }, R.extractURLParams = function(e) {
        var t = e.indexOf("?") > -1 ? e.split("?") : e.split("#");
        if (t.length < 1) return null;
        var n = {},
          i = new URLSearchParams(t[1]),
          o = !0,
          a = !1,
          l = void 0;
        try {
          for (var s, d = (0, r.default)(i.entries()); !(o = (s = d.next()).done); o = !0) {
            var c = s.value;
            n[c[0]] = c[1].split("#")[0];
          }
        } catch (e) {
          a = !0, l = e;
        } finally {
          try {
            !o && d.return && d.return();
          } finally {
            if (a) throw l;
          }
        }
        return n;
      }, R.logInFromBrowser = function(e, t, o, r) {
        D.info("logInFromBrowser, service:", e, t, r), y.isInDesktopMode().then(function(a) {
          if (a) {
            D.info("Logging in from browser, service:", e);
            var c,
              u = [2259, 6460, 7119, 8870, 9096],
              f = "https://google.com",
              g = ["error"],
              p = {
                command: "QUERY_HTTPSERVER_START",
                ports: u,
                redirectUrl: f,
                redirectParams: g
              };
            y.query(p, !0).then(angular.noop, angular.noop, function(a) {
              a = JSON.parse(a);
              var u;
              if ("serverCreated" === a.callbackReason) {
                l.closeOSC();
                var f = d.getOAuthUrl(e);
                c = a.portNumber, f = f.replace("{{portNumber}}", c.toString()), u = n.open(f,
                  "_blank");
              } else if ("httpRequest" === a.callbackReason) {
                var m = R.extractURLParams(a.url),
                  g = d.providers[d.services[e].providerName],
                  p = g.redirectUriOverride;
                p = p.replace("{{portNumber}}", c.toString()), d.setAuthToken(e, m, p), u && (u
                  .close(), u = null), i(function() {
                  r ? s.show(C.LOGIN_COMPLETED_WINDOWED_GAME_BROADCAST, e) : l.openOSC(t,
                  o);
                }, 0);
              }
            });
          } else D.info("FS mode, avoid in-browser login", e), m.trigger(_.LOGIN_BLOCKED), s.show(C
            .CONNECT_EXIT_FULL_SCREEN, e);
        });
      };
    }
  ]);
  exports.ugcService = l;
}
