// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 260
// controller FolderBrowserController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.FolderBrowserController = void 0;
  var o = require(3),
    r = i(o),
    a = require(1) /* app/1 — main (module) */;
  require(25) /* app/25 — hardwareService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(11);
  var l = a.ngMainModule.controller("FolderBrowserController", ["$q", "$scope", "$state", "$stateParams",
    "$log", "$filter", "$document", "hardwareService", "eventAggregator", "shadowPlayService",
    "galleryService", "RECORDING_PATH_TYPES", "KEYBOARD_EVENTS", "OSC_KEYBOARD",
    function(e, t, n, i, o, a, l, s, d, c, u, f, m, g) {
      function p(e) {
        var t = void 0;
        i && i.currentPath ? t = i : e && e.currentPath ? t = e : (O.error(
            "Folder browser had an invalid current path. Likely due to registry corruption. Attempting to continue."
            ), t = e || i), T.parentView = t.parentView, T.heading = t.heading, T.callbackFunction = t
          .callback, T.initialVal = t.init, T.callbackParam = t.callbackParam, T.currentPath = t
          .currentPath, T.pathType = t.pathType, T.includeFiles = t.includeFiles || !1, T.match = t
          .match || ".png", T.pathType || (T.showWarningMessage = !1);
      }

      function h() {
        T.abort = !0, T.parentView ? n.go(T.parentView) : T.callbackFunction && T.callbackFunction(void 0,
          T.callbackParam);
      }

      function b(e) {
        var t = e;
        return T.pathType !== f.TEMP_FILES && T.pathType !== f.HIGHLIGHTS || t.endsWith("\\") || (t +=
          "\\"), t;
      }

      function x(e) {
        var t = e;
        return T.pathType !== f.TEMP_FILES && T.pathType !== f.HIGHLIGHTS || !t.endsWith("\\") || (t = t
          .slice(0, t.length - 1)), t;
      }

      function v(e) {
        return !!e.startsWith("$");
      }

      function y(e, t) {
        var n = e,
          i = n.split("\\");
        return 1 === i.length;
      }

      function w(e) {
        switch (e) {
          case "network":
            return "icon-browser_disk_drive_network";
          case "removable":
            return "icon-browser_disk_drive_removable";
          default:
            return "icon-browser_disk_drive";
        }
      }

      function S(e, t) {
        r.find(T.dirFolderData, function(n) {
          return n.folder === e && (n.src = t, n.icon = null, !0);
        });
      }

      function E(t) {
        if (T.thumbnailList.length <= 0 || T.abort || T.loadCount !== t) return e.reject("");
        var n = I,
          i = e.defer(),
          o = [];
        return r.each(r.first(T.thumbnailList, n), function(e) {
          var t = T.currentPath + "\\" + e.folder;
          o.push(u.getGalleryThumbnail(t).then(function(t) {
            S(e.folder, t.thumbnail);
          }));
        }), T.thumbnailList = r.rest(T.thumbnailList, n), e.all(o).then(function() {
          i.resolve(!0);
        }), i.promise;
      }

      function k(e) {
        E(e).then(function() {
          k(e);
        });
      }

      function _(e) {
        if (!T.includeFiles && !T.dirFolderData[e].isFile) {
          var t = T.currentPath + "\\" + T.dirFolderData[e].folder;
          u.isDirectoryWritable(t).then(function(n) {
            T.dirFolderData[e] && t === T.currentPath + "\\" + T.dirFolderData[e].folder && (T
              .dirFolderData[e].isReadOnly = !n);
          });
        }
      }
      var T = this;
      T.title = "l10n.settings", T.icon = "icon-settings", T.status = "", T.showWarningMessage = !0;
      var C,
        O = o.getInstance("osc/RecordingsFolderBrowserController"),
        A = "TOP_LEVEL_DRIVES",
        I = 12;
      T.currentPath = "", T.currentFolder = "", T.pathType = "", T.dirFolderData = [], T
        .thumbnailList = [], T.focusedFolderID = -1, T.hoveredFolderID = -1, T.activeFolderID = -1, T
        .abort = !1, T.loadCount = 0, T.rowSize = 5, T.displayWarning = !1, T.doneDisable = !1;
      var M = 4096,
        R = 4096;
      T.initialize = function(e) {
        p(e);
        var t = T.currentPath,
          n = T.pathType;
        return n === f.VIDEOS || n === f.TEMP_FILES || n === f.HIGHLIGHTS || n === f.FILE_LOGGING || T
          .includeFiles ? ("" === t && (O.error("ERROR, invalid current path"), t = "C:\\"), T
            .pathType = n, T.currentPath = x(t), void T.loadData(!0)) : (O.error(
            "ERROR, invalid path type"), void h());
      }, T.reload = function() {
        T.dirFolderData = [], T.thumbnailList = [], T.focusedFolderID = -1, T.hoveredFolderID = -1, T
          .activeFolderID = -1, T.loadCount++, T.loadData();
      }, T.loadData = function(e) {
        function t(e) {
          var t = e.name.toLowerCase();
          return t = t.slice(t.lastIndexOf(".")), T.match.indexOf(t) >= 0;
        }
        O.info("Current path: " + T.currentPath + ", path type: " + T.pathType);
        var n = T.loadCount;
        if (T.currentPath === A) return s.getSystemInfo().then(function(e) {
          C = e.PCName;
        }), void u.enumerateDrives().then(function(e) {
          T.loadCount === n && r.each(e.drives, function(e, t) {
            var n = {};
            n.index = t, n.folder = e.name, n.icon = w(e.type), n.isFile = !1, T.dirFolderData
              .push(n);
          });
        }, function(e) {
          O.error("ERROR when enumerating drives");
        });
        var i = !T.includeFiles,
          o = u.EXCLUDE_HIDDENANDEMPTY,
          l = "",
          d = 0,
          c = T.currentPath;
        !T.includeFiles && e && (c = T.currentFolder = T.currentPath.slice(0, T.currentPath.lastIndexOf(
              "\\")), l = T.currentPath.slice(T.currentPath.lastIndexOf("\\") + 1), T.currentPath = T
            .currentFolder, T.currentFolder = T.currentPath.slice(0, T.currentPath.lastIndexOf("\\"))),
          u.getGalleryFolderListing(c, !1, i, o).then(function(e) {
            if (T.loadCount === n) {
              var i = 0;
              if (r.each(e.directories, function(e) {
                  if (!v(e)) {
                    var t = {};
                    t.index = i++, t.folder = e, t.icon = "icon-browser_folder", t.isFile = !1, t
                      .isReadOnly = !1, T.dirFolderData.push(t), e === l && (d = t.index);
                  }
                }), !T.includeFiles) return void(i > 0 && T.singleClickFolder(d));
              r.each(e.files, function(e) {
                if (t(e)) {
                  var n = {};
                  n.index = i++, n.folder = e.name, n.icon = "icon-browser_file_png", n.isFile = !
                    0, n.isFileInvalid = !1, n.warningText = void 0;
                  var o = T.currentPath + "//" + e.name;
                  u.getGalleryImageFileDimensions(o).then(function(e) {
                      (e.width > M || e.height > R) && (n.isFileInvalid = !0, n.warningText = a(
                        "translate")("l10n.fileTooLargeWarning", {
                        width: M,
                        height: R
                      }));
                    }, function() {
                      n.isFileInvalid = !0, n.warningText = "l10n.fileCorruptWarning";
                    }), T.dirFolderData.push(n), T.thumbnailList.push(n), e.name === T
                    .initialVal && (T.focusedFolderID = i - 1, T.activeFolderID = i - 1);
                }
              }), T.thumbnailList.length > 0 && k(n);
            }
          }, function(e) {
            O.error("ERROR when retrieving directory listing");
          });
      }, T.getRecordingsPathTitle = function() {
        return T.heading;
      }, T.getCurrentPath = function(e) {
        return T.currentPath === A ? C : T.currentPath;
      }, T.isActiveFolder = function(e) {
        return e === T.focusedFolderID || e === T.activeFolderID;
      }, T.isInvalidFolder = function(e) {
        return !!(!T.dirFolderData[e] || T.dirFolderData[e].isFile && T.dirFolderData[e].isFileInvalid);
      }, T.singleClickFolder = function(e) {
        T.activeFolderID = e, T.isInvalidFolder(e) || (T.focusedFolderID = e, _(e));
      }, T.doubleClickFolder = function(e) {
        T.activeFolderID = e, T.isInvalidFolder(e) || (T.focusedFolderID = e, T.dirFolderData[e]
          .isFile ? T.selectFolder() : T.openFolder());
      }, T.goUpOneFolder = function() {
        if (T.currentPath !== A) {
          if (y(T.currentPath, T.pathType)) return T.currentPath = A, void T.reload();
          T.includeFiles ? T.currentPath = T.currentPath.slice(0, T.currentPath.lastIndexOf("\\")) : (T
            .currentPath = T.currentFolder, T.currentFolder = T.currentPath.slice(0, T.currentPath
              .lastIndexOf("\\"))), T.reload();
        }
      }, T.openFolder = function() {
        if (T.focusedFolderID !== -1) {
          var e = "";
          T.currentPath !== A && (e = T.currentPath + "\\"), e += T.dirFolderData[T.focusedFolderID]
            .folder, T.currentPath === A && (e = e.split("\\")[0]), T.currentPath = e, T.currentFolder =
            T.currentPath.slice(0, T.currentPath.lastIndexOf("\\")), T.reload();
        }
      }, T.selectFolder = function(e) {
        if (!(T.currentPath === A || T.includeFiles && T.isInvalidFolder(T.focusedFolderID))) {
          var t = T.currentPath;
          if (T.focusedFolderID !== -1 && (t = t + "\\" + T.dirFolderData[T.focusedFolderID].folder), !T
            .includeFiles || T.focusedFolderID !== -1 && T.dirFolderData[T.focusedFolderID].isFile) {
            if (T.includeFiles || T.pathType === f.HIGHLIGHTS) return void(r.isUndefined(T
              .callbackFunction) ? h() : T.pathType === f.HIGHLIGHTS ? T.callbackFunction(t, T
              .callbackParam) : T.callbackFunction(t, T.callbackParam).then(function() {
              h();
            }));
            t = b(t), T.pathType === f.FILE_LOGGING && (T.callbackFunction(t, T.callbackParam), h()), c
              .getRecordingPaths().then(function(e) {
                var n = T.pathType === f.VIDEOS ? e.videos : e.tempFiles;
                if (t === n) O.info("Old " + T.pathType +
                  " recording path and new one are the same: " + t), h();
                else {
                  var i = T.pathType === f.VIDEOS ? t : e.videos,
                    o = T.pathType === f.TEMP_FILES ? t : e.tempFiles;
                  c.setRecordingPaths(i, o).then(function(e) {
                    O.info("Setting " + T.pathType + " recording path to " + t), h();
                  }, function(e) {
                    O.error("ERROR, cannot set recording paths"), h();
                  });
                }
              }, function(e) {
                O.error("ERROR, cannot get recording paths"), h();
              });
          }
        }
      }, T.hoverFolder = function(e) {
        T.hoveredFolderID = e, T.displayWarning = !1, T.isInvalidFolder(e) && (T.displayWarning = !0, T
          .warningText = T.dirFolderData[e].warningText);
      }, T.keyDownNav = r.throttle(function(e) {
        O.debug("keyDownNav keyCode: " + e.keyCode), e.keyCode === g.TAB && e.shiftKey === !1 && T
          .focusedFolderID === -1 && (T.focusedFolderID = 0);
      }, 100), T.keyDownDone = r.throttle(function(e) {
        O.debug("keyDownDone keyCode: " + e.keyCode), e.keyCode === g.TAB && e.shiftKey === !0 && T
          .focusedFolderID === -1 && (T.focusedFolderID = 0);
      }, 100), T.dontPropagateEvent = function() {
        event.preventDefault(), event.stopImmediatePropagation(), event.stopPropagation();
      }, T.keyDown = r.throttle(function(e, t, n) {
        if (O.debug("keyDown keyCode: " + n.keyCode), n.keyCode === g.TAB) {
          var i = n.shiftKey ? "button_navup" : "button_done";
          angular.element(l[0].getElementById(i)).focus(), T.dontPropagateEvent(n);
        } else if (n.keyCode === g.ENTER) T.doubleClickFolder(t), T.focusedFolderID = -1, angular
          .element(l[0].getElementById("button_done")).focus(), T.dontPropagateEvent(n);
        else if (n.keyCode >= g.LEFT_ARROW && n.keyCode <= g.DOWN_ARROW) {
          var o = 0;
          n.keyCode === g.RIGHT_ARROW ? o = 1 : n.keyCode === g.LEFT_ARROW ? o = -1 : n.keyCode === g
            .UP_ARROW ? o = 0 - T.rowSize : n.keyCode === g.DOWN_ARROW && (o = T.rowSize);
          var r = T.activeFolderID + o;
          if (r < 0 || r > T.dirFolderData.length - 1) return;
          T.isInvalidFolder(r) ? (T.displayWarning = !0, T.warningText = T.dirFolderData[r]
            .warningText) : T.displayWarning = !1, T.singleClickFolder(r);
        }
      }, 100), T.cancel = function() {
        h();
      }, T.isDoneDisabled = function() {
        return T.includeFiles ? !T.dirFolderData[T.focusedFolderID] || !T.dirFolderData[T
            .focusedFolderID].isFile || T.hoveredFolderID >= 0 && T.isInvalidFolder(T
          .hoveredFolderID) || T.isInvalidFolder(T.activeFolderID) ? T.doneDisable = !0 : T
          .doneDisable = !1 : T.currentPath === A ? T.doneDisable = !0 : T.focusedFolderID >= 0 && T
          .dirFolderData[T.focusedFolderID].isReadOnly ? T.doneDisable = !0 : T.doneDisable = !1, T
          .doneDisable;
      }, d.on(m.ESCAPE, T.cancel), t.$on("$destroy", function() {
        d.off(m.ESCAPE, T.cancel);
      });
    }
  ]);
  exports.FolderBrowserController = l;
}
