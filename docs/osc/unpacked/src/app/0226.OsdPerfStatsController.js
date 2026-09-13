// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 226
// controller OsdPerfStatsController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.OsdPerfStatsController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(4) /* app/4 — shadowPlayService (service) */, require(34) /* app/34 — keyboardService (service) */, require(22) /* app/22 — octoolService (service) */, require(47) /* app/47 — hotkeyService (service) */, require(12) /* app/12 — oscDisplayService (service) */;
  var o = i.ngMainModule.controller("OsdPerfStatsController", ["$log", "$scope", "$interval", "$filter",
    "eventAggregator", "oscDisplayService", "shadowPlayService", "keyboardService", "octoolService",
    "hotkeyService", "osdService", "PERFTOOL_EVENTS", "COMMON_EVENTS", "HOTKEY_EVENTS",
    "SHADOWPLAY_EVENTS", "PERFTOOL_VIEWNAMES",
    function(e, t, n, i, o, r, a, l, s, d, c, u, f, m, g, p) {
      function h() {
        _.firstGpuName = s.gpuData[0] ? s.gpuData[0].gpu.name : "", _.secondGpuName = s.gpuData[1] ? s
          .gpuData[1].gpu.name : "", a.getHotkeyShortcut(a.HotkeyShortcuts.PERFOVERLAYTOGGLE).then(
            function(e) {
              e && e.keys && (_.perfOverlayText = i("translate")(
                "l10n.perfmonoc.performanceOverlayWithHotkey", {
                  arg1: l.shortcutToStr(e.keys)
                }));
            });
      }

      function b(e) {
        var t = [];
        if (_.activeView = s.overlayViews.find(function(t) {
            return t.id === e;
          }), "Latency" === _.activeView.id && (s.isRLAMouseSupported ? s.isReflexStatsSupported ? _
            .activeView.metricSet = ["avgFps", "e2eSystemLatency", "AvgSWPCLatency"] : _.activeView
            .metricSet = ["avgFps", "e2eSystemLatency", "renderingLatency"] : s.isReflexStatsSupported ? _
            .activeView.metricSet = ["avgFps", "pcDisplayLatency", "AvgSWPCLatency"] : _.activeView
            .metricSet = ["avgFps", "pcDisplayLatency", "renderingLatency"]), "Basic" === _.activeView
          .id && (s.isReflexStatsSupported ? _.activeView.metricSet = ["avgFps", "fps99",
            "AvgSWPCLatency", "cpuUtilization", "utilization"
          ] : _.activeView.metricSet = ["avgFps", "fps99", "renderingLatency", "cpuUtilization",
            "utilization"
          ]), "Advanced" === _.activeView.id && (s.isReflexStatsSupported ? _.activeView.metricSet = [
            "avgFps", "fps99", "AvgSWPCLatency", "cpuUtilization", "utilization", "frequency",
            "memoryFrequency", "temperature", "fansPerfMetrics", "tgpWatts", "voltage", "perfLimiter"
          ] : _.activeView.metricSet = ["avgFps", "fps99", "renderingLatency", "cpuUtilization",
            "utilization", "frequency", "memoryFrequency", "temperature", "fansPerfMetrics", "tgpWatts",
            "voltage", "perfLimiter"
          ]), "Reflex Analyzer" === _.activeView.id && (s.isReflexStatsSupported ? _.activeView
            .metricSet = ["avgFps", "utilization", "renderingLatency", "AvgSWPCLatency",
              "lamMonitoringRect", "mouseLatency", "avgMouseLatency", "pcDisplayLatency",
              "avgPCDisplayLatency", "e2eSystemLatency", "avgE2ESystemLatency"
            ] : _.activeView.metricSet = ["avgFps", "utilization", "renderingLatency",
              "lamMonitoringRect", "mouseLatency", "avgMouseLatency", "pcDisplayLatency",
              "avgPCDisplayLatency", "e2eSystemLatency", "avgE2ESystemLatency"
            ]), s.createPerfMetricsSets(), _.innerTitleVisible = !0, "FPS" !== _.activeView.id &&
          "Latency" !== _.activeView.id && "Reflex Analyzer" !== _.activeView.id || (_
            .innerTitleVisible = !1), _.currentView = _.activeView.id, _.activeView && _.activeView
          .enabled !== !1 || (T.info("Falling back to the default view"), _.activeView = s.overlayViews
            .find(function(e) {
              return e.id === s.defaultOverlayViewName;
            })), _.activeView.metricSet.forEach(function(e) {
            var n = void 0,
              i = void 0;
            if (n = s.perfMetrics[0].find(function(t) {
                return t.metricId === e;
              }), _.secondGpuName && (i = s.perfMetrics[1].find(function(t) {
                return t.metricId === e;
              })), n && i) {
              var o = [];
              o[0] = n, o[1] = i, t.push(o);
            } else n && (t.push(n), "Latency" === _.activeView.id && ("pcDisplayLatency" === n
              .metricId && ("No Flash Detected" !== n.value && "N/A" != n.value ? (_
                  .cachedpcDisplayLatency = n.value, _.validpcDisplay = !0) : _.validpcDisplay = !1,
                angular.isNumber(_.cachedpcDisplayLatency) && (_.validpcDisplay = !0)),
              "e2eSystemLatency" === n.metricId && ("N/A" !== n.value ? (_.cachede2eLatency = n
                .value, T.info("Stored cachede2eLatency : ", _.cachede2eLatency), _
                .valide2eLatency = !0) : _.valide2eLatency = !1, angular.isNumber(_
                .cachede2eLatency) && (_.valide2eLatency = !0))));
          }), s.isPerfEnabledbyFileLogging) {
          var n = [];
          _.loggingText = i("translate")("l10n.perfmonoc.perfLogging"), _.loggingOn = "On", _
            .secondGpuName ? (n[0] = [], n[1] = [], n[0].name = _.loggingText, n[0].value = _.loggingOn,
              n[1].value = _.loggingOn, n[0].visible = !0) : (n.name = _.loggingText, n.value = _
              .loggingOn, n.visible = !0), t.push(n);
        }
        return t;
      }

      function x() {
        C && (n.cancel(C), C = void 0);
      }

      function v() {
        var e = s.isPerfOverlayVisible();
        if (_.isFileLoggingEnabled = s.isPerfEnabledbyFileLogging, T.info("update visibility:", e), !s
          .getFlashIndicatorStatus() && s.isRLASupportedDD && s.isRLAMonitor && (e ? s
            .selectedOverlayViewName === p.LATENCY || s.selectedOverlayViewName === p.REFLEX_ANALYZER ? s
            .setFlashIndicatorVisibility(!e) : s.setFlashIndicatorVisibility(e) : s
            .setFlashIndicatorVisibility(!e)), _.isVisible = e, h(), _.isVisible)
          if (T.info("active view:", s.selectedOverlayViewName), _.perfMetrics = b(s
              .selectedOverlayViewName), _.secondGpuName) {
            var t = void 0;
            t = _.perfMetrics.find(function(e) {
              return "mouseLatency" === e[0].metricId;
            }), O = t ? t[0] : void 0, t = _.perfMetrics.find(function(e) {
              return "avgMouseLatency" === e[0].metricId;
            }), A = t ? t[0] : void 0, t = _.perfMetrics.find(function(e) {
              return "avgFps" === e[0].metricId;
            }), t ? _.fpsMetric = t[0] : _.fpsMetric = void 0;
          } else O = _.perfMetrics.find(function(e) {
            return "mouseLatency" === e.metricId;
          }), A = _.perfMetrics.find(function(e) {
            return "avgMouseLatency" === e.metricId;
          }), _.fpsMetric = _.perfMetrics.find(function(e) {
            return "avgFps" === e.metricId;
          }), "N/A" === _.fpsMetric.value && (_.fpsMetric = void 0);
        var n = [d.hotKeyMapping.PerfOverlayCycle];
        a.dynamicHotkeyToggle(n, _.isVisible), y();
      }

      function y() {
        T.info("updateDisplayRects"), !_.isVisible && !s.isPerfEnabledbyFileLogging || s
          .isAnyOtherOSDVisible || s.isReflexStatsSupported ? r.setDisplayRects([]) : (x(), C = n(
            function(e) {
              if (!_.isVisible && !s.isPerfEnabledbyFileLogging) return T.info(
                "Cancelling Interval because perf overlay is not visible"), n.cancel(C), void(C =
                void 0);
              var t = angular.element(document.querySelector(".perf-stat-div")),
                i = 0,
                o = 0,
                a = 0,
                l = 0,
                d = {};
              if (t && t.length && (T.info("Valid perf overlay found."), d = t[0]
                .getBoundingClientRect(), i = Math.round(d.x), o = Math.round(d.y), a = Math.round(d
                  .right) - i, l = Math.round(d.bottom) - o), a > 5 && l > 5) {
                T.info("Found Valid Rect"), T.info("DOM rect is ", d);
                var u = c.overlaySettings.Performance,
                  f = u.position;
                if (T.info("quadrant is ", f), "FPS" === _.currentView && u)
                  if ("LeftTop" === f || "LeftBottom" === f) a *= 3;
                  else if ("RightTop" === f || "RightBottom" === f) {
                  var m = i - 2 * a;
                  m > 0 && (i = m, a *= 3);
                }
                a += 10, l += 10, "LeftBottom" === f ? o -= 10 : "RightTop" === f ? i -= 10 :
                  "RightBottom" === f && (i -= 10, o -= 10);
                var g = [{
                  x: i,
                  y: o,
                  width: a,
                  height: l
                }];
                r.setDisplayRects(g), T.info("Cancelling Interval"), n.cancel(C), C = void 0;
              }
            }, 100));
      }

      function w() {
        y();
      }

      function S() {
        o.on(u.PERF_OVERLAY_VISIBILITY_CHANGED, v), o.on(u.PERF_OVERLAY_LAMSUPPORT_CHANGED, w), o.on(u
          .PERF_OVERLAY_OTHER_OSD_VISIBILITY_CHANGED, v), o.on(f.LOCALE_CHANGED, v), o.on(m
          .PERFOVERLAY_TOGGLE, k), o.on(g.GAME_EXITED, k);
      }

      function E() {
        S(), v(), T.info("Initializing Perf Overlay");
      }

      function k() {
        _.cachedpcDisplayLatency = i("translate")("l10n.perfmonoc.noFlashDetected"), _.cachede2eLatency =
          i("translate")("l10n.perfmonoc.notApplicable"), T.info("Resetting Cached values");
      }
      var _ = this,
        T = e.getInstance("osc/PerfStatsController");
      _.isVisible = !1, _.showHideMessage = void 0, _.firstGpuName = s.gpuData[0] ? s.gpuData[0].gpu
        .name : "", _.secondGpuName = s.gpuData[1] ? s.gpuData[1].gpu.name : "", _.mouseSuffixVisible = !
        1, _.avgMouseSuffixVisible = !1, _.isPcLatencyString = !1, _.currentView = null, _.fpsMetric = {};
      var C,
        O = {},
        A = {};
      _.isFileLoggingEnabled = s.isPerfEnabledbyFileLogging, _.activeView = {}, _.cachedpcDisplayLatency =
        i("translate")("l10n.perfmonoc.noFlashDetected"), _.cachede2eLatency = i("translate")(
          "l10n.perfmonoc.notApplicable"), _.validpcDisplay = !1, _.valide2eLatency = !1, _
        .shouldShowSpinner = function() {
          return s.isFvSDKSessionStartInProgress && "FPS" !== _.currentView;
        }, _.isMouseSuffixVisible = function() {
          var e = O && O.visible && O.isSuffixApplied;
          return e !== _.mouseSuffixVisible && (T.info("Mouse suffix status changed to: ", e), _
            .mouseSuffixVisible = e, y()), e;
        }, _.isAvgMouseSuffixVisible = function() {
          var e = A && A.visible && A.isSuffixApplied;
          return e !== _.avgMouseSuffixVisible && (T.info("Avg Mouse suffix status changed to: ", e), _
            .avgMouseSuffixVisible = e, y()), e;
        }, _.isSuffixVisible = function() {
          return _.isMouseSuffixVisible() || _.isAvgMouseSuffixVisible();
        }, _.mouseSuffixString = function() {
          return O && O.suffix;
        }, _.avgMouseSuffixString = function() {
          return A && A.suffix;
        }, t.$on("$destroy", function() {
          o.off(u.PERF_OVERLAY_VISIBILITY_CHANGED, v), o.off(u.PERF_OVERLAY_LAMSUPPORT_CHANGED, w), o
            .off(u.PERF_OVERLAY_OTHER_OSD_VISIBILITY_CHANGED, v), o.off(f.LOCALE_CHANGED, v), x();
        }), E();
    }
  ]);
  exports.OsdPerfStatsController = o;
}
