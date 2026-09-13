// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 242
// controller PreferencesHighlightsController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.PreferencesHighlightsController = void 0;
  var r = require(8),
    a = o(r),
    l = require(3),
    s = i(l),
    d = require(1) /* app/1 — main (module) */;
  require(60) /* app/60 — sdkService (service) */;
  var c = d.ngMainModule.controller("PreferencesHighlightsController", ["$log", "$scope", "$state",
    "$stateParams", "$filter", "$timeout", "$q", "eventAggregator", "sdkService", "oscDisplayService",
    "telemetryService", "RECORDING_PATH_TYPES", "KEYBOARD_EVENTS", "HIGHLIGHTS_EVENTS", "OSC_KEYBOARD",
    "TELEMETRY_OSC_EVENT_NAMES", "HIGHLIGHT_PERMISSIONS",
    function(e, t, n, i, o, r, l, d, c, u, f, m, g, p, h, b, x) {
      function v(e, t) {
        var n = {
            title: "l10n.error",
            icon: "icon-notify_warning",
            status: "",
            question: "l10n.highlights",
            footnote: e,
            topButton: "l10n.gotIt",
            bottomButton: "",
            topAction: t,
            bottomAction: "",
            closeOSC: !1,
            lastState: ""
          },
          i = "main.confirmation";
        u.openOSC(i, n);
      }

      function y() {
        return c.getHighlightsEnabled().then(function(e) {
          O.highlightsEnabled = e, O.originalSettings.highlightsEnabled = e;
        }, function(e) {
          A.error("ERROR retrieving highlights enabled: ", e), O.highlightsEnabled = !1, O
            .originalSettings.highlightsEnabled = !1;
        });
      }

      function w(e) {
        c.getPermissions(e).then(function(e) {
          return O.gamePermissions = e.permissions && (e.permissions.highlightsRecordVideo == x
              .GRANTED || e.permissions.highlightsRecordScreenshot == x.GRANTED) && e.permissions
            .highlightsRecordVideo != x.DENIED && e.permissions.highlightsRecordScreenshot != x
            .DENIED, O.originalSettings.gamePermissions = O.gamePermissions, O.gamePermissions;
        }, function(e) {
          return A.error("Exception getting game highlight permission", e), O.gamePermissions = !1, l
            .when(O.gamePermissions);
        });
      }

      function S() {
        var e = c.getSDKInstance();
        return void 0 !== e.shortName && "" !== e.shortName ? c.getGameHighlightsSettings(c
          .getSDKInstance().shortName).then(function(e) {
          return O.sdkGameRunning = !0, O.gameShortName = c.getSDKInstance().shortName, O
            .gameDisplayName = c.getSDKInstance().drsProfileName, O.gameHighlights = e.highlights, O
            .originalSettings.gameHighlights = JSON.parse((0, a.default)(O.gameHighlights)), w(O
              .gameShortName);
        }) : (O.gamePermissions = !1, l.when(O.gamePermissions));
      }

      function E() {
        return y().then(function() {
          return S();
        });
      }

      function k() {
        return c.getCustomize().then(function(e) {
          0 !== e ? (O.highlightsPath = e.tempSaveFolder, O.maxSpace = O.initSpace = e.sizeMB / 1024,
            "" === O.highlightsPath && (O.highlightsPath = M)) : (O.maxSpace = I, O.highlightsPath =
            M, A.info("Using default max size: ", O.maxSpace));
        });
      }

      function _() {
        A.info("Move progress bar started!"), O.percentComplete = 0, O.moveInProgress = !0, d.off(p
          .MOVE_STARTED, _);
      }

      function T(e) {
        A.info("Move update progress bar: ", e.toFixed(0)), O.percentComplete = e;
      }

      function C() {
        d.off(p.MOVE_INPROGRESS, T), d.off(p.MOVE_DONE, C), r(function() {
          A.info("Move progress bar done!"), O.moveInProgress = !1;
        }, O.timeoutDelay);
      }
      var O = this;
      O.title = "l10n.settings", O.icon = "icon-settings", O.status = "", O.hlWarningString = "";
      var A = e.getInstance("osc/preferencesHighlightsController"),
        I = 5,
        M = "C:";
      O.highlightsPath = "", O.highlightsNewPath = "", O.maxSpace = I, O.initSpace = 0, O
        .moveInProgress = !1, O.timeoutDelay = 500, O.highlightsEnabled = !1, O.settingsDisabled = !1, O
        .gamePermissions = !1, O.gameHighlights = {}, O.sdkGameRunning = !1, O.originalSettings = {
          highlightsEnabled: !1,
          gamePermissions: !1,
          gameHighlights: {}
        }, O.entries = {
          on: {
            name: "on",
            title: "l10n.yes",
            value: !0,
            initialFocus: !0
          },
          off: {
            name: "off",
            title: "l10n.no",
            value: !1,
            initialFocus: !1
          }
        }, O.setHighlightsEnabled = function() {
          return c.setHighlightsEnabled(O.highlightsEnabled).then(function() {
            return R();
          }, function(e) {
            A.error("ERROR setting highlights enabled: ", e);
          });
        };
      var R = function(e) {
        return c.isHighlightsActive().then(function(e) {
          O.settingsDisabled = e, e === !0 && (O.hlWarningString = o("translate")(
            "l10n.settingsHighlightsDisable", {
              arg1: c.sdkInstance.drsProfileName
            }));
        });
      };
      O.setGamePermissions = function() {
        var e = {};
        return e.highlightsRecordVideo = e.highlightsRecordScreenshot = O.gamePermissions ? x.GRANTED :
          x.DENIED, c.setPermissions(O.gameShortName, e).then(function() {
            return A.info("Game permissions changed " + c.sdkInstance.drsProfileName + " to " + O
              .gamePermissions), R();
          });
      }, O.setGameHighlights = function(e) {
        c.setGameHighlights(O.gameShortName, O.gamePermissions, O.gameHighlights).then(function() {
          A.info("Game highlights changed " + c.sdkInstance.drsProfileName + " to " + O
            .gameHighlights);
        });
      }, O.initPreferencesHighlights = function() {
        return R().then(function() {
          return s.isUndefined(i.newPath) || "" === i.newPath ? void l.all([E(), k()]) : (O
            .highlightsNewPath = i.newPath, O.initSpace = O.maxSpace = i.currentSize, O
            .highlightsEnabled = O.originalSettings.highlightsEnabled = i.enabled, O
            .initializeMoveProgressBar(), c.setCustomizePath(O.highlightsNewPath).then(function(
            e) {
              void 0 === O.percentComplete && (O.timeoutDelay = 0), r(function() {
                O.moveComplete();
              }, O.timeoutDelay);
            }, function(e) {
              A.error("Move error:", e);
              var t = "l10n.moveError",
                n = o("translate")(t, {
                  arg1: i.oldPath,
                  arg2: O.highlightsNewPath
                });
              v(n, O.moveComplete);
            }));
        });
      }, O.moveComplete = function() {
        d.off(p.MOVE_INPROGRESS, T), d.off(p.MOVE_DONE, C), d.off(p.MOVE_STARTED, _);
        var e = {
          newPath: "",
          currentSize: 0
        };
        n.go("main.preferences.highlights", e);
      }, O.setHighlightsTempFilePath = function() {
        var e = {
          newPath: O.highlightsNewPath,
          oldPath: O.highlightsPath,
          currentSize: O.maxSpace
        };
        n.go("main.preferences.highlights", e);
      }, O.promptUserToMoveFiles = function(e) {
        if (A.info("User changed temp folder, path: ", e), O.highlightsPath === e) A.info(
          "Folder did not change, do nothing!"), n.go("main.preferences.highlights");
        else {
          O.highlightsNewPath = e;
          var t = "l10n.moveHighlights",
            i = o("translate")(t, {
              originalLocation: O.highlightsPath,
              newLocation: e
            }),
            r = {
              title: "l10n.settings",
              icon: "icon-settings",
              status: "l10n.highlights",
              question: i,
              footnote: "",
              topButton: "l10n.yes",
              bottomButton: "l10n.back",
              topAction: O.setHighlightsTempFilePath,
              bottomAction: "",
              closeOSC: !1,
              lastState: "main.preferences.highlights"
            },
            a = "main.confirmation";
          u.openOSC(a, r);
        }
      }, O.getHighlightsPath = function() {
        return A.info("Changing highlights path: ", O.highlightsPath), O.checkSettings().then(
      function() {
          n.go("main.preferences.recordings.folder-browser", {
            parentView: "main.preferences.highlights",
            heading: "l10n.folderBrowserTitleHighlights",
            currentPath: O.highlightsPath,
            pathType: m.HIGHLIGHTS,
            callback: O.promptUserToMoveFiles
          });
        });
      }, O.HighlightsSlider = {
        min: 1,
        max: 100,
        step: 1,
        ticks: [1, 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100]
      }, O.highlightChanged = function() {
        A.info("Highlight max space changed: ", 1024 * O.maxSpace);
      }, O.checkMaxSpace = function() {
        if (O.initSpace != O.maxSpace) {
          var e = 1024 * O.maxSpace;
          return O.initSpace = O.maxSpace, c.setCustomizeSize(e).then(function(t) {
            return t === !0 && (A.info("Using new maxSpace (MB): ", e), f.push(b
              .OSC_HIGHLIGHTS_DISC_SPACE_SETTING, {
                sizeMB: e
              })), c.highlightsRecoverSpace();
          }, function(e) {
            A.error("Max space error:", e);
          });
        }
        return l.when(!0);
      }, O.updateTelemetry = function() {
        O.originalSettings.highlightsEnabled != O.highlightsEnabled && (A.info(
            "Update HL Enabled telemetry"), f.push(b.OSC_HIGHLIGHTS_GAME_TOGGLE, {
            gameName: "All",
            shutoffType: "Settings",
            onOffState: f.bool2Toggle(O.highlightsEnabled),
            DRSName: c.sdkInstance.drsName,
            DRSProfileName: c.sdkInstance.drsProfileName
          }), O.originalSettings.highlightsEnabled = O.highlightsEnabled), O.originalSettings
          .gamePermissions != O.gamePermissions && (A.info("Update Game Permissions telemetry"), f.push(
            b.OSC_HIGHLIGHTS_GAME_TOGGLE, {
              gameName: c.sdkInstance.shortName,
              shutoffType: "Settings",
              onOffState: O.gamePermissions ? "On" : "Off",
              DRSName: c.sdkInstance.drsName,
              DRSProfileName: c.sdkInstance.drsProfileName
            }), O.originalSettings.gamePermissions = O.gamePermissions), s.isEqual(O.gameHighlights, O
            .originalSettings.gameHighlights) || (A.info("Update Game Highlights telemetry"), s.each(O
            .gameHighlights,
            function(e, t) {
              f.push(b.OSC_HIGHLIGHTS_INDIVIDUAL_TOGGLE, {
                gameName: c.sdkInstance.shortName,
                highlightId: t || "",
                onOffState: e.userEnabled ? "On" : "Off",
                DRSName: c.sdkInstance.drsName,
                DRSProfileName: c.sdkInstance.drsProfileName
              });
            }), O.originalSettings.gameHighlights = JSON.parse((0, a.default)(O.gameHighlights)));
      }, O.checkSettings = function() {
        return O.updateTelemetry(), O.checkMaxSpace();
      }, O.keyUp = function(e, t) {
        e.keyCode === h.ENTER && (O.highlightsEnabled = 1 !== t, O.setHighlightsEnabled());
      }, O.initializeMoveProgressBar = function() {
        d.on(p.MOVE_STARTED, _), d.on(p.MOVE_INPROGRESS, T), d.on(p.MOVE_DONE, C);
      }, O.done = function() {
        O.checkSettings().then(function() {
          n.go("main.preferences");
        });
      }, d.on(g.ESCAPE, O.done), O.initPreferencesHighlights(), t.$on("$destroy", function() {
        O.checkSettings().then(function() {
          A.info("Preferences Highlights shutting down!"), d.off(g.ESCAPE, O.done);
        });
      });
    }
  ]);
  exports.PreferencesHighlightsController = c;
}
