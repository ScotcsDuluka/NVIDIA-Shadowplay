// ─────────────────────────────────────────────────────────────
// APP MODULE 184
// role       : controller nvGalleryHistoryMenu
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
  }), t.nvGalleryHistoryMenu = void 0;
  var o = n(3),
    r = i(o),
    a = n(1);
  n(11), n(12), n(23);
  var l = a.ngMainModule.controller("nvGalleryHistoryMenu", ["$scope", "$state", "$log", "$timeout", "$window",
    "$filter", "eventAggregator", "galleryService", "oscGalleryService", "cefService", "oscDisplayService",
    "oscNotificationService", "NOTIFIER_SELECTIONS", "KEYBOARD_EVENTS", "GALLERY_TYPES", "GALLERY_SUBTYPES",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h) {
      var b = this;
      b.title = "l10n.gallery", b.icon = "icon-gallery", b.status = "l10n.uploadHistory", b.uploadHistory = [], b
        .historyIndex = 0, b.history = "", b.disableButtons = !1, b.videos = 0, b.images = 0, b.foundItems = !1, b
        .enableFilter = !1;
      var x = !1,
        v = n.getInstance("osc/nvGalleryHistoryMenu");
      b.filterData = "", b.setfilterDataPreText = function() {
        b.filterData = {
          preText: a("translate")("l10n.uploadHistory"),
          enableFilter: b.enableFilter
        }
      }, b.filterDataChange = function(e) {
        b.item = e.item, b.updateFilterView(), b.enableEscapeEvent(!e.filterOpen)
      }, b.copyUrl = function() {
        b.disableButtons !== !0 && void 0 !== b.uploadHistory && b.uploadHistory.length && ("" === b.history && (b
          .history = b.uploadHistory[0]), v.info("Copy URL: ", b.history.url), c.setClipboardData(b.history
          .url), f.show(m.UPLOAD_URL_COPIED))
      }, b.clearList = function() {
        b.disableButtons !== !0 && void 0 !== b.uploadHistory && b.uploadHistory.length && (v.info(
          "Clearing upload history"), s.clearUploadHistory().then(function() {
          b.back()
        }))
      }, b.back = function() {
        t.go("main.gallery.files")
      }, b.selectHistory = function(e, t) {
        b.historyIndex = t, b.history = e
      }, b.openURL = function(e) {
        e && (b.history = e), void 0 === b.history && (b.history = b.uploadHistory[b.historyIndex]), u.closeOSC(),
          v.info("Go to URL: ", b.history.url), o.open(b.history.url)
      }, b.isActiveHistoryIndex = function(e) {
        return b.historyIndex === e
      }, b.isHistoryVisible = function(e) {
        var t = !1;
        if (!b.item) return t;
        if (1 === b.item.id) t = !0;
        else if (2 === b.item.id && e.type === p.VIDEO) t = !0;
        else {
          if (10 === b.item.id && e.subtype === h.MTA) return !0;
          5 === b.item.id && e.subtype === b.item.subtype ? t = !0 : e.type === p.IMAGE && (e.subtype === b.item
            .subtype || 3 === b.item.id && e.subtype === h.HIGHLIGHTS || 3 === b.item.id && e.subtype === h
            .NORMAL_ANSEL) && (t = !0)
        }
        return b.foundItems = t, t
      }, b.isHistoryItemsVisible = function() {
        for (var e = 0; e < b.uploadHistory.length; e++)
          if (b.isHistoryVisible(b.uploadHistory[e]) === !0) return !0;
        return !1
      }, b.enableEscapeEvent = function(e) {
        var t = e;
        i(function() {
          t === !0 ? l.on(g.ESCAPE, b.back) : l.off(g.ESCAPE, b.back)
        }, 200)
      }, b.findFirstItemBySubtype = function(e) {
        for (var t = 0; t < b.uploadHistory.length; t++)
          if (b.uploadHistory[t].type === p.IMAGE && b.uploadHistory[t].subtype === e) return t
      }, b.findFirstItemByType = function(e) {
        for (var t = 0; t < b.uploadHistory.length; t++)
          if (b.uploadHistory[t].type === e) return t
      }, b.updateFilterView = function() {
        if (void 0 === b.item) return !1;
        var e = 0;
        e = b.item.id >= 3 ? b.findFirstItemBySubtype(b.item.subtype) : 2 === b.item.id ? b.findFirstItemByType(p
            .VIDEO) : 0, b.selectHistory(b.uploadHistory[e], e), d.setCurrentFolderDisplaySetting(b.item.id), b
          .controlButtons(), b.foundItems = !1
      }, b.findSubtypeCount = function() {
        for (var e = 0, t = b.item.subtype, n = 0; n < b.uploadHistory.length; n++) b.uploadHistory[n].type === p
          .IMAGE && b.uploadHistory[n].subtype === t && e++;
        return v.info("Subtype count: ", e), e
      }, b.controlButtons = function() {
        0 === b.uploadHistory.length || b.item.id >= 3 && 0 === b.findSubtypeCount() || 2 === b.item.id && 0 === b
          .videos ? b.disableButtons = !0 : b.disableButtons = !1
      }, b.initialize = function() {
        v.info("Initialize Gallery History VM"), b.enableEscapeEvent(!0), x = !0, b.setfilterDataPreText(), b
          .uploadHistory.length = 0, s.getUploadHistory().then(function(e) {
            b.uploadHistory = e, r.forEach(e, function(e) {
              b.images += e.type === p.IMAGE ? 1 : 0, b.videos += e.type === p.VIDEO ? 1 : 0
            }), b.enableFilter = !0, b.setfilterDataPreText(), b.controlButtons(), b.updateFilterView()
          }), v.info("Items Available: ", b.uploadHistory.length)
      }, b.initialize(), e.$on("$destroy", function() {
        b.enableEscapeEvent(!1), x = !1
      })
    }
  ]);
  t.nvGalleryHistoryMenu = l
}
