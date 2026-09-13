// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 171
// directive nvWebrtcChat
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvWebrtcChat = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(94) /* app/94 — webrtcP2PService (service) */;
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
            e ? (o = e.getVideoTracks().length > 0, n.attr("src", URL.createObjectURL(e))) : n.attr(
              "src", ""), i && n[0].setSinkId(i).then(function() {
              n.removeAttr("muted");
            }), o ? n[0].style.display = "block" : n[0].style.display = "none";
          };
        t.webrtcLocal ? e.registerLocalVideoCallback(i, o) : e.registerRemoteVideoCallback(i, o);
      }],
      controllerAs: "vm",
      link: function(e, t, n) {
        t.attr("autoplay", "");
      }
    };
  }]);
  exports.nvWebrtcChat = o;
}
