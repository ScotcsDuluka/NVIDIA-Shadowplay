// ─────────────────────────────────────────────────────────────
// APP MODULE 171
// role       : directive nvWebrtcChat
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvWebrtcChat = void 0;
  var i = n(1);
  n(94);
  var o = i.ngMainModule.directive("nvWebrtcChat", ["webrtcP2PService", function(e) {
    return {
      scope: {
        webrtcLocal: "@",
        webrtcSession: "@"
      },
      replace: !1,
      restrict: "A",
      controller: ["$scope", "$element", function(t, n) {
        var i = t.webrtcSession || "defaultSession",
          o = function(e, t, i) {
            var o = !1;
            e ? (o = e.getVideoTracks().length > 0, n.attr("src", URL.createObjectURL(e))) : n.attr("src",
              ""), i && n[0].setSinkId(i).then(function() {
                n.removeAttr("muted")
              }), o ? n[0].style.display = "block" : n[0].style.display = "none"
          };
        t.webrtcLocal ? e.registerLocalVideoCallback(i, o) : e.registerRemoteVideoCallback(i, o)
      }],
      controllerAs: "vm",
      link: function(e, t, n) {
        t.attr("autoplay", "")
      }
    }
  }]);
  t.nvWebrtcChat = o
}
