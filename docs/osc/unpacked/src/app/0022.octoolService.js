// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 22
// service octoolService
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
  }), exports.octoolService = void 0;
  var o = require(109),
    r = i(o),
    a = require(8),
    l = i(a),
    s = require(105),
    d = i(s),
    c = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(12) /* app/12 — oscDisplayService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(47) /* app/47 — hotkeyService (service) */;
  var u = c.ngMainCommonModule.service("octoolService", ["$state", "$log", "$q", "$window", "$filter",
    "$http", "eventAggregator", "cefService", "oscDisplayService", "shadowPlayService", "hotkeyService",
    "settingsService", "telemetryService", "hardwareService", "oscNotificationService", "gfwslService",
    "PERFTOOL_EVENTS", "SHADOWPLAY_EVENTS", "HOTKEY_EVENTS", "COMMON_EVENTS", "OSC_CONFIG",
    "OVERLAY_VIEW", "SCAN_TYPE", "MODULE", "TELEMETRY_OSC_AVAILABLE_STATUS", "TELEMETRY_OSC_EVENT_NAMES",
    "TELEMETRY_OSC_TOGGLE_STATE", "SYSTEM_TYPE", "OVERCLOCKING_FEATURE", "DISCLAIMER_OPTIONS", "$timeout",
    "$interval", "OC_SCANNER_STATUS", "NOTIFIER_SELECTIONS", "PERFTOOL_VIEWNAMES",
    function(e, t, n, i, o, a, s, c, u, f, m, g, p, h, b, x, v, y, w, S, E, k, T, C, O, A, I, M, R, P, D,
      N, L, F, U) {
      function z() {
        D(function() {
          !Tt && Oe.persistedData.isPerfOverlayVisible && (Ae.info("showing perf overlay"), ke());
        }, 2e3);
      }

      function G() {
        var e = Me;
        return Ae.info("experimental", g.experimentalSetting), Ie = E.perfmonOCTool, B().then(function(
        e) {
          Ae.info("support response", e), Ie = e;
        }).finally(function() {
          return Ie && (Ae.info("Toggling Optimization of Perf-Overlay to ", Ie), c.query({
            command: "QUERY_OSC_SET_EXPERIMENTAL",
            isExperimental: Ie
          })), Me = Ie && (Be || g.experimentalSetting), H(Ie), D.cancel(Ce), Ce = D(function() {
            s.trigger(v.FEATURE_SUPPORT_STATE_CHANGE, Ie);
          }), j(), Me || !e || Be || g.experimentalSetting || Z(), Ie ? (W(), Oe.callbackMap = new d
            .default(), Oe.getGPUInfoAndFillData().then(function() {
              Me = Me && Oe.isOCSupported(), ae(), Ae.info("persisted data:", (0, l.default)(Oe
                .persistedData)), Oe.enableReflexEnhancements && (Oe.setCustAvgOnInit(), Oe
                .setDefaultLoggingPathOnInit(), Oe.checkRLAMonitor().then(function() {
                  Oe.isRLAMonitor ? (Oe.overlayViews[4].enabled = !0, Oe
                    .defaultOverlayViewName = "Latency") : (Oe.overlayViews[4].enabled = !1,
                    Oe.defaultOverlayViewName = "Basic");
                }), Oe.getSupportedDD(), Oe.getFlashIndicatorStatus() && Oe
                .setFlashIndicatorSize(!0)), Oe.createPerfMetricsSets(), Ae.info("Initialized");
            }).then(function() {
              Oe.checkForActiveGame();
            }).then(function() {
              z();
            }), n.when(!0)) : (Y(!0), n.when(!0));
        });
      }

      function V() {
        Be || G();
      }

      function H(e) {
        var t = [m.hotKeyMapping.OcToolUI, m.hotKeyMapping.ToggleLogging, m.hotKeyMapping.ResetAverages];
        Oe.enableReflexEnhancements || (t = [m.hotKeyMapping.OcToolUI]), f.dynamicHotkeyToggle(t, e);
      }

      function B() {
        return Ie ? h.getSystemInfo().then(function(e) {
          if (e) {
            var t = "1" === x.isLaptop(e);
            Be = !!Oe.betaToProdFeatureEnabled() && !t;
            var n = _.isString(e.OSVersion) ? parseInt(e.OSVersion, 10) : 0;
            if (n >= tt) {
              Ae.info("OS is suported");
              var i = _.isString(e.DriverVersion) ? parseInt(e.DriverVersion, 10) : 0;
              if (i >= nt) {
                Ae.info("driver is supported");
                var o = Array.isArray(e.GPU) && _.isString(e.GPU[0].GPUArchitecture) ? parseInt(e.GPU[
                  0].GPUArchitecture, 10) : 0;
                Ae.info("gpuArch", o);
                var r = ot < o,
                  a = it <= o && ot >= o && !t;
                if (r || a) return Ae.info("perf supported"), !0;
              }
            }
          }
          return !1;
        }).catch(function(e) {
          return Ae.error("error getSystemInfo", e.data), !1;
        }) : n.when(!1);
      }

      function Y(e) {
        if (Ae.info("Logging toggle hotkey pressed, is perf. currently enabled by file logging - ", Oe
            .isPerfEnabledbyFileLogging), Oe.isPerfEnabledbyFileLogging || e) {
          if (Oe.isPerfEnabledbyFileLogging) {
            var t = {
              enable: !1
            };
            Oe.enablePerfTracking(!1, void 0, !0), ne("LoggingToggle", t).then(function() {
              Ae.info("File logging disabled"), s.trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED, Tt), Oe
                .sendPerformanceToolLoggingSessionTelemetry(!1, !0);
            });
          }
        } else {
          var n = Oe.getLoggingPath(),
            i = {
              enable: !0,
              path: n
            };
          Oe.enablePerfTracking(!0, void 0, !0).then(function(e) {
            ne("LoggingToggle", i).then(function() {
              Ae.info("File logging enabled"), s.trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED, Tt),
                Oe.sendPerformanceToolLoggingSessionTelemetry(!0, !1);
            });
          });
        }
      }

      function $() {
        ne("ResetAverageMetrics").then(function() {
          Ae.info("Reset average called via hotkey");
        }), Oe.sendPerformanceToolResetAverageTelemetry();
      }

      function W() {
        Ae.info("registernotifications"), s.on(w.OCTOOLUI_TOGGLE, yt), s.on(w.PERFOVERLAY_TOGGLE, wt), s
          .on(w.PERFOVERLAY_CYCLE, St), s.on(w.RESET_AVERAGES, Et), s.on(w.TOGGLE_LOGGING, kt), s.on(y
            .GAME_STARTED, K), s.on(y.GAME_EXITED, X), c.registerForNotification(Xe,
            "RegisterPerfStatsNotifications").then(angular.noop, function(e) {
            Ae.info("register pert metrics notification error: ", e);
          }, Q), c.registerForNotification(Xe, "OcScanStatusUpdatesNotifications").then(angular.noop,
            function(e) {
              Ae.info("register scan status notification error: ", e);
            }, J), c.registerForNotification(Xe, "powerStatusNotification").then(angular.noop, function(
          e) {
            Ae.info("register power status notification error: ", e);
          }, _e);
      }

      function j() {
        Ae.info("unRegisterNotification"), s.off(w.OCTOOLUI_TOGGLE, yt), s.off(w.PERFOVERLAY_TOGGLE, wt),
          s.off(w.PERFOVERLAY_CYCLE, St), s.off(w.RESET_AVERAGES, $), s.off(w.TOGGLE_LOGGING, kt), s.off(y
            .GAME_STARTED, K), s.off(y.GAME_EXITED, X);
      }

      function K(e) {
        Le = !0, Ae.info("onAppStarted: ", e, " isValidProcessAttached:", Re), Oe.setProcessId(e
            .startedAppPID), Oe.isRFIIntegrated(e.startedAppCmsID, e.startedAppPID), void 0 == Oe
          .persistedData.rectAlignmentStatus && (Oe.persistedData.rectAlignmentStatus = !0), Oe
          .persistedData.rectAlignmentStatus && Oe.isRLASupportedDD && Oe.isRLAMonitor && (Tt ? Oe
            .alignMonitoringRect() : Oe.enablePerfTracking(!0, void 0, !1).then(function(e) {
              Ae.info("onAppStarted enablePerfTracking : ", e), D(function() {
                Oe.alignMonitoringRect(), D(function() {
                  Tt || Oe.enablePerfTracking(!1);
                }, st);
              }, lt);
            }));
      }

      function q() {
        if (Fe) return n.when(!0);
        if (!Ue) {
          Ue = n.defer();
          var e = 0,
            t = null,
            i = N(function() {
              e++, e >= 5 ? (Ae.error(
                  "Container plugin decalred as failed to load after waiting for 2.5 sec"), Ue
                .resolve(!1), N.cancel(i), i = void 0, Fe = !1) : (Ae.debug(
                "checking whether container plugin loaded.."), t = c.nativeQuery(Ze,
                "GetFeatureSupportState", void 0), t.finally(function() {
                Ae.debug("overclocking container plugin loaded"), t = null, Fe = !0, N.cancel(i),
                  i = void 0, Ue.resolve(!0);
              }));
            }, 500);
        }
        return Ue.promise;
      }

      function X(e) {
        e.exitedAppPID === Oe.currentAppPID && (Ae.info("Current Game app exited: ", Oe.currentAppPID),
          Re = !1, Oe.setProcessId(0), Oe.sendLatencyMetricsTelemetry()), Le = !1;
      }

      function Z() {
        Ae.info("experimental is OFF, disable auto tuning if enabled"), Oe.gpuData.forEach(function(e) {
          e.backgroundOcScannerEnabled && Oe.enableBackgroundScan(e.gpu.index, !1).then(function() {
            return Oe.stopManualScan(e.gpu.index);
          });
        });
      }

      function Q(e) {
        var t = JSON.parse(e).payload;
        if (Ie || Ae.error("updatePerfStats notification should be disabled in case of feature disable"),
          !(t && t.gpusPerfStats && t.gpusPerfStats.length > 0)) return void Ae.error(
          "No GPU stats present, ignore this FvSDK sample:", t);
        if (Oe.isFvSDKSessionStartInProgress && (Oe.isFvSDKSessionStartInProgress = !1, Ae.info(
            "Fv SDK service started succesfully"), Ae.info("perf metrics data:", t)), Pe && Ae.info(
            "perf metrics data:", t), Re = void 0 !== t.avgFps, t.AvgSWPCLatency ? Oe
          .isReflexStatsSupported = !0 : Oe.isReflexStatsSupported = !1, Oe.isRLAMonitor || Oe
          .isReflexStatsSupported === Oe.cachedReflexStatsSupported || s.trigger(v
            .PERF_OVERLAY_VISIBILITY_CHANGED, Tt), Oe.cachedReflexStatsSupported = Oe
          .isReflexStatsSupported, t.gSyncLamp) {
          var n = {
              dbAverage: !1,
              unverifiedFW: !1
            },
            i = !1;
          (0, r.default)(t.gSyncLamp).forEach(function(e) {
            if ("lamEnabled" === e) t.gSyncLamp[e] !== De && (Ae.info("Lam metrics support recieved ",
              t.gSyncLamp[e]), De = t.gSyncLamp[e], Ae.info("perf metrics data:", t), s.trigger(v
              .PERF_OVERLAY_LAMSUPPORT_CHANGED));
            else if ("lamMonitoringRect" === e) {
              var o = t.gSyncLamp[e];
              t[e] = "(" + o.topleftx + ", " + o.toplefty + ")";
            } else e.toLowerCase().includes("mouselatency") ? "mouseLatency" === e ? (n.dbAverage = n
                .dbAverage || 1 === t.gSyncLamp[e].latencySource, n.unverifiedFW = n.unverifiedFW ||
                1 === t.gSyncLamp[e].unverifiedFW, Ae.info("updatePerfStats islatestRFISupported :",
                  Oe.islatestRFISupported), t.gSyncLamp[e].latency >= 0 ? (Oe.isRLAMouseSupported = !
                  0, Oe.isReflexStatsSupported ? Oe.overlayViews[0].metricSet = ["avgFps",
                    "e2eSystemLatency", "AvgSWPCLatency"
                  ] : Oe.overlayViews[0].metricSet = ["avgFps", "e2eSystemLatency",
                    "renderingLatency"], s.trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED, Tt)) : (Oe
                  .isRLAMouseSupported = !1, Oe.isReflexStatsSupported ? Oe.overlayViews[0]
                  .metricSet = ["avgFps", "pcDisplayLatency", "AvgSWPCLatency"] : Oe.overlayViews[0]
                  .metricSet = ["avgFps", "pcDisplayLatency", "renderingLatency"], s.trigger(v
                    .PERF_OVERLAY_VISIBILITY_CHANGED, Tt))) : i = 1 === t.gSyncLamp[e]
              .polledActuation : t[e] = t.gSyncLamp[e] / 1e3;
          });
          var a = ["mouseLatency", "avgMouseLatency"];
          a.forEach(function(e) {
            t.gSyncLamp[e] && void 0 !== t.gSyncLamp[e].latency ? t.gSyncLamp[e].latency < 0 ? t[e] =
              t.gSyncLamp[e].latency : t[e] = t.gSyncLamp[e].latency / 1e3 : t[e] = void 0;
            var r = !1,
              a = "";
            "avgMouseLatency" === e ? i && (a = o("translate")(
              "l10n.perfmonoc.polledAutuationDetection"), r = !0) : (n.dbAverage && (a += o(
              "translate")("l10n.perfmonoc.databseaverage"), r = !0), n.unverifiedFW && (r ? a +=
              ", " : r = !0, a += o("translate")("l10n.perfmonoc.unverified") + " " + o(
                "translate")("l10n.perfmonoc.firmWare"))), Oe.perfMetrics.forEach(function(t) {
              var n = t.find(function(t) {
                return t.metricId === e;
              });
              n.isSuffixApplied = r, r ? n.suffix = a : n.suffix = void 0;
            });
          }), delete t.gSyncLamp;
        }
        Oe.perfStats = t, xe();
      }

      function J(e) {
        Ie || Ae.error("updateScanStatus should not come in case of feature disable");
        var t = JSON.parse(e).payload;
        if (Ae.info("scan status notification:", t), 1 === t.type) {
          var n = ce(t.gpuId);
          if (void 0 !== Oe.callbackMap.get(n)) return Oe.callbackMap.get(n)(t), void Oe.checkLimiters(t
            .gpuId);
        }
        var i = Oe.gpuData[t.gpuId];
        Oe.scanOperationInProgress = !1, i && (0 === t.type ? (Oe.gpuData[t.gpuId]
          .backgroundOcScannerEnabled = !0, s.trigger(v.PERF_AUTO_TUNING_INFO_UPDATED, t), Ae.info(
            "update ongoing scan progress:", t.percentComplete), i.scanStatus.percentComplete = t
          .percentComplete, 100 === t.percentComplete ? i.scanStatus.isScanInProgress = !1 : i
          .scanStatus.isScanInProgress = !0) : 1 === t.type && (Ae.info("ongoing scan completed"), me(
          t, !0))), Oe.checkLimiters(t.gpuId);
      }

      function ee(e, t) {
        var n = ye(e);
        return n.forEach(function(e) {
          var n = t.find(function(t) {
            return t.type === e.type;
          });
          e.min = n.min, e.max = n.max, e.default = n.default, e.value = n.current, e.backedUpvalue =
            n.current, e.current = n.current, e.isSupported = n.isSupported;
        }), n;
      }

      function te(e, t) {
        var n = {};
        if (t.length > 0 ? n.isSupported = t[0].isVisible && t[0].sliderSetting.isSupported : n
          .isSupported = !1, n.enabled = n.isSupported, n.isSupported) {
          n.autoPower = t[0].autoPower, n.autoPowerApplied = n.autoPower, n.isSwitchSupported = !0;
          var i = 0;
          t.forEach(function(e) {
              e.sliderSetting.current && (i += e.sliderSetting.current);
            }), i = Math.round(i / t.length), n.fanCount = t.length, n.autoPowerSwitch = {}, n
            .autoPowerSwitch.items = ["l10n.perfmonoc.automatic", "l10n.perfmonoc.manual"], n
            .autoPowerSwitch.disable = !1, n.autoPowerSwitch.selectedIndex = n.autoPower ? 0 : 1;
          var o = t[0].sliderSetting;
          i < o.min ? i = o.min : i > o.max && (i = o.max), n.sliderSetting = {
            type: 6,
            category: "fanSpeed",
            needsPersistence: !0,
            enabled: n.isSupported,
            isSupported: n.isSupported,
            name: "l10n.perfmonoc.fanSpeedTarget",
            value: i,
            current: i,
            backedUpvalue: i,
            absolute: i,
            min: o.min,
            max: o.max,
            default: o.default,
            unit: "RPM",
            stepSize: 1,
            onChange: ve(e)
          };
        }
        return n;
      }

      function ne(e, t) {
        var i = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : Xe;
        return i === Ze ? q().then(function(o) {
          return Ue && (Ue = null), o ? ie(e, t, i) : (Ae.error("Container plugin ", i,
            " Failed to load"), n.reject("Error: container plugin failed to load"));
        }) : ie(e, t, i);
      }

      function ie(e, t, n) {
        return c.nativeQuery(n, e, t).then(function(t) {
          var n = JSON.parse(t).payload;
          Ae.info(e, "response:", n);
          var i = "";
          return n.result !== !1 && 0 === n._return_code || (i = i = e + " - " + n._return_code, gt
            .errorType = i, gt.reportedBy = C.plugin, Oe.sendPerformanceToolErrorTelemetry()), n;
        }).catch(function(t) {
          Ae.error(e, "failed:", (0, l.default)(t));
          var n = e + " - " + (0, l.default)(t);
          return gt.errorType = n, gt.reportedBy = C.plugin, Oe.sendPerformanceToolErrorTelemetry(),
            null;
        });
      }

      function oe() {
        if (Oe.persistedData.version != et) {
          if (!Oe.persistedData.version) {
            Ae.info("Upgrading stored data to version 1.0 format");
            var e = Oe.persistedData.consent;
            Oe.persistedData = {
              version: et,
              ocData: {
                consent: e,
                gpuData: []
              }
            };
          }
          le();
        }
      }

      function re() {
        Ae.info("Checking for any persistsed settings");
        var e = n.when(!0);
        Oe.persistedData.ocData.gpuData.forEach(function(t, i) {
          e = e.then(function() {
            Ae.info("Apply persisted settings for GPU Index:", i);
            var e = !1;
            t.ocSliderControls.forEach(function(t) {
              var n = Oe.gpuData[i].ocSliderControls.find(function(e) {
                return e.type === t.type;
              });
              t.current >= n.min && t.current <= n.max && n.current !== t.current && (n
                .current = t.current, e = !0);
            });
            var o = void 0;
            return e ? o = Oe.setManualOCLimits(i) : (Ae.info(
              "No need to update limiter settings"), o = n.when(!0)), o.then(function() {
              if (!Oe.gpuData[i].fanSettings.isSupported) return Ae.info(
                "Fan settings not supported"), n.when(!0);
              var e = !1;
              return t.fanSettings.autoPower !== Oe.gpuData[i].fanSettings.autoPower && (Oe
                  .gpuData[i].fanSettings.autoPower = t.fanSettings.autoPower, e = !0), t
                .fanSettings.sliderSetting.current !== Oe.gpuData[i].fanSettings.sliderSetting
                .current && (Oe.gpuData[i].fanSettings.sliderSetting.current = t.fanSettings
                  .sliderSetting.current, e = !0), e ? Oe.setFanSettings(i) : (Ae.info(
                  "No need to update fan settings"), n.when(!0));
            });
          });
        });
      }

      function ae() {
        var e = i.localStorage.getItem(Je);
        return e ? (Oe.persistedData = JSON.parse(e), oe(), void(Me && re())) : (Ae.info(
          "No stored data found"), Oe.persistedData = {
          version: et,
          ocData: {
            gpuData: []
          }
        }, void le());
      }

      function le() {
        u.setLocalStorage(Je, (0, l.default)(Oe.persistedData));
      }

      function se(e) {
        var t = ue(e);
        Oe.callbackMap.set(t, function(e) {
          Oe.callbackMap.delete(t), Ae.info("call last incomplete scan status for GPU", e);
          var n = {
            uiGpuId: e
          };
          ne("GetLastIncompleteOcScannerResults", n).then(function(e) {
            e.result || Ae.info("GetLastIncompleteOcScannerResults failed");
          });
        });
      }

      function de(e) {
        function t(e, t) {
          return (e & t) === t;
        }
        var n = {
          errorCode: 0,
          errorString: "ScanInternalError"
        };
        return t(e, L.SCAN_CANCELLED) && (n.errorCode = L.SCAN_CANCELLED, n.errorString =
          "ScanCancelled"), t(e, L.INTERNAL_ERROR) && (n.errorCode = L.INTERNAL_ERROR, n.errorString =
            "ScanInternalError"), t(e, L.CANCELLED_USER_REQUESTED) ? (n.errorCode = L
            .CANCELLED_USER_REQUESTED, n.errorString = "cancelledUserRequested") : t(e, L
            .CANCELLED_GPU_BUSY) ? (n.errorCode = L.CANCELLED_GPU_BUSY, n.errorString =
            "cancelledGpuBusy") : t(e, L.CANCELLED_USER_INPUT_DETECTED) ? (n.errorCode = L
            .CANCELLED_USER_INPUT_DETECTED, n.errorString = "cancelledInputDetected") : t(e, L
            .CANCELLED_POWER_SWITCH_TO_DC) ? (n.errorCode = L.CANCELLED_POWER_SWITCH_TO_DC, n
            .errorString = "cancelledSwitchToDC") : t(e, L.CANCELLED_SERVICE_SUSPENDED) ? (n.errorCode = L
            .CANCELLED_SERVICE_SUSPENDED, n.errorString = "cancelledServiceSuspended") : t(e, L
            .CANCELLED_BACKGROUND_SCAN_DISABLED) ? (n.errorCode = L.CANCELLED_BACKGROUND_SCAN_DISABLED, n
            .errorString = "cancelledBackgroundScanDisabled") : t(e, L.CANCELLED_SERVICE_STOPPED) ? (n
            .errorCode = L.CANCELLED_SERVICE_STOPPED, n.errorString = "cancelledServiceStopped") : t(e, L
            .CANCELLED_CONFIGURATION_NOT_SUPPORTED) && (n.errorCode = L
            .CANCELLED_CONFIGURATION_NOT_SUPPORTED, n.errorString = "cancelledConfigNotSuppoted"), n;
      }

      function ce(e) {
        return "lastScanStatus" + e;
      }

      function ue(e) {
        return "lastIncompleteScanStatus" + e;
      }

      function fe(e, t) {
        function n(e, t) {
          return (e & t) === t;
        }
        var i = e.completionStatus;
        t.completionStatus = i;
        var o = n(i, 1),
          r = n(i, 4),
          a = n(i, L.SCAN_CANCELLED),
          l = {
            errorCode: a === !0 ? L.SCAN_CANCELLED : 0,
            errorString: ""
          };
        l = de(i), a = a === !0 || 0 !== l.errorCode, Ae.info("Error code is", l.errorCode);
        var d = n(i, 32);
        if (t.isScanInProgress && !o) {
          var c = void 0;
          r ? c = "ScanCompletedSuccessFully" : a && (c = "ScanCancelled"), c && (gt.errorType = c, gt
            .reportedBy = C.others, Oe.sendPerformanceToolErrorTelemetry());
        }
        if (o ? t.isScanInProgress || (Ae.info("A current scan is found to be ongoing"), t
            .isScanInProgress = !0, t.percentComplete = 0, Oe.scanOperationInProgress = !1) : (t
            .isScanInProgress = !1, Oe.scanStopInProgress = !1), a && (Oe.scanOperationInProgress = !1, Oe
            .scanStopInProgress && (Oe.scanStopInProgress = !1)), 0 !== l.errorCode ? (t.errorCode = l
            .errorCode, t.resultText = "--", t.isLastScanSuccessful = !1, Oe.scanStopInProgress = !1, Ae
            .info("Last scan failed because of error", l.errorCode), gt.errorType = l.errorString, gt
            .reportedBy = C.others, Oe.sendPerformanceToolErrorTelemetry()) : t.errorCode = 0, r && (Ae
            .info("Updating successfull last scan results"), Oe.scanOperationInProgress = !1, Oe
            .scanStopInProgress = !1, t.isLastScanSuccessful = !0, t.completionStatus = e
            .completionStatus, t.completionTime = e.completionTime, t.memoryOcOffsetMhz = e
            .memoryOcOffsetMhz, t.averageGpuClockOffsetMhz = e.averageGpuClockOffsetMhz, t.resultText = e
            .averageGpuClockOffsetMhz + "Mhz", e.averageGpuClockOffsetMhz >= 0 && (t.resultText = "+" + t
              .resultText)), d) {
          Ae.info("No OC results are available on the system");
          var u = ue(e.gpuId);
          void 0 !== Oe.callbackMap.get(u) && Oe.callbackMap.get(u)(e.gpuId), t.resultText = "--", t
            .isLastScanSuccessful = !1;
        }
        s.trigger(v.PERF_OCSCAN_COMPLETION_UPDATE);
      }

      function me(e, t) {
        t = t || !1;
        var n = Oe.gpuData[e.gpuId];
        if (n && (n.backgroundOcScannerEnabled = e.stable, s.trigger(v.PERF_AUTO_TUNING_INFO_UPDATED, e),
            Ae.info("Filling up scan status info"), fe(e, n.scanStatus), t)) {
          ft.gpuName = n.gpu.name;
          var i = n.ocSliderControls.find(function(e) {
            return 3 === e.type;
          });
          ft.voltageLimit = i.current;
          var o = n.ocSliderControls.find(function(e) {
            return 4 === e.type;
          });
          ft.powerLimit = o.current;
          var r = n.ocSliderControls.find(function(e) {
            return 5 === e.type;
          });
          ft.temperatureLimit = r.current, ft.gpuCoreClockOffset = e.averageGpuClockOffsetMhz, ft
            .gpuMemoryClockOffset = e.memoryOcOffsetMhz, Oe.sendLastScanResultsTelemetry();
        }
      }

      function ge() {
        var e = [{
          metricId: "frequency",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.GPUClock"),
          value: 0,
          visible: !0,
          unit: "MHz",
          actualUnit: "MHz"
        }, {
          metricId: "tgpWatts",
          isSidebarMetric: !0,
          name: o("translate")("l10n.perfmonoc.power"),
          value: 0,
          visible: !0,
          unit: "Watts",
          actualUnit: "Watts"
        }, {
          metricId: "temperature",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.GPUTemp"),
          value: 0,
          visible: !0,
          unit: "℃",
          actualUnit: "℃"
        }, {
          metricId: "fansPerfMetrics",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.fanSpeed"),
          value: 0,
          visible: !0,
          unit: "RPM",
          actualUnit: "RPM"
        }, {
          metricId: "voltage",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.GPUVoltage"),
          value: 0,
          visible: !0,
          unit: "Volt",
          actualUnit: "Volt"
        }, {
          metricId: "memoryFrequency",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.memeoryClock"),
          value: 0,
          visible: !0,
          unit: "MHz",
          actualUnit: "MHz"
        }, {
          metricId: "utilization",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.GPUUtilization"),
          value: 0,
          visible: !0,
          unit: "%",
          actualUnit: "%"
        }, {
          metricId: "cpuUtilization",
          isSidebarMetric: !0,
          name: o("translate")("l10n.perfmonoc.CPUUtilization"),
          value: 0,
          visible: !0,
          unit: "%",
          actualUnit: "%"
        }, {
          metricId: "avgFps",
          isSidebarMetric: !0,
          name: o("translate")("l10n.perfmonoc.framesPerSecond"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !0,
          unit: "",
          actualUnit: "FPS"
        }, {
          metricId: "renderingPresentLatency",
          isSidebarMetric: !0,
          name: o("translate")("l10n.perfmonoc.renderPresentLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !0,
          unit: "",
          actualUnit: "ms"
        }, {
          metricId: "fps99",
          isSidebarMetric: !0,
          name: o("translate")("l10n.perfmonoc.fps99"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !0,
          unit: "",
          actualUnit: "FPS"
        }, {
          metricId: "perfLimiter",
          isSidebarMetric: !0,
          metricType: "gpu",
          name: o("translate")("l10n.perfmonoc.perfLimitingReasons"),
          value: "-",
          visible: !1,
          dynamicDisable: !1,
          disableValue: "-",
          unit: "",
          actualUnit: ""
        }, {
          metricId: "renderingLatency",
          isSidebarMetric: !1,
          name: o("translate")("l10n.perfmonoc.renderLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !0,
          dynamicDisable: !1,
          disableValue: 0,
          unit: "",
          actualUnit: "ms"
        }, {
          metricId: "lamMonitoringRect",
          name: o("translate")("l10n.perfmonoc.reflexMonitoringPosition"),
          value: "(0, 0)",
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: o("translate")("l10n.perfmonoc.pixels"),
          actualUnit: o("translate")("l10n.perfmonoc.pixels")
        }, {
          metricId: "mouseLatency",
          name: o("translate")("l10n.perfmonoc.mouseLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.unsupported"),
          actualUnit: "ms"
        }, {
          metricId: "pcDisplayLatency",
          name: o("translate")("l10n.perfmonoc.PCDisplayLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.noFlashDetected"),
          actualUnit: "ms"
        }, {
          metricId: "e2eSystemLatency",
          name: o("translate")("l10n.perfmonoc.systemLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.notApplicable"),
          actualUnit: "ms"
        }, {
          metricId: "avgMouseLatency",
          name: o("translate")("l10n.perfmonoc.average") + " " + o("translate")(
            "l10n.perfmonoc.mouseLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.notApplicable"),
          actualUnit: "ms",
          valueOnResetAvg: "-"
        }, {
          metricId: "avgPCDisplayLatency",
          name: o("translate")("l10n.perfmonoc.average") + " " + o("translate")(
            "l10n.perfmonoc.PCDisplayLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.notApplicable"),
          actualUnit: "ms",
          valueOnResetAvg: "-"
        }, {
          metricId: "avgE2ESystemLatency",
          name: o("translate")("l10n.perfmonoc.average") + " " + o("translate")(
            "l10n.perfmonoc.systemLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.notApplicable"),
          actualUnit: "ms",
          valueOnResetAvg: "-"
        }, {
          metricId: "AvgSWPCLatency",
          name: o("translate")("l10n.perfmonoc.averagePCLatency"),
          value: o("translate")("l10n.perfmonoc.notApplicable"),
          visible: !1,
          dynamicDisable: !0,
          disableValue: void 0,
          unit: "",
          fallbackValue: o("translate")("l10n.perfmonoc.notApplicable"),
          actualUnit: "ms"
        }];
        return e;
      }

      function pe(e, t) {
        return Math.round(e * (t = Math.pow(10, t))) / t;
      }

      function he(e) {
        var t = Oe.perfMetrics[e];
        t || (t = Oe.perfMetrics[e] = ge());
        var n = Oe.perfStats.gpusPerfStats.find(function(t) {
          return t.gpuIndex === e;
        });
        t.forEach(function(e) {
          e.extraClass = "";
          var t = void 0;
          if ("gpu" === e.metricType && n) {
            if (t = n[e.metricId], "fansPerfMetrics" === e.metricId && t && t.length > 0) {
              for (var i = 0, r = 0; r < t.length; r++) i += parseInt(t[r].fanSpeed);
              t = parseInt(i / t.length);
            }
          } else t = Oe.perfStats[e.metricId];
          if (!e.dynamicDisable || (e.visible = t !== e.disableValue, e.visible)) {
            if ("avgFps" === e.metricId) {
              if ("FPS" === Oe.selectedOverlayViewName && void 0 === t) return e.value = "", void(e
                .visible = !1);
              e.visible = !0;
            }
            if (void 0 === t) return e.value = o("translate")("l10n.perfmonoc.notApplicable"), e
              .unit = "", e.isFallbackValue = !0, void(e.isSuffixApplied = !1);
            if (e.unit = e.actualUnit, e.isFallbackValue = !1, "-" === e.valueOnResetAvg) {
              if (t === -2 || 1e3 * t === -2) return e.value = e.valueOnResetAvg, e.unit = "", e
                .isFallbackValue = !1, void(e.isSuffixApplied = !1);
              e.unit = e.actualUnit, e.isFallbackValue = !1;
            }
            if (void 0 !== e.fallbackValue) {
              if (t < 0 || 0 == t && "AvgSWPCLatency" === e.metricId) return e.value = e
                .fallbackValue, e.unit = "", e.isFallbackValue = !0, void(e.isSuffixApplied = !1);
              e.unit = e.actualUnit, e.isFallbackValue = !1;
            }
            "lamMonitoringRect" === e.metricId ? (e.value = t, e.extraClass = e.isFallbackValue ? "" :
              "perf-overlay-valid-value perf-smaller-font") : (e.value = pe(t, 1), e.extraClass = e
              .isFallbackValue ? "" : "perf-overlay-valid-value", e.isSuffixApplied && (e
                .extraClass += " perf-overlay-suffixed-value")), be(e);
          }
        });
      }

      function be(e) {
        angular.isNumber(e.value) && ("avgFps" === e.metricId && (mt.meanFPS = e.value),
          "avgPCDisplayLatency" === e.metricId && (mt.meanPCDisplayLatency = e.value),
          "avgE2ESystemLatency" === e.metricId && (mt.meanSystemLatency = e.value),
          "avgMouseLatency" === e.metricId && (mt.meanMouseLatency = e.value), "renderingLatency" === e
          .metricId && (mt.meanRenderingLatency = e.value), "renderingPresentLatency" === e.metricId &&
          (mt.meanRenderPresentLatency = e.value));
      }

      function xe() {
        Oe.gpuData.forEach(function(e) {
          var t = e.gpu.index;
          he(t);
        });
      }

      function ve(e) {
        return function() {
          Oe.gpuData[e].uiState.isSliderStateDirty = !0;
        };
      }

      function ye(e) {
        var t = [{
          type: 1,
          category: "offset",
          enabled: !1,
          name: "l10n.perfmonoc.GPUClockOffset",
          value: 0,
          current: 0,
          backedUpvalue: 0,
          absolute: 0,
          unit: "MHz",
          min: -1,
          max: 1,
          default: 0,
          stepSize: 1,
          onChange: ve(e)
        }, {
          type: 2,
          enabled: !1,
          category: "offset",
          name: "l10n.perfmonoc.MemoryClockOffset",
          value: 0,
          current: 0,
          backedUpvalue: 0,
          absolute: 0,
          unit: "MHz",
          min: -1,
          max: 1,
          default: 0,
          stepSize: 1,
          onChange: ve(e)
        }, {
          type: 3,
          enabled: !0,
          category: "limit",
          needsPersistence: !0,
          name: "l10n.perfmonoc.voltageMaximum",
          value: 0,
          current: 0,
          absolute: 0,
          unit: "%",
          min: -1,
          max: 1,
          default: 0,
          stepSize: 1,
          onChange: ve(e)
        }, {
          type: 4,
          enabled: !0,
          category: "limit",
          needsPersistence: !0,
          name: "l10n.perfmonoc.powerMaximum",
          value: 0,
          current: 0,
          absolute: 0,
          unit: "%",
          min: -1,
          max: -1,
          default: 50,
          stepSize: 1,
          onChange: ve(e)
        }, {
          type: 5,
          enabled: !0,
          category: "limit",
          needsPersistence: !0,
          name: "l10n.perfmonoc.temperatureTarget",
          value: 0,
          current: 0,
          absolute: 0,
          unit: "℃",
          min: -1,
          max: -1,
          default: 50,
          stepSize: 1,
          onChange: ve(e)
        }];
        return t;
      }

      function we(e) {
        Tt = e, Oe.persistedData.isPerfOverlayVisible = e, le();
      }

      function Se(t) {
        if (Ae.info("toggleOCToolMenu: forceLaunch:", t, "perfOverlayVisible:", Tt), t || !_t) {
          Ct = Tt, we(!1), _t = !0, Ct || Oe.enablePerfTracking(!0);
          var n = {
            lastState: e.current.name,
            lastParams: e.params
          };
          u.openOSC("octoolmenu", n), s.trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED, Tt), Ct && Oe
            .sendOverlaySessionTelemetry(!1, !0);
        } else u.closeOSC();
      }

      function Ee() {
        var e = Oe.overlayViews.filter(function(e) {
            return e.enabled;
          }),
          t = e.length,
          n = e.findIndex(function(e) {
            return e.id === Oe.selectedOverlayViewName;
          });
        n = (n + 1) % t, Oe.selectedOverlayViewName = e[n].id, s.trigger(v
            .PERF_OVERLAY_VISIBILITY_CHANGED, Tt), Ae.info("Performance overlay view changed to", Oe
            .selectedOverlayViewName), Oe.overlayViewUpdated = !0, ut.hotkeyUsed = O.yes, Oe
          .sendOverlaySessionTelemetry(!1, !0), D(function() {
            Oe.sendOverlaySessionTelemetry(!0, !1);
          }, 50);
      }

      function ke(e) {
        if (!_t) {
          var t = e || Tt,
            n = t;
          we(!t), Oe.enablePerfTracking(Tt), void 0 == Oe.persistedData.rectAlignmentStatus && (Oe
              .persistedData.rectAlignmentStatus = !0), s.trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED, Tt),
            Ae.info("togglePerfOverlayVisibility isVisible : ", t), !Oe.getFlashIndicatorStatus() && Oe
            .isRLASupportedDD && Oe.isRLAMonitor && (t ? Oe.setFlashIndicatorVisibility(t) : Oe
              .selectedOverlayViewName === U.LATENCY || Oe.selectedOverlayViewName === U.REFLEX_ANALYZER ?
              Oe.setFlashIndicatorVisibility(t) : Oe.setFlashIndicatorVisibility(!t)), Tt && Oe
            .isRLAMonitor && Le && (Oe.islegacyRFISupported ? (Ae.info(
                "Toggle legacy RFI from in-game settings"), b.show(F.PERFMON_RFI_SUPPORT)) : Oe
              .islegacyRFISupported || Oe.islatestRFISupported || (Ae.info("RFI not supported"), b.show(F
                .PERFMON_RECTALIGNMENT_SUPPORT))), n ? Oe.sendOverlaySessionTelemetry(!1, !0) : Oe
            .sendOverlaySessionTelemetry(!0, !1);
        }
      }

      function _e(e) {
        Ie || Ae.error("powerStatusNotification should not come in case of feature disable");
        var t = JSON.parse(e).payload;
        Ae.info("power status notification:", t), Oe.isDC = !!t.powerStatus;
      }

      function Te() {
        return Ae.info("requesting HTTP chroma url", ct), a({
          method: "GET",
          url: ct,
          timeout: qe,
          headers: {
            "cache-control": "no-cache"
          },
          cache: !1
        });
      }
      var Ce,
        Oe = this,
        Ae = t.getInstance("osc/octoolService"),
        Ie = !1,
        Me = !1,
        Re = !1,
        Pe = E.logPerfData,
        De = void 0,
        Ne = null,
        Le = !1,
        Fe = !1,
        Ue = null,
        ze = !1,
        Ge = 0,
        Ve = {
          DDVersion: 511.79
        };
      Oe.isPerfEnabledbyFileLogging = !1;
      var He = !1,
        Be = !1,
        Ye = [],
        $e = [],
        We = [];
      Oe.islatestRFISupported = !1, Oe.islegacyRFISupported = !1, Oe.isReflexStatsSupported = !1, Oe
        .cachedReflexStatsSupported = !1, Oe.isReflexIntegrated = !1, Oe.overlayViewUpdated = !1;
      var je = void 0,
        Ke = void 0,
        qe = 15e3;
      Oe.enableReflexEnhancements = !0;
      var Xe = "OverClocking",
        Ze = "OverClockingNvc",
        Qe = 500,
        Je = "overclockings",
        et = "1.0",
        tt = 10,
        nt = 455,
        it = 224,
        ot = 256,
        rt = 250,
        at = 2e4,
        lt = 5e3,
        st = 3e4,
        dt = 20,
        ct = "https://static.nvidiagrid.net/titles/nvidiatech";
      Oe.defaultOverlayEnabledState = !0, Oe.defaultOverlayQuadrant = "RightTop", Oe
        .defaultOverlayViewName = "Latency", Oe.selectedOverlayViewName = Oe.defaultOverlayViewName, Oe
        .isPerfOverlayEnabled = !1, Oe.currentAppPID = 0, Oe.isAnyOtherOSDVisible = !1, Oe.isDC = !1, Oe
        .isRLAMonitor = !1, Oe.isRLAMouseSupported = !1, Oe.isRLASupportedDD = !1, Oe
        .flashIndicatorStatus = !1, Oe.rectAlignmentStatus = !0, Oe.islatestRFISupported = !1, Oe
        .islegacyRFISupported = !1, Oe.gpuData = [], Oe.perfMetrics = [], Oe.persistedData = {}, Oe
        .sidebarSessionParams = {
          featureSupported: R.performanceMonitoring,
          appliedStatus: O.no,
          automaticTuningEnabled: O.no,
          disclaimerPromptOptionSelected: P.notPresented,
          isFullscreen: O.no,
          restoredDefaults: O.no,
          gpuChevronUsed: O.no,
          tuningTypeChevronUsed: O.no,
          perfMetricScrollUsed: O.no,
          HUDSettingUsed: O.no,
          gpuCount: 0,
          GPU1: "",
          GPU2: "",
          systemType: M.desktop
        };
      var ut = {
          overlayView: k.off,
          hotkeyUsed: O.no,
          totalMs: 0,
          isSystemRLACapable: O.no,
          isMouseRLACapable: O.no,
          isRLAEnabled: O.no,
          isRFISupported: O.no,
          IsReflexStatsSupported: O.no
        },
        ft = {
          scanType: T.manual,
          gpuName: "",
          gpuCoreClockOffset: 0,
          gpuMemoryClockOffset: 0,
          temperatureLimit: 0,
          powerLimit: 0,
          voltageLimit: 0
        },
        mt = {
          meanFPS: 0,
          meanRenderPresentLatency: 0,
          meanRenderingLatency: 0,
          meanMouseLatency: 0,
          meanPCDisplayLatency: 0,
          meanSystemLatency: 0,
          gpuName: ""
        },
        gt = {
          errorType: "",
          reportedBy: C.plugin,
          gpuName: "",
          osVersion: "",
          installedDDVersion: "",
          systemType: M.desktop
        },
        pt = {
          setSampleSize: 0
        },
        ht = {
          totalMs: 0
        },
        bt = {},
        xt = {
          settingName: "",
          settingValue: ""
        },
        vt = {
          ReflexAnalyzer: "ReflexAnalyzer",
          FlashIndicator: "FlashIndicator",
          Others: "Others"
        };
      Oe.skipExperimentalFlag = function() {
        return Be;
      }, Oe.betaToProdFeatureEnabled = function() {
        return !0;
      }, Oe.init = function() {
        return s.on(S.EXPERIMENTAL_CHANGED, V), G().catch(function(e) {
          Ae.error("error during init:", e);
        });
      }, Oe.getIsFeatureAvailable = function() {
        return Ie;
      }, Oe.setCustAvgOnInit = function() {
        var e = Oe.getCustAvgSampleSize();
        ne("SetMetricSampleSize", {
          sampleSize: e
        }).then(function() {
          Ae.info("Custom average size set to " + e + " on init.");
        });
      }, Oe.setDefaultLoggingPathOnInit = function() {
        var e = Oe.getLoggingPath();
        e || (Ae.info("Logging path not found, switching to default path"), ne("GetDefaultLoggingPath")
          .then(function(e) {
            Oe.persistedData.loggingPath = String(e.path.logpath), le(), Ae.info(
              "Default logging path is - " + Oe.persistedData.loggingPath);
          }));
      };
      var yt = _.throttle(Se, rt),
        wt = _.throttle(ke, rt),
        St = _.throttle(Ee, rt),
        Et = _.throttle($, rt),
        kt = _.throttle(Y, rt);
      Oe.setOCAdminConsent = function(e) {
        var t = {
          adminConsent: e
        };
        return ne("SetOCAdminConsent", t);
      }, Oe.getOCAdminConsent = function() {
        return ne("GetOCAdminConsent");
      }, Oe.getGameIsRunning = function() {
        return Le;
      }, Oe.createPerfMetricsSets = function() {
        Ae.info("createPerfMetricsSets"), Oe.gpuData.forEach(function(e, t) {
          var n = ge();
          Oe.perfMetrics[t] ? (0, r.default)(Oe.perfMetrics[t]).forEach(function(e) {
            Oe.perfMetrics[t][e].name = n[e].name, Oe.perfMetrics[t][e].fallbackValue = n[e]
              .fallbackValue;
          }) : (Ae.info("createPerfMetricsSet, index:", t), Oe.perfMetrics[t] = n);
        });
      }, Oe.isOCSupported = function() {
        var e = !1;
        return Oe.gpuData && 1 == Oe.gpuData.length && (e = Oe.gpuData[0].ocConfig
          .manualOcScannerSupported && Oe.gpuData[0].ocConfig.backgroundOcScannerSupported), e;
      }, Oe.getGPUData = function(e) {
        var t = e.index,
          i = {};
        return i.gpu = e, Oe.getCurrentOCConfig(t).then(function(e) {
          return i.ocConfig = e.ocConfig, i.ocConfig.manualOcScannerSupported && i.ocConfig
            .backgroundOcScannerSupported ? Oe.getManualOCLimits(t) : n.when(void 0);
        }).then(function(e) {
          if (e) {
            var o = e.sliders;
            return i.ocSliderControls = ee(t, o), Oe.getFanSettings(t);
          }
          return n.when(void 0);
        }).then(function(e) {
          if (e) {
            var n = e.fanSettings;
            i.fanSettings = te(t, n);
          }
          return i.uiState = Oe.gpuData[t] && Oe.gpuData[t].uiState || {}, i.uiState
            .tuningTypeSwitch = i.uiState.tuningTypeSwitch || {}, i.uiState.isSliderStateDirty = !1,
            i.scanStatus = Oe.gpuData[t] && Oe.gpuData[t].scanStatus || {}, i;
        });
      }, Oe.fillGPUData = function(e) {
        var t = e.index;
        return Oe.getGPUData(e).then(function(e) {
          return Oe.gpuData[t] = e, Ae.info("gpuData : ", e), e.ocConfig.manualOcScannerSupported &&
            e.ocConfig.backgroundOcScannerSupported ? Oe.getScanStatus(t).then(function() {
              return e;
            }) : void n.when(e);
        });
      }, Oe.getGPUInfoAndFillData = function() {
        return Oe.getGPUInfo().then(function(e) {
          var t = n.when(!0);
          return e.forEach(function(e) {
            t = t.then(function() {
              return Oe.fillGPUData(e);
            });
          }), t;
        });
      }, Oe.updateCustAvgSampleSize = function(e) {
        ne("SetMetricSampleSize", {
          sampleSize: e
        }).then(function() {
          Oe.persistedData.avgSampleSize = e, le(), Ae.info(
            "Storing current custom average sample size to local storage - " + Oe.persistedData
            .avgSampleSize);
        }), pt.setSampleSize = e, Oe.sendPerformanceToolSampleSizeTelemetry();
      }, Oe.getCustAvgSampleSize = function() {
        var e = Oe.persistedData.avgSampleSize ? Oe.persistedData.avgSampleSize : dt;
        return e;
      }, Oe.getLoggingPath = function() {
        var e = Oe.persistedData.loggingPath;
        return e;
      }, Oe.storeLoggingPath = function(e) {
        Oe.persistedData.loggingPath = e, le();
      }, Oe.updatePersistedOCSettings = function() {
        Ae.info("Updating persisted settings"), Oe.gpuData.forEach(function(e, t) {
          Oe.persistedData.ocData.gpuData[t] || (Oe.persistedData.ocData.gpuData[t] = {}), Oe
            .persistedData.ocData.gpuData[t].ocSliderControls = e.ocSliderControls.filter(function(
              e) {
              return e.needsPersistence && e.enabled;
            }), Oe.persistedData.ocData.gpuData[t].fanSettings = e.fanSettings;
        });
      }, Oe.getGPUInfo = function() {
        return ne("GetGpuInfo").then(function(e) {
          if (e && e.gpus) {
            var t = e.gpus;
            return t.length > 1 && t.forEach(function(e) {
              e.name = e.index + 1 + ". " + e.name;
            }), t;
          }
          return [];
        });
      }, Oe.setProcessId = function(e) {
        Oe.currentAppPID = e;
        var t = {
          processId: e
        };
        return ne("SetProcessId", t);
      }, Oe.enablePerfTracking = function(e, t) {
        var i = arguments.length > 2 && void 0 !== arguments[2] && arguments[2];
        if (Ae.info("isPerfEnabled - " + He + " and isPerfEnabledByFileLogging - " + Oe
            .isPerfEnabledbyFileLogging + " and isCalledByFileLogging - " + i), !e) {
          if (He && Oe.isPerfEnabledbyFileLogging) return i ? Oe.isPerfEnabledbyFileLogging = !1 :
            He = !1, !0;
          if (He && i) return Oe.isPerfEnabledbyFileLogging = !1, !0;
          if (Oe.isPerfEnabledbyFileLogging && !i) return He = !1, !0;
          i ? Oe.isPerfEnabledbyFileLogging = !1 : Oe.isPerfEnabled = !1;
        }
        if (e && (i ? Oe.isPerfEnabledbyFileLogging = !0 : He = !0), Ae.info(
            "enablePerfTracking enable:", e, "duration:", t), t = t || Qe, Ne && (Ae.info(
            "Cancel ongoing stop FvSDK service timer"), D.cancel(Ne), Ne = null), !e) return Ae.info(
          "Start FvSDK stop service timer"), Ne = D(function() {
          var e = {
            isEnable: !1,
            intervalMS: t
          };
          return Ae.info("Turning off perf tracking now"), ne("EnablePerfStatsNotification", e);
        }, at), n.when(!1);
        Oe.isFvSDKSessionStartInProgress = !0;
        var o = {
          isEnable: e,
          intervalMS: t
        };
        return ne("EnablePerfStatsNotification", o);
      }, Oe.getManualOCLimits = function(e) {
        var t = {
          uiGpuId: e
        };
        return ne("GetManualOCLimits", t);
      }, Oe.setManualOCLimits = function(e) {
        var t = {};
        t.uiGpuId = e, t.sliders = Oe.gpuData[e].ocSliderControls.filter(function(e) {
          return e.enabled && "offset" !== e.category;
        });
        var n = Oe.betaToProdFeatureEnabled() ? Ze : Xe;
        return ne("SetManualOCLimits", t, n);
      }, Oe.enableBackgroundScan = function(e, t) {
        var n = {
            uiGpuId: e,
            enable: t
          },
          i = Oe.betaToProdFeatureEnabled() ? Ze : Xe;
        return ne("EnableBackgroundOcScan", n, i);
      }, Oe.startManualScan = function(e) {
        var t = "ScanTriggered";
        gt.errorType = t, gt.reportedBy = C.others, Oe.sendPerformanceToolErrorTelemetry();
        var n = {
            uiGpuId: e
          },
          i = Oe.betaToProdFeatureEnabled() ? Ze : Xe;
        return ne("StartOcManualScan", n, i).then(function(t) {
          return t.result || (Ae.error("Failed to start manual scan: ", e), Oe
              .scanOperationInProgress = !1, Oe.gpuData[e].scanStatus.resultText = "--", Oe.gpuData[
                e].scanStatus.errorCode = L.INTERNAL_ERROR, Oe.gpuData[e].scanStatus
              .isLastScanSuccessful = !1, s.trigger(v.PERF_OCSCAN_COMPLETION_UPDATE)), Oe
            .checkLimiters(e), t.result;
        });
      }, Oe.stopManualScan = function(e) {
        if (Ae.info("stopManualScan, gpuIndex:", e), !Oe.gpuData[e].scanStatus.isScanInProgress)
        return Ae.info("Manual scan not running, do nothing"), Oe.scanOperationInProgress = !1, n
          .when(!0);
        Oe.scanStopInProgress = !0;
        var t = {
            uiGpuId: e
          },
          i = Oe.betaToProdFeatureEnabled() ? Ze : Xe;
        return ne("StopOcManualScan", t, i).then(function(t) {
          return t.result || (Ae.error("Stop scan API failed: ", e), Oe.scanOperationInProgress = !
            1, Oe.scanStopInProgress = !1), Oe.checkLimiters(e), t;
        });
      }, Oe.checkLimiters = function(e) {
        if (Oe.gpuData[e] && Oe.gpuData[e].scanStatus) {
          var t = Oe.gpuData[e].ocSliderControls.filter(function(e) {
              return "limit" === e.category;
            }),
            n = Oe.gpuData[e].scanStatus.isScanInProgress === !0;
          t.forEach(function(e) {
            e.enabled = !n;
          });
          var i = Oe.gpuData[e].fanSettings;
          if (!i || !i.isSupported) return;
          if (i.enabled = !n, !i.sliderSetting) return;
          i.sliderSetting.enabled = !n, i.isSwitchSupported = !1, D(function() {
            i.autoPowerSwitch.disable = n, i.autoPowerSwitch.selectedIndex = i.autoPower ? 0 : 1, i
              .isSwitchSupported = i.isSupported;
          }, 0);
        }
      }, Oe.getScanStatus = function(e) {
        var t = ce(e);
        Oe.callbackMap.set(t, function(n) {
          Oe.callbackMap.delete(t), Ae.info("updating last scan status for GPU", e, n), me(n);
        }), se(e);
        var n = {
          uiGpuId: e
        };
        return ne("GetLastOcScanResults", n).then(function(e) {
          e.result || (Ae.info("getScanStatus failed, deleting callback"), Oe.callbackMap.delete(
          t));
        });
      }, Oe.getFanSettings = function(e) {
        var t = {
          uiGpuId: e
        };
        return ne("GetFanSettings", t);
      }, Oe.setFanSettings = function(e) {
        var t = {};
        t.uiGpuId = e;
        for (var n = [], i = Oe.gpuData[e].fanSettings.fanCount, o = 0; o < i; o++) {
          var r = {};
          r.index = o, r.autoPower = Oe.gpuData[e].fanSettings.autoPower, r.sliderSetting = Oe.gpuData[
            e].fanSettings.sliderSetting, r.isVisible = Oe.gpuData[e].fanSettings.isSupported, n.push(
            r);
        }
        t.fanSettings = n;
        var a = Oe.betaToProdFeatureEnabled() ? Ze : Xe;
        return ne("SetFanSettings", t, a);
      }, Oe.restoreOCDefaults = function(e) {
        var t = {
            uiGpuId: e
          },
          n = Oe.betaToProdFeatureEnabled() ? Ze : Xe;
        return ne("RestoreOcDefault", t, n);
      }, Oe.getCurrentOCConfig = function(e) {
        var t = {
          uiGpuId: e
        };
        return ne("GetOCConfig", t);
      }, Oe.overlayViews = [{
        id: "Latency",
        name: "l10n.perfmonoc.latency",
        enabled: !0,
        metricSet: Oe.isRLAMouseSupported ? ["avgFps", "e2eSystemLatency", "AvgSWPCLatency",
          "renderingLatency"
        ] : ["avgFps", "pcDisplayLatency", "AvgSWPCLatency", "renderingLatency"]
      }, {
        id: "FPS",
        name: "l10n.perfmonoc.framesPerSecond",
        enabled: !0,
        metricSet: ["avgFps"]
      }, {
        id: "Basic",
        name: "l10n.perfmonoc.basic",
        enabled: !0,
        metricSet: ["avgFps", "fps99", "renderingLatency", "cpuUtilization", "utilization"]
      }, {
        id: "Advanced",
        name: "l10n.perfmonoc.advanced",
        enabled: !0,
        metricSet: ["avgFps", "fps99", "renderingLatency", "cpuUtilization", "utilization",
          "frequency", "memoryFrequency", "temperature", "fansPerfMetrics", "tgpWatts", "voltage",
          "perfLimiter"
        ]
      }, {
        id: "Reflex Analyzer",
        name: "l10n.perfmonoc.reflexAnalyzer",
        enabled: !0,
        metricSet: ["avgFps", "utilization", "renderingLatency", "lamMonitoringRect", "mouseLatency",
          "avgMouseLatency", "pcDisplayLatency", "avgPCDisplayLatency", "e2eSystemLatency",
          "avgE2ESystemLatency"
        ]
      }], Oe.sidebarActiveGPUIndex = 0;
      var _t = !1,
        Tt = !1,
        Ct = void 0;
      Oe.isPerfOverlayVisible = function() {
        return Tt;
      }, Oe.closeOcToolMenu = function() {
        Ae.info("closeOcToolMenu"), _t = !1, we(Ct), Ct = void 0, Tt || Oe.enablePerfTracking(!1), s
          .trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED, Tt), Me && (Oe.updatePersistedOCSettings(), le()),
          Oe.sendSidebarSessionTelemetry(), Tt && Oe.sendOverlaySessionTelemetry(!0, !1);
      }, Oe.launchOCToolMenu = function() {
        Se(!0);
      }, Oe.updatePerfOverlaySetting = function(e, t) {
        Ae.info("updatePerfOverlaySetting:", e);
        var n = e.enabled;
        Oe.isPerfOverlayEnabled = n, Oe.selectedOverlayViewName = e.view;
        var i = [m.hotKeyMapping.PerfOverlayToggle, m.hotKeyMapping.ResetAverages];
        Oe.enableReflexEnhancements || (i = [m.hotKeyMapping.PerfOverlayToggle]), f.dynamicHotkeyToggle(
          i, n), t && (we(n), Oe.enablePerfTracking(Tt), s.trigger(v.PERF_OVERLAY_VISIBILITY_CHANGED,
          Tt), Oe.sendOverlaySessionTelemetry(Tt, !Tt));
      }, Oe.sendSidebarSessionTelemetry = function() {
        h.getSystemInfo().then(function(e) {
          e && (Oe.sidebarSessionParams.systemType = e.MoboType), Ae.info(
            "sendSidebarSessionTelemetry: ", Oe.sidebarSessionParams), p.push(A
            .OSC_PERFORMANCE_TOOL_SIDEBAR_SESSION, Oe.sidebarSessionParams);
        });
      }, Oe.sendOverlaySessionTelemetry = function(e, t) {
        e && (Oe.selectedOverlayViewName === U.FPS ? ut.overlayView = k.fps : Oe
          .selectedOverlayViewName === U.REFLEX_ANALYZER ? ut.overlayView = k.reflexAnalyzer : ut
          .overlayView = Oe.selectedOverlayViewName, ut.hotkeyUsed = O.no, Ae.info(
            "sendOverlaySessionTelemetry start timer"), p.startTimer(A
            .OSC_PERFORMANCE_TOOL_OVERLAY_SESSION)), t && (ut.isSystemRLACapable = Oe.isRLAMonitor ? O
          .yes : O.no, ut.isMouseRLACapable = Oe.isRLAMouseSupported ? O.yes : O.no, Oe.isRLAMonitor ?
          ut.isRLAEnabled = Oe.rectAlignmentStatus ? O.yes : O.no : ut.isRLAEnabled = O.no, ut
          .isRFISupported = Oe.islegacyRFISupported || Oe.islatestRFISupported ? O.yes : O.no, ut
          .IsReflexStatsSupported = Oe.isReflexStatsSupported ? O.yes : O.no, Ae.info(
            "sendOverlaySessionTelemetry stop timer: ", ut), p.endTimer(A
            .OSC_PERFORMANCE_TOOL_OVERLAY_SESSION, {
              info: ut
            }));
      }, Oe.sendLastScanResultsTelemetry = function() {
        Ae.info("sendLastScanResultsTelemetry: ", ft), p.push(A.OSC_PERFORMANCE_TOOL_LAST_SCAN_RESULTS,
          ft);
      }, Oe.sendLatencyMetricsTelemetry = function() {
        h.getSystemInfo().then(function(e) {
          e && (mt.gpuName = e.GPU[0].LongGPUName), Ae.info("sendLatencyMetricsTelemetry: ", mt), mt
            .meanPCDisplayLatency > 0 && p.push(A.OSC_PERFORMANCE_TOOL_LATENCY_METRICS, mt);
        });
      }, Oe.sendPerformanceToolErrorTelemetry = function() {
        h.getSystemInfo().then(function(e) {
          e && (gt.osVersion = e.OSVersion, gt.installedDDVersion = e.DriverVersion, gt.systemType =
            e.MoboType, gt.gpuName = e.GPU[0].LongGPUName), Ae.info(
            "sendPerformanceToolErrorTelemetry: ", gt), p.push(A.OSC_PERFORMANCE_TOOL_ERROR, gt);
        });
      }, Oe.resetSidebarSessionParams = function() {
        Oe.sidebarSessionParams.featureSupported = R.performanceMonitoring, Oe.sidebarSessionParams
          .appliedStatus = O.no, Oe.sidebarSessionParams.automaticTuningEnabled = O.no, Oe
          .sidebarSessionParams.disclaimerPromptOptionSelected = P.notPresented, Oe.sidebarSessionParams
          .isFullscreen = O.no, Oe.sidebarSessionParams.restoredDefaults = O.no, Oe.sidebarSessionParams
          .gpuChevronUsed = O.no, Oe.sidebarSessionParams.tuningTypeChevronUsed = O.no, Oe
          .sidebarSessionParams.perfMetricScrollUsed = O.no, Oe.sidebarSessionParams.HUDSettingUsed = O
          .no, Oe.sidebarSessionParams.gpuCount = 0, Oe.sidebarSessionParams.GPU1 = "", Oe
          .sidebarSessionParams.GPU2 = "", Oe.sidebarSessionParams.systemType = M.desktop;
      }, Oe.checkForActiveGame = function() {
        return Ae.info("checkForActiveGame"), f.getLastAppID().then(function(e) {
          e && (Oe.currentAppPID = e.processID, e.processID && Oe.setProcessId(Oe.currentAppPID));
        });
      }, Oe.sendPerformanceToolSampleSizeTelemetry = function() {
        Ae.info("sendPerformanceToolSampleSizeTelemetry: ", pt), p.push(A
          .OSC_PERFORMANCE_TOOL_SAMPLE_SIZE, pt);
      }, Oe.sendPerformanceToolResetAverageTelemetry = function() {
        Ae.info("sendPerformanceToolResetAverageTelemetry"), p.push(A
          .OSC_PERFORMANCE_TOOL_RESET_AVERAGE, bt);
      }, Oe.sendPerformanceToolLoggingSessionTelemetry = function(e, t) {
        e && p.startTimer(A.OSC_PERFORMANCE_TOOL_LOGGING_SESSION), t && (Ae.info(
          "sendPerformanceToolLoggingSessionTelemetry"), p.endTimer(A
          .OSC_PERFORMANCE_TOOL_LOGGING_SESSION, {
            info: ht
          }));
      }, Oe.sendPerformanceToolSettingsTelemetry = function(e) {
        xt.settingName = e, e === vt.ReflexAnalyzer ? Oe.isRLAMonitor ? xt.settingValue = Oe
          .rectAlignmentStatus ? I.on : I.off : xt.settingValue = I.off : xt.settingValue = Oe
          .flashIndicatorStatus ? I.on : I.off, Ae.info("sendPerformanceToolSettingsTelemetry: ", xt), p
          .push(A.OSC_PERFORMANCE_TOOL_SETTINGS, xt);
      }, Oe.updateRectAlignStatus = function(e) {
        Oe.persistedData.rectAlignmentStatus = e, Oe.rectAlignmentStatus = e, le(), Ae.info(
          "Storing current rectangle alignment status to local storage - " + Oe.persistedData
          .rectAlignmentStatus), Oe.sendPerformanceToolSettingsTelemetry(vt.ReflexAnalyzer);
      }, Oe.getRectAlignStatus = function() {
        void 0 == Oe.persistedData.rectAlignmentStatus && (Oe.persistedData.rectAlignmentStatus = !0);
        var e = void 0 === Oe.persistedData.rectAlignmentStatus || Oe.persistedData.rectAlignmentStatus;
        return e;
      }, Oe.setRectAlignStatusOnInit = function() {
        var e = Oe.persistedData.rectAlignmentStatus;
        return e;
      }, Oe.updateFlashIndicatorStatus = function(e) {
        Oe.persistedData.flashIndicatorStatus = e, Oe.flashIndicatorStatus = e, le(), Ae.info(
          "Storing current Flash Indicator status to local storage - " + Oe.persistedData
          .flashIndicatorStatus), Oe.sendPerformanceToolSettingsTelemetry(vt.FlashIndicator);
      }, Oe.getFlashIndicatorStatus = function() {
        return Oe.flashIndicatorStatus = Oe.persistedData.flashIndicatorStatus, Oe.flashIndicatorStatus;
      }, Oe.alignMonitoringRect = function() {
        ne("AlignLatencyMonitoringRectangle").then(function(e) {
          Ae.info("alignMonitoringRect :", e);
        });
      }, Oe.checkRLAMonitor = function() {
        var e = n.defer();
        return ne("GetRLAMonitorSupport").then(function(t) {
          Oe.isRLAMonitor = t.supported, Ae.info("GetRLAMonitorSupport - " + t), e.resolve(!0);
        }), e.promise;
      }, Oe.setFlashIndicatorVisibility = function(e) {
        ze = !e, ne("ShowFlashIndicator", {
          isVisible: ze
        }).then(function(e) {
          Ae.info("ShowFlashIndicator - " + e);
        });
      }, Oe.setFlashIndicatorSize = function(e) {
        ne("SetFlashIndicatorSize", {
          size: e
        }).then(function(e) {
          Ae.info("SetFlashIndicatorSize - " + e);
        });
      }, Oe.getSupportedDD = function() {
        h.getSystemInfo().then(function(e) {
          return e && (Ge = parseFloat(e.DriverVersion, 10), Oe.isRLASupportedDD = Ve.DDVersion <=
            Ge), Ae.info("getSupportedDD : ", Oe.isRLASupportedDD), Oe.isRLASupportedDD;
        });
      }, Oe.isRFIIntegrated = function(e, t) {
        Ke = e, Ae.info("currentGameCmsId : ", Ke), Te().then(function(e) {
          Ae.info(" HTTP chroma response ", (0, l.default)(e.data)), e.data && e.data.data && (e
            .data.data.forEach(function(e) {
              e.isReflexFISupported === !0 && (e.isReflexFIFullyAutomatic === !0 ? $e.push(e
                .cms_appId) : e.isReflexFISupported === !0 ? Ye.push(e.cms_appId) : Ae.info(
                "Game do not have RFI integrated"), e.isReflexIntegrated === !0 && We.push(e
                .cms_appId));
            }), Oe.islatestRFISupported = $e.includes(Ke), Ae.info(
              "currentGame islatestRFISupported : ", Oe.islatestRFISupported), Oe
            .islegacyRFISupported = Ye.includes(Ke), Ae.info(
              "currentGame islegacyRFISupported : ", Oe.islegacyRFISupported), Oe
            .isReflexIntegrated = We.includes(Ke), Ae.info("currentGame isReflexIntegrated : ", Oe
              .isReflexIntegrated), (Oe.islatestRFISupported || Oe.islegacyRFISupported) && (je =
              t));
        }, function(e) {
          Ae.info("HTTP chroma request failed", e);
        });
      };
    }
  ]);
  exports.octoolService = u;
}
