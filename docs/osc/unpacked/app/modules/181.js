// ─────────────────────────────────────────────────────────────
// APP MODULE 181
// role       : controller GalleryFilesMenuController
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
  }), t.GalleryFilesMenuController = void 0;
  var o = n(3),
    r = i(o),
    a = n(1);
  n(4), n(11), n(26);
  var l = a.ngMainModule.controller("GalleryFilesMenuController", ["$scope", "$state", "$log", "$document", "$timeout",
    "oscDisplayService", "oscGalleryService", "eventAggregator", "shadowPlayService", "galleryService",
    "telemetryService", "cefService", "KEYBOARD_EVENTS", "GALLERY_STATE", "OSC_KEYBOARD", "GALLERY_TYPES",
    "GALLERY_AUDIOTYPES", "GALLERY_SUBTYPES", "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_PERF_ID",
    "piplConfigService", "COMMON_EVENTS",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b, x, v, y, w, S) {
      function E(e, t, n) {
        n === y.galleryFullLoad ? u.endGalleryPerf(e, t, c.wasCached ? y.galleryCached : n) : c.wasCached || u
          .endGalleryPerf(e, t, n)
      }

      function k(e) {
        return e.keyCode === p.TAB && e.shiftKey === !0
      }

      function _(e) {
        return e.keyCode === p.TAB && e.shiftKey === !1
      }

      function T(e) {
        O.info("Connect status:", e.isConnectEnabled), C.isConnectEnabled = e.isConnectEnabled
      }
      var C = this;
      C.title = "l10n.gallery", C.icon = "icon-gallery", C.status = "", C.folders = [], C.files = [], C.thumbs = [],
        C.recentFiles = [], C.populatedRecentFilesFolder = [], C.videoName = 0, C.folderIndex = 0, C.folder = "", C
        .fileIndex = 0, C.showRecent = !1, C.recentLines = 0, C.recentFileIndex = "", C.buttonText = "", C
        .contentHeight = "600px", C.outlineHeight = "630px", C.initialized = !1, C.abortLoading = !1, C.rowSize = 4,
        C.maxRows = 4, C.disableRemoveButton = !0, C.disableUploadButton = !1, C.disableBackButton = !1, C
        .disableLocationButton = !0, C.currentFolderVideos = 0, C.currentFolderImages = 0, C.foundItems = !1, C
        .enableFilter = !1, C.topRow = 0, C.displayFileString = "", C.monitoringFirstLoad = !1, C
        .numMonitoredFiles = 0, C.isConnectEnabled = !1;
      var O = n.getInstance("osc/GalleryFilesMenuController");
      C.filterData = "", C.setfilterDataPreText = function() {
        C.filterData = {
          preText: C.videoName,
          enableFilter: C.enableFilter
        }
      }, C.filterDataChange = function(e) {
        C.item = e.item, C.updateFilterView(), C.enableEscapeEvent(!e.filterOpen), e.filterTabExit === !0 && C
          .processFilterTab()
      }, C.abort = function(e, t) {
        C.abortLoading = e, O.info("Abort: " + e + " from: " + t)
      }, C.upload = function() {
        u.startPerf(y.uploadScreen), C.abort(!0, "Upload Button"), l.setParentGalleryState(!0), t.go(
          "main.gallery.upload", {
            callback: function(e) {
              u.endPerfAfterDigest(y.uploadScreen, e)
            }
          })
      }, C.remove = function() {
        if (!C.disableRemoveButton) {
          if (C.abort(!0, "Remove Button"), void 0 === C.file && (C.file = C.files[0]), !C.file) return;
          l.setFileToProcess(C.file), t.go("main.gallery.remove")
        }
      }, C.uploadHistory = function() {
        t.go("main.gallery.history")
      }, C.openLocation = function() {
        var e = C.displayFileString;
        if (C.isDirectoryView() && "" === C.fileIndex || !C.isDirectoryView()) {
          var t = C.displayFileString.lastIndexOf("\\");
          t != -1 && (e = C.displayFileString.slice(0, t))
        }
        O.error("OpenLocation: ", e), "" !== e && (a.closeOSC(), f.browseDirectory(e))
      }, C.back = function() {
        C.disableBackButton = !0, C.abort(!0, "Back Button");
        var e = l.getGalleryState();
        e === g.DIRECTORY || e === g.EMPTY ? (l.resetGalleryState(), t.go("main.main-menu")) : C
          .loadDirectoryView().then(function() {
            C.disableBackButton = !1, C.disableUploadButton = !1, "" !== C.recentFileIndex ? C
              .disableRemoveButton = !1 : C.disableRemoveButton = !0
          })
      }, C.selectFolder = function(e, t) {
        C.fileIndex = t, C.displayFileString = e.path + "\\" + e.folder, C.disableLocationButton = !1, C
          .recentFileIndex = "", O.info("SelectFolder - SingleClick or Mouseover function"), C.folderIndex = t, l
          .setCurrentFolderIndex(t), l.setCurrentFolder(e), C.folder = e, C.videoName = e.folder, C
          .setfilterDataPreText(), l.setCurrentFileIndex(0), l.setRecentFileIndex(-1), C.disableRemoveButton = !0,
          C.buttonText = "l10n.open", C.disableUploadButton = !1
      }, C.highlightFile = function() {
        var e = l.getCurrentFileIndex();
        if (e >= 0 && e < C.files.length)
          if (C.filteredView.getFilteredIndex(e) > 0) {
            var t = C.files[e];
            C.selectFile(t, e, !1)
          } else {
            var t = C.filteredView.getFirstItem();
            t ? C.selectFile(t, t.index, !1) : (C.displayFileString = "", C.disableLocationButton = !0, C
              .disableRemoveButton = !0)
          }
      };
      var A = function() {
        this.filteredMap = [], this.loadedPages = {}, this.numItems = 0, this.PAGE_SIZE = 4
      };
      A.prototype.getItemAtIndex = function(e) {
        var t = this.filteredMap[e].index,
          n = Math.floor(t / this.PAGE_SIZE),
          i = this.loadedPages[n];
        if (i) {
          if (i.length > 0) return i[t % this.PAGE_SIZE]
        } else null !== i && this.fetchPage_(n);
        return C.files[t]
      }, A.prototype.getLength = function() {
        return this.numItems
      }, A.prototype.filterList = function() {
        this.filteredMap = r.filter(C.files, function(e) {
          return C.checkVisibility(e.file)
        }), this.numItems = this.filteredMap.length, this.numItems > 0 && (C.foundItems = !0)
      }, A.prototype.getFirstItem = function() {
        return this.filteredMap.length > 0 ? C.files[this.filteredMap[0].index] : null
      }, A.prototype.getLastItem = function() {
        return this.numItems > 0 ? C.files[this.filteredMap[this.numItems - 1].index] : null
      }, A.prototype.getItemAtOffset = function(e, t) {
        var n = this.getFilteredIndex(e);
        return n < 0 ? null : n + t < 0 || n + t >= this.numItems ? null : C.files[this.filteredMap[n + t].index]
      }, A.prototype.getFilteredIndex = function(e) {
        return r.findIndex(this.filteredMap, {
          index: e
        }, !0)
      }, A.prototype.scrollTo = function(e) {
        var t = this.getFilteredIndex(e);
        if (!(t < 0)) {
          var n = Math.floor(t / C.rowSize),
            i = C.maxRows / 2;
          if (C.topRow < n - i || C.topRow > n - 2 * i) {
            var o = n - i,
              r = Math.floor((this.numItems - 1) / C.rowSize) - C.maxRows + 1;
            C.topRow = Math.max(0, Math.min(o, r))
          }
        }
      }, A.prototype.fetchPage_ = function(e) {
        this.loadedPages[e] = null;
        var t = e * this.PAGE_SIZE;
        c.getFolderData(C.useFolder, t, this.PAGE_SIZE).then(angular.bind(this, function() {
          this.loadedPages[e] = [];
          for (var n = t + this.PAGE_SIZE, i = t; i < n && i < C.files.length; i++) C.files[i] = c
            .getPopulateFolderReference()[i], this.loadedPages[e].push(C.files[i]), C.numMonitoredFiles++,
            i === C.fileIndex && C.selectFile(C.files[i], i, !1);
          if (C.monitoringFirstLoad) {
            var o = C.numMonitoredFiles >= 32;
            if (!o) {
              o = !0;
              for (i in this.loadedPages)
                if (!this.loadedPages[i]) {
                  o = !1;
                  break
                }
            }
            o && (C.monitoringFirstLoad = !1, E(y.galleryPopulateFiles, C.numMonitoredFiles, y
              .galleryFullLoad))
          }
        }))
      }, C.openFolderDelayed = function() {
        return C.items = C.useFolder.videos + C.useFolder.screenshots, C.filteredView = new A, C
          .currentFolderVideos = C.useFolder.videos, C.currentFolderImages = C.useFolder.screenshots, O.info(
            "OpenFolder items:" + C.items), c.getRawFolderData(C.useFolder, !0).then(function() {
            var e = c.getFilesReference();
            C.items = e.length, C.files = [], C.files = r.map(e, function(e, t) {
                return {
                  file: e,
                  index: t
                }
              }), C.filteredView.filterList(), l.setCurrentFolder(C.useFolder), C.enableFilter = !0, C
              .setfilterDataPreText(), C.controlButtons(), C.highlightFile(), C.updateFilterView()
          })
      }, C.openFolder = function(e, t) {
        u.startPerf(y.galleryPopulateFiles), C.monitoringFirstLoad = !0, C.numMonitoredFiles = 0, O.info(
            "OpenFolder - DoubleClick or Button function"), l.setGalleryState(g.FILES), C.foundItems = !1, C
          .folderIndex = void 0 === t ? C.folderIndex : t, C.useFolder = void 0 === e ? C.folder : e;
        var n = C.useFolder.path + "\\" + C.useFolder.folder;
        return c.checkCachedData(n).then(function() {
          return C.openFolderDelayed()
        }).then(function() {
          O.info("OpenFolder - Complete"), E(y.galleryPopulateFiles, C.files.length, y.galleryFirstScreen)
        })
      }, C.isActiveFolderIndex = function(e) {
        return C.folderIndex === e
      }, C.checkIfUploadUnsupported = function(e) {
        return !!e && (void 0 !== e.audiotype && (e.audiotype === b.SEPARATE || e.file.subtype === x.EXR || e.file
          .subtype == x.SUPER_RESOLUTION_OVERSIZED))
      }, C.selectFile = function(e, t, n) {
        C.file = e, C.displayFileString = e.fullFilename, C.disableLocationButton = !1, O.info(" selectFile : ",
          e), l.setHevc(e.hevc), n ? (C.fileIndex = "", C.recentFileIndex = t, l.setCurrentFolderIndex(0), l
          .setRecentFileIndex(t), C.folderIndex = "", C.disableRemoveButton = !1, C.buttonText = "l10n.upload",
          C.disableUploadButton = C.checkIfUploadUnsupported(C.file) || !C.isConnectEnabled) : (C.fileIndex = t,
          l.setCurrentFileIndex(t), C.recentFileIndex = "", C.filteredView.scrollTo(t), C.controlButtons())
      }, C.openFile = function(e) {
        C.disableUploadButton || (e && (C.file = e), void 0 === C.file && (C.file = C.files[0]), C.file && (u
          .push(v.OSC_GALLERY_FILE_OPEN), l.setFileToProcess(C.file), C.upload()))
      }, C.open = function() {
        "" === C.fileIndex ? C.openFile() : C.openFolder()
      }, C.isActiveFileIndex = function(e) {
        return C.fileIndex === e
      }, C.isActiveRecentFileIndex = function(e) {
        return C.recentFileIndex === e
      }, C.updateFilterView = function() {
        if (void 0 !== C.filteredView) {
          C.foundItems = !1, C.filteredView.filterList(), C.controlButtons();
          var e = C.filteredView.getFirstItem();
          e ? C.selectFile(e, e.index, !1) : C.displayFileString = ""
        }
      }, C.controlButtons = function() {
        return C.foundItems && C.file && C.file.fullFilename ? (C.disableRemoveButton = !1, C
          .disableUploadButton = C.checkIfUploadUnsupported(C.file), void(C.disableLocationButton = !1)) : (C
          .disableRemoveButton = !0, C.disableUploadButton = !0, void(C.disableLocationButton = !0))
      }, C.isEmpty = function() {
        return l.getGalleryState() === g.EMPTY
      }, C.isDirectoryView = function() {
        return l.getGalleryState() === g.DIRECTORY
      }, C.isFilesView = function() {
        return l.getGalleryState() === g.FILES
      }, C.checkVisibility = function(e) {
        return void 0 !== C.item && (1 === C.item.id || (2 === C.item.id && e.type === h.VIDEO || (10 === C.item
          .id && e.subtype === x.MTA || (5 === C.item.id && e.subtype === C.item.subtype || "undefined" !==
            e.type && e.type === h.IMAGE && (e.subtype === C.item.subtype || 3 === C.item.id && e
              .subtype === x.HIGHLIGHTS || 3 === C.item.id && e.subtype === x.NORMAL_ANSEL || 6 === C.item
              .id && e.subtype === x.SUPER_RESOLUTION_OVERSIZED)))))
      }, C.setHighlightOnDirectoryView = function() {
        var e = l.getRecentFileIndex();
        if (0 === C.recentLines || e === -1) {
          var t = l.getCurrentFolder();
          if (e = 0, "" !== t && (e = r.findIndex(C.folders, {
              folder: t.folder
            }), e === -1 && (e = 0)), e < C.folders.length) {
            var n = C.folders[e];
            C.selectFolder(n, e)
          }
        } else if (e < C.populatedRecentFilesFolder.length && e >= 0) {
          var i = C.populatedRecentFilesFolder[e];
          C.selectFile(i, e, !0)
        }
      }, C.loadDirectoryView = function() {
        return u.startPerf(y.galleryPopulateFolders), d.getRecordingPaths().then(function(e) {
          return c.setRecordingPaths(e)
        }).then(function() {
          return c.getRecentFiles()
        }).then(function() {
          return c.checkCachedData()
        }).then(function() {
          return C.recentFiles = c.recentFiles, C.populatedRecentFilesFolder = c.populatedRecentFilesFolder, C
            .showRecent = 0 !== C.populatedRecentFilesFolder.length, C.recentLines = 0 === C
            .populatedRecentFilesFolder.length ? 0 : C.populatedRecentFilesFolder.length <= C.rowSize ? 1 : 2,
            c.getUsableData(!0)
        }).then(function(e) {
          if (e) {
            C.folders = c.populatedDirs, C.setHighlightOnDirectoryView();
            var t = [620, 450, 300];
            C.contentHeight = t[C.recentLines] + "px";
            var n = [630, 450, 280];
            C.outlineHeight = n[C.recentLines] + "px", l.setGalleryState(g.DIRECTORY)
          } else l.setGalleryState(g.EMPTY);
          E(y.galleryPopulateFolders, C.folders.length, y.galleryFirstScreen)
        }).then(function(e) {
          return c.getUsableData(!1)
        }).then(function(e) {
          C.disableUploadButton = !1, C.abort(!1, "loadDirectoryView")
        }).then(function(e) {
          E(y.galleryPopulateFolders, C.folders.length, y.galleryFullLoad)
        })
      }, C.loadFilesView = function() {
        C.folder = l.getCurrentFolder(), C.videoName = C.folder.folder, C.setfilterDataPreText();
        var e = C.folder.path + "\\" + C.videoName;
        c.getFolderStats(e).then(function(e) {
          e.videos + e.screenshots === 0 ? C.loadDirectoryView() : (C.openFolder(C.folder, C.folder.index), C
            .currentFolderVideos = e.videos, C.currentFolderImages = e.screenshots, C.controlButtons())
        }).then({}, function() {
          C.loadDirectoryView()
        })
      }, C.enableEscapeEvent = function(e) {
        var t = e;
        o(function() {
          t === !0 ? s.on(m.ESCAPE, C.back) : s.off(m.ESCAPE, C.back)
        }, 200)
      }, C.keyDownRecent = function(e) {
        if (_(e) && 0 !== C.populatedRecentFilesFolder.length) {
          var t = C.populatedRecentFilesFolder[0];
          C.selectFile(t, 0, !0)
        }
      }, C.keyDownOpen1 = function(e) {
        if (k(e) && 0 !== C.folders.length) {
          var t = C.folders[C.folders.length - 1];
          C.selectFolder(t, C.folders.length - 1)
        }
      }, C.keyDownOpen2 = function(e) {
        k(e) && (useFile = C.filteredView.getLastItem(), useFile && C.selectFile(useFile, useFile.index, !1))
      }, C.processFilterTab = function() {
        var e = C.filteredView.getFirstItem();
        e && C.selectFile(e, e.index, !1)
      }, C.keyDown = r.throttle(function(e, t, n, o) {
        if (O.debug("keyDown keyCode: " + n.keyCode), n.keyCode === p.TAB) {
          var r;
          if (2 === o ? r = n.shiftKey ? "button_filter" : "button_upload" : 1 === o ? r = n.shiftKey ?
            "recent" : "button_open" : 0 === o && (r = n.shiftKey ? "button_close" : "folders"),
            "button_upload" === r && C.disableUploadButton && (r = "button_openLoc"), angular.element(i[0]
              .getElementById(r)).focus(), "recent" === r) {
            if (0 !== C.populatedRecentFilesFolder.length) {
              var a = C.populatedRecentFilesFolder.length - 1,
                s = C.populatedRecentFilesFolder[a];
              C.selectFile(s, a, !0)
            }
          } else if ("folders" === r && C.folders.length > 0) {
            var d = C.folders[0];
            C.selectFolder(d, 0)
          }
          n.preventDefault(), n.stopImmediatePropagation(), n.stopPropagation()
        } else if (n.keyCode === p.ENTER) 1 === o ? C.openFolder(e, t) : C.openFile(e);
        else if (n.keyCode >= p.LEFT_ARROW && n.keyCode <= p.DOWN_ARROW) {
          var c, u;
          if (1 === o) c = l.getCurrentFolder().index, u = C.folders;
          else if (0 === o) c = l.getRecentFileIndex(), u = C.populatedRecentFilesFolder;
          else {
            if (2 !== o) return;
            c = l.getCurrentFileIndex()
          }
          var f = 0;
          if (n.keyCode === p.RIGHT_ARROW ? f = 1 : n.keyCode === p.LEFT_ARROW ? f = -1 : n.keyCode === p
            .UP_ARROW ? f = 0 - C.rowSize : n.keyCode === p.DOWN_ARROW && (f = C.rowSize), 2 === o) return e = C
            .filteredView.getItemAtOffset(c, f), void(e && C.selectFile(e, e.index, !1));
          var m = c + f;
          if (m < 0 || m > u.length - 1) return;
          1 === o ? (e = u[m], C.selectFolder(e, m)) : (e = u[m], C.selectFile(e, m, !0))
        }
      }, 100), C.initialize = function() {
        O.info("Initialize Gallery Files VM"), w.isConnectEnabled().then(function(e) {
          C.isConnectEnabled = e, s.on(S.PIPL_CONFIG_UPDATED, T), C.initGallery()
        })
      }, C.initGallery = function() {
        C.disableUploadButton = !0;
        var e = l.getGalleryState();
        e === g.DIRECTORY || e === g.EMPTY ? C.loadDirectoryView().then({}, function() {
            l.setGalleryState(g.EMPTY)
          }) : C.loadFilesView(), C.enableEscapeEvent(!0), C.initialized = !0, l.resetVideoParams(), l
          .resetOutputData(), c.active(!0)
      }, C.initialize(), e.$on("$destroy", function() {
        c.active(!1), C.abort(!0, "$destroy"), C.enableEscapeEvent(!1), s.off(S.PIPL_CONFIG_UPDATED, T), C
          .initialized = !1
      })
    }
  ]);
  t.GalleryFilesMenuController = l
}
