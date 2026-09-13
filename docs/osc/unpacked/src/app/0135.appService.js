// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 135
// service appService
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
  }), exports.appService = void 0;
  var o = require(372),
    r = i(o),
    a = require(1) /* app/1 — main (module) */,
    l = require(484),
    s = i(l);
  require(87), require(48) /* app/48 — oscService (provider) */, require(47) /* app/47 — hotkeyService (service) */, require(12) /* app/12 — oscDisplayService (service) */, require(146) /* app/146 — oscGalleryService (service) */, require(145) /* app/145 — gameProfileService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(60) /* app/60 — sdkService (service) */,
    require(32) /* app/32 — broadcastService (service) */, require(34) /* app/34 — keyboardService (service) */, require(25) /* app/25 — hardwareService (service) */, require(33) /* app/33 — coplayService (service) */, require(61) /* app/61 — settingsService (service) */, require(40) /* app/40 — nvCameraService (service) */, require(20) /* app/20 — socketService (provider) */, require(23) /* app/23 — oscNotificationService (service) */,
    require(35) /* app/35 — osdService (service) */, require(91) /* app/91 — gamepadService (service) */, require(49) /* app/49 — ugcService (service) */, require(147) /* app/147 — oscTargetService (service) */, require(92) /* app/92 — gfwslService (service) */, require(89) /* app/89 — edgeDevKitService (service) */, require(22) /* app/22 — octoolService (service) */;
  var d = require(337),
    c = i(d),
    u = a.ngMainModule.service("appService", ["$log", "$q", "$window", "$translatePartialLoader", "$state",
      "$rootScope", "$interpolate", "$translate", "eventAggregator", "cefService", "oscService",
      "hotkeyService", "oscDisplayService", "shadowPlayService", "sdkService", "broadcastService",
      "keyboardService", "telemetryService", "hardwareService", "coplayService", "settingsService",
      "uploadManagerService", "nvCameraService", "socketService", "oscNotificationService", "osdService",
      "gamepadService", "ugcService", "gameProfileService", "octoolService", "gfwslService",
      "edgeDevKitService", "piplConfigService", "jarvis", "TELEMETRY_OSC_EVENT_NAMES", "OSC_CONFIG",
      "OSC_BUILD_INFO", "COMMON_EVENTS", "quietMode2Service",
      function(e, t, n, i, o, a, l, d, u, f, m, g, p, h, b, x, v, y, w, S, E, k, _, T, C, O, A, I, M, R, P,
        D, N, L, F, U, z, G, V) {
        function H() {
          n.addEventListener("online", function() {
            X.info("Transition to online mode"), m.setOnline(!0), u.trigger(G.ONLINE);
          }), n.addEventListener("offline", function() {
            X.info("Transition to offline mode"), m.setOnline(!1), u.trigger(G.OFFLINE);
          }), u.on(G.LOCALE_CHANGED, Y);
        }

        function B() {
          var e,
            t = (0, r.default)(a);
          e = {
            go: o.go.bind(o)
          }, o.includes && (e.includes = o.includes.bind(o)), t.$state = e;
        }

        function Y(e) {
          W(e);
        }

        function $(e) {
          var t = e;
          return "en-us" !== e.toLowerCase() && (t += ", en-us"), t;
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
            }), n.acceptLanguage = $(e), f.loadStringTable(n);
          }, function(e) {
            X.error("Failed to load CEF string table");
          }) : f.loadStringTable(n);
        }

        function j() {
          var e = ["broadcasttoggle", "broadcastpausetoggle"];
          N.isConnectEnabled().then(function(t) {
            h.dynamicHotkeyToggle(e, t);
          }), u.on(G.PIPL_CONFIG_UPDATED, function(t) {
            U = angular.merge(U, t.configData), y.updateServer(U.jsEvents.server), P.setServer(U.gfwsl
              .server), L.setServer(U.jarvis.server), h.dynamicHotkeyToggle(e, t.isConnectEnabled);
          });
        }
        var K = this,
          q = !1,
          X = e.getInstance("main/appService");
        K.main = function() {
          X.info("Init"), X.info("OSC Build Info:", z), n.name = U.windowName, a.telemetryEventNames = F,
            W(), m.setOnline(n.navigator.onLine), H(), i.addPart("l10n"), B(), y.startCold(F
              .OSC_LAUNCH_TIME_COLD), q = !0;
        }, K.initializeUI = function() {
          X.info("Init UI"), T.connect(), N.initialize(), j(), y.init().then(function() {
            return t.all([v.init(), M.init(), h.init(), w.init(), E.init(), C.init(), b.init(), A
              .init()
            ]);
          }).then(function() {
            return U.nvCamera ? P.init() : t.when(!0);
          }).then(function(e) {
            return t.all([I.init(h), k.init(), S.init(), U.nvCamera ? _.init() : t.when(!0), R.init(),
              U.wm2 ? V.init() : t.when(!0), U.enableEDGEDevKit ? D.init() : t.when(!0)
            ]);
          }).then(function(e) {
            return t.all([O.init()]);
          }).then(function(e) {
            return t.all([x.init()]);
          }).then(function(e) {
            return t.all([p.init(), g.init()]);
          }).then(function(e) {
            return h.setOscReady();
          }).then(function(e) {
            y.endCold(F.OSC_LAUNCH_TIME_COLD);
          }), U.showWatchStats && (0, s.default)();
        };
      }
    ]);
  exports.appService = u;
}
