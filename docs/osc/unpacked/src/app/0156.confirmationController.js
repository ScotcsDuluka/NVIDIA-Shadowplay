// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 156
// controller confirmationController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.confirmationController = void 0;
  var i = require(1) /* app/1 — main (module) */;
  require(12) /* app/12 — oscDisplayService (service) */, require(4) /* app/4 — shadowPlayService (service) */;
  var o = i.ngMainModule.controller("confirmationController", ["$scope", "$state", "$stateParams", "$log",
    "oscDisplayService", "shadowPlayService", "eventAggregator", "KEYBOARD_EVENTS",
    function(e, t, n, i, o, r, a, l) {
      var s = this,
        d = i.getInstance("osc/confirmationController");
      s.title = "", s.icon = "", s.status = "", s.question = "", s.footnote = "", s.topButton = "", s
        .bottomButton = "", s.topAction = "", s.bottomAction = "", s.closeOSC = !0, s.lastState = "", s
        .hideBottomButton = !1, s.topActionArgs = {};
      r.captureState;
      "" !== n.title && (d.info("Title: ", n.title), s.title = n.title), "" !== n.icon && (d.info(
          "Icon: ", n.icon), s.icon = n.icon), "" !== n.status && (d.info("Status: ", n.status), s
          .status = n.status), "" !== n.question && (d.info("Question: ", n.question), s.question = n
          .question), "" !== n.footnote && (d.info("Footnote: ", n.footnote), s.footnote = n.footnote),
        "" !== n.topButton && (d.info("Top Button: ", n.topButton), s.topButton = n.topButton), "" !== n
        .bottomButton && (d.info("Bottom Button: ", n.bottomButton), s.bottomButton = n.bottomButton),
        "" !== n.topAction && (s.topAction = n.topAction), "" !== n.bottomAction && (s.bottomAction = n
          .bottomAction), "" !== n.closeOSC && (s.closeOSC = n.closeOSC), "" !== n.lastState && (s
          .lastState = n.lastState), "" !== n.topActionArgs && (d.info("Top Action Arguments: ", n
          .topActionArgs), s.topActionArgs = n.topActionArgs), "" === n.bottomButton && "" === n
        .bottomAction && (s.hideBottomButton = !0), s.topButtonFunction = function() {
          "" !== s.topAction && s.topAction(s.topActionArgs), s.closeOSC === !0 && o.closeOSC();
        }, s.bottomButtonFunction = function() {
          "" !== s.bottomAction && s.bottomAction(), s.back();
        }, s.back = function() {
          "" === s.lastState ? o.closeOSC() : t.go(s.lastState);
        }, a.on(l.ESCAPE, s.back), e.$on("$destroy", function() {
          a.off(l.ESCAPE, s.back);
        });
    }
  ]);
  exports.confirmationController = o;
}
