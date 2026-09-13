// ─────────────────────────────────────────────────────────────
// APP MODULE 91
// role       : service gamepadService
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
  }), t.gamepadService = void 0;
  var o = n(3),
    r = i(o),
    a = n(2),
    l = a.ngMainCommonModule.service("gamepadService", ["$log", "$window", "$rootScope", "$timeout", "OSC_KEYBOARD",
      "cefService", "eventAggregator", "GAMEPAD_EVENTS",
      function(e, t, n, i, o, a, l, s) {
        function d(e) {
          p.info("GamepadService entered")
        }

        function c(e) {
          h = !e
        }

        function u() {
          return h ? (h = !1, m("Left", !1), m("Right", !1), E = "None", void p.info("Gamepad Polling stopped")) : (d
            .poll(), void window.requestAnimationFrame(u))
        }

        function f(e) {
          switch (e) {
            case "Left":
              return v.DpadLeft;
            case "Right":
              return v.DpadRight
          }
        }

        function m(e, t) {
          switch (e) {
            case "Left":
              v.DpadLeft = t;
            case "Right":
              v.DpadRight = t;
            default:
              return
          }
        }

        function g(e, t) {
          switch (e) {
            case "Left":
              t ? y = i(function() {
                v.DpadLeft = !0
              }, 500) : (i.cancel(y), v.DpadLeft = !1);
            case "Right":
              t ? w = i(function() {
                v.DpadRight = !0
              }, 500) : (i.cancel(w), v.DpadRight = !1);
            default:
              return
          }
        }
        var p = e.getInstance("osc/gamepadService"),
          h = !1,
          b = .239,
          x = .265,
          v = {
            DpadLeft: !1,
            DpadRight: !1
          },
          y = null,
          w = null,
          S = "NVIDIA Controller v01.03 (STANDARD GAMEPAD Vendor: 0955 Product: 7210)",
          E = "None";
        return d.poll = function() {
          var e, n = t.navigator.getGamepads(),
            i = {};
          for (e in d.gamepads) i[e] = null;
          for (e = 0; e != n.length; ++e) {
            var o = n[e];
            if (o) {
              if (o.id === S) {
                E = o.id;
                continue
              }
              "None" === E && (E = o.id);
              var c, u, m = o.buttons,
                h = o.axes,
                v = !1,
                y = !1,
                w = [];
              o.index in d.gamepads ? c = d.gamepads[o.index] : (c = d.gamepads[o.index] = {
                index: o.index,
                id: null,
                mapping: null,
                buttons: angular.copy(d.DEFAULT_BUTTONS),
                DPad: angular.copy(d.DEFAULT_DPAD),
                LS: angular.copy(d.DEFAULT_AXIS),
                RS: angular.copy(d.DEFAULT_AXIS)
              }, y = !0), c.id = o.id;
              for (u in d.BUTTON_MAPPING) {
                var k = d.BUTTON_MAPPING[u],
                  _ = k < m.length ? m[k].value : 0;
                if (m[k] && ("L1" === u && m[k].pressed && l.trigger(s.LEFT_BUMPER), "R1" === u && m[k].pressed && l
                    .trigger(s.RIGHT_BUMPER)), c.buttons[u] !== _) {
                  c.buttons[u] = _, 1 === _ && (v = !0, w.push({
                    code: d.CONVERT_TO_KEYBOARD[u],
                    modifier: d.KEYBOARD_MODIFIER[u]
                  })), "X" === u && l.trigger(s.X_BUTTON), "Y" === u && l.trigger(s.Y_BUTTON)
                }
              }
              for (u in d.DPAD_MAPPING) {
                var k = d.DPAD_MAPPING[u],
                  _ = k < m.length ? m[k].value : 0;
                if (c.DPad[u] !== _) {
                  c.DPad[u] = _, 1 === _ ? (v = !0, w.push({
                    code: d.CONVERT_TO_KEYBOARD[u],
                    modifier: d.KEYBOARD_MODIFIER[u]
                  }), g(u, !0)) : g(u, !1)
                } else m[k] && m[k].pressed && f(u) && (v = !0, w.push({
                  code: d.CONVERT_TO_KEYBOARD[u],
                  modifier: d.KEYBOARD_MODIFIER[u]
                }))
              }
              for (u in d.AXIS_MAPPING) {
                var T = d.AXIS_MAPPING[u];
                for (var C in T) {
                  var O = T[C],
                    _ = O < h.length ? h[O] : 0;
                  c[u][C] !== _ && (c[u][C] = _, "LS" === u ? (_ > b || _ < -b) && (v = !0) : "RS" === u && (_ >
                    x || _ < -x) && (v = !0))
                }
              }
              delete i[c.index], y && (++d.count, p.info("Poll: Connected")), v && (l.trigger(s.NAVIGATION), r.each(
                w,
                function(e, t) {
                  0 != e.code && a.oscSendWinKBMessage(e.code, e.modifier)
                }))
            }
          }
          for (e in i) --d.count, p.info("Poll: Disconnected"), delete d.gamepads[e]
        }, d.BUTTON_MAPPING = {
          A: 0,
          B: 1,
          X: 2,
          Y: 3,
          L1: 4,
          L2: 6,
          LS: 10,
          R1: 5,
          R2: 7,
          RS: 11,
          Select: 8,
          Start: 9
        }, d.DPAD_MAPPING = {
          Up: 12,
          Down: 13,
          Right: 15,
          Left: 14
        }, d.AXIS_MAPPING = {
          LS: {
            X: 0,
            Y: 1
          },
          RS: {
            X: 2,
            Y: 3
          }
        }, d.DEFAULT_BUTTONS = {
          A: 0,
          B: 0,
          X: 0,
          Y: 0,
          L1: 0,
          L2: 0,
          LS: 0,
          R1: 0,
          R2: 0,
          RS: 0,
          Select: 0,
          Start: 0,
          Guide: 0
        }, d.DEFAULT_DPAD = {
          Up: 0,
          Down: 0,
          Left: 0,
          Right: 0
        }, d.DEFAULT_AXIS = {
          X: 0,
          Y: 0
        }, d.CONVERT_TO_KEYBOARD = {
          A: o.ENTER,
          B: o.ESCAPE,
          X: 0,
          Y: 0,
          L1: 0,
          L2: 0,
          LS: 0,
          R1: 0,
          R2: 0,
          RS: 0,
          Select: 0,
          Start: 0,
          Up: o.UP_ARROW,
          Down: o.DOWN_ARROW,
          Left: o.LEFT_ARROW,
          Right: o.RIGHT_ARROW
        }, d.KEYBOARD_MODIFIER = {
          A: 0,
          B: 0,
          X: 0,
          Y: 0,
          L1: 0,
          L2: 0,
          LS: 0,
          R1: 0,
          R2: 0,
          RS: 0,
          Select: 0,
          Start: 0,
          Up: 0,
          Down: 0,
          Left: 0,
          Right: 0
        }, d.gamePadInputPolling = function(e) {
          "start" === e ? (p.info("OSC Opening, start polling gamepad"), c(!0), u()) : "stop" === e && (c(!1), p
            .info("OSC Closing, stop polling gamepad"))
        }, d.getControllerDeviceInfo = function() {
          return E
        }, p.info("Gamepad Service entered"), d.$inject = ["$rootScope"], d.init = function() {
          p.info("Initialize Gamepad"), d.$rootScope = n, d.manual = !1, d.gamepads = {}, d.count = 0, window
            .addEventListener("gamepadconnected", function() {
              console.log("gamepad connected")
            }), window.addEventListener("gamepaddisconnected", function() {
              console.log("gamepad disconnected")
            }), window.addEventListener("unload", function() {
              c(!1)
            }), p.info("Initialize Gamepad complete")
        }, d
      }
    ]);
  t.gamepadService = l
}
