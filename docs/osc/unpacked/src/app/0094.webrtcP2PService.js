// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 94
// service webrtcP2PService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.webrtcP2PService = void 0;
  var o = require(8),
    r = i(o),
    a = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(152) /* app/152 — webrtcPeerConnectionFactory (factory) */, require(153) /* app/153 — websocketService (provider) */;
  var l = a.ngMainCommonModule.service("webrtcP2PService", ["$log", "$q", "websocketService",
    "webrtcPeerConnectionFactory",
    function(e, t, n, i) {
      var o = {
          iceServers: [{
            url: "stun:stun.l.google.com:19302"
          }, {
            url: "stun:stun1.l.google.com:19302"
          }, {
            url: "stun:stun2.l.google.com:19302"
          }, {
            url: "stun:stun3.l.google.com:19302"
          }, {
            url: "stun:stun4.l.google.com:19302"
          }]
        },
        a = {},
        l = function(t) {
          return function() {
            var n = Array.prototype.slice.call(arguments);
            n.unshift(t), e.info(n);
          };
        },
        s = function(e) {
          var t = "WebRTC";
          return e && (t = null != e.host ? "WebRTC[" + e.id + "." + (e.host ? "host" : "guest") + "]:" :
            "WebRTC[" + e.id + "]:"), l(t);
        },
        d = function(e) {
          var t = this;
          t.id = e, t.host = null, t.incomingSocket = null, t.outgoingSocket = null, t.localVideoCB = [],
            t.remoteVideoCB = [], t.hangupCB = [], t.proxyUrl = null, t.proxySessionId = null, t
            .rtcPeerConnection = null, t.remoteVideoStream = null, t.verbose = !1, t.micMuted = !1, t
            .remoteAudioMuted = !1, t.cameraMuted = !1, t.muteMicrophone = function(e) {
              t.micMuted = e, s(t)("Set microphone mute", e), t.rtcPeerConnection && t.rtcPeerConnection
                .setMicMute(e);
            }, t.muteRemoteAudio = function(e) {
              t.remoteAudioMuted = e, s(t)("Set remote audio mute", e);
              for (var n = 0; n < t.remoteVideoCB.length; ++n) t.remoteVideoCB[n](t.remoteVideoStream, t
                .remoteAudioMuted);
            }, t.muteCamera = function(e) {
              t.cameraMuted = e, s(t)("Set camera mute", e), t.rtcPeerConnection && t.rtcPeerConnection
                .setCamMute(e);
            }, t.initMuteStates = function() {
              t.muteMicrophone(t.micMuted), t.muteCamera(t.cameraMuted);
            }, t.setPreferredAudioDevice = function(e) {
              navigator.mediaDevices.enumerateDevices().then(function(n) {
                var i = _.findWhere(n, {
                  label: e
                });
                if (i)
                  for (var o = i.deviceId, r = 0; r < t.remoteVideoCB.length; ++r) t.remoteVideoCB[r](
                    t.remoteVideoStream, !1, o);
              });
            }, t.onSendRTCData = function(e) {
              t.outgoingSocket && t.outgoingSocket.send((0, r.default)(e));
            }, t.gotRemoteStream = function(e) {
              t.remoteVideoStream = null, e && (t.remoteVideoStream = e.stream);
              for (var n = 0; n < t.remoteVideoCB.length; ++n) t.remoteVideoCB[n](t.remoteVideoStream, t
                .remoteAudioMuted);
            }, t.gotLocalStream = function(e) {
              for (var n = 0; n < t.localVideoCB.length; ++n) t.localVideoCB[n](e, !1);
            };
        },
        c = function(t) {
          var n = this;
          n.localStream = null, n.remoteDesc = null, n.peerConnection = null, n.storedIceCandidates = [],
            n.muteAudio = !1, n.muteVideo = !1;
          var a = function() {
            return t.verbose ? l("WebRTCConnection[" + t.name + "]") : function() {};
          };
          a()("config: " + t.videoEnabled);
          var s = function(e) {
              for (var t in e) {
                var o = i.newRTCIceCandidate(JSON.parse(e[t]));
                a()("Adding remote ICE candidate ", o), n.peerConnection.addIceCandidate(o);
              }
            },
            d = function(e) {
              if (e.candidate) {
                a()("Trickle sending ICE candidate to peer: ", e.candidate);
                var n = [(0, r.default)(e.candidate)],
                  i = {
                    webrtc_type: "ice_trickle",
                    icecandidates: n
                  };
                t.onSendData(i);
              } else {
                a()("Finished sending ICE candidates to peer");
                var o = {
                  webrtc_type: "ice_trickle_done"
                };
                t.onSendData(o);
              }
            },
            c = function(e) {
              return function(i) {
                a()("Got local description"), n.peerConnection.setLocalDescription(i);
                var o = {};
                e ? (o.webrtc_type = "offer", o.do_audio = t.audioEnabled, o.do_video = t
                  .videoEnabled) : o.webrtc_type = "answer", o.sessioninfo = i, t.onSendData(o);
              };
            },
            u = function(e) {
              n.peerConnection = i.newRTCPeerConnection(o), n.peerConnection.onicecandidate = d, n
                .peerConnection.onaddstream = t.onRemoteStream, n.peerConnection.addStream(n.localStream),
                e ? (a()("Creating Offer"), n.peerConnection.createOffer(c(!0), function(e) {
                  a()("offer error: " + e);
                })) : (a()("Creating Answer", n.remoteDesc), n.peerConnection.setRemoteDescription(n
                  .remoteDesc), n.peerConnection.createAnswer(c(!1), function(e) {
                  a()("answer error: " + e);
                }), n.addRemoteIceCandidates([]));
            },
            f = function(e) {
              return function(i) {
                n.localStream = i, i.getVideoTracks().length > 0 && a()("Using video device:" + i
                    .getVideoTracks()[0].label), i.getAudioTracks().length > 0 && a()(
                    "Using audio device:" + i.getAudioTracks()[0].label), t.onLocalStream && t
                  .onLocalStream(i), n.setMicMute(n.muteAudio), n.setCamMute(n.muteVideo), a()(
                    "Got local stream. Create peer connection"), u(e);
              };
            },
            m = function(o) {
              a()("Requesting local stream");
              var r = {};
              r.video = t.videoEnabled, r.audio = !1, t.audioEnabled && (r.audio = {
                optional: [{
                  googDucking: !1
                }]
              }), i.getUserMedia(r, f(o), function(t) {
                e.error(n.TAG + " navigator.getUserMedia error: " + t);
              });
            };
          this.createOffer = function() {
            a()("Creating offer to guest aud=" + t.audioEnabled + " vid=" + t.videoEnabled), m(!0);
          }, this.createAnswer = function(e) {
            a()("Creating answer to host aud=" + t.audioEnabled + " vid=" + t.videoEnabled, e), n
              .remoteDesc = i.newRTCSessionDescription(e), m(!1);
          }, this.beginCall = function(e) {
            a()("Accepting answer from guest"), n.remoteDesc = i.newRTCSessionDescription(e), n
              .peerConnection.setRemoteDescription(n.remoteDesc), n.addRemoteIceCandidates([]);
          }, this.addRemoteIceCandidates = function(e) {
            n.peerConnection && n.remoteDesc ? (n.storedIceCandidates.length > 0 && (a()(
                "Passing in stored ICE candidates n=" + n.storedIceCandidates.length), s(n
                .storedIceCandidates), n.storedIceCandidates = []), s(e)) : n.storedIceCandidates = n
              .storedIceCandidates.concat(e);
          }, this.doneRemoteIceCandidates = function() {
            a()("Peer has finished sending us its ICE candidates");
          }, this.setMicMute = function(e) {
            a()("setting microphone mute=" + e), n.muteAudio = e, n.localStream && n.localStream
              .getAudioTracks()[0] && (n.localStream.getAudioTracks()[0].enabled = !e);
          }, this.setCamMute = function(e) {
            a()("setting camera mute=" + e), n.muteVideo = e, n.localStream && n.localStream
              .getVideoTracks()[0] && (n.localStream.getVideoTracks()[0].enabled = !e);
          }, this.hangup = function() {
            if (!n.peerConnection) return void a()("No call is active");
            a()("Hangup call");
            var e = {
              webrtc_type: "hangup"
            };
            t.onSendData(e), n.localStream.getTracks().forEach(function(e) {
                e.stop();
              }), n.peerConnection.close(), n.peerConnection = null, n.gotRemoteDesc = !1, n
              .localStream = null, t.onRemoteStream && t.onRemoteStream(null), t.onLocalStream && t
              .onLocalStream(null);
          };
        },
        u = function(e, t, i, o) {
          var r = new URL(e);
          return o ? (t += i ? "_guest" : "_host", e = "wss://" + r.hostname +
            ":47984/upgrade?sessionid=" + t) : (t += i ? "_host" : "_guest", e = "wss://" + r.hostname +
            "/server/" + t), n.create(e);
        },
        f = function(e) {
          return e in a || (a[e] = new d(e)), a[e];
        },
        m = function(e) {
          e.rtcPeerConnection && (e.rtcPeerConnection.hangup(), e.rtcPeerConnection = null, e.host =
            null), e.outgoingSocket && (e.outgoingSocket.close(), e.outgoingSocket = null), e
            .incomingSocket && (e.incomingSocket.close(), e.incomingSocket = null);
          for (var t = 0; t < e.hangupCB.length; ++t) e.hangupCB[t]();
          var n = e.id,
            i = e.localVideoCB,
            o = e.remoteVideoCB,
            r = e.hangupCB;
          e.id in a && delete a[e.id];
          var l = f(n);
          l.localVideoCB = i, l.remoteVideoCB = o, l.hangupCB = r;
        },
        g = {
          offer: function(e, t) {
            var n = t.sessioninfo,
              i = t.do_video || !1,
              o = t.do_audio || !1;
            e.outgoingSocket = u(e.proxyUrl, e.proxySessionId, !1, !0), e.outgoingSocket.open(), s(e)(
                "Sending messages to host from ", e.outgoingSocket.url()), e.rtcPeerConnection = new c({
                name: e.id + ".guest",
                videoEnabled: i,
                audioEnabled: o,
                onSendData: e.onSendRTCData,
                onRemoteStream: e.gotRemoteStream,
                onLocalStream: e.gotLocalStream,
                verbose: e.verbose
              }), e.initMuteStates(), e.rtcPeerConnection.createAnswer(n), "icecandidates" in t && e
              .rtcPeerConnection.addRemoteIceCandidates(t.icecandidates);
          },
          answer: function(e, t) {
            var n = t.sessioninfo;
            s(e)("got answer ", t), e.initMuteStates(), e.rtcPeerConnection.beginCall(n),
              "icecandidates" in t && e.rtcPeerConnection.addRemoteIceCandidates(t.icecandidates);
          },
          ice_trickle: function(e, t) {
            e.rtcPeerConnection.addRemoteIceCandidates(t.icecandidates);
          },
          ice_trickle_done: function(e, t) {
            e.rtcPeerConnection.doneRemoteIceCandidates();
          },
          hangup: function(e, t) {
            s(e)("hangup initiated by peer"), m(e);
          }
        },
        p = function(t) {
          return function(n) {
            e.info("message: " + n);
            try {
              if ("WEBSOCKET" === n["request-type"]) {
                var i = JSON.parse(n["request-body"]),
                  o = i.webrtc_type;
                g[o](t, i);
              }
            } catch (t) {
              e.error("Expected JSON object with correct syntax " + t);
            }
          };
        };
      this.registerLocalVideoCallback = function(e, t) {
        var n = f(e);
        n.localVideoCB.push(t);
      }, this.registerRemoteVideoCallback = function(e, t) {
        var n = f(e);
        n.remoteVideoCB.push(t);
      }, this.registerHangupCallback = function(e, t) {
        var n = f(e);
        n.hangupCB.push(t);
      }, this.listenForCall = function(e, n) {
        var i = f(e);
        if (i.remoteVideoCB.length < 1)
        throw "Error: Need to at least have one remote video tag registered";
        i.proxyUrl = n.proxyAddress, i.proxySessionId = n.proxySessionId, i.host = !1;
        var o = t.defer();
        return i.incomingSocket = u(i.proxyUrl, i.proxySessionId, !1, !1), i.incomingSocket.on("$open",
          function() {
            s(i)("Listening for connection from host on ", i.incomingSocket.url()), o.resolve(
              "Connection successful.");
          }), i.incomingSocket.on("$close", function() {
          o.reject("Failed to connect");
        }), i.incomingSocket.on("$message", p(i)), i.incomingSocket.open(), o.promise;
      }, this.startCall = function(e, n) {
        var i = f(e);
        if (i.remoteVideoCB.length < 1)
        throw "Error: Need to at least have one remote video tag registered";
        i.proxyUrl = n.proxyAddress, i.proxySessionId = n.proxySessionId, i.host = !0, i
          .rtcPeerConnection = new c({
            name: i.id + ".host",
            videoEnabled: n.videoOn || !1,
            audioEnabled: n.audioOn || !1,
            onSendData: i.onSendRTCData,
            onRemoteStream: i.gotRemoteStream,
            onLocalStream: i.gotLocalStream,
            verbose: i.verbose
          });
        var o = t.defer();
        return i.incomingSocket = u(i.proxyUrl, i.proxySessionId, !0, !1), i.incomingSocket.on("$open",
          function() {
            s(i)("Listening for messages from guest on", i.incomingSocket.url()), i.outgoingSocket =
              u(i.proxyUrl, i.proxySessionId, !0, !0), i.outgoingSocket.on("$open", function() {
                s(i)("Sending messages to guest on", i.outgoingSocket.url()), i.rtcPeerConnection
                  .createOffer(), o.resolve("Connection successful.");
              }), i.outgoingSocket.on("$close", function() {
                o.reject("Failed to connect to outgoing socket");
              }), i.outgoingSocket.open();
          }), i.incomingSocket.on("$close", function() {
          o.reject("Failed to connect to incoming socket");
        }), i.incomingSocket.on("$message", p(i)), i.incomingSocket.open(), o.promise;
      }, this.hangup = function(e) {
        var t = f(e);
        m(t);
      }, this.setVerbose = function(e, t) {
        var n = f(e);
        n.verbose = t;
      }, this.setMicrophoneMute = function(e, t) {
        var n = f(e);
        n.muteMicrophone(t);
      }, this.setPreferredAudioDevice = function(e, t) {
        var n = f(e);
        n.setPreferredAudioDevice(t);
      }, this.getMicrophoneMute = function(e, t) {
        var n = f(e);
        return n.micMuted;
      }, this.setCameraMute = function(e, t) {
        var n = f(e);
        n.muteCamera(t);
      }, this.getCameraMute = function(e, t) {
        var n = f(e);
        return n.cameraMuted;
      }, this.setRemoteAudioMute = function(e, t) {
        var n = f(e);
        n.muteRemoteAudio(t);
      }, this.getRemoteAudioMute = function(e, t) {
        var n = f(e);
        return n.remoteAudioMuted;
      };
    }
  ]);
  exports.webrtcP2PService = l;
}
