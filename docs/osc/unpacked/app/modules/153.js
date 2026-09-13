// ─────────────────────────────────────────────────────────────
// APP MODULE 153
// role       : provider websocketService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.websocketService = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.provider("websocketService", [function() {
      function e(e, t, n) {
        var i = this;
        i.readyState = a, i.queue = [], i.onsend = null;
        var o = function(e) {
          n && t.info("WSMock " + e)
        };
        this.flush = function() {
          for (var e = i.queue.length, t = 0; t < e; ++t) i.queue[t]();
          i.queue = []
        }, this.mockOpen = function(t, n, r, a) {
          i.onopen = t, i.onclose = n, i.onmessage = r, i.onerror = a, o("opened mock server url=" + e), i.queue
            .push(function() {
              i.readyState = l, i.onopen()
            }), setTimeout(function() {
              i.flush()
            }, 1e3)
        }, this.mockServerSend = function(e) {
          if (i.readyState !== l) throw "Websocket not in OPEN state";
          o("mock server sends message " + e);
          var t = {};
          t.data = e, i.queue.push(i.onmessage.bind(void 0, t)), setTimeout(function() {
            i.flush()
          }, 1e3)
        }, this.mockOnSend = function(e) {
          i.onsend = e
        }, this.close = function() {
          i.readyState = s, o("mock server closing"), i.queue.push(function() {
            i.readyState = d, i.onclose()
          }), setTimeout(function() {
            i.flush()
          }, 1e3)
        }, this.send = function(e) {
          if (i.readyState !== l) throw "Websocket not in OPEN state";
          o("mock server receiving message " + e), i.onsend && i.onsend(e)
        }
      }

      function t(e, t, n, i) {
        var o = this;
        if (o.webSocket = void 0, o.eventMap = {}, o.queue = [], !e) throw "URL must be specified";
        var a = function(n) {
            i && t.info("WS[" + e + "]: " + n)
          },
          s = function(e, t) {
            e in o.eventMap && o.eventMap[e](t)
          },
          d = function() {
            return o.webSocket ? o.webSocket.readyState : o.CLOSED
          },
          c = function() {
            a("onopen");
            for (var e = 0; e < o.queue.length && o.ready(); ++e) o.send(o.queue[e]);
            o.queue = [], s("$open")
          },
          u = function() {
            a("onclose"), s("$close")
          },
          f = function(e) {
            a("onmessage: " + e);
            var t;
            try {
              t = JSON.parse(e.data)
            } catch (n) {
              t = e.data
            }
            s("$message", t)
          },
          m = function(e) {
            a("onerror: " + e), s("$error", e)
          };
        this.open = function() {
          n ? (o.webSocket = r[e], o.webSocket ? o.webSocket.mockOpen(c, u, f, m) : (a("Cannot find mock server"),
            u())) : (o.webSocket = new WebSocket(e), o.webSocket.onopen = c, o.webSocket.onclose = u, o
            .webSocket.onmessage = f, o.webSocket.onerror = m)
        }, this.close = function() {
          o.webSocket && (o.webSocket.close(), o.webSocket = null)
        }, this.send = function(e) {
          o.ready() ? o.webSocket.send(e) : o.queue.push(e)
        }, this.on = function(e, t) {
          o.eventMap[e] = t
        }, this.ready = function() {
          return d() === l
        }, this.url = function() {
          return e
        }
      }
      var n = !1,
        i = !1;
      o = {};
      var o = {},
        r = {},
        a = 0,
        l = 1,
        s = 2,
        d = 3;
      return r = {}, {
        mockBackend: function() {
          n = !0
        },
        setVerbose: function(e) {
          i = e
        },
        $get: ["$log", function(a) {
          return {
            create: function(e) {
              var r = o[e];
              return r || (r = new t(e, a, n, i), o[e] = r), r
            },
            createMockServer: function(t) {
              var n = r[t];
              return n || (n = new e(t, a, i), r[t] = n), n
            }
          }
        }]
      }
    }]);
  t.websocketService = o
}
