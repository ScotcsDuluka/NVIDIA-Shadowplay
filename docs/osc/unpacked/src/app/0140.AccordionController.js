// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 140
// controller AccordionController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.accordionController = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.controller("AccordionController", ["$scope", "ACCORDION_MODES", "OSC_KEYBOARD",
      function(e, t, n) {
        var i = this,
          o = [],
          r = 0;
        void 0 === e.nvAccordionMode && (e.nvAccordionMode = t.MULTIPLE_OPEN), i.addPane = function(e) {
          return o[r] = e, r++;
        }, i.getNumPanes = function() {
          return o.length;
        }, i.removePane = function(e) {
          o = _.without(o, e), r = o.length;
        }, i.onClick = function(n) {
          e.nvAccordionMode === t.ONLY_ONE ? angular.forEach(o, function(e, t) {
            e.onExpand(t === n && !o[n].isExpanded());
          }) : e.nvAccordionMode === t.ALWAYS_ONE ? angular.forEach(o, function(e, t) {
            e.onExpand(t === n);
          }) : o[n].onExpand(!o[n].isExpanded());
        }, i.onKeyDown = function(e, t) {
          e.keyCode !== n.ENTER && e.keyCode !== n.RIGHT_ARROW || i.onClick(t);
        };
      }
    ]);
  exports.accordionController = o;
}
