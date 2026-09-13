// ─────────────────────────────────────────────────────────────
// APP MODULE 170
// role       : directive nvOscNotifier
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
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.nvOscNotifier = void 0;
  var o = n(1),
    r = n(322),
    a = i(r);
  n(168), n(12);
  var l = o.ngMainModule.directive("nvOscNotifier", ["$document", "$timeout", "$log", "oscDisplayService", function(e,
    t, n, i) {
    var o = (n.getInstance("osc/notifier"), 5);
    return {
      restrict: "EAC",
      template: a.default,
      controllerAs: "oscNotifierCtrl",
      transclude: !1,
      scope: {
        notifierOpen: "=",
        notifierMessage: "@",
        notifierMessageSubtext: "@",
        notifierIconPath: "@",
        notifierIcon: "@",
        notifierAutoClose: "=?"
      },
      link: function(n, r, a) {
        function l(e) {
          "tall" === e ? (r.removeClass("notifier-large"), r.addClass("notifier-tall")) : "large" === e ? (r
            .removeClass("notifier-tall"), r.addClass("notifier-large")) : "small" === e && (r.removeClass(
            "notifier-tall"), r.removeClass("notifier-large"))
        }

        function s(t, n, i) {
          var o = e[0].createElement("canvas"),
            r = o.getContext("2d");
          r.font = i + "px" + n;
          var a = r.measureText(t).width;
          return a
        }

        function d(e) {
          i.closeOSCForNotification(), r.removeClass("notifier-flip"), r.removeClass("notifier-open")
        }

        function c(e) {
          r.removeClass("notifier-flip"), t(function() {
            r.addClass("notifier-flip")
          }, 50)
        }

        function u(e) {
          if (0 !== e.style.width) {
            i.openOSCForNotification(!0);
            var a = s(n.notifierMessage, "Segoe UI", "16px");
            n.notifierMessageSubtext && (a += s(n.notifierMessageSubtext, "Segoe UI", "14px")), l(a > 525 ?
              "tall" : a > 320 ? "large" : "small"), r.addClass("notifier-open");
            var u;
            u = function() {
              "" === n.notifierOpen ? f = 0 : f < 0 && m > 0 ? (f = o, c(e), m--, t(function() {
                n.notifierMessage = g, n.notifierMessageSubtext = p, n.notifierIconPath = b, n
                  .notifierIcon = h
              }, 250), t(u, 1e3)) : f < 0 ? (f = 0, d(e)) : (f--, t(u, 1e3))
            }, u()
          }
        }
        var f = 4,
          m = 0,
          g = "",
          p = "",
          h = "",
          b = "";
        r.addClass("notifier-slider");
        var x = e[0].body,
          v = r[0];
        x.appendChild(v), n.$watch("notifierOpen", function(e) {
          "" === e.message ? d(v) : e.flip === !0 ? (m++, g = e.message, p = e.messageSubtext, b = e.img,
            h = e.icon) : (n.notifierMessage = e.message, n.notifierMessageSubtext = e.messageSubtext, n
            .notifierIconPath = e.img, n.notifierIcon = e.icon, f = o, u(v))
        }, !0), n.$on("$destroy", function() {
          x.removeChild(v)
        }), n.notifierAutoClose && (n.$on("$locationChangeStart", function() {
          d(v)
        }), n.$on("$stateChangeStart", function() {
          d(v)
        }))
      }
    }
  }]);
  t.nvOscNotifier = l
}
