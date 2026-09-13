// ─────────────────────────────────────────────────────────────
// APP MODULE 141
// role       : directive nvAccordion | directive nvAccordionPane
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvAccordionPane = t.nvAccordion = void 0;
  var i = n(2);
  n(140);
  var o = i.ngMainCommonModule.directive("nvAccordion", ["ACCORDION_MODES", function(e) {
      return {
        restrict: "E",
        transclude: !0,
        scope: {
          nvAccordionMode: "@"
        },
        template: '<div flex layout="column" ng-transclude></div>',
        controller: "AccordionController",
        link: function(t, n, i, o, r) {
          n.attr("layout", "column"), i.nvAccordionMode === e.ONLY_ONE ? n.addClass("common-accordion-only-one") :
            i.nvAccordionMode === e.ALWAYS_ONE ? n.addClass("common-accordion-always-one") : n.addClass(
              "common-accordion-multiple"), n.addClass("common-accordion")
        }
      }
    }]),
    r = i.ngMainCommonModule.directive("nvAccordionPane", ["$compile", function(e) {
      return {
        restrict: "E",
        require: "^nvAccordion",
        transclude: !0,
        scope: {
          nvStartExpanded: "@",
          nvOnExpand: "&",
          nvOnExpandOne: "&",
          nvOnCollapse: "&"
        },
        template: '<div ng-transclude flex layout="column"></div>',
        link: function(t, n, i, o) {
          function r(e) {
            o.onClick(c)
          }

          function a(e) {
            o.onKeyDown(e, c)
          }

          function l() {
            h(), o.removePane(u), d = null, g = null, p = null, m.off("click", r), m = null, s = null, f.empty(),
              f.remove(), f = null, c = null, u = null
          }
          var s, d, c, u, f = n,
            m = n.find("nv-accordion-pane-header"),
            g = n.find("nv-accordion-pane-content");
          u = {
            onExpand: function(e) {
              e ? (f.removeClass("collapsed"), t.nvOnExpand({
                index: c
              })) : (f.addClass("collapsed"), t.nvOnCollapse({
                index: c
              }))
            },
            isExpanded: function() {
              return !f.hasClass("collapsed")
            }
          }, f.attr("flex", ""), f.attr("layout", "column"), s = m.wrap(
            '<nv-accordion-pane-header-wrapper layout="row"></nv-accordion-pane-header-wrapper>'), m.on("click",
            r), m.on("keydown", a);
          var p = s.append(e(
            '<div class="arrow"><md-icon class="share-icon icon-normal icon24 icon-chevron_down"></md-icon></div>'
            )(t));
          d = g.wrap("<nv-accordion-pane-content-wrapper flex></nv-accordion-pane-content-wrapper"), c = o
            .addPane(u), u.onExpand("true" === t.nvStartExpanded);
          var h = i.$observe("nvStartExpanded", function(e) {
            u.onExpand("true" === e)
          });
          t.$on("$destroy", l), n.on("$destroy", function() {
            t.$destroy()
          })
        }
      }
    }]);
  t.nvAccordion = o, t.nvAccordionPane = r
}
