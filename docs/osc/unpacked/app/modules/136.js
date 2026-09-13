// ─────────────────────────────────────────────────────────────
// APP MODULE 136
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  n(137), n(87), n(17), n(20), n(26), n(25), n(143), n(49), n(149);
  var i = n(1);
  n(155), n(159), n(160), n(139), n(173), n(175), n(179), n(182), n(185), n(189), n(191), n(211), n(213), n(219), n(
    217), n(237), n(239), n(241), n(245), n(247), n(253), n(257), n(259), n(101), n(262), n(243), n(251), n(249), n(
    235), n(264), n(167), n(157), n(221), n(255), i.ngMainModule.config(["$stateProvider", "$urlRouterProvider",
      function(e, t) {
        t.otherwise("/base"), e.state("base", {
          url: "/base",
          template: "<nv-base></nv-base>",
          resolve: {
            localNodeInfo: ["cefService", "localSdk", "$log", "nvAccountEndpoints", "ugcLib", function(e, t, n, i,
              o) {
              var r = n.getInstance("osc/urlRouter/main/resolve");
              return r.info("Request NodeInfo"), e.localNodeInfo().then(function(e) {
                var n = {};
                n = JSON.parse(e), t.updateNodeInfo(n), i.updateNodeInfo(n), o.updateNodeInfo(n), r.info(
                  "Node info found")
              }, function(e) {
                r.error("failed")
              })
            }],
            localizedConfig: ["localNodeInfo", "$log", "OSC_CONFIG", "piplConfigService", "gfwslEndpoints",
              "telemetryService", "jarvis",
              function(e, t, n, i, o, r, a) {
                var l = t.getInstance("/main/uiRouter/main/resolve/pipl");
                return l.info("attempting to resolve localized config"), i.getPiplConfig().then(function(e) {
                  var t = e.configData;
                  return l.info("pipl config data", t), n = angular.merge(n, t), l.info(
                    "GFE localized config merged"), t
                }).then(function(e) {
                  return r.updateServer(n.jsEvents.server), o.setServer(n.gfwsl.server), a.setServer(n
                    .jarvis.server), !0
                }).catch(function(e) {
                  return l.error("failed to get configuration info", e), !1
                })
              }
            ],
            hardwareInfo: ["localNodeInfo", "hardwareService", "telemetryService", "$q", "$log", "ugcService",
              "settingsService", "oscTargetService", "OSC_CONFIG", "localizedConfig",
              function(e, t, n, i, o, r, a, l, s, d) {
                var c, u, f = o.getInstance("main/resolve/hardwareInfo");
                return t.getSystemInfo().then(function(e) {
                  c = e.TelemetryDeviceId, a.setSystemLocale(e.UserDefaultUILanguage)
                }).catch(function(e) {
                  f.error("Unable to get device id")
                }).finally(function() {
                  r.setupHwConfig(), r.connectService.getJarvisUserId().then(function(e) {
                    if (u = e, s.jarvisEnabled) {
                      var t = r.jarvisService;
                      return t.hasSession() ? t.getLoggedInUser() : t.loginFromDatabase().then(function(
                        e) {
                        return t.hasSession() ? i.when(0) : t.startSession(e).finally(function() {
                          l.init()
                        })
                      }, function(e) {
                        return f.error("Unable to get user session (rejection)"), i.reject(e &&
                          401 === e.status ? "sessionExpired" : null)
                      })
                    }
                  }).catch(function(e) {
                    f.error("Unable to get user id", e)
                  }).finally(function() {
                    n.setEventsCommonData({
                      deviceId: c,
                      userId: u
                    })
                  })
                })
              }
            ]
          },
          onEnter: ["appService", "hardwareInfo", "localizedConfig", function(e, t, n) {
            e.initializeUI()
          }]
        }).state("main", {
          url: "",
          template: "<nv-main></nv-main>",
          abstract: !0,
          parent: "base"
        }).state("main.main-menu", {
          url: "/main-menu",
          template: "<nv-main-menu></nv-main-menu>",
          parent: "main"
        }).state("main.error-dialog", {
          url: "/error-dialog",
          template: "<nv-error-dialog></nv-error-dialog>",
          params: {
            error: "",
            details: "",
            lastState: "",
            lastParams: "",
            arg1: "",
            arg2: ""
          },
          parent: "main"
        }).state("main.guest-controls", {
          url: "/guest-controls",
          template: "<nv-coplay-guest-controls></nv-coplay-guest-controls>",
          parent: "main"
        }).state("main.coplay-invite", {
          url: "/stream-invite",
          template: "<nv-coplay-invite></nv-coplay-invite>",
          parent: "main"
        }).state("main.preferences", {
          url: "/preferences",
          template: "<nv-preferences-menu></nv-preferences-menu>",
          parent: "main",
          params: {
            callback: null
          }
        }).state("main.preferences.connect", {
          url: "/preferences/connect",
          template: "<nv-preferences-connect></nv-preferences-connect>",
          params: {
            oauth: "",
            selectedServiceName: ""
          },
          parent: "main"
        }).state("main.preferences.hangout", {
          url: "/preferences/hangout",
          template: "<nv-preferences-hangout></nv-preferences-hangout>",
          parent: "main"
        }).state("main.preferences.overlays", {
          url: "/preferences/overlays",
          template: "<nv-preferences-overlays></nv-preferences-overlays>",
          parent: "main",
          params: {
            selectedOverlay: void 0,
            lastState: "",
            lastParams: void 0
          }
        }).state("main.preferences.keyboard-shortcuts", {
          url: "/preferences/keyboard-shortcuts",
          template: "<nv-preferences-keyboard-shortcuts></nv-preferences-keyboard-shortcuts>",
          parent: "main",
          params: {
            callback: null
          }
        }).state("main.preferences.recordings", {
          url: "/preferences/recordings",
          template: "<nv-preferences-recordings></nv-preferences-recordings>",
          parent: "main"
        }).state("main.preferences.recordings.folder-browser", {
          url: "/preferences/recordings/folder-browser",
          template: "<nv-preferences-recordings-folder-browser></nv-preferences-recordings-folder-browser>",
          params: {
            currentPath: "",
            pathType: "",
            parentView: "",
            init: "",
            heading: "",
            callback: void 0,
            includeFiles: !1,
            match: "",
            callbackParam: void 0
          },
          parent: "main"
        }).state("main.preferences.stream", {
          url: "/preferences/stream",
          template: "<nv-preferences-stream></nv-preferences-stream>",
          parent: "main"
        }).state("main.preferences.mods", {
          url: "/preferences/mods",
          template: "<nv-preferences-mods></nv-preferences-mods>",
          parent: "main"
        }).state("main.preferences.broadcast", {
          url: "/preferences/broadcast",
          template: "<nv-preferences-broadcast></nv-preferences-broadcast>",
          params: {
            callback: null
          },
          parent: "main"
        }).state("main.preferences.privacy-control", {
          url: "/preferences/privacy-control",
          template: "<nv-preferences-privacy-control></nv-preferences-privacy-control>",
          parent: "main"
        }).state("main.preferences.highlights", {
          url: "/preferences/highlights",
          template: "<nv-preferences-highlights></nv-preferences-highlights>",
          parent: "main",
          params: {
            newPath: void 0,
            oldPath: void 0,
            currentSize: void 0,
            enabled: void 0
          }
        }).state("main.preferences.notifications", {
          url: "/preferences/notifications",
          template: "<nv-preferences-notifications></nv-preferences-notifications>",
          parent: "main"
        }).state("main.preferences.audio", {
          url: "/preferences/audio",
          template: "<nv-preferences-audio></nv-preferences-audio>",
          parent: "main"
        }).state("main.preferences.video", {
          url: "/preferences/video",
          template: "<nv-preferences-video></nv-preferences-video>",
          params: {
            callback: null
          },
          parent: "main"
        }).state("main.broadcast-menu", {
          url: "/broadcast-menu",
          template: "<nv-broadcast-menu></nv-broadcast-menu>",
          parent: "main"
        }).state("main.gallery", {
          url: "/gallery",
          template: "<nv-gallery></nv-gallery>",
          parent: "main"
        }).state("main.gallery.files", {
          url: "/gallery/files-menu",
          template: "<nv-gallery-files-menu></nv-gallery-files-menu>",
          parent: "main"
        }).state("main.gallery.history", {
          url: "/gallery/history-menu",
          template: "<nv-gallery-history-menu></nv-gallery-history-menu>",
          parent: "main"
        }).state("main.gallery.remove", {
          url: "/gallery/remove-menu",
          template: "<nv-gallery-remove-menu></nv-gallery-remove-menu>",
          parent: "main"
        }).state("main.gallery.upload", {
          url: "/gallery/upload-menu",
          template: "<nv-gallery-upload-menu></nv-gallery-upload-menu>",
          parent: "main",
          params: {
            callback: null,
            moments: void 0,
            lastMomentIndex: void 0,
            file: void 0,
            gameName: void 0,
            drsName: void 0,
            drsProfileName: void 0,
            sdkVersion: void 0
          }
        }).state("main.content", {
          url: "/content/content",
          template: "<nv-content-content></nv-content-content>",
          parent: "base",
          params: {
            source: "",
            file: void 0,
            callback: null
          }
        }).state("main.history", {
          url: "/content/history",
          template: "<nv-content-history></nv-content-history>",
          parent: "base"
        }).state("main.myrig", {
          url: "/myrig",
          template: "<nv-myrig-menu></nv-myrig-menu>",
          parent: "base"
        }).state("main.microphone", {
          url: "/microphone",
          template: "<nv-microphone-menu></nv-microphone-menu>",
          parent: "main"
        }).state("main.oauth-menu", {
          url: "/oauth-menu",
          template: "<nv-oauth-menu><nv-oauth-menu>",
          params: {
            service: "",
            oscTileParams: void 0,
            lastState: "",
            lastParams: void 0
          },
          parent: "main"
        }).state("nvcamera", {
          url: "/nvcamera",
          template: "<nv-camera-menu></nv-camera-menu>",
          params: {
            lastState: "",
            lastParams: "",
            isAnselLiteMode: void 0
          },
          parent: "base"
        }).state("mods", {
          url: "/mods",
          template: "<mods-menu></mods-menu>",
          params: {
            lastState: "",
            lastParams: ""
          },
          parent: "base"
        }).state("main.confirmation", {
          url: "/confirmation",
          template: "<nv-confirmation></nv-confirmation>",
          params: {
            title: "",
            icon: "",
            status: "",
            question: "",
            footnote: "",
            topButton: "",
            bottomButton: "",
            topAction: "",
            bottomAction: "",
            closeOSC: "",
            lastState: "",
            topActionArgs: "",
            confirmationType: ""
          },
          parent: "main"
        }).state("main.edge", {
          url: "/edge",
          template: "<edge-dev-kit></edge-dev-kit>",
          parent: "base"
        }).state("octoolmenu", {
          url: "/octoolmenu",
          template: "<octool-menu></octool-menu>",
          params: {
            lastState: "",
            lastParams: ""
          },
          parent: "base"
        }).state("main.preferences.perfsettings", {
          url: "/preferences/perfsettings",
          template: "<nv-preferences-performance></nv-preferences-performance>",
          parent: "main"
        })
      }
    ])
}
