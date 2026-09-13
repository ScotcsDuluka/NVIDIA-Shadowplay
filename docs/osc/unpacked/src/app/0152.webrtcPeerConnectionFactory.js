// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 152
// factory webrtcPeerConnectionFactory
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.webrtcPeerConnectionFactory = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.factory("webrtcPeerConnectionFactory", [function() {
      var e = {};
      return e.getUserMedia = function(e, t, n) {
        navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator
          .mozGetUserMedia, navigator.getUserMedia(e, t, n);
      }, e.newRTCPeerConnection = function(e) {
        var t = webkitRTCPeerConnection || mozRTCPeerConnection || RTCPeerConnection;
        return new t(e);
      }, e.newRTCSessionDescription = function(e) {
        return new RTCSessionDescription(e);
      }, e.newRTCIceCandidate = function(e) {
        return new RTCIceCandidate(e);
      }, e;
    }]);
  exports.webrtcPeerConnectionFactory = o;
}
