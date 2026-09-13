// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 34
// service keyboardService
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
  }), exports.keyboardService = void 0;
  var o = require(108),
    r = i(o),
    a = require(2) /* app/2 — WINDOW_STYLES (constant) */,
    l = a.ngMainCommonModule.service("keyboardService", ["$log", "$document", "eventAggregator",
      "OSC_KEYBOARD", "KEYBOARD_EVENTS",
      function(e, t, n, i, o) {
        var a = this,
          l = e.getInstance("osc/keyboardService");
        a.keyboardHandler = function(e) {
          e.keyCode === i.ESCAPE && (l.info("Escape pressed"), n.trigger(o.ESCAPE, null));
        }, a.addKeyboardMonitor = function() {
          t[0].addEventListener("keyup", a.keyboardHandler);
        }, a.removeKeyboardMonitor = function() {
          t[0].removeEventListener("keyup", a.keyboardHandler);
        }, a.init = function() {
          return l.info("keyboard Service initialized!"), a.addKeyboardMonitor(), !0;
        };
        var s = [16, 160, 161, 17, 162, 163, 18, 164, 165, 91, 92],
          d = {
            160: 16,
            161: 16,
            162: 17,
            163: 17,
            164: 18,
            165: 18
          },
          c = {
            0: "None",
            8: "Backsp",
            9: "Tab",
            16: "Shift",
            17: "Ctrl",
            18: "Alt",
            12: "Clear",
            13: "Enter",
            19: "Pause",
            20: "Caps",
            27: "Esc",
            32: "Space",
            33: "PgUp",
            34: "PgDown",
            35: "End",
            36: "Home",
            37: "Left",
            38: "Up",
            39: "Right",
            40: "Down",
            41: "Select",
            42: "Print",
            43: "Exec",
            44: "PrtScn",
            45: "Insert",
            46: "Delete",
            47: "Help",
            91: "Win",
            92: "Win",
            93: "Apps",
            95: "Sleep",
            96: "Num0",
            97: "Num1",
            98: "Num2",
            99: "Num3",
            100: "Num4",
            101: "Num5",
            102: "Num6",
            103: "Num7",
            104: "Num8",
            105: "Num9",
            106: "Num*",
            107: "Num+",
            108: "Separ",
            109: "Num-",
            110: "Num.",
            111: "Num/",
            112: "F1",
            113: "F2",
            114: "F3",
            115: "F4",
            116: "F5",
            117: "F6",
            118: "F7",
            119: "F8",
            120: "F9",
            121: "F10",
            122: "F11",
            123: "F12",
            124: "F13",
            125: "F14",
            126: "F15",
            127: "F16",
            128: "F17",
            129: "F18",
            130: "F19",
            131: "F20",
            132: "F21",
            133: "F22",
            134: "F23",
            135: "F24",
            144: "NumLck",
            145: "ScrLck",
            160: "Shift",
            161: "Shift",
            162: "Ctrl",
            163: "Ctrl",
            164: "Alt",
            165: "Alt",
            186: ";",
            187: "=",
            188: ",",
            189: "-",
            190: ".",
            191: "/",
            192: "`",
            219: "[",
            220: "\\",
            221: "]",
            222: "'"
          };
        r.default && (0, r.default)(c), a.processCharacter = function(e) {
          if (e in c) return c[e];
          var t = String.fromCharCode(e);
          return "" === t && l.error("ERROR unsupported character read: " + e), t;
        }, a.shortcutToStr = function(e) {
          for (var t = 0 === e.length ? c[0] : "", n = 0; n < e.length; n++) t += a.processCharacter(e[
            n]), n < e.length - 1 && (t += "+");
          return t;
        }, a.hasModifier = function(e) {
          return e.shiftKey || e.ctrlKey || e.altKey || e.metaKey;
        }, a.isModifierOnly = function(e) {
          return a.hasModifier(e) && s.indexOf(e.keyCode) !== -1;
        }, a.normalizeModifier = function(e) {
          return d[e] || e;
        };
      }
    ]);
  exports.keyboardService = l;
}
