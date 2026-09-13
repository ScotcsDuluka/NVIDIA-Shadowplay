// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 168
// controller OscNotifierController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.OscNotifierController = void 0;
  var o = require(3),
    r = i(o),
    a = require(1) /* app/1 — main (module) */;
  require(169) /* app/169 — NOTIFICATION_DATA (constant) */;
  var l = a.ngMainModule.controller("OscNotifierController", ["$filter", "$scope", "$log", "eventAggregator",
    "NOTIFICATION_DATA", "NOTIFICATION_EVENT", "NOTIFIER_SELECTIONS", "UGC_NOTIFICATION_EVENTS",
    function(e, t, n, i, o, a, l, s) {
      function d(n) {
        var i = n.selection;
        m.info("Controller: setupNotifier selection: ", i, "flip: ", n.flip, " Arg1: ", n.arg1, " Arg2: ",
          n.arg2), f.selection = i;
        var o = r.find(f.notificationData, function(e) {
          return e.id === f.selection;
        });
        t.notifierObject.img = o.img, t.notifierObject.icon = o.icon, t.notifierObject.flip = n.flip;
        var a = n.arg1;
        r.isString(a) && a.startsWith("l10n") && (a = e("translate")(a));
        var l = n.arg2;
        r.isString(l) && l.startsWith("l10n") && (l = e("translate")(l)), o.messageSubtext ? (
          "l10n.whisperModeSettings" === o.messageSubtext ? t.notifierObject.messageSubtext = e(
            "translate")(o.messageSubtext, {
            fanVolume: a,
            baseFrameRate: l
          }) : t.notifierObject.messageSubtext = e("translate")(o.messageSubtext, {
            arg1: a,
            arg2: l
          }), t.notifierObject.message = e("translate")(o.message)) : (t.notifierObject.messageSubtext =
          void 0, t.notifierObject.message = e("translate")(o.message, {
            arg1: a,
            arg2: l
          })), t.notifierObject.message === f.previous && (t.notifierObject.changed = !t.notifierObject
          .changed), f.previous = t.notifierObject.message;
      }

      function c(e) {
        t.$evalAsync(d(e));
      }

      function u(e) {
        var t = "";
        switch (e.eventName) {
          case s.BROADCAST_LOGIN:
            t = l.BROADCAST_LOGIN;
            break;
          case s.NO_YOUTUBE_CHANNEL:
            t = l.NO_YOUTUBE_CHANNEL;
            break;
          case s.UPLOAD_STARTED:
            t = l.UPLOAD_STARTED;
            break;
          case s.UPLOAD_SUCCESS:
            t = l.UPLOAD_SUCCESS;
            break;
          case s.UPLOAD_FAILED:
            t = l.UPLOAD_FAILED;
            break;
          default:
            m.error("Invalid event from UGC lib: " + e.eventName);
        }
        c({
          selection: t,
          arg1: e.data,
          arg2: void 0,
          flip: !1
        });
      }
      var f = this;
      f.previous = "", f.selection = 0, f.notificationData = o;
      var m = n.getInstance("osc/notifierController");
      m.info("Controller: Enter"), t.notifierObject = {
        changed: !1,
        img: "",
        icon: "",
        message: "",
        flip: !1
      }, i.on(a, c), i.on(s.UGC_NOTIFICATION, u), t.$on("$destroy", function() {
        i.off(a, c), i.off(s.UGC_NOTIFICATION, u);
      });
    }
  ]);
  exports.OscNotifierController = l;
}
