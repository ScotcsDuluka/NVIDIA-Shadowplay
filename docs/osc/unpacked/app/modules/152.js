// ─────────────────────────────────────────────────────────────
// APP MODULE 152
// role       : factory webrtcPeerConnectionFactory
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.webrtcPeerConnectionFactory = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.factory("webrtcPeerConnectionFactory", [function() {
      var e = {};
      return e.getUserMedia = function(e, t, n) {
        navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator
          .mozGetUserMedia, navigator.getUserMedia(e, t, n)
      }, e.newRTCPeerConnection = function(e) {
        var t = webkitRTCPeerConnection || mozRTCPeerConnection || RTCPeerConnection;
        return new t(e)
      }, e.newRTCSessionDescription = function(e) {
        return new RTCSessionDescription(e)
      }, e.newRTCIceCandidate = function(e) {
        return new RTCIceCandidate(e)
      }, e
    }]);
  t.webrtcPeerConnectionFactory = o
}
