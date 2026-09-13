// ─────────────────────────────────────────────────────────────
// APP MODULE 135
// role       : service appService
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
  }), t.appService = void 0;
  var o = n(372),
    r = i(o),
    a = n(1),
    l = n(484),
    s = i(l);
  n(87), n(48), n(47), n(12), n(146), n(145), n(4), n(60), n(32), n(34), n(25), n(33), n(61), n(40), n(20), n(23), n(
    35), n(91), n(49), n(147), n(92), n(89), n(22);
  var d = n(337),
    c = i(d),
    u = a.ngMainModule.service("appService", ["$log", "$q", "$window", "$translatePartialLoader", "$state",
      "$rootScope", "$interpolate", "$translate", "eventAggregator", "cefService", "oscService", "hotkeyService",
      "oscDisplayService", "shadowPlayService", "sdkService", "broadcastService", "keyboardService",
      "telemetryService", "hardwareService", "coplayService", "settingsService", "uploadManagerService",
      "nvCameraService", "socketService", "oscNotificationService", "osdService", "gamepadService", "ugcService",
      "gameProfileService", "octoolService", "gfwslService", "edgeDevKitService", "piplConfigService", "jarvis",
      "TELEMETRY_OSC_EVENT_NAMES", "OSC_CONFIG", "OSC_BUILD_INFO", "COMMON_EVENTS", "quietMode2Service",
      function(e, t, n, i, o, a, l, d, u, f, m, g, p, h, b, x, v, y, w, S, E, k, _, T, C, O, A, I, M, R, P, D, N, L,
        F, U, z, G, V) {
        function H() {
          n.addEventListener("online", function() {
            X.info("Transition to online mode"), m.setOnline(!0), u.trigger(G.ONLINE)
          }), n.addEventListener("offline", function() {
            X.info("Transition to offline mode"), m.setOnline(!1), u.trigger(G.OFFLINE)
          }), u.on(G.LOCALE_CHANGED, Y)
        }

        function B() {
          var e, t = (0, r.default)(a);
          e = {
            go: o.go.bind(o)
          }, o.includes && (e.includes = o.includes.bind(o)), t.$state = e
        }

        function Y(e) {
          W(e)
        }

        function $(e) {
          var t = e;
          return "en-us" !== e.toLowerCase() && (t += ", en-us"), t
        }

        function W(e) {
          var t = z.oscPackageVersion;
          t || (t = z.oscClientVersion + "-" + z.gitHash);
          var n = {
            userAgent: U.userAgent + "/" + t
          };
          e ? d(["l10n.pageCouldNotBeLoaded", "l10n.retry"]).then(function(t) {
            n.onLoadError = l(c.default)({
              errorMessage: t["l10n.pageCouldNotBeLoaded"],
              retry: t["l10n.retry"]
            }), n.acceptLanguage = $(e), f.loadStringTable(n)
          }, function(e) {
            X.error("Failed to load CEF string table")
          }) : f.loadStringTable(n)
        }

        function j() {
          var e = ["broadcasttoggle", "broadcastpausetoggle"];
          N.isConnectEnabled().then(function(t) {
            h.dynamicHotkeyToggle(e, t)
          }), u.on(G.PIPL_CONFIG_UPDATED, function(t) {
            U = angular.merge(U, t.configData), y.updateServer(U.jsEvents.server), P.setServer(U.gfwsl.server), L
              .setServer(U.jarvis.server), h.dynamicHotkeyToggle(e, t.isConnectEnabled)
          })
        }
        var K = this,
          q = !1,
          X = e.getInstance("main/appService");
        K.main = function() {
          X.info("Init"), X.info("OSC Build Info:", z), n.name = U.windowName, a.telemetryEventNames = F, W(), m
            .setOnline(n.navigator.onLine), H(), i.addPart("l10n"), B(), y.startCold(F.OSC_LAUNCH_TIME_COLD), q = !0
        }, K.initializeUI = function() {
          X.info("Init UI"), T.connect(), N.initialize(), j(), y.init().then(function() {
            return t.all([v.init(), M.init(), h.init(), w.init(), E.init(), C.init(), b.init(), A.init()])
          }).then(function() {
            return U.nvCamera ? P.init() : t.when(!0)
          }).then(function(e) {
            return t.all([I.init(h), k.init(), S.init(), U.nvCamera ? _.init() : t.when(!0), R.init(), U.wm2 ? V
              .init() : t.when(!0), U.enableEDGEDevKit ? D.init() : t.when(!0)
            ])
          }).then(function(e) {
            return t.all([O.init()])
          }).then(function(e) {
            return t.all([x.init()])
          }).then(function(e) {
            return t.all([p.init(), g.init()])
          }).then(function(e) {
            return h.setOscReady()
          }).then(function(e) {
            y.endCold(F.OSC_LAUNCH_TIME_COLD)
          }), U.showWatchStats && (0, s.default)()
        }
      }
    ]);
  t.appService = u
}
