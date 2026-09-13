// ─────────────────────────────────────────────────────────────
// APP MODULE 236
// role       : controller PreferencesBroadcastController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.PreferencesBroadcastController = void 0;
  var i = n(1);
  n(4), n(32), n(11), n(25);
  var o = i.ngMainModule.controller("PreferencesBroadcastController", ["$scope", "$log", "$state", "$stateParams",
    "$filter", "$q", "broadcastService", "shadowPlayService", "twitchService", "eventAggregator", "hardwareService",
    "galleryService", "KEYBOARD_EVENTS", "VIDEO_STATE", "OSC_KEYBOARD", "OSC_MODE", "BROADCAST_STATES",
    function(e, t, n, i, o, r, a, l, s, d, c, u, f, m, g, p, h) {
      var b = this,
        x = t.getInstance("main.preferences.broadcast");
      b.title = "l10n.settings", b.icon = "icon-settings", b.status = "", b.settingsDisabled = !1, b
      .settingsData = {}, b.blDisabled = "", b.maxCustomOverlays = [1, 2, 3], b.multipleCustomOverlayFileSize = [
        "", "", ""
      ], b.multipleCustomOverlayFilePath = [o("translate")("l10n.slotEmpty", {
        slotNumber: 1
      }), o("translate")("l10n.slotEmpty", {
        slotNumber: 2
      }), o("translate")("l10n.slotEmpty", {
        slotNumber: 3
      })], b.isMultipleCustomOverlayEnabled = [void 0, void 0, void 0], b.slot = [o("translate")(
      "l10n.slotEmpty", {
        slotNumber: 1
      }), o("translate")("l10n.slotEmpty", {
        slotNumber: 2
      }), o("translate")("l10n.slotEmpty", {
        slotNumber: 3
      })], b.lastSelectedFilePath = "", b.broadcastControlStatus = !0, b.setBroadcastControlStatus = function() {
        a.setBroadcastPreference(b.broadcastControlStatus ? "AlwaysAsk" : "DoNotBroadcast").then(function(e) {
          l.setBroadcastPreference(e)
        })
      };
      var v = function(e) {
        return r.all([a.getBroadcastState()])
      };
      b.initPreferencesBroadcast = function() {
        return a.getBroadcastPreference().then(function(e) {
          b.selectedBroadcastTargetName = e, x.info("Broadcast Preference: " + b.selectedBroadcastTargetName),
            b.broadcastControlStatus = "DoNotBroadcast" !== b.selectedBroadcastTargetName
        }), s.getTwitchIngestServerList().then(function(e) {
          b.ingestServers = e, l.getPreferredTwitchIngestServer().then(function(e) {
            var t = b.ingestServers.map(function(e) {
                return e.name
              }),
              n = t.indexOf(e);
            (n < 0 || !b.ingestServers[n].availability) && (n = s.getClosestIngestServer(b
            .ingestServers)), (n < 0 || !b.ingestServers[n].availability) && (n = s.getDefaultServer(b
              .ingestServers)), b.selectedIngestServerName = t[n]
          })
        }), a.getCustomOverlaySupportType().then(function(e) {
          b.CustomOverlaySupportType = e, b.loadCustomOverlayInfo()
        }), l.getusersDirectory().then(function(e) {
          b.userDir = e
        }), c.reloadSystemInfo().then(function(e) {
          x.info("HOSTNAME = ", e.PCName), b.hostMachineName = e.PCName
        }), b.blDisabled = o("translate")("l10n.settingsBroadcastLiveDisable", {
          arg1: a._currentPortalName
        }), v().then(function(e) {
          b.settingsDisabled = e[0] === h.ACTIVE || e[0] === h.PAUSED, b.settingsData = {
            feature: p.BROADCAST,
            source: l.getVideoState(),
            disabled: b.settingsDisabled,
            executeBack: !1,
            executeSave: !1,
            callback: i.callback
          }
        })
      }, b.loadCustomOverlayInfo = function() {
        if (1 === b.CustomOverlaySupportType) return void a.getCustomOverlayEnabled().then(function(e) {
          b.isCustomOverlayEnabled = e, x.info("Broadcast Custom Overlay enabled = ", b
              .isCustomOverlayEnabled), b.customOverlayFilePath = "", b.isCustomOverlayEnabled && a
            .getCustomOverlayFilePath().then(function(e) {
              b.customOverlayFilePath = e, x.info("Broadcast Custom Overlay PNG file = ", b
                .customOverlayFilePath)
            })
        });
        if (2 === b.CustomOverlaySupportType) {
          b.lastSelectedFilePath = a.lastSelectedFilePath;
          for (var e = function(e) {
              a.getMultipleCustomOverlayEnabled(e).then(function(t) {
                b.isMultipleCustomOverlayEnabled[e - 1] = t, b.multipleCustomOverlayFilePath[e - 1] = b
                  .slot[e - 1], b.multipleCustomOverlayFileSize[e - 1] = "", b
                  .isMultipleCustomOverlayEnabled[e - 1] && a.getMultipleCustomOverlayFilePath(e).then(
                    function(t) {
                      b.multipleCustomOverlayFilePath[e - 1] = t, x.info(
                        " Broadcast Multiple Custom Overlay PNG file = ", b.multipleCustomOverlayFilePath[
                          e - 1]), u.getGalleryImageFileDimensions(t).then(function(t) {
                        if (t) {
                          var n = t.width,
                            i = t.height,
                            o = n.toString(),
                            r = i.toString(),
                            a = o;
                          a = a.concat(" x "), a = a.concat(r), b.multipleCustomOverlayFileSize[e - 1] =
                            a, x.info(" Broadcast Multiple Custom Overlay PNG file size = ", b
                              .multipleCustomOverlayFileSize[e - 1])
                        } else b.disableMultipleCustomOverlay(e)
                      })
                    })
              })
            }, t = 1; t <= 3; t++) e(t)
        }
      }, b.keyUp = function(e, t) {
        e.keyCode === g.ENTER && (b.broadcastControlStatus = 1 !== t, b.setBroadcastControlStatus())
      }, b.setIngestServer = function() {
        l.setPreferredTwitchIngestServer(b.selectedIngestServerName), x.info("Setting ingest server " + b
          .selectedIngestServerName)
      }, b.disableCustomOverlay = function() {
        a.setCustomOverlayEnabled(!1), b.isCustomOverlayEnabled = !1, b.customOverlayFilePath = ""
      }, b.disableMultipleCustomOverlay = function(e) {
        a.setMultipleCustomOverlayEnabled(!1, e), b.isMultipleCustomOverlayEnabled[e - 1] = !1, b
          .multipleCustomOverlayFilePath[e - 1] = b.slot[e - 1], b.multipleCustomOverlayFileSize[e - 1] = ""
      }, b.setCustomOverlayFilePath = function(e) {
        return a.setCustomOverlayFilePath(e).then(function(e) {
          return a.setCustomOverlayEnabled(!0)
        })
      }, b.setMultipleCustomOverlayFilePath = function(e, t) {
        return a.setMultipleCustomOverlayFilePath(e, t).then(function(e) {
          return a.setMultipleCustomOverlayEnabled(!0, t)
        })
      }, b.launchFileBrowser = function() {
        var e = o("translate")("l10n.broadcastLive") + " > " + o("translate")("l10n.broadcastOverlay"),
          t = b.customOverlayFilePath,
          i = b.customOverlayFilePath;
        t.length > 0 && i.lastIndexOf("\\") > 0 ? (t = t.slice(0, t.lastIndexOf("\\")), i = i.slice(i.lastIndexOf(
          "\\") + 1)) : (i = "", t = b.userDir + "\\Pictures");
        var r = {
          parentView: "main.preferences.broadcast",
          heading: e,
          init: i,
          currentPath: t,
          callback: b.setCustomOverlayFilePath,
          includeFiles: !0,
          match: ".png"
        };
        n.go("main.preferences.recordings.folder-browser", r)
      }, b.launchFileBrowserMultiple = function(e) {
        var t = o("translate")("l10n.broadcastLive") + " > " + o("translate")("l10n.broadcastOverlay"),
          i = "",
          r = "";
        e >= 1 && e <= 3 && b.isMultipleCustomOverlayEnabled[e - 1] ? (i = b.multipleCustomOverlayFilePath[e - 1],
          r = b.multipleCustomOverlayFilePath[e - 1], i.length > 0 && i.lastIndexOf("\\") > 0 && (i = i.slice(0,
            i.lastIndexOf("\\")), r = r.slice(r.lastIndexOf("\\") + 1))) : (i = b.lastSelectedFilePath, i = i
          .slice(0, i.lastIndexOf("\\")), r = ""), i.length <= 0 && (r = "", i = b.userDir + "\\Pictures");
        var a = {
          parentView: "main.preferences.broadcast",
          heading: t,
          init: r,
          currentPath: i,
          callback: b.setMultipleCustomOverlayFilePath,
          includeFiles: !0,
          match: ".png",
          callbackParam: e
        };
        n.go("main.preferences.recordings.folder-browser", a)
      }, b.fromMainMenu = function() {
        return l.getVideoState() === m.MAIN
      }, b.return = function() {
        b.fromMainMenu() ? n.go("main.main-menu") : n.go("main.preferences")
      }, b.back = function() {
        b.settingsData.executeBack = !0
      }, b.save = function() {
        b.settingsData.executeSave = !0
      }, b.customizeCloseComplete = function() {
        x.info("Customize close complete (broadcast)"), b.return()
      }, d.on(f.ESCAPE, b.back), e.$on("$destroy", function() {
        d.off(f.ESCAPE, b.back)
      })
    }
  ]);
  t.PreferencesBroadcastController = o
}
