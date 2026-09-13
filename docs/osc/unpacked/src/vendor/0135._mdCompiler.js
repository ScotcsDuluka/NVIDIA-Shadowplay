// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 135
// service $mdCompiler | service $mdInteraction | service $mdLiveAnnouncer | service $mdColors | service mdFabToolbarAnimation | service mdFabSpeedDialFlingAnimation | service mdFabSpeedDialScaleAnimation | service $$mdInput | service mdInputInvalidAnimation | service mdInputMessagesAnimation | service mdInputMessageAnimation | service $$mdTooltipRegistry | factory $mdColorUtil | factory $mdConstant | factory $mdMedia | factory $mdUtil | factory $$MdGestureHandler | factory $mdComponentRegistry | factory $mdButtonInkRipple | factory $mdCheckboxInkRipple | factory $mdListInkRipple | factory $mdTabInkRipple | factory $$mdAnimate | factory $$forceReflow | factory $$AnimateRunner | factory $$rAFMutex | factory $animateCss | factory $mdGridLayout | factory $mdSidenav | factory $mdSticky | factory $$mdDateUtil | controller form | controller mdNoInk | controller mdInkRipple | controller mdInkRipple | controller mdTheme | controller mdTheme | controller mdFabToolbar | controller MdFabController | controller mdFabSpeedDial | controller mdFabSpeedDial | controller mdGridTile | controller mdInputContainer | controller MdListController | controller MdNavBarController | controller MdNavItemController | controller mdNavItem | controller mdSelectMenu | controller mdSelectMenu | controller mdOption | controller mdSelectMenu | controller mdSelectMenu | controller mdSelectMenu | controller $mdSidenavController | controller mdContent | controller ngModel | controller ngModel | controller mdVirtualRepeatContainer | controller MdAutocompleteCtrl | controller MdHighlightCtrl | controller MdChipCtrl | controller MdChipsCtrl | controller ngModel | controller ngModel | controller mdAutocomplete | controller MdContactChipsCtrl | controller mdCalendar | controller MenuBarController | controller mdMenu | controller mdMenu | controller mdMenu | controller mdMenu | controller mdMenu | controller mdMenu | controller MenuItemController | controller mdMenu | controller mdMenu | controller mdMenuCtrl | controller MdTabsController | directive mdAutofocus | directive mdAutoFocus | directive mdSidenavFocus | directive mdLayoutCss | directive ngCloak | directive layoutWrap | directive layoutNowrap | directive layoutNoWrap | directive layoutFill | directive layoutLtMd | directive layoutLtLg | directive flexLtMd | directive flexLtLg | directive layoutAlignLtMd | directive layoutAlignLtLg | directive flexOrderLtMd | directive flexOrderLtLg | directive offsetLtMd | directive offsetLtLg | directive hideLtMd | directive hideLtLg | directive showLtMd | directive showLtLg | directive mdInkRipple | directive mdNoInk | directive mdNoBar | directive mdNoStretch | directive mdTheme | directive mdThemable | directive mdThemesDisabled | directive mdButton | directive a | directive mdBottomSheet | directive mdBackdrop | directive mdColors | directive mdCard | directive mdCheckbox | directive mdContent | directive mdDialog | directive mdDivider | directive mdFabActions | directive mdFabToolbar | directive mdFabSpeedDial | directive mdGridList | directive mdGridTile | directive mdGridTileFooter | directive mdGridTileHeader | directive mdInputContainer | directive label | directive input | directive textarea | directive mdMaxlength | directive placeholder | directive ngMessages | directive ngMessage | directive ngMessageExp | directive mdSelectOnFocus | directive mdList | directive mdListItem | directive mdNavBar | directive mdNavItem | directive ngShow | directive ngHide | directive mdProgressLinear | directive mdSelect | directive mdSelectMenu | directive mdOption | directive mdOptgroup | directive mdSelectHeader | directive mdSidenav | directive mdSidenavFocus | directive mdRadioGroup | directive mdRadioButton | directive mdSlider | directive mdSliderContainer | directive mdSwitch | directive mdSubheader | directive mdSwipeLeft | directive mdSwipeRight | directive mdSwipeUp | directive mdSwipeDown | directive mdTooltip | directive mdToolbar | directive mdToast | directive mdTruncate | directive mdVirtualRepeatContainer | directive mdVirtualRepeat | directive mdWhiteframe | directive mdAutocomplete | directive mdAutocompleteParentScope | directive mdHighlightText | directive mdChip | directive mdChipRemove | directive mdChipTransclude | directive mdChips | directive mdContactChips | directive mdCalendar | directive mdCalendarMonth | directive mdCalendarMonthBody | directive mdCalendarYear | directive mdCalendarYearBody | directive mdDatepicker | directive mdIcon | directive mdMenuBar | directive mdMenuDivider | directive mdMenuItem | directive mdMenu | directive mdProgressCircular | directive mdTab | directive mdTabItem | directive mdTabLabel | directive mdTabScroll | directive mdTabs | directive mdTabsDummyWrapper | directive mdTabsTemplate | provider $mdAria | provider $mdGesture | provider $$mdLayout | provider $$interimElement | provider $$mdMeta | provider $mdInkRipple | provider $mdTheming | provider $mdBottomSheet | provider $mdDialog | provider $mdPanel | provider $mdSelect | provider $mdToast | provider $mdDateLocale | provider $mdIcon | provider $mdMenu | provider $mdProgressCircular | constant $mdColorPalette | constant $$mdSvgRegistry | constant $MD_THEME_CSS | defines angular.module("ngMaterial")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  /*!
   * Angular Material Design
   * https://github.com/angular/material
   * @license MIT
   * v1.1.3
   */
  ! function(e, t, n) {
    "use strict";
    ! function() {
      t.module("ngMaterial", ["ng", "ngAnimate", "ngAria", "material.core", "material.core.gestures",
        "material.core.interaction", "material.core.layout", "material.core.meta",
        "material.core.theming.palette", "material.core.theming", "material.core.animate",
        "material.components.autocomplete", "material.components.button",
        "material.components.bottomSheet", "material.components.backdrop", "material.components.colors",
        "material.components.card", "material.components.checkbox", "material.components.content",
        "material.components.chips", "material.components.dialog", "material.components.divider",
        "material.components.fabActions", "material.components.datepicker",
        "material.components.fabToolbar", "material.components.fabShared",
        "material.components.fabSpeedDial", "material.components.gridList", "material.components.icon",
        "material.components.input", "material.components.list", "material.components.panel",
        "material.components.menuBar", "material.components.menu", "material.components.navBar",
        "material.components.progressCircular", "material.components.showHide",
        "material.components.progressLinear", "material.components.select", "material.components.sidenav",
        "material.components.radioButton", "material.components.slider", "material.components.sticky",
        "material.components.switch", "material.components.subheader", "material.components.swipe",
        "material.components.tabs", "material.components.tooltip", "material.components.toolbar",
        "material.components.toast", "material.components.truncate", "material.components.virtualRepeat",
        "material.components.whiteframe"
      ]);
    }(),
    function() {
      function e(e, t) {
        if (t.has("$swipe")) {
          var n =
            "You are using the ngTouch module. \nAngular Material already has mobile click, tap, and swipe support... \nngTouch is not supported with Angular Material!";
          e.warn(n);
        }
      }

      function n(e, t) {
        e.decorator("$$rAF", ["$delegate", r]), e.decorator("$q", ["$delegate", i]), t.theme("default")
          .primaryPalette("indigo").accentPalette("pink").warnPalette("deep-orange").backgroundPalette(
          "grey");
      }

      function r(e) {
        return e.throttle = function(t) {
          var n, r, i, o;
          return function() {
            n = arguments, o = this, i = t, r || (r = !0, e(function() {
              i.apply(o, Array.prototype.slice.call(n)), r = !1;
            }));
          };
        }, e;
      }

      function i(e) {
        return e.resolve || (e.resolve = e.when), e;
      }
      e.$inject = ["$log", "$injector"], n.$inject = ["$provide", "$mdThemingProvider"], r.$inject = [
        "$delegate"
      ], i.$inject = ["$delegate"], t.module("material.core", ["ngAnimate", "material.core.animate",
        "material.core.layout", "material.core.interaction", "material.core.gestures",
        "material.core.theming"
      ]).config(n).run(e);
    }(),
    function() {
      function e(e) {
        function n(n, r, i) {
          function o(e) {
            t.isUndefined(e) && (e = !0), r.toggleClass("md-autofocus", !!e);
          }
          var a = i.mdAutoFocus || i.mdAutofocus || i.mdSidenavFocus;
          o(e(a)(n)), a && n.$watch(a, o);
        }
        return {
          restrict: "A",
          link: {
            pre: n
          }
        };
      }
      e.$inject = ["$parse"], t.module("material.core").directive("mdAutofocus", e).directive("mdAutoFocus",
        e).directive("mdSidenavFocus", e);
    }(),
    function() {
      function e() {
        function e(e) {
          var t = "#" === e[0] ? e.substr(1) : e,
            n = t.length / 3,
            r = t.substr(0, n),
            i = t.substr(n, n),
            o = t.substr(2 * n);
          return 1 === n && (r += r, i += i, o += o), "rgba(" + parseInt(r, 16) + "," + parseInt(i, 16) +
            "," + parseInt(o, 16) + ",0.1)";
        }

        function t(e) {
          e = e.match(/^rgba?[\s+]?\([\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?/i);
          var t = e && 4 === e.length ? "#" + ("0" + parseInt(e[1], 10).toString(16)).slice(-2) + ("0" +
              parseInt(e[2], 10).toString(16)).slice(-2) + ("0" + parseInt(e[3], 10).toString(16)).slice(-2) :
            "";
          return t.toUpperCase();
        }

        function n(e) {
          return e.replace(")", ", 0.1)").replace("(", "a(");
        }

        function r(e) {
          return e ? e.replace("rgba", "rgb").replace(/,[^\),]+\)/, ")") : "rgb(0,0,0)";
        }
        return {
          rgbaToHex: t,
          hexToRgba: e,
          rgbToRgba: n,
          rgbaToRgb: r
        };
      }
      t.module("material.core").factory("$mdColorUtil", e);
    }(),
    function() {
      function e() {
        function e(e) {
          var t = a + "-" + e,
            i = r(t),
            s = i.charAt(0).toLowerCase() + i.substring(1);
          return n(o, e) ? e : n(o, i) ? i : n(o, s) ? s : e;
        }

        function n(e, n) {
          return t.isDefined(e.style[n]);
        }

        function r(e) {
          return e.replace(c, function(e, t, n, r) {
            return r ? n.toUpperCase() : n;
          });
        }

        function i(e) {
          var t,
            n,
            r = /^(Moz|webkit|ms)(?=[A-Z])/;
          for (t in e.style)
            if (n = r.exec(t)) return n[0];
        }
        var o = document.createElement("div"),
          a = i(o),
          s = /webkit/i.test(a),
          c = /([:\-_]+(.))/g,
          u = {
            isInputKey: function(e) {
              return e.keyCode >= 31 && e.keyCode <= 90;
            },
            isNumPadKey: function(e) {
              return 3 === e.location && e.keyCode >= 97 && e.keyCode <= 105;
            },
            isNavigationKey: function(e) {
              var t = u.KEY_CODE,
                n = [t.SPACE, t.ENTER, t.UP_ARROW, t.DOWN_ARROW];
              return n.indexOf(e.keyCode) != -1;
            },
            ELEMENT_MAX_PIXELS: 1533917,
            BEFORE_NG_ARIA: 210,
            KEY_CODE: {
              COMMA: 188,
              SEMICOLON: 186,
              ENTER: 13,
              ESCAPE: 27,
              SPACE: 32,
              PAGE_UP: 33,
              PAGE_DOWN: 34,
              END: 35,
              HOME: 36,
              LEFT_ARROW: 37,
              UP_ARROW: 38,
              RIGHT_ARROW: 39,
              DOWN_ARROW: 40,
              TAB: 9,
              BACKSPACE: 8,
              DELETE: 46
            },
            CSS: {
              TRANSITIONEND: "transitionend" + (s ? " webkitTransitionEnd" : ""),
              ANIMATIONEND: "animationend" + (s ? " webkitAnimationEnd" : ""),
              TRANSFORM: e("transform"),
              TRANSFORM_ORIGIN: e("transformOrigin"),
              TRANSITION: e("transition"),
              TRANSITION_DURATION: e("transitionDuration"),
              ANIMATION_PLAY_STATE: e("animationPlayState"),
              ANIMATION_DURATION: e("animationDuration"),
              ANIMATION_NAME: e("animationName"),
              ANIMATION_TIMING: e("animationTimingFunction"),
              ANIMATION_DIRECTION: e("animationDirection")
            },
            MEDIA: {
              xs: "(max-width: 599px)",
              "gt-xs": "(min-width: 600px)",
              sm: "(min-width: 600px) and (max-width: 959px)",
              "gt-sm": "(min-width: 960px)",
              md: "(min-width: 960px) and (max-width: 1279px)",
              "gt-md": "(min-width: 1280px)",
              lg: "(min-width: 1280px) and (max-width: 1919px)",
              "gt-lg": "(min-width: 1920px)",
              xl: "(min-width: 1920px)",
              landscape: "(orientation: landscape)",
              portrait: "(orientation: portrait)",
              print: "print"
            },
            MEDIA_PRIORITY: ["xl", "gt-lg", "lg", "gt-md", "md", "gt-sm", "sm", "gt-xs", "xs", "landscape",
              "portrait", "print"
            ]
          };
        return u;
      }
      t.module("material.core").factory("$mdConstant", e);
    }(),
    function() {
      function e(e, n) {
        function r() {
          return [].concat(y);
        }

        function i() {
          return y.length;
        }

        function o(e) {
          return y.length && e > -1 && e < y.length;
        }

        function a(e) {
          return !!e && o(f(e) + 1);
        }

        function s(e) {
          return !!e && o(f(e) - 1);
        }

        function c(e) {
          return o(e) ? y[e] : null;
        }

        function u(e, t) {
          return y.filter(function(n) {
            return n[e] === t;
          });
        }

        function l(e, n) {
          return e ? (t.isNumber(n) || (n = y.length), y.splice(n, 0, e), f(e)) : -1;
        }

        function d(e) {
          h(e) && y.splice(f(e), 1);
        }

        function f(e) {
          return y.indexOf(e);
        }

        function h(e) {
          return e && f(e) > -1;
        }

        function p() {
          return y.length ? y[0] : null;
        }

        function m() {
          return y.length ? y[y.length - 1] : null;
        }

        function v(e, r, i, a) {
          i = i || g;
          for (var s = f(r);;) {
            if (!o(s)) return null;
            var c = s + (e ? -1 : 1),
              u = null;
            if (o(c) ? u = y[c] : n && (u = e ? m() : p(), c = f(u)), null === u || c === a) return null;
            if (i(u)) return u;
            t.isUndefined(a) && (a = c), s = c;
          }
        }
        var g = function() {
          return !0;
        };
        e && !t.isArray(e) && (e = Array.prototype.slice.call(e)), n = !!n;
        var y = e || [];
        return {
          items: r,
          count: i,
          inRange: o,
          contains: h,
          indexOf: f,
          itemAt: c,
          findBy: u,
          add: l,
          remove: d,
          first: p,
          last: m,
          next: t.bind(null, v, !1),
          previous: t.bind(null, v, !0),
          hasPrevious: s,
          hasNext: a
        };
      }
      t.module("material.core").config(["$provide", function(t) {
        t.decorator("$mdUtil", ["$delegate", function(t) {
          return t.iterator = e, t;
        }]);
      }]);
    }(),
    function() {
      function e(e, n, r) {
        function i(e) {
          var n = f[e];
          t.isUndefined(n) && (n = f[e] = o(e));
          var r = p[n];
          return t.isUndefined(r) && (r = a(n)), r;
        }

        function o(t) {
          return e.MEDIA[t] || ("(" !== t.charAt(0) ? "(" + t + ")" : t);
        }

        function a(e) {
          var t = h[e];
          return t || (t = h[e] = r.matchMedia(e)), t.addListener(s), p[t.media] = !!t.matches;
        }

        function s(e) {
          n.$evalAsync(function() {
            p[e.media] = !!e.matches;
          });
        }

        function c(e) {
          return h[e];
        }

        function u(t, n) {
          for (var r = 0; r < e.MEDIA_PRIORITY.length; r++) {
            var i = e.MEDIA_PRIORITY[r];
            if (h[f[i]].matches) {
              var o = d(t, n + "-" + i);
              if (t[o]) return t[o];
            }
          }
          return t[d(t, n)];
        }

        function l(n, r, i) {
          var o = [];
          return n.forEach(function(n) {
              var a = d(r, n);
              t.isDefined(r[a]) && o.push(r.$observe(a, t.bind(void 0, i, null)));
              for (var s in e.MEDIA) a = d(r, n + "-" + s), t.isDefined(r[a]) && o.push(r.$observe(a, t
                .bind(void 0, i, s)));
            }),
            function() {
              o.forEach(function(e) {
                e();
              });
            };
        }

        function d(e, t) {
          return m[t] || (m[t] = e.$normalize(t));
        }
        var f = {},
          h = {},
          p = {},
          m = {};
        return i.getResponsiveAttribute = u, i.getQuery = c, i.watchResponsiveAttributes = l, i;
      }
      e.$inject = ["$mdConstant", "$rootScope", "$window"], t.module("material.core").factory("$mdMedia", e);
    }(),
    function() {
      function e(e, n) {
        function r(e) {
          return e = t.isArray(e) ? e : [e], e.forEach(function(t) {
            c.forEach(function(n) {
              e.push(n + "-" + t);
            });
          }), e;
        }

        function i(e) {
          return e = t.isArray(e) ? e : [e], r(e).map(function(e) {
            return "[" + e + "]";
          }).join(",");
        }

        function o(e, t) {
          if (e = s(e), !e) return !1;
          for (var n = r(t), i = 0; i < n.length; i++)
            if (e.hasAttribute(n[i])) return !0;
          return !1;
        }

        function a(e, t) {
          e = s(e), e && r(t).forEach(function(t) {
            e.removeAttribute(t);
          });
        }

        function s(e) {
          if (e = e[0] || e, e.nodeType) return e;
        }
        var c = ["data", "x"];
        return e ? n ? i(e) : r(e) : {
          buildList: r,
          buildSelector: i,
          hasAttribute: o,
          removeAttribute: a
        };
      }
      t.module("material.core").config(["$provide", function(t) {
        t.decorator("$mdUtil", ["$delegate", function(t) {
          return t.prefixer = e, t;
        }]);
      }]);
    }(),
    function() {
      function r(r, o, a, s, c, u, l, d, f, h) {
        function p(e) {
          return e ? m(e) || v(e) ? e : e + "px" : "0";
        }

        function m(e) {
          return String(e).indexOf("px") > -1;
        }

        function v(e) {
          return String(e).indexOf("%") > -1;
        }

        function g(e) {
          return e[0] || e;
        }
        var y = u.startSymbol(),
          b = u.endSymbol(),
          E = "{{" === y && "}}" === b,
          _ = function(e, n, r) {
            var i = !1;
            if (e && e.length) {
              var o = f.getComputedStyle(e[0]);
              i = t.isDefined(o[n]) && (!r || o[n] == r);
            }
            return i;
          },
          $ = {
            dom: {},
            now: e.performance && e.performance.now ? t.bind(e.performance, e.performance.now) : Date.now ||
              function() {
                return new Date().getTime();
              },
            getModelOption: function(e, t) {
              if (e.$options) {
                var n = e.$options;
                return n.getOption ? n.getOption(t) : n[t];
              }
            },
            bidi: function(e, n, i, o) {
              var a = !("rtl" == r[0].dir || "rtl" == r[0].body.dir);
              if (0 == arguments.length) return a ? "ltr" : "rtl";
              var s = t.element(e);
              a && t.isDefined(i) ? s.css(n, p(i)) : !a && t.isDefined(o) && s.css(n, p(o));
            },
            bidiProperty: function(e, n, i, o) {
              var a = !("rtl" == r[0].dir || "rtl" == r[0].body.dir),
                s = t.element(e);
              a && t.isDefined(n) ? (s.css(n, p(o)), s.css(i, "")) : !a && t.isDefined(i) && (s.css(i, p(
                o)), s.css(n, ""));
            },
            clientRect: function(e, t, n) {
              var r = g(e);
              t = g(t || r.offsetParent || document.body);
              var i = r.getBoundingClientRect(),
                o = n ? t.getBoundingClientRect() : {
                  left: 0,
                  top: 0,
                  width: 0,
                  height: 0
                };
              return {
                left: i.left - o.left,
                top: i.top - o.top,
                width: i.width,
                height: i.height
              };
            },
            offsetRect: function(e, t) {
              return $.clientRect(e, t, !0);
            },
            nodesToArray: function(e) {
              e = e || [];
              for (var t = [], n = 0; n < e.length; ++n) t.push(e.item(n));
              return t;
            },
            getViewportTop: function() {
              return e.scrollY || e.pageYOffset || 0;
            },
            findFocusTarget: function(e, n) {
              function r(e, n) {
                var r,
                  i = e[0].querySelectorAll(n);
                return i && i.length && i.length && t.forEach(i, function(e) {
                  e = t.element(e);
                  var n = e.hasClass("md-autofocus");
                  n && (r = e);
                }), r;
              }
              var i,
                o = this.prefixer("md-autofocus", !0);
              return i = r(e, n || o), i || n == o || (i = r(e, this.prefixer("md-auto-focus", !0)), i || (
                i = r(e, o))), i;
            },
            disableScrollAround: function(e, n, i) {
              function o(e) {
                function n(e) {
                  e.preventDefault();
                }
                e = t.element(e || s);
                var r;
                return i.disableScrollMask ? r = e : (r = t.element(
                      '<div class="md-scroll-mask">  <div class="md-scroll-mask-bar"></div></div>'), e
                    .append(r)), r.on("wheel", n), r.on("touchmove", n),
                  function() {
                    r.off("wheel"), r.off("touchmove"), i.disableScrollMask || r[0].parentNode.removeChild(
                      r[0]);
                  };
              }

              function a() {
                var e = r[0].documentElement,
                  n = e.style.cssText || "",
                  i = s.style.cssText || "",
                  o = $.getViewportTop(),
                  a = s.clientWidth,
                  c = s.scrollHeight > s.clientHeight + 1;
                return c && t.element(s).css({
                    position: "fixed",
                    width: "100%",
                    top: -o + "px"
                  }), s.clientWidth < a && (s.style.overflow = "hidden"), c && (e.style.overflowY =
                    "scroll"),
                  function() {
                    s.style.cssText = i, e.style.cssText = n, s.scrollTop = o;
                  };
              }
              if (i = i || {}, $.disableScrollAround._count = Math.max(0, $.disableScrollAround._count ||
                0), $.disableScrollAround._count++, $.disableScrollAround._restoreScroll) return $
                .disableScrollAround._restoreScroll;
              var s = r[0].body,
                c = a(),
                u = o(n);
              return $.disableScrollAround._restoreScroll = function() {
                --$.disableScrollAround._count <= 0 && (c(), u(), delete $.disableScrollAround
                  ._restoreScroll);
              };
            },
            enableScrolling: function() {
              var e = this.disableScrollAround._restoreScroll;
              e && e();
            },
            floatingScrollbars: function() {
              if (this.floatingScrollbars.cached === n) {
                var e = t.element("<div><div></div></div>").css({
                  width: "100%",
                  "z-index": -1,
                  position: "absolute",
                  height: "35px",
                  "overflow-y": "scroll"
                });
                e.children().css("height", "60px"), r[0].body.appendChild(e[0]), this.floatingScrollbars
                  .cached = e[0].offsetWidth == e[0].childNodes[0].offsetWidth, e.remove();
              }
              return this.floatingScrollbars.cached;
            },
            forceFocus: function(t) {
              var n = t[0] || t;
              document.addEventListener("click", function e(t) {
                t.target === n && t.$focus && (n.focus(), t.stopImmediatePropagation(), t
                  .preventDefault(), n.removeEventListener("click", e));
              }, !0);
              var r = document.createEvent("MouseEvents");
              r.initMouseEvent("click", !1, !0, e, {}, 0, 0, 0, 0, !1, !1, !1, !1, 0, null), r.$material = !
                0, r.$focus = !0, n.dispatchEvent(r);
            },
            createBackdrop: function(e, t) {
              return a($.supplant('<md-backdrop class="{0}">', [t]))(e);
            },
            supplant: function(e, t, n) {
              return n = n || /\{([^\{\}]*)\}/g, e.replace(n, function(e, n) {
                var r = n.split("."),
                  i = t;
                try {
                  for (var o in r) r.hasOwnProperty(o) && (i = i[r[o]]);
                } catch (t) {
                  i = e;
                }
                return "string" == typeof i || "number" == typeof i ? i : e;
              });
            },
            fakeNgModel: function() {
              return {
                $fake: !0,
                $setTouched: t.noop,
                $setViewValue: function(e) {
                  this.$viewValue = e, this.$render(e), this.$viewChangeListeners.forEach(function(e) {
                    e();
                  });
                },
                $isEmpty: function(e) {
                  return 0 === ("" + e).length;
                },
                $parsers: [],
                $formatters: [],
                $viewChangeListeners: [],
                $render: t.noop
              };
            },
            debounce: function(e, t, r, i) {
              var a;
              return function() {
                var s = r,
                  c = Array.prototype.slice.call(arguments);
                o.cancel(a), a = o(function() {
                  a = n, e.apply(s, c);
                }, t || 10, i);
              };
            },
            throttle: function(e, t) {
              var n;
              return function() {
                var r = this,
                  i = arguments,
                  o = $.now();
                (!n || o - n > t) && (e.apply(r, i), n = o);
              };
            },
            time: function(e) {
              var t = $.now();
              return e(), $.now() - t;
            },
            valueOnUse: function(e, t, n) {
              var r = null,
                i = Array.prototype.slice.call(arguments),
                o = i.length > 3 ? i.slice(3) : [];
              Object.defineProperty(e, t, {
                get: function() {
                  return null === r && (r = n.apply(e, o)), r;
                }
              });
            },
            nextUid: function() {
              return "" + i++;
            },
            disconnectScope: function(e) {
              if (e && e.$root !== e && !e.$$destroyed) {
                var t = e.$parent;
                e.$$disconnected = !0, t.$$childHead === e && (t.$$childHead = e.$$nextSibling), t
                  .$$childTail === e && (t.$$childTail = e.$$prevSibling), e.$$prevSibling && (e
                    .$$prevSibling.$$nextSibling = e.$$nextSibling), e.$$nextSibling && (e.$$nextSibling
                    .$$prevSibling = e.$$prevSibling), e.$$nextSibling = e.$$prevSibling = null;
              }
            },
            reconnectScope: function(e) {
              if (e && e.$root !== e && e.$$disconnected) {
                var t = e,
                  n = t.$parent;
                t.$$disconnected = !1, t.$$prevSibling = n.$$childTail, n.$$childHead ? (n.$$childTail
                  .$$nextSibling = t, n.$$childTail = t) : n.$$childHead = n.$$childTail = t;
              }
            },
            getClosest: function(e, n, r) {
              if (t.isString(n)) {
                var i = n.toUpperCase();
                n = function(e) {
                  return e.nodeName.toUpperCase() === i;
                };
              }
              if (e instanceof t.element && (e = e[0]), r && (e = e.parentNode), !e) return null;
              do
                if (n(e)) return e; while (e = e.parentNode);
              return null;
            },
            elementContains: function(n, r) {
              var i = e.Node && e.Node.prototype && Node.prototype.contains,
                o = i ? t.bind(n, n.contains) : t.bind(n, function(e) {
                  return n === r || !!(16 & this.compareDocumentPosition(e));
                });
              return o(r);
            },
            extractElementByName: function(e, n, r, i) {
              function o(e) {
                return a(e) || (r ? s(e) : null);
              }

              function a(e) {
                if (e)
                  for (var t = 0, r = e.length; t < r; t++)
                    if (e[t].nodeName.toLowerCase() === n) return e[t];
                return null;
              }

              function s(e) {
                var t;
                if (e)
                  for (var n = 0, r = e.length; n < r; n++) {
                    var i = e[n];
                    if (!t)
                      for (var a = 0, s = i.childNodes.length; a < s; a++) t = t || o([i.childNodes[a]]);
                  }
                return t;
              }
              var c = o(e);
              return !c && i && l.warn($.supplant("Unable to find node '{0}' in element '{1}'.", [n, e[0]
                .outerHTML
              ])), t.element(c || e);
            },
            initOptionalProperties: function(e, n, r) {
              r = r || {}, t.forEach(e.$$isolateBindings, function(i, o) {
                if (i.optional && t.isUndefined(e[o])) {
                  var a = t.isDefined(n[i.attrName]);
                  e[o] = t.isDefined(r[o]) ? r[o] : a;
                }
              });
            },
            nextTick: function(e, t, n) {
              function r() {
                var e = i.queue,
                  t = i.digest;
                i.queue = [], i.timeout = null, i.digest = !1, e.forEach(function(e) {
                  var t = e.scope && e.scope.$$destroyed;
                  t || e.callback();
                }), t && s.$digest();
              }
              var i = $.nextTick,
                a = i.timeout,
                c = i.queue || [];
              return c.push({
                scope: n,
                callback: e
              }), null == t && (t = !0), i.digest = i.digest || t, i.queue = c, a || (i.timeout = o(r, 0,
                !1));
            },
            processTemplate: function(e) {
              return E ? e : e && t.isString(e) ? e.replace(/\{\{/g, y).replace(/}}/g, b) : e;
            },
            getParentWithPointerEvents: function(e) {
              for (var t = e.parent(); _(t, "pointer-events", "none");) t = t.parent();
              return t;
            },
            getNearestContentElement: function(e) {
              for (var t = e.parent()[0]; t && t !== d[0] && t !== document.body && "MD-CONTENT" !== t
                .nodeName.toUpperCase();) t = t.parentNode;
              return t;
            },
            checkStickySupport: function() {
              var e,
                n = t.element("<div>");
              r[0].body.appendChild(n[0]);
              for (var i = ["sticky", "-webkit-sticky"], o = 0; o < i.length; ++o)
                if (n.css({
                    position: i[o],
                    top: 0,
                    "z-index": 2
                  }), n.css("position") == i[o]) {
                  e = i[o];
                  break;
                }
              return n.remove(), e;
            },
            parseAttributeBoolean: function(e, t) {
              return "" === e || !!e && (t === !1 || "false" !== e && "0" !== e);
            },
            hasComputedStyle: _,
            isParentFormSubmitted: function(e) {
              var n = $.getClosest(e, "form"),
                r = n ? t.element(n).controller("form") : null;
              return !!r && r.$submitted;
            },
            animateScrollTo: function(e, t, n) {
              function r() {
                var n = i();
                e.scrollTop = n, (c ? n < t : n > t) && h(r);
              }

              function i() {
                var e = n || 1e3,
                  t = $.now() - u;
                return o(t, a, s, e);
              }

              function o(e, t, n, r) {
                if (e > r) return t + n;
                var i = (e /= r) * e,
                  o = i * e;
                return t + n * (-2 * o + 3 * i);
              }
              var a = e.scrollTop,
                s = t - a,
                c = a < t,
                u = $.now();
              h(r);
            },
            uniq: function(e) {
              if (e) return e.filter(function(e, t, n) {
                return n.indexOf(e) === t;
              });
            }
          };
        return $.dom.animator = c($), $;
      }
      r.$inject = ["$document", "$timeout", "$compile", "$rootScope", "$$mdAnimate", "$interpolate", "$log",
        "$rootElement", "$window", "$$rAF"
      ];
      var i = 0;
      t.module("material.core").factory("$mdUtil", r), t.element.prototype.focus = t.element.prototype
        .focus || function() {
          return this.length && this[0].focus(), this;
        }, t.element.prototype.blur = t.element.prototype.blur || function() {
          return this.length && this[0].blur(), this;
        };
    }(),
    function() {
      function e() {
        function e() {
          t.showWarnings = !1;
        }
        var t = {
          showWarnings: !0
        };
        return {
          disableWarnings: e,
          $get: ["$$rAF", "$log", "$window", "$interpolate", function(e, r, i, o) {
            return n.apply(t, arguments);
          }]
        };
      }

      function n(e, n, r, i) {
        function o(e, r, i) {
          var o = t.element(e)[0] || e;
          !o || o.hasAttribute(r) && 0 !== o.getAttribute(r).length || l(o, r) || (i = t.isString(i) ? i
          .trim() : "", i.length ? e.attr(r, i) : d && n.warn('ARIA: Attribute "', r,
            '", required for accessibility, is missing on node:', o));
        }

        function a(t, n, r) {
          e(function() {
            o(t, n, r());
          });
        }

        function s(e, t) {
          var n = u(e) || "",
            r = n.indexOf(i.startSymbol()) > -1;
          r ? a(e, t, function() {
            return u(e);
          }) : o(e, t, n);
        }

        function c(e, t) {
          var n = u(e),
            r = n.indexOf(i.startSymbol()) > -1;
          r || n || o(e, t, n);
        }

        function u(e) {
          function t(t) {
            for (; t.parentNode && (t = t.parentNode) !== e;)
              if (t.getAttribute && "true" === t.getAttribute("aria-hidden")) return !0;
          }
          e = e[0] || e;
          for (var n, r = document.createTreeWalker(e, NodeFilter.SHOW_TEXT, null, !1), i = ""; n = r
            .nextNode();) t(n) || (i += n.textContent);
          return i.trim() || "";
        }

        function l(e, t) {
          function n(e) {
            var t = e.currentStyle ? e.currentStyle : r.getComputedStyle(e);
            return "none" === t.display;
          }
          var i = e.hasChildNodes(),
            o = !1;
          if (i)
            for (var a = e.childNodes, s = 0; s < a.length; s++) {
              var c = a[s];
              1 === c.nodeType && c.hasAttribute(t) && (n(c) || (o = !0));
            }
          return o;
        }
        var d = this.showWarnings;
        return {
          expect: o,
          expectAsync: a,
          expectWithText: s,
          expectWithoutText: c,
          getText: u
        };
      }
      n.$inject = ["$$rAF", "$log", "$window", "$interpolate"], t.module("material.core").provider("$mdAria",
        e);
    }(),
    function() {
      function e(e, t, n, r, i) {
        this.$q = e, this.$templateRequest = t, this.$injector = n, this.$compile = r, this.$controller = i;
      }
      e.$inject = ["$q", "$templateRequest", "$injector", "$compile", "$controller"], t.module(
        "material.core").service("$mdCompiler", e), e.prototype.compile = function(e) {
        return e.contentElement ? this._prepareContentElement(e) : this._compileTemplate(e);
      }, e.prototype._prepareContentElement = function(e) {
        var t = this._fetchContentElement(e);
        return this.$q.resolve({
          element: t.element,
          cleanup: t.restore,
          locals: {},
          link: function() {
            return t.element;
          }
        });
      }, e.prototype._compileTemplate = function(e) {
        var n = this,
          r = e.templateUrl,
          i = e.template || "",
          o = t.extend({}, e.resolve),
          a = t.extend({}, e.locals),
          s = e.transformTemplate || t.identity;
        return t.forEach(o, function(e, r) {
          t.isString(e) ? o[r] = n.$injector.get(e) : o[r] = n.$injector.invoke(e);
        }), t.extend(o, a), r ? o.$$ngTemplate = this.$templateRequest(r) : o.$$ngTemplate = this.$q.when(
          i), this.$q.all(o).then(function(r) {
          var i = s(r.$$ngTemplate, e),
            o = e.element || t.element("<div>").html(i.trim()).contents();
          return n._compileElement(r, o, e);
        });
      }, e.prototype._compileElement = function(e, n, r) {
        function i(i) {
          if (e.$scope = i, r.controller) {
            var c = t.extend(e, {
                $element: n
              }),
              u = o.$controller(r.controller, c, !0, r.controllerAs);
            r.bindToController && t.extend(u.instance, e);
            var l = u();
            n.data("$ngControllerController", l), n.children().data("$ngControllerController", l), s
              .controller = l;
          }
          return a(i);
        }
        var o = this,
          a = this.$compile(n),
          s = {
            element: n,
            cleanup: n.remove.bind(n),
            locals: e,
            link: i
          };
        return s;
      }, e.prototype._fetchContentElement = function(e) {
        function n(e) {
          var t = e.parentNode,
            n = e.nextElementSibling;
          return function() {
            n ? t.insertBefore(e, n) : t.appendChild(e);
          };
        }
        var r = e.contentElement,
          i = null;
        return t.isString(r) ? (r = document.querySelector(r), i = n(r)) : (r = r[0] || r, i = document
          .contains(r) ? n(r) : function() {
            r.parentNode && r.parentNode.removeChild(r);
          }), {
          element: t.element(r),
          restore: i
        };
      };
    }(),
    function() {
      function n() {}

      function r(n, r, i) {
        function o(e) {
          return function(t, n) {
            n.distance < this.state.options.maxDistance && this.dispatchEvent(t, e, n);
          };
        }

        function a(e, t, n) {
          var r = p[t.replace(/^\$md./, "")];
          if (!r) throw new Error("Failed to register element with handler " + t + ". Available handlers: " +
            Object.keys(p).join(", "));
          return r.registerElement(e, n);
        }

        function c(e, r) {
          var i = new n(e);
          return t.extend(i, r), p[e] = i, y;
        }

        function u() {
          for (var e = document.createElement("div"), n = ["", "webkit", "Moz", "MS", "ms", "o"], r = 0; r < n
            .length; r++) {
            var i = n[r],
              o = i ? i + "TouchAction" : "touchAction";
            if (t.isDefined(e.style[o])) return o;
          }
        }
        var d = navigator.userAgent || navigator.vendor || e.opera,
          f = d.match(/ipad|iphone|ipod/i),
          h = d.match(/android/i),
          v = u(),
          g = "undefined" != typeof e.jQuery && t.element === e.jQuery,
          y = {
            handler: c,
            register: a,
            isHijackingClicks: (f || h) && !g && !m
          };
        if (y.isHijackingClicks) {
          var b = 6;
          y.handler("click", {
            options: {
              maxDistance: b
            },
            onEnd: o("click")
          }), y.handler("focus", {
            options: {
              maxDistance: b
            },
            onEnd: function(e, t) {
              function n(e) {
                var t = ["INPUT", "SELECT", "BUTTON", "TEXTAREA", "VIDEO", "AUDIO"];
                return "-1" != e.getAttribute("tabindex") && !e.hasAttribute("DISABLED") && (e
                  .hasAttribute("tabindex") || e.hasAttribute("href") || e.isContentEditable || t
                  .indexOf(e.nodeName) != -1);
              }
              t.distance < this.state.options.maxDistance && n(e.target) && (this.dispatchEvent(e,
                "focus", t), e.target.focus());
            }
          }), y.handler("mouseup", {
            options: {
              maxDistance: b
            },
            onEnd: o("mouseup")
          }), y.handler("mousedown", {
            onStart: function(e) {
              this.dispatchEvent(e, "mousedown");
            }
          });
        }
        return y.handler("press", {
          onStart: function(e, t) {
            this.dispatchEvent(e, "$md.pressdown");
          },
          onEnd: function(e, t) {
            this.dispatchEvent(e, "$md.pressup");
          }
        }).handler("hold", {
          options: {
            maxDistance: 6,
            delay: 500
          },
          onCancel: function() {
            i.cancel(this.state.timeout);
          },
          onStart: function(e, n) {
            return this.state.registeredParent ? (this.state.pos = {
              x: n.x,
              y: n.y
            }, void(this.state.timeout = i(t.bind(this, function() {
              this.dispatchEvent(e, "$md.hold"), this.cancel();
            }), this.state.options.delay, !1))) : this.cancel();
          },
          onMove: function(e, t) {
            v || "touchmove" !== e.type || e.preventDefault();
            var n = this.state.pos.x - t.x,
              r = this.state.pos.y - t.y;
            Math.sqrt(n * n + r * r) > this.options.maxDistance && this.cancel();
          },
          onEnd: function() {
            this.onCancel();
          }
        }).handler("drag", {
          options: {
            minDistance: 6,
            horizontal: !0,
            cancelMultiplier: 1.5
          },
          onSetup: function(e, t) {
            v && (this.oldTouchAction = e[0].style[v], e[0].style[v] = t.horizontal ? "pan-y" :
            "pan-x");
          },
          onCleanup: function(e) {
            this.oldTouchAction && (e[0].style[v] = this.oldTouchAction);
          },
          onStart: function(e) {
            this.state.registeredParent || this.cancel();
          },
          onMove: function(e, t) {
            var n, r;
            v || "touchmove" !== e.type || e.preventDefault(), this.state.dragPointer ? this
              .dispatchDragMove(e) : (this.state.options.horizontal ? (n = Math.abs(t.distanceX) > this
                .state.options.minDistance, r = Math.abs(t.distanceY) > this.state.options
                .minDistance * this.state.options.cancelMultiplier) : (n = Math.abs(t.distanceY) >
                this.state.options.minDistance, r = Math.abs(t.distanceX) > this.state.options
                .minDistance * this.state.options.cancelMultiplier), n ? (this.state.dragPointer = s(
                e), l(e, this.state.dragPointer), this.dispatchEvent(e, "$md.dragstart", this.state
                .dragPointer)) : r && this.cancel());
          },
          dispatchDragMove: r.throttle(function(e) {
            this.state.isRunning && (l(e, this.state.dragPointer), this.dispatchEvent(e, "$md.drag",
              this.state.dragPointer));
          }),
          onEnd: function(e, t) {
            this.state.dragPointer && (l(e, this.state.dragPointer), this.dispatchEvent(e,
              "$md.dragend", this.state.dragPointer));
          }
        }).handler("swipe", {
          options: {
            minVelocity: .65,
            minDistance: 10
          },
          onEnd: function(e, t) {
            var n;
            Math.abs(t.velocityX) > this.state.options.minVelocity && Math.abs(t.distanceX) > this.state
              .options.minDistance ? (n = "left" == t.directionX ? "$md.swipeleft" : "$md.swiperight",
                this.dispatchEvent(e, n)) : Math.abs(t.velocityY) > this.state.options.minVelocity &&
              Math.abs(t.distanceY) > this.state.options.minDistance && (n = "up" == t.directionY ?
                "$md.swipeup" : "$md.swipedown", this.dispatchEvent(e, n));
          }
        });
      }

      function i(e) {
        this.name = e, this.state = {};
      }

      function o() {
        function n(e, n, r) {
          r = r || f;
          var i = new t.element.Event(n);
          i.$material = !0, i.pointer = r, i.srcEvent = e, t.extend(i, {
            clientX: r.x,
            clientY: r.y,
            screenX: r.x,
            screenY: r.y,
            pageX: r.x,
            pageY: r.y,
            ctrlKey: e.ctrlKey,
            altKey: e.altKey,
            shiftKey: e.shiftKey,
            metaKey: e.metaKey
          }), t.element(r.target).trigger(i);
        }

        function r(t, n, r) {
          r = r || f;
          var i;
          "click" === n || "mouseup" == n || "mousedown" == n ? (i = document.createEvent("MouseEvents"), i
              .initMouseEvent(n, !0, !0, e, t.detail, r.x, r.y, r.x, r.y, t.ctrlKey, t.altKey, t.shiftKey, t
                .metaKey, t.button, t.relatedTarget || null)) : (i = document.createEvent("CustomEvent"), i
              .initCustomEvent(n, !0, !0, {})), i.$material = !0, i.pointer = r, i.srcEvent = t, r.target
            .dispatchEvent(i);
        }
        var o = "undefined" != typeof e.jQuery && t.element === e.jQuery;
        return i.prototype = {
          options: {},
          dispatchEvent: o ? n : r,
          onSetup: t.noop,
          onCleanup: t.noop,
          onStart: t.noop,
          onMove: t.noop,
          onEnd: t.noop,
          onCancel: t.noop,
          start: function(e, n) {
            if (!this.state.isRunning) {
              var r = this.getNearestParent(e.target),
                i = r && r.$mdGesture[this.name] || {};
              this.state = {
                isRunning: !0,
                options: t.extend({}, this.options, i),
                registeredParent: r
              }, this.onStart(e, n);
            }
          },
          move: function(e, t) {
            this.state.isRunning && this.onMove(e, t);
          },
          end: function(e, t) {
            this.state.isRunning && (this.onEnd(e, t), this.state.isRunning = !1);
          },
          cancel: function(e, t) {
            this.onCancel(e, t), this.state = {};
          },
          getNearestParent: function(e) {
            for (var t = e; t;) {
              if ((t.$mdGesture || {})[this.name]) return t;
              t = t.parentNode;
            }
            return null;
          },
          registerElement: function(e, t) {
            function n() {
              delete e[0].$mdGesture[r.name], e.off("$destroy", n), r.onCleanup(e, t || {});
            }
            var r = this;
            return e[0].$mdGesture = e[0].$mdGesture || {}, e[0].$mdGesture[this.name] = t || {}, e.on(
              "$destroy", n), r.onSetup(e, t || {}), n;
          }
        }, i;
      }

      function a(e, n) {
        function r(e) {
          var t = !e.clientX && !e.clientY;
          t || e.$material || e.isIonicTap || u(e) || (e.preventDefault(), e.stopPropagation());
        }

        function i(e) {
          var t = 0 === e.clientX && 0 === e.clientY;
          t || e.$material || e.isIonicTap || u(e) ? (v = null, "label" == e.target.tagName.toLowerCase() && (
            v = {
              x: e.x,
              y: e.y
            })) : (e.preventDefault(), e.stopPropagation(), v = null);
        }

        function o(e, t) {
          var r;
          for (var i in p) r = p[i], r instanceof n && ("start" === e && r.cancel(), r[e](t, f));
        }

        function a(e) {
          if (!f) {
            var t = +Date.now();
            h && !c(e, h) && t - h.endTime < 1500 || (f = s(e), o("start", e));
          }
        }

        function d(e) {
          f && c(e, f) && (l(e, f), o("move", e));
        }

        function m(e) {
          f && c(e, f) && (l(e, f), f.endTime = +Date.now(), o("end", e), h = f, f = null);
        }
        document.contains || (document.contains = function(e) {
          return document.body.contains(e);
        }), !g && e.isHijackingClicks && (document.addEventListener("click", i, !0), document
          .addEventListener("mouseup", r, !0), document.addEventListener("mousedown", r, !0), document
          .addEventListener("focus", r, !0), g = !0);
        var y = "mousedown touchstart pointerdown",
          b = "mousemove touchmove pointermove",
          E = "mouseup mouseleave touchend touchcancel pointerup pointercancel";
        t.element(document).on(y, a).on(b, d).on(E, m).on("$$mdGestureReset", function() {
          h = f = null;
        });
      }

      function s(e) {
        var t = d(e),
          n = {
            startTime: +Date.now(),
            target: e.target,
            type: e.type.charAt(0)
          };
        return n.startX = n.x = t.pageX, n.startY = n.y = t.pageY, n;
      }

      function c(e, t) {
        return e && t && e.type.charAt(0) === t.type;
      }

      function u(e) {
        return v && v.x == e.x && v.y == e.y;
      }

      function l(e, t) {
        var n = d(e),
          r = t.x = n.pageX,
          i = t.y = n.pageY;
        t.distanceX = r - t.startX, t.distanceY = i - t.startY, t.distance = Math.sqrt(t.distanceX * t
            .distanceX + t.distanceY * t.distanceY), t.directionX = t.distanceX > 0 ? "right" : t.distanceX <
          0 ? "left" : "", t.directionY = t.distanceY > 0 ? "down" : t.distanceY < 0 ? "up" : "", t
          .duration = +Date.now() - t.startTime, t.velocityX = t.distanceX / t.duration, t.velocityY = t
          .distanceY / t.duration;
      }

      function d(e) {
        return e = e.originalEvent || e, e.touches && e.touches[0] || e.changedTouches && e.changedTouches[
          0] || e;
      }
      r.$inject = ["$$MdGestureHandler", "$$rAF", "$timeout"], a.$inject = ["$mdGesture",
        "$$MdGestureHandler"];
      var f,
        h,
        p = {},
        m = !1,
        v = null,
        g = !1;
      t.module("material.core.gestures", []).provider("$mdGesture", n).factory("$$MdGestureHandler", o).run(
        a), n.prototype = {
          skipClickHijack: function() {
            return m = !0;
          },
          $get: ["$$MdGestureHandler", "$$rAF", "$timeout", function(e, t, n) {
            return new r(e, t, n);
          }]
        };
    }(),
    function() {
      function n(e, n) {
        this.$timeout = e, this.$mdUtil = n, this.bodyElement = t.element(document.body), this.isBuffering = !
          1, this.bufferTimeout = null, this.lastInteractionType = null, this.lastInteractionTime = null, this
          .inputEventMap = {
            keydown: "keyboard",
            mousedown: "mouse",
            mouseenter: "mouse",
            touchstart: "touch",
            pointerdown: "pointer",
            MSPointerDown: "pointer"
          }, this.iePointerMap = {
            2: "touch",
            3: "touch",
            4: "mouse"
          }, this.initializeEvents();
      }
      n.$inject = ["$timeout", "$mdUtil"], t.module("material.core.interaction", []).service("$mdInteraction",
        n), n.prototype.initializeEvents = function() {
        var t = "MSPointerEvent" in e ? "MSPointerDown" : "PointerEvent" in e ? "pointerdown" : null;
        this.bodyElement.on("keydown mousedown", this.onInputEvent.bind(this)), "ontouchstart" in document
          .documentElement && this.bodyElement.on("touchstart", this.onBufferInputEvent.bind(this)), t &&
          this.bodyElement.on(t, this.onInputEvent.bind(this));
      }, n.prototype.onInputEvent = function(e) {
        if (!this.isBuffering) {
          var t = this.inputEventMap[e.type];
          "pointer" === t && (t = this.iePointerMap[e.pointerType] || e.pointerType), this
            .lastInteractionType = t, this.lastInteractionTime = this.$mdUtil.now();
        }
      }, n.prototype.onBufferInputEvent = function(e) {
        this.$timeout.cancel(this.bufferTimeout), this.onInputEvent(e), this.isBuffering = !0, this
          .bufferTimeout = this.$timeout(function() {
            this.isBuffering = !1;
          }.bind(this), 650, !1);
      }, n.prototype.getLastInteractionType = function() {
        return this.lastInteractionType;
      }, n.prototype.isUserInvoked = function(e) {
        var n = t.isNumber(e) ? e : 15;
        return this.lastInteractionTime >= this.$mdUtil.now() - n;
      };
    }(),
    function() {
      ! function() {
        function e(e) {
          function s(e) {
            return e.replace(d, "").replace(f, function(e, t, n, r) {
              return r ? n.toUpperCase() : n;
            });
          }
          var d = /^((?:x|data)[\:\-_])/i,
            f = /([\:\-\_]+(.))/g,
            h = ["", "xs", "gt-xs", "sm", "gt-sm", "md", "gt-md", "lg", "gt-lg", "xl", "print"],
            p = ["layout", "flex", "flex-order", "flex-offset", "layout-align"],
            m = ["show", "hide", "layout-padding", "layout-margin"];
          t.forEach(h, function(n) {
              t.forEach(p, function(t) {
                var r = n ? t + "-" + n : t;
                e.directive(s(r), o(r));
              }), t.forEach(m, function(t) {
                var r = n ? t + "-" + n : t;
                e.directive(s(r), a(r));
              });
            }), e.provider("$$mdLayout", function() {
              return {
                $get: t.noop,
                validateAttributeValue: l,
                validateAttributeUsage: u,
                disableLayouts: function(e) {
                  C.enabled = e !== !0;
                }
              };
            }).directive("mdLayoutCss", r).directive("ngCloak", i("ng-cloak")).directive("layoutWrap", a(
              "layout-wrap")).directive("layoutNowrap", a("layout-nowrap")).directive("layoutNoWrap", a(
              "layout-no-wrap")).directive("layoutFill", a("layout-fill")).directive("layoutLtMd", c(
              "layout-lt-md", !0)).directive("layoutLtLg", c("layout-lt-lg", !0)).directive("flexLtMd", c(
              "flex-lt-md", !0)).directive("flexLtLg", c("flex-lt-lg", !0)).directive("layoutAlignLtMd", c(
              "layout-align-lt-md")).directive("layoutAlignLtLg", c("layout-align-lt-lg")).directive(
              "flexOrderLtMd", c("flex-order-lt-md")).directive("flexOrderLtLg", c("flex-order-lt-lg"))
            .directive("offsetLtMd", c("flex-offset-lt-md")).directive("offsetLtLg", c("flex-offset-lt-lg"))
            .directive("hideLtMd", c("hide-lt-md")).directive("hideLtLg", c("hide-lt-lg")).directive(
              "showLtMd", c("show-lt-md")).directive("showLtLg", c("show-lt-lg")).config(n);
        }

        function n() {
          var e = !!document.querySelector("[md-layouts-disabled]");
          C.enabled = !e;
        }

        function r() {
          return C.enabled = !1, {
            restrict: "A",
            priority: "900"
          };
        }

        function i(e) {
          return ["$timeout", function(n) {
            return {
              restrict: "A",
              priority: -10,
              compile: function(r) {
                return C.enabled ? (r.addClass(e), function(t, r) {
                  n(function() {
                    r.removeClass(e);
                  }, 10, !1);
                }) : t.noop;
              }
            };
          }];
        }

        function o(e) {
          function n(t, n, r) {
            var i = s(n, e, r),
              o = r.$observe(r.$normalize(e), i);
            i(h(e, r, "")), t.$on("$destroy", function() {
              o();
            });
          }
          return ["$mdUtil", "$interpolate", "$log", function(r, i, o) {
            return v = r, g = i, y = o, {
              restrict: "A",
              compile: function(r, i) {
                var o;
                return C.enabled && (u(e, i, r, y), l(e, h(e, i, ""), d(r, e, i)), o = n), o || t
                .noop;
              }
            };
          }];
        }

        function a(e) {
          function n(t, n) {
            n.addClass(e);
          }
          return ["$mdUtil", "$interpolate", "$log", function(r, i, o) {
            return v = r, g = i, y = o, {
              restrict: "A",
              compile: function(r, i) {
                var o;
                return C.enabled && (l(e, h(e, i, ""), d(r, e, i)), n(null, r), o = n), o || t.noop;
              }
            };
          }];
        }

        function s(e, n) {
          var r;
          return function(i) {
            var o = l(n, i || "");
            t.isDefined(o) && (r && e.removeClass(r), r = o ? n + "-" + o.replace(E, "-") : n, e.addClass(
              r));
          };
        }

        function c(e) {
          var n = e.split("-");
          return ["$log", function(r) {
            return r.warn(e + "has been deprecated. Please use a `" + n[0] + "-gt-<xxx>` variant."), t
              .noop;
          }];
        }

        function u(e, t, n, r) {
          var i,
            o,
            a,
            s = n[0].nodeName.toLowerCase();
          switch (e.replace(b, "")) {
            case "flex":
              "md-button" != s && "fieldset" != s || (o = "<" + s + " " + e + "></" + s + ">", a =
                "https://github.com/philipwalton/flexbugs#9-some-html-elements-cant-be-flex-containers", i =
                "Markup '{0}' may not work as expected in IE Browsers. Consult '{1}' for details.", r.warn(v
                  .supplant(i, [o, a])));
          }
        }

        function l(e, n, r) {
          var i = n;
          if (!f(n)) {
            switch (e.replace(b, "")) {
              case "layout":
                p(n, $) || (n = $[0]);
                break;
              case "flex":
                p(n, _) || isNaN(n) && (n = "");
                break;
              case "flex-offset":
              case "flex-order":
                n && !isNaN(+n) || (n = "0");
                break;
              case "layout-align":
                var o = m(n);
                n = v.supplant("{main}-{cross}", o);
                break;
              case "layout-padding":
              case "layout-margin":
              case "layout-fill":
              case "layout-wrap":
              case "layout-nowrap":
              case "layout-nowrap":
                n = "";
            }
            n != i && (r || t.noop)(n);
          }
          return n;
        }

        function d(e, t, n) {
          return function(e) {
            f(e) || (n[n.$normalize(t)] = e);
          };
        }

        function f(e) {
          return (e || "").indexOf(g.startSymbol()) > -1;
        }

        function h(e, t, n) {
          var r = t.$normalize(e);
          return t[r] ? t[r].replace(E, "-") : n || null;
        }

        function p(e, t, n) {
          e = n && e ? e.replace(E, n) : e;
          var r = !1;
          return e && t.forEach(function(t) {
            t = n ? t.replace(E, n) : t, r = r || t === e;
          }), r;
        }

        function m(e) {
          var t,
            n = {
              main: "start",
              cross: "stretch"
            };
          return e = e || "", 0 !== e.indexOf("-") && 0 !== e.indexOf(" ") || (e = "none" + e), t = e
            .toLowerCase().trim().replace(E, "-").split("-"), t.length && "space" === t[0] && (t = [t[0] +
              "-" + t[1], t[2]
            ]), t.length > 0 && (n.main = t[0] || n.main), t.length > 1 && (n.cross = t[1] || n.cross), w
            .indexOf(n.main) < 0 && (n.main = "start"), T.indexOf(n.cross) < 0 && (n.cross = "stretch"), n;
        }
        var v,
          g,
          y,
          b = /(-gt)?-(sm|md|lg|print)/g,
          E = /\s+/g,
          _ = ["grow", "initial", "auto", "none", "noshrink", "nogrow"],
          $ = ["row", "column"],
          w = ["", "start", "center", "end", "stretch", "space-around", "space-between"],
          T = ["", "start", "center", "end", "stretch"],
          C = {
            enabled: !0,
            breakpoints: []
          };
        e(t.module("material.core.layout", ["ng"]));
      }();
    }(),
    function() {
      function e() {
        function e(e) {
          function n(e) {
            return c.optionsFactory = e.options, c.methods = (e.methods || []).concat(a), u;
          }

          function r(e, t) {
            return s[e] = t, u;
          }

          function i(t, n) {
            if (n = n || {}, n.methods = n.methods || [], n.options = n.options || function() {
                return {};
              }, /^cancel|hide|show$/.test(t)) throw new Error("Preset '" + t + "' in " + e +
            " is reserved!");
            if (n.methods.indexOf("_options") > -1) throw new Error("Method '_options' in " + e +
              " is reserved!");
            return c.presets[t] = {
              methods: n.methods.concat(a),
              optionsFactory: n.options,
              argOption: n.argOption
            }, u;
          }

          function o(n, r) {
            function i(e) {
              return e = e || {}, e._options && (e = e._options), d.show(t.extend({}, l, e));
            }

            function o(e) {
              return d.destroy(e);
            }

            function a(t, n) {
              var i = {};
              return i[e] = f, r.invoke(t || function() {
                return n;
              }, {}, i);
            }
            var u,
              l,
              d = n(),
              f = {
                hide: d.hide,
                cancel: d.cancel,
                show: i,
                destroy: o
              };
            return u = c.methods || [], l = a(c.optionsFactory, {}), t.forEach(s, function(e, t) {
              f[t] = e;
            }), t.forEach(c.presets, function(e, n) {
              function r(e) {
                this._options = t.extend({}, i, e);
              }
              var i = a(e.optionsFactory, {}),
                o = (e.methods || []).concat(u);
              if (t.extend(i, {
                  $type: n
                }), t.forEach(o, function(e) {
                  r.prototype[e] = function(t) {
                    return this._options[e] = t, this;
                  };
                }), e.argOption) {
                var s = "show" + n.charAt(0).toUpperCase() + n.slice(1);
                f[s] = function(e) {
                  var t = f[n](e);
                  return f.show(t);
                };
              }
              f[n] = function(n) {
                return arguments.length && e.argOption && !t.isObject(n) && !t.isArray(n) ? new r()[e
                  .argOption](n) : new r(n);
              };
            }), f;
          }
          o.$inject = ["$$interimElement", "$injector"];
          var a = ["onHide", "onShow", "onRemove"],
            s = {},
            c = {
              presets: {}
            },
            u = {
              setDefaults: n,
              addPreset: i,
              addMethod: r,
              $get: o
            };
          return u.addPreset("build", {
            methods: ["controller", "controllerAs", "resolve", "multiple", "template", "templateUrl",
              "themable", "transformTemplate", "parent", "contentElement"
            ]
          }), u;
        }

        function r(e, r, i, o, a, s, c, u, l, d, f) {
          return function() {
            function h(e) {
              e = e || {};
              var t = new y(e || {}),
                n = e.multiple ? r.resolve() : r.all(_);
              e.multiple || (n = n.then(function() {
                var e = $.concat(w.map(b.cancel));
                return r.all(e);
              }));
              var i = n.then(function() {
                return t.show().catch(function(e) {
                  return e;
                }).finally(function() {
                  _.splice(_.indexOf(i), 1), w.push(t);
                });
              });
              return _.push(i), t.deferred.promise.catch(function(e) {
                return e instanceof Error && f(e), e;
              }), t.deferred.promise;
            }

            function p(e, t) {
              function i(n) {
                var r = n.remove(e, !1, t || {}).catch(function(e) {
                  return e;
                }).finally(function() {
                  $.splice($.indexOf(r), 1);
                });
                return w.splice(w.indexOf(n), 1), $.push(r), n.deferred.promise;
              }
              return t = t || {}, t.closeAll ? r.all(w.slice().reverse().map(i)) : t.closeTo !== n ? r.all(w
                .slice(t.closeTo).map(i)) : i(w[w.length - 1]);
            }

            function m(e, n) {
              var i = w.pop();
              if (!i) return r.when(e);
              var o = i.remove(e, !0, n || {}).catch(function(e) {
                return e;
              }).finally(function() {
                $.splice($.indexOf(o), 1);
              });
              return $.push(o), i.deferred.promise.catch(t.noop);
            }

            function v(e) {
              return function() {
                var t = arguments;
                return w.length ? e.apply(b, t) : _.length ? _[0].finally(function() {
                  return e.apply(b, t);
                }) : r.when("No interim elements currently showing up.");
              };
            }

            function g(e) {
              var n = e ? null : w.shift(),
                i = t.element(e).length && t.element(e)[0].parentNode;
              if (i) {
                var o = w.filter(function(e) {
                  return e.options.element[0] === i;
                });
                o.length && (n = o[0], w.splice(w.indexOf(n), 1));
              }
              return n ? n.remove(E, !1, {
                $destroy: !0
              }) : r.when(E);
            }

            function y(d) {
              function f() {
                return r(function(e, t) {
                  function n(e) {
                    $.deferred.reject(e), t(e);
                  }
                  d.onCompiling && d.onCompiling(d), m(d).then(function(t) {
                    w = v(t, d), d.cleanupElement = t.cleanup, T = E(w, d, t.controller).then(e, n);
                  }).catch(n);
                });
              }

              function h(e, n, i) {
                function o(e) {
                  $.deferred.resolve(e);
                }

                function a(e) {
                  $.deferred.reject(e);
                }
                return w ? (d = t.extend(d || {}, i || {}), d.cancelAutoHide && d.cancelAutoHide(), d
                  .element.triggerHandler("$mdInterimElementRemove"), d.$destroy === !0 ? _(d.element, d)
                  .then(function() {
                    n && a(e) || o(e);
                  }) : (r.when(T).finally(function() {
                    _(d.element, d).then(function() {
                      n ? a(e) : o(e);
                    }, a);
                  }), $.deferred.promise)) : r.when(!1);
              }

              function p(e) {
                return e = e || {}, e.template && (e.template = c.processTemplate(e.template)), t.extend({
                  preserveScope: !1,
                  cancelAutoHide: t.noop,
                  scope: e.scope || i.$new(e.isolateScope),
                  onShow: function(e, t, n) {
                    return s.enter(t, n.parent);
                  },
                  onRemove: function(e, t) {
                    return t && s.leave(t) || r.when();
                  }
                }, e);
              }

              function m(e) {
                var t = e.skipCompile ? null : u.compile(e);
                return t || r(function(t) {
                  t({
                    locals: {},
                    link: function() {
                      return e.element;
                    }
                  });
                });
              }

              function v(e, n) {
                t.extend(e.locals, n);
                var r = e.link(n.scope);
                return n.element = r, n.parent = g(r, n), n.themable && l(r), r;
              }

              function g(n, r) {
                var i = r.parent;
                if (i = t.isFunction(i) ? i(r.scope, n, r) : t.isString(i) ? t.element(e[0].querySelector(
                    i)) : t.element(i), !(i || {}).length) {
                  var o;
                  return a[0] && a[0].querySelector && (o = a[0].querySelector(":not(svg) > body")), o || (
                    o = a[0]), "#comment" == o.nodeName && (o = e[0].body), t.element(o);
                }
                return i;
              }

              function y() {
                var e,
                  r = t.noop;
                d.hideDelay && (e = o(b.hide, d.hideDelay), r = function() {
                  o.cancel(e);
                }), d.cancelAutoHide = function() {
                  r(), d.cancelAutoHide = n;
                };
              }

              function E(e, n, i) {
                var o = n.onShowing || t.noop,
                  a = n.onComplete || t.noop;
                try {
                  o(n.scope, e, n, i);
                } catch (e) {
                  return r.reject(e);
                }
                return r(function(t, o) {
                  try {
                    r.when(n.onShow(n.scope, e, n, i)).then(function() {
                      a(n.scope, e, n), y(), t(e);
                    }, o);
                  } catch (e) {
                    o(e.message);
                  }
                });
              }

              function _(e, n) {
                var i = n.onRemoving || t.noop;
                return r(function(t, o) {
                  try {
                    var a = r.when(n.onRemove(n.scope, e, n) || !0);
                    i(e, a), n.$destroy ? (t(e), !n.preserveScope && n.scope && a.then(function() {
                      n.scope.$destroy();
                    })) : a.then(function() {
                      !n.preserveScope && n.scope && n.scope.$destroy(), t(e);
                    }, o);
                  } catch (e) {
                    o(e.message);
                  }
                });
              }
              var $,
                w,
                T = r.when(!0);
              return d = p(d), $ = {
                options: d,
                deferred: r.defer(),
                show: f,
                remove: h
              };
            }
            var b,
              E = !1,
              _ = [],
              $ = [],
              w = [];
            return b = {
              show: h,
              hide: v(p),
              cancel: v(m),
              destroy: g,
              $injector_: d
            };
          };
        }
        return r.$inject = ["$document", "$q", "$rootScope", "$timeout", "$rootElement", "$animate",
          "$mdUtil", "$mdCompiler", "$mdTheming", "$injector", "$exceptionHandler"
        ], e.$get = r, e;
      }
      t.module("material.core").provider("$$interimElement", e);
    }(),
    function() {
      function e(e) {
        this._$timeout = e, this._liveElement = this._createLiveElement(), this._announceTimeout = 100;
      }
      e.$inject = ["$timeout"], t.module("material.core").service("$mdLiveAnnouncer", e), e.prototype
        .announce = function(e, t) {
          t || (t = "polite");
          var n = this;
          n._liveElement.textContent = "", n._liveElement.setAttribute("aria-live", t), n._$timeout(
        function() {
            n._liveElement.textContent = e;
          }, n._announceTimeout, !1);
        }, e.prototype._createLiveElement = function() {
          var e = document.createElement("div");
          return e.classList.add("md-visually-hidden"), e.setAttribute("role", "status"), e.setAttribute(
            "aria-atomic", "true"), e.setAttribute("aria-live", "polite"), document.body.appendChild(e), e;
        };
    }(),
    function() {
      t.module("material.core.meta", []).provider("$$mdMeta", function() {
        function e(e) {
          if (o[e]) return !0;
          var n = document.getElementsByName(e)[0];
          return !!n && (o[e] = t.element(n), !0);
        }

        function n(n, r) {
          if (e(n), o[n]) o[n].attr("content", r);
          else {
            var a = t.element('<meta name="' + n + '" content="' + r + '"/>');
            i.append(a), o[n] = a;
          }
          return function() {
            o[n].attr("content", ""), o[n].remove(), delete o[n];
          };
        }

        function r(t) {
          if (!e(t)) throw Error("$$mdMeta: could not find a meta tag with the name '" + t + "'");
          return o[t].attr("content");
        }
        var i = t.element(document.head),
          o = {},
          a = {
            setMeta: n,
            getMeta: r
          };
        return t.extend({}, a, {
          $get: function() {
            return a;
          }
        });
      });
    }(),
    function() {
      function e(e, r) {
        function i(e) {
          return e && "" !== e;
        }
        var o,
          a = [],
          s = {};
        return o = {
          notFoundError: function(t, n) {
            e.error((n || "") + "No instance found for handle", t);
          },
          getInstances: function() {
            return a;
          },
          get: function(e) {
            if (!i(e)) return null;
            var t, n, r;
            for (t = 0, n = a.length; t < n; t++)
              if (r = a[t], r.$$mdHandle === e) return r;
            return null;
          },
          register: function(e, n) {
            function r() {
              var t = a.indexOf(e);
              t !== -1 && a.splice(t, 1);
            }

            function i() {
              var t = s[n];
              t && (t.forEach(function(t) {
                t.resolve(e);
              }), delete s[n]);
            }
            return n ? (e.$$mdHandle = n, a.push(e), i(), r) : t.noop;
          },
          when: function(e) {
            if (i(e)) {
              var t = r.defer(),
                a = o.get(e);
              return a ? t.resolve(a) : (s[e] === n && (s[e] = []), s[e].push(t)), t.promise;
            }
            return r.reject("Invalid `md-component-id` value.");
          }
        };
      }
      e.$inject = ["$log", "$q"], t.module("material.core").factory("$mdComponentRegistry", e);
    }(),
    function() {
      ! function() {
        function e(e) {
          function n(e) {
            return e.hasClass("md-icon-button") ? {
              isMenuItem: e.hasClass("md-menu-item"),
              fitRipple: !0,
              center: !0
            } : {
              isMenuItem: e.hasClass("md-menu-item"),
              dimBackground: !0
            };
          }
          return {
            attach: function(r, i, o) {
              return o = t.extend(n(i), o), e.attach(r, i, o);
            }
          };
        }
        e.$inject = ["$mdInkRipple"], t.module("material.core").factory("$mdButtonInkRipple", e);
      }();
    }(),
    function() {
      ! function() {
        function e(e) {
          function n(n, r, i) {
            return e.attach(n, r, t.extend({
              center: !0,
              dimBackground: !1,
              fitRipple: !0
            }, i));
          }
          return {
            attach: n
          };
        }
        e.$inject = ["$mdInkRipple"], t.module("material.core").factory("$mdCheckboxInkRipple", e);
      }();
    }(),
    function() {
      ! function() {
        function e(e) {
          function n(n, r, i) {
            return e.attach(n, r, t.extend({
              center: !1,
              dimBackground: !0,
              outline: !1,
              rippleSize: "full"
            }, i));
          }
          return {
            attach: n
          };
        }
        e.$inject = ["$mdInkRipple"], t.module("material.core").factory("$mdListInkRipple", e);
      }();
    }(),
    function() {
      function e(e, n) {
        return {
          controller: t.noop,
          link: function(t, r, i) {
            i.hasOwnProperty("mdInkRippleCheckbox") ? n.attach(t, r) : e.attach(t, r);
          }
        };
      }

      function n() {
        function e() {
          n = !0;
        }
        var n = !1;
        return {
          disableInkRipple: e,
          $get: ["$injector", function(e) {
            function i(i, o, a) {
              return n || o.controller("mdNoInk") ? t.noop : e.instantiate(r, {
                $scope: i,
                $element: o,
                rippleOptions: a
              });
            }
            return {
              attach: i
            };
          }]
        };
      }

      function r(e, n, r, i, o, a, s) {
        this.$window = i, this.$timeout = o, this.$mdUtil = a, this.$mdColorUtil = s, this.$scope = e, this
          .$element = n, this.options = r, this.mousedown = !1, this.ripples = [], this.timeout = null, this
          .lastRipple = null, a.valueOnUse(this, "container", this.createContainer), this.$element.addClass(
            "md-ink-ripple"), (n.controller("mdInkRipple") || {}).createRipple = t.bind(this, this
            .createRipple), (n.controller("mdInkRipple") || {}).setColor = t.bind(this, this.color), this
          .bindEvents();
      }

      function i(e, n) {
        (e.mousedown || e.lastRipple) && (e.mousedown = !1, e.$mdUtil.nextTick(t.bind(e, n), !1));
      }

      function o() {
        return {
          controller: t.noop
        };
      }
      r.$inject = ["$scope", "$element", "rippleOptions", "$window", "$timeout", "$mdUtil", "$mdColorUtil"], e
        .$inject = ["$mdButtonInkRipple", "$mdCheckboxInkRipple"], t.module("material.core").provider(
          "$mdInkRipple", n).directive("mdInkRipple", e).directive("mdNoInk", o).directive("mdNoBar", o)
        .directive("mdNoStretch", o);
      var a = 450;
      r.prototype.color = function(e) {
        function n() {
          var e = r.options && r.options.colorElement ? r.options.colorElement : [],
            t = e.length ? e[0] : r.$element[0];
          return t ? r.$window.getComputedStyle(t).color : "rgb(0,0,0)";
        }
        var r = this;
        return t.isDefined(e) && (r._color = r._parseColor(e)), r._color || r._parseColor(r.inkRipple()) ||
          r._parseColor(n());
      }, r.prototype.calculateColor = function() {
        return this.color();
      }, r.prototype._parseColor = function(e, t) {
        t = t || 1;
        var n = this.$mdColorUtil;
        if (e) return 0 === e.indexOf("rgba") ? e.replace(/\d?\.?\d*\s*\)\s*$/, (.1 * t).toString() + ")") :
          0 === e.indexOf("rgb") ? n.rgbToRgba(e) : 0 === e.indexOf("#") ? n.hexToRgba(e) : void 0;
      }, r.prototype.bindEvents = function() {
        this.$element.on("mousedown", t.bind(this, this.handleMousedown)), this.$element.on(
          "mouseup touchend", t.bind(this, this.handleMouseup)), this.$element.on("mouseleave", t.bind(
          this, this.handleMouseup)), this.$element.on("touchmove", t.bind(this, this.handleTouchmove));
      }, r.prototype.handleMousedown = function(e) {
        if (!this.mousedown)
          if (e.hasOwnProperty("originalEvent") && (e = e.originalEvent), this.mousedown = !0, this.options
            .center) this.createRipple(this.container.prop("clientWidth") / 2, this.container.prop(
            "clientWidth") / 2);
          else if (e.srcElement !== this.$element[0]) {
          var t = this.$element[0].getBoundingClientRect(),
            n = e.clientX - t.left,
            r = e.clientY - t.top;
          this.createRipple(n, r);
        } else this.createRipple(e.offsetX, e.offsetY);
      }, r.prototype.handleMouseup = function() {
        i(this, this.clearRipples);
      }, r.prototype.handleTouchmove = function() {
        i(this, this.deleteRipples);
      }, r.prototype.deleteRipples = function() {
        for (var e = 0; e < this.ripples.length; e++) this.ripples[e].remove();
      }, r.prototype.clearRipples = function() {
        for (var e = 0; e < this.ripples.length; e++) this.fadeInComplete(this.ripples[e]);
      }, r.prototype.createContainer = function() {
        var e = t.element('<div class="md-ripple-container"></div>');
        return this.$element.append(e), e;
      }, r.prototype.clearTimeout = function() {
        this.timeout && (this.$timeout.cancel(this.timeout), this.timeout = null);
      }, r.prototype.isRippleAllowed = function() {
        var e = this.$element[0];
        do {
          if (!e.tagName || "BODY" === e.tagName) break;
          if (e && t.isFunction(e.hasAttribute)) {
            if (e.hasAttribute("disabled")) return !1;
            if ("false" === this.inkRipple() || "0" === this.inkRipple()) return !1;
          }
        } while (e = e.parentNode);
        return !0;
      }, r.prototype.inkRipple = function() {
        return this.$element.attr("md-ink-ripple");
      }, r.prototype.createRipple = function(e, n) {
        function r(e, t, n) {
          return e ? Math.max(t, n) : Math.sqrt(Math.pow(t, 2) + Math.pow(n, 2));
        }
        if (this.isRippleAllowed()) {
          var i = this,
            o = i.$mdColorUtil,
            s = t.element('<div class="md-ripple"></div>'),
            c = this.$element.prop("clientWidth"),
            u = this.$element.prop("clientHeight"),
            l = 2 * Math.max(Math.abs(c - e), e),
            d = 2 * Math.max(Math.abs(u - n), n),
            f = r(this.options.fitRipple, l, d),
            h = this.calculateColor();
          s.css({
              left: e + "px",
              top: n + "px",
              background: "black",
              width: f + "px",
              height: f + "px",
              backgroundColor: o.rgbaToRgb(h),
              borderColor: o.rgbaToRgb(h)
            }), this.lastRipple = s, this.clearTimeout(), this.timeout = this.$timeout(function() {
              i.clearTimeout(), i.mousedown || i.fadeInComplete(s);
            }, .35 * a, !1), this.options.dimBackground && this.container.css({
              backgroundColor: h
            }), this.container.append(s), this.ripples.push(s), s.addClass("md-ripple-placed"), this.$mdUtil
            .nextTick(function() {
              s.addClass("md-ripple-scaled md-ripple-active"), i.$timeout(function() {
                i.clearRipples();
              }, a, !1);
            }, !1);
        }
      }, r.prototype.fadeInComplete = function(e) {
        this.lastRipple === e ? this.timeout || this.mousedown || this.removeRipple(e) : this.removeRipple(
          e);
      }, r.prototype.removeRipple = function(e) {
        var t = this,
          n = this.ripples.indexOf(e);
        n < 0 || (this.ripples.splice(this.ripples.indexOf(e), 1), e.removeClass("md-ripple-active"), e
          .addClass("md-ripple-remove"), 0 === this.ripples.length && this.container.css({
            backgroundColor: ""
          }), this.$timeout(function() {
            t.fadeOutComplete(e);
          }, a, !1));
      }, r.prototype.fadeOutComplete = function(e) {
        e.remove(), this.lastRipple = null;
      };
    }(),
    function() {
      ! function() {
        function e(e) {
          function n(n, r, i) {
            return e.attach(n, r, t.extend({
              center: !1,
              dimBackground: !0,
              outline: !1,
              rippleSize: "full"
            }, i));
          }
          return {
            attach: n
          };
        }
        e.$inject = ["$mdInkRipple"], t.module("material.core").factory("$mdTabInkRipple", e);
      }();
    }(),
    function() {
      t.module("material.core.theming.palette", []).constant("$mdColorPalette", {
        red: {
          50: "#ffebee",
          100: "#ffcdd2",
          200: "#ef9a9a",
          300: "#e57373",
          400: "#ef5350",
          500: "#f44336",
          600: "#e53935",
          700: "#d32f2f",
          800: "#c62828",
          900: "#b71c1c",
          A100: "#ff8a80",
          A200: "#ff5252",
          A400: "#ff1744",
          A700: "#d50000",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 300 A100",
          contrastStrongLightColors: "400 500 600 700 A200 A400 A700"
        },
        pink: {
          50: "#fce4ec",
          100: "#f8bbd0",
          200: "#f48fb1",
          300: "#f06292",
          400: "#ec407a",
          500: "#e91e63",
          600: "#d81b60",
          700: "#c2185b",
          800: "#ad1457",
          900: "#880e4f",
          A100: "#ff80ab",
          A200: "#ff4081",
          A400: "#f50057",
          A700: "#c51162",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 A100",
          contrastStrongLightColors: "500 600 A200 A400 A700"
        },
        purple: {
          50: "#f3e5f5",
          100: "#e1bee7",
          200: "#ce93d8",
          300: "#ba68c8",
          400: "#ab47bc",
          500: "#9c27b0",
          600: "#8e24aa",
          700: "#7b1fa2",
          800: "#6a1b9a",
          900: "#4a148c",
          A100: "#ea80fc",
          A200: "#e040fb",
          A400: "#d500f9",
          A700: "#aa00ff",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 A100",
          contrastStrongLightColors: "300 400 A200 A400 A700"
        },
        "deep-purple": {
          50: "#ede7f6",
          100: "#d1c4e9",
          200: "#b39ddb",
          300: "#9575cd",
          400: "#7e57c2",
          500: "#673ab7",
          600: "#5e35b1",
          700: "#512da8",
          800: "#4527a0",
          900: "#311b92",
          A100: "#b388ff",
          A200: "#7c4dff",
          A400: "#651fff",
          A700: "#6200ea",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 A100",
          contrastStrongLightColors: "300 400 A200"
        },
        indigo: {
          50: "#e8eaf6",
          100: "#c5cae9",
          200: "#9fa8da",
          300: "#7986cb",
          400: "#5c6bc0",
          500: "#3f51b5",
          600: "#3949ab",
          700: "#303f9f",
          800: "#283593",
          900: "#1a237e",
          A100: "#8c9eff",
          A200: "#536dfe",
          A400: "#3d5afe",
          A700: "#304ffe",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 A100",
          contrastStrongLightColors: "300 400 A200 A400"
        },
        blue: {
          50: "#e3f2fd",
          100: "#bbdefb",
          200: "#90caf9",
          300: "#64b5f6",
          400: "#42a5f5",
          500: "#2196f3",
          600: "#1e88e5",
          700: "#1976d2",
          800: "#1565c0",
          900: "#0d47a1",
          A100: "#82b1ff",
          A200: "#448aff",
          A400: "#2979ff",
          A700: "#2962ff",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 300 400 A100",
          contrastStrongLightColors: "500 600 700 A200 A400 A700"
        },
        "light-blue": {
          50: "#e1f5fe",
          100: "#b3e5fc",
          200: "#81d4fa",
          300: "#4fc3f7",
          400: "#29b6f6",
          500: "#03a9f4",
          600: "#039be5",
          700: "#0288d1",
          800: "#0277bd",
          900: "#01579b",
          A100: "#80d8ff",
          A200: "#40c4ff",
          A400: "#00b0ff",
          A700: "#0091ea",
          contrastDefaultColor: "dark",
          contrastLightColors: "600 700 800 900 A700",
          contrastStrongLightColors: "600 700 800 A700"
        },
        cyan: {
          50: "#e0f7fa",
          100: "#b2ebf2",
          200: "#80deea",
          300: "#4dd0e1",
          400: "#26c6da",
          500: "#00bcd4",
          600: "#00acc1",
          700: "#0097a7",
          800: "#00838f",
          900: "#006064",
          A100: "#84ffff",
          A200: "#18ffff",
          A400: "#00e5ff",
          A700: "#00b8d4",
          contrastDefaultColor: "dark",
          contrastLightColors: "700 800 900",
          contrastStrongLightColors: "700 800 900"
        },
        teal: {
          50: "#e0f2f1",
          100: "#b2dfdb",
          200: "#80cbc4",
          300: "#4db6ac",
          400: "#26a69a",
          500: "#009688",
          600: "#00897b",
          700: "#00796b",
          800: "#00695c",
          900: "#004d40",
          A100: "#a7ffeb",
          A200: "#64ffda",
          A400: "#1de9b6",
          A700: "#00bfa5",
          contrastDefaultColor: "dark",
          contrastLightColors: "500 600 700 800 900",
          contrastStrongLightColors: "500 600 700"
        },
        green: {
          50: "#e8f5e9",
          100: "#c8e6c9",
          200: "#a5d6a7",
          300: "#81c784",
          400: "#66bb6a",
          500: "#4caf50",
          600: "#43a047",
          700: "#388e3c",
          800: "#2e7d32",
          900: "#1b5e20",
          A100: "#b9f6ca",
          A200: "#69f0ae",
          A400: "#00e676",
          A700: "#00c853",
          contrastDefaultColor: "dark",
          contrastLightColors: "500 600 700 800 900",
          contrastStrongLightColors: "500 600 700"
        },
        "light-green": {
          50: "#f1f8e9",
          100: "#dcedc8",
          200: "#c5e1a5",
          300: "#aed581",
          400: "#9ccc65",
          500: "#8bc34a",
          600: "#7cb342",
          700: "#689f38",
          800: "#558b2f",
          900: "#33691e",
          A100: "#ccff90",
          A200: "#b2ff59",
          A400: "#76ff03",
          A700: "#64dd17",
          contrastDefaultColor: "dark",
          contrastLightColors: "700 800 900",
          contrastStrongLightColors: "700 800 900"
        },
        lime: {
          50: "#f9fbe7",
          100: "#f0f4c3",
          200: "#e6ee9c",
          300: "#dce775",
          400: "#d4e157",
          500: "#cddc39",
          600: "#c0ca33",
          700: "#afb42b",
          800: "#9e9d24",
          900: "#827717",
          A100: "#f4ff81",
          A200: "#eeff41",
          A400: "#c6ff00",
          A700: "#aeea00",
          contrastDefaultColor: "dark",
          contrastLightColors: "900",
          contrastStrongLightColors: "900"
        },
        yellow: {
          50: "#fffde7",
          100: "#fff9c4",
          200: "#fff59d",
          300: "#fff176",
          400: "#ffee58",
          500: "#ffeb3b",
          600: "#fdd835",
          700: "#fbc02d",
          800: "#f9a825",
          900: "#f57f17",
          A100: "#ffff8d",
          A200: "#ffff00",
          A400: "#ffea00",
          A700: "#ffd600",
          contrastDefaultColor: "dark"
        },
        amber: {
          50: "#fff8e1",
          100: "#ffecb3",
          200: "#ffe082",
          300: "#ffd54f",
          400: "#ffca28",
          500: "#ffc107",
          600: "#ffb300",
          700: "#ffa000",
          800: "#ff8f00",
          900: "#ff6f00",
          A100: "#ffe57f",
          A200: "#ffd740",
          A400: "#ffc400",
          A700: "#ffab00",
          contrastDefaultColor: "dark"
        },
        orange: {
          50: "#fff3e0",
          100: "#ffe0b2",
          200: "#ffcc80",
          300: "#ffb74d",
          400: "#ffa726",
          500: "#ff9800",
          600: "#fb8c00",
          700: "#f57c00",
          800: "#ef6c00",
          900: "#e65100",
          A100: "#ffd180",
          A200: "#ffab40",
          A400: "#ff9100",
          A700: "#ff6d00",
          contrastDefaultColor: "dark",
          contrastLightColors: "800 900",
          contrastStrongLightColors: "800 900"
        },
        "deep-orange": {
          50: "#fbe9e7",
          100: "#ffccbc",
          200: "#ffab91",
          300: "#ff8a65",
          400: "#ff7043",
          500: "#ff5722",
          600: "#f4511e",
          700: "#e64a19",
          800: "#d84315",
          900: "#bf360c",
          A100: "#ff9e80",
          A200: "#ff6e40",
          A400: "#ff3d00",
          A700: "#dd2c00",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 300 400 A100 A200",
          contrastStrongLightColors: "500 600 700 800 900 A400 A700"
        },
        brown: {
          50: "#efebe9",
          100: "#d7ccc8",
          200: "#bcaaa4",
          300: "#a1887f",
          400: "#8d6e63",
          500: "#795548",
          600: "#6d4c41",
          700: "#5d4037",
          800: "#4e342e",
          900: "#3e2723",
          A100: "#d7ccc8",
          A200: "#bcaaa4",
          A400: "#8d6e63",
          A700: "#5d4037",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 A100 A200",
          contrastStrongLightColors: "300 400"
        },
        grey: {
          50: "#fafafa",
          100: "#f5f5f5",
          200: "#eeeeee",
          300: "#e0e0e0",
          400: "#bdbdbd",
          500: "#9e9e9e",
          600: "#757575",
          700: "#616161",
          800: "#424242",
          900: "#212121",
          A100: "#ffffff",
          A200: "#000000",
          A400: "#303030",
          A700: "#616161",
          contrastDefaultColor: "dark",
          contrastLightColors: "600 700 800 900 A200 A400 A700"
        },
        "blue-grey": {
          50: "#eceff1",
          100: "#cfd8dc",
          200: "#b0bec5",
          300: "#90a4ae",
          400: "#78909c",
          500: "#607d8b",
          600: "#546e7a",
          700: "#455a64",
          800: "#37474f",
          900: "#263238",
          A100: "#cfd8dc",
          A200: "#b0bec5",
          A400: "#78909c",
          A700: "#455a64",
          contrastDefaultColor: "light",
          contrastDarkColors: "50 100 200 300 A100 A200",
          contrastStrongLightColors: "400 500 700"
        }
      });
    }(),
    function() {
      ! function(e) {
        function t(e) {
          var t = !!document.querySelector("[md-themes-disabled]");
          e.disableTheming(t);
        }

        function r(t, r) {
          function i(e, t) {
            return t = t || {}, h[e] = a(e, t), p;
          }

          function o(t, n) {
            return a(t, e.extend({}, h[t] || {}, n));
          }

          function a(e, t) {
            var n = x.filter(function(e) {
              return !t[e];
            });
            if (n.length) throw new Error("Missing colors %1 in palette %2!".replace("%1", n.join(", "))
              .replace("%2", e));
            return t;
          }

          function c(t, n) {
            if (b[t]) return b[t];
            n = n || "default";
            var r = "string" == typeof n ? b[n] : n,
              i = new l(t);
            return r && e.forEach(r.colors, function(t, n) {
              i.colors[n] = {
                name: t.name,
                hues: e.extend({}, t.hues)
              };
            }), b[t] = i, i;
          }

          function l(t) {
            function n(t) {
              if (t = 0 === arguments.length || !!t, t !== r.isDark) {
                r.isDark = t, r.foregroundPalette = r.isDark ? v : m, r.foregroundShadow = r.isDark ? g : y;
                var n = r.isDark ? C : T,
                  i = r.isDark ? T : C;
                return e.forEach(n, function(e, t) {
                  var n = r.colors[t],
                    o = i[t];
                  if (n)
                    for (var a in n.hues) n.hues[a] === o[a] && (n.hues[a] = e[a]);
                }), r;
              }
            }
            var r = this;
            r.name = t, r.colors = {}, r.dark = n, n(!1), $.forEach(function(t) {
              var n = (r.isDark ? C : T)[t];
              r[t + "Palette"] = function(i, o) {
                var a = r.colors[t] = {
                  name: i,
                  hues: e.extend({}, n, o)
                };
                return Object.keys(a.hues).forEach(function(e) {
                  if (!n[e]) throw new Error(
                    "Invalid hue name '%1' in theme %2's %3 color %4. Available hue names: %4"
                    .replace("%1", e).replace("%2", r.name).replace("%3", i).replace("%4",
                      Object.keys(n).join(", ")));
                }), Object.keys(a.hues).map(function(e) {
                  return a.hues[e];
                }).forEach(function(e) {
                  if (x.indexOf(e) == -1) throw new Error(
                    "Invalid hue value '%1' in theme %2's %3 color %4. Available hue values: %5"
                    .replace("%1", e).replace("%2", r.name).replace("%3", t).replace("%4", i)
                    .replace("%5", x.join(", ")));
                }), r;
              }, r[t + "Color"] = function() {
                var e = Array.prototype.slice.call(arguments);
                return console.warn("$mdThemingProviderTheme." + t +
                  "Color() has been deprecated. Use $mdThemingProviderTheme." + t +
                  "Palette() instead."), r[t + "Palette"].apply(r, e);
              };
            });
          }

          function d(t, r, i, o) {
            function a(e) {
              return e === n || "" === e || l.THEMES[e] !== n;
            }

            function s(e, t) {
              function n() {
                return s && s.$mdTheme || ("default" == _ ? "" : _);
              }

              function i(t) {
                if (t) {
                  a(t) || o.warn("Attempted to use unregistered theme '" + t +
                    "'. Register it with $mdThemingProvider.theme().");
                  var n = e.data("$mdThemeName");
                  n && e.removeClass("md-" + n + "-theme"), e.addClass("md-" + t + "-theme"), e.data(
                    "$mdThemeName", t), s && e.data("$mdThemeController", s);
                }
              }
              var s = t.controller("mdTheme") || e.data("$mdThemeController");
              if (i(n()), s) var c = E || s.$shouldWatch || r.parseAttributeBoolean(e.attr("md-theme-watch")),
                u = s.registerChanges(function(t) {
                  i(t), c ? e.on("$destroy", u) : u();
                });
            }
            var l = function(e, r) {
              r === n && (r = e, e = n), e === n && (e = t), l.inherit(r, r);
            };
            return Object.defineProperty(l, "THEMES", {
              get: function() {
                return e.extend({}, b);
              }
            }), Object.defineProperty(l, "PALETTES", {
              get: function() {
                return e.extend({}, h);
              }
            }), Object.defineProperty(l, "ALWAYS_WATCH", {
              get: function() {
                return E;
              }
            }), l.inherit = s, l.registered = a, l.defaultTheme = function() {
              return _;
            }, l.generateTheme = function(e) {
              u(b[e], e, S.nonce);
            }, l.defineTheme = function(e, t) {
              t = t || {};
              var n = c(e);
              return t.primary && n.primaryPalette(t.primary), t.accent && n.accentPalette(t.accent), t
                .warn && n.warnPalette(t.warn), t.background && n.backgroundPalette(t.background), t.dark &&
                n.dark(), this.generateTheme(e), i.resolve(e);
            }, l.setBrowserColor = A, l;
          }
          d.$inject = ["$rootScope", "$mdUtil", "$q", "$log"], h = {};
          var p,
            b = {},
            E = !1,
            _ = "default";
          e.extend(h, t);
          var w = function(e) {
              var t = r.setMeta("theme-color", e),
                n = r.setMeta("msapplication-navbutton-color", e);
              return function() {
                t(), n();
              };
            },
            A = function(t) {
              t = e.isObject(t) ? t : {};
              var n = t.theme || "default",
                r = t.hue || "800",
                i = h[t.palette] || h[b[n].colors[t.palette || "primary"].name],
                o = e.isObject(i[r]) ? i[r].hex : i[r];
              return w(o);
            };
          return p = {
            definePalette: i,
            extendPalette: o,
            theme: c,
            configuration: function() {
              return e.extend({}, S, {
                defaultTheme: _,
                alwaysWatchTheme: E,
                registeredStyles: [].concat(S.registeredStyles)
              });
            },
            disableTheming: function(t) {
              S.disableTheming = e.isUndefined(t) || !!t;
            },
            registerStyles: function(e) {
              S.registeredStyles.push(e);
            },
            setNonce: function(e) {
              S.nonce = e;
            },
            generateThemesOnDemand: function(e) {
              S.generateOnDemand = e;
            },
            setDefaultTheme: function(e) {
              _ = e;
            },
            alwaysWatchTheme: function(e) {
              E = e;
            },
            enableBrowserColor: A,
            $get: d,
            _LIGHT_DEFAULT_HUES: T,
            _DARK_DEFAULT_HUES: C,
            _PALETTES: h,
            _THEMES: b,
            _parseRules: s,
            _rgba: f
          };
        }

        function i(t, n, r, i, o, a) {
          return {
            priority: 101,
            link: {
              pre: function(s, c, u) {
                var l = [],
                  d = n.startSymbol(),
                  f = n.endSymbol(),
                  h = u.mdTheme.trim(),
                  p = h.substr(0, d.length) === d && h.lastIndexOf(f) === h.length - f.length,
                  m = "::",
                  v = u.mdTheme.split(d).join("").split(f).join("").trim().substr(0, m.length) === m,
                  g = {
                    registerChanges: function(t, n) {
                      return n && (t = e.bind(n, t)), l.push(t),
                        function() {
                          var e = l.indexOf(t);
                          e > -1 && l.splice(e, 1);
                        };
                    },
                    $setTheme: function(e) {
                      t.registered(e) || a.warn("attempted to use unregistered theme '" + e + "'"), g
                        .$mdTheme = e;
                      for (var n = l.length; n--;) l[n](e);
                    },
                    $shouldWatch: i.parseAttributeBoolean(c.attr("md-theme-watch")) || t.ALWAYS_WATCH ||
                      p && !v
                  };
                c.data("$mdThemeController", g);
                var y = function() {
                    var e = n(u.mdTheme)(s);
                    return r(e)(s) || e;
                  },
                  b = function(t) {
                    return "string" == typeof t ? g.$setTheme(t) : void o.when(e.isFunction(t) ? t() : t)
                      .then(function(e) {
                        g.$setTheme(e);
                      });
                  };
                b(y());
                var E = s.$watch(y, function(e) {
                  e && (b(e), g.$shouldWatch || E());
                });
              }
            }
          };
        }

        function o() {
          return S.disableTheming = !0, {
            restrict: "A",
            priority: "900"
          };
        }

        function a(e) {
          return e;
        }

        function s(t, n, r) {
          l(t, n), r = r.replace(/THEME_NAME/g, t.name);
          var i = [],
            o = t.colors[n],
            a = new RegExp("\\.md-" + t.name + "-theme", "g"),
            s = new RegExp("('|\")?{{\\s*(" + n + ")-(color|contrast)-?(\\d\\.?\\d*)?\\s*}}(\"|')?", "g"),
            c =
            /'?"?\{\{\s*([a-zA-Z]+)-(A?\d+|hue\-[0-3]|shadow|default)-?(\d\.?\d*)?(contrast)?\s*\}\}'?"?/g,
            u = h[o.name];
          return r = r.replace(c, function(e, n, r, i, o) {
            return "foreground" === n ? "shadow" == r ? t.foregroundShadow : t.foregroundPalette[r] || t
              .foregroundPalette[1] : (0 !== r.indexOf("hue") && "default" !== r || (r = t.colors[n].hues[
                r]), f((h[t.colors[n].name][r] || "")[o ? "contrast" : "value"], i));
          }), e.forEach(o.hues, function(e, n) {
            var o = r.replace(s, function(t, n, r, i, o) {
              return f(u[e]["color" === i ? "value" : "contrast"], o);
            });
            if ("default" !== n && (o = o.replace(a, ".md-" + t.name + "-theme.md-" + n)), "default" == t
              .name) {
              var c =
                /((?:\s|>|\.|\w|-|:|\(|\)|\[|\]|"|'|=)*)\.md-default-theme((?:\s|>|\.|\w|-|:|\(|\)|\[|\]|"|'|=)*)/g;
              o = o.replace(c, function(e, t, n) {
                return e + ", " + t + n;
              });
            }
            i.push(o);
          }), i;
        }

        function c(t, n) {
          function r(t, n) {
            var r = t.contrastDefaultColor,
              i = t.contrastLightColors || [],
              o = t.contrastStrongLightColors || [],
              a = t.contrastDarkColors || [];
            "string" == typeof i && (i = i.split(" ")), "string" == typeof o && (o = o.split(" ")),
              "string" == typeof a && (a = a.split(" ")), delete t.contrastDefaultColor, delete t
              .contrastLightColors, delete t.contrastStrongLightColors, delete t.contrastDarkColors, e
              .forEach(t, function(n, s) {
                function c() {
                  return "light" === r ? a.indexOf(s) > -1 ? b : o.indexOf(s) > -1 ? _ : E : i.indexOf(s) >
                    -1 ? o.indexOf(s) > -1 ? _ : E : b;
                }
                if (!e.isObject(n)) {
                  var u = d(n);
                  if (!u) throw new Error(
                    "Color %1, in palette %2's hue %3, is invalid. Hex or rgb(a) color expected."
                    .replace("%1", n).replace("%2", t.name).replace("%3", s));
                  t[s] = {
                    hex: t[s],
                    value: u,
                    contrast: c()
                  };
                }
              });
          }
          var i = document.head,
            o = i ? i.firstElementChild : null,
            a = !S.disableTheming && t.has("$MD_THEME_CSS") ? t.get("$MD_THEME_CSS") : "";
          if (a += S.registeredStyles.join(""), o && 0 !== a.length) {
            e.forEach(h, r);
            var s = a.split(/\}(?!(\}|'|"|;))/).filter(function(e) {
                return e && e.trim().length;
              }).map(function(e) {
                return e.trim() + "}";
              }),
              c = new RegExp("md-(" + $.join("|") + ")", "g");
            $.forEach(function(e) {
              A[e] = "";
            }), s.forEach(function(e) {
              for (var t, n = (e.match(c), 0); t = $[n]; n++)
                if (e.indexOf(".md-" + t) > -1) return A[t] += e;
              for (n = 0; t = $[n]; n++)
                if (e.indexOf(t) > -1) return A[t] += e;
              return A[w] += e;
            }), S.generateOnDemand || e.forEach(n.THEMES, function(e) {
              p[e.name] || "default" !== n.defaultTheme() && "default" === e.name || u(e, e.name, S
              .nonce);
            });
          }
        }

        function u(e, t, n) {
          var r = document.head,
            i = r ? r.firstElementChild : null;
          p[t] || ($.forEach(function(t) {
            for (var o = s(e, t, A[t]); o.length;) {
              var a = o.shift();
              if (a) {
                var c = document.createElement("style");
                c.setAttribute("md-theme-style", ""), n && c.setAttribute("nonce", n), c.appendChild(
                  document.createTextNode(a)), r.insertBefore(c, i);
              }
            }
          }), p[e.name] = !0);
        }

        function l(e, t) {
          if (!h[(e.colors[t] || {}).name]) throw new Error(
            "You supplied an invalid color palette for theme %1's %2 palette. Available palettes: %3"
            .replace("%1", e.name).replace("%2", t).replace("%3", Object.keys(h).join(", ")));
        }

        function d(t) {
          if (e.isArray(t) && 3 == t.length) return t;
          if (/^rgb/.test(t)) return t.replace(/(^\s*rgba?\(|\)\s*$)/g, "").split(",").map(function(e, t) {
            return 3 == t ? parseFloat(e, 10) : parseInt(e, 10);
          });
          if ("#" == t.charAt(0) && (t = t.substring(1)), /^([a-fA-F0-9]{3}){1,2}$/g.test(t)) {
            var n = t.length / 3,
              r = t.substr(0, n),
              i = t.substr(n, n),
              o = t.substr(2 * n);
            return 1 === n && (r += r, i += i, o += o), [parseInt(r, 16), parseInt(i, 16), parseInt(o, 16)];
          }
        }

        function f(t, n) {
          return t ? (4 == t.length && (t = e.copy(t), n ? t.pop() : n = t.pop()), n && ("number" ==
              typeof n || "string" == typeof n && n.length) ? "rgba(" + t.join(",") + "," + n + ")" :
            "rgb(" + t.join(",") + ")") : "rgb('0,0,0')";
        }
        t.$inject = ["$mdThemingProvider"], i.$inject = ["$mdTheming", "$interpolate", "$parse", "$mdUtil",
            "$q", "$log"
          ], a.$inject = ["$mdTheming"], r.$inject = ["$mdColorPalette", "$$mdMetaProvider"], c.$inject = [
            "$injector", "$mdTheming"
          ], e.module("material.core.theming", ["material.core.theming.palette", "material.core.meta"])
          .directive("mdTheme", i).directive("mdThemable", a).directive("mdThemesDisabled", o).provider(
            "$mdTheming", r).config(t).run(c);
        var h,
          p = {},
          m = {
            name: "dark",
            1: "rgba(0,0,0,0.87)",
            2: "rgba(0,0,0,0.54)",
            3: "rgba(0,0,0,0.38)",
            4: "rgba(0,0,0,0.12)"
          },
          v = {
            name: "light",
            1: "rgba(255,255,255,1.0)",
            2: "rgba(255,255,255,0.7)",
            3: "rgba(255,255,255,0.5)",
            4: "rgba(255,255,255,0.12)"
          },
          g = "1px 1px 0px rgba(0,0,0,0.4), -1px -1px 0px rgba(0,0,0,0.4)",
          y = "",
          b = d("rgba(0,0,0,0.87)"),
          E = d("rgba(255,255,255,0.87)"),
          _ = d("rgb(255,255,255)"),
          $ = ["primary", "accent", "warn", "background"],
          w = "primary",
          T = {
            accent: {
              default: "A200",
              "hue-1": "A100",
              "hue-2": "A400",
              "hue-3": "A700"
            },
            background: {
              default: "50",
              "hue-1": "A100",
              "hue-2": "100",
              "hue-3": "300"
            }
          },
          C = {
            background: {
              default: "A400",
              "hue-1": "800",
              "hue-2": "900",
              "hue-3": "A200"
            }
          };
        $.forEach(function(e) {
          var t = {
            default: "500",
            "hue-1": "300",
            "hue-2": "800",
            "hue-3": "A100"
          };
          T[e] || (T[e] = t), C[e] || (C[e] = t);
        });
        var x = ["50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "A100", "A200", "A400",
            "A700"
          ],
          S = {
            disableTheming: !1,
            generateOnDemand: !1,
            registeredStyles: [],
            nonce: null
          },
          A = {};
      }(e.angular);
    }(),
    function() {
      function n(n, r, i, o, a) {
        var s;
        return s = {
          translate3d: function(e, t, n, r) {
            function i(n) {
              return a(e, {
                to: n || t,
                addClass: r.transitionOutClass,
                removeClass: r.transitionInClass,
                duration: r.duration
              }).start();
            }
            return a(e, {
              from: t,
              to: n,
              addClass: r.transitionInClass,
              removeClass: r.transitionOutClass,
              duration: r.duration
            }).start().then(function() {
              return i;
            });
          },
          waitTransitionEnd: function(t, n) {
            var a = 3e3;
            return r(function(r, s) {
              function c(e) {
                e && e.target !== t[0] || (e && i.cancel(l), t.off(o.CSS.TRANSITIONEND, c), r());
              }

              function u(n) {
                return n = n || e.getComputedStyle(t[0]), "0s" == n.transitionDuration || !n
                  .transition && !n.transitionProperty;
              }
              n = n || {}, u(n.cachedTransitionStyles) && (a = 0);
              var l = i(c, n.timeout || a);
              t.on(o.CSS.TRANSITIONEND, c);
            });
          },
          calculateTransformValues: function(e, t) {
            function n() {
              var t = e ? e.parent() : null,
                n = t ? t.parent() : null;
              return n ? s.clientRect(n) : null;
            }
            var r = t.element,
              i = t.bounds;
            if (r || i) {
              var o = r ? s.clientRect(r) || n() : s.copyRect(i),
                a = s.copyRect(e[0].getBoundingClientRect()),
                c = s.centerPointFor(a),
                u = s.centerPointFor(o);
              return {
                centerX: u.x - c.x,
                centerY: u.y - c.y,
                scaleX: Math.round(100 * Math.min(.5, o.width / a.width)) / 100,
                scaleY: Math.round(100 * Math.min(.5, o.height / a.height)) / 100
              };
            }
            return {
              centerX: 0,
              centerY: 0,
              scaleX: .5,
              scaleY: .5
            };
          },
          calculateZoomToOrigin: function(e, r) {
            var i = "translate3d( {centerX}px, {centerY}px, 0 ) scale( {scaleX}, {scaleY} )",
              o = t.bind(null, n.supplant, i);
            return o(s.calculateTransformValues(e, r));
          },
          calculateSlideToOrigin: function(e, r) {
            var i = "translate3d( {centerX}px, {centerY}px, 0 )",
              o = t.bind(null, n.supplant, i);
            return o(s.calculateTransformValues(e, r));
          },
          toCss: function(e) {
            function n(e, n, i) {
              t.forEach(n.split(" "), function(e) {
                r[e] = i;
              });
            }
            var r = {},
              i = "left top right bottom width height x y min-width min-height max-width max-height";
            return t.forEach(e, function(e, a) {
              if (!t.isUndefined(e))
                if (i.indexOf(a) >= 0) r[a] = e + "px";
                else switch (a) {
                  case "transition":
                    n(a, o.CSS.TRANSITION, e);
                    break;
                  case "transform":
                    n(a, o.CSS.TRANSFORM, e);
                    break;
                  case "transformOrigin":
                    n(a, o.CSS.TRANSFORM_ORIGIN, e);
                    break;
                  case "font-size":
                    r["font-size"] = e;
                }
            }), r;
          },
          toTransformCss: function(e, n, r) {
            var i = {};
            return t.forEach(o.CSS.TRANSFORM.split(" "), function(t) {
                i[t] = e;
              }), n && (r = r || "all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1) !important", i.transition =
              r), i;
          },
          copyRect: function(e, n) {
            return e ? (n = n || {}, t.forEach("left top right bottom width height".split(" "), function(
                t) {
                n[t] = Math.round(e[t]);
              }), n.width = n.width || n.right - n.left, n.height = n.height || n.bottom - n.top, n) :
              null;
          },
          clientRect: function(e) {
            var n = t.element(e)[0].getBoundingClientRect(),
              r = function(e) {
                return e && e.width > 0 && e.height > 0;
              };
            return r(n) ? s.copyRect(n) : null;
          },
          centerPointFor: function(e) {
            return e ? {
              x: Math.round(e.left + e.width / 2),
              y: Math.round(e.top + e.height / 2)
            } : {
              x: 0,
              y: 0
            };
          }
        };
      }
      t.module("material.core").factory("$$mdAnimate", ["$q", "$timeout", "$mdConstant", "$animateCss",
        function(e, t, r, i) {
          return function(o) {
            return n(o, e, t, r, i);
          };
        }
      ]);
    }(),
    function() {
      t.version.minor >= 4 ? t.module("material.core.animate", []) : ! function() {
        function e(e) {
          return e.replace(/-[a-z]/g, function(e) {
            return e.charAt(1).toUpperCase();
          });
        }
        var n = t.forEach,
          r = t.isDefined(document.documentElement.style.WebkitAppearance),
          i = r ? "-webkit-" : "",
          o = (r ? "webkitTransitionEnd " : "") + "transitionend",
          a = (r ? "webkitAnimationEnd " : "") + "animationend",
          s = ["$document", function(e) {
            return function() {
              return e[0].body.clientWidth + 1;
            };
          }],
          c = ["$$rAF", function(e) {
            return function() {
              var t = !1;
              return e(function() {
                  t = !0;
                }),
                function(n) {
                  t ? n() : e(n);
                };
            };
          }],
          u = ["$q", "$$rAFMutex", function(e, r) {
            function i(e) {
              this.setHost(e), this._doneCallbacks = [], this._runInAnimationFrame = r(), this._state = 0;
            }
            var o = 0,
              a = 1,
              s = 2;
            return i.prototype = {
              setHost: function(e) {
                this.host = e || {};
              },
              done: function(e) {
                this._state === s ? e() : this._doneCallbacks.push(e);
              },
              progress: t.noop,
              getPromise: function() {
                if (!this.promise) {
                  var t = this;
                  this.promise = e(function(e, n) {
                    t.done(function(t) {
                      t === !1 ? n() : e();
                    });
                  });
                }
                return this.promise;
              },
              then: function(e, t) {
                return this.getPromise().then(e, t);
              },
              catch: function(e) {
                return this.getPromise().catch(e);
              },
              finally: function(e) {
                return this.getPromise().finally(e);
              },
              pause: function() {
                this.host.pause && this.host.pause();
              },
              resume: function() {
                this.host.resume && this.host.resume();
              },
              end: function() {
                this.host.end && this.host.end(), this._resolve(!0);
              },
              cancel: function() {
                this.host.cancel && this.host.cancel(), this._resolve(!1);
              },
              complete: function(e) {
                var t = this;
                t._state === o && (t._state = a, t._runInAnimationFrame(function() {
                  t._resolve(e);
                }));
              },
              _resolve: function(e) {
                this._state !== s && (n(this._doneCallbacks, function(t) {
                  t(e);
                }), this._doneCallbacks.length = 0, this._state = s);
              }
            }, i.all = function(e, t) {
              function r(n) {
                o = o && n, ++i === e.length && t(o);
              }
              var i = 0,
                o = !0;
              n(e, function(e) {
                e.done(r);
              });
            }, i;
          }];
        t.module("material.core.animate", []).factory("$$forceReflow", s).factory("$$AnimateRunner", u)
          .factory("$$rAFMutex", c).factory("$animateCss", ["$window", "$$rAF", "$$AnimateRunner",
            "$$forceReflow", "$$jqLite", "$timeout", "$animate",
            function(t, s, c, u, l, d, f) {
              function h(r, s) {
                var u = [],
                  l = _(r),
                  h = l && f.enabled(),
                  v = !1,
                  w = !1;
                h && (s.transitionStyle && u.push([i + "transition", s.transitionStyle]), s
                  .keyframeStyle && u.push([i + "animation", s.keyframeStyle]), s.delay && u.push([i +
                    "transition-delay", s.delay + "s"
                  ]), s.duration && u.push([i + "transition-duration", s.duration + "s"]), v = s
                  .keyframeStyle || s.to && (s.duration > 0 || s.transitionStyle), w = !!s.addClass || !
                  !s.removeClass, $(r, !0));
                var T = h && (v || w);
                b(r, s);
                var C,
                  x,
                  S = !1;
                return {
                  close: t.close,
                  start: function() {
                    function t() {
                      if (!S) return S = !0, C && x && r.off(C, x), p(r, s), y(r, s), n(u, function(t) {
                        l.style[e(t[0])] = "";
                      }), f.complete(!0), f;
                    }
                    var f = new c();
                    return g(function() {
                      if ($(r, !1), !T) return t();
                      n(u, function(t) {
                        var n = t[0],
                          r = t[1];
                        l.style[e(n)] = r;
                      }), p(r, s);
                      var c = m(r);
                      if (0 === c.duration) return t();
                      var f = [];
                      s.easing && (c.transitionDuration && f.push([i + "transition-timing-function",
                        s.easing
                      ]), c.animationDuration && f.push([i + "animation-timing-function", s
                        .easing
                      ])), s.delay && c.animationDelay && f.push([i + "animation-delay", s.delay +
                        "s"
                      ]), s.duration && c.animationDuration && f.push([i + "animation-duration", s
                        .duration + "s"
                      ]), n(f, function(t) {
                        var n = t[0],
                          r = t[1];
                        l.style[e(n)] = r, u.push(t);
                      });
                      var h = c.delay,
                        v = 1e3 * h,
                        g = c.duration,
                        y = 1e3 * g,
                        b = Date.now();
                      C = [], c.transitionDuration && C.push(o), c.animationDuration && C.push(a),
                        C = C.join(" "), x = function(e) {
                          e.stopPropagation();
                          var n = e.originalEvent || e,
                            r = n.timeStamp || Date.now(),
                            i = parseFloat(n.elapsedTime.toFixed(3));
                          Math.max(r - b, 0) >= v && i >= g && t();
                        }, r.on(C, x), E(r, s), d(t, v + 1.5 * y, !1);
                    }), f;
                  }
                };
              }

              function p(e, t) {
                t.addClass && (l.addClass(e, t.addClass), t.addClass = null), t.removeClass && (l
                  .removeClass(e, t.removeClass), t.removeClass = null);
              }

              function m(e) {
                function n(e) {
                  return r ? "Webkit" + e.charAt(0).toUpperCase() + e.substr(1) : e;
                }
                var i = _(e),
                  o = t.getComputedStyle(i),
                  a = v(o[n("transitionDuration")]),
                  s = v(o[n("animationDuration")]),
                  c = v(o[n("transitionDelay")]),
                  u = v(o[n("animationDelay")]);
                s *= parseInt(o[n("animationIterationCount")], 10) || 1;
                var l = Math.max(s, a),
                  d = Math.max(u, c);
                return {
                  duration: l,
                  delay: d,
                  animationDuration: s,
                  transitionDuration: a,
                  animationDelay: u,
                  transitionDelay: c
                };
              }

              function v(e) {
                var t = 0,
                  r = (e || "").split(/\s*,\s*/);
                return n(r, function(e) {
                  "s" == e.charAt(e.length - 1) && (e = e.substring(0, e.length - 1)), e = parseFloat(
                    e) || 0, t = t ? Math.max(e, t) : e;
                }), t;
              }

              function g(e) {
                w && w(), T.push(e), w = s(function() {
                  w = null;
                  for (var e = u(), t = 0; t < T.length; t++) T[t](e);
                  T.length = 0;
                });
              }

              function y(e, t) {
                b(e, t), E(e, t);
              }

              function b(e, t) {
                t.from && (e.css(t.from), t.from = null);
              }

              function E(e, t) {
                t.to && (e.css(t.to), t.to = null);
              }

              function _(e) {
                for (var t = 0; t < e.length; t++)
                  if (1 === e[t].nodeType) return e[t];
              }

              function $(t, n) {
                var r = _(t),
                  o = e(i + "transition-delay");
                r.style[o] = n ? "-9999s" : "";
              }
              var w,
                T = [];
              return h;
            }
          ]);
      }();
    }(),
    function() {
      t.module("material.components.autocomplete", ["material.core", "material.components.icon",
        "material.components.virtualRepeat"
      ]);
    }(),
    function() {
      function e(e) {
        return {
          restrict: "E",
          link: function(t, n) {
            e(n);
          }
        };
      }

      function n(e, n, r, i) {
        function o(e) {
          return t.isDefined(e.href) || t.isDefined(e.ngHref) || t.isDefined(e.ngLink) || t.isDefined(e
            .uiSref);
        }

        function a(e, t) {
          if (o(t)) return '<a class="md-button" ng-transclude></a>';
          var n = "undefined" == typeof t.type ? "button" : t.type;
          return '<button class="md-button" type="' + n + '" ng-transclude></button>';
        }

        function s(a, s, c) {
          n(s), e.attach(a, s), r.expectWithoutText(s, "aria-label"), o(c) && t.isDefined(c.ngDisabled) && a
            .$watch(c.ngDisabled, function(e) {
              s.attr("tabindex", e ? -1 : 0);
            }), s.on("click", function(e) {
              c.disabled === !0 && (e.preventDefault(), e.stopImmediatePropagation());
            }), s.hasClass("md-no-focus") || (s.on("focus", function() {
              i.isUserInvoked() && "keyboard" !== i.getLastInteractionType() || s.addClass("md-focused");
            }), s.on("blur", function() {
              s.removeClass("md-focused");
            }));
        }
        return {
          restrict: "EA",
          replace: !0,
          transclude: !0,
          template: a,
          link: s
        };
      }
      n.$inject = ["$mdButtonInkRipple", "$mdTheming", "$mdAria", "$mdInteraction"], e.$inject = [
          "$mdTheming"], t.module("material.components.button", ["material.core"]).directive("mdButton", n)
        .directive("a", e);
    }(),
    function() {
      function e(e) {
        return {
          restrict: "E",
          link: function(t, n) {
            n.addClass("_md"), t.$on("$destroy", function() {
              e.destroy();
            });
          }
        };
      }

      function n(e) {
        function n(e, n, o, a, s, c, u, l) {
          function d(r, i, u, d) {
            if (i = o.extractElementByName(i, "md-bottom-sheet"), i.attr("tabindex", "-1"), i.hasClass(
                "ng-cloak")) {
              var f =
                "$mdBottomSheet: using `<md-bottom-sheet ng-cloak >` will affect the bottom-sheet opening animations.";
              l.warn(f, i[0]);
            }
            u.disableBackdrop || (p = o.createBackdrop(r, "md-bottom-sheet-backdrop md-opaque"), p[0]
              .tabIndex = -1, u.clickOutsideToClose && p.on("click", function() {
                o.nextTick(s.cancel, !0);
              }), a.inherit(p, u.parent), e.enter(p, u.parent, null));
            var m = new h(i, u.parent);
            return u.bottomSheet = m, a.inherit(m.element, u.parent), u.disableParentScroll && (u
                .restoreScroll = o.disableScrollAround(m.element, u.parent)), e.enter(m.element, u.parent, p)
              .then(function() {
                var e = o.findFocusTarget(i) || t.element(i[0].querySelector("button") || i[0]
                  .querySelector("a") || i[0].querySelector(o.prefixer("ng-click", !0))) || p;
                u.escapeToClose && (u.rootElementKeyupCallback = function(e) {
                  e.keyCode === n.KEY_CODE.ESCAPE && o.nextTick(s.cancel, !0);
                }, c.on("keyup", u.rootElementKeyupCallback), e && e.focus());
              });
          }

          function f(t, n, r) {
            var i = r.bottomSheet;
            return r.disableBackdrop || e.leave(p), e.leave(i.element).then(function() {
              r.disableParentScroll && (r.restoreScroll(), delete r.restoreScroll), i.cleanup();
            });
          }

          function h(e, t) {
            function a(t) {
              e.css(n.CSS.TRANSITION_DURATION, "0ms");
            }

            function c(t) {
              var r = t.pointer.distanceY;
              r < 5 && (r = Math.max(-i, r / 2)), e.css(n.CSS.TRANSFORM, "translate3d(0," + (i + r) +
              "px,0)");
            }

            function l(t) {
              if (t.pointer.distanceY > 0 && (t.pointer.distanceY > 20 || Math.abs(t.pointer.velocityY) >
                r)) {
                var i = e.prop("offsetHeight") - t.pointer.distanceY,
                  a = Math.min(i / t.pointer.velocityY * .75, 500);
                e.css(n.CSS.TRANSITION_DURATION, a + "ms"), o.nextTick(s.cancel, !0);
              } else e.css(n.CSS.TRANSITION_DURATION, ""), e.css(n.CSS.TRANSFORM, "");
            }
            var d = u.register(t, "drag", {
              horizontal: !1
            });
            return t.on("$md.dragstart", a).on("$md.drag", c).on("$md.dragend", l), {
              element: e,
              cleanup: function() {
                d(), t.off("$md.dragstart", a), t.off("$md.drag", c), t.off("$md.dragend", l);
              }
            };
          }
          var p;
          return {
            themable: !0,
            onShow: d,
            onRemove: f,
            disableBackdrop: !1,
            escapeToClose: !0,
            clickOutsideToClose: !0,
            disableParentScroll: !0
          };
        }
        n.$inject = ["$animate", "$mdConstant", "$mdUtil", "$mdTheming", "$mdBottomSheet", "$rootElement",
          "$mdGesture", "$log"
        ];
        var r = .5,
          i = 80;
        return e("$mdBottomSheet").setDefaults({
          methods: ["disableParentScroll", "escapeToClose", "clickOutsideToClose"],
          options: n
        });
      }
      e.$inject = ["$mdBottomSheet"], n.$inject = ["$$interimElementProvider"], t.module(
        "material.components.bottomSheet", ["material.core", "material.components.backdrop"]).directive(
        "mdBottomSheet", e).provider("$mdBottomSheet", n);
    }(),
    function() {
      t.module("material.components.backdrop", ["material.core"]).directive("mdBackdrop", ["$mdTheming",
        "$mdUtil", "$animate", "$rootElement", "$window", "$log", "$$rAF", "$document",
        function(e, n, r, i, o, a, s, c) {
          function u(u, d, f) {
            function h() {
              var e = parseInt(p.height, 10) + Math.abs(parseInt(p.top, 10));
              d.css("height", e + "px");
            }
            r.pin && r.pin(d, i);
            var p;
            s(function() {
              if (p = o.getComputedStyle(c[0].body), "fixed" === p.position) {
                var r = n.debounce(function() {
                  p = o.getComputedStyle(c[0].body), h();
                }, 60, null, !1);
                h(), t.element(o).on("resize", r), u.$on("$destroy", function() {
                  t.element(o).off("resize", r);
                });
              }
              var i = d.parent();
              if (i.length) {
                "BODY" === i[0].nodeName && d.css("position", "fixed");
                var s = o.getComputedStyle(i[0]);
                "static" === s.position && a.warn(l), e.inherit(d, i);
              }
            });
          }
          var l =
          "<md-backdrop> may not work properly in a scrolled, static-positioned parent container.";
          return {
            restrict: "E",
            link: u
          };
        }
      ]);
    }(),
    function() {
      ! function() {
        function e(e, n, r) {
          function o(e, t) {
            try {
              t && e.css(c(t));
            } catch (e) {
              r.error(e.message);
            }
          }

          function a(e) {
            var t = l(e);
            return s(t);
          }

          function s(t, r) {
            r = r || !1;
            var i = e.PALETTES[t.palette][t.hue];
            return i = r ? i.contrast : i.value, n.supplant("rgba({0}, {1}, {2}, {3})", [i[0], i[1], i[2], i[
              3] || t.opacity]);
          }

          function c(e) {
            var n = {},
              r = e.hasOwnProperty("color");
            return t.forEach(e, function(e, t) {
              var i = l(e),
                o = t.indexOf("background") > -1;
              n[t] = s(i), o && !r && (n.color = s(i, !0));
            }), n;
          }

          function u(n) {
            return t.isDefined(e.THEMES[n.split("-")[0]]);
          }

          function l(n) {
            var r = n.split("-"),
              i = t.isDefined(e.THEMES[r[0]]),
              o = i ? r.splice(0, 1)[0] : e.defaultTheme();
            return {
              theme: o,
              palette: d(r, o),
              hue: f(r, o),
              opacity: r[2] || 1
            };
          }

          function d(t, r) {
            var o = t.length > 1 && i.indexOf(t[1]) !== -1,
              a = t[0].replace(/([a-z])([A-Z])/g, "$1-$2").toLowerCase();
            if (o && (a = t[0] + "-" + t.splice(1, 1)), i.indexOf(a) === -1) {
              var s = e.THEMES[r].colors[a];
              if (!s) throw new Error(n.supplant("mdColors: couldn't find '{palette}' in the palettes.", {
                palette: a
              }));
              a = s.name;
            }
            return a;
          }

          function f(t, r) {
            var i = e.THEMES[r].colors;
            if ("hue" === t[1]) {
              var o = parseInt(t.splice(2, 1)[0], 10);
              if (o < 1 || o > 3) throw new Error(n.supplant(
                "mdColors: 'hue-{hueNumber}' is not a valid hue, can be only 'hue-1', 'hue-2' and 'hue-3'", {
                  hueNumber: o
                }));
              if (t[1] = "hue-" + o, !(t[0] in i)) throw new Error(n.supplant(
                "mdColors: 'hue-x' can only be used with [{availableThemes}], but was used with '{usedTheme}'", {
                  availableThemes: Object.keys(i).join(", "),
                  usedTheme: t[0]
                }));
              return i[t[0]].hues[t[1]];
            }
            return t[1] || i[t[0] in i ? t[0] : "primary"].hues.default;
          }
          return i = i || Object.keys(e.PALETTES), {
            applyThemeColors: o,
            getThemeColor: a,
            hasTheme: u
          };
        }

        function n(e, n, i, o) {
          return {
            restrict: "A",
            require: ["^?mdTheme"],
            compile: function(a, s) {
              function c() {
                var e = s.mdColors,
                  i = e.indexOf("::") > -1,
                  o = !!i || r.test(s.mdColors);
                s.mdColors = e.replace("::", "");
                var a = t.isDefined(s.mdColorsWatch);
                return !i && !o && (!a || n.parseAttributeBoolean(s.mdColorsWatch));
              }
              var u = c();
              return function(n, r, a, s) {
                var c = s[0],
                  l = {},
                  d = function(t) {
                    "string" != typeof t && (t = ""), a.mdColors || (a.mdColors = "{}");
                    var r = o(a.mdColors)(n);
                    return c && Object.keys(r).forEach(function(n) {
                      var i = r[n];
                      e.hasTheme(i) || (r[n] = (t || c.$mdTheme) + "-" + i);
                    }), f(r), r;
                  },
                  f = function(e) {
                    if (!t.equals(e, l)) {
                      var n = Object.keys(l);
                      l.background && !n.color && n.push("color"), n.forEach(function(e) {
                        r.css(e, "");
                      });
                    }
                    l = e;
                  },
                  h = t.noop;
                c && (h = c.registerChanges(function(t) {
                  e.applyThemeColors(r, d(t));
                })), n.$on("$destroy", function() {
                  h();
                });
                try {
                  u ? n.$watch(d, t.bind(this, e.applyThemeColors, r), !0) : e.applyThemeColors(r, d());
                } catch (e) {
                  i.error(e.message);
                }
              };
            }
          };
        }
        n.$inject = ["$mdColors", "$mdUtil", "$log", "$parse"], e.$inject = ["$mdTheming", "$mdUtil", "$log"];
        var r = /^{((\s|,)*?["'a-zA-Z-]+?\s*?:\s*?('|")[a-zA-Z0-9-.]*('|"))+\s*}$/,
          i = null;
        t.module("material.components.colors", ["material.core"]).directive("mdColors", n).service(
          "$mdColors", e);
      }();
    }(),
    function() {
      function e(e) {
        return {
          restrict: "E",
          link: function(t, n, r) {
            n.addClass("_md"), e(n);
          }
        };
      }
      e.$inject = ["$mdTheming"], t.module("material.components.card", ["material.core"]).directive("mdCard",
        e);
    }(),
    function() {
      function e(e, n, r, i, o, a) {
        function s(s, c) {
          function u(s, c, u, l) {
            function d(e, t, n) {
              u[e] && s.$watch(u[e], function(e) {
                n[e] && c.attr(t, n[e]);
              });
            }

            function f(e) {
              var t = e.which || e.keyCode;
              t !== r.KEY_CODE.SPACE && t !== r.KEY_CODE.ENTER || (e.preventDefault(), c.addClass(
                "md-focused"), h(e));
            }

            function h(e) {
              c[0].hasAttribute("disabled") || s.skipToggle || s.$apply(function() {
                var t = u.ngChecked ? u.checked : !y.$viewValue;
                y.$setViewValue(t, e && e.type), y.$render();
              });
            }

            function p() {
              c.toggleClass("md-checked", !!y.$viewValue && !v);
            }

            function m(e) {
              v = e !== !1, v && c.attr("aria-checked", "mixed"), c.toggleClass("md-indeterminate", v);
            }
            var v,
              g = l[0],
              y = l[1] || o.fakeNgModel(),
              b = l[2];
            if (g) {
              var E = g.isErrorGetter || function() {
                return y.$invalid && (y.$touched || b && b.$submitted);
              };
              g.input = c, s.$watch(E, g.setInvalid);
            }
            i(c), c.children().on("focus", function() {
                c.focus();
              }), o.parseAttributeBoolean(u.mdIndeterminate) && (m(), s.$watch(u.mdIndeterminate, m)), u
              .ngChecked && s.$watch(s.$eval.bind(s, u.ngChecked), function(e) {
                y.$setViewValue(e), y.$render();
              }), d("ngDisabled", "tabindex", {
                true: "-1",
                false: u.tabindex
              }), n.expectWithText(c, "aria-label"), e.link.pre(s, {
                on: t.noop,
                0: {}
              }, u, [y]), c.on("click", h).on("keypress", f).on("focus", function() {
                "keyboard" === a.getLastInteractionType() && c.addClass("md-focused");
              }).on("blur", function() {
                c.removeClass("md-focused");
              }), y.$render = p;
          }
          return c.$set("tabindex", c.tabindex || "0"), c.$set("type", "checkbox"), c.$set("role", c.type), {
            pre: function(e, t) {
              t.on("click", function(e) {
                this.hasAttribute("disabled") && e.stopImmediatePropagation();
              });
            },
            post: u
          };
        }
        return e = e[0], {
          restrict: "E",
          transclude: !0,
          require: ["^?mdInputContainer", "?ngModel", "?^form"],
          priority: r.BEFORE_NG_ARIA,
          template: '<div class="md-container" md-ink-ripple md-ink-ripple-checkbox><div class="md-icon"></div></div><div ng-transclude class="md-label"></div>',
          compile: s
        };
      }
      e.$inject = ["inputDirective", "$mdAria", "$mdConstant", "$mdTheming", "$mdUtil", "$mdInteraction"], t
        .module("material.components.checkbox", ["material.core"]).directive("mdCheckbox", e);
    }(),
    function() {
      function e(e) {
        function t(e, t) {
          this.$scope = e, this.$element = t;
        }
        return {
          restrict: "E",
          controller: ["$scope", "$element", t],
          link: function(t, r) {
            r.addClass("_md"), e(r), t.$broadcast("$mdContentLoaded", r), n(r[0]);
          }
        };
      }

      function n(e) {
        t.element(e).on("$md.pressdown", function(t) {
          "t" === t.pointer.type && (t.$materialScrollFixed || (t.$materialScrollFixed = !0, 0 === e
            .scrollTop ? e.scrollTop = 1 : e.scrollHeight === e.scrollTop + e.offsetHeight && (e
              .scrollTop -= 1)));
        });
      }
      e.$inject = ["$mdTheming"], t.module("material.components.content", ["material.core"]).directive(
        "mdContent", e);
    }(),
    function() {
      t.module("material.components.chips", ["material.core", "material.components.autocomplete"]);
    }(),
    function() {
      function e(e, n, r) {
        return {
          restrict: "E",
          link: function(i, o) {
            o.addClass("_md"), n(o), e(function() {
              function e() {
                o.toggleClass("md-content-overflow", a.scrollHeight > a.clientHeight);
              }
              var n,
                a = o[0].querySelector("md-dialog-content");
              a && (n = a.getElementsByTagName("img"), e(), t.element(n).on("load", e)), i.$on(
                "$destroy",
                function() {
                  r.destroy(o);
                });
            });
          }
        };
      }

      function r(e) {
        function r(e, t) {
          return {
            template: [
              '<md-dialog md-theme="{{ dialog.theme || dialog.defaultTheme }}" aria-label="{{ dialog.ariaLabel }}" ng-class="dialog.css">',
              '  <md-dialog-content class="md-dialog-content" role="document" tabIndex="-1">',
              '    <h2 class="md-title">{{ dialog.title }}</h2>',
              '    <div ng-if="::dialog.mdHtmlContent" class="md-dialog-content-body" ',
              '        ng-bind-html="::dialog.mdHtmlContent"></div>',
              '    <div ng-if="::!dialog.mdHtmlContent" class="md-dialog-content-body">',
              "      <p>{{::dialog.mdTextContent}}</p>", "    </div>",
              '    <md-input-container md-no-float ng-if="::dialog.$type == \'prompt\'" class="md-prompt-input-container">',
              '      <input ng-keypress="dialog.keypress($event)" md-autofocus ng-model="dialog.result"              placeholder="{{::dialog.placeholder}}">',
              "    </md-input-container>", "  </md-dialog-content>", "  <md-dialog-actions>",
              '    <md-button ng-if="dialog.$type === \'confirm\' || dialog.$type === \'prompt\'"               ng-click="dialog.abort()" class="md-primary md-cancel-button">',
              "      {{ dialog.cancel }}", "    </md-button>",
              '    <md-button ng-click="dialog.hide()" class="md-primary md-confirm-button" md-autofocus="dialog.$type===\'alert\'">',
              "      {{ dialog.ok }}", "    </md-button>", "  </md-dialog-actions>", "</md-dialog>"
            ].join("").replace(/\s\s+/g, ""),
            controller: function() {
              var n = "prompt" == this.$type;
              n && this.initialValue && (this.result = this.initialValue), this.hide = function() {
                e.hide(!n || this.result);
              }, this.abort = function() {
                e.cancel();
              }, this.keypress = function(n) {
                n.keyCode === t.KEY_CODE.ENTER && e.hide(this.result);
              };
            },
            controllerAs: "dialog",
            bindToController: !0
          };
        }

        function i(e, r, i, s, c, u, l, d, f, h, p, m, v) {
          function g(e) {
            e.defaultTheme = p.defaultTheme(), _(e);
          }

          function y(e, t, n, r) {
            if (r) {
              var i = r.htmlContent || n.htmlContent || "",
                o = r.textContent || n.textContent || r.content || n.content || "";
              if (i && !h.has("$sanitize")) throw Error(
                "The ngSanitize module must be loaded in order to use htmlContent.");
              if (i && o) throw Error("md-dialog cannot have both `htmlContent` and `textContent`");
              r.mdHtmlContent = i, r.mdTextContent = o;
            }
          }

          function b(e, n, r, o) {
            function a() {
              n[0].querySelector(".md-actions") && f.warn(
                "Using a class of md-actions is deprecated, please use <md-dialog-actions>.");
            }

            function s() {
              function e() {
                return n[0].querySelector(".dialog-close, md-dialog-actions button:last-child");
              }
              if (r.focusOnOpen) {
                var t = i.findFocusTarget(n) || e() || c;
                t.focus();
              }
            }
            t.element(u[0].body).addClass("md-dialog-is-showing");
            var c = n.find("md-dialog");
            if (c.hasClass("ng-cloak")) {
              var l = "$mdDialog: using `<md-dialog ng-cloak>` will affect the dialog opening animations.";
              f.warn(l, n[0]);
            }
            return $(r), C(c, r), T(e, n, r), w(n, r), A(n, r).then(function() {
              x(n, r), a(), s();
            });
          }

          function E(e, n, r) {
            function i() {
              return M(n, r);
            }

            function s() {
              t.element(u[0].body).removeClass("md-dialog-is-showing"), r.contentElement && r
                .reverseContainerStretch(), r.cleanupElement(), r.$destroy || "keyboard" !== r
                .originInteraction || r.origin.focus();
            }
            return r.deactivateListeners(), r.unlockScreenReader(), r.hideBackdrop(r.$destroy), o && o
              .parentNode && o.parentNode.removeChild(o), a && a.parentNode && a.parentNode.removeChild(a), r
              .$destroy ? s() : i().then(s);
          }

          function _(e) {
            var n;
            e.targetEvent && e.targetEvent.target && (n = t.element(e.targetEvent.target));
            var r = n && n.controller("mdTheme");
            if (r) {
              e.themeWatch = r.$shouldWatch;
              var i = e.theme || r.$mdTheme;
              i && (e.scope.theme = i);
              var o = r.registerChanges(function(t) {
                e.scope.theme = t, e.themeWatch || o();
              });
            }
          }

          function $(e) {
            function r(e, r) {
              var i = t.element(e || {});
              if (i && i.length) {
                var o = {
                    top: 0,
                    left: 0,
                    height: 0,
                    width: 0
                  },
                  a = t.isFunction(i[0].getBoundingClientRect);
                return t.extend(r || {}, {
                  element: a ? i : n,
                  bounds: a ? i[0].getBoundingClientRect() : t.extend({}, o, i[0]),
                  focus: t.bind(i, i.focus)
                });
              }
            }

            function i(e, n) {
              return t.isString(e) && (e = u[0].querySelector(e)), t.element(e || n);
            }
            e.origin = t.extend({
              element: null,
              bounds: null,
              focus: t.noop
            }, e.origin || {}), e.parent = i(e.parent, d), e.closeTo = r(i(e.closeTo)), e.openFrom = r(i(e
              .openFrom)), e.targetEvent && (e.origin = r(e.targetEvent.target, e.origin), e
              .originInteraction = v.getLastInteractionType());
          }

          function w(n, r) {
            var o = t.element(l),
              a = i.debounce(function() {
                S(n, r);
              }, 60),
              c = [],
              u = function() {
                var t = "alert" == r.$type ? e.hide : e.cancel;
                i.nextTick(t, !0);
              };
            if (r.escapeToClose) {
              var d = r.parent,
                f = function(e) {
                  e.keyCode === s.KEY_CODE.ESCAPE && (e.stopPropagation(), e.preventDefault(), u());
                };
              n.on("keydown", f), d.on("keydown", f), c.push(function() {
                n.off("keydown", f), d.off("keydown", f);
              });
            }
            if (o.on("resize", a), c.push(function() {
                o.off("resize", a);
              }), r.clickOutsideToClose) {
              var h,
                p = n,
                m = function(e) {
                  h = e.target;
                },
                v = function(e) {
                  h === p[0] && e.target === p[0] && (e.stopPropagation(), e.preventDefault(), u());
                };
              p.on("mousedown", m), p.on("mouseup", v), c.push(function() {
                p.off("mousedown", m), p.off("mouseup", v);
              });
            }
            r.deactivateListeners = function() {
              c.forEach(function(e) {
                e();
              }), r.deactivateListeners = null;
            };
          }

          function T(e, t, n) {
            n.disableParentScroll && (n.restoreScroll = i.disableScrollAround(t, n.parent)), n.hasBackdrop &&
              (n.backdrop = i.createBackdrop(e, "md-dialog-backdrop md-opaque"), c.enter(n.backdrop, n
                .parent)), n.hideBackdrop = function(e) {
                n.backdrop && (e ? n.backdrop.remove() : c.leave(n.backdrop)), n.disableParentScroll && (n
                  .restoreScroll && n.restoreScroll(), delete n.restoreScroll), n.hideBackdrop = null;
              };
          }

          function C(e, t) {
            var n = "alert" === t.$type ? "alertdialog" : "dialog",
              s = e.find("md-dialog-content"),
              c = e.attr("id"),
              u = "dialogContent_" + (c || i.nextUid());
            e.attr({
                role: n,
                tabIndex: "-1"
              }), 0 === s.length && (s = e, c && (u = c)), s.attr("id", u), e.attr("aria-describedby", u), t
              .ariaLabel ? r.expect(e, "aria-label", t.ariaLabel) : r.expectAsync(e, "aria-label",
            function() {
                var e = s.text().split(/\s+/);
                return e.length > 3 && (e = e.slice(0, 3).concat("...")), e.join(" ");
              }), o = document.createElement("div"), o.classList.add("md-dialog-focus-trap"), o.tabIndex = 0,
              a = o.cloneNode(!1);
            var l = function() {
              e.focus();
            };
            o.addEventListener("focus", l), a.addEventListener("focus", l), e[0].parentNode.insertBefore(o, e[
              0]), e.after(a);
          }

          function x(e, t) {
            function n(e) {
              for (; e.parentNode;) {
                if (e === document.body) return;
                for (var t = e.parentNode.children, i = 0; i < t.length; i++) e === t[i] || k(t[i], ["SCRIPT",
                  "STYLE"
                ]) || t[i].setAttribute("aria-hidden", r);
                n(e = e.parentNode);
              }
            }
            var r = !0;
            n(e[0]), t.unlockScreenReader = function() {
              r = !1, n(e[0]), t.unlockScreenReader = null;
            };
          }

          function S(e, t) {
            var n = "fixed" == l.getComputedStyle(u[0].body).position,
              r = t.backdrop ? l.getComputedStyle(t.backdrop[0]) : null,
              i = r ? Math.min(u[0].body.clientHeight, Math.ceil(Math.abs(parseInt(r.height, 10)))) : 0,
              o = {
                top: e.css("top"),
                height: e.css("height")
              },
              a = Math.abs(t.parent[0].getBoundingClientRect().top);
            return e.css({
                top: (n ? a : 0) + "px",
                height: i ? i + "px" : "100%"
              }),
              function() {
                e.css(o);
              };
          }

          function A(e, t) {
            t.parent.append(e), t.reverseContainerStretch = S(e, t);
            var n = e.find("md-dialog"),
              r = i.dom.animator,
              o = r.calculateZoomToOrigin,
              a = {
                transitionInClass: "md-transition-in",
                transitionOutClass: "md-transition-out"
              },
              s = r.toTransformCss(o(n, t.openFrom || t.origin)),
              c = r.toTransformCss("");
            return n.toggleClass("md-dialog-fullscreen", !!t.fullscreen), r.translate3d(n, s, c, a).then(
              function(e) {
                return t.reverseAnimate = function() {
                  return delete t.reverseAnimate, t.closeTo ? (a = {
                    transitionInClass: "md-transition-out",
                    transitionOutClass: "md-transition-in"
                  }, s = c, c = r.toTransformCss(o(n, t.closeTo)), r.translate3d(n, s, c, a)) : e(c = r
                    .toTransformCss(o(n, t.origin)));
                }, t.clearAnimate = function() {
                  return delete t.clearAnimate, n.removeClass([a.transitionOutClass, a.transitionInClass]
                    .join(" ")), r.translate3d(n, c, r.toTransformCss(""), {});
                }, !0;
              });
          }

          function M(e, t) {
            return t.reverseAnimate().then(function() {
              t.contentElement && t.clearAnimate();
            });
          }

          function k(e, t) {
            if (t.indexOf(e.nodeName) !== -1) return !0;
          }
          return {
            hasBackdrop: !0,
            isolateScope: !0,
            onCompiling: g,
            onShow: b,
            onShowing: y,
            onRemove: E,
            clickOutsideToClose: !1,
            escapeToClose: !0,
            targetEvent: null,
            closeTo: null,
            openFrom: null,
            focusOnOpen: !0,
            disableParentScroll: !0,
            autoWrap: !0,
            fullscreen: !1,
            transformTemplate: function(e, t) {
              function n(e) {
                return t.autoWrap && !/<\/md-dialog>/g.test(e) ? "<md-dialog>" + (e || "") +
                  "</md-dialog>" : e || "";
              }
              var r = m.startSymbol(),
                i = m.endSymbol(),
                o = r + (t.themeWatch ? "" : "::") + "theme" + i;
              return '<div class="md-dialog-container" tabindex="-1" md-theme="' + o + '">' + n(e) +
                "</div>";
            }
          };
        }
        r.$inject = ["$mdDialog", "$mdConstant"], i.$inject = ["$mdDialog", "$mdAria", "$mdUtil",
          "$mdConstant", "$animate", "$document", "$window", "$rootElement", "$log", "$injector",
          "$mdTheming", "$interpolate", "$mdInteraction"
        ];
        var o, a;
        return e("$mdDialog").setDefaults({
          methods: ["disableParentScroll", "hasBackdrop", "clickOutsideToClose", "escapeToClose",
            "targetEvent", "closeTo", "openFrom", "parent", "fullscreen", "multiple"
          ],
          options: i
        }).addPreset("alert", {
          methods: ["title", "htmlContent", "textContent", "content", "ariaLabel", "ok", "theme", "css"],
          options: r
        }).addPreset("confirm", {
          methods: ["title", "htmlContent", "textContent", "content", "ariaLabel", "ok", "cancel",
            "theme", "css"
          ],
          options: r
        }).addPreset("prompt", {
          methods: ["title", "htmlContent", "textContent", "initialValue", "content", "placeholder",
            "ariaLabel", "ok", "cancel", "theme", "css"
          ],
          options: r
        });
      }
      e.$inject = ["$$rAF", "$mdTheming", "$mdDialog"], r.$inject = ["$$interimElementProvider"], t.module(
        "material.components.dialog", ["material.core", "material.components.backdrop"]).directive(
        "mdDialog", e).provider("$mdDialog", r);
    }(),
    function() {
      function e(e) {
        return {
          restrict: "E",
          link: e
        };
      }
      e.$inject = ["$mdTheming"], t.module("material.components.divider", ["material.core"]).directive(
        "mdDivider", e);
    }(),
    function() {
      ! function() {
        function e(e) {
          return {
            restrict: "E",
            require: ["^?mdFabSpeedDial", "^?mdFabToolbar"],
            compile: function(t, n) {
              var r = t.children(),
                i = e.prefixer().hasAttribute(r, "ng-repeat");
              i ? r.addClass("md-fab-action-item") : r.wrap('<div class="md-fab-action-item">');
            }
          };
        }
        e.$inject = ["$mdUtil"], t.module("material.components.fabActions", ["material.core"]).directive(
          "mdFabActions", e);
      }();
    }(),
    function() {
      t.module("material.components.datepicker", ["material.core", "material.components.icon",
        "material.components.virtualRepeat"
      ]);
    }(),
    function() {
      ! function() {
        function n() {
          function e(e, t, n) {
            t.addClass("md-fab-toolbar"), t.find("md-fab-trigger").find("button").prepend(
              '<div class="md-fab-toolbar-background"></div>');
          }
          return {
            restrict: "E",
            transclude: !0,
            template: '<div class="md-fab-toolbar-wrapper">  <div class="md-fab-toolbar-content" ng-transclude></div></div>',
            scope: {
              direction: "@?mdDirection",
              isOpen: "=?mdOpen"
            },
            bindToController: !0,
            controller: "MdFabController",
            controllerAs: "vm",
            link: e
          };
        }

        function r() {
          function n(n, r, i) {
            if (r) {
              var o = n[0],
                a = n.controller("mdFabToolbar"),
                s = o.querySelector(".md-fab-toolbar-background"),
                c = o.querySelector("md-fab-trigger button"),
                u = o.querySelector("md-toolbar"),
                l = o.querySelector("md-fab-trigger button md-icon"),
                d = n.find("md-fab-actions").children();
              if (c && s) {
                var f = e.getComputedStyle(c).getPropertyValue("background-color"),
                  h = o.offsetWidth,
                  p = (o.offsetHeight, 2 * (h / c.offsetWidth));
                s.style.backgroundColor = f, s.style.borderRadius = h + "px", a.isOpen ? (u.style
                  .pointerEvents = "inherit", s.style.width = c.offsetWidth + "px", s.style.height = c
                  .offsetHeight + "px", s.style.transform = "scale(" + p + ")", s.style.transitionDelay =
                  "0ms", l && (l.style.transitionDelay = ".3s"), t.forEach(d, function(e, t) {
                    e.style.transitionDelay = 25 * (d.length - t) + "ms";
                  })) : (u.style.pointerEvents = "none", s.style.transform = "scale(1)", s.style.top = "0",
                  n.hasClass("md-right") && (s.style.left = "0", s.style.right = null), n.hasClass(
                    "md-left") && (s.style.right = "0", s.style.left = null), s.style.transitionDelay =
                  "200ms", l && (l.style.transitionDelay = "0ms"), t.forEach(d, function(e, t) {
                    e.style.transitionDelay = 200 + 25 * t + "ms";
                  }));
              }
            }
          }
          return {
            addClass: function(e, t, r) {
              n(e, t, r), r();
            },
            removeClass: function(e, t, r) {
              n(e, t, r), r();
            }
          };
        }
        t.module("material.components.fabToolbar", ["material.core", "material.components.fabShared",
          "material.components.fabActions"
        ]).directive("mdFabToolbar", n).animation(".md-fab-toolbar", r).service("mdFabToolbarAnimation", r);
      }();
    }(),
    function() {
      ! function() {
        function e(e, n, r, i, o, a) {
          function s() {
            k.direction = k.direction || "down", k.isOpen = k.isOpen || !1, l(), n.addClass(
              "md-animations-waiting");
          }

          function c() {
            var r = ["click", "focusin", "focusout"];
            t.forEach(r, function(e) {
              n.on(e, u);
            }), e.$on("$destroy", function() {
              t.forEach(r, function(e) {
                n.off(e, u);
              }), p();
            });
          }

          function u(e) {
            "click" == e.type && S(e), "focusout" != e.type || I || (I = a(function() {
              k.close();
            }, 100, !1)), "focusin" == e.type && I && (a.cancel(I), I = null);
          }

          function l() {
            k.currentActionIndex = -1;
          }

          function d() {
            e.$watch("vm.direction", function(e, t) {
              r.removeClass(n, "md-" + t), r.addClass(n, "md-" + e), l();
            });
            var t, i;
            e.$watch("vm.isOpen", function(e) {
              l(), t && i || (t = A(), i = M()), e ? h() : p();
              var o = e ? "md-is-open" : "",
                a = e ? "" : "md-is-open";
              t.attr("aria-haspopup", !0), t.attr("aria-expanded", e), i.attr("aria-hidden", !e), r
                .setClass(n, o, a);
            });
          }

          function f() {
            n[0].scrollHeight > 0 ? r.addClass(n, "_md-animations-ready").then(function() {
              n.removeClass("md-animations-waiting");
            }) : N < 10 && (a(f, 100), N += 1);
          }

          function h() {
            n.on("keydown", v), i.nextTick(function() {
              t.element(document).on("click touchend", m);
            });
          }

          function p() {
            n.off("keydown", v), t.element(document).off("click touchend", m);
          }

          function m(e) {
            if (e.target) {
              var t = i.getClosest(e.target, "md-fab-trigger"),
                n = i.getClosest(e.target, "md-fab-actions");
              t || n || k.close();
            }
          }

          function v(e) {
            switch (e.which) {
              case o.KEY_CODE.ESCAPE:
                return k.close(), e.preventDefault(), !1;
              case o.KEY_CODE.LEFT_ARROW:
                return _(e), !1;
              case o.KEY_CODE.UP_ARROW:
                return $(e), !1;
              case o.KEY_CODE.RIGHT_ARROW:
                return w(e), !1;
              case o.KEY_CODE.DOWN_ARROW:
                return T(e), !1;
            }
          }

          function g(e) {
            b(e, -1);
          }

          function y(e) {
            b(e, 1);
          }

          function b(e, n) {
            var r = E();
            k.currentActionIndex = k.currentActionIndex + n, k.currentActionIndex = Math.min(r.length - 1, k
              .currentActionIndex), k.currentActionIndex = Math.max(0, k.currentActionIndex);
            var i = t.element(r[k.currentActionIndex]).children()[0];
            t.element(i).attr("tabindex", 0), i.focus(), e.preventDefault(), e.stopImmediatePropagation();
          }

          function E() {
            var e = M()[0].querySelectorAll(".md-fab-action-item");
            return t.forEach(e, function(e) {
              t.element(t.element(e).children()[0]).attr("tabindex", -1);
            }), e;
          }

          function _(e) {
            "left" === k.direction ? y(e) : g(e);
          }

          function $(e) {
            "down" === k.direction ? g(e) : y(e);
          }

          function w(e) {
            "left" === k.direction ? g(e) : y(e);
          }

          function T(e) {
            "up" === k.direction ? g(e) : y(e);
          }

          function C(e) {
            return i.getClosest(e, "md-fab-trigger");
          }

          function x(e) {
            return i.getClosest(e, "md-fab-actions");
          }

          function S(e) {
            C(e.target) && k.toggle(), x(e.target) && k.close();
          }

          function A() {
            return n.find("md-fab-trigger");
          }

          function M() {
            return n.find("md-fab-actions");
          }
          var k = this,
            N = 0;
          k.open = function() {
            e.$evalAsync("vm.isOpen = true");
          }, k.close = function() {
            e.$evalAsync("vm.isOpen = false"), n.find("md-fab-trigger")[0].focus();
          }, k.toggle = function() {
            e.$evalAsync("vm.isOpen = !vm.isOpen");
          }, k.$onInit = function() {
            s(), c(), d(), f();
          }, 1 === t.version.major && t.version.minor <= 4 && this.$onInit();
          var I;
        }
        e.$inject = ["$scope", "$element", "$animate", "$mdUtil", "$mdConstant", "$timeout"], t.module(
          "material.components.fabShared", ["material.core"]).controller("MdFabController", e);
      }();
    }(),
    function() {
      ! function() {
        function n() {
          function e(e, t) {
            t.prepend('<div class="_md-css-variables"></div>');
          }
          return {
            restrict: "E",
            scope: {
              direction: "@?mdDirection",
              isOpen: "=?mdOpen"
            },
            bindToController: !0,
            controller: "MdFabController",
            controllerAs: "vm",
            link: e
          };
        }

        function r(n) {
          function r(e) {
            n(e, o, !1);
          }

          function i(n) {
            if (!n.hasClass("md-animations-waiting") || n.hasClass("_md-animations-ready")) {
              var r = n[0],
                i = n.controller("mdFabSpeedDial"),
                o = r.querySelectorAll(".md-fab-action-item"),
                a = r.querySelector("md-fab-trigger"),
                s = r.querySelector("._md-css-variables"),
                c = parseInt(e.getComputedStyle(s).zIndex);
              t.forEach(o, function(e, t) {
                var n = e.style;
                n.transform = n.webkitTransform = "", n.transitionDelay = "", n.opacity = 1, n.zIndex = o
                  .length - t + c;
              }), a.style.zIndex = c + o.length + 1, i.isOpen || t.forEach(o, function(e, t) {
                var n,
                  r,
                  o = e.style,
                  s = (a.clientHeight - e.clientHeight) / 2,
                  c = (a.clientWidth - e.clientWidth) / 2;
                switch (i.direction) {
                  case "up":
                    n = e.scrollHeight * (t + 1) + s, r = "Y";
                    break;
                  case "down":
                    n = -(e.scrollHeight * (t + 1) + s), r = "Y";
                    break;
                  case "left":
                    n = e.scrollWidth * (t + 1) + c, r = "X";
                    break;
                  case "right":
                    n = -(e.scrollWidth * (t + 1) + c), r = "X";
                }
                var u = "translate" + r + "(" + n + "px)";
                o.transform = o.webkitTransform = u;
              });
            }
          }
          return {
            addClass: function(e, t, n) {
              e.hasClass("md-fling") ? (i(e), r(n)) : n();
            },
            removeClass: function(e, t, n) {
              i(e), r(n);
            }
          };
        }

        function i(n) {
          function r(e) {
            n(e, o, !1);
          }

          function i(n) {
            var r = n[0],
              i = n.controller("mdFabSpeedDial"),
              o = r.querySelectorAll(".md-fab-action-item"),
              s = r.querySelector("._md-css-variables"),
              c = parseInt(e.getComputedStyle(s).zIndex);
            t.forEach(o, function(e, t) {
              var n = e.style,
                r = t * a;
              n.opacity = i.isOpen ? 1 : 0, n.transform = n.webkitTransform = i.isOpen ? "scale(1)" :
                "scale(0)", n.transitionDelay = (i.isOpen ? r : o.length - r) + "ms", n.zIndex = o
                .length - t + c;
            });
          }
          var a = 65;
          return {
            addClass: function(e, t, n) {
              i(e), r(n);
            },
            removeClass: function(e, t, n) {
              i(e), r(n);
            }
          };
        }
        r.$inject = ["$timeout"], i.$inject = ["$timeout"];
        var o = 300;
        t.module("material.components.fabSpeedDial", ["material.core", "material.components.fabShared",
          "material.components.fabActions"
        ]).directive("mdFabSpeedDial", n).animation(".md-fling", r).animation(".md-scale", i).service(
          "mdFabSpeedDialFlingAnimation", r).service("mdFabSpeedDialScaleAnimation", i);
      }();
    }(),
    function() {
      function e(e, r, i, o) {
        function a(n, a, s, c) {
          function u() {
            for (var e in r.MEDIA) o(e), o.getQuery(r.MEDIA[e]).addListener(w);
            return o.watchResponsiveAttributes(["md-cols", "md-row-height", "md-gutter"], s, d);
          }

          function l() {
            c.layoutDelegate = t.noop, T();
            for (var e in r.MEDIA) o.getQuery(r.MEDIA[e]).removeListener(w);
          }

          function d(e) {
            null == e ? c.invalidateLayout() : o(e) && c.invalidateLayout();
          }

          function f(e) {
            var r = v(),
              o = {
                tileSpans: g(r),
                colCount: y(),
                rowMode: _(),
                rowHeight: E(),
                gutter: b()
              };
            if (e || !t.equals(o, C)) {
              var s = i(o.colCount, o.tileSpans, r).map(function(e, n) {
                return {
                  grid: {
                    element: a,
                    style: m(o.colCount, n, o.gutter, o.rowMode, o.rowHeight)
                  },
                  tiles: e.map(function(e, i) {
                    return {
                      element: t.element(r[i]),
                      style: p(e.position, e.spans, o.colCount, n, o.gutter, o.rowMode, o.rowHeight)
                    };
                  })
                };
              }).reflow().performance();
              n.mdOnLayout({
                $event: {
                  performance: s
                }
              }), C = o;
            }
          }

          function h(e) {
            return x + e + S;
          }

          function p(e, t, n, r, i, o, a) {
            var s = 1 / n * 100,
              c = (n - 1) / n,
              u = A({
                share: s,
                gutterShare: c,
                gutter: i
              }),
              l = {
                left: M({
                  unit: u,
                  offset: e.col,
                  gutter: i
                }),
                width: k({
                  unit: u,
                  span: t.col,
                  gutter: i
                }),
                paddingTop: "",
                marginTop: "",
                top: "",
                height: ""
              };
            switch (o) {
              case "fixed":
                l.top = M({
                  unit: a,
                  offset: e.row,
                  gutter: i
                }), l.height = k({
                  unit: a,
                  span: t.row,
                  gutter: i
                });
                break;
              case "ratio":
                var d = s / a,
                  f = A({
                    share: d,
                    gutterShare: c,
                    gutter: i
                  });
                l.paddingTop = k({
                  unit: f,
                  span: t.row,
                  gutter: i
                }), l.marginTop = M({
                  unit: f,
                  offset: e.row,
                  gutter: i
                });
                break;
              case "fit":
                var h = (r - 1) / r,
                  d = 1 / r * 100,
                  f = A({
                    share: d,
                    gutterShare: h,
                    gutter: i
                  });
                l.top = M({
                  unit: f,
                  offset: e.row,
                  gutter: i
                }), l.height = k({
                  unit: f,
                  span: t.row,
                  gutter: i
                });
            }
            return l;
          }

          function m(e, t, n, r, i) {
            var o = {};
            switch (r) {
              case "fixed":
                o.height = k({
                  unit: i,
                  span: t,
                  gutter: n
                }), o.paddingBottom = "";
                break;
              case "ratio":
                var a = 1 === e ? 0 : (e - 1) / e,
                  s = 1 / e * 100,
                  c = s * (1 / i),
                  u = A({
                    share: c,
                    gutterShare: a,
                    gutter: n
                  });
                o.height = "", o.paddingBottom = k({
                  unit: u,
                  span: t,
                  gutter: n
                });
                break;
              case "fit":
            }
            return o;
          }

          function v() {
            return [].filter.call(a.children(), function(e) {
              return "MD-GRID-TILE" == e.tagName && !e.$$mdDestroyed;
            });
          }

          function g(e) {
            return [].map.call(e, function(e) {
              var n = t.element(e).controller("mdGridTile");
              return {
                row: parseInt(o.getResponsiveAttribute(n.$attrs, "md-rowspan"), 10) || 1,
                col: parseInt(o.getResponsiveAttribute(n.$attrs, "md-colspan"), 10) || 1
              };
            });
          }

          function y() {
            var e = parseInt(o.getResponsiveAttribute(s, "md-cols"), 10);
            if (isNaN(e))
            throw "md-grid-list: md-cols attribute was not found, or contained a non-numeric value";
            return e;
          }

          function b() {
            return $(o.getResponsiveAttribute(s, "md-gutter") || 1);
          }

          function E() {
            var e = o.getResponsiveAttribute(s, "md-row-height");
            if (!e) throw "md-grid-list: md-row-height attribute was not found";
            switch (_()) {
              case "fixed":
                return $(e);
              case "ratio":
                var t = e.split(":");
                return parseFloat(t[0]) / parseFloat(t[1]);
              case "fit":
                return 0;
            }
          }

          function _() {
            var e = o.getResponsiveAttribute(s, "md-row-height");
            if (!e) throw "md-grid-list: md-row-height attribute was not found";
            return "fit" == e ? "fit" : e.indexOf(":") !== -1 ? "ratio" : "fixed";
          }

          function $(e) {
            return /\D$/.test(e) ? e : e + "px";
          }
          a.addClass("_md"), a.attr("role", "list"), c.layoutDelegate = f;
          var w = t.bind(c, c.invalidateLayout),
            T = u();
          n.$on("$destroy", l);
          var C,
            x = e.startSymbol(),
            S = e.endSymbol(),
            A = e(h("share") + "% - (" + h("gutter") + " * " + h("gutterShare") + ")"),
            M = e("calc((" + h("unit") + " + " + h("gutter") + ") * " + h("offset") + ")"),
            k = e("calc((" + h("unit") + ") * " + h("span") + " + (" + h("span") + " - 1) * " + h("gutter") +
              ")");
        }
        return {
          restrict: "E",
          controller: n,
          scope: {
            mdOnLayout: "&"
          },
          link: a
        };
      }

      function n(e) {
        this.layoutInvalidated = !1, this.tilesInvalidated = !1, this.$timeout_ = e.nextTick, this
          .layoutDelegate = t.noop;
      }

      function r(e) {
        function n(t, n) {
          var r, a, s, c, u, l;
          return c = e.time(function() {
            a = i(t, n);
          }), r = {
            layoutInfo: function() {
              return a;
            },
            map: function(t) {
              return u = e.time(function() {
                var e = r.layoutInfo();
                s = t(e.positioning, e.rowCount);
              }), r;
            },
            reflow: function(t) {
              return l = e.time(function() {
                var e = t || o;
                e(s.grid, s.tiles);
              }), r;
            },
            performance: function() {
              return {
                tileCount: n.length,
                layoutTime: c,
                mapTime: u,
                reflowTime: l,
                totalTime: c + u + l
              };
            }
          };
        }

        function r(e, t) {
          e.element.css(e.style), t.forEach(function(e) {
            e.element.css(e.style);
          });
        }

        function i(e, t) {
          function n(t, n) {
            if (t.col > e) throw "md-grid-list: Tile at position " + n + " has a colspan (" + t.col +
              ") that exceeds the column count (" + e + ")";
            for (var a = 0, l = 0; l - a < t.col;) s >= e ? r() : (a = u.indexOf(0, s), a !== -1 && (l = o(a +
              1)) !== -1 ? s = l + 1 : (a = l = 0, r()));
            return i(a, t.col, t.row), s = a + t.col, {
              col: a,
              row: c
            };
          }

          function r() {
            s = 0, c++, i(0, e, -1);
          }

          function i(e, t, n) {
            for (var r = e; r < e + t; r++) u[r] = Math.max(u[r] + n, 0);
          }

          function o(e) {
            var t;
            for (t = e; t < u.length; t++)
              if (0 !== u[t]) return t;
            if (t === u.length) return t;
          }

          function a() {
            for (var t = [], n = 0; n < e; n++) t.push(0);
            return t;
          }
          var s = 0,
            c = 0,
            u = a();
          return {
            positioning: t.map(function(e, t) {
              return {
                spans: e,
                position: n(e, t)
              };
            }),
            rowCount: c + Math.max.apply(Math, u)
          };
        }
        var o = r;
        return n.animateWith = function(e) {
          o = t.isFunction(e) ? e : r;
        }, n;
      }

      function i(e) {
        function n(n, r, i, o) {
          r.attr("role", "listitem");
          var a = e.watchResponsiveAttributes(["md-colspan", "md-rowspan"], i, t.bind(o, o.invalidateLayout));
          o.invalidateTiles(), n.$on("$destroy", function() {
            r[0].$$mdDestroyed = !0, a(), o.invalidateLayout();
          }), t.isDefined(n.$parent.$index) && n.$watch(function() {
            return n.$parent.$index;
          }, function(e, t) {
            e !== t && o.invalidateTiles();
          });
        }
        return {
          restrict: "E",
          require: "^mdGridList",
          template: "<figure ng-transclude></figure>",
          transclude: !0,
          scope: {},
          controller: ["$attrs", function(e) {
            this.$attrs = e;
          }],
          link: n
        };
      }

      function o() {
        return {
          template: "<figcaption ng-transclude></figcaption>",
          transclude: !0
        };
      }
      n.$inject = ["$mdUtil"], r.$inject = ["$mdUtil"], e.$inject = ["$interpolate", "$mdConstant",
        "$mdGridLayout", "$mdMedia"
      ], i.$inject = ["$mdMedia"], t.module("material.components.gridList", ["material.core"]).directive(
        "mdGridList", e).directive("mdGridTile", i).directive("mdGridTileFooter", o).directive(
        "mdGridTileHeader", o).factory("$mdGridLayout", r), n.prototype = {
        invalidateTiles: function() {
          this.tilesInvalidated = !0, this.invalidateLayout();
        },
        invalidateLayout: function() {
          this.layoutInvalidated || (this.layoutInvalidated = !0, this.$timeout_(t.bind(this, this
            .layout)));
        },
        layout: function() {
          try {
            this.layoutDelegate(this.tilesInvalidated);
          } finally {
            this.layoutInvalidated = !1, this.tilesInvalidated = !1;
          }
        }
      };
    }(),
    function() {
      t.module("material.components.icon", ["material.core"]);
    }(),
    function() {
      function n(e, t) {
        function n(t) {
          var n = t[0].querySelector(o),
            r = t[0].querySelector(a);
          return n && t.addClass("md-icon-left"), r && t.addClass("md-icon-right"),
            function(t, n) {
              e(n);
            };
        }

        function r(e, n, r, i) {
          var o = this;
          o.isErrorGetter = r.mdIsError && t(r.mdIsError), o.delegateClick = function() {
            o.input.focus();
          }, o.element = n, o.setFocused = function(e) {
            n.toggleClass("md-input-focused", !!e);
          }, o.setHasValue = function(e) {
            n.toggleClass("md-input-has-value", !!e);
          }, o.setHasPlaceholder = function(e) {
            n.toggleClass("md-input-has-placeholder", !!e);
          }, o.setInvalid = function(e) {
            e ? i.addClass(n, "md-input-invalid") : i.removeClass(n, "md-input-invalid");
          }, e.$watch(function() {
            return o.label && o.input;
          }, function(e) {
            e && !o.label.attr("for") && o.label.attr("for", o.input.attr("id"));
          });
        }
        r.$inject = ["$scope", "$element", "$attrs", "$animate"];
        var i = ["INPUT", "TEXTAREA", "SELECT", "MD-SELECT"],
          o = i.reduce(function(e, t) {
            return e.concat(["md-icon ~ " + t, ".md-icon ~ " + t]);
          }, []).join(","),
          a = i.reduce(function(e, t) {
            return e.concat([t + " ~ md-icon", t + " ~ .md-icon"]);
          }, []).join(",");
        return {
          restrict: "E",
          compile: n,
          controller: r
        };
      }

      function r() {
        return {
          restrict: "E",
          require: "^?mdInputContainer",
          link: function(e, t, n, r) {
            !r || n.mdNoFloat || t.hasClass("md-container-ignore") || (r.label = t, e.$on("$destroy",
              function() {
                r.label = null;
              }));
          }
        };
      }

      function i(e, n, r, i, o) {
        function a(a, s, c, u) {
          function l(e) {
            return p.setHasValue(!v.$isEmpty(e)), e;
          }

          function d() {
            p.label && c.$observe("required", function(e) {
              p.label.toggleClass("md-required", e && !b);
            });
          }

          function f() {
            p.setHasValue(s.val().length > 0 || (s[0].validity || {}).badInput);
          }

          function h() {
            function r() {
              s.attr("rows", 1).css("height", "auto").addClass("md-no-flex");
              var e = u();
              if (!E) {
                var t = s[0].style.padding || "";
                E = s.css("padding", 0).prop("offsetHeight"), s[0].style.padding = t;
              }
              if (g && E && (e = Math.max(e, E * g)), y && E) {
                var n = E * y;
                n < e ? (s.attr("md-no-autogrow", ""), e = n) : s.removeAttr("md-no-autogrow");
              }
              E && s.attr("rows", Math.round(e / E)), s.css("height", e + "px").removeClass("md-no-flex");
            }

            function u() {
              var e = _.offsetHeight,
                t = _.scrollHeight - e;
              return e + Math.max(t, 0);
            }

            function l(t) {
              return e.nextTick(r), t;
            }

            function d() {
              if (h && (h = !1, t.element(n).off("resize", r), b && b(), s.attr("md-no-autogrow", "").off(
                  "input", r), m)) {
                var e = v.$formatters.indexOf(l);
                e > -1 && v.$formatters.splice(e, 1);
              }
            }

            function f() {
              function e(e) {
                e.preventDefault(), l = !0, f = e.clientY, h = parseFloat(s.css("height")) || s.prop(
                  "offsetHeight");
              }

              function n(e) {
                l && (e.preventDefault(), d(), m.addClass("md-input-resized"));
              }

              function r(e) {
                l && s.css("height", h + e.pointer.distanceY + "px");
              }

              function i(e) {
                l && (l = !1, m.removeClass("md-input-resized"));
              }
              if (!c.hasOwnProperty("mdNoResize")) {
                var u = t.element('<div class="md-resize-handle"></div>'),
                  l = !1,
                  f = null,
                  h = 0,
                  m = p.element,
                  v = o.register(u, "drag", {
                    horizontal: !1
                  });
                s.wrap('<div class="md-resize-wrapper">').after(u), u.on("mousedown", e), m.on(
                  "$md.dragstart", n).on("$md.drag", r).on("$md.dragend", i), a.$on("$destroy", function() {
                  u.off("mousedown", e).remove(), m.off("$md.dragstart", n).off("$md.drag", r).off(
                    "$md.dragend", i), v(), u = null, m = null, v = null;
                });
              }
            }
            var h = !c.hasOwnProperty("mdNoAutogrow");
            if (f(), h) {
              var g = c.hasOwnProperty("rows") ? parseInt(c.rows) : NaN,
                y = c.hasOwnProperty("maxRows") ? parseInt(c.maxRows) : NaN,
                b = a.$on("md-resize-textarea", r),
                E = null,
                _ = s[0];
              if (i(function() {
                  e.nextTick(r);
                }, 10, !1), s.on("input", r), m && v.$formatters.push(l), g || s.attr("rows", 1), t.element(n)
                .on("resize", r), a.$on("$destroy", d), c.hasOwnProperty("mdDetectHidden")) {
                var $ = function() {
                  var e = !1;
                  return function() {
                    var t = 0 === _.offsetHeight;
                    t === !1 && e === !0 && r(), e = t;
                  };
                }();
                a.$watch(function() {
                  return e.nextTick($, !1), !0;
                });
              }
            }
          }
          var p = u[0],
            m = !!u[1],
            v = u[1] || e.fakeNgModel(),
            g = u[2],
            y = t.isDefined(c.readonly),
            b = e.parseAttributeBoolean(c.mdNoAsterisk),
            E = s[0].tagName.toLowerCase();
          if (p) {
            if ("hidden" === c.type) return void s.attr("aria-hidden", "true");
            if (p.input) {
              if (p.input[0].contains(s[0])) return;
              throw new Error(
                "<md-input-container> can only have *one* <input>, <textarea> or <md-select> child element!"
                );
            }
            p.input = s, d();
            var _ = t.element('<div class="md-errors-spacer">');
            s.after(_), p.label || r.expect(s, "aria-label", c.placeholder), s.addClass("md-input"), s.attr(
                "id") || s.attr("id", "input_" + e.nextUid()), "input" === E && "number" === c.type && c
              .min && c.max && !c.step ? s.attr("step", "any") : "textarea" === E && h(), m || f();
            var $ = p.isErrorGetter || function() {
              return v.$invalid && (v.$touched || g && g.$submitted);
            };
            a.$watch($, p.setInvalid), c.ngValue && c.$observe("value", f), v.$parsers.push(l), v.$formatters
              .push(l), s.on("input", f), y || s.on("focus", function(t) {
                e.nextTick(function() {
                  p.setFocused(!0);
                });
              }).on("blur", function(t) {
                e.nextTick(function() {
                  p.setFocused(!1), f();
                });
              }), a.$on("$destroy", function() {
                p.setFocused(!1), p.setHasValue(!1), p.input = null;
              });
          }
        }
        return {
          restrict: "E",
          require: ["^?mdInputContainer", "?ngModel", "?^form"],
          link: a
        };
      }

      function o(e, n) {
        function r(r, i, o, a) {
          function s(e) {
            return u.parent ? (u.text(String(i.val() || e || "").length + " / " + c), e) : e;
          }
          var c,
            u,
            l,
            d = a[0],
            f = a[1];
          n.nextTick(function() {
            l = t.element(f.element[0].querySelector(".md-errors-spacer")), u = t.element(
              '<div class="md-char-counter">'), l.append(u), o.$set("ngTrim", "false"), r.$watch(o
              .mdMaxlength,
              function(n) {
                c = n, t.isNumber(n) && n > 0 ? (u.parent().length || e.enter(u, l), s()) : e.leave(u);
              }), d.$validators["md-maxlength"] = function(e, n) {
              return !t.isNumber(c) || c < 0 || (s(), (e || i.val() || n || "").length <= c);
            };
          });
        }
        return {
          restrict: "A",
          require: ["ngModel", "^mdInputContainer"],
          link: r
        };
      }

      function a(e) {
        function n(n, r, i, o) {
          if (o) {
            var a = o.element.find("label"),
              s = o.element.attr("md-no-float");
            if (a && a.length || "" === s || n.$eval(s)) return void o.setHasPlaceholder(!0);
            if ("MD-SELECT" != r[0].nodeName) {
              var c = t.element('<label ng-click="delegateClick()" tabindex="-1">' + i.placeholder +
                "</label>");
              i.$set("placeholder", null), o.element.addClass("md-icon-float").prepend(c), e(c)(n);
            }
          }
        }
        return {
          restrict: "A",
          require: "^^?mdInputContainer",
          priority: 200,
          link: {
            pre: n
          }
        };
      }

      function s(e) {
        function t(t, n, r) {
          function i() {
            a = !0, e(function() {
              n[0].select(), a = !1;
            }, 1, !1);
          }

          function o(e) {
            a && e.preventDefault();
          }
          if ("INPUT" === n[0].nodeName || "TEXTAREA" === n[0].nodeName) {
            var a = !1;
            n.on("focus", i).on("mouseup", o), t.$on("$destroy", function() {
              n.off("focus", i).off("mouseup", o);
            });
          }
        }
        return {
          restrict: "A",
          link: t
        };
      }

      function c() {
        function e(e, n, r, i) {
          i && (n.toggleClass("md-input-messages-animation", !0), n.toggleClass("md-auto-hide", !0), (
            "false" == r.mdAutoHide || t(r)) && n.toggleClass("md-auto-hide", !1));
        }

        function t(e) {
          return C.some(function(t) {
            return e[t];
          });
        }
        return {
          restrict: "EA",
          link: e,
          require: "^^?mdInputContainer"
        };
      }

      function u(e) {
        function t(t) {
          function n() {
            for (var e = t[0]; e = e.parentNode;)
              if (e.nodeType === Node.DOCUMENT_FRAGMENT_NODE) return !0;
            return !1;
          }

          function r(t) {
            return !!e.getClosest(t, "md-input-container");
          }

          function i(e) {
            e.toggleClass("md-input-message-animation", !0);
          }
          if (r(t)) i(t);
          else if (n()) return function(e, n) {
            r(n) && i(t);
          };
        }
        return {
          restrict: "EA",
          compile: t,
          priority: 100
        };
      }

      function l(e, t, n, r) {
        return b(e, t, n, r), {
          addClass: function(e, t, n) {
            h(e, n);
          }
        };
      }

      function d(e, t, n, r) {
        return b(e, t, n, r), {
          enter: function(e, t) {
            h(e, t);
          },
          leave: function(e, t) {
            p(e, t);
          },
          addClass: function(e, t, n) {
            "ng-hide" == t ? p(e, n) : n();
          },
          removeClass: function(e, t, n) {
            "ng-hide" == t ? h(e, n) : n();
          }
        };
      }

      function f(e, t, n, r) {
        return b(e, t, n, r), {
          enter: function(e, t) {
            var n = m(e);
            n.start().done(t);
          },
          leave: function(e, t) {
            var n = v(e);
            n.start().done(t);
          }
        };
      }

      function h(e, n) {
        var r,
          i = [],
          o = y(e),
          a = o.children();
        return 0 == o.length || 0 == a.length ? (T.warn(
          "mdInput messages show animation called on invalid messages element: ", e), void n()) : (t
          .forEach(a, function(e) {
            r = m(t.element(e)), i.push(r.start());
          }), void _.all(i, n));
      }

      function p(e, n) {
        var r,
          i = [],
          o = y(e),
          a = o.children();
        return 0 == o.length || 0 == a.length ? (T.warn(
          "mdInput messages hide animation called on invalid messages element: ", e), void n()) : (t
          .forEach(a, function(e) {
            r = v(t.element(e)), i.push(r.start());
          }), void _.all(i, n));
      }

      function m(t) {
        var n = parseInt(e.getComputedStyle(t[0]).height),
          r = parseInt(e.getComputedStyle(t[0]).marginTop),
          i = y(t),
          o = g(t),
          a = r > -n;
        return a || i.hasClass("md-auto-hide") && !o.hasClass("md-input-invalid") ? $(t, {}) : $(t, {
          event: "enter",
          structural: !0,
          from: {
            opacity: 0,
            "margin-top": -n + "px"
          },
          to: {
            opacity: 1,
            "margin-top": "0"
          },
          duration: .3
        });
      }

      function v(t) {
        var n = t[0].offsetHeight,
          r = e.getComputedStyle(t[0]);
        return 0 === parseInt(r.opacity) ? $(t, {}) : $(t, {
          event: "leave",
          structural: !0,
          from: {
            opacity: 1,
            "margin-top": 0
          },
          to: {
            opacity: 0,
            "margin-top": -n + "px"
          },
          duration: .3
        });
      }

      function g(e) {
        var t = e.controller("mdInputContainer");
        return t.element;
      }

      function y(e) {
        return e.hasClass("md-input-messages-animation") ? e : e.hasClass("md-input-message-animation") ? t
          .element(w.getClosest(e, function(e) {
            return e.classList.contains("md-input-messages-animation");
          })) : t.element(e[0].querySelector(".md-input-messages-animation"));
      }

      function b(e, t, n, r) {
        _ = e, $ = t, w = n, T = r;
      }
      n.$inject = ["$mdTheming", "$parse"], i.$inject = ["$mdUtil", "$window", "$mdAria", "$timeout",
          "$mdGesture"
        ], o.$inject = ["$animate", "$mdUtil"], a.$inject = ["$compile"], u.$inject = ["$mdUtil"], s
        .$inject = ["$timeout"], l.$inject = ["$$AnimateRunner", "$animateCss", "$mdUtil", "$log"], d
        .$inject = ["$$AnimateRunner", "$animateCss", "$mdUtil", "$log"], f.$inject = ["$$AnimateRunner",
          "$animateCss", "$mdUtil", "$log"
        ];
      var E = t.module("material.components.input", ["material.core"]).directive("mdInputContainer", n)
        .directive("label", r).directive("input", i).directive("textarea", i).directive("mdMaxlength", o)
        .directive("placeholder", a).directive("ngMessages", c).directive("ngMessage", u).directive(
          "ngMessageExp", u).directive("mdSelectOnFocus", s).animation(".md-input-invalid", l).animation(
          ".md-input-messages-animation", d).animation(".md-input-message-animation", f);
      e._mdMocksIncluded && E.service("$$mdInput", function() {
        return {
          messages: {
            show: h,
            hide: p,
            getElement: y
          }
        };
      }).service("mdInputInvalidAnimation", l).service("mdInputMessagesAnimation", d).service(
        "mdInputMessageAnimation", f);
      var _,
        $,
        w,
        T,
        C = ["ngIf", "ngShow", "ngHide", "ngSwitchWhen", "ngSwitchDefault"];
    }(),
    function() {
      function e(e) {
        return {
          restrict: "E",
          compile: function(t) {
            return t[0].setAttribute("role", "list"), e;
          }
        };
      }

      function n(e, n, r, i) {
        var o = ["md-checkbox", "md-switch", "md-menu"];
        return {
          restrict: "E",
          controller: "MdListController",
          compile: function(a, s) {
            function c() {
              for (var e, t, n = ["md-switch", "md-checkbox"], r = 0; t = n[r]; ++r)
                if ((e = a.find(t)[0]) && !e.hasAttribute("aria-label")) {
                  var i = a.find("p")[0];
                  if (!i) return;
                  e.setAttribute("aria-label", "Toggle " + i.textContent);
                }
            }

            function u() {
              var e = t.element(b),
                n = e.parent().hasClass("md-secondary-container") || b.parentNode.firstElementChild !== b,
                r = "left";
              n && (r = "right"), e.attr("md-position-mode") || e.attr("md-position-mode", r + " target");
              var i = e.children().eq(0);
              v(i[0]) || i.attr("ng-click", "$mdMenu.open($event)"), i.attr("aria-label") || i.attr(
                "aria-label", "Open List Menu");
            }

            function l(n) {
              if ("div" == n) _ = t.element('<div class="md-no-style md-list-item-inner">'), _.append(a
                .contents()), a.addClass("md-proxy-focus");
              else {
                _ = t.element(
                  '<div class="md-button md-no-style">   <div class="md-list-item-inner"></div></div>');
                var r = t.element('<md-button class="md-no-style"></md-button>');
                h(a[0], r[0]), r.attr("aria-label") || r.attr("aria-label", e.getText(a)), a.hasClass(
                  "md-no-focus") && r.addClass("md-no-focus"), _.prepend(r), _.children().eq(1).append(a
                  .contents()), a.addClass("_md-button-wrap");
              }
              a[0].setAttribute("tabindex", "-1"), a.append(_);
            }

            function d() {
              var e = t.element('<div class="md-secondary-container">');
              t.forEach(E, function(t) {
                f(t, e);
              }), _.append(e);
            }

            function f(n, r) {
              if (n && !m(n) && n.hasAttribute("ng-click")) {
                e.expect(n, "aria-label");
                var i = t.element('<md-button class="md-secondary md-icon-button">');
                h(n, i[0], ["ng-if", "ng-hide", "ng-show"]), n.setAttribute("tabindex", "-1"), i.append(n),
                  n = i[0];
              }
              n && (!v(n) || !s.ngClick && p(n)) && t.element(n).removeClass("md-secondary"), a.addClass(
                "md-with-secondary"), r.append(n);
            }

            function h(e, n, i) {
              var o = r.prefixer(["ng-if", "ng-click", "ng-dblclick", "aria-label", "ng-disabled",
                "ui-sref", "href", "ng-href", "target", "ng-attr-ui-sref", "ui-sref-opts"
              ]);
              i && (o = o.concat(r.prefixer(i))), t.forEach(o, function(t) {
                e.hasAttribute(t) && (n.setAttribute(t, e.getAttribute(t)), e.removeAttribute(t));
              });
            }

            function p(e) {
              return o.indexOf(e.nodeName.toLowerCase()) != -1;
            }

            function m(e) {
              var t = e.nodeName.toUpperCase();
              return "MD-BUTTON" == t || "BUTTON" == t;
            }

            function v(e) {
              for (var t = e.attributes, n = 0; n < t.length; n++)
                if ("ngClick" === s.$normalize(t[n].name)) return !0;
              return !1;
            }

            function g(e, a, s, c) {
              function u() {
                h && h.children && !g && !y && t.forEach(o, function(e) {
                  t.forEach(h.querySelectorAll(e + ":not(.md-secondary)"), function(e) {
                    f.push(e);
                  });
                });
              }

              function l() {
                (1 == f.length || g) && (a.addClass("md-clickable"), g || c.attachRipple(e, t.element(a[0]
                  .querySelector(".md-no-style"))));
              }

              function d(e) {
                var t = ["md-slider"];
                if (!e.path) return t.indexOf(e.target.tagName.toLowerCase()) !== -1;
                for (var n = e.path.indexOf(a.children()[0]), r = 0; r < n; r++)
                  if (t.indexOf(e.path[r].tagName.toLowerCase()) !== -1) return !0;
              }
              a.addClass("_md");
              var f = [],
                h = a[0].firstElementChild,
                p = a.hasClass("_md-button-wrap"),
                m = p ? h.firstElementChild : h,
                g = m && v(m),
                y = a.hasClass("md-no-proxy");
              u(), l(), f.length && t.forEach(f, function(n) {
                n = t.element(n), e.mouseActive = !1, n.on("mousedown", function() {
                  e.mouseActive = !0, i(function() {
                    e.mouseActive = !1;
                  }, 100);
                }).on("focus", function() {
                  e.mouseActive === !1 && a.addClass("md-focused"), n.on("blur", function e() {
                    a.removeClass("md-focused"), n.off("blur", e);
                  });
                });
              });
              var b = function(e) {
                if ("INPUT" != e.target.nodeName && "TEXTAREA" != e.target.nodeName && !e.target
                  .isContentEditable) {
                  var t = e.which || e.keyCode;
                  t == n.KEY_CODE.SPACE && m && (m.click(), e.preventDefault(), e.stopPropagation());
                }
              };
              g || f.length || m && m.addEventListener("keypress", b), a.off("click"), a.off("keypress"),
                1 == f.length && m && a.children().eq(0).on("click", function(e) {
                  if (!d(e)) {
                    var n = r.getClosest(e.target, "BUTTON");
                    !n && m.contains(e.target) && t.forEach(f, function(n) {
                      e.target === n || n.contains(e.target) || ("MD-MENU" === n.nodeName && (n = n
                        .children[0]), t.element(n).triggerHandler("click"));
                    });
                  }
                }), e.$on("$destroy", function() {
                  m && m.removeEventListener("keypress", b);
                });
            }
            var y,
              b,
              E = a[0].querySelectorAll(".md-secondary"),
              _ = a;
            if (a[0].setAttribute("role", "listitem"), s.ngClick || s.ngDblclick || s.ngHref || s.href || s
              .uiSref || s.ngAttrUiSref) l("button");
            else if (!a.hasClass("md-no-proxy")) {
              for (var $, w = 0; $ = o[w]; ++w)
                if (b = a[0].querySelector($)) {
                  y = !0;
                  break;
                }
              y ? l("div") : a.addClass("md-no-proxy");
            }
            return d(), c(), y && "MD-MENU" === b.nodeName && u(), g;
          }
        };
      }

      function r(e, t, n) {
        function r(e, t) {
          var r = {};
          n.attach(e, t, r);
        }
        var i = this;
        i.attachRipple = r;
      }
      r.$inject = ["$scope", "$element", "$mdListInkRipple"], e.$inject = ["$mdTheming"], n.$inject = [
          "$mdAria", "$mdConstant", "$mdUtil", "$timeout"
        ], t.module("material.components.list", ["material.core"]).controller("MdListController", r)
        .directive("mdList", e).directive("mdListItem", n);
    }(),
    function() {
      function e() {
        return {
          definePreset: r,
          getAllPresets: i,
          clearPresets: o,
          $get: a()
        };
      }

      function r(e, t) {
        if (!e || !t) throw new Error(
          "mdPanelProvider: The panel preset definition is malformed. The name and preset object are required."
          );
        if (v.hasOwnProperty(e)) throw new Error(
          "mdPanelProvider: The panel preset you have requested has already been defined.");
        delete t.id, delete t.position, delete t.animation, v[e] = t;
      }

      function i() {
        return t.copy(v);
      }

      function o() {
        v = {};
      }

      function a() {
        return ["$rootElement", "$rootScope", "$injector", "$window", function(e, t, n, r) {
          return new s(v, e, t, n, r);
        }];
      }

      function s(e, n, r, i, o) {
        this._defaultConfigOptions = {
            bindToController: !0,
            clickOutsideToClose: !1,
            disableParentScroll: !1,
            escapeToClose: !1,
            focusOnOpen: !0,
            fullscreen: !1,
            hasBackdrop: !1,
            propagateContainerEvents: !1,
            transformTemplate: t.bind(this, this._wrapTemplate),
            trapFocus: !1,
            zIndex: h
          }, this._config = {}, this._presets = e, this._$rootElement = n, this._$rootScope = r, this
          ._$injector = i, this._$window = o, this._$mdUtil = this._$injector.get("$mdUtil"), this
          ._trackedPanels = {}, this._groups = Object.create(null), this.animation = l.animation, this
          .xPosition = u.xPosition, this.yPosition = u.yPosition, this.interceptorTypes = c.interceptorTypes,
          this.closeReasons = c.closeReasons, this.absPosition = u.absPosition;
      }

      function c(e, t) {
        this._$q = t.get("$q"), this._$mdCompiler = t.get("$mdCompiler"), this._$mdConstant = t.get(
            "$mdConstant"), this._$mdUtil = t.get("$mdUtil"), this._$mdTheming = t.get("$mdTheming"), this
          ._$rootScope = t.get("$rootScope"), this._$animate = t.get("$animate"), this._$mdPanel = t.get(
            "$mdPanel"), this._$log = t.get("$log"), this._$window = t.get("$window"), this._$$rAF = t.get(
            "$$rAF"), this.id = e.id, this.config = e, this.panelContainer, this.panelEl, this.isAttached = !
          1, this._removeListeners = [], this._topFocusTrap, this._bottomFocusTrap, this._backdropRef, this
          ._restoreScroll = null, this._interceptors = Object.create(null), this._compilerCleanup = null, this
          ._restoreCache = {
            styles: "",
            classes: ""
          };
      }

      function u(e) {
        this._$window = e.get("$window"), this._isRTL = "rtl" === e.get("$mdUtil").bidi(), this._$mdConstant =
          e.get("$mdConstant"), this._absolute = !1, this._relativeToEl, this._top = "", this._bottom = "",
          this._left = "", this._right = "", this._translateX = [], this._translateY = [], this
          ._positions = [], this._actualPosition;
      }

      function l(e) {
        this._$mdUtil = e.get("$mdUtil"), this._openFrom, this._closeTo, this._animationClass = "", this
          ._openDuration, this._closeDuration, this._rawDuration;
      }

      function d(e) {
        var n = t.isString(e) ? document.querySelector(e) : e;
        return t.element(n);
      }

      function f(e, t) {
        var n = getComputedStyle(e[0] || e)[t],
          r = n.indexOf("("),
          i = n.lastIndexOf(")"),
          o = {
            x: 0,
            y: 0
          };
        if (r > -1 && i > -1) {
          var a = n.substring(r + 1, i).split(", ").slice(-2);
          o.x = parseInt(a[0]), o.y = parseInt(a[1]);
        }
        return o;
      }
      s.$inject = ["presets", "$rootElement", "$rootScope", "$injector", "$window"], t.module(
        "material.components.panel", ["material.core", "material.components.backdrop"]).provider("$mdPanel",
        e);
      var h = 80,
        p = "_md-panel-hidden",
        m = t.element('<div class="_md-panel-focus-trap" tabindex="0"></div>'),
        v = {};
      s.prototype.create = function(e, n) {
        if ("string" == typeof e ? e = this._getPresetByName(e) : "object" != typeof e || !t.isUndefined(
          n) && n || (n = e, e = {}), e = e || {}, n = n || {}, t.isDefined(n.id) && this._trackedPanels[n
            .id]) {
          var r = this._trackedPanels[n.id];
          return t.extend(r.config, n), r;
        }
        this._config = t.extend({
          id: n.id || "panel_" + this._$mdUtil.nextUid(),
          scope: this._$rootScope.$new(!0),
          attachTo: this._$rootElement
        }, this._defaultConfigOptions, n, e);
        var i = new c(this._config, this._$injector);
        return this._trackedPanels[n.id] = i, this._config.groupName && (t.isString(this._config
          .groupName) && (this._config.groupName = [this._config.groupName]), t.forEach(this._config
            .groupName,
            function(e) {
              i.addToGroup(e);
            })), this._config.scope.$on("$destroy", t.bind(i, i.detach)), i;
      }, s.prototype.open = function(e, t) {
        var n = this.create(e, t);
        return n.open().then(function() {
          return n;
        });
      }, s.prototype._getPresetByName = function(e) {
        if (!this._presets[e]) throw new Error(
          "mdPanel: The panel preset configuration that you requested does not exist. Use the $mdPanelProvider to create a preset before requesting one."
          );
        return this._presets[e];
      }, s.prototype.newPanelPosition = function() {
        return new u(this._$injector);
      }, s.prototype.newPanelAnimation = function() {
        return new l(this._$injector);
      }, s.prototype.newPanelGroup = function(e, t) {
        if (!this._groups[e]) {
          t = t || {};
          var n = {
            panels: [],
            openPanels: [],
            maxOpen: t.maxOpen > 0 ? t.maxOpen : 1 / 0
          };
          this._groups[e] = n;
        }
        return this._groups[e];
      }, s.prototype.setGroupMaxOpen = function(e, t) {
        if (!this._groups[e]) throw new Error("mdPanel: Group does not exist yet. Call newPanelGroup().");
        this._groups[e].maxOpen = t;
      }, s.prototype._openCountExceedsMaxOpen = function(e) {
        if (this._groups[e]) {
          var t = this._groups[e];
          return t.maxOpen > 0 && t.openPanels.length > t.maxOpen;
        }
        return !1;
      }, s.prototype._closeFirstOpenedPanel = function(e) {
        this._groups[e].openPanels[0].close();
      }, s.prototype._wrapTemplate = function(e) {
        var t = e || "";
        return '<div class="md-panel-outer-wrapper">  <div class="md-panel" style="left: -9999px;">' + t +
          "</div></div>";
      }, s.prototype._wrapContentElement = function(e) {
        var n = t.element('<div class="md-panel-outer-wrapper">');
        return e.addClass("md-panel").css("left", "-9999px"), n.append(e), n;
      }, c.interceptorTypes = {
        CLOSE: "onClose"
      }, c.prototype.open = function() {
        var e = this;
        return this._$q(function(n, r) {
          var i = e._done(n, e),
            o = e._simpleBind(e.show, e),
            a = function() {
              e.config.groupName && t.forEach(e.config.groupName, function(t) {
                e._$mdPanel._openCountExceedsMaxOpen(t) && e._$mdPanel._closeFirstOpenedPanel(t);
              });
            };
          e.attach().then(o).then(a).then(i).catch(r);
        });
      }, c.prototype.close = function(e) {
        var n = this;
        return this._$q(function(r, i) {
          n._callInterceptors(c.interceptorTypes.CLOSE).then(function() {
            var o = n._done(r, n),
              a = n._simpleBind(n.detach, n),
              s = n.config.onCloseSuccess || t.noop;
            s = t.bind(n, s, n, e), n.hide().then(a).then(o).then(s).catch(i);
          }, i);
        });
      }, c.prototype.attach = function() {
        if (this.isAttached && this.panelEl) return this._$q.when(this);
        var e = this;
        return this._$q(function(n, r) {
          var i = e._done(n, e),
            o = e.config.onDomAdded || t.noop,
            a = function(t) {
              return e.isAttached = !0, e._addEventListeners(), t;
            };
          e._$q.all([e._createBackdrop(), e._createPanel().then(a).catch(r)]).then(o).then(i).catch(r);
        });
      }, c.prototype.detach = function() {
        if (!this.isAttached) return this._$q.when(this);
        var e = this,
          n = e.config.onDomRemoved || t.noop,
          r = function() {
            return e._removeEventListeners(), e._topFocusTrap && e._topFocusTrap.parentNode && e
              ._topFocusTrap.parentNode.removeChild(e._topFocusTrap), e._bottomFocusTrap && e
              ._bottomFocusTrap.parentNode && e._bottomFocusTrap.parentNode.removeChild(e._bottomFocusTrap),
              e._restoreCache.classes && (e.panelEl[0].className = e._restoreCache.classes), e.panelEl[0]
              .style.cssText = e._restoreCache.styles || "", e._compilerCleanup(), e.panelContainer
            .remove(), e.isAttached = !1, e._$q.when(e);
          };
        return this._restoreScroll && (this._restoreScroll(), this._restoreScroll = null), this._$q(
          function(t, i) {
            var o = e._done(t, e);
            e._$q.all([r(), !e._backdropRef || e._backdropRef.detach()]).then(n).then(o).catch(i);
          });
      }, c.prototype.destroy = function() {
        var e = this;
        this.config.groupName && t.forEach(this.config.groupName, function(t) {
          e.removeFromGroup(t);
        }), this.config.scope.$destroy(), this.config.locals = null, this._interceptors = null;
      }, c.prototype.show = function() {
        if (!this.panelContainer) return this._$q(function(e, t) {
          t("mdPanel: Panel does not exist yet. Call open() or attach().");
        });
        if (!this.panelContainer.hasClass(p)) return this._$q.when(this);
        var e = this,
          n = function() {
            return e.panelContainer.removeClass(p), e._animateOpen();
          };
        return this._$q(function(r, i) {
          var o = e._done(r, e),
            a = e.config.onOpenComplete || t.noop,
            s = function() {
              e.config.groupName && t.forEach(e.config.groupName, function(t) {
                e._$mdPanel._groups[t].openPanels.push(e);
              });
            };
          e._$q.all([e._backdropRef ? e._backdropRef.show() : e, n().then(function() {
            e._focusOnOpen();
          }, i)]).then(a).then(s).then(o).catch(i);
        });
      }, c.prototype.hide = function() {
        if (!this.panelContainer) return this._$q(function(e, t) {
          t("mdPanel: Panel does not exist yet. Call open() or attach().");
        });
        if (this.panelContainer.hasClass(p)) return this._$q.when(this);
        var e = this;
        return this._$q(function(n, r) {
          var i = e._done(n, e),
            o = e.config.onRemoving || t.noop,
            a = function() {
              e.panelContainer.addClass(p);
            },
            s = function() {
              if (e.config.groupName) {
                var n;
                t.forEach(e.config.groupName, function(t) {
                  t = e._$mdPanel._groups[t], n = t.openPanels.indexOf(e), n > -1 && t.openPanels
                    .splice(n, 1);
                });
              }
            },
            c = function() {
              var t = e.config.origin;
              t && d(t).focus();
            };
          e._$q.all([e._backdropRef ? e._backdropRef.hide() : e, e._animateClose().then(o).then(a).then(
            s).then(c).catch(r)]).then(i, r);
        });
      }, c.prototype.addClass = function(e, t) {
        if (this._$log.warn(
            "mdPanel: The addClass method is in the process of being deprecated. Full deprecation is scheduled for the Angular Material 1.2 release. To achieve the same results, use the panelContainer or panelEl JQLite elements that are referenced in MdPanelRef."
            ), !this.panelContainer) throw new Error(
          "mdPanel: Panel does not exist yet. Call open() or attach().");
        t || this.panelContainer.hasClass(e) ? t && !this.panelEl.hasClass(e) && this.panelEl.addClass(e) :
          this.panelContainer.addClass(e);
      }, c.prototype.removeClass = function(e, t) {
        if (this._$log.warn(
            "mdPanel: The removeClass method is in the process of being deprecated. Full deprecation is scheduled for the Angular Material 1.2 release. To achieve the same results, use the panelContainer or panelEl JQLite elements that are referenced in MdPanelRef."
            ), !this.panelContainer) throw new Error(
          "mdPanel: Panel does not exist yet. Call open() or attach().");
        !t && this.panelContainer.hasClass(e) ? this.panelContainer.removeClass(e) : t && this.panelEl
          .hasClass(e) && this.panelEl.removeClass(e);
      }, c.prototype.toggleClass = function(e, t) {
        if (this._$log.warn(
            "mdPanel: The toggleClass method is in the process of being deprecated. Full deprecation is scheduled for the Angular Material 1.2 release. To achieve the same results, use the panelContainer or panelEl JQLite elements that are referenced in MdPanelRef."
            ), !this.panelContainer) throw new Error(
          "mdPanel: Panel does not exist yet. Call open() or attach().");
        t ? this.panelEl.toggleClass(e) : this.panelContainer.toggleClass(e);
      }, c.prototype._compile = function() {
        var e = this;
        return e._$mdCompiler.compile(e.config).then(function(n) {
          var r = e.config;
          if (r.contentElement) {
            var i = n.element;
            e._restoreCache.styles = i[0].style.cssText, e._restoreCache.classes = i[0].className, e
              .panelContainer = e._$mdPanel._wrapContentElement(i), e.panelEl = i;
          } else e.panelContainer = n.link(r.scope), e.panelEl = t.element(e.panelContainer[0]
            .querySelector(".md-panel"));
          return e._compilerCleanup = n.cleanup, d(e.config.attachTo).append(e.panelContainer), e;
        });
      }, c.prototype._createPanel = function() {
        var e = this;
        return this._$q(function(t, n) {
          e.config.locals || (e.config.locals = {}), e.config.locals.mdPanelRef = e, e._compile().then(
            function() {
              e.config.disableParentScroll && (e._restoreScroll = e._$mdUtil.disableScrollAround(null,
                  e.panelContainer, {
                    disableScrollMask: !0
                  })), e.config.panelClass && e.panelEl.addClass(e.config.panelClass), e.config
                .propagateContainerEvents && e.panelContainer.css("pointer-events", "none"), e
                ._$animate.pin && e._$animate.pin(e.panelContainer, d(e.config.attachTo)), e
                ._configureTrapFocus(), e._addStyles().then(function() {
                  t(e);
                }, n);
            }, n);
        });
      }, c.prototype._addStyles = function() {
        var e = this;
        return this._$q(function(t) {
          e.panelContainer.css("z-index", e.config.zIndex), e.panelEl.css("z-index", e.config.zIndex +
            1);
          var n = function() {
            e._setTheming(), e.panelEl.css("left", ""), e.panelContainer.addClass(p), t(e);
          };
          if (e.config.fullscreen) return e.panelEl.addClass("_md-panel-fullscreen"), void n();
          var r = e.config.position;
          return r ? void e._$rootScope.$$postDigest(function() {
            e._updatePosition(!0), e._setTheming(), t(e);
          }) : void n();
        });
      }, c.prototype._setTheming = function() {
        this._$mdTheming(this.panelEl), this._$mdTheming(this.panelContainer);
      }, c.prototype.updatePosition = function(e) {
        if (!this.panelContainer) throw new Error(
          "mdPanel: Panel does not exist yet. Call open() or attach().");
        this.config.position = e, this._updatePosition();
      }, c.prototype._updatePosition = function(e) {
        var t = this.config.position;
        t && (t._setPanelPosition(this.panelEl), e && this.panelContainer.addClass(p), this.panelEl.css(u
            .absPosition.TOP, t.getTop()), this.panelEl.css(u.absPosition.BOTTOM, t.getBottom()), this
          .panelEl.css(u.absPosition.LEFT, t.getLeft()), this.panelEl.css(u.absPosition.RIGHT, t
          .getRight()));
      }, c.prototype._focusOnOpen = function() {
        if (this.config.focusOnOpen) {
          var e = this;
          this._$rootScope.$$postDigest(function() {
            var t = e._$mdUtil.findFocusTarget(e.panelEl) || e.panelEl;
            t.focus();
          });
        }
      }, c.prototype._createBackdrop = function() {
        if (this.config.hasBackdrop) {
          if (!this._backdropRef) {
            var e = this._$mdPanel.newPanelAnimation().openFrom(this.config.attachTo).withAnimation({
              open: "_md-opaque-enter",
              close: "_md-opaque-leave"
            });
            this.config.animation && e.duration(this.config.animation._rawDuration);
            var t = {
              animation: e,
              attachTo: this.config.attachTo,
              focusOnOpen: !1,
              panelClass: "_md-panel-backdrop",
              zIndex: this.config.zIndex - 1
            };
            this._backdropRef = this._$mdPanel.create(t);
          }
          if (!this._backdropRef.isAttached) return this._backdropRef.attach();
        }
      }, c.prototype._addEventListeners = function() {
        this._configureEscapeToClose(), this._configureClickOutsideToClose(), this
        ._configureScrollListener();
      }, c.prototype._removeEventListeners = function() {
        this._removeListeners && this._removeListeners.forEach(function(e) {
          e();
        }), this._removeListeners = [];
      }, c.prototype._configureEscapeToClose = function() {
        if (this.config.escapeToClose) {
          var e = d(this.config.attachTo),
            t = this,
            n = function(e) {
              e.keyCode === t._$mdConstant.KEY_CODE.ESCAPE && (e.stopPropagation(), e.preventDefault(), t
                .close(c.closeReasons.ESCAPE));
            };
          this.panelContainer.on("keydown", n), e.on("keydown", n), this._removeListeners.push(function() {
            t.panelContainer.off("keydown", n), e.off("keydown", n);
          });
        }
      }, c.prototype._configureClickOutsideToClose = function() {
        if (this.config.clickOutsideToClose) {
          var e,
            n = this.config.propagateContainerEvents ? t.element(document.body) : this.panelContainer,
            r = function(t) {
              e = t.target;
            },
            i = this,
            o = function(t) {
              i.config.propagateContainerEvents ? e === i.panelEl[0] || i.panelEl[0].contains(e) || i
              .close() : e === n[0] && t.target === n[0] && (t.stopPropagation(), t.preventDefault(), i
                .close(c.closeReasons.CLICK_OUTSIDE));
            };
          n.on("mousedown", r), n.on("mouseup", o), this._removeListeners.push(function() {
            n.off("mousedown", r), n.off("mouseup", o);
          });
        }
      }, c.prototype._configureScrollListener = function() {
        if (!this.config.disableParentScroll) {
          var e = t.bind(this, this._updatePosition),
            n = this._$$rAF.throttle(e),
            r = this,
            i = function() {
              n();
            };
          this._$window.addEventListener("scroll", i, !0), this._removeListeners.push(function() {
            r._$window.removeEventListener("scroll", i, !0);
          });
        }
      }, c.prototype._configureTrapFocus = function() {
        if (this.panelEl.attr("tabIndex", "-1"), this.config.trapFocus) {
          var e = this.panelEl;
          this._topFocusTrap = m.clone()[0], this._bottomFocusTrap = m.clone()[0];
          var t = function() {
            e.focus();
          };
          this._topFocusTrap.addEventListener("focus", t), this._bottomFocusTrap.addEventListener("focus",
            t), this._removeListeners.push(this._simpleBind(function() {
            this._topFocusTrap.removeEventListener("focus", t), this._bottomFocusTrap
              .removeEventListener("focus", t);
          }, this)), e[0].parentNode.insertBefore(this._topFocusTrap, e[0]), e.after(this
            ._bottomFocusTrap);
        }
      }, c.prototype.updateAnimation = function(e) {
        this.config.animation = e, this._backdropRef && this._backdropRef.config.animation.duration(e
          ._rawDuration);
      }, c.prototype._animateOpen = function() {
        this.panelContainer.addClass("md-panel-is-showing");
        var e = this.config.animation;
        if (!e) return this.panelContainer.addClass("_md-panel-shown"), this._$q.when(this);
        var t = this;
        return this._$q(function(n) {
          var r = t._done(n, t),
            i = function() {
              t._$log.warn("mdPanel: MdPanel Animations failed. Showing panel without animating."), r();
            };
          e.animateOpen(t.panelEl).then(r, i);
        });
      }, c.prototype._animateClose = function() {
        var e = this.config.animation;
        if (!e) return this.panelContainer.removeClass("md-panel-is-showing"), this.panelContainer
          .removeClass("_md-panel-shown"), this._$q.when(this);
        var t = this;
        return this._$q(function(n) {
          var r = function() {
              t.panelContainer.removeClass("md-panel-is-showing"), n(t);
            },
            i = function() {
              t._$log.warn("mdPanel: MdPanel Animations failed. Hiding panel without animating."), r();
            };
          e.animateClose(t.panelEl).then(r, i);
        });
      }, c.prototype.registerInterceptor = function(e, n) {
        var r = null;
        if (t.isString(e) ? t.isFunction(n) || (r =
            "Interceptor callback must be a function, instead got " + typeof n) : r =
          "Interceptor type must be a string, instead got " + typeof e, r) throw new Error("MdPanel: " + r);
        var i = this._interceptors[e] = this._interceptors[e] || [];
        return i.indexOf(n) === -1 && i.push(n), this;
      }, c.prototype.removeInterceptor = function(e, t) {
        var n = this._interceptors[e] ? this._interceptors[e].indexOf(t) : -1;
        return n > -1 && this._interceptors[e].splice(n, 1), this;
      }, c.prototype.removeAllInterceptors = function(e) {
        return e ? this._interceptors[e] = [] : this._interceptors = Object.create(null), this;
      }, c.prototype._callInterceptors = function(e) {
        var n = this,
          r = n._$q,
          i = n._interceptors && n._interceptors[e] || [];
        return i.reduceRight(function(e, i) {
          var o = i && t.isFunction(i.then),
            a = o ? i : null;
          return e.then(function() {
            if (!a) try {
              a = i(n);
            } catch (e) {
              a = r.reject(e);
            }
            return a;
          });
        }, r.resolve(n));
      }, c.prototype._simpleBind = function(e, t) {
        return function(n) {
          return e.apply(t, n);
        };
      }, c.prototype._done = function(e, t) {
        return function() {
          e(t);
        };
      }, c.prototype.addToGroup = function(e) {
        this._$mdPanel._groups[e] || this._$mdPanel.newPanelGroup(e);
        var t = this._$mdPanel._groups[e],
          n = t.panels.indexOf(this);
        n < 0 && t.panels.push(this);
      }, c.prototype.removeFromGroup = function(e) {
        if (!this._$mdPanel._groups[e]) throw new Error("mdPanel: The group " + e + " does not exist.");
        var t = this._$mdPanel._groups[e],
          n = t.panels.indexOf(this);
        n > -1 && t.panels.splice(n, 1);
      }, c.closeReasons = {
        CLICK_OUTSIDE: "clickOutsideToClose",
        ESCAPE: "escapeToClose"
      }, u.xPosition = {
        CENTER: "center",
        ALIGN_START: "align-start",
        ALIGN_END: "align-end",
        OFFSET_START: "offset-start",
        OFFSET_END: "offset-end"
      }, u.yPosition = {
        CENTER: "center",
        ALIGN_TOPS: "align-tops",
        ALIGN_BOTTOMS: "align-bottoms",
        ABOVE: "above",
        BELOW: "below"
      }, u.absPosition = {
        TOP: "top",
        RIGHT: "right",
        BOTTOM: "bottom",
        LEFT: "left"
      }, u.viewportMargin = 8, u.prototype.absolute = function() {
        return this._absolute = !0, this;
      }, u.prototype._setPosition = function(e, n) {
        if (e === u.absPosition.RIGHT || e === u.absPosition.LEFT) this._left = this._right = "";
        else {
          if (e !== u.absPosition.BOTTOM && e !== u.absPosition.TOP) {
            var r = Object.keys(u.absPosition).join().toLowerCase();
            throw new Error("mdPanel: Position must be one of " + r + ".");
          }
          this._top = this._bottom = "";
        }
        return this["_" + e] = t.isString(n) ? n : "0", this;
      }, u.prototype.top = function(e) {
        return this._setPosition(u.absPosition.TOP, e);
      }, u.prototype.bottom = function(e) {
        return this._setPosition(u.absPosition.BOTTOM, e);
      }, u.prototype.start = function(e) {
        var t = this._isRTL ? u.absPosition.RIGHT : u.absPosition.LEFT;
        return this._setPosition(t, e);
      }, u.prototype.end = function(e) {
        var t = this._isRTL ? u.absPosition.LEFT : u.absPosition.RIGHT;
        return this._setPosition(t, e);
      }, u.prototype.left = function(e) {
        return this._setPosition(u.absPosition.LEFT, e);
      }, u.prototype.right = function(e) {
        return this._setPosition(u.absPosition.RIGHT, e);
      }, u.prototype.centerHorizontally = function() {
        return this._left = "50%", this._right = "", this._translateX = ["-50%"], this;
      }, u.prototype.centerVertically = function() {
        return this._top = "50%", this._bottom = "", this._translateY = ["-50%"], this;
      }, u.prototype.center = function() {
        return this.centerHorizontally().centerVertically();
      }, u.prototype.relativeTo = function(e) {
        return this._absolute = !1, this._relativeToEl = d(e), this;
      }, u.prototype.addPanelPosition = function(e, t) {
        if (!this._relativeToEl) throw new Error(
          "mdPanel: addPanelPosition can only be used with relative positioning. Set relativeTo first.");
        return this._validateXPosition(e), this._validateYPosition(t), this._positions.push({
          x: e,
          y: t
        }), this;
      }, u.prototype._validateYPosition = function(e) {
        if (null != e) {
          for (var t, n = Object.keys(u.yPosition), r = [], i = 0; t = n[i]; i++) {
            var o = u.yPosition[t];
            if (r.push(o), o === e) return;
          }
          throw new Error("mdPanel: Panel y position only accepts the following values:\n" + r.join(" | "));
        }
      }, u.prototype._validateXPosition = function(e) {
        if (null != e) {
          for (var t, n = Object.keys(u.xPosition), r = [], i = 0; t = n[i]; i++) {
            var o = u.xPosition[t];
            if (r.push(o), o === e) return;
          }
          throw new Error("mdPanel: Panel x Position only accepts the following values:\n" + r.join(" | "));
        }
      }, u.prototype.withOffsetX = function(e) {
        return this._translateX.push(e), this;
      }, u.prototype.withOffsetY = function(e) {
        return this._translateY.push(e), this;
      }, u.prototype.getTop = function() {
        return this._top;
      }, u.prototype.getBottom = function() {
        return this._bottom;
      }, u.prototype.getLeft = function() {
        return this._left;
      }, u.prototype.getRight = function() {
        return this._right;
      }, u.prototype.getTransform = function() {
        var e = this._reduceTranslateValues("translateX", this._translateX),
          t = this._reduceTranslateValues("translateY", this._translateY);
        return (e + " " + t).trim();
      }, u.prototype._setTransform = function(e) {
        return e.css(this._$mdConstant.CSS.TRANSFORM, this.getTransform());
      }, u.prototype._isOnscreen = function(e) {
        var t = parseInt(this.getLeft()),
          n = parseInt(this.getTop());
        if (this._translateX.length || this._translateY.length) {
          var r = this._$mdConstant.CSS.TRANSFORM,
            i = f(e, r);
          t += i.x, n += i.y;
        }
        var o = t + e[0].offsetWidth,
          a = n + e[0].offsetHeight;
        return t >= 0 && n >= 0 && a <= this._$window.innerHeight && o <= this._$window.innerWidth;
      }, u.prototype.getActualPosition = function() {
        return this._actualPosition;
      }, u.prototype._reduceTranslateValues = function(e, n) {
        return n.map(function(n) {
          var r = t.isFunction(n) ? n(this) : n;
          return e + "(" + r + ")";
        }, this).join(" ");
      }, u.prototype._setPanelPosition = function(e) {
        if (e.removeClass("_md-panel-position-adjusted"), this._absolute) return void this._setTransform(e);
        if (this._actualPosition) return this._calculatePanelPosition(e, this._actualPosition), this
          ._setTransform(e), void this._constrainToViewport(e);
        for (var t = 0; t < this._positions.length; t++)
          if (this._actualPosition = this._positions[t], this._calculatePanelPosition(e, this
              ._actualPosition), this._setTransform(e), this._isOnscreen(e)) return;
        this._constrainToViewport(e);
      }, u.prototype._constrainToViewport = function(e) {
        var t = u.viewportMargin,
          n = this._top,
          r = this._left;
        if (this.getTop()) {
          var i = parseInt(this.getTop()),
            o = e[0].offsetHeight + i,
            a = this._$window.innerHeight;
          i < t ? this._top = t + "px" : o > a && (this._top = i - (o - a + t) + "px");
        }
        if (this.getLeft()) {
          var s = parseInt(this.getLeft()),
            c = e[0].offsetWidth + s,
            l = this._$window.innerWidth;
          s < t ? this._left = t + "px" : c > l && (this._left = s - (c - l + t) + "px");
        }
        e.toggleClass("_md-panel-position-adjusted", this._top !== n || this._left !== r);
      }, u.prototype._reverseXPosition = function(e) {
        if (e !== u.xPosition.CENTER) {
          var t = "start",
            n = "end";
          return e.indexOf(t) > -1 ? e.replace(t, n) : e.replace(n, t);
        }
      }, u.prototype._bidi = function(e) {
        return this._isRTL ? this._reverseXPosition(e) : e;
      }, u.prototype._calculatePanelPosition = function(e, t) {
        var n = e[0].getBoundingClientRect(),
          r = n.width,
          i = n.height,
          o = this._relativeToEl[0].getBoundingClientRect(),
          a = o.left,
          s = o.right,
          c = o.width;
        switch (this._bidi(t.x)) {
          case u.xPosition.OFFSET_START:
            this._left = a - r + "px";
            break;
          case u.xPosition.ALIGN_END:
            this._left = s - r + "px";
            break;
          case u.xPosition.CENTER:
            var l = a + .5 * c - .5 * r;
            this._left = l + "px";
            break;
          case u.xPosition.ALIGN_START:
            this._left = a + "px";
            break;
          case u.xPosition.OFFSET_END:
            this._left = s + "px";
        }
        var d = o.top,
          f = o.bottom,
          h = o.height;
        switch (t.y) {
          case u.yPosition.ABOVE:
            this._top = d - i + "px";
            break;
          case u.yPosition.ALIGN_BOTTOMS:
            this._top = f - i + "px";
            break;
          case u.yPosition.CENTER:
            var p = d + .5 * h - .5 * i;
            this._top = p + "px";
            break;
          case u.yPosition.ALIGN_TOPS:
            this._top = d + "px";
            break;
          case u.yPosition.BELOW:
            this._top = f + "px";
        }
      }, l.animation = {
        SLIDE: "md-panel-animate-slide",
        SCALE: "md-panel-animate-scale",
        FADE: "md-panel-animate-fade"
      }, l.prototype.openFrom = function(e) {
        return e = e.target ? e.target : e, this._openFrom = this._getPanelAnimationTarget(e), this
          ._closeTo || (this._closeTo = this._openFrom), this;
      }, l.prototype.closeTo = function(e) {
        return this._closeTo = this._getPanelAnimationTarget(e), this;
      }, l.prototype.duration = function(e) {
        function n(e) {
          if (t.isNumber(e)) return e / 1e3;
        }
        return e && (t.isNumber(e) ? this._openDuration = this._closeDuration = n(e) : t.isObject(e) && (
          this._openDuration = n(e.open), this._closeDuration = n(e.close))), this._rawDuration = e, this;
      }, l.prototype._getPanelAnimationTarget = function(e) {
        return t.isDefined(e.top) || t.isDefined(e.left) ? {
          element: n,
          bounds: {
            top: e.top || 0,
            left: e.left || 0
          }
        } : this._getBoundingClientRect(d(e));
      }, l.prototype.withAnimation = function(e) {
        return this._animationClass = e, this;
      }, l.prototype.animateOpen = function(e) {
        var n = this._$mdUtil.dom.animator;
        this._fixBounds(e);
        var r = {},
          i = e[0].style.transform || "",
          o = n.toTransformCss(i),
          a = n.toTransformCss(i);
        switch (this._animationClass) {
          case l.animation.SLIDE:
            e.css("opacity", "1"), r = {
              transitionInClass: "_md-panel-animate-enter"
            };
            var s = n.calculateSlideToOrigin(e, this._openFrom) || "";
            o = n.toTransformCss(s + " " + i);
            break;
          case l.animation.SCALE:
            r = {
              transitionInClass: "_md-panel-animate-enter"
            };
            var c = n.calculateZoomToOrigin(e, this._openFrom) || "";
            o = n.toTransformCss(c + " " + i);
            break;
          case l.animation.FADE:
            r = {
              transitionInClass: "_md-panel-animate-enter"
            };
            break;
          default:
            r = t.isString(this._animationClass) ? {
              transitionInClass: this._animationClass
            } : {
              transitionInClass: this._animationClass.open,
              transitionOutClass: this._animationClass.close
            };
        }
        return r.duration = this._openDuration, n.translate3d(e, o, a, r);
      }, l.prototype.animateClose = function(e) {
        var n = this._$mdUtil.dom.animator,
          r = {},
          i = e[0].style.transform || "",
          o = n.toTransformCss(i),
          a = n.toTransformCss(i);
        switch (this._animationClass) {
          case l.animation.SLIDE:
            e.css("opacity", "1"), r = {
              transitionInClass: "_md-panel-animate-leave"
            };
            var s = n.calculateSlideToOrigin(e, this._closeTo) || "";
            a = n.toTransformCss(s + " " + i);
            break;
          case l.animation.SCALE:
            r = {
              transitionInClass: "_md-panel-animate-scale-out _md-panel-animate-leave"
            };
            var c = n.calculateZoomToOrigin(e, this._closeTo) || "";
            a = n.toTransformCss(c + " " + i);
            break;
          case l.animation.FADE:
            r = {
              transitionInClass: "_md-panel-animate-fade-out _md-panel-animate-leave"
            };
            break;
          default:
            r = t.isString(this._animationClass) ? {
              transitionOutClass: this._animationClass
            } : {
              transitionInClass: this._animationClass.close,
              transitionOutClass: this._animationClass.open
            };
        }
        return r.duration = this._closeDuration, n.translate3d(e, o, a, r);
      }, l.prototype._fixBounds = function(e) {
        var t = e[0].offsetWidth,
          n = e[0].offsetHeight;
        this._openFrom && null == this._openFrom.bounds.height && (this._openFrom.bounds.height = n), this
          ._openFrom && null == this._openFrom.bounds.width && (this._openFrom.bounds.width = t), this
          ._closeTo && null == this._closeTo.bounds.height && (this._closeTo.bounds.height = n), this
          ._closeTo && null == this._closeTo.bounds.width && (this._closeTo.bounds.width = t);
      }, l.prototype._getBoundingClientRect = function(e) {
        if (e instanceof t.element) return {
          element: e,
          bounds: e[0].getBoundingClientRect()
        };
      };
    }(),
    function() {
      t.module("material.components.menuBar", ["material.core", "material.components.icon",
        "material.components.menu"
      ]);
    }(),
    function() {
      t.module("material.components.menu", ["material.core", "material.components.backdrop"]);
    }(),
    function() {
      function e(e, n) {
        return {
          restrict: "E",
          transclude: !0,
          controller: r,
          controllerAs: "ctrl",
          bindToController: !0,
          scope: {
            mdSelectedNavItem: "=?",
            mdNoInkBar: "=?",
            navBarAriaLabel: "@?"
          },
          template: '<div class="md-nav-bar"><nav role="navigation"><ul class="_md-nav-bar-list" ng-transclude role="listbox"tabindex="0"ng-focus="ctrl.onFocus()"ng-keydown="ctrl.onKeydown($event)"aria-label="{{ctrl.navBarAriaLabel}}"></ul></nav><md-nav-ink-bar ng-hide="ctrl.mdNoInkBar"></md-nav-ink-bar></div>',
          link: function(r, i, o, a) {
            n(i), a.navBarAriaLabel || e.expectAsync(i, "aria-label", t.noop);
          }
        };
      }

      function r(e, t, n, r) {
        this._$timeout = n, this._$scope = t, this._$mdConstant = r, this.mdSelectedNavItem, this
          .navBarAriaLabel, this._navBarEl = e[0], this._inkbar;
        var i = this,
          o = this._$scope.$watch(function() {
            return i._navBarEl.querySelectorAll("._md-nav-button").length;
          }, function(e) {
            e > 0 && (i._initTabs(), o());
          });
      }

      function i(e, n) {
        return {
          restrict: "E",
          require: ["mdNavItem", "^mdNavBar"],
          controller: o,
          bindToController: !0,
          controllerAs: "ctrl",
          replace: !0,
          transclude: !0,
          template: function(e, t) {
            var n,
              r,
              i,
              o = t.mdNavClick,
              a = t.mdNavHref,
              s = t.mdNavSref,
              c = t.srefOpts;
            if ((o ? 1 : 0) + (a ? 1 : 0) + (s ? 1 : 0) > 1) throw Error(
              "Must not specify more than one of the md-nav-click, md-nav-href, or md-nav-sref attributes per nav-item directive."
              );
            return o ? n = 'ng-click="ctrl.mdNavClick()"' : a ? n = 'ng-href="{{ctrl.mdNavHref}}"' : s && (
                n = 'ui-sref="{{ctrl.mdNavSref}}"'), r = c ? 'ui-sref-opts="{{ctrl.srefOpts}}" ' : "", n &&
              (i =
                '<md-button class="_md-nav-button md-accent" ng-class="ctrl.getNgClassMap()" ng-blur="ctrl.setFocused(false)" tabindex="-1" ' +
                r + n + '><span ng-transclude class="_md-nav-button-text"></span></md-button>'),
              '<li class="md-nav-item" role="option" aria-selected="{{ctrl.isSelected()}}">' + (i || "") +
              "</li>";
          },
          scope: {
            mdNavClick: "&?",
            mdNavHref: "@?",
            mdNavSref: "@?",
            srefOpts: "=?",
            name: "@"
          },
          link: function(r, i, o, a) {
            n(function() {
              var n = a[0],
                o = a[1],
                s = t.element(i[0].querySelector("._md-nav-button"));
              n.name || (n.name = t.element(i[0].querySelector("._md-nav-button-text")).text().trim()),
                s.on("click", function() {
                  o.mdSelectedNavItem = n.name, r.$apply();
                }), e.expectWithText(i, "aria-label");
            });
          }
        };
      }

      function o(e) {
        this._$element = e, this.mdNavClick, this.mdNavHref, this.mdNavSref, this.srefOpts, this.name, this
          ._selected = !1, this._focused = !1;
      }
      r.$inject = ["$element", "$scope", "$timeout", "$mdConstant"], i.$inject = ["$mdAria", "$$rAF"], o
        .$inject = ["$element"], e.$inject = ["$mdAria", "$mdTheming"], t.module("material.components.navBar",
          ["material.core"]).controller("MdNavBarController", r).directive("mdNavBar", e).controller(
          "MdNavItemController", o).directive("mdNavItem", i), r.prototype._initTabs = function() {
          this._inkbar = t.element(this._navBarEl.querySelector("md-nav-ink-bar"));
          var e = this;
          this._$timeout(function() {
            e._updateTabs(e.mdSelectedNavItem, n);
          }), this._$scope.$watch("ctrl.mdSelectedNavItem", function(t, n) {
            e._$timeout(function() {
              e._updateTabs(t, n);
            });
          });
        }, r.prototype._updateTabs = function(e, t) {
          var n = this,
            r = this._getTabs();
          if (r) {
            var i = -1,
              o = -1,
              a = this._getTabByName(e),
              s = this._getTabByName(t);
            s && (s.setSelected(!1), i = r.indexOf(s)), a && (a.setSelected(!0), o = r.indexOf(a)), this
              ._$timeout(function() {
                n._updateInkBarStyles(a, o, i);
              });
          }
        }, r.prototype._updateInkBarStyles = function(e, t, n) {
          if (this._inkbar.toggleClass("_md-left", t < n).toggleClass("_md-right", t > n), this._inkbar.css({
              display: t < 0 ? "none" : ""
            }), e) {
            var r = e.getButtonEl(),
              i = r.offsetLeft;
            this._inkbar.css({
              left: i + "px",
              width: r.offsetWidth + "px"
            });
          }
        }, r.prototype._getTabs = function() {
          var e = Array.prototype.slice.call(this._navBarEl.querySelectorAll(".md-nav-item")).map(function(
          e) {
            return t.element(e).controller("mdNavItem");
          });
          return e.indexOf(n) ? e : null;
        }, r.prototype._getTabByName = function(e) {
          return this._findTab(function(t) {
            return t.getName() == e;
          });
        }, r.prototype._getSelectedTab = function() {
          return this._findTab(function(e) {
            return e.isSelected();
          });
        }, r.prototype.getFocusedTab = function() {
          return this._findTab(function(e) {
            return e.hasFocus();
          });
        }, r.prototype._findTab = function(e) {
          for (var t = this._getTabs(), n = 0; n < t.length; n++)
            if (e(t[n])) return t[n];
          return null;
        }, r.prototype.onFocus = function() {
          var e = this._getSelectedTab();
          e && e.setFocused(!0);
        }, r.prototype._moveFocus = function(e, t) {
          e.setFocused(!1), t.setFocused(!0);
        }, r.prototype.onKeydown = function(e) {
          var t = this._$mdConstant.KEY_CODE,
            n = this._getTabs(),
            r = this.getFocusedTab();
          if (r) {
            var i = n.indexOf(r);
            switch (e.keyCode) {
              case t.UP_ARROW:
              case t.LEFT_ARROW:
                i > 0 && this._moveFocus(r, n[i - 1]);
                break;
              case t.DOWN_ARROW:
              case t.RIGHT_ARROW:
                i < n.length - 1 && this._moveFocus(r, n[i + 1]);
                break;
              case t.SPACE:
              case t.ENTER:
                this._$timeout(function() {
                  r.getButtonEl().click();
                });
            }
          }
        }, o.prototype.getNgClassMap = function() {
          return {
            "md-active": this._selected,
            "md-primary": this._selected,
            "md-unselected": !this._selected,
            "md-focused": this._focused
          };
        }, o.prototype.getName = function() {
          return this.name;
        }, o.prototype.getButtonEl = function() {
          return this._$element[0].querySelector("._md-nav-button");
        }, o.prototype.setSelected = function(e) {
          this._selected = e;
        }, o.prototype.isSelected = function() {
          return this._selected;
        }, o.prototype.setFocused = function(e) {
          this._focused = e, e && this.getButtonEl().focus();
        }, o.prototype.hasFocus = function() {
          return this._focused;
        };
    }(),
    function() {
      t.module("material.components.progressCircular", ["material.core"]);
    }(),
    function() {
      function e(e, t) {
        return ["$mdUtil", "$window", function(n, r) {
          return {
            restrict: "A",
            multiElement: !0,
            link: function(i, o, a) {
              var s = i.$on("$md-resize-enable", function() {
                s();
                var c = o[0],
                  u = c.nodeType === r.Node.ELEMENT_NODE ? r.getComputedStyle(c) : {};
                i.$watch(a[e], function(e) {
                  if (!!e === t) {
                    n.nextTick(function() {
                      i.$broadcast("$md-resize");
                    });
                    var r = {
                      cachedTransitionStyles: u
                    };
                    n.dom.animator.waitTransitionEnd(o, r).then(function() {
                      i.$broadcast("$md-resize");
                    });
                  }
                });
              });
            }
          };
        }];
      }
      t.module("material.components.showHide", ["material.core"]).directive("ngShow", e("ngShow", !0))
        .directive("ngHide", e("ngHide", !1));
    }(),
    function() {
      function e(e, n, r) {
        function i(e, t, n) {
          return e.attr("aria-valuemin", 0), e.attr("aria-valuemax", 100), e.attr("role", "progressbar"), o;
        }

        function o(r, i, o) {
          function f() {
            o.$observe("value", function(e) {
              var t = a(e);
              i.attr("aria-valuenow", t), p() != l && m(E, t);
            }), o.$observe("mdBufferValue", function(e) {
              m(b, a(e));
            }), o.$observe("disabled", function(e) {
              g = e === !0 || e === !1 ? !!e : t.isDefined(e), i.toggleClass(d, g), _.toggleClass(v, !g);
            }), o.$observe("mdMode", function(e) {
              switch (v && _.removeClass(v), e) {
                case l:
                case u:
                case s:
                case c:
                  _.addClass(v = "md-mode-" + e);
                  break;
                default:
                  _.addClass(v = "md-mode-" + c);
              }
            });
          }

          function h() {
            if (t.isUndefined(o.mdMode)) {
              var e = t.isDefined(o.value),
                n = e ? s : c;
              i.attr("md-mode", n), o.mdMode = n;
            }
          }

          function p() {
            var e = (o.mdMode || "").trim();
            if (e) switch (e) {
              case s:
              case c:
              case u:
              case l:
                break;
              default:
                e = c;
            }
            return e;
          }

          function m(e, r) {
            if (!g && p()) {
              var i = n.supplant("translateX({0}%) scale({1},1)", [(r - 100) / 2, r / 100]),
                o = y({
                  transform: i
                });
              t.element(e).css(o);
            }
          }
          e(i);
          var v,
            g = o.hasOwnProperty("disabled"),
            y = n.dom.animator.toCss,
            b = t.element(i[0].querySelector(".md-bar1")),
            E = t.element(i[0].querySelector(".md-bar2")),
            _ = t.element(i[0].querySelector(".md-container"));
          i.attr("md-mode", p()).toggleClass(d, g), h(), f();
        }

        function a(e) {
          return Math.max(0, Math.min(e || 0, 100));
        }
        var s = "determinate",
          c = "indeterminate",
          u = "buffer",
          l = "query",
          d = "_md-progress-linear-disabled";
        return {
          restrict: "E",
          template: '<div class="md-container"><div class="md-dashed"></div><div class="md-bar md-bar1"></div><div class="md-bar md-bar2"></div></div>',
          compile: i
        };
      }
      e.$inject = ["$mdTheming", "$mdUtil", "$log"], t.module("material.components.progressLinear", [
        "material.core"
      ]).directive("mdProgressLinear", e);
    }(),
    function() {
      function r(e, r, i, o, a, s, c, u) {
        function l(u, l) {
          var d = t.element("<md-select-value><span></span></md-select-value>");
          if (d.append('<span class="md-select-icon" aria-hidden="true"></span>'), d.addClass(
              "md-select-value"), d[0].hasAttribute("id") || d.attr("id", "select_value_label_" + r
          .nextUid()), u.find("md-content").length || u.append(t.element("<md-content>").append(u
          .contents())), l.mdOnOpen && (u.find("md-content").prepend(t.element(
              '<div> <md-progress-circular md-mode="indeterminate" ng-if="$$loadingAsyncDone === false" md-diameter="25px"></md-progress-circular></div>'
              )), u.find("md-option").attr("ng-show", "$$loadingAsyncDone")), l.name) {
            var f = t.element('<select class="md-visually-hidden">');
            f.attr({
              name: l.name,
              "aria-hidden": "true",
              tabindex: "-1"
            });
            var h = u.find("md-option");
            t.forEach(h, function(e) {
              var n = t.element("<option>" + e.innerHTML + "</option>");
              e.hasAttribute("ng-value") ? n.attr("ng-value", e.getAttribute("ng-value")) : e
                .hasAttribute("value") && n.attr("value", e.getAttribute("value")), f.append(n);
            }), f.append('<option ng-value="' + l.ngModel + '" selected></option>'), u.parent().append(f);
          }
          var p = r.parseAttributeBoolean(l.multiple),
            m = p ? "multiple" : "",
            v =
            '<div class="md-select-menu-container" aria-hidden="true"><md-select-menu {0}>{1}</md-select-menu></div>';
          return v = r.supplant(v, [m, u.html()]), u.empty().append(d), u.append(v), l.tabindex || l.$set(
              "tabindex", 0),
            function(u, l, d, f) {
              function h() {
                var e = l.attr("aria-label") || l.attr("placeholder");
                !e && T && T.label && (e = T.label.text()), $ = e, a.expect(l, "aria-label", e);
              }

              function m() {
                I && (D = D || I.find("md-select-menu").controller("mdSelectMenu"), C.setLabelText(D
                  .selectedLabels()));
              }

              function v() {
                if ($) {
                  var e = D.selectedLabels({
                    mode: "aria"
                  });
                  l.attr("aria-label", e.length ? $ + ": " + e : $);
                }
              }

              function g() {
                T && T.setHasValue(D.selectedLabels().length > 0 || (l[0].validity || {}).badInput);
              }

              function y() {
                if (I = t.element(l[0].querySelector(".md-select-menu-container")), O = u, d
                  .mdContainerClass) {
                  var e = I[0].getAttribute("class") + " " + d.mdContainerClass;
                  I[0].setAttribute("class", e);
                }
                D = I.find("md-select-menu").controller("mdSelectMenu"), D.init(x, d.ngModel), l.on(
                  "$destroy",
                  function() {
                    I.remove();
                  });
              }

              function b(e) {
                if (i.isNavigationKey(e)) e.preventDefault(), E(e);
                else if (i.isInputKey(e) || i.isNumPadKey(e)) {
                  e.preventDefault();
                  var n = D.optNodeForKeyboardSearch(e);
                  if (!n || n.hasAttribute("disabled")) return;
                  var r = t.element(n).controller("mdOption");
                  D.isMultiple || D.deselect(Object.keys(D.selected)[0]), D.select(r.hashKey, r.value), D
                    .refreshViewValue();
                }
              }

              function E() {
                O._mdSelectIsOpen = !0, l.attr("aria-expanded", "true"), e.show({
                  scope: O,
                  preserveScope: !0,
                  skipCompile: !0,
                  element: I,
                  target: l[0],
                  selectCtrl: C,
                  preserveElement: !0,
                  hasBackdrop: !0,
                  loadingAsync: !!d.mdOnOpen && (u.$eval(d.mdOnOpen) || !0)
                }).finally(function() {
                  O._mdSelectIsOpen = !1, l.focus(), l.attr("aria-expanded", "false"), x.$setTouched();
                });
              }
              var _,
                $,
                w = !0,
                T = f[0],
                C = f[1],
                x = f[2],
                S = f[3],
                A = l.find("md-select-value"),
                M = t.isDefined(d.readonly),
                k = r.parseAttributeBoolean(d.mdNoAsterisk);
              if (k && l.addClass("md-no-asterisk"), T) {
                var N = T.isErrorGetter || function() {
                  return x.$invalid && (x.$touched || S && S.$submitted);
                };
                if (T.input && l.find("md-select-header").find("input")[0] !== T.input[0]) throw new Error(
                  "<md-input-container> can only have *one* child <input>, <textarea> or <select> element!"
                  );
                T.input = l, T.label || a.expect(l, "aria-label", l.attr("placeholder")), u.$watch(N, T
                  .setInvalid);
              }
              var I, O, D;
              y(), o(l), S && t.isDefined(d.multiple) && r.nextTick(function() {
                var e = x.$modelValue || x.$viewValue;
                e && S.$setPristine();
              });
              var R = x.$render;
              x.$render = function() {
                R(), m(), v(), g();
              }, d.$observe("placeholder", x.$render), T && T.label && d.$observe("required", function(e) {
                T.label.toggleClass("md-required", e && !k);
              }), C.setLabelText = function(e) {
                C.setIsPlaceholder(!e);
                var t = !1;
                if (d.mdSelectedText && d.mdSelectedHtml) throw Error(
                  "md-select cannot have both `md-selected-text` and `md-selected-html`");
                if (d.mdSelectedText || d.mdSelectedHtml) e = s(d.mdSelectedText || d.mdSelectedHtml)(u),
                  t = !0;
                else if (!e) {
                  var n = d.placeholder || (T && T.label ? T.label.text() : "");
                  e = n || "", t = !0;
                }
                var r = A.children().eq(0);
                d.mdSelectedHtml ? r.html(c.getTrustedHtml(e)) : t ? r.text(e) : r.html(e);
              }, C.setIsPlaceholder = function(e) {
                e ? (A.addClass("md-select-placeholder"), T && T.label && T.label.addClass(
                  "md-placeholder")) : (A.removeClass("md-select-placeholder"), T && T.label && T.label
                  .removeClass("md-placeholder"));
              }, M || (l.on("focus", function(e) {
                T && T.setFocused(!0);
              }), l.on("blur", function(e) {
                w && (w = !1, O._mdSelectIsOpen && e.stopImmediatePropagation()), O._mdSelectIsOpen || (
                  T && T.setFocused(!1), g());
              })), C.triggerClose = function() {
                s(d.mdOnClose)(u);
              }, u.$$postDigest(function() {
                h(), m(), v();
              }), u.$watch(function() {
                return D.selectedLabels();
              }, m);
              var P;
              d.$observe("ngMultiple", function(e) {
                P && P();
                var t = s(e);
                P = u.$watch(function() {
                  return t(u);
                }, function(e, t) {
                  e === n && t === n || (e ? l.attr("multiple", "multiple") : l.removeAttr(
                    "multiple"), l.attr("aria-multiselectable", e ? "true" : "false"), I && (D
                      .setMultiple(e), R = x.$render, x.$render = function() {
                        R(), m(), v(), g();
                      }, x.$render()));
                });
              }), d.$observe("disabled", function(e) {
                t.isString(e) && (e = !0), _ !== n && _ === e || (_ = e, e ? l.attr({
                  "aria-disabled": "true"
                }).removeAttr("tabindex").off("click", E).off("keydown", b) : l.attr({
                  tabindex: d.tabindex,
                  "aria-disabled": "false"
                }).on("click", E).on("keydown", b));
              }), d.hasOwnProperty("disabled") || d.hasOwnProperty("ngDisabled") || (l.attr({
                "aria-disabled": "false"
              }), l.on("click", E), l.on("keydown", b));
              var L = {
                role: "listbox",
                "aria-expanded": "false",
                "aria-multiselectable": p && !d.ngMultiple ? "true" : "false"
              };
              l[0].hasAttribute("id") || (L.id = "select_" + r.nextUid());
              var U = "select_container_" + r.nextUid();
              I.attr("id", U), L["aria-owns"] = U, l.attr(L), u.$on("$destroy", function() {
                e.destroy().finally(function() {
                  T && (T.setFocused(!1), T.setHasValue(!1), T.input = null), x.$setTouched();
                });
              });
            };
        }
        var d = i.KEY_CODE;
        [d.SPACE, d.ENTER, d.UP_ARROW, d.DOWN_ARROW];
        return {
          restrict: "E",
          require: ["^?mdInputContainer", "mdSelect", "ngModel", "?^form"],
          compile: l,
          controller: function() {}
        };
      }

      function i(e, r, i, o) {
        function a(e, n, i, a) {
          function s(e) {
            13 != e.keyCode && 32 != e.keyCode || c(e);
          }

          function c(n) {
            var i = r.getClosest(n.target, "md-option"),
              o = i && t.element(i).data("$mdOptionController");
            if (i && o) {
              if (i.hasAttribute("disabled")) return n.stopImmediatePropagation(), !1;
              var a = u.hashGetter(o.value),
                s = t.isDefined(u.selected[a]);
              e.$apply(function() {
                u.isMultiple ? s ? u.deselect(a) : u.select(a, o.value) : s || (u.deselect(Object.keys(u
                  .selected)[0]), u.select(a, o.value)), u.refreshViewValue();
              });
            }
          }
          var u = a[0];
          n.addClass("_md"), o(n), n.on("click", c), n.on("keypress", s);
        }

        function s(o, a, s) {
          function c() {
            var e = d.ngModel.$modelValue || d.ngModel.$viewValue || [];
            if (t.isArray(e)) {
              var n = Object.keys(d.selected),
                r = e.map(d.hashGetter),
                i = n.filter(function(e) {
                  return r.indexOf(e) === -1;
                });
              i.forEach(d.deselect), r.forEach(function(t, n) {
                d.select(t, e[n]);
              });
            }
          }

          function u() {
            var e = d.ngModel.$viewValue || d.ngModel.$modelValue;
            Object.keys(d.selected).forEach(d.deselect), d.select(d.hashGetter(e), e);
          }
          var d = this;
          d.isMultiple = t.isDefined(a.multiple), d.selected = {}, d.options = {}, o.$watchCollection(
            function() {
              return d.options;
            },
            function() {
              d.ngModel.$render();
            });
          var f, h;
          d.setMultiple = function(e) {
            function n(e, n) {
              return t.isArray(e || n || []);
            }
            var r = d.ngModel;
            h = h || r.$isEmpty, d.isMultiple = e, f && f(), d.isMultiple ? (r.$validators["md-multiple"] =
              n, r.$render = c, o.$watchCollection(d.modelBinding, function(e) {
                n(e) && c(e), d.ngModel.$setPristine();
              }), r.$isEmpty = function(e) {
                return !e || 0 === e.length;
              }) : (delete r.$validators["md-multiple"], r.$render = u);
          };
          var p,
            m,
            v,
            g = "",
            y = 300;
          d.optNodeForKeyboardSearch = function(e) {
            p && clearTimeout(p), p = setTimeout(function() {
              p = n, g = "", v = n, m = n;
            }, y);
            var r = e.keyCode - (i.isNumPadKey(e) ? 48 : 0);
            g += String.fromCharCode(r);
            var o = new RegExp("^" + g, "i");
            m || (m = s.find("md-option"), v = new Array(m.length), t.forEach(m, function(e, t) {
              v[t] = e.textContent.trim();
            }));
            for (var a = 0; a < v.length; ++a)
              if (o.test(v[a])) return m[a];
          }, d.init = function(n, i) {
            d.ngModel = n, d.modelBinding = i, d.ngModel.$isEmpty = function(e) {
              return !d.options[d.hashGetter(e)];
            };
            var a = r.getModelOption(n, "trackBy");
            if (a) {
              var s = {},
                c = e(a);
              d.hashGetter = function(e, t) {
                return s.$value = e, c(t || o, s);
              };
            } else d.hashGetter = function(e) {
              return t.isObject(e) ? "object_" + (e.$$mdSelectId || (e.$$mdSelectId = ++l)) : e;
            };
            d.setMultiple(d.isMultiple);
          }, d.selectedLabels = function(e) {
            e = e || {};
            var t = e.mode || "html",
              n = r.nodesToArray(s[0].querySelectorAll("md-option[selected]"));
            if (n.length) {
              var i;
              return "html" == t ? i = function(e) {
                if (e.hasAttribute("md-option-empty")) return "";
                var t = e.innerHTML,
                  n = e.querySelector(".md-ripple-container");
                n && (t = t.replace(n.outerHTML, ""));
                var r = e.querySelector(".md-container");
                return r && (t = t.replace(r.outerHTML, "")), t;
              } : "aria" == t && (i = function(e) {
                return e.hasAttribute("aria-label") ? e.getAttribute("aria-label") : e.textContent;
              }), r.uniq(n.map(i)).join(", ");
            }
            return "";
          }, d.select = function(e, t) {
            var n = d.options[e];
            n && n.setSelected(!0), d.selected[e] = t;
          }, d.deselect = function(e) {
            var t = d.options[e];
            t && t.setSelected(!1), delete d.selected[e];
          }, d.addOption = function(e, n) {
            if (t.isDefined(d.options[e])) throw new Error(
              'Duplicate md-option values are not allowed in a select. Duplicate value "' + n.value +
              '" found.');
            d.options[e] = n, t.isDefined(d.selected[e]) && (d.select(e, n.value), t.isDefined(d.ngModel
                .$modelValue) && d.hashGetter(d.ngModel.$modelValue) === e && d.ngModel.$validate(), d
              .refreshViewValue());
          }, d.removeOption = function(e) {
            delete d.options[e];
          }, d.refreshViewValue = function() {
            var e,
              n = [];
            for (var i in d.selected)(e = d.options[i]) ? n.push(e.value) : n.push(d.selected[i]);
            var o = r.getModelOption(d.ngModel, "trackBy"),
              a = d.isMultiple ? n : n[0],
              s = d.ngModel.$modelValue;
            (o ? t.equals(s, a) : s + "" === a) || (d.ngModel.$setViewValue(a), d.ngModel.$render());
          };
        }
        return s.$inject = ["$scope", "$attrs", "$element"], {
          restrict: "E",
          require: ["mdSelectMenu"],
          scope: !1,
          controller: s,
          link: {
            pre: a
          }
        };
      }

      function o(e, n) {
        function r(e, n) {
          return e.append(t.element('<div class="md-text">').append(e.contents())), e.attr("tabindex", n
            .tabindex || "0"), i(n) || e.attr("md-option-empty", ""), o;
        }

        function i(e) {
          var t = e.value,
            n = e.ngValue;
          return t || n;
        }

        function o(r, i, o, a) {
          function s(e, t, n) {
            if (!l.hashGetter) return void(n || r.$$postDigest(function() {
              s(e, t, !0);
            }));
            var i = l.hashGetter(t, r),
              o = l.hashGetter(e, r);
            u.hashKey = o, u.value = e, l.removeOption(i, u), l.addOption(o, u);
          }

          function c() {
            var e = {
              role: "option",
              "aria-selected": "false"
            };
            i[0].hasAttribute("id") || (e.id = "select_option_" + n.nextUid()), i.attr(e);
          }
          var u = a[0],
            l = a[1];
          l.isMultiple && (i.addClass("md-checkbox-enabled"), i.prepend(d.clone())), t.isDefined(o.ngValue) ?
            r.$watch(o.ngValue, s) : t.isDefined(o.value) ? s(o.value) : r.$watch(function() {
              return i.text().trim();
            }, s), o.$observe("disabled", function(e) {
              e ? i.attr("tabindex", "-1") : i.attr("tabindex", "0");
            }), r.$$postDigest(function() {
              o.$observe("selected", function(e) {
                t.isDefined(e) && ("string" == typeof e && (e = !0), e ? (l.isMultiple || l.deselect(
                    Object.keys(l.selected)[0]), l.select(u.hashKey, u.value)) : l.deselect(u
                  .hashKey), l.refreshViewValue());
              });
            }), e.attach(r, i), c(), r.$on("$destroy", function() {
              l.removeOption(u.hashKey, u);
            });
        }

        function a(e) {
          this.selected = !1, this.setSelected = function(t) {
            t && !this.selected ? e.attr({
                selected: "selected",
                "aria-selected": "true"
              }) : !t && this.selected && (e.removeAttr("selected"), e.attr("aria-selected", "false")), this
              .selected = t;
          };
        }
        return a.$inject = ["$element"], {
          restrict: "E",
          require: ["mdOption", "^^mdSelectMenu"],
          controller: a,
          compile: r
        };
      }

      function a() {
        function e(e, n) {
          function r() {
            return e.parent().find("md-select-header").length;
          }

          function i() {
            var r = e.find("label");
            r.length || (r = t.element("<label>"), e.prepend(r)), r.addClass("md-container-ignore"), n
              .label && r.text(n.label);
          }
          r() || i();
        }
        return {
          restrict: "E",
          compile: e
        };
      }

      function s() {
        return {
          restrict: "E"
        };
      }

      function c(r) {
        function i(r, i, l, d, f, h, p, m, v) {
          function g(e, t, n) {
            function r() {
              return p(t, {
                addClass: "md-leave"
              }).start();
            }

            function i() {
              t.removeClass("md-active"), t.attr("aria-hidden", "true"), t[0].style.display = "none", b(n), !n
                .$destroy && n.restoreFocus && n.target.focus();
            }
            return n = n || {}, n.cleanupInteraction(), n.cleanupResizing(), n.hideBackdrop(), n.$destroy ===
              !0 ? i() : r().then(i);
          }

          function y(e, o, a) {
            function s(e, t, n) {
              return n.parent.append(t), f(function(e, n) {
                try {
                  p(t, {
                    removeClass: "md-leave",
                    duration: 0
                  }).start().then(c).then(e);
                } catch (e) {
                  n(e);
                }
              });
            }

            function c() {
              return f(function(t) {
                if (a.isRemoved) return f.reject(!1);
                var n = E(e, o, a);
                n.container.element.css($.toCss(n.container.styles)), n.dropDown.element.css($.toCss(n
                  .dropDown.styles)), h(function() {
                  o.addClass("md-active"), n.dropDown.element.css($.toCss({
                    transform: ""
                  })), g(a.focusedNode), t();
                });
              });
            }

            function u(e, t, n) {
              return n.disableParentScroll && !l.getClosest(n.target, "MD-DIALOG") ? n.restoreScroll = l
                .disableScrollAround(n.element, n.parent) : n.disableParentScroll = !1, n.hasBackdrop && (n
                  .backdrop = l.createBackdrop(e, "md-select-backdrop md-click-catcher"), m.enter(n.backdrop,
                    v[0].body, null, {
                      duration: 0
                    })),
                function() {
                  n.backdrop && n.backdrop.remove(), n.disableParentScroll && n.restoreScroll(), delete n
                    .restoreScroll;
                };
            }

            function g(e) {
              e && !e.hasAttribute("disabled") && e.focus();
            }

            function y(e, n) {
              var r = o.find("md-select-menu");
              if (!n.target) throw new Error(l.supplant(_, [n.target]));
              t.extend(n, {
                isRemoved: !1,
                target: t.element(n.target),
                parent: t.element(n.parent),
                selectEl: r,
                contentEl: o.find("md-content"),
                optionNodes: r[0].getElementsByTagName("md-option")
              });
            }

            function b() {
              var n = function(e, t, n) {
                  return function() {
                    if (!n.isRemoved) {
                      var r = E(e, t, n),
                        i = r.container,
                        o = r.dropDown;
                      i.element.css($.toCss(i.styles)), o.element.css($.toCss(o.styles));
                    }
                  };
                }(e, o, a),
                r = t.element(d);
              return r.on("resize", n), r.on("orientationchange", n),
                function() {
                  r.off("resize", n), r.off("orientationchange", n);
                };
            }

            function T() {
              a.loadingAsync && !a.isRemoved && (e.$$loadingAsyncDone = !1, f.when(a.loadingAsync).then(
                function() {
                  e.$$loadingAsyncDone = !0, delete a.loadingAsync;
                }).then(function() {
                h(c);
              }));
            }

            function C() {
              function e(e) {
                e.preventDefault(), e.stopPropagation(), a.restoreFocus = !1, l.nextTick(r.hide, !0);
              }

              function t(e) {
                switch (e.preventDefault(), e.stopPropagation(), e.keyCode) {
                  case w.UP_ARROW:
                    return u();
                  case w.DOWN_ARROW:
                    return c();
                  case w.SPACE:
                  case w.ENTER:
                    var t = l.getClosest(e.target, "md-option");
                    t && (f.triggerHandler({
                      type: "click",
                      target: t
                    }), e.preventDefault()), d(e);
                    break;
                  case w.TAB:
                  case w.ESCAPE:
                    e.stopPropagation(), e.preventDefault(), a.restoreFocus = !0, l.nextTick(r.hide, !0);
                    break;
                  default:
                    if (i.isInputKey(e) || i.isNumPadKey(e)) {
                      var n = f.controller("mdSelectMenu").optNodeForKeyboardSearch(e);
                      a.focusedNode = n || a.focusedNode, n && n.focus();
                    }
                }
              }

              function s(e) {
                var t,
                  r = l.nodesToArray(a.optionNodes),
                  i = r.indexOf(a.focusedNode);
                do i === -1 ? i = 0 : "next" === e && i < r.length - 1 ? i++ : "prev" === e && i > 0 && i--,
                  t = r[i], t.hasAttribute("disabled") && (t = n); while (!t && i < r.length - 1 && i > 0);
                t && t.focus(), a.focusedNode = t;
              }

              function c() {
                s("next");
              }

              function u() {
                s("prev");
              }

              function d(e) {
                function t() {
                  var t = !1;
                  if (e && e.currentTarget.children.length > 0) {
                    var n = e.currentTarget.children[0],
                      r = n.scrollHeight > n.clientHeight;
                    if (r && n.children.length > 0) {
                      var i = e.pageX - e.currentTarget.getBoundingClientRect().left;
                      i > n.querySelector("md-option").offsetWidth && (t = !0);
                    }
                  }
                  return t;
                }
                if (!(e && "click" == e.type && e.currentTarget != f[0] || t())) {
                  var n = l.getClosest(e.target, "md-option");
                  n && n.hasAttribute && !n.hasAttribute("disabled") && (e.preventDefault(), e
                    .stopPropagation(), h.isMultiple || (a.restoreFocus = !0, l.nextTick(function() {
                      r.hide(h.ngModel.$viewValue);
                    }, !0)));
                }
              }
              if (!a.isRemoved) {
                var f = a.selectEl,
                  h = f.controller("mdSelectMenu") || {};
                return o.addClass("md-clickable"), a.backdrop && a.backdrop.on("click", e), f.on("keydown",
                  t), f.on("click", d),
                  function() {
                    a.backdrop && a.backdrop.off("click", e), f.off("keydown", t), f.off("click", d), o
                      .removeClass("md-clickable"), a.isRemoved = !0;
                  };
              }
            }
            return T(), y(e, a), a.hideBackdrop = u(e, o, a), s(e, o, a).then(function(e) {
              return o.attr("aria-hidden", "false"), a.alreadyOpen = !0, a.cleanupInteraction = C(), a
                .cleanupResizing = b(), e;
            }, a.hideBackdrop);
          }

          function b(e) {
            var t = e.selectCtrl;
            if (t) {
              var n = e.selectEl.controller("mdSelectMenu");
              t.setLabelText(n ? n.selectedLabels() : ""), t.triggerClose();
            }
          }

          function E(n, r, i) {
            var f,
              h = r[0],
              p = i.target[0].children[0],
              m = v[0].body,
              g = i.selectEl[0],
              y = i.contentEl[0],
              b = m.getBoundingClientRect(),
              E = p.getBoundingClientRect(),
              _ = !1,
              $ = {
                left: b.left + u,
                top: u,
                bottom: b.height - u,
                right: b.width - u - (l.floatingScrollbars() ? 16 : 0)
              },
              w = {
                top: E.top - $.top,
                left: E.left - $.left,
                right: $.right - (E.left + E.width),
                bottom: $.bottom - (E.top + E.height)
              },
              T = b.width - 2 * u,
              C = g.querySelector("md-option[selected]"),
              x = g.getElementsByTagName("md-option"),
              S = g.getElementsByTagName("md-optgroup"),
              A = c(r, y),
              M = o(i.loadingAsync);
            f = M ? y.firstElementChild || y : C ? C : S.length ? S[0] : x.length ? x[0] : y
              .firstElementChild || y, y.offsetWidth > T ? y.style["max-width"] = T + "px" : y.style
              .maxWidth = null, _ && (y.style["min-width"] = E.width + "px"), A && g.classList.add(
                "md-overflow");
            var k = f;
            "MD-OPTGROUP" === (k.tagName || "").toUpperCase() && (k = x[0] || y.firstElementChild || y, f =
              k), i.focusedNode = k, h.style.display = "block";
            var N = g.getBoundingClientRect(),
              I = s(f);
            if (f) {
              var O = d.getComputedStyle(f);
              I.paddingLeft = parseInt(O.paddingLeft, 10) || 0, I.paddingRight = parseInt(O.paddingRight,
                10) || 0;
            }
            if (A) {
              var D = y.offsetHeight / 2;
              y.scrollTop = I.top + I.height / 2 - D, w.top < D ? y.scrollTop = Math.min(I.top, y.scrollTop +
                D - w.top) : w.bottom < D && (y.scrollTop = Math.max(I.top + I.height - N.height, y
                .scrollTop - D + w.bottom));
            }
            var R, P, L, U, F;
            _ ? (R = E.left, P = E.top + E.height, L = "50% 0", P + N.height > $.bottom && (P = E.top - N
              .height, L = "50% 100%")) : (R = E.left + I.left - I.paddingLeft + 2, P = Math.floor(E.top + E
              .height / 2 - I.height / 2 - I.top + y.scrollTop) + 2, L = I.left + E.width / 2 + "px " + (I
              .top + I.height / 2 - y.scrollTop) + "px 0px", U = Math.min(E.width + I.paddingLeft + I
              .paddingRight, T), F = e.getComputedStyle(p)["font-size"]);
            var j = h.getBoundingClientRect(),
              H = Math.round(100 * Math.min(E.width / N.width, 1)) / 100,
              B = Math.round(100 * Math.min(E.height / N.height, 1)) / 100;
            return {
              container: {
                element: t.element(h),
                styles: {
                  left: Math.floor(a($.left, R, $.right - j.width)),
                  top: Math.floor(a($.top, P, $.bottom - j.height)),
                  "min-width": U,
                  "font-size": F
                }
              },
              dropDown: {
                element: t.element(g),
                styles: {
                  transformOrigin: L,
                  transform: i.alreadyOpen ? "" : l.supplant("scale({0},{1})", [H, B])
                }
              }
            };
          }
          var _ = "$mdSelect.show() expected a target element in options.target but got '{0}'!",
            $ = l.dom.animator,
            w = i.KEY_CODE;
          return {
            parent: "body",
            themable: !0,
            onShow: y,
            onRemove: g,
            hasBackdrop: !0,
            disableParentScroll: !0
          };
        }

        function o(e) {
          return e && t.isFunction(e.then);
        }

        function a(e, t, n) {
          return Math.max(e, Math.min(t, n));
        }

        function s(e) {
          return e ? {
            left: e.offsetLeft,
            top: e.offsetTop,
            width: e.offsetWidth,
            height: e.offsetHeight
          } : {
            left: 0,
            top: 0,
            width: 0,
            height: 0
          };
        }

        function c(e, t) {
          var n = !1;
          try {
            var r = e[0].style.display;
            e[0].style.display = "block", n = t.scrollHeight > t.offsetHeight, e[0].style.display = r;
          } finally {}
          return n;
        }
        return i.$inject = ["$mdSelect", "$mdConstant", "$mdUtil", "$window", "$q", "$$rAF", "$animateCss",
          "$animate", "$document"
        ], r("$mdSelect").setDefaults({
          methods: ["target"],
          options: i
        });
      }
      r.$inject = ["$mdSelect", "$mdUtil", "$mdConstant", "$mdTheming", "$mdAria", "$parse", "$sce",
        "$injector"
      ], i.$inject = ["$parse", "$mdUtil", "$mdConstant", "$mdTheming"], o.$inject = ["$mdButtonInkRipple",
        "$mdUtil"
      ], c.$inject = ["$$interimElementProvider"];
      var u = 8,
        l = 0,
        d = t.element('<div class="md-container"><div class="md-icon"></div></div>');
      t.module("material.components.select", ["material.core", "material.components.backdrop"]).directive(
          "mdSelect", r).directive("mdSelectMenu", i).directive("mdOption", o).directive("mdOptgroup", a)
        .directive("mdSelectHeader", s).provider("$mdSelect", c);
    }(),
    function() {
      function e(e, r, i, o) {
        function a(e, n) {
          var o = function() {
              return !1;
            },
            a = function() {
              return i.when(r.supplant(u, [n || ""]));
            };
          return t.extend({
            isLockedOpen: o,
            isOpen: o,
            toggle: a,
            open: a,
            close: a,
            onClose: t.noop,
            then: function(e) {
              return c(n).then(e || t.noop);
            }
          }, e);
        }

        function s(t, i) {
          var a = e.get(t);
          return a || i ? a : (o.error(r.supplant(u, [t || ""])), n);
        }

        function c(t) {
          return e.when(t).catch(o.error);
        }
        var u = "SideNav '{0}' is not available! Did you use md-component-id='{0}'?",
          l = {
            find: s,
            waitFor: c
          };
        return function(e, n) {
          if (t.isUndefined(e)) return l;
          var r = n === !0,
            i = l.find(e, r);
          return !i && r ? l.waitFor(e) : !i && t.isUndefined(n) ? a(l, e) : i;
        };
      }

      function r() {
        return {
          restrict: "A",
          require: "^mdSidenav",
          link: function(e, t, n, r) {}
        };
      }

      function i(e, r, i, o, a, s, c, u, l, d, f, h, p) {
        function m(c, m, v, g) {
          function y(e, t) {
            c.isLockedOpen = e, e === t ? m.toggleClass("md-locked-open", !!e) : s[e ? "addClass" :
              "removeClass"](m, "md-locked-open"), x && x.toggleClass("md-locked-open", !!e);
          }

          function b(e) {
            var t = r.findFocusTarget(m) || r.findFocusTarget(m, "[md-sidenav-focus]") || m,
              n = m.parent();
            n[e ? "on" : "off"]("keydown", w), x && x[e ? "on" : "off"]("click", T);
            var i = E(n, e);
            return e && (k = f[0].activeElement, S = a.getLastInteractionType()), _(e), N = d.all([e && x ? s
              .enter(x, n) : x ? s.leave(x) : d.when(!0), s[e ? "removeClass" : "addClass"](m,
                "md-closed")
            ]).then(function() {
              c.isOpen && (p(function() {
                O.triggerHandler("resize");
              }), t && t.focus()), i && i();
            });
          }

          function E(e, t) {
            var n = m[0],
              r = e[0].scrollTop;
            if (t && r) {
              A = {
                top: n.style.top,
                bottom: n.style.bottom,
                height: n.style.height
              };
              var i = {
                top: r + "px",
                bottom: "auto",
                height: e[0].clientHeight + "px"
              };
              m.css(i), x.css(i);
            }
            if (!t && A) return function() {
              n.style.top = A.top, n.style.bottom = A.bottom, n.style.height = A.height, x[0].style.top =
                null, x[0].style.bottom = null, x[0].style.height = null, A = null;
            };
          }

          function _(e) {
            e && !C ? (C = M.css("overflow"), M.css("overflow", "hidden")) : t.isDefined(C) && (M.css(
              "overflow", C), C = n);
          }

          function $(e) {
            return c.isOpen == e ? d.when(!0) : (c.isOpen && g.onCloseCb && g.onCloseCb(), d(function(t) {
              c.isOpen = e, r.nextTick(function() {
                N.then(function(e) {
                  !c.isOpen && k && "keyboard" === S && (k.focus(), k = null), t(e);
                });
              });
            }));
          }

          function w(e) {
            var t = e.keyCode === i.KEY_CODE.ESCAPE;
            return t ? T(e) : d.when(!0);
          }

          function T(e) {
            return e.preventDefault(), g.close();
          }
          var C,
            x,
            S,
            A,
            M = null,
            k = null,
            N = d.when(!0),
            I = u(v.mdIsLockedOpen),
            O = t.element(h),
            D = function() {
              return I(c.$parent, {
                $media: function(t) {
                  return l.warn("$media is deprecated for is-locked-open. Use $mdMedia instead."), e(t);
                },
                $mdMedia: e
              });
            };
          v.mdDisableScrollTarget && (M = f[0].querySelector(v.mdDisableScrollTarget), M ? M = t.element(M) :
            l.warn(r.supplant(
              'mdSidenav: couldn\'t find element matching selector "{selector}". Falling back to parent.', {
                selector: v.mdDisableScrollTarget
              }))), M || (M = m.parent()), v.hasOwnProperty("mdDisableBackdrop") || (x = r.createBackdrop(c,
            "md-sidenav-backdrop md-opaque ng-enter")), m.addClass("_md"), o(m), x && o.inherit(x, m), m.on(
            "$destroy",
            function() {
              x && x.remove(), g.destroy();
            }), c.$on("$destroy", function() {
            x && x.remove();
          }), c.$watch(D, y), c.$watch("isOpen", b), g.$toggleOpen = $;
        }
        return {
          restrict: "E",
          scope: {
            isOpen: "=?mdIsOpen"
          },
          controller: "$mdSidenavController",
          compile: function(e) {
            return e.addClass("md-closed").attr("tabIndex", "-1"), m;
          }
        };
      }

      function o(e, t, n, r, i) {
        var o = this;
        o.isOpen = function() {
          return !!e.isOpen;
        }, o.isLockedOpen = function() {
          return !!e.isLockedOpen;
        }, o.onClose = function(e) {
          return o.onCloseCb = e, o;
        }, o.open = function() {
          return o.$toggleOpen(!0);
        }, o.close = function() {
          return o.$toggleOpen(!1);
        }, o.toggle = function() {
          return o.$toggleOpen(!e.isOpen);
        }, o.$toggleOpen = function(t) {
          return r.when(e.isOpen = t);
        };
        var a = t.mdComponentId,
          s = a && a.indexOf(i.startSymbol()) > -1,
          c = s ? i(a)(e.$parent) : a;
        o.destroy = n.register(o, c), s && t.$observe("mdComponentId", function(e) {
          e && e !== o.$$mdHandle && (o.destroy(), o.destroy = n.register(o, e));
        });
      }
      e.$inject = ["$mdComponentRegistry", "$mdUtil", "$q", "$log"], i.$inject = ["$mdMedia", "$mdUtil",
        "$mdConstant", "$mdTheming", "$mdInteraction", "$animate", "$compile", "$parse", "$log", "$q",
        "$document", "$window", "$$rAF"
      ], o.$inject = ["$scope", "$attrs", "$mdComponentRegistry", "$q", "$interpolate"], t.module(
        "material.components.sidenav", ["material.core", "material.components.backdrop"]).factory(
        "$mdSidenav", e).directive("mdSidenav", i).directive("mdSidenavFocus", r).controller(
        "$mdSidenavController", o);
    }(),
    function() {
      function e(e, n, r, i) {
        function o(o, a, s, c) {
          function u() {
            a.hasClass("md-focused") || a.addClass("md-focused");
          }

          function l(r) {
            var i = r.which || r.keyCode;
            if (i == n.KEY_CODE.ENTER || r.currentTarget == r.target) switch (i) {
              case n.KEY_CODE.LEFT_ARROW:
              case n.KEY_CODE.UP_ARROW:
                r.preventDefault(), d.selectPrevious(), u();
                break;
              case n.KEY_CODE.RIGHT_ARROW:
              case n.KEY_CODE.DOWN_ARROW:
                r.preventDefault(), d.selectNext(), u();
                break;
              case n.KEY_CODE.ENTER:
                var o = t.element(e.getClosest(a[0], "form"));
                o.length > 0 && o.triggerHandler("submit");
            }
          }
          a.addClass("_md"), r(a);
          var d = c[0],
            f = c[1] || e.fakeNgModel();
          d.init(f), o.mouseActive = !1, a.attr({
            role: "radiogroup",
            tabIndex: a.attr("tabindex") || "0"
          }).on("keydown", l).on("mousedown", function(e) {
            o.mouseActive = !0, i(function() {
              o.mouseActive = !1;
            }, 100);
          }).on("focus", function() {
            o.mouseActive === !1 && d.$element.addClass("md-focused");
          }).on("blur", function() {
            d.$element.removeClass("md-focused");
          });
        }

        function a(e) {
          this._radioButtonRenderFns = [], this.$element = e;
        }

        function s() {
          return {
            init: function(e) {
              this._ngModelCtrl = e, this._ngModelCtrl.$render = t.bind(this, this.render);
            },
            add: function(e) {
              this._radioButtonRenderFns.push(e);
            },
            remove: function(e) {
              var t = this._radioButtonRenderFns.indexOf(e);
              t !== -1 && this._radioButtonRenderFns.splice(t, 1);
            },
            render: function() {
              this._radioButtonRenderFns.forEach(function(e) {
                e();
              });
            },
            setViewValue: function(e, t) {
              this._ngModelCtrl.$setViewValue(e, t), this.render();
            },
            getViewValue: function() {
              return this._ngModelCtrl.$viewValue;
            },
            selectNext: function() {
              return c(this.$element, 1);
            },
            selectPrevious: function() {
              return c(this.$element, -1);
            },
            setActiveDescendant: function(e) {
              this.$element.attr("aria-activedescendant", e);
            },
            isDisabled: function() {
              return this.$element[0].hasAttribute("disabled");
            }
          };
        }

        function c(n, r) {
          var i = e.iterator(n[0].querySelectorAll("md-radio-button"), !0);
          if (i.count()) {
            var o = function(e) {
                return !t.element(e).attr("disabled");
              },
              a = n[0].querySelector("md-radio-button.md-checked"),
              s = i[r < 0 ? "previous" : "next"](a, o) || i.first();
            t.element(s).triggerHandler("click");
          }
        }
        return a.prototype = s(), {
          restrict: "E",
          controller: ["$element", a],
          require: ["mdRadioGroup", "?ngModel"],
          link: {
            pre: o
          }
        };
      }

      function n(e, t, n) {
        function r(r, o, a, s) {
          function c() {
            if (!s) throw "RadioButton: No RadioGroupController could be found.";
            s.add(l), a.$observe("value", l), o.on("click", u).on("$destroy", function() {
              s.remove(l);
            });
          }

          function u(e) {
            o[0].hasAttribute("disabled") || s.isDisabled() || r.$apply(function() {
              s.setViewValue(a.value, e && e.type);
            });
          }

          function l() {
            var e = s.getViewValue() == a.value;
            e !== f && ("md-radio-group" !== o[0].parentNode.nodeName.toLowerCase() && o.parent().toggleClass(
                i, e), e && s.setActiveDescendant(o.attr("id")), f = e, o.attr("aria-checked", e)
              .toggleClass(i, e));
          }

          function d(n, r) {
            n.attr({
              id: a.id || "radio_" + t.nextUid(),
              role: "radio",
              "aria-checked": "false"
            }), e.expectWithText(n, "aria-label");
          }
          var f;
          n(o), d(o, r), a.ngValue ? t.nextTick(c, !1) : c();
        }
        var i = "md-checked";
        return {
          restrict: "E",
          require: "^mdRadioGroup",
          transclude: !0,
          template: '<div class="md-container" md-ink-ripple md-ink-ripple-checkbox><div class="md-off"></div><div class="md-on"></div></div><div ng-transclude class="md-label"></div>',
          link: r
        };
      }
      e.$inject = ["$mdUtil", "$mdConstant", "$mdTheming", "$timeout"], n.$inject = ["$mdAria", "$mdUtil",
          "$mdTheming"
        ], t.module("material.components.radioButton", ["material.core"]).directive("mdRadioGroup", e)
        .directive("mdRadioButton", n);
    }(),
    function() {
      function e() {
        return {
          controller: function() {},
          compile: function(e) {
            var r = e.find("md-slider");
            if (r) {
              var i = r.attr("md-vertical");
              return i !== n && e.attr("md-vertical", ""), r.attr("flex") || r.attr("flex", ""),
                function(e, n, r, i) {
                  function o(e) {
                    n.children().attr("disabled", e), n.find("input").attr("disabled", e);
                  }
                  n.addClass("_md");
                  var a = t.noop;
                  r.disabled ? o(!0) : r.ngDisabled && (a = e.$watch(r.ngDisabled, function(e) {
                    o(e);
                  })), e.$on("$destroy", function() {
                    a();
                  });
                  var s;
                  i.fitInputWidthToTextLength = function(e) {
                    var t = n[0].querySelector("md-input-container");
                    if (t) {
                      var r = getComputedStyle(t),
                        i = parseInt(r.minWidth),
                        o = 2 * parseInt(r.padding);
                      s = s || parseInt(r.maxWidth);
                      var a = Math.max(s, i + o + i / 2 * e);
                      t.style.maxWidth = a + "px";
                    }
                  };
                };
            }
          }
        };
      }

      function r(e, n, r, i, o, a, s, c, u, l) {
        function d(e, n) {
          var i = t.element(e[0].getElementsByClassName("md-slider-wrapper")),
            o = n.tabindex || 0;
          return i.attr("tabindex", o), (n.disabled || n.ngDisabled) && i.attr("tabindex", -1), i.attr("role",
            "slider"), r.expect(e, "aria-label"), f;
        }

        function f(r, d, f, h) {
          function p() {
            $(), M();
          }

          function m(e) {
            ce = parseFloat(e), d.attr("aria-valuemin", e), p();
          }

          function v(e) {
            ue = parseFloat(e), d.attr("aria-valuemax", e), p();
          }

          function g(e) {
            le = parseFloat(e);
          }

          function y(e) {
            de = k(parseInt(e), 0, 6);
          }

          function b() {
            d.attr("aria-disabled", !!W());
          }

          function E() {
            if (ie && !W() && !t.isUndefined(le)) {
              if (le <= 0) {
                var e = "Slider step value must be greater than zero when in discrete mode";
                throw u.error(e), new Error(e);
              }
              var r = Math.floor((ue - ce) / le);
              fe || (fe = t.element("<canvas>").css("position", "absolute"), Z.append(fe), he = fe[0]
                .getContext("2d"));
              var i = w();
              !i || i.height || i.width || ($(), i = pe), fe[0].width = i.width, fe[0].height = i.height;
              for (var o, a = 0; a <= r; a++) {
                var s = n.getComputedStyle(Z[0]);
                he.fillStyle = s.color || "black", o = Math.floor((re ? i.height : i.width) * (a / r)), he
                  .fillRect(re ? 0 : o - 1, re ? o - 1 : 0, re ? i.width : 2, re ? 2 : i.height);
              }
            }
          }

          function _() {
            if (fe && he) {
              var e = w();
              he.clearRect(0, 0, e.width, e.height);
            }
          }

          function $() {
            pe = Q[0].getBoundingClientRect();
          }

          function w() {
            return te(), pe;
          }

          function T(e) {
            if (!W()) {
              var t;
              (re ? e.keyCode === o.KEY_CODE.DOWN_ARROW : e.keyCode === o.KEY_CODE.LEFT_ARROW) ? t = -le: (
                  re ? e.keyCode === o.KEY_CODE.UP_ARROW : e.keyCode === o.KEY_CODE.RIGHT_ARROW) && (t = le),
                t = oe ? -t : t, t && ((e.metaKey || e.ctrlKey || e.altKey) && (t *= 4), e.preventDefault(), e
                  .stopPropagation(), r.$evalAsync(function() {
                    A(G.$viewValue + t);
                  }));
            }
          }

          function C() {
            E(), r.mouseActive = !0, ee.removeClass("md-focused"), l(function() {
              r.mouseActive = !1;
            }, 100);
          }

          function x() {
            r.mouseActive === !1 && ee.addClass("md-focused");
          }

          function S() {
            ee.removeClass("md-focused"), d.removeClass("md-active"), _();
          }

          function A(e) {
            G.$setViewValue(k(N(e)));
          }

          function M() {
            isNaN(G.$viewValue) && (G.$viewValue = G.$modelValue), G.$viewValue = k(G.$viewValue);
            var e = q(G.$viewValue);
            r.modelValue = G.$viewValue, d.attr("aria-valuenow", G.$viewValue), I(e), K.text(G.$viewValue);
          }

          function k(e, n, r) {
            if (t.isNumber(e)) return n = t.isNumber(n) ? n : ce, r = t.isNumber(r) ? r : ue, Math.max(n, Math
              .min(r, e));
          }

          function N(e) {
            if (t.isNumber(e)) {
              var n = Math.round((e - ce) / le) * le + ce;
              return n = Math.round(n * Math.pow(10, de)) / Math.pow(10, de), V && V
                .fitInputWidthToTextLength && i.debounce(function() {
                  V.fitInputWidthToTextLength(n.toString().length);
                }, 100)(), n;
            }
          }

          function I(e) {
            e = H(e);
            var t = 100 * e + "%",
              n = oe ? 100 * (1 - e) + "%" : t;
            re ? X.css("bottom", t) : i.bidiProperty(X, "left", "right", t), J.css(re ? "height" : "width",
              n), d.toggleClass(oe ? "md-max" : "md-min", 0 === e), d.toggleClass(oe ? "md-min" : "md-max",
                1 === e);
          }

          function O(e) {
            if (!W()) {
              d.addClass("md-active"), d[0].focus(), $();
              var t = z(B(re ? e.pointer.y : e.pointer.x)),
                n = k(N(t));
              r.$apply(function() {
                A(n), I(q(n));
              });
            }
          }

          function D(e) {
            if (!W()) {
              d.removeClass("md-dragging");
              var t = z(B(re ? e.pointer.y : e.pointer.x)),
                n = k(N(t));
              r.$apply(function() {
                A(n), M();
              });
            }
          }

          function R(e) {
            W() || (me = !0, e.stopPropagation(), d.addClass("md-dragging"), U(e));
          }

          function P(e) {
            me && (e.stopPropagation(), U(e));
          }

          function L(e) {
            me && (e.stopPropagation(), me = !1);
          }

          function U(e) {
            ie ? j(re ? e.pointer.y : e.pointer.x) : F(re ? e.pointer.y : e.pointer.x);
          }

          function F(e) {
            r.$evalAsync(function() {
              A(z(B(e)));
            });
          }

          function j(e) {
            var t = z(B(e)),
              n = k(N(t));
            I(B(e)), K.text(n);
          }

          function H(e) {
            return Math.max(0, Math.min(e || 0, 1));
          }

          function B(e) {
            var t = re ? pe.top : pe.left,
              n = re ? pe.height : pe.width,
              r = (e - t) / n;
            return re || "rtl" !== i.bidi() || (r = 1 - r), Math.max(0, Math.min(1, re ? 1 - r : r));
          }

          function z(e) {
            var t = oe ? 1 - e : e;
            return ce + t * (ue - ce);
          }

          function q(e) {
            var t = (e - ce) / (ue - ce);
            return oe ? 1 - t : t;
          }
          a(d);
          var G = h[0] || {
              $setViewValue: function(e) {
                this.$viewValue = e, this.$viewChangeListeners.forEach(function(e) {
                  e();
                });
              },
              $parsers: [],
              $formatters: [],
              $viewChangeListeners: []
            },
            V = h[1],
            W = (t.element(i.getClosest(d, "_md-slider-container", !0)), f.ngDisabled ? t.bind(null, c(f
              .ngDisabled), r.$parent) : function() {
              return d[0].hasAttribute("disabled");
            }),
            Y = t.element(d[0].querySelector(".md-thumb")),
            K = t.element(d[0].querySelector(".md-thumb-text")),
            X = Y.parent(),
            Q = t.element(d[0].querySelector(".md-track-container")),
            J = t.element(d[0].querySelector(".md-track-fill")),
            Z = t.element(d[0].querySelector(".md-track-ticks")),
            ee = t.element(d[0].getElementsByClassName("md-slider-wrapper")),
            te = (t.element(d[0].getElementsByClassName("md-slider-content")), i.throttle($, 5e3)),
            ne = 3,
            re = t.isDefined(f.mdVertical),
            ie = t.isDefined(f.mdDiscrete),
            oe = t.isDefined(f.mdInvert);
          t.isDefined(f.min) ? f.$observe("min", m) : m(0), t.isDefined(f.max) ? f.$observe("max", v) : v(
            100), t.isDefined(f.step) ? f.$observe("step", g) : g(1), t.isDefined(f.round) ? f.$observe(
              "round", y) : y(ne);
          var ae = t.noop;
          f.ngDisabled && (ae = r.$parent.$watch(f.ngDisabled, b)), s.register(ee, "drag", {
            horizontal: !re
          }), r.mouseActive = !1, ee.on("keydown", T).on("mousedown", C).on("focus", x).on("blur", S).on(
            "$md.pressdown", O).on("$md.pressup", D).on("$md.dragstart", R).on("$md.drag", P).on(
            "$md.dragend", L), setTimeout(p, 0);
          var se = e.throttle(p);
          t.element(n).on("resize", se), r.$on("$destroy", function() {
            t.element(n).off("resize", se);
          }), G.$render = M, G.$viewChangeListeners.push(M), G.$formatters.push(k), G.$formatters.push(N);
          var ce,
            ue,
            le,
            de,
            fe,
            he,
            pe = {};
          $();
          var me = !1;
        }
        return {
          scope: {},
          require: ["?ngModel", "?^mdSliderContainer"],
          template: '<div class="md-slider-wrapper"><div class="md-slider-content"><div class="md-track-container"><div class="md-track"></div><div class="md-track md-track-fill"></div><div class="md-track-ticks"></div></div><div class="md-thumb-container"><div class="md-thumb"></div><div class="md-focus-thumb"></div><div class="md-focus-ring"></div><div class="md-sign"><span class="md-thumb-text"></span></div><div class="md-disabled-thumb"></div></div></div></div>',
          compile: d
        };
      }
      r.$inject = ["$$rAF", "$window", "$mdAria", "$mdUtil", "$mdConstant", "$mdTheming", "$mdGesture",
        "$parse", "$log", "$timeout"
      ], t.module("material.components.slider", ["material.core"]).directive("mdSlider", r).directive(
        "mdSliderContainer", e);
    }(),
    function() {
      function e(e, t, r, i) {
        function o(i) {
          function o(e, t) {
            t.addClass("md-sticky-clone");
            var n = {
              element: e,
              clone: t
            };
            return m.items.push(n), r.nextTick(function() {
                h.prepend(n.clone);
              }), p(),
              function() {
                m.items.forEach(function(t, n) {
                  t.element[0] === e[0] && (m.items.splice(n, 1), t.clone.remove());
                }), p();
              };
          }

          function s() {
            m.items.forEach(c), m.items = m.items.sort(function(e, t) {
              return e.top < t.top ? -1 : 1;
            });
            for (var e, t = h.prop("scrollTop"), n = m.items.length - 1; n >= 0; n--)
              if (t > m.items[n].top) {
                e = m.items[n];
                break;
              }
            l(e);
          }

          function c(e) {
            var t = e.element[0];
            for (e.top = 0, e.left = 0, e.right = 0; t && t !== h[0];) e.top += t.offsetTop, e.left += t
              .offsetLeft, t.offsetParent && (e.right += t.offsetParent.offsetWidth - t.offsetWidth - t
                .offsetLeft), t = t.offsetParent;
            e.height = e.element.prop("offsetHeight");
            var i = r.floatingScrollbars() ? "0" : n;
            r.bidi(e.clone, "margin-left", e.left, i), r.bidi(e.clone, "margin-right", i, e.right);
          }

          function u() {
            var e = h.prop("scrollTop"),
              t = e > (u.prevScrollTop || 0);
            if (u.prevScrollTop = e, 0 === e) return void l(null);
            if (t) {
              if (m.next && m.next.top <= e) return void l(m.next);
              if (m.current && m.next && m.next.top - e <= m.next.height) return void f(m.current, e + (m.next
                .top - m.next.height - e));
            }
            if (!t) {
              if (m.current && m.prev && e < m.current.top) return void l(m.prev);
              if (m.next && m.current && e >= m.next.top - m.current.height) return void f(m.current, e + (m
                .next.top - e - m.current.height));
            }
            m.current && f(m.current, e);
          }

          function l(e) {
            if (m.current !== e) {
              m.current && (f(m.current, null), d(m.current, null)), e && d(e, "active"), m.current = e;
              var t = m.items.indexOf(e);
              m.next = m.items[t + 1], m.prev = m.items[t - 1], d(m.next, "next"), d(m.prev, "prev");
            }
          }

          function d(e, t) {
            e && e.state !== t && (e.state && (e.clone.attr("sticky-prev-state", e.state), e.element.attr(
              "sticky-prev-state", e.state)), e.clone.attr("sticky-state", t), e.element.attr(
              "sticky-state", t), e.state = t);
          }

          function f(t, i) {
            t && (null === i || i === n ? t.translateY && (t.translateY = null, t.clone.css(e.CSS.TRANSFORM,
              "")) : (t.translateY = i, r.bidi(t.clone, e.CSS.TRANSFORM, "translate3d(" + t.left + "px," +
              i + "px,0)", "translateY(" + i + "px)")));
          }
          var h = i.$element,
            p = t.throttle(s);
          a(h), h.on("$scrollstart", p), h.on("$scroll", u);
          var m;
          return m = {
            prev: null,
            current: null,
            next: null,
            items: [],
            add: o,
            refreshElements: s
          };
        }

        function a(e) {
          function n() {
            +r.now() - o > a ? (i = !1, e.triggerHandler("$scrollend")) : (e.triggerHandler("$scroll"), t
              .throttle(n));
          }
          var i,
            o,
            a = 200;
          e.on("scroll touchmove", function() {
            i || (i = !0, t.throttle(n), e.triggerHandler("$scrollstart")), e.triggerHandler("$scroll"),
              o = +r.now();
          });
        }
        var s = r.checkStickySupport();
        return function(e, t, n) {
          var r = t.controller("mdContent");
          if (r)
            if (s) t.css({
              position: s,
              top: 0,
              "z-index": 2
            });
            else {
              var a = r.$element.data("$$sticky");
              a || (a = o(r), r.$element.data("$$sticky", a));
              var c = n || i(t.clone())(e),
                u = a.add(t, c);
              e.$on("$destroy", u);
            }
        };
      }
      e.$inject = ["$mdConstant", "$$rAF", "$mdUtil", "$compile"], t.module("material.components.sticky", [
        "material.core", "material.components.content"
      ]).factory("$mdSticky", e);
    }(),
    function() {
      function e(e, n, r, i, o, a, s) {
        function c(e, c) {
          var l = u.compile(e, c).post;
          return e.addClass("md-dragging"),
            function(e, c, u, d) {
              function f(t) {
                g && g(e) || (t.stopPropagation(), c.addClass("md-dragging"), _ = {
                  width: y.prop("offsetWidth")
                });
              }

              function h(e) {
                if (_) {
                  e.stopPropagation(), e.srcEvent && e.srcEvent.preventDefault();
                  var t = e.pointer.distanceX / _.width,
                    n = v.$viewValue ? 1 + t : t;
                  n = Math.max(0, Math.min(1, n)), y.css(r.CSS.TRANSFORM, "translate3d(" + 100 * n +
                    "%,0,0)"), _.translate = n;
                }
              }

              function p(t) {
                if (_) {
                  t.stopPropagation(), c.removeClass("md-dragging"), y.css(r.CSS.TRANSFORM, "");
                  var n = v.$viewValue ? _.translate < .5 : _.translate > .5;
                  n && m(!v.$viewValue), _ = null, e.skipToggle = !0, s(function() {
                    e.skipToggle = !1;
                  }, 1);
                }
              }

              function m(t) {
                e.$apply(function() {
                  v.$setViewValue(t), v.$render();
                });
              }
              var v = (d[0], d[1] || n.fakeNgModel()),
                g = (d[2], null);
              null != u.disabled ? g = function() {
                return !0;
              } : u.ngDisabled && (g = i(u.ngDisabled));
              var y = t.element(c[0].querySelector(".md-thumb-container")),
                b = t.element(c[0].querySelector(".md-container")),
                E = t.element(c[0].querySelector(".md-label"));
              o(function() {
                c.removeClass("md-dragging");
              }), l(e, c, u, d), g && e.$watch(g, function(e) {
                c.attr("tabindex", e ? -1 : 0);
              }), u.$observe("mdInvert", function(e) {
                var t = n.parseAttributeBoolean(e);
                t ? c.prepend(E) : c.prepend(b), c.toggleClass("md-inverted", t);
              }), a.register(b, "drag"), b.on("$md.dragstart", f).on("$md.drag", h).on("$md.dragend", p);
              var _;
            };
        }
        var u = e[0];
        return {
          restrict: "E",
          priority: r.BEFORE_NG_ARIA,
          transclude: !0,
          template: '<div class="md-container"><div class="md-bar"></div><div class="md-thumb-container"><div class="md-thumb" md-ink-ripple md-ink-ripple-checkbox></div></div></div><div ng-transclude class="md-label"></div>',
          require: ["^?mdInputContainer", "?ngModel", "?^form"],
          compile: c
        };
      }
      e.$inject = ["mdCheckboxDirective", "$mdUtil", "$mdConstant", "$parse", "$$rAF", "$mdGesture",
          "$timeout"
        ], t.module("material.components.switch", ["material.core", "material.components.checkbox"])
        .directive("mdSwitch", e);
    }(),
    function() {
      function e(e, n, r, i, o) {
        return {
          restrict: "E",
          replace: !0,
          transclude: !0,
          template: '<div class="md-subheader _md">  <div class="md-subheader-inner">    <div class="md-subheader-content"></div>  </div></div>',
          link: function(a, s, c, u, l) {
            function d(e) {
              return t.element(e[0].querySelector(".md-subheader-content"));
            }
            r(s), s.addClass("_md"), i.prefixer().removeAttribute(s, "ng-repeat");
            var f = s[0].outerHTML;
            c.$set("role", "heading"), o.expect(s, "aria-level", "2"), l(a, function(e) {
              d(s).append(e);
            }), s.hasClass("md-no-sticky") || l(a, function(t) {
              var r = n('<div class="md-subheader-wrapper" aria-hidden="true">' + f + "</div>")(a);
              i.nextTick(function() {
                d(r).append(t);
              }), e(a, s, r);
            });
          }
        };
      }
      e.$inject = ["$mdSticky", "$compile", "$mdTheming", "$mdUtil", "$mdAria"], t.module(
        "material.components.subheader", ["material.core", "material.components.sticky"]).directive(
        "mdSubheader", e);
    }(),
    function() {
      function e(e) {
        function t(e) {
          function t(t, i, o) {
            i.css("touch-action", "none");
            var a = e(o[n]);
            i.on(r, function(e) {
              t.$applyAsync(function() {
                a(t, {
                  $event: e
                });
              });
            });
          }
          return {
            restrict: "A",
            link: t
          };
        }
        t.$inject = ["$parse"];
        var n = "md" + e,
          r = "$md." + e.toLowerCase();
        return t;
      }
      t.module("material.components.swipe", ["material.core"]).directive("mdSwipeLeft", e("SwipeLeft"))
        .directive("mdSwipeRight", e("SwipeRight")).directive("mdSwipeUp", e("SwipeUp")).directive(
          "mdSwipeDown", e("SwipeDown"));
    }(),
    function() {
      t.module("material.components.tabs", ["material.core", "material.components.icon"]);
    }(),
    function() {
      function n(e, n, r, i, o, a, s, c) {
        function u(u, v, g) {
          function y() {
            u.mdZIndex = u.mdZIndex || f, u.mdDelay = u.mdDelay || h, m[u.mdDirection] || (u.mdDirection = p);
          }

          function b(e) {
            if (e || !O.attr("aria-label")) {
              var t = e || o(v.text().trim())(u.$parent);
              O.attr("aria-label", t);
            }
          }

          function E() {
            y(), k && k.panelEl && k.panelEl.removeClass(S), S = "md-origin-" + u.mdDirection, A = m[u
                .mdDirection], M = s.newPanelPosition().relativeTo(O).addPanelPosition(A.x, A.y), k && k
              .panelEl && (k.panelEl.addClass(S), k.updatePosition(M));
          }

          function _() {
            function t(e) {
              return e.some(function(e) {
                return "disabled" === e.attributeName && O[0].disabled;
              }), !1;
            }

            function r() {
              w(!1);
            }

            function o() {
              P = document.activeElement === O[0];
            }

            function s(e) {
              "focus" === e.type && P ? P = !1 : u.mdVisible || (O.on(d, f), w(!0), "touchstart" === e.type &&
                O.one("touchend", function() {
                  a.nextTick(function() {
                    i.one("touchend", f);
                  }, !1);
                }));
            }

            function f() {
              N = u.hasOwnProperty("mdAutohide") ? u.mdAutohide : g.hasOwnProperty("mdAutohide"), (N || R ||
                i[0].activeElement !== O[0]) && (I && (e.cancel(I), w.queued = !1, I = null), O.off(d, f), O
                .triggerHandler("blur"), w(!1)), R = !1;
            }

            function h() {
              R = !0;
            }

            function p() {
              c.deregister("scroll", r, !0), c.deregister("blur", o), c.deregister("resize", D), O.off(l, s)
                .off(d, f).off("mousedown", h), f(), m && m.disconnect();
            }
            if (O[0] && "MutationObserver" in n) {
              var m = new MutationObserver(function(e) {
                t(e) && a.nextTick(function() {
                  w(!1);
                });
              });
              m.observe(O[0], {
                attributes: !0
              });
            }
            P = !1, c.register("scroll", r, !0), c.register("blur", o), c.register("resize", D), u.$on(
              "$destroy", p), O.on("mousedown", h), O.on(l, s);
          }

          function $() {
            function e() {
              u.$destroy();
            }
            if (v[0] && "MutationObserver" in n) {
              var t = new MutationObserver(function(e) {
                e.forEach(function(e) {
                  "md-visible" !== e.attributeName || u.visibleWatcher || (u.visibleWatcher = u
                    .$watch("mdVisible", T));
                });
              });
              t.observe(v[0], {
                attributes: !0
              }), g.hasOwnProperty("mdVisible") && (u.visibleWatcher = u.$watch("mdVisible", T));
            } else u.visibleWatcher = u.$watch("mdVisible", T);
            u.$watch("mdDirection", E), v.one("$destroy", e), O.one("$destroy", e), u.$on("$destroy",
              function() {
                w(!1), k && k.destroy(), t && t.disconnect(), v.remove();
              }), v.text().indexOf(o.startSymbol()) > -1 && u.$watch(function() {
              return v.text().trim();
            }, b);
          }

          function w(t) {
            w.queued && w.value === !!t || !w.queued && u.mdVisible === !!t || (w.value = !!t, w.queued || (
              t ? (w.queued = !0, I = e(function() {
                u.mdVisible = w.value, w.queued = !1, I = null, u.visibleWatcher || T(u.mdVisible);
              }, u.mdDelay)) : a.nextTick(function() {
                u.mdVisible = !1, u.visibleWatcher || T(!1);
              })));
          }

          function T(e) {
            e ? C() : x();
          }

          function C() {
            if (!v[0].textContent.trim()) throw new Error(
              "Text for the tooltip has not been provided. Please include text within the mdTooltip element."
              );
            if (!k) {
              var e = "tooltip-" + a.nextUid(),
                n = t.element(document.body),
                r = s.newPanelAnimation().openFrom(O).closeTo(O).withAnimation({
                  open: "md-show",
                  close: "md-hide"
                }),
                i = {
                  id: e,
                  attachTo: n,
                  contentElement: v,
                  propagateContainerEvents: !0,
                  panelClass: "md-tooltip " + S,
                  animation: r,
                  position: M,
                  zIndex: u.mdZIndex,
                  focusOnOpen: !1
                };
              k = s.create(i);
            }
            k.open().then(function() {
              k.panelEl.attr("role", "tooltip");
            });
          }

          function x() {
            k && k.close();
          }
          var S,
            A,
            M,
            k,
            N,
            I,
            O = a.getParentWithPointerEvents(v),
            D = r.throttle(E),
            R = !1,
            P = null;
          y(), b(), v.detach(), E(), _(), $();
        }
        var l = "focus touchstart mouseenter",
          d = "blur touchcancel mouseleave",
          f = 100,
          h = 0,
          p = "bottom",
          m = {
            top: {
              x: s.xPosition.CENTER,
              y: s.yPosition.ABOVE
            },
            right: {
              x: s.xPosition.OFFSET_END,
              y: s.yPosition.CENTER
            },
            bottom: {
              x: s.xPosition.CENTER,
              y: s.yPosition.BELOW
            },
            left: {
              x: s.xPosition.OFFSET_START,
              y: s.yPosition.CENTER
            }
          };
        return {
          restrict: "E",
          priority: 210,
          scope: {
            mdZIndex: "=?mdZIndex",
            mdDelay: "=?mdDelay",
            mdVisible: "=?mdVisible",
            mdAutohide: "=?mdAutohide",
            mdDirection: "@?mdDirection"
          },
          link: u
        };
      }

      function r() {
        function n(e) {
          o[e.type] && o[e.type].forEach(function(t) {
            t.call(this, e);
          }, this);
        }

        function r(t, r, i) {
          var s = o[t] = o[t] || [];
          s.length || (i ? e.addEventListener(t, n, !0) : a.on(t, n)), s.indexOf(r) === -1 && s.push(r);
        }

        function i(t, r, i) {
          var s = o[t],
            c = s ? s.indexOf(r) : -1;
          c > -1 && (s.splice(c, 1), 0 === s.length && (i ? e.removeEventListener(t, n, !0) : a.off(t, n)));
        }
        var o = {},
          a = t.element(e);
        return {
          register: r,
          deregister: i
        };
      }
      n.$inject = ["$timeout", "$window", "$$rAF", "$document", "$interpolate", "$mdUtil", "$mdPanel",
        "$$mdTooltipRegistry"
      ], t.module("material.components.tooltip", ["material.core", "material.components.panel"]).directive(
        "mdTooltip", n).service("$$mdTooltipRegistry", r);
    }(),
    function() {
      function e(e, n, r, i, o) {
        var a = t.bind(null, r.supplant, "translate3d(0,{0}px,0)");
        return {
          template: "",
          restrict: "E",
          link: function(s, c, u) {
            function l() {
              function i(e) {
                var t = c.parent().find("md-content");
                !m && t.length && l(null, t), e = s.$eval(e), e === !1 ? v() : v = f();
              }

              function l(e, t) {
                t && c.parent()[0] === t.parent()[0] && (m && m.off("scroll", E), m = t, v = f());
              }

              function d(e) {
                var t = e ? e.target.scrollTop : y;
                _(), g = Math.min(p / b, Math.max(0, g + t - y)), c.css(n.CSS.TRANSFORM, a([-g * b])), m
                  .css(n.CSS.TRANSFORM, a([(p - g) * b])), y = t, r.nextTick(function() {
                    var e = c.hasClass("md-whiteframe-z1");
                    e && !g ? o.removeClass(c, "md-whiteframe-z1") : !e && g && o.addClass(c,
                      "md-whiteframe-z1");
                  });
              }

              function f() {
                return m ? (m.on("scroll", E), m.attr("scroll-shrink", "true"), r.nextTick(h, !1),
                function() {
                  m.off("scroll", E), m.attr("scroll-shrink", "false"), h();
                }) : t.noop;
              }

              function h() {
                p = c.prop("offsetHeight");
                var e = -p * b + "px";
                m.css({
                  "margin-top": e,
                  "margin-bottom": e
                }), d();
              }
              var p,
                m,
                v = t.noop,
                g = 0,
                y = 0,
                b = u.mdShrinkSpeedFactor || .5,
                E = e.throttle(d),
                _ = r.debounce(h, 5e3);
              s.$on("$mdContentLoaded", l), u.$observe("mdScrollShrink", i), u.ngShow && s.$watch(u.ngShow,
                h), u.ngHide && s.$watch(u.ngHide, h), s.$on("$destroy", v);
            }
            c.addClass("_md"), i(c), r.nextTick(function() {
              c.addClass("_md-toolbar-transitions");
            }, !1), t.isDefined(u.mdScrollShrink) && l();
          }
        };
      }
      e.$inject = ["$$rAF", "$mdConstant", "$mdUtil", "$mdTheming", "$animate"], t.module(
        "material.components.toolbar", ["material.core", "material.components.content"]).directive(
        "mdToolbar", e);
    }(),
    function() {
      function e(e) {
        return {
          restrict: "E",
          link: function(t, n) {
            n.addClass("_md"), t.$on("$destroy", function() {
              e.destroy();
            });
          }
        };
      }

      function n(e) {
        function n(e) {
          i = e;
        }

        function r(e, n, r, o) {
          function a(t, a, s) {
            i = s.textContent || s.content;
            var l = !o("gt-sm");
            return a = r.extractElementByName(a, "md-toast", !0), s.element = a, s.onSwipe = function(e, t) {
                var i = e.type.replace("$md.", ""),
                  o = i.replace("swipe", "");
                "down" === o && s.position.indexOf("top") != -1 && !l || "up" === o && (s.position.indexOf(
                  "bottom") != -1 || l) || ("left" !== o && "right" !== o || !l) && (a.addClass("md-" + i),
                  r.nextTick(n.cancel));
              }, s.openClass = c(s.position), a.addClass(s.toastClass), s.parent.addClass(s.openClass), r
              .hasComputedStyle(s.parent, "position", "static") && s.parent.css("position", "relative"), a.on(
                u, s.onSwipe), a.addClass(l ? "md-bottom" : s.position.split(" ").map(function(e) {
                return "md-" + e;
              }).join(" ")), s.parent && s.parent.addClass("md-toast-animating"), e.enter(a, s.parent).then(
                function() {
                  s.parent && s.parent.removeClass("md-toast-animating");
                });
          }

          function s(t, n, i) {
            return n.off(u, i.onSwipe), i.parent && i.parent.addClass("md-toast-animating"), i.openClass && i
              .parent.removeClass(i.openClass), (1 == i.$destroy ? n.remove() : e.leave(n)).then(function() {
                i.parent && i.parent.removeClass("md-toast-animating"), r.hasComputedStyle(i.parent,
                  "position", "static") && i.parent.css("position", "");
              });
          }

          function c(e) {
            return o("gt-xs") ? "md-toast-open-" + (e.indexOf("top") > -1 ? "top" : "bottom") :
              "md-toast-open-bottom";
          }
          var u = "$md.swipeleft $md.swiperight $md.swipeup $md.swipedown";
          return {
            onShow: a,
            onRemove: s,
            toastClass: "",
            position: "bottom left",
            themable: !0,
            hideDelay: 3e3,
            autoWrap: !0,
            transformTemplate: function(e, n) {
              var r = n.autoWrap && e && !/md-toast-content/g.test(e);
              if (r) {
                var i = document.createElement("md-template");
                i.innerHTML = e;
                for (var o = 0; o < i.children.length; o++)
                  if ("MD-TOAST" === i.children[o].nodeName) {
                    var a = t.element('<div class="md-toast-content">');
                    a.append(t.element(i.children[o].childNodes)), i.children[o].appendChild(a[0]);
                  }
                return i.innerHTML;
              }
              return e || "";
            }
          };
        }
        r.$inject = ["$animate", "$mdToast", "$mdUtil", "$mdMedia"];
        var i,
          o = "ok",
          a = e("$mdToast").setDefaults({
            methods: ["position", "hideDelay", "capsule", "parent", "position", "toastClass"],
            options: r
          }).addPreset("simple", {
            argOption: "textContent",
            methods: ["textContent", "content", "action", "highlightAction", "highlightClass", "theme",
              "parent"
            ],
            options: ["$mdToast", "$mdTheming", function(e, t) {
              return {
                template: '<md-toast md-theme="{{ toast.theme }}" ng-class="{\'md-capsule\': toast.capsule}">  <div class="md-toast-content">    <span class="md-toast-text" role="alert" aria-relevant="all" aria-atomic="true">      {{ toast.content }}    </span>    <md-button class="md-action" ng-if="toast.action" ng-click="toast.resolve()"         ng-class="highlightClasses">      {{ toast.action }}    </md-button>  </div></md-toast>',
                controller: ["$scope", function(t) {
                  var n = this;
                  n.highlightAction && (t.highlightClasses = ["md-highlight", n.highlightClass]), t
                    .$watch(function() {
                      return i;
                    }, function() {
                      n.content = i;
                    }), this.resolve = function() {
                      e.hide(o);
                    };
                }],
                theme: t.defaultTheme(),
                controllerAs: "toast",
                bindToController: !0
              };
            }]
          }).addMethod("updateTextContent", n).addMethod("updateContent", n);
        return a;
      }
      e.$inject = ["$mdToast"], n.$inject = ["$$interimElementProvider"], t.module(
        "material.components.toast", ["material.core", "material.components.button"]).directive("mdToast",
        e).provider("$mdToast", n);
    }(),
    function() {
      function e() {
        return {
          restrict: "AE",
          controller: n,
          controllerAs: "$ctrl",
          bindToController: !0
        };
      }

      function n(e) {
        e.addClass("md-truncate");
      }
      n.$inject = ["$element"], t.module("material.components.truncate", ["material.core"]).directive(
        "mdTruncate", e);
    }(),
    function() {
      function e() {
        return {
          controller: r,
          template: n,
          compile: function(e, t) {
            e.addClass("md-virtual-repeat-container").addClass(t.hasOwnProperty("mdOrientHorizontal") ?
              "md-orient-horizontal" : "md-orient-vertical");
          }
        };
      }

      function n(e) {
        return '<div class="md-virtual-repeat-scroller"><div class="md-virtual-repeat-sizer"></div><div class="md-virtual-repeat-offsetter">' +
          e[0].innerHTML + "</div></div>";
      }

      function r(e, n, r, i, o, a, s, c, u) {
        this.$rootScope = o, this.$scope = s, this.$element = c, this.$attrs = u, this.size = 0, this
          .scrollSize = 0, this.scrollOffset = 0, this.horizontal = this.$attrs.hasOwnProperty(
            "mdOrientHorizontal"), this.repeater = null, this.autoShrink = this.$attrs.hasOwnProperty(
            "mdAutoShrink"), this.autoShrinkMin = parseInt(this.$attrs.mdAutoShrinkMin, 10) || 0, this
          .originalSize = null, this.offsetSize = parseInt(this.$attrs.mdOffsetSize, 10) || 0, this
          .oldElementSize = null, this.maxElementPixels = r.ELEMENT_MAX_PIXELS, this.$attrs.mdTopIndex ? (this
            .bindTopIndex = i(this.$attrs.mdTopIndex), this.topIndex = this.bindTopIndex(this.$scope), t
            .isDefined(this.topIndex) || (this.topIndex = 0, this.bindTopIndex.assign(this.$scope, 0)), this
            .$scope.$watch(this.bindTopIndex, t.bind(this, function(e) {
              e !== this.topIndex && this.scrollToIndex(e);
            }))) : this.topIndex = 0, this.scroller = c[0].querySelector(".md-virtual-repeat-scroller"), this
          .sizer = this.scroller.querySelector(".md-virtual-repeat-sizer"), this.offsetter = this.scroller
          .querySelector(".md-virtual-repeat-offsetter");
        var l = t.bind(this, this.updateSize);
        e(t.bind(this, function() {
          l();
          var e = n.debounce(l, 10, null, !1),
            r = t.element(a);
          this.size || e(), r.on("resize", e), s.$on("$destroy", function() {
            r.off("resize", e);
          }), s.$emit("$md-resize-enable"), s.$on("$md-resize", l);
        }));
      }

      function i(e) {
        return {
          controller: o,
          priority: 1e3,
          require: ["mdVirtualRepeat", "^^mdVirtualRepeatContainer"],
          restrict: "A",
          terminal: !0,
          transclude: "element",
          compile: function(t, n) {
            var r = n.mdVirtualRepeat,
              i = r.match(/^\s*([\s\S]+?)\s+in\s+([\s\S]+?)\s*$/),
              o = i[1],
              a = e(i[2]),
              s = n.mdExtraName && e(n.mdExtraName);
            return function(e, t, n, r, i) {
              r[0].link_(r[1], i, o, a, s);
            };
          }
        };
      }

      function o(e, n, r, i, o, a, s, c) {
        this.$scope = e, this.$element = n, this.$attrs = r, this.$browser = i, this.$document = o, this
          .$rootScope = a, this.$$rAF = s, this.onDemand = c.parseAttributeBoolean(r.mdOnDemand), this
          .browserCheckUrlChange = i.$$checkUrlChange, this.newStartIndex = 0, this.newEndIndex = 0, this
          .newVisibleEnd = 0, this.startIndex = 0, this.endIndex = 0, this.itemSize = e.$eval(r.mdItemSize) ||
          null, this.isFirstRender = !0, this.isVirtualRepeatUpdating_ = !1, this.itemsLength = 0, this
          .unwatchItemSize_ = t.noop, this.blocks = {}, this.pooledBlocks = [], e.$on("$destroy", t.bind(this,
            this.cleanupBlocks_));
      }

      function a(e) {
        if (!t.isFunction(e.getItemAtIndex) || !t.isFunction(e.getLength)) throw Error(
          "When md-on-demand is enabled, the Object passed to md-virtual-repeat must implement functions getItemAtIndex() and getLength() "
          );
        this.model = e;
      }
      r.$inject = ["$$rAF", "$mdUtil", "$mdConstant", "$parse", "$rootScope", "$window", "$scope", "$element",
        "$attrs"
      ], o.$inject = ["$scope", "$element", "$attrs", "$browser", "$document", "$rootScope", "$$rAF",
        "$mdUtil"
      ], i.$inject = ["$parse"], t.module("material.components.virtualRepeat", ["material.core",
        "material.components.showHide"
      ]).directive("mdVirtualRepeatContainer", e).directive("mdVirtualRepeat", i);
      var s = 3;
      r.prototype.register = function(e) {
        this.repeater = e, t.element(this.scroller).on("scroll wheel touchmove touchend", t.bind(this, this
          .handleScroll_));
      }, r.prototype.isHorizontal = function() {
        return this.horizontal;
      }, r.prototype.getSize = function() {
        return this.size;
      }, r.prototype.setSize_ = function(e) {
        var t = this.getDimensionName_();
        this.size = e, this.$element[0].style[t] = e + "px";
      }, r.prototype.unsetSize_ = function() {
        this.$element[0].style[this.getDimensionName_()] = this.oldElementSize, this.oldElementSize = null;
      }, r.prototype.updateSize = function() {
        this.originalSize || (this.size = this.isHorizontal() ? this.$element[0].clientWidth : this
          .$element[0].clientHeight, this.handleScroll_(), this.repeater && this.repeater
          .containerUpdated());
      }, r.prototype.getScrollSize = function() {
        return this.scrollSize;
      }, r.prototype.getDimensionName_ = function() {
        return this.isHorizontal() ? "width" : "height";
      }, r.prototype.sizeScroller_ = function(e) {
        var t = this.getDimensionName_(),
          n = this.isHorizontal() ? "height" : "width";
        if (this.sizer.innerHTML = "", e < this.maxElementPixels) this.sizer.style[t] = e + "px";
        else {
          this.sizer.style[t] = "auto", this.sizer.style[n] = "auto";
          var r = Math.floor(e / this.maxElementPixels),
            i = document.createElement("div");
          i.style[t] = this.maxElementPixels + "px", i.style[n] = "1px";
          for (var o = 0; o < r; o++) this.sizer.appendChild(i.cloneNode(!1));
          i.style[t] = e - r * this.maxElementPixels + "px", this.sizer.appendChild(i);
        }
      }, r.prototype.autoShrink_ = function(e) {
        var t = Math.max(e, this.autoShrinkMin * this.repeater.getItemSize());
        if (this.autoShrink && t !== this.size) {
          null === this.oldElementSize && (this.oldElementSize = this.$element[0].style[this
            .getDimensionName_()]);
          var n = this.originalSize || this.size;
          if (!n || t < n) this.originalSize || (this.originalSize = this.size), this.setSize_(t);
          else if (null !== this.originalSize) {
            this.unsetSize_();
            var r = this.originalSize;
            this.originalSize = null, r || this.updateSize(), this.setSize_(r || this.size);
          }
          this.repeater.containerUpdated();
        }
      }, r.prototype.setScrollSize = function(e) {
        var t = e + this.offsetSize;
        this.scrollSize !== t && (this.sizeScroller_(t), this.autoShrink_(t), this.scrollSize = t);
      }, r.prototype.getScrollOffset = function() {
        return this.scrollOffset;
      }, r.prototype.scrollTo = function(e) {
        this.scroller[this.isHorizontal() ? "scrollLeft" : "scrollTop"] = e, this.handleScroll_();
      }, r.prototype.scrollToIndex = function(e) {
        var t = this.repeater.getItemSize(),
          n = this.repeater.itemsLength;
        e > n && (e = n - 1), this.scrollTo(t * e);
      }, r.prototype.resetScroll = function() {
        this.scrollTo(0);
      }, r.prototype.handleScroll_ = function() {
        var e = "rtl" != document.dir && "rtl" != document.body.dir;
        e || this.maxSize || (this.scroller.scrollLeft = this.scrollSize, this.maxSize = this.scroller
          .scrollLeft);
        var t = this.isHorizontal() ? e ? this.scroller.scrollLeft : this.maxSize - this.scroller
          .scrollLeft : this.scroller.scrollTop;
        if (!(t === this.scrollOffset || t > this.scrollSize - this.size)) {
          var n = this.repeater.getItemSize();
          if (n) {
            var r = Math.max(0, Math.floor(t / n) - s),
              i = (this.isHorizontal() ? "translateX(" : "translateY(") + (!this.isHorizontal() || e ? r *
                n : -(r * n)) + "px)";
            if (this.scrollOffset = t, this.offsetter.style.webkitTransform = i, this.offsetter.style
              .transform = i, this.bindTopIndex) {
              var o = Math.floor(t / n);
              o !== this.topIndex && o < this.repeater.getItemCount() && (this.topIndex = o, this
                .bindTopIndex.assign(this.$scope, o), this.$rootScope.$$phase || this.$scope.$digest());
            }
            this.repeater.containerUpdated();
          }
        }
      }, o.Block, o.prototype.link_ = function(e, n, r, i, o) {
        this.container = e, this.transclude = n, this.repeatName = r, this.rawRepeatListExpression = i, this
          .extraName = o, this.sized = !1, this.repeatListExpression = t.bind(this, this
            .repeatListExpression_), this.container.register(this);
      }, o.prototype.cleanupBlocks_ = function() {
        t.forEach(this.pooledBlocks, function(e) {
          e.element.remove();
        });
      }, o.prototype.readItemSize_ = function() {
        if (!this.itemSize) {
          this.items = this.repeatListExpression(this.$scope), this.parentNode = this.$element[0]
          .parentNode;
          var e = this.getBlock_(0);
          e.element[0].parentNode || this.parentNode.appendChild(e.element[0]), this.itemSize = e.element[0]
            [this.container.isHorizontal() ? "offsetWidth" : "offsetHeight"] || null, this.blocks[0] = e,
            this.poolBlock_(0), this.itemSize && this.containerUpdated();
        }
      }, o.prototype.repeatListExpression_ = function(e) {
        var t = this.rawRepeatListExpression(e);
        if (this.onDemand && t) {
          var n = new a(t);
          return n.$$includeIndexes(this.newStartIndex, this.newVisibleEnd), n;
        }
        return t;
      }, o.prototype.containerUpdated = function() {
        return this.itemSize ? (this.sized || (this.items = this.repeatListExpression(this.$scope)), this
          .sized || (this.unwatchItemSize_(), this.sized = !0, this.$scope.$watchCollection(this
            .repeatListExpression, t.bind(this, function(e, t) {
              this.isVirtualRepeatUpdating_ || this.virtualRepeatUpdate_(e, t);
            }))), this.updateIndexes_(), void((this.newStartIndex !== this.startIndex || this
            .newEndIndex !== this.endIndex || this.container.getScrollOffset() > this.container
            .getScrollSize()) && (this.items instanceof a && this.items.$$includeIndexes(this
            .newStartIndex, this.newEndIndex), this.virtualRepeatUpdate_(this.items, this.items)))) : (
          this.unwatchItemSize_ && this.unwatchItemSize_ !== t.noop && this.unwatchItemSize_(), this
          .unwatchItemSize_ = this.$scope.$watchCollection(this.repeatListExpression, t.bind(this,
            function(e) {
              e && e.length && this.readItemSize_();
            })), void(this.$rootScope.$$phase || this.$scope.$digest()));
      }, o.prototype.getItemSize = function() {
        return this.itemSize;
      }, o.prototype.getItemCount = function() {
        return this.itemsLength;
      }, o.prototype.virtualRepeatUpdate_ = function(e, n) {
        this.isVirtualRepeatUpdating_ = !0;
        var r = e && e.length || 0,
          i = !1;
        if (this.items && r < this.items.length && 0 !== this.container.getScrollOffset()) {
          this.items = e;
          var o = this.container.getScrollOffset();
          this.container.resetScroll(), this.container.scrollTo(o);
        }
        if (r !== this.itemsLength && (i = !0, this.itemsLength = r), this.items = e, (e !== n || i) && this
          .updateIndexes_(), this.parentNode = this.$element[0].parentNode, i && this.container
          .setScrollSize(r * this.itemSize), this.isFirstRender) {
          this.isFirstRender = !1;
          var a = this.$attrs.mdStartIndex ? this.$scope.$eval(this.$attrs.mdStartIndex) : this.container
            .topIndex;
          this.container.scrollToIndex(a);
        }
        Object.keys(this.blocks).forEach(function(e) {
          var t = parseInt(e, 10);
          (t < this.newStartIndex || t >= this.newEndIndex) && this.poolBlock_(t);
        }, this), this.$browser.$$checkUrlChange = t.noop;
        var s,
          c,
          u = [],
          l = [];
        for (s = this.newStartIndex; s < this.newEndIndex && null == this.blocks[s]; s++) c = this
          .getBlock_(s), this.updateBlock_(c, s), u.push(c);
        for (; null != this.blocks[s]; s++) this.updateBlock_(this.blocks[s], s);
        for (var d = s - 1; s < this.newEndIndex; s++) c = this.getBlock_(s), this.updateBlock_(c, s), l
          .push(c);
        u.length && this.parentNode.insertBefore(this.domFragmentFromBlocks_(u), this.$element[0]
            .nextSibling), l.length && this.parentNode.insertBefore(this.domFragmentFromBlocks_(l), this
            .blocks[d] && this.blocks[d].element[0].nextSibling), this.$browser.$$checkUrlChange = this
          .browserCheckUrlChange, this.startIndex = this.newStartIndex, this.endIndex = this.newEndIndex,
          this.isVirtualRepeatUpdating_ = !1;
      }, o.prototype.getBlock_ = function(e) {
        if (this.pooledBlocks.length) return this.pooledBlocks.pop();
        var n;
        return this.transclude(t.bind(this, function(t, r) {
          n = {
            element: t,
            new: !0,
            scope: r
          }, this.updateScope_(r, e), this.parentNode.appendChild(t[0]);
        })), n;
      }, o.prototype.updateBlock_ = function(e, t) {
        this.blocks[t] = e, (e.new || e.scope.$index !== t || e.scope[this.repeatName] !== this.items[t]) &&
          (e.new = !1, this.updateScope_(e.scope, t), this.$rootScope.$$phase || e.scope.$digest());
      }, o.prototype.updateScope_ = function(e, t) {
        e.$index = t, e[this.repeatName] = this.items && this.items[t], this.extraName && (e[this.extraName(
          this.$scope)] = this.items[t]);
      }, o.prototype.poolBlock_ = function(e) {
        this.pooledBlocks.push(this.blocks[e]), this.parentNode.removeChild(this.blocks[e].element[0]),
          delete this.blocks[e];
      }, o.prototype.domFragmentFromBlocks_ = function(e) {
        var t = this.$document[0].createDocumentFragment();
        return e.forEach(function(e) {
          t.appendChild(e.element[0]);
        }), t;
      }, o.prototype.updateIndexes_ = function() {
        var e = this.items ? this.items.length : 0,
          t = Math.ceil(this.container.getSize() / this.itemSize);
        this.newStartIndex = Math.max(0, Math.min(e - t, Math.floor(this.container.getScrollOffset() / this
          .itemSize))), this.newVisibleEnd = this.newStartIndex + t + s, this.newEndIndex = Math.min(e,
          this.newVisibleEnd), this.newStartIndex = Math.max(0, this.newStartIndex - s);
      }, a.prototype.$$includeIndexes = function(e, t) {
        for (var n = e; n < t; n++) this.hasOwnProperty(n) || (this[n] = this.model.getItemAtIndex(n));
        this.length = this.model.getLength();
      };
    }(),
    function() {
      function e(e) {
        function t(t, a, s) {
          var c = "";
          s.$observe("mdWhiteframe", function(t) {
            t = parseInt(t, 10) || o, t != n && (t > i || t < r) && (e.warn(
              "md-whiteframe attribute value is invalid. It should be a number between " + r +
              " and " + i, a[0]), t = o);
            var u = t == n ? "" : "md-whiteframe-" + t + "dp";
            s.$updateClass(u, c), c = u;
          });
        }
        var n = -1,
          r = 1,
          i = 24,
          o = 4;
        return {
          link: t
        };
      }
      e.$inject = ["$log"], t.module("material.components.whiteframe", ["material.core"]).directive(
        "mdWhiteframe", e);
    }(),
    function() {
      function e(e, s, c, u, l, d, f, h, p, m, v, g) {
        function y() {
          c.initOptionalProperties(e, p, {
            searchText: "",
            selectedItem: null,
            clearButton: !1
          }), l(s), w(), c.nextTick(function() {
            x(), _(), e.autofocus && s.on("focus", $);
          });
        }

        function b() {
          e.requireMatch && Ie && Ie.$setValidity("md-require-match", !!e.selectedItem || !e.searchText);
        }

        function E() {
          function t() {
            var e = 0,
              t = s.find("md-input-container");
            if (t.length) {
              var n = t.find("input");
              e = t.prop("offsetHeight"), e -= n.prop("offsetTop"), e -= n.prop("offsetHeight"), e += t.prop(
                "offsetTop");
            }
            return e;
          }

          function n() {
            var e = Ce.scrollContainer.getBoundingClientRect(),
              t = {};
            e.right > h.right - o && (t.left = d.right - e.width + "px"), Ce.$.scrollContainer.css(t);
          }
          if (!Ce) return c.nextTick(E, !1, e);
          var u,
            l = (e.dropdownItems || i) * r,
            d = Ce.wrap.getBoundingClientRect(),
            f = Ce.snap.getBoundingClientRect(),
            h = Ce.root.getBoundingClientRect(),
            m = f.bottom - h.top,
            v = h.bottom - f.top,
            g = d.left - h.left,
            y = d.width,
            b = t(),
            _ = e.dropdownPosition;
          if (_ || (_ = m > v && h.height - d.bottom - o < l ? "top" : "bottom"), p.mdFloatingLabel && (g +=
              a, y -= 2 * a), u = {
              left: g + "px",
              minWidth: y + "px",
              maxWidth: Math.max(d.right - h.left, h.right - d.left) - o + "px"
            }, "top" === _) u.top = "auto", u.bottom = v + "px", u.maxHeight = Math.min(l, d.top - h.top -
            o) + "px";
          else {
            var $ = h.bottom - d.bottom - o + c.getViewportTop();
            u.top = m - b + "px", u.bottom = "auto", u.maxHeight = Math.min(l, $) + "px";
          }
          Ce.$.scrollContainer.css(u), c.nextTick(n, !1);
        }

        function _() {
          Ce.$.root.length && (l(Ce.$.scrollContainer), Ce.$.scrollContainer.detach(), Ce.$.root.append(Ce.$
            .scrollContainer), f.pin && f.pin(Ce.$.scrollContainer, h));
        }

        function $() {
          Ce.input.focus();
        }

        function w() {
          var n = parseInt(e.delay, 10) || 0;
          p.$observe("disabled", function(e) {
              $e.isDisabled = c.parseAttributeBoolean(e, !1);
            }), p.$observe("required", function(e) {
              $e.isRequired = c.parseAttributeBoolean(e, !1);
            }), p.$observe("readonly", function(e) {
              $e.isReadonly = c.parseAttributeBoolean(e, !1);
            }), e.$watch("searchText", n ? c.debounce(j, n) : j), e.$watch("selectedItem", D), t.element(d)
            .on("resize", Oe), e.$on("$destroy", T);
        }

        function T() {
          if ($e.hidden || c.enableScrolling(), t.element(d).off("resize", Oe), Ce) {
            var e = ["ul", "scroller", "scrollContainer", "input"];
            t.forEach(e, function(e) {
              Ce.$[e].remove();
            });
          }
        }

        function C() {
          $e.hidden || E();
        }

        function x() {
          var e = S();
          Ce = {
            main: s[0],
            scrollContainer: s[0].querySelector(".md-virtual-repeat-container"),
            scroller: s[0].querySelector(".md-virtual-repeat-scroller"),
            ul: s.find("ul")[0],
            input: s.find("input")[0],
            wrap: e.wrap,
            snap: e.snap,
            root: document.body
          }, Ce.li = Ce.ul.getElementsByTagName("li"), Ce.$ = A(Ce), Ie = Ce.$.input.controller("ngModel");
        }

        function S() {
          var e, n;
          for (e = s; e.length && (n = e.attr("md-autocomplete-snap"), !t.isDefined(n)); e = e.parent());
          if (e.length) return {
            snap: e[0],
            wrap: "width" === n.toLowerCase() ? e[0] : s.find("md-autocomplete-wrap")[0]
          };
          var r = s.find("md-autocomplete-wrap")[0];
          return {
            snap: r,
            wrap: r
          };
        }

        function A(e) {
          var n = {};
          for (var r in e) e.hasOwnProperty(r) && (n[r] = t.element(e[r]));
          return n;
        }

        function M(e, n) {
          !e && n ? (E(), fe(!0, De.Count | De.Selected), Ce && (c.disableScrollAround(Ce.ul), Ne = k(t
            .element(Ce.wrap)))) : e && !n && (c.enableScrolling(), Ne && (Ne(), Ne = null));
        }

        function k(e) {
          function t(e) {
            e.preventDefault();
          }
          return e.on("wheel", t), e.on("touchmove", t),
            function() {
              e.off("wheel", t), e.off("touchmove", t);
            };
        }

        function N() {
          Se = !0;
        }

        function I() {
          Me || $e.hidden || Ce.input.focus(), Se = !1, $e.hidden = X();
        }

        function O() {
          Ce.input.focus();
        }

        function D(n, r) {
          b(), n ? V(n).then(function(t) {
            e.searchText = t, L(n, r);
          }) : r && e.searchText && V(r).then(function(n) {
            t.isString(e.searchText) && n.toString().toLowerCase() === e.searchText.toLowerCase() && (e
              .searchText = "");
          }), n !== r && R();
        }

        function R() {
          t.isFunction(e.itemChange) && e.itemChange(W(e.selectedItem));
        }

        function P() {
          t.isFunction(e.textChange) && e.textChange();
        }

        function L(e, t) {
          Ae.forEach(function(n) {
            n(e, t);
          });
        }

        function U(e) {
          Ae.indexOf(e) == -1 && Ae.push(e);
        }

        function F(e) {
          var t = Ae.indexOf(e);
          t != -1 && Ae.splice(t, 1);
        }

        function j(t, n) {
          $e.index = Y(), t !== n && (b(), V(e.selectedItem).then(function(r) {
            t !== r && (e.selectedItem = null, t !== n && P(), oe() ? ye() : ($e.matches = [], K(!1),
              fe(!1, De.Count)));
          }));
        }

        function H(e) {
          Me = !1, Se || ($e.hidden = X(), _e("ngBlur", {
            $event: e
          }));
        }

        function B(e) {
          e && (Se = !1, Me = !1), Ce.input.blur();
        }

        function z(e) {
          Me = !0, Q() && oe() && ye(), $e.hidden = X(), _e("ngFocus", {
            $event: e
          });
        }

        function q(t) {
          switch (t.keyCode) {
            case u.KEY_CODE.DOWN_ARROW:
              if ($e.loading) return;
              t.stopPropagation(), t.preventDefault(), $e.index = Math.min($e.index + 1, $e.matches.length -
                1), pe(), fe(!1, De.Selected);
              break;
            case u.KEY_CODE.UP_ARROW:
              if ($e.loading) return;
              t.stopPropagation(), t.preventDefault(), $e.index = $e.index < 0 ? $e.matches.length - 1 : Math
                .max(0, $e.index - 1), pe(), fe(!1, De.Selected);
              break;
            case u.KEY_CODE.TAB:
              if (I(), $e.hidden || $e.loading || $e.index < 0 || $e.matches.length < 1) return;
              se($e.index);
              break;
            case u.KEY_CODE.ENTER:
              if ($e.hidden || $e.loading || $e.index < 0 || $e.matches.length < 1) return;
              if (ne()) return;
              t.stopPropagation(), t.preventDefault(), se($e.index);
              break;
            case u.KEY_CODE.ESCAPE:
              if (t.preventDefault(), !J()) return;
              t.stopPropagation(), ue(), e.searchText && Z("clear") && le(), $e.hidden = !0, Z("blur") && B(!
                0);
          }
        }

        function G() {
          return t.isNumber(e.minLength) ? e.minLength : 1;
        }

        function V(n) {
          function r(t) {
            return t && e.itemText ? e.itemText(W(t)) : null;
          }
          return m.when(r(n) || n).then(function(e) {
            return e && !t.isString(e) && v.warn(
              "md-autocomplete: Could not resolve display value to a string. Please check the `md-item-text` attribute."
              ), e;
          });
        }

        function W(e) {
          if (!e) return n;
          var t = {};
          return $e.itemName && (t[$e.itemName] = e), t;
        }

        function Y() {
          return e.autoselect ? 0 : -1;
        }

        function K(e) {
          $e.loading != e && ($e.loading = e), $e.hidden = X();
        }

        function X() {
          return !Q() || !ee();
        }

        function Q() {
          return !($e.loading && !te()) && !ne() && !!Me;
        }

        function J() {
          return Z("blur") || !$e.hidden || $e.loading || Z("clear") && e.searchText;
        }

        function Z(t) {
          return !e.escapeOptions || e.escapeOptions.toLowerCase().indexOf(t) !== -1;
        }

        function ee() {
          return oe() && te() || ge();
        }

        function te() {
          return !!$e.matches.length;
        }

        function ne() {
          return !!$e.scope.selectedItem;
        }

        function re() {
          return $e.loading && !ne();
        }

        function ie() {
          return V($e.matches[$e.index]);
        }

        function oe() {
          return (e.searchText || "").length >= G();
        }

        function ae(e, t, n) {
          Object.defineProperty($e, e, {
            get: function() {
              return n;
            },
            set: function(e) {
              var r = n;
              n = e, t(e, r);
            }
          });
        }

        function se(t) {
          c.nextTick(function() {
            V($e.matches[t]).then(function(e) {
              var t = Ce.$.input.controller("ngModel");
              t.$setViewValue(e), t.$render();
            }).finally(function() {
              e.selectedItem = $e.matches[t], K(!1);
            });
          }, !1);
        }

        function ce() {
          ue(), le();
        }

        function ue() {
          $e.index = 0, $e.matches = [];
        }

        function le() {
          K(!0), e.searchText = "";
          var t = document.createEvent("CustomEvent");
          t.initCustomEvent("change", !0, !0, {
            value: ""
          }), Ce.input.dispatchEvent(t), Ce.input.blur(), e.searchText = "", Ce.input.focus();
        }

        function de(n) {
          function r(t) {
            t && (t = m.when(t), ke++, K(!0), c.nextTick(function() {
              t.then(i).finally(function() {
                0 === --ke && K(!1);
              });
            }, !0, e));
          }

          function i(t) {
            xe[a] = t, (n || "") === (e.searchText || "") && be(t);
          }
          var o = e.$parent.$eval(Te),
            a = n.toLowerCase(),
            s = t.isArray(o),
            u = !!o.then;
          s ? i(o) : u && r(o);
        }

        function fe(e, t) {
          var n = e ? "polite" : "assertive",
            r = [];
          t & De.Selected && $e.index !== -1 && r.push(ie()), t & De.Count && r.push(m.resolve(he())), m.all(
            r).then(function(e) {
            g.announce(e.join(" "), n);
          });
        }

        function he() {
          switch ($e.matches.length) {
            case 0:
              return "There are no matches available.";
            case 1:
              return "There is 1 match available.";
            default:
              return "There are " + $e.matches.length + " matches available.";
          }
        }

        function pe() {
          if (Ce.li[0]) {
            var e = Ce.li[0].offsetHeight,
              t = e * $e.index,
              n = t + e,
              r = Ce.scroller.clientHeight,
              i = Ce.scroller.scrollTop;
            t < i ? ve(t) : n > i + r && ve(n - r);
          }
        }

        function me() {
          return 0 !== ke;
        }

        function ve(e) {
          Ce.$.scrollContainer.controller("mdVirtualRepeatContainer").scrollTo(e);
        }

        function ge() {
          var e = ($e.scope.searchText || "").length;
          return $e.hasNotFound && !te() && (!$e.loading || me()) && e >= G() && (Me || Se) && !ne();
        }

        function ye() {
          var t = e.searchText || "",
            n = t.toLowerCase();
          !e.noCache && xe[n] ? be(xe[n]) : de(t), $e.hidden = X();
        }

        function be(t) {
          $e.matches = t, $e.hidden = X(), $e.loading && K(!1), e.selectOnMatch && Ee(), E(), fe(!0, De
          .Count);
        }

        function Ee() {
          var t = e.searchText,
            n = $e.matches,
            r = n[0];
          1 === n.length && V(r).then(function(n) {
            var r = t == n;
            e.matchInsensitive && !r && (r = t.toLowerCase() == n.toLowerCase()), r && se(0);
          });
        }

        function _e(t, n) {
          p[t] && e.$parent.$eval(p[t], n || {});
        }
        var $e = this,
          we = e.itemsExpr.split(/ in /i),
          Te = we[1],
          Ce = null,
          xe = {},
          Se = !1,
          Ae = [],
          Me = !1,
          ke = 0,
          Ne = null,
          Ie = null,
          Oe = c.debounce(C);
        ae("hidden", M, !0), $e.scope = e, $e.parent = e.$parent, $e.itemName = we[0], $e.matches = [], $e
          .loading = !1, $e.hidden = !0, $e.index = null, $e.id = c.nextUid(), $e.isDisabled = null, $e
          .isRequired = null, $e.isReadonly = null, $e.hasNotFound = !1, $e.keydown = q, $e.blur = H, $e
          .focus = z, $e.clear = ce, $e.select = se, $e.listEnter = N, $e.listLeave = I, $e.mouseUp = O, $e
          .getCurrentDisplayValue = ie, $e.registerSelectedItemWatcher = U, $e.unregisterSelectedItemWatcher =
          F, $e.notFoundVisible = ge, $e.loadingIsVisible = re, $e.positionDropdown = E;
        var De = {
          Count: 1,
          Selected: 2
        };
        return y();
      }
      e.$inject = ["$scope", "$element", "$mdUtil", "$mdConstant", "$mdTheming", "$window", "$animate",
        "$rootElement", "$attrs", "$q", "$log", "$mdLiveAnnouncer"
      ], t.module("material.components.autocomplete").controller("MdAutocompleteCtrl", e);
      var r = 48,
        i = 5,
        o = 8,
        a = 2;
    }(),
    function() {
      function e(e) {
        return {
          controller: "MdAutocompleteCtrl",
          controllerAs: "$mdAutocompleteCtrl",
          scope: {
            inputName: "@mdInputName",
            inputMinlength: "@mdInputMinlength",
            inputMaxlength: "@mdInputMaxlength",
            searchText: "=?mdSearchText",
            selectedItem: "=?mdSelectedItem",
            itemsExpr: "@mdItems",
            itemText: "&mdItemText",
            placeholder: "@placeholder",
            noCache: "=?mdNoCache",
            requireMatch: "=?mdRequireMatch",
            selectOnMatch: "=?mdSelectOnMatch",
            matchInsensitive: "=?mdMatchCaseInsensitive",
            itemChange: "&?mdSelectedItemChange",
            textChange: "&?mdSearchTextChange",
            minLength: "=?mdMinLength",
            delay: "=?mdDelay",
            autofocus: "=?mdAutofocus",
            floatingLabel: "@?mdFloatingLabel",
            autoselect: "=?mdAutoselect",
            menuClass: "@?mdMenuClass",
            inputId: "@?mdInputId",
            escapeOptions: "@?mdEscapeOptions",
            dropdownItems: "=?mdDropdownItems",
            dropdownPosition: "@?mdDropdownPosition",
            clearButton: "=?mdClearButton"
          },
          compile: function(e, n) {
            var r = ["md-select-on-focus", "md-no-asterisk", "ng-trim", "ng-pattern"],
              i = e.find("input");
            return r.forEach(function(e) {
                var t = n[n.$normalize(e)];
                null !== t && i.attr(e, t);
              }),
              function(e, n, r, i) {
                i.hasNotFound = !!n.attr("md-has-not-found"), t.isDefined(r.mdClearButton) || e
                  .floatingLabel || (e.clearButton = !0);
              };
          },
          template: function(t, n) {
            function r() {
              var e = t.find("md-item-template").detach(),
                n = e.length ? e.html() : t.html();
              return e.length || t.empty(), "<md-autocomplete-parent-scope md-autocomplete-replace>" + n +
                "</md-autocomplete-parent-scope>";
            }

            function i() {
              var e = t.find("md-not-found").detach(),
                n = e.length ? e.html() : "";
              return n ?
                '<li ng-if="$mdAutocompleteCtrl.notFoundVisible()"\t                         md-autocomplete-parent-scope>' +
                n + "</li>" : "";
            }

            function o() {
              return n.mdFloatingLabel ?
                '\t            <md-input-container ng-if="floatingLabel">\t              <label>{{floatingLabel}}</label>\t              <input type="search"\t                  ' +
                (null != l ? 'tabindex="' + l + '"' : "") +
                '\t                  id="{{ inputId || \'fl-input-\' + $mdAutocompleteCtrl.id }}"\t                  name="{{inputName}}"\t                  autocomplete="off"\t                  ng-required="$mdAutocompleteCtrl.isRequired"\t                  ng-readonly="$mdAutocompleteCtrl.isReadonly"\t                  ng-minlength="inputMinlength"\t                  ng-maxlength="inputMaxlength"\t                  ng-disabled="$mdAutocompleteCtrl.isDisabled"\t                  ng-model="$mdAutocompleteCtrl.scope.searchText"\t                  ng-model-options="{ allowInvalid: true }"\t                  ng-keydown="$mdAutocompleteCtrl.keydown($event)"\t                  ng-blur="$mdAutocompleteCtrl.blur($event)"\t                  ng-focus="$mdAutocompleteCtrl.focus($event)"\t                  aria-owns="ul-{{$mdAutocompleteCtrl.id}}"\t                  aria-label="{{floatingLabel}}"\t                  aria-autocomplete="list"\t                  role="combobox"\t                  aria-haspopup="true"\t                  aria-activedescendant=""\t                  aria-expanded="{{!$mdAutocompleteCtrl.hidden}}"/>\t              <div md-autocomplete-parent-scope md-autocomplete-replace>' +
                u + "</div>\t            </md-input-container>" :
                '\t            <input type="search"\t                ' + (null != l ? 'tabindex="' + l +
                  '"' : "") +
                '\t                id="{{ inputId || \'input-\' + $mdAutocompleteCtrl.id }}"\t                name="{{inputName}}"\t                ng-if="!floatingLabel"\t                autocomplete="off"\t                ng-required="$mdAutocompleteCtrl.isRequired"\t                ng-disabled="$mdAutocompleteCtrl.isDisabled"\t                ng-readonly="$mdAutocompleteCtrl.isReadonly"\t                ng-minlength="inputMinlength"\t                ng-maxlength="inputMaxlength"\t                ng-model="$mdAutocompleteCtrl.scope.searchText"\t                ng-keydown="$mdAutocompleteCtrl.keydown($event)"\t                ng-blur="$mdAutocompleteCtrl.blur($event)"\t                ng-focus="$mdAutocompleteCtrl.focus($event)"\t                placeholder="{{placeholder}}"\t                aria-owns="ul-{{$mdAutocompleteCtrl.id}}"\t                aria-label="{{placeholder}}"\t                aria-autocomplete="list"\t                role="combobox"\t                aria-haspopup="true"\t                aria-activedescendant=""\t                aria-expanded="{{!$mdAutocompleteCtrl.hidden}}"/>';
            }

            function a() {
              return '<button type="button" aria-label="Clear Input" tabindex="-1" ng-if="clearButton && $mdAutocompleteCtrl.scope.searchText && !$mdAutocompleteCtrl.isDisabled" ng-click="$mdAutocompleteCtrl.clear($event)"><md-icon md-svg-src="' +
                e.mdClose + '"></md-icon></button>';
            }
            var s = i(),
              c = r(),
              u = t.html(),
              l = n.tabindex;
            return s && t.attr("md-has-not-found", !0), t.attr("tabindex", "-1"),
              "\t        <md-autocomplete-wrap\t            ng-class=\"{ 'md-whiteframe-z1': !floatingLabel, \t                        'md-menu-showing': !$mdAutocompleteCtrl.hidden, \t                        'md-show-clear-button': !!clearButton }\">\t          " +
              o() + "\t          " + a() + '\t          <md-progress-linear\t              class="' + (n
                .mdFloatingLabel ? "md-inline" : "") +
              '"\t              ng-if="$mdAutocompleteCtrl.loadingIsVisible()"\t              md-mode="indeterminate"></md-progress-linear>\t          <md-virtual-repeat-container\t              md-auto-shrink\t              md-auto-shrink-min="1"\t              ng-mouseenter="$mdAutocompleteCtrl.listEnter()"\t              ng-mouseleave="$mdAutocompleteCtrl.listLeave()"\t              ng-mouseup="$mdAutocompleteCtrl.mouseUp()"\t              ng-hide="$mdAutocompleteCtrl.hidden"\t              class="md-autocomplete-suggestions-container md-whiteframe-z1"\t              ng-class="{ \'md-not-found\': $mdAutocompleteCtrl.notFoundVisible() }"\t              role="presentation">\t            <ul class="md-autocomplete-suggestions"\t                ng-class="::menuClass"\t                id="ul-{{$mdAutocompleteCtrl.id}}">\t              <li md-virtual-repeat="item in $mdAutocompleteCtrl.matches"\t                  ng-class="{ selected: $index === $mdAutocompleteCtrl.index }"\t                  ng-click="$mdAutocompleteCtrl.select($index)"\t                  md-extra-name="$mdAutocompleteCtrl.itemName">\t                  ' +
              c + "\t                  </li>" + s +
              "\t            </ul>\t          </md-virtual-repeat-container>\t        </md-autocomplete-wrap>";
          }
        };
      }
      e.$inject = ["$$mdSvgRegistry"], t.module("material.components.autocomplete").directive(
        "mdAutocomplete", e);
    }(),
    function() {
      function e(e, t) {
        function n(e, n, r) {
          return function(e, n, i) {
            function o(n, r) {
              c[r] = e[n], e.$watch(n, function(e) {
                t.nextTick(function() {
                  c[r] = e;
                });
              });
            }

            function a() {
              var t = !1,
                n = !1;
              e.$watch(function() {
                n || t || (t = !0, e.$$postDigest(function() {
                  n || c.$digest(), t = n = !1;
                }));
              }), c.$watch(function() {
                n = !0;
              });
            }
            var s = e.$mdAutocompleteCtrl,
              c = s.parent.$new(),
              u = s.itemName;
            o("$index", "$index"), o("item", u), a(), r(c, function(e) {
              n.after(e);
            });
          };
        }
        return {
          restrict: "AE",
          compile: n,
          terminal: !0,
          transclude: "element"
        };
      }
      e.$inject = ["$compile", "$mdUtil"], t.module("material.components.autocomplete").directive(
        "mdAutocompleteParentScope", e);
    }(),
    function() {
      function e(e, t, n) {
        this.$scope = e, this.$element = t, this.$attrs = n, this.regex = null;
      }
      e.$inject = ["$scope", "$element", "$attrs"], t.module("material.components.autocomplete").controller(
        "MdHighlightCtrl", e), e.prototype.init = function(e, t) {
        this.flags = this.$attrs.mdHighlightFlags || "", this.unregisterFn = this.$scope.$watch(function(
        n) {
          return {
            term: e(n),
            contentText: t(n)
          };
        }.bind(this), this.onRender.bind(this), !0), this.$element.on("$destroy", this.unregisterFn);
      }, e.prototype.onRender = function(e, t) {
        var n = e.contentText;
        null !== this.regex && e.term === t.term || (this.regex = this.createRegex(e.term, this.flags)), e
          .term ? this.applyRegex(n) : this.$element.text(n);
      }, e.prototype.applyRegex = function(e) {
        var n = this.resolveTokens(e);
        this.$element.empty(), n.forEach(function(e) {
          if (e.isMatch) {
            var n = t.element('<span class="highlight">').text(e.text);
            this.$element.append(n);
          } else this.$element.append(document.createTextNode(e));
        }.bind(this));
      }, e.prototype.resolveTokens = function(e) {
        function t(t, r) {
          var i = e.slice(t, r);
          i && n.push(i);
        }
        var n = [],
          r = 0;
        return e.replace(this.regex, function(e, i) {
          t(r, i), n.push({
            text: e,
            isMatch: !0
          }), r = i + e.length;
        }), t(r), n;
      }, e.prototype.createRegex = function(e, t) {
        var n = "",
          r = "",
          i = this.sanitizeRegex(e);
        return t.indexOf("^") >= 0 && (n = "^"), t.indexOf("$") >= 0 && (r = "$"), new RegExp(n + i + r, t
          .replace(/[$\^]/g, ""));
      }, e.prototype.sanitizeRegex = function(e) {
        return e && e.toString().replace(/[\\\^\$\*\+\?\.\(\)\|\{}\[\]]/g, "\\$&");
      };
    }(),
    function() {
      function e(e, t) {
        return {
          terminal: !0,
          controller: "MdHighlightCtrl",
          compile: function(n, r) {
            var i = t(r.mdHighlightText),
              o = e(n.html());
            return function(e, t, n, r) {
              r.init(i, o);
            };
          }
        };
      }
      e.$inject = ["$interpolate", "$parse"], t.module("material.components.autocomplete").directive(
        "mdHighlightText", e);
    }(),
    function() {
      function r(e, t, r, i, o) {
        this.$scope = e, this.$element = t, this.$mdConstant = r, this.$timeout = i, this.$mdUtil = o, this
          .isEditting = !1, this.parentController = n, this.enableChipEdit = !1;
      }
      r.$inject = ["$scope", "$element", "$mdConstant", "$timeout", "$mdUtil"], t.module(
        "material.components.chips").controller("MdChipCtrl", r), r.prototype.init = function(e) {
        this.parentController = e, this.enableChipEdit = this.parentController.enableChipEdit, this
          .enableChipEdit && (this.$element.on("keydown", this.chipKeyDown.bind(this)), this.$element.on(
            "mousedown", this.chipMouseDown.bind(this)), this.getChipContent().addClass(
            "_md-chip-content-edit-is-enabled"));
      }, r.prototype.getChipContent = function() {
        var e = this.$element[0].getElementsByClassName("md-chip-content");
        return t.element(e[0]);
      }, r.prototype.getContentElement = function() {
        return t.element(this.getChipContent().children()[0]);
      }, r.prototype.getChipIndex = function() {
        return parseInt(this.$element.attr("index"));
      }, r.prototype.goOutOfEditMode = function() {
        if (this.isEditting) {
          this.isEditting = !1, this.$element.removeClass("_md-chip-editing"), this.getChipContent()[0]
            .contentEditable = "false";
          var e = this.getChipIndex(),
            t = this.getContentElement().text();
          t ? (this.parentController.updateChipContents(e, this.getContentElement().text()), this.$mdUtil
            .nextTick(function() {
              this.parentController.selectedChip === e && this.parentController.focusChip(e);
            }.bind(this))) : this.parentController.removeChipAndFocusInput(e);
        }
      }, r.prototype.selectNodeContents = function(t) {
        var n, r;
        document.body.createTextRange ? (n = document.body.createTextRange(), n.moveToElementText(t), n
          .select()) : e.getSelection && (r = e.getSelection(), n = document.createRange(), n
          .selectNodeContents(t), r.removeAllRanges(), r.addRange(n));
      }, r.prototype.goInEditMode = function() {
        this.isEditting = !0, this.$element.addClass("_md-chip-editing"), this.getChipContent()[0]
          .contentEditable = "true", this.getChipContent().on("blur", function() {
            this.goOutOfEditMode();
          }.bind(this)), this.selectNodeContents(this.getChipContent()[0]);
      }, r.prototype.chipKeyDown = function(e) {
        this.isEditting || e.keyCode !== this.$mdConstant.KEY_CODE.ENTER && e.keyCode !== this.$mdConstant
          .KEY_CODE.SPACE ? this.isEditting && e.keyCode === this.$mdConstant.KEY_CODE.ENTER && (e
            .preventDefault(), this.goOutOfEditMode()) : (e.preventDefault(), this.goInEditMode());
      }, r.prototype.chipMouseDown = function() {
        this.getChipIndex() == this.parentController.selectedChip && this.enableChipEdit && !this
          .isEditting && this.goInEditMode();
      };
    }(),
    function() {
      function e(e, r, i, o) {
        function a(n, r, a, c) {
          var u = c.shift(),
            l = c.shift(),
            d = t.element(r[0].querySelector(".md-chip-content"));
          e(r), u && (l.init(u), d.append(i(s)(n)), d.on("blur", function() {
            u.resetSelectedChip(), u.$scope.$applyAsync();
          })), o(function() {
            u && u.shouldFocusLastChip && u.focusLastChipThenInput();
          });
        }
        var s = r.processTemplate(n);
        return {
          restrict: "E",
          require: ["^?mdChips", "mdChip"],
          link: a,
          controller: "MdChipCtrl"
        };
      }
      e.$inject = ["$mdTheming", "$mdUtil", "$compile", "$timeout"], t.module("material.components.chips")
        .directive("mdChip", e);
      var n =
        '\t    <span ng-if="!$mdChipsCtrl.readonly" class="md-visually-hidden">\t      {{$mdChipsCtrl.deleteHint}}\t    </span>';
    }(),
    function() {
      function e(e) {
        function t(t, n, r, i) {
          n.on("click", function(e) {
            t.$apply(function() {
              i.removeChip(t.$$replacedScope.$index);
            });
          }), e(function() {
            n.attr({
              tabindex: -1,
              "aria-hidden": !0
            }), n.find("button").attr("tabindex", "-1");
          });
        }
        return {
          restrict: "A",
          require: "^mdChips",
          scope: !1,
          link: t
        };
      }
      e.$inject = ["$timeout"], t.module("material.components.chips").directive("mdChipRemove", e);
    }(),
    function() {
      function e(e) {
        function t(t, n, r) {
          var i = t.$parent.$mdChipsCtrl,
            o = i.parent.$new(!1, i.parent);
          o.$$replacedScope = t, o.$chip = t.$chip, o.$index = t.$index, o.$mdChipsCtrl = i;
          var a = i.$scope.$eval(r.mdChipTransclude);
          n.html(a), e(n.contents())(o);
        }
        return {
          restrict: "EA",
          terminal: !0,
          link: t,
          scope: !1
        };
      }
      e.$inject = ["$compile"], t.module("material.components.chips").directive("mdChipTransclude", e);
    }(),
    function() {
      function e(e, t, r, i, o, a, s) {
        this.$timeout = a, this.$mdConstant = r, this.$scope = e, this.parent = e.$parent, this.$mdUtil = s,
          this.$log = i, this.$element = o, this.$attrs = t, this.ngModelCtrl = null, this
          .userInputNgModelCtrl = null, this.autocompleteCtrl = null, this.userInputElement = null, this
          .items = [], this.selectedChip = -1, this.enableChipEdit = s.parseAttributeBoolean(t
            .mdEnableChipEdit), this.addOnBlur = s.parseAttributeBoolean(t.mdAddOnBlur), this.inputAriaLabel =
          "Chips input.", this.containerHint = "Chips container. Use arrow keys to select chips.", this
          .deleteHint = "Press delete to remove this chip.", this.deleteButtonLabel = "Remove", this
          .chipBuffer = "", this.useTransformChip = !1, this.useOnAdd = !1, this.useOnRemove = !1, this
          .wrapperId = "", this.contentIds = [], this.ariaTabIndex = null, this.chipAppendDelay = n, this
          .init();
      }
      e.$inject = ["$scope", "$attrs", "$mdConstant", "$log", "$element", "$timeout", "$mdUtil"];
      var n = 300;
      t.module("material.components.chips").controller("MdChipsCtrl", e), e.prototype.init = function() {
        var e = this;
        e.wrapperId = "_md-chips-wrapper-" + e.$mdUtil.nextUid(), e.$scope.$watchCollection(
          "$mdChipsCtrl.items",
          function() {
            e.setupInputAria(), e.setupWrapperAria();
          }), e.$attrs.$observe("mdChipAppendDelay", function(t) {
          e.chipAppendDelay = parseInt(t) || n;
        });
      }, e.prototype.setupInputAria = function() {
        var e = this.$element.find("input");
        e && (e.attr("role", "textbox"), e.attr("aria-multiline", !0));
      }, e.prototype.setupWrapperAria = function() {
        var e = this,
          t = this.$element.find("md-chips-wrap");
        this.items && this.items.length ? (t.attr("role", "listbox"), this.contentIds = this.items.map(
          function() {
            return e.wrapperId + "-chip-" + e.$mdUtil.nextUid();
          }), t.attr("aria-owns", this.contentIds.join(" "))) : (t.removeAttr("role"), t.removeAttr(
          "aria-owns"));
      }, e.prototype.inputKeydown = function(e) {
        var t = this.getChipBuffer();
        if (!(this.autocompleteCtrl && e.isDefaultPrevented && e.isDefaultPrevented())) {
          if (e.keyCode === this.$mdConstant.KEY_CODE.BACKSPACE) {
            if (0 !== this.getCursorPosition(e.target)) return;
            return e.preventDefault(), e.stopPropagation(), void(this.items.length && this
              .selectAndFocusChipSafe(this.items.length - 1));
          }
          if ((!this.separatorKeys || this.separatorKeys.length < 1) && (this.separatorKeys = [this
              .$mdConstant.KEY_CODE.ENTER
            ]), this.separatorKeys.indexOf(e.keyCode) !== -1) {
            if (this.autocompleteCtrl && this.requireMatch || !t) return;
            if (e.preventDefault(), this.hasMaxChipsReached()) return;
            return this.appendChip(t.trim()), this.resetChipBuffer(), !1;
          }
        }
      }, e.prototype.getCursorPosition = function(e) {
        try {
          if (e.selectionStart === e.selectionEnd) return e.selectionStart;
        } catch (t) {
          if (!e.value) return 0;
        }
      }, e.prototype.updateChipContents = function(e, t) {
        e >= 0 && e < this.items.length && (this.items[e] = t, this.ngModelCtrl.$setDirty());
      }, e.prototype.isEditingChip = function() {
        return !!this.$element[0].querySelector("._md-chip-editing");
      }, e.prototype.isRemovable = function() {
        return !!this.ngModelCtrl && (this.readonly ? this.removable : !t.isDefined(this.removable) || this
          .removable);
      }, e.prototype.chipKeydown = function(e) {
        if (!this.getChipBuffer() && !this.isEditingChip()) switch (e.keyCode) {
          case this.$mdConstant.KEY_CODE.BACKSPACE:
          case this.$mdConstant.KEY_CODE.DELETE:
            if (this.selectedChip < 0) return;
            if (e.preventDefault(), !this.isRemovable()) return;
            this.removeAndSelectAdjacentChip(this.selectedChip);
            break;
          case this.$mdConstant.KEY_CODE.LEFT_ARROW:
            e.preventDefault(), (this.selectedChip < 0 || this.readonly && 0 == this.selectedChip) && (
              this.selectedChip = this.items.length), this.items.length && this.selectAndFocusChipSafe(
              this.selectedChip - 1);
            break;
          case this.$mdConstant.KEY_CODE.RIGHT_ARROW:
            e.preventDefault(), this.selectAndFocusChipSafe(this.selectedChip + 1);
            break;
          case this.$mdConstant.KEY_CODE.ESCAPE:
          case this.$mdConstant.KEY_CODE.TAB:
            if (this.selectedChip < 0) return;
            e.preventDefault(), this.onFocus();
        }
      }, e.prototype.getPlaceholder = function() {
        var e = this.items && this.items.length && ("" == this.secondaryPlaceholder || this
          .secondaryPlaceholder);
        return e ? this.secondaryPlaceholder : this.placeholder;
      }, e.prototype.removeAndSelectAdjacentChip = function(e) {
        var t = this,
          n = t.getAdjacentChipIndex(e);
        this.$element[0].querySelector("md-chips-wrap"), this.$element[0].querySelector('md-chip[index="' +
          e + '"]');
        t.removeChip(e), t.$timeout(function() {
          t.$timeout(function() {
            t.selectAndFocusChipSafe(n);
          });
        });
      }, e.prototype.resetSelectedChip = function() {
        this.selectedChip = -1, this.ariaTabIndex = null;
      }, e.prototype.getAdjacentChipIndex = function(e) {
        var t = this.items.length - 1;
        return 0 == t ? -1 : e == t ? e - 1 : e;
      }, e.prototype.appendChip = function(e) {
        if (this.shouldFocusLastChip = !0, this.useTransformChip && this.transformChip) {
          var n = this.transformChip({
            $chip: e
          });
          t.isDefined(n) && (e = n);
        }
        if (t.isObject(e)) {
          var r = this.items.some(function(n) {
            return t.equals(e, n);
          });
          if (r) return;
        }
        if (!(null == e || this.items.indexOf(e) + 1)) {
          var i = this.items.push(e),
            o = i - 1;
          this.ngModelCtrl.$setDirty(), this.validateModel(), this.useOnAdd && this.onAdd && this.onAdd({
            $chip: e,
            $index: o
          });
        }
      }, e.prototype.useTransformChipExpression = function() {
        this.useTransformChip = !0;
      }, e.prototype.useOnAddExpression = function() {
        this.useOnAdd = !0;
      }, e.prototype.useOnRemoveExpression = function() {
        this.useOnRemove = !0;
      }, e.prototype.useOnSelectExpression = function() {
        this.useOnSelect = !0;
      }, e.prototype.getChipBuffer = function() {
        var e = this.userInputElement ? this.userInputNgModelCtrl ? this.userInputNgModelCtrl.$viewValue :
          this.userInputElement[0].value : this.chipBuffer;
        return t.isString(e) ? e : "";
      }, e.prototype.resetChipBuffer = function() {
        this.userInputElement ? this.userInputNgModelCtrl ? (this.userInputNgModelCtrl.$setViewValue(""),
            this.userInputNgModelCtrl.$render()) : this.userInputElement[0].value = "" : this.chipBuffer =
          "";
      }, e.prototype.hasMaxChipsReached = function() {
        return t.isString(this.maxChips) && (this.maxChips = parseInt(this.maxChips, 10) || 0), this
          .maxChips > 0 && this.items.length >= this.maxChips;
      }, e.prototype.validateModel = function() {
        this.ngModelCtrl.$setValidity("md-max-chips", !this.hasMaxChipsReached());
      }, e.prototype.removeChip = function(e) {
        var t = this.items.splice(e, 1);
        this.ngModelCtrl.$setDirty(), this.validateModel(), t && t.length && this.useOnRemove && this
          .onRemove && this.onRemove({
            $chip: t[0],
            $index: e
          });
      }, e.prototype.removeChipAndFocusInput = function(e) {
        this.removeChip(e), this.autocompleteCtrl ? (this.autocompleteCtrl.hidden = !0, this.$mdUtil
          .nextTick(this.onFocus.bind(this))) : this.onFocus();
      }, e.prototype.selectAndFocusChipSafe = function(e) {
        if (!this.items.length || e === -1) return this.focusInput();
        if (e >= this.items.length) {
          if (!this.readonly) return this.onFocus();
          e = 0;
        }
        e = Math.max(e, 0), e = Math.min(e, this.items.length - 1), this.selectChip(e), this.focusChip(e);
      }, e.prototype.focusLastChipThenInput = function() {
        var e = this;
        e.shouldFocusLastChip = !1, e.focusChip(this.items.length - 1), e.$timeout(function() {
          e.focusInput();
        }, e.chipAppendDelay);
      }, e.prototype.focusInput = function() {
        this.selectChip(-1), this.onFocus();
      }, e.prototype.selectChip = function(e) {
        e >= -1 && e <= this.items.length ? (this.selectedChip = e, this.useOnSelect && this.onSelect &&
          this.onSelect({
            $chip: this.items[e]
          })) : this.$log.warn("Selected Chip index out of bounds; ignoring.");
      }, e.prototype.selectAndFocusChip = function(e) {
        this.selectChip(e), e != -1 && this.focusChip(e);
      }, e.prototype.focusChip = function(e) {
        var t = this.$element[0].querySelector('md-chip[index="' + e + '"] .md-chip-content');
        this.ariaTabIndex = e, t.focus();
      }, e.prototype.configureNgModel = function(e) {
        this.ngModelCtrl = e;
        var t = this;
        e.$render = function() {
          t.items = t.ngModelCtrl.$viewValue;
        };
      }, e.prototype.onFocus = function() {
        var e = this.$element[0].querySelector("input");
        e && e.focus(), this.resetSelectedChip();
      }, e.prototype.onInputFocus = function() {
        this.inputHasFocus = !0, this.setupInputAria(), this.resetSelectedChip();
      }, e.prototype.onInputBlur = function() {
        this.inputHasFocus = !1, this.shouldAddOnBlur() && (this.appendChip(this.getChipBuffer().trim()),
          this.resetChipBuffer());
      }, e.prototype.configureUserInput = function(e) {
        this.userInputElement = e;
        var n = e.controller("ngModel");
        n != this.ngModelCtrl && (this.userInputNgModelCtrl = n);
        var r = this.$scope,
          i = this,
          o = function(e, n) {
            r.$evalAsync(t.bind(i, n, e));
          };
        e.attr({
          tabindex: 0
        }).on("keydown", function(e) {
          o(e, i.inputKeydown);
        }).on("focus", function(e) {
          o(e, i.onInputFocus);
        }).on("blur", function(e) {
          o(e, i.onInputBlur);
        });
      }, e.prototype.configureAutocomplete = function(e) {
        e && (this.autocompleteCtrl = e, e.registerSelectedItemWatcher(t.bind(this, function(e) {
          if (e) {
            if (this.hasMaxChipsReached()) return;
            this.appendChip(e), this.resetChipBuffer();
          }
        })), this.$element.find("input").on("focus", t.bind(this, this.onInputFocus)).on("blur", t.bind(
          this, this.onInputBlur)));
      }, e.prototype.shouldAddOnBlur = function() {
        this.validateModel();
        var e = this.getChipBuffer().trim(),
          t = this.ngModelCtrl.$valid,
          n = this.autocompleteCtrl && !this.autocompleteCtrl.hidden;
        return this.userInputNgModelCtrl && (t = t && this.userInputNgModelCtrl.$valid), this.addOnBlur && !
          this.requireMatch && e && t && !n;
      }, e.prototype.hasFocus = function() {
        return this.inputHasFocus || this.selectedChip >= 0;
      }, e.prototype.contentIdFor = function(e) {
        return this.contentIds[e];
      };
    }(),
    function() {
      function e(e, t, a, s, c, u) {
        function l(n, r) {
          function i(e) {
            if (r.ngModel) {
              var t = o[0].querySelector(e);
              return t && t.outerHTML;
            }
          }
          var o = r.$mdUserTemplate;
          r.$mdUserTemplate = null;
          var l = i("md-chips>md-chip-template"),
            d = t.prefixer().buildList("md-chip-remove").map(function(e) {
              return "md-chips>*[" + e + "]";
            }).join(","),
            h = i(d) || f.remove,
            p = l || f.default,
            m = i("md-chips>md-autocomplete") || i("md-chips>input") || f.input,
            v = o.find("md-chip");
          return o[0].querySelector("md-chip-template>*[md-chip-remove]") && s.warn(
              "invalid placement of md-chip-remove within md-chip-template."),
            function(n, i, o, s) {
              t.initOptionalProperties(n, r), e(i);
              var d = s[0];
              if (l && (d.enableChipEdit = !1), d.chipContentsTemplate = p, d.chipRemoveTemplate = h, d
                .chipInputTemplate = m, d.mdCloseIcon = u.mdClose, i.attr({
                  tabindex: -1
                }).on("focus", function() {
                  d.onFocus();
                }), r.ngModel && (d.configureNgModel(i.controller("ngModel")), o.mdTransformChip && d
                  .useTransformChipExpression(), o.mdOnAppend && d.useOnAppendExpression(), o.mdOnAdd && d
                  .useOnAddExpression(), o.mdOnRemove && d.useOnRemoveExpression(), o.mdOnSelect && d
                  .useOnSelectExpression(), m != f.input && n.$watch("$mdChipsCtrl.readonly", function(e) {
                    e || t.nextTick(function() {
                      if (0 === m.indexOf("<md-autocomplete")) {
                        var e = i.find("md-autocomplete");
                        d.configureAutocomplete(e.controller("mdAutocomplete"));
                      }
                      d.configureUserInput(i.find("input"));
                    });
                  }), t.nextTick(function() {
                    var e = i.find("input");
                    e && e.toggleClass("md-input", !0);
                  })), v.length > 0) {
                var g = a(v.clone())(n.$parent);
                c(function() {
                  i.find("md-chips-wrap").prepend(g);
                });
              }
            };
        }

        function d() {
          return {
            chips: t.processTemplate(n),
            input: t.processTemplate(r),
            default: t.processTemplate(i),
            remove: t.processTemplate(o)
          };
        }
        var f = d();
        return {
          template: function(e, t) {
            return t.$mdUserTemplate = e.clone(), f.chips;
          },
          require: ["mdChips"],
          restrict: "E",
          controller: "MdChipsCtrl",
          controllerAs: "$mdChipsCtrl",
          bindToController: !0,
          compile: l,
          scope: {
            readonly: "=readonly",
            removable: "=mdRemovable",
            placeholder: "@",
            secondaryPlaceholder: "@",
            maxChips: "@mdMaxChips",
            transformChip: "&mdTransformChip",
            onAppend: "&mdOnAppend",
            onAdd: "&mdOnAdd",
            onRemove: "&mdOnRemove",
            onSelect: "&mdOnSelect",
            inputAriaLabel: "@",
            containerHint: "@",
            deleteHint: "@",
            deleteButtonLabel: "@",
            separatorKeys: "=?mdSeparatorKeys",
            requireMatch: "=?mdRequireMatch",
            chipAppendDelayString: "@?mdChipAppendDelay"
          }
        };
      }
      e.$inject = ["$mdTheming", "$mdUtil", "$compile", "$log", "$timeout", "$$mdSvgRegistry"], t.module(
        "material.components.chips").directive("mdChips", e);
      var n =
        '\t      <md-chips-wrap\t          id="{{$mdChipsCtrl.wrapperId}}"\t          tabindex="{{$mdChipsCtrl.readonly ? 0 : -1}}"\t          ng-keydown="$mdChipsCtrl.chipKeydown($event)"\t          ng-class="{ \'md-focused\': $mdChipsCtrl.hasFocus(), \t                      \'md-readonly\': !$mdChipsCtrl.ngModelCtrl || $mdChipsCtrl.readonly,\t                      \'md-removable\': $mdChipsCtrl.isRemovable() }"\t          aria-setsize="{{$mdChipsCtrl.items.length}}"\t          class="md-chips">\t        <span ng-if="$mdChipsCtrl.readonly" class="md-visually-hidden">\t          {{$mdChipsCtrl.containerHint}}\t        </span>\t        <md-chip ng-repeat="$chip in $mdChipsCtrl.items"\t            index="{{$index}}"\t            ng-class="{\'md-focused\': $mdChipsCtrl.selectedChip == $index, \'md-readonly\': !$mdChipsCtrl.ngModelCtrl || $mdChipsCtrl.readonly}">\t          <div class="md-chip-content"\t              tabindex="{{$mdChipsCtrl.ariaTabIndex == $index ? 0 : -1}}"\t              id="{{$mdChipsCtrl.contentIdFor($index)}}"\t              role="option"\t              aria-selected="{{$mdChipsCtrl.selectedChip == $index}}" \t              aria-posinset="{{$index}}"\t              ng-click="!$mdChipsCtrl.readonly && $mdChipsCtrl.focusChip($index)"\t              ng-focus="!$mdChipsCtrl.readonly && $mdChipsCtrl.selectChip($index)"\t              md-chip-transclude="$mdChipsCtrl.chipContentsTemplate"></div>\t          <div ng-if="$mdChipsCtrl.isRemovable()"\t               class="md-chip-remove-container"\t               tabindex="-1"\t               md-chip-transclude="$mdChipsCtrl.chipRemoveTemplate"></div>\t        </md-chip>\t        <div class="md-chip-input-container" ng-if="!$mdChipsCtrl.readonly && $mdChipsCtrl.ngModelCtrl">\t          <div md-chip-transclude="$mdChipsCtrl.chipInputTemplate"></div>\t        </div>\t      </md-chips-wrap>',
        r =
        '\t        <input\t            class="md-input"\t            tabindex="0"\t            aria-label="{{$mdChipsCtrl.inputAriaLabel}}" \t            placeholder="{{$mdChipsCtrl.getPlaceholder()}}"\t            ng-model="$mdChipsCtrl.chipBuffer"\t            ng-focus="$mdChipsCtrl.onInputFocus()"\t            ng-blur="$mdChipsCtrl.onInputBlur()"\t            ng-keydown="$mdChipsCtrl.inputKeydown($event)">',
        i = "\t      <span>{{$chip}}</span>",
        o =
        '\t      <button\t          class="md-chip-remove"\t          ng-if="$mdChipsCtrl.isRemovable()"\t          ng-click="$mdChipsCtrl.removeChipAndFocusInput($$replacedScope.$index)"\t          type="button"\t          tabindex="-1">\t        <md-icon md-svg-src="{{ $mdChipsCtrl.mdCloseIcon }}"></md-icon>\t        <span class="md-visually-hidden">\t          {{$mdChipsCtrl.deleteButtonLabel}}\t        </span>\t      </button>';
    }(),
    function() {
      function e() {
        this.selectedItem = null, this.searchText = "";
      }
      t.module("material.components.chips").controller("MdContactChipsCtrl", e), e.prototype.queryContact =
        function(e) {
          return this.contactQuery({
            $query: e
          });
        }, e.prototype.itemName = function(e) {
          return e[this.contactName];
        };
    }(),
    function() {
      function e(e, t) {
        function r(n, r) {
          return function(n, i, o, a) {
            var s = a;
            t.initOptionalProperties(n, r), e(i), i.attr("tabindex", "-1"), o.$observe("mdChipAppendDelay",
              function(e) {
                s.chipAppendDelay = e;
              });
          };
        }
        return {
          template: function(e, t) {
            return n;
          },
          restrict: "E",
          controller: "MdContactChipsCtrl",
          controllerAs: "$mdContactChipsCtrl",
          bindToController: !0,
          compile: r,
          scope: {
            contactQuery: "&mdContacts",
            placeholder: "@",
            secondaryPlaceholder: "@",
            contactName: "@mdContactName",
            contactImage: "@mdContactImage",
            contactEmail: "@mdContactEmail",
            contacts: "=ngModel",
            requireMatch: "=?mdRequireMatch",
            minLength: "=?mdMinLength",
            highlightFlags: "@?mdHighlightFlags",
            chipAppendDelay: "@?mdChipAppendDelay"
          }
        };
      }
      e.$inject = ["$mdTheming", "$mdUtil"], t.module("material.components.chips").directive("mdContactChips",
        e);
      var n =
        '\t      <md-chips class="md-contact-chips"\t          ng-model="$mdContactChipsCtrl.contacts"\t          md-require-match="$mdContactChipsCtrl.requireMatch"\t          md-chip-append-delay="{{$mdContactChipsCtrl.chipAppendDelay}}" \t          md-autocomplete-snap>\t          <md-autocomplete\t              md-menu-class="md-contact-chips-suggestions"\t              md-selected-item="$mdContactChipsCtrl.selectedItem"\t              md-search-text="$mdContactChipsCtrl.searchText"\t              md-items="item in $mdContactChipsCtrl.queryContact($mdContactChipsCtrl.searchText)"\t              md-item-text="$mdContactChipsCtrl.itemName(item)"\t              md-no-cache="true"\t              md-min-length="$mdContactChipsCtrl.minLength"\t              md-autoselect\t              placeholder="{{$mdContactChipsCtrl.contacts.length == 0 ?\t                  $mdContactChipsCtrl.placeholder : $mdContactChipsCtrl.secondaryPlaceholder}}">\t            <div class="md-contact-suggestion">\t              <img \t                  ng-src="{{item[$mdContactChipsCtrl.contactImage]}}"\t                  alt="{{item[$mdContactChipsCtrl.contactName]}}"\t                  ng-if="item[$mdContactChipsCtrl.contactImage]" />\t              <span class="md-contact-name" md-highlight-text="$mdContactChipsCtrl.searchText"\t                    md-highlight-flags="{{$mdContactChipsCtrl.highlightFlags}}">\t                {{item[$mdContactChipsCtrl.contactName]}}\t              </span>\t              <span class="md-contact-email" >{{item[$mdContactChipsCtrl.contactEmail]}}</span>\t            </div>\t          </md-autocomplete>\t          <md-chip-template>\t            <div class="md-contact-avatar">\t              <img \t                  ng-src="{{$chip[$mdContactChipsCtrl.contactImage]}}"\t                  alt="{{$chip[$mdContactChipsCtrl.contactName]}}"\t                  ng-if="$chip[$mdContactChipsCtrl.contactImage]" />\t            </div>\t            <div class="md-contact-name">\t              {{$chip[$mdContactChipsCtrl.contactName]}}\t            </div>\t          </md-chip-template>\t      </md-chips>';
    }(),
    function() {
      ! function() {
        function e() {
          return {
            template: function(e, t) {
              var n = t.hasOwnProperty("ngIf") ? "" : 'ng-if="calendarCtrl.isInitialized"',
                r = '<div ng-switch="calendarCtrl.currentView" ' + n +
                '><md-calendar-year ng-switch-when="year"></md-calendar-year><md-calendar-month ng-switch-default></md-calendar-month></div>';
              return r;
            },
            scope: {
              minDate: "=mdMinDate",
              maxDate: "=mdMaxDate",
              dateFilter: "=mdDateFilter",
              _currentView: "@mdCurrentView"
            },
            require: ["ngModel", "mdCalendar"],
            controller: n,
            controllerAs: "calendarCtrl",
            bindToController: !0,
            link: function(e, t, n, r) {
              var i = r[0],
                o = r[1];
              o.configureNgModel(i);
            }
          };
        }

        function n(e, n, r, o, a, s, c, u, l) {
          s(e), this.$element = e, this.$scope = n, this.dateUtil = r, this.$mdUtil = o, this.keyCode = a
            .KEY_CODE, this.$$rAF = c, this.$mdDateLocale = l, this.today = this.dateUtil
            .createDateAtMidnight(), this.ngModelCtrl = null, this.SELECTED_DATE_CLASS =
            "md-calendar-selected-date", this.TODAY_CLASS = "md-calendar-date-today", this
            .FOCUSED_DATE_CLASS = "md-focus", this.id = i++, this.displayDate = null, this.selectedDate =
            null, this.firstRenderableDate = null, this.lastRenderableDate = null, this.isInitialized = !1,
            this.width = 0, this.scrollbarWidth = 0, u.tabindex || e.attr("tabindex", "-1");
          var d,
            f = t.bind(this, this.handleKeyEvent);
          d = e.parent().hasClass("md-datepicker-calendar") ? t.element(document.body) : e, d.on("keydown",
            f), n.$on("$destroy", function() {
              d.off("keydown", f);
            }), 1 === t.version.major && t.version.minor <= 4 && this.$onInit();
        }
        n.$inject = ["$element", "$scope", "$$mdDateUtil", "$mdUtil", "$mdConstant", "$mdTheming", "$$rAF",
          "$attrs", "$mdDateLocale"
        ], t.module("material.components.datepicker").directive("mdCalendar", e);
        var r = 340,
          i = 0;
        n.prototype.$onInit = function() {
          this.currentView = this._currentView || "month";
          var e = this.$mdDateLocale;
          this.minDate && this.minDate > e.firstRenderableDate ? this.firstRenderableDate = this.minDate :
            this.firstRenderableDate = e.firstRenderableDate, this.maxDate && this.maxDate < e
            .lastRenderableDate ? this.lastRenderableDate = this.maxDate : this.lastRenderableDate = e
            .lastRenderableDate;
        }, n.prototype.configureNgModel = function(e) {
          var t = this;
          t.ngModelCtrl = e, t.$mdUtil.nextTick(function() {
            t.isInitialized = !0;
          }), e.$render = function() {
            var e = this.$viewValue;
            t.$scope.$broadcast("md-calendar-parent-changed", e), t.selectedDate || (t.selectedDate = e),
              t.displayDate || (t.displayDate = t.selectedDate || t.today);
          };
        }, n.prototype.setNgModelValue = function(e) {
          var t = this.dateUtil.createDateAtMidnight(e);
          return this.focus(t), this.$scope.$emit("md-calendar-change", t), this.ngModelCtrl.$setViewValue(
            t), this.ngModelCtrl.$render(), t;
        }, n.prototype.setCurrentView = function(e, n) {
          var r = this;
          r.$mdUtil.nextTick(function() {
            r.currentView = e, n && (r.displayDate = t.isDate(n) ? n : new Date(n));
          });
        }, n.prototype.focus = function(e) {
          if (this.dateUtil.isValidDate(e)) {
            var t = this.$element[0].querySelector(".md-focus");
            t && t.classList.remove(this.FOCUSED_DATE_CLASS);
            var n = this.getDateId(e, this.currentView),
              r = document.getElementById(n);
            r && (r.classList.add(this.FOCUSED_DATE_CLASS), r.focus(), this.displayDate = e);
          } else {
            var i = this.$element[0].querySelector("[ng-switch]");
            i && i.focus();
          }
        }, n.prototype.getActionFromKeyEvent = function(e) {
          var t = this.keyCode;
          switch (e.which) {
            case t.ENTER:
              return "select";
            case t.RIGHT_ARROW:
              return "move-right";
            case t.LEFT_ARROW:
              return "move-left";
            case t.DOWN_ARROW:
              return e.metaKey ? "move-page-down" : "move-row-down";
            case t.UP_ARROW:
              return e.metaKey ? "move-page-up" : "move-row-up";
            case t.PAGE_DOWN:
              return "move-page-down";
            case t.PAGE_UP:
              return "move-page-up";
            case t.HOME:
              return "start";
            case t.END:
              return "end";
            default:
              return null;
          }
        }, n.prototype.handleKeyEvent = function(e) {
          var t = this;
          this.$scope.$apply(function() {
            if (e.which == t.keyCode.ESCAPE || e.which == t.keyCode.TAB) return t.$scope.$emit(
              "md-calendar-close"), void(e.which == t.keyCode.TAB && e.preventDefault());
            var n = t.getActionFromKeyEvent(e);
            n && (e.preventDefault(), e.stopPropagation(), t.$scope.$broadcast(
              "md-calendar-parent-action", n));
          });
        }, n.prototype.hideVerticalScrollbar = function(e) {
          function t() {
            var t = n.width || r,
              i = n.scrollbarWidth,
              a = e.calendarScroller;
            o.style.width = t + "px", a.style.width = t + i + "px", a.style.paddingRight = i + "px";
          }
          var n = this,
            i = e.$element[0],
            o = i.querySelector(".md-calendar-scroll-mask");
          n.width > 0 ? t() : n.$$rAF(function() {
            var r = e.calendarScroller;
            n.scrollbarWidth = r.offsetWidth - r.clientWidth, n.width = i.querySelector("table")
              .offsetWidth, t();
          });
        }, n.prototype.getDateId = function(e, t) {
          if (!t) throw new Error("A namespace for the date id has to be specified.");
          return ["md", this.id, t, e.getFullYear(), e.getMonth(), e.getDate()].join("-");
        }, n.prototype.updateVirtualRepeat = function() {
          var e = this.$scope,
            t = e.$on("$md-resize-enable", function() {
              e.$$phase || e.$apply(), t();
            });
        };
      }();
    }(),
    function() {
      ! function() {
        function e() {
          return {
            template: '<table aria-hidden="true" class="md-calendar-day-header"><thead></thead></table><div class="md-calendar-scroll-mask"><md-virtual-repeat-container class="md-calendar-scroll-container" md-offset-size="' +
              (i - r) +
              '"><table role="grid" tabindex="0" class="md-calendar" aria-readonly="true"><tbody md-calendar-month-body role="rowgroup" md-virtual-repeat="i in monthCtrl.items" md-month-offset="$index" class="md-calendar-month" md-start-index="monthCtrl.getSelectedMonthIndex()" md-item-size="' +
              r + '"><tr aria-hidden="true" style="height:' + r +
              'px;"></tr></tbody></table></md-virtual-repeat-container></div>',
            require: ["^^mdCalendar", "mdCalendarMonth"],
            controller: n,
            controllerAs: "monthCtrl",
            bindToController: !0,
            link: function(e, t, n, r) {
              var i = r[0],
                o = r[1];
              o.initialize(i);
            }
          };
        }

        function n(e, t, n, r, i, o) {
          this.$element = e, this.$scope = t, this.$animate = n, this.$q = r, this.dateUtil = i, this
            .dateLocale = o, this.calendarScroller = e[0].querySelector(".md-virtual-repeat-scroller"), this
            .isInitialized = !1, this.isMonthTransitionInProgress = !1;
          var a = this;
          this.cellClickHandler = function() {
            var e = i.getTimestampFromNode(this);
            a.$scope.$apply(function() {
              a.calendarCtrl.setNgModelValue(e);
            });
          }, this.headerClickHandler = function() {
            a.calendarCtrl.setCurrentView("year", i.getTimestampFromNode(this));
          };
        }
        n.$inject = ["$element", "$scope", "$animate", "$q", "$$mdDateUtil", "$mdDateLocale"], t.module(
          "material.components.datepicker").directive("mdCalendarMonth", e);
        var r = 265,
          i = 45;
        n.prototype.initialize = function(e) {
          this.items = {
              length: this.dateUtil.getMonthDistance(e.firstRenderableDate, e.lastRenderableDate) + 2
            }, this.calendarCtrl = e, this.attachScopeListeners(), e.updateVirtualRepeat(), e.ngModelCtrl &&
            e.ngModelCtrl.$render();
        }, n.prototype.getSelectedMonthIndex = function() {
          var e = this.calendarCtrl;
          return this.dateUtil.getMonthDistance(e.firstRenderableDate, e.displayDate || e.selectedDate || e
            .today);
        }, n.prototype.changeSelectedDate = function(e) {
          var t = this,
            n = t.calendarCtrl,
            r = n.selectedDate;
          n.selectedDate = e, this.changeDisplayDate(e).then(function() {
            var t = n.SELECTED_DATE_CLASS,
              i = "month";
            if (r) {
              var o = document.getElementById(n.getDateId(r, i));
              o && (o.classList.remove(t), o.setAttribute("aria-selected", "false"));
            }
            if (e) {
              var a = document.getElementById(n.getDateId(e, i));
              a && (a.classList.add(t), a.setAttribute("aria-selected", "true"));
            }
          });
        }, n.prototype.changeDisplayDate = function(e) {
          if (!this.isInitialized) return this.buildWeekHeader(), this.calendarCtrl.hideVerticalScrollbar(
            this), this.isInitialized = !0, this.$q.when();
          if (!this.dateUtil.isValidDate(e) || this.isMonthTransitionInProgress) return this.$q.when();
          this.isMonthTransitionInProgress = !0;
          var t = this.animateDateChange(e);
          this.calendarCtrl.displayDate = e;
          var n = this;
          return t.then(function() {
            n.isMonthTransitionInProgress = !1;
          }), t;
        }, n.prototype.animateDateChange = function(e) {
          if (this.dateUtil.isValidDate(e)) {
            var t = this.dateUtil.getMonthDistance(this.calendarCtrl.firstRenderableDate, e);
            this.calendarScroller.scrollTop = t * r;
          }
          return this.$q.when();
        }, n.prototype.buildWeekHeader = function() {
          for (var e = this.dateLocale.firstDayOfWeek, t = this.dateLocale.shortDays, n = document
              .createElement("tr"), r = 0; r < 7; r++) {
            var i = document.createElement("th");
            i.textContent = t[(r + e) % 7], n.appendChild(i);
          }
          this.$element.find("thead").append(n);
        }, n.prototype.attachScopeListeners = function() {
          var e = this;
          e.$scope.$on("md-calendar-parent-changed", function(t, n) {
            e.changeSelectedDate(n);
          }), e.$scope.$on("md-calendar-parent-action", t.bind(this, this.handleKeyEvent));
        }, n.prototype.handleKeyEvent = function(e, t) {
          var n = this.calendarCtrl,
            r = n.displayDate;
          if ("select" === t) n.setNgModelValue(r);
          else {
            var i = null,
              o = this.dateUtil;
            switch (t) {
              case "move-right":
                i = o.incrementDays(r, 1);
                break;
              case "move-left":
                i = o.incrementDays(r, -1);
                break;
              case "move-page-down":
                i = o.incrementMonths(r, 1);
                break;
              case "move-page-up":
                i = o.incrementMonths(r, -1);
                break;
              case "move-row-down":
                i = o.incrementDays(r, 7);
                break;
              case "move-row-up":
                i = o.incrementDays(r, -7);
                break;
              case "start":
                i = o.getFirstDateOfMonth(r);
                break;
              case "end":
                i = o.getLastDateOfMonth(r);
            }
            i && (i = this.dateUtil.clampDate(i, n.minDate, n.maxDate), this.changeDisplayDate(i).then(
              function() {
                n.focus(i);
              }));
          }
        };
      }();
    }(),
    function() {
      ! function() {
        function e(e, r) {
          var i = e('<md-icon md-svg-src="' + r.mdTabsArrow + '"></md-icon>')({})[0];
          return {
            require: ["^^mdCalendar", "^^mdCalendarMonth", "mdCalendarMonthBody"],
            scope: {
              offset: "=mdMonthOffset"
            },
            controller: n,
            controllerAs: "mdMonthBodyCtrl",
            bindToController: !0,
            link: function(e, n, r, o) {
              var a = o[0],
                s = o[1],
                c = o[2];
              c.calendarCtrl = a, c.monthCtrl = s, c.arrowIcon = i.cloneNode(!0), e.$watch(function() {
                return c.offset;
              }, function(e) {
                t.isNumber(e) && c.generateContent();
              });
            }
          };
        }

        function n(e, t, n) {
          this.$element = e, this.dateUtil = t, this.dateLocale = n, this.monthCtrl = null, this
            .calendarCtrl = null, this.offset = null, this.focusAfterAppend = null;
        }
        e.$inject = ["$compile", "$$mdSvgRegistry"], n.$inject = ["$element", "$$mdDateUtil",
          "$mdDateLocale"], t.module("material.components.datepicker").directive("mdCalendarMonthBody", e), n
          .prototype.generateContent = function() {
            var e = this.dateUtil.incrementMonths(this.calendarCtrl.firstRenderableDate, this.offset);
            this.$element.empty().append(this.buildCalendarForMonth(e)), this.focusAfterAppend && (this
              .focusAfterAppend.classList.add(this.calendarCtrl.FOCUSED_DATE_CLASS), this.focusAfterAppend
              .focus(), this.focusAfterAppend = null);
          }, n.prototype.buildDateCell = function(e) {
            var t = this.monthCtrl,
              n = this.calendarCtrl,
              r = document.createElement("td");
            if (r.tabIndex = -1, r.classList.add("md-calendar-date"), r.setAttribute("role", "gridcell"), e) {
              r.setAttribute("tabindex", "-1"), r.setAttribute("aria-label", this.dateLocale
                  .longDateFormatter(e)), r.id = n.getDateId(e, "month"), r.setAttribute("data-timestamp", e
                  .getTime()), this.dateUtil.isSameDay(e, n.today) && r.classList.add(n.TODAY_CLASS), this
                .dateUtil.isValidDate(n.selectedDate) && this.dateUtil.isSameDay(e, n.selectedDate) && (r
                  .classList.add(n.SELECTED_DATE_CLASS), r.setAttribute("aria-selected", "true"));
              var i = this.dateLocale.dates[e.getDate()];
              if (this.isDateEnabled(e)) {
                var o = document.createElement("span");
                o.classList.add("md-calendar-date-selection-indicator"), o.textContent = i, r.appendChild(o),
                  r.addEventListener("click", t.cellClickHandler), n.displayDate && this.dateUtil.isSameDay(e,
                    n.displayDate) && (this.focusAfterAppend = r);
              } else r.classList.add("md-calendar-date-disabled"), r.textContent = i;
            }
            return r;
          }, n.prototype.isDateEnabled = function(e) {
            return this.dateUtil.isDateWithinRange(e, this.calendarCtrl.minDate, this.calendarCtrl.maxDate) &&
              (!t.isFunction(this.calendarCtrl.dateFilter) || this.calendarCtrl.dateFilter(e));
          }, n.prototype.buildDateRow = function(e) {
            var t = document.createElement("tr");
            return t.setAttribute("role", "row"), t.setAttribute("aria-label", this.dateLocale
              .weekNumberFormatter(e)), t;
          }, n.prototype.buildCalendarForMonth = function(e) {
            var t = this.dateUtil.isValidDate(e) ? e : new Date(),
              n = this.dateUtil.getFirstDateOfMonth(t),
              r = this.getLocaleDay_(n),
              i = this.dateUtil.getNumberOfDaysInMonth(t),
              o = document.createDocumentFragment(),
              a = 1,
              s = this.buildDateRow(a);
            o.appendChild(s);
            var c = this.offset === this.monthCtrl.items.length - 1,
              u = 0,
              l = document.createElement("td"),
              d = document.createElement("span");
            if (d.textContent = this.dateLocale.monthHeaderFormatter(t), l.appendChild(d), l.classList.add(
                "md-calendar-month-label"), this.calendarCtrl.maxDate && n > this.calendarCtrl.maxDate ? l
              .classList.add("md-calendar-month-label-disabled") : (l.addEventListener("click", this.monthCtrl
                .headerClickHandler), l.setAttribute("data-timestamp", n.getTime()), l.setAttribute(
                "aria-label", this.dateLocale.monthFormatter(t)), l.appendChild(this.arrowIcon.cloneNode(!
                0))), r <= 2) {
              l.setAttribute("colspan", "7");
              var f = this.buildDateRow();
              if (f.appendChild(l), o.insertBefore(f, s), c) return o;
            } else u = 3, l.setAttribute("colspan", "3"), s.appendChild(l);
            for (var h = u; h < r; h++) s.appendChild(this.buildDateCell());
            for (var p = r, m = n, v = 1; v <= i; v++) {
              if (7 === p) {
                if (c) return o;
                p = 0, a++, s = this.buildDateRow(a), o.appendChild(s);
              }
              m.setDate(v);
              var g = this.buildDateCell(m);
              s.appendChild(g), p++;
            }
            for (; s.childNodes.length < 7;) s.appendChild(this.buildDateCell());
            for (; o.childNodes.length < 6;) {
              for (var y = this.buildDateRow(), b = 0; b < 7; b++) y.appendChild(this.buildDateCell());
              o.appendChild(y);
            }
            return o;
          }, n.prototype.getLocaleDay_ = function(e) {
            return (e.getDay() + (7 - this.dateLocale.firstDayOfWeek)) % 7;
          };
      }();
    }(),
    function() {
      ! function() {
        function e() {
          return {
            template: '<div class="md-calendar-scroll-mask"><md-virtual-repeat-container class="md-calendar-scroll-container"><table role="grid" tabindex="0" class="md-calendar" aria-readonly="true"><tbody md-calendar-year-body role="rowgroup" md-virtual-repeat="i in yearCtrl.items" md-year-offset="$index" class="md-calendar-year" md-start-index="yearCtrl.getFocusedYearIndex()" md-item-size="' +
              r + '"><tr aria-hidden="true" style="height:' + r +
              'px;"></tr></tbody></table></md-virtual-repeat-container></div>',
            require: ["^^mdCalendar", "mdCalendarYear"],
            controller: n,
            controllerAs: "yearCtrl",
            bindToController: !0,
            link: function(e, t, n, r) {
              var i = r[0],
                o = r[1];
              o.initialize(i);
            }
          };
        }

        function n(e, t, n, r, i) {
          this.$element = e, this.$scope = t, this.$animate = n, this.$q = r, this.dateUtil = i, this
            .calendarScroller = e[0].querySelector(".md-virtual-repeat-scroller"), this.isInitialized = !1,
            this.isMonthTransitionInProgress = !1;
          var o = this;
          this.cellClickHandler = function() {
            o.calendarCtrl.setCurrentView("month", i.getTimestampFromNode(this));
          };
        }
        n.$inject = ["$element", "$scope", "$animate", "$q", "$$mdDateUtil"], t.module(
          "material.components.datepicker").directive("mdCalendarYear", e);
        var r = 88;
        n.prototype.initialize = function(e) {
          this.items = {
              length: this.dateUtil.getYearDistance(e.firstRenderableDate, e.lastRenderableDate) + 1
            }, this.calendarCtrl = e, this.attachScopeListeners(), e.updateVirtualRepeat(), e.ngModelCtrl &&
            e.ngModelCtrl.$render();
        }, n.prototype.getFocusedYearIndex = function() {
          var e = this.calendarCtrl;
          return this.dateUtil.getYearDistance(e.firstRenderableDate, e.displayDate || e.selectedDate || e
            .today);
        }, n.prototype.changeDate = function(e) {
          if (!this.isInitialized) return this.calendarCtrl.hideVerticalScrollbar(this), this
            .isInitialized = !0, this.$q.when();
          if (this.dateUtil.isValidDate(e) && !this.isMonthTransitionInProgress) {
            var t = this,
              n = this.animateDateChange(e);
            return t.isMonthTransitionInProgress = !0, t.calendarCtrl.displayDate = e, n.then(function() {
              t.isMonthTransitionInProgress = !1;
            });
          }
        }, n.prototype.animateDateChange = function(e) {
          if (this.dateUtil.isValidDate(e)) {
            var t = this.dateUtil.getYearDistance(this.calendarCtrl.firstRenderableDate, e);
            this.calendarScroller.scrollTop = t * r;
          }
          return this.$q.when();
        }, n.prototype.handleKeyEvent = function(e, t) {
          var n = this.calendarCtrl,
            r = n.displayDate;
          if ("select" === t) this.changeDate(r).then(function() {
            n.setCurrentView("month", r), n.focus(r);
          });
          else {
            var i = null,
              o = this.dateUtil;
            switch (t) {
              case "move-right":
                i = o.incrementMonths(r, 1);
                break;
              case "move-left":
                i = o.incrementMonths(r, -1);
                break;
              case "move-row-down":
                i = o.incrementMonths(r, 6);
                break;
              case "move-row-up":
                i = o.incrementMonths(r, -6);
            }
            if (i) {
              var a = n.minDate ? o.getFirstDateOfMonth(n.minDate) : null,
                s = n.maxDate ? o.getFirstDateOfMonth(n.maxDate) : null;
              i = o.getFirstDateOfMonth(this.dateUtil.clampDate(i, a, s)), this.changeDate(i).then(
              function() {
                n.focus(i);
              });
            }
          }
        }, n.prototype.attachScopeListeners = function() {
          var e = this;
          e.$scope.$on("md-calendar-parent-changed", function(t, n) {
            e.changeDate(n);
          }), e.$scope.$on("md-calendar-parent-action", t.bind(e, e.handleKeyEvent));
        };
      }();
    }(),
    function() {
      ! function() {
        function e() {
          return {
            require: ["^^mdCalendar", "^^mdCalendarYear", "mdCalendarYearBody"],
            scope: {
              offset: "=mdYearOffset"
            },
            controller: n,
            controllerAs: "mdYearBodyCtrl",
            bindToController: !0,
            link: function(e, n, r, i) {
              var o = i[0],
                a = i[1],
                s = i[2];
              s.calendarCtrl = o, s.yearCtrl = a, e.$watch(function() {
                return s.offset;
              }, function(e) {
                t.isNumber(e) && s.generateContent();
              });
            }
          };
        }

        function n(e, t, n) {
          this.$element = e, this.dateUtil = t, this.dateLocale = n, this.calendarCtrl = null, this.yearCtrl =
            null, this.offset = null, this.focusAfterAppend = null;
        }
        n.$inject = ["$element", "$$mdDateUtil", "$mdDateLocale"], t.module("material.components.datepicker")
          .directive("mdCalendarYearBody", e), n.prototype.generateContent = function() {
            var e = this.dateUtil.incrementYears(this.calendarCtrl.firstRenderableDate, this.offset);
            this.$element.empty().append(this.buildCalendarForYear(e)), this.focusAfterAppend && (this
              .focusAfterAppend.classList.add(this.calendarCtrl.FOCUSED_DATE_CLASS), this.focusAfterAppend
              .focus(), this.focusAfterAppend = null);
          }, n.prototype.buildMonthCell = function(e, t) {
            var n = this.calendarCtrl,
              r = this.yearCtrl,
              i = this.buildBlankCell(),
              o = new Date(e, t, 1);
            i.setAttribute("aria-label", this.dateLocale.monthFormatter(o)), i.id = n.getDateId(o, "year"), i
              .setAttribute("data-timestamp", o.getTime()), this.dateUtil.isSameMonthAndYear(o, n.today) && i
              .classList.add(n.TODAY_CLASS), this.dateUtil.isValidDate(n.selectedDate) && this.dateUtil
              .isSameMonthAndYear(o, n.selectedDate) && (i.classList.add(n.SELECTED_DATE_CLASS), i
                .setAttribute("aria-selected", "true"));
            var a = this.dateLocale.shortMonths[t];
            if (this.dateUtil.isMonthWithinRange(o, n.minDate, n.maxDate)) {
              var s = document.createElement("span");
              s.classList.add("md-calendar-date-selection-indicator"), s.textContent = a, i.appendChild(s), i
                .addEventListener("click", r.cellClickHandler), n.displayDate && this.dateUtil
                .isSameMonthAndYear(o, n.displayDate) && (this.focusAfterAppend = i);
            } else i.classList.add("md-calendar-date-disabled"), i.textContent = a;
            return i;
          }, n.prototype.buildBlankCell = function() {
            var e = document.createElement("td");
            return e.tabIndex = -1, e.classList.add("md-calendar-date"), e.setAttribute("role", "gridcell"), e
              .setAttribute("tabindex", "-1"), e;
          }, n.prototype.buildCalendarForYear = function(e) {
            var t,
              n = e.getFullYear(),
              r = document.createDocumentFragment(),
              i = document.createElement("tr"),
              o = document.createElement("td");
            for (o.className = "md-calendar-month-label", o.textContent = n, i.appendChild(o), t = 0; t <
              6; t++) i.appendChild(this.buildMonthCell(n, t));
            r.appendChild(i);
            var a = document.createElement("tr");
            for (a.appendChild(this.buildBlankCell()), t = 6; t < 12; t++) a.appendChild(this.buildMonthCell(
              n, t));
            return r.appendChild(a), r;
          };
      }();
    }(),
    function() {
      ! function() {
        t.module("material.components.datepicker").config(["$provide", function(e) {
          function t() {
            this.months = null, this.shortMonths = null, this.days = null, this.shortDays = null, this
              .dates = null, this.firstDayOfWeek = 0, this.formatDate = null, this.parseDate = null,
              this.monthHeaderFormatter = null, this.weekNumberFormatter = null, this
              .longDateFormatter = null, this.msgCalendar = "", this.msgOpenCalendar = "";
          }
          t.prototype.$get = function(e, t) {
            function n(e, n) {
              if (!e) return "";
              var r = e.toLocaleTimeString(),
                i = e;
              return 0 !== e.getHours() || r.indexOf("11:") === -1 && r.indexOf("23:") === -1 || (i =
                new Date(e.getFullYear(), e.getMonth(), e.getDate(), 1, 0, 0)), t("date")(i,
                "M/d/yyyy", n);
            }

            function r(e) {
              return new Date(e);
            }

            function i(e) {
              e = e.trim();
              var t = /^(([a-zA-Z]{3,}|[0-9]{1,4})([ \.,]+|[\/\-])){2}([a-zA-Z]{3,}|[0-9]{1,4})$/;
              return t.test(e);
            }

            function o(e) {
              return v.shortMonths[e.getMonth()] + " " + e.getFullYear();
            }

            function a(e) {
              return v.months[e.getMonth()] + " " + e.getFullYear();
            }

            function s(e) {
              return "Week " + e;
            }

            function c(e) {
              return [v.days[e.getDay()], v.months[e.getMonth()], v.dates[e.getDate()], e
              .getFullYear()].join(" ");
            }
            for (var u = e.DATETIME_FORMATS.SHORTDAY.map(function(e) {
                return e.substring(0, 1);
              }), l = Array(32), d = 1; d <= 31; d++) l[d] = d;
            var f = "Calendar",
              h = "Open calendar",
              p = new Date(1880, 0, 1),
              m = new Date(p.getFullYear() + 250, 0, 1),
              v = {
                months: this.months || e.DATETIME_FORMATS.MONTH,
                shortMonths: this.shortMonths || e.DATETIME_FORMATS.SHORTMONTH,
                days: this.days || e.DATETIME_FORMATS.DAY,
                shortDays: this.shortDays || u,
                dates: this.dates || l,
                firstDayOfWeek: this.firstDayOfWeek || 0,
                formatDate: this.formatDate || n,
                parseDate: this.parseDate || r,
                isDateComplete: this.isDateComplete || i,
                monthHeaderFormatter: this.monthHeaderFormatter || o,
                monthFormatter: this.monthFormatter || a,
                weekNumberFormatter: this.weekNumberFormatter || s,
                longDateFormatter: this.longDateFormatter || c,
                msgCalendar: this.msgCalendar || f,
                msgOpenCalendar: this.msgOpenCalendar || h,
                firstRenderableDate: this.firstRenderableDate || p,
                lastRenderableDate: this.lastRenderableDate || m
              };
            return v;
          }, t.prototype.$get.$inject = ["$locale", "$filter"], e.provider("$mdDateLocale", new t());
        }]);
      }();
    }(),
    function() {
      ! function() {
        t.module("material.components.datepicker").factory("$$mdDateUtil", function() {
          function e(e) {
            return new Date(e.getFullYear(), e.getMonth(), 1);
          }

          function n(e) {
            return new Date(e.getFullYear(), e.getMonth() + 1, 0).getDate();
          }

          function r(e) {
            return new Date(e.getFullYear(), e.getMonth() + 1, 1);
          }

          function i(e) {
            return new Date(e.getFullYear(), e.getMonth() - 1, 1);
          }

          function o(e, t) {
            return e.getFullYear() === t.getFullYear() && e.getMonth() === t.getMonth();
          }

          function a(e, t) {
            return e.getDate() == t.getDate() && o(e, t);
          }

          function s(e, t) {
            var n = r(e);
            return o(n, t);
          }

          function c(e, t) {
            var n = i(e);
            return o(t, n);
          }

          function u(e, t) {
            return g((e.getTime() + t.getTime()) / 2);
          }

          function l(t) {
            var n = e(t);
            return Math.floor((n.getDay() + t.getDate() - 1) / 7);
          }

          function d(e, t) {
            return new Date(e.getFullYear(), e.getMonth(), e.getDate() + t);
          }

          function f(e, t) {
            var r = new Date(e.getFullYear(), e.getMonth() + t, 1),
              i = n(r);
            return i < e.getDate() ? r.setDate(i) : r.setDate(e.getDate()), r;
          }

          function h(e, t) {
            return 12 * (t.getFullYear() - e.getFullYear()) + (t.getMonth() - e.getMonth());
          }

          function p(e) {
            return new Date(e.getFullYear(), e.getMonth(), n(e));
          }

          function m(e) {
            return e && e.getTime && !isNaN(e.getTime());
          }

          function v(e) {
            m(e) && e.setHours(0, 0, 0, 0);
          }

          function g(e) {
            var n;
            return n = t.isUndefined(e) ? new Date() : new Date(e), v(n), n;
          }

          function y(e, t, n) {
            var r = g(e),
              i = m(t) ? g(t) : null,
              o = m(n) ? g(n) : null;
            return (!i || i <= r) && (!o || o >= r);
          }

          function b(e, t) {
            return f(e, 12 * t);
          }

          function E(e, t) {
            return t.getFullYear() - e.getFullYear();
          }

          function _(e, t, n) {
            var r = e;
            return t && e < t && (r = new Date(t.getTime())), n && e > n && (r = new Date(n.getTime())),
            r;
          }

          function $(e) {
            if (e && e.hasAttribute("data-timestamp")) return Number(e.getAttribute("data-timestamp"));
          }

          function w(e, t, n) {
            var r = e.getMonth(),
              i = e.getFullYear();
            return (!t || t.getFullYear() < i || t.getMonth() <= r) && (!n || n.getFullYear() > i || n
              .getMonth() >= r);
          }
          return {
            getFirstDateOfMonth: e,
            getNumberOfDaysInMonth: n,
            getDateInNextMonth: r,
            getDateInPreviousMonth: i,
            isInNextMonth: s,
            isInPreviousMonth: c,
            getDateMidpoint: u,
            isSameMonthAndYear: o,
            getWeekOfMonth: l,
            incrementDays: d,
            incrementMonths: f,
            getLastDateOfMonth: p,
            isSameDay: a,
            getMonthDistance: h,
            isValidDate: m,
            setDateTimeToMidnight: v,
            createDateAtMidnight: g,
            isDateWithinRange: y,
            incrementYears: b,
            getYearDistance: E,
            clampDate: _,
            getTimestampFromNode: $,
            isMonthWithinRange: w
          };
        });
      }();
    }(),
    function() {
      ! function() {
        function n(e, n, i, o) {
          return {
            template: function(t, n) {
              var r = n.mdHideIcons,
                i = n.ariaLabel || n.mdPlaceholder,
                o = "all" === r || "calendar" === r ? "" :
                '<md-button class="md-datepicker-button md-icon-button" type="button" tabindex="-1" aria-hidden="true" ng-click="ctrl.openCalendarPane($event)"><md-icon class="md-datepicker-calendar-icon" aria-label="md-calendar" md-svg-src="' +
                e.mdCalendar + '"></md-icon></md-button>',
                a = "";
              return "all" !== r && "triangle" !== r && (a =
                  '<md-button type="button" md-no-ink class="md-datepicker-triangle-button md-icon-button" ng-click="ctrl.openCalendarPane($event)" aria-label="{{::ctrl.locale.msgOpenCalendar}}"><div class="md-datepicker-expand-triangle"></div></md-button>',
                  t.addClass(u)), o +
                '<div class="md-datepicker-input-container" ng-class="{\'md-datepicker-focused\': ctrl.isFocused}"><input ' +
                (i ? 'aria-label="' + i + '" ' : "") +
                'class="md-datepicker-input" aria-haspopup="true" aria-expanded="{{ctrl.isCalendarOpen}}" ng-focus="ctrl.setFocused(true)" ng-blur="ctrl.setFocused(false)"> ' +
                a +
                '</div><div class="md-datepicker-calendar-pane md-whiteframe-z1" id="{{::ctrl.calendarPaneId}}"><div class="md-datepicker-input-mask"><div class="md-datepicker-input-mask-opaque"></div></div><div class="md-datepicker-calendar"><md-calendar role="dialog" aria-label="{{::ctrl.locale.msgCalendar}}" md-current-view="{{::ctrl.currentView}}"md-min-date="ctrl.minDate"md-max-date="ctrl.maxDate"md-date-filter="ctrl.dateFilter"ng-model="ctrl.date" ng-if="ctrl.isCalendarOpen"></md-calendar></div></div>';
            },
            require: ["ngModel", "mdDatepicker", "?^mdInputContainer", "?^form"],
            scope: {
              minDate: "=mdMinDate",
              maxDate: "=mdMaxDate",
              placeholder: "@mdPlaceholder",
              currentView: "@mdCurrentView",
              dateFilter: "=mdDateFilter",
              isOpen: "=?mdIsOpen",
              debounceInterval: "=mdDebounceInterval",
              dateLocale: "=mdDateLocale"
            },
            controller: r,
            controllerAs: "ctrl",
            bindToController: !0,
            link: function(e, r, a, u) {
              var l = u[0],
                d = u[1],
                f = u[2],
                h = u[3],
                p = n.parseAttributeBoolean(a.mdNoAsterisk);
              if (d.configureNgModel(l, f, o), f) {
                var m = r[0].querySelector(".md-errors-spacer");
                m && r.after(t.element("<div>").append(m)), f.setHasPlaceholder(a.mdPlaceholder), f.input =
                  r, f.element.addClass(s).toggleClass(c, "calendar" !== a.mdHideIcons && "all" !== a
                    .mdHideIcons), f.label ? p || a.$observe("required", function(e) {
                    f.label.toggleClass("md-required", !!e);
                  }) : i.expect(r, "aria-label", a.mdPlaceholder), e.$watch(f.isErrorGetter || function() {
                    return l.$invalid && (l.$touched || h && h.$submitted);
                  }, f.setInvalid);
              } else if (h) var v = e.$watch(function() {
                return h.$submitted;
              }, function(e) {
                e && (d.updateErrorState(), v());
              });
            }
          };
        }

        function r(n, r, i, o, a, s, c, u, l, d, f) {
          this.$window = o, this.dateUtil = l, this.$mdConstant = a, this.$mdUtil = c, this.$$rAF = d, this
            .$mdDateLocale = u, this.documentElement = t.element(document.documentElement), this.ngModelCtrl =
            null, this.inputElement = r[0].querySelector("input"), this.ngInputElement = t.element(this
              .inputElement), this.inputContainer = r[0].querySelector(".md-datepicker-input-container"), this
            .calendarPane = r[0].querySelector(".md-datepicker-calendar-pane"), this.calendarButton = r[0]
            .querySelector(".md-datepicker-button"), this.inputMask = t.element(r[0].querySelector(
              ".md-datepicker-input-mask-opaque")), this.$element = r, this.$attrs = i, this.$scope = n, this
            .date = null, this.isFocused = !1, this.isDisabled, this.setDisabled(r[0].disabled || t.isString(i
              .disabled)), this.isCalendarOpen = !1, this.openOnFocus = i.hasOwnProperty("mdOpenOnFocus"),
            this.mdInputContainer = null, this.calendarPaneOpenedFrom = null, this.calendarPaneId =
            "md-date-pane-" + c.nextUid(), this.bodyClickHandler = t.bind(this, this.handleBodyClick), this
            .windowEventName = h.test(navigator.userAgent || navigator.vendor || e.opera) ?
            "orientationchange" : "resize", this.windowEventHandler = c.debounce(t.bind(this, this
              .closeCalendarPane), 100), this.windowBlurHandler = t.bind(this, this.handleWindowBlur), this
            .ngDateFilter = f("date"), this.leftMargin = 20, this.topMargin = null, i.tabindex ? (this
              .ngInputElement.attr("tabindex", i.tabindex), i.$set("tabindex", null)) : i.$set("tabindex",
              "-1"), i.$set("aria-owns", this.calendarPaneId), s(r), s(t.element(this.calendarPane));
          var p = this;
          n.$on("$destroy", function() {
            p.detachCalendarPane();
          }), i.mdIsOpen && n.$watch("ctrl.isOpen", function(e) {
            e ? p.openCalendarPane({
              target: p.inputElement
            }) : p.closeCalendarPane();
          }), 1 === t.version.major && t.version.minor <= 4 && this.$onInit();
        }
        r.$inject = ["$scope", "$element", "$attrs", "$window", "$mdConstant", "$mdTheming", "$mdUtil",
          "$mdDateLocale", "$$mdDateUtil", "$$rAF", "$filter"
        ], n.$inject = ["$$mdSvgRegistry", "$mdUtil", "$mdAria", "inputDirective"], t.module(
          "material.components.datepicker").directive("mdDatepicker", n);
        var i = 3,
          o = "md-datepicker-invalid",
          a = "md-datepicker-open",
          s = "_md-datepicker-floating-label",
          c = "_md-datepicker-has-calendar-icon",
          u = "_md-datepicker-has-triangle-icon",
          l = 500,
          d = 368,
          f = 360,
          h = /ipad|iphone|ipod|android/i;
        r.prototype.$onInit = function() {
          this.locale = this.dateLocale ? t.extend({}, this.$mdDateLocale, this.dateLocale) : this
            .$mdDateLocale, this.installPropertyInterceptors(), this.attachChangeListeners(), this
            .attachInteractionListeners();
        }, r.prototype.configureNgModel = function(e, n, r) {
          this.ngModelCtrl = e, this.mdInputContainer = n, this.$attrs.$set("type", "date"), r[0].link.pre(
            this.$scope, {
              on: t.noop,
              val: t.noop,
              0: {}
            }, this.$attrs, [e]);
          var i = this;
          i.ngModelCtrl.$formatters.push(function(e) {
            if (e && !(e instanceof Date)) throw Error(
              "The ng-model for md-datepicker must be a Date instance. Currently the model is a: " +
              typeof e);
            return i.onExternalChange(e), e;
          }), e.$viewChangeListeners.unshift(t.bind(this, this.updateErrorState));
          var o = i.$mdUtil.getModelOption(e, "updateOn");
          o && this.ngInputElement.on(o, t.bind(this.$element, this.$element.triggerHandler, o));
        }, r.prototype.attachChangeListeners = function() {
          var e = this;
          e.$scope.$on("md-calendar-change", function(t, n) {
            e.setModelValue(n), e.onExternalChange(n), e.closeCalendarPane();
          }), e.ngInputElement.on("input", t.bind(e, e.resizeInputElement));
          var n = t.isDefined(this.debounceInterval) ? this.debounceInterval : l;
          e.ngInputElement.on("input", e.$mdUtil.debounce(e.handleInputEvent, n, e));
        }, r.prototype.attachInteractionListeners = function() {
          var e = this,
            n = this.$scope,
            r = this.$mdConstant.KEY_CODE;
          e.ngInputElement.on("keydown", function(t) {
            t.altKey && t.keyCode == r.DOWN_ARROW && (e.openCalendarPane(t), n.$digest());
          }), e.openOnFocus && (e.ngInputElement.on("focus", t.bind(e, e.openCalendarPane)), t.element(e
            .$window).on("blur", e.windowBlurHandler), n.$on("$destroy", function() {
            t.element(e.$window).off("blur", e.windowBlurHandler);
          })), n.$on("md-calendar-close", function() {
            e.closeCalendarPane();
          });
        }, r.prototype.installPropertyInterceptors = function() {
          var e = this;
          if (this.$attrs.ngDisabled) {
            var t = this.$scope.$parent;
            t && t.$watch(this.$attrs.ngDisabled, function(t) {
              e.setDisabled(t);
            });
          }
          Object.defineProperty(this, "placeholder", {
            get: function() {
              return e.inputElement.placeholder;
            },
            set: function(t) {
              e.inputElement.placeholder = t || "";
            }
          });
        }, r.prototype.setDisabled = function(e) {
          this.isDisabled = e, this.inputElement.disabled = e, this.calendarButton && (this.calendarButton
            .disabled = e);
        }, r.prototype.updateErrorState = function(e) {
          var n = e || this.date;
          if (this.clearErrorState(), this.dateUtil.isValidDate(n)) {
            if (n = this.dateUtil.createDateAtMidnight(n), this.dateUtil.isValidDate(this.minDate)) {
              var r = this.dateUtil.createDateAtMidnight(this.minDate);
              this.ngModelCtrl.$setValidity("mindate", n >= r);
            }
            if (this.dateUtil.isValidDate(this.maxDate)) {
              var i = this.dateUtil.createDateAtMidnight(this.maxDate);
              this.ngModelCtrl.$setValidity("maxdate", n <= i);
            }
            t.isFunction(this.dateFilter) && this.ngModelCtrl.$setValidity("filtered", this.dateFilter(n));
          } else this.ngModelCtrl.$setValidity("valid", null == n);
          t.element(this.inputContainer).toggleClass(o, !this.ngModelCtrl.$valid);
        }, r.prototype.clearErrorState = function() {
          this.inputContainer.classList.remove(o), ["mindate", "maxdate", "filtered", "valid"].forEach(
            function(e) {
              this.ngModelCtrl.$setValidity(e, !0);
            }, this);
        }, r.prototype.resizeInputElement = function() {
          this.inputElement.size = this.inputElement.value.length + i;
        }, r.prototype.handleInputEvent = function() {
          var e = this.inputElement.value,
            t = e ? this.locale.parseDate(e) : null;
          this.dateUtil.setDateTimeToMidnight(t);
          var n = "" == e || this.dateUtil.isValidDate(t) && this.locale.isDateComplete(e) && this
            .isDateEnabled(t);
          n && (this.setModelValue(t), this.date = t), this.updateErrorState(t);
        }, r.prototype.isDateEnabled = function(e) {
          return this.dateUtil.isDateWithinRange(e, this.minDate, this.maxDate) && (!t.isFunction(this
            .dateFilter) || this.dateFilter(e));
        }, r.prototype.attachCalendarPane = function() {
          var e = this.calendarPane,
            n = document.body;
          e.style.transform = "", this.$element.addClass(a), this.mdInputContainer && this.mdInputContainer
            .element.addClass(a), t.element(n).addClass("md-datepicker-is-showing");
          var r = this.inputContainer.getBoundingClientRect(),
            i = n.getBoundingClientRect();
          (!this.topMargin || this.topMargin < 0) && (this.topMargin = (this.inputMask.parent().prop(
            "clientHeight") - this.ngInputElement.prop("clientHeight")) / 2);
          var o = r.top - i.top - this.topMargin,
            s = r.left - i.left - this.leftMargin,
            c = i.top < 0 && 0 == document.body.scrollTop ? -i.top : document.body.scrollTop,
            u = i.left < 0 && 0 == document.body.scrollLeft ? -i.left : document.body.scrollLeft,
            l = c + this.$window.innerHeight,
            h = u + this.$window.innerWidth;
          if (this.inputMask.css({
              position: "absolute",
              left: this.leftMargin + "px",
              top: this.topMargin + "px",
              width: r.width - 1 + "px",
              height: r.height - 2 + "px"
            }), s + f > h) {
            if (h - f > 0) s = h - f;
            else {
              s = u;
              var p = this.$window.innerWidth / f;
              e.style.transform = "scale(" + p + ")";
            }
            e.classList.add("md-datepicker-pos-adjusted");
          }
          o + d > l && l - d > c && (o = l - d, e.classList.add("md-datepicker-pos-adjusted")), e.style
            .left = s + "px", e.style.top = o + "px", document.body.appendChild(e), this.$$rAF(function() {
              e.classList.add("md-pane-open");
            });
        }, r.prototype.detachCalendarPane = function() {
          this.$element.removeClass(a), this.mdInputContainer && this.mdInputContainer.element.removeClass(
              a), t.element(document.body).removeClass("md-datepicker-is-showing"), this.calendarPane
            .classList.remove("md-pane-open"), this.calendarPane.classList.remove(
              "md-datepicker-pos-adjusted"), this.isCalendarOpen && this.$mdUtil.enableScrolling(), this
            .calendarPane.parentNode && this.calendarPane.parentNode.removeChild(this.calendarPane);
        }, r.prototype.openCalendarPane = function(t) {
          if (!this.isCalendarOpen && !this.isDisabled && !this.inputFocusedOnWindowBlur) {
            this.isCalendarOpen = this.isOpen = !0, this.calendarPaneOpenedFrom = t.target, this.$mdUtil
              .disableScrollAround(this.calendarPane), this.attachCalendarPane(), this.focusCalendar(), this
              .evalAttr("ngFocus");
            var n = this;
            this.$mdUtil.nextTick(function() {
              n.documentElement.on("click touchstart", n.bodyClickHandler);
            }, !1), e.addEventListener(this.windowEventName, this.windowEventHandler);
          }
        }, r.prototype.closeCalendarPane = function() {
          function t() {
            n.isCalendarOpen = n.isOpen = !1;
          }
          if (this.isCalendarOpen) {
            var n = this;
            n.detachCalendarPane(), n.ngModelCtrl.$setTouched(), n.evalAttr("ngBlur"), n.documentElement
              .off("click touchstart", n.bodyClickHandler), e.removeEventListener(n.windowEventName, n
                .windowEventHandler), n.calendarPaneOpenedFrom.focus(), n.calendarPaneOpenedFrom = null, n
              .openOnFocus ? n.$mdUtil.nextTick(t) : t();
          }
        }, r.prototype.getCalendarCtrl = function() {
          return t.element(this.calendarPane.querySelector("md-calendar")).controller("mdCalendar");
        }, r.prototype.focusCalendar = function() {
          var e = this;
          this.$mdUtil.nextTick(function() {
            e.getCalendarCtrl().focus();
          }, !1);
        }, r.prototype.setFocused = function(e) {
          e || this.ngModelCtrl.$setTouched(), this.openOnFocus || this.evalAttr(e ? "ngFocus" : "ngBlur"),
            this.isFocused = e;
        }, r.prototype.handleBodyClick = function(e) {
          if (this.isCalendarOpen) {
            var t = this.$mdUtil.getClosest(e.target, "md-calendar");
            t || this.closeCalendarPane(), this.$scope.$digest();
          }
        }, r.prototype.handleWindowBlur = function() {
          this.inputFocusedOnWindowBlur = document.activeElement === this.inputElement;
        }, r.prototype.evalAttr = function(e) {
          this.$attrs[e] && this.$scope.$parent.$eval(this.$attrs[e]);
        }, r.prototype.setModelValue = function(e) {
          var t = this.$mdUtil.getModelOption(this.ngModelCtrl, "timezone");
          this.ngModelCtrl.$setViewValue(this.ngDateFilter(e, "yyyy-MM-dd", t));
        }, r.prototype.onExternalChange = function(e) {
          var t = this.$mdUtil.getModelOption(this.ngModelCtrl, "timezone");
          this.date = e, this.inputElement.value = this.locale.formatDate(e, t), this.mdInputContainer &&
            this.mdInputContainer.setHasValue(!!e), this.resizeInputElement(), this.updateErrorState();
        };
      }();
    }(),
    function() {
      function e(e, t, n, r) {
        function i(r, i, o) {
          function a() {
            var e = i.parent();
            return !(!e.attr("aria-label") && !e.text()) || !(!e.parent().attr("aria-label") && !e.parent()
              .text());
          }

          function s() {
            o.mdSvgIcon || o.mdSvgSrc || (o.mdFontIcon && i.addClass("md-font " + o.mdFontIcon), i.addClass(
              l));
          }

          function c() {
            if (!o.mdSvgIcon && !o.mdSvgSrc) {
              o.mdFontIcon && (i.removeClass(u), i.addClass(o.mdFontIcon), u = o.mdFontIcon);
              var t = e.fontSet(o.mdFontSet);
              l !== t && (i.removeClass(l), i.addClass(t), l = t);
            }
          }
          t(i);
          var u = o.mdFontIcon,
            l = e.fontSet(o.mdFontSet);
          s(), o.$observe("mdFontIcon", c), o.$observe("mdFontSet", c);
          var d = (i[0].getAttribute(o.$attr.mdSvgSrc), o.alt || o.mdFontIcon || o.mdSvgIcon || i.text()),
            f = o.$normalize(o.$attr.mdSvgIcon || o.$attr.mdSvgSrc || "");
          o["aria-label"] || ("" === d || a() ? i.text() || n.expect(i, "aria-hidden", "true") : (n.expect(i,
            "aria-label", d), n.expect(i, "role", "img"))), f && o.$observe(f, function(t) {
            i.empty(), t && e(t).then(function(e) {
              i.empty(), i.append(e);
            });
          });
        }
        return {
          restrict: "E",
          link: i
        };
      }
      t.module("material.components.icon").directive("mdIcon", ["$mdIcon", "$mdTheming", "$mdAria", "$sce",
        e]);
    }(),
    function() {
      function n() {}

      function r(e, t) {
        this.url = e, this.viewBoxSize = t || o.defaultViewBoxSize;
      }

      function i(n, r, i, o, a, s) {
        function c(e) {
          if (e = e || "", t.isString(e) || (e = s.getTrustedUrl(e)), b[e]) return i.when(l(b[e]));
          if (_.test(e) || $.test(e)) return p(e).then(d(e));
          e.indexOf(":") == -1 && (e = "$default:" + e);
          var r = n[e] ? f : h;
          return r(e).then(d(e));
        }

        function u(e) {
          var r = t.isUndefined(e) || !(e && e.length);
          if (r) return n.defaultFontSet;
          var i = e;
          return t.forEach(n.fontSets, function(t) {
            t.alias == e && (i = t.fontSet || i);
          }), i;
        }

        function l(e) {
          var n = e.clone(),
            r = "_cache" + a.nextUid();
          return n.id && (n.id += r), t.forEach(n.querySelectorAll("[id]"), function(e) {
            e.id += r;
          }), n;
        }

        function d(e) {
          return function(t) {
            return b[e] = m(t) ? t : new v(t, n[e]), b[e].clone();
          };
        }

        function f(e) {
          var t = n[e];
          return p(t.url).then(function(e) {
            return new v(e, t);
          });
        }

        function h(e) {
          function t(t) {
            var n = e.slice(e.lastIndexOf(":") + 1),
              i = t.querySelector("#" + n);
            return i ? new v(i, s) : r(e);
          }

          function r(e) {
            var t = "icon " + e + " not found";
            return o.warn(t), i.reject(t || e);
          }
          var a = e.substring(0, e.lastIndexOf(":")) || "$default",
            s = n[a];
          return s ? p(s.url).then(t) : r(e);
        }

        function p(n) {
          function a(n) {
            var r = $.exec(n),
              o = /base64/i.test(n),
              a = o ? e.atob(r[2]) : r[2];
            return i.when(t.element(a)[0]);
          }

          function s(e) {
            return i(function(n, i) {
              var a = function(e) {
                  var n = t.isString(e) ? e : e.message || e.data || e.statusText;
                  o.warn(n), i(e);
                },
                s = function(r) {
                  E[e] || (E[e] = t.element("<div>").append(r)[0].querySelector("svg")), n(E[e]);
                };
              r(e, !0).then(s, a);
            });
          }
          return $.test(n) ? a(n) : s(n);
        }

        function m(e) {
          return t.isDefined(e.element) && t.isDefined(e.config);
        }

        function v(e, n) {
          e && "svg" != e.tagName && (e = t.element('<svg xmlns="http://www.w3.org/2000/svg">').append(e
            .cloneNode(!0))[0]), e.getAttribute("xmlns") || e.setAttribute("xmlns",
            "http://www.w3.org/2000/svg"), this.element = e, this.config = n, this.prepare();
        }

        function g() {
          var e = this.config ? this.config.viewBoxSize : n.defaultViewBoxSize;
          t.forEach({
            fit: "",
            height: "100%",
            width: "100%",
            preserveAspectRatio: "xMidYMid meet",
            viewBox: this.element.getAttribute("viewBox") || "0 0 " + e + " " + e,
            focusable: !1
          }, function(e, t) {
            this.element.setAttribute(t, e);
          }, this);
        }

        function y() {
          return this.element.cloneNode(!0);
        }
        var b = {},
          E = {},
          _ = /[-\w@:%\+.~#?&\/\/=]{2,}\.[a-z]{2,4}\b(\/[-\w@:%\+.~#?&\/\/=]*)?/i,
          $ = /^data:image\/svg\+xml[\s*;\w\-\=]*?(base64)?,(.*)$/i;
        return v.prototype = {
          clone: y,
          prepare: g
        }, c.fontSet = u, c;
      }
      i.$inject = ["config", "$templateRequest", "$q", "$log", "$mdUtil", "$sce"], t.module(
        "material.components.icon").constant("$$mdSvgRegistry", {
        mdTabsArrow: "data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHg9IjBweCIgeT0iMHB4IiB2aWV3Qm94PSIwIDAgMjQgMjQiPjxnPjxwb2x5Z29uIHBvaW50cz0iMTUuNCw3LjQgMTQsNiA4LDEyIDE0LDE4IDE1LjQsMTYuNiAxMC44LDEyICIvPjwvZz48L3N2Zz4=",
        mdClose: "data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHg9IjBweCIgeT0iMHB4IiB2aWV3Qm94PSIwIDAgMjQgMjQiPjxnPjxwYXRoIGQ9Ik0xOSA2LjQxbC0xLjQxLTEuNDEtNS41OSA1LjU5LTUuNTktNS41OS0xLjQxIDEuNDEgNS41OSA1LjU5LTUuNTkgNS41OSAxLjQxIDEuNDEgNS41OS01LjU5IDUuNTkgNS41OSAxLjQxLTEuNDEtNS41OS01LjU5eiIvPjwvZz48L3N2Zz4=",
        mdCancel: "data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHg9IjBweCIgeT0iMHB4IiB2aWV3Qm94PSIwIDAgMjQgMjQiPjxnPjxwYXRoIGQ9Ik0xMiAyYy01LjUzIDAtMTAgNC40Ny0xMCAxMHM0LjQ3IDEwIDEwIDEwIDEwLTQuNDcgMTAtMTAtNC40Ny0xMC0xMC0xMHptNSAxMy41OWwtMS40MSAxLjQxLTMuNTktMy41OS0zLjU5IDMuNTktMS40MS0xLjQxIDMuNTktMy41OS0zLjU5LTMuNTkgMS40MS0xLjQxIDMuNTkgMy41OSAzLjU5LTMuNTkgMS40MSAxLjQxLTMuNTkgMy41OSAzLjU5IDMuNTl6Ii8+PC9nPjwvc3ZnPg==",
        mdMenu: "data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHg9IjBweCIgeT0iMHB4IiB2aWV3Qm94PSIwIDAgMjQgMjQiPjxwYXRoIGQ9Ik0zLDZIMjFWOEgzVjZNMywxMUgyMVYxM0gzVjExTTMsMTZIMjFWMThIM1YxNloiIC8+PC9zdmc+",
        mdToggleArrow: "data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHg9IjBweCIgeT0iMHB4IiB2aWV3Qm94PSIwIDAgNDggNDgiPjxwYXRoIGQ9Ik0yNCAxNmwtMTIgMTIgMi44MyAyLjgzIDkuMTctOS4xNyA5LjE3IDkuMTcgMi44My0yLjgzeiIvPjxwYXRoIGQ9Ik0wIDBoNDh2NDhoLTQ4eiIgZmlsbD0ibm9uZSIvPjwvc3ZnPg==",
        mdCalendar: "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgdmlld0JveD0iMCAwIDI0IDI0Ij48cGF0aCBkPSJNMTkgM2gtMVYxaC0ydjJIOFYxSDZ2Mkg1Yy0xLjExIDAtMS45OS45LTEuOTkgMkwzIDE5YzAgMS4xLjg5IDIgMiAyaDE0YzEuMSAwIDItLjkgMi0yVjVjMC0xLjEtLjktMi0yLTJ6bTAgMTZINVY4aDE0djExek03IDEwaDV2NUg3eiIvPjwvc3ZnPg==",
        mdChecked: "data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHg9IjBweCIgeT0iMHB4IiB2aWV3Qm94PSIwIDAgMjQgMjQiPjxnPjxwYXRoIGQ9Ik05IDE2LjE3TDQuODMgMTJsLTEuNDIgMS40MUw5IDE5IDIxIDdsLTEuNDEtMS40MXoiLz48L2c+PC9zdmc+"
      }).provider("$mdIcon", n);
      var o = {
        defaultViewBoxSize: 24,
        defaultFontSet: "material-icons",
        fontSets: []
      };
      n.prototype = {
        icon: function(e, t, n) {
          return e.indexOf(":") == -1 && (e = "$default:" + e), o[e] = new r(t, n), this;
        },
        iconSet: function(e, t, n) {
          return o[e] = new r(t, n), this;
        },
        defaultIconSet: function(e, t) {
          var n = "$default";
          return o[n] || (o[n] = new r(e, t)), o[n].viewBoxSize = t || o.defaultViewBoxSize, this;
        },
        defaultViewBoxSize: function(e) {
          return o.defaultViewBoxSize = e, this;
        },
        fontSet: function(e, t) {
          return o.fontSets.push({
            alias: e,
            fontSet: t || e
          }), this;
        },
        defaultFontSet: function(e) {
          return o.defaultFontSet = e ? e : "", this;
        },
        defaultIconSize: function(e) {
          return o.defaultIconSize = e, this;
        },
        $get: ["$templateRequest", "$q", "$log", "$mdUtil", "$sce", function(e, t, n, r, a) {
          return i(o, e, t, n, r, a);
        }]
      };
    }(),
    function() {
      function e(e, n, i, o, a, s, c, u) {
        this.$element = i, this.$attrs = o, this.$mdConstant = a, this.$mdUtil = c, this.$document = s, this
          .$scope = e, this.$rootScope = n, this.$timeout = u;
        var l = this;
        t.forEach(r, function(e) {
          l[e] = t.bind(l, l[e]);
        });
      }
      e.$inject = ["$scope", "$rootScope", "$element", "$attrs", "$mdConstant", "$document", "$mdUtil",
        "$timeout"
      ], t.module("material.components.menuBar").controller("MenuBarController", e);
      var r = ["handleKeyDown", "handleMenuHover", "scheduleOpenHoveredMenu", "cancelScheduledOpen"];
      e.prototype.init = function() {
        var e = this.$element,
          t = this.$mdUtil,
          r = this.$scope,
          i = this,
          o = [];
        e.on("keydown", this.handleKeyDown), this.parentToolbar = t.getClosest(e, "MD-TOOLBAR"), o.push(this
          .$rootScope.$on("$mdMenuOpen", function(t, n) {
            i.getMenus().indexOf(n[0]) != -1 && (e[0].classList.add("md-open"), n[0].classList.add(
                "md-open"), i.currentlyOpenMenu = n.controller("mdMenu"), i.currentlyOpenMenu
              .registerContainerProxy(i.handleKeyDown), i.enableOpenOnHover());
          })), o.push(this.$rootScope.$on("$mdMenuClose", function(r, o, a) {
          var s = i.getMenus();
          if (s.indexOf(o[0]) != -1 && (e[0].classList.remove("md-open"), o[0].classList.remove(
              "md-open")), e[0].contains(o[0])) {
            for (var c = o[0]; c && s.indexOf(c) == -1;) c = t.getClosest(c, "MD-MENU", !0);
            c && (a.skipFocus || c.querySelector("button:not([disabled])").focus(), i
              .currentlyOpenMenu = n, i.disableOpenOnHover(), i.setKeyboardMode(!0));
          }
        })), r.$on("$destroy", function() {
          for (i.disableOpenOnHover(); o.length;) o.shift()();
        }), this.setKeyboardMode(!0);
      }, e.prototype.setKeyboardMode = function(e) {
        e ? this.$element[0].classList.add("md-keyboard-mode") : this.$element[0].classList.remove(
          "md-keyboard-mode");
      }, e.prototype.enableOpenOnHover = function() {
        if (!this.openOnHoverEnabled) {
          var e = this;
          e.openOnHoverEnabled = !0, e.parentToolbar && (e.parentToolbar.classList.add("md-has-open-menu"),
            e.$mdUtil.nextTick(function() {
              t.element(e.parentToolbar).on("click", e.handleParentClick);
            }, !1)), t.element(e.getMenus()).on("mouseenter", e.handleMenuHover);
        }
      }, e.prototype.handleMenuHover = function(e) {
        this.setKeyboardMode(!1), this.openOnHoverEnabled && this.scheduleOpenHoveredMenu(e);
      }, e.prototype.disableOpenOnHover = function() {
        this.openOnHoverEnabled && (this.openOnHoverEnabled = !1, this.parentToolbar && (this.parentToolbar
          .classList.remove("md-has-open-menu"), t.element(this.parentToolbar).off("click", this
            .handleParentClick)), t.element(this.getMenus()).off("mouseenter", this.handleMenuHover));
      }, e.prototype.scheduleOpenHoveredMenu = function(e) {
        var n = t.element(e.currentTarget),
          r = n.controller("mdMenu");
        this.setKeyboardMode(!1), this.scheduleOpenMenu(r);
      }, e.prototype.scheduleOpenMenu = function(e) {
        var t = this,
          r = this.$timeout;
        e != t.currentlyOpenMenu && (r.cancel(t.pendingMenuOpen), t.pendingMenuOpen = r(function() {
          t.pendingMenuOpen = n, t.currentlyOpenMenu && t.currentlyOpenMenu.close(!0, {
            closeAll: !0
          }), e.open();
        }, 200, !1));
      }, e.prototype.handleKeyDown = function(e) {
        var n = this.$mdConstant.KEY_CODE,
          r = this.currentlyOpenMenu,
          i = r && r.isOpen;
        this.setKeyboardMode(!0);
        var o, a, s;
        switch (e.keyCode) {
          case n.DOWN_ARROW:
            r ? r.focusMenuContainer() : this.openFocusedMenu(), o = !0;
            break;
          case n.UP_ARROW:
            r && r.close(), o = !0;
            break;
          case n.LEFT_ARROW:
            a = this.focusMenu(-1), i && (s = t.element(a).controller("mdMenu"), this.scheduleOpenMenu(s)),
              o = !0;
            break;
          case n.RIGHT_ARROW:
            a = this.focusMenu(1), i && (s = t.element(a).controller("mdMenu"), this.scheduleOpenMenu(s)),
              o = !0;
        }
        o && (e && e.preventDefault && e.preventDefault(), e && e.stopImmediatePropagation && e
          .stopImmediatePropagation());
      }, e.prototype.focusMenu = function(e) {
        var t = this.getMenus(),
          n = this.getFocusedMenuIndex();
        n == -1 && (n = this.getOpenMenuIndex());
        var r = !1;
        if (n == -1 ? (n = 0, r = !0) : (e < 0 && n > 0 || e > 0 && n < t.length - e) && (n += e, r = !0),
          r) return t[n].querySelector("button").focus(), t[n];
      }, e.prototype.openFocusedMenu = function() {
        var e = this.getFocusedMenu();
        e && t.element(e).controller("mdMenu").open();
      }, e.prototype.getMenus = function() {
        var e = this.$element;
        return this.$mdUtil.nodesToArray(e[0].children).filter(function(e) {
          return "MD-MENU" == e.nodeName;
        });
      }, e.prototype.getFocusedMenu = function() {
        return this.getMenus()[this.getFocusedMenuIndex()];
      }, e.prototype.getFocusedMenuIndex = function() {
        var e = this.$mdUtil,
          t = e.getClosest(this.$document[0].activeElement, "MD-MENU");
        if (!t) return -1;
        var n = this.getMenus().indexOf(t);
        return n;
      }, e.prototype.getOpenMenuIndex = function() {
        for (var e = this.getMenus(), t = 0; t < e.length; ++t)
          if (e[t].classList.contains("md-open")) return t;
        return -1;
      }, e.prototype.handleParentClick = function(e) {
        var n = this.querySelector("md-menu.md-open");
        n && !n.contains(e.target) && t.element(n).controller("mdMenu").close(!0, {
          closeAll: !0
        });
      };
    }(),
    function() {
      function e(e, n) {
        return {
          restrict: "E",
          require: "mdMenuBar",
          controller: "MenuBarController",
          compile: function(r, i) {
            return i.ariaRole || r[0].setAttribute("role", "menubar"), t.forEach(r[0].children, function(
              n) {
                if ("MD-MENU" == n.nodeName) {
                  n.hasAttribute("md-position-mode") || (n.setAttribute("md-position-mode",
                    "left bottom"), n.querySelector("button, a, md-button").setAttribute("role",
                      "menuitem"));
                  var r = e.nodesToArray(n.querySelectorAll("md-menu-content"));
                  t.forEach(r, function(e) {
                    e.classList.add("md-menu-bar-menu"), e.classList.add("md-dense"), e.hasAttribute(
                      "width") || e.setAttribute("width", 5);
                  });
                }
              }), r.find("md-menu-item").addClass("md-in-menu-bar"),
              function(e, t, r, i) {
                t.addClass("_md"), n(e, t), i.init();
              };
          }
        };
      }
      e.$inject = ["$mdUtil", "$mdTheming"], t.module("material.components.menuBar").directive("mdMenuBar",
      e);
    }(),
    function() {
      function e() {
        return {
          restrict: "E",
          compile: function(e, t) {
            t.role || e[0].setAttribute("role", "separator");
          }
        };
      }
      t.module("material.components.menuBar").directive("mdMenuDivider", e);
    }(),
    function() {
      function e(e, t, n) {
        this.$element = t, this.$attrs = n, this.$scope = e;
      }
      e.$inject = ["$scope", "$element", "$attrs"], t.module("material.components.menuBar").controller(
        "MenuItemController", e), e.prototype.init = function(e) {
        var t = this.$element,
          n = this.$attrs;
        this.ngModel = e, "checkbox" != n.type && "radio" != n.type || (this.mode = n.type, this.iconEl = t[
          0].children[0], this.buttonEl = t[0].children[1], e && this.initClickListeners());
      }, e.prototype.clearNgAria = function() {
        var e = this.$element[0],
          n = ["role", "tabindex", "aria-invalid", "aria-checked"];
        t.forEach(n, function(t) {
          e.removeAttribute(t);
        });
      }, e.prototype.initClickListeners = function() {
        function e() {
          if ("radio" == s) {
            var e = a.ngValue ? o.$eval(a.ngValue) : a.value;
            return i.$modelValue == e;
          }
          return i.$modelValue;
        }

        function n(e) {
          e ? u.off("click", l) : u.on("click", l);
        }
        var r = this,
          i = this.ngModel,
          o = this.$scope,
          a = this.$attrs,
          s = (this.$element, this.mode);
        this.handleClick = t.bind(this, this.handleClick);
        var c = this.iconEl,
          u = t.element(this.buttonEl),
          l = this.handleClick;
        a.$observe("disabled", n), n(a.disabled), i.$render = function() {
          r.clearNgAria(), e() ? (c.style.display = "", u.attr("aria-checked", "true")) : (c.style
            .display = "none", u.attr("aria-checked", "false"));
        }, o.$$postDigest(i.$render);
      }, e.prototype.handleClick = function(e) {
        var t,
          n = this.mode,
          r = this.ngModel,
          i = this.$attrs;
        "checkbox" == n ? t = !r.$modelValue : "radio" == n && (t = i.ngValue ? this.$scope.$eval(i
          .ngValue) : i.value), r.$setViewValue(t), r.$render();
      };
    }(),
    function() {
      function e(e, n, r) {
        return {
          controller: "MenuItemController",
          require: ["mdMenuItem", "?ngModel"],
          priority: n.BEFORE_NG_ARIA,
          compile: function(n, i) {
            function o(e, r, i) {
              i = i || n, i instanceof t.element && (i = i[0]), i.hasAttribute(e) || i.setAttribute(e, r);
            }

            function a(r) {
              var i = e.prefixer(r);
              t.forEach(i, function(e) {
                if (n[0].hasAttribute(e)) {
                  var t = n[0].getAttribute(e);
                  l[0].setAttribute(e, t), n[0].removeAttribute(e);
                }
              });
            }
            var s = i.type,
              c = "md-in-menu-bar";
            if ("checkbox" != s && "radio" != s || !n.hasClass(c)) o("role", "menuitem", n[0].querySelector(
              "md-button, button, a"));
            else {
              var u = n[0].textContent,
                l = t.element('<md-button type="button"></md-button>'),
                d = '<md-icon md-svg-src="' + r.mdChecked + '"></md-icon>';
              l.html(u), l.attr("tabindex", "0"), n.html(""), n.append(t.element(d)), n.append(l), n
                .addClass("md-indent").removeClass(c), o("role", "checkbox" == s ? "menuitemcheckbox" :
                  "menuitemradio", l), a("ng-disabled");
            }
            return function(e, t, n, r) {
              var i = r[0],
                o = r[1];
              i.init(o);
            };
          }
        };
      }
      e.$inject = ["$mdUtil", "$mdConstant", "$$mdSvgRegistry"], t.module("material.components.menuBar")
        .directive("mdMenuItem", e);
    }(),
    function() {
      function e(e, r, i, o, a, s, c, u, l) {
        var d,
          f,
          h = a.prefixer(),
          p = this;
        this.nestLevel = parseInt(r.mdNestLevel, 10) || 0, this.init = function(n, r) {
          r = r || {}, d = n, f = i[0].querySelector(h.buildSelector(["ng-click", "ng-mouseenter"])), f
            .setAttribute("aria-expanded", "false"), this.isInMenuBar = r.isInMenuBar, this.nestedMenus = a
            .nodesToArray(d[0].querySelectorAll(".md-nested-menu")), d.on("$mdInterimElementRemove",
              function() {
                p.isOpen = !1, a.nextTick(function() {
                  p.onIsOpenChanged(p.isOpen);
                });
              }), a.nextTick(function() {
              p.onIsOpenChanged(p.isOpen);
            });
          var s = "menu_container_" + a.nextUid();
          d.attr("id", s), t.element(f).attr({
            "aria-owns": s,
            "aria-haspopup": "true"
          }), o.$on("$destroy", t.bind(this, function() {
            this.disableHoverListener(), e.destroy();
          })), d.on("$destroy", function() {
            e.destroy();
          });
        };
        var m,
          v,
          g = [];
        this.enableHoverListener = function() {
          g.push(c.$on("$mdMenuOpen", function(e, t) {
            d[0].contains(t[0]) && (p.currentlyOpenMenu = t.controller("mdMenu"), p
              .isAlreadyOpening = !1, p.currentlyOpenMenu.registerContainerProxy(p
                .triggerContainerProxy.bind(p)));
          })), g.push(c.$on("$mdMenuClose", function(e, t) {
            d[0].contains(t[0]) && (p.currentlyOpenMenu = n);
          })), v = t.element(a.nodesToArray(d[0].children[0].children)), v.on("mouseenter", p
            .handleMenuItemHover), v.on("mouseleave", p.handleMenuItemMouseLeave);
        }, this.disableHoverListener = function() {
          for (; g.length;) g.shift()();
          v && v.off("mouseenter", p.handleMenuItemHover), v && v.off("mouseleave", p
            .handleMenuItemMouseLeave);
        }, this.handleMenuItemHover = function(e) {
          if (!p.isAlreadyOpening) {
            var n = e.target.querySelector("md-menu") || a.getClosest(e.target, "MD-MENU");
            m = s(function() {
              if (n && (n = t.element(n).controller("mdMenu")), p.currentlyOpenMenu && p
                .currentlyOpenMenu != n) {
                var e = p.nestLevel + 1;
                p.currentlyOpenMenu.close(!0, {
                  closeTo: e
                }), p.isAlreadyOpening = !!n, n && n.open();
              } else n && !n.isOpen && n.open && (p.isAlreadyOpening = !!n, n && n.open());
            }, n ? 100 : 250);
            var r = e.currentTarget.querySelector(".md-button:not([disabled])");
            r && r.focus();
          }
        }, this.handleMenuItemMouseLeave = function() {
          m && (s.cancel(m), m = n);
        }, this.open = function(t) {
          t && t.stopPropagation(), t && t.preventDefault(), p.isOpen || (p.enableHoverListener(), p
            .isOpen = !0, a.nextTick(function() {
              p.onIsOpenChanged(p.isOpen);
            }), f = f || (t ? t.target : i[0]), f.setAttribute("aria-expanded", "true"), o.$emit(
              "$mdMenuOpen", i), e.show({
              scope: o,
              mdMenuCtrl: p,
              nestLevel: p.nestLevel,
              element: d,
              target: f,
              preserveElement: !0,
              parent: "body"
            }).finally(function() {
              f.setAttribute("aria-expanded", "false"), p.disableHoverListener();
            }));
        }, this.onIsOpenChanged = function(e) {
          e ? (d.attr("aria-hidden", "false"), i[0].classList.add("md-open"), t.forEach(p.nestedMenus,
              function(e) {
                e.classList.remove("md-open");
              })) : (d.attr("aria-hidden", "true"), i[0].classList.remove("md-open")), o.$mdMenuIsOpen = p
            .isOpen;
        }, this.focusMenuContainer = function() {
          var e = d[0].querySelector(h.buildSelector(["md-menu-focus-target", "md-autofocus"]));
          e || (e = d[0].querySelector(".md-button:not([disabled])")), e.focus();
        }, this.registerContainerProxy = function(e) {
          this.containerProxy = e;
        }, this.triggerContainerProxy = function(e) {
          this.containerProxy && this.containerProxy(e);
        }, this.destroy = function() {
          return p.isOpen ? e.destroy() : u.when(!1);
        }, this.close = function(n, r) {
          if (p.isOpen) {
            p.isOpen = !1, a.nextTick(function() {
              p.onIsOpenChanged(p.isOpen);
            });
            var s = t.extend({}, r, {
              skipFocus: n
            });
            if (o.$emit("$mdMenuClose", i, s), e.hide(null, r), !n) {
              var c = p.restoreFocusTo || i.find("button")[0];
              c instanceof t.element && (c = c[0]), c && c.focus();
            }
          }
        }, this.positionMode = function() {
          var e = (r.mdPositionMode || "target").split(" ");
          return 1 == e.length && e.push(e[0]), {
            left: e[0],
            top: e[1]
          };
        }, this.offsets = function() {
          var e = (r.mdOffset || "0 0").split(" ").map(parseFloat);
          if (2 == e.length) return {
            left: e[0],
            top: e[1]
          };
          if (1 == e.length) return {
            top: e[0],
            left: e[0]
          };
          throw Error("Invalid offsets specified. Please follow format <x, y> or <n>");
        }, o.$mdMenu = {
          open: this.open,
          close: this.close
        }, o.$mdOpenMenu = t.bind(this, function() {
          return l.warn("mdMenu: The $mdOpenMenu method is deprecated. Please use `$mdMenu.open`."), this
            .open.apply(this, arguments);
        });
      }
      e.$inject = ["$mdMenu", "$attrs", "$element", "$scope", "$mdUtil", "$timeout", "$rootScope", "$q",
        "$log"
      ], t.module("material.components.menu").controller("mdMenuCtrl", e);
    }(),
    function() {
      function e(e) {
        function n(n) {
          n.addClass("md-menu");
          var o = n.children()[0],
            a = n.children()[1],
            s = e.prefixer();
          s.hasAttribute(o, "ng-click") || (o = o.querySelector(s.buildSelector(["ng-click",
            "ng-mouseenter"])) || o);
          var c = "MD-BUTTON" === o.nodeName || "BUTTON" === o.nodeName;
          if (o && c && !o.hasAttribute("type") && o.setAttribute("type", "button"), !o) throw Error(i +
            "Expected the menu to have a trigger element.");
          if (!a || "MD-MENU-CONTENT" !== a.nodeName) throw Error(i +
            "Expected the menu to contain a `md-menu-content` element.");
          o && o.setAttribute("aria-haspopup", "true");
          var u = n[0].querySelectorAll("md-menu"),
            l = parseInt(n[0].getAttribute("md-nest-level"), 10) || 0;
          return u && t.forEach(e.nodesToArray(u), function(e) {
            e.hasAttribute("md-position-mode") || e.setAttribute("md-position-mode", "cascade"), e
              .classList.add("_md-nested-menu"), e.setAttribute("md-nest-level", l + 1);
          }), r;
        }

        function r(e, n, r, i) {
          var o = i[0],
            a = !!i[1],
            s = t.element('<div class="_md md-open-menu-container md-whiteframe-z2"></div>'),
            c = n.children()[1];
          n.addClass("_md"), c.hasAttribute("role") || c.setAttribute("role", "menu"), s.append(c), n.on(
            "$destroy",
            function() {
              s.remove();
            }), n.append(s), s[0].style.display = "none", o.init(s, {
            isInMenuBar: a
          });
        }
        var i = "Invalid HTML for md-menu: ";
        return {
          restrict: "E",
          require: ["mdMenu", "?^mdMenuBar"],
          controller: "mdMenuCtrl",
          scope: !0,
          compile: n
        };
      }
      e.$inject = ["$mdUtil"], t.module("material.components.menu").directive("mdMenu", e);
    }(),
    function() {
      function e(e) {
        function r(e, r, a, s, c, u, l, d, f, h) {
          function p(n, r, i) {
            return i.nestLevel ? t.noop : (i.disableParentScroll && !e.getClosest(i.target, "MD-DIALOG") ? i
              .restoreScroll = e.disableScrollAround(i.element, i.parent) : i.disableParentScroll = !1, i
              .hasBackdrop && (i.backdrop = e.createBackdrop(n, "md-menu-backdrop md-click-catcher"), f
                .enter(i.backdrop, s[0].body)),
              function() {
                i.backdrop && i.backdrop.remove(), i.disableParentScroll && i.restoreScroll();
              });
          }

          function m(e, t, n) {
            function r() {
              return d(t, {
                addClass: "md-leave"
              }).start();
            }

            function i() {
              t.removeClass("md-active"), b(t, n), n.alreadyOpen = !1;
            }
            return n.cleanupInteraction(), n.cleanupBackdrop(), n.cleanupResizing(), n.hideBackdrop(), t
              .removeClass("md-clickable"), n.$destroy === !0 ? i() : r().then(i);
          }

          function v(n, i, o) {
            function s() {
              return o.parent.append(i), i[0].style.display = "", u(function(e) {
                var t = E(i, o);
                i.removeClass("md-leave"), d(i, {
                  addClass: "md-active",
                  from: $.toCss(t),
                  to: $.toCss({
                    transform: ""
                  })
                }).start().then(e);
              });
            }

            function f() {
              if (!o.target) throw Error(
              "$mdMenu.show() expected a target to animate from in options.target");
              t.extend(o, {
                alreadyOpen: !1,
                isRemoved: !1,
                target: t.element(o.target),
                parent: t.element(o.parent),
                menuContentEl: t.element(i[0].querySelector("md-menu-content"))
              });
            }

            function m() {
              var e = function(e, t) {
                return l.throttle(function() {
                  if (!o.isRemoved) {
                    var n = E(e, t);
                    e.css($.toCss(n));
                  }
                });
              }(i, o);
              return c.addEventListener("resize", e), c.addEventListener("orientationchange", e),
                function() {
                  c.removeEventListener("resize", e), c.removeEventListener("orientationchange", e);
                };
            }

            function v() {
              return o.backdrop ? (o.backdrop.on("click", y), function() {
                o.backdrop.off("click", y);
              }) : t.noop;
            }

            function y(e) {
              e.preventDefault(), e.stopPropagation(), n.$apply(function() {
                o.mdMenuCtrl.close(!0, {
                  closeAll: !0
                });
              });
            }

            function b() {
              function r(t) {
                var n;
                switch (t.keyCode) {
                  case a.KEY_CODE.ESCAPE:
                    o.mdMenuCtrl.close(!1, {
                      closeAll: !0
                    }), n = !0;
                    break;
                  case a.KEY_CODE.UP_ARROW:
                    g(t, o.menuContentEl, o, -1) || o.nestLevel || o.mdMenuCtrl.triggerContainerProxy(t),
                      n = !0;
                    break;
                  case a.KEY_CODE.DOWN_ARROW:
                    g(t, o.menuContentEl, o, 1) || o.nestLevel || o.mdMenuCtrl.triggerContainerProxy(t), n = !
                      0;
                    break;
                  case a.KEY_CODE.LEFT_ARROW:
                    o.nestLevel ? o.mdMenuCtrl.close() : o.mdMenuCtrl.triggerContainerProxy(t), n = !0;
                    break;
                  case a.KEY_CODE.RIGHT_ARROW:
                    var r = e.getClosest(t.target, "MD-MENU");
                    r && r != o.parent[0] ? t.target.click() : o.mdMenuCtrl.triggerContainerProxy(t), n = !0;
                }
                n && (t.preventDefault(), t.stopImmediatePropagation());
              }

              function i(t) {
                function r() {
                  n.$apply(function() {
                    o.mdMenuCtrl.close(!0, {
                      closeAll: !0
                    });
                  });
                }

                function i(e, t) {
                  if (!e) return !1;
                  for (var n, r = 0; n = t[r]; ++r)
                    if (_.hasAttribute(e, n)) return !0;
                  return !1;
                }
                var a = t.target;
                do {
                  if (a == o.menuContentEl[0]) return;
                  if ((i(a, ["ng-click", "ng-href", "ui-sref"]) || "BUTTON" == a.nodeName || "MD-BUTTON" == a
                      .nodeName) && !i(a, ["md-prevent-menu-close"])) {
                    var s = e.getClosest(a, "MD-MENU");
                    a.hasAttribute("disabled") || s && s != o.parent[0] || r();
                    break;
                  }
                } while (a = a.parentNode);
              }
              if (!o.menuContentEl[0]) return t.noop;
              o.menuContentEl.on("keydown", r), o.menuContentEl[0].addEventListener("click", i, !0);
              var s = o.menuContentEl[0].querySelector(_.buildSelector(["md-menu-focus-target",
                "md-autofocus"]));
              if (!s)
                for (var c = o.menuContentEl[0].children.length, u = 0; u < c; u++) {
                  var l = o.menuContentEl[0].children[u];
                  if (s = l.querySelector(".md-button:not([disabled])")) break;
                  if (l.firstElementChild && !l.firstElementChild.disabled) {
                    s = l.firstElementChild;
                    break;
                  }
                }
              return s && s.focus(),
                function() {
                  o.menuContentEl.off("keydown", r), o.menuContentEl[0].removeEventListener("click", i, !0);
                };
            }
            return f(o), o.menuContentEl[0] ? r.inherit(o.menuContentEl, o.target) : h.warn(
              "$mdMenu: Menu elements should always contain a `md-menu-content` element,otherwise interactivity features will not work properly.",
              i), o.cleanupResizing = m(), o.hideBackdrop = p(n, i, o), s().then(function(e) {
              return o.alreadyOpen = !0, o.cleanupInteraction = b(), o.cleanupBackdrop = v(), i.addClass(
                "md-clickable"), e;
            });
          }

          function g(t, n, r, i) {
            for (var o, a = e.getClosest(t.target, "MD-MENU-ITEM"), s = e.nodesToArray(n[0].children), c = s
                .indexOf(a), u = c + i; u >= 0 && u < s.length; u += i) {
              var l = s[u].querySelector(".md-button");
              if (o = y(l)) break;
            }
            return o;
          }

          function y(e) {
            if (e && e.getAttribute("tabindex") != -1) return e.focus(), s[0].activeElement == e;
          }

          function b(e, t) {
            t.preserveElement ? i(e).style.display = "none" : i(e).parentNode === i(t.parent) && i(t.parent)
              .removeChild(i(e));
          }

          function E(t, r) {
            function i(e) {
              e.top = Math.max(Math.min(e.top, y.bottom - l.offsetHeight), y.top), e.left = Math.max(Math.min(
                e.left, y.right - l.offsetWidth), y.left);
            }

            function a() {
              for (var e = 0; e < d.children.length; ++e)
                if ("none" != c.getComputedStyle(d.children[e]).display) return d.children[e];
            }
            var u,
              l = t[0],
              d = t[0].firstElementChild,
              f = d.getBoundingClientRect(),
              h = s[0].body,
              p = h.getBoundingClientRect(),
              m = c.getComputedStyle(d),
              v = r.target[0].querySelector(_.buildSelector("md-menu-origin")) || r.target[0],
              g = v.getBoundingClientRect(),
              y = {
                left: p.left + o,
                top: Math.max(p.top, 0) + o,
                bottom: Math.max(p.bottom, Math.max(p.top, 0) + p.height) - o,
                right: p.right - o
              },
              b = {
                top: 0,
                left: 0,
                right: 0,
                bottom: 0
              },
              E = {
                top: 0,
                left: 0,
                right: 0,
                bottom: 0
              },
              $ = r.mdMenuCtrl.positionMode();
            "target" != $.top && "target" != $.left && "target-right" != $.left || (u = a(), u && (u = u
              .firstElementChild || u, u = u.querySelector(_.buildSelector("md-menu-align-target")) || u,
              b = u.getBoundingClientRect(), E = {
                top: parseFloat(l.style.top || 0),
                left: parseFloat(l.style.left || 0)
              }));
            var w = {},
              T = "top ";
            switch ($.top) {
              case "target":
                w.top = E.top + g.top - b.top;
                break;
              case "cascade":
                w.top = g.top - parseFloat(m.paddingTop) - v.style.top;
                break;
              case "bottom":
                w.top = g.top + g.height;
                break;
              default:
                throw new Error('Invalid target mode "' + $.top + '" specified for md-menu on Y axis.');
            }
            var C = "rtl" == e.bidi();
            switch ($.left) {
              case "target":
                w.left = E.left + g.left - b.left, T += C ? "right" : "left";
                break;
              case "target-left":
                w.left = g.left, T += "left";
                break;
              case "target-right":
                w.left = g.right - f.width + (f.right - b.right), T += "right";
                break;
              case "cascade":
                var x = C ? g.left - f.width < y.left : g.right + f.width < y.right;
                w.left = x ? g.right - v.style.left : g.left - v.style.left - f.width, T += x ? "left" :
                  "right";
                break;
              case "right":
                C ? (w.left = g.right - g.width, T += "left") : (w.left = g.right - f.width, T += "right");
                break;
              case "left":
                C ? (w.left = g.right - f.width, T += "right") : (w.left = g.left, T += "left");
                break;
              default:
                throw new Error('Invalid target mode "' + $.left + '" specified for md-menu on X axis.');
            }
            var S = r.mdMenuCtrl.offsets();
            w.top += S.top, w.left += S.left, i(w);
            var A = Math.round(100 * Math.min(g.width / l.offsetWidth, 1)) / 100,
              M = Math.round(100 * Math.min(g.height / l.offsetHeight, 1)) / 100;
            return {
              top: Math.round(w.top),
              left: Math.round(w.left),
              transform: r.alreadyOpen ? n : e.supplant("scale({0},{1})", [A, M]),
              transformOrigin: T
            };
          }
          var _ = e.prefixer(),
            $ = e.dom.animator;
          return {
            parent: "body",
            onShow: v,
            onRemove: m,
            hasBackdrop: !0,
            disableParentScroll: !0,
            skipCompile: !0,
            preserveScope: !0,
            multiple: !0,
            themable: !0
          };
        }

        function i(e) {
          return e instanceof t.element && (e = e[0]), e;
        }
        r.$inject = ["$mdUtil", "$mdTheming", "$mdConstant", "$document", "$window", "$q", "$$rAF",
          "$animateCss", "$animate", "$log"
        ];
        var o = 8;
        return e("$mdMenu").setDefaults({
          methods: ["target"],
          options: r
        });
      }
      e.$inject = ["$$interimElementProvider"], t.module("material.components.menu").provider("$mdMenu", e);
    }(),
    function() {
      function e(e, n, r, i, o, a) {
        function s(a, s, b) {
          function E(t, r, o, s, c, l) {
            function p(e) {
              A.attr("stroke-dashoffset", u(y, b, e, w)), A.attr("transform", "rotate(" + $ + " " + y / 2 +
                " " + y / 2 + ")");
            }
            var m = ++I,
              v = i.now(),
              g = r - t,
              y = d(a.mdDiameter),
              b = f(y),
              E = o || n.easeFn,
              _ = s || n.duration,
              $ = -90 * (c || 0),
              w = l || 100;
            r === t ? p(r) : T = h(function n() {
              var r = e.Math.max(0, e.Math.min(i.now() - v, _));
              p(E(r, t, g, _)), m === I && r < _ && (T = h(n));
            });
          }

          function _() {
            E(M, k, n.easeFnIndeterminate, n.durationIndeterminate, N, 75), N = ++N % 4;
          }

          function $() {
            C || (C = o(_, n.durationIndeterminate, 0, !1), _(), s.addClass(y).removeAttr("aria-valuenow"));
          }

          function w() {
            C && (o.cancel(C), C = null, s.removeClass(y));
          }
          var T,
            C,
            x = s[0],
            S = t.element(x.querySelector("svg")),
            A = t.element(x.querySelector("path")),
            M = n.startIndeterminate,
            k = n.endIndeterminate,
            N = 0,
            I = 0;
          r(s), s.toggleClass(g, b.hasOwnProperty("disabled")), a.mdMode === v && $(), a.$on("$destroy",
            function() {
              w(), T && p(T);
            }), a.$watchGroup(["value", "mdMode", function() {
            var e = x.disabled;
            return e === !0 || e === !1 ? e : t.isDefined(s.attr("disabled"));
          }], function(e, t) {
            var n = e[1],
              r = e[2],
              i = t[2];
            if (r !== i && s.toggleClass(g, !!r), r) w();
            else if (n !== m && n !== v && (n = v, b.$set("mdMode", n)), n === v) $();
            else {
              var o = l(e[0]);
              w(), s.attr("aria-valuenow", o), E(l(t[0]), o);
            }
          }), a.$watch("mdDiameter", function(t) {
            var n = d(t),
              r = f(n),
              i = l(a.value),
              o = n / 2 + "px",
              h = {
                width: n + "px",
                height: n + "px"
              };
            S[0].setAttribute("viewBox", "0 0 " + n + " " + n), S.css(h).css("transform-origin", o + " " +
                o + " " + o), s.css(h), A.attr("stroke-width", r), A.attr("stroke-linecap", "square"), a
              .mdMode == v ? (A.attr("d", c(n, r, !0)), A.attr("stroke-dasharray", (n - r) * e.Math.PI *
                .75), A.attr("stroke-dashoffset", u(n, r, 1, 75))) : (A.attr("d", c(n, r, !1)), A.attr(
                  "stroke-dasharray", (n - r) * e.Math.PI), A.attr("stroke-dashoffset", u(n, r, 0, 100)),
                E(i, i));
          });
        }

        function c(e, t, n) {
          var r = e / 2,
            i = t / 2,
            o = r + "," + i,
            a = i + "," + r,
            s = r - i;
          return "M" + o + "A" + s + "," + s + " 0 1 1 " + a + (n ? "" : "A" + s + "," + s + " 0 0 1 " + o);
        }

        function u(t, n, r, i) {
          return (t - n) * e.Math.PI * (3 * (i || 100) / 100 - r / 100);
        }

        function l(t) {
          return e.Math.max(0, e.Math.min(t || 0, 100));
        }

        function d(e) {
          var t = n.progressSize;
          if (e) {
            var r = parseFloat(e);
            return e.lastIndexOf("%") === e.length - 1 && (r = r / 100 * t), r;
          }
          return t;
        }

        function f(e) {
          return n.strokeWidth / 100 * e;
        }
        var h = e.requestAnimationFrame || e.webkitRequestAnimationFrame || t.noop,
          p = e.cancelAnimationFrame || e.webkitCancelAnimationFrame || e.webkitCancelRequestAnimationFrame ||
          t.noop,
          m = "determinate",
          v = "indeterminate",
          g = "_md-progress-circular-disabled",
          y = "md-mode-indeterminate";
        return {
          restrict: "E",
          scope: {
            value: "@",
            mdDiameter: "@",
            mdMode: "@"
          },
          template: '<svg xmlns="http://www.w3.org/2000/svg"><path fill="none"/></svg>',
          compile: function(e, n) {
            if (e.attr({
                "aria-valuemin": 0,
                "aria-valuemax": 100,
                role: "progressbar"
              }), t.isUndefined(n.mdMode)) {
              var r = n.hasOwnProperty("value") ? m : v;
              n.$set("mdMode", r);
            } else n.$set("mdMode", n.mdMode.trim());
            return s;
          }
        };
      }
      e.$inject = ["$window", "$mdProgressCircular", "$mdTheming", "$mdUtil", "$interval", "$log"], t.module(
        "material.components.progressCircular").directive("mdProgressCircular", e);
    }(),
    function() {
      function e() {
        function e(e, t, n, r) {
          return n * e / r + t;
        }

        function n(e, t, n, r) {
          var i = (e /= r) * e,
            o = i * e;
          return t + n * (6 * o * i + -15 * i * i + 10 * o);
        }
        var r = {
          progressSize: 50,
          strokeWidth: 10,
          duration: 100,
          easeFn: e,
          durationIndeterminate: 1333,
          startIndeterminate: 1,
          endIndeterminate: 149,
          easeFnIndeterminate: n,
          easingPresets: {
            linearEase: e,
            materialEase: n
          }
        };
        return {
          configure: function(e) {
            return r = t.extend(r, e || {});
          },
          $get: function() {
            return r;
          }
        };
      }
      t.module("material.components.progressCircular").provider("$mdProgressCircular", e);
    }(),
    function() {
      function e() {
        function e(e, r, i, o) {
          if (o) {
            var a = o.getTabElementIndex(r),
              s = n(r, "md-tab-body").remove(),
              c = n(r, "md-tab-label").remove(),
              u = o.insertTab({
                scope: e,
                parent: e.$parent,
                index: a,
                element: r,
                template: s.html(),
                label: c.html()
              }, a);
            e.select = e.select || t.noop, e.deselect = e.deselect || t.noop, e.$watch("active", function(e) {
              e && o.select(u.getIndex(), !0);
            }), e.$watch("disabled", function() {
              o.refreshIndex();
            }), e.$watch(function() {
              return o.getTabElementIndex(r);
            }, function(e) {
              u.index = e, o.updateTabOrder();
            }), e.$on("$destroy", function() {
              o.removeTab(u);
            });
          }
        }

        function n(e, n) {
          for (var r = e[0].children, i = 0, o = r.length; i < o; i++) {
            var a = r[i];
            if (a.tagName === n.toUpperCase()) return t.element(a);
          }
          return t.element();
        }
        return {
          require: "^?mdTabs",
          terminal: !0,
          compile: function(r, i) {
            var o = n(r, "md-tab-label"),
              a = n(r, "md-tab-body");
            if (0 === o.length && (o = t.element("<md-tab-label></md-tab-label>"), i.label ? o.text(i
                .label) : o.append(r.contents()), 0 === a.length)) {
              var s = r.contents().detach();
              a = t.element("<md-tab-body></md-tab-body>"), a.append(s);
            }
            return r.append(o), a.html() && r.append(a), e;
          },
          scope: {
            active: "=?mdActive",
            disabled: "=?ngDisabled",
            select: "&?mdOnSelect",
            deselect: "&?mdOnDeselect"
          }
        };
      }
      t.module("material.components.tabs").directive("mdTab", e);
    }(),
    function() {
      function e() {
        return {
          require: "^?mdTabs",
          link: function(e, t, n, r) {
            r && r.attachRipple(e, t);
          }
        };
      }
      t.module("material.components.tabs").directive("mdTabItem", e);
    }(),
    function() {
      function e() {
        return {
          terminal: !0
        };
      }
      t.module("material.components.tabs").directive("mdTabLabel", e);
    }(),
    function() {
      function e(e) {
        return {
          restrict: "A",
          compile: function(t, n) {
            var r = e(n.mdTabScroll, null, !0);
            return function(e, t) {
              t.on("mousewheel", function(t) {
                e.$apply(function() {
                  r(e, {
                    $event: t
                  });
                });
              });
            };
          }
        };
      }
      e.$inject = ["$parse"], t.module("material.components.tabs").directive("mdTabScroll", e);
    }(),
    function() {
      function e(e, r, i, o, a, s, c, u, l, d, f) {
        function h() {
          y("stretchTabs", _), K("focusIndex", S, fe.selectedIndex || 0), K("offsetLeft", x, 0), K(
              "hasContent", C, !1), K("maxTabWidth", w, J()), K("shouldPaginate", T, !1), b("noInkBar", L), b(
              "dynamicHeight", U), b("noPagination"), b("swipeContent"), b("noDisconnect"), b("autoselect"),
            b("noSelectClick"), b("centerTabs", $, !1), b("enableDisconnect"), fe.scope = e, fe.parent = e
            .$parent, fe.tabs = [], fe.lastSelectedIndex = null, fe.hasFocus = !1, fe.styleTabItemFocus = !1,
            fe.shouldCenterTabs = V(), fe.tabContentPrefix = "tab-content-", p();
        }

        function p() {
          fe.selectedIndex = fe.selectedIndex || 0, m(), g(), v(), d(r), s.nextTick(function() {
            pe = H(), ae(), ne(), se(), fe.tabs[fe.selectedIndex] && fe.tabs[fe.selectedIndex].scope
              .select(), ge = !0, X();
          });
        }

        function m() {
          var e = u.$mdTabsTemplate,
            n = t.element(r[0].querySelector("md-tab-data"));
          n.html(e), l(n.contents())(fe.parent), delete u.$mdTabsTemplate;
        }

        function v() {
          t.element(i).on("resize", P), e.$on("$destroy", E);
        }

        function g() {
          e.$watch("$mdTabsCtrl.selectedIndex", A);
        }

        function y(e, t) {
          var n = u.$normalize("md-" + e);
          t && K(e, t), u.$observe(n, function(t) {
            fe[e] = t;
          });
        }

        function b(e, t) {
          function n(t) {
            fe[e] = "false" !== t;
          }
          var r = u.$normalize("md-" + e);
          t && K(e, t), u.hasOwnProperty(r) && n(u[r]), u.$observe(r, n);
        }

        function E() {
          ve = !0, t.element(i).off("resize", P);
        }

        function _(e) {
          var n = H();
          t.element(n.wrapper).toggleClass("md-stretch-tabs", G()), se();
        }

        function $(e) {
          fe.shouldCenterTabs = V();
        }

        function w(e, n) {
          if (e !== n) {
            var r = H();
            t.forEach(r.tabs, function(t) {
              t.style.maxWidth = e + "px";
            }), s.nextTick(fe.updateInkBarStyles);
          }
        }

        function T(e, t) {
          e !== t && (fe.maxTabWidth = J(), fe.shouldCenterTabs = V(), s.nextTick(function() {
            fe.maxTabWidth = J(), ne(fe.selectedIndex);
          }));
        }

        function C(e) {
          r[e ? "removeClass" : "addClass"]("md-no-tab-content");
        }

        function x(n) {
          var r = H(),
            i = fe.shouldCenterTabs ? "" : "-" + n + "px";
          t.element(r.paging).css(o.CSS.TRANSFORM, "translate3d(" + i + ", 0, 0)"), e.$broadcast(
            "$mdTabsPaginationChanged");
        }

        function S(e, t) {
          e !== t && H().tabs[e] && (ne(), te());
        }

        function A(t, n) {
          t !== n && (fe.selectedIndex = Y(t), fe.lastSelectedIndex = n, fe.updateInkBarStyles(), ae(), ne(t),
            e.$broadcast("$mdTabsChanged"), fe.tabs[n] && fe.tabs[n].scope.deselect(), fe.tabs[t] && fe
            .tabs[t].scope.select());
        }

        function M(e) {
          var t = r[0].getElementsByTagName("md-tab");
          return Array.prototype.indexOf.call(t, e[0]);
        }

        function k() {
          k.watcher || (k.watcher = e.$watch(function() {
            s.nextTick(function() {
              k.watcher && r.prop("offsetParent") && (k.watcher(), k.watcher = null, P());
            }, !1);
          }));
        }

        function N(e) {
          switch (e.keyCode) {
            case o.KEY_CODE.LEFT_ARROW:
              e.preventDefault(), ee(-1, !0);
              break;
            case o.KEY_CODE.RIGHT_ARROW:
              e.preventDefault(), ee(1, !0);
              break;
            case o.KEY_CODE.SPACE:
            case o.KEY_CODE.ENTER:
              e.preventDefault(), he || I(fe.focusIndex);
          }
        }

        function I(e, t) {
          he || (fe.focusIndex = fe.selectedIndex = e), t && fe.noSelectClick || s.nextTick(function() {
            fe.tabs[e].element.triggerHandler("click");
          }, !1);
        }

        function O(e) {
          fe.shouldPaginate && (e.preventDefault(), fe.offsetLeft = ue(fe.offsetLeft - e.wheelDelta));
        }

        function D() {
          var e,
            t,
            n = H(),
            r = n.canvas.clientWidth,
            i = r + fe.offsetLeft;
          for (e = 0; e < n.tabs.length && (t = n.tabs[e], !(t.offsetLeft + t.offsetWidth > i)); e++);
          r > t.offsetWidth ? fe.offsetLeft = ue(t.offsetLeft) : fe.offsetLeft = ue(t.offsetLeft + (t
            .offsetWidth - r + 1));
        }

        function R() {
          var e,
            t,
            n = H();
          for (e = 0; e < n.tabs.length && (t = n.tabs[e], !(t.offsetLeft + t.offsetWidth >= fe
            .offsetLeft)); e++);
          n.canvas.clientWidth > t.offsetWidth ? fe.offsetLeft = ue(t.offsetLeft + t.offsetWidth - n.canvas
            .clientWidth) : fe.offsetLeft = ue(t.offsetLeft);
        }

        function P() {
          fe.lastSelectedIndex = fe.selectedIndex, fe.offsetLeft = ue(fe.offsetLeft), s.nextTick(function() {
            fe.updateInkBarStyles(), X();
          });
        }

        function L(e) {
          t.element(H().inkBar).toggleClass("ng-hide", e);
        }

        function U(e) {
          r.toggleClass("md-dynamic-height", e);
        }

        function F(e) {
          if (!ve) {
            var t = fe.selectedIndex,
              n = fe.tabs.splice(e.getIndex(), 1)[0];
            oe(), fe.selectedIndex === t && (n.scope.deselect(), fe.tabs[fe.selectedIndex] && fe.tabs[fe
              .selectedIndex].scope.select()), s.nextTick(function() {
              X(), fe.offsetLeft = ue(fe.offsetLeft);
            });
          }
        }

        function j(e, n) {
          var r = ge,
            i = {
              getIndex: function() {
                return fe.tabs.indexOf(o);
              },
              isActive: function() {
                return this.getIndex() === fe.selectedIndex;
              },
              isLeft: function() {
                return this.getIndex() < fe.selectedIndex;
              },
              isRight: function() {
                return this.getIndex() > fe.selectedIndex;
              },
              shouldRender: function() {
                return !fe.noDisconnect || this.isActive();
              },
              hasFocus: function() {
                return fe.styleTabItemFocus && fe.hasFocus && this.getIndex() === fe.focusIndex;
              },
              id: s.nextUid(),
              hasContent: !(!e.template || !e.template.trim())
            },
            o = t.extend(i, e);
          return t.isDefined(n) ? fe.tabs.splice(n, 0, o) : fe.tabs.push(o), re(), ie(), s.nextTick(
        function() {
            X(), de(o), r && fe.autoselect && s.nextTick(function() {
              s.nextTick(function() {
                I(fe.tabs.indexOf(o));
              });
            });
          }), o;
        }

        function H() {
          var e = {},
            t = r[0];
          return e.wrapper = t.querySelector("md-tabs-wrapper"), e.canvas = e.wrapper.querySelector(
              "md-tabs-canvas"), e.paging = e.canvas.querySelector("md-pagination-wrapper"), e.inkBar = e
            .paging.querySelector("md-ink-bar"), e.contents = t.querySelectorAll(
              "md-tabs-content-wrapper > md-tab-content"), e.tabs = e.paging.querySelectorAll("md-tab-item"),
            e.dummies = e.canvas.querySelectorAll("md-dummy-tab"), e;
        }

        function B() {
          return fe.offsetLeft > 0;
        }

        function z() {
          var e = H(),
            t = e.tabs[e.tabs.length - 1];
          return t && t.offsetLeft + t.offsetWidth > e.canvas.clientWidth + fe.offsetLeft;
        }

        function q() {
          var e = fe.tabs[fe.focusIndex];
          return e && e.id ? "tab-item-" + e.id : null;
        }

        function G() {
          switch (fe.stretchTabs) {
            case "always":
              return !0;
            case "never":
              return !1;
            default:
              return !fe.shouldPaginate && i.matchMedia("(max-width: 600px)").matches;
          }
        }

        function V() {
          return fe.centerTabs && !fe.shouldPaginate;
        }

        function W() {
          if (fe.noPagination || !ge) return !1;
          var e = r.prop("clientWidth");
          return t.forEach(H().dummies, function(t) {
            e -= t.offsetWidth;
          }), e < 0;
        }

        function Y(e) {
          if (e === -1) return -1;
          var t,
            n,
            r = Math.max(fe.tabs.length - e, e);
          for (t = 0; t <= r; t++) {
            if (n = fe.tabs[e + t], n && n.scope.disabled !== !0) return n.getIndex();
            if (n = fe.tabs[e - t], n && n.scope.disabled !== !0) return n.getIndex();
          }
          return e;
        }

        function K(e, t, n) {
          Object.defineProperty(fe, e, {
            get: function() {
              return n;
            },
            set: function(e) {
              var r = n;
              n = e, t && t(e, r);
            }
          });
        }

        function X() {
          fe.maxTabWidth = J(), fe.shouldPaginate = W();
        }

        function Q(e) {
          var n = 0;
          return t.forEach(e, function(e) {
            n += Math.max(e.offsetWidth, e.getBoundingClientRect().width);
          }), Math.ceil(n);
        }

        function J() {
          return r.prop("clientWidth");
        }

        function Z() {
          var e = fe.tabs[fe.selectedIndex],
            t = fe.tabs[fe.focusIndex];
          fe.tabs = fe.tabs.sort(function(e, t) {
            return e.index - t.index;
          }), fe.selectedIndex = fe.tabs.indexOf(e), fe.focusIndex = fe.tabs.indexOf(t);
        }

        function ee(e, t) {
          var n,
            r = t ? "focusIndex" : "selectedIndex",
            i = fe[r];
          for (n = i + e; fe.tabs[n] && fe.tabs[n].scope.disabled; n += e);
          fe.tabs[n] && (fe[r] = n);
        }

        function te() {
          fe.styleTabItemFocus = "keyboard" === f.getLastInteractionType(), H().dummies[fe.focusIndex]
        .focus();
        }

        function ne(e) {
          var n = H();
          if (t.isNumber(e) || (e = fe.focusIndex), n.tabs[e] && !fe.shouldCenterTabs) {
            var r = n.tabs[e],
              i = r.offsetLeft,
              o = r.offsetWidth + i;
            fe.offsetLeft = Math.max(fe.offsetLeft, ue(o - n.canvas.clientWidth + 64)), fe.offsetLeft = Math
              .min(fe.offsetLeft, ue(i));
          }
        }

        function re() {
          me.forEach(function(e) {
            s.nextTick(e);
          }), me = [];
        }

        function ie() {
          for (var e = !1, t = 0; t < fe.tabs.length; t++)
            if (fe.tabs[t].hasContent) {
              e = !0;
              break;
            }
          fe.hasContent = e;
        }

        function oe() {
          fe.selectedIndex = Y(fe.selectedIndex), fe.focusIndex = Y(fe.focusIndex);
        }

        function ae() {
          if (!fe.dynamicHeight) return r.css("height", "");
          if (!fe.tabs.length) return me.push(ae);
          var e = H(),
            t = e.contents[fe.selectedIndex],
            i = t ? t.offsetHeight : 0,
            o = e.wrapper.offsetHeight,
            a = i + o,
            u = r.prop("clientHeight");
          if (u !== a) {
            "bottom" === r.attr("md-align-tabs") && (u -= o, a -= o, r.attr("md-border-bottom") !== n && ++u),
              he = !0;
            var l = {
                height: u + "px"
              },
              d = {
                height: a + "px"
              };
            r.css(l), c(r, {
              from: l,
              to: d,
              easing: "cubic-bezier(0.35, 0, 0.25, 1)",
              duration: .5
            }).start().done(function() {
              r.css({
                transition: "none",
                height: ""
              }), s.nextTick(function() {
                r.css("transition", "");
              }), he = !1;
            });
          }
        }

        function se() {
          var e = H();
          if (!e.tabs[fe.selectedIndex]) return void t.element(e.inkBar).css({
            left: "auto",
            right: "auto"
          });
          if (!fe.tabs.length) return me.push(fe.updateInkBarStyles);
          if (!r.prop("offsetParent")) return k();
          var n = fe.selectedIndex,
            i = e.paging.offsetWidth,
            o = e.tabs[n],
            a = o.offsetLeft,
            c = i - a - o.offsetWidth;
          if (fe.shouldCenterTabs) {
            var u = Q(e.tabs);
            i > u && s.nextTick(se, !1);
          }
          ce(), t.element(e.inkBar).css({
            left: a + "px",
            right: c + "px"
          });
        }

        function ce() {
          var e = H(),
            n = fe.selectedIndex,
            r = fe.lastSelectedIndex,
            i = t.element(e.inkBar);
          t.isNumber(r) && i.toggleClass("md-left", n < r).toggleClass("md-right", n > r);
        }

        function ue(e) {
          var t = H();
          if (!t.tabs.length || !fe.shouldPaginate) return 0;
          var n = t.tabs[t.tabs.length - 1],
            r = n.offsetLeft + n.offsetWidth;
          return e = Math.max(0, e), e = Math.min(r - t.canvas.clientWidth, e);
        }

        function le(e, n) {
          var r = H(),
            i = {
              colorElement: t.element(r.inkBar)
            };
          a.attach(e, n, i);
        }

        function de(e) {
          if (e.hasContent) {
            var n = r[0].querySelectorAll('[md-tab-id="' + e.id + '"]');
            t.element(n).attr("aria-controls", fe.tabContentPrefix + e.id);
          }
        }
        var fe = this,
          he = !1,
          pe = H(),
          me = [],
          ve = !1,
          ge = !1;
        fe.$onInit = h, fe.updatePagination = s.debounce(X, 100), fe.redirectFocus = te, fe.attachRipple = le,
          fe.insertTab = j, fe.removeTab = F, fe.select = I, fe.scroll = O, fe.nextPage = D, fe.previousPage =
          R, fe.keydown = N, fe.canPageForward = z, fe.canPageBack = B, fe.refreshIndex = oe, fe
          .incrementIndex = ee, fe.getTabElementIndex = M, fe.updateInkBarStyles = s.debounce(se, 100), fe
          .updateTabOrder = s.debounce(Z, 100), fe.getFocusedTabId = q, 1 === t.version.major && t.version
          .minor <= 4 && this.$onInit();
      }
      e.$inject = ["$scope", "$element", "$window", "$mdConstant", "$mdTabInkRipple", "$mdUtil",
        "$animateCss", "$attrs", "$compile", "$mdTheming", "$mdInteraction"
      ], t.module("material.components.tabs").controller("MdTabsController", e);
    }(),
    function() {
      function e(e) {
        return {
          scope: {
            selectedIndex: "=?mdSelected"
          },
          template: function(t, n) {
            return n.$mdTabsTemplate = t.html(),
              '<md-tabs-wrapper> <md-tab-data></md-tab-data> <md-prev-button tabindex="-1" role="button" aria-label="Previous Page" aria-disabled="{{!$mdTabsCtrl.canPageBack()}}" ng-class="{ \'md-disabled\': !$mdTabsCtrl.canPageBack() }" ng-if="$mdTabsCtrl.shouldPaginate" ng-click="$mdTabsCtrl.previousPage()"> <md-icon md-svg-src="' +
              e.mdTabsArrow +
              '"></md-icon> </md-prev-button> <md-next-button tabindex="-1" role="button" aria-label="Next Page" aria-disabled="{{!$mdTabsCtrl.canPageForward()}}" ng-class="{ \'md-disabled\': !$mdTabsCtrl.canPageForward() }" ng-if="$mdTabsCtrl.shouldPaginate" ng-click="$mdTabsCtrl.nextPage()"> <md-icon md-svg-src="' +
              e.mdTabsArrow +
              '"></md-icon> </md-next-button> <md-tabs-canvas tabindex="{{ $mdTabsCtrl.hasFocus ? -1 : 0 }}" aria-activedescendant="{{$mdTabsCtrl.getFocusedTabId()}}" ng-focus="$mdTabsCtrl.redirectFocus()" ng-class="{ \'md-paginated\': $mdTabsCtrl.shouldPaginate, \'md-center-tabs\': $mdTabsCtrl.shouldCenterTabs }" ng-keydown="$mdTabsCtrl.keydown($event)" role="tablist"> <md-pagination-wrapper ng-class="{ \'md-center-tabs\': $mdTabsCtrl.shouldCenterTabs }" md-tab-scroll="$mdTabsCtrl.scroll($event)"> <md-tab-item tabindex="-1" class="md-tab" ng-repeat="tab in $mdTabsCtrl.tabs" role="tab" md-tab-id="{{::tab.id}}"aria-selected="{{tab.isActive()}}" aria-disabled="{{tab.scope.disabled || \'false\'}}" ng-click="$mdTabsCtrl.select(tab.getIndex())" ng-class="{ \'md-active\':    tab.isActive(), \'md-focused\':   tab.hasFocus(), \'md-disabled\':  tab.scope.disabled }" ng-disabled="tab.scope.disabled" md-swipe-left="$mdTabsCtrl.nextPage()" md-swipe-right="$mdTabsCtrl.previousPage()" md-tabs-template="::tab.label" md-scope="::tab.parent"></md-tab-item> <md-ink-bar></md-ink-bar> </md-pagination-wrapper> <md-tabs-dummy-wrapper class="md-visually-hidden md-dummy-wrapper"> <md-dummy-tab class="md-tab" tabindex="-1" id="tab-item-{{::tab.id}}" md-tab-id="{{::tab.id}}"aria-selected="{{tab.isActive()}}" aria-disabled="{{tab.scope.disabled || \'false\'}}" ng-focus="$mdTabsCtrl.hasFocus = true" ng-blur="$mdTabsCtrl.hasFocus = false" ng-repeat="tab in $mdTabsCtrl.tabs" md-tabs-template="::tab.label" md-scope="::tab.parent"></md-dummy-tab> </md-tabs-dummy-wrapper> </md-tabs-canvas> </md-tabs-wrapper> <md-tabs-content-wrapper ng-show="$mdTabsCtrl.hasContent && $mdTabsCtrl.selectedIndex >= 0" class="_md"> <md-tab-content id="{{:: $mdTabsCtrl.tabContentPrefix + tab.id}}" class="_md" role="tabpanel" aria-labelledby="tab-item-{{::tab.id}}" md-swipe-left="$mdTabsCtrl.swipeContent && $mdTabsCtrl.incrementIndex(1)" md-swipe-right="$mdTabsCtrl.swipeContent && $mdTabsCtrl.incrementIndex(-1)" ng-if="tab.hasContent" ng-repeat="(index, tab) in $mdTabsCtrl.tabs" ng-class="{ \'md-no-transition\': $mdTabsCtrl.lastSelectedIndex == null, \'md-active\':        tab.isActive(), \'md-left\':          tab.isLeft(), \'md-right\':         tab.isRight(), \'md-no-scroll\':     $mdTabsCtrl.dynamicHeight }"> <div md-tabs-template="::tab.template" md-connected-if="tab.isActive()" md-scope="::tab.parent" ng-if="$mdTabsCtrl.enableDisconnect || tab.shouldRender()"></div> </md-tab-content> </md-tabs-content-wrapper>';
          },
          controller: "MdTabsController",
          controllerAs: "$mdTabsCtrl",
          bindToController: !0
        };
      }
      e.$inject = ["$$mdSvgRegistry"], t.module("material.components.tabs").directive("mdTabs", e);
    }(),
    function() {
      function e(e, t) {
        return {
          require: "^?mdTabs",
          link: function(n, r, i, o) {
            if (o) {
              var a,
                s,
                c = function() {
                  o.updatePagination(), o.updateInkBarStyles();
                };
              if ("MutationObserver" in t) {
                var u = {
                  childList: !0,
                  subtree: !0,
                  characterData: !0
                };
                a = new MutationObserver(c), a.observe(r[0], u), s = a.disconnect.bind(a);
              } else {
                var l = e.debounce(c, 15, null, !1);
                r.on("DOMSubtreeModified", l), s = r.off.bind(r, "DOMSubtreeModified", l);
              }
              n.$on("$destroy", function() {
                s();
              });
            }
          }
        };
      }
      e.$inject = ["$mdUtil", "$window"], t.module("material.components.tabs").directive("mdTabsDummyWrapper",
        e);
    }(),
    function() {
      function e(e, t) {
        function n(n, r, i, o) {
          function a() {
            n.$watch("connected", function(e) {
              e === !1 ? s() : c();
            }), n.$on("$destroy", c);
          }

          function s() {
            o.enableDisconnect && t.disconnectScope(u);
          }

          function c() {
            o.enableDisconnect && t.reconnectScope(u);
          }
          if (o) {
            var u = o.enableDisconnect ? n.compileScope.$new() : n.compileScope;
            return r.html(n.template), e(r.contents())(u), t.nextTick(a);
          }
        }
        return {
          restrict: "A",
          link: n,
          scope: {
            template: "=mdTabsTemplate",
            connected: "=?mdConnectedIf",
            compileScope: "=mdScope"
          },
          require: "^?mdTabs"
        };
      }
      e.$inject = ["$compile", "$mdUtil"], t.module("material.components.tabs").directive("mdTabsTemplate",
      e);
    }(),
    function() {
      t.module("material.core").constant("$MD_THEME_CSS",
        'md-autocomplete.md-THEME_NAME-theme{background:"{{background-A100}}"}md-autocomplete.md-THEME_NAME-theme[disabled]:not([md-floating-label]){background:"{{background-100}}"}md-autocomplete.md-THEME_NAME-theme button md-icon path{fill:"{{background-600}}"}md-autocomplete.md-THEME_NAME-theme button:after{background:"{{background-600-0.3}}"}.md-autocomplete-suggestions-container.md-THEME_NAME-theme{background:"{{background-A100}}"}.md-autocomplete-suggestions-container.md-THEME_NAME-theme li{color:"{{background-900}}"}.md-autocomplete-suggestions-container.md-THEME_NAME-theme li .highlight{color:"{{background-600}}"}.md-autocomplete-suggestions-container.md-THEME_NAME-theme li.selected,.md-autocomplete-suggestions-container.md-THEME_NAME-theme li:hover{background:"{{background-200}}"}.md-button.md-THEME_NAME-theme:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme:not([disabled]):hover{background-color:"{{background-500-0.2}}"}.md-button.md-THEME_NAME-theme:not([disabled]).md-icon-button:hover{background-color:transparent}.md-button.md-THEME_NAME-theme.md-fab md-icon{color:"{{accent-contrast}}"}.md-button.md-THEME_NAME-theme.md-primary{color:"{{primary-color}}"}.md-button.md-THEME_NAME-theme.md-primary.md-fab,.md-button.md-THEME_NAME-theme.md-primary.md-raised{color:"{{primary-contrast}}";background-color:"{{primary-color}}"}.md-button.md-THEME_NAME-theme.md-primary.md-fab:not([disabled]) md-icon,.md-button.md-THEME_NAME-theme.md-primary.md-raised:not([disabled]) md-icon{color:"{{primary-contrast}}"}.md-button.md-THEME_NAME-theme.md-primary.md-fab:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-primary.md-fab:not([disabled]):hover,.md-button.md-THEME_NAME-theme.md-primary.md-raised:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-primary.md-raised:not([disabled]):hover{background-color:"{{primary-600}}"}.md-button.md-THEME_NAME-theme.md-primary:not([disabled]) md-icon{color:"{{primary-color}}"}.md-button.md-THEME_NAME-theme.md-fab{background-color:"{{accent-color}}";color:"{{accent-contrast}}"}.md-button.md-THEME_NAME-theme.md-fab:not([disabled]) .md-icon{color:"{{accent-contrast}}"}.md-button.md-THEME_NAME-theme.md-fab:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-fab:not([disabled]):hover{background-color:"{{accent-A700}}"}.md-button.md-THEME_NAME-theme.md-raised{color:"{{background-900}}";background-color:"{{background-50}}"}.md-button.md-THEME_NAME-theme.md-raised:not([disabled]) md-icon{color:"{{background-900}}"}.md-button.md-THEME_NAME-theme.md-raised:not([disabled]):hover{background-color:"{{background-50}}"}.md-button.md-THEME_NAME-theme.md-raised:not([disabled]).md-focused{background-color:"{{background-200}}"}.md-button.md-THEME_NAME-theme.md-warn{color:"{{warn-color}}"}.md-button.md-THEME_NAME-theme.md-warn.md-fab,.md-button.md-THEME_NAME-theme.md-warn.md-raised{color:"{{warn-contrast}}";background-color:"{{warn-color}}"}.md-button.md-THEME_NAME-theme.md-warn.md-fab:not([disabled]) md-icon,.md-button.md-THEME_NAME-theme.md-warn.md-raised:not([disabled]) md-icon{color:"{{warn-contrast}}"}.md-button.md-THEME_NAME-theme.md-warn.md-fab:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-warn.md-fab:not([disabled]):hover,.md-button.md-THEME_NAME-theme.md-warn.md-raised:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-warn.md-raised:not([disabled]):hover{background-color:"{{warn-600}}"}.md-button.md-THEME_NAME-theme.md-warn:not([disabled]) md-icon{color:"{{warn-color}}"}.md-button.md-THEME_NAME-theme.md-accent{color:"{{accent-color}}"}.md-button.md-THEME_NAME-theme.md-accent.md-fab,.md-button.md-THEME_NAME-theme.md-accent.md-raised{color:"{{accent-contrast}}";background-color:"{{accent-color}}"}.md-button.md-THEME_NAME-theme.md-accent.md-fab:not([disabled]) md-icon,.md-button.md-THEME_NAME-theme.md-accent.md-raised:not([disabled]) md-icon{color:"{{accent-contrast}}"}.md-button.md-THEME_NAME-theme.md-accent.md-fab:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-accent.md-fab:not([disabled]):hover,.md-button.md-THEME_NAME-theme.md-accent.md-raised:not([disabled]).md-focused,.md-button.md-THEME_NAME-theme.md-accent.md-raised:not([disabled]):hover{background-color:"{{accent-A700}}"}.md-button.md-THEME_NAME-theme.md-accent:not([disabled]) md-icon{color:"{{accent-color}}"}.md-button.md-THEME_NAME-theme.md-accent[disabled],.md-button.md-THEME_NAME-theme.md-fab[disabled],.md-button.md-THEME_NAME-theme.md-raised[disabled],.md-button.md-THEME_NAME-theme.md-warn[disabled],.md-button.md-THEME_NAME-theme[disabled]{color:"{{foreground-3}}";cursor:default}.md-button.md-THEME_NAME-theme.md-accent[disabled] md-icon,.md-button.md-THEME_NAME-theme.md-fab[disabled] md-icon,.md-button.md-THEME_NAME-theme.md-raised[disabled] md-icon,.md-button.md-THEME_NAME-theme.md-warn[disabled] md-icon,.md-button.md-THEME_NAME-theme[disabled] md-icon{color:"{{foreground-3}}"}.md-button.md-THEME_NAME-theme.md-fab[disabled],.md-button.md-THEME_NAME-theme.md-raised[disabled]{background-color:"{{foreground-4}}"}.md-button.md-THEME_NAME-theme[disabled]{background-color:transparent}._md a.md-THEME_NAME-theme:not(.md-button).md-primary{color:"{{primary-color}}"}._md a.md-THEME_NAME-theme:not(.md-button).md-primary:hover{color:"{{primary-700}}"}._md a.md-THEME_NAME-theme:not(.md-button).md-accent:hover{color:"{{accent-700}}"}._md a.md-THEME_NAME-theme:not(.md-button).md-accent{color:"{{accent-color}}"}._md a.md-THEME_NAME-theme:not(.md-button).md-accent:hover{color:"{{accent-A700}}"}._md a.md-THEME_NAME-theme:not(.md-button).md-warn{color:"{{warn-color}}"}._md a.md-THEME_NAME-theme:not(.md-button).md-warn:hover{color:"{{warn-700}}"}md-bottom-sheet.md-THEME_NAME-theme{background-color:"{{background-50}}";border-top-color:"{{background-300}}"}md-bottom-sheet.md-THEME_NAME-theme.md-list md-list-item{color:"{{foreground-1}}"}md-bottom-sheet.md-THEME_NAME-theme .md-subheader{background-color:"{{background-50}}";color:"{{foreground-1}}"}md-backdrop{background-color:"{{background-900-0.0}}"}md-backdrop.md-opaque.md-THEME_NAME-theme{background-color:"{{background-900-1.0}}"}md-card.md-THEME_NAME-theme{color:"{{foreground-1}}";background-color:"{{background-hue-1}}";border-radius:2px}md-card.md-THEME_NAME-theme .md-card-image{border-radius:2px 2px 0 0}md-card.md-THEME_NAME-theme md-card-header md-card-avatar md-icon{color:"{{background-color}}";background-color:"{{foreground-3}}"}md-card.md-THEME_NAME-theme md-card-header md-card-header-text .md-subhead,md-card.md-THEME_NAME-theme md-card-title md-card-title-text:not(:only-child) .md-subhead{color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme .md-ripple{color:"{{accent-A700}}"}md-checkbox.md-THEME_NAME-theme.md-checked .md-ripple{color:"{{background-600}}"}md-checkbox.md-THEME_NAME-theme.md-checked.md-focused .md-container:before{background-color:"{{accent-color-0.26}}"}md-checkbox.md-THEME_NAME-theme .md-ink-ripple{color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme.md-checked .md-ink-ripple{color:"{{accent-color-0.87}}"}md-checkbox.md-THEME_NAME-theme:not(.md-checked) .md-icon{border-color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme.md-checked .md-icon{background-color:"{{accent-color-0.87}}"}md-checkbox.md-THEME_NAME-theme.md-checked .md-icon:after{border-color:"{{accent-contrast-0.87}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary .md-ripple{color:"{{primary-600}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-ripple{color:"{{background-600}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary .md-ink-ripple{color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-ink-ripple{color:"{{primary-color-0.87}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary:not(.md-checked) .md-icon{border-color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-icon{background-color:"{{primary-color-0.87}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked.md-focused .md-container:before{background-color:"{{primary-color-0.26}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-icon:after{border-color:"{{primary-contrast-0.87}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-primary .md-indeterminate[disabled] .md-container{color:"{{foreground-3}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn .md-ripple{color:"{{warn-600}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn .md-ink-ripple{color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-ink-ripple{color:"{{warn-color-0.87}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn:not(.md-checked) .md-icon{border-color:"{{foreground-2}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-icon{background-color:"{{warn-color-0.87}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked.md-focused:not([disabled]) .md-container:before{background-color:"{{warn-color-0.26}}"}md-checkbox.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-icon:after{border-color:"{{background-200}}"}md-checkbox.md-THEME_NAME-theme[disabled]:not(.md-checked) .md-icon{border-color:"{{foreground-3}}"}md-checkbox.md-THEME_NAME-theme[disabled].md-checked .md-icon{background-color:"{{foreground-3}}"}md-checkbox.md-THEME_NAME-theme[disabled].md-checked .md-icon:after{border-color:"{{background-200}}"}md-checkbox.md-THEME_NAME-theme[disabled] .md-icon:after{border-color:"{{foreground-3}}"}md-checkbox.md-THEME_NAME-theme[disabled] .md-label{color:"{{foreground-3}}"}md-content.md-THEME_NAME-theme{color:"{{foreground-1}}";background-color:"{{background-default}}"}md-chips.md-THEME_NAME-theme .md-chips{box-shadow:0 1px "{{foreground-4}}"}md-chips.md-THEME_NAME-theme .md-chips.md-focused{box-shadow:0 2px "{{primary-color}}"}md-chips.md-THEME_NAME-theme .md-chips .md-chip-input-container input{color:"{{foreground-1}}"}md-chips.md-THEME_NAME-theme .md-chips .md-chip-input-container input:-moz-placeholder,md-chips.md-THEME_NAME-theme .md-chips .md-chip-input-container input::-moz-placeholder{color:"{{foreground-3}}"}md-chips.md-THEME_NAME-theme .md-chips .md-chip-input-container input:-ms-input-placeholder{color:"{{foreground-3}}"}md-chips.md-THEME_NAME-theme .md-chips .md-chip-input-container input::-webkit-input-placeholder{color:"{{foreground-3}}"}md-chips.md-THEME_NAME-theme md-chip{background:"{{background-300}}";color:"{{background-800}}"}md-chips.md-THEME_NAME-theme md-chip md-icon{color:"{{background-700}}"}md-chips.md-THEME_NAME-theme md-chip.md-focused{background:"{{primary-color}}";color:"{{primary-contrast}}"}md-chips.md-THEME_NAME-theme md-chip.md-focused md-icon{color:"{{primary-contrast}}"}md-chips.md-THEME_NAME-theme md-chip._md-chip-editing{background:transparent;color:"{{background-800}}"}md-chips.md-THEME_NAME-theme md-chip-remove .md-button md-icon path{fill:"{{background-500}}"}.md-contact-suggestion span.md-contact-email{color:"{{background-400}}"}md-dialog.md-THEME_NAME-theme{border-radius:4px;background-color:"{{background-hue-1}}";color:"{{foreground-1}}"}md-dialog.md-THEME_NAME-theme.md-content-overflow .md-actions,md-dialog.md-THEME_NAME-theme.md-content-overflow md-dialog-actions,md-divider.md-THEME_NAME-theme{border-top-color:"{{foreground-4}}"}.layout-gt-lg-row>md-divider.md-THEME_NAME-theme,.layout-gt-md-row>md-divider.md-THEME_NAME-theme,.layout-gt-sm-row>md-divider.md-THEME_NAME-theme,.layout-gt-xs-row>md-divider.md-THEME_NAME-theme,.layout-lg-row>md-divider.md-THEME_NAME-theme,.layout-md-row>md-divider.md-THEME_NAME-theme,.layout-row>md-divider.md-THEME_NAME-theme,.layout-sm-row>md-divider.md-THEME_NAME-theme,.layout-xl-row>md-divider.md-THEME_NAME-theme,.layout-xs-row>md-divider.md-THEME_NAME-theme{border-right-color:"{{foreground-4}}"}.md-calendar.md-THEME_NAME-theme{background:"{{background-A100}}";color:"{{background-A200-0.87}}"}.md-calendar.md-THEME_NAME-theme tr:last-child td{border-bottom-color:"{{background-200}}"}.md-THEME_NAME-theme .md-calendar-day-header{background:"{{background-300}}";color:"{{background-A200-0.87}}"}.md-THEME_NAME-theme .md-calendar-date.md-calendar-date-today .md-calendar-date-selection-indicator{border:1px solid "{{primary-500}}"}.md-THEME_NAME-theme .md-calendar-date.md-calendar-date-today.md-calendar-date-disabled{color:"{{primary-500-0.6}}"}.md-calendar-date.md-focus .md-THEME_NAME-theme .md-calendar-date-selection-indicator,.md-THEME_NAME-theme .md-calendar-date-selection-indicator:hover{background:"{{background-300}}"}.md-THEME_NAME-theme .md-calendar-date.md-calendar-selected-date .md-calendar-date-selection-indicator,.md-THEME_NAME-theme .md-calendar-date.md-focus.md-calendar-selected-date .md-calendar-date-selection-indicator{background:"{{primary-500}}";color:"{{primary-500-contrast}}";border-color:transparent}.md-THEME_NAME-theme .md-calendar-date-disabled,.md-THEME_NAME-theme .md-calendar-month-label-disabled{color:"{{background-A200-0.435}}"}.md-THEME_NAME-theme .md-datepicker-input{color:"{{foreground-1}}"}.md-THEME_NAME-theme .md-datepicker-input:-moz-placeholder,.md-THEME_NAME-theme .md-datepicker-input::-moz-placeholder{color:"{{foreground-3}}"}.md-THEME_NAME-theme .md-datepicker-input:-ms-input-placeholder{color:"{{foreground-3}}"}.md-THEME_NAME-theme .md-datepicker-input::-webkit-input-placeholder{color:"{{foreground-3}}"}.md-THEME_NAME-theme .md-datepicker-input-container{border-bottom-color:"{{foreground-4}}"}.md-THEME_NAME-theme .md-datepicker-input-container.md-datepicker-focused{border-bottom-color:"{{primary-color}}"}.md-accent .md-THEME_NAME-theme .md-datepicker-input-container.md-datepicker-focused{border-bottom-color:"{{accent-color}}"}.md-THEME_NAME-theme .md-datepicker-input-container.md-datepicker-invalid,.md-warn .md-THEME_NAME-theme .md-datepicker-input-container.md-datepicker-focused{border-bottom-color:"{{warn-A700}}"}.md-THEME_NAME-theme .md-datepicker-calendar-pane{border-color:"{{background-hue-1}}"}.md-THEME_NAME-theme .md-datepicker-triangle-button .md-datepicker-expand-triangle{border-top-color:"{{foreground-2}}"}.md-THEME_NAME-theme .md-datepicker-open .md-datepicker-calendar-icon{color:"{{primary-color}}"}.md-accent .md-THEME_NAME-theme .md-datepicker-open .md-datepicker-calendar-icon,.md-THEME_NAME-theme .md-datepicker-open.md-accent .md-datepicker-calendar-icon{color:"{{accent-color}}"}.md-THEME_NAME-theme .md-datepicker-open.md-warn .md-datepicker-calendar-icon,.md-warn .md-THEME_NAME-theme .md-datepicker-open .md-datepicker-calendar-icon{color:"{{warn-A700}}"}.md-THEME_NAME-theme .md-datepicker-calendar{background:"{{background-A100}}"}.md-THEME_NAME-theme .md-datepicker-input-mask-opaque{box-shadow:0 0 0 9999px "{{background-hue-1}}"}.md-THEME_NAME-theme .md-datepicker-open .md-datepicker-input-container{background:"{{background-hue-1}}"}md-icon.md-THEME_NAME-theme{color:"{{foreground-2}}"}md-icon.md-THEME_NAME-theme.md-primary{color:"{{primary-color}}"}md-icon.md-THEME_NAME-theme.md-accent{color:"{{accent-color}}"}md-icon.md-THEME_NAME-theme.md-warn{color:"{{warn-color}}"}md-input-container.md-THEME_NAME-theme .md-input{color:"{{foreground-1}}";border-color:"{{foreground-4}}"}md-input-container.md-THEME_NAME-theme .md-input:-moz-placeholder,md-input-container.md-THEME_NAME-theme .md-input::-moz-placeholder{color:"{{foreground-3}}"}md-input-container.md-THEME_NAME-theme .md-input:-ms-input-placeholder{color:"{{foreground-3}}"}md-input-container.md-THEME_NAME-theme .md-input::-webkit-input-placeholder{color:"{{foreground-3}}"}md-input-container.md-THEME_NAME-theme>md-icon{color:"{{foreground-1}}"}md-input-container.md-THEME_NAME-theme .md-placeholder,md-input-container.md-THEME_NAME-theme label{color:"{{foreground-3}}"}md-input-container.md-THEME_NAME-theme label.md-required:after{color:"{{warn-A700}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-focused):not(.md-input-invalid) label.md-required:after{color:"{{foreground-2}}"}md-input-container.md-THEME_NAME-theme .md-input-message-animation,md-input-container.md-THEME_NAME-theme .md-input-messages-animation{color:"{{warn-A700}}"}md-input-container.md-THEME_NAME-theme .md-input-message-animation .md-char-counter,md-input-container.md-THEME_NAME-theme .md-input-messages-animation .md-char-counter{color:"{{foreground-1}}"}md-input-container.md-THEME_NAME-theme.md-input-focused .md-input:-moz-placeholder,md-input-container.md-THEME_NAME-theme.md-input-focused .md-input::-moz-placeholder{color:"{{foreground-2}}"}md-input-container.md-THEME_NAME-theme.md-input-focused .md-input:-ms-input-placeholder{color:"{{foreground-2}}"}md-input-container.md-THEME_NAME-theme.md-input-focused .md-input::-webkit-input-placeholder{color:"{{foreground-2}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-has-value label{color:"{{foreground-2}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused .md-input,md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-resized .md-input{border-color:"{{primary-color}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused label,md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused md-icon{color:"{{primary-color}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused.md-accent .md-input{border-color:"{{accent-color}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused.md-accent label,md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused.md-accent md-icon{color:"{{accent-color}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused.md-warn .md-input{border-color:"{{warn-A700}}"}md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused.md-warn label,md-input-container.md-THEME_NAME-theme:not(.md-input-invalid).md-input-focused.md-warn md-icon{color:"{{warn-A700}}"}md-input-container.md-THEME_NAME-theme.md-input-invalid .md-input{border-color:"{{warn-A700}}"}md-input-container.md-THEME_NAME-theme.md-input-invalid .md-char-counter,md-input-container.md-THEME_NAME-theme.md-input-invalid .md-input-message-animation,md-input-container.md-THEME_NAME-theme.md-input-invalid label{color:"{{warn-A700}}"}[disabled] md-input-container.md-THEME_NAME-theme .md-input,md-input-container.md-THEME_NAME-theme .md-input[disabled]{border-bottom-color:transparent;color:"{{foreground-3}}";background-image:linear-gradient(90deg,"{{foreground-3}}" 0,"{{foreground-3}}" 33%,transparent 0);background-image:-ms-linear-gradient(left,transparent 0,"{{foreground-3}}" 100%)}md-list.md-THEME_NAME-theme md-list-item.md-2-line .md-list-item-text h3,md-list.md-THEME_NAME-theme md-list-item.md-2-line .md-list-item-text h4,md-list.md-THEME_NAME-theme md-list-item.md-3-line .md-list-item-text h3,md-list.md-THEME_NAME-theme md-list-item.md-3-line .md-list-item-text h4{color:"{{foreground-1}}"}md-list.md-THEME_NAME-theme md-list-item.md-2-line .md-list-item-text p,md-list.md-THEME_NAME-theme md-list-item.md-3-line .md-list-item-text p{color:"{{foreground-2}}"}md-list.md-THEME_NAME-theme .md-proxy-focus.md-focused div.md-no-style{background-color:"{{background-100}}"}md-list.md-THEME_NAME-theme md-list-item .md-avatar-icon{background-color:"{{foreground-3}}";color:"{{background-color}}"}md-list.md-THEME_NAME-theme md-list-item>md-icon{color:"{{foreground-2}}"}md-list.md-THEME_NAME-theme md-list-item>md-icon.md-highlight{color:"{{primary-color}}"}md-list.md-THEME_NAME-theme md-list-item>md-icon.md-highlight.md-accent{color:"{{accent-color}}"}._md-panel-backdrop.md-THEME_NAME-theme{background-color:"{{background-900-1.0}}"}md-menu-bar.md-THEME_NAME-theme>button.md-button{color:"{{foreground-2}}";border-radius:2px}md-menu-bar.md-THEME_NAME-theme md-menu.md-open>button,md-menu-bar.md-THEME_NAME-theme md-menu>button:focus{outline:none;background:"{{background-200}}"}md-menu-bar.md-THEME_NAME-theme.md-open:not(.md-keyboard-mode) md-menu:hover>button{background-color:"{{ background-500-0.2}}"}md-menu-bar.md-THEME_NAME-theme:not(.md-keyboard-mode):not(.md-open) md-menu button:focus,md-menu-bar.md-THEME_NAME-theme:not(.md-keyboard-mode):not(.md-open) md-menu button:hover{background:transparent}md-menu-content.md-THEME_NAME-theme .md-menu>.md-button:after{color:"{{background-A200-0.54}}"}md-menu-content.md-THEME_NAME-theme .md-menu.md-open>.md-button{background-color:"{{ background-500-0.2}}"}md-toolbar.md-THEME_NAME-theme.md-menu-toolbar{background-color:"{{background-A100}}";color:"{{background-A200}}"}md-toolbar.md-THEME_NAME-theme.md-menu-toolbar md-toolbar-filler{background-color:"{{primary-color}}";color:"{{background-A100-0.87}}"}md-toolbar.md-THEME_NAME-theme.md-menu-toolbar md-toolbar-filler md-icon{color:"{{background-A100-0.87}}"}md-menu-content.md-THEME_NAME-theme{background-color:"{{background-A100}}"}md-menu-content.md-THEME_NAME-theme md-menu-item{color:"{{background-A200-0.87}}"}md-menu-content.md-THEME_NAME-theme md-menu-item md-icon{color:"{{background-A200-0.54}}"}md-menu-content.md-THEME_NAME-theme md-menu-item .md-button[disabled],md-menu-content.md-THEME_NAME-theme md-menu-item .md-button[disabled] md-icon{color:"{{background-A200-0.25}}"}md-menu-content.md-THEME_NAME-theme md-menu-divider{background-color:"{{background-A200-0.11}}"}md-nav-bar.md-THEME_NAME-theme .md-nav-bar{background-color:transparent;border-color:"{{foreground-4}}"}md-nav-bar.md-THEME_NAME-theme .md-button._md-nav-button.md-unselected{color:"{{foreground-2}}"}md-nav-bar.md-THEME_NAME-theme md-nav-ink-bar{color:"{{accent-color}}";background:"{{accent-color}}"}md-progress-circular.md-THEME_NAME-theme path{stroke:"{{primary-color}}"}md-progress-circular.md-THEME_NAME-theme.md-warn path{stroke:"{{warn-color}}"}md-progress-circular.md-THEME_NAME-theme.md-accent path{stroke:"{{accent-color}}"}md-progress-linear.md-THEME_NAME-theme .md-container{background-color:"{{primary-100}}"}md-progress-linear.md-THEME_NAME-theme .md-bar{background-color:"{{primary-color}}"}md-progress-linear.md-THEME_NAME-theme.md-warn .md-container{background-color:"{{warn-100}}"}md-progress-linear.md-THEME_NAME-theme.md-warn .md-bar{background-color:"{{warn-color}}"}md-progress-linear.md-THEME_NAME-theme.md-accent .md-container{background-color:"{{accent-100}}"}md-progress-linear.md-THEME_NAME-theme.md-accent .md-bar{background-color:"{{accent-color}}"}md-progress-linear.md-THEME_NAME-theme[md-mode=buffer].md-warn .md-bar1{background-color:"{{warn-100}}"}md-progress-linear.md-THEME_NAME-theme[md-mode=buffer].md-warn .md-dashed:before{background:radial-gradient("{{warn-100}}" 0,"{{warn-100}}" 16%,transparent 42%)}md-progress-linear.md-THEME_NAME-theme[md-mode=buffer].md-accent .md-bar1{background-color:"{{accent-100}}"}md-progress-linear.md-THEME_NAME-theme[md-mode=buffer].md-accent .md-dashed:before{background:radial-gradient("{{accent-100}}" 0,"{{accent-100}}" 16%,transparent 42%)}md-input-container md-select.md-THEME_NAME-theme .md-select-value span:first-child:after{color:"{{warn-A700}}"}md-input-container:not(.md-input-focused):not(.md-input-invalid) md-select.md-THEME_NAME-theme .md-select-value span:first-child:after{color:"{{foreground-3}}"}md-input-container.md-input-focused:not(.md-input-has-value) md-select.md-THEME_NAME-theme .md-select-value,md-input-container.md-input-focused:not(.md-input-has-value) md-select.md-THEME_NAME-theme .md-select-value.md-select-placeholder{color:"{{primary-color}}"}md-input-container.md-input-invalid md-select.md-THEME_NAME-theme .md-select-value{color:"{{warn-A700}}"!important;border-bottom-color:"{{warn-A700}}"!important}md-input-container.md-input-invalid md-select.md-THEME_NAME-theme.md-no-underline .md-select-value{border-bottom-color:transparent!important}md-select.md-THEME_NAME-theme[disabled] .md-select-value{border-bottom-color:transparent;background-image:linear-gradient(90deg,"{{foreground-3}}" 0,"{{foreground-3}}" 33%,transparent 0);background-image:-ms-linear-gradient(left,transparent 0,"{{foreground-3}}" 100%)}md-select.md-THEME_NAME-theme .md-select-value{border-bottom-color:"{{foreground-4}}"}md-select.md-THEME_NAME-theme .md-select-value.md-select-placeholder{color:"{{foreground-3}}"}md-select.md-THEME_NAME-theme .md-select-value span:first-child:after{color:"{{warn-A700}}"}md-select.md-THEME_NAME-theme.md-no-underline .md-select-value{border-bottom-color:transparent!important}md-select.md-THEME_NAME-theme.ng-invalid.ng-touched .md-select-value{color:"{{warn-A700}}"!important;border-bottom-color:"{{warn-A700}}"!important}md-select.md-THEME_NAME-theme.ng-invalid.ng-touched.md-no-underline .md-select-value{border-bottom-color:transparent!important}md-select.md-THEME_NAME-theme:not([disabled]):focus .md-select-value{border-bottom-color:"{{primary-color}}";color:"{{ foreground-1 }}"}md-select.md-THEME_NAME-theme:not([disabled]):focus .md-select-value.md-select-placeholder{color:"{{ foreground-1 }}"}md-select.md-THEME_NAME-theme:not([disabled]):focus.md-no-underline .md-select-value{border-bottom-color:transparent!important}md-select.md-THEME_NAME-theme:not([disabled]):focus.md-accent .md-select-value{border-bottom-color:"{{accent-color}}"}md-select.md-THEME_NAME-theme:not([disabled]):focus.md-warn .md-select-value{border-bottom-color:"{{warn-color}}"}md-select.md-THEME_NAME-theme[disabled] .md-select-icon,md-select.md-THEME_NAME-theme[disabled] .md-select-value,md-select.md-THEME_NAME-theme[disabled] .md-select-value.md-select-placeholder{color:"{{foreground-3}}"}md-select.md-THEME_NAME-theme .md-select-icon{color:"{{foreground-2}}"}md-select-menu.md-THEME_NAME-theme md-content{background:"{{background-A100}}"}md-select-menu.md-THEME_NAME-theme md-content md-optgroup{color:"{{background-600-0.87}}"}md-select-menu.md-THEME_NAME-theme md-content md-option{color:"{{background-900-0.87}}"}md-select-menu.md-THEME_NAME-theme md-content md-option[disabled] .md-text{color:"{{background-400-0.87}}"}md-select-menu.md-THEME_NAME-theme md-content md-option:not([disabled]):focus,md-select-menu.md-THEME_NAME-theme md-content md-option:not([disabled]):hover{background:"{{background-200}}"}md-select-menu.md-THEME_NAME-theme md-content md-option[selected]{color:"{{primary-500}}"}md-select-menu.md-THEME_NAME-theme md-content md-option[selected]:focus{color:"{{primary-600}}"}md-select-menu.md-THEME_NAME-theme md-content md-option[selected].md-accent{color:"{{accent-color}}"}md-select-menu.md-THEME_NAME-theme md-content md-option[selected].md-accent:focus{color:"{{accent-A700}}"}.md-checkbox-enabled.md-THEME_NAME-theme .md-ripple{color:"{{primary-600}}"}.md-checkbox-enabled.md-THEME_NAME-theme[selected] .md-ripple{color:"{{background-600}}"}.md-checkbox-enabled.md-THEME_NAME-theme .md-ink-ripple{color:"{{foreground-2}}"}.md-checkbox-enabled.md-THEME_NAME-theme[selected] .md-ink-ripple{color:"{{primary-color-0.87}}"}.md-checkbox-enabled.md-THEME_NAME-theme:not(.md-checked) .md-icon{border-color:"{{foreground-2}}"}.md-checkbox-enabled.md-THEME_NAME-theme[selected] .md-icon{background-color:"{{primary-color-0.87}}"}.md-checkbox-enabled.md-THEME_NAME-theme[selected].md-focused .md-container:before{background-color:"{{primary-color-0.26}}"}.md-checkbox-enabled.md-THEME_NAME-theme[selected] .md-icon:after{border-color:"{{primary-contrast-0.87}}"}.md-checkbox-enabled.md-THEME_NAME-theme .md-indeterminate[disabled] .md-container{color:"{{foreground-3}}"}.md-checkbox-enabled.md-THEME_NAME-theme md-option .md-text{color:"{{background-900-0.87}}"}md-sidenav.md-THEME_NAME-theme,md-sidenav.md-THEME_NAME-theme md-content{background-color:"{{background-hue-1}}"}md-radio-button.md-THEME_NAME-theme .md-off{border-color:"{{foreground-2}}"}md-radio-button.md-THEME_NAME-theme .md-on{background-color:"{{accent-color-0.87}}"}md-radio-button.md-THEME_NAME-theme.md-checked .md-off{border-color:"{{accent-color-0.87}}"}md-radio-button.md-THEME_NAME-theme.md-checked .md-ink-ripple{color:"{{accent-color-0.87}}"}md-radio-button.md-THEME_NAME-theme .md-container .md-ripple{color:"{{accent-A700}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-primary .md-on,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-primary .md-on,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-primary .md-on,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-primary .md-on{background-color:"{{primary-color-0.87}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-off,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-primary.md-checked .md-off,md-radio-button.md-THEME_NAME-theme:not([disabled]).md-primary .md-checked .md-off,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-primary .md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-primary.md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-primary .md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-primary .md-checked .md-off{border-color:"{{primary-color-0.87}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-ink-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-primary.md-checked .md-ink-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]).md-primary .md-checked .md-ink-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-primary .md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-primary.md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-primary.md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-primary .md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-primary .md-checked .md-ink-ripple{color:"{{primary-color-0.87}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-primary .md-container .md-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-primary .md-container .md-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-primary .md-container .md-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-primary .md-container .md-ripple{color:"{{primary-600}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-warn .md-on,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-warn .md-on,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-warn .md-on,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-warn .md-on{background-color:"{{warn-color-0.87}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-off,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-warn.md-checked .md-off,md-radio-button.md-THEME_NAME-theme:not([disabled]).md-warn .md-checked .md-off,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-warn .md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-warn.md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-warn .md-checked .md-off,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-warn .md-checked .md-off{border-color:"{{warn-color-0.87}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-ink-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-warn.md-checked .md-ink-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]).md-warn .md-checked .md-ink-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-warn .md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-warn.md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-warn.md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-warn .md-checked .md-ink-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-warn .md-checked .md-ink-ripple{color:"{{warn-color-0.87}}"}md-radio-button.md-THEME_NAME-theme:not([disabled]).md-warn .md-container .md-ripple,md-radio-button.md-THEME_NAME-theme:not([disabled]) .md-warn .md-container .md-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]).md-warn .md-container .md-ripple,md-radio-group.md-THEME_NAME-theme:not([disabled]) .md-warn .md-container .md-ripple{color:"{{warn-600}}"}md-radio-button.md-THEME_NAME-theme[disabled],md-radio-group.md-THEME_NAME-theme[disabled]{color:"{{foreground-3}}"}md-radio-button.md-THEME_NAME-theme[disabled] .md-container .md-off,md-radio-button.md-THEME_NAME-theme[disabled] .md-container .md-on,md-radio-group.md-THEME_NAME-theme[disabled] .md-container .md-off,md-radio-group.md-THEME_NAME-theme[disabled] .md-container .md-on{border-color:"{{foreground-3}}"}md-radio-group.md-THEME_NAME-theme .md-checked .md-ink-ripple{color:"{{accent-color-0.26}}"}md-radio-group.md-THEME_NAME-theme .md-checked:not([disabled]).md-primary .md-ink-ripple,md-radio-group.md-THEME_NAME-theme.md-primary .md-checked:not([disabled]) .md-ink-ripple{color:"{{primary-color-0.26}}"}md-radio-group.md-THEME_NAME-theme .md-checked.md-primary .md-ink-ripple{color:"{{warn-color-0.26}}"}md-radio-group.md-THEME_NAME-theme.md-focused:not(:empty) .md-checked .md-container:before{background-color:"{{accent-color-0.26}}"}md-radio-group.md-THEME_NAME-theme.md-focused:not(:empty) .md-checked.md-primary .md-container:before,md-radio-group.md-THEME_NAME-theme.md-focused:not(:empty).md-primary .md-checked .md-container:before{background-color:"{{primary-color-0.26}}"}md-radio-group.md-THEME_NAME-theme.md-focused:not(:empty) .md-checked.md-warn .md-container:before,md-radio-group.md-THEME_NAME-theme.md-focused:not(:empty).md-warn .md-checked .md-container:before{background-color:"{{warn-color-0.26}}"}md-slider.md-THEME_NAME-theme .md-track{background-color:"{{foreground-3}}"}md-slider.md-THEME_NAME-theme .md-track-ticks{color:"{{background-contrast}}"}md-slider.md-THEME_NAME-theme .md-focus-ring{background-color:"{{accent-A200-0.2}}"}md-slider.md-THEME_NAME-theme .md-disabled-thumb{border-color:"{{background-color}}";background-color:"{{background-color}}"}md-slider.md-THEME_NAME-theme.md-min .md-thumb:after{background-color:"{{background-color}}";border-color:"{{foreground-3}}"}md-slider.md-THEME_NAME-theme.md-min .md-focus-ring{background-color:"{{foreground-3-0.38}}"}md-slider.md-THEME_NAME-theme.md-min[md-discrete] .md-thumb:after{background-color:"{{background-contrast}}";border-color:transparent}md-slider.md-THEME_NAME-theme.md-min[md-discrete] .md-sign{background-color:"{{background-400}}"}md-slider.md-THEME_NAME-theme.md-min[md-discrete] .md-sign:after{border-top-color:"{{background-400}}"}md-slider.md-THEME_NAME-theme.md-min[md-discrete][md-vertical] .md-sign:after{border-top-color:transparent;border-left-color:"{{background-400}}"}md-slider.md-THEME_NAME-theme .md-track.md-track-fill{background-color:"{{accent-color}}"}md-slider.md-THEME_NAME-theme .md-thumb:after{border-color:"{{accent-color}}";background-color:"{{accent-color}}"}md-slider.md-THEME_NAME-theme .md-sign{background-color:"{{accent-color}}"}md-slider.md-THEME_NAME-theme .md-sign:after{border-top-color:"{{accent-color}}"}md-slider.md-THEME_NAME-theme[md-vertical] .md-sign:after{border-top-color:transparent;border-left-color:"{{accent-color}}"}md-slider.md-THEME_NAME-theme .md-thumb-text{color:"{{accent-contrast}}"}md-slider.md-THEME_NAME-theme.md-warn .md-focus-ring{background-color:"{{warn-200-0.38}}"}md-slider.md-THEME_NAME-theme.md-warn .md-track.md-track-fill{background-color:"{{warn-color}}"}md-slider.md-THEME_NAME-theme.md-warn .md-thumb:after{border-color:"{{warn-color}}";background-color:"{{warn-color}}"}md-slider.md-THEME_NAME-theme.md-warn .md-sign{background-color:"{{warn-color}}"}md-slider.md-THEME_NAME-theme.md-warn .md-sign:after{border-top-color:"{{warn-color}}"}md-slider.md-THEME_NAME-theme.md-warn[md-vertical] .md-sign:after{border-top-color:transparent;border-left-color:"{{warn-color}}"}md-slider.md-THEME_NAME-theme.md-warn .md-thumb-text{color:"{{warn-contrast}}"}md-slider.md-THEME_NAME-theme.md-primary .md-focus-ring{background-color:"{{primary-200-0.38}}"}md-slider.md-THEME_NAME-theme.md-primary .md-track.md-track-fill{background-color:"{{primary-color}}"}md-slider.md-THEME_NAME-theme.md-primary .md-thumb:after{border-color:"{{primary-color}}";background-color:"{{primary-color}}"}md-slider.md-THEME_NAME-theme.md-primary .md-sign{background-color:"{{primary-color}}"}md-slider.md-THEME_NAME-theme.md-primary .md-sign:after{border-top-color:"{{primary-color}}"}md-slider.md-THEME_NAME-theme.md-primary[md-vertical] .md-sign:after{border-top-color:transparent;border-left-color:"{{primary-color}}"}md-slider.md-THEME_NAME-theme.md-primary .md-thumb-text{color:"{{primary-contrast}}"}md-slider.md-THEME_NAME-theme[disabled] .md-thumb:after{border-color:transparent}md-slider.md-THEME_NAME-theme[disabled]:not(.md-min) .md-thumb:after,md-slider.md-THEME_NAME-theme[disabled][md-discrete] .md-thumb:after{background-color:"{{foreground-3}}";border-color:transparent}md-slider.md-THEME_NAME-theme[disabled][readonly] .md-sign{background-color:"{{background-400}}"}md-slider.md-THEME_NAME-theme[disabled][readonly] .md-sign:after{border-top-color:"{{background-400}}"}md-slider.md-THEME_NAME-theme[disabled][readonly][md-vertical] .md-sign:after{border-top-color:transparent;border-left-color:"{{background-400}}"}md-slider.md-THEME_NAME-theme[disabled][readonly] .md-disabled-thumb{border-color:transparent;background-color:transparent}md-slider-container[disabled]>:first-child:not(md-slider),md-slider-container[disabled]>:last-child:not(md-slider){color:"{{foreground-3}}"}md-switch.md-THEME_NAME-theme .md-ink-ripple{color:"{{background-500}}"}md-switch.md-THEME_NAME-theme .md-thumb{background-color:"{{background-50}}"}md-switch.md-THEME_NAME-theme .md-bar{background-color:"{{background-500}}"}md-switch.md-THEME_NAME-theme.md-checked .md-ink-ripple{color:"{{accent-color}}"}md-switch.md-THEME_NAME-theme.md-checked .md-thumb{background-color:"{{accent-color}}"}md-switch.md-THEME_NAME-theme.md-checked .md-bar{background-color:"{{accent-color-0.5}}"}md-switch.md-THEME_NAME-theme.md-checked.md-focused .md-thumb:before{background-color:"{{accent-color-0.26}}"}md-switch.md-THEME_NAME-theme.md-checked.md-primary .md-ink-ripple{color:"{{primary-color}}"}md-switch.md-THEME_NAME-theme.md-checked.md-primary .md-thumb{background-color:"{{primary-color}}"}md-switch.md-THEME_NAME-theme.md-checked.md-primary .md-bar{background-color:"{{primary-color-0.5}}"}md-switch.md-THEME_NAME-theme.md-checked.md-primary.md-focused .md-thumb:before{background-color:"{{primary-color-0.26}}"}md-switch.md-THEME_NAME-theme.md-checked.md-warn .md-ink-ripple{color:"{{warn-color}}"}md-switch.md-THEME_NAME-theme.md-checked.md-warn .md-thumb{background-color:"{{warn-color}}"}md-switch.md-THEME_NAME-theme.md-checked.md-warn .md-bar{background-color:"{{warn-color-0.5}}"}md-switch.md-THEME_NAME-theme.md-checked.md-warn.md-focused .md-thumb:before{background-color:"{{warn-color-0.26}}"}md-switch.md-THEME_NAME-theme[disabled] .md-thumb{background-color:"{{background-400}}"}md-switch.md-THEME_NAME-theme[disabled] .md-bar{background-color:"{{foreground-4}}"}.md-subheader.md-THEME_NAME-theme{color:"{{ foreground-2-0.23 }}";background-color:"{{background-default}}"}.md-subheader.md-THEME_NAME-theme.md-primary{color:"{{primary-color}}"}.md-subheader.md-THEME_NAME-theme.md-accent{color:"{{accent-color}}"}.md-subheader.md-THEME_NAME-theme.md-warn{color:"{{warn-color}}"}md-tabs.md-THEME_NAME-theme md-tabs-wrapper{background-color:transparent;border-color:"{{foreground-4}}"}md-tabs.md-THEME_NAME-theme .md-paginator md-icon{color:"{{primary-color}}"}md-tabs.md-THEME_NAME-theme md-ink-bar{color:"{{accent-color}}";background:"{{accent-color}}"}md-tabs.md-THEME_NAME-theme .md-tab{color:"{{foreground-2}}"}md-tabs.md-THEME_NAME-theme .md-tab[disabled],md-tabs.md-THEME_NAME-theme .md-tab[disabled] md-icon{color:"{{foreground-3}}"}md-tabs.md-THEME_NAME-theme .md-tab.md-active,md-tabs.md-THEME_NAME-theme .md-tab.md-active md-icon,md-tabs.md-THEME_NAME-theme .md-tab.md-focused,md-tabs.md-THEME_NAME-theme .md-tab.md-focused md-icon{color:"{{primary-color}}"}md-tabs.md-THEME_NAME-theme .md-tab.md-focused{background:"{{primary-color-0.1}}"}md-tabs.md-THEME_NAME-theme .md-tab .md-ripple-container{color:"{{accent-A100}}"}md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper{background-color:"{{accent-color}}"}md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]),md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]) md-icon{color:"{{accent-A100}}"}md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active,md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active md-icon,md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused,md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused md-icon{color:"{{accent-contrast}}"}md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused{background:"{{accent-contrast-0.1}}"}md-tabs.md-THEME_NAME-theme.md-accent>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-ink-bar{color:"{{primary-600-1}}";background:"{{primary-600-1}}"}md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper{background-color:"{{primary-color}}"}md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]),md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]) md-icon{color:"{{primary-100}}"}md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active,md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active md-icon,md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused,md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused md-icon{color:"{{primary-contrast}}"}md-tabs.md-THEME_NAME-theme.md-primary>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused{background:"{{primary-contrast-0.1}}"}md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper{background-color:"{{warn-color}}"}md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]),md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]) md-icon{color:"{{warn-100}}"}md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active,md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active md-icon,md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused,md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused md-icon{color:"{{warn-contrast}}"}md-tabs.md-THEME_NAME-theme.md-warn>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused{background:"{{warn-contrast-0.1}}"}md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper{background-color:"{{primary-color}}"}md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]),md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]) md-icon{color:"{{primary-100}}"}md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active,md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active md-icon,md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused,md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused md-icon{color:"{{primary-contrast}}"}md-toolbar>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused{background:"{{primary-contrast-0.1}}"}md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper{background-color:"{{accent-color}}"}md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]),md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]) md-icon{color:"{{accent-A100}}"}md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active,md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active md-icon,md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused,md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused md-icon{color:"{{accent-contrast}}"}md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused{background:"{{accent-contrast-0.1}}"}md-toolbar.md-accent>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-ink-bar{color:"{{primary-600-1}}";background:"{{primary-600-1}}"}md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper{background-color:"{{warn-color}}"}md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]),md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]) md-icon{color:"{{warn-100}}"}md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active,md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-active md-icon,md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused,md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused md-icon{color:"{{warn-contrast}}"}md-toolbar.md-warn>md-tabs.md-THEME_NAME-theme>md-tabs-wrapper>md-tabs-canvas>md-pagination-wrapper>md-tab-item:not([disabled]).md-focused{background:"{{warn-contrast-0.1}}"}.md-panel.md-tooltip.md-THEME_NAME-theme{color:"{{background-700-contrast}}";background-color:"{{background-700}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar){background-color:"{{primary-color}}";color:"{{primary-contrast}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar) md-icon{color:"{{primary-contrast}}";fill:"{{primary-contrast}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar) .md-button[disabled] md-icon{color:"{{primary-contrast-0.26}}";fill:"{{primary-contrast-0.26}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar).md-accent{background-color:"{{accent-color}}";color:"{{accent-contrast}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar).md-accent .md-ink-ripple{color:"{{accent-contrast}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar).md-accent md-icon{color:"{{accent-contrast}}";fill:"{{accent-contrast}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar).md-accent .md-button[disabled] md-icon{color:"{{accent-contrast-0.26}}";fill:"{{accent-contrast-0.26}}"}md-toolbar.md-THEME_NAME-theme:not(.md-menu-toolbar).md-warn{background-color:"{{warn-color}}";color:"{{warn-contrast}}"}md-toast.md-THEME_NAME-theme .md-toast-content{background-color:#323232;color:"{{background-50}}"}md-toast.md-THEME_NAME-theme .md-toast-content .md-button{color:"{{background-50}}"}md-toast.md-THEME_NAME-theme .md-toast-content .md-button.md-highlight{color:"{{accent-color}}"}md-toast.md-THEME_NAME-theme .md-toast-content .md-button.md-highlight.md-primary{color:"{{primary-color}}"}md-toast.md-THEME_NAME-theme .md-toast-content .md-button.md-highlight.md-warn{color:"{{warn-color}}"}body.md-THEME_NAME-theme,html.md-THEME_NAME-theme{color:"{{foreground-1}}";background-color:"{{background-color}}"}'
        );
    }();
  }(window, window.angular), window.ngMaterial = {
    version: {
      full: "1.1.3"
    }
  };
}
