// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 151
// directive nvVirtualGridList | directive nvVirtualGridListRepeat | directive nvVirtualGridListItem
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.nvVirtualGridListItem = exports.nvVirtualGridListRepeat = exports.nvVirtualGridList = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(150) /* app/150 — VirtualGridListController (controller) */;
  var o = i.ngMainCommonModule.directive("nvVirtualGridList", function() {
      return {
        restrict: "E",
        template: function(e) {
          return e[0].innerHTML;
        },
        controller: "VirtualGridListController"
      };
    }),
    r = i.ngMainCommonModule.directive("nvVirtualGridListRepeat", ["$document", "_", "$compile", function(e,
      t, n) {
      function i(e) {
        var t, n, i, o, r;
        if (t = e.match(
            /^\s*([\s\S]+?)\s+in\s+([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+track\s+by\s+([\s\S]+?))?\s*$/),
          !t) throw new Error(
          'Expected expression in form of "_item_ in _collection_[ track by _id_]" but got {0}', e);
        return n = t[1], i = t[2], o = t[3], r = t[4], {
          key: n,
          collection: i,
          aliasAs: o,
          trackBy: r
        };
      }

      function o(e, t, n, o) {
        var r = i(t.attr("nv-virtual-grid-list-repeat")),
          a = o,
          d = void 0 !== t.attr("nv-on-demand"),
          c = parseInt(l) + parseInt(s),
          u = t[0].offsetWidth,
          f = Math.floor(u / c);
        a.initialize(d), a.refresh({
          list: e.$eval(r.collection),
          itemWidth: c,
          columnCount: f
        }), e.$watchCollection(function() {
          return e.$eval(r.collection);
        }, function(t, n) {
          t && a.refresh({
            list: e.$eval(r.collection)
          });
        });
      }

      function r(t, n, i, o) {
        for (var r = e[0].styleSheets, a = n.attr("nv-repeat-item-class"), d = 0, c = r.length; d <
          c; d++) {
          var u = r[d];
          if (u.cssRules)
            for (var f = 0, m = u.cssRules.length; f < m; f++) {
              var g = u.cssRules[f];
              if (g.selectorText && g.selectorText.split(",").indexOf("." + a) !== -1) return l = g.style[
                "max-width"], void(s = g.style["margin-right"]);
            }
        }
      }

      function a(e) {
        var t,
          n,
          o = i(e.attr("nv-virtual-grid-list-repeat")),
          r = void 0 !== e.attr("nv-on-demand"),
          a = e[0].outerHTML,
          l = e.attr("nv-repeat-item-class"),
          s = "",
          d = e.attr("nv-top-index"),
          c = "";
        return t = o.key + " in row", o.aliasAs && (t += " alias as " + o.aliasAs), o.trackBy && (t +=
            " track by " + o.trackBy), r && (s = "md-on-demand"), d && (c = 'md-top-index="' + d + '"'),
          a = a.replace(/nv-virtual-grid-list-repeat=["'].*?['"]/g, ""), n =
          '<md-content layout="row" flex><md-virtual-repeat-container flex id="vertical-container" ' + c +
          '><div md-virtual-repeat="row in nvVirtualGridListItems" layout="row" flex ' + s + ' class="' +
          l + '"><nv-virtual-grid-list-item ng-repeat="' + t + '">' + a +
          "</nv-virtual-grid-list-item></div></md-virtual-repeat-container></md-content>";
      }
      var l = null,
        s = null;
      return {
        restrict: "A",
        require: "^nvVirtualGridList",
        replace: !0,
        template: a,
        compile: function(e, t) {
          return e.removeAttr("style"), e.removeAttr("class"), {
            pre: r,
            post: o
          };
        }
      };
    }]),
    a = i.ngMainCommonModule.directive("nvVirtualGridListItem", [function() {
      return {
        restrict: "E"
      };
    }]);
  exports.nvVirtualGridList = o, exports.nvVirtualGridListRepeat = r, exports.nvVirtualGridListItem = a;
}
