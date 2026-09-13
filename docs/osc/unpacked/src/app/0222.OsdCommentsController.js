// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 222
// controller OsdCommentsController
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
  }), exports.OsdCommentsController = void 0;
  var o = require(106),
    r = i(o),
    a = require(1) /* app/1 — main (module) */;
  require(35) /* app/35 — osdService (service) */, require(4) /* app/4 — shadowPlayService (service) */, require(34) /* app/34 — keyboardService (service) */;
  var l = a.ngMainModule.controller("OsdCommentsController", ["$log", "$scope", "$filter", "eventAggregator",
    "osdService", "shadowPlayService", "keyboardService", "COMMON_EVENTS", "CONNECT_EVENTS",
    "HOTKEY_EVENTS",
    function(e, t, n, i, o, a, l, s, d, c) {
      function u() {
        a.getHotkeyShortcut(a.HotkeyShortcuts.COMMENTSTOGGLE).then(function(e) {
          e && e.keys && (p.showHideMessage = n("translate")(h, {
            arg1: l.shortcutToStr(e.keys)
          }));
        });
      }

      function f() {
        p.isVisible = o.anythingBroadcasting && p.commentsCount > 0 && b, o.anythingBroadcasting ? u() : p
          .commentsCount = 0;
      }

      function m(e) {
        e && e.summary && e.summary.total_count && (p.commentsCount = e.summary.total_count, p.comments =
          e.data, f());
      }

      function g() {
        b = !b, f();
      }
      var p = this;
      e.getInstance("osc/CommentsController");
      p.width = (0, r.default)(240), p.height = 2 * p.width, p.isVisible = !1, p.commentsCount = 0, p
        .showHideMessage = void 0;
      var h = "l10n.showHideComments",
        b = !0;
      i.on(s.OSD_SETTINGS_CHANGED, f), i.on(d.FACEBOOK_COMMENTS, m), i.on(c.COMMENTS_TOGGLE, g), t.$on(
        "$destroy",
        function() {
          i.off(s.OSD_SETTINGS_CHANGED, f), i.off(d.FACEBOOK_COMMENTS, m), i.off(c.COMMENTS_TOGGLE, g);
        });
      var x = o.getBroadcastComments();
      x ? m(x) : f();
    }
  ]);
  exports.OsdCommentsController = l;
}
