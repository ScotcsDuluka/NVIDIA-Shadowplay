// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 146
// service oscGalleryService
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
  }), exports.oscGalleryService = void 0;
  var r = require(8),
    a = o(r),
    l = require(3),
    s = i(l),
    d = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(11);
  var c = d.ngMainCommonModule.service("oscGalleryService", ["$q", "$log", "$timeout", "$document",
    "galleryService", "galleryEndpoints", "eventAggregator", "GALLERY_STATE", "GALLERY_EVENTS",
    function(e, t, n, i, o, r, l, d, c) {
      function u(e) {
        function t(t) {
          return t.fullFilename !== e.fullFilename || (n = t, !1);
        }
        if (h.clearMemeStrings(), 0 === h.deletionList.length) return void b.info(
          "Trying to delete item from empty list");
        b.info("Deletion item file: ", e.fullFilename);
        var n = {};
        h.deletionList = h.deletionList.filter(t), b.info("Deletion list post: ", h.deletionList), r
          .removeGalleryItem({}, {
            file: n.fullFilename,
            forceDelete: !0
          }).then(function() {
            b.info("Deletion of temp file complete.");
          }, function(e) {
            b.info("Deletion of temp file failed:", e);
          });
      }

      function f(e, t) {
        var n = i[0].createElement("canvas"),
          o = n.getContext("2d");
        o.font = e + "px Impact";
        var r = o.measureText(t).width;
        return r;
      }

      function m(e) {
        var t = parseInt(e),
          n = Math.round((t + 20) / 20);
        return Math.min(n, 5);
      }

      function g(e, t) {
        b.info("Begin creating Bitmap at: " + e + "x" + t);
        var n = e,
          i = t,
          o = document.createElement("CANVAS");
        o.width = n, o.height = i;
        var r = o.getContext("2d");
        r.fillStyle = "rgba(0,0,0,0)", r.fillRect(0, 0, n, i);
        var a = h.memeParams.topFontSize * (n / h.memeParams.clientWidth),
          l = h.memeParams.bottomFontSize * (n / h.memeParams.clientWidth),
          s = h.memeParams.strokeColor || "black",
          d = h.memeParams.fillColor || "white",
          c = a.toFixed(0);
        r.font = c + "px Impact", b.info("Video Font top: ", r.font);
        var u = f(c, h.memeParams.topString),
          g = Math.floor(n / 2 - u / 2),
          p = parseInt(c);
        b.info("Top string at x: " + g + " y: " + p), r.strokeStyle = s, r.lineWidth = m(c), r.strokeText(
            h.memeParams.topString, g, p, u), r.fillStyle = d, r.fillText(h.memeParams.topString, g, p,
          u), c = l.toFixed(0), r.font = c + "px Impact", b.info("Video Font bottom: ", r.font), u = f(c,
            h.memeParams.bottomString);
        var x = .025 * i;
        g = Math.floor(n / 2 - u / 2), p = Math.floor(i - x), b.info("Bottom string at x: " + g + " y: " +
            p), r.strokeStyle = s, r.lineWidth = m(c), r.strokeText(h.memeParams.bottomString, g, p, u), r
          .fillStyle = d, r.fillText(h.memeParams.bottomString, g, p, u), b.info("Canvas created");
        var v = S.toDataURL(o);
        return b.info("BMP Image created"), v;
      }

      function p(e) {
        var t = void 0;
        return h.memeStringList.forEach(function(n, i) {
          n.source === e && (b.info("Fetched meme: ", n), t = i);
        }), t;
      }
      var h = this,
        b = t.getInstance("osc/galleryService"),
        x = null,
        v = null;
      h.deletionList = [], h.fileToProcess = "", h.memeParams = "", h.memeCanvasData = "", h.memePromise =
        void 0, h.galleryState = d.EMPTY, h.isBackGallery = !1, h.momentsFiles = null, h
        .populatedMomentsFilesFolder = [], h.summary = {}, h.setThumbSizes = function() {
          o.thumbSizes = {
            width: 160,
            height: 90
          };
        }, h.setFileToProcess = function(e) {
          h.fileToProcess = e;
        }, h.setParentGalleryState = function(e) {
          h.isBackGallery = e;
        }, h.checkGalleryState = function() {
          return h.isBackGallery;
        }, h.getGalleryState = function() {
          return h.galleryState;
        }, h.setGalleryState = function(e) {
          h.galleryState = e;
        }, h.getCurrentFolder = function() {
          return h.currentFolder;
        }, h.setCurrentFolder = function(e) {
          h.currentFolder = e;
        }, h.getCurrentFileIndex = function() {
          return h.currentFileIndex;
        }, h.setCurrentFileIndex = function(e) {
          h.currentFileIndex = e;
        }, h.getRecentFileIndex = function() {
          return h.recentFileIndex;
        }, h.setRecentFileIndex = function(e) {
          h.recentFileIndex = e;
        }, h.getCurrentFolderIndex = function() {
          return h.currentFolderIndex;
        }, h.setCurrentFolderIndex = function(e) {
          h.currentFolderIndex = e;
        }, h.getCurrentFolderDisplaySetting = function() {
          return h.currentFolderDisplaySetting;
        }, h.setCurrentFolderDisplaySetting = function(e) {
          h.currentFolderDisplaySetting = e;
        };
      var y = {
        trimmed: !1,
        startMs: 0,
        durationMs: -1,
        sliderPosition: 0,
        sliderTrimRight: 0,
        sliderTrimLeft: 0,
        volumeChanged: !1,
        volume: 100,
        mute: !1,
        hevc: "0"
      };
      h.setHevc = function(e) {
        y.hevc = e;
      }, h.getVideoParams = function() {
        return y;
      };
      var w = 0;
      h.setInitialVideoDuration = function(e) {
        w = e;
      }, h.getVideoDurationMs = function() {
        return 1e3 * w;
      }, h.getDPSettings = function() {
        return v && x ? {
          outputType: x,
          service: v
        } : null;
      }, h.setVideoTrimParams = function(e, t, n, i, o, r) {
        y.trimmed = e, y.startMs = t, y.durationMs = n, y.sliderPosition = i, y.sliderTrimRight = o, y
          .sliderTrimLeft = r;
      }, h.setSliderPosition = function(e) {
        y.sliderPosition = e;
      }, h.setVideoVolume = function(e) {
        y.volumeChanged = !0, y.volume = e;
      }, h.setVideoMute = function(e) {
        y.volumeChanged = !0, y.mute = e;
      }, h.setOutputFormat = function(e) {
        x = e;
      }, h.setOutputService = function(e) {
        v = e;
      }, h.resetVideoParams = function(e) {
        y.trimmed = !1, void 0 !== e && e !== !0 || (y.volumeChanged = !1, y.volume = 100, y.mute = !1);
      }, h.resetGalleryState = function() {
        h.currentFolder = "", h.currentFileIndex = 0, h.recentFileIndex = -1, h.currentFolderIndex = 0,
          h.currentFolderDisplaySetting = 1;
      }, h.resetOutputData = function() {
        x = null, v = null;
      }, h.setMoments = function(e, t, n, i) {
        h.moments = e, h.moments = s.map(h.moments, function(e, o) {
          var r = {
            moment: e
          };
          return r.moment.gameName = r.moment.shortName || t, r.moment.drsName = r.moment.drsName ||
            n, r.moment.drsProfileName = r.moment.drsProfileName || i, r;
        }), h.momentsFiles = s.map(h.moments, function(e) {
          return e.moment.filename;
        });
      }, h.getMomentsData = function(t, n, i) {
        function r() {
          for (; 0 !== h.populatedMomentsFilesFolder.length;) {
            var e = h.populatedMomentsFilesFolder.pop(),
              t = h.moments[e.hlIndex];
            t.fullFilename = e.fullFilename, t.file = e.file, t.folder = e.folder, t.index = e.hlIndex,
              t.duration = e.duration, t.date = e.date, t.fileSize = e.fileSize, t.audiotype = e
              .audiotype, t.data = e.data;
          }
          h.fileToProcess = h.moments[0], a.resolve(!0);
        }
        var a = e.defer(),
          l = [];
        i && 0 === t && (h.populatedMomentsFilesFolder.length = 0);
        for (var s = t; s < t + n; s++)
          if (h.moments[s].moment.busy === !1) {
            var d = h.momentsFiles[s],
              c = o.extractGameName(d);
            l.push(o.getMetaData(d, c, d, s, h.populatedMomentsFilesFolder, !1, !0, s));
          }
        return e.all(l).then(r), a.promise;
      }, h.getPopulatedMomentsFolder = function() {
        return h.moments;
      }, h.getHighlightsData = function(e, t) {
        return s.each(h.moments, function(e) {
          e.moment.busy = "" === e.moment.filename, e.moment.error = !1;
        }), void 0 === t && (t = 0), h.getMomentsData(t, e, !0).then(function() {
          return h.moments;
        });
      }, h.saveSummary = function(e) {
        h.summary = JSON.parse((0, a.default)(e));
      }, h.getSummary = function() {
        return h.summary;
      }, h.clearSummary = function() {
        h.summary = {};
      }, h.pushToTempStackForDeletion = function(e) {
        var t = {
          fullFilename: e.fullFilename
        };
        h.deletionList.push(t), b.info("Deletion list push: ", h.deletionList);
      };
      var S = {
        toArrayBuffer: function(e) {
          function t(e) {
            h.setUint16(b, e, !0), b += 2;
          }

          function n(e) {
            h.setUint32(b, e, !0), b += 4;
          }
          var i,
            o,
            r,
            a,
            l = e.width,
            s = e.height,
            d = 4 * l,
            c = e.getContext("2d").getImageData(0, 0, l, s),
            u = new Uint32Array(c.data.buffer),
            f = 4 * Math.floor((32 * l + 31) / 32),
            m = f * s,
            g = 56 + m,
            p = new ArrayBuffer(g),
            h = new DataView(p),
            b = 0,
            x = 0,
            v = 0,
            y = 54;
          for (t(19778), n(g), b += 4, n(y), n(40), n(l), n(s), t(1), t(32), b += 24, x = s - 1; x >=
            0;) {
            for (o = y + x * f, i = 0; i < d;) a = u[v++], r = a >>> 24, h.setUint32(o + i, a << 8 |
              r), i += 4;
            x--;
          }
          return p;
        },
        toDataURL: function(e) {
          for (var t = new Uint8Array(this.toArrayBuffer(e)), n = "", i = 0, o = t.length; i < o;)
            n += String.fromCharCode(t[i++]);
          return btoa(n);
        }
      };
      h.createMeme = function(t) {
        return h.memeParams = t, h.memeCanvasData = "", b.info("Meme params: ", t), 0 === t.width ?
          void(h.memePromise = e.when(h.memeCanvasData)) : void(h.memePromise = n(function() {
            return h.memeCanvasData = g(h.memeParams.width, h.memeParams.height), h.memeCanvasData;
          }, 0));
      }, h.checkForMeme = function() {
        return e.when(h.memePromise);
      }, h.getMemeLengths = function() {
        var e = {
          topMemeLength: h.memeParams.topString.length,
          bottomMemeLength: h.memeParams.bottomString.length
        };
        return e;
      }, h.memeStringList = [], h.saveMemeStrings = function(e) {
        h.memeStrings = e, void 0 === p(e.source) ? (h.memeStringList.push(e), b.info("Saved meme: ",
          e)) : b.info("Meme already saved: ", e);
      }, h.getMemeStrings = function(e) {
        if (void 0 === e) return h.memeStrings;
        var t = p(e);
        if (void 0 === t) {
          var n = {
            top: "",
            bottom: "",
            source: void 0,
            color: ""
          };
          return n;
        }
        return h.memeStringList[t];
      }, h.clearMemeStrings = function(e) {
        if (b.info("Source to clear: ", e), void 0 === e) h.memeStrings = {
          top: "",
          bottom: "",
          source: void 0
        };
        else {
          var t = e.replace(/\\/g, "/");
          b.info("Modified source to clear: ", t);
          var n = void 0;
          h.memeStringList.forEach(function(e, i) {
            e.source === t && (n = i);
          }), void 0 !== n && (b.info("Removed meme item: ", h.memeStringList[n]), h.memeStringList =
            s.without(h.memeStringList, h.memeStringList[n]), b.info("Array after delete: ", h
              .memeStringList));
        }
      };
      var E = 80;
      h.upperMemeMaxed = E, h.lowerMemeMaxed = E, h.setUpperMemeMaxed = function(e) {
        h.upperMemeMaxed = e;
      }, h.setLowerMemeMaxed = function(e) {
        h.lowerMemeMaxed = e;
      }, h.isUpperMemeMaxed = function() {
        return h.upperMemeMaxed;
      }, h.isLowerMemeMaxed = function() {
        return h.lowerMemeMaxed;
      }, h.init = function() {
        h.clearMemeStrings(), h.resetGalleryState(), h.setThumbSizes();
      }, l.on(c.UPLOAD_COMPLETE, u), h.init();
    }
  ]);
  exports.oscGalleryService = c;
}
