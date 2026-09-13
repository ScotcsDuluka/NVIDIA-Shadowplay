// ─────────────────────────────────────────────────────────────
// APP MODULE 176
// role       : controller SettingsCustomizeController | directive getCustomizeWidth
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
  }), t.getCustomizeWidth = t.SettingsCustomizeController = void 0;
  var o = n(3),
    r = i(o),
    a = n(1);
  n(4), n(32), n(26);
  var l = a.ngMainModule.controller("SettingsCustomizeController", ["$scope", "$q", "$log", "$filter", "$document",
      "$timeout", "$window", "eventAggregator", "shadowPlayService", "broadcastService", "telemetryService",
      "connectService", "OSC_MODE", "VIDEO_STATE", "KEYBOARD_EVENTS", "TELEMETRY_OSC_EVENT_NAMES",
      "TELEMETRY_OSC_CAPTURE_TYPE",
      function(e, t, n, i, o, a, l, s, d, c, u, f, m, g, p, h, b) {
        function x(e) {
          var t = {
            Low: "Average",
            Medium: "Good",
            High: "VeryGood",
            Ultra: "UltraGood",
            Custom: "Custom"
          };
          return t[e.id]
        }

        function v(e) {
          var t = {
            Average: "Low",
            Good: "Medium",
            VeryGood: "High",
            UltraGood: "Ultra",
            Custom: "Custom"
          };
          return N.Qualities[t[e]]
        }

        function y(e) {
          return e.name
        }

        function w(e) {
          for (var t = 0; t < N.Resolutions.length; t++)
            if (N.Resolutions[t].name === e) return N.Resolutions[t]
        }

        function S(e) {
          return parseInt(e.name, 10)
        }

        function E(e) {
          for (var t = 0; t < N.Framerates.length; t++) {
            var n = parseInt(N.Framerates[t].name, 10);
            if (n === e) return N.Framerates[t]
          }
        }

        function k(e) {
          return 1e6 * e
        }

        function _(e) {
          return e / 1e6
        }

        function T(e) {
          return 60 * e
        }

        function C(e) {
          var t = x(N.settings.quality);
          return N.feature === m.BROADCAST && (t = f.endpoints[N.selectedPortal.providerName].getBroadcastQuality(t)),
            d.getBitrateRange(t, y(N.settings.resolution)).then(function(t) {
              return N.feature === m.BROADCAST ? (N.BitrateSlider.min = 1, N.BitrateSlider.max = N
                  .broadcast2KSupported === !0 ? N.selectedPortal.bitrateMax2K : N.selectedPortal.bitrateMax, N
                  .BitrateSlider.step = N.broadcast2KSupported === !0 ? 1 : .5) : (N.BitrateSlider.min = _(t
                  .bitrateBpsMin), N.BitrateSlider.max = _(t.bitrateBpsMax)), e && (N.settings.bitrate = _(t
                  .bitrateBpsDefault)), N.settings.bitrate < N.BitrateSlider.min ? N.settings.bitrate = N
                .BitrateSlider.min : N.settings.bitrate > N.BitrateSlider.max && (N.settings.bitrate = N
                  .BitrateSlider.max), t
            })
        }

        function O(e) {
          N.Framerates[1].supported = !0, e || (N.settings.framerate = N.Framerates[1]), "4320p 8K" === N.settings
            .resolution.name && (N.useDefaultBitRate = !0, N.support8K60 ? (N.Framerates[1].supported = !0, e || (N
              .settings.framerate = N.Framerates[1])) : (N.settings.framerate = N.Framerates[0], N.Framerates[1]
              .supported = !1)), N.selectedPortal && N.feature === m.BROADCAST && N.selectedPortal.providerName === f
            .providerNames.FACEBOOK && (N.Framerates[1].supported = !1, N.settings.framerate = N.Framerates[0])
        }

        function A(e) {
          return "Custom" === e ? (N.settings.resolution = N.settings.rememberedResolution, N.settings.framerate = N
            .settings.rememberedFramerate, N.settings.bitrate = N.settings.rememberedBitrate, O(!1), C(!1).then(
              function(e) {})) : (N.feature === m.BROADCAST && (e = f.endpoints[N.selectedPortal.providerName]
            .getBroadcastQuality(e)), d.getDefaultResolution(e).then(function(t) {
            return N.settings.resolution = w(t), d.getDefaultFramerate(e)
          }).then(function(e) {
            return N.settings.framerate = E(e), C(!0)
          }).then(function(e) {
            N.settings.bitrate = _(e.bitrateBpsDefault)
          }))
        }

        function I() {
          var e = N.selectedPortal.supported_settings,
            t = [];
          for (var n in e) t = t.concat(e[n]);
          N.Resolutions.forEach(function(e) {
            if (e.supported = t.indexOf(e.name) !== -1, "1440p HD" === e.name && e.supported) return d
              .isBroadcast2KEnabled().then(function(t) {
                e.supported = t
              })
          })
        }

        function M() {
          var e = N.selectedPortal.supported_settings;
          N.Framerates.forEach(function(t) {
            angular.isDefined(e[t.name]) ? t.supported = e[t.name].indexOf(N.settings.resolution.name) !== -1 : t
              .supported = !1
          }), N.settings.framerate.supported || (N.settings.framerate = r.findWhere(N.Framerates, {
            supported: !0
          }), N.settings.updateSettings = !0)
        }

        function R() {
          switch (N.selectedPortal.providerName) {
            case f.providerNames.GOOGLE:
              u.push(h.OSC_BROADCAST_YOUTUBE_CUSTOMIZE);
              break;
            case f.providerNames.TWITCH:
              u.push(h.OSC_BROADCAST_TWITCH_CUSTOMIZE)
          }
          u.push(h.OSC_BROADCAST_CUSTOMIZE, {
            provider: N.selectedPortal.providerName
          })
        }

        function P() {
          N.settings.rememberedFramerate = N.settings.framerate, N.settings.rememberedBitrate = N.settings.bitrate, N
            .settings.rememberedResolution = N.settings.resolution, N.settings.quality = N.Qualities.Custom, N
            .settings.updateSettings = !0
        }

        function D(e, t) {
          e.quality = v(t.quality), e.resolution = w(t.resolution), e.framerate = E(t.framerate), e.bitrate = _(t
              .bitrateBps), e.rememberedResolution = e.resolution, e.rememberedFramerate = e.framerate, e
            .rememberedBitrate = e.bitrate, N.feature === m.INSTANTREPLAY && (N.replayTime = t.replayLengthSeconds /
              60, L.info("replayTime ", N.replayTime)), L.info("Current Settings: ", e)
        }
        var N = this;
        N.feature = "", N.source = "", N.disabled = !1, N.broadcast2KSupported = d.isBroadcast2KSupported(), N
          .service = d, N.broadcastPortals = c.portals, N.sortedPortals = r.sortBy(N.broadcastPortals, "name"), N
          .broadcastPortals = {}, N.sortedPortals.forEach(function(e) {
            N.broadcastPortals[e.name] = e
          }), N.settingsData = {}, N.settings = {}, N.replayTime = .5, N.callback = "";
        var L = n.getInstance("main.customize/customizecontroller"),
          F = !1,
          U = !1;
        N.useDefaultBitRate = !1, N.support8K60 = d.get8K60Support(), N.Qualities = {
          Low: {
            id: "Low",
            title: "l10n.low",
            icon: "icon-low",
            initialFocus: "false",
            supported: !0
          },
          Medium: {
            id: "Medium",
            title: "l10n.medium",
            icon: "icon-medium",
            initialFocus: "false",
            supported: !0
          },
          High: {
            id: "High",
            title: "l10n.high",
            icon: "icon-high",
            initialFocus: "false",
            supported: !0
          },
          Ultra: {
            id: "Ultra",
            title: "l10n.ultra",
            icon: "icon-ultra",
            initialFocus: "false",
            supported: N.feature === m.BROADCAST
          },
          Custom: {
            id: "Custom",
            title: "l10n.custom",
            icon: "icon-custom",
            initialFocus: "false",
            supported: !0
          }
        }, N.Resolutions = [{
          id: 1,
          name: "In-game",
          title: "l10n.inGame",
          supported: N.feature !== m.BROADCAST
        }, {
          id: 2,
          name: "4320p 8K",
          title: "4320p 8K",
          supported: N.feature !== m.BROADCAST
        }, {
          id: 3,
          name: "2160p 4K",
          title: "2160p 4K",
          supported: N.feature !== m.BROADCAST
        }, {
          id: 4,
          name: "1440p HD",
          title: "1440p HD",
          supported: N.feature !== m.BROADCAST
        }, {
          id: 5,
          name: "1080p HD",
          title: "1080p HD",
          supported: !0
        }, {
          id: 6,
          name: "720p HD",
          title: "720p HD",
          supported: !0
        }, {
          id: 7,
          name: "480p",
          title: "480p",
          supported: !0
        }, {
          id: 8,
          name: "360p",
          title: "360p",
          supported: !0
        }], N.Framerates = [{
          id: 1,
          name: "30",
          title: "30 FPS",
          supported: !0
        }, {
          id: 2,
          name: "60",
          title: "60 FPS",
          supported: !0
        }], N.ReplayLengthSlider = {
          min: .25,
          max: 20,
          step: .25,
          ticks: [.25, 5, 10, 15, 20]
        }, N.BitrateSlider = {
          min: N.feature === m.BROADCAST ? 1 : 10,
          max: N.feature === m.BROADCAST ? N.broadcast2KSupported === !0 ? 18 : 9 : 130,
          step: N.feature === m.BROADCAST ? N.broadcast2KSupported === !0 ? 1 : .5 : 5,
          getTicks: function() {
            var e = [],
              t = 1;
            this.max > 100 && (t = 2), N.support8K60 && N.feature !== m.BROADCAST && (t = 4);
            for (var n = this.min; n <= this.max; n += this.step * t) e.push(n);
            return e
          }
        };
        var z = function(e) {
          var t = this;
          return e ? t = e : (t.quality = null, t.bitrate = null, t.resolution = null, t.framerate = null, t
            .rememberedResolution = null, t.rememberedFramerate = null, t.rememberedBitrate = null, t.portal =
            null, t.updateSettings = !1), t
        };
        N.supportsOnlyOneOption = function(e) {
          return 1 === r.where(e, {
            supported: !0
          }).length
        }, N.selectQuality = function(e) {
          if (U !== !1 && N.disabled !== !0 && e !== N.settings.quality) {
            N.settings.quality = e;
            var t = x(e);
            N.Framerates[1].supported = !0, A(t).then(function(e) {
              N.settings.updateSettings = !0, L.info("quality change done"), N.feature === m.BROADCAST && M()
            })
          }
        }, N.isActiveQuality = function(e) {
          return N.settings.quality === e
        }, N.setBroadcastPortal = function(e) {
          U !== !1 && N.disabled !== !0 && (N.selectedPortal = e, R(), N.refreshParams())
        }, N.isActivePortal = function(e) {
          return N.selectedPortal === e
        }, N.resolutionChanged = function() {
          return N.settings.quality = N.Qualities.Custom, N.feature === m.BROADCAST && M(), N.useDefaultBitRate = !
            1, O(!1), N.settings.rememberedResolution = N.settings.resolution, N.settings.rememberedFramerate = N
            .settings.framerate, N.settings.rememberedBitrate = N.settings.bitrate, C(N.useDefaultBitRate).then(
              function(e) {
                N.settings.updateSettings = !0
              })
        }, N.fpsChanged = function() {
          P()
        }, N.bitrateChanged = function() {
          P()
        }, N.replayTimeChanged = function() {
          N.settings.updateSettings = !0
        }, N.refreshParams = function() {
          if (N.feature === m.BROADCAST) {
            N.settings = N.settingsData[N.selectedPortal.name], M(), I();
            var e = r.findIndex(N.Resolutions, {
              name: "1080p HD"
            });
            N.Qualities.Ultra.supported = N.Resolutions[e].supported
          } else N.settings = N.settingsData.default;
          return C(!1)
        }, N.save = function() {
          var n = t.all(null);
          r.each(N.settingsData, function(e) {
            if (e.updateSettings) {
              var t = N.saveSettingsFunctionCreator(e);
              n = n.then(t)
            }
          }), n.then(function() {
            e.onCustomizeCloseComplete()
          })
        }, N.back = function() {
          N.source === g.PREFERENCES ? N.save() : e.onCustomizeCloseComplete()
        }, N.getSettingsFunctionCreator = function(e) {
          return function() {
            var t = r.isNull(e.portal) ? void 0 : e.portal.id;
            return N.feature === m.BROADCAST ? d.getCurrentSettingsBR(t).then(function(t) {
              D(e, t)
            }) : N.feature === m.INSTANTREPLAY ? d.getCurrentSettingsIR().then(function(t) {
              D(e, t)
            }) : d.getCurrentSettingsMR().then(function(t) {
              D(e, t)
            })
          }
        }, N.saveSettingsFunctionCreator = function(e) {
          return function() {
            var t = x(e.quality),
              n = {};
            return "Custom" !== t ? (N.feature === m.INSTANTREPLAY && (n.replayLengthSeconds = T(N.replayTime)), n
              .quality = t) : (P(), N.feature === m.INSTANTREPLAY && (n.replayLengthSeconds = T(N.replayTime)),
              n.quality = t, n.resolution = y(e.rememberedResolution), n.framerate = S(e.rememberedFramerate), n
              .bitrate = k(e.rememberedBitrate)), N.feature === m.BROADCAST && (n.provider = e.portal
              .portalIdentifier), L.info("New Settings: ", e), d.setQualitySettings(t, N.feature, n)
          }
        }, N.estimateVideoSize = function() {
          var e = "l10n.instantReplayTime1",
            t = "l10n.instantReplayTime2",
            n = "l10n.instantReplayTime2a",
            o = "l10n.instantReplayTime3",
            r = "l10n.instantReplayTime3a",
            a = "l10n.memoryInMB",
            l = 60 * N.replayTime * N.settings.bitrate / 8;
          l > 1e3 && (a = "l10n.memoryInGB", l /= 1e3);
          var s = i("translate")(a, {
              arg1: parseFloat(l.toFixed(1))
            }),
            d = Math.floor(N.replayTime),
            c = 60 * (N.replayTime - d),
            u = d === N.replayTime,
            f = 0 === d,
            m = 1 === d;
          return f === !0 ? i("translate")(e, {
            arg1: c.toString(),
            arg2: s
          }) : u === !0 && 0 == m ? i("translate")(t, {
            arg1: d.toString(),
            arg2: s
          }) : u === !0 && 1 == m ? i("translate")(n, {
            arg1: d.toString(),
            arg2: s
          }) : 0 == m ? i("translate")(o, {
            arg1: d.toString(),
            arg2: c.toString(),
            arg3: s
          }) : i("translate")(r, {
            arg1: d.toString(),
            arg2: c.toString(),
            arg3: s
          })
        }, N.enableEscapeEvent = function(e) {
          var t = e;
          a(function() {
            t === !0 ? s.on(p.ESCAPE, N.back) : s.off(p.ESCAPE, N.back)
          }, 200)
        }, N.selectionOpen = function() {
          N.enableEscapeEvent(!1)
        }, N.selectionClose = function() {
          F === !0 && N.enableEscapeEvent(!0)
        }, N.initInProgress = function() {
          return U === !1 || N.disabled === !0
        }, N.initialize = function() {
          switch (L.info("Initialize Customize VM"), N.feature) {
            case m.INSTANTREPLAY:
            case m.MANUALRECORD:
              N.settingsData.default = new z;
              break;
            case m.BROADCAST:
              r.each(N.broadcastPortals, function(e, t) {
                N.settingsData[t] = new z, N.settingsData[t].portal = e
              });
              break;
            default:
              return
          }
          d.getSupportedResolutions().then(function(e) {
            return N.Resolutions.forEach(function(t) {
              e.indexOf(t.name) === -1 && (t.supported = !1)
            }), d.getSupportedFramerates()
          }).then(function(e) {
            N.Framerates.forEach(function(t) {
              var n = parseInt(t.name, 10);
              e.indexOf(n) === -1 && (t.supported = !1)
            });
            var n = t.all(null);
            r.each(N.settingsData, function(e) {
              var t = N.getSettingsFunctionCreator(e);
              n = n.then(t)
            }), n.then(function() {
              return N.feature === m.BROADCAST && (N.selectedPortal = f.getLastUsedServiceOrDefault(f
                  .serviceTypes.STREAMING)),
                N.refreshParams()
            }).then(function() {
              var e = null;
              N.feature === m.BROADCAST ? (R(), e = N.selectedPortal.providerName) : N.feature === m
                .INSTANTREPLAY ? e = b.instantReplay : N.feature === m.MANUALRECORD && (e = b.manualRecord),
                null !== N.callback && "" !== N.callback && N.callback(e), N.feature !== m.BROADCAST &&
                angular.element(o[0].getElementById("replayTime")).focus(), O(!0), U = !0
            })
          }), N.enableEscapeEvent(!0), F = !0
        };
        var G = null;
        e.getMinWidth = function(e) {
          if (G) return G;
          var t = l.getComputedStyle(e[0]),
            n = angular.element("<canvas>"),
            o = n[0].getContext("2d");
          o.font = t.font || "16px Segoe UI";
          var r = 0;
          for (var a in N.Qualities)
            if (N.Qualities.hasOwnProperty(a)) {
              var s = N.Qualities[a];
              if (s.supported) {
                var d = i("translate")(s.title),
                  c = o.measureText(d).width;
                c > r && (r = c)
              }
            } return r <= 0 && (r = 124), n.remove(), r = Math.ceil(r), G = r, r
        }, e.$watch("nvChangeSettings", function() {
          void 0 !== e.nvChangeSettings && (N.feature !== e.nvChangeSettings.feature && (L.info("Initialize"), N
            .feature = e.nvChangeSettings.feature, N.source = e.nvChangeSettings.source, N.disabled = e
            .nvChangeSettings.disabled, N.initialize()), e.nvChangeSettings.executeBack === !0 && (e
            .nvChangeSettings.executeBack = !1, N.back()), e.nvChangeSettings.executeSave === !0 && (e
            .nvChangeSettings.executeSave = !1, N.save()), N.callback !== e.nvChangeSettings.callback && (N
            .callback = e.nvChangeSettings.callback))
        }, !0), e.$on("$destroy", function() {
          N.enableEscapeEvent(!1), F = !1
        })
      }
    ]),
    s = a.ngMainModule.directive("getCustomizeWidth", function() {
      return {
        link: function(e, t) {
          t[0].style.minWidth = e.getMinWidth(t) + "px"
        }
      }
    });
  t.SettingsCustomizeController = l, t.getCustomizeWidth = s
}
