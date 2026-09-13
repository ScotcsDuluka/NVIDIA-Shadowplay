// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 125
// service applicationLifetimeService | service cefService | service eventsDetailService | service loggingSanityService | service telemetryService | factory eventAggregator | provider cmsService | provider imageFormatService | provider nesEndpoints | provider loggingService | constant ASSET_TYPES | constant CMS_ERRORS | constant CMS_URLS | constant APPLICATION_LIFETIME_EVENT_TYPE | constant SHUTDOWN_REASON | constant CEF_WINDOW_STYLES | constant EVENTS_RETURN_CODE | constant EVENTS_NOTIFICATIONS | defines angular.module("crimson")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  var i,
    o,
    a = require(77),
    s = r(a),
    c = require(25),
    u = r(c),
    l = require(78),
    d = r(l),
    f = require(26),
    h = r(f);
  ! function(r, a) {
    i = [require(24)], o = function(e) {
      return a(e);
    }.apply(exports, i), !(void 0 !== o && (module.exports = o));
  }(void 0, function(e) {
    e.module("crimson", ["nvJsEvents", "nvAngularHttpEndpoint"]).constant("ASSET_TYPES", {
        BOX_ART: 3,
        GAME_ICON: 4,
        SCREENSHOT: 5,
        KEY_ART: 14,
        HERO_IMAGE: 15,
        FEATURE_IMAGE: 19,
        CAROUSEL_IMAGE: 21,
        KEY_ICON: 22,
        BANNER_IMAGE: 23
      }).constant("CMS_ERRORS", {
        MISSING_GAME_SCHEMA: "No game schema provided.",
        MISSING_COUNTRY_METADATA: "No countryMetaData provided.",
        MISSING_COUNTRY_METADATA_URL: "No countryMetaData metadata url provided.",
        MISSING_LANGUAGE_METADATA: "No languageMetaData provided.",
        MISSING_LANGUAGE_METADATA_URL: "No languageMetaData metadata url provided.",
        MISSING_APP_METADATA_URL: "No app metata url provided.",
        ERROR_FETCHING_NES_RESOURCE: "Could not get resource from NES"
      }).constant("CMS_URLS", {
        BASE_IMAGE_URL: "https://img.nvidiagrid.net/appimg"
      }), e.module("crimson").constant("APPLICATION_LIFETIME_EVENT_TYPE", {
        APPLICATION_EXIT: "APPLICATION_EXIT",
        SYSTEM_LOGOUT: "SYSTEM_LOGOUT",
        SYSTEM_SUSPEND: "SYSTEM_SUSPEND"
      }), e.module("crimson").constant("SHUTDOWN_REASON", {
        USER_CLOSED_WINDOW: "USER_CLOSED_WINDOW",
        USER_QUIT_WITH_SHORTCUT: "USER_USED_QUIT_SHORTCUT",
        USER_LOGGED_OUT: "USER_LOGGED_OUT",
        APPLICATION_REQUESTED_EXIT: "APPLICATION_REQUESTED_EXIT",
        IPC_REQUESTED_EXIT: "IPC_REQUESTED_EXIT",
        SELF_UPDATE: "SELF_UPDATE",
        RELAUNCH: "RELAUNCH"
      }), e.module("crimson").service("applicationLifetimeService", ["$log", "$q", "SHUTDOWN_REASON",
        "APPLICATION_LIFETIME_EVENT_TYPE", "cefService",
        function(e, t, n, r, i) {
          "use strict";

          function o(e) {
            return v.indexOf(e) === -1;
          }

          function a() {
            return 0 === p.length ? void u.info("no pre-sleep work") : (u.info(
              "starting pre-sleep functions"), void p.forEach(function(e) {
              e();
            }));
          }

          function s(e) {
            if (l) return void u.info("performShutdown already called, ignoring");
            if (l = !0, 0 === f.length && 0 === h.length) return u.info(
              "no pre-shutdown work, closing immediately"), void i.windowClose();
            var n = o(e);
            u.info("starting pre-shutdown promises. shutdown ", n ? "is" : "isn't", " interruptable");
            var r;
            r = n && f.length > 0 ? c(f, e) : t.when(!1), r.then(function(t) {
              return t ? void u.info("shutdown was interrupted during onShutdownRequested") : 0 === h
                .length ? (u.info("no OnShutdown work, closing"), void i.windowClose()) : c(h, e)
                .finally(function() {
                  i.windowClose();
                });
            }).finally(function() {
              l = !1;
            });
          }

          function c(e, n) {
            var r = t.defer(),
              i = !1,
              o = e.length,
              a = function() {
                --o, 0 === o && r.resolve(i);
              };
            return e.forEach(function(e) {
              e({
                reason: n
              }).then(function(e) {
                i = e === !0 || i;
              }).catch(function(e) {
                u.info("shutdown promise failed with error", e);
              }).finally(function() {
                a();
              });
            }), r.promise;
          }
          var u = e.getInstance("application-lifetime/applicationLifetimeService");
          u.info("applicationLifetimeService created");
          var l = !1,
            f = [],
            h = [],
            p = [],
            m = [n.USER_CLOSED_WINDOW, n.USER_QUIT_WITH_SHORTCUT, n.USER_LOGGED_OUT, n.RELAUNCH],
            v = [n.USER_LOGGED_OUT, n.IPC_REQUESTED_EXIT, n.SELF_UPDATE, n.RELAUNCH];
          this.handleCefClientEvent = function(e) {
            if ((0, d.default)(r).indexOf(e.type) === -1) return void u.info("Event type", e.type,
              "not found in constants, not handling");
            if (e.type === r.SYSTEM_SUSPEND) a();
            else {
              if ((0, d.default)(n).indexOf(e.reason) === -1) return void u.info("Shutdown reason", e
                .reason, "not found in constants, not handling");
              s(e.reason);
            }
          }, this.shutdownApplicationForReason = function(e) {
            m.indexOf(e) !== -1 ? u.error(
              "Cannot supply a system reason for shutting down from the application layer") : s(e);
          }, this.shutdownApplication = function() {
            this.shutdownApplicationForReason(n.APPLICATION_REQUESTED_EXIT);
          }, this.sleepApplication = function() {
            a();
          }, this.addWorkOnSleep = function(e) {
            var t = p.indexOf(e) !== -1;
            t || p.push(e);
          }, this.removeWorkOnSleep = function(e) {
            p = _.without(p, e);
          }, this.addWorkOnShutdownRequested = function(e) {
            var t = f.indexOf(e) !== -1;
            t || f.push(e);
          }, this.removeWorkOnShutdownRequested = function(e) {
            f = _.without(f, e);
          }, this.addWorkOnShutdown = function(e) {
            var t = h.indexOf(e) !== -1;
            t || h.push(e);
          }, this.removeWorkOnShutdown = function(e) {
            h = _.without(h, e);
          };
        }
      ]), e.module("crimson").constant("CEF_WINDOW_STYLES", {
        WINDOWS: "windows",
        OTHER: "other"
      }), e.module("crimson").service("cefService", ["$q", "$log", "$window", "CEF_WINDOW_STYLES", function(
        e, t, n, r) {
        "use strict";

        function i(t, r) {
          var i,
            o = e.defer(),
            a = r === !0;
          return t = (0, u.default)(t), n.cefQuery ? n.cefQuery({
            request: t,
            persistent: a,
            onSuccess: function(e) {
              "true" === e ? o.resolve(!0) : "false" === e ? o.resolve(!1) : a ? o.notify(e) : o
                .resolve(e);
            },
            onFailure: function(e, n) {
              var r = {
                errorCode: e,
                errorMessage: n
              };
              204 === e && (r.isCancelled = !0), i = "cefQuery returned error: cmd=" + t +
                "errorMessage=" + n, c.error(i), o.reject(r);
            }
          }) : (i = "Cannot find cefQuery. cmd=" + t, c.error(i), o.reject(i)), o.promise;
        }

        function o(e, t, n) {
          return i({
            command: "QUERY_IPC_EXTENSION_MESSAGE",
            module: t,
            request: (0, u.default)(e)
          }, n);
        }
        var a = this,
          s = null,
          c = t.getInstance("crimson.cef/cefService");
        a.readSharedStorage = function(e) {
          return i({
            command: "QUERY_READ_SHARED_STORAGE",
            path: e
          });
        }, a.writeSharedStorage = function(e, t) {
          return i({
            command: "QUERY_WRITE_SHARED_STORAGE",
            path: e,
            data: t
          });
        }, a.localNodeInfo = function() {
          return i({
            command: "QUERY_WIN_NODE_INFO"
          });
        }, a.localDirectoryExplorer = function(e, t, n) {
          return i({
            command: "QUERY_WIN_DIR_INFO",
            includeFiles: e,
            filter: t,
            Win7dlg: n
          });
        }, a.getTimeInfo = function(e) {
          return i({
            command: "QUERY_TIME_INFO",
            type: e
          });
        }, a.enableCloseButton = function(e) {
          c.debug("ENABLE CLOSE BUTTON", e), i({
            command: "QUERY_WIN_ALLOW_CLOSE",
            enable: e
          });
        }, a.isBorderless = function() {
          return i({
            command: "QUERY_WIN_IS_BORDERLESS"
          });
        }, a.isUIRefreshed = function() {
          return i({
            command: "QUERY_IS_UI_REFRESHED"
          });
        }, a.getWindowStyle = function() {
          return s;
        }, a.setWindowStyle = function(e) {
          s = e ? r.WINDOWS : r.OTHER;
        }, a.getMaxWindowSize = function() {
          return c.debug("getMaxWindowSize"), i({
            command: "QUERY_GET_MAX_WINDOW_SIZE"
          });
        }, a.isMaximized = function() {
          return i({
            command: "QUERY_WIN_IS_MAXIMIZED"
          });
        }, a.windowClose = function() {
          i({
            command: "QUERY_WIN_CLOSE"
          });
        }, a.windowMinimize = function() {
          i({
            command: "QUERY_WIN_MINIMIZE"
          });
        }, a.hideApplication = function(e) {
          i({
            command: "QUERY_HIDE_APPLICATION"
          });
        }, a.requestUserAttention = function() {
          return i({
            command: "QUERY_REQUEST_USER_ATTENTION"
          });
        }, a.windowToggleMax = function() {
          this.isMaximized().then(function(e) {
            i(e ? {
              command: "QUERY_WIN_RESTORE"
            } : {
              command: "QUERY_WIN_MAXIMIZE"
            });
          }, function(e) {
            c.error(e);
          });
        }, a.windowFocus = function(e) {
          i({
            command: "QUERY_WIN_FOCUS",
            name: e
          });
        }, a.resizeStart = function(e, t) {
          c.debug("RESIZE START"), i({
            command: "QUERY_WIN_MOUSE_START",
            region: e
          });
        }, a.moveStart = function(e) {
          c.debug("MOVE START"), i({
            command: "QUERY_WIN_MOUSE_START",
            region: "move"
          });
        }, a.openOSC = function(e) {
          return i({
            command: "QUERY_WIN_OPEN_OSC",
            enableInput: e
          });
        }, a.closeOSC = function() {
          i({
            command: "QUERY_WIN_CLOSE_OSC"
          });
        }, a.isInDesktopMode = function() {
          return i({
            command: "QUERY_OSC_DISPLAY_IS_DESKTOP_MODE"
          });
        }, a.allowOSCPainting = function(e, t) {
          var n = t || !1;
          return i({
            command: "QUERY_OSC_SET_PAINTING",
            enablePainting: e,
            startImmediately: n
          });
        }, a.oscCreateDropUrl = function(e, t, n) {
          return i({
            command: "QUERY_OSC_DROP_URL",
            url: e,
            xpos: t,
            ypos: n
          });
        }, a.oscSendWinKBMessage = function(e, t) {
          return i({
            command: "QUERY_WIN_KB_MESSAGE",
            keycode: e,
            keymodifier: t
          });
        }, a.deleteCookies = function(e, t) {
          return c.debug("DELETE COOKIES", e, t), i({
            command: "QUERY_DELETE_COOKIES",
            url: e,
            cookiename: t
          });
        }, a.setClipboardData = function(e) {
          return i({
            command: "QUERY_WIN_COPY_TO_CLIPBOARD",
            clipBoardData: e
          });
        }, a.loadStringTable = function(e) {
          c.debug("LOAD STRING TABLE", e), i({
            command: "QUERY_LOAD_STRING_TABLE",
            stringTable: e
          });
        }, a.allowSetForegroundWindow = function(e) {
          c.debug("ALLOW SET FOREGROUND WINDOW", e), i({
            command: "QUERY_WIN_ALLOW_SET_FOREGROUND",
            pid: e
          });
        }, a.taskbarProgress = function(e, t) {
          c.debug("TASKBAR PROGRESS", e, t), i({
            command: "QUERY_WIN_TASKBAR_PROGRESS",
            state: e,
            percent: t
          });
        }, a.restartNode = function(e) {
          return c.debug("restartNode with reload", e), i({
            command: "QUERY_NODE_RESTART",
            reload: e
          });
        }, a.registerWindowEventsCallback = function() {
          return c.debug("registerCallback with hook"), i({
            command: "QUERY_REGISTER_WINDOW_EVENTS_CALLBACK"
          }, !0);
        }, a.registerApplicationLifetimeEventsCallback = function() {
          return c.debug("registerApplicationLifetimeEventsCallback with hook"), i({
            command: "QUERY_REGISTER_APPLICATION_LIFETIME_EVENTS_CALLBACK"
          }, !0);
        }, a.launchApp = function(e) {
          return c.debug("Launching " + e), i({
            command: "QUERY_LAUNCH_COMPANION_APP",
            appName: e
          });
        }, a.getNotificationData = function() {
          return c.debug("getNotificationData"), i({
            command: "QUERY_NOTIFICATION_DATA"
          });
        }, a.animateAction = function(e) {
          return c.debug("action = " + e), i({
            command: "QUERY_WIN_ANIMATE_ACTIONS",
            action: e
          });
        }, a.getLanguageCode = function(e) {
          c.debug("getLanguageCode appName = " + e);
          var t = void 0 != e ? e : "GeForce Experience";
          return i({
            command: "QUERY_WIN_LANG_CODE",
            app_name: t
          });
        }, a.gfnPrepare = function(e, t, n, r, o, a, s, u) {
          return c.debug("gfnPrepare ", e, t, n, r, o, a, s, u), _.isUndefined(u) ? (c.debug(
            "QUERY_GFN_PREPARE - NO Streaming Profile settings"), i({
            command: "QUERY_GFN_PREPARE",
            address: e,
            serverType: t,
            port: n,
            profile: r,
            deviceId: o,
            advancedLatencyOptimization: a,
            directInput: s
          })) : (c.debug("QUERY_GFN_PREPARE - passed Streaming Profile settings", u), i({
            command: "QUERY_GFN_PREPARE",
            address: e,
            serverType: t,
            port: n,
            profile: r,
            deviceId: o,
            advancedLatencyOptimization: a,
            directInput: s,
            streamingProfileWidth: parseInt(u.width),
            streamingProfileHeight: parseInt(u.height),
            streamingProfileFps: u.fps,
            streamingProfileMaxBitrate: u.maxBitrate,
            streamingProfileDrc: u.drc,
            streamingProfileVSync: u.vSync
          }));
        }, a.gfnSetAuthInfo = function(e, t) {
          return c.debug("gfnSetAuthInfo"), i({
            command: "QUERY_GFN_SET_AUTH_INFO",
            token: e,
            tokenType: t
          });
        }, a.gfnStart = function(e, t, n, r, o, a) {
          return c.debug("gfnStart", e, t, n, r, o, a), i({
            command: "QUERY_GFN_START",
            appId: e,
            frameStatsEnabled: t,
            summaryStatsEnabled: n,
            maxControllersForSingleSession: r,
            advancedLatencyOptimization: o,
            session: a
          });
        }, a.gfnResume = function() {
          return c.debug("gfnResume"), i({
            command: "QUERY_GFN_RESUME"
          });
        }, a.gfnStop = function(e) {
          return c.debug("gfnStop", e), i({
            command: "QUERY_GFN_STOP",
            session: e
          });
        }, a.gfnCancel = function() {
          return c.debug("gfnCancel"), i({
            command: "QUERY_GFN_CANCEL"
          });
        }, a.gfnRegisterCallback = function() {
          return c.debug("gfnRegisterCallback"), i({
            command: "QUERY_GFN_REGISTER_CALLBACK"
          }, !0);
        }, a.gfnControlStats = function(e, t) {
          return c.debug("gfnControlStats"), i({
            command: "QUERY_CONTROL_STATS",
            option: e,
            enable: t
          });
        }, a.gfnGetActiveSessions = function() {
          return c.debug("gfnGetActiveSessions"), i({
            command: "QUERY_GFN_GET_ACTIVE_SESSIONS"
          });
        }, a.gfnSetAuthToken = function(e) {
          return c.debug("gfnSetAuthToken"), i({
            command: "QUERY_GFN_SET_AUTH_TOKEN",
            token: e
          });
        }, a.gfnStreamerClose = function() {
          return c.debug("gfnStreamerClose"), i({
            command: "QUERY_STREAMER_CLOSE"
          });
        }, a.gfnStreamerLaunch = function(e, t, n, r, o) {
          return c.debug("gfnStreamerLaunch"), i({
            command: "QUERY_STREAMER_LAUNCH",
            streamer: e,
            cmsId: "" + t,
            appName: n,
            shortName: r,
            iconUrl: o
          });
        }, a.gfnStreamerShortcutInstalled = function(e) {
          return c.debug("gfnStreamerShortcutInstalled"), i({
            command: "QUERY_STREAMER_IS_INSTALLED",
            cmsId: "" + e
          });
        }, a.gfnStreamerInstallShortcut = function(e, t, n, r, o) {
          return c.debug("gfnStreamerInstallShortcut"), i({
            command: "QUERY_STREAMER_INSTALL",
            streamer: e,
            cmsId: "" + t,
            appName: n,
            shortName: r,
            iconUrl: o
          });
        }, a.gfnReadUpdateTicket = function() {
          return c.debug("gfnReadUpdateTicket"), i({
            command: "QUERY_READ_UPDATE_TICKET"
          });
        }, a.isApplicationInstalled = function() {
          return c.debug("isApplicationInstalled"), i({
            command: "QUERY_IS_APPLICATION_INSTALLED"
          });
        }, a.isApplicationFullscreenExclusive = function() {
          return c.debug("isApplicationFullscreenExclusive"), i({
            command: "QUERY_IS_IN_FULLSCREEN_EXCLUSIVE"
          });
        }, a.windowSetRect = function(e, t, n, r) {
          return c.debug("windowSetRect", e, t, n, r), i({
            command: "QUERY_WIN_RECT",
            x: e,
            y: t,
            w: n,
            h: r
          });
        }, a.osrShowSDLWindow = function() {
          i({
            command: "QUERY_OSR_SHOW_SDL_WINDOW"
          });
        }, a.osrHideSDLWindow = function() {
          i({
            command: "QUERY_OSR_HIDE_SDL_WINDOW"
          });
        }, a.openOSR = function(e) {
          return i({
            command: "QUERY_WIN_OPEN_OSR",
            enableInput: e
          });
        }, a.closeOSR = function() {
          return i({
            command: "QUERY_WIN_CLOSE_OSR"
          });
        }, a.getDeviceId = function() {
          return c.debug("getDeviceId"), i({
            command: "QUERY_DEVICE_ID"
          });
        }, a.browseDirectory = function(e) {
          return c.debug("browseDirectory: ", e), i({
            command: "QUERY_BROWSE_DIRECTORY",
            name: e
          });
        }, a.isApplicationRunning = function(e) {
          return c.debug("isApplicationRunning", e), i({
            command: "QUERY_IS_APPLICATION_RUNNING",
            name: e
          });
        }, a.inputMonitor = function(e) {
          return c.debug("input monitor ", e), i({
            command: "QUERY_OSR_INPUT_MONITOR",
            enable: e
          });
        }, a.inputMonitorRegisterCallback = function(e) {
          return c.debug("input monitor register callback"), i({
            command: "QUERY_OSR_REGISTER_INPUT_MONITOR"
          }, e);
        }, a.getSystemInfo = function() {
          return c.debug("getSystemInfo"), i({
            command: "QUERY_SYSTEM_INFO"
          });
        }, a.dnsLookup = function(e) {
          return c.debug("dnslookup"), i({
            command: "QUERY_DNS",
            name: e
          });
        }, a.osrRegisterKeyPressCallback = function() {
          return c.debug("osrRegisterKeyPressCallback"), i({
            command: "QUERY_OSR_REGISTER_KEYPRESS_CALLBACK"
          }, !0);
        }, a.osrRegisterCustomKeyPress = function(e, t) {
          return c.debug("osrRegisterCustomKeyPress", e, t), i({
            command: "QUERY_OSR_REGISTER_KEYPRESS",
            name: e,
            keyCombination: t
          });
        }, a.restartApp = function(e) {
          return c.debug("restart App "), i({
            command: "QUERY_RESTART_APP",
            launchArguments: e
          });
        }, a.networkTest = function(e) {
          return c.debug("networkTest"), i({
            command: "QUERY_GFN_NETWORK_TEST",
            address: e.address,
            user: e.user,
            deviceId: e.deviceId,
            platformId: e.platformId,
            profiles: e.profiles,
            latencyLimit: e.latencyLimit,
            latencyRecommended: e.latencyRecommended,
            frameLossLimit: e.frameLossLimit,
            frameLossRecommended: e.frameLossRecommended,
            percentile99thFrameJitterLimit: e.percentile99thFrameJitterLimit,
            percentile99thFrameJitterRecommended: e.percentile99thFrameJitterRecommended,
            maxFEC: e.maxFEC,
            displayResolution: e.displayResolution,
            supportedResolutions: e.supportedResolutions
          });
        }, a.findRouteWithLeastLatency = function(e) {
          return c.debug("findRouteWithLeastLatency", e), i({
            command: "QUERY_GFN_LATENCY_BASED_ROUTING",
            user: e.user,
            deviceId: e.deviceId,
            platformId: e.platformId,
            addresses: e.addresses
          });
        }, a.gfnSelfUpdate = function() {
          return c.debug("gfnSelfUpdate"), i({
            command: "QUERY_GFN_UPDATE_APP"
          });
        }, a.ipcPushMessage = function(e) {
          return c.debug("ipcPushMessage", e), i({
            command: "QUERY_IPC_PUSH_MESSAGE",
            message: e
          });
        }, a.ipcPopMessage = function() {
          return c.debug("ipcPopMessage"), i({
            command: "QUERY_IPC_POP_MESSAGE"
          });
        }, a.ipcClearMessages = function() {
          return c.debug("ipcClearMessages"), i({
            command: "QUERY_IPC_CLEAR_MESSAGES"
          });
        }, a.ipcGetNumberOfMessages = function() {
          return c.debug("ipcGetNumberOfMessages"), i({
            command: "QUERY_IPC_GET_NUMBER_OF_MESSAGES"
          });
        }, a.readConfig = function(e) {
          return c.debug("readConfig:", e), i({
            command: "QUERY_READ_CONFIG",
            appname: e
          });
        }, a.writeConfig = function(e, t) {
          return c.debug("writeConfig:", e), i({
            command: "QUERY_WRITE_CONFIG",
            appname: e,
            data: t
          });
        }, a.openCustomLayer = function() {
          return c.debug("open custom layer"), i({
            command: "QUERY_OPEN_CUSTOM_LAYER"
          });
        }, a.closeCustomLayer = function() {
          return c.debug("close custom layer"), i({
            command: "QUERY_CLOSE_CUSTOM_LAYER"
          });
        }, a.nativeQuery = function(e, t, n) {
          return c.debug("Native query: ", e, t, n), i({
            command: "QUERY_IPC_EXTENSION_MESSAGE",
            system: "CrimsonNative",
            module: e,
            method: t,
            payload: n
          });
        }, a.query = function(e, t) {
          return c.debug("generic query"), i(e, t);
        }, a.onJsonMessage = function(e, t) {
          return o(e, t, !1);
        }, a.onNotify = function(e, t) {
          return o(e, t, !0);
        }, a.registerForNotification = function(e, t, n) {
          return c.info("register for notificartion", e, t), i({
            command: "QUERY_IPC_EXTENSION_MESSAGE",
            system: "CrimsonNative",
            module: e,
            method: t,
            payload: n
          }, !0);
        };
      }]), e.module("crimson").provider("cmsService", function() {
        "use strict";
        var t = 0,
          n = 15e3,
          r = "US";
        return {
          setConfig: function(e) {
            t = e.defaultRetries || t, n = e.defaultTimeout || n;
          },
          setCountryCode: function(e) {
            r = e;
          },
          $get: ["NvEndpointFactory", "$log", "$q", "nesEndpoints", "CMS_ERRORS", "CMS_URLS",
            "ASSET_TYPES",
            function(i, o, a, s, c, u, l) {
              var f = new i(),
                h = o.getInstance("crimson/cmsService"),
                p = {};
              _.forEach(l, function(e) {
                p["" + e] = !0;
              }), f.setUrlGenerator(function(e, t) {
                var n = t.jsonUrl;
                return delete t.jsonUrl, n;
              }), f.setDefaultTimeout(n), f.setDefaultRetries(t);
              var m = f.createEndpoint({
                  url: ":jsonUrl",
                  method: "GET",
                  params: {
                    jsonUrl: ""
                  },
                  includeRequestId: !0
                }),
                v = function(e, t) {
                  return s.getResource({
                    cmsId: e,
                    delegateToken: t
                  }).then(function(e) {
                    return e.data;
                  }).catch(function(e) {
                    return e.reason = c.ERROR_FETCHING_NES_RESOURCE, a.reject(e);
                  });
                },
                g = function(e, t) {
                  return v(e, t).then(C).then(w).then(T);
                },
                y = function(t) {
                  return C(e.merge({}, t)).then(w).then(T);
                },
                b = function(e, t, n) {
                  if (t) {
                    var r = _.findWhere(t, {
                      subType: n
                    });
                    if (!_.isUndefined(r)) return r.url;
                    h.error("image type ", n, " undefined for ", e.toString());
                  } else h.error("assets undefined for ", e.toString());
                },
                E = function(e) {
                  var t = {
                    list: e.assets
                  };
                  _.isUndefined(t.list) ? t.list = [] : t.list.forEach(function(e) {
                    t[e.subType] = e;
                  }), e.assets = t;
                },
                $ = function(e) {
                  return a.reject({
                    reason: e
                  });
                },
                w = function(t) {
                  return _.isUndefined(t) ? $(c.MISSING_GAME_SCHEMA) : _.isUndefined(t
                    .countryMetaData) ? $(c.MISSING_COUNTRY_METADATA) : _.isUndefined(t.countryMetaData
                      .url) ? $(c.MISSING_COUNTRY_METADATA_URL) : m({
                      jsonUrl: t.countryMetaData.url
                    }).then(function(n) {
                      return t = e.merge(t, n.data), E(t), t;
                    });
                },
                T = function(t) {
                  return _.isUndefined(t) ? $(c.MISSING_GAME_SCHEMA) : _.isUndefined(t
                    .languageMetaData) ? $(c.MISSING_LANGUAGE_METADATA) : _.isUndefined(t
                      .languageMetaData.url) ? $(c.MISSING_LANGUAGE_METADATA_URL) : m({
                      jsonUrl: t.languageMetaData.url
                    }).then(function(n) {
                      return e.merge(t, n.data);
                    });
                },
                C = function(t) {
                  return _.isUndefined(t) ? $(c.MISSING_GAME_SCHEMA) : _.isUndefined(t.appMetaDataUrl) ?
                    $(c.MISSING_APP_METADATA_URL) : m({
                      jsonUrl: t.appMetaDataUrl
                    }).then(function(n) {
                      return e.merge(t, n.data);
                    });
                },
                x = function(e) {
                  return m({
                    jsonUrl: e
                  });
                },
                S = function(e) {
                  return _.has(p, "" + e);
                },
                A = function(e, t, n, i) {
                  var o;
                  return S(t) && (o = u.BASE_IMAGE_URL + "/" + e + "/" + r + "/" + t + ";w=" + n +
                    ";h=" + i), o;
                },
                M = function(e, t, n) {
                  var i;
                  return S(t) && (i = u.BASE_IMAGE_URL + "/" + e + "/" + r + "/" + t, _.forEach((0, d
                    .default)(n), function(e) {
                    var t = n[e];
                    i += ";" + e + "=" + t;
                  })), i;
                },
                k = function(e, t, n, i) {
                  var o;
                  return S(t) && (o = u.BASE_IMAGE_URL + "/" + e + "/" + r + "/" + t + ";w=" + n, i && (
                    o += ";h=" + i), o += ";bg=000000;f=jpg"), o;
                };
              return {
                getFullUrl: f.generateFullUrl,
                getAvatarMetaData: x,
                getAppMetaData: C,
                getLanguageMetaData: T,
                getCountryMetaData: w,
                getImage: b,
                getAsset: g,
                getAssetWithResource: y,
                getImageUrl: A,
                getImageUrlWithOptions: M,
                getJpegUrl: k
              };
            }
          ]
        };
      }), e.module("events-detail", ["crimson", "underscore", "ngEventAggregator"]), e.module(
        "events-detail").constant("EVENTS_RETURN_CODE", {
        OK: "OK",
        INVALID_INFO_FOR_EVENT_TYPE: "INVALID_INFO_FOR_EVENT_TYPE",
        UNKNOWN_EVENT_TYPE: "UNKNOWN_EVENT_TYPE",
        UNPROCESSED: "UNPROCESSED"
      }).constant("EVENTS_NOTIFICATIONS", {
        SKEWED_POSITIVE_DURATIONS: "nvJsEventsService.skewedPositiveDurations",
        SKEWED_NEGATIVE_DURATIONS: "nvJsEventsService.skewedNegativeDurations"
      }), e.module("events-detail").service("eventsDetailService", ["$log", "_", "EVENTS_RETURN_CODE",
        "eventAggregator", "EVENTS_NOTIFICATIONS",
        function(t, n, r, i, o) {
          "use strict";

          function a(e, t) {
            var r = n.omit(t, "id"),
              i = (0, u.default)(e);
            e = JSON.parse(i);
            var o = e || {},
              c = (0, s.default)(r),
              l = (0, s.default)(o);
            return c.length === l.length && n.every(r, function(e, t) {
              return "object" === ("undefined" == typeof e ? "undefined" : (0, h.default)(e)) ?
                "object" === (0, h.default)(o[t]) && a(o[t], e) : n.has(o, t);
            });
          }

          function c(t, i, o) {
            var s = {
              status: r.UNKNOWN_EVENT_TYPE,
              eventDataFormat: null
            };
            return t.type && n.find(o, function(n) {
              n.name === t.type && (s.eventDataFormat = e.merge({}, n), s.status = r.OK);
            }), s.status === r.OK && a(i, s.eventDataFormat.parameters) === !1 && (s.status = r
              .INVALID_INFO_FOR_EVENT_TYPE), s;
          }

          function l(t, r, i) {
            var o = e.merge({}, i);
            return e.merge(o.parameters, r), o.ts = new Date().toISOString(), n.has(o.parameters, "id") &&
              (o.parameters.id = t.enumId), f.event("eventDetail", o, " set for eventName", t), o;
          }

          function d(e, t, a, s, u) {
            if (a && u) {
              var d = Date.now(),
                h = d - a;
              f.info("started ", e.name, "at ", a, "and ended at ", d, "Time elapsed(ms)", h), t = n
                .extend({}, t, {
                  totalMs: h
                });
              var p = c(e, t, u);
              return p.status === r.UNKNOWN_EVENT_TYPE ? (f.error("Unknown event type", e.type), r
                .UNKNOWN_EVENT_TYPE) : p.status === r.INVALID_INFO_FOR_EVENT_TYPE ? (f.error(
                "invalid input", t, "for event", e), r.INVALID_INFO_FOR_EVENT_TYPE) : (s && s
                .positiveDurationLimit && h > s.positiveDurationLimit && (f.info("positive duration", e
                  .name, h), i.trigger(o.SKEWED_POSITIVE_DURATIONS, {
                  eventInfo: e,
                  startTime: a,
                  endTime: d,
                  durationMs: t.totalMs
                })), s && s.negativeDurationLimit && h < s.negativeDurationLimit && (f.info(
                  "negative duration", e.name, h), i.trigger(o.SKEWED_NEGATIVE_DURATIONS, {
                  eventInfo: e,
                  startTime: a,
                  endTime: d,
                  durationMs: t.totalMs
                })), l(e, t, p.eventDataFormat));
            }
            return f.error("No start record found for ", e), r.UNPROCESSED;
          }
          var f = t.getInstance("events-detail/eventsDetailService");
          f.info("eventsDetailService created"), this.getFormattedEvent = function(e, t, i) {
            if (e && i) {
              if (e.name && e.enumId) {
                n.isUndefined(t) && (t = "");
                var o = c(e, t, i);
                return o.status === r.UNKNOWN_EVENT_TYPE ? (f.event("Unknown event type", e.type), r
                  .UNKNOWN_EVENT_TYPE) : o.status === r.INVALID_INFO_FOR_EVENT_TYPE ? (f.error(
                  "invalid input", t, "for event", e), r.INVALID_INFO_FOR_EVENT_TYPE) : l(e, t, o
                  .eventDataFormat);
              }
              f.event("event not tracked for", e);
            } else f.error("eventName undefined ");
            return r.UNPROCESSED;
          }, this.getFormattedDurationEvent = function(e, t, n, i, o) {
            if (e) {
              if (e.name && e.enumId) return d(e, t, n, i, o);
              f.event("event not tracked for", e);
            } else f.error("eventName undefined ");
            return r.UNPROCESSED;
          };
        }
      ]), e.module("crimson").provider("imageFormatService", function() {
        "use strict";
        var e;
        return {
          setConfig: function(t) {
            e = t.server;
          },
          $get: ["$log", function(t) {
            var n = t.getInstance("crimson/imageFormatServices");
            return {
              formatImage: function(t, r) {
                var i = t;
                if (t) {
                  var o = "https?://.*(/apps/[0-9]*/.*)",
                    a = t.match(o);
                  a && a.length > 1 && a[1] ? i = e + a[1] + ";w=" + r + ";bg=0x00000000" : n.debug(
                    "Following image could not beprocessed by imageFormat Service ", t);
                }
                return i;
              },
              formatCmsImage: function(t, n, r, i) {
                var o = e + "/apps/" + t + "/" + r + "/" + n;
                for (var a in i) {
                  var s = i[a] ? "" + i[a] : void 0;
                  s && s.length > 0 && (o += ";" + a + "=" + i[a]);
                }
                return o;
              }
            };
          }]
        };
      }), e.module("crimson").provider("nesEndpoints", [function() {
        "use strict";
        var t,
          n,
          r,
          i,
          o,
          a,
          s,
          c,
          u = 0,
          l = "v2",
          d = "v1",
          f = 15e3;
        return {
          setCountryCode: function(e) {
            o = e;
          },
          setLanguageCode: function(e) {
            a = e;
          },
          setConfig: function(e) {
            t = e.server, n = e.optimizedServer, d = d || e.optimizedServerVersion, r = e.cacheServer,
              l = e.version || l, u = e.defaultRetries || u, f = e.defaultTimeout || f, i = e
              .deviceId, s = e.serviceName, c = e.layoutClientName;
          },
          $get: ["$log", "NvEndpointFactory", "eventAggregator", function(h, p, m) {
            var v = new p();
            h.getInstance("main.nesSdk/nesEndpoints");
            v.setDefaultTimeout(f), v.setDefaultRetries(u), v.setUrlGenerator(function(e, i) {
              var o;
              return i && i.useCacheServer ? (o = r + e.url, delete i.useCacheServer) : i && i
                .useOptimizedServer ? (o = n + e.url, delete i.useOptimizedServer) : o = t + e
                .url, o;
            }), v.setHeaderGenerator(function(t, n) {
              var r = e.merge({}, t.headers);
              if (!n.useCacheServer) {
                var i = n.delegateToken;
                i && (r.Authorization = 'JarvisAuth auth={"access_token":"' + i + '"}');
              }
              return delete n.delegateToken, r;
            });
            var g = v.createEndpoint({
                url: "/:version/subscriptions",
                method: "GET",
                params: {
                  version: l,
                  countryCode: o,
                  languageCode: a,
                  serviceName: ""
                },
                includeRequestId: !0
              }),
              y = v.createEndpoint({
                url: "/:version/resources/:cmsId",
                method: "GET",
                params: {
                  version: l,
                  locale: a
                },
                includeRequestId: !0
              }),
              b = v.createEndpoint({
                url: "/:version/resources",
                method: "GET",
                params: {
                  version: l,
                  serviceName: s,
                  languageCode: a,
                  countryCode: o
                },
                includeRequestId: !0
              }),
              E = v.createEndpoint({
                url: "/:version/cloudmatch/apps/:appId",
                method: "DELETE",
                params: {
                  version: l
                },
                includeRequestId: !0
              }),
              _ = v.createEndpoint({
                url: "/:optimizedServerVersion/public/apps",
                method: "GET",
                params: {
                  optimizedServerVersion: d,
                  "country-code": o,
                  "language-code": a
                },
                includeRequestId: !0
              }),
              $ = v.createEndpoint({
                url: "/:optimizedServerVersion/public/layouts",
                method: "GET",
                params: {
                  optimizedServerVersion: d,
                  "country-code": o,
                  "language-code": a
                },
                includeRequestId: !0
              }),
              w = v.createEndpoint({
                url: "/:version/layouts",
                method: "GET",
                params: {
                  version: l,
                  countryCode: o,
                  languageCode: a,
                  deviceId: i,
                  clientName: c,
                  useCacheServer: !1
                },
                includeRequestId: !0
              }),
              T = v.createEndpoint({
                url: "/:version/apps",
                method: "GET",
                params: {
                  version: l,
                  serviceName: s,
                  languageCode: a,
                  countryCode: o,
                  vpcId: "DC1-GNC1",
                  clientIp: "10.0.0.1",
                  useCacheServer: !1
                },
                includeRequestId: !0
              });
            return {
              getFullUrl: v.generateFullUrl,
              getSubscriptions: g,
              getResource: y,
              getResources: b,
              uninstallApp: E,
              getAppsList: _,
              getLayout: $,
              getLayouts: w,
              getApps: T
            };
          }]
        };
      }]),
      function(e, t, n) {
        "use strict";
        t.module("ngEventAggregator", []).factory("eventAggregator", [function() {
          function e(e, t) {
            var n = i[e] || (i[e] = []);
            n.push(t);
          }

          function n(e, n) {
            var r = i[e];
            if (!t.isUndefined(r)) {
              for (var o = r.length - 1; o >= 0; --o) r[o] === n && r.splice(o, 1);
              0 === r.length && delete i[e];
            }
          }

          function r(e, n) {
            var r = i[e];
            if (t.isDefined(r))
              for (var o = r.slice(), a = 0; a < o.length; a++) o[a](n);
          }
          var i = {};
          return {
            on: e,
            off: n,
            trigger: r
          };
        }]);
      }(window, e), e.module("crimson").config(["$provide", function(e) {
        "use strict";
        e.decorator("$log", ["$delegate", "loggingService", function(e, t) {
          return t.enhancedAngularLog(e), e;
        }]);
      }]), e.module("crimson").service("loggingSanityService", [function() {
        "use strict";

        function e(e) {
          var t = null;
          try {
            t = new RegExp("\\b" + e + "\\b", "gi");
          } catch (o) {
            for (var n = ["[", "\\", "^", "$", ".", "|", "?", "*", "+", "(", ")", "{", "}", "]"], r = [
                "\\[", "\\\\", "\\^", "\\$", "\\.", "\\|", "\\?", "\\*", "\\+", "\\(", "\\)", "\\{",
                "\\}", "]"
              ], i = 0; i < n.length; i++) e = e.replace(n[i], r[i]);
            try {
              t = new RegExp(e, "gi");
            } catch (e) {
              t = new RegExp("a^");
            }
          }
          return t;
        }

        function t(e) {
          return e && "string" == typeof e;
        }

        function n(e) {
          return o.indexOf(e) === -1;
        }
        var r = this,
          i = [],
          o = [];
        r.getPiiSensitiveValues = function() {
          return o;
        }, r.setPiiSensitiveValues = function(r) {
          Array.isArray(r) && (r = r.filter(function(e) {
            return t(e) && n(e);
          }), r.forEach(function(t) {
            o.push(t), i.push(e(t));
          }));
        }, r.addPiiSensitiveValue = function(r) {
          t(r) && n(r) && (o.push(r), i.push(e(r)));
        }, r.clearPiiSensitiveValues = function() {
          o = [], i = [];
        }, r.sanitizeForPii = function(e) {
          if (e) {
            var t = (0, u.default)(e),
              n = "xx_pii_sanitized_xx";
            i.forEach(function(e) {
              t = t.replace(e, n);
            }), e = JSON.parse(t);
          }
          return e;
        };
      }]), e.module("crimson").provider("loggingService", function() {
        "use strict";
        var t = !1,
          n = !1,
          r = !1;
        return {
          setVerboseLoggingEnabled: function(e) {
            t = e;
          },
          setEventLoggingEnabled: function(e) {
            n = e;
          },
          setPerformanceLoggingEnabled: function(e) {
            r = e;
          },
          $get: ["$filter", "loggingSanityService", function(i, o) {
            function a(a, s, c) {
              return function() {
                if ((t || "DEBUG" !== s) && (n || "EVENT" !== s) && (r || "PERF" !== s)) {
                  var u = i("date")(new Date(), "yyyy-MM-dd HH:mm:ss.sss"),
                    l = [].slice.call(arguments),
                    d = "",
                    f = "";
                  if (d = u + "  " + s + "  " + c + "-", "PERF" === s) {
                    if (l.length < 3 || l.length > 4 || !isNaN(l[0]) || isNaN(l[1]) || isNaN(l[2]))
                      d = u + "  ERROR  " + c +
                      "- Performance log called with incorrect arguments: " + e.toJson(l) +
                      ". Expected [identifier, startTime, endTime<, extraData>].";
                    else {
                      var h = l[0],
                        p = l[1],
                        m = l[2],
                        v = l[3],
                        g = m - p;
                      h = e.toJson(h, !0), v = e.toJson(v || "", !0), d = d + ' "started " ' + h +
                        ' "at " ' + p + ' "and ended at " ' + m + ' "Time elapsed(ms)" ' + g + " " +
                        v;
                    }
                  } else
                    for (var y = 0; y < l.length; y++) {
                      try {
                        f = e.toJson(l[y], !0);
                      } catch (e) {}
                      d = d + " " + f;
                    }
                  d = o.sanitizeForPii(d), a.call(null, d);
                }
              };
            }

            function s(e) {
              e.getInstance = function(t) {
                return {
                  info: a(e.info, "INFO", t),
                  debug: a(e.debug, "DEBUG", t),
                  error: a(e.error, "ERROR", t),
                  event: a(e.info, "EVENT", t),
                  perf: a(e.info, "PERF", t)
                };
              };
            }
            return {
              enhancedAngularLog: s
            };
          }]
        };
      });
    e.module("crimson").service("telemetryService", ["$log", "cefService", "jsEventsService",
      "eventsDetailService", "EVENTS_RETURN_CODE",
      function(e, t, n, r, i) {
        "use strict";

        function o() {
          var e = Date.now(),
            t = "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(t) {
              var n = (e + 16 * Math.random()) % 16 | 0;
              return e = Math.floor(e / 16), ("x" === t ? n : 3 & n | 8).toString(16);
            });
          return a.info("telemetry sessionId ", t), t;
        }
        var a = e.getInstance("crimson/telemetryService");
        a.info("telemetryService created");
        var s = this;
        s.sessionId = "undefined", s.deviceId = "undefined", s.userId = "undefined", s.isUIRefreshed = !
          1;
        var c = "",
          u = function() {
            return !0;
          };
        s.clientVersion = "undefined", s.launchUiEventSent = !1, s.clientConfig = {}, s
          .eventsDetailData = {}, s.clientType = "UNKNOWN", s.setLaunchUiEventSent = function(e) {
            s.launchUiEventSent = e, a.info("Launch UI event sent: ", s.launchUiEventSent);
          }, s.getLaunchUiEventSent = function() {
            return s.launchUiEventSent;
          }, s.getUIRefreshState = function() {
            a.info("Getting  UI Refreshed state"), t.isUIRefreshed().then(function(e) {
              s.isUIRefreshed = e, a.debug("Refreshed state", e.toString());
            }, function() {
              a.error("Failed to get UI Refreshed state");
            });
          }, s.setLoggingEvaluator = function(e) {
            u = e;
          }, s.push = function(e, t, o) {
            if (s.isUIRefreshed) return void a.debug("Disabled telemetry", e.name);
            o = o || {}, o = _.extend(o, {
              appExit: s.sync
            });
            var c;
            c = r.getFormattedEvent(e, t, s.eventsDetailData), c && c !== i.UNPROCESSED ? (u(c) && a
              .info("eventDetail", c), n.sendEventDetail(e, c, o)) : a.info(
              "no event detail formatted");
          }, s.startLoad = function(e, t) {
            if (s.isUIRefreshed) return void a.debug("Disabled telemetry", e.name);
            if (e) {
              if (e.name && e.enumId) return _.isUndefined(t) ? Date.now() : t;
              a.event("event not tracked for", e);
            } else a.error("eventName undefined ");
          }, s.endLoad = function(e, t, o, c) {
            if (s.isUIRefreshed) return void a.debug("Disabled telemetry", e.name);
            c = c || {}, c = _.extend(c, {
              appExit: s.sync,
              negativeDurationLimit: -1e8
            });
            var u = r.getFormattedDurationEvent(e, o, t, c, s.eventsDetailData);
            u && u !== i.UNPROCESSED ? (a.info("durationEventDetail", u), n.sendEventDetail(e, u, c)) :
              a.info("no event detail formatted");
          }, s.setEventsCommonData = function(e, t) {
            var r = !1;
            !s.deviceId && e && e.deviceId && a.info("telemetry deviceId", e.deviceId), (!s.userId ||
              t) && e && e.userId && a.info("telemetry userId ", e.userId), s.sessionId =
              "undefined" !== s.sessionId ? s.sessionId : o(), s.deviceId = "undefined" !== s.deviceId ?
              s.deviceId : e && e.deviceId, s.clientVersion = e && e.clientVersion || s.clientVersion,
              t === !0 ? e && (e.userId && e.userId !== s.userId || "" === e.userId) && (s.userId = e
                .userId, r = !0) : s.userId = "undefined" !== s.userId ? s.userId : e && e.userId, (r ||
                _.isUndefined(t)) && (a.info("Set common events data: ", s.clientConfig.gxTelemetry
                .clientId, s.clientVersion, s.sessionId, s.deviceId, s.userId, s.clientConfig.jsEvents
                .schemaVersion), n.setEventsCommonData({
                clientProductId: s.clientConfig.gxTelemetry.clientId,
                clientProductVer: s.clientVersion,
                clientSessionId: s.sessionId,
                clientDeviceId: s.deviceId,
                clientUserId: s.userId,
                eventSchemaVer: s.clientConfig.jsEvents.schemaVersion
              }));
          }, s.setScreen = function(e) {
            c = e;
          }, s.getScreen = function() {
            return c;
          }, s.changeSync = function(e) {
            s.sync = e;
          }, s.getClientType = function() {
            return s.clientType;
          }, s.initialize = function(e, t, r, i, o) {
            s.clientConfig = t, s.eventsDetailData = i, s.clientType = o || s.clientType;
            var a = {
              msInterval: s.clientConfig.jsEvents.msBetweenSendRequest,
              maxEvents: s.clientConfig.jsEvents.maxEventsPerRequest
            };
            n.setBatchModeSettings(a), s.getUIRefreshState(), s.setEventsCommonData({
              clientVersion: r,
              deviceId: e
            });
          }, s.setDefaultClientConsent = function(e) {
            a.info("Set client consent: ", e), n.setDefaultConsent(e);
          }, s.setUserConsent = function(e) {
            a.info("Set user consent: ", e), n.syncUserConsentInfo(e);
          }, s.setExperienceControlInfo = n.setExperienceControlInfo, s.resetExperienceControlInfo = n
          .resetExperienceControlInfo;
      }
    ]);
  });
}
