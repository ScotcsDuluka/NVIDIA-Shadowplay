// ─────────────────────────────────────────────────────────────
// APP MODULE 183
// role       : controller GalleryFilterMenuController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.GalleryFilterMenuController = void 0;
  var i = n(1);
  n(11);
  var o = i.ngMainModule.controller("GalleryFilterMenuController", ["$scope", "$log", "$timeout", "oscGalleryService",
    "eventAggregator", "shadowPlayService", "KEYBOARD_EVENTS", "GALLERY_SUBTYPES", "OSC_KEYBOARD",
    function(e, t, n, i, o, r, a, l, s) {
      function d(e) {
        return e.keyCode === s.TAB && e.shiftKey === !1
      }
      var c = this;
      c.initialized = !1, c.preText = "", c.enableFilter = !1, c.filterOpen = !1, c.filterTabExit = !1;
      var u = t.getInstance("osc/GalleryFilterMenuController");
      c.setupMenu = function() {
        e.Items = [{
          id: 1,
          title: "l10n.showAll"
        }, {
          id: "divider"
        }, {
          id: 2,
          title: "l10n.recordings",
          image: "icon-webcam_on"
        }, {
          id: 10,
          title: "l10n.recordingsMta",
          subtype: l.MTA,
          image: "icon-webcam_on",
          hidden: c.hideMTA
        }, {
          id: 3,
          title: "l10n.screenshots",
          subtype: l.NORMAL,
          image: "icon-screenshot"
        }, {
          id: 4,
          title: "l10n.screenshotsEXR",
          subtype: l.EXR,
          hidden: !0
        }, {
          id: 5,
          title: "l10n.highlights",
          subtype: l.HIGHLIGHTS,
          image: "icon-highlights"
        }, {
          id: "divider"
        }, {
          id: 6,
          title: "l10n.highResolutionPhotos",
          subtype: l.SUPER_RESOLUTION,
          image: "icon-screenshot_hi_res"
        }, {
          id: 7,
          title: "l10n.360PhotoSpheres",
          subtype: l.MONO_360
        }, {
          id: "divider"
        }, {
          id: 8,
          title: "l10n.screenshots3D",
          subtype: l.STEREO
        }, {
          id: 9,
          title: "l10n.360PhotoSpheres3D",
          subtype: l.STEREO_360
        }]
      }, c.selectionOpen = function() {
        c.enableEscapeEvent(!1), c.filterOpen = !0, c.setFilterData()
      }, c.selectionClose = function() {
        c.initialized === !0 && c.enableEscapeEvent(!0), c.filterOpen = !1, i.setCurrentFolderDisplaySetting(c
          .item.id), c.controlButtons()
      }, c.back = function() {}, c.enableEscapeEvent = function(e) {
        var t = e;
        n(function() {
          t === !0 ? o.on(a.ESCAPE, c.back) : o.off(a.ESCAPE, c.back)
        }, 200)
      }, c.keyDownDropdown = function(e) {
        d(e) && (c.filterTabExit = !0, c.setFilterData())
      }, c.setFilterData = function() {
        c.filterData = {
          item: c.item,
          filterOpen: c.filterOpen,
          filterTabExit: c.filterTabExit
        }, e.onFilterDataChanged({
          data: c.filterData
        })
      }, c.findItemId = function(e) {
        return e.id === c.itemid
      }, c.controlButtons = function() {
        c.itemid = i.getCurrentFolderDisplaySetting(), c.item = e.Items.find(c.findItemId), c.setFilterData()
      }, c.initialize = function() {
        return u.info("Initialize Gallery Filter VM"), c.item = e.Items[0], r.isMultiTrackAudioAvailable().then(
          function(e) {
            c.hideMTA = !e, c.setupMenu(), c.controlButtons(), c.initialized = !0
          })
      }, c.setupMenu(), c.initialize(), e.$watch("nvChangeFilter", function() {
        c.nvChangeFilter !== e.nvChangeFilter && (c.preText = e.nvChangeFilter.preText, c.enableFilter = e
          .nvChangeFilter.enableFilter)
      }, !0), e.$on("$destroy", function() {
        c.enableEscapeEvent(!1)
      })
    }
  ]);
  t.GalleryFilterMenuController = o
}
