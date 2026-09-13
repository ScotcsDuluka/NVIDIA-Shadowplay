// ─────────────────────────────────────────────────────────────
// APP MODULE 163
// role       : controller nvChevronController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvChevronController = void 0;
  var i = n(1),
    o = i.ngMainModule.controller("nvChevronController", ["$scope", "$log", function(e, t) {
      function n() {
        if (!i.disabled) {
          i.selectedItem = e.input.items[i.selectedIndex];
          var t = {};
          t.selectedIndex = i.selectedIndex, e.onSelected(t)
        }
      }
      var i = this;
      i.selectedItem = void 0, i.selectedIndex = 0, i.disabled = !1, i.wide = !1;
      t.getInstance("osc/nvChevronController");
      i.goLeft = function() {
        if (!i.disabled) {
          var t = e.input.items.length;
          0 === i.selectedIndex ? i.selectedIndex = t - 1 : i.selectedIndex--, n()
        }
      }, i.goRight = function() {
        if (!i.disabled) {
          var t = e.input.items.length;
          i.selectedIndex === t - 1 ? i.selectedIndex = 0 : i.selectedIndex++, n()
        }
      }, i.keyDown = function() {}, e.$watch("input", function() {
        void 0 !== e.input && void 0 !== e.input.items && (i.selectedIndex = e.input.selectedIndex, i
          .selectedItem = e.input.items[i.selectedIndex], e.input.wide && (i.wide = e.input.wide), i
          .disabled = e.input.disable || e.input.items.length < 2)
      }, !0)
    }]);
  t.nvChevronController = o
}
