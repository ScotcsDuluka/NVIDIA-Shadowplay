// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 190
// controller GalleryUploadMenuController
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

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.GalleryUploadMenuController = void 0;
  var r = require(8),
    a = o(r),
    l = require(3),
    s = i(l),
    d = require(1) /* app/1 — main (module) */;
  require(11), require(25) /* app/25 — hardwareService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(23) /* app/23 — oscNotificationService (service) */, require(48) /* app/48 — oscService (provider) */, require(46) /* app/46 — errorDialogService (service) */;
  var c = d.ngMainModule.controller("GalleryUploadMenuController", ["$q", "$scope", "$state", "$log",
    "$timeout", "$filter", "$stateParams", "sdkService", "eventAggregator", "galleryService",
    "connectService", "uploadService", "oscGalleryService", "ugcService", "oscDisplayService",
    "oscNotificationService", "telemetryService", "shadowPlayService", "errorDialogService", "oscService",
    "galleryEndpoints", "KEYBOARD_EVENTS", "NOTIFIER_SELECTIONS", "COMMON_EVENTS", "GALLERY_TYPES",
    "GALLERY_SUBTYPES", "GALLERY_FILEPROCESS", "piplConfigService", "HIGHLIGHTS_EVENTS",
    "TELEMETRY_OSC_EVENT_NAMES", "TELEMETRY_OSC_AVAILABLE_STATUS",
    function(e, t, n, i, o, r, l, d, c, u, f, m, g, p, h, b, x, v, y, w, S, E, k, _, T, C, O, A, I, M,
    R) {
      function P(e, t) {
        e.fileName = t, e.fullFilename = t.replace(/\//g, "\\"), m.uploadContent(e), g
          .pushToTempStackForDeletion(e);
      }

      function D() {
        g.resetVideoParams(), b.show(k.UPLOAD_FAILED, B.uploadService.name);
      }

      function N(e, t) {
        var n = B.videoSrc,
          i = n.lastIndexOf(".");
        i >= 0 && (n = n.slice(0, i));
        var o = Date.now();
        n += "-" + Math.floor(e.startMs / 1e3) + "-" + Math.floor(e.durationMs / 1e3) + "-" + o + ".mp4",
          v.trimVideo(B.videoSrc, n, e.startMs, e.durationMs).then(function() {
            P(t, n);
          }, function(e) {
            s.isUndefined(e) || s.isNull(e) || $.info("Video trim failure: " + e.data), D();
          });
      }

      function L(e) {
        var t = B.videoSrc,
          n = t.lastIndexOf(".");
        n >= 0 && (t = t.slice(0, n)), t += "-" + Date.now() + ".mp4", u.copyFile(B.videoSrc, t).then(
          function(n) {
            n ? P(e, t) : ($.info("copyFile failure, aborting upload"), D());
          });
      }

      function F(e) {
        var t = /.*[\\\/](.*)/,
          n = t.exec(e);
        return n[1];
      }

      function U(e, t, n) {
        return v.getRecordingPaths().then(function(i) {
          var o = i.tempFiles,
            r = i.tempFiles + F(e),
            a = r.lastIndexOf(".");
          return a >= 0 && (r = r.slice(0, a)), r += "-" + Math.floor(t) + "-" + Math.floor(n) + "-" +
            Date.now() + ".mp4", {
              tempFileName: r,
              tempFilePath: o
            };
        });
      }

      function z(t) {
        return S.transcodeVideoToGIF({}, t).then(function(t) {
          return t && t.data && t.data.errorString && "" !== t.data.errorString ? e.reject(t) : t;
        });
      }

      function G(t, n) {
        U(n.fileName, t.startMs / 1e3, t.durationMs / 1e3).then(function(i) {
          v.trimVideo(n.fileName, i.tempFileName, t.startMs, t.durationMs).then(function() {
            g.resetVideoParams(), n.conversionStartTime = x.startTimer(M
              .OSC_GIF_CONVERSION_ATTEMPT), n.uploadServiceName = n.uploadService.name;
            var o = Math.ceil(3 * t.durationMs / 1e3);
            B.uploadService.gifOptions && void 0 !== B.uploadService.gifOptions.maxFileSizeMB && (
              o = Math.min(o, B.uploadService.gifOptions.maxFileSizeMB));
            var r = "high";
            B.uploadService.gifOptions && B.uploadService.gifOptions.fpsLevel && (r = B
              .uploadService.gifOptions.fpsLevel);
            var l = 0;
            return B.uploadService.gifOptions && void 0 !== B.uploadService.gifOptions
              .maxHeight && (l = B.uploadService.gifOptions.maxHeight), "" !== B.memeImage ? (B
                .uploadService.gifOptions && B.uploadService.gifOptions.maxFileSizeMB && (o = B
                  .uploadService.gifOptions.maxFileSizeMB), B.uploadService.gifOptions &&
                void 0 !== B.uploadService.gifOptions.memeHeight && (l = B.uploadService
                  .gifOptions.memeHeight), $.info("Transcoding " + n.fileName +
                  " to target resolution: " + l + "p and max size: " + o + "MB for upload to " + n
                  .uploadService.name)) : $.info("Transcoding " + n.fileName +
                " to target file size: " + o + "MB for upload to " + n.uploadService.name), n
              .uploadService = void 0, z({
                file: i.tempFileName,
                maxFileSizeMB: o,
                quality: "medium",
                newHeight: l,
                newFps: r,
                targetPath: i.tempFilePath,
                newDuration: Math.round(t.durationMs / 1e3),
                userData: (0, a.default)(n),
                memeImage: B.memeImage
              }).then(function(t) {
                S.removeGalleryItem({}, {
                  file: i.tempFileName,
                  forceDelete: !0
                });
                var n = t.data;
                $.info("Transcoded video '" + n.newFile + "' for upload to " + n.userData +
                  ". New dimensions: " + n.newFileWidth + "x" + n.newFileHeight +
                  ". New filesize: " + n.newFileSizeB);
                var o = JSON.parse(n.userData);
                return o.fileName ? (o.fileName = n.newFile, o.uploadService = f.services[o
                    .uploadServiceName], o.cleanupPromise = e.defer(), o.cleanupPromise
                  .promise.then(function() {
                    S.removeGalleryItem({}, {
                      file: n.newFile,
                      forceDelete: !0
                    });
                  }), m.uploadContent(o), x.endTimer(M.OSC_GIF_CONVERSION_ATTEMPT, {
                    info: {
                      conversionIterations: n.numIterations,
                      convertedFileSize: n.newFileSizeB,
                      containsMeme: o.containsMeme,
                      topMemeLength: o.topMemeLength,
                      bottomMemeLength: o.bottomMemeLength,
                      width: n.newFileWidth,
                      height: n.newFileHeight,
                      errorString: "",
                      DRSName: o.DRSName || "",
                      DRSProfileName: o.DRSProfileName || ""
                    },
                    startTime: o.conversionStartTime
                  }), e.when(n.newFile)) : ($.error("GIF transcode failed, aborting upload."),
                  e.reject());
              }, function(e) {
                S.removeGalleryItem({}, {
                  file: i.tempFileName,
                  forceDelete: !0
                }), s.isUndefined(e) || s.isUndefined(e.data) ? $.error(
                  "GIF transcode failed!") : $.error("GIF transcode failed: " + e.data
                  .codeText + " with: " + e.data.message);
                var t = e.data;
                !t && e.config && (t = e.config.data);
                var n = JSON.parse(t.userData);
                x.endTimer(M.OSC_GIF_CONVERSION_ATTEMPT, {
                  info: {
                    conversionIterations: t.numIterations || 0,
                    convertedFileSize: t.newFileSizeB || 0,
                    containsMeme: n.containsMeme,
                    topMemeLength: n.topMemeLength,
                    bottomMemeLength: n.bottomMemeLength,
                    width: t.newFileWidth || 0,
                    height: t.newFileHeight || 0,
                    errorString: t.errorString,
                    DRSName: n.DRSName || "",
                    DRSProfileName: n.DRSProfileName || ""
                  },
                  startTime: n.conversionStartTime
                });
                var n = JSON.parse(t.userData);
                b.show(k.UPLOAD_FAILED, n.uploadServiceName);
              });
          }, function(e) {
            s.isUndefined(e) || s.isNull(e) || $.info("Video trim failure: " + e.data), g
              .resetVideoParams(), b.show(k.UPLOAD_FAILED, B.uploadService.name);
          });
        }), b.show(k.UPLOAD_STARTED, B.uploadService.title), B.back(!0);
      }

      function V(e) {
        $.info("Connect status:", e.isConnectEnabled), B.isConnectEnabled = e.isConnectEnabled;
      }

      function H(e) {
        $.info("displayEmptyVideo: ", e), B.fileSize = e, B.videoSrc = "";
        var t = {
          name: "",
          type: "video",
          subtype: "",
          source: ""
        };
        B.fileToUpload = {
          file: t
        }, B.serviceType = f.serviceTypes.VIDEO_UPLOAD, B.showDestinationPicker = !1;
      }
      var B = this;
      B.title = "l10n.gallery", B.backButton = "l10n.back", B.icon = "icon-gallery", B.showMoments = !1, B
        .showEmptyMoments = !1, B.sdkVersion = "", B.fileToUpload = "", B.fileIndex = 0, B.selected = "",
        B.populatedMomentsDataValid = !1, B.saveInProgress = !1, B.momentsInitialFetchFetchSize = 8, B
        .showIndex = 0, B.savedFilesList = [], B.isLoggedIn = void 0, B.videoUploadTitle = void 0, B
        .selectedPrivacy = void 0, B.selectedLocationType = void 0, B.selectedLocation = void 0, B
        .serviceType = "", B.uploadService = "", B.momentsLength = 0, B.selectedOutputType = {
          type: T.VIDEO,
          subtype: C.NORMAL
        };
      var Y = null;
      B.openMemeEditor = !1, B.saveMemeEditor = !1, B.cancelMemeEditor = !1, B.uploadGifService = void 0,
        B.pickerData = {}, B.showDestinationPicker = !1, B.isConnectEnabled = !1, B.memeImage = "", B
        .displayEmptyTypes = {
          SPINNER: 0,
          ERROR: -2
        };
      var $ = i.getInstance("osc/GalleryUploadMenuController"),
        W = !1;
      B.destinationDataChange = function(e) {
        B.isLoggedIn = e.isLoggedIn, B.videoUploadTitle = e.uploadTitle, B.selectedPrivacy = e
          .selectedPrivacy, B.selectedLocationType = e.selectedLocationType, B.selectedLocation = e
          .selectedLocation, B.serviceType = e.serviceType, B.uploadService = e.uploadService, B
          .selectedOutputType = e.selectedOutputType, B.validConnection = e.validConnection, B
          .gifMaxDuration = B.uploadService && B.uploadService.gifOptions ? B.uploadService.gifOptions
          .maxDuration : void 0, B.uploadGifService = B.uploadService, j();
      };
      var j = function(e) {
        return void 0 === B.selectedOutputType ? void(W = !1) : void(B.selectedOutputType.subtype !==
          Y && (Y = B.selectedOutputType.subtype, B.selectedOutputType.subtype === C.GIF ? (W = !0, x
            .push(M.OSC_TRIM_GIF, {
              provider: "none"
            }), B.pickerData.warningNoticeText = void 0) : (W = !1, B.openMemeEditor = !1)));
      };
      B.login = function(e) {
        if ($.info("Login from upload controller: ", e.name), n.params.lastMomentIndex = B.fileIndex, B
          .savedFilesList.length > 0 && (s.each(B.savedFilesList, function(e, t) {
            s.each(n.params.moments, function(t, n) {
              t.filename === e.oldFileName && (t.filename = e.newFileName, t.savedToGallery = !
                0);
            });
          }), B.savedFilesList.length = 0), p.checkJarvisLoginRequirement(e.name)) b.show(k
          .CONNECT_LOGIN_TO_GFE);
        else if (p.checkIfBrowserLogin(e.name)) p.logInFromBrowser(e.name, n.current.name, n.params);
        else {
          var t = {
              title: B.title,
              icon: B.icon,
              status: ""
            },
            i = {
              service: e,
              oscTileParams: t,
              lastState: n.current.name,
              lastParams: n.params
            };
          n.go("main.oauth-menu", i);
        }
      }, B.upload = function() {
        if ($.info("Upload Entered"), w.onlineState && w.onlineState.online === !1) return $.info(
          "No Internet connection"), void y.show("l10n.systemRequirement",
          "l10n.notificationCoplayNetworkUnavailable");
        x.push(M.OSC_UPLOAD_ATTEMPT, "type: " + B.fileToUpload.file.type + ", subtype: " + (W ? C.GIF :
          B.fileToUpload.file.subtype));
        var e = x.startTimer(M.OSC_UPLOAD_DATA),
          t = {
            folder: B.fileToUpload.folder,
            fileSize: B.fileSize,
            type: B.fileToUpload.file.type,
            subtype: W ? C.GIF : B.fileToUpload.file.subtype,
            videoUploadTitle: B.videoUploadTitle,
            privacy: B.selectedPrivacy,
            destination: B.selectedLocation,
            fileSource: B.fileToUpload.file.source,
            highlightDefinitionId: B.showMoments ? B.fileToUpload.moment.highlightDefinitionId : "",
            uploadService: B.uploadService,
            uploadStartTime: e,
            DRSName: B.showMoments ? B.fileToUpload.moment.drsName : B.fileToUpload.file.DRSName,
            DRSProfileName: B.showMoments ? B.fileToUpload.moment.drsProfileName : B.fileToUpload.file
              .DRSProfileName
          };
        if ("video" === B.fileToUpload.file.type) {
          f.setLastUsedService(f.serviceTypes.VIDEO_UPLOAD, B.uploadService.name), t.fullFilename = B
            .videoSrc.replace(/\//g, "\\"), t.fileName = B.videoSrc, t.containsMeme = "No", t
            .topMemeLength = 0, t.bottomMemeLength = 0;
          var n = g.getVideoParams();
          $.info("Video trim parameters: " + n.trimmed + " " + n.startMs + " " + n.durationMs), W ? (f
            .setLastUploadedVideoType(f.serviceTypes.GIF_UPLOAD), g.checkForMeme().then(function(e) {
              if (B.memeImage = void 0 === e ? "" : e, t.containsMeme = "" !== B.memeImage ? "Yes" :
                "No", void 0 !== e) {
                var i = g.getMemeLengths();
                t.topMemeLength = i.topMemeLength, t.bottomMemeLength = i.bottomMemeLength;
              }
              G(n, t);
            })) : n.trimmed ? (f.setLastUploadedVideoType(f.serviceTypes.VIDEO_UPLOAD), N(n, t)) : (f
            .setLastUploadedVideoType(f.serviceTypes.VIDEO_UPLOAD), B.showMoments ? L(t) : m
            .uploadContent(t));
        } else f.setLastUsedService(f.serviceTypes.IMAGE_UPLOAD, B.uploadService.name), f
          .setLastUploadedVideoType(f.serviceTypes.IMAGE_UPLOAD), t.fullFilename = B.fileToUpload
          .fullFilename, t.fileName = B.actualImageSrc, t.containsMeme = "No", t.topMemeLength = 0, t
          .bottomMemeLength = 0, m.uploadContent(t);
        B.back(!0);
      }, B.back = function(e) {
        g.resetOutputData(), $.info("BACK: ", e), e && f.setServiceParameters({
          privacy: B.selectedPrivacy,
          destinationType: B.selectedLocationType,
          destination: B.selectedLocation
        }, B.uploadService.name);
        var t = g.checkGalleryState();
        t ? n.go("main.gallery.files") : e !== !1 && null !== e || B.showMoments !== !0 && B
          .showEmptyMoments !== !0 ? e !== !1 && null !== e && B.showMoments !== !1 || n.go(
            "main.main-menu") : h.closeOSC();
      }, B.enableEscapeEvent = function(e) {
        var t = e;
        o(function() {
          t === !0 ? c.on(E.ESCAPE, B.back) : c.off(E.ESCAPE, B.back);
        }, 200);
      }, B.updatePicker = function(e) {
        if (B.showDestinationPicker = !0, void 0 !== e.fullFilename) {
          $.info("updatePicker FileToUpload: ", e.fullFilename), B.fileToUpload = e;
          var t = void 0;
          "video" === B.fileToUpload.file.type ? B.serviceType = f.serviceTypes.VIDEO_UPLOAD | f
            .serviceTypes.GIF_UPLOAD : B.serviceType = f.serviceTypes.IMAGE_UPLOAD, B.pickerData = {
              fileToUpload: B.fileToUpload,
              serviceType: B.serviceType,
              showMoments: B.showMoments,
              warningNoticeText: t
            };
        }
      }, B.selectFile = function(e, t) {
        if (void 0 !== e && e.moment.busy !== !0) return e.moment.error === !0 ? (B.fileIndex = t, B
          .videoHighlight = e.moment.highlightName, void H(B.displayEmptyTypes.ERROR)) : void(
          void 0 !== e.file && ($.info("selectFile: " + e.file.name + " index: " + t), u
            .getGalleryFileMetaDataNoThumbnail(e.file.name).then(function(n) {
              0 !== n && (c.trigger(k.SELECT_FILE_GALLERY, n.hevc), e.hevc = n.hevc), B
                .fileIndex = t, B.updatePicker(e), g.resetVideoParams(), B.showData();
            })));
      }, B.uploadDisabled = function() {
        if (B.pickerData.warningNoticeText = void 0, !B.isLoggedIn) return !0;
        if (void 0 === B.uploadService || "" === B.uploadService) return !0;
        if (!B.videoUploadTitle || "" === B.videoUploadTitle || B.saveInProgress) return !0;
        if (B.populatedMomentsDataValid === !0 && B.populatedMomentsFolder.length > 0 && B
          .populatedMomentsFolder[B.fileIndex].moment.error) return !0;
        if (B.uploadService.providerName === f.providerNames.SINA && B.videoUploadTitle.split("#")
          .length > 2) return B.pickerData.warningNoticeText = "l10n.noHashtagError", !0;
        if (B.uploadService.videoOptions && "video" === B.fileToUpload.file.type && B.selectedOutputType
          .subtype === C.NORMAL) {
          var e = B.uploadService.videoOptions.maxVideoDurationSeconds,
            t = g.getVideoParams(),
            n = 100 * Math.floor(t.durationMs / 100);
          if (n > 1e3 * e) return B.pickerData.warningNoticeText = r("translate")(
            "l10n.videoTooLongWarning", {
              arg1: r("translate")(B.uploadService.title),
              arg2: e / 60
            }), !0;
          var i = B.uploadService.videoOptions.maxVideoFileSizeMB;
          if (B.fileSize > i) {
            if (!t.trimmed) return B.pickerData.warningNoticeText = r("translate")(
              "l10n.videoTooLargeWarning", {
                arg1: r("translate")(B.uploadService.title)
              }), !0;
            var o = B.fileSize * (t.sliderTrimRight - t.sliderTrimLeft) / 100,
              a = 1.1 * o;
            if (a > i) return B.pickerData.warningNoticeText = r("translate")(
              "l10n.videoTooLargeWarning", {
                arg1: r("translate")(B.uploadService.title)
              }), !0;
          }
        }
      }, B.isActiveFileIndex = function(e) {
        return B.fileIndex === e;
      }, B.keyDown = s.throttle(function(e, t, n, i) {
        $.debug("keyDown keyCode: " + n.keyCode);
      }, 100), B.getGalleryFileToUpload = function() {
        B.updatePicker(g.fileToProcess);
      }, B.save = function() {
        B.saveInProgress = !0;
        var e = "",
          t = "",
          n = !1;
        if ("video" === B.fileToUpload.file.type) {
          var i = g.getVideoParams();
          $.info("Video trim parameters: " + i.trimmed + " " + i.startMs + " " + i.durationMs), i
            .trimmed && (e = i.startMs, t = i.durationMs, n = !0);
        }
        var o = B.populatedMomentsFolder[B.fileIndex].moment.savedToGallery ? O.COPY : O.MOVE;
        $.info("Save highlight property: " + o === O.MOVE ? "Move" : "Copy Filename: " + B.fileToUpload
          .fullFilename), d.importHighlightToGallery(B.fileToUpload, o, e, t).then(function(e) {
          "" !== e && ($.info("Import HL to gallery: ", e), n === !1 && o === O.MOVE && (B
              .savedFilesList.push({
                newFileName: e,
                oldFileName: B.fileToUpload.fullFilename
              }), B.fileToUpload.fullFilename = e, B.fileToUpload.file.name = e, B.fileToUpload
              .moment.filename = e, B.populatedMomentsFolder[B.fileIndex].moment
              .savedToGallery = !0, B.populatedMomentsFolder[B.fileIndex].moment.filename = e),
            "video" === B.fileToUpload.file.type ? b.show(k.RECORD_STOPPED_AND_SAVED_TO_GALLERY) :
            b.show(k.SCREENSHOT_SAVED_TO_GALLERY), B.showData()), B.saveInProgress = !1;
        });
        var r = 0;
        if (!isNaN(parseInt(B.fileToUpload.duration))) {
          var a = B.fileToUpload.duration.split(":");
          r = 3600 * a[0] + 60 * a[1] + 1 * a[2];
        }
        var l = R.no;
        v.getModsActiveStatus() && (l = R.yes), x.push(M.OSC_HIGHLIGHT_EVENT, {
          type: B.fileToUpload.file.type,
          videoLength: r,
          gameTitle: B.fileToUpload.moment.gameName,
          highlightType: B.fileToUpload.moment.highlightDefinitionId,
          highlightAction: "Saved",
          sdkVersion: B.sdkVersion,
          DRSName: B.fileToUpload.moment.drsName,
          DRSProfileName: B.fileToUpload.moment.drsProfileName,
          modsActive: l
        });
      }, B.saveDisabled = function() {
        return B.showEmptyMoments === !0 || "" === B.fileToUpload || void 0 === B.fileToUpload.file || B
          .populatedMomentsDataValid === !1 || !!(B.populatedMomentsFolder.length > 0 && (B
            .populatedMomentsFolder[B.fileIndex].moment.busy || B.populatedMomentsFolder[B.fileIndex]
            .moment.error));
      }, B.highlightUpdate = function(e) {
        var t = 0,
          i = !1;
        return s.each(B.populatedMomentsFolder, function(n, o) {
          n.moment.id === e.id && n.moment.groupId === e.groupId && (n.moment.busy = !1, t = o,
            i = !0);
        }), i === !1 ? void $.error("highlightUpdate could not find a match...skipping") : (i === !
          0 && void 0 === e.filename && ($.info("Filename is missing, cancelling entry"), e.cancel = !
            0), e.cancel === !0 ? ($.info("Pending highlight canceled, entry: ", e), B
            .populatedMomentsFolder[t].moment.error = !0, t === B.fileIndex && (B.videoHighlight = B
              .populatedMomentsFolder[t].moment.highlightName, H(B.displayEmptyTypes.ERROR)), void x
            .push(M.OSC_HIGHLIGHT_CANCELED, {
              gameName: B.populatedMomentsFolder[t].moment.gameName,
              highlightType: B.populatedMomentsFolder[t].moment.highlightDefinitionId,
              DRSName: B.populatedMomentsFolder[t].moment.drsName,
              DRSProfileName: B.populatedMomentsFolder[t].moment.drsProfileName
            })) : i === !0 && "" !== B.populatedMomentsFolder[t].moment.filename ? void $.info(
            "Current update ignored, filename already present") : ($.info(
            "Pending highlight updated, entry: ", e), g.momentsFiles[t] = e.filename, s.isEmpty(n
            .params) || void 0 === n.params.moments ? void $.info(
            "Dialog closed, state params no longer valid") : (n.params.moments[t].filename = e
            .filename, g.getMomentsData(t, 1, !1).then(function() {
              B.populatedMomentsFolder = g.getPopulatedMomentsFolder(), B
                .populatedMomentsDataValid = !0, t === B.fileIndex && (B.updatePicker(B
                  .populatedMomentsFolder[t]), B.showData());
            }))));
      }, B.findMissingHighlight = function() {
        for (var e; void 0 !== (e = d.getPendingHighlights());) B.highlightUpdate(e);
      }, B.getMomentsFileToUpload = function(e, t) {
        return g.getMomentsData(e, t, !0).then(function() {
          B.populatedMomentsFolder = g.getPopulatedMomentsFolder(), B.populatedMomentsDataValid = !
            0, d.isHighlightSummaryOpen(!0), B.findMissingHighlight(), s.isEmpty(n.params) || s
            .isEmpty(l) || (B.fileIndex = void 0 === l.lastMomentIndex ? 0 : l.lastMomentIndex, n
              .params.lastMomentIndex = void 0, B.fileIndex >= e && B.fileIndex < e + t && (B
                .populatedMomentsFolder[B.fileIndex].moment.busy === !0 ? H(B.displayEmptyTypes
                  .SPINNER) : (B.selectFile(B.populatedMomentsFolder[B.fileIndex], B.fileIndex), B
                  .updatePicker(B.populatedMomentsFolder[B.fileIndex]))));
        });
      }, B.showData = function() {
        if (!s.isUndefined(B.fileToUpload.fullFilename))
          if (B.fileSize = B.fileToUpload.fileSize, "video" === B.fileToUpload.file.type) B.videoSrc = B
            .fileToUpload.fullFilename.replace(/\\/g, "/");
          else if ("image" === B.fileToUpload.file.type) {
          var e = B.fileToUpload.fullFilename;
          B.actualImageSrc = B.imageSrc = e.replace(/\\/g, "/");
        }
      }, B.initialize = function() {
        if ($.info("Initialize Upload Controller VM"), A.isConnectEnabled().then(function(e) {
            B.isConnectEnabled = e, c.on(_.PIPL_CONFIG_UPDATED, V);
          }), void 0 !== l.file) g.setFileToProcess(l.file), g.setParentGalleryState(!0);
        else if (void 0 !== l.moments) {
          c.on(I.HIGHLIGHT_COMPLETED, B.highlightUpdate), B.title = "l10n.highlights", B.icon =
            "icon-highlights", B.backButton = "l10n.done", B.momentsLength = l.moments.length, B
            .sdkVersion = l.sdkVersion, B.momentsLength > 0 ? (B.showMoments = !0, H(B.displayEmptyTypes
              .SPINNER), g.setMoments(l.moments, l.gameName, l.drsName, l.drsProfileName)) : B
            .showEmptyMoments = !0, g.setParentGalleryState(!1);
          var e = 0;
          $.info("Number of highlights: ", B.momentsLength), s.each(l.moments, function(t) {
            t.busy = "" === t.filename, t.error = !1, t.busy === !0 ? e++ : t.filename.endsWith(
              ".mp4") && e++;
          }), x.push(M.OSC_HIGHLIGHTS_OPENED, {
            numImages: B.momentsLength - e,
            numVideos: e,
            gameName: l.gameName,
            sdkVersion: B.sdkVersion,
            DRSName: l.drsName,
            DRSProfileName: l.drsProfileName
          });
        }
        if (B.enableEscapeEvent(!0), B.showMoments) return l && l.callback && l.callback(B.uploadService
          .providerName), B.initDynamicItems();
        if (B.showEmptyMoments) $.info("There are no highlights currently available");
        else {
          if ("" === g.fileToProcess) return $.error("No file to upload, file undefined!"), void h
            .closeOSC();
          B.getGalleryFileToUpload(), B.showData(), B.fileToUpload && B.fileToUpload.file && "video" ===
            B.fileToUpload.file.type ? x.push(M.OSC_VIDEO_UPLOAD_SCREEN_OPENED) : B.fileToUpload && B
            .fileToUpload.file && x.push(M.OSC_IMAGE_UPLOAD_SCREEN_OPENED);
        }
      }, B.onContentLoaded = function() {
        l && l.callback && l.callback(B.uploadService.providerName);
      };
      var K = function() {
        B.loadedPages = {}, B.PAGE_SIZE = 4, B.initialShow = 0, void 0 !== l.lastMomentIndex && (B
          .showIndex = l.lastMomentIndex, B.initialShow = B.showIndex & ~(B.PAGE_SIZE - 1));
      };
      K.prototype.getItemAtIndex = function(e) {
        if (!(e >= B.momentsLength)) {
          var t = Math.floor(e / B.PAGE_SIZE),
            n = B.loadedPages[t];
          return n ? n[e % B.PAGE_SIZE] : void(null !== n && this.fetchPage_(t));
        }
      }, K.prototype.getLength = function() {
        return B.momentsLength;
      }, K.prototype.fetchPage_ = function(e) {
        B.loadedPages[e] = [];
        var t = e * B.PAGE_SIZE,
          n = B.momentsLength,
          i = t + B.PAGE_SIZE,
          o = i < n ? B.PAGE_SIZE : n - t;
        B.getMomentsFileToUpload(t, o).then(function(n) {
          for (var i = t; i < t + o; i++) B.loadedPages[e].push(B.populatedMomentsFolder[i]);
          B.initialShow === t && (B.populatedMomentsFolder[B.fileIndex].moment.busy ? H(B
            .displayEmptyTypes.SPINNER) : B.showData());
        });
      }, B.initDynamicItems = function() {
        B.dynamicItems = new K();
      }, B.addMeme = function() {
        B.openMemeEditor = !0, B.uploadGifService = B.uploadService;
      }, B.saveMeme = function() {
        B.saveMemeEditor = !0;
      }, B.backMeme = function() {
        B.cancelMemeEditor = !0;
      }, B.memeDisabled = function() {
        return !B.selectedOutputType || !B.selectedOutputType.subtype || B.selectedOutputType
          .subtype !== C.GIF;
      }, B.initialize(), t.$on("$destroy", function() {
        B.enableEscapeEvent(!1), d.isHighlightSummaryOpen(!1), c.off(I.HIGHLIGHT_COMPLETED, B
          .highlightUpdate), c.off(_.PIPL_CONFIG_UPDATED, V);
      });
    }
  ]);
  exports.GalleryUploadMenuController = c;
}
