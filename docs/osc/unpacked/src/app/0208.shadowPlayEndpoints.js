// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 208
// provider shadowPlayEndpoints | defines angular.module("main.localSdk.shadowPlaySdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.shadowPlaySdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("shadowPlayEndpoints", [function() {
    var e,
      t = "v.1.1";
    return {
      setConfig: function(t) {
        e = t.version;
      },
      $get: ["NvEndpointFactory", "localSdk", function(n, i) {
        var o,
          r,
          a,
          l,
          s,
          d,
          c,
          u,
          f,
          m,
          g,
          p,
          h,
          b,
          x,
          v,
          y,
          w,
          S,
          E,
          k,
          _,
          T,
          C,
          O,
          A,
          I,
          M,
          R,
          P,
          D,
          N,
          L,
          F,
          U,
          z,
          G,
          V,
          H,
          B,
          Y,
          $,
          W,
          j,
          K,
          q,
          X,
          Z,
          Q,
          J,
          ee,
          te,
          ne,
          ie,
          oe,
          re,
          ae,
          le,
          se,
          de,
          ce,
          ue,
          fe,
          me,
          ge,
          pe,
          he,
          be,
          xe,
          ve,
          ye,
          we,
          Se,
          Ee,
          ke,
          _e,
          Te,
          Ce,
          Oe,
          Ae,
          Ie,
          Me,
          Re,
          Pe,
          De,
          Ne,
          Le,
          Fe,
          Ue,
          ze,
          Ge,
          Ve,
          He,
          Be,
          Ye,
          $e,
          We,
          je,
          Ke,
          qe,
          Xe,
          Ze,
          Qe,
          Je,
          et,
          tt,
          nt,
          it,
          ot,
          rt,
          at,
          lt = i.newLocalEndpointFactory("ShadowPlay", e),
          st = i.newLocalEndpointFactory("ShadowPlay", t);
        return o = lt.createEndpoint({
          url: "",
          method: "GET"
        }), r = lt.createEndpoint({
          url: "/Launch",
          method: "POST",
          data: {
            launch: ""
          }
        }), a = lt.createEndpoint({
          url: "/Launch",
          method: "GET"
        }), l = lt.createEndpoint({
          url: "/InstantReplay/Enable",
          method: "GET"
        }), s = lt.createEndpoint({
          url: "/InstantReplay/Running",
          method: "GET"
        }), d = lt.createEndpoint({
          url: "/InstantReplay/Enable",
          method: "POST",
          data: {
            status: ""
          }
        }), c = lt.createEndpoint({
          url: "/InstantReplay/Save",
          method: "POST"
        }), u = lt.createEndpoint({
          url: "/InstantReplay/Upload",
          method: "POST"
        }), f = lt.createEndpoint({
          url: "/InstantReplay/BufferLength",
          method: "GET"
        }), g = lt.createEndpoint({
          url: "/InstantReplay/Settings",
          method: "GET"
        }), m = lt.createEndpoint({
          url: "/InstantReplay/Settings",
          method: "POST",
          data: {
            replayLengthSeconds: "",
            quality: ""
          }
        }), p = lt.createEndpoint({
          url: "/InstantReplay/Settings",
          method: "POST",
          data: {
            replayLengthSeconds: "",
            quality: "",
            resolution: "",
            framerate: "",
            bitrateBps: ""
          }
        }), h = lt.createEndpoint({
          url: "/Record/Enable",
          method: "GET"
        }), b = lt.createEndpoint({
          url: "/Record/Running",
          method: "GET"
        }), x = lt.createEndpoint({
          url: "/Record/Enable",
          method: "POST",
          data: {
            status: ""
          }
        }), v = lt.createEndpoint({
          url: "/Record/Settings",
          method: "GET"
        }), y = lt.createEndpoint({
          url: "/Record/Settings",
          method: "POST",
          data: {
            quality: ""
          }
        }), w = lt.createEndpoint({
          url: "/Record/Settings",
          method: "POST",
          data: {
            quality: "",
            resolution: "",
            framerate: "",
            bitrateBps: ""
          }
        }), S = lt.createEndpoint({
          url: "/Record/Concurrency/Broadcast",
          method: "GET"
        }), E = lt.createEndpoint({
          url: "/GetHDRState",
          method: "GET"
        }), k = lt.createEndpoint({
          url: "/Record/Concurrency/Gamestream",
          method: "GET"
        }), _ = lt.createEndpoint({
          url: "/Resolutions",
          method: "GET"
        }), T = lt.createEndpoint({
          url: "/FrameRates",
          method: "GET"
        }), C = lt.createEndpoint({
          url: "/Resolutions/:quality",
          method: "GET",
          params: {
            quality: ""
          }
        }), O = lt.createEndpoint({
          url: "/Framerates/:quality",
          method: "GET",
          params: {
            quality: ""
          }
        }), A = lt.createEndpoint({
          url: "/BitRates/:quality/:resolution",
          method: "GET",
          params: {
            quality: "",
            resolution: ""
          }
        }), I = lt.createEndpoint({
          url: "/Broadcast/Support",
          method: "GET"
        }), M = lt.createEndpoint({
          url: "/Broadcast/Enable",
          method: "GET"
        }), R = lt.createEndpoint({
          url: "/Broadcast/Enable",
          method: "POST",
          data: {
            status: ""
          }
        }), P = lt.createEndpoint({
          url: "/Broadcast/Pause",
          method: "POST",
          data: {
            pause: ""
          }
        }), D = lt.createEndpoint({
          url: "/Broadcast/SessionParam",
          method: "POST",
          data: {
            sessionUrl: "",
            provider: ""
          }
        }), N = st.createEndpoint({
          url: "/Broadcast/SessionParam/:type",
          method: "POST",
          params: {
            type: ""
          },
          data: {
            sessionUrl: "",
            provider: "",
            port: ""
          }
        }), L = lt.createEndpoint({
          url: "/Broadcast/Settings",
          method: "GET"
        }), F = lt.createEndpoint({
          url: "/Broadcast/Settings",
          method: "POST",
          data: {
            quality: "",
            provider: ""
          }
        }), U = lt.createEndpoint({
          url: "/Broadcast/Settings",
          method: "POST",
          data: {
            provider: "",
            quality: "",
            resolution: "",
            framerate: "",
            bitrateBps: ""
          }
        }), z = lt.createEndpoint({
          url: "/Broadcast/Title",
          method: "GET"
        }), G = lt.createEndpoint({
          url: "/Broadcast/Provider",
          method: "GET"
        }), V = lt.createEndpoint({
          url: "/Broadcast/Provider",
          method: "POST",
          data: {
            provider: ""
          }
        }), H = lt.createEndpoint({
          url: "/Broadcast/IngestServer",
          method: "GET"
        }), B = lt.createEndpoint({
          url: "/Broadcast/IngestServer",
          method: "POST",
          data: {
            ingestserver: ""
          }
        }), Y = lt.createEndpoint({
          url: "/Broadcast/Viewers/Max",
          method: "POST",
          data: {
            count: ""
          }
        }), $ = lt.createEndpoint({
          url: "/Broadcast/Running",
          method: "GET"
        }), W = lt.createEndpoint({
          url: "/CustomOverlay/Support",
          method: "GET"
        }), j = lt.createEndpoint({
          url: "/CustomOverlay/Enable",
          method: "GET"
        }), K = lt.createEndpoint({
          url: "/CustomOverlay/Enable",
          method: "POST",
          data: {
            enable: ""
          }
        }), q = lt.createEndpoint({
          url: "/CustomOverlay/Path",
          method: "GET"
        }), X = lt.createEndpoint({
          url: "/CustomOverlay/Path",
          method: "POST",
          data: {
            path: ""
          }
        }), Q = st.createEndpoint({
          url: "/CustomOverlay/Enable/:index",
          method: "GET",
          params: {
            index: ""
          }
        }), J = st.createEndpoint({
          url: "/CustomOverlay/Enable/:index",
          method: "POST",
          params: {
            index: ""
          },
          data: {
            enable: ""
          }
        }), ee = st.createEndpoint({
          url: "/CustomOverlay/Path/:index",
          method: "GET",
          params: {
            index: ""
          }
        }), te = st.createEndpoint({
          url: "/CustomOverlay/Path/:index",
          method: "POST",
          params: {
            index: ""
          },
          data: {
            path: ""
          }
        }), Z = lt.createEndpoint({
          url: "/CustomOverlay/DefaultPath",
          method: "GET"
        }), ne = lt.createEndpoint({
          url: "/CustomOverlay/Display",
          method: "GET"
        }), ie = lt.createEndpoint({
          url: "/Broadcast/Viewers",
          method: "POST",
          data: {
            ViewerCountImage: "",
            ImageHeight: "",
            ImageWidth: ""
          }
        }), re = lt.createEndpoint({
          url: "/Broadcast/2KSupport",
          method: "GET"
        }), ae = lt.createEndpoint({
          url: "/Broadcast/2KEnable",
          method: "GET"
        }), le = lt.createEndpoint({
          url: "/Microphone/Present",
          method: "GET"
        }), se = lt.createEndpoint({
          url: "/Microphone",
          method: "GET"
        }), de = lt.createEndpoint({
          url: "/Microphone",
          method: "POST",
          data: {
            mode: ""
          }
        }), ce = lt.createEndpoint({
          url: "/Microphone/PTT",
          method: "POST",
          data: {
            mode: ""
          }
        }), ue = lt.createEndpoint({
          url: "/Microphone/Settings",
          method: "GET"
        }), fe = lt.createEndpoint({
          url: "/Microphone/:index/Settings",
          method: "GET",
          params: {
            index: ""
          }
        }), me = lt.createEndpoint({
          url: "/Microphone/:index/Settings",
          method: "POST",
          params: {
            index: ""
          },
          data: {
            muted: "",
            volumePercent: "",
            boostPercent: ""
          }
        }), ge = lt.createEndpoint({
          url: "/Audio",
          method: "GET"
        }), pe = lt.createEndpoint({
          url: "/Audio",
          method: "POST",
          data: {
            mode: ""
          }
        }), he = lt.createEndpoint({
          url: "/AudioSettings",
          method: "GET"
        }), be = lt.createEndpoint({
          url: "/AudioSettings",
          method: "POST",
          data: {
            systemVolumePercent: "",
            separateTracks: ""
          }
        }), xe = lt.createEndpoint({
          url: "/4KSupport",
          method: "GET"
        }), ve = lt.createEndpoint({
          url: "/8k60",
          method: "GET"
        }), ye = lt.createEndpoint({
          url: "/Webcam/Present",
          method: "GET"
        }), we = lt.createEndpoint({
          url: "/Webcam/Enable",
          method: "GET"
        }), Se = lt.createEndpoint({
          url: "/Webcam/Enable",
          method: "POST",
          data: {
            status: ""
          }
        }), Ee = lt.createEndpoint({
          url: "/Webcam/Toggle",
          method: "POST"
        }), ke = lt.createEndpoint({
          url: "/Webcam/Shown",
          method: "GET"
        }), _e = lt.createEndpoint({
          url: "/DesktopCapture/Enable",
          method: "GET"
        }), Te = lt.createEndpoint({
          url: "/DesktopCapture/Enable",
          method: "POST",
          data: {
            enable: ""
          }
        }), Ce = lt.createEndpoint({
          url: "/DesktopCapture/Support",
          method: "GET"
        }), Oe = lt.createEndpoint({
          url: "/DesktopCapture/Support/Reason",
          method: "GET"
        }), Ae = lt.createEndpoint({
          url: "/CoPlay/Enable",
          method: "GET"
        }), Ie = lt.createEndpoint({
          url: "/CoPlay/Enable",
          method: "POST",
          data: {
            enable: ""
          }
        }), Me = lt.createEndpoint({
          url: "/CoPlay/Support",
          method: "GET"
        }), Re = lt.createEndpoint({
          url: "/Capture/State",
          method: "GET"
        }), Pe = lt.createEndpoint({
          url: "/Capture/PIDMode",
          method: "GET"
        }), De = function(e) {
          return e = e || 0, lt.createEndpoint({
            url: "/Capture/ProcessInfo/:PID",
            method: "GET",
            params: {
              PID: e
            }
          });
        }, Ne = lt.createEndpoint({
          url: "/Webcam/Settings",
          method: "GET"
        }), Le = lt.createEndpoint({
          url: "/Webcam/Settings",
          method: "POST",
          data: {
            enable: "",
            position: "",
            size: ""
          }
        }), Fe = lt.createEndpoint({
          url: "/Indicator/:id/Support",
          method: "GET",
          params: {
            id: ""
          }
        }), Ue = lt.createEndpoint({
          url: "/Indicator/:id/Settings",
          method: "GET",
          params: {
            id: ""
          }
        }), ze = lt.createEndpoint({
          url: "/Indicator/:id/Settings",
          method: "POST",
          params: {
            id: ""
          },
          data: {
            enable: "",
            position: ""
          }
        }), Ge = lt.createEndpoint({
          url: "/RecordPaths",
          method: "GET"
        }), Ve = lt.createEndpoint({
          url: "/RecordPaths",
          method: "POST",
          data: {
            videos: "",
            tempFiles: ""
          }
        }), He = lt.createEndpoint({
          url: "/Hotkey/:hk",
          method: "GET",
          params: {
            hk: ""
          }
        }), Be = lt.createEndpoint({
          url: "/Hotkey/:hk",
          method: "POST",
          params: {
            hk: ""
          },
          data: {
            keys: ""
          }
        }), Ye = lt.createEndpoint({
          url: "/Hotkey/Monitor",
          method: "GET"
        }), $e = lt.createEndpoint({
          url: "/Hotkey/Monitor",
          method: "POST",
          data: {
            enable: ""
          }
        }), je = lt.createEndpoint({
          url: "/Screenshot/Support",
          method: "GET"
        }), Ke = lt.createEndpoint({
          url: "/Screenshot/Capture",
          method: "POST",
          data: {
            scale: "",
            effect: ""
          }
        }), qe = lt.createEndpoint({
          url: "/Screenshot/NGXShot",
          method: "POST",
          data: {
            scale: "",
            path: ""
          }
        }), Xe = lt.createEndpoint({
          url: "/Screenshot/NGXCancelShot",
          method: "POST"
        }), We = lt.createEndpoint({
          url: "/Osc",
          method: "POST",
          data: {
            ready: ""
          }
        }), Ze = lt.createEndpoint({
          url: "/Video/Trim",
          method: "POST",
          data: {
            input: "",
            output: "",
            headTrimMs: "",
            lengthMs: ""
          }
        }), Qe = lt.createEndpoint({
          url: "/Input",
          method: "POST",
          data: {
            redirect: "",
            type: "",
            hid: ""
          }
        }), tt = lt.createEndpoint({
          url: "/OSC/Init",
          method: "GET"
        }), et = lt.createEndpoint({
          url: "/OSC/MainView",
          method: "POST",
          data: {
            fetchPartial: ""
          }
        }), Je = lt.createEndpoint({
          url: "/Hotkey/DynamicToggle",
          method: "POST",
          data: {
            enable: "",
            hotkeyNames: ""
          }
        }), nt = lt.createEndpoint({
          url: "/Highlights/Customize",
          method: "GET"
        }), it = lt.createEndpoint({
          url: "/Highlights/Customize",
          method: "POST",
          data: {
            sizeMB: ""
          }
        }), ot = lt.createEndpoint({
          url: "/Highlights/Customize",
          method: "POST",
          data: {
            tempSaveFolder: ""
          }
        }), rt = lt.createEndpoint({
          url: "/Highlights/GalleryImport",
          method: "POST",
          data: {
            file: "",
            property: "",
            gameName: "",
            groupId: "",
            id: "",
            headTrimMs: "",
            lengthMs: ""
          }
        }), at = lt.createEndpoint({
          url: "/Highlights/Session",
          method: "GET"
        }), {
          getFullShadowPlayUrl: lt.generateFullUrl,
          get: o,
          launch: r,
          isRunning: a,
          getIREnableStatus: l,
          getIRRunningStatus: s,
          setInstantReplayRecording: d,
          saveInstantReplay: c,
          uploadInstantReplay: u,
          getInstantReplayBufferLength: f,
          getInstantReplaySettings: g,
          setInstantReplayPresetSettings: m,
          setInstantReplaySettings: p,
          getMREnableStatus: h,
          getMRRunningStatus: b,
          setManualRecording: x,
          getManualRecordSettings: v,
          setManualRecordPresetSettings: y,
          setManualRecordSettings: w,
          getRecordBroadcastConcurrencySupport: S,
          getHDRActiveState: E,
          getRecordGamestreamConcurrencySupport: k,
          getSupportedResolutions: _,
          getSupportedFramerates: T,
          getDefaultResolution: C,
          getDefaultFramerate: O,
          getBitrateRange: A,
          getBroadcastSupportedStatus: I,
          getBroadcastSessionStatus: M,
          setBroadcastSessionStatus: R,
          pauseBroadcastSession: P,
          setBroadcastSessionParam: D,
          setBroadcastSessionParamV1: N,
          getBroadcastSettings: L,
          setBroadcastPresetSettings: F,
          setBroadcastSettings: U,
          getBroadcastTitle: z,
          getBroadcastProvider: G,
          setBroadcastProvider: V,
          getBroadcastIngestServer: H,
          setBroadcastIngestServer: B,
          setBroadcastViewerCountMax: Y,
          getBroadcastRunningStatus: $,
          getCustomOverlaySupportType: W,
          getCustomOverlayEnabled: j,
          setCustomOverlayEnabled: K,
          getCustomOverlayPath: q,
          setCustomOverlayPath: X,
          getCustomOverlayEnabledV1: Q,
          setCustomOverlayEnabledV1: J,
          getCustomOverlayPathV1: ee,
          setCustomOverlayPathV1: te,
          getCustomOverlayDisplayState: ne,
          getCustomOverlayDefaultPath: Z,
          updateBroadcastViewerCountImage: ie,
          setBroadcastMaxViewerCount: oe,
          getBroadcast2KSupported: re,
          getBroadcast2KEnabled: ae,
          getMicrophoneCount: le,
          getMicrophoneMode: se,
          setMicrophoneMode: de,
          setMicrophoneOnOff: ce,
          getMicrophoneSelectedSettings: ue,
          getMicrophoneSettings: fe,
          setMicrophoneSettings: me,
          getAudioMode: ge,
          setAudioMode: pe,
          getAudioSettings: he,
          setAudioSettings: be,
          get4KSupport: xe,
          get8K60Support: ve,
          getWebcamPresent: ye,
          getWebcamMode: we,
          setWebcamMode: Se,
          toggleWebcamDisplay: Ee,
          getWebcamShown: ke,
          getDesktopCaptureEnabled: _e,
          setDesktopCaptureEnabled: Te,
          getDesktopCaptureSupported: Ce,
          getDesktopCaptureSupportReason: Oe,
          getCoplayEnabled: Ae,
          setCoplayEnabled: Ie,
          getCoplaySupported: Me,
          getCaptureState: Re,
          getCaptureControlPIDMode: Pe,
          getCaptureProcessInfo: De,
          getWebcamOverlaySettings: Ne,
          setWebcamOverlaySettings: Le,
          getIndicatorOverlaySupported: Fe,
          getIndicatorOverlaySettings: Ue,
          setIndicatorOverlaySettings: ze,
          getRecordingPaths: Ge,
          setRecordingPaths: Ve,
          getHotkeyShortcut: He,
          setHotkeyShortcut: Be,
          getHotkeyMonitoringEnabled: Ye,
          setHotkeyMonitoringEnabled: $e,
          getScreenshotSupported: je,
          captureScreenshot: Ke,
          captureNGXShot: qe,
          cancelNGXShot: Xe,
          setOscReady: We,
          videoTrim: Ze,
          getMainViewData: et,
          getInitData: tt,
          setInputRedirection: Qe,
          dynamicHotkeyToggle: Je,
          getCustomize: nt,
          setCustomizeSize: it,
          setCustomizePath: ot,
          importHighlightToGallery: rt,
          getHighlightsActive: at
        };
      }]
    };
  }]), exports.ngShadowplaySdkModule = n;
}
