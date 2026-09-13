// ─────────────────────────────────────────────────────────────
// APP MODULE 98
// role       : directive nvGalleryFilterMenu
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }

  function o(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvGalleryFilterMenu = void 0;
  var r = n(3),
    a = o(r),
    l = n(1),
    s = n(330),
    d = i(s);
  n(183), n(5);
  var c = l.ngMainModule.directive("nvGalleryFilterMenu", ["$document", "$filter", "$window", "$log", function(e, t, n,
    i) {
    var o = i.getInstance("osc/galleryFilter");
    return {
      restrict: "E",
      scope: {
        nvChangeFilter: "=",
        onFilterDataChanged: "&",
        Items: "@",
        leftWidth: "@",
        rightWidth: "@"
      },
      template: d.default,
      controller: "GalleryFilterMenuController",
      controllerAs: "filterMenu",
      link: function(i, r, l) {
        function s(t, n, i) {
          var o = e[0].createElement("canvas"),
            r = o.getContext("2d");
          r.font = i + " " + n;
          var a = r.measureText(t).width;
          return a
        }
        var d = document.getElementById("gallery"),
          c = n.getComputedStyle(d).fontSize,
          u = [];
        a.each(i.Items, function(e) {
          if (void 0 !== e.title) {
            var n = t("translate")(e.title),
              i = s(n, "Segoe UI", c);
            void 0 !== e.image && (i += 25), u.push(i)
          }
        });
        var f = a.max(u);
        o.info("Largest string: ", f);
        var m = n.getComputedStyle(d),
          g = Number(m.minWidth.replace("px", "")) - 80;
        o.info("Gallery width: ", g);
        var p = (g - (f + 60)).toFixed(0),
          h = g - p - 10;
        i.leftWidth = p.toString() + "px", i.rightWidth = h.toString() + "px", o.info("Left width: ", i
          .leftWidth), o.info("Right width: ", i.rightWidth)
      }
    }
  }]);
  t.nvGalleryFilterMenu = c
}
