// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 205
// provider nvCameraEndpoints | defines angular.module("main.localSdk.nvCameraSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.localSdk.nvCameraSdk", ["nvAngularHttpEndpoint", "main.common"]);
  n.provider("nvCameraEndpoints", [function() {
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
          B = i.newLocalEndpointFactory("NvCamera", e),
          Y = i.newLocalEndpointFactory("NvCamera", t);
        return o = B.createEndpoint({
          url: "",
          method: "GET"
        }), r = B.createEndpoint({
          url: "/GetAvailable",
          method: "POST"
        }), a = B.createEndpoint({
          url: "/Compatible",
          method: "GET"
        }), l = B.createEndpoint({
          url: "/GetIntegration",
          method: "POST"
        }), s = B.createEndpoint({
          url: "/Capture/GetTypes",
          method: "POST"
        }), d = B.createEndpoint({
          url: "/Capture/Control",
          method: "POST"
        }), c = B.createEndpoint({
          url: "/Capture/Control",
          method: "POST",
          data: {
            capture: "",
            pauseOnEnable: "",
            leaveFiltersEnabled: ""
          }
        }), u = B.createEndpoint({
          url: "/GetFilters",
          method: "POST"
        }), f = B.createEndpoint({
          url: "/GameEngine",
          method: "POST"
        }), m = B.createEndpoint({
          url: "/",
          method: "POST",
          data: {
            type: "",
            resolutionMultiplier: "",
            width: "",
            height: "",
            panoramaResolutionW: "",
            panoramaResolutionH: "",
            saveAsExr: "",
            enhance: !1
          }
        }), g = B.createEndpoint({
          url: "/Cancel",
          method: "POST"
        }), p = B.createEndpoint({
          url: "/Capture/GetResolutions/:type",
          method: "POST",
          params: {
            type: ""
          },
          data: {}
        }), h = B.createEndpoint({
          url: "/Camera/GetRange",
          method: "POST",
          data: {
            roll: !0,
            fov: !0
          }
        }), b = B.createEndpoint({
          url: "/Camera/GetAdjust",
          method: "POST",
          data: {
            fov: !0
          }
        }), x = B.createEndpoint({
          url: "/Camera/Adjust",
          method: "POST",
          data: {
            roll: "",
            fov: ""
          }
        }), v = B.createEndpoint({
          url: "/Filter",
          method: "POST",
          data: {
            type: "",
            stackIdx: ""
          }
        }), y = B.createEndpoint({
          url: "/Filter/Remove",
          method: "POST",
          data: {
            stackIdx: ""
          }
        }), w = B.createEndpoint({
          url: "/Filter/ResetStack",
          method: "POST"
        }), S = B.createEndpoint({
          url: "/Filter/:id/Attribute",
          method: "POST",
          params: {
            id: ""
          },
          data: {
            controlId: "",
            value: "",
            stackIdx: ""
          }
        }), E = Y.createEndpoint({
          url: "/Filter/:id/Attribute",
          method: "POST",
          params: {
            id: ""
          },
          data: {
            controlId: "",
            type: "",
            value: "",
            stackIdx: "",
            dataType: ""
          }
        }), k = B.createEndpoint({
          url: "/Filter/:id/SetFilterAndAttributes",
          method: "POST",
          params: {
            id: ""
          },
          data: {
            controls: "",
            stackIdx: ""
          }
        }), _ = B.createEndpoint({
          url: "/SetFilterAndAttributesSupported",
          method: "GET"
        }), T = B.createEndpoint({
          url: "/Filter/GetInfo",
          method: "POST",
          data: {
            stackIdx: ""
          }
        }), C = B.createEndpoint({
          url: "/Filter/ResetAll",
          method: "POST"
        }), O = B.createEndpoint({
          url: "/Filter/Reset",
          method: "POST",
          data: {
            stackIdx: ""
          }
        }), A = B.createEndpoint({
          url: "/Language",
          method: "POST",
          data: {
            langId: ""
          }
        }), I = B.createEndpoint({
          url: "/IPC",
          method: "POST",
          data: {
            enable: ""
          }
        }), M = B.createEndpoint({
          url: "/EnableMods",
          method: "POST",
          data: {
            globalEnable: ""
          }
        }), R = B.createEndpoint({
          url: "/uiReady",
          method: "GET"
        }), P = B.createEndpoint({
          url: "/uiControlChanged",
          method: "POST",
          data: {
            controlId: "",
            type: "",
            data: ""
          }
        }), D = B.createEndpoint({
          url: "/reportControlVisibility",
          method: "POST",
          data: {
            isVisible: ""
          }
        }), N = B.createEndpoint({
          url: "/GetProcessInfo",
          method: "GET"
        }), L = B.createEndpoint({
          url: "/GridOfThirds",
          method: "POST",
          data: {
            enable: ""
          }
        }), F = B.createEndpoint({
          url: "/GetFreestyleSupport",
          method: "POST",
          data: {
            profileName: ""
          }
        }), U = B.createEndpoint({
          url: "/GetNvCameraConfig",
          method: "GET"
        }), z = B.createEndpoint({
          url: "/ReshadeSupported",
          method: "GET"
        }), G = B.createEndpoint({
          url: "/Filter/setMultipleFiltersAndAttributes",
          method: "POST",
          data: {
            filters: ""
          }
        }), V = B.createEndpoint({
          url: "/SetMultipleFilterAPISupport",
          method: "GET"
        }), H = B.createEndpoint({
          url: "/SetSharpnessForApp",
          method: "POST",
          data: {
            sharpness: ""
          }
        }), {
          getFullScreenshotUrl: B.generateFullUrl,
          get: o,
          getAvailableFlag: r,
          getCompatibilityInfo: a,
          getGameIntegrationFlag: l,
          getCaptureTypes: s,
          getCaptureControl: d,
          setCaptureControl: c,
          getSupportedFilterTypes: u,
          setReadyForGameEngine: f,
          captureScreenshot: m,
          cancelScreenshotCapture: g,
          getCaptureTypeResolutions: p,
          getCameraAdjustmentsRange: h,
          getCameraAdjustments: b,
          setCameraAdjustments: x,
          setFilterType: v,
          removeFilterAtIndex: y,
          resetEntireStack: w,
          setAttribute: S,
          setAttributeV1: E,
          setFilterAndAttributes: k,
          checkFilterAndAttributesAPISupport: _,
          getFilterAttributes: T,
          resetAllFilters: C,
          resetFilter: O,
          setLanguage: A,
          setIPCEnabled: I,
          setModsEnabled: M,
          uiReady: R,
          controlChanged: P,
          reportVisibility: D,
          getProcessInfo: N,
          setGridOfThirdsStatus: L,
          getFreeStyleSupportForGame: F,
          getNvCameraConfigValues: U,
          getNvCameraReshadeSupport: z,
          setMultipleFiltersAndAttributes: G,
          getMultipleFilterAPISupport: V,
          setSharpnessValuesForGame: H
        };
      }]
    };
  }]), exports.ngNvCameraSdkModule = n;
}
