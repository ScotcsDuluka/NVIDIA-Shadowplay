// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 188
// controller GalleryRemoveMenuController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.GalleryRemoveMenuController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(11);
  var o = i.ngMainModule.controller("GalleryRemoveMenuController", ["$scope", "$state", "$log", "$timeout",
    "eventAggregator", "galleryService", "oscGalleryService", "telemetryService", "KEYBOARD_EVENTS",
    "TELEMETRY_OSC_EVENT_NAMES",
    function(e, t, n, i, o, r, a, l, s, d) {
      var c = this;
      c.title = "l10n.gallery", c.icon = "icon-gallery", c.status = "l10n.remove", c.fileToRemove = "";
      var u = n.getInstance("osc/GalleryRemoveMenuController"),
        f = !1;
      c.remove = function() {
        u.info("Removing file: ", c.fileToRemove.fullFilename), l.push(d.OSC_GALLERY_FILE_REMOVE), r
          .removeGalleryItem(c.fileToRemove.fullFilename).then(function() {
            var e = r.populatedRecentFilesFolder.length > 0 ? 0 : -1;
            a.setRecentFileIndex(e), a.setCurrentFileIndex(0), c.back();
          }, function(e) {
            l.push(d.OSC_GALLERY_FILE_REMOVE_ERROR), c.back();
          });
      }, c.back = function() {
        t.go("main.gallery.files");
      }, c.enableEscapeEvent = function(e) {
        var t = e;
        i(function() {
          t === !0 ? o.on(s.ESCAPE, c.back) : o.off(s.ESCAPE, c.back);
        }, 200);
      }, c.initialize = function() {
        u.info("Initialize Gallery Remove VM"), c.enableEscapeEvent(!0), c.fileToRemove = a
          .fileToProcess, f = !0;
      }, c.initialize(), e.$on("$destroy", function() {
        c.enableEscapeEvent(!1), f = !1;
      });
    }
  ]);
  exports.GalleryRemoveMenuController = o;
}
