// ─────────────────────────────────────────────────────────────
// APP MODULE 252
// role       : controller PreferencesOverlaysController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.PreferencesOverlaysController = void 0;
  var i = n(1);
  n(4), n(35), n(22);
  var o = i.ngMainModule.controller("PreferencesOverlaysController", ["$log", "$scope", "$state", "$stateParams",
    "osdService", "octoolService", "eventAggregator", "KEYBOARD_EVENTS", "OSC_KEYBOARD", "PERFTOOL_EVENTS",
    function(e, t, n, i, o, r, a, l, s, d) {
      function c() {
        u.overlaySettings[u.selectedToggleItem].view = r.selectedOverlayViewName
      }
      var u = this;
      u.title = "l10n.settings", u.icon = "icon-settings", u.status = "";
      var f = e.getInstance("main.preferences.overlays/preferencesoverlayscontroller");
      u.cameraSizes = {
        Small: "overlays-editor-camera-size-small",
        Medium: "overlays-editor-camera-size-medium",
        Large: "overlays-editor-camera-size-large"
      }, u.quadrants = {
        left: {
          LeftTop: {
            border: "overlays-editor-top-left-border",
            nested: "overlays-editor-content-nested-top-left",
            margin: "overlays-preview-top-left-margin",
            contents: "overlays-preview-top-contents"
          },
          LeftBottom: {
            border: "overlays-editor-bottom-left-border",
            nested: "overlays-editor-content-nested-bottom-left",
            margin: "overlays-preview-bottom-left-margin",
            contents: "overlays-preview-bottom-contents"
          }
        },
        right: {
          RightTop: {
            border: "overlays-editor-top-right-border",
            nested: "overlays-editor-content-nested-top-right",
            margin: "overlays-preview-top-right-margin",
            contents: "overlays-preview-top-contents"
          },
          RightBottom: {
            border: "overlays-editor-bottom-right-border",
            nested: "overlays-editor-content-nested-bottom-right",
            margin: "overlays-preview-bottom-right-margin",
            contents: "overlays-preview-bottom-contents"
          }
        }
      }, u.initPreferencesOverlays = function() {
        u.overlaySettings = o.overlaySettings, u.editorQuadrantMouseover = "Nowhere", u.perfOverlayViews = r
          .overlayViews.filter(function(e) {
            return e.enabled
          }), i.selectedOverlay && u.overlaySettings[i.selectedOverlay].supported ? u.setToggleItem(i
            .selectedOverlay) : u.overlaySettings.Camera.supported ? u.setToggleItem("Camera") : u.setToggleItem(
            "Status")
      }, u.isSettingSupported = function(e) {
        return u.overlaySettings[e].supported
      }, u.isToggleItemSelected = function(e) {
        return e === u.selectedToggleItem
      }, u.setToggleItem = function(e) {
        f.info("Setting toggle item " + e), u.isToggleItemSelected(e) || (u.selectedToggleItem = e)
      }, u.getIcon = function(e, t) {
        return "Camera" === e ? u.overlaySettings[e].icon[t] : "Performance" === e && "FPS" === u.overlaySettings
          .Performance.view ? u.overlaySettings.FPS.icon : u.overlaySettings[e].icon
      }, u.getEditorIcon = function() {
        return u.getIcon(u.selectedToggleItem, "Small")
      }, u.getPreviewIcon = function(e) {
        return u.getIcon(e, u.overlaySettings[e].size)
      }, u.getEditorIconOpacity = function(e) {
        var t = 0;
        return u.overlaySettings[u.selectedToggleItem].supported ? u.isEditorQuadrantSelected(e) ? t = 1 : e === u
          .editorQuadrantMouseover && (t = .5) : t = 0, {
            margin: "3px",
            opacity: t,
            filter: "alpha(opacity=" + 100 * t + ")"
          }
      }, u.setEditorQuadrantMouseover = function(e) {
        u.editorQuadrantMouseover = e
      }, u.isEditorQuadrantSelected = function(e) {
        return u.overlaySettings[u.selectedToggleItem].enabled && e === u.overlaySettings[u.selectedToggleItem]
          .position
      }, u.setEditorQuadrant = function(e) {
        return f.info("Setting editor quadrant for toggle item " + u.selectedToggleItem + " as " + e),
          "Nowhere" === e ? void f.error("Invalid position") : void(u.isEditorQuadrantSelected(e) || (u
            .overlaySettings[u.selectedToggleItem].position = e, u.overlaySettings[u.selectedToggleItem]
            .enabled = !0, o.overlaySettings = u.overlaySettings, o.saveOverlaySettings(u.selectedToggleItem)))
      }, u.isToggleItemOff = function() {
        return !u.overlaySettings[u.selectedToggleItem].enabled
      }, u.setToggleItemOff = function() {
        f.info("Setting toggle item " + u.selectedToggleItem + " off"), u.isToggleItemOff() || (u.overlaySettings[
          u.selectedToggleItem].enabled = !1, o.overlaySettings = u.overlaySettings, o.saveOverlaySettings(u
          .selectedToggleItem))
      }, u.isCamera = function() {
        return "Camera" === u.selectedToggleItem
      }, u.hideCameraSize = function() {
        return !(u.isCamera() && !u.isToggleItemOff())
      }, u.isPerformanceSelected = function() {
        return "Performance" === u.selectedToggleItem
      }, u.hidePerfOverlayViews = function() {
        return !(u.isPerformanceSelected() && !u.isToggleItemOff())
      }, u.isPerfOverlayViewSet = function(e) {
        return u.overlaySettings[u.selectedToggleItem].view === e.id
      }, u.setPerfOverlayView = function(e) {
        f.info("Setting perf overlay view to " + e.id), u.isPerformanceSelected() && (u.isPerfOverlayViewSet(e) ||
          (u.overlaySettings[u.selectedToggleItem].view = e.id, o.overlaySettings = u.overlaySettings, o
            .saveOverlaySettings(u.selectedToggleItem), r.overlayViewUpdated = !0))
      }, u.isCameraSizeThis = function(e) {
        return u.isCamera() && e === u.overlaySettings[u.selectedToggleItem].size
      }, u.setCameraSize = function(e) {
        return f.info("Setting camera size to " + e), "NoSize" === e ? void f.error("Invalid camera size") : void(
          "Camera" === u.selectedToggleItem && (u.isCameraSizeThis(e) || (u.overlaySettings[u
            .selectedToggleItem].size = e, o.overlaySettings = u.overlaySettings, o.saveOverlaySettings(u
            .selectedToggleItem))))
      }, u.isPreviewIconVisibleHere = function(e, t) {
        return u.isSettingSupported(e) && u.overlaySettings[e].enabled && t === u.overlaySettings[e].position
      }, u.getPreviewIconStyle = function(e, t) {
        if (u.isPreviewIconVisibleHere(e, t)) {
          var n = "right";
          return "LeftTop" !== t && "LeftBottom" !== t || (n = "left"), {
            float: n,
            margin: "3px"
          }
        }
        return {
          display: "none"
        }
      }, u.done = function() {
        i.lastState ? "octoolmenu" === i.lastState ? r.launchOCToolMenu() : n.go(i.lastState, i.lastParams) : n
          .go("main.preferences")
      }, u.keyDownItem = function(e, t) {
        e.keyCode === s.TAB && (e.shiftKey || "Viewers" !== t || u.setEditorQuadrantMouseover("LeftTop"))
      }, u.keyDownOff = function(e) {
        e.keyCode === s.TAB && e.shiftKey && u.setEditorQuadrantMouseover("RightBottom")
      }, u.keyDown = function(e, t) {
        if (f.debug("keyDown keyCode: " + e.keyCode + " Position: " + t), e.keyCode === s.TAB) {
          var n = e.shiftKey;
          switch (t) {
            case "LeftTop":
              t = n ? "Nowhere" : "LeftBottom";
              break;
            case "LeftBottom":
              t = n ? "LeftTop" : "RightTop";
              break;
            case "RightTop":
              t = n ? "LeftBottom" : "RightBottom";
              break;
            case "RightBottom":
              t = n ? "RightTop" : "Nowhere";
              break;
            case "Nowhere":
              t = n ? "RightBottom" : "LeftTop"
          }
          u.setEditorQuadrantMouseover(t)
        } else e.keyCode === s.ENTER && u.setEditorQuadrant(t)
      }, a.on(l.ESCAPE, u.done), a.on(d.PERF_OVERLAY_VISIBILITY_CHANGED, c), t.$on("$destroy", function() {
        a.off(l.ESCAPE, u.done), a.off(d.PERF_OVERLAY_VISIBILITY_CHANGED, c)
      })
    }
  ]);
  t.PreferencesOverlaysController = o
}
