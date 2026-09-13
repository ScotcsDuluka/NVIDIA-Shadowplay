// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 5
// directive hoverFocus | directive hoverFocusGallery | directive focus1 | directive focus | directive focusOn | directive focusOnly
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
      value: !0
    }), exports.focusOnly = exports.focusOn = exports.focus = exports.focus1 = exports.hoverFocusGallery =
    exports.hoverFocus = void 0;
  var i = require(1) /* app/1 — main (module) */,
    o = i.ngMainModule.directive("hoverFocus", function() {
      return {
        link: function(e, t) {
          t.on("mouseenter", function() {
            t[0].focus();
          }), t.on("mouseleave", function() {
            t[0].blur();
          });
        }
      };
    }),
    r = i.ngMainModule.directive("hoverFocusGallery", ["$log", function(e) {
      return {
        restrict: "A",
        link: function(e, t, n) {
          e.$watch(n.hoverFocusGallery, function(e) {
            e === !0 && (t.on("mouseenter", function() {
              t[0].focus();
            }), t.on("mouseleave", function() {
              t[0].blur();
            }));
          });
        }
      };
    }]),
    a = i.ngMainModule.directive("focus1", ["$timeout", "$log", function(e, t) {
      return {
        restrict: "A",
        scope: {
          trigger: "@focus1"
        },
        link: function(t, n, i) {
          t.$watch("trigger", function(t) {
            var i = t.split(",");
            i[0] === i[1] && e(function() {
              n[0].focus();
            }, 10);
          });
        }
      };
    }]),
    l = i.ngMainModule.directive("focus", ["$timeout", function(e) {
      return {
        restrict: "A",
        scope: {
          trigger: "@focus"
        },
        link: function(t, n) {
          t.$watch("trigger", function(t) {
            "true" === t && e(function() {
              n[0].focus();
            }, 10);
          });
        }
      };
    }]),
    s = i.ngMainModule.directive("focusOn", ["$timeout", function(e) {
      return {
        restrict: "A",
        link: function(t, n, i) {
          t.$watch(i.focusOn, function(t) {
            t === !0 && e(function() {
              n[0].focus();
            }, 10);
          });
        }
      };
    }]),
    d = i.ngMainModule.directive("focusOnly", ["eventAggregator", "COMMON_EVENTS", function(e, t) {
      return {
        link: function(n, i) {
          i.on("mouseenter", function() {
            i[0].focus();
          }), i.on("focus", function() {
            e.trigger(t.ELEMENT_FOCUSSED, i[0]);
          });
        }
      };
    }]);
  exports.hoverFocus = o, exports.hoverFocusGallery = r, exports.focus1 = a, exports.focus = l, exports
    .focusOn = s, exports.focusOnly = d;
}
