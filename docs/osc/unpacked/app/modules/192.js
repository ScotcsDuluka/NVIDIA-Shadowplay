// ─────────────────────────────────────────────────────────────
// APP MODULE 192
// role       : controller VideoEditorController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.VideoEditorController = void 0;
  var i = n(1);
  n(11);
  var o = i.ngMainModule.controller("VideoEditorController", ["$scope", "$document", "$interval", "$filter", "$sce",
    "$log", "oscGalleryService", "eventAggregator", "NOTIFIER_SELECTIONS",
    function(e, t, n, i, o, r, a, l, s) {
      function d(t) {
        if (g.sliderTrimLeftMax = g.sliderTrimRight - p, g.sliderTrimRightMin = g.sliderTrimLeft + p, g.fileSize = (
            g.fullFileSize * (g.sliderTrimRight - g.sliderTrimLeft) / 100).toFixed(2), 0 === g.sliderTrimLeft &&
          100 === g.sliderTrimRight && 0 === g.sliderPosition) {
          var n = 100 === g.volume && g.mute === !1;
          a.resetVideoParams(n)
        } else {
          var i = g.sliderTrimLeft * g.media[0].duration * 10,
            o = (g.sliderTrimRight - g.sliderTrimLeft) * g.media[0].duration * 10;
          a.setVideoTrimParams(!0, i, o, g.sliderPosition, g.sliderTrimRight, g.sliderTrimLeft)
        }
        g.media[0].pause(), e.$applyAsync(function() {
          g.sliderPosition = t
        })
      }

      function c() {
        var e = g.media[0].duration * (g.sliderTrimRight - g.sliderTrimLeft) / 100;
        g.totalDuration = i("date")(1e3 * e, "HH:mm:ss.sss", "UTC").slice(0, -2)
      }

      function u() {
        g.totalDuration = 0;
        var e = 0;
        c(), g.currentVideoTime = i("date")(0, "HH:mm:ss.sss", "UTC").slice(0, -2) + " / " + g.totalDuration, a
          .setInitialVideoDuration(g.media[0].duration), 100 === g.volume ? g.volume = 100 * g.media[0].volume : g
          .media[0].volume = g.volume / 100, g.mute === !0 && (g.media[0].muted = g.mute), g.playbackTimer = n(
            function() {
              if (!h) {
                c();
                var t, n = g.media[0].duration;
                if (g.media[0].currentTime >= n * g.sliderTrimRight / 100 && (g.media[0].pause(), g.sliderPosition =
                    g.sliderTrimRight), e !== g.sliderPosition) {
                  t = (g.sliderPosition - g.sliderTrimLeft) * n / 100, g.currentVideoTime = i("date")(1e3 * t,
                    "HH:mm:ss.sss", "UTC").slice(0, -2) + " / " + g.totalDuration;
                  var o = g.sliderPosition * n / 100;
                  return isNaN(parseFloat(o)) || (g.media[0].currentTime = o), void(e = g.sliderPosition)
                }
                g.media[0].paused && g.media[0].currentTime !== n || (t = g.media[0].currentTime - n * g
                  .sliderTrimLeft / 100, g.currentVideoTime = i("date")(1e3 * t, "HH:mm:ss.sss", "UTC").slice(0, -
                    2) + " / " + g.totalDuration, g.sliderPosition = e = Math.max(g.sliderPosition, Math.floor(g
                    .media[0].currentTime / n * 100)))
              }
            }, 100)
      }

      function f() {
        void 0 !== e.nvSrc && void 0 !== e.nvFileSize && (g.nvSrc = e.nvSrc, g.videoSrc = o.trustAsResourceUrl(g
            .nvSrc), g.fullFileSize = e.nvFileSize, g.fileSize = g.fullFileSize.toFixed(2), g.media && (g.media[0]
            .pause(), angular.isDefined(g.playbackTimer) && n.cancel(g.playbackTimer), g.media[0].src = g
            .videoSrc, g.media[0].load(), g.currentVideoTime = "00:00:00 / 00:00:00", g.sliderPosition = 0, g
            .sliderTrimLeft = 0, g.sliderTrimLeftMax = 100 - p, g.sliderTrimRightMin = p, g.sliderTrimRight = 100
            ))
      }

      function m(e) {
        g.videotoolbarDisabled = !1, "1" === e && (g.videotoolbarDisabled = !0)
      }
      var g = this;
      g.currentVideoTime = "00:00:00 / 00:00:00", g.sliderPosition = 0, g.videoLoaded = !1, g.directiveElement =
        null, g.media = null, g.totalDuration = 0, g.videoWaiting = !0, g.nvHighlight = "", g.videoErrorString = "";
      var p = 4;
      g.sliderTrimLeft = 0, g.sliderTrimLeftMax = 100 - p, g.sliderTrimRightMin = p, g.sliderTrimRight = 100;
      var h = !1;
      g.nvSrc = e.nvSrc, g.fullFileSize = -1, g.fileSize = g.fullFileSize.toFixed(2), g.volume = 100, g.mute = !1;
      var b = !1;
      g.displayTypes = {
        NORMAL: 0,
        SPINNER: 1,
        ERROR: 2
      }, g.useDisplay = g.displayTypes.NORMAL, g.trimStep = 1;
      var x = r.getInstance("osc/VideoEditorController");
      g.videoSrc = o.trustAsResourceUrl(g.nvSrc), g.videotoolbarDisabled = !1;
      var v = a.getVideoParams();
      v.trimmed === !0 && (x.info("Use saved videoParams: ", v), g.sliderPosition = v.sliderPosition, g
          .sliderTrimRight = v.sliderTrimRight, g.sliderTrimLeft = v.sliderTrimLeft, b = !0), v.volumeChanged === !
        0 && (x.info("Use saved volume: ", v.volume), g.volume = v.volume, g.mute = v.mute, b = !0), m(v.hevc), g
        .calcTrimRightWidth = function() {
          return {
            width: 100 - g.sliderTrimLeft - p + "%"
          }
        }, g.sliderChanged = function() {
          a.setSliderPosition(g.sliderPosition)
        }, g.trimChangedLeft = function() {
          d(g.sliderTrimLeft)
        }, g.trimChangedRight = function() {
          d(g.sliderTrimRight)
        }, g.toggleMediaPlay = function() {
          if (g.media && !g.videotoolbarDisabled)
            if (g.media[0].paused) {
              if (g.sliderPosition >= g.sliderTrimRight) {
                var e = g.sliderTrimLeft * g.media[0].duration / 100;
                isNaN(parseFloat(e)) || (g.media[0].currentTime = e), g.sliderPosition = g.sliderTrimLeft
              }
              g.media[0].play()
            } else g.media[0].pause();
        }, g.showPlaybutton = function() {
          return !!g.media && g.media[0].paused
        }, g.muted = function() {
          return !!g.media && g.media[0].muted
        }, g.muteUnmute = function() {
          g.media && !videoEditor.videotoolbarDisabled && (g.media[0].muted = !g.media[0].muted, a.setVideoMute(g
            .media[0].muted))
        }, g.volumeChanged = function() {
          g.media && (g.media[0].volume = g.volume / 100, a.setVideoVolume(g.volume))
        }, t.ready(function() {
          function n() {
            e.$apply(function() {
              g.videoWaiting = !1
            })
          }

          function i() {
            e.$apply(function() {
              g.videoWaiting = !0
            })
          }
          null === g.directiveElement && (g.directiveElement = angular.element(t[0].querySelector(
            ".gallery-video-panel")), g.media = g.directiveElement.find("video")), g.media.bind(
            "loadedmetadata",
            function() {
              h || (e.$apply(function() {
                  g.videoLoaded = !0
                }), g.trimStep = 10 / g.media[0].duration, u(), b === !0 ? d(g.sliderPosition) : a
                .setVideoTrimParams(!1, 0, 1e3 * g.media[0].duration, 0, 0, 0), e.onContentLoaded())
            }), g.media.bind("canplay", function() {
            n()
          }), g.media.bind("playing", function() {
            n()
          }), g.media.bind("waiting", function() {
            i()
          }), g.media.bind("stalled", function() {
            i()
          }), e.$apply()
        }), l.on(s.SELECT_FILE_GALLERY, m), e.$watch("nvFileSize", function() {
          g.fullFileSize !== e.nvFileSize && (e.nvFileSize < -1 ? g.useDisplay = g.displayTypes.ERROR : e
            .nvFileSize <= 0 ? g.useDisplay = g.displayTypes.SPINNER : g.useDisplay = g.displayTypes.NORMAL, f()
            )
        }, !0), e.$watch("nvSrc", function() {
          g.nvSrc !== e.nvSrc && f()
        }, !0), e.$watch("nvHighlight", function() {
          g.nvHighlight !== e.nvHighlight && (g.nvHighlight = e.nvHighlight, g.videoErrorString = i("translate")(
            "l10n.videoNotAvailable", {
              arg1: g.nvHighlight
            }))
        }, !0), e.$on("$destroy", function() {
          h = !0, angular.isDefined(g.playbackTimer) && n.cancel(g.playbackTimer), g.media && (g.media[0].pause(),
            g.media[0].src = "", g.media[0].load())
        })
    }
  ]);
  t.VideoEditorController = o
}
