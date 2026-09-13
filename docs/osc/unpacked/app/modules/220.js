// ─────────────────────────────────────────────────────────────
// APP MODULE 220
// role       : controller OctoolMenuController
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
  }), t.OctoolMenuController = void 0;
  var o = n(444),
    r = i(o),
    a = n(1);
  n(47), n(12), n(22), n(4);
  var l = a.ngMainModule.controller("OctoolMenuController", ["$state", "$stateParams", "$scope", "$q", "$filter",
    "$log", "$interval", "$timeout", "eventAggregator", "cefService", "hotkeyService", "octoolService",
    "oscDisplayService", "shadowPlayService", "keyboardService", "settingsService", "KEYBOARD_EVENTS",
    "HOTKEY_EVENTS", "OSC_KEYBOARD", "TELEMETRY_OSC_AVAILABLE_STATUS", "OVERCLOCKING_FEATURE", "DISCLAIMER_OPTIONS",
    "PERFTOOL_EVENTS", "OC_NOT_SUPPORTED_STATE", "OC_SCANNER_STATUS",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S, E, k, _) {
      function T(e) {
        var t = {
          isOn: !1,
          visible: !0,
          gpuIndex: e,
          onKeyDown: function(e) {
            (this.isOn && e.keyCode === v.LEFT_ARROW || !this.isOn && e.keyCode === v.RIGHT_ARROW) && (this
              .isOn = !this.isOn, this.onChange())
          },
          onChange: function(t) {
            var n = i.when(!0);
            t || (n = L(e, this.isOn));
            var o = function(t) {
              var n = this;
              return t ? (this.isOn === !1 && H(), void Q[e].ocSliderControls.forEach(function(e) {
                "offset" === e.category && (e.enabled = !n.isOn, n.isOn ? (e.backedUpvalue = e.value, e
                  .value = e.default) : e.value = e.backedUpvalue)
              })) : void(this.isOn = !this.isOn)
            };
            n.then(o.bind(this))
          }
        };
        return t
      }

      function C(e) {
        var t = {};
        return t.items = [], e.ocConfig.manualOcSupported && (t.items.push("l10n.perfmonoc.manual"), f
            .sidebarSessionParams.featureSupported = w.manualOverclocking, e.ocConfig.manualOcScannerSupported && e
            .ocConfig.backgroundOcScannerSupported ? (X.info("AutoTuning supported"), t.items.push(
                "l10n.perfmonoc.automatic"), e.ocConfig.grdOcSupported ? f.sidebarSessionParams.featureSupported = w
              .gameReadyDriverOverclocking : f.sidebarSessionParams.featureSupported = w.automaticOverclocking, e
              .backgroundOcScannerEnabled ? t.selectedIndex = 1 : t.selectedIndex = e.uiState.tuningTypeSwitch && e
              .uiState.tuningTypeSwitch.selectedIndex || 0) : t.selectedIndex = 0), q.forceRemoveManualOC && t.items
          .length > 1 && (t.selectedIndex = 1), t
      }

      function O(e) {
        var t = e.gpu.index;
        Q[t].gpu = e.gpu, Q[t].ocConfig = e.ocConfig, Q[t].uiState = e.uiState, f.isOCSupported() ? (Q[t]
          .ocSliderControls = e.ocSliderControls, Q[t].fanSettings = e.fanSettings, Q[t].fanSettings
          .autoPowerSwitch && (Q[t].fanSettings.isSwitchSupported = !1, s(function() {
            Q[t].fanSettings.autoPowerSwitch.selectedIndex = Q[t].fanSettings.autoPower ? 0 : 1, Q[t]
              .fanSettings.isSwitchSupported = Q[t].fanSettings.isSupported
          }, 0)), Q[t].scanStatus = e.scanStatus, Q[t].autoTuningSwitch = T(t), Q[t].autoTuningSwitch.isOn = e
          .backgroundOcScannerEnabled, Q[t].autoTuningSwitch.visible = Q[t].ocConfig
          .backgroundOcScannerSupported && Q[t].ocConfig.manualOcScannerSupported, Q[t].uiState.tuningTypeSwitch =
          C(e), Q[t].uiState.isOCSupported = !0, Q[t].uiState.isSliderStateDirty = !1, Q[t].autoTuningSwitch
          .onChange(!0)) : Q[t].uiState.isOCSupported = !1
      }

      function A(e) {
        return Q = f.gpuData, Q[e].isBackgroundOcScannerStatusUpdated = !1, f.getGPUInfoAndFillData().then(
        function() {
          f.gpuData.forEach(function(e) {
              O(e), q.gpuSelectorInput.items[e.gpu.index] = e.gpu.name
            }), q.currentPerfMetrics = ne[e], q.currentGPUData = Q[e], q.gpuSelectorInput.selectedIndex = q
            .currentGPUData.gpu.index, f.isOCSupported() && (q.offsetSliders = q.currentGPUData.ocSliderControls
              .filter(function(e) {
                return e.isSupported && "offset" === e.category
              }), q.limitSliders = q.currentGPUData.ocSliderControls.filter(function(e) {
                return e.isSupported && "limit" === e.category
              }), J || R(), f.checkLimiters(e)), X.info("All data refreshed"), f.sidebarSessionParams.gpuCount =
            Q.length, X.info("refreshRequiredData allGPUData[0].gpu : ", Q[0].gpu), f.sidebarSessionParams
            .GPU1 = Q[0].gpu.name, f.sidebarSessionParams.gpuCount > 1 && (f.sidebarSessionParams.GPU2 = Q[1]
              .gpu.name)
        })
      }

      function I() {
        q.disclaimerText = o("translate")("l10n.perfmonoc.disclaimerText"), f.betaToProdFeatureEnabled() && (q
          .disclaimerText = q.disclaimerText + "<br><br>" + o("translate")("l10n.perfmonoc.disclaimerText2"), f
          .getGameIsRunning() && c.isInDesktopMode().then(function(e) {
            e || (q.disclaimerText = q.disclaimerText + "<br><br>" + o("translate")(
              "l10n.perfmonoc.fullScreenDisclaimer"))
          }))
      }

      function M() {
        h.getLanguage().then(function(e) {
          X.info("language", e), q.isTrunctionLocaleSet = !ie.includes(e), q.disclaimerText = o("translate")(
            "l10n.perfmonoc.disclaimerText")
        }), N();
        var e = 0;
        q.currentGPUData = f.gpuData[e], q.currentPerfMetrics = ne[e], q.gpuSelectorInput.items = [], q
          .currentGPUData.gpu && (q.gpuSelectorInput.items[e] = q.currentGPUData.gpu.name), A(e).then(function() {
            ne.length !== f.gpuData.length && (X.info("Refresh perf metric sets"), N(), q.currentPerfMetrics = ne[
              e])
          }), U(), D(), P(), f.resetSidebarSessionParams(), B(), f.scanOperationInProgress = !1, f
          .scanStopInProgress = !1, K(), X.info("Initialized")
      }

      function R() {
        H(), J = l(H, 6e4)
      }

      function P() {
        if (q.perfOverlayText = o("translate")("l10n.perfmonoc.performanceOverlay"), g.getHotkeyShortcut(g
            .HotkeyShortcuts.PERFOVERLAYTOGGLE).then(function(e) {
            e && e.keys && (q.perfOverlayText = o("translate")("l10n.perfmonoc.performanceOverlayWithHotkey", {
              arg1: p.shortcutToStr(e.keys)
            }))
          }), f.isPerfOverlayEnabled) {
          var e = f.overlayViews.find(function(e) {
            return e.id === f.selectedOverlayViewName
          });
          return void(e && (q.perfOverlayViewText = o("translate")(e.name)))
        }
        q.perfOverlayViewText = o("translate")("l10n.off")
      }

      function D() {
        c.isInDesktopMode().then(function(e) {
          e || (Z = u.createHotKey(g.HotkeyShortcuts.OCTOOLUITOGGLE, x.OCTOOLUI_TOGGLE), null != Z && Z
            .startHotkeyDetection())
        })
      }

      function N() {
        f.createPerfMetricsSets(), f.perfMetrics.forEach(function(e, t) {
          var n = [],
            i = 2,
            o = e.filter(function(e) {
              return e.isSidebarMetric && e.visible
            });
          o.forEach(function(e, t) {
            var o = t % i;
            n[o] = n[o] || [], n[o].push(e)
          }), ne[t] = n
        })
      }

      function L(e, t) {
        X.info("set Automatic Tuning to", t);
        var n = i.when(!0);
        return n = G(), n.then(function(n) {
          if (oe && (oe = void 0), n) return X.info("concent recieved, proceed", t), f
            .scanOperationInProgress = !0, f.sidebarSessionParams.appliedStatus = y.yes, f.checkLimiters(e), f
            .enableBackgroundScan(e, t).then(function() {
              var n = t ? f.startManualScan(e) : f.stopManualScan(e);
              return n
            }).then(function(e) {
              return X.info("set Automatic Tuning response", e), !0
            })
        })
      }

      function F() {
        return q.showConsentForm ? (X.info("ESC pressed on consent form"), void q.consentCallback(!1)) : (X.info(
          "ESC pressed, close menu"), void W())
      }

      function U() {
        d.on(b.ESCAPE, F), d.on(x.OCTOOLUI_TOGGLE, W), d.on(E.PERF_OCSCAN_COMPLETION_UPDATE, H), d.on(E
          .PERF_AUTO_TUNING_INFO_UPDATED, j)
      }

      function z() {
        f.setOCAdminConsent(!0).then(function() {
          f.getOCAdminConsent().then(function(e) {
            X.info("admin consent after UAC prompt", e), f.sidebarSessionParams
              .disclaimerPromptOptionSelected = e.adminConsent ? S.agreed : S.uacCancelled, oe.resolve(e
                .adminConsent)
          }).catch(function(e) {
            X.error("Failed to set consent", e), oe.resolve(!1)
          })
        })
      }

      function G() {
        if (f.betaToProdFeatureEnabled()) oe = i.defer(), f.getOCAdminConsent().then(function(e) {
          e.adminConsent && f.persistedData.ocData.consent ? (X.info("Already have consent, proceed"), q
            .showConsentForm = !1, oe.resolve(!0)) : (X.info("No consent, show disclamimer"), q
            .showConsentForm = !0, I())
        });
        else {
          if (f.persistedData.ocData.consent) return X.info("Already have consent, proceed"), i.when(!0);
          q.showConsentForm = !0, oe = i.defer()
        }
        return oe.promise
      }

      function V(e) {
        var t = "";
        switch (e) {
          case _.CANCELLED_SERVICE_SUSPENDED:
          case _.CANCELLED_SERVICE_STOPPED:
          case _.CANCELLED_BACKGROUND_SCAN_DISABLED:
          case _.INTERNAL_ERROR:
          case _.SCAN_CANCELLED:
          case _.CANCELLED_CONFIGURATION_NOT_SUPPORTED:
            t = o("translate")("l10n.perfmonoc.serviceStopped");
            break;
          case _.CANCELLED_USER_REQUESTED:
            t = o("translate")("l10n.perfmonoc.canceledByUser");
            break;
          case _.CANCELLED_GPU_BUSY:
            t = o("translate")("l10n.perfmonoc.highGpuUtil");
            break;
          case _.CANCELLED_USER_INPUT_DETECTED:
            t = o("translate")("l10n.perfmonoc.noIdleSystem");
            break;
          case _.CANCELLED_POWER_SWITCH_TO_DC:
            t = o("translate")("l10n.perfmonoc.laptopInDC");
            break;
          default:
            X.error("Invalid error code found", e), t = o("translate")("l10n.perfmonoc.serviceStopped")
        }
        return t
      }

      function H() {
        if (angular.isDefined(q.currentGPUData.scanStatus.errorCode) && 0 !== q.currentGPUData.scanStatus
          .errorCode && !q.currentGPUData.scanStatus.isLastScanSuccessful && (q.currentGPUData.autoTuningSwitch
            .isOn === !1 && q.currentGPUData.scanStatus.errorCode === _.CANCELLED_USER_REQUESTED || q.currentGPUData
            .autoTuningSwitch.isOn === !0)) return void(q.lastScanTime = V(q.currentGPUData.scanStatus.errorCode));
        if (!q.currentGPUData.scanStatus.isLastScanSuccessful) return q.lastScanTime = "", void(q.currentGPUData
          .scanStatus.resultText = "--");
        var e = q.currentGPUData.scanStatus.completionTime;
        if (e) {
          var t = new Date,
            n = Math.floor(t.getTime() / 6e4),
            i = n - Math.floor(e / 6e7),
            r = "";
          if (i < 0) return r;
          if (i > 2880) {
            var a = Math.round(i / 1440);
            r = o("translate")("l10n.perfmonoc.daysAgo", {
              arg1: a
            })
          } else if (i > 119) {
            var l = Math.round(i / 60);
            r = o("translate")("l10n.perfmonoc.hoursAgo", {
              arg1: l
            })
          } else i < 1 && (i = 1), r = o("translate")("l10n.perfmonoc.minutesAgo", {
            arg1: i
          });
          q.lastScanTime = "(" + r + ")"
        }
      }

      function B() {
        ee = -392, te = 392, q.posDashLeftCnt = -1, q.posDashRightCnt = 0
      }

      function Y(e) {
        f.isOCSupported() && (X.info("cancelForGPU gpuIndex:", e), Q[e].ocSliderControls.forEach(function(e) {
          e.value = e.current, e.backedUpvalue = e.current
        }), Q[e].fanSettings.isSupported && (Q[e].fanSettings.sliderSetting.value = Q[e].fanSettings
          .sliderSetting.current, Q[e].fanSettings.autoPower = Q[e].fanSettings.autoPowerApplied, Q[e]
          .fanSettings.autoPowerSwitch.selectedIndex = Q[e].fanSettings.autoPower ? 0 : 1, Q[e].fanSettings
          .isSwitchSupported = !1, s(function() {
            var t = Q[e].fanSettings.isSupported;
            Q[e].fanSettings.isSwitchSupported = t
          }, 0)), Q[e].uiState.isSliderStateDirty = !1)
      }

      function $() {
        Q.forEach(function(e) {
          Y(e.gpu.index)
        })
      }

      function W() {
        X.info("closeMenu"), t && "base" === t.lastState ? m.closeOSC() : e.go("main.main-menu")
      }

      function j(e) {
        var t = e.gpuId;
        return X.info("updateAutoTuningInfo for GPU", t, f.gpuData[t].backgroundOcScannerEnabled), f
          .scanStopInProgress && 0 === e.type ? void X.info("stop scan is in progress, ignoring") : Q[t] ? Q[t]
          .isBackgroundOcScannerStatusUpdated ? void X.info("background scanner status already updated, ignoring") :
          void(Q[t].autoTuningSwitch && (Q[t].isBackgroundOcScannerStatusUpdated = !0, Q[t].autoTuningSwitch.isOn =
            f.gpuData[t].backgroundOcScannerEnabled)) : void 0
      }

      function K() {
        f.getGameIsRunning() && c.isInDesktopMode().then(function(e) {
          X.info("Fullscreen Status when sidebar launched : ", !e), e || (f.sidebarSessionParams.isFullscreen =
            y.yes)
        })
      }
      var q = this,
        X = a.getInstance("osc/OctoolMenuController"),
        Z = null,
        Q = [];
      q.currentGPUData = {}, q.currentPerfMetrics = [], q.gpuSelectorInput = {
        wide: !0
      }, q.showConsentForm = !1, q.perfOverlayText = "", q.perfOverlayViewText = "", q.forceRemoveManualOC = !0;
      var J, ee, te, ne = [];
      q.isTrunctionLocaleSet = !0, q.disclaimerText = "";
      var ie = ["en-US", "en-GB"],
        oe = void 0;
      q.consentCallback = function(e) {
        oe || X.error("consentPromise undefined, should not be here"), X.info("Consent agree status:", e), f
          .betaToProdFeatureEnabled() || oe.resolve(e), q.showConsentForm = !1, e ? (f.persistedData.ocData
            .consent = !0, f.sidebarSessionParams.disclaimerPromptOptionSelected = S.agreed, f
            .betaToProdFeatureEnabled() && z()) : (f.sidebarSessionParams.disclaimerPromptOptionSelected = S
            .cancelled, f.betaToProdFeatureEnabled() && oe.resolve(e))
      }, q.isLastScanIconHidden = function() {
        return !q.currentGPUData.scanStatus.isLastScanSuccessful
      }, q.perfDashboardLeft = function() {
        0 != q.posDashLeftCnt && (r.selectAll(".metricRow").transition().duration(1e3).style("transform",
            "translate(" + ee + "px,0px)"), te = ee + 392, ee -= 392, q.posDashLeftCnt++, q.posDashRightCnt--, f
          .sidebarSessionParams.perfMetricScrollUsed = y.yes)
      }, q.perfDashboardRight = function() {
        0 != q.posDashRightCnt && (r.selectAll(".metricRow").transition().duration(1e3).style("transform",
            "translate(" + te + "px,0px)"), ee = te - 392, te += 392, q.posDashLeftCnt--, q.posDashRightCnt++, f
          .sidebarSessionParams.perfMetricScrollUsed = y.yes)
      }, q.gotoHudSetting = function() {
        X.info("Going to HUD setting menu"), f.sidebarSessionParams.HUDSettingUsed = y.yes;
        var t = {
          selectedOverlay: "Performance",
          lastState: e.current.name,
          lastParams: e.params
        };
        e.go("main.preferences.overlays", t)
      }, q.onGPUChange = function(e) {
        X.info("GPU index changed to:", e), q.currentGPUData = Q[e], q.currentPerfMetrics = ne[e], B(), f
          .sidebarSessionParams.gpuChevronUsed = y.yes
      }, q.onTuningTypeChange = function(e) {
        X.info("Tuning type changed to:", e), q.currentGPUData.uiState.tuningTypeSwitch.selectedIndex = e, 0 ===
          e && (q.currentGPUData.autoTuningSwitch.isOn = !1, q.currentGPUData.autoTuningSwitch.onChange()), f
          .sidebarSessionParams.tuningTypeChevronUsed = y.yes;
      }, q.onFanSettingModeChange = function(e) {
        X.info("Fan setting mode changed to:", e), q.currentGPUData.fanSettings.autoPower = 0 === e, q
          .currentGPUData.uiState.isSliderStateDirty = !0
      }, q.apply = function() {
        var e = q.currentGPUData.gpu.index;
        X.info("Apply gpuIndex:", e), G().then(function(t) {
          oe && (oe = void 0), t && (q.currentGPUData.ocSliderControls.forEach(function(e) {
            e.enabled && (e.current = e.value)
          }), f.setManualOCLimits(e).then(function() {
            q.currentGPUData.fanSettings.isSupported && (q.currentGPUData.fanSettings.sliderSetting
                .current = q.currentGPUData.fanSettings.sliderSetting.value, q.currentGPUData
                .fanSettings.autoPowerApplied = q.currentGPUData.fanSettings.autoPower, f
                .setFanSettings(e)), q.currentGPUData.uiState.isSliderStateDirty = !1, f
              .sidebarSessionParams.appliedStatus = y.yes
          }), f.updatePersistedOCSettings())
        })
      }, q.cancel = function() {
        X.info("cancel");
        var e = q.currentGPUData.gpu.index;
        Y(e)
      }, q.areButtonsVisible = function() {
        return q.currentGPUData.uiState.isSliderStateDirty
      }, q.areButtonsDisabled = function() {
        return q.currentGPUData.scanStatus.isScanInProgress
      };
      var re;
      q.restoreOCDefaults = function() {
        var e = q.currentGPUData.gpu.index;
        return X.info("Restore defaults gpuIndex:", e), q.restoreDefaultsInprogress = !0, f.sidebarSessionParams
          .restoredDefaults = y.yes, L(e, !1).then(function(e) {
            var t = i.defer();
            if (!e) return q.restoreDefaultsInprogress = !1, i.reject("No consent");
            if (!f.scanStopInProgress) return i.when(!0);
            re && l.cancel(re);
            var n = 0;
            return re = l(function() {
              return n++, f.scanStopInProgress ? void(120 === n ? (X.error(
                  "Scan stop operation decalred as failed after waiting for one minute"), f
                .scanStopInProgress = !1, t.resolve(!1), l.cancel(re), re = void 0) : X.info(
                "Waiting for the scan stop to complete")) : (X.info(
                  "Scan stop is completed now, proceed to restore defaults"), t.resolve(!0), l.cancel(re),
                void(re = void 0))
            }, 500), t.promise
          }).then(function() {
            return f.restoreOCDefaults(e)
          }).then(function() {
            return q.currentGPUData.fanSettings.isSupported ? (q.currentGPUData.fanSettings.autoPower = !0, f
              .setFanSettings(e)) : i.when(!0)
          }).then(function() {
            return q.currentGPUData.fanSettings.autoPowerSwitch && (q.currentGPUData.fanSettings
              .isSwitchSupported = !1, s(function() {
                q.currentGPUData.fanSettings.autoPowerSwitch.selectedIndex = q.currentGPUData.fanSettings
                  .autoPower ? 0 : 1, q.currentGPUData.fanSettings.isSwitchSupported = q.currentGPUData
                  .fanSettings.isSupported
              }, 0)), A(e)
          }).catch(function(e) {
            X.error("Failed to set tuning", e)
          }).finally(function() {
            X.info("Restore defaults operation done"), q.restoreDefaultsInprogress = !1
          })
      }, q.shouldDisableRestoreDefaults = function() {
        return q.restoreDefaultsInprogress || q.shouldShowOCSpinner()
      }, q.closeOSC = function() {
        X.info("close button clicked"), m.closeOSC()
      }, q.cancelButtonText = o("translate")("l10n.cancel"), q.headerText = o("translate")(
        "l10n.perfmonoc.performance"), q.closeMenu = W, M(), n.$on("$destroy", function() {
        X.info("OnDestroy"), $(), f.closeOcToolMenu(), Z && Z.stopHotKeyDetection(), J && l.cancel(J), q
          .currentGPUData && q.currentGPUData.autoTuningSwitch && q.currentGPUData.autoTuningSwitch.isOn ? f
          .sidebarSessionParams.automaticTuningEnabled = y.yes : f.sidebarSessionParams.automaticTuningEnabled =
          y.no, d.off(b.ESCAPE, F), d.off(x.OCTOOLUI_TOGGLE, W), d.off(E.PERF_OCSCAN_COMPLETION_UPDATE, H), d
          .off(E.PERF_AUTO_TUNING_INFO_UPDATED, j), re && l.cancel(re)
      }), q.getOcSupported = function() {
        var e = q.currentGPUData && q.currentGPUData.uiState && q.currentGPUData.uiState.isOCSupported && f
          .gpuData && 1 === f.gpuData.length;
        return f.skipExperimentalFlag() ? e : h.experimentalSetting && e
      }, q.getOcErrorMessage = function() {
        if (f.gpuData && f.gpuData.length > 1) return "l10n.perfmonoc.perfTuningMultiGpuNotSupported";
        if (q.currentGPUData && q.currentGPUData.uiState && q.currentGPUData.uiState.isOCSupported === !1) {
          var e = function(e, t) {
              return (e & t) === t
            },
            t = q.currentGPUData.ocConfig.notSupportedStatus;
          if (e(t, k.GPU_NOT_SUPPORTED)) return "l10n.perfmonoc.perfTuningNotSupported";
          if (e(t, k.DRIVER_NOT_SUPPORTED)) return "l10n.perfmonoc.perfDriverNotSupported"
        } else if (h.experimentalSetting) return "l10n.perfmonoc.enableExperimentalForOC";
        return f.gpuData && f.gpuData.length > 1 ? "l10n.perfmonoc.perfTuningMultiGpuNotSupported" : q
          .currentGPUData && q.currentGPUData.uiState && q.currentGPUData.uiState.isOCSupported === !1 ?
          "l10n.perfmonoc.perfTuningNotSupported" : "l10n.perfmonoc.enableExperimentalForOC"
      }, q.shouldShowPerfMonSpinner = function() {
        return f.isFvSDKSessionStartInProgress
      }, q.shouldShowOCSpinner = function() {
        return f.scanOperationInProgress || f.scanStopInProgress
      }, q.isDCMode = function() {
        return f.isDC
      }
    }
  ]);
  t.OctoolMenuController = l
}
