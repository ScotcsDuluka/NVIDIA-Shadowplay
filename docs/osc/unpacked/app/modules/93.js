// ─────────────────────────────────────────────────────────────
// APP MODULE 93
// role       : service quietMode2Service
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.quietMode2Service = void 0;
  var o = n(3),
    r = i(o),
    a = n(2);
  n(99);
  var l = a.ngMainCommonModule.service("quietMode2Service", ["$q", "$timeout", "$log", "eventAggregator",
    "quietMode2Endpoints", "socketService", "OSC_CONFIG", "QUIET_MODE2_EVENTS", "COMMON_EVENTS",
    "QUIET_MODE2_SERVICE_EVENTS",
    function(e, t, n, i, o, a, l, s, d, c) {
      function u(e) {
        return I.info("processSupportInfo response:", e), O = e.data, e.data
      }

      function f(e) {
        I.info("processStateInfo response:", e), A = e.data, A.fanVolumeMode = m(A.fanVolume), _ && (_.resolve(A), t
          .cancel(T), T = null, _ = null)
      }

      function m(e) {
        switch (e) {
          case 1:
            return "l10n.quieter";
          case 2:
            return "l10n.quiet";
          case 3:
            return "l10n.balancedMode";
          default:
            return ""
        }
      }

      function g() {
        var t = e.defer();
        if (t.promise.cancel = function() {
            I.info("fetchSupportInfo cancelled"), t.reject()
          }, r.isNull(k)) {
          O = null;
          var n = o.support();
          k = n.then(u).catch(function(t) {
            return I.error("failed to get quietmode support info", t), e.reject(t)
          }).finally(function() {
            k = null
          })
        }
        return k.then(function(e) {
          return t.resolve(e)
        }).catch(function(e) {
          return t.reject(e)
        }), t.promise
      }

      function p() {
        if (r.isNull(_)) {
          t.cancel(T), _ = e.defer();
          var n = o.state();
          return n.then(f).catch(function(e) {
            I.error("failed to get quietmode state info", e), _.reject(e), t.cancel(T), T = null, _ = null
          }), T = t(function() {
            _ && (_.reject("Timed out waiting for the callback"), _ = null), T = null
          }, M), _.promise
        }
        return _.promise
      }

      function h() {
        return O ? e.when(O) : g()
      }

      function b(t) {
        return t = t || !1, l.wm2 || (A = {}, A.supported = !1, t = !1), !t && A ? e.when(A) : p()
      }

      function x(e) {
        A && (A.enabled = e)
      }

      function v(t) {
        return I.info("setStateInfo", t), o.setState({}, {
          enabled: t.enabled,
          baseFrameRate: t.baseFrameRate,
          fanVolume: t.fanVolume
        }).then(function(e) {
          return x(t.enabled), i.trigger(s.CHANGED), e
        }).catch(function(t) {
          return I.error("failed quietmode SetState info", t), e.reject(t)
        })
      }

      function y(e) {
        I.info("onSupportInfoUpdate data:", e), u({
          data: e
        }), i.trigger(s.SUPPORT_UPDATE)
      }

      function w(e) {
        I.info("onStateInfoUpdated data:", e), f({
          data: e
        }), i.trigger(s.STATE_UPDATE)
      }

      function S() {
        k = null, g()
      }

      function E() {
        var e = "/QuietMode2/v.1.0/state",
          t = "/QuietMode2/v.1.0/support";
        a.register(e, c.STATE_UPDATE), a.register(t, c.SUPPORT_UPDATE), i.on(c.STATE_UPDATE, w), i.on(c
          .SUPPORT_UPDATE, y), i.on(d.LOCALE_CHANGED, S), p(), I.info("service init done")
      }
      var k = null,
        _ = null,
        T = null,
        C = this,
        O = null,
        A = null,
        I = n.getInstance("main.common/quietMode2Service"),
        M = 2e3;
      C.init = E, C.getStateInfo = b, C.getSupportInfo = h, C.setStateInfo = v
    }
  ]);
  t.quietMode2Service = l
}
