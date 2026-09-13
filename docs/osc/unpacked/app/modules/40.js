// ─────────────────────────────────────────────────────────────
// APP MODULE 40
// role       : service nvCameraService
// requires   : (none)
// channels   : /NvCamera/v.1.0/Notifications
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvCameraService = void 0;
  var o = n(8),
    r = i(o),
    a = n(370),
    l = i(a),
    s = n(105),
    d = i(s),
    c = n(64),
    u = i(c),
    f = n(2);
  n(17), n(12), n(91), n(25), n(61), n(23), n(4), n(92);
  var m = f.ngMainCommonModule.service("nvCameraService", ["$q", "$state", "$log", "$timeout", "$window", "$filter",
    "$http", "$interval", "eventAggregator", "gfwslService", "socketService", "oscDisplayService",
    "shadowPlayService", "oscNotificationService", "nvCameraEndpoints", "settingsService", "telemetryService",
    "gamepadService", "hardwareService", "cefService", "oscService", "nisEndpoints", "dvcEndpoints",
    "HOTKEY_EVENTS", "NVCAMERA_EVENTS", "NOTIFIER_SELECTIONS", "COMMON_EVENTS", "SHADOWPLAY_EVENTS", "OSC_CONFIG",
    "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_ANSEL_TYPE", "TELEMETRY_OSC_ANSEL_FAILURE_TYPE",
    "TELEMETRY_OSC_SCREENSHOT_FAILURE_TYPE", "TELEMETRY_OSC_PERF_ID", "TELEMETRY_OSC_BOOLEAN_STATUS",
    "TELEMETRY_OSC_ANSEL_GAMEPAD_TYPE", "TELEMETRY_OSC_TRIGGER_MODE", "NVCAMERA_MODE", "TELEMETRY_OSC_MENU_TYPE",
    "NVCAMERA_STATUS", "TELEMETRY_OSC_AVAILABLE_STATUS", "TELEMETRY_OSC_FILTER_CONTROL_TYPES",
    "TELEMETRY_OSC_SCREEN_STATE", "OSC_KEYBOARD",
    function(e, t, n, i, o, a, s, c, f, m, g, p, h, b, x, v, y, w, S, E, k, T, C, O, A, I, M, R, P, D, N, L, F, U,
      z, G, V, H, B, Y, $, W, j, K) {
      function q(t) {
        var n = !(arguments.length > 1 && void 0 !== arguments[1]) || arguments[1],
          o = arguments.length > 2 && void 0 !== arguments[2] && arguments[2];
        qe.info("setCaptureControl: ", t, n, o);
        var r = e.defer();
        return Ke.callbackMap.set("captureControlChange", function(e) {
          i.cancel(Yt), Yt = void 0, Ke.callbackMap.delete("captureControlChange"), r.resolve(e), qe.info(
            "Capture control successfully set to ", t), Ce()
        }), Wt = void 0, x.setCaptureControl({}, {
          capture: t,
          pauseOnEnable: n,
          leaveFiltersEnabled: o
        }).then(function(e) {
          qe.info("Set Capture control: ", e.status), Yt && i.cancel(Yt), Yt = i(function() {
            r.reject(Ht.NVCAMERA_TIMED_OUT), Ke.callbackMap.delete("captureControlChange"), t && xt && (
            Wt = {}, Wt.time = (new Date).getTime(), Wt.modeType = at), qe.info(
              "Set Capture control to " + t + " failed")
          }, et)
        }), r.promise
      }

      function X() {
        var t;
        if (xt) {
          var n = Ke.isUiRunning ? e.when(Y.ALREADY_ENABLED) : be("mods");
          i.cancel(jt);
          var o = !1;
          n.then(function(t) {
            return t === Y.OK || t === Y.OK_MODSONLY || t === Y.ALREADY_ENABLED ? (t !== Y.ALREADY_ENABLED && (
              o = !0), Ke.captureScreenshot("regular", !1)) : e.reject(!1)
          }).then(function() {
            if (t = a("translate")("l10n.screenshot"), b.show(I.PHOTOGRAPHIC_SCREENSHOT_SAVED_TO_GALLERY, t),
              o) {
              var e = Ce();
              jt = i(function() {
                xe(e)
              }, Qe)
            }
          }).catch(function(e) {
            qe.info("NvCamera session declined for hdrScreenshot ", e), b.show(I.HDR_ERROR_SCREENSHOT)
          })
        } else b.show(I.HDR_ERROR_SCREENSHOT)
      }

      function Z() {
        kt && _t > 0 && (Ke.modsSlots[_t - 1] = se(_t), Ke.lastActiveSlot = -1, Ke.lastSlot = -1, Ke
          .modsIsEnabled = !1, Ke.modsActiveSlot = void 0, qe.info("cleared filter slot ", _t), kt = !1, _t =
          void 0)
      }

      function Q(e, t, n) {
        if ("LaunchFailure" === e && !un) return void qe.info(
          "Not sending failure telemetry as feature was disabled at the time of game launch");
        var i, o = "";
        "LaunchFailure" === e ? (i = L.anselUILaunchFailure, at === H.gameFilter && yt++) : i = L.appNotSupported, x
          .getNvCameraConfigValues().then(function(e) {
            o = "isUILaunchError:" + n + ", isNvCameraOnBus:" + xt + ", isAttemptTooEarly:" + fn +
              ", fatalErrorCode:" + wt + ", IPCenabledRegkey:" + e.data.ipcEnabled + ", FreestyleEnabledRegkey:" +
              e.data.freestyleEnabled + ", IsFirstFreestyleFailure:" + (1 === yt) + ", isSLI:" + sn +
              ", graphicsAPI:" + dn + ", reason:" + t + ", cmsId:" + gt, Me(i, o, at, n)
          })
      }

      function J(e, t, n, i) {
        var o = e;
        if ("%" === i && (o = (e - t) / (n - t) * 100), !isNaN(o)) return o
      }

      function ee() {
        var e = w.getControllerDeviceInfo();
        if (_.isUndefined(e) || _.isNull(e) || "None" === e) return G.None;
        switch (e) {
          case zt.GCType_XBox360:
            return G.XBox360;
          case zt.GCType_PS4:
            return G.SonyPS4;
          case zt.GCType_Shield:
            return G.NvidiaShield;
          default:
            return G.Others
        }
      }

      function te() {
        var e = 416.34;
        return S.getSystemInfo().then(function(t) {
          if (t) return !(parseFloat(t.DriverVersion, 10) < e) || (qe.info("AnselLite min driver failed", t
            .DriverVersion), !1)
        })
      }

      function ne() {
        return v.getExperimentalSetting().then(function(e) {
          return it = (!Ke.hasVariableAvailability() || e) && bt, dt = P.anselLite && it, te().then(function(
          e) {
            return dt = dt && e, Ve().then(function(e) {
              return pt = e, qe.info("isRTXUpResAvailable: ", pt), x.setIPCEnabled({}, {
                enable: it
              }).then(function() {
                f.trigger(A.AVAILABILITY_CHANGED), qe.info("Setting Ansel HKeys status: ", it);
                var e = ["nvcameraui", "modsui", "modstoggle", "modspresetcycle"];
                return h.dynamicHotkeyToggle(e, it).then(function(e) {
                  return Ke.hasVariableAvailability() ? x.setModsEnabled({}, {
                    globalEnable: it
                  }) : Ke.setModsStatusToNvcamera(it && Ke.getModsEnableStatus())
                })
              }).catch(function(e) {
                if (qe.info("NvCamera setConfig failed:", e), it && e.data && e.data.code) {
                  var t = L.generalFailure,
                    n = "ConfigError:" + e.data.code,
                    i = H.gameFilter;
                  Me(t, n, i)
                }
              })
            })
          })
        })
      }

      function ie() {
        qe.info("refreshStateVariables"), Ke.UIRunning = !1, Ke.modsIsEnabled = !1, Ke.modsActiveSlot = void 0,
          Qt = -1, Ke.lastActiveSlot = -1, Ke.lastSlot = -1, Ke.setupFilterStackEvents(!1), cn = !1, gn = !1, en =
          void 0, $t = void 0, Wt = void 0, wt = 0, kt = !1, _t = void 0, Ke.checkReshadeSupport(), Pe()
      }

      function oe() {
        var t = vt + yt;
        if (0 === t || !un) return e.when(!0);
        var n, i = L.generalFailure,
          o = H.gameFilter,
          r = "GameSessionSummary - Verdict:";
        n = 0 === yt ? "NeverFailed" : 0 === vt ? "AlwaysFailed" : "SometimesFailed", r += n, r += ", Attempts:" +
          t + ", Failures:" + yt;
        var a = yt > 0 ? x.getNvCameraConfigValues() : e.when({});
        return a.then(function(e) {
          yt > 0 && e.data && (r += ", isNvCameraOnBus:" + xt + ", fatalErrorCode:" + wt +
            ", IPCenabledRegkey:" + e.data.ipcEnabled + ", FreestyleEnabledRegkey:" + e.data
            .freestyleEnabled + ", isSLI:" + sn + ", graphicsAPI:" + dn), Me(i, r, o)
        })
      }

      function re(e) {
        qe.info("onGameAppExit invoked. UIRunning = " + Ke.getUIRunning() + " ,app proc ID " + e.exitedAppPID),
          ut === e.exitedAppPID && (ut = void 0), ft === e.exitedAppPID && (qe.info(
            "freestyle supported game exited"), ct = !1, ft = void 0, oe().finally(function() {
            ie(), xt = !1, vt = 0, yt = 0
          })), ue(), Ke.reapplyDesktopSharpness(), gt = void 0, Ke.isdeepDVCChromaAllowListed = !1
      }

      function ae(e) {
        qe.info("onGameAppStarted invoked", e), gt = e.startedAppCmsID, ut && ut === ft || (le().then(function(t) {
          qe.info(" HTTP chroma response ", (0, r.default)(t.data)), t.data && t.data.data ? (Tt = [],
          Ct = [], t.data.data.forEach(function(e) {
              e.isFreeStyleSupported === !1 && Tt.push(e.cms_appId), e.isDeepDVCAllowed === !1 && Ct.push(
                e.cms_appId)
            }), ct = !Tt.includes(gt), Ke.isdeepDVCChromaAllowListed = !Ct.includes(gt), ct && (ft = e
              .startedAppPID), Ke.isdeepDVCChromaAllowListed && (mt = e.startedAppPID)) : (ct = !!Tt && !Tt
            .includes(gt), ct && (ft = e.startedAppPID), Ke.isdeepDVCChromaAllowListed = !!Ct && !Ct
            .includes(gt), Ke.isdeepDVCChromaAllowListed && (mt = e.startedAppPID)), gt || (Ke
            .isdeepDVCChromaAllowListed = !1)
        }, function(t) {
          403 === t.status || 404 === t.status ? (ct = !!Tt && !Tt.includes(gt), ct && (ft = e.startedAppPID),
            Ke.isdeepDVCChromaAllowListed = !!Ct && !Ct.includes(gt), Ke.isdeepDVCChromaAllowListed && (mt =
              e.startedAppPID)) : (ct = !1, Ke.isdeepDVCChromaAllowListed = !1), gt || (Ke
            .isdeepDVCChromaAllowListed = !1), qe.info("HTTP chroma request failed", t)
        }), qe.info(" HTTP chroma isFreeStyleChromaWhilteListed ", Ke.isFreeStyleChromaWhilteListed), qe.info(
          " HTTP chroma isdeepDVCChromaAllowListed ", Ke.isdeepDVCChromaAllowListed))
      }

      function le(e) {
        return qe.info("requesting HTTP chroma url", Pt), s({
          method: "GET",
          url: Pt,
          timeout: Je,
          headers: {
            "cache-control": "no-cache"
          },
          cache: !1
        })
      }

      function se(e) {
        return {
          id: e,
          filters: [],
          activeFilterStack: [],
          activeFilter: void 0,
          activeIndex: -1,
          selectedFilter: void 0
        }
      }

      function de() {
        for (var e = [], t = 1; t <= Ke.NUM_MODS_SLOTS + 1; t++) e.push(se(t));
        return e
      }

      function ce() {
        function t() {
          qe.info("loadSlot for game: ", en), Ke.persistSlots[en] && "Desktop" !== en || (qe.info(
              "Filter persistSlots data not present for game"), Ke.persistSlots[en] = de()), Jt[en] && "Desktop" !==
            en || (qe.info("modsData not present for game"), Jt[en] = {
              modsIsEnabled: !1,
              lastSlot: -1
            }), Ke.modsSlots = Ke.persistSlots[en]
        }
        if (!tn) {
          try {
            var n = o.localStorage.getItem(Xt);
            n ? Ke.persistSlots = JSON.parse(n) : qe.info("Filter persistSlots data not present"), n = o
              .localStorage.getItem(Zt), n ? Jt = JSON.parse(n) : qe.info("modsData not present")
          } catch (e) {
            qe.info("getSlotForGame error:", e), ue(!0)
          }
          tn = !0
        }
        en = "Desktop";
        var r = e.defer(),
          a = i(function() {
            qe.info("fallback to desktop"), t(), r.resolve(!1)
          }, et);
        return Ke.callbackMap.set("procInfo", function(e) {
          Ke.callbackMap.delete("procInfo"), i.cancel(a), e && e.procPath && (e.procPath.length > 1 && (en = e
            .procPath), e.procId && (ut = e.procId)), t(), r.resolve(!0)
        }), x.getProcessInfo(), r.promise
      }

      function ue() {
        var e = arguments.length > 0 && void 0 !== arguments[0] && arguments[0];
        if (e || tn) {
          Jt[en] && (Jt[en].lastSlot = Ke.lastSlot, Jt[en].modsIsEnabled = Ke.modsIsEnabled);
          var t = (0, r.default)(Jt);
          p.setLocalStorage(Zt, t), t = (0, r.default)(Ke.persistSlots), p.setLocalStorage(Xt, t)
        }
      }

      function fe(e) {
        if (qe.info("applySetOfFiltersAndAttributes, isSetMultipleFilterAPISupported:", Ot), Ot) Ke
          .setMultipleFiltersAndAttributes(e);
        else
          for (var t = 0; t < e.length; t++) St ? Ke.setFilterAndAttributes(e[t], t) : Ke.setFilterType(e[t].id, t)
      }

      function me(t) {
        Ke.modsSlots.forEach(function(e) {
            var n = e.filters;
            e.filters = [], t.forEach(function(t) {
              var i = _.findWhere(n, {
                id: t.id
              });
              i = i || {}, i.savedConfigs = i.savedConfigs || {}, i.controls = i.controls || [];
              var o = !_.findWhere(e.activeFilterStack, {
                id: t.id
              });
              e.filters.push({
                id: t.id,
                title: t.name,
                savedConfigs: i.savedConfigs,
                visibleInCombo: o,
                controls: i.controls
              })
            });
            var i = [];
            e.activeFilterStack.forEach(function(t) {
                var n = _.findWhere(e.filters, {
                  id: t.id
                });
                n && (n.isWarningMessageDisplayed = t.isWarningMessageDisplayed, i.push(n))
              }), e.activeFilterStack = i, e.selectedFilter = _.findWhere(e.filters, {
                visibleInCombo: !0
              }), e.activeFilterStack.length > 0 ? (e.activeFilter = e.activeFilterStack[0], e.activeIndex = 0) :
              (e.activeFilter = void 0, e.activeIndex = -1)
          }), f.off(A.FILTERS, me), f.trigger(A.MODS_FILTERS_LOADED, t), Qt >= 0 && Qt <= Ke.NUM_MODS_SLOTS - 1 ? Ke
          .applySlot(Ke.modsSlots[Qt]) : Qt != Ke.ANSEL_MOD_SLOT ? Ke.applySlot(void 0) : (nn = e.defer(), Pe()),
          Qt = -1, rn && rn.resolve()
      }

      function ge(e) {
        if (qe.info("Mods Filter Settings"), e.id === tt || !Ke.modsActiveSlot) return void(e.stackIdx == on &&
        Pe());
        var t = Ke.modsActiveSlot.filters,
          n = e.id,
          i = _.findWhere(t, {
            id: n
          });
        return i ? (e.controls.forEach(function(t) {
          function o(e) {
            return e = Math.floor(1e3 * e) / 1e3
          }
          var r = "" + n + "#" + t.id;
          if ("pulldown" === t.type ? t.dataType = "int" : Array.isArray(i.savedConfigs[r]) || (i
              .savedConfigs[r] = void 0), "slider" === t.type)
            for (var a = 0; a < t.currentValue.length; a++) t.currentValue[a] = o(t.currentValue[a]), t
              .minValue[a] = o(t.minValue[a]), t.maxValue[a] = o(t.maxValue[a]), t.defaultValue[a] = o(t
                .defaultValue[a]), t.stepSize[a] = o(t.stepSize[a]), 0 === t.stepSize[a] && (t.stepSize[a] = t
                .maxValue[a] / 100);
          var l = void 0 !== i.savedConfigs[r];
          if (l) Ke.setFilterAttribute(n, e.stackIdx, t.id, i.savedConfigs[r], t.type, t.dataType),
            "pulldown" === t.type ? t.currentId = i.savedConfigs[r][0] : t.currentValue = i.savedConfigs[r];
          else if ("boolean" === t.type) t.currentValue.forEach(function(e, n) {
            t.currentValue[n] = !!e
          }), i.savedConfigs[r] = t.currentValue;
          else if ("slider" === t.type) {
            for (var s = !1, a = 0; a < t.currentValue.length; a++) {
              var d = t.currentValue[a];
              t.currentValue[a] > t.maxValue[a] && (t.currentValue[a] = t.maxValue[a]), t.currentValue[a] < t
                .minValue[a] && (t.currentValue[a] = t.minValue[a]), t.currentValue[a] !== d && (s = !0)
            }
            i.savedConfigs[r] = t.currentValue, s && Ke.setFilterAttribute(n, e.stackIdx, t.id, i
              .savedConfigs[r], t.type, t.dataType)
          } else "pulldown" === t.type ? i.savedConfigs[r] = [t.currentId] : "edit" === t.type && (i
            .savedConfigs[r] = Array.isArray(t.currentValue) ? t.currentValue : [t.currentValue])
        }), f.trigger(A.MODS_FILTER_SETTINGS_READY, e), qe.info(
          "Current filter application response , index : ", e.stackIdx), void(e.stackIdx == on && (Pe(), qe
          .info("Mods filterslot application complete")))) : void qe.info("attributes against unknown filter: ",
          e)
      }

      function pe() {
        (Ke.lastActiveSlot < 0 || Ke.lastActiveSlot >= Ke.NUM_MODS_SLOTS) && (Ke.lastActiveSlot = 0), (Ke.lastSlot <
          0 || Ke.lastSlot >= Ke.NUM_MODS_SLOTS) && (Ke.lastSlot = 0)
      }

      function he(e) {
        return !k.onlineState || k.onlineState.online !== !0 || ct && ut === ft ? !(!k.onlineState || k.onlineState
          .online !== !1 || !Et || e) && (qe.info("Feature not available in offline mode for this app"), !0) : (qe
          .info("Feature not available, app blacklisted in ChromaDB"), !0)
      }

      function be(t, n) {
        var o = e.defer();
        n = xt || n;
        var r = n ? et : Xe;
        return Ke.callbackMap.set("available", function(n) {
          if (i.cancel(Kt), Ke.callbackMap.delete("available"), n) {
            var r = !1;
            if ("sdk" == t)
              if (n.indexOf(t) === -1) {
                if (at = H.anselLite, !dt) return void o.reject(Ht.FEATURE_NOT_ENABLED);
                if (n.indexOf("mods") === -1) return void o.reject(Ht.FEATURE_NOT_AVAILABLE);
                if (he(n.indexOf("allowOffline") !== -1)) return void o.reject(Ht.FEATURE_NOT_AVAILABLE);
                r = !1
              } else at = H.gamePhoto, r = !0;
            else if (n.indexOf(t) === -1 || he(n.indexOf("allowOffline") !== -1)) return void o.reject(Ht
              .FEATURE_NOT_AVAILABLE);
            var a = "sdk" === t ? Te() : e.when(!0);
            a.then(function() {
              return q(!0, r, !1)
            }).then(function(e) {
              Ke.setupFilterStackEvents(!0), o.resolve(e)
            }).catch(function(e) {
              o.reject(e)
            })
          } else o.reject(Ht.FEATURE_NOT_AVAILABLE)
        }), $t = void 0, x.getAvailableFlag().then(function(e) {
          qe.info("getAvailableFlag status = ", e.status), Kt = i(function() {
            o.reject(Ht.NVCAMERA_TIMED_OUT), Ke.callbackMap.delete("available"), xt && ($t = {}, $t
              .modeType = at, $t.time = (new Date).getTime())
          }, r)
        }), o.promise
      }

      function xe(e) {
        return Ke.setupFilterStackEvents(!1), Z(), q(!1, !1, e)
      }

      function ve(t) {
        var n = {};
        return t ? x.getFreeStyleSupportForGame({}, {
          profileName: t
        }).then(function(e) {
          return qe.info("getFreeStyleSupportForGame response : ", e), n.isFreeStyleWhiteListed = e.data
            .freestyleWhitelisted, n.isAnselWhiteListed = e.data.anselWhitelisted, n
        }, function(e) {
          return qe.error("getFreeStyleSupportForGame error : ", e), n
        }) : e.when(n)
      }

      function ye(e, t) {
        function n(e, n) {
          t && b.show(e, n)
        }
        var i, o, r = 4294967295;
        return at === H.anselLite && e === Ht.FEATURE_NOT_ENABLED ? void te().then(function(e) {
          return e ? void(Ke.hasVariableAvailability() || (i = I.WARNING_SUPPORTED_GAME_REQUIRED, n(i))) : (o =
            a("translate")("l10n.openAnsel"), void n(I.FEATURE_UPDATE_DRIVER, o))
        }) : void h.appCaptureProcessInfo(r).then(function(o) {
          return sn = parseInt(m.isSLIDevice()), dn = o.graphicsAPI, qe.info("drsInfo = ", o), sn && 9 === dn ||
            1 === dn ? (n(I.WARNING_SUPPORTED_GAME_REQUIRED), Q("Unsupported", e, t), void qe.info(
              "Unsupported config error [graphicsAPI, isSLI] = ", dn, sn)) : void(e === Ht.SESSION_NOT_ALLOWED ?
              n(I.WARNING_PHOTOGRAPHY_NOT_ALLOWED) : e === Ht.NVCAMERA_ERROR ? (n(I
                .ERROR_NVCAMERA_LAUNCH_FAILED), Q("LaunchFailure", e, t)) : ve(o.drsName).then(function(o) {
                var r = !1;
                r = o.isFreeStyleWhiteListed, r && ct ? (i = I.ERROR_NVCAMERA_LAUNCH_FAILED, n(i), Q(
                  "LaunchFailure", e, t)) : (Ke.getSharpnessSliderSupport() || Ke
                .getVibranceSliderSupport() ? we(!0, !0) : (i = I.WARNING_SUPPORTED_GAME_REQUIRED, n(i)), Q(
                    "Unsupported", e, t))
              }))
        }).catch(function() {
          i = I.WARNING_SUPPORTED_GAME_REQUIRED, n(i), $t = void 0, Wt = void 0
        })
      }

      function we(n, i) {
        function o() {
          return at = H.gameFilter, Ke.modsIsEnabled && !n ? be("mods").then(function() {
            return Ke.enableModsMode(!1)
          }).then(function() {
            return xe(!1)
          }) : (n && (qe.info("launching NvCamera-Mods UI: ", n), y.startPerf(U.anselBringup), Ke
            .sendStartTimerTelemetry(D.OSC_MENU_LAUNCH)), be("mods", i).then(function(i) {
            if (i === Y.OK || i === Y.OK_MODSONLY || i === Y.ALREADY_ENABLED) return ce().then(function() {
              if (Ke.modsSupported = !0, !n) return pe(), Qt = Ke.lastSlot, Ke.enableModsMode(!0).then(
                function() {
                  return xe(!0)
                });
              var e = {
                lastState: t.current.name,
                lastParams: t.params
              };
              p.openOSC("mods", e), Ke.UIRunning = !0
            });
            if (i === Y.PROCESS_DECLINED || i === Y.OK_ANSEL) {
              qe.info("Mods not allowed: ", i), Ke.modsSupported = !1;
              var o = i === Y.OK_ANSEL ? xe(!1) : e.when(!0);
              return o.finally(function() {
                return e.reject(Ht.SESSION_NOT_ALLOWED)
              })
            }
            return Ke.modsSupported = !1, qe.info("Mods error: ", i), e.reject(Ht.NVCAMERA_ERROR)
          }, function(i) {
            if (qe.info("Mods not available: ", i), Ke.modsSupported = !1, !n) return e.reject(i);
            if (!Ke.getSharpnessSliderSupport() && !Ke.getVibranceSliderSupport()) return e.reject(i);
            qe.info(" Launching NIS and DeepDVC Menu ");
            var o = {
              lastState: t.current.name,
              lastParams: t.params
            };
            p.openOSC("mods", o), Ke.UIRunning = !0
          }))
        }
        return n = n || !1, i = i || !1, qe.info("Mods toggle launchUI = ", n, ", useRelaxedTimeout = ", i), Ke
          .isModsOn() ? Ke.hasVariableAvailability() || Ke.modsEnableStatus ? cn ? e.when(!1) : Ke.UIRunning ? (qe
            .info("NvCamera UI already up"), e.when(!1)) : (cn = !0, o().then(function() {
            qe.info("Mods toggle successfull"), cn = !1, vt++, !n && Ke.modsIsEnabled && Ke
              .sendModsAppliedTelemetry()
          }, function(e) {
            cn = !1, ye(e, n), qe.error("Mods toggle failed : ", e)
          })) : (n && ("uiTrigger" === rt ? p.openOSC("main.preferences.mods") : b.show(I.ENABLE_MODS)), qe.info(
            "Mods is disabled by user, returning"), e.when(!1)) : e.when(!1)
      }

      function Se(t) {
        function n(t) {
          var n;
          return be("mods").then(function(i) {
            return i !== Y.OK && i !== Y.OK_MODSONLY ? e.reject(i) : (pe(), n = void 0 === t || null === t ? (Ke
              .lastActiveSlot + 1) % Ke.NUM_MODS_SLOTS : t, Ke.enableModsMode(!0))
          }).then(function() {
            return Ke.applySlot(Ke.modsSlots[n]), nn.promise.then(function() {
              nn = void 0
            })
          }).then(function() {
            return xe(!0)
          })
        }
        return Ke.isModsOn() && (Ke.hasVariableAvailability() || Ke.modsEnableStatus) ? !Ke.modsIsEnabled || Ke
          .UIRunning || cn ? e.when(!1) : (Ke.modsActiveSlot && (Ft.currentSlotID = Ke.modsActiveSlot.id, Ft
              .currentSlotFilters = Ke.getStackedFilterDisplayNames(Ke.modsActiveSlot)), cn = !0, at = H.gameFilter,
            n(t).then(function() {
              cn = !1, vt++, qe.info("Mods slot switch successfull"), Ke.modsActiveSlot && (Ft.newSlotID = Ke
                  .modsActiveSlot.id, Ft.newSlotFilters = Ke.getStackedFilterDisplayNames(Ke.modsActiveSlot), y
                  .push(D.OSC_FREESTYLE_FILTERS_SLOT_CHANGED, Ft)), h.getModsActiveStatus() && Ke
                .sendModsAppliedTelemetry()
            }, function(e) {
              cn = !1, qe.info("Mods slot switch failed : ", e), ye(e, !1)
            })) : e.when(!1)
      }

      function Ee(e) {
        rt = e, we(!0)
      }

      function ke() {
        return x.getProcessInfo()
      }

      function _e(e) {
        wt = 0, $t = void 0, Wt = void 0, Ke.isGfeAnselSupported().then(function(t) {
          if (t !== !0) return void(un = !1);
          if (un = !0, mn && i.cancel(mn), fn = !0, mn = i(function() {
              fn = !1, mn = void 0
            }, 15e3), e && "None" !== e) {
            if (Ke.UIRunning === !0) return void qe.info(
              "NvCamera UI already up. No need to show notification.");
            Ke.callbackMap.set("getAvailableForAnselPoster", function(t) {
              Ke.callbackMap.delete("getAvailableForAnselPoster"), i.cancel(Kt), t && t.indexOf("sdk") !== -
                1 && (ke().then(function(e) {
                  200 !== e.status && qe.error("Ansel App process info could not be fetched.")
                }), b.flipTo(I.ANSEL_READY_APP_STARTED, e))
            }), x.getAvailableFlag().then(function(e) {
              Kt = i(function() {
                Ke.callbackMap.delete("getAvailableForAnselPoster")
              }, Ze)
            })
          }
        })
      }

      function Te() {
        return an = Ke.modsIsEnabled, Ke.modsIsEnabled ? be("mods").then(function() {
          return Ke.enableModsMode(!1).then(function() {
            return xe(!1)
          })
        }) : e.when(!0)
      }

      function Ce() {
        var e = !1;
        return Ke.modsActiveSlot && Ke.modsActiveSlot.activeFilterStack && Ke.modsActiveSlot.activeFilterStack
          .length > 0 && (e = !0), h.setModsActiveStatus(e), e
      }

      function Oe(n) {
        function i() {
          return rt = n, Ke.isOn() ? (y.startPerf(U.anselBringup), Ke.sendStartTimerTelemetry(D.OSC_MENU_LAUNCH), qe
            .info("launching NvCamera UI: "), be("sdk").then(function(n) {
              if (n === Y.OK || n === Y.OK_ANSEL || n === Y.ALREADY_ENABLED || n === Y.OK_MODSONLY && dt &&
                at === H.anselLite) return ce().then(function() {
                var e = {
                  lastState: t.current.name,
                  lastParams: t.params,
                  isAnselLiteMode: at === H.anselLite
                };
                Qt = Ke.ANSEL_MOD_SLOT, p.openOSC("nvcamera", e)
              });
              var i;
              n === Y.PROCESS_DECLINED || n === Y.OK_MODSONLY ? (i = Ht.SESSION_NOT_ALLOWED, qe.info(
                "NvCamera not allowed: ", n)) : (i = Ht.NVCAMERA_ERROR, qe.info("NvCamera error: ", n));
              var o = n === Y.OK_MODSONLY ? xe(!1) : e.when(!0);
              return o.finally(function() {
                return an && (an = void 0, we(!1)), e.reject(Ht.SESSION_NOT_ALLOWED)
              })
            }, function(t) {
              return e.reject(t)
            })) : e.reject(Ht.FEATURE_NOT_ENABLED)
        }
        return cn || gn ? e.when(!1) : Ke.UIRunning ? (qe.info("NvCamera UI already up"), e.when(!1)) : Ke
          .hasVariableAvailability() || Ke.modsEnableStatus ? (gn = !0, i().then(function() {
            qe.info("NvCamera UI launch successfull"), gn = !1
          }, function(e) {
            gn = !1, ye(e, !0), qe.error("NvCamera UI launch failed: ", e)
          })) : ("uiTrigger" === n ? p.openOSC("main.preferences.mods") : b.show(I.ENABLE_ANSEL), qe.info(
            "Ansel is disabled by user, returning"), e.when(!1))
      }

      function Ae(t) {
        return Ke.hasVariableAvailability() || Ke.modsEnableStatus ? void i(function() {
          var n, i = !1;
          et = Je, 1 === t ? (qe.info("DX creation done"), ie(), xt = !0, n = ce().then(function(e) {
            e && en && (Ke.lastSlot = Jt[en].lastSlot, i = Jt[en].modsIsEnabled)
          })) : (Ke.modsIsEnabled && Ke.lastSlot > -1 && (Ke.modsIsEnabled = !1, Ke.modsActiveSlot = void 0,
            i = !0), n = e.when(!0)), n.then(function() {
            return i ? (qe.info("Applying mods filter slot", Ke.lastSlot + 1), void we(!1, !0).finally(
              function() {
                et = Ze, qe.info("Mods automatic filter application complete"), Nt.persistedFilters = $
                  .yes
              })) : void(et = Ze)
          })
        }, Ze) : (1 === t && ie(), void qe.info("Mods is disabled by user, returning"))
      }

      function Ie(e) {
        "started" === e.screenshot ? (Ke.shotCount = e.shotCount, Ke.percentDone = 0, f.trigger(A
            .SCREENSHOT_CAPTURE_STARTED)) : "progress" === e.screenshot ? (Ke.currentShot = e.currentShot, Ke
            .percentDone = Ke.currentShot / Ke.shotCount * 100, f.trigger(A.SCREENSHOT_CAPTURE_INPROGRESS, Ke
              .percentDone)) : "shotFinished" === e.screenshot ? (Ke.percentDone = 100, f.trigger(A
            .SCREENSHOT_CAPTURE_INPROGRESS, Ke.percentDone), i(function() {
            f.trigger(A.SCREENSHOT_CAPTURE_FINISHED), f.trigger(A.SCREENSHOT_CAPTURE_DONE, e.captureType)
          }, 500)) : "finished" === e.screenshot ? f.trigger(A.SCREENSHOT_CAPTURE_FILE_WRITE_DONE, e) : "cancel" ===
          e.screenshot ? void 0 !== Ke.callbackMap.get("cancelScreenshotCapture") ? Ke.callbackMap.get(
            "cancelScreenshotCapture")(e.status) : qe.info("cancel: No callback") : e.status == Y
          .FAILED_TO_SAVE_SHOT_NO_SPACE_LEFT ? f.trigger(A.SCREENSHOT_CAPTURE_FAILED, {
            errorId: Y.FAILED_TO_SAVE_SHOT_NO_SPACE_LEFT,
            data: {
              dirpath: ot,
              diskspaceReq: Math.round(e.spaceInfo / 8388608 * 100) / 100
            }
          }) : e.status == Y.PERMISSION_DENIED && f.trigger(A.SCREENSHOT_CAPTURE_FAILED, {
            errorId: Y.PERMISSION_DENIED,
            data: {
              dirpath: ot
            }
          })
      }

      function Me(e, t, n, i) {
        qe.info("sendAnselErrorTelemetry errorValue:", e, " errorString:", t, ", mode:", n), i = i || Ke.UIRunning,
          S.getSystemInfo().then(function(o) {
            o && y.push(D.OSC_ANSEL_ERROR, {
              errorValue: e,
              errorString: t,
              installedDDVersion: o.DriverVersion,
              systemType: o.MoboType,
              osVersion: o.OSVersion,
              isOptimus: 0 === o.IsOptimus ? $.yes : $.no,
              gpuName: o.GPU[0].LongGPUName,
              cpuName: o.CPUName,
              mode: n,
              usedMenu: i ? $.yes : $.no
            })
          })
      }

      function Re(e) {
        if (_.isUndefined(e) || _.isNull(e) || _.isUndefined(e.type) || _.isNull(e.type)) return void qe.info(
          "processNotifications: undefined or null data, data.type");
        qe.info("processNotifications: ", e);
        var t, n = !1;
        t = "errorResponse" === e.type ? Ke.errorTelemetryMap[e.errorType] : Ke.errorTelemetryMap[e.status];
        var i = "";
        if (t && (t !== L.appFatalError && t !== L.appNonFatalError || (i = "errorCode:" + e.code +
            " errorReason:" + e.errorReason + " filename and line:" + e.fileName + ": " + e.line, t === L
            .appFatalError && (Ke.modsActiveSlot && Ke.modsActiveSlot.id > 0 && (qe.info(
                  "Nvcamera meets fatal error, clearing slot no. ", Ke.modsActiveSlot.id, " on menu close"),
                kt = !0, _t = Ke.modsActiveSlot.id, Pe()), wt = e.code, "invalid" === st ? n = !0 : st =
              "invalid")), n || Me(t, i, at)), "settingsList" === e.type)
          for (var o = 0; o < e.settings.length; o++) "SnapshotsDir" === e.settings[o].name && (ot = e.settings[o]
            .value);
        else if ("highResResolutions" === e.type) f.trigger(A.HIGHRES_RESOLUTIONS, e.resolutions);
        else if ("panoramaResolutionRange" === e.type) f.trigger(A.PANORAMA_RESOLUTION_RANGE, {
          minX: e.minX,
          maxX: e.maxX
        });
        else if ("gameResolution" === e.type) f.trigger(A.SCREENSHOT_RESOLUTION, {
          w: e.width,
          h: e.height
        });
        else if ("filters" === e.type) f.trigger(A.FILTERS, e.filters);
        else if ("screenshot" === e.type) Ie(e);
        else if ("rollRange" === e.type) f.trigger(A.CAMERA_ROLL_RANGE, e.roll);
        else if ("fovRange" === e.type) f.trigger(A.CAMERA_FOV_RANGE, e.fov);
        else if ("fovValue" === e.type) f.trigger(A.CAMERA_FOV_VALUE, e.fov);
        else if ("updateFovValue" === e.type) f.trigger(A.CAMERA_FOV_VALUE, e.currentFovValue);
        else if ("setRoll" === e.type) e.status === Y.OK && f.trigger(A.CAMERA_ROLL_VALUE_SET);
        else if ("setFov" === e.type) e.status === Y.OK && f.trigger(A.CAMERA_FOV_VALUE_SET);
        else if ("available" === e.type) {
          if (void 0 !== Ke.callbackMap.get(e.type)) Ke.callbackMap.get(e.type)(e.features);
          else if (void 0 !== Ke.callbackMap.get("getAvailableForAnselPoster")) Ke.callbackMap.get(
            "getAvailableForAnselPoster")(e.features);
          else if (qe.info("available: No callback"), $t) {
            var r = (new Date).getTime(),
              a = r - $t.time,
              l = L.generalFailure,
              s = "FalseTimeout:GetAvailable, Time:";
            s += a, Me(l, s, $t.modeType)
          }
        } else if ("procInfo" === e.type) void 0 !== Ke.callbackMap.get(e.type) && Ke.callbackMap.get(e.type)(e);
        else if ("currentFilter" === e.type) e.status === Y.OK ? f.trigger(A.CURRENT_FILTER_SETTINGS, e) : e
          .status !== Y.ALREADY_SET || Ke.UIRunning !== !0 && Ke.modsIsEnabled !== !0 || (Ke
            .getCurrentFilterAttribute(e.stackIdx), Ke.modsIsEnabled && f.trigger(A.CURRENT_FILTER_SETTINGS, e));
        else if ("setFilterAttribute" === e.type) qe.info("setFilterAttribute: ", e.status);
        else if ("filterAndAttributes" === e.type) qe.info("setFilterAndAttributes for stackIndex " + e.stackIdx +
          " nvcamera status = " + (e.status === Y.OK ? "Success" : "Failure")), Ke.modsActiveSlot && f.trigger(A
          .MODS_FILTER_SETTINGS_READY, e), e.stackIdx != on || Ot || Pe();
        else if ("captureControlChange" === e.type) {
          if (void 0 !== Ke.callbackMap.get(e.type)) Ke.callbackMap.get(e.type)(e.capture);
          else if (qe.info("captureControlChange: No callback"), Wt) {
            var r = (new Date).getTime(),
              a = r - Wt.time,
              l = L.generalFailure,
              s = "FalseTimeout:captureControlChange, Time:";
            s += a, Me(l, s, Wt.modeType)
          }
        } else if ("captureControl" === e.type) f.trigger(A.CAPTURE_CONTROL_ENABLED, e.capture);
        else if ("captureTypes" === e.type) f.trigger(A.CAPTURE_TYPES, e);
        else if ("language" === e.type) qe.info("language: ", e.status);
        else if ("resetFilter" === e.type) qe.info("resetFilter: ", e.status);
        else if ("addUIElement" === e.type) f.trigger(A.ADD_UI_ELEMENT, e);
        else if ("uiControlRemove" === e.type) f.trigger(A.REMOVE_UI_ELEMENT, e);
        else if ("uiControlSetVisibility" === e.type) f.trigger(A.SET_UI_ELEMENT_VISIBILITY, e);
        else if ("uiControlGetVisibility" === e.type) f.trigger(A.GET_UI_ELEMENT_VISIBILITY, e);
        else if ("uiControlRemoveAllRequest" === e.type) f.trigger(A.REMOVE_ALL_GAME_SETTINGS, e);
        else if ("updateRollValue" === e.type) f.trigger(A.CAMERA_ROLL_VALUE_UPDATE, e);
        else if ("resetEntireStack" === e.type) f.trigger(A.FILTER_STACK_RESETTED, e);
        else if ("nvCameraReady" === e.type) Ae(e.deviceCreationCounter);
        else if ("displayWarningMessage" === e.type) {
          var d, u, m = function(e) {
            e.length > 0 && (b.show(I.WARNING_NVCAMERA_FILTER_DISPLAY_INFO, e[0]), e.length > 1 && c(function(t) {
              b.show(I.WARNING_NVCAMERA_FILTER_DISPLAY_INFO, e[t])
            }, 7e3, e.length - 1))
          };
          if (Ke.modsActiveSlot && (u = _.findIndex(Ke.modsActiveSlot.activeFilterStack, {
              id: e.messageStringsArray[0]
            }), u > -1 && (d = Ke.modsActiveSlot.activeFilterStack[u], e.status === Y.ERROR_FILEPARSING && (Ke
              .modsHelper.activateFilter(Ke.modsActiveSlot, d), Ke.modsHelper.removeActiveItem(Ke
                .modsActiveSlot), f.trigger(A.UPDATE_UI_DATA)))), on === u && Pe(), d && !d
            .isWarningMessageDisplayed) {
            for (qe.info("filter addition warning", e.messageStringsArray), d.isWarningMessageDisplayed = !0, e
              .messageStringsArray.shift(), o = 0; o < e.messageStringsArray.length; o++) e.messageStringsArray[o] =
              e.messageStringsArray[o].replace(d.id, d.title);
            m(e.messageStringsArray)
          }
        } else "multipleSetFilterAndAttributes" === e.type && Pe()
      }

      function Pe() {
        nn && nn.resolve()
      }

      function De(e, t, n, i, o) {
        i.tabIndex = 0, i.id = t.id, i.textPosition = "header-right", i.footerLeft = t.minValue[n], i.footerRight =
          t.maxValue[n], i.range = {
            min: t.minValue[n],
            max: t.maxValue[n]
          }, i.uiRange = {
            min: t.uiMinValueV1 ? t.uiMinValueV1 : [t.uiMinValue],
            max: t.uiMaxValueV1 ? t.uiMaxValueV1 : [t.uiMaxValue]
          }, i.measureUnit = t.measureUnit || "", i.step = t.stepSize[n], i.default = t.defaultValue[n], i.value = t
          .currentValue[n], i.dataType = t.dataType, i.text = Ke.controlValue, i.dimension = n, "int" === t
          .dataType && (i.step = 1), i.onChange = function(n) {
            o && ("Brightness" === n.title ? o.brightness = Math.round(100 * n.value) : "Contrast" === n.title ? o
              .contrast = Math.round(100 * n.value) : "Vibrance" === n.title ? o.vibrance = Math.round(100 * n
                .value) : "Sketch" === n.title ? o.sketch = Math.round(100 * n.value) : "Color enhancer" === n
              .title ? o.colorEnhancer = Math.round(100 * n.value) : "Vignette" === n.title ? o.vignette = Math
              .round(100 * n.value) : o.filterAttributesValues = (100 * n.value).toString());
            var i = Ke.modsHelper.activateFilter(Ke.modsActiveSlot, e),
              r = "" + e.id + "#" + t.id;
            e.savedConfigs[r][n.dimension] = n.value, Ke.setFilterAttribute(e.id, i, n.id, e.savedConfigs[r], t
              .type, t.dataType)
          }
      }

      function Ne(e, t, n, i) {
        i.id = t.id, i.dataType = t.dataType, i.dimension = n, i.value = t.currentValue[n], i.min = t.minValue[n], i
          .max = t.maxValue[n], i.step = .01, i.placeholderText = "", i.onChange = function(n) {
            var i = parseFloat(n.value),
              o = Ke.modsHelper.activateFilter(Ke.modsActiveSlot, e),
              r = "" + e.id + "#" + t.id;
            e.savedConfigs[r][n.dimension] = i, Ke.setFilterAttribute(e.id, o, n.id, e.savedConfigs[r], t.type, t
              .dataType)
          }
      }

      function Le() {
        var e = "/NvCamera/v.1.0/Notifications",
          t = "processNvCameraNotifications";
        qe.info("Registering nvcamera notification events"), g.register(e, t), f.on(t, Re), f.on(M
            .EXPERIMENTAL_CHANGED, ne), f.on(R.GAME_STARTED, ae), f.on(R.GAME_EXITED, re), f.on(A
            .CHECK_AND_INIT_ACTION, _e), f.on(R.HDR_SCREENSHOT, X), f.on(O.NVCAMERAUI, Oe), f.on(O.MODS_TOGGLE, we),
          f.on(O.MODS_CYCLE, Se), f.on(O.MODS_PRESET1, Fe), f.on(O.MODS_PRESET2, Ue), f.on(O.MODS_PRESET3, ze), f
          .on(O.MODS_SHOWUI, Ee), f.on(M.ONLINE, Ge)
      }

      function Fe() {
        return Se(0)
      }

      function Ue() {
        return Se(1)
      }

      function ze() {
        return Se(2)
      }

      function Ge() {
        return bt = !1, it = !1, m.getFeature(lt).then(function(e) {
          var t = !_.isNull(e) && JSON.parse(e.overallState);
          return bt = P.nvCamera && t, ne()
        }, function(e) {
          return qe.error("GFWSL endpoint failed, proceed with OSC initialization", e), bt = P.nvCamera, ne()
        })
      }

      function Ve() {
        var e = !1;
        return S.getSystemInfo().then(function(t) {
          var n = _.findWhere(t.GPU, {
              IsPrimary: "1"
            }) || t.GPU[0],
            i = t.OSBuildNumber,
            o = n.GPUArchitecture,
            r = parseInt(n.GPUArchImplementation, 16),
            a = parseFloat(t.DriverVersion, 10),
            l = Bt.GPUArchitecture <= o,
            s = l && !Bt.ExGPUArchImplementation.includes(r),
            d = Bt.OSBuildNumber <= i,
            c = Bt.DDVersion <= a;
          return e = l && s && d && c
        })
      }

      function He(e) {
        return qe.info("processStateInfo response:", e), At = e.data, e.data
      }

      function Be() {
        if (_.isNull(It)) {
          At = null;
          var t = T.getState({
            cmsId: gt ? gt.toString() : "0"
          });
          It = t.then(He).catch(function(t) {
            return qe.error("failed to get nis state info", t), e.reject(t)
          }).finally(function() {
            It = null
          })
        }
        return It
      }

      function Ye(e) {
        qe.info("sendImageSharpeningTelemetry: ", e), Gt.controlValue = e, Gt.CMSId = gt, E.isInDesktopMode().then(
          function(e) {
            Gt.gameLaunchMode = e === !0 ? j.desktop : j.fullscreen, qe.info(
              "Inside sendImageSharpeningTelemetry: ", Gt), y.push(D.OSC_ANSEL_FILTER_CONTROL_SETTINGS, Gt)
          })
      }

      function $e(e) {
        return qe.info("processdvcStateInfo response:", e), At = e.data, e.data
      }

      function We() {
        if (_.isNull(Rt)) {
          Mt = null, qe.info("fetchDVCStateInfo for cmsId:", gt);
          var t = C.getState({
            cmsId: gt ? gt.toString() : "0",
            processId: mt ? mt : 0
          });
          Rt = t.then($e).catch(function(t) {
            return qe.error("failed to get DeepDVC state info", t), e.reject(t)
          }).finally(function() {
            Rt = null
          })
        }
        return Rt
      }
      var je, Ke = this,
        qe = n.getInstance("osc/nvCameraService"),
        Xe = 250,
        Ze = 2e3,
        Qe = 1e3,
        Je = 15e3,
        et = Ze,
        tt = "None",
        nt = !1,
        it = P.nvCamera,
        ot = "",
        rt = "hotkeyTrigger",
        at = H.gameFilter,
        lt = "getAnselReady",
        st = "invalid",
        dt = !1,
        ct = !1;
      Ke.isdeepDVCChromaAllowListed = !1;
      var ut = void 0,
        ft = void 0,
        mt = void 0,
        gt = void 0,
        pt = !1,
        ht = ", ",
        bt = !1,
        xt = !1,
        vt = 0,
        yt = 0,
        wt = 0,
        St = !1,
        Et = !1,
        kt = !1,
        _t = void 0,
        Tt = void 0,
        Ct = void 0,
        Ot = !1,
        At = null,
        It = null,
        Mt = null,
        Rt = null;
      Ke.deepDVCSystemSupport = !1, Ke.nisSystemSupport = !1, Ke.modsSupported = !1;
      var Pt = "https://static.nvidiagrid.net/titles/nvidiatech",
        Dt = {
          KEYBOARD: z.FALSE,
          MOUSE: z.FALSE,
          GAMEPAD: z.FALSE
        },
        Nt = {
          gameName: "",
          usedSlots: 0,
          slot1Filters: "",
          slot2Filters: "",
          slot3Filters: "",
          activeSlot: 0,
          activeFilters: "",
          persistedFilters: $.no,
          usedMenu: $.no,
          installedDDVersion: 0
        },
        Lt = {},
        Ft = {
          currentSlotID: 0,
          newSlotID: 0,
          currentSlotFilters: "",
          newSlotFilters: ""
        };
      Ke.EFFECTS = {
        ADJUSTMENTS: "Adjustments",
        SPECIALFX: "SpecialFX"
      };
      var Ut = {
          CAMERA: !1,
          FILTER: !1,
          ADJUSTMENTS: !1,
          SPECIALFX: !1,
          SANDBOX: !1,
          GAMEENGINE: !1
        },
        zt = {
          GCType_XBox360: "Xbox 360 Controller (XInput STANDARD GAMEPAD)",
          GCType_PS4: "Wireless Controller (STANDARD GAMEPAD Vendor: 054c Product: 05c4)",
          GCType_Shield: "NVIDIA Controller v01.03 (STANDARD GAMEPAD Vendor: 0955 Product: 7210)"
        },
        Gt = {
          menuName: B.freestyle,
          filterName: "Image Sharpening",
          controlName: "Image Sharpening",
          controlType: W.slider,
          controlValue: 0,
          gameLaunchMode: "",
          CMSId: 0
        },
        Vt = {
          menuName: B.freestyle,
          filterName: "DeepDVC",
          controlName: "Vibrance",
          controlType: W.slider,
          controlValue: 0,
          gameLaunchMode: "",
          CMSId: 0
        };
      Ke.errorTelemetryMap = (je = {}, (0, u.default)(je, Y.FAILED_TO_START, L.failedToStart), (0, u.default)(je, Y
        .NO_SPACE_LEFT, L.noSpaceLeft), (0, u.default)(je, Y.PERMISSION_DENIED, L.permissionDenied), (0, u
        .default)(je, Y.INVALID_REQUEST, L.invalidRequest), (0, u.default)(je, Y.FAILED_TO_PROCESS, L
        .failedToProcess), (0, u.default)(je, Y.PROCESS_DECLINED, L.processDeclined), (0, u.default)(je, Y
        .ALREADY_ENABLED, L.alreadyEnabled), (0, u.default)(je, Y.ALREADY_DISABLED, L.alreadyDisabled), (0, u
        .default)(je, Y.OUT_OF_RANGE, L.outOfRange), (0, u.default)(je, Y.ALREADY_SET, L.alreadySet), (0, u
        .default)(je, Y.INCOMPATIBLE_VERSION, L.incompatibleVersion), (0, u.default)(je, Y.APP_FATAL_ERROR, L
        .appFatalError), (0, u.default)(je, Y.APP_NON_FATAL_ERROR, L.appNonFatalError), (0, u.default)(je, Y
        .NGX_FEATURE_NOT_SUPPORTED, F.ngxFeatureNotSupported), (0, u.default)(je, Y.NGX_OUT_OF_DATE, F
        .ngxOutOfDate), (0, u.default)(je, Y.NGX_OUT_OF_GPU_MEMORY, F.ngxOutOfGpuMemory), (0, u.default)(je, Y
        .SCREENSHOT_TIMEOUT_FAILURE, F.timeoutFailure), (0, u.default)(je, Y.SCREENSHOT_GENERIC_FAILURE, F
        .generalFailure), je);
      var Ht = {
          FEATURE_NOT_ENABLED: "Feature not enabled",
          NVCAMERA_TIMED_OUT: "NvCamera timed out",
          FEATURE_NOT_AVAILABLE: "Feature not available",
          SESSION_NOT_ALLOWED: "NvCamera session not allowed",
          NVCAMERA_ERROR: "Nvcamera error"
        },
        Bt = {
          GPUArchitecture: 352,
          ExGPUArchImplementation: [8, 7],
          OSBuildNumber: 17134,
          DDVersion: 455
        };
      Ke.sendStartTelemetry = function() {
        y.endPerfAfterDigest(U.anselBringup), y.startTimer(D.OSC_ANSEL_NAVIGATION), y.push(D.OSC_ANSEL_TYPE, {
          type: N.full
        }), st = "valid"
      }, Ke.sendEndTelemetry = function(e, t, n) {
        var i = ee();
        e = e ? $.yes : $.no, t = t ? $.yes : $.no, n = n ? $.yes : $.no, i || (i = G.None), qe.info(
          "Input used for navigation ? [keyboard, mouse, gamepad, controllerType]", Dt.KEYBOARD, Dt.MOUSE, Dt
          .GAMEPAD, i);
        var o = Ke.usedNvCameraHide;
        S.getSystemInfo().then(function(r) {
          r && y.endTimer(D.OSC_ANSEL_NAVIGATION, {
            info: {
              usedKeyboard: Dt.KEYBOARD,
              usedMouse: Dt.MOUSE,
              usedController: Dt.GAMEPAD,
              controllerType: i,
              mode: at,
              usedHideMenu: o,
              panningUsed: e,
              panningwithKB: t,
              panningwithMouse: n,
              installedDDVersion: r.DriverVersion
            }
          })
        }), Ke.setNvCameraHideState(!1)
      }, Ke.sendStartTimerTelemetry = function(e) {
        e === D.OSC_MENU_LAUNCH ? y.startTimer(D.OSC_MENU_LAUNCH) : e === D.OSC_ANSEL_SCREENSHOT_CANCELLED && y
          .startTimer(D.OSC_ANSEL_SCREENSHOT_CANCELLED)
      }, Ke.sendEndTimerTelemetry = function(e, t) {
        var n = V.hotkey,
          i = "";
        i = "nvcamera" === t ? B.ansel : "nvcameralite" === t ? B.ansellite : B.freestyle;
        var o = "";
        "uiTrigger" === rt && (n = V.ui), e === D.OSC_ANSEL_SCREENSHOT_CANCELLED && E.isInDesktopMode().then(
          function(t) {
            Lt.gameLaunchMode = t === !0 ? j.desktop : j.fullscreen, e === D.OSC_ANSEL_SCREENSHOT_CANCELLED && y
              .endTimer(D.OSC_ANSEL_SCREENSHOT_CANCELLED, {
                info: Lt
              })
          }), S.getSystemInfo().then(function(t) {
          t && (o = t.DriverVersion), e === D.OSC_MENU_LAUNCH && y.endTimer(D.OSC_MENU_LAUNCH, {
            info: {
              menuName: i,
              triggerMode: n,
              installedDDVersion: o
            }
          })
        }), "nvcamera" === t || "nvcameralite" === t ? h.sendHotkeyTelemetry(h.HotkeyShortcuts.NVCAMERAUI) : (h
          .sendHotkeyTelemetry(h.HotkeyShortcuts.MODSUI), h.sendHotkeyTelemetry(h.HotkeyShortcuts
            .MODSPRESETCYCLE))
      }, Ke.sendScreenshotParams = function(e) {
        Lt = e
      }, Ke.sendScreenshotTelemetry = function(e) {
        E.isInDesktopMode().then(function(t) {
          Lt.gameLaunchMode = t === !0 ? j.desktop : j.fullscreen, e === D.OSC_ANSEL_SCREENSHOT_STARTED && y
            .push(D.OSC_ANSEL_SCREENSHOT_STARTED, Lt)
        })
      }, Ke.setNavigationInputDevice = function(e, t) {
        if (e.length === t.length)
          for (var n = 0; n < e.length; n++) switch (e[n]) {
            case "keyboard":
              Dt.KEYBOARD = t[n] ? z.TRUE : z.FALSE;
              break;
            case "mouse":
              Dt.MOUSE = t[n] ? z.TRUE : z.FALSE;
              break;
            case "gamepad":
              Dt.GAMEPAD = t[n] ? z.TRUE : z.FALSE
          }
      }, Ke.getPaneState = function(e) {
        switch (e) {
          case "Camera":
            return Ut.CAMERA;
          case "Adjustments":
            return Ut.ADJUSTMENTS;
          case "SpecialFX":
            return Ut.SPECIALFX;
          case "SandBox":
            return Ut.SANDBOX;
          case "GameEngine":
            return Ut.GAMEENGINE;
          default:
            return !1
        }
      }, Ke.setPaneState = function(e, t) {
        switch (e) {
          case "Camera":
            Ut.CAMERA = t;
            break;
          case "Adjustments":
            Ut.ADJUSTMENTS = t;
            break;
          case "SpecialFX":
            Ut.SPECIALFX = t;
            break;
          case "SandBox":
            Ut.SANDBOX = t;
            break;
          case "GameEngine":
            Ut.GAMEENGINE = t
        }
      };
      var Yt = void 0,
        $t = void 0,
        Wt = void 0;
      Ke.getSupportedCaptureTypes = function() {
        return x.getCaptureTypes().then(function(e) {
          return qe.info("Supported Capture Type: ", e.data), {
            types: e.data.types
          }
        })
      }, Ke.getSupportedFilterTypes = function() {
        return x.getSupportedFilterTypes().then(function(e) {
          return qe.info("Supported Filter Type response: ", e.status), e.status
        })
      }, Ke.setReadyForGameEngine = function() {
        return x.setReadyForGameEngine().then(function(e) {
          return qe.info("Status of setting game engine readiness: ", e.status), e.status
        })
      }, Ke.getFilterAttributes = function(e) {
        return x.getFilterAttributes({
          id: e
        }).then(function(e) {
          return qe.info("Get Filter Attributes response: ", e.status), e.status
        })
      }, Ke.formatDataForUnifiedIPC = function(e, t) {
        if (!e || !e.controls || !Array.isArray(e.controls)) return qe.error(
          "Fields missing (Filter / Controls). Abort formatting..."), null;
        var n, i, o = {},
          r = {
            "sidebar-slider": 1,
            "sidebar-multislider": 1,
            "sidebar-boolean": 2,
            "sidebar-edit": 5,
            "sidebar-multiedit": 5,
            "sidebar-list": 7
          },
          a = "" + e.id + "#";
        return o.filterId = e.id, o.stackIdx = t, o.controls = [], e.controls.forEach(function(t) {
          n = a + t.id, i = e.savedConfigs[n] || t.value || 0, o.controls.push({
            id: t.id,
            type: r[t.type],
            value: Array.isArray(i) ? i : [i],
            dataType: t.dataType
          })
        }), o
      }, Ke.setMultipleFiltersAndAttributes = function(e) {
        qe.info("setMultipleFiltersAndAttributes");
        var t = [];
        return e.forEach(function(e, n) {
          var i = Ke.formatDataForUnifiedIPC(e, n),
            o = {};
          o.stackIdx = i.stackIdx, o.id = i.filterId, o.controls = i.controls, t.push(o)
        }), x.setMultipleFiltersAndAttributes({}, {
          filters: t
        }).then(function(e) {
          return qe.info("setMultipleFiltersAndAttributes response: ", e.status), e.status
        }, function(e) {
          return qe.error("setMultipleFiltersAndAttributes error: ", e), Pe(), Y.FAILED
        })
      }, Ke.setFilterAndAttributes = function(e, t) {
        qe.info("service.setFilterAndAttributes(). Try to formatDataForUnifiedIPC().");
        var n = Ke.formatDataForUnifiedIPC(e, t);
        return n ? (qe.info("SetFilterAndAttributes request: ", n), x.setFilterAndAttributes({
          id: n.filterId
        }, {
          controls: n.controls,
          stackIdx: n.stackIdx
        }).then(function(e) {
          return qe.info("SetFilterAndAttributes response: ", e.status), e.status
        }, function(e) {
          return qe.error("setFilterAndAttributes error: ", e), on === t && Pe(), Y.FAILED
        })) : (qe.error("Fallback to old API (setFilterType)."), Ke.setFilterType(e.id, t))
      }, Ke.checkFilterAndAttributesAPISupport = function() {
        return x.checkFilterAndAttributesAPISupport().then(function(e) {
          return St = e.data.setFreestyleAndAttributesSupported, qe.info(
            "isFilterAndAttributesAPISupported = " + St), St
        })
      }, Ke.checkReshadeSupport = function() {
        return x.getNvCameraReshadeSupport().then(function(e) {
          Et = e.data.reshadeSupported, qe.info("Reshade support", Et)
        }, function(e) {
          qe.info("checkSetMultipleFilterAPISupport error:", e)
        })
      }, Ke.resetFilterAtIdx = function(e) {
        return Ke.setFilterType(tt, e)
      }, Ke.setFilterType = function(e, t) {
        return x.setFilterType({}, {
          type: e,
          stackIdx: t
        }).then(function(e) {
          return qe.info("Set Filter type response: ", e.status), e.status
        })
      }, Ke.removeFilterAtIndex = function(e) {
        return x.removeFilterAtIndex({}, {
          stackIdx: e
        }).then(function(e) {
          return qe.info("Remove Filter at Index response: ", e.status), e.status
        })
      }, Ke.resetEntireStack = function(e) {
        return x.resetEntireStack().then(function(e) {
          return qe.info("Reset Entire Stack response: ", e.status), e.status
        })
      }, Ke.setFilterAttribute = function(e, t, n, i, o, r) {
        Ke.setAttributeV1(e, t, n, i, o, r).then(function(e) {
          return qe.info("Set Filter Attribute response: ", e.status), e.status
        })
      }, Ke.setGridOfThirdsStatus = function(e) {
        return x.setGridOfThirdsStatus({}, {
          enable: e
        })
      }, Ke.setGameHUDStatus = function(e) {}, Ke.setAttributeV1 = function(e, t, n, i, o, r) {
        qe.info("setAttributeV1: ", e, t, n, i, o, r);
        var a = {
          slider: 1,
          boolean: 2,
          button: 3,
          list: 4,
          edit: 5,
          label: 6,
          pulldown: 7
        };
        return x.setAttributeV1({
          id: e
        }, {
          controlId: n,
          type: a[o],
          value: i,
          stackIdx: t,
          dataType: r
        })
      }, Ke.sendSandboxAttribute = function(e, t, n) {
        var i = {
          slider: 1,
          boolean: 2,
          button: 3,
          list: 4,
          edit: 5,
          label: 6,
          pulldown: 7
        };
        return x.controlChanged({}, {
          controlId: e,
          type: i[t],
          data: n
        })
      }, Ke.captureScreenshot = function(e, t) {
        return x.captureScreenshot({}, {
          type: e,
          saveAsExr: t
        }).then(function(e) {
          return qe.info("Capture Screenshot response: ", e.status), e.status
        })
      };
      var jt = void 0;
      Ke.captureHighResScreenshot = function(e, t, n, i, o, r) {
        return x.captureScreenshot({}, {
          type: e,
          resolutionMultiplier: t,
          width: n,
          height: i,
          saveAsExr: o,
          enhance: r
        }).then(function(e) {
          return qe.info("Capture HighRes Screenshot response: ", e.status), e.status
        })
      }, Ke.capturePanoramaScreenshot = function(e, t, n, i) {
        return x.captureScreenshot({}, {
          type: e,
          panoramaResolutionW: t,
          panoramaResolutionH: n,
          saveAsExr: i
        }).then(function(e) {
          return qe.info("Capture Panorama Screenshot response: ", e.status), e.status
        })
      }, Ke.cancelScreenshotCapture = function() {
        var t = e.defer();
        return Ke.callbackMap.set("cancelScreenshotCapture", function(e) {
          Ke.callbackMap.delete("cancelScreenshotCapture"), t.resolve(e), qe.info(
            "Screenshot capture cancelled successfully, status:", e)
        }), x.cancelScreenshotCapture().then(function(e) {
          qe.info("Cancel Screenshot Capture response: ", e.status)
        }), t.promise
      }, Ke.getCaptureTypeResolutions = function(e) {
        return x.getCaptureTypeResolutions({
          type: e
        }).then(function(e) {
          return qe.info("Capture Type Resolutions response: ", e.status), 200 === e.status
        })
      }, Ke.getCameraAdjustmentsRange = function() {
        return x.getCameraAdjustmentsRange().then(function(e) {
          return qe.info("Camera Adjustments Range response: ", e.status), e.status
        })
      }, Ke.getCameraAdjustments = function() {
        return x.getCameraAdjustments().then(function(e) {
          return qe.info("Get Camera Adjustments response: ", e.status), e.status
        })
      }, Ke.setCameraAdjustments = function(e, t) {
        return qe.info("setCameraAdjustments :" + e + " " + t), x.setCameraAdjustments({}, {
          roll: e,
          fov: t
        }).then(function(e) {
          return qe.info("Set Camera Adjustments: ", e.status), e.status
        })
      }, Ke.getCurrentFilterAttribute = function(e) {
        return qe.info("getCurrentFilterAttribute"), x.getFilterAttributes({}, {
          stackIdx: e
        }).then(function(e) {
          return qe.info("Get current filter attribute: ", e.status), e.status
        })
      }, Ke.resetAllFilters = function() {
        return qe.info("resetAllFilters"), x.resetAllFilters().then(function(e) {
          return qe.info("Reset all filters: ", e.status), e.status
        })
      }, Ke.setLanguage = function(e) {
        return qe.info("setLanguage: ", e), x.setLanguage({}, {
          langId: e
        }).then(function(e) {
          return qe.info("Set language: ", e.status), e.status
        })
      }, Ke.UIRunning = !1, Ke.setUIRunning = function(t, n, i) {
        function o(t, n, i) {
          if (n = n || !1, i = i || !1, t === !1) {
            if (w.gamePadInputPolling("stop"), !n) return ue(), xe(!1)
          } else Ke.setNavigationInputDevice(["keyboard", "mouse", "gamepad"], [!1, !1, !1]), i && w
            .gamePadInputPolling("start");
          return e.when(!0)
        }
        return o(t, n, i).finally(function() {
          Ke.UIRunning = t
        })
      }, Ke.shutdownAnselUI = function() {
        return qe.info("Shutting down NvCamera UI"), Ke.setUIRunning(!1).then(function() {
          if (an) return an = void 0, we(!1)
        })
      }, Ke.getUIRunning = function() {
        return Ke.UIRunning
      }, Ke.callbackMap = new d.default;
      var Kt = void 0;
      Ke.hasVariableAvailability = function() {
        return P.nvCameraExperimental
      }, Ke.isOn = function() {
        return it
      }, Ke.getStackedFilterDisplayNames = function(e) {
        var t = "";
        if (!e || _.isUndefined(e) || _.isNull(e)) return t;
        for (var n = 0; n < e.activeFilterStack.length; n++) {
          var i = e.activeFilterStack[n].id.split("\\"),
            o = i[i.length - 1].split(".");
          o && (t = 0 === n ? o[0] : t + ht + o[0])
        }
        return t
      }, Ke.sendFilterTelemetry = function(e, t, n) {
        if (e && ("nvcamera" === n || "nvcameralite" === n || "mods" === n)) {
          var i = e.split(ht);
          if ("nvcamera" === n || "nvcameralite" === n) {
            var o = "";
            o = "nvcamera" === n ? H.gamePhoto : H.anselLite, i.forEach(function(e) {
              y.push(D.OSC_ANSEL_FILTER_SELECTION, {
                filter: e,
                mode: o
              })
            })
          } else if ("mods" === n) {
            var r = {
              slotID: t,
              filterName: ""
            };
            i.forEach(function(e) {
              r.filterName = e, y.push(D.OSC_FREESTYLE_FILTERS_ADDED, r)
            }), Ke.sendFilterControlTelemetry("mods", Ke.modsActiveSlot)
          } else qe.info("sendFilterTelemetry- Invalid mode")
        }
      }, Ke.sendFilterControlTelemetry = function(e, t) {
        if (t && ("nvcamera" === e || "mods" === e)) {
          var n = t.activeFilterStack;
          if (n && n.length) {
            var i = "nvcamera" === e ? B.ansel : "nvcameralite" === e ? B.ansellite : B.freestyle,
              o = {
                menuName: i,
                filterName: "",
                controlName: "",
                controlType: "",
                controlValue: 0,
                gameLaunchMode: "",
                CMSId: 0
              };
            n.forEach(function(e) {
              if (e && e.controls && e.controls.length) {
                var t = e.id.split("\\"),
                  n = t[t.length - 1].split(".");
                n && (o.filterName = n[0]), o.CMSId = gt, e.controls.forEach(function(e) {
                  o.controlName = e.title;
                  var t = 0;
                  "sidebar-slider" === e.type ? (o.controlType = W.slider, t = J(e.value, e.range.min, e
                      .range.max, e.measureUnit)) : "sidebar-boolean" === e.type ? (o.controlType = W
                      .boolean, t = 1 == e.value ? 1 : 0) : (o.controlType = W.unknown, t = e.value), t &&
                    (o.controlValue = Math.round(100 * (parseFloat(t) + l.default)) / 100), E
                    .isInDesktopMode().then(function(e) {
                      o.gameLaunchMode = e === !0 ? j.desktop : j.fullscreen, y.push(D
                        .OSC_ANSEL_FILTER_CONTROL_SETTINGS, o)
                    })
                })
              }
            })
          }
        }
      }, Ke.getModsSlotInfo = function() {
        for (var e = {
            usedSlots: 0,
            slot1Filters: "",
            slot2Filters: "",
            slot3Filters: "",
            activeSlot: 0,
            activeFilters: ""
          }, t = 0; t < Ke.NUM_MODS_SLOTS; t++) {
          if (!Ke.modsSlots) return;
          if (Ke.modsSlots[t] && Ke.modsSlots[t].activeFilterStack.length > 0) switch (e.usedSlots++, t) {
            case 0:
              e.slot1Filters = Ke.getStackedFilterDisplayNames(Ke.modsSlots[t]);
              break;
            case 1:
              e.slot2Filters = Ke.getStackedFilterDisplayNames(Ke.modsSlots[t]);
              break;
            case 2:
              e.slot3Filters = Ke.getStackedFilterDisplayNames(Ke.modsSlots[t])
          }
        }
        return Ke.modsActiveSlot ? (e.activeSlot = Ke.modsActiveSlot.id, e.activeFilters = Ke
          .getStackedFilterDisplayNames(Ke.modsActiveSlot)) : e.activeSlot = 0, qe.info("modSlotsInfo : ", e), e
      }, Ke.sendModsAppliedTelemetry = function(e) {
        Ke.modsActiveSlot && 0 !== Ke.modsActiveSlot.activeFilterStack.length && S.getSystemInfo().then(function(
          t) {
          var n = e ? "Yes" : "No";
          Nt.gameName = "DDVersion: " + t.DriverVersion + ", usingMenu = " + n;
          var i = Ke.getModsSlotInfo();
          i && (Ke.sendFilterTelemetry(i.slot1Filters, 1, "mods"), Ke.sendFilterTelemetry(i.slot2Filters, 2,
              "mods"), Ke.sendFilterTelemetry(i.slot3Filters, 3, "mods"), Nt.usedSlots = i.usedSlots, Nt
            .slot1Filters = i.slot1Filters, Nt.slot2Filters = i.slot2Filters, Nt.slot3Filters = i
            .slot3Filters, Nt.activeSlot = i.activeSlot, Nt.activeFilters = i.activeFilters, "Yes" === n ?
            Nt.usedMenu = $.yes : Nt.usedMenu = $.no), S.getSystemInfo().then(function(e) {
            e && (Nt.installedDDVersion = e.DriverVersion), 0 !== Nt.activeSlot && (qe.info(
              "modsAppliedTelemetryParam:", Nt), y.push(D.OSC_FREESTYLE_FILTERS_APPLIED, Nt))
          }), Nt.persistedFilters === $.yes && (Nt.persistedFilters = $.no)
        })
      }, Ke.usedNvCameraHide = $.no, Ke.setNvCameraHideState = function(e) {
        e ? Ke.usedNvCameraHide = $.yes : Ke.usedNvCameraHide = $.no
      };
      var qt = "ModsEnableStatus";
      Ke.setModsStatusToNvcamera = function(e) {
        return x.setModsEnabled({}, {
          globalEnable: e
        }).then(function() {
          Ke.modsEnableStatus = e, p.setLocalStorage(qt, e), qe.info("setModsStatusToNvcamera enable  = ", e);
          var t = ["modstoggle", "modspresetcycle"];
          return h.dynamicHotkeyToggle(t, e)
        })
      }, Ke.getModsEnableStatus = function() {
        var e = o.localStorage.getItem(qt);
        return e ? (Ke.modsEnableStatus = JSON.parse(e), qe.info("getModsEnableStatus status = ", Ke
          .modsEnableStatus)) : (qe.info("No mods setting found, Setting true as default"), Ke
          .modsEnableStatus = !0), Ke.modsEnableStatus
      }, Ke.sendUIReady = function() {
        return x.uiReady()
      }, Ke.reportVisibility = function(e) {
        return x.reportVisibility({}, {
          isVisible: e
        })
      }, Ke.NUM_MODS_SLOTS = 3, Ke.ANSEL_MOD_SLOT = Ke.NUM_MODS_SLOTS;
      var Xt = "ModsSlotStorage",
        Zt = "ModsDataStorage";
      Ke.modsActiveSlot = void 0;
      var Qt = -1;
      Ke.lastActiveSlot = -1, Ke.lastSlot = -1;
      var Jt = {},
        en = void 0;
      Ke.modsSlots = de(), Ke.persistSlots = {};
      var tn = !1,
        nn = void 0,
        on = -1;
      Ke.applySlot = function(t) {
        if (nn = e.defer(), on = -1, !Ot && t === Ke.modsActiveSlot) return qe.info(
          "applySlot not needed. slot already applied."), Pe(), t;
        Ke.modsActiveSlot = t;
        var n;
        return n = t && Ot && 0 !== t.activeFilterStack.length ? e.resolve(200) : Ke.resetEntireStack(), n.then(
          function(e) {
            return 200 !== e ? void Pe() : (t ? (t.id - 1 !== Ke.ANSEL_MOD_SLOT && (Ke.lastActiveSlot = t.id -
              1, Ke.lastSlot = t.id - 1), t.activeFilterStack.length > 0 && (on = Math.max(on, t
              .activeFilterStack.length - 1), fe(t.activeFilterStack))) : Ke.lastActiveSlot = -1, void(on <
              0 && Pe()))
          }).catch(function(e) {
          qe.error("Error in service.applySlot(). Reason = " + (0, r.default)(e)), Pe()
        }), t
      }, Ke.modsHelper = {
        addDisabled: function(e) {
          return !(e && e.selectedFilter && e.filters) || !!_.findWhere(e.activeFilterStack, {
            id: e.selectedFilter.id
          })
        },
        activateFilter: function(e, t) {
          return e ? (e.activeFilter = t, e.activeIndex = _.indexOf(e.activeFilterStack, e.activeFilter), e
            .activeIndex) : -1
        },
        addCurrentFilter: function(t) {
          if (!t) return e.when(!1);
          var n = _.findWhere(t.activeFilterStack, {
            id: t.selectedFilter.id
          });
          if (n) return e.when(!1);
          var i = t.activeFilterStack.length,
            o = t.selectedFilter.id;
          return Ke.setFilterType(o, i).then(function(e) {
            qe.info("Current filter response received, slot index : ", i), 0 === i && (t.activeFilter = t
                .selectedFilter), t.selectedFilter.visibleInCombo = !1, t.activeFilterStack.push(t
                .selectedFilter), Ke.modsHelper.activateFilter(t, t.selectedFilter), t.selectedFilter =
              void 0;
            for (var n = _.indexOf(t.filters, t.selectedFilter), o = !1; !o && ++n < t.filters.length;) t
              .filters[n].visibleInCombo && (t.selectedFilter = t.filters[n], o = !0);
            for (; !o && --n >= 0;) t.filters[n].visibleInCombo && (t.selectedFilter = t.filters[n], o = !0)
          })
        },
        tempAddFilter: function(t, n) {
          if (!t) return e.when(!1);
          var i = _.findWhere(t.activeFilterStack, {
            id: n.id
          });
          if (i) return e.when(!1);
          var o = t.activeFilterStack.length,
            r = n.id;
          return Ke.setFilterType(r, o)
        },
        tempRemoveFilter: function(t) {
          if (!t) return e.when(!1);
          var n = t.activeFilterStack.length;
          return Ke.resetFilterAtIdx(n)
        },
        swapFilters: function(e, t, n) {
          var i = e.activeFilterStack[t];
          return e.activeFilterStack[t] = e.activeFilterStack[n], e.activeFilterStack[n] = i, Ot ? Ke
            .setMultipleFiltersAndAttributes(e.activeFilterStack) : St ? Ke.setFilterAndAttributes(e
              .activeFilterStack[t], t).then(function() {
              return Ke.setFilterAndAttributes(e.activeFilterStack[n], n)
            }) : Ke.setFilterType(e.activeFilterStack[t].id, t).then(function() {
              return Ke.setFilterType(e.activeFilterStack[n].id, n)
            })
        },
        moveActiveItemUp: function(t) {
          var n = arguments.length > 1 && void 0 !== arguments[1] && arguments[1];
          if (n) return this.moveActiveItemDown(t);
          if (!t || !t.activeFilter || 0 === t.activeIndex) return e.when(!1);
          var i = t.activeIndex;
          return this.swapFilters(t, i - 1, i).then(function() {
            Ke.modsHelper.activateFilter(t, t.activeFilter)
          })
        },
        moveActiveItemDown: function(t) {
          var n = arguments.length > 1 && void 0 !== arguments[1] && arguments[1];
          if (n) return this.moveActiveItemUp(t);
          if (!t || !t.activeFilter || t.activeIndex === t.activeFilterStack.length - 1) return e.when(!1);
          var i = t.activeIndex;
          return this.swapFilters(t, i, i + 1).then(function() {
            Ke.modsHelper.activateFilter(t, t.activeFilter)
          })
        },
        removeActiveItem: function(t) {
          if (this.trashDisabled(t)) return e.when(!1);
          t.activeFilter.visibleInCombo = !0, t.activeFilter.isWarningMessageDisplayed = !1, t.selectedFilter ||
            (t.selectedFilter = t.activeFilter);
          var n = t.activeIndex;
          return t.activeFilterStack.splice(n, 1), fe(t.activeFilterStack), Ke.resetFilterAtIdx(t
            .activeFilterStack.length), n >= t.activeFilterStack.length && (n = t.activeFilterStack.length -
            1), n >= 0 ? Ke.modsHelper.activateFilter(t, t.activeFilterStack[n]) : (t.activeFilter = void 0, t
            .activeIndex = -1), e.when(!0)
        },
        upDisabled: function(e) {
          var t = arguments.length > 1 && void 0 !== arguments[1] && arguments[1];
          return t ? this.downDisabled(e) : !e || !e.activeFilter || e.activeIndex <= 0
        },
        downDisabled: function(e) {
          var t = arguments.length > 1 && void 0 !== arguments[1] && arguments[1];
          return t ? this.upDisabled(e) : !e || !e.activeFilter || e.activeIndex >= e.activeFilterStack.length -
            1
        },
        trashDisabled: function(e) {
          return !e || !e.activeFilter || 0 === e.activeFilterStack.length
        }
      };
      var rn = void 0;
      Ke.modsIsEnabled = !1;
      var an = void 0,
        ln = !1;
      Ke.setupFilterStackEvents = function(e) {
        e !== ln && (e ? (f.on(A.FILTERS, me), f.on(A.CURRENT_FILTER_SETTINGS, ge)) : (f.off(A.FILTERS, me), f
          .off(A.CURRENT_FILTER_SETTINGS, ge)), ln = e)
      }, Ke.enableModsMode = function(t) {
        function n() {
          Ke.modsIsEnabled = !1
        }
        return t === Ke.modsIsEnabled ? e.when(!0) : t ? v.getLanguage().then(function(e) {
          return Ke.setLanguage(e)
        }).then(function(t) {
          return Ke.modsIsEnabled = !0, rn = e.defer(), Ke.getSupportedFilterTypes().then(function() {
            return rn.promise.then(function() {
              return nn.promise.then(function() {
                return nn = void 0, !0
              })
            })
          })
        }) : (Ke.applySlot(void 0), ue(), nn ? nn.promise.then(function() {
          n(), nn = void 0
        }) : (n(), e.when(!0)))
      };
      var sn = 0,
        dn = 0,
        cn = !1;
      Ke.shutdownModsUI = function(t) {
        qe.info("Shutting down mods UI");
        var n = e.when(!0);
        return t || (n = Ke.enableModsMode(!1)), n.then(function() {
          return xe(t)
        }).finally(function() {
          Ke.setUIRunning(!1, !0)
        })
      };
      var un = !1,
        fn = !1,
        mn = void 0,
        gn = !1;
      Ke.sendAnselScreenshotErrorTelemetry = function(e) {
        qe.info("sendAnselScreenshotErrorTelemetry errorValue:", e), S.getSystemInfo().then(function(t) {
          t && E.isInDesktopMode().then(function(n) {
            Lt.gameLaunchMode = n === !0 ? j.desktop : j.fullscreen, y.push(D
            .OSC_ANSEL_SCREENSHOT_FAILED, {
              mode: Lt.mode,
              errorValue: e,
              errorString: "",
              installedDDVersion: t.DriverVersion,
              systemType: t.MoboType,
              osVersion: t.OSVersion,
              isOptimus: 0 === t.IsOptimus ? $.yes : $.no,
              gpuName: t.GPU[0].LongGPUName,
              cpuName: t.CPUName,
              gameLaunchMode: Lt.gameLaunchMode,
              screenshotType: Lt.screenshotType,
              screenshotResolution: Lt.screenshotResolution,
              superResolutionFactor: Lt.superResolutionFactor,
              hdrMode: Lt.hdrMode,
              panningUsed: Lt.panningUsed,
              method: Lt.method,
              GFEDLISRVersion: Lt.GFEDLISRVersion
            })
          })
        })
      }, Ke.controlValue = function(e) {
        var t = "#" == e.measureUnit ? "" : e.measureUnit,
          n = e.value;
        if (("#" === e.measureUnit || "" === e.measureUnit || "%" === e.measureUnit || "°" === e.measureUnit) && e
          .uiRange && void 0 !== e.uiRange.min && void 0 !== e.uiRange.max) {
          var i = (e.value - e.range.min) / (e.range.max - e.range.min) * (e.uiRange.max[e.dimension] - e.uiRange
            .min[e.dimension]) + e.uiRange.min[e.dimension];
          n = "%" === e.measureUnit || "°" === e.measureUnit ? Math.round(i) : Math.round(1e4 * i) / 1e4
        }
        return n + t
      }, Ke.createFilterControl = function(e, t, n, i) {
        var o = {};
        o.enabled = !0, o.tabIndex = 0;
        var r = (e.defaultValue || e.currentValue || e.defaultId || e.currentId || []).length;
        if ("slider" === e.type) {
          if (o.title = e.displayName, 1 === r) o.type = "sidebar-slider", De(t, e, 0, o, i);
          else if (r > 1) {
            o.type = "sidebar-multislider", o.subcontrols = [], o.id = e.id, o.dataType = e.dataType;
            for (var a = 0; a < r; a++) {
              var l = {};
              De(t, e, a, l, i), l.title = e.controlDisplayName ? e.controlDisplayName[a] : "", o.subcontrols
                .push(l)
            }
          }
        } else if ("boolean" === e.type) e.currentValue = _.map(e.currentValue, function(e) {
            return 1 == e || e === !0
          }), o.id = e.id, o.title = e.displayName, o.type = "sidebar-boolean", o.dimension = 0, o.default = nt, o
          .value = e.currentValue[o.dimension], o.dataType = e.dataType, o.onChange = function(i) {
            var o = Ke.modsHelper.activateFilter(Ke.modsActiveSlot, t);
            t.savedConfigs[n][i.dimension] = i.value, Ke.setFilterAttribute(t.id, o, i.id, t.savedConfigs[n], e
              .type, e.dataType)
          }, o.onKeyDown = function(e) {
            (o.value && e.keyCode === K.LEFT_ARROW || !o.value && e.keyCode === K.RIGHT_ARROW) && (o.value = !o
              .value, o.onChange(o)), Ke.setNavigationInputDevice(["keyboard"], [!0])
          };
        else if ("pulldown" === e.type) {
          o.type = "sidebar-list", o.dimension = 0, o.id = e.id, o.title = e.displayName, o.items = [];
          for (var a = 0; a < e.controlDisplayName.length; a++) {
            var s = {};
            s.id = a, s.title = e.controlDisplayName[a], o.items.push(s)
          }
          o.selectedItem = o.items[e.currentId], o.dataType = "int", o.selectItem = function(i, r) {
            i.selectedItem = r;
            var a = Ke.modsHelper.activateFilter(Ke.modsActiveSlot, t);
            t.savedConfigs[n][i.dimension] = i.selectedItem.id, Ke.setFilterAttribute(t.id, a, i.id, t
              .savedConfigs[n], e.type, o.dataType)
          }
        } else if ("edit" === e.type)
          if (o.title = e.displayName, 1 === r) o.type = "sidebar-edit", Ne(t, e, 0, o);
          else if (r > 1) {
          o.type = "sidebar-multiedit", o.subcontrols = [], o.id = e.id, o.dataType = e.dataType;
          for (var a = 0; a < r; a++) {
            var l = {};
            Ne(t, e, a, l), l.title = e.controlDisplayName ? e.controlDisplayName[a] : "", o.subcontrols.push(l)
          }
        }
        t.controls.push(o)
      }, Ke.isRTXUpResAvailable = function() {
        return pt
      }, Ke.init = function() {
        return qe.info("Initialize NvCameraService"), Ge().then(function(e) {
          Le(), Ke.checkFilterAndAttributesAPISupport(), Ke.checkSetMultipleFilterAPISupport(), Ke
            .checkReshadeSupport(), Ke.checkForActiveGame(), Ke.geNvcameraVersionInfo(), Ke
            .getDeepDVCSystemSupport(), Ke.getNISSystemSupport()
        }).catch(function() {
          qe.info("NvCameraService init failed : ", error)
        })
      }, Ke.launchUIForNvCamera = Oe, Ke.launchUIForMods = Ee, Ke.isGfeAnselSupported = function() {
        return e.when(Ke.isOn())
      }, Ke.isModsOn = function() {
        return Ke.isOn() && P.mods
      }, Ke.checkForActiveGame = function() {
        qe.info("checkForActiveGame"), h.getLastAppID().then(function(e) {
          if (e.processID) {
            var t = {};
            t.startedAppCmsID = e.cmsID, t.startedAppPID = e.processID, ae(t)
          }
        }, function(e) {
          qe.info("checkForActiveGame error: ", e)
        })
      }, Ke.checkSetMultipleFilterAPISupport = function() {
        return x.getMultipleFilterAPISupport().then(function(e) {
          Ot = e.data.isSetMultipleFilterAPISupported, qe.info("Multiple set filter API support: ", Ot)
        }, function(e) {
          qe.info("checkSetMultipleFilterAPISupport error: ", e)
        })
      }, Ke.geNvcameraVersionInfo = function() {
        return x.getCompatibilityInfo().then(function(e) {
          qe.info("Nvcamera version info: ", e.data)
        }, function(e) {
          qe.info("geNvcameraVersionInfo error: ", e)
        })
      }, Ke.isSetMultipleFilterAPISupported = function() {
        return Ot
      }, Ke.getNISStateInfo = function() {
        return Be()
      }, Ke.getNISSystemSupport = function() {
        if (_.isNull(It)) {
          var t = T.getState({
            cmsId: "0"
          });
          It = t.then(function(e) {
            qe.info("NIS system support response: ", e), Ke.nisSystemSupport = e.data.supported
          }).catch(function(t) {
            return qe.error("failed to get NIS state info", t), Ke.nisSystemSupport = !1, e.reject(t)
          }).finally(function() {
            It = null
          })
        }
        return It
      }, Ke.setNISStateInfo = function(t) {
        return T.setState({}, {
          sharpen: t.sharpen,
          cmsId: gt ? gt.toString() : "0"
        }).then(function(e) {
          return e
        }).catch(function(t) {
          return qe.error("failed nisSettState info", t), e.reject(t)
        })
      }, Ke.checkDesktopMode = function() {
        return E.isInDesktopMode().then(function(e) {
          return qe.info("isInDesktopMode", e), e
        })
      }, Ke.sharpnessValuesForGame = function(e) {
        return qe.info("setSharpnessValuesForGame: " + e), x.setSharpnessValuesForGame({}, {
          sharpness: e
        }).then(function(e) {
          qe.info("setSharpnessValuesForGame response: ", e.data.setSharpness)
        }, function(e) {
          qe.error("setSharpnessValuesForGame error: ", e)
        })
      }, Ke.sharpnessChanged = function(e, t) {
        return e.sharpen && t && Ye(e.sharpen), Ke.setNISStateInfo(e).then(function(e) {
          qe.info("NIS setState info", e)
        }).catch(function(e) {
          qe.error("setNISState failed with error:", e)
        }).finally(function() {
          qe.error("onSetNISStateFinish finally")
        })
      }, Ke.reapplyDesktopSharpness = function() {
        if (_.isNull(It)) {
          At = null;
          var t = T.getState({
            cmsId: "0"
          });
          It = t.then(function(e) {
            qe.info("Sharpness For Desktop response: ", e), Ke.sharpnessValuesForGame(e.data.sharpen)
          }).catch(function(t) {
            return qe.error("failed to get nis state info", t), e.reject(t)
          }).finally(function() {
            It = null
          })
        }
        return It
      }, Ke.getDVCStateInfo = function() {
        return We()
      }, Ke.setDVCStateInfo = function(t, n) {
        return C.setState({}, {
          cmsId: gt ? gt.toString() : "0",
          processId: mt ? mt : 0,
          enabled: Ke.isdeepDVCChromaAllowListed,
          supported: t.supported,
          vibrance: t.vibrance,
          saveToDRS: n
        }).then(function(e) {
          return e
        }).catch(function(t) {
          return qe.error("failed dvc Set State info", t), e.reject(t)
        })
      }, Ke.getDeepDVCSystemSupport = function() {
        if (_.isNull(Rt)) {
          var t = C.getState({
            cmsId: "0",
            processId: 0
          });
          Rt = t.then(function(e) {
            qe.info("DeepDVC system support response: ", e), Ke.deepDVCSystemSupport = e.data.supported
          }).catch(function(t) {
            return qe.error("failed to get DeepDVC state info", t), Ke.deepDVCSystemSupport = !1, e.reject(t)
          }).finally(function() {
            Rt = null
          })
        }
        return Rt
      }, Ke.getVibranceSliderSupport = function() {
        return Ke.deepDVCSystemSupport && Ke.isdeepDVCChromaAllowListed
      }, Ke.getSharpnessSliderSupport = function() {
        return Ke.nisSystemSupport && gt
      }, Ke.sendDeepDVCTelemetry = function(e) {
        qe.info("sendDeepDVCTelemetry: ", e), Vt.controlValue = e, Vt.CMSId = gt, E.isInDesktopMode().then(
          function(e) {
            Vt.gameLaunchMode = e === !0 ? j.desktop : j.fullscreen, y.push(D.OSC_ANSEL_FILTER_CONTROL_SETTINGS,
              Vt)
          })
      }
    }
  ]);
  t.nvCameraService = m
}
