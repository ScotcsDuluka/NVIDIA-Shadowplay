// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 216
// controller ModsMenuController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  require(40) /* app/40 — nvCameraService (service) */, require(12) /* app/12 — oscDisplayService (service) */, require(5) /* app/5 — hoverFocus (directive) */, angular.module("main").controller("ModsMenuController", ["$scope",
    "$state", "$stateParams", "$log", "$document", "$timeout", "SHADOWPLAY_EVENTS", "$q",
    "eventAggregator", "nvCameraService", "oscDisplayService", "shadowPlayService", "KEYBOARD_EVENTS",
    "NVCAMERA_EVENTS", "TELEMETRY_OSC_EVENT_NAMES", "HOTKEY_EVENTS", "OSC_KEYBOARD", "hotkeyService",
    "cefService", "$filter",
    function(e, t, n, i, o, r, a, l, s, d, c, u, f, m, g, p, h, b, x, v) {
      function y() {
        O.slot ? (O.filters = O.slot.filters, O.activeFilterStack = O.slot.activeFilterStack, A.info(
            "activeFilterStack : ", O.activeFilterStack), O.activeFilter = O.slot.activeFilter, O
          .activeIndex = O.slot.activeIndex, O.selectedFilter = O.slot.selectedFilter) : (O.filters =
          void 0, O.activeFilterStack = void 0, O.activeFilter = void 0, O.activeIndex = -1, O
          .selectedFilter = void 0);
      }

      function w(e) {
        A.info("Filter Settings");
        if ("None" !== e.id) {
          var t = e.id,
            n = _.findWhere(O.filters, {
              id: t
            });
          _.isUndefined(n) || (n.controls = [], e.controls.forEach(function(e) {
            var i = "" + t + "#" + e.id;
            d.createFilterControl(e, n, i);
          }));
        }
      }

      function S(e) {
        O.slot = void 0, O.slots = d.modsSlots, d.lastActiveSlot >= 0 ? O.clickSlot(O.slots[d
          .lastActiveSlot]) : O.clickSlot(void 0), s.off(m.MODS_FILTERS_LOADED, S);
      }

      function E() {
        A.info("ModsMenuController::onGameAppExit invoked. ModsUIRunning = " + d.ModsUIRunning), c
          .closeOSC();
      }

      function k() {
        if (d.sendEndTelemetry(), void 0 !== O.slotFocus && 0 !== O.slotFocus && O.activeFilterStack && O
          .activeFilterStack.length > 0) {
          var e = 1;
          d.sendModsAppliedTelemetry(e);
        }
      }

      function T() {
        d.getNISStateInfo().then(function(e) {
          e ? (O.nisStateInfo = e, O.sharpness.value = O.nisStateInfo.sharpen, O
            .sharpnessSliderSupported = O.nisStateInfo.supported, d.checkDesktopMode().then(
              function(e) {
                A.info("checkDesktopMode response: ", e), O.sharpnessSliderEnabled = O.nisStateInfo
                  .enabled && !e;
              }), A.info("NIS state info", O.nisStateInfo)) : O.sharpnessSliderSupported = !1;
        }).catch(function(e) {
          A.error("get nisStateInfo failed with error:", e);
        });
      }

      function C() {
        d.getDVCStateInfo().then(function(e) {
          e ? (O.dvcStateInfo = e, O.vibrance.value = O.dvcStateInfo.vibrance, O
            .vibranceSliderEnabled = O.dvcStateInfo.enabled, O.vibranceSliderSupported = O
            .dvcStateInfo.supported) : O.vibranceSliderSupported = !1, A.info("DVC state info", O
            .dvcStateInfo);
        }).catch(function(e) {
          A.error("get getDVCStateInfo failed with error:", e);
        });
      }
      var O = this,
        A = i.getInstance("main.nvcamera/ModsMenuController"),
        I = !1,
        M = null,
        R = !0,
        P = null,
        D = !1;
      O.slotFocus = 0, O.slots = d.modsSlots, O.totalSlots = d.NUM_MODS_SLOTS, O.activeFilter = void 0, O
        .activeIndex = -1, O.selectedFilter = void 0, O.slot = void 0, O.showFilterDropDown = !1, O
        .sharpness = {
          value: 0,
          min: 0,
          max: 100,
          steps: 1,
          default: 50,
          text: O.getSharpnessText
        }, O.nisStateInfo = null, O.sharpnessSliderEnabled = !1, O.sharpnessSliderSupported = !1, O
        .sharpnessValueChanged = !1, O.vibrance = {
          value: 0,
          min: 0,
          max: 100,
          steps: 1,
          default: 50,
          text: O.getVibranceText
        }, O.dvcStateInfo = null, O.vibranceSliderEnabled = !1, O.vibranceSliderSupported = !1, O
        .vibranceValueChanged = !1, O.filterStackSupported = !1, O.clickSlot = function(e) {
          e ? (A.info("slot " + e.id + " clicked"), O.slotFocus = e.id) : (A.info("slot off clicked"), O
            .slotFocus = void 0), O.slot && (O.slot.activeFilter = O.activeFilter, O.slot.activeIndex =
            O.activeIndex, O.slot.selectedFilter = O.selectedFilter), O.slot = d.applySlot(e), y();
        }, O.addDisabled = function() {
          return !O.slot || d.modsHelper.addDisabled(O.slot);
        }, O.addTempFilter = function(e) {
          if (O.slot) return d.modsHelper.tempAddFilter(O.slot, e);
        }, O.removeTempFilter = function() {
          O.slot && d.modsHelper.tempRemoveFilter(O.slot);
        }, O.addCurrentFilter = function(e) {
          A.info("Current filter application started"), O.slot && (O.slot.selectedFilter = e, d.modsHelper
            .addCurrentFilter(O.slot).then(function() {
              y();
            }));
        };
      var N = null;
      O.toggleSubMenuVisibility = function(e, t) {
        O.slot && N && N.then(function() {
          e.stopImmediatePropagation(), N = null;
        });
      }, O.openFilterDropDown = function() {
        O.showFilterDropDown = !0, o[0].getElementById("filter-dropdown").click();
      }, O.activateFilter = function(e) {
        O.slot && (N = O.activeFilter !== e ? l.when(!0) : null, d.modsHelper.activateFilter(O.slot, e),
          y());
      }, O.moveActiveItemUp = function() {
        O.slot && d.modsHelper.moveActiveItemUp(O.slot, R).then(function() {
          y();
        });
      }, O.moveActiveItemDown = function() {
        O.slot && d.modsHelper.moveActiveItemDown(O.slot, R).then(function() {
          y();
        });
      }, O.removeActiveItem = function() {
        O.slot && d.modsHelper.removeActiveItem(O.slot).then(function() {
          y();
        });
      }, O.upDisabled = function() {
        return !O.slot || d.modsHelper.upDisabled(O.slot, R);
      }, O.downDisabled = function() {
        return !O.slot || d.modsHelper.downDisabled(O.slot, R);
      }, O.trashDisabled = function() {
        return !O.slot || d.modsHelper.trashDisabled(O.slot);
      }, O.enableEscapeEvent = function(e) {
        r(function() {
          e === !0 ? s.on(f.ESCAPE, O.close) : s.off(f.ESCAPE, O.close);
        }, 200);
      };
      var L = !1;
      O.initialize = function() {
        A.info("Initialize mods UI"), d.setUIRunning(!0), s.on(a.GAME_EXITED, E), s.on(m
            .MODS_FILTERS_LOADED, S), s.on(m.MODS_FILTER_SETTINGS_READY, w), s.on(m.UPDATE_UI_DATA, y),
          x.isInDesktopMode().then(function(e) {
            D = !e, D && (M = b.createHotKey(u.HotkeyShortcuts.MODSUI, p.MODS_SHOWUI), P = b
              .createHotKey(u.HotkeyShortcuts.SCREENSHOT, p.SCREENSHOT), null != M && M
              .startHotkeyDetection(), null != P && P.startHotkeyDetection());
          });
        var e = d.modsIsEnabled;
        return O.filterStackSupported = d.modsSupported, O.sharpnessSliderSupported = d
          .getSharpnessSliderSupport(), O.sharpnessSliderSupported && T(), O.vibranceSliderSupported = d
          .getVibranceSliderSupport(), O.vibranceSliderSupported && C(), O.enableEscapeEvent(!0), d
          .enableModsMode(!0).then(function(t) {
            if (e) {
              var n = d.modsActiveSlot;
              d.isSetMultipleFilterAPISupported() || d.applySlot(void 0), O.clickSlot(n), L = !0;
            }
            d.sendStartTelemetry(), d.sendEndTimerTelemetry(g.OSC_MENU_LAUNCH, "mods"), O
              .enableEscapeEvent(!0), s.on(p.MODS_SHOWUI, O.close), I = !0, A.info(
                "Mods UI initialized");
          });
      }, O.initialize(), O.close = function() {
        if (!F) return O.destroy().finally(function() {
          _.isUndefined(n.lastState) || "base" === n.lastState || "nvcamera" === n.lastState ? c
            .closeOSC() : t.go(n.lastState, n.lastParams);
        });
      };
      var F = !1;
      O.destroy = function() {
        A.info("Destroying the controller"), F = !0, O.enableEscapeEvent(!1);
        var e = !1;
        O.slot && O.activeFilterStack && O.activeFilterStack.length > 0 && (O.slot.activeFilter = O
          .activeFilter, O.slot.activeIndex = O.activeIndex, O.slot.selectedFilter = O.selectedFilter,
          e = !0), s.off(m.MODS_FILTERS_LOADED, S), s.off(m.MODS_FILTER_SETTINGS_READY, w), s.off(a
          .GAME_EXITED, E), s.off(p.MODS_SHOWUI, O.close), s.off(m.UPDATE_UI_DATA, y);
        var t = [];
        return null != M && t.push(M.stopHotKeyDetection()), null != P && t.push(P
          .stopHotKeyDetection()), O.sharpnessSliderSupported && O.nisStateInfo && O
          .sharpnessValueChanged && O.sharpnessChanged(O.nisStateInfo.sharpen, !0), O
          .vibranceSliderSupported && O.dvcStateInfo && O.vibranceValueChanged && O.vibranceChanged(O
            .dvcStateInfo.vibrance, !0), l.all(t).then(function() {
            return d.shutdownModsUI(e);
          }).then(function() {
            k();
          });
      }, e.$on("$destroy", function e() {
        return I ? void(F ? (O.enableEscapeEvent(!1), I = !1, F = !1) : O.destroy().finally(
      function() {
          I = !1, F = !1;
        })) : void r(e, 50);
      }), O.sharpnessChanged = function(e, t) {
        A.info(" sharpnessChanged : " + e), O.sharpnessValueChanged = !0, O.nisStateInfo && (O
          .nisStateInfo.sharpen = e, d.sharpnessChanged(O.nisStateInfo, t));
      }, O.onSliderKeyDown = function(e, t) {
        e.keyCode === h.LEFT_ARROW || e.keyCode === h.RIGHT_ARROW ? (A.info(" sharpnessChanged : " + t),
          O.nisStateInfo.sharpen = t, O.sharpnessValueChanged = !0) : e.keyCode === h.ENTER && (A
          .info(" Set Sharpness : " + t), O.nisStateInfo.sharpen = t, O.sharpnessChanged(t, !1));
      }, O.getSharpnessText = function(e) {
        return e + "% ";
      }, O.sharpnessForGame = function(e) {
        0 !== e && 100 !== e || O.sharpnessChanged(e, !1), d.sharpnessValuesForGame(e), O
          .sharpnessValueChanged = !0;
      }, O.vibranceChanged = function(e, t) {
        A.info(" vibranceChanged : " + e), O.vibranceValueChanged = !0, O.dvcStateInfo && (O
          .dvcStateInfo.vibrance = e, t && d.sendDeepDVCTelemetry(O.dvcStateInfo.vibrance), d
          .setDVCStateInfo(O.dvcStateInfo, t).then(function(e) {
            A.info("DVC setState info", e);
          }).catch(function(e) {
            A.error("setDVCState failed with error:", e);
          }).finally(function() {
            A.error("onSetDVCStateFinish finally");
          }));
      }, O.getVibranceText = function(e) {
        return 0 === e ? v("translate")("l10n.off") : e + "% ";
      };
    }
  ]);
}
