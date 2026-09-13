// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 235
// directive nvPreferencesAudio
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
  }), exports.nvPreferencesAudio = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(352),
    a = i(r);
  require(234) /* app/234 — PreferencesAudioController (controller) */, require(14) /* app/14 — nvSlider (directive) */, require(5) /* app/5 — hoverFocus (directive) */, require(6) /* app/6 — nvOscTile (directive) */;
  var l = o.ngMainModule.directive("nvPreferencesAudio", ["$document", "$filter", "$window", "$log", function(
    e, t, n, i) {
    var o = i.getInstance("main.preferences.audio");
    return {
      restrict: "E",
      scope: {
        textVolume: "@",
        textSource: "@",
        textBoost: "@",
        leftWidth: "@",
        rightWidth: "@"
      },
      template: a.default,
      controller: "PreferencesAudioController",
      controllerAs: "controller",
      link: function(i, r, a) {
        function l(t, n, i) {
          var o = e[0].createElement("canvas"),
            r = o.getContext("2d");
          r.font = i + " " + n;
          var a = r.measureText(t).width;
          return a;
        }
        var s = .3,
          d = .1,
          c = 20,
          u = t("translate")(i.textVolume),
          f = t("translate")(i.textSource),
          m = t("translate")(i.textBoost),
          g = n.getComputedStyle(document.getElementById("firstString")).fontSize;
        o.info("Font size: ", g);
        var p = [];
        p.push(l(u, "Segoe UI", g)), p.push(l(f, "Segoe UI", g)), p.push(l(m, "Segoe UI", g)), o.info(
          "Audio width array: ", p);
        var h = _.max(p);
        o.info("Largest string: ", h);
        var b = document.getElementById("topLevel"),
          x = n.getComputedStyle(b),
          v = Number(x.minWidth.replace("px", "")) - 80;
        o.info("Audio width: ", v);
        var y = v * s - c,
          w = v * d - c,
          S = 0,
          E = 0;
        S = h > y ? s : h < w ? d : (h + c) / v, S = (100 * S).toFixed(0), E = 100 - S, i.leftWidth =
          S.toString() + "%", i.rightWidth = E.toString() + "%", o.info("Left width: ", i.leftWidth),
          o.info("Right width: ", i.rightWidth);
      }
    };
  }]);
  exports.nvPreferencesAudio = l;
}
