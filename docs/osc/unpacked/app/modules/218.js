// ─────────────────────────────────────────────────────────────
// APP MODULE 218
// role       : controller NvCameraMenuController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  n(40), n(12), n(5), angular.module("main").controller("NvCameraMenuController", ["$scope", "$state", "$stateParams",
    "$log", "$document", "$timeout", "$q", "$filter", "$window", "eventAggregator", "nvCameraService",
    "oscDisplayService", "shadowPlayService", "cefService", "settingsService", "galleryService",
    "oscNotificationService", "telemetryService", "hotkeyService", "KEYBOARD_EVENTS", "NVCAMERA_EVENTS",
    "NOTIFIER_SELECTIONS", "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_TOGGLE_STATE", "OSC_KEYBOARD",
    "GAMEPAD_EVENTS", "COMMON_EVENTS", "NVCAMERA_STATUS", "HOTKEY_EVENTS", "SHADOWPLAY_EVENTS", "NGX_NOTIFICATIONS",
    "TELEMETRY_OSC_AVAILABLE_STATUS", "TELEMETRY_OSC_SCREEN_STATE", "NVCAMERA_MODE",
    function(e, t, n, i, o, r, a, l, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S, E, k, T, C, O, A, I, M, R, P, D) {
      function N() {
        we.disableRedirect(["keyboard"])
      }

      function L(e) {
        var t;
        return 0 === e.length ? void(we.Resolution.enabled = !1) : (we.Resolution.enabled = !0, t = e.length - 1, we
          .Resolution.resolutions = e, we.Resolution.value = 0, we.Resolution.range.min = 0, we.Resolution.range
          .max = t, we.currentResolution = we.Resolution.resolutions[we.Resolution.value], d.off(y
            .HIGHRES_RESOLUTIONS, L), void Se.info("fillHighResResolutionSlider: ", we.Resolution))
      }

      function F() {
        var e = [];
        we.Resolution.value = 0, we.Resolution.range.min = 0, we.Resolution.range.max = _e, we.Resolution
          .enabled = !1;
        var t = 2560,
          n = 1440;
        Fe.width * Fe.height > t * n && (we.Resolution.range.max = 0);
        for (var i = 0; i <= we.Resolution.range.max; i++) {
          var o = {};
          i > 0 ? (o.multiplier = 2 * e[i - 1].multiplier, we.Resolution.enabled = !0) : o.multiplier = i + 2, o.w =
            Fe.width * o.multiplier, o.h = Fe.height * o.multiplier, e.push(o)
        }
        we.Resolution.resolutions = e, we.currentResolution = we.Resolution.resolutions[we.Resolution.value]
      }

      function U(e) {
        !_.isUndefined(e) && _.isNumber(e.w) && _.isNumber(e.h) && ("stereoRegular" === we.captureType.id && (e.w =
            2 * e.w), Fe.width = e.w, Fe.height = e.h, Se.info("Game resolution (W x H) = (" + e.w + " x " + e.h +
            ")."), we.Resolution.resolutions = [e], we.Resolution.value = 0, we.Resolution.range = {
            min: 0,
            max: 0
          }, we.currentResolution = we.Resolution.resolutions[we.Resolution.value], we.Resolution.enabled = !1, d
          .off(y.SCREENSHOT_RESOLUTION, U), Se.info("fillScreenshotResolutionSlider: ", we.Resolution))
      }

      function z(e) {
        var t = 8,
          n = 0;
        we.Resolution.enabled = !0;
        var i = e.minX,
          o = e.maxX,
          r = "sphericalPanorama" === we.captureType.id ? 2 : 1,
          a = (o - i) / 8;
        for (Se.info("step: ", a), we.Resolution.resolutions = [], n = 0; n <= t; n++) we.Resolution.resolutions[
          n] = {
            w: i + a * n,
            h: (i + a * n) / r
          };
        we.Resolution.range.min = 0, we.Resolution.range.max = t, we.Resolution.value = 0, we.currentResolution = we
          .Resolution.resolutions[we.Resolution.value], d.off(y.PANORAMA_RESOLUTION_RANGE, z), Se.info(
            "fillPanoramaResolutionSlider: ", we.Resolution)
      }

      function G(e) {
        d.off(y.CAMERA_ROLL_VALUE_SET, G)
      }

      function V(e) {
        d.off(y.CAMERA_FOV_VALUE_SET, V)
      }

      function H() {
        var e = 0,
          t = 1;
        e = we.panes.Camera.items.Roll.value - t, e >= we.panes.Camera.items.Roll.range.min && (we.cameraAdjustRoll(
          e), we.panes.Camera.items.Roll.value = e)
      }

      function B() {
        var e = 0,
          t = 1;
        e = we.panes.Camera.items.Roll.value + t, e <= we.panes.Camera.items.Roll.range.max && (we.cameraAdjustRoll(
          e), we.panes.Camera.items.Roll.value = e)
      }

      function Y() {
        document.getElementById("snap-button").focus(), we.captureScreenshot()
      }

      function $() {
        document.getElementById("done-button").focus(), we.close()
      }

      function W() {
        we.gamepadRedirect(!0, ["gamepad"]), c.setNavigationInputDevice(["gamepad"], [!0])
      }

      function j() {
        we.percentComplete = 0, we.captureInProgress = !0;
        var e = "";
        switch (r(function() {
            var e = document.getElementById("progress-bar");
            e && (e = e.getElementsByClassName("progress-indicator-button"), e && e[0].focus())
          }, 0), we.captureType.id) {
          case "regular":
            e = "Screenshot";
            break;
          case "stereoRegular":
            e = "Screenshot(Stereo)";
            break;
          case "highRes":
            e = "Super resolution photo";
            break;
          case "stereoSphericalPanorama":
            e = "360° photo sphere(Stereo)";
            break;
          case "sphericalPanorama":
            e = "360° photo sphere"
        }
        we.isAnselLiteMode ? b.startTimer(S.OSC_ANSEL_LITE_SCREENSHOT_TAKEN) : b.startTimer(S
            .OSC_ANSEL_SCREENSHOT_COMPLETED), c.sendStartTimerTelemetry(S.OSC_ANSEL_SCREENSHOT_CANCELLED), Oe
          .screenshotType = e, Oe.superResolutionFactor = 1, Oe.filterID = "0", Oe.stackedFilters = c
          .getStackedFilterDisplayNames(we.filterSlot), "highRes" === we.captureType.id && (Oe
            .superResolutionFactor = we.currentResolution.multiplier), Ie.screenshotType = Oe.screenshotType, Ie
          .screenshotResolution = Oe.screenshotResolution, Ie.superResolutionFactor = Oe.superResolutionFactor, Ie
          .hdrMode = Oe.hdrMode, we.isAnselLiteMode ? Ie.mode = D.anselLite : Ie.mode = D.gamePhoto, we
          .panningUsed ? Ie.panningUsed = R.yes : Ie.panningUsed = R.no, "highRes" === we.captureType.id && (Ie
            .method = we.HighResEnhanceMenu.selectedOption.id), c.sendScreenshotParams(Ie), c
          .sendScreenshotTelemetry(S.OSC_ANSEL_SCREENSHOT_STARTED), d.off(y.SCREENSHOT_CAPTURE_STARTED, j)
      }

      function K(e) {
        Se.info("Update progress bar: ", e), we.percentComplete = e
      }

      function q() {
        d.off(y.SCREENSHOT_CAPTURE_INPROGRESS, K), d.off(y.SCREENSHOT_CAPTURE_FINISHED, q), d.off(y
          .SCREENSHOT_CAPTURE_FAILED, Q), we.captureInProgress = !1, we.cancelInProgress = !1
      }

      function X(e) {
        we.captureInProgress = !1, be();
        var t = "";
        switch (e) {
          case "regular":
            t = l("translate")("l10n.screenshot");
            break;
          case "stereoregular":
            t = l("translate")("l10n.3Dscreenshot");
            break;
          case "highres":
            t = l("translate")("l10n.highResolutionPhoto");
            break;
          case "stereosphericalpanorama":
            t = l("translate")("l10n.3D360PhotoSphere");
            break;
          case "sphericalpanorama":
            t = l("translate")("l10n.360PhotoSphere")
        }
        Ae.screenshotType = Oe.screenshotType, Ae.screenshotResolution = Oe.screenshotResolution, Ae
          .superResolutionFactor = Oe.superResolutionFactor, Ae.stackedFilters = Oe.stackedFilters, Ae.hdrMode = Oe
          .hdrMode, Ae.grid = Oe.grid, Oe.method = "None", we.panningUsed ? Ae.panningUsed = R.yes : Ae
          .panningUsed = R.no, "highRes" === we.captureType.id && (Ae.method = we.HighResEnhanceMenu.selectedOption
            .id, Oe.method = we.HighResEnhanceMenu.selectedOption.id), m.isInDesktopMode().then(function(e) {
            Oe.gameLaunchMode = e === !0 ? P.desktop : P.fullscreen, e && (Ae.panningUsed = R.no), Ae
              .gameLaunchMode = Oe.gameLaunchMode, we.isAnselLiteMode ? b.endTimer(S
                .OSC_ANSEL_LITE_SCREENSHOT_TAKEN, {
                  info: Ae
                }) : b.endTimer(S.OSC_ANSEL_SCREENSHOT_COMPLETED, {
                info: Oe
              })
          }), we.isAnselLiteMode ? c.sendFilterTelemetry(Oe.stackedFilters, 0, "nvcameralite") : c
          .sendFilterTelemetry(Oe.stackedFilters, 0, "nvcamera"), h.show(w.PHOTOGRAPHIC_SCREENSHOT_SAVED_TO_GALLERY,
            t), c.sendFilterControlTelemetry("nvcamera", we.filterSlot), d.off(y.SCREENSHOT_CAPTURE_DONE, X)
      }

      function Z() {
        we.captureInProgress = !1, we.cancelInProgress = !1, c.sendEndTimerTelemetry(S
          .OSC_ANSEL_SCREENSHOT_CANCELLED), d.off(y.SCREENSHOT_CAPTURE_DONE, X), d.off(y
          .SCREENSHOT_CAPTURE_FINISHED, q), d.off(y.SCREENSHOT_CAPTURE_FAILED, Q), be()
      }

      function Q(e) {
        e && (e.ngxFailureReason ? e.ngxFailureReason != O.SCREENSHOT_TIMEOUT_FAILURE && e.ngxFailureReason != O
            .SCREENSHOT_GENERIC_FAILURE || (we.errorDialogParam.error =
              "l10n.rtxScreenshotFailed.genericFailure.title", we.errorDialogParam.details =
              "l10n.rtxScreenshotFailed.genericFailure.message") : e.errorId == O
            .FAILED_TO_SAVE_SHOT_NO_SPACE_LEFT ? (we.errorDialogParam.error =
              "l10n.anselScreenshotFail.lowDiskSpace.title", we.errorDialogParam.details =
              "l10n.anselScreenshotFail.lowDiskSpace.message", e.data.hasOwnProperty("diskspaceReq") && (we
                .errorDialogParam.arg1 = e.data.diskspaceReq), e.data.hasOwnProperty("dirpath") && (we
                .errorDialogParam.arg2 = e.data.dirpath)) : e.errorId == O.PERMISSION_DENIED ? (we.errorDialogParam
              .error = "l10n.anselScreenshotFail.permissionDenied.title", we.errorDialogParam.details =
              "l10n.anselScreenshotFail.permissionDenied.message", e.data.hasOwnProperty("dirpath") && (we
                .errorDialogParam.arg1 = e.data.dirpath)) : (we.errorDialogParam.error =
              "l10n.anselScreenshotFail.genericFailure.title", we.errorDialogParam.details =
              "l10n.anselScreenshotFail.genericFailure.message")), we.captureInProgress = !1, we.errorDialogParam
          .isCustomError = !0, d.off(y.SCREENSHOT_CAPTURE_FAILED, Q), d.off(y.SCREENSHOT_CAPTURE_INPROGRESS, K), d
          .off(y.SCREENSHOT_CAPTURE_DONE, X), d.off(y.SCREENSHOT_CAPTURE_FINISHED, q), d.off(y
            .SCREENSHOT_CAPTURE_STARTED, j), be()
      }

      function J(e) {
        var t = 0;
        if (e.notification === M.AI_SUPER_RES_STARTED) d.on(y.SCREENSHOT_CAPTURE_INPROGRESS, K), Se.info(
          "NGX Up res triggered");
        else if (e.notification === M.AI_SUPER_RES_PROGRESS) void 0 != e.progress && (t = e.progress, t < 100 && we
          .triggerCaptureProgressEvent("progress", t));
        else if (e.notification === M.AI_SUPER_RES_DONE) {
          if (Ce)
            for (; Ce.length > 0;) p.removeGalleryItem(Ce.pop(), !0);
          t = 100, K(t), r(function() {
            X("highres"), q()
          }, 500)
        } else if (e.notification === M.AI_SUPER_RES_FAILED) {
          Se.info("AI SuperRes failed"), e.errorId = O.FAILED;
          var n = c.errorTelemetryMap[e.ngxFailureReason];
          Se.info("errorvalue : ", n), c.sendAnselScreenshotErrorTelemetry(n), d.trigger(y
            .SCREENSHOT_CAPTURE_FAILED, e)
        }
      }

      function ee() {
        if (Ce.length > 0) {
          var e = Ce[0],
            t = e.slice(e.length - 3, e.length);
          "png" !== t && (e = Ce[0].slice(0, e.length - 4), e = e.concat(".png"), Ce.push(e)), f.captureNGXShot(J,
            we.currentResolution.multiplier, e)
        }
      }

      function te(e) {
        if (Se.info("Filter Settings"), "None" !== e.id) {
          var t = _.findWhere(we.filterSlot.filters, {
            id: e.id
          });
          t.controls = [], e.controls.forEach(function(e) {
            var n = "" + t.id + "#" + e.id;
            c.createFilterControl(e, t, n, Oe)
          })
        }
      }

      function ne() {
        we.filterSlot = c.modsSlots[c.ANSEL_MOD_SLOT], c.applySlot(we.filterSlot), d.off(y.MODS_FILTERS_LOADED, ne)
      }

      function ie(e) {
        we.panes.Camera.items.Roll.range = e[0], we.panes.Camera.items.Roll.value = 0, d.off(y.CAMERA_ROLL_RANGE,
          ie)
      }

      function oe(e) {
        we.panes.Camera.items.FOV.range = e[0], (we.panes.Camera.items.FOV.value < e[0].min || we.panes.Camera.items
          .FOV.value > e[0].max) && (we.panes.Camera.items.FOV.value = e[0].min), d.off(y.CAMERA_FOV_RANGE, oe)
      }

      function re(e) {
        we.panes.Camera.items.FOV.default = e, we.panes.Camera.items.FOV.value = e, Oe.fov = Math.round(e), d.off(y
          .CAMERA_FOV_VALUE, re)
      }

      function ae(e) {
        var t = Math.round(e.currentValue);
        t != we.panes.Camera.items.Roll.value && (we.panes.Camera.items.Roll.value = t)
      }

      function le(e) {
        if (Se.info("addUIElement called"), _.isUndefined(e) || _.isNull(e) || _.isUndefined(e.controlType) || _
          .isNull(e.controlType) || _.isUndefined(e.controlId) || _.isNull(e.controlId)) {
          var t = {};
          throw t.data = e, t.message = "addUIElement called with invalid data!", t
        }
        var n = 0 === we.panes.GameEngine.items.length;
        if (we.panes.GameEngine.show || (we.panes.GameEngine.show = !0), "slider" === e.controlType) {
          if (_.isUndefined(e.minValue) || _.isNull(e.minValue) || isNaN(e.minValue) || _.isUndefined(e.maxValue) ||
            _.isNull(e.maxValue) || isNaN(e.maxValue) || _.isUndefined(e.defaultValue) || _.isNull(e
            .defaultValue) || isNaN(e.defaultValue) || _.isUndefined(e.stepSize) || _.isNull(e.stepSize) || isNaN(e
              .stepSize)) {
            var t = {};
            throw t.data = e, t.message = "addUIElement called with invalid data for control of type slider", t
          }
          Se.info("Adding game engine slider '" + e.displayName + "'"), we.panes.GameEngine.items.push({
            id: e.controlId,
            title: e.displayName,
            type: "sidebar-slider",
            enabled: e.visible,
            controlType: e.controlType,
            textPosition: "header-right",
            text: c.controlValue,
            footerLeft: e.minValue[0],
            footerRight: e.maxValue[0],
            range: {
              min: e.minValue[0],
              max: e.maxValue[0]
            },
            step: 0 === e.stepSize[0] ? .1 : e.stepSize[0],
            default: e.defaultValue[0],
            measureUnit: e.measureUnit || "",
            value: we.panes.GameEngine.savedConfigs[e.controlId] ? we.panes.GameEngine.savedConfigs[e
              .controlId] : e.currentValue[0],
            used: !1,
            onChange: function(t) {
              we.panes.GameEngine.savedConfigs[t.id] = t.value, c.sendSandboxAttribute(t.id, e.controlType, t
                .value), this.used = ge(this)
            },
            onReset: function() {
              return c.sendSandboxAttribute(this.id, this.controlType, this.value)
            }
          })
        } else if ("boolean" === e.controlType) {
          if (_.isUndefined(e.currentValue) || _.isNull(e.currentValue)) {
            var t = {};
            throw t.data = e, t.message = "addUIElement called with invalid data for control of type boolean!", t
          }
          Se.info("Adding game engine boolean '" + e.displayName + "'"), we.panes.GameEngine.items.push({
            id: e.controlId,
            title: e.displayName,
            type: "sidebar-boolean",
            enabled: e.visible,
            controlType: e.controlType,
            default: ke,
            set: we.panes.GameEngine.savedConfigs[e.controlId] ? we.panes.GameEngine.savedConfigs[e
              .controlId] : 1 === e.currentValue[0] || e.currentValue[0] === !0,
            used: !1,
            onChange: function(t) {
              we.panes.GameEngine.savedConfigs[t.id] = t.set, c.sendSandboxAttribute(t.id, e.controlType, t
                .set), this.used = ge(this)
            },
            onKeyDown: function(e) {
              (this.set && e.keyCode === k.LEFT_ARROW || !this.set && e.keyCode === k.RIGHT_ARROW) && (this
                .set = !this.set, this.onChange(this)), c.setNavigationInputDevice(["keyboard"], [!0])
            },
            onReset: function() {
              return c.sendSandboxAttribute(this.id, this.controlType, this.default)
            }
          })
        } else if ("button" === e.controlType) Se.info("Adding sandbox button '" + e.displayName + "'"), we.panes
          .Sandbox.items.push({
            id: e.controlId,
            buttonText: e.displayName,
            type: "sidebar-button",
            enabled: "data.visible",
            used: !1,
            onClick: function() {
              console.info("button " + this.buttonText + " clicked"), c.sendSandboxAttribute(this.id, e
                .controlType), this.used = ge(this)
            }
          });
        else if ("list" === e.controlType) {
          Se.info("Adding sandbox list '" + e.displayName + "'");
          var i = [];
          _.each(e.listItems, function(e, t) {
            i.push({
              id: t,
              title: e
            })
          }), i.push({
            id: "divider"
          }), we.panes.Sandbox.items.push({
            id: e.controlId,
            type: "sidebar-list",
            selectItem: function(t) {
              Se.info("item selected from list:", t), c.sendSandboxAttribute(this.id, e.controlType, t.id),
                this.used = ge(this)
            },
            title: e.displayName,
            selectedItem: "",
            items: i
          })
        } else if ("pulldown" === e.controlType) {
          Se.info("Adding sandbox pulldown '" + e.displayName + "'");
          var i = [];
          _.each(e.ids, function(e, t) {
            var n = _.findWhere(we.panes.Sandbox.items, {
              id: e
            });
            n && (we.panes.Sandbox.items = _.without(we.panes.Sandbox.items, n), i.push(n))
          }), we.panes["Sandbox_pulldown_" + e.controlId] = {
            id: e.controlId,
            title: e.displayName,
            show: !1,
            items: i
          }
        } else if ("label" === e.controlType) Se.info("Adding sandbox label '" + e.displayName + "'"), we.panes
          .Sandbox.items.push({
            id: e.controlId,
            title: e.displayName,
            type: "sidebar-label",
            enabled: "data.visible"
          });
        else {
          if ("edit" !== e.controlType) return void Se.error("Invalid control type '" + e.controlType +
            "' added to nvCamera UI");
          Se.info("Adding sandbox edit box '" + e.displayName + "'");
          var o, r, a, l, s = !1,
            d = e.data,
            u = null;
          switch (e.allowedType) {
            case 0:
              o = "number", r = -2147483648, a = 2147483647, l = 1, d = 0, u = function() {
                this.value = parseInt(this.value.replace(/[^\-0-9]/g, "")), this.value.lastIdexOf("-") > 0 && (
                  this.value = parseInt(this.value)), this.used = ge(this), Se.debug("Edit text submitted: ",
                  this.value), c.sendSandboxAttribute(this.id, e.controlType, this.value)
              };
              break;
            case 1:
              o = "number", r = 0, a = 4294967295, l = 1, d = 0, u = function() {
                this.value = this.value.replace(/[^0-9]/g, "").trim(), Se.debug("Edit text submitted: ", this
                  .value), c.sendSandboxAttribute(this.id, e.controlType, this.value), this.used = ge(this)
              };
              break;
            case 2:
              l = .001, o = "number", d = 0, u = function() {
                this.value = this.value.replace(/[^\-0-9.,]/g, ""), this.value.lastIndexOf("-") > 0 && (this
                    .value = parseFloat(this.value)), (this.value.match(/[,.]/g) || []).length > 1 && (this
                    .value = parseFloat(this.value)), Se.debug("Edit text submitted: ", this.value), c
                  .sendSandboxAttribute(this.id, e.controlType, this.value), this.used = ge(this)
              };
              break;
            case 3:
              o = "text", s = !0, d = "", u = function() {
                this.value = this.value.replace(/[0-9]/g, "").trim(), Se.debug("Edit text submitted: ", this
                  .value), c.sendSandboxAttribute(this.id, e.controlType, this.value), this.used = ge(this)
              };
              break;
            case 4:
              o = "text", d = "", u = function() {
                Se.debug("Edit text submitted: ", this.value).trim(), c.sendSandboxAttribute(this.id, e
                  .controlType, this.value), this.used = ge(this)
              };
              break;
            default:
              var t = {};
              throw t.data = e, t.message = "addUIElement edit called with invalid allowedType!", t
          }
          we.panes.Sandbox.items.push({
            id: e.controlId,
            title: e.DisplayName,
            type: "sidebar-edit",
            dataType: o,
            value: d,
            min: r,
            max: a,
            step: l,
            placeholderText: "" + e.data,
            used: !1,
            onChange: u
          })
        }
        n && se()
      }

      function se() {
        m.oscSendWinKBMessage(k.TAB, k.SHIFT), m.oscSendWinKBMessage(k.TAB, 0)
      }

      function de(e) {
        if (_.isUndefined(e) || _.isNull(e) || _.isUndefined(e.ids) || _.isNull(e.ids)) {
          var t = {};
          throw t.data = e, t.message = "addUIElement called with invalid data!", t
        }
        _.each(we.panes.Sandbox.items, function(t, n) {
          _.contains(e.ids, n.id) && we.panes.Sandbox.items.splice(t, 1)
        }), 0 === we.panes.Sandbox.items.length && (delete we.panes.Sandbox, we.hasSandboxPane = !1)
      }

      function ce(e) {
        if (_.isUndefined(e) || _.isNull(e) || _.isUndefined(e.id) || _.isNull(e.id) || isNaN(e.id) || _
          .isUndefined(e.visible) || _.isNull(e.visible)) {
          var t = {};
          throw t.data = e, t.message = "setUIElementVisibility called with invalid data!", t
        }
        _.each(we.panes.Sandbox.items, function(t, n) {
          e.id === n.id && (n.enabled = e.visible)
        })
      }

      function ue(e) {
        Se.info("removeAllGameSettingControls called. Clearing the GameSetting controls."), we.panes.GameEngine
          .items.splice(0)
      }

      function fe(e) {
        if (_.isUndefined(e) || _.isNull(e) || _.isUndefined(e.ids) || _.isNull(e.ids)) {
          var t = {};
          throw t.data = e, t.message = "getUIElementVisibility called with invalid data!", t
        }
        _.each(we.panes.Sandbox.items, function(t, n) {
          if (_.contains(e.ids, n.id)) return void c.reportVisibility(n.enabled)
        })
      }

      function me(e) {
        if (e && e.captureTypes) {
          we.CaptureMenu.items.forEach(function(t) {
            t.disabled = e.captureTypes.indexOf(t.id) === -1, "highRes" === t.id && we.HighResEnhanceMenu.items
              .forEach(function(e) {
                t.disabled && (e.disabled = !0), "ngx" === e.id && (e.disabled = !we.isRTXUpResAvailable, t
                  .disabled = t.disabled && e.disabled)
              })
          });
          for (var t = 0; t < we.HighResEnhanceMenu.items.length; t++)
            if (!we.HighResEnhanceMenu.items[t].disabled) {
              we.HighResEnhanceMenu.isAvailable = !0, we.HighResEnhanceMenu.selectedOption = we.HighResEnhanceMenu
                .items[t];
              break
            } we.selectCapture(_.findWhere(we.CaptureMenu.items, {
            supported: !0
          })), we.exrMode.supported = e.exrAllowed
        }
      }

      function ge(e) {
        return !(!e || _.isUndefined(e) || _.isNull(e)) && (e.used === !1 && ("None" === Oe.gameEngineControls ? Oe
          .gameEngineControls = e.title : Oe.gameEngineControls += ", " + e.title), !0)
      }

      function pe() {
        return we.captureInProgress ? we.cancelCapture() : we.MousePanningActive && we.isAnselLiteMode ? ye(!0) : we
          .close()
      }

      function he(e) {
        Te = e
      }

      function be() {
        Te && r(function() {
          angular.element(Te).focus()
        }, 0)
      }

      function xe() {
        Se.info("NvCameraMenuController::onGameAppExit invoked. UIRunning = " + c.getUIRunning()), u.closeOSC()
      }

      function ve(e, t, n, i) {
        if (e) {
          var o = !1;
          e.keyCode === t ? (m.oscSendWinKBMessage(k.TAB, 0), o = !0) : e.keyCode === n && (m.oscSendWinKBMessage(k
            .TAB, k.SHIFT), o = !0), o && i && e.stopImmediatePropagation()
        }
      }

      function ye(e) {
        Se.info("toggleAnselLiteRedirection MousePanningActive = ", we.MousePanningActive);
        var t = ["mouse"];
        we.MousePanningActive ? (f.setInputRedirection(!1, t, "spuirleak"), we.MousePanningActive = !1, be()) : e ||
          m.isInDesktopMode().then(function(e) {
            e || (we.MousePanningActive = !0, we.panningUsed = !0, f.setInputRedirection(!0, t, "spuirleak").then(
              function() {}))
          })
      }
      var we = this,
        Se = i.getInstance("main.nvcamera/NvCameraMenuController"),
        Ee = !1,
        ke = !1,
        _e = 1,
        Te = null,
        Ce = [],
        Oe = {
          gameName: "",
          gameLaunchMode: "",
          enhanceMode: E.off,
          gameEngineControls: "None",
          screenshotType: "",
          screenshotResolution: "",
          superResolutionFactor: 1,
          resolution: 1,
          filterID: "",
          filterAttributesValues: "",
          roll: 0,
          fov: 0,
          sketch: 0,
          colorEnhancer: 0,
          vignette: 0,
          brightness: 50,
          contrast: 50,
          vibrance: 50,
          hdrMode: E.off,
          grid: E.off,
          stackedFilters: "",
          method: "",
          GFEDLISRVersion: "0"
        },
        Ae = {
          gameLaunchMode: "",
          screenshotType: "",
          screenshotResolution: "",
          superResolutionFactor: 1,
          stackedFilters: "",
          hdrMode: E.off,
          grid: E.off,
          panningUsed: "",
          method: "",
          GFEDLISRVersion: "0"
        },
        Ie = {
          gameLaunchMode: "",
          screenshotType: "",
          screenshotResolution: "",
          superResolutionFactor: 1,
          hdrMode: E.off,
          panningUsed: "",
          method: "",
          mode: "",
          GFEDLISRVersion: "0"
        };
      we.captureInProgress = !1, we.cancelInProgress = !1, we.MousePanningActive = !1, we.percentComplete = 0, we
        .redirectEnabled = !1, we.styleTransferEnabled = !1, we.showFolderBrowser = !1;
      var Me = null,
        Re = null,
        Pe = null;
      we.hideUI = !1;
      var De = !0,
        Ne = !1,
        Le = null;
      we.errorDialogParam = {
          error: "",
          details: "",
          isCustomError: !1
        }, we.isAnselLiteMode = n.isAnselLiteMode, we.isRTXUpResAvailable = c.isRTXUpResAvailable(), we
        .panningUsed = !1, we.panningWithKB = !1, we.panningWithMouse = !1, we.FilterStack = {
          activationPromise: null,
          showFilterDropDown: !1,
          openFilterDropDown: function() {
            we.FilterStack.showFilterDropDown = !0, o[0].getElementById("filter-dropdown").click()
          },
          onKeyDown: function(e, t) {
            e.keyCode === k.DELETE && this.onStackItemDelete(), e.keyCode !== k.ENTER && e.keyCode !== k
              .RIGHT_ARROW || (this.onStackItemClick(t), we.FilterStack.activationPromise && we.FilterStack
                .activationPromise.then(function() {
                  e.stopImmediatePropagation(), we.FilterStack.activationPromise = null
                })), c.setNavigationInputDevice(["keyboard"], [!0]);
          },
          onHeaderClick: function(e, t) {
            we.FilterStack.activationPromise && we.FilterStack.activationPromise.then(function() {
              e.stopImmediatePropagation(), we.FilterStack.activationPromise = null
            })
          },
          onStackItemClick: function(e) {
            we.filterSlot.activeFilter !== e ? we.FilterStack.activationPromise = a.when(!0) : we.FilterStack
              .activationPromise = null, c.modsHelper.activateFilter(we.filterSlot, e)
          },
          onStackItemMoveUp: function() {
            c.modsHelper.moveActiveItemUp(we.filterSlot, De).then(function() {
              c.modsHelper.upDisabled(we.filterSlot, De) && r(function() {
                o[0].getElementById("filter-down-button").focus()
              }, 0)
            })
          },
          onStackItemMoveDown: function() {
            c.modsHelper.moveActiveItemDown(we.filterSlot, De).then(function() {
              c.modsHelper.downDisabled(we.filterSlot, De) && r(function() {
                o[0].getElementById("filter-up-button").focus()
              }, 0)
            })
          },
          onStackItemDelete: function() {
            c.modsHelper.removeActiveItem(we.filterSlot).then(function() {
              c.modsHelper.trashDisabled(we.filterSlot) && o[0].getElementById("filter-dropdown").focus()
            })
          },
          getLen: function() {
            return we.filterSlot && we.filterSlot.activeFilterStack ? we.filterSlot.activeFilterStack.length : 0
          },
          filtersAppliedString: function() {
            return l("translate")("l10n.filtersApplied", {
              arg1: we.FilterStack.getLen()
            })
          },
          upDisabled: function() {
            return c.modsHelper.upDisabled(we.filterSlot, De)
          },
          downDisabled: function() {
            return c.modsHelper.downDisabled(we.filterSlot, De)
          },
          trashDisabled: function() {
            return c.modsHelper.trashDisabled(we.filterSlot)
          },
          addDisabled: function() {
            return c.modsHelper.addDisabled(we.filterSlot)
          },
          onSelectMenuKeyDown: function(e, t) {
            e.keyCode === k.ENTER && we.stackThisFilter(t), e.keyCode === k.RIGHT_ARROW && (m.oscSendWinKBMessage(
              k.ENTER, 0), e.stopImmediatePropagation())
          },
          onSelectKeyDown: function(e) {
            e.keyCode == k.DOWN_ARROW ? (we.onKeyDown(e), e.stopImmediatePropagation()) : e.keyCode == k
              .UP_ARROW ? (we.onKeyDown(e), e.stopImmediatePropagation()) : ve(e, k.RIGHT_ARROW, void 0, !0)
          },
          onSelectMouseOver: function() {
            o[0].getElementById("filter-dropdown").focus()
          },
          onFilterAddKeyDown: function(e) {
            ve(e, void 0, k.LEFT_ARROW, !0), we.onKeyDown(e)
          },
          onFilterMoveUpKeyDown: function(e) {
            ve(e, k.RIGHT_ARROW, void 0, !0), we.onKeyDown(e)
          },
          onFilterMoveDownKeyDown: function(e) {
            ve(e, k.RIGHT_ARROW, k.LEFT_ARROW, !0), we.onKeyDown(e)
          },
          onFilterDeleteKeyDown: function(e) {
            ve(e, void 0, k.LEFT_ARROW, !0), we.onKeyDown(e)
          }
        }, we.exrMode = {
          isOn: !1,
          supported: !0,
          onKeyDown: function(e) {
            (this.isOn && e.keyCode === k.LEFT_ARROW || !this.isOn && e.keyCode === k.RIGHT_ARROW) && (this
              .isOn = !this.isOn, this.onChange()), c.setNavigationInputDevice(["keyboard"], [!0])
          },
          onChange: function() {
            Oe.hdrMode = this.isOn ? E.on : E.off
          },
          isExrModeSupported: function() {
            return "highRes" === we.captureType.id && "ngx" === we.HighResEnhanceMenu.selectedOption.id ? (we
              .exrMode.isOn = !1, !1) : we.exrMode.supported
          }
        }, we.gridOfThirds = {
          isOn: !1,
          onKeyDown: function(e) {
            (this.isOn && e.keyCode === k.LEFT_ARROW || !this.isOn && e.keyCode === k.RIGHT_ARROW) && (this
              .isOn = !this.isOn, this.onChange()), c.setNavigationInputDevice(["keyboard"], [!0])
          },
          gridLines: {
            height: 0,
            weight: 0,
            line_horizontal_1: 0,
            line_horizontal_2: 0,
            line_vertical_1: 0,
            line_vertical_2: 0
          },
          onChange: function() {
            this.isOn && we.isAnselLiteMode && (this.gridLines.height = o[0].getElementById("nvcameraContainer")
              .clientHeight, this.gridLines.width = o[0].getElementById("nvcameraContainer").clientWidth, this
              .gridLines.line_horizontal_1 = {
                x1: 0,
                y1: this.gridLines.height / 3,
                x2: this.gridLines.width,
                y2: this.gridLines.height / 3
              }, this.gridLines.line_horizontal_2 = {
                x1: 0,
                y1: 2 * this.gridLines.height / 3,
                x2: this.gridLines.width,
                y2: 2 * this.gridLines.height / 3
              }, this.gridLines.line_vertical_1 = {
                x1: this.gridLines.width / 3,
                y1: 0,
                x2: this.gridLines.width / 3,
                y2: this.gridLines.height
              }, this.gridLines.line_vertical_2 = {
                x1: 2 * this.gridLines.width / 3,
                y1: 0,
                x2: 2 * this.gridLines.width / 3,
                y2: this.gridLines.height
              }), c.setGridOfThirdsStatus(this.isOn), Oe.grid = this.isOn ? E.on : E.off
          }
        }, we.gameHUD = {
          isOn: !0,
          supported: we.isAnselLiteMode && !1,
          onKeyDown: function(e) {
            (this.isOn && e.keyCode === k.LEFT_ARROW || !this.isOn && e.keyCode === k.RIGHT_ARROW) && (this
              .isOn = !this.isOn, this.onChange()), c.setNavigationInputDevice(["keyboard"], [!0])
          },
          onChange: function() {
            c.setGameHUDStatus(this.isOn)
          }
        }, we.topMostInFocus = !1, we.bottomMostInFocus = !1, we.HighResEnhanceMenu = {
          isVisible: function() {
            return "highRes" === we.captureType.id
          },
          isAvailable: !1,
          items: [{
            id: "standard",
            title: "l10n.ansel.capModeStandard",
            supported: !0,
            disabled: !1
          }, {
            id: "enhanced",
            title: "l10n.ansel.capModeEnhanced",
            supported: !0,
            disabled: !1
          }, {
            id: "ngx",
            title: "l10n.ansel.capModeNGX",
            supported: !0,
            disabled: !1
          }],
          onKeyDown: function(e, t) {
            t.keyCode !== k.RIGHT_ARROW && t.keyCode !== k.ENTER || this.onClick(e, t), c
              .setNavigationInputDevice(["keyboard"], [!0])
          },
          first: we.isAnselLiteMode ? "ngx" : "standard",
          last: (we.isAnselLiteMode, "ngx"),
          onClick: function(e, t) {
            e.open(), Le = e, we.enableEscapeEvent(!1)
          },
          onMenuItemKeyDown: function(e, t, n) {
            t.keyCode === k.LEFT_ARROW && e.close(), we.onKeyDown(t)
          },
          onMenuItemClick: function(e) {
            this.selectedOption = e, "ngx" === we.HighResEnhanceMenu.selectedOption.id ? F() : c
              .getCaptureTypeResolutions("highres").then(function(e) {
                e === !0 && d.on(y.HIGHRES_RESOLUTIONS, L)
              })
          }
        }, we.CaptureMenu = {
          items: [{
            id: "regular",
            title: "l10n.screenshot",
            supported: !0,
            disabled: !1
          }, {
            id: "highRes",
            title: "l10n.highResolutionPhoto",
            supported: !0,
            disabled: !1
          }, {
            id: "sphericalPanorama",
            title: "l10n.360PhotoSphere",
            supported: !0,
            disabled: !1
          }, {
            id: "stereoRegular",
            title: "l10n.screenshot3D",
            supported: !0,
            disabled: !1
          }, {
            id: "stereoSphericalPanorama",
            title: "l10n.360PhotoSphere3D",
            supported: !0,
            disabled: !1
          }],
          first: "regular",
          last: we.isAnselLiteMode ? "highRes" : "stereoSphericalPanorama",
          onKeyDown: function(e, t) {
            t.keyCode !== k.RIGHT_ARROW && t.keyCode !== k.ENTER || this.onClick(e, t), c
              .setNavigationInputDevice(["keyboard"], [!0])
          },
          onClick: function(e, t) {
            e.open();
            var n = angular.element(o[0].getElementsByClassName("md-open-menu-container"));
            n.addClass("align-menu-top"), we.enableEscapeEvent(!1), Le = e
          },
          onMenuItemKeyDown: function(e, t) {
            t.keyCode === k.LEFT_ARROW && e.close(), we.onKeyDown(t)
          }
        }, we.captureType = we.CaptureMenu.items[0], we.getRollText = function(e) {
          if (0 === e.value) return e.value + "°";
          var t = e.value < 0 ? "l10n.degreeLeft" : "l10n.degreeRight";
          return l("translate")(t, {
            arg1: Math.abs(e.value)
          })
        }, we.getFOVText = function(e) {
          var t = Math.round(1e3 * e.value) / 1e3;
          return t.toFixed() + "°"
        }, we.getResolutionText = function(e) {
          var t = "";
          return "highRes" === we.captureType.id && (t = we.Resolution.resolutions[e].multiplier + "X - "), Oe
            .screenshotResolution = we.Resolution.resolutions[e].w + " x " + we.Resolution.resolutions[e].h, t + we
            .Resolution.resolutions[e].w + " x " + we.Resolution.resolutions[e].h
        }, we.selectFilterOnFocus = function(e, t) {
          return we.onFocus(t), c.modsHelper.tempAddFilter(we.filterSlot, e)
        }, we.unselectTempFilter = function() {
          return c.modsHelper.tempRemoveFilter(we.filterSlot)
        }, we.stackThisFilter = function(e) {
          return Se.info("Current filter application started"), we.filterSlot.selectedFilter = e, c.modsHelper
            .addCurrentFilter(we.filterSlot).then(function(e) {
              c.modsHelper.addDisabled(we.filterSlot) && o[0].getElementById("filter-stack0").focus()
            })
        }, we.Resolution = {
          title: "l10n.captureResolution",
          type: "sidebar-slider",
          enabled: !1,
          textPosition: "footer-center",
          text: we.getResolutionText,
          range: {
            min: 0,
            max: 0
          },
          step: 1,
          value: 0,
          tabIndex: -1,
          onChange: function(e) {
            we.resolutionChanged(e)
          },
          resolutions: [{
            multiplier: 0,
            w: 0,
            h: 0
          }]
        }, we.currentResolution = we.Resolution.resolutions[0], we.hasSandboxPane = !1, we.panes = {
          StyleTF: {
            name: "styletf",
            title: "l10n.styleTransfer",
            show: !1,
            items: {
              Enable: {
                id: "1",
                name: "enable",
                title: "l10n.styleTransferEnable",
                type: "sidebar-boolean",
                enabled: !0,
                set: !1,
                onChange: function(e) {
                  we.enableStyleTransfer(e.set)
                }
              },
              Model: {
                id: "2",
                name: "model",
                title: "l10n.styleTransferModel",
                type: "sidebar-list",
                enabled: !1,
                items: [],
                selectedItem: void 0,
                selectItem: function(e) {
                  we.selectStyleTransferModel(e)
                }
              },
              File: {
                id: "3",
                name: "file",
                title: "l10n.styleTransferImage",
                type: "sidebar-filechooser",
                enabled: !1,
                fileName: void 0,
                onClick: function() {
                  we.selectStyleTransferPath()
                }
              }
            },
            isExpanded: c.getPaneState("StyleTF"),
            onExpand: function() {
              c.setPaneState("StyleTF", !0)
            },
            onCollapse: function() {
              c.setPaneState("StyleTF", !1)
            }
          },
          Camera: {
            name: "camera",
            title: "l10n.camera",
            show: !0,
            items: {
              Roll: {
                id: "1",
                name: "roll",
                title: "l10n.roll",
                type: "sidebar-slider",
                enabled: !0,
                textPosition: "header-right",
                text: we.getRollText,
                range: {},
                step: 1,
                default: 0,
                value: 0,
                onChange: function(e) {
                  we.cameraAdjustRoll(e.value)
                }
              },
              FOV: {
                id: "2",
                name: "fov",
                title: "l10n.fieldOfView",
                type: "sidebar-slider",
                enabled: !0,
                textPosition: "header-right",
                footerLeft: "l10n.less",
                footerRight: "l10n.more",
                text: we.getFOVText,
                range: {},
                step: 1,
                value: 0,
                onChange: function(e) {
                  we.cameraAdjustFOV(e.value)
                }
              }
            },
            isExpanded: c.getPaneState("Camera"),
            onExpand: function() {
              c.setPaneState("Camera", !0)
            },
            onCollapse: function() {
              c.setPaneState("Camera", !1)
            }
          },
          GameEngine: {
            id: "gameengine",
            title: "l10n.ansel.gameEngine",
            items: [],
            show: !1,
            savedConfigs: {},
            isExpanded: c.getPaneState("GameEngine"),
            onExpand: function() {
              c.setPaneState("GameEngine", !0)
            },
            onCollapse: function() {
              c.setPaneState("GameEngine", !1)
            }
          }
        }, we.enableStyleTransfer = function(e) {
          if (!e) return we.styleTransferEnabled = !1, we.panes.StyleTF.items.Model.enabled = !1, we.panes.StyleTF
            .items.File.enabled = !1, void(we.panes.StyleTF.items.Model.items = []);
          we.styleTransferEnabled = !0, we.panes.StyleTF.items.Model.enabled = !0, we.panes.StyleTF.items.File
            .enabled = !0;
          var t = [{
            id: 0,
            title: "Low"
          }, {
            id: 1,
            title: "High"
          }];
          we.panes.StyleTF.items.Model.items = t, we.panes.StyleTF.items.Model.selectItem(t[0])
        }, we.selectStyleTransferModel = function(e) {
          we.panes.StyleTF.items.Model.selectedItem = e.title
        }, we.setStyleTransferPath = function(e) {
          if (!e) return we.showFolderBrowser = !1, a.when(!0);
          var t = e;
          return t && t.lastIndexOf("\\") > 0 && (t = t.slice(t.lastIndexOf("\\") + 1)), we.panes.StyleTF.items.File
            .fileName = t, Se.info("path is: ", e), we.showFolderBrowser = !1, a.when(!0)
        }, we.selectStyleTransferPath = function() {
          var e = we.styleTransferPath || "",
            t = we.styleTransferPath || "";
          t.length > 0 && e.lastIndexOf("\\") > 0 ? (t = t.slice(0, t.lastIndexOf("\\")), e = e.slice(e.lastIndexOf(
            "\\") + 1)) : (e = "", t = "c:\\users\\irichards\\Pictures"), we.filePickerParams = {
            heading: "l10n.styleTransferSelectFile",
            init: e,
            currentPath: t,
            callback: we.setStyleTransferPath,
            includeFiles: !0,
            match: ".png|.jpg|.bmp",
            pathType: "Videos"
          }, we.showFolderBrowser = !0
        }, angular.element(s).on("blur", N);
      var Fe = {};
      e.$on("$mdMenuClose", function(e, t) {
        var n = angular.element(o[0].getElementsByClassName("md-open-menu-container"));
        n.removeClass("align-menu-top"), we.enableEscapeEvent(!0), Le = null, be()
      }), we.selectCapture = function(e) {
        if (e !== we.captureType) {
          Se.info("selectCapture: ", e);
          var t = null;
          t = "highRes" === e.id ? "highres" : "sphericalPanorama" === e.id || "stereoSphericalPanorama" === e
            .id ? "panorama" : "screenshot", null !== t ? ("highres" === t && "ngx" === we.HighResEnhanceMenu
              .selectedOption.id ? (F(), we.captureType = e) : c.getCaptureTypeResolutions(t).then(function(n) {
                n === !0 && ("highres" === t ? d.on(y.HIGHRES_RESOLUTIONS, L) : "panorama" === t ? d.on(y
                    .PANORAMA_RESOLUTION_RANGE, z) : "screenshot" === t && d.on(y.SCREENSHOT_RESOLUTION, U),
                  we.captureType = e)
              }), we.Resolution.tabIndex = "screenshot" === t ? -1 : 0) : (we.captureType = e, we.Resolution
              .value = 0, we.Resolution.enabled = !1, we.Resolution.tabIndex = -1)
        }
      }, we.resolutionChanged = function(e) {
        we.Resolution.value = e, Oe.superResolutionFactor = e, we.currentResolution = we.Resolution.resolutions[we
          .Resolution.value]
      }, we.cameraAdjustRoll = function(e) {
        var t = Math.round(e);
        t ? Oe.roll = t : Oe.roll = 0, d.on(y.CAMERA_ROLL_VALUE_SET, G), c.setCameraAdjustments(e, we.panes.Camera
          .items.FOV.value)
      }, we.cameraAdjustFOV = function(e) {
        var t = Math.round(e);
        t ? Oe.fov = t : Oe.fov = 0, d.on(y.CAMERA_FOV_VALUE_SET, V), c.setCameraAdjustments(we.panes.Camera.items
          .Roll.value, e)
      }, we.captureScreenshot = function() {
        switch (we.captureType.id) {
          case "regular":
          case "stereoRegular":
            c.captureScreenshot(we.captureType.id, we.exrMode.isOn);
            break;
          case "highRes":
            if ("ngx" === we.HighResEnhanceMenu.selectedOption.id) {
              var e = function e(t) {
                Ce.push(t.path), ee(), d.off(y.SCREENSHOT_CAPTURE_FILE_WRITE_DONE, e)
              };
              return c.captureScreenshot("regular", !1), d.on(y.SCREENSHOT_CAPTURE_STARTED, j), d.on(y
                .SCREENSHOT_CAPTURE_FILE_WRITE_DONE, e), void d.on(y.SCREENSHOT_CAPTURE_FAILED, Q)
            }
            Ce = [], c.captureHighResScreenshot(we.captureType.id, we.currentResolution.multiplier, we
              .currentResolution.w, we.currentResolution.h, we.exrMode.isOn, "enhanced" === we
              .HighResEnhanceMenu.selectedOption.id);
            break;
          case "stereoSphericalPanorama":
          case "sphericalPanorama":
            c.capturePanoramaScreenshot(we.captureType.id, we.currentResolution.w, we.currentResolution.h, we
              .exrMode.isOn)
        }
        d.on(y.SCREENSHOT_CAPTURE_STARTED, j), d.on(y.SCREENSHOT_CAPTURE_INPROGRESS, K), d.on(y
          .SCREENSHOT_CAPTURE_FINISHED, q), d.on(y.SCREENSHOT_CAPTURE_DONE, X), d.on(y
          .SCREENSHOT_CAPTURE_FAILED, Q)
      }, we.triggerCaptureProgressEvent = function(e, t, n) {
        "started" === e ? d.trigger(y.SCREENSHOT_CAPTURE_STARTED) : "progress" === e ? d.trigger(y
          .SCREENSHOT_CAPTURE_INPROGRESS, t) : "shotFinished" === e && (d.trigger(y
          .SCREENSHOT_CAPTURE_INPROGRESS, t), r(function() {
          d.trigger(y.SCREENSHOT_CAPTURE_FINISHED), d.trigger(y.SCREENSHOT_CAPTURE_DONE, n)
        }, 500))
      }, we.cancelCapture = function() {
        return Se.info("Cancelling current screenshot capture."), we.cancelInProgress = !0, d.off(y
            .SCREENSHOT_CAPTURE_INPROGRESS, K), "highRes" === we.captureType.id && "ngx" === we.HighResEnhanceMenu
          .selectedOption.id ? f.cancelNGXShot().then(function(e) {
            if (Ce)
              for (; Ce.length > 0;) p.removeGalleryItem(Ce.pop(), !0);
            Z()
          }) : c.cancelScreenshotCapture().then(function(e) {
            Z()
          })
      }, we.enableRedirect = function(e) {
        if (!we.isAnselLiteMode) return c.setNavigationInputDevice(["mouse"], [!0]), we.redirectEnabled === !0 ? a
          .when(!0) : m.isInDesktopMode().then(function(t) {
            return f.setInputRedirection(!0, e, t === !0 ? "windowed" : "fullscreen").then(function() {
              return we.redirectEnabled = !0
            })
          })
      }, we.disableRedirect = function(e) {
        if (!we.isAnselLiteMode) return document.body.style.cursor = "auto", c.setNavigationInputDevice(["mouse"],
          [!0]), we.redirectEnabled === !1 ? a.when(!1) : f.setInputRedirection(!1, e).then(function(e) {
          return we.redirectEnabled = !1
        })
      }, we.gamepadRedirect = function(e, t) {
        if (!we.isAnselLiteMode) return e ? we.redirectEnabled === !0 ? a.when(!0) : m.isInDesktopMode().then(
          function(n) {
            return f.setInputRedirection(e, t, n === !0 ? "windowed" : "fullscreen").then(function() {
              return we.redirectEnabled = !0
            })
          }) : we.redirectEnabled === !1 ? a.when(!1) : f.setInputRedirection(!1, t).then(function(e) {
          return we.redirectEnabled = !1
        })
      }, we.enableEscapeEvent = function(e) {
        r(function() {
          e === !0 ? d.on(v.ESCAPE, pe) : d.off(v.ESCAPE, pe)
        }, 200)
      }, we.hideUIFunction = function() {
        r(function() {
          we.hideUI = !we.hideUI, be()
        }, 0)
      }, we.initialize = function() {
        return Se.info("Initialize Ansel UI"), f.appInFocusTitle().then(function(e) {
          Oe.gameName = e, m.isInDesktopMode().then(function(e) {
            Ne = !e, Ne && (Me = x.createHotKey(f.HotkeyShortcuts.NVCAMERAUI, A.NVCAMERAUI), Re = x
              .createHotKey(f.HotkeyShortcuts.SCREENSHOT, A.SCREENSHOT), null != Me && Me
              .startHotkeyDetection(), null != Re && Re.startHotkeyDetection())
          }), Pe = x.createHotKey("anselHideUIHK", A.ANSEL_HIDEUI), null != Pe && Pe.startHotkeyDetection()
        }), we.CaptureMenu.onMenuItemClick = we.selectCapture, we.filterSlot = void 0, c.setUIRunning(!0, !1, !
          we.isAnselLiteMode), we.gamepadRedirect(!0, ["gamepad"]), g.getLanguage().then(function(e) {
          return c.setLanguage(e)
        }).then(function() {
          d.on(y.CAPTURE_TYPES, me), d.on(y.MODS_FILTERS_LOADED, ne), d.on(y.MODS_FILTER_SETTINGS_READY, te),
            d.on(y.SCREENSHOT_RESOLUTION, U), d.on(T.LEFT_BUMPER, H), d.on(T.RIGHT_BUMPER, B), d.on(T
              .X_BUTTON, Y), d.on(T.Y_BUTTON, $), d.on(T.NAVIGATION, W), d.on(C.ELEMENT_FOCUSSED, he), d.on(I
              .GAME_EXITED, xe), d.on(A.ANSEL_HIDEUI, we.hideUIFunction), we.isAnselLiteMode || (d.on(y
                .CAMERA_ROLL_RANGE, ie), d.on(y.CAMERA_FOV_RANGE, oe), d.on(y.CAMERA_FOV_VALUE, re), d.on(y
                .ADD_UI_ELEMENT, le), d.on(y.REMOVE_UI_ELEMENT, de), d.on(y.GET_UI_ELEMENT_VISIBILITY, fe), d
              .on(y.SET_UI_ELEMENT_VISIBILITY, ce), d.on(y.REMOVE_ALL_GAME_SETTINGS, ue), d.on(y
                .CAMERA_ROLL_VALUE_UPDATE, ae));
          var e = [];
          return e.push(c.getSupportedCaptureTypes()), e.push(c.getSupportedFilterTypes()), e.push(c
            .getCaptureTypeResolutions("screenshot")), we.isAnselLiteMode || (e.push(c
            .getCameraAdjustmentsRange()), e.push(c.getCameraAdjustments())), a.all(e).then(function(e) {
            Se.info("getNvCameraInfo Response: ", e), we.enableEscapeEvent(!0), c.sendUIReady(), c
              .setReadyForGameEngine(), c.sendStartTelemetry(), we.isAnselLiteMode ? c
              .sendEndTimerTelemetry(S.OSC_MENU_LAUNCH, "nvcameralite") : c.sendEndTimerTelemetry(S
                .OSC_MENU_LAUNCH, "nvcamera");
            var t = o[0].getElementById("CaptureMenu");
            he(t), d.on(A.NVCAMERAUI, we.close), Ee = !0, Se.info("Ansel UI initialized")
          })
        })
      }, we.initialize(), we.close = function() {
        if (!Ue) return we.destroy().then(function() {
          c.sendEndTelemetry(we.panningUsed, we.panningWithKB, we.panningWithMouse), _.isUndefined(n
            .lastState) || "base" === n.lastState || "nvcamera" === n.lastState ? u.closeOSC() : t.go(n
            .lastState, n.lastParams)
        })
      }, we.onFocus = function(e) {
        var t = angular.element(e.target).attr("data-item-position");
        "topMost" === t ? (we.topMostInFocus = !0, we.bottomMostInFocus = !1) : "bottomMost" === t ? (we
          .bottomMostInFocus = !0, we.topMostInFocus = !1) : (we.bottomMostInFocus = !1, we.topMostInFocus = !1)
      }, we.onBlur = function(e) {
        var t = angular.element(e.target).attr("data-item-position");
        "topMost" === t ? we.topMostInFocus = !1 : "bottomMost" === t && (we.bottomMostInFocus = !1)
      }, we.HandleMouseDown = function(e, t) {
        we.isAnselLiteMode || we.MousePanningActive || 0 === t.button && (document.body.style.cursor = "none", we
          .MousePanningActive = !0, we.enableRedirect(["mouse"]))
      }, we.HandleMouseClick = function(e) {
        return we.isAnselLiteMode ? void ye() : void we.enableRedirect(["keyboard"])
      }, we.HandleMouseUp = function(e, t, n) {
        if (!we.isAnselLiteMode) return 0 === t.button && (document.body.style.cursor = "auto", we
          .MousePanningActive = !1), n === !1 ? void t.stopImmediatePropagation() : void be()
      }, we.HandleMouseMove = function(e, t) {
        we.isAnselLiteMode && we.MousePanningActive && (we.panningWithMouse = !0)
      }, we.checkCameraMenuVisibility = function() {
        return !we.captureInProgress && !we.hideUI || (Le && (Le.close(), Le = null), we.hideUI && c
          .setNvCameraHideState(!0), Se.info("hiding UI"), !1)
      }, we.onKeyDown = function(e) {
        if (we.MousePanningActive && we.isAnselLiteMode) e.preventDefault(), e.stopPropagation(), we
          .panningWithKB = !0;
        else switch (e.keyCode) {
          case 87:
          case 83:
          case 88:
          case 89:
          case 90:
          case 65:
          case 68:
            we.enableRedirect(["keyboard"]);
            break;
          case k.DOWN_ARROW:
            m.oscSendWinKBMessage(k.TAB, 0), e.preventDefault(), e.stopPropagation();
            break;
          case k.UP_ARROW:
            m.oscSendWinKBMessage(k.TAB, k.SHIFT), e.preventDefault(), e.stopPropagation();
            break;
          case k.TAB:
            (e.shiftKey && we.topMostInFocus || !e.shiftKey && we.bottomMostInFocus) && e.preventDefault()
        }
        c.setNavigationInputDevice(["keyboard"], [!0])
      };
      var Ue = !1;
      we.destroy = function() {
        return Se.info("Destroying the controller"), Ue = !0, we.enableEscapeEvent(!1), a.all([we
          .captureInProgress ? we.cancelCapture() : a.when(!0), we.disableRedirect(["mouse"]), we
          .gamepadRedirect(!1, ["gamepad"]), null != Me ? Me.stopHotKeyDetection() : a.when(!0), null != Re ?
          Re.stopHotKeyDetection() : a.when(!0), null != Pe ? Pe.stopHotKeyDetection() : a.when(!0)
        ]).then(function(e) {
          ye(!0), c.applySlot(void 0), c.setGridOfThirdsStatus(!1), c.shutdownAnselUI(), d.off(y
              .CAPTURE_TYPES, me), d.off(y.ADD_UI_ELEMENT, le), d.off(y.REMOVE_UI_ELEMENT, de), d.off(y
              .GET_UI_ELEMENT_VISIBILITY, fe), d.off(y.SET_UI_ELEMENT_VISIBILITY, ce), d.off(y
              .REMOVE_ALL_GAME_SETTINGS, ue), d.off(T.LEFT_BUMPER, H), d.off(T.RIGHT_BUMPER, B), d.off(T
              .X_BUTTON, Y), d.off(T.Y_BUTTON, $), d.off(T.NAVIGATION, W), d.off(C.ELEMENT_FOCUSSED, he), d
            .off(I.GAME_EXITED, xe), d.off(y.CAMERA_ROLL_VALUE_UPDATE, ae), d.off(y
              .MODS_FILTER_SETTINGS_READY, te), d.off(A.NVCAMERAUI, we.close), d.off(A.ANSEL_HIDEUI, we
              .hideUIFunction)
        })
      }, e.$on("$destroy", function e() {
        return Se.info("onDestroy"), Ee ? (Ue ? (we.enableEscapeEvent(!1), Ee = !1, Ue = !1) : we.destroy()
          .then(function() {
            Ee = !1, Ue = !1
          }), void angular.element(s).off("blur", N)) : (Se.info(
          "Controller not yet initialized, defer for 50 msec"), void r(e, 50))
      })
    }
  ])
}
