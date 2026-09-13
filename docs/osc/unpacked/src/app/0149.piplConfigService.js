// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 149
// service piplConfigService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.piplConfigService = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    o = i.ngMainCommonModule.service("piplConfigService", ["$log", "$q", "eventAggregator", "COMMON_EVENTS",
      "piplConfigEndpoints", "PIPL_CONFIG_SOCKET_EVENTS", "PIPL_CONFIG_SERVICE_EVENTS", "socketService",
      function(e, t, n, i, o, r, a, l) {
        function s() {
          if (_.isNull(m)) {
            f = null;
            var e = o.getPiplConfig();
            return m = e.then(function(e) {
              return f = e.data, u.info("data:", f), n.trigger(i.PIPL_CONFIG_UPDATED, f), f;
            }).catch(function(e) {
              return u.error("failed to get pipl config", e), t.reject(e);
            });
          }
          return m;
        }

        function d(e) {
          u.info("config updated:", e), f = e, m = null, n.trigger(i.PIPL_CONFIG_UPDATED, e);
        }
        var c = this,
          u = e.getInstance("main.utils/PiplConfigService"),
          f = null,
          m = null,
          g = !1;
        c.getPiplConfig = function() {
          return u.info("PiplConfig called"), f ? (u.info("Returning info:", f), t.when(f)) : (u.info(
            "Fetching config"), s());
        }, c.isConnectEnabled = function() {
          return c.getPiplConfig().then(function(e) {
            return u.info("Connect enabled:", e.isConnectEnabled), e.isConnectEnabled;
          }).catch(function(e) {
            return !1;
          });
        }, c.initialize = function() {
          g || (l.register(r.LOCALIZED_CONFIG_UPDATED, a.LOCALIZED_CONFIG_UPDATED), n.on(a
            .LOCALIZED_CONFIG_UPDATED, d), g = !0, u.info("Initialization completed"));
        };
      }
    ]);
  exports.piplConfigService = o;
}
