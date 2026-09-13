// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 136
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  require(137), require(87), require(17) /* app/17 — localSdk (provider) */, require(20) /* app/20 — socketService (provider) */, require(26) /* app/26 — telemetryService (service) */, require(25) /* app/25 — hardwareService (service) */, require(143) /* app/143 — exceptionService (provider) */, require(49) /* app/49 — ugcService (service) */,
    require(149) /* app/149 — piplConfigService (service) */;
  var i = require(1) /* app/1 — main (module) */;
  require(155) /* app/155 — nvBase (directive) */, require(159) /* app/159 — nvErrorDialog (directive) */, require(160) /* app/160 — nvMain (directive) */, require(139) /* app/139 — nvBroadcastMenu (directive) */, require(173) /* app/173 — nvCoplayGuestControls (directive) */, require(175) /* app/175 — nvCoplayInvite (directive) */, require(179) /* app/179 — edgeDevKit (directive) */, require(
      182), require(185), require(189) /* app/189 — nvGalleryRemoveMenu (directive) */, require(191) /* app/191 — nvGalleryUploadMenu (directive) */, require(211) /* app/211 — nvMainMenu (directive) */, require(213) /* app/213 — nvMicrophoneMenu (directive) */, require(219) /* app/219 — nvCameraMenu (directive) */, require(217) /* app/217 — modsMenu (directive) */,
    require(237) /* app/237 — nvPreferencesBroadcast (directive) */, require(239) /* app/239 — nvPreferencesConnect (directive) */, require(241) /* app/241 — nvPreferencesHangout (directive) */, require(245) /* app/245 — nvPreferencesKeyboardShortcuts (directive) */, require(247) /* app/247 — nvPreferencesMenu (directive) */, require(253) /* app/253 — nvPreferencesOverlays (directive) */, require(257) /* app/257 — nvPreferencesPrivacyControl (directive) */, require(
      259), require(101) /* app/101 — nvPreferencesRecordingsFolderBrowser (directive) */, require(262) /* app/262 — nvPreferencesStream (directive) */, require(243) /* app/243 — nvPreferencesHighlights (directive) */, require(251) /* app/251 — nvPreferencesNotifications (directive) */, require(249) /* app/249 — nvPreferencesMods (directive) */, require(235) /* app/235 — nvPreferencesAudio (directive) */, require(264) /* app/264 — nvPreferencesVideo (directive) */,
    require(167) /* app/167 — nvOauthMenu (directive) */, require(157) /* app/157 — nvConfirmation (directive) */, require(221) /* app/221 — octoolMenu (directive) */, require(255) /* app/255 — nvPreferencesPerformance (directive) */, i.ngMainModule.config(["$stateProvider",
      "$urlRouterProvider",
      function(e, t) {
        t.otherwise("/base"), e.state("base", {
          url: "/base",
          template: "<nv-base></nv-base>",
          resolve: {
            localNodeInfo: ["cefService", "localSdk", "$log", "nvAccountEndpoints", "ugcLib", function(
              e, t, n, i, o) {
              var r = n.getInstance("osc/urlRouter/main/resolve");
              return r.info("Request NodeInfo"), e.localNodeInfo().then(function(e) {
                var n = {};
                n = JSON.parse(e), t.updateNodeInfo(n), i.updateNodeInfo(n), o.updateNodeInfo(
                  n), r.info("Node info found");
              }, function(e) {
                r.error("failed");
              });
            }],
            localizedConfig: ["localNodeInfo", "$log", "OSC_CONFIG", "piplConfigService",
              "gfwslEndpoints", "telemetryService", "jarvis",
              function(e, t, n, i, o, r, a) {
                var l = t.getInstance("/main/uiRouter/main/resolve/pipl");
                return l.info("attempting to resolve localized config"), i.getPiplConfig().then(
                  function(e) {
                    var t = e.configData;
                    return l.info("pipl config data", t), n = angular.merge(n, t), l.info(
                      "GFE localized config merged"), t;
                  }).then(function(e) {
                  return r.updateServer(n.jsEvents.server), o.setServer(n.gfwsl.server), a
                    .setServer(n.jarvis.server), !0;
                }).catch(function(e) {
                  return l.error("failed to get configuration info", e), !1;
                });
              }
            ],
            hardwareInfo: ["localNodeInfo", "hardwareService", "telemetryService", "$q", "$log",
              "ugcService", "settingsService", "oscTargetService", "OSC_CONFIG", "localizedConfig",
              function(e, t, n, i, o, r, a, l, s, d) {
                var c,
                  u,
                  f = o.getInstance("main/resolve/hardwareInfo");
                return t.getSystemInfo().then(function(e) {
                  c = e.TelemetryDeviceId, a.setSystemLocale(e.UserDefaultUILanguage);
                }).catch(function(e) {
                  f.error("Unable to get device id");
                }).finally(function() {
                  r.setupHwConfig(), r.connectService.getJarvisUserId().then(function(e) {
                    if (u = e, s.jarvisEnabled) {
                      var t = r.jarvisService;
                      return t.hasSession() ? t.getLoggedInUser() : t.loginFromDatabase()
                        .then(function(e) {
                          return t.hasSession() ? i.when(0) : t.startSession(e).finally(
                            function() {
                              l.init();
                            });
                        }, function(e) {
                          return f.error("Unable to get user session (rejection)"), i
                            .reject(e && 401 === e.status ? "sessionExpired" : null);
                        });
                    }
                  }).catch(function(e) {
                    f.error("Unable to get user id", e);
                  }).finally(function() {
                    n.setEventsCommonData({
                      deviceId: c,
                      userId: u
                    });
                  });
                });
              }
            ]
          },
          onEnter: ["appService", "hardwareInfo", "localizedConfig", function(e, t, n) {
            e.initializeUI();
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
        });
      }
    ]);
}
