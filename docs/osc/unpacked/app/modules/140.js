// ─────────────────────────────────────────────────────────────
// APP MODULE 140
// role       : controller AccordionController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.accordionController = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.controller("AccordionController", ["$scope", "ACCORDION_MODES", "OSC_KEYBOARD", function(e,
      t, n) {
      var i = this,
        o = [],
        r = 0;
      void 0 === e.nvAccordionMode && (e.nvAccordionMode = t.MULTIPLE_OPEN), i.addPane = function(e) {
        return o[r] = e, r++
      }, i.getNumPanes = function() {
        return o.length
      }, i.removePane = function(e) {
        o = _.without(o, e), r = o.length
      }, i.onClick = function(n) {
        e.nvAccordionMode === t.ONLY_ONE ? angular.forEach(o, function(e, t) {
          e.onExpand(t === n && !o[n].isExpanded())
        }) : e.nvAccordionMode === t.ALWAYS_ONE ? angular.forEach(o, function(e, t) {
          e.onExpand(t === n)
        }) : o[n].onExpand(!o[n].isExpanded())
      }, i.onKeyDown = function(e, t) {
        e.keyCode !== n.ENTER && e.keyCode !== n.RIGHT_ARROW || i.onClick(t)
      }
    }]);
  t.accordionController = o
}
