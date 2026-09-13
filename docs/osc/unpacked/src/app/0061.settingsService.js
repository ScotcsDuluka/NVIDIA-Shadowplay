// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 61
// service settingsService
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
  }), exports.settingsService = void 0;
  var o = require(3),
    r = i(o),
    a = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(17) /* app/17 — localSdk (provider) */, require(20) /* app/20 — socketService (provider) */;
  var l = a.ngMainCommonModule.service("settingsService", ["$log", "$translate", "$q", "socketService",
    "eventAggregator", "settingsEndpoints", "gfeUpdatesEndpoints", "CLIENT_LOCALES", "COMMON_EVENTS",
    function(e, t, n, i, o, a, l, s, d) {
      function c() {
        m.getAndApplyLanguage();
      }

      function u() {
        m.fetchExperimentalSetting().then(function(e) {
          g.info("experimental flag changed: ", e), o.trigger(d.EXPERIMENTAL_CHANGED, e);
        });
      }

      function f() {
        var e = "/Settings/v.1.0/Language",
          t = "languageNotifier";
        i.register(e, t), o.on(t, c);
        var n = "/gfeupdate/autoGFEDownload/autoGFEbeta",
          r = "experimental.socket";
        i.register(n, r), o.on(r, u);
      }
      var m = this,
        g = e.getInstance("osc/settingsService"),
        p = "en-US",
        h = p;
      m.getLanguage = function() {
        return a.getLanguage().then(function(e) {
          var t = e.data.language;
          return t && t.length > 0 ? t : (g.info(
            "Current language is not saved on the server, use system locale."), h);
        }, function(e) {
          return g.info("Get language error: ", e), p;
        });
      }, m.getAndApplyLanguage = function() {
        return m.getLanguage().then(function(e) {
          g.info("Setting language to: ", e), t.use(e).then(function() {
            t.preferredLanguage(e), t.refresh().then(function() {
              o.trigger(d.LOCALE_CHANGED, e);
            });
          });
        });
      }, m.setSystemLocale = function(e) {
        g.info("Setting language from system: ", e), h = m.getLocaleCodefromLocaleLCID(Number(e));
      }, m.getLocaleCodefromLocaleLCID = function(e) {
        var t = r.findWhere(s, {
          LCID: e
        });
        return r.isUndefined(t) ? p : t.code;
      }, m.experimentalSetting = !1, m.fetchExperimentalSetting = function() {
        return l.getBetaSetting().then(function(e) {
          return m.experimentalSetting = "1" === e.data, m.experimentalSetting;
        }, function(e) {
          return g.info("experiment failed: ", e), m.experimentalSetting = !1, !1;
        });
      }, m.getExperimentalSetting = function() {
        return n.when(m.experimentalSetting);
      }, m.isBeta = !1, m.getIsBetaPackage = function() {
        return a.getBetaPackage().then(function(e) {
          return g.info("BETA: ", e.data.beta), m.isBeta = 1 == e.data.beta, m.isBeta;
        }, function(e) {
          return g.info("beta info read failed:", e), !1;
        });
      }, m.isBetaPackage = function() {
        return m.isBeta;
      }, m.init = function() {
        return g.info("Initialize Settings service"), f(), m.getAndApplyLanguage().then(function() {
          return m.fetchExperimentalSetting().then(function() {
            return m.getIsBetaPackage();
          });
        });
      };
    }
  ]);
  exports.settingsService = l;
}
