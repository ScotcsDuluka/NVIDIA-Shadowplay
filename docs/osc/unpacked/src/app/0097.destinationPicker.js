// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 97
// directive destinationPicker
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
  }), exports.destinationPicker = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(328),
    a = i(r);
  require(180) /* app/180 — DestinationPickerController (controller) */, require(14) /* app/14 — nvSlider (directive) */;
  var l = o.ngMainModule.directive("destinationPicker", ["$document", "$filter", "$window", "$log", function(
    e, t, n, i) {
    var o = i.getInstance("osc/picker");
    return {
      restrict: "E",
      scope: {
        nvChangePicker: "=",
        onUploadDataChanged: "&",
        onLoginViaParent: "&",
        textDestination: "@",
        textPostAs: "@",
        textTitle: "@",
        textLocation: "@",
        textAudience: "@",
        textPage: "@",
        textGroup: "@",
        textFormat: "@",
        leftWidth: "@",
        rightWidth: "@",
        lastItemWidth1: "@",
        lastItemWidth2: "@",
        lastItemWidth3: "@"
      },
      template: a.default,
      controller: "DestinationPickerController",
      controllerAs: "destination",
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
          u = t("translate")(i.textDestination),
          f = t("translate")(i.textPostAs),
          m = t("translate")(i.textTitle),
          g = t("translate")(i.textLocation),
          p = t("translate")(i.textAudience),
          h = t("translate")(i.textPage),
          b = t("translate")(i.textGroup),
          x = t("translate")(i.textFormat),
          v = n.getComputedStyle(document.getElementById("firstString")).fontSize;
        o.info("Font size: ", v);
        var y = [];
        y.push(l(u, "Segoe UI", v)), y.push(l(f, "Segoe UI", v)), y.push(l(m, "Segoe UI", v)), y.push(
          l(g, "Segoe UI", v)), o.info("Picker width: ", y);
        var w = l(p, "Segoe UI", v),
          S = l(h, "Segoe UI", v),
          E = l(b, "Segoe UI", v),
          k = l(x, "Segoe UI", v);
        i.lastItemWidth1 = w.toFixed(0).toString() + "px", i.lastItemWidth2 = S.toFixed(0)
        .toString() + "px", i.lastItemWidth3 = E.toFixed(0).toString() + "px", i.lastItemWidth4 = k
          .toFixed(0).toString() + "px";
        var T = _.max(y);
        o.info("Largest string: ", T);
        var C = document.getElementById("topLevel"),
          O = n.getComputedStyle(C),
          A = Number(O.minWidth.replace("px", "")) - 80;
        o.info("Picker width: ", A);
        var I = A * s - c,
          M = A * d - c,
          R = 0,
          P = 0;
        R = T > I ? s : T < M ? d : (T + c) / A, R = (100 * R).toFixed(1), P = 100 - R, i.leftWidth =
          R.toString() + "%", i.rightWidth = P.toString() + "%", o.info("Left width: ", i.leftWidth),
          o.info("Right width: ", i.rightWidth), o.info("Last item width1: ", i.lastItemWidth1), o
          .info("Last item width2: ", i.lastItemWidth2), o.info("Last item width3: ", i
            .lastItemWidth3), o.info("Last item width4: ", i.lastItemWidth4);
      }
    };
  }]);
  exports.destinationPicker = l;
}
