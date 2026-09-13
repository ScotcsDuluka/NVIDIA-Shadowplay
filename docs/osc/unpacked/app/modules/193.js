// ─────────────────────────────────────────────────────────────
// APP MODULE 193
// role       : controller VideoEditorGifController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.VideoEditorGifController = void 0;
  var i = n(1);
  n(11);
  var o = i.ngMainModule.controller("VideoEditorGifController", ["$scope", "$document", "$interval", "$filter", "$sce",
    "$log", "oscGalleryService",
    function(e, t, n, i, o, r, a) {
      function l(t) {
        if (v.media) {
          if (v.sliderTrimRightMin = v.sliderTrimLeft + w, v.sliderTrimRightMin > 100 && (v.sliderTrimRightMin =
              100), v.sliderTrimRight = v.sliderTrimRightMin, v.sliderPosition = v.sliderTrimLeft, 0 === v
            .sliderTrimLeft && 100 === v.sliderTrimRight && 0 === v.sliderPosition) {
            var n = 100 === v.volume && v.mute === !1;
            a.resetVideoParams(n), v.currentVideoTime = i("date")(0, "HH:mm:ss.sss", "UTC").slice(0, -2)
          } else {
            var o = v.sliderTrimLeft * v.media[0].duration * 10,
              r = (v.sliderTrimRight - v.sliderTrimLeft) * v.media[0].duration * 10;
            v.currentVideoTime = i("date")(1e3 * o, "HH:mm:ss.sss", "UTC").slice(0, -2), a.setVideoTrimParams(!0, o,
              r, v.sliderPosition, v.sliderTrimRight, v.sliderTrimLeft)
          }
          v.media[0].pause(), e.$applyAsync(function() {
            v.sliderPosition = v.sliderTrimLeft
          })
        }
      }

      function s() {
        v.totalDuration = 0;
        var e = 0,
          t = v.currentVideoDuration,
          o = v.sliderTrimRight;
        Math.round(v.media[0].duration * v.sliderTrimRight / 100 * 10) / 10;
        v.currentVideoTime = i("date")(0, "HH:mm:ss.sss", "UTC").slice(0, -2), 100 === v.volume ? v.volume = 100 * v
          .media[0].volume : v.media[0].volume = v.volume / 100, v.mute === !0 && (v.media[0].muted = v.mute), v
          .playbackTimer = n(function() {
            if (!S) {
              var n = v.media[0].duration,
                i = v.media[0].currentTime - n * v.sliderTrimLeft / 100,
                r = Math.round(10 * v.media[0].currentTime) / 10;
              Math.round(n * v.sliderTrimRight / 100 * 10) / 10;
              if (e !== v.sliderPosition || t !== v.currentVideoDuration || o != v.sliderTrimRight) {
                var a = v.sliderPosition * n / 100;
                return isNaN(parseFloat(a)) || (v.media[0].currentTime = a, r = Math.round(10 * v.media[0]
                  .currentTime) / 10), e = v.sliderPosition, t = v.currentVideoDuration, void(o = v
                  .sliderTrimRight)
              }
              if (v.media[0].paused && v.media[0].currentTime !== n || (v.sliderPosition = e = Math.max(v
                  .sliderPosition, Math.floor(v.media[0].currentTime / n * 100))), !v.videoWaiting && (v
                  .sliderPosition >= v.sliderTrimRight || i > v.currentVideoDuration)) {
                v.sliderPosition = v.sliderTrimLeft;
                var l = v.sliderTrimLeft * v.media[0].duration / 100;
                v.media[0].currentTime = l
              }
            }
          }, 100)
      }

      function d() {
        if (v.volume = 0, v.isGIFValue = !0, v.fileSize = 15, v.currentVideoDuration = Math.round(10 * v
            .maxGIFLength) / 10, v.currentVideoDuration > 5 && (v.currentVideoDuration = 5), M.trimmed === !0 && (v
            .currentVideoDuration = M.durationMs / 1e3), v.media) {
          var t = v.media[0].duration;
          v.videoWidth = v.media[0].videoWidth, v.videoHeight = v.media[0].videoHeight, t < v.sliderMaxDuration && (
              v.durationMax = Math.round(10 * t) / 10), E = Math.round((t - 1) / t * 100), w = v
            .currentVideoDuration / t * 100, w > 100 && (w = 100), v.sliderTrimLeftMax = E, v.sliderTrimRightMin =
            w, v.sliderTrimRight = v.sliderTrimLeft + w, v.mute = v.media[0].muted = !0, v.durationChanged()
        }
        I = I || e.$watch("videoEditor.sliderTrimLeft", function(e, t) {
          if (v.media) {
            var n = v.sliderTrimLeft + w - 100;
            if (n > 0) v.currentVideoDuration -= n * v.media[0].duration / 100, v.durationMax = v
              .currentVideoDuration;
            else {
              var i = (100 - v.sliderTrimLeft) * v.media[0].duration / 100;
              i > v.sliderMaxDuration ? v.durationMax = v.sliderMaxDuration : v.durationMax = Math.round(10 *
                i) / 10
            }
          }
        }), A = A || e.$watch("videoEditor.currentVideoDuration", function(e, t) {
          if (v.media) {
            var n = v.media[0].duration;
            v.currentVideoDuration > v.maxGIFLength && (v.currentVideoDuration = v.maxGIFLength), w = v
              .currentVideoDuration / n * 100, w > 100 && (w = 100), v.sliderTrimRightMin = v.sliderTrimLeft +
              w, v.sliderTrimRightMin > 100 && (v.sliderTrimRightMin = 100), v.sliderTrimRight = v
              .sliderTrimRightMin
          }
          var o = Math.round(10 * v.currentVideoDuration) / 10;
          1 === o ? v.currentVideoDurationStr = i("translate")("l10n.GIFLengthUnitsSingular", {
            arg1: 1
          }) : v.currentVideoDurationStr = i("translate")("l10n.GIFLengthUnitsPlural", {
            arg1: o
          })
        })
      }

      function c() {
        void 0 !== e.nvSrc && void 0 !== e.nvFileSize && (f(), v.nvSrc = e.nvSrc, v.videoSrc = o.trustAsResourceUrl(
            v.nvSrc), v.fullFileSize = e.nvFileSize, v.fileSize = v.fullFileSize.toFixed(2), v.sliderMaxDuration =
          parseInt(e.nvGifMaxDuration), v.durationMax = v.maxGIFLength = v.sliderMaxDuration, v.media && (v.media[
              0].pause(), angular.isDefined(v.playbackTimer) && n.cancel(v.playbackTimer), v.media[0].src = v
            .videoSrc, v.media[0].load(), v.currentVideoTime = "00:00:00 / 00:00:00", v.sliderPosition = 0, v
            .sliderTrimLeft = 0, v.sliderTrimLeftMax = 100))
      }

      function u() {
        var e = document.getElementById("videoViewer"),
          t = e.clientWidth / e.clientHeight,
          n = v.videoWidth / v.videoHeight;
        v.usableWidth = e.clientWidth * (n / t) - 40, O.info("View Width: " + e.clientWidth + " height: " + e
          .clientHeight), O.info("Video Width: " + v.videoWidth + " height: " + v.videoHeight), O.info(
          "Ratios view: " + t + " video: " + n + " usableWidth: " + v.usableWidth)
      }

      function f() {
        O.info("Resetting meme!"), v.memeCancel()
      }

      function m() {
        var t = v.videoHeight,
          n = v.videoWidth,
          i = e.nvUploadGifService.gifOptions.memeHeight;
        O.info("Meme max height: ", i), t > i && (t = i, n = t * (v.videoWidth / v.videoHeight)), O.info(
          "Meme size: " + n + "x" + t);
        var o = document.getElementById("videoViewer");
        if (null !== o) {
          var r = {
            width: n,
            height: t,
            clientWidth: o.clientWidth,
            topString: v.memes.topUI.string || "",
            topFontSize: v.memes.topUI.fontSize,
            bottomString: v.memes.bottomUI.string || "",
            bottomFontSize: v.memes.bottomUI.fontSize
          };
          a.createMeme(r)
        }
      }

      function g(e, n) {
        var i = t[0].createElement("canvas"),
          o = i.getContext("2d");
        o.font = e + "px Impact";
        var r = o.measureText(n).width;
        return r
      }

      function p(e) {
        for (var t = g(C, e.string); t > v.usableWidth;) {
          var n = e.string.length;
          e.string = e.string.substring(0, n - 1), t = g(C, e.string), O.info("Trunc meme chars: " + e.string
            .length + " width: " + t + " String: " + e.string)
        }
        return e.maxLength = e.string.length, t
      }

      function h(e) {
        var t = e.fontSize,
          n = g(t, e.string),
          i = t;
        if (n > v.usableWidth && t === C) return p(e), t;
        if (e.maxLength = _, n > v.usableWidth && t > C) {
          for (; n > v.usableWidth && t > C;) t--, n = g(t, e.string);
          i !== t && O.info("shrink meme font from: " + i + "px to: " + t + "px"), n > v.usableWidth && t === C && (
            O.info("Special case. Length: " + n + " usableWidth: " + v.usableWidth), n = p(e))
        } else if (n < e.previousWidth && t < T) {
          for (; n < v.usableWidth && t < T;) t++, n = g(t, e.string);
          t = n > v.usableWidth ? t - 1 : t, i !== t && O.info("Grow meme font from: " + i + "px to: " + t + "px")
        }
        return e.previousWidth = n, t
      }

      function b() {
        var e = a.getMemeStrings();
        void 0 !== e.source && e.source === v.nvSrc ? "" === e.top && "" === e.bottom || (O.info(
            "Using previous meme strings: ", e), v.memes.topUI.string = e.top, v.memes.bottomUI.string = e.bottom,
          u(), v.setMemeDataTop(), v.setMemeDataBottom(), v.memeDone()) : a.clearMemeStrings()
      }

      function x() {
        if ("" !== v.memes.topUI.string || "" !== v.memes.bottomUI.string) {
          v.showMemeEditor && v.memeDone();
          var e = {
            top: v.memes.topUI.string || "",
            bottom: v.memes.bottomUI.string || "",
            source: v.nvSrc
          };
          O.info("Saving meme data: ", e), a.saveMemeStrings(e)
        } else a.clearMemeStrings()
      }
      var v = this;
      v.currentVideoTime = "00:00:00 / 00:00:00", v.sliderPosition = 0, v.videoLoaded = !1, v.directiveElement =
        null, v.media = null, v.totalDuration = 0, v.videoWaiting = !0, v.nvHighlight = "", v.videoErrorString = "";
      var y = 4;
      v.sliderTrimLeft = 0, v.sliderTrimLeftMax = 100 - y, v.sliderTrimRightMin = y, v.sliderTrimRight = 100;
      var w = y;
      v.maxGIFLength = 15, v.currentVideoDuration = 5;
      var S = !1;
      v.nvSrc = e.nvSrc, v.fullFileSize = -1, v.fileSize = v.fullFileSize.toFixed(2), v.outputType = e.nvOutputType,
        v.sliderMaxDuration = 15, v.durationMax = v.sliderMaxDuration;
      var E = 100;
      v.volume = 100, v.mute = !0;
      var k = !1,
        _ = 80;
      v.showMemeEditor = !1, v.showMeme = !1;
      var T = 42,
        C = 20;
      v.placeholderMemeTop = i("translate")("l10n.memeTextTop"), v.placeholderMemeBottom = i("translate")(
        "l10n.memeTextBottom"), v.videoWidth = 0, v.videoHeight = 0, v.currentUploadService = void 0, v.memes = {
        topUI: {
          string: "",
          fontSize: T,
          previousWidth: 0,
          maxLength: _,
          id: "memeTopString"
        },
        bottomUI: {
          string: "",
          fontSize: T,
          previousWidth: 0,
          maxLength: _,
          id: "memeBottomString"
        }
      }, v.displayTypes = {
        NORMAL: 0,
        SPINNER: 1,
        ERROR: 2
      }, v.useDisplay = v.displayTypes.NORMAL, v.isGIFValue = !0, v.trimStep = 1;
      var O = r.getInstance("osc/VideoEditorController");
      v.videoSrc = o.trustAsResourceUrl(v.nvSrc);
      var A = null,
        I = null,
        M = a.getVideoParams();
      M.trimmed === !0 && (O.info("Use saved videoParams: ", M), v.sliderPosition = M.sliderPosition, v
          .sliderTrimRight = M.sliderTrimRight, v.sliderTrimLeft = M.sliderTrimLeft, k = !0), M.volumeChanged === !
        0 && (O.info("Use saved volume: ", M.volume), v.volume = M.volume, v.mute = M.mute, k = !0), v
        .calcTrimRightWidth = function() {
          return {
            width: 100 - v.sliderTrimRightMin + "%"
          }
        }, v.sliderChanged = function() {
          a.setSliderPosition(v.sliderPosition), v.media && v.media[0].pause()
        }, v.trimChangedLeft = function() {
          l(v.sliderTrimLeft)
        }, v.trimChangedRight = function() {
          l(v.sliderTrimRight)
        }, v.durationChanged = function() {
          if (v.media) {
            var e = v.sliderTrimLeft * v.media[0].duration * 10,
              t = 1e3 * v.currentVideoDuration;
            a.setVideoTrimParams(!0, e, t, v.sliderPosition, v.sliderTrimRight, v.sliderTrimLeft), v.media[0]
            .pause()
          }
        }, v.toggleMediaPlay = function() {
          if (v.media)
            if (v.media[0].paused) {
              if (v.sliderPosition >= v.sliderTrimRight) {
                var e = v.sliderTrimLeft * v.media[0].duration / 100;
                isNaN(parseFloat(e)) || (v.media[0].currentTime = e), v.sliderPosition = v.sliderTrimLeft
              }
              v.media[0].play()
            } else v.media[0].pause()
        }, v.showPlaybutton = function() {
          return !!v.media && v.media[0].paused
        }, v.muted = function() {
          return !!v.media && v.media[0].muted
        }, v.isGIF = function() {
          return v.isGIFValue
        }, v.stepStartTime = function(e) {
          e ? (v.sliderTrimLeft > 0 && (v.sliderTrimLeft -= v.trimStep), v.sliderTrimLeft < 0 && (v.sliderTrimLeft =
            0)) : (v.sliderTrimLeft < v.sliderTrimLeftMax && (v.sliderTrimLeft += v.trimStep), v.sliderTrimLeft >
            v.sliderTrimLeftMax && (v.sliderTrimLeft = v.sliderTrimLeftMax)), v.trimChangedLeft()
        }, v.stepDuration = function(e) {
          e ? (v.currentVideoDuration > 1 && (v.currentVideoDuration -= .1), v.currentVideoDuration < 1 && (v
              .currentVideoDuration = 1)) : (v.currentVideoDuration < v.durationMax && (v.currentVideoDuration +=
              .1), v.currentVideoDuration > v.durationMax && (v.currentVideoDuration = v.durationMax)), v
            .durationChanged()
        }, t.ready(function() {
          function n() {
            e.$apply(function() {
              v.videoWaiting = !1
            })
          }

          function i() {
            e.$apply(function() {
              v.videoWaiting = !0
            })
          }
          null === v.directiveElement && (v.directiveElement = angular.element(t[0].querySelector(
            ".gallery-video-panel")), v.media = v.directiveElement.find("video")), v.media.bind(
            "loadedmetadata",
            function() {
              if (!S) {
                e.$apply(function() {
                  v.videoLoaded = !0
                });
                var t = v.media[0].duration;
                t < v.maxGIFLength && (v.maxGIFLength = t), v.trimStep = 10 / t, w = v.maxGIFLength / t * 100,
                  w > 100 && (w = 100), s(), d(), b(), k === !0 && l(v.sliderPosition), e.onContentLoaded()
              }
            }), v.media.bind("canplay", function() {
            n()
          }), v.media.bind("playing", function() {
            n()
          }), v.media.bind("waiting", function() {
            i()
          }), v.media.bind("stalled", function() {
            i()
          }), v.media.bind("ended", function() {
            var e = v.sliderTrimLeft * v.media[0].duration / 100;
            v.media[0].currentTime = e, v.sliderPosition = v.sliderTrimLeft, v.media[0].play()
          }), e.$apply()
        }), v.memeOpen = function() {
          v.showMemeEditor = !0, v.showMeme = !0, e.upperMeme.$setPristine(), e.upperMeme.$setValidity(), e
            .upperMeme.$setUntouched(), u()
        }, v.memeDone = function(t) {
          v.showMemeEditor = !1, e.nvAddMeme = !1, e.nvSaveMeme = !1, e.nvCancelMeme = !1, t || (O.info(
              "Meme top: ", v.memes.topUI.string), O.info("Meme bottom: ", v.memes.bottomUI.string), v.showMeme =
            "" !== v.memes.topUI.string || "" !== v.memes.bottomUI.string, m())
        }, v.memeCancel = function() {
          v.memes.topUI.string = "", v.memes.bottomUI.string = "";
          var e = {
            width: 0,
            height: 0,
            clientWidth: 0,
            topString: "",
            topFontSize: 0,
            bottomString: "",
            bottomFontSize: 0
          };
          a.createMeme(e), v.memeDone(!0)
        }, v.setMemeDataTop = function() {
          if (void 0 !== v.memes.topUI.string) {
            v.memes.topUI.fontSize = h(v.memes.topUI);
            var e = document.getElementById(v.memes.topUI.id);
            null !== e && (e.style.fontSize = v.memes.topUI.fontSize + "px")
          }
        }, v.setMemeDataBottom = function() {
          if (void 0 !== v.memes.bottomUI.string) {
            v.memes.bottomUI.fontSize = h(v.memes.bottomUI);
            var e = document.getElementById(v.memes.bottomUI.id);
            if (null !== e) {
              e.style.fontSize = v.memes.bottomUI.fontSize + "px";
              var t = 310 + (T - v.memes.bottomUI.fontSize);
              e.style.top = t + "px"
            }
          }
        }, e.$watch("nvFileSize", function() {
          v.fullFileSize !== e.nvFileSize && (e.nvFileSize < -1 ? v.useDisplay = v.displayTypes.ERROR : e
            .nvFileSize <= 0 ? v.useDisplay = v.displayTypes.SPINNER : v.useDisplay = v.displayTypes.NORMAL, c()
            )
        }, !0), e.$watch("nvSrc", function() {
          v.nvSrc !== e.nvSrc && c()
        }, !0), e.$watch("nvHighlight", function() {
          v.nvHighlight !== e.nvHighlight && (v.nvHighlight = e.nvHighlight, v.videoErrorString = i("translate")(
            "l10n.videoNotAvailable", {
              arg1: v.nvHighlight
            }))
        }), e.$watch("nvGifMaxDuration", function() {
          if (v.sliderMaxDuration !== e.nvGifMaxDuration) {
            if (v.sliderMaxDuration = parseInt(e.nvGifMaxDuration), v.durationMax = v.maxGIFLength = v
              .sliderMaxDuration, v.media) {
              var t = v.media[0].duration;
              t < v.sliderMaxDuration && (v.durationMax = Math.round(10 * t) / 10), v.currentVideoDuration > v
                .maxGIFLength && (v.currentVideoDuration = v.maxGIFLength)
            }
            v.durationChanged()
          }
        }, !0), e.$watch("nvAddMeme", function() {
          e.nvAddMeme && !v.showMemeEditor && v.memeOpen(!1)
        }, !0), e.$watch("nvSaveMeme", function() {
          e.nvSaveMeme && v.showMemeEditor && v.memeDone(!1)
        }, !0), e.$watch("nvCancelMeme", function() {
          e.nvCancelMeme && v.showMemeEditor && v.memeCancel()
        }, !0), e.$watch("nvUploadGifService", function() {
          e.nvUploadGifService && (O.info("GIF Upload Service Selected: ", e.nvUploadGifService), void 0 === v
            .currentUploadService ? v.currentUploadService = e.nvUploadGifService : v.currentUploadService !== e
            .nvUploadGifService && (v.currentUploadService.gifOptions.memeHeight !== e.nvUploadGifService
              .gifOptions.memeHeight && m(), v.currentUploadService = e.nvUploadGifService))
        }, !0), d(), b(), e.$on("$destroy", function() {
          x(), S = !0, angular.isDefined(v.playbackTimer) && n.cancel(v.playbackTimer), v.media && (v.media[0]
            .pause(), v.media[0].src = "", v.media[0].load())
        })
    }
  ]);
  t.VideoEditorGifController = o
}
