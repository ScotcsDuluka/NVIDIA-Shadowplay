// ─────────────────────────────────────────────────────────────
// APP MODULE 165
// role       : controller NvOauthDialogueController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.NvOauthDialogueController = void 0;
  var i = n(1);
  n(11);
  var o = i.ngMainModule.controller("NvOauthDialogueController", ["$scope", "$element", "$window", "$document",
    "$interval", "$log", "$timeout", "connectService", "ugcService", "cefService",
    function(e, t, n, i, o, r, a, l, s, d) {
      var c = this;
      c.dialogueClosedCallback = e.dialogueClosedCallback;
      var u, f, m, g = (r.getInstance("osc/nvOauthDialogueController"), 600),
        p = 224,
        h = 721,
        b = 760,
        x = function(e) {
          u && !u.closed ? (e.preventDefault(), u.focus(), d.windowFocus("app_oauthWindow")) : y()
        },
        v = function e() {
          var r = !0;
          if (t[0].parentElement) {
            var d = t[0].parentElement.getBoundingClientRect();
            d.width > 0 && (r = !1)
          }
          if (r) return void a(e, 0);
          if (f = angular.element('<div class="oobe-oauth-blocker-overlay"></div>'), i.find("body").eq(0).append(f),
            n.addEventListener("click", x), n.addEventListener("keydown", x), t[0].parentElement) {
            var d = t[0].parentElement.getBoundingClientRect();
            d.width > 0 && (g = d.left, p = d.top, h = d.width + 1)
          }
          var v = l.getOAuthUrl(c.service);
          u = n.open(v, "app_oauthWindow", "toolbar=0,location=0,menubar=0,status=0,titlebar=0,left=" + g +
            ",top=" + p + ",width=" + h + ",height=" + b), m = o(function() {
            if (u)
              if (u.closed) y();
              else if (u.document && u.document.URL && u.document.URL.startsWith(l.providers[l.services[c
                .service].providerName].redirectUri)) {
              var e = s.extractURLParams(u.document.URL);
              e && (l.setAuthToken(c.service, e), o.cancel(m), y(s.checkJarvisLoginRequirement(c.service)))
            }
          }, 100)
        },
        y = function(e) {
          o.cancel(m), n.removeEventListener("click", x), n.removeEventListener("keydown", x), void 0 !== f && f
            .remove(), u && (u.close(), u = null, angular.isDefined(c.dialogueClosedCallback) && !e && c
              .dialogueClosedCallback())
        };
      e.$watch("dialogueParams", function(e) {
        angular.isDefined(e.openPopup) && (c.service = e.serviceName, c.openPopup = e.openPopup, c.openPopup ?
          angular.isDefined(c.service) && v() : y())
      }, !0), e.$on("$destroy", function() {
        y(!0)
      })
    }
  ]);
  t.NvOauthDialogueController = o
}
