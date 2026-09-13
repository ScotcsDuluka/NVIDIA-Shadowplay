// ─────────────────────────────────────────────────────────────
// APP MODULE 60
// role       : service sdkService
// requires   : (none)
// channels   : /SDK/v.1.0/Notification
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
  }), t.sdkService = void 0;
  var o = n(3),
    r = i(o),
    a = n(2);
  n(20), n(17);
  var l = a.ngMainCommonModule.service("sdkService", ["$log", "$filter", "$q", "eventAggregator", "highlightsEndpoints",
    "shadowPlayEndpoints", "socketService", "oscDisplayService", "oscNotificationService", "telemetryService",
    "oscGalleryService", "NOTIFIER_SELECTIONS", "HIGHLIGHTS_EVENTS", "TELEMETRY_OSC_EVENT_NAMES",
    "CONFIRMATION_TYPE", "HIGHLIGHT_PERMISSIONS", "GALLERY_FILEPROCESS", "TELEMETRY_OSC_AVAILABLE_STATUS",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b, x) {
      function v(e) {
        var n = "l10n.enableHighlights",
          i = t("translate")(n, {
            arg1: e
          }),
          o = {
            title: "l10n.highlights",
            icon: "icon-highlights",
            status: "",
            question: i,
            footnote: "l10n.enableHighlightsFooter",
            topButton: "l10n.yes",
            bottomButton: "l10n.no",
            topAction: E.allowHighlights,
            bottomAction: E.disallowHighlights,
            closeOSC: !0,
            lastState: "",
            confirmationType: p.PERMISSION
          },
          r = "main.confirmation";
        s.openOSC(r, o)
      }

      function y(e) {
        k.info("highlightsNotifier: ", e);
        var t = r.propertyOf(e)("event");
        if (!t) return void k.info("highlightsNotifier: undefined or null data, data.type");
        if ("openSummary" === t) {
          var n = r.propertyOf(e)("gameName");
          if (!n || 0 === n.length) return void k.error("Missing game name");
          var o = {
            moments: e.highlights,
            sdkVersion: e.sdkVersion,
            gameName: n,
            drsName: e.drsName,
            drsProfileName: e.displayName,
            galleryView: !1,
            highlightsView: !0,
            summaryView: !0
          };
          s.openOSC("main.gallery.upload", o)
        } else if ("requestPermissions" === t) E.shortName = e.shortName, E.scopes = e.scopes, v(e.displayName);
        else if ("mediaSaved" === t) {
          if (!r.isString(e.fileType) || !r.isString(e.name)) return void k.info(
            "mediaSaved event: undefined or null fileType, or name");
          if (e.unannounced || d.show(f.HIGHLIGHTS_SAVED, e.name), c.push(g.OSC_HIGHLIGHT_EVENT, {
              type: "screenshot" === e.fileType ? "image" : "video",
              videoLength: e.videoLength || 0,
              gameTitle: e.gameName,
              highlightType: e.highlightDefinitionId,
              highlightAction: "New",
              sdkVersion: e.sdkVersion,
              DRSName: e.drsName,
              DRSProfileName: e.displayName,
              modsActive: T
            }), E.delayedHighlight.push({
              id: e.id,
              groupId: e.groupId,
              filename: e.filename,
              cancel: !1
            }), _ === !0) {
            var a = E.delayedHighlight.pop();
            i.trigger(m.HIGHLIGHT_COMPLETED, a)
          }
        } else if ("HighlightCanceled" === t) {
          if (E.delayedHighlight.push({
              id: e.id,
              groupId: e.groupId,
              cancel: !0
            }), _ === !0) {
            var a = E.delayedHighlight.pop();
            i.trigger(m.HIGHLIGHT_COMPLETED, a)
          }
        } else if ("manualRecordRunning" === t) {
          if (!r.isString(e.manualRecordGameName) || !r.isString(e.gameName)) return void k.info(
            "manualRecordRunning event: undefined or null game name");
          d.show(f.HIGHLIGHT_MANUAL_RECORD_RUNNING, e.manualRecordGameName, e.gameName)
        } else if ("broadcastRunning" === t) {
          if (!r.isString(e.broadcastGameName) || !r.isString(e.gameName)) return void k.info(
            "broadcastRunning event: undefined or null game name");
          d.show(f.HIGHLIGHT_BROADCAST_RUNNING, e.broadcastGameName, e.gameName)
        } else if ("HighlightsMoveStarted" === t) E.totalMoveCount = r.propertyOf(e)("totalHighlightsCount"), i
          .trigger(m.MOVE_STARTED);
        else if ("HighlightsMoveInProgress" === t) {
          var l = r.propertyOf(e)("count"),
            u = 0;
          0 !== E.totalMoveCount && (u = l / E.totalMoveCount * 100), i.trigger(m.MOVE_INPROGRESS, u)
        } else if ("HighlightsMoveDone" === t) {
          var u = 100;
          E.totalMoveCount = 0, i.trigger(m.MOVE_INPROGRESS, u), i.trigger(m.MOVE_DONE)
        } else "actionStatus" === t ? c.push(g.OSC_HIGHLIGHT_ERROR, {
          action: e.action,
          status: e.status,
          internalCode: e.internalCode,
          gameName: e.shortName,
          sdkVersion: e.sdkVersion,
          DRSName: e.drsName,
          DRSProfileName: e.displayName
        }) : "createInstance" === t ? (E.sdkInstance.shortName = e.shortName, E.sdkInstance.drsProfileName = e
          .displayName, E.sdkInstance.drsName = e.drsName) : "destroyInstance" === t && (E.sdkInstance.shortName =
          "", E.sdkInstance.drsProfileName = "", E.sdkInstance.drsName = "")
      }

      function w(e) {
        var t = {};
        return r.forEach(E.scopes, function(n) {
          t[n] = e
        }), t
      }

      function S() {
        var e = "/SDK/v.1.0/Notification",
          t = "highlightsNotifier";
        k.info("Registering SDK notification events"), l.register(e, t), i.on(t, y)
      }
      var E = this,
        k = e.getInstance("osc/sdkService");
      E.totalMoveCount = 0, E.shortName = "", E.scopes = [];
      var _ = !1;
      E.delayedHighlight = [], E.sdkInstance = {}, E.getSDKInstance = function() {
        return k.info("getSDKInstance: ", E.sdkInstance), E.sdkInstance
      }, E.getPendingHighlights = function() {
        return 0 === E.delayedHighlight.length ? void 0 : E.delayedHighlight.pop()
      }, E.allowHighlights = function() {
        var e = w(h.GRANTED);
        return o.setPermissions({}, {
          shortName: E.shortName,
          permissions: e
        }).then(function(e) {
          k.info("Permissions granted for: ", E.shortName), c.push(g.OSC_HIGHLIGHTS_GAME_TOGGLE, {
            gameName: E.sdkInstance.shortName,
            shutoffType: "Initial",
            onOffState: "On",
            DRSName: E.sdkInstance.drsName,
            DRSProfileName: E.sdkInstance.drsProfileName
          })
        }, function(e) {
          k.error("setPermissions failed: ", e)
        })
      }, E.disallowHighlights = function() {
        var e = w(h.DENIED);
        return o.setPermissions({}, {
          shortName: E.shortName,
          permissions: e
        }).then(function(e) {
          k.info("Permissions denied for: ", E.shortName), c.push(g.OSC_HIGHLIGHTS_GAME_TOGGLE, {
            gameName: E.sdkInstance.shortName,
            shutoffType: "Initial",
            onOffState: "Off",
            DRSName: E.sdkInstance.drsName,
            DRSProfileName: E.sdkInstance.drsProfileName
          })
        }, function(e) {
          k.error("setPermissions failed: ", e)
        })
      }, E.isHighlightsActive = function() {
        return o.getActive().then(function(e) {
          return k.info("Highlights Active Status: ", e.data.active), e.data.active
        }, function(e) {
          return k.error("getActive endpoint error: ", e), !1
        })
      }, E.highlightsRecoverSpace = function() {
        return o.recoverSpace().then(function() {
          return k.info("Highlights Recover Folder Space complete"), !0
        }, function(e) {
          return k.error("Highlights Recover Folder Space error: ", e), !1
        })
      }, E.getCustomize = function() {
        return a.getCustomize().then(function(e) {
          return k.info("getCustomize succeeded: ", e.data), e.data
        }, function(e) {
          return k.error("getCustomize endpoint error: ", e), 0
        })
      }, E.setCustomizeSize = function(e) {
        return a.setCustomizeSize({}, {
          sizeMB: e
        }).then(function(e) {
          return k.info("setCustomizeSize succeeded!"), !0
        }, function(e) {
          return k.error("setCustomizeSize endpoint error: ", e), !1
        })
      }, E.setCustomizePath = function(e) {
        return a.setCustomizePath({}, {
          tempSaveFolder: e
        }).then(function(e) {
          return k.info("setCustomizePath succeeded!"), !0
        })
      }, E.importHighlightToGallery = function(e, t, n, i) {
        return a.importHighlightToGallery({}, {
          file: e.fullFilename,
          id: e.moment.id,
          groupId: e.moment.groupId,
          gameName: e.moment.gameName,
          property: t,
          headTrimMs: n,
          lengthMs: i
        }).then(function(e) {
          return k.info("importHighlightToGallery succeeded!"), e.data.destination
        }, function(e) {
          return k.error("importHighlightToGallery endpoint error: ", e), ""
        })
      }, E.isHighlightSummaryOpen = function(e) {
        _ = e
      }, E.getGameHighlightsSettings = function(e) {
        return o.getConfig({}, {
          shortName: e
        }).then(function(e) {
          return k.info("getConfig succeeded"), e.data
        }, function(e) {
          return k.error("getConfig endpoint error: ", e), n.reject(e)
        })
      }, E.setGameHighlights = function(e, t, i) {
        return o.setConfig({}, {
          shortName: e,
          enabled: t,
          highlights: i
        }).then(function(e) {
          return k.info("setConfig succeeded"), e.status
        }, function(e) {
          return k.error("setConfig endpoint error: ", e), n.reject(e)
        })
      }, E.getPermissions = function(e) {
        return o.getPermissions({}, {
          shortName: e
        }).then(function(e) {
          return k.info("getPermissions succeeded"), e.data
        }, function(e) {
          return k.error("getPermissions endpoint error: ", e), n.reject(e)
        })
      }, E.setPermissions = function(e, t) {
        return o.setPermissions({}, {
          shortName: e,
          permissions: t
        }).then(function(e) {
          k.info("setPermissions succeeded")
        }, function(e) {
          return k.error("setPermissions endpoint error: ", e), n.reject(e)
        })
      }, E.getHighlightsEnabled = function() {
        return o.getHighlightsEnabled().then(function(e) {
          return k.info("SP Highlights enabled: ", e.data.enabled), e.data.enabled
        }, function(e) {
          return k.info("getHighlightsEnabled endpoint error: ", e), !1
        })
      }, E.setHighlightsEnabled = function(e) {
        return o.setHighlightsEnabled({}, {
          enabled: e
        }).then(function(t) {
          k.info("SP Highlights enabled: ", e)
        }, function(e) {
          return k.info("setHighlightsEnabled endpoint error: ", e), n.reject(e)
        })
      }, E.getRecentHighlights = function(e, t) {
        return o.getRecent({}, {
          gameName: e.shortName || "all",
          maxItems: t
        }).then(function(e) {
          return k.info("getRecent succeeded", e), e.data
        }, function(e) {
          return k.error("getRecent endpoint error: ", e), n.reject(e)
        })
      }, E.getGamesConfig = function() {
        return o.getGamesConfig().then(function(e) {
          return k.info("Get games config: ", e.data), e.data.games
        }, function(e) {
          return k.info("getGamesConfig endpoint error: ", e), !1
        })
      }, E.getGameHighlights = function(e) {
        return o.getGameHighlights({}, {
          gameName: e
        }).then(function(e) {
          return k.info("getGameHighlights succeeded"), e.data
        }, function(e) {
          return k.error("getGameHighlights endpoint error: ", e), n.reject(e)
        })
      }, E.saveHighlight = function(e) {
        var t = "",
          n = "",
          i = !1;
        if ("video" === e.file.type) {
          var o = u.getVideoParams();
          k.info("Video trim parameters: " + o.trimmed + " " + o.startMs + " " + o.durationMs), o.trimmed && (t =
            o.startMs, n = o.durationMs, i = !0)
        }
        var r = e.moment.savedToGallery ? b.COPY : b.MOVE,
          a = r === b.MOVE ? "Move" : "Copy";
        k.info("Highlight to Gallery: " + a + " Filename: " + e.fullFilename), E.importHighlightToGallery(e, r, t,
          n).then(function(t) {
          "" !== t && (k.info("Import HL to gallery: ", t), i === !1 && r === b.MOVE && (e.fullFilename = t, e
              .file.name = t, e.moment.filename = t, e.moment.savedToGallery = !0, e.moment.filename = t),
            "video" === e.file.type ? d.show(f.RECORD_STOPPED_AND_SAVED_TO_GALLERY) : d.show(f
              .SCREENSHOT_SAVED_TO_GALLERY))
        });
        var l = 0;
        if (!isNaN(parseInt(e.duration))) {
          var s = e.duration.split(":");
          l = 3600 * s[0] + 60 * s[1] + 1 * s[2]
        }
        c.push(g.OSC_HIGHLIGHT_EVENT, {
          type: e.file.type,
          videoLength: l,
          gameTitle: e.moment.gameName,
          highlightType: e.moment.highlightDefinitionId,
          highlightAction: "Saved",
          sdkVersion: 0,
          DRSName: e.moment.drsName,
          DRSProfileName: e.moment.profileName,
          modsActive: T
        })
      };
      var T = x.no;
      E.setModsActiveStatus = function(e) {
        T = e ? x.yes : x.no
      }, E.init = function() {
        k.info("Initialize Sdk Service"), S()
      }
    }
  ]);
  t.sdkService = l
}
