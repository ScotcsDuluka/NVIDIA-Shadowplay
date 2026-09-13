// ─────────────────────────────────────────────────────────────
// APP MODULE 20
// role       : provider socketService
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
  }), t.socketService = void 0;
  var o = n(2),
    r = n(485),
    a = i(r);
  n(17);
  var l = o.ngMainCommonModule.provider("socketService", [function() {
    var e, t;
    return {
      setConfig: function(n) {
        e = n.server, t = n.port
      },
      $get: ["$rootScope", "$log", "$window", "eventAggregator", "SOCKETIO_EVENTS", "localSdk", function(e, t, n,
        i, o, r) {
        function l(t) {
          m.on(t, function() {
            g.info("socket io event", t), e.$apply(function() {
              i.trigger(t, {})
            })
          })
        }

        function s() {
          l(o.CONNECT), l(o.DISCONNECT), l(o.ERROR)
        }

        function d() {
          if (a.default) {
            var e = r.getNodeConfig(),
              t = {
                query: {
                  X_LOCAL_SECURITY_COOKIE: e.commonHeaders.X_LOCAL_SECURITY_COOKIE
                }
              };
            m = a.default(e.server + e.port, t), m && (g.info("socket connect"), m.connect(), s(m))
          }
        }

        function c() {
          m && (g.info("socket disconnect"), m.disconnect())
        }

        function u(t, n, i) {
          m && m.emit(t, n, function() {
            var t = arguments;
            e.$apply(function() {
              i && i.apply(m, t)
            })
          })
        }

        function f(t, n) {
          m && m.on(t, function() {
            var t = Array.prototype.slice.call(arguments);
            e.$apply(function() {
              var e = i.trigger;
              t.unshift(n), e.apply(null, t)
            })
          })
        }
        var m, g = t.getInstance("osc/socketService");
        return {
          connect: d,
          disconnect: c,
          emit: u,
          register: f
        }
      }]
    }
  }]);
  t.socketService = l
}
