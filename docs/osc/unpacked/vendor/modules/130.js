// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 130
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  (function(e) {
    (function(e) {
      "Window" in this || ! function(e) {
          e.constructor ? e.Window = e.constructor : (e.Window = e.constructor = Function("return function(){}")())
            .prototype = this
        }(this), "Document" in this || (this.HTMLDocument ? this.Document = this.HTMLDocument : (this.Document =
          this.HTMLDocument = document.constructor = Function("return function(){}")(), this.Document.prototype =
          document)), "Element" in this && "HTMLElement" in this || ! function() {
          function t() {
            return d-- || clearTimeout(l), !(!document.body || document.body.prototype || !/(complete|interactive)/
              .test(document.readyState)) && (s(document, !0), l && document.body.prototype && clearTimeout(l), !!
              document.body.prototype)
          }
          var n, r, i, o, a, s, c, u, l, d;
          return window.Element && !window.HTMLElement ? (window.HTMLElement = window.Element, e) : (window
            .Element = window.HTMLElement = Function("return function(){}")(), n = document.appendChild(document
              .createElement("body")), r = n.appendChild(document.createElement("iframe")), i = r.contentWindow
            .document, o = Element.prototype = i.appendChild(i.createElement("*")), a = {}, s = function(e, t) {
              var n, r, i, o = e.childNodes || [],
                c = -1;
              if (1 === e.nodeType && e.constructor !== Element) {
                e.constructor = Element;
                for (n in a) r = a[n], e[n] = r
              }
              for (; i = t && o[++c];) s(i, t);
              return e
            }, c = document.getElementsByTagName("*"), u = document.createElement, d = 100, o.attachEvent(
              "onpropertychange",
              function(e) {
                for (var t, n = e.propertyName, r = !a.hasOwnProperty(n), i = o[n], s = a[n], u = -1; t = c[++u];)
                  1 === t.nodeType && (r || t[n] === s) && (t[n] = i);
                a[n] = i
              }), o.constructor = Element, o.hasAttribute || (o.hasAttribute = function(e) {
              return null !== this.getAttribute(e)
            }), t(!0) || (document.onreadystatechange = t, l = setInterval(t, 25)), document.createElement =
            function(e) {
              var t = u((e + "").toLowerCase());
              return s(t)
            }, document.removeChild(n), e)
        }(), "defineProperty" in Object && function() {
          try {
            var e = {};
            return Object.defineProperty(e, "test", {
              value: 42
            }), !0
          } catch (e) {
            return !1
          }
        }() || ! function(e) {
          var t = Object.prototype.hasOwnProperty("__defineGetter__"),
            n = "Getters & setters cannot be defined on this javascript engine",
            r = "A property cannot both have accessors and be writable or have a value";
          Object.defineProperty = function(i, o, a) {
            var s, c, u, l;
            if (e && (i === window || i === document || i === Element.prototype || i instanceof Element))
            return e(i, o, a);
            if (s = o + "", c = "value" in a || "writable" in a, u = "get" in a && typeof a.get, l = "set" in a &&
              typeof a.set, null === i || !(i instanceof Object || "object" == typeof i)) throw new TypeError(
              "Object must be an object (Object.defineProperty polyfill)");
            if (!(a instanceof Object)) throw new TypeError(
              "Descriptor must be an object (Object.defineProperty polyfill)");
            if (u) {
              if ("function" !== u) throw new TypeError(
                "Getter expected a function (Object.defineProperty polyfill)");
              if (!t) throw new TypeError(n);
              if (c) throw new TypeError(r);
              i.__defineGetter__(s, a.get)
            } else i[s] = a.value;
            if (l) {
              if ("function" !== l) throw new TypeError(
                "Setter expected a function (Object.defineProperty polyfill)");
              if (!t) throw new TypeError(n);
              if (c) throw new TypeError(r);
              i.__defineSetter__(s, a.set)
            }
            return "value" in a && (i[s] = a.value), i
          }
        }(Object.defineProperty),
        function(e) {
          if (!("Event" in e)) return !1;
          if ("function" == typeof e.Event) return !0;
          try {
            return new Event("click"), !0
          } catch (e) {
            return !1
          }
        }(this) || ! function() {
          function t(e, t) {
            for (var n = -1, r = e.length; ++n < r;)
              if (n in e && e[n] === t) return n;
            return -1
          }
          var n = {
              click: 1,
              dblclick: 1,
              keyup: 1,
              keypress: 1,
              keydown: 1,
              mousedown: 1,
              mouseup: 1,
              mousemove: 1,
              mouseover: 1,
              mouseenter: 1,
              mouseleave: 1,
              mouseout: 1,
              storage: 1,
              storagecommit: 1,
              textinput: 1
            },
            r = window.Event && window.Event.prototype || null;
          window.Event = Window.prototype.Event = function(t, n) {
            var r, i, o;
            if (!t) throw Error("Not enough arguments");
            return "createEvent" in document ? (r = document.createEvent("Event"), i = !(!n || n.bubbles === e) &&
              n.bubbles, o = !(!n || n.cancelable === e) && n.cancelable, r.initEvent(t, i, o), r) : (r =
              document.createEventObject(), r.type = t, r.bubbles = !(!n || n.bubbles === e) && n.bubbles, r
              .cancelable = !(!n || n.cancelable === e) && n.cancelable, r)
          }, r && Object.defineProperty(window.Event, "prototype", {
            configurable: !1,
            enumerable: !1,
            writable: !0,
            value: r
          }), "createEvent" in document || (window.addEventListener = Window.prototype.addEventListener = Document
            .prototype.addEventListener = Element.prototype.addEventListener = function() {
              var e = this,
                r = arguments[0],
                i = arguments[1];
              if (e === window && r in n) throw Error("In IE8 the event: " + r +
                " is not available on the window object. Please see https://github.com/Financial-Times/polyfill-service/issues/317 for more information."
                );
              e._events || (e._events = {}), e._events[r] || (e._events[r] = function(n) {
                  var r, i = e._events[n.type].list,
                    o = i.slice(),
                    a = -1,
                    s = o.length;
                  for (n.preventDefault = function() {
                      n.cancelable !== !1 && (n.returnValue = !1)
                    }, n.stopPropagation = function() {
                      n.cancelBubble = !0
                    }, n.stopImmediatePropagation = function() {
                      n.cancelBubble = !0, n.cancelImmediate = !0
                    }, n.currentTarget = e, n.relatedTarget = n.fromElement || null, n.target = n.target || n
                    .srcElement || e, n.timeStamp = (new Date).getTime(), n.clientX && (n.pageX = n.clientX +
                      document.documentElement.scrollLeft, n.pageY = n.clientY + document.documentElement
                      .scrollTop); ++a < s && !n.cancelImmediate;) a in o && (r = o[a], -1 !== t(i, r) &&
                    "function" == typeof r && r.call(e, n))
                }, e._events[r].list = [], e.attachEvent && e.attachEvent("on" + r, e._events[r])), e._events[r]
                .list.push(i)
            }, window.removeEventListener = Window.prototype.removeEventListener = Document.prototype
            .removeEventListener = Element.prototype.removeEventListener = function() {
              var e, n = this,
                r = arguments[0],
                i = arguments[1];
              n._events && n._events[r] && n._events[r].list && (e = t(n._events[r].list, i), -1 !== e && (n
                ._events[r].list.splice(e, 1), n._events[r].list.length || (n.detachEvent && n.detachEvent(
                  "on" + r, n._events[r]), delete n._events[r])))
            }, window.dispatchEvent = Window.prototype.dispatchEvent = Document.prototype.dispatchEvent = Element
            .prototype.dispatchEvent = function(e) {
              var t, n, r;
              if (!arguments.length) throw Error("Not enough arguments");
              if (!e || "string" != typeof e.type) throw Error("DOM Events Exception 0");
              t = this, n = e.type;
              try {
                e.bubbles || (e.cancelBubble = !0, r = function(e) {
                  e.cancelBubble = !0, (t || window).detachEvent("on" + n, r)
                }, this.attachEvent("on" + n, r)), this.fireEvent("on" + n, e)
              } catch (r) {
                e.target = t;
                do e.currentTarget = t, "_events" in t && "function" == typeof t._events[n] && t._events[n].call(
                    t, e), "function" == typeof t["on" + n] && t["on" + n].call(t, e), t = 9 === t.nodeType ? t
                  .parentWindow : t.parentNode; while (t && !e.cancelBubble)
              }
              return !0
            })
        }(), "CustomEvent" in this && ("function" == typeof this.CustomEvent || ("" + this.CustomEvent).indexOf(
          "CustomEventConstructor") > -1) || (this.CustomEvent = function(e, t) {
          if (!e) throw Error('TypeError: Failed to construct "CustomEvent": An event name must be provided.');
          var n;
          if (t = t || {
              bubbles: !1,
              cancelable: !1,
              detail: null
            }, "createEvent" in document) try {
            n = document.createEvent("CustomEvent"), n.initCustomEvent(e, t.bubbles, t.cancelable, t.detail)
          } catch (r) {
            n = document.createEvent("Event"), n.initEvent(e, t.bubbles, t.cancelable), n.detail = t.detail
          } else n = new Event(e, t), n.detail = t && t.detail || null;
          return n
        }, CustomEvent.prototype = Event.prototype)
    }).call("object" == typeof window && window || "object" == typeof self && self || "object" == typeof e &&
      e || {}), ! function(e) {
        ! function() {
          function t(e) {
            var t, n = i.exec(e)[1];
            return r.test(n) ? "" : (t = o.exec(n), null === t || 0 === t.length ? "" : (n = t[0], 0 === n.indexOf(
              a) ? n.substr(4) : n))
          }

          function n(t) {
            var n;
            s[t] !== n && (e[t] = s[t])
          }
          var r = /[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}/,
            i = /([^:]*)(:[0-9]{0,5})?/,
            o = /([^\.]+\.[^\.]{3}|[^\.]+\.[^\.]+\.[^\.]{2})$/,
            a = "www.",
            s = window.targetGlobalSettings || {};
          e.cookieDomain = t(window.location.hostname), n("clientCode"), n("serverDomain"), n("cookieDomain"), n(
            "crossDomain"), n("timeout"), n("globalMboxAutoCreate"), n("visitorApiTimeout"), n("enabled"), n(
            "defaultContentHiddenStyle"), n("defaultContentVisibleStyle"), n("bodyHidingEnabled"), n(
            "bodyHiddenStyle"), n("imsOrgId"), n("overrideMboxEdgeServer"), n("optoutEnabled")
        }(), e.enabled !== !1 && ! function() {
          function t(e) {
            e.fn.isHead = function() {
              return e(this).is("head")
            }, e.fn.isBody = function() {
              return e(this).is("body")
            }, e.fn.isHeadOrBody = function() {
              var t = e(this);
              return t.isHead() || t.isBody()
            }
          }

          function n(t) {
            t.fn.showElement = function() {
              var n = t(this),
                r = t.trim(n.attr("style")),
                i = [e.defaultContentVisibleStyle];
              return r && (";" !== r[r.length - 1] && (r += ";"), i.unshift(t.trim(r))), n.addClass(Ns), n.attr(
                "style", i.join(" "))
            }, t.fn.exists = function() {
              return t(this).length > 0
            }, t.isElement = function(e) {
              return !!e && 1 === e.nodeType && "object" === t.type(e) && !t.isPlainObject(e)
            }, t.sequential = function(e) {
              var n = t.Deferred().resolve([]);
              return t.each(e, function(e, t) {
                n = n.then(t)
              }), n
            }
          }

          function r(e) {
            try {
              return encodeURIComponent(e)
            } catch (t) {
              return e
            }
          }

          function i(e) {
            try {
              return decodeURIComponent(e)
            } catch (t) {
              return e
            }
          }

          function o(e) {
            for (var t = ui.exec(e), n = {}, r = 14; r--;) n[di[r]] = t[r] || "";
            return n.queryParams = {}, n.query.replace(li, function(e, t, r) {
              var o = i(t),
                a = i(r);
              k(S(o)) && M(a) && (n.queryParams[S(o)] = S(a))
            }), n
          }

          function a(e) {
            var t = o(e);
            return t.queryParams
          }

          function s(e, t) {
            var n = a(e);
            return D(n[t]) ? null : n[t]
          }

          function c(e) {
            var t, n, i, a, s, c = arguments.length <= 1 || void 0 === arguments[1] ? {} : arguments[1];
            return U(c) ? e : (t = {}, n = [], i = o(e), a = e.split("?")[0], C(i.queryParams, function(e, n) {
              return t[n] = e
            }), E(t, c), C(t, function(e, t) {
              return n.push(r(t) + "=" + r(e))
            }), s = n.join("&"), k(i.anchor) && (s = s + "#" + i.anchor), a + "?" + s)
          }

          function u(e) {
            var t = {},
              n = o("?" + e);
            return C(n.queryParams, function(e, n) {
              return t[n] = e
            }), t
          }

          function l(e) {
            return e.split("=")
          }

          function d(e) {
            return !B(e) && 2 === e.length && k(S(e[0]))
          }

          function f(e) {
            var t = {},
              n = x(e, function(e) {
                return k(e)
              }),
              r = T(n, function(e) {
                return [l(e)]
              }),
              o = x(r, function(e) {
                return d(e)
              });
            return C(o, function(e) {
              return t[i(S(e[0]))] = i(S(e[1]))
            }), t
          }

          function h(e, t, n) {
            C(e, function(e, r) {
              F(e) ? (t.push(r), h(e, t, n), t.pop()) : B(t) ? n[r] = e : n[t.concat(r).join(".")] = e
            })
          }

          function p(e) {
            var t, n;
            if (!P(e)) return {};
            t = null, n = {};
            try {
              t = e()
            } catch (e) {}
            return L(t) ? {} : R(t) ? f(t) : k(t) ? u(t) : F(t) ? (h(t, [], n), n) : {}
          }

          function m(e) {
            return Is(e).showElement()
          }

          function v(e) {
            return Is(e).html()
          }

          function g(e) {
            return Is(e).isHead()
          }

          function y(e) {
            return Is(e).isBody()
          }

          function b(e) {
            return Is(e).isHeadOrBody()
          }

          function E(e, t) {
            return C(t, function(t, n) {
              return e[n] = t
            }), e
          }

          function _() {
            var e, t, n = [],
              r = "0123456789abcdef";
            for (e = 0; 36 > e; e++) n[e] = r.substr(Math.floor(16 * Math.random()), 1);
            return n[14] = "4", n[19] = r.substr(3 & n[19] | 8, 1), n[8] = n[13] = n[18] = n[23] = "-", t = n.join(
              ""), t.replace(/-/g, "")
          }

          function $(e) {
            var t, n, r = 5381;
            for (t = 0; t < e.length; t++) n = e.charCodeAt(t), r = (r << 5) + r + n;
            return r
          }

          function w(e, t, n, i, a) {
            var s = o(e),
              c = {},
              u = [];
            return C(s.queryParams, function(e, t) {
              return u.push(r(t) + "=" + r(e))
            }), C(t, function(e, t) {
              return k(i[t]) ? void(c[t] = i[t]) : void(k(e) && (c[t] = e))
            }), C(n, function(e, n) {
              return k(a[n]) ? void(c[n] = a[n]) : void((N(i[n]) || D(t[n])) && k(e) && (c[n] = e))
            }), C(c, function(e, t) {
              return u.push(r(t) + "=" + r(e))
            }), s.path + "?" + u.join("&")
          }

          function T(e, t) {
            return Is.map(e, t)
          }

          function C(e, t) {
            Is.each(e, function(e, n) {
              t(n, e)
            })
          }

          function x(e, t) {
            return Is.grep(e, t)
          }

          function S(e) {
            return Is.trim(e)
          }

          function A(e, t) {
            return e.length > t
          }

          function M(e) {
            return "string" === Is.type(e)
          }

          function k(e) {
            return M(e) && Is.trim(e).length > 0
          }

          function N(e) {
            return !M(e) || 0 === e.length
          }

          function I(e) {
            return "boolean" === Is.type(e)
          }

          function O(e) {
            return "number" === Is.type(e)
          }

          function D(e) {
            return "undefined" === Is.type(e)
          }

          function R(e) {
            return Is.isArray(e)
          }

          function P(e) {
            return Is.isFunction(e)
          }

          function L(e) {
            return "null" === Is.type(e)
          }

          function U(e) {
            return Is.isEmptyObject(e)
          }

          function F(e) {
            return "object" === Is.type(e)
          }

          function j(e) {
            return Is.isElement(e)
          }

          function H(e) {
            return !j(e)
          }

          function B(e) {
            return !(R(e) && e.length > 0)
          }

          function z(e, t) {
            var n = e.is("div." + Zr);
            return D(t) ? n : n && e.hasClass(Jr + t)
          }

          function q(e, t) {
            de(e).append(t)
          }

          function G(e, t) {
            de(e).prepend(t)
          }

          function V(e, t) {
            de(e).after(t)
          }

          function W(e, t) {
            de(e).before(t)
          }

          function Y(e) {
            de(e).remove()
          }

          function K(e, t) {
            Is(e).before(t)
          }

          function X(e) {
            var t, n = ["protocol", "host"];
            return !(!k(e) || (t = function() {
              var t = o(e),
                r = x(n, function(e) {
                  return N(t[e])
                });
              return {
                v: B(r)
              }
            }(), "object" != typeof t)) && t.v
          }

          function Q(e) {
            var t = Is("." + e);
            return t.last()
          }

          function J(e) {
            var t = Q(Jr + e);
            return t.exists() ? t : Q(Zr)
          }

          function Z(e, t) {
            return Is(e).text(t)
          }

          function ee(e, t) {
            return Is(e).html(t)
          }

          function te(e) {
            return Is("<div></div>").append(e)
          }

          function ne(e) {
            return k(ge(e, "src"))
          }

          function re(e) {
            return '<script type="text/atjs-marker-script" class="' + hi + "-" + e + '"></script>'
          }

          function ie(e) {
            var t = te(e),
              n = -1;
            return C(t.find(Ei), function(e) {
              var t = ne(e);
              t || (n += 1, K(e, re(n))), de(e).remove()
            }), t.html()
          }

          function oe(e) {
            var t = te(e),
              n = t.find("img");
            return C(n, function(e) {
              return ae(e, "src", fi)
            }), t.html()
          }

          function ae(e, t, n) {
            var r = ge(e, t);
            be(e, n, r), ye(e, t)
          }

          function se(e) {
            var t = de(e),
              n = t.find("img");
            C(n, function(e) {
              var t = ge(e, fi);
              be(e, "src", t), ye(e, fi)
            })
          }

          function ce(e) {
            var t = ie(e);
            return oe(t)
          }

          function ue() {
            e.bodyHidingEnabled === !0 && Is("#" + pi).remove()
          }

          function le(e) {
            return Is(e).exists()
          }

          function de(e) {
            return Is(e)
          }

          function fe(e) {
            return de(e).parent()
          }

          function he(e, t) {
            var n = {},
              r = x(t, function(t) {
                return !D(e[t])
              });
            return C(r, function(t) {
              return n[t] = e[t]
            }), n
          }

          function pe() {
            return Is.Deferred()
          }

          function me(e) {
            var t = pe();
            return t.resolve(e), t.promise()
          }

          function ve(e) {
            var t = pe();
            return t.reject(e), t.promise()
          }

          function ge(e, t) {
            return Is(e).attr(t)
          }

          function ye(e, t) {
            Is(e).removeAttr(t)
          }

          function be(e, t, n) {
            Is(e).attr(t, n)
          }

          function Ee() {
            return Is.isReady
          }

          function _e(e) {
            var t = {};
            return x(e, function(e) {
              return !!D(t[e]) && (t[e] = !0, !0)
            })
          }

          function $e(e) {
            return Is.sequential(e)
          }

          function we(e) {
            return Is.when.apply(null, e)
          }

          function Te(e) {
            var t = {},
              n = e.getSessionId(),
              r = e.getDeviceId();
            return k(n) && (t.sessionId = n), k(r) && (t.deviceId = r), t
          }

          function Ce() {
            var t = vi,
              n = "." + Zr + " {" + e.defaultContentHiddenStyle + "}";
            de("head").append('<style id="' + t + '">' + n + "</style>")
          }

          function xe() {
            Is("#" + vi).remove()
          }

          function Se(t, n) {
            function c(e, t) {
              Is(A).trigger(e, t)
            }

            function u() {
              c(bi)
            }

            function l() {
              c(yi)
            }

            function d() {
              c(gi)
            }

            function h(e) {
              W(Is("script").eq(0), e)
            }

            function m(e, t) {
              return '<style id="' + e + '" class="' + mi + '">' + t + "</style>"
            }

            function v(t) {
              return B(t) ? void l() : (C(t, function(t) {
                var n = t + " {" + e.defaultContentHiddenStyle + "}";
                h(m("at-" + $(t), n))
              }), void l())
            }

            function g() {
              de("." + mi).remove()
            }

            function y() {
              e.bodyHidingEnabled === !0 && (h(m(pi, e.bodyHiddenStyle)), Is(A).one(yi, ue))
            }

            function b(e) {
              Ee() ? e() : T(bi, e)
            }

            function T(e, t) {
              Is(A).one(e, t)
            }

            function x(e, t) {
              Is(A).off(e, t)
            }

            function S(e) {
              var t = Is.ajax(e);
              return T(gi, function() {
                return t.abort()
              }), t
            }
            var A = t.document;
            return {
              hideBody: y,
              hideElements: v,
              showElements: g,
              triggerRedirectEvent: d,
              triggerShowBody: l,
              parseUri: o,
              getAjax: S,
              getPageParameters: a,
              getPageParameter: s,
              generateId: _,
              buildDynamicContentUrl: w,
              getParametersFromArray: f,
              mergeParameters: E,
              encode: r,
              decode: i,
              getTargetPageParameters: function(e) {
                var r = n.globalMboxName;
                return r !== e ? {} : p(t.targetPageParams)
              },
              getTargetPageParametersAll: function() {
                return p(t.targetPageParamsAll)
              },
              delayCallback: function(e) {
                for (var n = arguments.length, r = Array(n > 1 ? n - 1 : 0), i = 1; n > i; i++) r[i - 1] =
                  arguments[i];
                t.setTimeout(function() {
                  return e.apply(e, r)
                }, 0)
              },
              findLastMboxNode: J,
              isNull: L,
              isMboxDiv: z,
              onDomReady: b,
              redirect: function(e) {
                t.location.replace(e)
              },
              subscribeOnce: T,
              trigger: c,
              triggerDomReady: u,
              unsubscribe: x
            }
          }

          function Ae(e, t) {
            return D(e.documentElement) ? e.body[t] : e.documentElement[t]
          }

          function Me(e, t, n, r) {
            return D(e[n]) ? Ae(t, r) : e[n]
          }

          function ke(e, t) {
            return function() {
              var n = {};
              return n.width = Me(e, t, "innerWidth", "clientWidth"), n.height = Me(e, t, "innerHeight",
                  "clientHeight"), n.screenWidth = e.screen.width, n.screenHeight = e.screen.height, n
                .colorDepth = e.screen.colorDepth, n.timeOffset = -(new Date).getTimezoneOffset(), n
            }
          }

          function Ne(e, t, n, r) {
            return function() {
              var i = r.crossDomain !== ai,
                o = e(),
                a = {};
              return a[Ai] = o.screenHeight, a[Mi] = o.screenWidth, a[Si] = o.colorDepth, a[Ii] = o.width, a[ki] =
                o.height, a[Ni] = o.timeOffset, a[ji] = n, a[Vi] = r.version, a[Ui] = t.location.hostname, a[Gi] =
                t.location.href, a[Bi] = t.referrer, i && (a[Pi] = r.crossDomain), a
            }
          }

          function Ie(e, t) {
            return O(e) ? e : t
          }

          function Oe(e, t) {
            var n = {
              status: e
            };
            return k(t) && (n.message = t), n
          }

          function De(e) {
            return Oe(Qi, e)
          }

          function Re(e) {
            return Oe(Ji, e)
          }

          function Pe(e) {
            return Oe(Ki, e)
          }

          function Le(e) {
            return P(e) ? e : Wi
          }

          function Ue(e) {
            return F(e.console) && P(e.console.log) && P(e.console.error)
          }

          function Fe(e, t) {
            var n = e.location.search;
            return k(t.getPageParameter(n, to))
          }

          function je(e, t, n) {
            var r = {
              log: Wi,
              error: Wi
            };
            return Ue(e) && (r.error = function() {
              e.console.error.apply(e.console, [].concat.apply([io], arguments))
            }, Fe(t, n) && (r.log = function() {
              e.console.log.apply(e.console, [].concat.apply([io], arguments))
            })), r
          }

          function He(e, t, n, i) {
            t = r(t + ""), n = r(n + ""), Be(e, t, n, i)
          }

          function Be(e, t, n, r) {
            if (r.path = r.path || "/", O(r.expires)) {
              var i = new Date;
              i = new Date(i.getTime() + r.expires), r.expires = i
            }
            e.cookie = t + "=" + n + (r.expires ? "; expires=" + r.expires.toUTCString() : "") + (r.path ?
              "; path=" + r.path : "") + (r.domain ? "; domain=" + r.domain : "")
          }

          function ze(e, t) {
            var n = RegExp("(^|; )" + t + "=([^;]*)").exec(e.cookie);
            return L(n) || 3 !== n.length ? null : i(n[2])
          }

          function qe(e, t, n) {
            return {
              name: e,
              value: t,
              expires: n
            }
          }

          function Ge(e) {
            return r(e.name) + "#" + r(e.value) + "#" + e.expires
          }

          function Ve(e) {
            var t = e.split("#");
            return B(t) || t.length < 3 ? null : isNaN(parseInt(t[2], 10)) ? null : qe(i(t[0]), i(t[1]), +t[2])
          }

          function We(e) {
            return N(e) ? [] : e.split("|")
          }

          function Ye(e) {
            return e.expires
          }

          function Ke(e) {
            return Math.max.apply(null, T(e, Ye))
          }

          function Xe(t) {
            function n(e) {
              var n = ze(t, r(e)),
                i = T(We(n), function(e) {
                  return Ve(e)
                }),
                o = new Date,
                a = Math.ceil(o.getTime() / 1e3),
                s = {},
                c = x(i, function(e) {
                  return F(e) && a <= e.expires
                });
              return C(c, function(e) {
                return s[e.name] = e
              }), s
            }

            function i(e, n) {
              var r = new Date,
                i = T(n, function(e) {
                  return e
                }),
                o = Math.abs(1e3 * Ke(i) - r.getTime()),
                a = T(i, function(e) {
                  return Ge(e)
                });
              Be(t, ao, a.join("|"), {
                domain: e,
                expires: o
              })
            }
            var o = e.cookieDomain,
              a = e.crossDomain === oi,
              s = {
                isEnabled: function() {
                  He(t, oo, !0, {
                    domain: o
                  });
                  var e = !L(ze(t, oo));
                  return He(t, oo, "", {
                    domain: o,
                    expires: -36e5
                  }), e
                },
                setCookie: function(e, t, r) {
                  var s, c, u;
                  a || N(e) || L(t) || D(t) || O(r) && (s = n(ao), c = new Date, u = Math.ceil(r + c.getTime() /
                    1e3), s[e] = qe(e, t, u), i(o, s))
                },
                getCookie: function(e) {
                  var t, r;
                  return a ? null : (t = n(ao), r = t[e], F(r) ? r : null)
                }
              };
            return s
          }

          function Qe(e) {
            var t = e.getCookie(so);
            return F(t) && k(t.value) ? t.value : ""
          }

          function Je(e, t, n) {
            e.setCookie(so, t, n)
          }

          function Ze(e, t) {
            var n = t.deviceIdLifetime / 1e3,
              r = Qe(e);
            return k(r) && Je(e, r, n), {
              getId: function() {
                return r
              },
              setId: function(t) {
                r = t, Je(e, t, n)
              }
            }
          }

          function et(e) {
            var t = e.getCookie(co);
            return F(t) && k(t.value) ? t.value : ""
          }

          function tt(e, t) {
            return t.getPageParameter(e.location.search, Hi)
          }

          function nt(e, t, n) {
            e.setCookie(co, t, n)
          }

          function rt(e, t, n, r) {
            var i = r.sessionIdLifetime / 1e3,
              o = tt(e, n);
            return o = o || et(t) || n.generateId(), nt(t, o, i), {
              getId: function() {
                return o
              },
              setId: function(e) {
                return o = e, nt(t, o, i)
              }
            }
          }

          function it(e) {
            return e.replace(/"/g, "&quot;").replace(/>/g, "&gt;")
          }

          function ot(e) {
            return ei.replace("{clientCode}", e)
          }

          function at(e, t) {
            return ho + e + ot(t) + "?"
          }

          function st(e) {
            if (fo.exec(e)) throw Error('Parameter "' + e + '" contains invalid characters.')
          }

          function ct() {
            var e = new Date;
            return e.getTime() - e.getTimezoneOffset() * Qr
          }

          function ut(e, t, n) {
            var r = k(e) && (I(t) || O(t) || M(t));
            r && (e = S(e + ""), t = S(t + ""), st(e), n[e] = t)
          }

          function lt(e, t) {
            var n, r = t.serverDomain;
            return t.overrideMboxEdgeServer ? (n = ze(e, uo), N(n) ? r : n) : r
          }

          function dt(e, t, n, r, i, o) {
            function a(e, t) {
              C(e, function(e, n) {
                return ut(n, e, t)
              })
            }

            function s(s) {
              var u, l = lt(e, o),
                d = at(l, o.clientCode),
                f = {},
                h = t();
              return a(h, c), a(i.getTargetPageParametersAll(), c), ut(Hi, n.getId(), c), ut(qi, r.getId(), c), ut(
                zi, ct(), c), C(c, function(e, t) {
                return f[t] = e
              }), F(s) && C(s, function(e, t) {
                return ut(t, e, f)
              }), u = T(f, function(e, t) {
                return i.encode(t) + "=" + i.encode(e)
              }), it(d + u.join("&"))
            }
            var c = {},
              u = {
                buildUrl: s
              };
            return u
          }

          function ft(e) {
            var t = {};
            return C(e.params, function(e) {
              D(t[e.type]) && (t[e.type] = {}), t[e.type][e.name] = e.defaultValue
            }), t
          }

          function ht(e) {
            var t = [];
            return D(e.request) || (t = e.request), t
          }

          function pt(e) {
            return -1 !== e.indexOf("mbox")
          }

          function mt(e) {
            var t = e.mbox,
              n = {};
            return D(t) ? n : (C(t, function(e, t) {
              pt(t) || (n[t] = e)
            }), n)
          }

          function vt(e, t, n) {
            function r(r) {
              var i = r.offer.content.url,
                o = ft(r.offer.content),
                a = ht(o),
                s = mt(o),
                c = e.location.search,
                u = t.getPageParameters(c),
                l = r.params,
                d = t.buildDynamicContentUrl(i, a, s, u, l);
              return t.getAjax({
                url: d,
                timeout: n.timeout
              })
            }
            return r
          }

          function gt(e, t, n, r, i, o, a) {
            function s(e) {
              return "CustomEvent" in e && ("function" == typeof e.CustomEvent || ("" + e.CustomEvent).indexOf(
                "CustomEventConstructor") > -1)
            }

            function c() {
              var i = L(r.getPageParameter(t.location.search, no));
              return f || (i = i && n.isEnabled()), i && L(ze(t, lo)) && s(e)
            }

            function u(e) {
              return !L(r.getPageParameter(e, ro))
            }

            function l() {
              return u(t.location.search) || u(t.referrer)
            }

            function d(e) {
              return "XMLHttpRequest" in e && "withCredentials" in new e.XMLHttpRequest
            }
            var f = i.crossDomain === oi,
              h = 0;
            return {
              isEnabled: function() {
                return c()
              },
              isMboxEdit: function() {
                return l()
              },
              isCorsSupported: function() {
                return d(e)
              },
              getSessionId: function() {
                return o.getId()
              },
              getDeviceId: function() {
                return a.getId()
              },
              requests: {
                incrementAndGet: function() {
                  return ++h
                }
              }
            }
          }

          function yt() {
            function e(e) {
              return n[e]
            }

            function t(e) {
              return R(n[e])
            }
            var n = {};
            return {
              add: function(e, r) {
                return t(e) ? void n[e].push(r) : void(n[e] = [r])
              },
              getKeys: function() {
                return T(n, function(e, t) {
                  return t
                })
              },
              findAll: function() {
                var e = {};
                return C(n, function(t, n) {
                  return e[n] = t
                }), e
              },
              remove: function(e, t) {
                if (P(t)) {
                  var r = x(this.findByKey(e), function(e) {
                    return !t(e)
                  });
                  n[e] = r
                } else delete n[e]
              },
              clear: function() {
                n = {}
              },
              findByKey: function(t) {
                var n = e(t);
                return R(n) ? n : []
              }
            }
          }

          function bt(e, t, n) {
            function r(t) {
              var n = void 0,
                r = pe();
              return D(e.Visitor) ? (r.reject(), r.promise()) : L(e.Visitor) ? (r.reject(), r.promise()) : P(e
                .Visitor.getInstance) ? (n = e.Visitor.getInstance(g), F(n) && P(n.isAllowed) && n.isAllowed() ? (
                t.visitor = n, r.resolve(t)) : r.reject(), r.promise()) : (r.reject(), r.promise())
            }

            function i(e, t, n) {
              var r = pe();
              return P(e[t]) ? (e[t](function(e) {
                r.resolve({
                  key: n,
                  value: e
                })
              }, !0), r.promise()) : (r.resolve(void 0), r.promise())
            }

            function o(e) {
              var t = [i(e, "getMarketingCloudVisitorID", Ci), i(e, "getAudienceManagerBlob", wi), i(e,
                "getAnalyticsVisitorID", $i), i(e, "getAudienceManagerLocationHint", Ti)];
              return we(t)
            }

            function a(e) {
              var t = {},
                n = x(e, function(e) {
                  return !D(e)
                });
              return C(n, function(e) {
                return t[e.key] = e.value
              }), t
            }

            function s(e) {
              return mo + e
            }

            function c(e, t, n) {
              C(e, function(e, r) {
                F(e) ? (t.push(r), c(e, t, n), t.pop()) : B(t) ? n[s(r)] = e : n[s(t.concat(r).join("."))] = e
              })
            }

            function u(e) {
              var t, n;
              return P(e.getCustomerIDs) ? (t = e.getCustomerIDs(), F(t) ? (n = {}, c(t, [], n), n) : {}) : {}
            }

            function l(e) {
              var t = {};
              return k(e.trackingServer) && (t[vo] = e.trackingServer), k(e.trackingServerSecure) && (t[go] = e
                .trackingServerSecure), t
            }

            function d(t) {
              return _ && P(t.isOptedOut) && !D(e.Visitor.OptOut)
            }

            function f(n) {
              var r, i = n.visitor,
                o = pe();
              return d(i) ? (t.log(po, "preparing opt-out request"), r = e.setTimeout(function() {
                return o.reject()
              }, b), i.isOptedOut(function(n) {
                e.clearTimeout(r), t.log(po, "opt-out value", n), o.resolve(n)
              }, e.Visitor.OptOut.GLOBAL, !0), o.promise()) : (o.resolve(!1), o.promise())
            }

            function h(n) {
              var r, i = n.visitor,
                s = n.mboxName,
                c = pe();
              return t.log(po, "requests fired"), r = e.setTimeout(function() {
                return c.reject()
              }, b), o(i).done(function() {
                var e, n, r, o;
                for (e = arguments.length, n = Array(e), r = 0; e > r; r++) n[r] = arguments[r];
                o = a(n), E(o, u(i)), E(o, l(i)), v(o, i, s), t.log(po, "success", o), c.resolve(o)
              }).fail(function() {
                return c.reject()
              }).always(function() {
                return e.clearTimeout(r)
              }), c.promise()
            }

            function p(e) {
              var t = {
                mboxName: e
              };
              return r(t).then(h, function() {
                return null
              })
            }

            function m() {
              var e = {},
                t = pe();
              return r(e).then(f).done(function(e) {
                return t.resolve(e)
              }).fail(function() {
                return t.resolve(!1)
              }), t.promise()
            }

            function v(e, t, n) {
              P(t.getSupplementalDataID) && (e[xi] = t.getSupplementalDataID("mbox:" + y + ":" + n))
            }
            var g = n.imsOrgId,
              y = n.clientCode,
              b = n.visitorApiTimeout,
              _ = n.optoutEnabled,
              $ = {
                getParameters: p,
                getOptOut: m
              };
            return $
          }

          function Et(e, t, n) {
            var r = {
              name: e,
              valid: function(n) {
                return t(n[e])
              }
            };
            return k(n) && (r.message = n), r
          }

          function _t(e, t) {
            return {
              message: t,
              valid: function(t) {
                return e(t)
              }
            }
          }

          function $t(e) {
            return 'missing mandatory parameter: "' + e + '"'
          }

          function wt(e, t) {
            var n, r, i = "";
            for (n = 0; n < t.length; n += 1)
              if (r = t[n], !r.valid(e)) {
                i = r.message;
                break
              } return N(i) ? Pe() : De(i)
          }

          function Tt(e) {
            return k(e) && !A(e, ti)
          }

          function Ct(e, t, n) {
            function r(e) {
              var t = Pe();
              return C(Ho, function(n) {
                var r = n.valid(e);
                r.status === Qi && t.status === Ki && (t = r)
              }), t
            }
            return function(i) {
              var o, a, s = {},
                c = pe(),
                u = wt(i, Oo);
              return u.status === Qi ? (t.error(Eo, u.message), c.reject(), c.promise()) : (i.type = i.type || Do
                .JSON, u = r(i), u.status === Qi ? (t.error(u.message), c.reject(), c.promise()) : (o = i.type
                  .toLowerCase(), a = Ie(i.timeout, n.timeout), o === Do.JSON && (s.xhrFields = {
                    withCredentials: !0
                  }), o === Do.JSONP && k(i.jsonp) && (s.jsonp = i.jsonp), k(i.method) && (s.method = i.method),
                  F(i.params) && (s.data = i.params), s.timeout = a, s.dataType = o, s.url = i.url, t.log(Eo,
                    "params:", i), e.getAjax(s).then(function(e, n) {
                    return t.log(Eo, n, i.url), e
                  }, function(e, n, r) {
                    return k(r) ? (t.error(Eo, n + ":", r), Oe(n, r)) : 0 === e.status ? (t.log(Eo, Xi + ":",
                      "cancelled"), Oe(Xi, "cancelled")) : {
                      jqXHR: e,
                      textStatus: n
                    }
                  })))
            }
          }

          function xt(e, t, n) {
            function r(r) {
              var i = {
                url: t.buildUrl(r.params),
                timeout: r.timeout
              };
              return e.isCorsSupported() || (i.type = Do.JSONP, i.jsonp = Oi), n(i)
            }
            return {
              fetch: r
            }
          }

          function St(e, t) {
            function n(t, n) {
              return C(t, function(e, t) {
                return n.params[t] = e
              }), e.fetch(n)
            }

            function r(t) {
              return e.fetch(t)
            }

            function i(e, i) {
              var o, a;
              return e ? ve(Re(Bo)) : (o = i.params[Fi], a = t.getParameters(o), a.then(function(e) {
                return n(e, i)
              }, function() {
                return r(i)
              }))
            }

            function o(e) {
              return t.getOptOut().then(function(t) {
                return i(t, e)
              })
            }
            return {
              fetch: o
            }
          }

          function At(e, t, n, r) {
            return {
              fetch: function(i) {
                var o = Ie(i.timeout, r.timeout),
                  a = Le(i.error),
                  s = F(i.params) ? i.params : {};
                return s[ji] = t.generateId(), n.log(zo, "request params:", s), e.fetch({
                  params: s,
                  timeout: o
                }).then(function(e) {
                  return n.log(zo, Ki + ":", e), e
                }, a)
              }
            }
          }

          function Mt(e) {
            return {
              eventType: "click",
              tagName: "a",
              valid: function(t) {
                return !j(t) || N(t.href) ? De(qo) : F(e) && F(e.location) ? Pe() : De(Go)
              },
              getAction: function(t) {
                return function() {
                  e.location.href = t.href
                }
              }
            }
          }

          function kt() {
            return {
              eventType: "submit",
              tagName: "form",
              valid: function() {
                return Pe()
              },
              getAction: function(e) {
                return function(t) {
                  e.submit()
                }
              }
            }
          }

          function Nt(e, t) {
            var n, r, i = k(e) && k(t);
            return i ? (n = Qo[e], D(n) ? De(Ko.replace("{0}", e)) : (r = x(n, function(e) {
              return e === t
            }), B(r) ? De(Wo.replace("{0}", t).replace("{1}", e)) : Pe())) : De(Yo)
          }

          function It(e) {
            Qo[e.tagName] = [e.eventType], Xo[e.tagName] = e
          }

          function Ot(e) {
            return Xo[e]
          }

          function Dt(e, t) {
            return It(kt()), It(Mt(e)), {
              build: function(e, n) {
                var r, i, o, a;
                return H(e) ? (t.log(Vo, Xi + ": no element."), Wi) : (r = e.tagName.toLowerCase(), i = Nt(r,
                  n), i.status === Qi ? (t.log(Vo, Xi + ": " + i.message), Wi) : (o = Ot(r), a = o.valid(e), a
                    .status === Qi ? (t.log(Vo, Xi + ": " + a.message), Wi) : o.getAction(e)))
              }
            }
          }

          function Rt(t, n) {
            function r(e, t) {
              n.error(Jo, e + ":", t)
            }

            function i(e) {
              var t = te(e),
                n = T(t.find(ea), function(e) {
                  return e
                });
              return x(n, function(e) {
                return k(ge(e, "src"))
              })
            }

            function o(e) {
              var t = te(e);
              return T(t.find(Zo), function(e) {
                return e
              })
            }

            function a(e, t, n) {
              return function() {
                var r = pe(),
                  i = de(t).find(n);
                return K(i, e), Y(i), r.resolve(), r.promise()
              }
            }

            function s(i) {
              return function() {
                var o = pe(),
                  a = {
                    dataType: "script",
                    timeout: e.timeout,
                    url: i
                  };
                return n.log(Jo, "start:", i), t.getAjax(a).done(function() {
                  n.log(Jo, "end:", i), o.resolve()
                }).fail(function(e, t, n) {
                  r(t, n), o.reject(Oe(t, "Failed fetching " + i + "."))
                }), o.promise()
              }
            }

            function c(e, t, n) {
              var r = ge(e, "src");
              return k(r) ? s(r) : a(e, t, n)
            }

            function u(e) {
              var t = ge(e, "src"),
                n = pe(),
                r = new Image;
              return r.onload = n.resolve, r.onerror = n.reject, r.src = t, n.promise()
            }

            function l(e, t) {
              var r, a = o(e),
                s = i(e),
                l = T(s, function(e) {
                  return u(e)
                }),
                d = -1,
                f = T(a, function(e) {
                  return ne(e) || (d += 1), c(e, t, "." + hi + "-" + d)
                });
              return B(l) ? $e(f) : (n.log(Jo, "images: start"), r = we(l), r.done(function() {
                se(t), n.log(Jo, "images: end")
              }), r.then(function() {
                return $e(f)
              }))
            }
            var d = {
              fetch: l
            };
            return d
          }

          function Pt(e, t) {
            var n = te(t);
            C(_i(n, "script"), function(t) {
              return V(e, t)
            }), Lt(e, n.html())
          }

          function Lt(e, t) {
            q(e, t)
          }

          function Ut(e, t) {
            var n = te(t),
              r = n.find(oa);
            n.remove(), q(e, r)
          }

          function Ft(e) {
            return b(e) ? g(e) ? Ut : y(e) ? Lt : void 0 : Pt
          }

          function jt(e) {
            return function(t) {
              var n = pe();
              return e(de(t[na.SELECTOR]), t), n.resolve(t), n.promise()
            }
          }

          function Ht(e, t, n, r) {
            function i(t) {
              return function(n) {
                var r = pe(),
                  i = t(n),
                  o = i.context,
                  a = i.content;
                return e.fetch(a, o).fail(function(e) {
                  return r.reject(e)
                }).always(function() {
                  return r.resolve(n)
                }), r.promise()
              }
            }

            function o(e, t) {
              q(e, t[na.CONTENT])
            }

            function a(e, t) {
              G(e, t[na.CONTENT])
            }

            function s(e, t) {
              return g(e) ? void q(e, t[na.CONTENT]) : void c(e, t)
            }

            function c(e, t) {
              W(e, t[na.CONTENT])
            }

            function u(e, t) {
              V(e, t[na.CONTENT])
            }

            function l(e, t) {
              c(e, t), e.remove()
            }

            function d(e) {
              var t = e[na.SELECTOR],
                n = e[na.CONTENT],
                r = ce(n);
              return s(t, {
                content: r
              }), {
                context: "head" === t ? t : fe(t),
                content: n
              }
            }

            function f(e, t) {
              var n = e[na.SELECTOR],
                r = e[na.CONTENT],
                i = ce(r);
              return t(n, {
                content: i
              }), {
                context: n,
                content: r
              }
            }

            function h(e) {
              return f(e, a)
            }

            function p(e) {
              return f(e, o)
            }

            function m(e) {
              var t = e[na.SELECTOR],
                n = e[na.CONTENT],
                r = e[na.CONTENT_TYPE],
                i = r === ia.TEXT ? Z : ee,
                o = ce(n);
              return i(t, o), {
                context: t,
                content: n
              }
            }

            function v(e, t) {
              e.css(t[na.CONTENT])
            }

            function y(e, t) {
              C(t.content, function(t, n) {
                "src" === n && ye(e, "src"), e.attr(n, t)
              })
            }

            function b(e, t) {
              t[na.PRIORITY] && P(e[0].style.setProperty) ? e.each(function(e, n) {
                n.style.setProperty(t[na.PROPERTY], t[na.VALUE], t[na.PRIORITY])
              }) : e.css(t[na.PROPERTY], t[na.VALUE])
            }

            function E(e) {
              e.remove()
            }

            function _(e, t) {
              n({
                element: e,
                clickToken: t[na.CLICK_TRACK_ID]
              })
            }

            function $(e, t) {
              var n = t[na.FROM],
                r = t[na.TO],
                i = e.children(),
                o = i.eq(n),
                a = i.eq(r);
              return !(!o.exists() || !a.exists()) && void(r > n ? a.after(o) : a.before(o))
            }

            function w(e, n) {
              t.redirect(n[na.URL])
            }
            return {
              getStrategyByAction: function(e) {
                switch (e) {
                  case ta.APPEND_CONTENT:
                    return i(p);
                  case ta.CUSTOM_CODE:
                    return i(d);
                  case ta.INSERT_AFTER:
                    return jt(u);
                  case ta.INSERT_BEFORE:
                    return jt(c);
                  case ta.MOVE:
                    return jt(v);
                  case ta.SET_CONTENT:
                    return i(m);
                  case ta.SET_ATTRIBUTE:
                    return jt(y);
                  case ta.SET_STYLE:
                    return jt(b);
                  case ta.PREPEND_CONTENT:
                    return i(h);
                  case ta.RESIZE:
                    return jt(v);
                  case ta.REMOVE:
                    return jt(E);
                  case ta.REARRANGE:
                    return jt($);
                  case ta.REDIRECT:
                    return jt(w);
                  case ta.REPLACE_CONTENT:
                    return jt(l);
                  case ta.TRACK_CLICK:
                    return jt(_);
                  default:
                    return r.error("Unknown action:", e),
                      function() {}
                }
              }
            }
          }

          function Bt(e, t) {
            return {
              success: function() {
                return Ut(e, t)
              },
              error: Wi
            }
          }

          function zt(e, t) {
            var n = ie(v(e)),
              r = ce(t);
            return ee(e, r), {
              success: Wi,
              error: function() {
                return ee(e, n)
              }
            }
          }

          function qt(e, t) {
            return g(e) ? Bt(e, t) : zt(e, t)
          }

          function Gt(e) {
            return function(t, n) {
              var r = qt(t, n);
              return e.fetch(n, t).done(r.success).fail(r.error)
            }
          }

          function Vt() {
            return function(e, t) {
              if (!R(t)) return me();
              var n = Ft(e);
              return C(t, function(t) {
                return n(e, t)
              }), me()
            }
          }

          function Wt(e) {
            return {
              build: function(t) {
                var n = {};
                return k(t.name) ? (n[Fi] = t.name + ii, n[Di] = t.clickToken) : e.error(ni), n
              }
            }
          }

          function Yt(e, t, n, r) {
            var i = n.currentTarget,
              o = i && i.tagName && i.tagName.toLowerCase() === sa,
              a = r.build(i, aa);
            o && n.preventDefault(), e.fetch({
              params: t
            }).then(a, a)
          }

          function Kt(e, t, n) {
            return function(r) {
              var i, o = t.build(r);
              U(o) || (i = r.element, de(i).on(aa, function(t) {
                return Yt(e, o, t, n)
              }))
            }
          }

          function Xt(e, t) {
            var n = new window.CustomEvent(e, {
              detail: t
            });
            document.dispatchEvent(n)
          }

          function Qt(e, t, n) {
            Xt(da, {
              type: da,
              mbox: e,
              message: t,
              tracking: n
            })
          }

          function Jt(e, t) {
            Xt(la, {
              type: la,
              mbox: e,
              tracking: t
            })
          }

          function Zt(e, t, n) {
            Xt(ua, {
              type: ua,
              mbox: e,
              message: t,
              tracking: n
            })
          }

          function en(e, t) {
            Xt(ca, {
              type: ca,
              mbox: e,
              tracking: t
            })
          }

          function tn(e, t, n, r) {
            function i(e) {
              var t = e.element,
                r = e.name,
                i = e.clickToken;
              b(t) || k(i) && n({
                name: r,
                element: t,
                clickToken: i
              })
            }

            function o(e) {
              return function(t) {
                return Qt(e, t.message, Te(r))
              }
            }

            function a(n) {
              var a = n.element,
                s = n.content,
                c = n.plugins,
                u = n.name;
              return e(a, s).then(function() {
                return t(a, c)
              }, o(u)).done(function() {
                i(n), Jt(u, Te(r))
              }).always(function() {
                return m(a)
              })
            }
            var s = {
              handle: a
            };
            return s
          }

          function nn(e) {
            return k(e.clickToken)
          }

          function rn(e, t) {
            function n(e) {
              var t = void 0;
              return R(e) && (t = x(e, function(e) {
                return F(e) && "default" === e.type
              })[0]), t
            }

            function r(r) {
              var i = r.elements,
                o = n(r.offers);
              return B(i) ? me(Zi) : F(o) ? (e(i[0], o.plugins), nn(o) && ! function() {
                var e = r.name,
                  n = o.clickToken;
                C(i, function(r) {
                  return t({
                    name: e,
                    element: r,
                    clickToken: n
                  })
                })
              }(), m(i), me(Zi)) : (m(i), me(Zi))
            }
            return {
              handle: r
            }
          }

          function on(e) {
            return x(e, function(e) {
              return "html" === e.type
            })[0]
          }

          function an(e) {
            function t(t) {
              var n, r, i, o, a, s = pe(),
                c = t.name,
                u = t.elements,
                l = t.offers;
              return B(u) || !R(l) ? me(eo) : (n = on(l), F(n) ? (r = n.plugins, i = n.content, o = n.clickToken,
                a = T(u, function(t, n) {
                  return function() {
                    var a = {
                      name: c,
                      element: t,
                      content: i,
                      clickToken: o
                    };
                    return 0 === n && (a.plugins = r), e.handle(a)
                  }
                }), $e(a).always(function() {
                  return s.resolve(Zi)
                }), s.promise()) : me(eo))
            }
            return {
              handle: t
            }
          }

          function sn(e) {
            return x(e, function(e) {
              return "redirect" === e.type
            })[0]
          }

          function cn(e) {
            function t(t) {
              if (!R(t.offers)) return me(eo);
              var n = sn(t.offers);
              return F(n) ? (e.trigger(gi), e.redirect(n.content), me(Zi)) : me(eo)
            }
            return {
              handle: t
            }
          }

          function un(e) {
            function t(t) {
              if (!nn(t)) return {};
              var n = {};
              return n[Fi] = e.globalMboxName + ii, n[Ri] = t.clickToken, n
            }
            return {
              build: t
            }
          }

          function ln(e, t, n) {
            if (!F(e) || B(t)) return !1;
            var r = x(t, function(t) {
              return n(e[t])
            });
            return B(r)
          }

          function dn(e, t) {
            return ln(e, t, function(e) {
              return N(e)
            })
          }

          function fn(e, t) {
            return ln(e, t, function(e) {
              return !O(e)
            })
          }

          function hn(e) {
            var t = {};
            return R(e) ? (C(e, function(e) {
              D(t[e.selector]) && (t[e.selector] = []), t[e.selector].push(e)
            }), T(t, function(e, t) {
              return {
                selector: t,
                group: e
              }
            })) : []
          }

          function pn(e) {
            return k(e[na.CSS_SELECTOR]) && !(e[na.ACTION] === ta.TRACK_CLICK || e[na.ACTION] === ta
              .PREPEND_CONTENT || e[na.ACTION] === ta.APPEND_CONTENT || e[na.ACTION] === ta.INSERT_AFTER || e[na
                .ACTION] === ta.INSERT_BEFORE)
          }

          function mn(e) {
            return e[na.ACTION] === ta.REPLACE_CONTENT || !pn(e) || e.action === ta.SET_STYLE && "visibility" === e[
              na.PROPERTY]
          }

          function vn(e) {
            return dn(e, [na.SELECTOR, na.ACTION])
          }

          function gn(e) {
            return dn(e, [na.ACTION])
          }

          function yn(e) {
            return dn(e, [na.CONTENT])
          }

          function bn(e) {
            return dn(e, [na.ASSET, na.VALUE])
          }

          function En(e) {
            var t = arguments.length <= 1 || void 0 === arguments[1] ? [] : arguments[1],
              n = {};
            return t.push(na.ACTION), $n(e, n, t), n
          }

          function _n(e) {
            var t = arguments.length <= 1 || void 0 === arguments[1] ? [] : arguments[1];
            return t.push(na.SELECTOR), k(e[na.CSS_SELECTOR]) && t.push(na.CSS_SELECTOR), En(e, t)
          }

          function $n(e, t, n) {
            var r = x(n, function(t) {
              return !D(e[t])
            });
            C(r, function(n) {
              return t[n] = e[n]
            })
          }

          function wn(e, t) {
            return {
              build: function(n) {
                var r, i, o = En(n),
                  a = n[na.URL];
                return I(n[na.INCLUDE_ALL_URL_PARAMETERS]) && n[na.INCLUDE_ALL_URL_PARAMETERS] && (r = u(e
                  .location.search.substring(1)), a = c(a, r)), I(n[na.PASS_MBOX_SESSION]) && n[na
                  .PASS_MBOX_SESSION] && (i = t.getId(), a = c(a, {
                  mboxSession: i
                })), o[na.URL] = a, o
              },
              valid: function(e) {
                return dn(e, [na.ACTION, na.URL]) && X(e[na.URL])
              }
            }
          }

          function Tn() {
            return {
              build: function(e) {
                return _n(e, [na.CLICK_TRACK_ID])
              },
              valid: function(e) {
                return vn(e) && dn(e, [na.CLICK_TRACK_ID])
              }
            }
          }

          function Cn() {
            return {
              build: function(e) {
                return _n(e, [na.FROM, na.TO])
              },
              valid: function(e) {
                return vn(e) && fn(e, [na.FROM, na.TO])
              }
            }
          }

          function xn() {
            return {
              build: function(e) {
                return _n(e)
              },
              valid: function(e) {
                return vn(e)
              }
            }
          }

          function Sn() {
            return {
              build: function(e) {
                var t = _n(e);
                return t[na.CONTENT] = {
                  height: e[na.FINAL_HEIGHT],
                  width: e[na.FINAL_WIDTH]
                }, t
              },
              valid: function(e) {
                return vn(e) && dn(e, [na.FINAL_HEIGHT, na.FINAL_WIDTH])
              }
            }
          }

          function An() {
            return {
              build: function(e) {
                var t = _n(e),
                  n = [na.PROPERTY, na.VALUE, na.SELECTOR];
                return C(n.concat(), function(n) {
                  return t[n] = e[n]
                }), e[na.PRIORITY] === ra.IMPORTANT && (t[na.PRIORITY] = e[na.PRIORITY]), t
              },
              valid: function(e) {
                return vn(e) && dn(e, [na.PROPERTY, na.VALUE])
              }
            }
          }

          function Mn() {
            return {
              build: function(e) {
                var t = _n(e),
                  n = ia.HTML;
                return e[na.CONTENT_TYPE] === ia.TEXT && (n = ia.TEXT), t[na.CONTENT_TYPE] = n, t[na.CONTENT] = e[
                  na.CONTENT], t
              },
              valid: function(e) {
                return vn(e) && yn(e)
              }
            }
          }

          function kn() {
            return {
              build: function(e) {
                var t, n = {},
                  r = e[na.ATTRIBUTE],
                  i = e[na.VALUE];
                return n[r] = i, t = _n(e), t[na.CONTENT] = n, t
              },
              valid: function(e) {
                return vn(e) && dn(e, [na.ATTRIBUTE, na.VALUE])
              }
            }
          }

          function Nn() {
            return {
              build: function(e) {
                var t = _n(e),
                  n = {
                    left: e[na.FINAL_LEFT_POSITION],
                    top: e[na.FINAL_TOP_POSITION]
                  };
                return k(e[na.POSITION]) && (n.position = e[na.POSITION]), t[na.CONTENT] = n, t
              },
              valid: function(e) {
                return vn(e) && fn(e, [na.FINAL_LEFT_POSITION, na.FINAL_TOP_POSITION])
              }
            }
          }

          function In() {
            return {
              build: function(e) {
                var t = En(e);
                return t[na.CONTENT] = e[na.CONTENT], k(e[na.SELECTOR]) ? t[na.SELECTOR] = e[na.SELECTOR] : t[na
                    .SELECTOR] = "head",
                  t
              },
              valid: function(e) {
                return gn(e) && yn(e)
              }
            }
          }

          function On() {
            return {
              build: function(e) {
                return _n(e, [na.CONTENT])
              },
              valid: function(e) {
                return vn(e) && yn(e)
              }
            }
          }

          function Dn(e, t) {
            var n = te(t[na.CONTENT]);
            n.find(":first").attr("id", e), t[na.CONTENT] = n.html(), t[na.SELECTOR] = t[na.SELECTOR].replace(ha,
              "")
          }

          function Rn() {
            return {
              build: function(e) {
                var t = _n(e, [na.CONTENT]),
                  n = e[na.SELECTOR].match(fa);
                return R(n) && 2 === n.length ? Dn(n[1], t) : bn(e) && (t[na.CONTENT] = '<img src="' + e[na
                  .VALUE] + '" />'), t
              },
              valid: function(e) {
                return vn(e) && (bn(e) || yn(e))
              }
            }
          }

          function Pn(e, t) {
            var n = {},
              r = Rn(),
              i = On();
            return n[ta.APPEND_CONTENT] = i, n[ta.CUSTOM_CODE] = In(), n[ta.INSERT_AFTER] = r, n[ta.INSERT_BEFORE] =
              r, n[ta.MOVE] = Nn(), n[ta.SET_ATTRIBUTE] = kn(), n[ta.SET_CONTENT] = Mn(), n[ta.SET_STYLE] = An(), n[
                ta.RESIZE] = Sn(), n[ta.PREPEND_CONTENT] = i, n[ta.REMOVE] = xn(), n[ta.REARRANGE] = Cn(), n[ta
                .REPLACE_CONTENT] = i, n[ta.TRACK_CLICK] = Tn(), n[ta.REDIRECT] = wn(e, t), n
          }

          function Ln(e) {
            function t(t) {
              var n = [],
                r = x(t, function(e) {
                  return gn(e)
                });
              return C(r, function(t) {
                var r = t[na.ACTION],
                  i = e[r];
                i.valid(t) && n.push(i.build(t))
              }), n
            }
            return {
              transform: t,
              isSupported: function(t) {
                return F(e[t[na.ACTION]])
              }
            }
          }

          function Un(e) {
            function t() {
              return P(e.requestAnimationFrame) ? function(t) {
                return e.requestAnimationFrame(t)
              } : function(e) {
                return r(e, pa)
              }
            }

            function n(e) {
              var n = t();
              n(e)
            }
            var r = function(t, n) {
              var r = e.setTimeout(t, n);
              return {
                dispose: function() {
                  return e.clearTimeout(r)
                }
              }
            };
            return {
              getFutureScheduler: t,
              scheduleFuture: r,
              schedule: n
            }
          }

          function Fn(e, t, n) {
            function r(e, t, n) {
              return function() {
                return i(e, t, n)
              }
            }

            function i(e, r, i) {
              var a = pe(),
                s = T(r.group, function(e) {
                  return function() {
                    return o(e)
                  }
                });
              return le(r.selector) ? ($e(s).fail(function(i) {
                t.log("failed applying:", JSON.stringify(r)), Qt(e, i, Te(n))
              }).always(function() {
                a.resolve(i)
              }), a.promise()) : (i.push(r), a.resolve(i), a.promise())
            }

            function o(n) {
              var r = pe(),
                i = e.getStrategyByAction(n[na.ACTION]);
              return i(n).then(function() {
                if (!mn(n)) {
                  var e = $(n[na.CSS_SELECTOR]);
                  Y("#at-" + e)
                }
                t.log(ma, JSON.stringify(n)), r.resolve()
              }, function(e) {
                return r.reject(e)
              }), r.promise()
            }
            return {
              createDeferred: r
            }
          }

          function jn(t, n, r, i, o) {
            function a() {
              o.log(va), i.trigger(va)
            }

            function s() {
              o.log("trigger " + va + " in " + l + "ms");
              var e = r.scheduleFuture(a, l);
              i.subscribeOnce(gi, function() {
                e.dispose(), a()
              })
            }

            function c(e, a) {
              var s, u;
              return d && !B(a) && (s = function() {
                var n = [],
                  i = T(a, function(r) {
                    return t.createDeferred(e, r, n)
                  });
                return o.log("Retrying actions:", a), $e(i).always(function(t) {
                  return r.schedule(function() {
                    return c(e, t)
                  })
                }), {
                  v: void 0
                }
              }(), "object" == typeof s) ? s.v : (B(a) ? (o.log("All selectors have been found"), Jt(e, Te(n))) :
                (u = T(a, function(e) {
                  return ci + " for " + e.selector
                }), o.log("Failed: ", u), Qt(e, u, Te(n))), void i.showElements())
            }

            function u(e, t) {
              c(e, t)
            }
            var l = e.pollingAfterDomReadyTimeout,
              d = !0;
            return i.onDomReady(s), i.subscribeOnce(va, function() {
              return d = !1
            }), {
              execute: u
            }
          }

          function Hn(e) {
            return x(e, function(e) {
              return "actions" === e.type
            })
          }

          function Bn(e) {
            var t = [];
            return C(e, function(e) {
              return t.push.apply(t, e.content)
            }), t
          }

          function zn(e) {
            var t = [];
            return C(e, function(e) {
              B(e.plugins) || t.push.apply(t, e.plugins)
            }), t
          }

          function qn(e, t, n, r, i, o, a, s) {
            function c(e, t) {
              return x(e, function(e) {
                return t(e)
              })
            }

            function u(e) {
              return c(e, function(e) {
                return !t.isSupported(e)
              })
            }

            function l(e) {
              return c(e, t.isSupported)
            }

            function d(e) {
              var n = l(e);
              return t.transform(n)
            }

            function f(e) {
              var t = u(e);
              C(t, function(e) {
                return o.log("unsupported offer", e)
              })
            }

            function h(e) {
              var t, r, o = x(e, function(e) {
                  return e[na.ACTION] === ta.REDIRECT
                }),
                a = d(o);
              return !!F(a[0]) && (t = a[0], r = n.getStrategyByAction(ta.REDIRECT), i.trigger(gi), r(t), !0)
            }

            function p(e) {
              var t = [];
              C(e, function(e) {
                pn(e) && t.push(e[na.CSS_SELECTOR])
              }), o.log("pre-hide", _e(t)), i.hideElements(_e(t))
            }

            function m(e, t) {
              var n = e.name;
              B(t) ? (o.log("There are no failed actions"), Jt(n, Te(a)), i.showElements()) : (o.log(
                "Start polling for failed actions"), s.execute(n, t))
            }

            function v(t) {
              var n, i, o, a, s, c, u, l = t.name,
                v = B(t.elements) ? de("head")[0] : t.elements[0],
                g = Hn(t.offers);
              return B(g) ? me(eo) : (n = Bn(g), f(n), h(n) ? me(Zi) : (i = d(n), p(i), o = [], a = hn(i), s = T(a,
                function(e) {
                  return r.createDeferred(l, e, o)
                }), c = pe(), u = zn(g), $e(s).always(function(n) {
                m(t, n), e(v, u), c.resolve(eo)
              }), c.promise()))
            }
            return {
              handle: v
            }
          }

          function Gn(e) {
            return R(e.plugins)
          }

          function Vn(e) {
            return R(e.actions) && !B(e.actions)
          }

          function Wn(e) {
            return k(e.redirect)
          }

          function Yn(e) {
            return D(e.actions) && D(e.dynamic) && D(e.html) && D(e.redirect)
          }

          function Kn(e) {
            return k(e.html)
          }

          function Xn(e) {
            return F(e.dynamic) && k(e.dynamic.url)
          }

          function Qn(e) {
            return {
              type: "redirect",
              content: e.redirect
            }
          }

          function Jn(e) {
            var t = {
              type: "html",
              content: e.html
            };
            return nr(t, e), rr(t, e), t
          }

          function Zn(e) {
            var t = {
              type: "dynamic",
              content: e.dynamic
            };
            return nr(t, e), rr(t, e), t
          }

          function er(e) {
            var t = {
              type: "default"
            };
            return nr(t, e), rr(t, e), t
          }

          function tr(e) {
            var t = {
              type: "actions",
              content: e.actions
            };
            return rr(t, e), t
          }

          function nr(e, t) {
            nn(t) && (e.clickToken = t.clickToken)
          }

          function rr(e, t) {
            Gn(t) && (e.plugins = t.plugins)
          }

          function ir(e, t, n) {
            var r = x(e, function(e) {
              return t(e)
            });
            return T(r, function(e) {
              return n(e)
            })
          }

          function or(e) {
            if (!Vn(e)) return !1;
            var t = ar(e.actions);
            return !B(t)
          }

          function ar(e) {
            return x(e, function(e) {
              return e[na.ACTION] === ta.REDIRECT
            })
          }

          function sr(e) {
            var t, n = ir(e, Wn, Qn);
            return B(n) ? (t = ir(e, or, cr), B(t) ? [] : t) : n
          }

          function cr(e) {
            var t = "actions",
              n = ar(e.actions).slice(0, 1);
            return {
              type: t,
              content: n
            }
          }

          function ur(e) {
            function t(t, n) {
              var r = ir(t, Kn, Jn),
                i = ir(t, Xn, Zn),
                o = ir(t, Vn, tr),
                a = ir(t, Yn, er),
                s = T(i, function(t) {
                  var i = {
                      offer: t,
                      params: n
                    },
                    o = t.clickToken,
                    s = t.plugins,
                    c = {
                      clickToken: o,
                      plugins: s
                    };
                  return function() {
                    var t = pe();
                    return e(i).then(function(e) {
                      c.html = e, r.push(Jn(c)), t.resolve()
                    }, function() {
                      a.push(er(c)), t.resolve()
                    }), t.promise()
                  }
                });
              return $e(s).then(function() {
                return [].concat(r, a, o)
              })
            }

            function n(e, n) {
              var r, i = e.offers;
              return R(i) ? (r = sr(i), B(r) ? t(i, n) : me(r)) : me([])
            }
            var r = {
              extract: n
            };
            return r
          }

          function lr(e, t) {
            return function(n) {
              return n === !1 ? ve() : e.handle(t)
            }
          }

          function dr(e, t) {
            return {
              process: function(n, r, i, o) {
                var a = {
                  name: n,
                  params: r,
                  elements: i
                };
                return e.extract(o, r).then(function(e) {
                  a.offers = e;
                  var n = T(t, function(e) {
                    return lr(e, a)
                  });
                  return $e(n)
                })
              }
            }
          }

          function fr(e, t) {
            D(e[ba]) ? e[ba] = [t] : R(e[ba]) && e[ba].push(t)
          }

          function hr(e) {
            return {
              handle: function(t) {
                var n = t.content;
                return F(n) && fr(e, n), Pe()
              }
            }
          }

          function pr(t, n) {
            var r = e.cookieDomain;
            return {
              handle: function(e) {
                var i, o, a, s = e.content;
                return F(s) ? (i = s.duration, O(i) || (i = Ea), o = s.message, N(o) && (o = _a), He(t, lo, o, {
                  expires: 1e3 * i,
                  domain: r
                }), xe(), a = e.name, k(a) && Zt(a, o, Te(n)), De(o)) : Pe()
              }
            }
          }

          function mr(e) {
            return {
              handle: function(t) {
                var n, r = t.content;
                return N(r) ? Pe() : (n = t.name, k(n) && Zt(n, r, Te(e)), De(r))
              }
            }
          }

          function vr(e) {
            return {
              handle: function(t) {
                var n = t.content;
                return k(n) && e.setId(n), Pe()
              }
            }
          }

          function gr(e) {
            return !L(ze(e, uo))
          }

          function yr(e) {
            var t = e.split(".");
            return 2 !== t.length || N(t[1]) ? null : (t = t[1].split("_"), 2 !== t.length || N(t[0]) ? null : t[0])
          }

          function br(e, t) {
            var n = e.clientCode,
              r = e.serverDomain;
            return r.replace(n, si + t)
          }

          function Er(e, t, n) {
            var r, i, o;
            t.overrideMboxEdgeServer && (gr(e) || (r = yr(n), N(r) || (i = t.overrideMboxEdgeServerTimeout, o = br(
              t, r), He(e, uo, o, {
              expires: i
            }))))
          }

          function _r(e, t, n) {
            return {
              handle: function(r) {
                var i = r.content;
                return k(i) && (n.setId(i), Er(e, t, i)), Pe()
              }
            }
          }

          function $r() {
            function e(e, n) {
              return t = t.then(e, n || e)
            }
            var t = me();
            return {
              addTask: e
            }
          }

          function wr(t, n) {
            function r(e, n, r) {
              var i = t[n],
                o = r[n];
              return F(i) ? i.handle({
                name: e,
                content: o
              }) : Pe()
            }

            function i(t, r) {
              var i, o;
              if (t === e.globalMboxName && e.globalMboxAutoCreate !== !1) {
                if (D(r.offers)) return void n.triggerShowBody();
                i = x(r.offers, function(e) {
                  return Vn(e)
                }), o = x(r.offers, function(e) {
                  return Wn(e)
                }), B(i) && B(o) && n.triggerShowBody()
              }
            }
            return {
              process: function(e, t) {
                var n, o;
                return i(e, t), n = T(t, function(n, i) {
                  return r(e, i, t)
                }), o = x(n, function(e) {
                  return Qi === e.status
                }), B(o) ? Pe() : o[0]
              }
            }
          }

          function Tr(e) {
            return !(j(e.element) && k(e.selector))
          }

          function Cr(e, t, n, r) {
            return function(i) {
              if (i === Zi) return ve();
              var o = {
                name: t,
                elements: n,
                offers: r
              };
              return e.handle(o)
            }
          }

          function xr(e, t) {
            function n(n) {
              var r, i, o, a, s, c = wt(n, Ca);
              return c.status === Qi ? (t.error($a, c.message), ve()) : (r = de(n.element || n.selector || "head"),
                i = x(r, j), o = n.offer, a = n.mbox, s = T(e, function(e) {
                  return Cr(e, a, i, o)
                }), $e(s))
            }
            return n
          }

          function Sr(e) {
            var t = {};
            return C(e, function(e, n) {
              return t[n] = e
            }), t
          }

          function Ar(e, t, n, r, i, o, a) {
            function s(t, i, a) {
              en(t, Te(e)), o.log(Sa, Ki + ":", i);
              var s = n.process(t, i);
              return Qi === s.status ? ve(s) : r.extract(i, a)
            }

            function c(t, n) {
              var r = n.status,
                i = n.message;
              return r === Ji && o.log(Sa, "request disabled:", r, i), Zt(t, i, Te(e)), n
            }

            function u(e) {
              return o.log(Sa, Xi + ":", ri), ve(Oe(Xi, ri))
            }

            function l(e, n) {
              var r = {};
              return r[Fi] = n, t.fetch(e).then(function(e) {
                return s(n, e, r)
              }, function(e) {
                return c(n, e)
              })
            }
            return function(t) {
              var n, r, s, c = wt(t, xa),
                d = c.status,
                f = c.message;
              return o.log("box " + t.mbox + " isEnabled: " + e.isEnabled()), d === Qi ? (o.error(Sa, f), ve(c)) :
                e.isEnabled() ? (n = t.mbox, r = Ie(t.timeout, a.timeout), s = F(t.params) ? Sr(t.params) : {}, s[
                  Fi] = n, s[Li] = e.requests.incrementAndGet(), o.log(Sa, "params:", s), l({
                  params: i.mergeParameters(i.getTargetPageParameters(n), s),
                  timeout: r
                }, n)) : u(t)
            }
          }

          function Mr(e, t, n) {
            t.log(Ma, Xi + ":", ri), P(n.error) && e.delayCallback(n.error, Xi, ri)
          }

          function kr(e, t, n, r) {
            return r ? e.build(t, n) : Wi
          }

          function Nr(e, t, n, r) {
            var i = t.mbox,
              o = Le(t.error),
              a = Le(t.success),
              s = F(t.params) ? t.params : {};
            return s[Fi] = i, e.fetch({
              timeout: r,
              params: s
            }).then(function() {
              a(), n()
            }, function() {
              o(), n()
            })
          }

          function Ir(e, t) {
            F(e) && P(e.preventDefault) && t && e.preventDefault()
          }

          function Or(e, t, n, r) {
            var i = n.type,
              o = n.selector,
              a = !!n.preventDefault,
              s = de(o);
            C(s, function(o) {
              var s = kr(t, o, i, a);
              de(o).on(i, function(t) {
                Ir(t, a), Nr(e, n, s, r)
              })
            })
          }

          function Dr(e, t, n, r, i) {
            var o = !!n.preventDefault,
              a = r.currentTarget,
              s = r.type,
              c = kr(t, a, s, o);
            Ir(r, o), Nr(e, n, c, i)
          }

          function Rr(e, t, n, r, i, o, a) {
            return function(s) {
              var c, u, l, d, f, h, p = wt(s, Aa);
              return p.status === Qi ? void o.error(Ma, p.message) : e.isEnabled() ? (c = s.type, u = k(c), l = s
                .selector, d = k(l), f = Ie(s.timeout, a.timeout), u && d ? void Or(n, r, s, f) : (h = t.event,
                  F(h) ? void Dr(n, r, s, h, f) : void Nr(n, s, Wi, f))) : void Mr(i, o, s)
            }
          }

          function Pr(e, t, n) {
            return e[Fi] = t, e[Li] = n, e
          }

          function Lr(t, n, r, i, o, a, s, c) {
            function u(e, n, r, a) {
              en(n, Te(t));
              var c = i.process(n, e);
              return c.status === Ki ? o.process(n, r, a, e).always(function() {
                return s.log(Na, "process success:", e, n)
              }) : (s.error(Na, "response process error:", c.message, n), m(a), me())
            }

            function l(e, n, r) {
              var i = e.status,
                o = e.message;
              i === Ji ? s.log(Na, "request disabled:", i, o) : s.error(Na, "request error:", i, o), Zt(n, o, Te(
                t)), m(r)
            }

            function d(t, n, i, o) {
              s.log(Na, o, n);
              var a = x(i, j);
              return r.fetch({
                params: n,
                timeout: e.timeout
              }).then(function(e) {
                return u(e, t, n, a)
              }, function(e) {
                return l(e, t, a)
              })
            }

            function f(e, r) {
              var i, o, c, u, l, d, f;
              if (t.isEnabled() || t.isMboxEdit()) {
                if (N(e)) return void s.error(Na, Ia, r);
                if (!le("#" + e)) return i = Te(t), Qt(r, ci + ' (no element with such id: "' + e + '").', i),
                  void s.error(Na, ci + ":", 'mboxDefine("' + e + '", "' + r + '")');
                for (o = de("#" + e), o.addClass(Jr + r), c = arguments.length, u = Array(c > 2 ? c - 2 : 0), l =
                  2; c > l; l++) u[l - 2] = arguments[l];
                if (d = a.getParametersFromArray(u), f = wt({
                    mbox: r
                  }, ka), f.status === Qi) return void s.error(Na, f.message);
                Pr(d, r, t.requests.incrementAndGet()), n.add(r, {
                  name: r,
                  params: d,
                  node: o
                }), s.log(Na, "create mbox, params:", d)
              }
            }

            function h(e) {
              var r, i, o, u, l;
              for (r = arguments.length, i = Array(r > 1 ? r - 1 : 0), o = 1; r > o; o++) i[o - 1] = arguments[o];
              if (t.isEnabled()) {
                if (u = wt({
                    mbox: e
                  }, ka), u.status === Qi) return void s.error(Na, u.message);
                l = n.findByKey(e), C(l, function(t) {
                  var n = a.mergeParameters(a.getTargetPageParameters(e), a.getParametersFromArray(i));
                  n = a.mergeParameters(t.params, n), n[ji] = a.generateId(), c.addTask(function() {
                    return d(e, n, t.node, "execute mbox request, params:")
                  })
                })
              }
            }

            function p(e) {
              var r, i, o, u, l, f, h, p;
              if (t.isEnabled() || t.isMboxEdit()) {
                if (r = wt({
                    mbox: e
                  }, ka), r.status === Qi) return void s.error(Na, r.message);
                if (i = a.findLastMboxNode(e), !le(i)) return o = Te(t), u = ci +
                  " (previous element is not a div.mboxDefault).", Qt(e, u, o), void s.error(Na, ci + ":",
                    'mboxCreate("' + e + '")');
                for (i.addClass(Jr + e), l = arguments.length, f = Array(l > 1 ? l - 1 : 0), h = 1; l > h; h++) f[
                  h - 1] = arguments[h];
                p = a.mergeParameters(a.getTargetPageParameters(e), a.getParametersFromArray(f)), Pr(p, e, t
                  .requests.incrementAndGet()), n.add(e, {
                  name: e,
                  params: p,
                  node: i
                }), t.isEnabled() && c.addTask(function() {
                  return d(e, p, i, "create mbox and execute mbox request, params:")
                })
              }
            }
            return {
              createMbox: f,
              fetchAndDisplayMbox: h,
              createFetchAndDisplayMbox: p
            }
          }

          function Ur(e) {
            e.document.addEventListener("click", function(t) {
              P(e._AT.clickHandlerForExperienceEditor) && e._AT.clickHandlerForExperienceEditor(t)
            }, !0)
          }

          function Fr(e, t, n, r) {
            e.isMboxEdit() && (t._AT = t._AT || {}, t._AT.querySelectorAll = de, n({
              url: Yi,
              type: Do.SCRIPT
            }).then(function() {
              return Ur(t)
            }, function() {
              return r.error(Oa)
            }))
          }

          function jr(e, t, n, r, i, o) {
            var a, s;
            o.globalMboxAutoCreate === !0 && (N(o.globalMboxName) || e.isEnabled() && (i.hideBody(), a = o
              .globalMboxName, s = function() {
                return n({
                  mbox: a,
                  params: i.getTargetPageParameters()
                }).then(function(e) {
                  return r({
                    mbox: a,
                    offer: e
                  })
                }, ue)
              }, t.addTask(s)))
          }

          function Hr(e) {
            e.event = {
              CONTENT_RENDERING_FAILED: da,
              CONTENT_RENDERING_SUCCEEDED: la,
              REQUEST_SUCCEEDED: ca,
              REQUEST_FAILED: ua
            }
          }

          function Br(e) {
            if (!F(e)) throw Error("Please provide options")
          }

          function zr(e) {
            if (N(e)) throw Error("Please provide extension name");
            var t = e.split(".");
            C(t, function(e) {
              if (!Ra.test(e)) throw Error("Name space should contain only letters")
            })
          }

          function qr(e, t) {
            if (!R(e)) throw Error("Please provide an array of dependencies");
            if (0 === e.length) throw Error("Please provide an array of dependencies");
            C(e, function(e) {
              if (D(t[e])) throw Error(e + " module does not exist")
            })
          }

          function Gr(e) {
            if (!P(e)) throw Error("Please provide extension registration function")
          }

          function Vr(e, t, n) {
            var r, i, o = t.split(".");
            for (r = 0; r < o.length - 1; r++) i = o[r], e[i] = e[i] || {}, e = e[i];
            e[o[o.length - 1]] = n
          }

          function Wr(t, n) {
            var r = {
              logger: n,
              settings: {
                clientCode: e.clientCode,
                serverDomain: e.serverDomain,
                timeout: e.timeout,
                globalMboxAutoCreate: e.globalMboxAutoCreate,
                globalMboxName: e.globalMboxName
              }
            };
            return function(e) {
              var n, i, o, a;
              Br(e), n = e.name, zr(e.name), i = e.modules, qr(i, r), o = e.register, Gr(o), t[Da] = t[Da] || {},
                a = [], C(i, function(e) {
                  return a.push(r[e])
                }), Vr(t[Da], n, o.apply(null, a))
            }
          }

          function Yr(e) {
            var t = pe();
            try {
              e(), t.resolve()
            } catch (e) {
              t.reject(e)
            }
            return t.promise()
          }

          function Kr(e) {
            function t(t, r) {
              Yr(function() {
                return r.success(t)
              }).fail(function(t) {
                e.error(La, Ua, t), n(De(t.message), r)
              })
            }

            function n(t, n) {
              var r = t.status,
                i = t.message;
              Yr(function() {
                return n.error(r, i)
              }).fail(function(t) {
                e.error(La, Fa), e.error(t)
              })
            }

            function r(r, i) {
              var o, a = wt(i, Pa);
              return a.status === Qi ? (e.error(La, a.message), function() {
                return me()
              }) : (o = he(i, ["success", "error"]), function(e) {
                return r(e).then(function(e) {
                  return t(e, o)
                }, function(e) {
                  return n(e, o)
                })
              })
            }
            return r
          }

          function Xr(e, t, n, r) {
            var i = Kr(r);
            e.getOffer = function(e) {
              var r = i(t, e);
              n.addTask(function() {
                return r(he(e, ja))
              })
            }, e.registerExtension = Wr(e, r)
          }
          var Qr, Jr, Zr, ei, ti, ni, ri, ii, oi, ai, si, ci, ui, li, di, fi, hi, pi, mi, vi, gi, yi, bi, Ei, _i,
            $i, wi, Ti, Ci, xi, Si, Ai, Mi, ki, Ni, Ii, Oi, Di, Ri, Pi, Li, Ui, Fi, ji, Hi, Bi, zi, qi, Gi, Vi, Wi,
            Yi, Ki, Xi, Qi, Ji, Zi, eo, to, no, ro, io, oo, ao, so, co, uo, lo, fo, ho, po, mo, vo, go, yo, bo, Eo,
            _o, $o, wo, To, Co, xo, So, Ao, Mo, ko, No, Io, Oo, Do, Ro, Po, Lo, Uo, Fo, jo, Ho, Bo, zo, qo, Go, Vo,
            Wo, Yo, Ko, Xo, Qo, Jo, Zo, ea, ta, na, ra, ia, oa, aa, sa, ca, ua, la, da, fa, ha, pa, ma, va, ga, ya,
            ba, Ea, _a, $a, wa, Ta, Ca, xa, Sa, Aa, Ma, ka, Na, Ia, Oa, Da, Ra, Pa, La, Ua, Fa, ja, Ha, Ba, za, qa,
            Ga, Va, Wa, Ya, Ka, Xa, Qa, Ja, Za, es, ts, ns, rs, is, os, as, ss, cs, us, ls, ds, fs, hs, ps, ms, vs,
            gs, ys, bs, Es, _s, $s, ws, Ts, Cs, xs, Ss, As, Ms, ks, Ns = "at-element-marker",
            Is = function(e, t) {
              return t(e)
            }("undefined" != typeof window ? window : void 0, function(e) {
              function t(e) {
                var t = !!e && "length" in e && e.length,
                  n = bt.type(e);
                return "function" !== n && !bt.isWindow(e) && ("array" === n || 0 === t || "number" == typeof t &&
                  t > 0 && t - 1 in e)
              }

              function n(e, t, n) {
                if (bt.isFunction(t)) return bt.grep(e, function(e, r) {
                  return !!t.call(e, r, e) !== n
                });
                if (t.nodeType) return bt.grep(e, function(e) {
                  return e === t !== n
                });
                if ("string" == typeof t) {
                  if (B.test(t)) return bt.filter(t, e, n);
                  t = bt.filter(t, e)
                }
                return bt.grep(e, function(e) {
                  return ht.call(t, e) > -1 !== n
                })
              }

              function r() {
                this.expando = bt.expando + r.uid++
              }

              function i() {
                return !0
              }

              function o() {
                return !1
              }

              function a() {
                try {
                  return ut.activeElement
                } catch (e) {}
              }

              function s(e, t, n, r, i, a) {
                var c, u;
                if ("object" == typeof t) {
                  "string" != typeof n && (r = r || n, n = void 0);
                  for (u in t) s(e, u, n, r, t[u], a);
                  return e
                }
                if (null == r && null == i ? (i = n, r = n = void 0) : null == i && ("string" == typeof n ? (i =
                    r, r = void 0) : (i = r, r = n, n = void 0)), i === !1) i = o;
                else if (!i) return e;
                return 1 === a && (c = i, i = function(e) {
                  return bt().off(e), c.apply(this, arguments)
                }, i.guid = c.guid || (c.guid = bt.guid++)), e.each(function() {
                  bt.event.add(this, t, i, r, n)
                })
              }

              function c(e) {
                var t = {};
                return bt.each(e.match(V) || [], function(e, n) {
                  t[n] = !0
                }), t
              }

              function u(e) {
                return function(t, n) {
                  "string" != typeof t && (n = t, t = "*");
                  var r, i = 0,
                    o = t.toLowerCase().match(V) || [];
                  if (bt.isFunction(n))
                    for (; r = o[i++];) "+" === r[0] ? (r = r.slice(1) || "*", (e[r] = e[r] || []).unshift(n)) :
                      (e[r] = e[r] || []).push(n)
                }
              }

              function l(e, t, n, r) {
                function i(s) {
                  var c;
                  return o[s] = !0, bt.each(e[s] || [], function(e, s) {
                    var u = s(t, n, r);
                    return "string" != typeof u || a || o[u] ? a ? !(c = u) : void 0 : (t.dataTypes.unshift(
                      u), i(u), !1)
                  }), c
                }
                var o = {},
                  a = e === ue;
                return i(t.dataTypes[0]) || !o["*"] && i("*")
              }

              function d(e, t) {
                var n, r, i = bt.ajaxSettings.flatOptions || {};
                for (n in t) void 0 !== t[n] && ((i[n] ? e : r || (r = {}))[n] = t[n]);
                return r && bt.extend(!0, e, r), e
              }

              function f(e, t, n) {
                for (var r, i, o, a, s = e.contents, c = e.dataTypes;
                  "*" === c[0];) c.shift(), void 0 === r && (r = e.mimeType || t.getResponseHeader(
                  "Content-Type"));
                if (r)
                  for (i in s)
                    if (s[i] && s[i].test(r)) {
                      c.unshift(i);
                      break
                    } if (c[0] in n) o = c[0];
                else {
                  for (i in n) {
                    if (!c[0] || e.converters[i + " " + c[0]]) {
                      o = i;
                      break
                    }
                    a || (a = i)
                  }
                  o = o || a
                }
                return o ? (o !== c[0] && c.unshift(o), n[o]) : void 0
              }

              function h(e, t, n, r) {
                var i, o, a, s, c, u = {},
                  l = e.dataTypes.slice();
                if (l[1])
                  for (a in e.converters) u[a.toLowerCase()] = e.converters[a];
                for (o = l.shift(); o;)
                  if (e.responseFields[o] && (n[e.responseFields[o]] = t), !c && r && e.dataFilter && (t = e
                      .dataFilter(t, e.dataType)), c = o, o = l.shift())
                    if ("*" === o) o = c;
                    else if ("*" !== c && c !== o) {
                  if (a = u[c + " " + o] || u["* " + o], !a)
                    for (i in u)
                      if (s = i.split(" "), s[1] === o && (a = u[c + " " + s[0]] || u["* " + s[0]])) {
                        a === !0 ? a = u[i] : u[i] !== !0 && (o = s[0], l.unshift(s[1]));
                        break
                      } if (a !== !0)
                    if (a && e.throws) t = a(t);
                    else try {
                      t = a(t)
                    } catch (e) {
                      return {
                        state: "parsererror",
                        error: a ? e : "No conversion from " + c + " to " + o
                      }
                    }
                }
                return {
                  state: "success",
                  data: t
                }
              }

              function p(e, t) {
                var n = void 0 !== e.getElementsByTagName ? e.getElementsByTagName(t || "*") : void 0 !== e
                  .querySelectorAll ? e.querySelectorAll(t || "*") : [];
                return void 0 === t || t && bt.nodeName(e, t) ? bt.merge([e], n) : n
              }

              function m(e, t) {
                for (var n = 0, r = e.length; r > n; n++) Q.set(e[n], "globalEval", !t || Q.get(t[n],
                  "globalEval"))
              }

              function v(e, t, n, r, i) {
                for (var o, a, s, c, u, l, d = t.createDocumentFragment(), f = [], h = 0, v = e.length; v >
                  h; h++)
                  if (o = e[h], o || 0 === o)
                    if ("object" === bt.type(o)) bt.merge(f, o.nodeType ? [o] : o);
                    else if (_e.test(o)) {
                  for (a = a || d.appendChild(t.createElement("div")), s = (ye.exec(o) || ["", ""])[1]
                    .toLowerCase(), c = Ee[s] || Ee._default, a.innerHTML = c[1] + bt.htmlPrefilter(o) + c[2], l =
                    c[0]; l--;) a = a.lastChild;
                  bt.merge(f, a.childNodes), a = d.firstChild, a.textContent = ""
                } else f.push(t.createTextNode(o));
                for (d.textContent = "", h = 0; o = f[h++];)
                  if (r && bt.inArray(o, r) > -1) i && i.push(o);
                  else if (u = bt.contains(o.ownerDocument, o), a = p(d.appendChild(o), "script"), u && m(a), n)
                  for (l = 0; o = a[l++];) be.test(o.type || "") && n.push(o);
                return d
              }

              function g(e, t) {
                for (;
                  (e = e[t]) && 1 !== e.nodeType;);
                return e
              }

              function y(e, t) {
                return bt.nodeName(e, "table") && bt.nodeName(11 !== t.nodeType ? t : t.firstChild, "tr") ? e
                  .getElementsByTagName("tbody")[0] || e.appendChild(e.ownerDocument.createElement("tbody")) : e
              }

              function b(e) {
                return e.type = (null !== e.getAttribute("type")) + "/" + e.type, e
              }

              function E(e) {
                var t = ke.exec(e.type);
                return t ? e.type = t[1] : e.removeAttribute("type"), e
              }

              function _(e, t) {
                var n, r, i, o, a, s, c, u;
                if (1 === t.nodeType) {
                  if (Q.hasData(e) && (o = Q.access(e), a = Q.set(t, o), u = o.events)) {
                    delete a.handle, a.events = {};
                    for (i in u)
                      for (n = 0, r = u[i].length; r > n; n++) bt.event.add(t, i, u[i][n])
                  }
                  $e.hasData(e) && (s = $e.access(e), c = bt.extend({}, s), $e.set(t, c))
                }
              }

              function $(e, t) {
                var n = t.nodeName.toLowerCase();
                "input" === n && ge.test(e.type) ? t.checked = e.checked : ("input" === n || "textarea" === n) &&
                  (t.defaultValue = e.defaultValue)
              }

              function w(e, t, n, r) {
                t = dt.apply([], t);
                var i, o, a, s, c, u, l = 0,
                  d = e.length,
                  f = d - 1,
                  h = t[0],
                  m = bt.isFunction(h);
                if (m || d > 1 && "string" == typeof h && !gt.checkClone && Me.test(h)) return e.each(function(
                i) {
                  var o = e.eq(i);
                  m && (t[0] = h.call(this, i, o.html())), w(o, t, n, r)
                });
                if (d && (i = v(t, e[0].ownerDocument, !1, e, r), o = i.firstChild, 1 === i.childNodes.length && (
                    i = o), o || r)) {
                  for (a = bt.map(p(i, "script"), b), s = a.length; d > l; l++) c = i, l !== f && (c = bt.clone(c,
                    !0, !0), s && bt.merge(a, p(c, "script"))), n.call(e[l], c, l);
                  if (s)
                    for (u = a[a.length - 1].ownerDocument, bt.map(a, E), l = 0; s > l; l++) c = a[l], be.test(c
                      .type || "") && !Q.access(c, "globalEval") && bt.contains(u, c) && (c.src ? bt._evalUrl &&
                      bt._evalUrl(c.src) : bt.globalEval(c.textContent.replace(Ne, "")))
                }
                return e
              }

              function T(e, t, n) {
                for (var r, i = t ? bt.filter(t, e) : e, o = 0; null != (r = i[o]); o++) n || 1 !== r.nodeType ||
                  bt.cleanData(p(r)), r.parentNode && (n && bt.contains(r.ownerDocument, r) && m(p(r, "script")),
                    r.parentNode.removeChild(r));
                return e
              }

              function C(e, t, n) {
                var r;
                if (void 0 === n && 1 === e.nodeType)
                  if (r = "data-" + t.replace(Oe, "-$&").toLowerCase(), n = e.getAttribute(r), "string" ==
                    typeof n) {
                    try {
                      n = "true" === n || "false" !== n && ("null" === n ? null : +n + "" === n ? +n : Ie.test(
                        n) ? bt.parseJSON(n) : n)
                    } catch (e) {}
                    $e.set(e, t, n)
                  } else n = void 0;
                return n
              }

              function x(e, t, n) {
                var r, i, o, a, s = e.style;
                return n = n || je(e), a = n ? n.getPropertyValue(t) || n[t] : void 0, "" !== a && void 0 !== a ||
                  bt.contains(e.ownerDocument, e) || (a = bt.style(e, t)), n && !gt.pixelMarginRight() && Le.test(
                    a) && Re.test(t) && (r = s.width, i = s.minWidth, o = s.maxWidth, s.minWidth = s.maxWidth = s
                    .width = a, a = n.width, s.width = r, s.minWidth = i, s.maxWidth = o), void 0 !== a ? a + "" :
                  a
              }

              function S(e, t, n, r) {
                var i, o = 1,
                  a = 20,
                  s = r ? function() {
                    return r.cur()
                  } : function() {
                    return bt.css(e, t, "")
                  },
                  c = s(),
                  u = n && n[3] || (bt.cssNumber[t] ? "" : "px"),
                  l = (bt.cssNumber[t] || "px" !== u && +c) && Pe.exec(bt.css(e, t));
                if (l && l[3] !== u) {
                  u = u || l[3], n = n || [], l = +c || 1;
                  do o = o || ".5", l /= o, bt.style(e, t, l + u); while (o !== (o = s() / c) && 1 !== o && --a)
                }
                return n && (l = +l || +c || 0, i = n[1] ? l + (n[1] + 1) * n[2] : +n[2], r && (r.unit = u, r
                  .start = l, r.end = i)), i
              }

              function A(e, t) {
                var n = bt(t.createElement(e)).appendTo(t.body),
                  r = bt.css(n[0], "display");
                return n.detach(), r
              }

              function M(e) {
                var t = ut,
                  n = qe[e];
                return n || (n = A(e, t), "none" !== n && n || (ze = (ze || bt(
                    "<iframe frameborder='0' width='0' height='0'/>")).appendTo(t.documentElement), t = ze[0]
                  .contentDocument, t.write(), t.close(), n = A(e, t), ze.detach()), qe[e] = n), n
              }

              function k(e, t) {
                return {
                  get: function() {
                    return e() ? void delete this.get : (this.get = t).apply(this, arguments)
                  }
                }
              }

              function N() {
                ut.removeEventListener("DOMContentLoaded", N), e.removeEventListener("load", N), bt.ready()
              }

              function I(e) {
                if (e in Xe) return e;
                for (var t = e[0].toUpperCase() + e.slice(1), n = Ke.length; n--;)
                  if (e = Ke[n] + t, e in Xe) return e
              }

              function O(e, t, n) {
                var r = Pe.exec(t);
                return r ? Math.max(0, r[2] - (n || 0)) + (r[3] || "px") : t
              }

              function D(e, t, n, r, i) {
                for (var o = n === (r ? "border" : "content") ? 4 : "width" === t ? 1 : 0, a = 0; 4 > o; o += 2)
                  "margin" === n && (a += bt.css(e, n + Ue[o], !0, i)), r ? ("content" === n && (a -= bt.css(e,
                    "padding" + Ue[o], !0, i)), "margin" !== n && (a -= bt.css(e, "border" + Ue[o] + "Width", !
                    0, i))) : (a += bt.css(e, "padding" + Ue[o], !0, i), "padding" !== n && (a += bt.css(e,
                    "border" + Ue[o] + "Width", !0, i)));
                return a
              }

              function R(t, n, r) {
                var i = !0,
                  o = "width" === n ? t.offsetWidth : t.offsetHeight,
                  a = je(t),
                  s = "border-box" === bt.css(t, "boxSizing", !1, a);
                if (ut.msFullscreenElement && e.top !== e && t.getClientRects().length && (o = Math.round(100 * t
                    .getBoundingClientRect()[n])), 0 >= o || null == o) {
                  if (o = x(t, n, a), (0 > o || null == o) && (o = t.style[n]), Le.test(o)) return o;
                  i = s && (gt.boxSizingReliable() || o === t.style[n]), o = parseFloat(o) || 0
                }
                return o + D(t, n, r || (s ? "border" : "content"), i, a) + "px"
              }

              function P(e, t) {
                for (var n, r, i, o = [], a = 0, s = e.length; s > a; a++) r = e[a], r.style && (o[a] = Q.get(r,
                  "olddisplay"), n = r.style.display, t ? (o[a] || "none" !== n || (r.style.display = ""),
                  "" === r.style.display && Fe(r) && (o[a] = Q.access(r, "olddisplay", M(r.nodeName)))) : (i =
                  Fe(r), "none" === n && i || Q.set(r, "olddisplay", i ? n : bt.css(r, "display"))));
                for (a = 0; s > a; a++) r = e[a], r.style && (t && "none" !== r.style.display && "" !== r.style
                  .display || (r.style.display = t ? o[a] || "" : "none"));
                return e
              }

              function L(e) {
                return e.getAttribute && e.getAttribute("class") || ""
              }

              function U(e, t, n, r) {
                var i;
                if (bt.isArray(t)) bt.each(t, function(t, i) {
                  n || it.test(e) ? r(e, i) : U(e + "[" + ("object" == typeof i && null != i ? t : "") + "]",
                    i, n, r)
                });
                else if (n || "object" !== bt.type(t)) r(e, t);
                else
                  for (i in t) U(e + "[" + i + "]", t[i], n, r)
              }
              var F, j, H, B, z, q, G, V, W, Y, K, X, Q, J, Z, ee, te, ne, re, ie, oe, ae, se, ce, ue, le, de, fe,
                he, pe, me, ve, ge, ye, be, Ee, _e, $e, we, Te, Ce, xe, Se, Ae, Me, ke, Ne, Ie, Oe, De, Re, Pe,
                Le, Ue, Fe, je, He, Be, ze, qe, Ge, Ve, We, Ye, Ke, Xe, Qe, Je, Ze, et, tt, nt, rt, it, ot, at,
                st, ct = [],
                ut = e.document,
                lt = ct.slice,
                dt = ct.concat,
                ft = ct.push,
                ht = ct.indexOf,
                pt = {},
                mt = pt.toString,
                vt = pt.hasOwnProperty,
                gt = {},
                yt = "2.2.2-pre",
                bt = function(e, t) {
                  return new bt.fn.init(e, t)
                },
                Et = /^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g,
                _t = /^-ms-/,
                $t = /-([\da-z])/gi,
                wt = function(e, t) {
                  return t.toUpperCase()
                };
              return bt.fn = bt.prototype = {
                  jquery: yt,
                  constructor: bt,
                  selector: "",
                  length: 0,
                  toArray: function() {
                    return lt.call(this)
                  },
                  get: function(e) {
                    return null != e ? 0 > e ? this[e + this.length] : this[e] : lt.call(this)
                  },
                  pushStack: function(e) {
                    var t = bt.merge(this.constructor(), e);
                    return t.prevObject = this, t.context = this.context, t
                  },
                  each: function(e) {
                    return bt.each(this, e)
                  },
                  map: function(e) {
                    return this.pushStack(bt.map(this, function(t, n) {
                      return e.call(t, n, t)
                    }))
                  },
                  slice: function() {
                    return this.pushStack(lt.apply(this, arguments))
                  },
                  first: function() {
                    return this.eq(0)
                  },
                  last: function() {
                    return this.eq(-1)
                  },
                  eq: function(e) {
                    var t = this.length,
                      n = +e + (0 > e ? t : 0);
                    return this.pushStack(n >= 0 && t > n ? [this[n]] : [])
                  },
                  end: function() {
                    return this.prevObject || this.constructor()
                  },
                  push: ft,
                  sort: ct.sort,
                  splice: ct.splice
                }, bt.extend = bt.fn.extend = function() {
                  var e, t, n, r, i, o, a = arguments[0] || {},
                    s = 1,
                    c = arguments.length,
                    u = !1;
                  for ("boolean" == typeof a && (u = a, a = arguments[s] || {}, s++), "object" == typeof a || bt
                    .isFunction(a) || (a = {}), s === c && (a = this, s--); c > s; s++)
                    if (null != (e = arguments[s]))
                      for (t in e) n = a[t], r = e[t], a !== r && (u && r && (bt.isPlainObject(r) || (i = bt
                        .isArray(r))) ? (i ? (i = !1, o = n && bt.isArray(n) ? n : []) : o = n && bt
                        .isPlainObject(n) ? n : {}, a[t] = bt.extend(u, o, r)) : void 0 !== r && (a[t] = r));
                  return a
                }, bt.extend({
                  expando: "ATJS" + (yt + Math.random()).replace(/\D/g, ""),
                  isReady: !0,
                  error: function(e) {
                    throw Error(e)
                  },
                  noop: function() {},
                  isFunction: function(e) {
                    return "function" === bt.type(e)
                  },
                  isArray: Array.isArray,
                  isWindow: function(e) {
                    return null != e && e === e.window
                  },
                  isNumeric: function(e) {
                    var t = e && "" + e;
                    return !bt.isArray(e) && t - parseFloat(t) + 1 >= 0
                  },
                  isPlainObject: function(e) {
                    var t;
                    if ("object" !== bt.type(e) || e.nodeType || bt.isWindow(e)) return !1;
                    if (e.constructor && !vt.call(e.constructor.prototype, "isPrototypeOf")) return !1;
                    for (t in e);
                    return void 0 === t || vt.call(e, t)
                  },
                  isEmptyObject: function(e) {
                    var t;
                    for (t in e) return !1;
                    return !0
                  },
                  type: function(e) {
                    return null == e ? e + "" : "object" == typeof e || "function" == typeof e ? pt[mt.call(
                      e)] || "object" : typeof e
                  },
                  globalEval: function(e) {
                    var t, n = eval;
                    e = bt.trim(e), e && (1 === e.indexOf("use strict") ? (t = ut.createElement("script"), t
                      .text = e, ut.head.appendChild(t).parentNode.removeChild(t)) : n(e))
                  },
                  camelCase: function(e) {
                    return e.replace(_t, "ms-").replace($t, wt)
                  },
                  nodeName: function(e, t) {
                    return e.nodeName && e.nodeName.toLowerCase() === t.toLowerCase()
                  },
                  each: function(e, n) {
                    var r, i = 0;
                    if (t(e))
                      for (r = e.length; r > i && n.call(e[i], i, e[i]) !== !1; i++);
                    else
                      for (i in e)
                        if (n.call(e[i], i, e[i]) === !1) break;
                    return e
                  },
                  trim: function(e) {
                    return null == e ? "" : (e + "").replace(Et, "")
                  },
                  makeArray: function(e, n) {
                    var r = n || [];
                    return null != e && (t(Object(e)) ? bt.merge(r, "string" == typeof e ? [e] : e) : ft.call(
                      r, e)), r
                  },
                  inArray: function(e, t, n) {
                    return null == t ? -1 : ht.call(t, e, n)
                  },
                  merge: function(e, t) {
                    for (var n = +t.length, r = 0, i = e.length; n > r; r++) e[i++] = t[r];
                    return e.length = i, e
                  },
                  grep: function(e, t, n) {
                    for (var r, i = [], o = 0, a = e.length, s = !n; a > o; o++) r = !t(e[o], o), r !== s && i
                      .push(e[o]);
                    return i
                  },
                  map: function(e, n, r) {
                    var i, o, a = 0,
                      s = [];
                    if (t(e))
                      for (i = e.length; i > a; a++) o = n(e[a], a, r), null != o && s.push(o);
                    else
                      for (a in e) o = n(e[a], a, r), null != o && s.push(o);
                    return dt.apply([], s)
                  },
                  guid: 1,
                  proxy: function e(t, n) {
                    var r, i, e;
                    return "string" == typeof n && (r = t[n], n = t, t = r), bt.isFunction(t) ? (i = lt.call(
                      arguments, 2), e = function() {
                      return t.apply(n || this, i.concat(lt.call(arguments)))
                    }, e.guid = t.guid = t.guid || bt.guid++, e) : void 0
                  },
                  now: Date.now,
                  support: gt
                }), "function" == typeof Symbol && (bt.fn[Symbol.iterator] = ct[Symbol.iterator]), bt.each(
                  "Boolean Number String Function Array Date RegExp Object Error Symbol".split(" "),
                  function(e, t) {
                    pt["[object " + t + "]"] = t.toLowerCase()
                  }), F = /^<([\w-]+)\s*\/?>(?:<\/\1>|)$/, j = function(e) {
                  function t(e, t, n, r) {
                    var i, o, a, s, c, u, d, h, p = t && t.ownerDocument,
                      m = t ? t.nodeType : 9;
                    if (n = n || [], "string" != typeof e || !e || 1 !== m && 9 !== m && 11 !== m) return n;
                    if (!r && ((t ? t.ownerDocument || t : j) !== I && N(t), t = t || I, D)) {
                      if (11 !== m && (u = ge.exec(e)))
                        if (i = u[1]) {
                          if (9 === m) {
                            if (!(a = t.getElementById(i))) return n;
                            if (a.id === i) return n.push(a), n
                          } else if (p && (a = p.getElementById(i)) && U(t, a) && a.id === i) return n.push(a), n
                        } else {
                          if (u[2]) return J.apply(n, t.getElementsByTagName(e)), n;
                          if ((i = u[3]) && _.getElementsByClassName && t.getElementsByClassName) return J.apply(
                            n, t.getElementsByClassName(i)), n
                        } if (_.qsa && !G[e + " "] && (!R || !R.test(e))) {
                        if (1 !== m) p = t, h = e;
                        else if ("object" !== t.nodeName.toLowerCase()) {
                          for ((s = t.getAttribute("id")) ? s = s.replace(be, "\\$&") : t.setAttribute("id", s =
                              F), d = C(e), o = d.length, c = fe.test(s) ? "#" + s : "[id='" + s + "']"; o--;) d[
                            o] = c + " " + f(d[o]);
                          h = d.join(","), p = ye.test(e) && l(t.parentNode) || t
                        }
                        if (h) try {
                          return J.apply(n, p.querySelectorAll(h)), n
                        } catch (e) {} finally {
                          s === F && t.removeAttribute("id")
                        }
                      }
                    }
                    return S(e.replace(se, "$1"), t, n, r)
                  }

                  function n() {
                    function e(n, r) {
                      return t.push(n + " ") > $.cacheLength && delete e[t.shift()], e[n + " "] = r
                    }
                    var t = [];
                    return e
                  }

                  function r(e) {
                    return e[F] = !0, e
                  }

                  function i(e) {
                    var t = I.createElement("div");
                    try {
                      return !!e(t)
                    } catch (e) {
                      return !1
                    } finally {
                      t.parentNode && t.parentNode.removeChild(t), t = null
                    }
                  }

                  function o(e, t) {
                    for (var n = e.split("|"), r = n.length; r--;) $.attrHandle[n[r]] = t
                  }

                  function a(e, t) {
                    var n = t && e,
                      r = n && 1 === e.nodeType && 1 === t.nodeType && (~t.sourceIndex || W) - (~e.sourceIndex ||
                        W);
                    if (r) return r;
                    if (n)
                      for (; n = n.nextSibling;)
                        if (n === t) return -1;
                    return e ? 1 : -1
                  }

                  function s(e) {
                    return function(t) {
                      var n = t.nodeName.toLowerCase();
                      return "input" === n && t.type === e
                    }
                  }

                  function c(e) {
                    return function(t) {
                      var n = t.nodeName.toLowerCase();
                      return ("input" === n || "button" === n) && t.type === e
                    }
                  }

                  function u(e) {
                    return r(function(t) {
                      return t = +t, r(function(n, r) {
                        for (var i, o = e([], n.length, t), a = o.length; a--;) n[i = o[a]] && (n[i] = !(
                          r[i] = n[i]))
                      })
                    })
                  }

                  function l(e) {
                    return e && void 0 !== e.getElementsByTagName && e
                  }

                  function d() {}

                  function f(e) {
                    for (var t = 0, n = e.length, r = ""; n > t; t++) r += e[t].value;
                    return r
                  }

                  function h(e, t, n) {
                    var r = t.dir,
                      i = n && "parentNode" === r,
                      o = B++;
                    return t.first ? function(t, n, o) {
                      for (; t = t[r];)
                        if (1 === t.nodeType || i) return e(t, n, o)
                    } : function(t, n, a) {
                      var s, c, u, l = [H, o];
                      if (a) {
                        for (; t = t[r];)
                          if ((1 === t.nodeType || i) && e(t, n, a)) return !0
                      } else
                        for (; t = t[r];)
                          if (1 === t.nodeType || i) {
                            if (u = t[F] || (t[F] = {}), c = u[t.uniqueID] || (u[t.uniqueID] = {}), (s = c[
                              r]) && s[0] === H && s[1] === o) return l[2] = s[2];
                            if (c[r] = l, l[2] = e(t, n, a)) return !0
                          }
                    }
                  }

                  function p(e) {
                    return e.length > 1 ? function(t, n, r) {
                      for (var i = e.length; i--;)
                        if (!e[i](t, n, r)) return !1;
                      return !0
                    } : e[0]
                  }

                  function m(e, n, r) {
                    for (var i = 0, o = n.length; o > i; i++) t(e, n[i], r);
                    return r
                  }

                  function v(e, t, n, r, i) {
                    for (var o, a = [], s = 0, c = e.length, u = null != t; c > s; s++)(o = e[s]) && (!n || n(o,
                      r, i)) && (a.push(o), u && t.push(s));
                    return a
                  }

                  function g(e, t, n, i, o, a) {
                    return i && !i[F] && (i = g(i)), o && !o[F] && (o = g(o, a)), r(function(r, a, s, c) {
                      var u, l, d, f = [],
                        h = [],
                        p = a.length,
                        g = r || m(t || "*", s.nodeType ? [s] : s, []),
                        y = !e || !r && t ? g : v(g, f, e, s, c),
                        b = n ? o || (r ? e : p || i) ? [] : a : y;
                      if (n && n(y, b, s, c), i)
                        for (u = v(b, h), i(u, [], s, c), l = u.length; l--;)(d = u[l]) && (b[h[l]] = !(y[h[
                          l]] = d));
                      if (r) {
                        if (o || e) {
                          if (o) {
                            for (u = [], l = b.length; l--;)(d = b[l]) && u.push(y[l] = d);
                            o(null, b = [], u, c)
                          }
                          for (l = b.length; l--;)(d = b[l]) && (u = o ? ee(r, d) : f[l]) > -1 && (r[u] = !(a[
                            u] = d))
                        }
                      } else b = v(b === a ? b.splice(p, b.length) : b), o ? o(null, a, b, c) : J.apply(a, b)
                    })
                  }

                  function y(e) {
                    for (var t, n, r, i = e.length, o = $.relative[e[0].type], a = o || $.relative[" "], s = o ?
                        1 : 0, c = h(function(e) {
                          return e === t
                        }, a, !0), u = h(function(e) {
                          return ee(t, e) > -1
                        }, a, !0), l = [function(e, n, r) {
                          var i = !o && (r || n !== A) || ((t = n).nodeType ? c(e, n, r) : u(e, n, r));
                          return t = null, i
                        }]; i > s; s++)
                      if (n = $.relative[e[s].type]) l = [h(p(l), n)];
                      else {
                        if (n = $.filter[e[s].type].apply(null, e[s].matches), n[F]) {
                          for (r = ++s; i > r && !$.relative[e[r].type]; r++);
                          return g(s > 1 && p(l), s > 1 && f(e.slice(0, s - 1).concat({
                              value: " " === e[s - 2].type ? "*" : ""
                            })).replace(se, "$1"), n, r > s && y(e.slice(s, r)), i > r && y(e = e.slice(r)), i >
                            r && f(e))
                        }
                        l.push(n)
                      } return p(l)
                  }

                  function b(e, n) {
                    var i = n.length > 0,
                      o = e.length > 0,
                      a = function(r, a, s, c, u) {
                        var l, d, f, h = 0,
                          p = "0",
                          m = r && [],
                          g = [],
                          y = A,
                          b = r || o && $.find.TAG("*", u),
                          E = H += null == y ? 1 : Math.random() || .1,
                          _ = b.length;
                        for (u && (A = a === I || a || u); p !== _ && null != (l = b[p]); p++) {
                          if (o && l) {
                            for (d = 0, a || l.ownerDocument === I || (N(l), s = !D); f = e[d++];)
                              if (f(l, a || I, s)) {
                                c.push(l);
                                break
                              } u && (H = E)
                          }
                          i && ((l = !f && l) && h--, r && m.push(l))
                        }
                        if (h += p, i && p !== h) {
                          for (d = 0; f = n[d++];) f(m, g, a, s);
                          if (r) {
                            if (h > 0)
                              for (; p--;) m[p] || g[p] || (g[p] = X.call(c));
                            g = v(g)
                          }
                          J.apply(c, g), u && !r && g.length > 0 && h + n.length > 1 && t.uniqueSort(c)
                        }
                        return u && (H = E, A = y), m
                      };
                    return i ? r(a) : a
                  }
                  var E, _, $, w, T, C, x, S, A, M, k, N, I, O, D, R, P, L, U, F = "sizzle" + 1 * new Date,
                    j = e.document,
                    H = 0,
                    B = 0,
                    z = n(),
                    q = n(),
                    G = n(),
                    V = function(e, t) {
                      return e === t && (k = !0), 0
                    },
                    W = 1 << 31,
                    Y = {}.hasOwnProperty,
                    K = [],
                    X = K.pop,
                    Q = K.push,
                    J = K.push,
                    Z = K.slice,
                    ee = function(e, t) {
                      for (var n = 0, r = e.length; r > n; n++)
                        if (e[n] === t) return n;
                      return -1
                    },
                    te =
                    "checked|selected|async|autofocus|autoplay|controls|defer|disabled|hidden|ismap|loop|multiple|open|readonly|required|scoped",
                    ne = "[\\x20\\t\\r\\n\\f]",
                    re = "(?:\\\\.|[\\w-]|[^\\x00-\\xa0])+",
                    ie = "\\[" + ne + "*(" + re + ")(?:" + ne + "*([*^$|!~]?=)" + ne +
                    "*(?:'((?:\\\\.|[^\\\\'])*)'|\"((?:\\\\.|[^\\\\\"])*)\"|(" + re + "))|)" + ne + "*\\]",
                    oe = ":(" + re +
                    ")(?:\\((('((?:\\\\.|[^\\\\'])*)'|\"((?:\\\\.|[^\\\\\"])*)\")|((?:\\\\.|[^\\\\()[\\]]|" + ie +
                    ")*)|.*)\\)|)",
                    ae = RegExp(ne + "+", "g"),
                    se = RegExp("^" + ne + "+|((?:^|[^\\\\])(?:\\\\.)*)" + ne + "+$", "g"),
                    ce = RegExp("^" + ne + "*," + ne + "*"),
                    ue = RegExp("^" + ne + "*([>+~]|" + ne + ")" + ne + "*"),
                    le = RegExp("=" + ne + "*([^\\]'\"]*?)" + ne + "*\\]", "g"),
                    de = RegExp(oe),
                    fe = RegExp("^" + re + "$"),
                    he = {
                      ID: RegExp("^#(" + re + ")"),
                      CLASS: RegExp("^\\.(" + re + ")"),
                      TAG: RegExp("^(" + re + "|[*])"),
                      ATTR: RegExp("^" + ie),
                      PSEUDO: RegExp("^" + oe),
                      CHILD: RegExp("^:(only|first|last|nth|nth-last)-(child|of-type)(?:\\(" + ne +
                        "*(even|odd|(([+-]|)(\\d*)n|)" + ne + "*(?:([+-]|)" + ne + "*(\\d+)|))" + ne + "*\\)|)",
                        "i"),
                      bool: RegExp("^(?:" + te + ")$", "i"),
                      needsContext: RegExp("^" + ne + "*[>+~]|:(even|odd|eq|gt|lt|nth|first|last)(?:\\(" + ne +
                        "*((?:-\\d)?\\d*)" + ne + "*\\)|)(?=[^-]|$)", "i")
                    },
                    pe = /^(?:input|select|textarea|button)$/i,
                    me = /^h\d$/i,
                    ve = /^[^{]+\{\s*\[native \w/,
                    ge = /^(?:#([\w-]+)|(\w+)|\.([\w-]+))$/,
                    ye = /[+~]/,
                    be = /'|\\/g,
                    Ee = RegExp("\\\\([\\da-f]{1,6}" + ne + "?|(" + ne + ")|.)", "ig"),
                    _e = function(e, t, n) {
                      var r = "0x" + t - 65536;
                      return r !== r || n ? t : 0 > r ? String.fromCharCode(r + 65536) : String.fromCharCode(r >>
                        10 | 55296, 1023 & r | 56320)
                    },
                    $e = function() {
                      N()
                    };
                  try {
                    J.apply(K = Z.call(j.childNodes), j.childNodes), K[j.childNodes.length].nodeType
                  } catch (e) {
                    J = {
                      apply: K.length ? function(e, t) {
                        Q.apply(e, Z.call(t))
                      } : function(e, t) {
                        for (var n = e.length, r = 0; e[n++] = t[r++];);
                        e.length = n - 1
                      }
                    }
                  }
                  _ = t.support = {}, T = t.isXML = function(e) {
                    var t = e && (e.ownerDocument || e).documentElement;
                    return !!t && "HTML" !== t.nodeName
                  }, N = t.setDocument = function(e) {
                    var t, n, r = e ? e.ownerDocument || e : j;
                    return r !== I && 9 === r.nodeType && r.documentElement ? (I = r, O = I.documentElement,
                      D = !T(I), (n = I.defaultView) && n.top !== n && (n.addEventListener ? n
                        .addEventListener("unload", $e, !1) : n.attachEvent && n.attachEvent("onunload", $e)),
                      _.attributes = i(function(e) {
                        return e.className = "i", !e.getAttribute("className")
                      }), _.getElementsByTagName = i(function(e) {
                        return e.appendChild(I.createComment("")), !e.getElementsByTagName("*").length
                      }), _.getElementsByClassName = ve.test(I.getElementsByClassName), _.getById = i(
                        function(e) {
                          return O.appendChild(e).id = F, !I.getElementsByName || !I.getElementsByName(F)
                            .length
                        }), _.getById ? ($.find.ID = function(e, t) {
                        if (void 0 !== t.getElementById && D) {
                          var n = t.getElementById(e);
                          return n ? [n] : []
                        }
                      }, $.filter.ID = function(e) {
                        var t = e.replace(Ee, _e);
                        return function(e) {
                          return e.getAttribute("id") === t
                        }
                      }) : (delete $.find.ID, $.filter.ID = function(e) {
                        var t = e.replace(Ee, _e);
                        return function(e) {
                          var n = void 0 !== e.getAttributeNode && e.getAttributeNode("id");
                          return n && n.value === t
                        }
                      }), $.find.TAG = _.getElementsByTagName ? function(e, t) {
                        return void 0 !== t.getElementsByTagName ? t.getElementsByTagName(e) : _.qsa ? t
                          .querySelectorAll(e) : void 0
                      } : function(e, t) {
                        var n, r = [],
                          i = 0,
                          o = t.getElementsByTagName(e);
                        if ("*" === e) {
                          for (; n = o[i++];) 1 === n.nodeType && r.push(n);
                          return r
                        }
                        return o
                      }, $.find.CLASS = _.getElementsByClassName && function(e, t) {
                        return void 0 !== t.getElementsByClassName && D ? t.getElementsByClassName(e) : void 0
                      }, P = [], R = [], (_.qsa = ve.test(I.querySelectorAll)) && (i(function(e) {
                        O.appendChild(e).innerHTML = "<a id='" + F + "'></a><select id='" + F +
                          "-\r\\' msallowcapture=''><option selected=''></option></select>", e
                          .querySelectorAll("[msallowcapture^='']").length && R.push("[*^$]=" + ne +
                            "*(?:''|\"\")"), e.querySelectorAll("[selected]").length || R.push("\\[" +
                            ne + "*(?:value|" + te + ")"), e.querySelectorAll("[id~=" + F + "-]")
                          .length || R.push("~="), e.querySelectorAll(":checked").length || R.push(
                            ":checked"), e.querySelectorAll("a#" + F + "+*").length || R.push(".#.+[+~]")
                      }), i(function(e) {
                        var t = I.createElement("input");
                        t.setAttribute("type", "hidden"), e.appendChild(t).setAttribute("name", "D"), e
                          .querySelectorAll("[name=d]").length && R.push("name" + ne + "*[*^$|!~]?="), e
                          .querySelectorAll(":enabled").length || R.push(":enabled", ":disabled"), e
                          .querySelectorAll("*,:x"), R.push(",.*:")
                      })), (_.matchesSelector = ve.test(L = O.matches || O.webkitMatchesSelector || O
                        .mozMatchesSelector || O.oMatchesSelector || O.msMatchesSelector)) && i(function(e) {
                        _.disconnectedMatch = L.call(e, "div"), L.call(e, "[s!='']:x"), P.push("!=", oe)
                      }), R = R.length && RegExp(R.join("|")), P = P.length && RegExp(P.join("|")), t = ve
                      .test(O.compareDocumentPosition), U = t || ve.test(O.contains) ? function(e, t) {
                        var n = 9 === e.nodeType ? e.documentElement : e,
                          r = t && t.parentNode;
                        return e === r || !(!r || 1 !== r.nodeType || !(n.contains ? n.contains(r) : e
                          .compareDocumentPosition && 16 & e.compareDocumentPosition(r)))
                      } : function(e, t) {
                        if (t)
                          for (; t = t.parentNode;)
                            if (t === e) return !0;
                        return !1
                      }, V = t ? function(e, t) {
                        if (e === t) return k = !0, 0;
                        var n = !e.compareDocumentPosition - !t.compareDocumentPosition;
                        return n ? n : (n = (e.ownerDocument || e) === (t.ownerDocument || t) ? e
                          .compareDocumentPosition(t) : 1, 1 & n || !_.sortDetached && t
                          .compareDocumentPosition(e) === n ? e === I || e.ownerDocument === j && U(j, e) ?
                          -1 : t === I || t.ownerDocument === j && U(j, t) ? 1 : M ? ee(M, e) - ee(M, t) :
                          0 : 4 & n ? -1 : 1)
                      } : function(e, t) {
                        if (e === t) return k = !0, 0;
                        var n, r = 0,
                          i = e.parentNode,
                          o = t.parentNode,
                          s = [e],
                          c = [t];
                        if (!i || !o) return e === I ? -1 : t === I ? 1 : i ? -1 : o ? 1 : M ? ee(M, e) - ee(
                          M, t) : 0;
                        if (i === o) return a(e, t);
                        for (n = e; n = n.parentNode;) s.unshift(n);
                        for (n = t; n = n.parentNode;) c.unshift(n);
                        for (; s[r] === c[r];) r++;
                        return r ? a(s[r], c[r]) : s[r] === j ? -1 : c[r] === j ? 1 : 0
                      }, I) : I
                  }, t.matches = function(e, n) {
                    return t(e, null, null, n)
                  }, t.matchesSelector = function(e, n) {
                    if ((e.ownerDocument || e) !== I && N(e), n = n.replace(le, "='$1']"), _.matchesSelector &&
                      D && !G[n + " "] && (!P || !P.test(n)) && (!R || !R.test(n))) try {
                      var r = L.call(e, n);
                      if (r || _.disconnectedMatch || e.document && 11 !== e.document.nodeType) return r
                    } catch (e) {}
                    return t(n, I, null, [e]).length > 0
                  }, t.contains = function(e, t) {
                    return (e.ownerDocument || e) !== I && N(e), U(e, t)
                  }, t.attr = function(e, t) {
                    (e.ownerDocument || e) !== I && N(e);
                    var n = $.attrHandle[t.toLowerCase()],
                      r = n && Y.call($.attrHandle, t.toLowerCase()) ? n(e, t, !D) : void 0;
                    return void 0 !== r ? r : _.attributes || !D ? e.getAttribute(t) : (r = e.getAttributeNode(
                      t)) && r.specified ? r.value : null
                  }, t.error = function(e) {
                    throw Error("Syntax error, unrecognized expression: " + e)
                  }, t.uniqueSort = function(e) {
                    var t, n = [],
                      r = 0,
                      i = 0;
                    if (k = !_.detectDuplicates, M = !_.sortStable && e.slice(0), e.sort(V), k) {
                      for (; t = e[i++];) t === e[i] && (r = n.push(i));
                      for (; r--;) e.splice(n[r], 1)
                    }
                    return M = null, e
                  }, w = t.getText = function(e) {
                    var t, n = "",
                      r = 0,
                      i = e.nodeType;
                    if (i) {
                      if (1 === i || 9 === i || 11 === i) {
                        if ("string" == typeof e.textContent) return e.textContent;
                        for (e = e.firstChild; e; e = e.nextSibling) n += w(e)
                      } else if (3 === i || 4 === i) return e.nodeValue
                    } else
                      for (; t = e[r++];) n += w(t);
                    return n
                  }, $ = t.selectors = {
                    cacheLength: 50,
                    createPseudo: r,
                    match: he,
                    attrHandle: {},
                    find: {},
                    relative: {
                      ">": {
                        dir: "parentNode",
                        first: !0
                      },
                      " ": {
                        dir: "parentNode"
                      },
                      "+": {
                        dir: "previousSibling",
                        first: !0
                      },
                      "~": {
                        dir: "previousSibling"
                      }
                    },
                    preFilter: {
                      ATTR: function(e) {
                        return e[1] = e[1].replace(Ee, _e), e[3] = (e[3] || e[4] || e[5] || "").replace(Ee,
                          _e), "~=" === e[2] && (e[3] = " " + e[3] + " "), e.slice(0, 4)
                      },
                      CHILD: function(e) {
                        return e[1] = e[1].toLowerCase(), "nth" === e[1].slice(0, 3) ? (e[3] || t.error(e[0]),
                          e[4] = +(e[4] ? e[5] + (e[6] || 1) : 2 * ("even" === e[3] || "odd" === e[3])), e[
                            5] = +(e[7] + e[8] || "odd" === e[3])) : e[3] && t.error(e[0]), e
                      },
                      PSEUDO: function(e) {
                        var t, n = !e[6] && e[2];
                        return he.CHILD.test(e[0]) ? null : (e[3] ? e[2] = e[4] || e[5] || "" : n && de.test(
                          n) && (t = C(n, !0)) && (t = n.indexOf(")", n.length - t) - n.length) && (e[0] =
                          e[0].slice(0, t), e[2] = n.slice(0, t)), e.slice(0, 3))
                      }
                    },
                    filter: {
                      TAG: function(e) {
                        var t = e.replace(Ee, _e).toLowerCase();
                        return "*" === e ? function() {
                          return !0
                        } : function(e) {
                          return e.nodeName && e.nodeName.toLowerCase() === t
                        }
                      },
                      CLASS: function(e) {
                        var t = z[e + " "];
                        return t || (t = RegExp("(^|" + ne + ")" + e + "(" + ne + "|$)")) && z(e, function(
                        e) {
                          return t.test("string" == typeof e.className && e.className || void 0 !== e
                            .getAttribute && e.getAttribute("class") || "")
                        })
                      },
                      ATTR: function(e, n, r) {
                        return function(i) {
                          var o = t.attr(i, e);
                          return null == o ? "!=" === n : !n || (o += "", "=" === n ? o === r : "!=" === n ?
                            o !== r : "^=" === n ? r && 0 === o.indexOf(r) : "*=" === n ? r && o.indexOf(
                              r) > -1 : "$=" === n ? r && o.slice(-r.length) === r : "~=" === n ? (" " + o
                              .replace(ae, " ") + " ").indexOf(r) > -1 : "|=" === n && (o === r || o
                              .slice(0, r.length + 1) === r + "-"))
                        }
                      },
                      CHILD: function(e, t, n, r, i) {
                        var o = "nth" !== e.slice(0, 3),
                          a = "last" !== e.slice(-4),
                          s = "of-type" === t;
                        return 1 === r && 0 === i ? function(e) {
                          return !!e.parentNode
                        } : function(t, n, c) {
                          var u, l, d, f, h, p, m = o !== a ? "nextSibling" : "previousSibling",
                            v = t.parentNode,
                            g = s && t.nodeName.toLowerCase(),
                            y = !c && !s,
                            b = !1;
                          if (v) {
                            if (o) {
                              for (; m;) {
                                for (f = t; f = f[m];)
                                  if (s ? f.nodeName.toLowerCase() === g : 1 === f.nodeType) return !1;
                                p = m = "only" === e && !p && "nextSibling"
                              }
                              return !0
                            }
                            if (p = [a ? v.firstChild : v.lastChild], a && y) {
                              for (f = v, d = f[F] || (f[F] = {}), l = d[f.uniqueID] || (d[f
                                .uniqueID] = {}), u = l[e] || [], h = u[0] === H && u[1], b = h && u[2], f =
                                h && v.childNodes[h]; f = ++h && f && f[m] || (b = h = 0) || p.pop();)
                                if (1 === f.nodeType && ++b && f === t) {
                                  l[e] = [H, h, b];
                                  break
                                }
                            } else if (y && (f = t, d = f[F] || (f[F] = {}), l = d[f.uniqueID] || (d[f
                                .uniqueID] = {}), u = l[e] || [], h = u[0] === H && u[1], b = h), b === !1)
                              for (;
                                (f = ++h && f && f[m] || (b = h = 0) || p.pop()) && ((s ? f.nodeName
                                  .toLowerCase() !== g : 1 !== f.nodeType) || !++b || (y && (d = f[F] || (
                                    f[F] = {}), l = d[f.uniqueID] || (d[f.uniqueID] = {}), l[e] = [H,
                                  b]), f !== t)););
                            return b -= i, b === r || b % r === 0 && b / r >= 0
                          }
                        }
                      },
                      PSEUDO: function(e, n) {
                        var i, o = $.pseudos[e] || $.setFilters[e.toLowerCase()] || t.error(
                          "unsupported pseudo: " + e);
                        return o[F] ? o(n) : o.length > 1 ? (i = [e, e, "", n], $.setFilters.hasOwnProperty(e
                          .toLowerCase()) ? r(function(e, t) {
                          for (var r, i = o(e, n), a = i.length; a--;) r = ee(e, i[a]), e[r] = !(t[r] =
                            i[a])
                        }) : function(e) {
                          return o(e, 0, i)
                        }) : o
                      }
                    },
                    pseudos: {
                      not: r(function(e) {
                        var t = [],
                          n = [],
                          i = x(e.replace(se, "$1"));
                        return i[F] ? r(function(e, t, n, r) {
                          for (var o, a = i(e, null, r, []), s = e.length; s--;)(o = a[s]) && (e[s] = !(
                            t[s] = o))
                        }) : function(e, r, o) {
                          return t[0] = e, i(t, null, o, n), t[0] = null, !n.pop()
                        }
                      }),
                      has: r(function(e) {
                        return function(n) {
                          return t(e, n).length > 0
                        }
                      }),
                      contains: r(function(e) {
                        return e = e.replace(Ee, _e),
                          function(t) {
                            return (t.textContent || t.innerText || w(t)).indexOf(e) > -1
                          }
                      }),
                      lang: r(function(e) {
                        return fe.test(e || "") || t.error("unsupported lang: " + e), e = e.replace(Ee, _e)
                          .toLowerCase(),
                          function(t) {
                            var n;
                            do
                              if (n = D ? t.lang : t.getAttribute("xml:lang") || t.getAttribute("lang"))
                                return n = n.toLowerCase(), n === e || 0 === n.indexOf(e + "-"); while ((t =
                                t.parentNode) && 1 === t.nodeType);
                            return !1
                          }
                      }),
                      target: function(t) {
                        var n = e.location && e.location.hash;
                        return n && n.slice(1) === t.id
                      },
                      root: function(e) {
                        return e === O
                      },
                      focus: function(e) {
                        return e === I.activeElement && (!I.hasFocus || I.hasFocus()) && !!(e.type || e
                          .href || ~e.tabIndex)
                      },
                      enabled: function(e) {
                        return e.disabled === !1
                      },
                      disabled: function(e) {
                        return e.disabled === !0
                      },
                      checked: function(e) {
                        var t = e.nodeName.toLowerCase();
                        return "input" === t && !!e.checked || "option" === t && !!e.selected
                      },
                      selected: function(e) {
                        return e.parentNode && e.parentNode.selectedIndex, e.selected === !0
                      },
                      empty: function(e) {
                        for (e = e.firstChild; e; e = e.nextSibling)
                          if (e.nodeType < 6) return !1;
                        return !0
                      },
                      parent: function(e) {
                        return !$.pseudos.empty(e)
                      },
                      header: function(e) {
                        return me.test(e.nodeName)
                      },
                      input: function(e) {
                        return pe.test(e.nodeName)
                      },
                      button: function(e) {
                        var t = e.nodeName.toLowerCase();
                        return "input" === t && "button" === e.type || "button" === t
                      },
                      text: function(e) {
                        var t;
                        return "input" === e.nodeName.toLowerCase() && "text" === e.type && (null == (t = e
                          .getAttribute("type")) || "text" === t.toLowerCase())
                      },
                      first: u(function() {
                        return [0]
                      }),
                      last: u(function(e, t) {
                        return [t - 1]
                      }),
                      eq: u(function(e, t, n) {
                        return [0 > n ? n + t : n]
                      }),
                      even: u(function(e, t) {
                        for (var n = 0; t > n; n += 2) e.push(n);
                        return e
                      }),
                      odd: u(function(e, t) {
                        for (var n = 1; t > n; n += 2) e.push(n);
                        return e
                      }),
                      lt: u(function(e, t, n) {
                        for (var r = 0 > n ? n + t : n; --r >= 0;) e.push(r);
                        return e
                      }),
                      gt: u(function(e, t, n) {
                        for (var r = 0 > n ? n + t : n; ++r < t;) e.push(r);
                        return e
                      })
                    }
                  }, $.pseudos.nth = $.pseudos.eq;
                  for (E in {
                      radio: !0,
                      checkbox: !0,
                      file: !0,
                      password: !0,
                      image: !0
                    }) $.pseudos[E] = s(E);
                  for (E in {
                      submit: !0,
                      reset: !0
                    }) $.pseudos[E] = c(E);
                  return d.prototype = $.filters = $.pseudos, $.setFilters = new d, C = t.tokenize = function(e,
                      n) {
                      var r, i, o, a, s, c, u, l = q[e + " "];
                      if (l) return n ? 0 : l.slice(0);
                      for (s = e, c = [], u = $.preFilter; s;) {
                        (!r || (i = ce.exec(s))) && (i && (s = s.slice(i[0].length) || s), c.push(o = [])), r = !
                          1, (i = ue.exec(s)) && (r = i.shift(), o.push({
                            value: r,
                            type: i[0].replace(se, " ")
                          }), s = s.slice(r.length));
                        for (a in $.filter) !(i = he[a].exec(s)) || u[a] && !(i = u[a](i)) || (r = i.shift(), o
                          .push({
                            value: r,
                            type: a,
                            matches: i
                          }), s = s.slice(r.length));
                        if (!r) break
                      }
                      return n ? s.length : s ? t.error(e) : q(e, c).slice(0)
                    }, x = t.compile = function(e, t) {
                      var n, r = [],
                        i = [],
                        o = G[e + " "];
                      if (!o) {
                        for (t || (t = C(e)), n = t.length; n--;) o = y(t[n]), o[F] ? r.push(o) : i.push(o);
                        o = G(e, b(i, r)), o.selector = e
                      }
                      return o
                    }, S = t.select = function(e, t, n, r) {
                      var i, o, a, s, c, u = "function" == typeof e && e,
                        d = !r && C(e = u.selector || e);
                      if (n = n || [], 1 === d.length) {
                        if (o = d[0] = d[0].slice(0), o.length > 2 && "ID" === (a = o[0]).type && _.getById &&
                          9 === t.nodeType && D && $.relative[o[1].type]) {
                          if (t = ($.find.ID(a.matches[0].replace(Ee, _e), t) || [])[0], !t) return n;
                          u && (t = t.parentNode), e = e.slice(o.shift().value.length)
                        }
                        for (i = he.needsContext.test(e) ? 0 : o.length; i-- && (a = o[i], !$.relative[s = a
                            .type]);)
                          if ((c = $.find[s]) && (r = c(a.matches[0].replace(Ee, _e), ye.test(o[0].type) && l(t
                              .parentNode) || t))) {
                            if (o.splice(i, 1), e = r.length && f(o), !e) return J.apply(n, r), n;
                            break
                          }
                      }
                      return (u || x(e, d))(r, t, !D, n, !t || ye.test(e) && l(t.parentNode) || t), n
                    }, _.sortStable = F.split("").sort(V).join("") === F, _.detectDuplicates = !!k, N(), _
                    .sortDetached = i(function(e) {
                      return 1 & e.compareDocumentPosition(I.createElement("div"))
                    }), i(function(e) {
                      return e.innerHTML = "<a href='#'></a>", "#" === e.firstChild.getAttribute("href")
                    }) || o("type|href|height|width", function(e, t, n) {
                      return n ? void 0 : e.getAttribute(t, "type" === t.toLowerCase() ? 1 : 2)
                    }), _.attributes && i(function(e) {
                      return e.innerHTML = "<input/>", e.firstChild.setAttribute("value", ""), "" === e
                        .firstChild.getAttribute("value")
                    }) || o("value", function(e, t, n) {
                      return n || "input" !== e.nodeName.toLowerCase() ? void 0 : e.defaultValue
                    }), i(function(e) {
                      return null == e.getAttribute("disabled")
                    }) || o(te, function(e, t, n) {
                      var r;
                      return n ? void 0 : e[t] === !0 ? t.toLowerCase() : (r = e.getAttributeNode(t)) && r
                        .specified ? r.value : null
                    }), t
                }(e), bt.find = j, bt.expr = j.selectors, bt.expr[":"] = bt.expr.pseudos, bt.uniqueSort = bt
                .unique = j.uniqueSort, bt.text = j.getText, bt.isXMLDoc = j.isXML, bt.contains = j.contains, H =
                bt.expr.match.needsContext, B = /^.[^:#\[\.,]*$/, bt.filter = function(e, t, n) {
                  var r = t[0];
                  return n && (e = ":not(" + e + ")"), 1 === t.length && 1 === r.nodeType ? bt.find
                    .matchesSelector(r, e) ? [r] : [] : bt.find.matches(e, bt.grep(t, function(e) {
                      return 1 === e.nodeType
                    }))
                }, bt.fn.extend({
                  find: function(e) {
                    var t, n = this.length,
                      r = [],
                      i = this;
                    if ("string" != typeof e) return this.pushStack(bt(e).filter(function() {
                      for (t = 0; n > t; t++)
                        if (bt.contains(i[t], this)) return !0
                    }));
                    for (t = 0; n > t; t++) bt.find(e, i[t], r);
                    return r = this.pushStack(n > 1 ? bt.unique(r) : r), r.selector = this.selector ? this
                      .selector + " " + e : e, r
                  },
                  filter: function(e) {
                    return this.pushStack(n(this, e || [], !1))
                  },
                  not: function(e) {
                    return this.pushStack(n(this, e || [], !0))
                  },
                  is: function(e) {
                    return !!n(this, "string" == typeof e && H.test(e) ? bt(e) : e || [], !1).length
                  }
                }), q = /^(?:\s*(<[\w\W]+>)[^>]*|#([\w-]*))$/, G = bt.fn.init = function(e, t, n) {
                  var r, i;
                  if (!e) return this;
                  if (n = n || z, "string" == typeof e) {
                    if (r = "<" === e[0] && ">" === e[e.length - 1] && e.length >= 3 ? [null, e, null] : q.exec(
                      e), !r || !r[1] && t) return !t || t.jquery ? (t || n).find(e) : this.constructor(t).find(
                    e);
                    if (r[1]) {
                      if (t = t instanceof bt ? t[0] : t, bt.merge(this, bt.parseHTML(r[1], t && t.nodeType ? t
                          .ownerDocument || t : ut, !0)), F.test(r[1]) && bt.isPlainObject(t))
                        for (r in t) bt.isFunction(this[r]) ? this[r](t[r]) : this.attr(r, t[r]);
                      return this
                    }
                    return i = ut.getElementById(r[2]), i && i.parentNode && (this.length = 1, this[0] = i), this
                      .context = ut, this.selector = e, this
                  }
                  return e.nodeType ? (this.context = this[0] = e, this.length = 1, this) : bt.isFunction(e) ?
                    void 0 !== n.ready ? n.ready(e) : e(bt) : (void 0 !== e.selector && (this.selector = e
                      .selector, this.context = e.context), bt.makeArray(e, this))
                }, G.prototype = bt.fn, z = bt(ut), V = /\S+/g, W = e.location, Y = bt.now(), K = /\?/, bt
                .parseJSON = function(e) {
                  return JSON.parse(e + "")
                }, bt.parseXML = function(t) {
                  var n;
                  if (!t || "string" != typeof t) return null;
                  try {
                    n = (new e.DOMParser).parseFromString(t, "text/xml")
                  } catch (e) {
                    n = void 0
                  }
                  return (!n || n.getElementsByTagName("parsererror").length) && bt.error("Invalid XML: " + t), n
                }, X = function(e) {
                  return 1 === e.nodeType || 9 === e.nodeType || !+e.nodeType
                }, r.uid = 1, r.prototype = {
                  register: function(e, t) {
                    var n = t || {};
                    return e.nodeType ? e[this.expando] = n : Object.defineProperty(e, this.expando, {
                      value: n,
                      writable: !0,
                      configurable: !0
                    }), e[this.expando]
                  },
                  cache: function(e) {
                    if (!X(e)) return {};
                    var t = e[this.expando];
                    return t || (t = {}, X(e) && (e.nodeType ? e[this.expando] = t : Object.defineProperty(e,
                      this.expando, {
                        value: t,
                        configurable: !0
                      }))), t
                  },
                  set: function(e, t, n) {
                    var r, i = this.cache(e);
                    if ("string" == typeof t) i[t] = n;
                    else
                      for (r in t) i[r] = t[r];
                    return i
                  },
                  get: function(e, t) {
                    return void 0 === t ? this.cache(e) : e[this.expando] && e[this.expando][t]
                  },
                  access: function(e, t, n) {
                    var r;
                    return void 0 === t || t && "string" == typeof t && void 0 === n ? (r = this.get(e, t),
                      void 0 !== r ? r : this.get(e, bt.camelCase(t))) : (this.set(e, t, n), void 0 !== n ?
                      n : t)
                  },
                  remove: function(e, t) {
                    var n, r, i, o = e[this.expando];
                    if (void 0 !== o) {
                      if (void 0 === t) this.register(e);
                      else {
                        bt.isArray(t) ? r = t.concat(t.map(bt.camelCase)) : (i = bt.camelCase(t), t in o ? r = [
                          t, i
                        ] : (r = i, r = r in o ? [r] : r.match(V) || [])), n = r.length;
                        for (; n--;) delete o[r[n]]
                      }(void 0 === t || bt.isEmptyObject(o)) && (e.nodeType ? e[this.expando] = void 0 :
                        delete e[this.expando])
                    }
                  },
                  hasData: function(e) {
                    var t = e[this.expando];
                    return void 0 !== t && !bt.isEmptyObject(t)
                  }
                }, Q = new r, J = /^key/, Z = /^(?:mouse|pointer|contextmenu|drag|drop)|click/, ee =
                /^([^.]*)(?:\.(.+)|)/, bt.event = {
                  global: {},
                  add: function(e, t, n, r, i) {
                    var o, a, s, c, u, l, d, f, h, p, m, v = Q.get(e);
                    if (v)
                      for (n.handler && (o = n, n = o.handler, i = o.selector), n.guid || (n.guid = bt.guid++),
                        (c = v.events) || (c = v.events = {}), (a = v.handle) || (a = v.handle = function(t) {
                          return void 0 !== bt && bt.event.triggered !== t.type ? bt.event.dispatch.apply(e,
                            arguments) : void 0
                        }), t = (t || "").match(V) || [""], u = t.length; u--;) s = ee.exec(t[u]) || [], h = m =
                        s[1], p = (s[2] || "").split(".").sort(), h && (d = bt.event.special[h] || {}, h = (i ?
                          d.delegateType : d.bindType) || h, d = bt.event.special[h] || {}, l = bt.extend({
                          type: h,
                          origType: m,
                          data: r,
                          handler: n,
                          guid: n.guid,
                          selector: i,
                          needsContext: i && bt.expr.match.needsContext.test(i),
                          namespace: p.join(".")
                        }, o), (f = c[h]) || (f = c[h] = [], f.delegateCount = 0, d.setup && d.setup.call(e,
                          r, p, a) !== !1 || e.addEventListener && e.addEventListener(h, a)), d.add && (d.add
                          .call(e, l), l.handler.guid || (l.handler.guid = n.guid)), i ? f.splice(f
                          .delegateCount++, 0, l) : f.push(l), bt.event.global[h] = !0)
                  },
                  remove: function(e, t, n, r, i) {
                    var o, a, s, c, u, l, d, f, h, p, m, v = Q.hasData(e) && Q.get(e);
                    if (v && (c = v.events)) {
                      for (t = (t || "").match(V) || [""], u = t.length; u--;)
                        if (s = ee.exec(t[u]) || [], h = m = s[1], p = (s[2] || "").split(".").sort(), h) {
                          for (d = bt.event.special[h] || {}, h = (r ? d.delegateType : d.bindType) || h, f = c[
                              h] || [], s = s[2] && RegExp("(^|\\.)" + p.join("\\.(?:.*\\.|)") + "(\\.|$)"), a =
                            o = f.length; o--;) l = f[o], !i && m !== l.origType || n && n.guid !== l.guid ||
                            s && !s.test(l.namespace) || r && r !== l.selector && ("**" !== r || !l.selector) ||
                            (f.splice(o, 1), l.selector && f.delegateCount--, d.remove && d.remove.call(e, l));
                          a && !f.length && (d.teardown && d.teardown.call(e, p, v.handle) !== !1 || bt
                            .removeEvent(e, h, v.handle), delete c[h])
                        } else
                          for (h in c) bt.event.remove(e, h + t[u], n, r, !0);
                      bt.isEmptyObject(c) && Q.remove(e, "handle events")
                    }
                  },
                  dispatch: function(e) {
                    e = bt.event.fix(e);
                    var t, n, r, i, o, a = [],
                      s = lt.call(arguments),
                      c = (Q.get(this, "events") || {})[e.type] || [],
                      u = bt.event.special[e.type] || {};
                    if (s[0] = e, e.delegateTarget = this, !u.preDispatch || u.preDispatch.call(this, e) !== !
                      1) {
                      for (a = bt.event.handlers.call(this, e, c), t = 0;
                        (i = a[t++]) && !e.isPropagationStopped();)
                        for (e.currentTarget = i.elem, n = 0;
                          (o = i.handlers[n++]) && !e.isImmediatePropagationStopped();)(!e.rnamespace || e
                          .rnamespace.test(o.namespace)) && (e.handleObj = o, e.data = o.data, r = ((bt.event
                            .special[o.origType] || {}).handle || o.handler).apply(i.elem, s), void 0 !== r &&
                          (e.result = r) === !1 && (e.preventDefault(), e.stopPropagation()));
                      return u.postDispatch && u.postDispatch.call(this, e), e.result
                    }
                  },
                  handlers: function(e, t) {
                    var n, r, i, o, a = [],
                      s = t.delegateCount,
                      c = e.target;
                    if (s && c.nodeType && ("click" !== e.type || isNaN(e.button) || e.button < 1))
                      for (; c !== this; c = c.parentNode || this)
                        if (1 === c.nodeType && (c.disabled !== !0 || "click" !== e.type)) {
                          for (r = [], n = 0; s > n; n++) o = t[n], i = o.selector + " ", void 0 === r[i] && (r[
                              i] = o.needsContext ? bt(i, this).index(c) > -1 : bt.find(i, this, null, [c])
                            .length), r[i] && r.push(o);
                          r.length && a.push({
                            elem: c,
                            handlers: r
                          })
                        } return s < t.length && a.push({
                      elem: this,
                      handlers: t.slice(s)
                    }), a
                  },
                  props: "altKey bubbles cancelable ctrlKey currentTarget detail eventPhase metaKey relatedTarget shiftKey target timeStamp view which"
                    .split(" "),
                  fixHooks: {},
                  keyHooks: {
                    props: "char charCode key keyCode".split(" "),
                    filter: function(e, t) {
                      return null == e.which && (e.which = null != t.charCode ? t.charCode : t.keyCode), e
                    }
                  },
                  mouseHooks: {
                    props: "button buttons clientX clientY offsetX offsetY pageX pageY screenX screenY toElement"
                      .split(" "),
                    filter: function(e, t) {
                      var n, r, i, o = t.button;
                      return null == e.pageX && null != t.clientX && (n = e.target.ownerDocument || ut, r = n
                        .documentElement, i = n.body, e.pageX = t.clientX + (r && r.scrollLeft || i && i
                          .scrollLeft || 0) - (r && r.clientLeft || i && i.clientLeft || 0), e.pageY = t
                        .clientY + (r && r.scrollTop || i && i.scrollTop || 0) - (r && r.clientTop || i && i
                          .clientTop || 0)), e.which || void 0 === o || (e.which = 1 & o ? 1 : 2 & o ? 3 : 4 &
                        o ? 2 : 0), e
                    }
                  },
                  fix: function(e) {
                    if (e[bt.expando]) return e;
                    var t, n, r, i = e.type,
                      o = e,
                      a = this.fixHooks[i];
                    for (a || (this.fixHooks[i] = a = Z.test(i) ? this.mouseHooks : J.test(i) ? this
                      .keyHooks : {}), r = a.props ? this.props.concat(a.props) : this.props, e = new bt.Event(
                        o), t = r.length; t--;) n = r[t], e[n] = o[n];
                    return e.target || (e.target = ut), 3 === e.target.nodeType && (e.target = e.target
                      .parentNode), a.filter ? a.filter(e, o) : e
                  },
                  special: {
                    load: {
                      noBubble: !0
                    },
                    focus: {
                      trigger: function() {
                        return this !== a() && this.focus ? (this.focus(), !1) : void 0
                      },
                      delegateType: "focusin"
                    },
                    blur: {
                      trigger: function() {
                        return this === a() && this.blur ? (this.blur(), !1) : void 0
                      },
                      delegateType: "focusout"
                    },
                    click: {
                      trigger: function() {
                        return "checkbox" === this.type && this.click && bt.nodeName(this, "input") ? (this
                          .click(), !1) : void 0
                      },
                      _default: function(e) {
                        return bt.nodeName(e.target, "a")
                      }
                    },
                    beforeunload: {
                      postDispatch: function(e) {
                        void 0 !== e.result && e.originalEvent && (e.originalEvent.returnValue = e.result)
                      }
                    }
                  }
                }, bt.removeEvent = function(e, t, n) {
                  e.removeEventListener && e.removeEventListener(t, n)
                }, bt.Event = function(e, t) {
                  return this instanceof bt.Event ? (e && e.type ? (this.originalEvent = e, this.type = e.type,
                      this.isDefaultPrevented = e.defaultPrevented || void 0 === e.defaultPrevented && e
                      .returnValue === !1 ? i : o) : this.type = e, t && bt.extend(this, t), this.timeStamp =
                    e && e.timeStamp || bt.now(), void(this[bt.expando] = !0)) : new bt.Event(e, t)
                }, bt.Event.prototype = {
                  constructor: bt.Event,
                  isDefaultPrevented: o,
                  isPropagationStopped: o,
                  isImmediatePropagationStopped: o,
                  preventDefault: function() {
                    var e = this.originalEvent;
                    this.isDefaultPrevented = i, e && e.preventDefault()
                  },
                  stopPropagation: function() {
                    var e = this.originalEvent;
                    this.isPropagationStopped = i, e && e.stopPropagation()
                  },
                  stopImmediatePropagation: function() {
                    var e = this.originalEvent;
                    this.isImmediatePropagationStopped = i, e && e.stopImmediatePropagation(), this
                      .stopPropagation()
                  }
                }, bt.each({
                  mouseenter: "mouseover",
                  mouseleave: "mouseout",
                  pointerenter: "pointerover",
                  pointerleave: "pointerout"
                }, function(e, t) {
                  bt.event.special[e] = {
                    delegateType: t,
                    bindType: t,
                    handle: function(e) {
                      var n, r = this,
                        i = e.relatedTarget,
                        o = e.handleObj;
                      return (!i || i !== r && !bt.contains(r, i)) && (e.type = o.origType, n = o.handler
                        .apply(this, arguments), e.type = t), n
                    }
                  }
                }), bt.fn.extend({
                  on: function(e, t, n, r) {
                    return s(this, e, t, n, r)
                  },
                  one: function(e, t, n, r) {
                    return s(this, e, t, n, r, 1)
                  },
                  off: function(e, t, n) {
                    var r, i;
                    if (e && e.preventDefault && e.handleObj) return r = e.handleObj, bt(e.delegateTarget)
                      .off(r.namespace ? r.origType + "." + r.namespace : r.origType, r.selector, r
                      .handler), this;
                    if ("object" == typeof e) {
                      for (i in e) this.off(i, t, e[i]);
                      return this
                    }
                    return (t === !1 || "function" == typeof t) && (n = t, t = void 0), n === !1 && (n = o),
                      this.each(function() {
                        bt.event.remove(this, e, n, t)
                      })
                  }
                }), te = /^(?:focusinfocus|focusoutblur)$/, bt.extend(bt.event, {
                  trigger: function(t, n, r, i) {
                    var o, a, s, c, u, l, d, f = [r || ut],
                      h = vt.call(t, "type") ? t.type : t,
                      p = vt.call(t, "namespace") ? t.namespace.split(".") : [];
                    if (a = s = r = r || ut, 3 !== r.nodeType && 8 !== r.nodeType && !te.test(h + bt.event
                        .triggered) && (h.indexOf(".") > -1 && (p = h.split("."), h = p.shift(), p.sort()),
                        u = h.indexOf(":") < 0 && "on" + h, t = t[bt.expando] ? t : new bt.Event(h,
                          "object" == typeof t && t), t.isTrigger = i ? 2 : 3, t.namespace = p.join("."), t
                        .rnamespace = t.namespace ? RegExp("(^|\\.)" + p.join("\\.(?:.*\\.|)") + "(\\.|$)") :
                        null, t.result = void 0, t.target || (t.target = r), n = null == n ? [t] : bt
                        .makeArray(n, [t]), d = bt.event.special[h] || {}, i || !d.trigger || d.trigger.apply(
                          r, n) !== !1)) {
                      if (!i && !d.noBubble && !bt.isWindow(r)) {
                        for (c = d.delegateType || h, te.test(c + h) || (a = a.parentNode); a; a = a
                          .parentNode) f.push(a), s = a;
                        s === (r.ownerDocument || ut) && f.push(s.defaultView || s.parentWindow || e)
                      }
                      for (o = 0;
                        (a = f[o++]) && !t.isPropagationStopped();) t.type = o > 1 ? c : d.bindType || h, l =
                        (Q.get(a, "events") || {})[t.type] && Q.get(a, "handle"), l && l.apply(a, n), l = u &&
                        a[u], l && l.apply && X(a) && (t.result = l.apply(a, n), t.result === !1 && t
                          .preventDefault());
                      return t.type = h, i || t.isDefaultPrevented() || d._default && d._default.apply(f
                      .pop(), n) !== !1 || !X(r) || u && bt.isFunction(r[h]) && !bt.isWindow(r) && (s = r[
                        u], s && (r[u] = null), bt.event.triggered = h, r[h](), bt.event.triggered = void 0,
                        s && (r[u] = s)), t.result
                    }
                  },
                  simulate: function(e, t, n) {
                    var r = bt.extend(new bt.Event, n, {
                      type: e,
                      isSimulated: !0
                    });
                    bt.event.trigger(r, null, t), r.isDefaultPrevented() && n.preventDefault()
                  }
                }), bt.fn.extend({
                  trigger: function(e, t) {
                    return this.each(function() {
                      bt.event.trigger(e, t, this)
                    })
                  },
                  triggerHandler: function(e, t) {
                    var n = this[0];
                    return n ? bt.event.trigger(e, t, n, !0) : void 0
                  }
                }), bt.Callbacks = function(e) {
                  e = "string" == typeof e ? c(e) : bt.extend({}, e);
                  var t, n, r, i, o = [],
                    a = [],
                    s = -1,
                    u = function() {
                      for (i = e.once, r = t = !0; a.length; s = -1)
                        for (n = a.shift(); ++s < o.length;) o[s].apply(n[0], n[1]) === !1 && e.stopOnFalse && (
                          s = o.length, n = !1);
                      e.memory || (n = !1), t = !1, i && (o = n ? [] : "")
                    },
                    l = {
                      add: function() {
                        return o && (n && !t && (s = o.length - 1, a.push(n)), function t(n) {
                          bt.each(n, function(n, r) {
                            bt.isFunction(r) ? e.unique && l.has(r) || o.push(r) : r && r.length &&
                              "string" !== bt.type(r) && t(r)
                          })
                        }(arguments), n && !t && u()), this
                      },
                      remove: function() {
                        return bt.each(arguments, function(e, t) {
                          for (var n;
                            (n = bt.inArray(t, o, n)) > -1;) o.splice(n, 1), s >= n && s--
                        }), this
                      },
                      has: function(e) {
                        return e ? bt.inArray(e, o) > -1 : o.length > 0
                      },
                      empty: function() {
                        return o && (o = []), this
                      },
                      disable: function() {
                        return i = a = [], o = n = "", this
                      },
                      disabled: function() {
                        return !o
                      },
                      lock: function() {
                        return i = a = [], n || (o = n = ""), this
                      },
                      locked: function() {
                        return !!i
                      },
                      fireWith: function(e, n) {
                        return i || (n = n || [], n = [e, n.slice ? n.slice() : n], a.push(n), t || u()), this
                      },
                      fire: function() {
                        return l.fireWith(this, arguments), this
                      },
                      fired: function() {
                        return !!r
                      }
                    };
                  return l
                }, bt.extend({
                  Deferred: function(e) {
                    var t = [
                        ["resolve", "done", bt.Callbacks("once memory"), "resolved"],
                        ["reject", "fail", bt.Callbacks("once memory"), "rejected"],
                        ["notify", "progress", bt.Callbacks("memory")]
                      ],
                      n = "pending",
                      r = {
                        state: function() {
                          return n
                        },
                        always: function() {
                          return i.done(arguments).fail(arguments), this
                        },
                        then: function() {
                          var e = arguments;
                          return bt.Deferred(function(n) {
                            bt.each(t, function(t, o) {
                              var a = bt.isFunction(e[t]) && e[t];
                              i[o[1]](function() {
                                var e = a && a.apply(this, arguments);
                                e && bt.isFunction(e.promise) ? e.promise().progress(n.notify)
                                  .done(n.resolve).fail(n.reject) : n[o[0] + "With"](this ===
                                    r ? n.promise() : this, a ? [e] : arguments)
                              })
                            }), e = null
                          }).promise()
                        },
                        promise: function(e) {
                          return null != e ? bt.extend(e, r) : r
                        }
                      },
                      i = {};
                    return r.pipe = r.then, bt.each(t, function(e, o) {
                      var a = o[2],
                        s = o[3];
                      r[o[1]] = a.add, s && a.add(function() {
                        n = s
                      }, t[1 ^ e][2].disable, t[2][2].lock), i[o[0]] = function() {
                        return i[o[0] + "With"](this === i ? r : this, arguments), this
                      }, i[o[0] + "With"] = a.fireWith
                    }), r.promise(i), e && e.call(i, i), i
                  },
                  when: function(e) {
                    var t, n, r, i = 0,
                      o = lt.call(arguments),
                      a = o.length,
                      s = 1 !== a || e && bt.isFunction(e.promise) ? a : 0,
                      c = 1 === s ? e : bt.Deferred(),
                      u = function(e, n, r) {
                        return function(i) {
                          n[e] = this, r[e] = arguments.length > 1 ? lt.call(arguments) : i, r === t ? c
                            .notifyWith(n, r) : --s || c.resolveWith(n, r)
                        }
                      };
                    if (a > 1)
                      for (t = Array(a), n = Array(a), r = Array(a); a > i; i++) o[i] && bt.isFunction(o[i]
                          .promise) ? o[i].promise().progress(u(i, n, t)).done(u(i, r, o)).fail(c.reject) : --
                        s;
                    return s || c.resolveWith(r, o), c.promise()
                  }
                }), ne = /#.*$/, re = /([?&])_=[^&]*/, ie = /^(.*?):[ \t]*([^\r\n]*)$/gm, oe =
                /^(?:about|app|app-storage|.+-extension|file|res|widget):$/, ae = /^(?:GET|HEAD)$/, se = /^\/\//,
                ce = {}, ue = {}, le = "*/".concat("*"), de = ut.createElement("a"), de.href = W.href, bt.extend({
                  active: 0,
                  lastModified: {},
                  etag: {},
                  ajaxSettings: {
                    url: W.href,
                    type: "GET",
                    isLocal: oe.test(W.protocol),
                    global: !0,
                    processData: !0,
                    async: !0,
                    contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                    accepts: {
                      "*": le,
                      text: "text/plain",
                      html: "text/html",
                      xml: "application/xml, text/xml",
                      json: "application/json, text/javascript"
                    },
                    contents: {
                      xml: /\bxml\b/,
                      html: /\bhtml/,
                      json: /\bjson\b/
                    },
                    responseFields: {
                      xml: "responseXML",
                      text: "responseText",
                      json: "responseJSON"
                    },
                    converters: {
                      "* text": String,
                      "text html": !0,
                      "text json": bt.parseJSON,
                      "text xml": bt.parseXML
                    },
                    flatOptions: {
                      url: !0,
                      context: !0
                    }
                  },
                  ajaxSetup: function(e, t) {
                    return t ? d(d(e, bt.ajaxSettings), t) : d(bt.ajaxSettings, e)
                  },
                  ajaxPrefilter: u(ce),
                  ajaxTransport: u(ue),
                  ajax: function(t, n) {
                    function r(t, n, r, s) {
                      var u, l, p, _, $, T = n;
                      2 !== w && (w = 2, c && e.clearTimeout(c), i = void 0, a = s || "", C.readyState = t >
                        0 ? 4 : 0, u = t >= 200 && 300 > t || 304 === t, r && (_ = f(m, C, r)), _ = h(m, _,
                          C, u), u ? (m.ifModified && ($ = C.getResponseHeader("Last-Modified"), $ && (bt
                            .lastModified[o] = $), $ = C.getResponseHeader("etag"), $ && (bt.etag[o] = $)),
                          204 === t || "HEAD" === m.type ? T = "nocontent" : 304 === t ? T = "notmodified" :
                          (T = _.state, l = _.data, p = _.error, u = !p)) : (p = T, (t || !T) && (T =
                          "error", 0 > t && (t = 0))), C.status = t, C.statusText = (n || T) + "", u ? y
                        .resolveWith(v, [l, T, C]) : y.rejectWith(v, [C, T, p]), C.statusCode(E), E =
                        void 0, d && g.trigger(u ? "ajaxSuccess" : "ajaxError", [C, m, u ? l : p]), b
                        .fireWith(v, [C, T]), d && (g.trigger("ajaxComplete", [C, m]), --bt.active || bt
                          .event.trigger("ajaxStop")))
                    }
                    "object" == typeof t && (n = t, t = void 0), n = n || {};
                    var i, o, a, s, c, u, d, p, m = bt.ajaxSetup({}, n),
                      v = m.context || m,
                      g = m.context && (v.nodeType || v.jquery) ? bt(v) : bt.event,
                      y = bt.Deferred(),
                      b = bt.Callbacks("once memory"),
                      E = m.statusCode || {},
                      _ = {},
                      $ = {},
                      w = 0,
                      T = "canceled",
                      C = {
                        readyState: 0,
                        getResponseHeader: function(e) {
                          var t;
                          if (2 === w) {
                            if (!s)
                              for (s = {}; t = ie.exec(a);) s[t[1].toLowerCase()] = t[2];
                            t = s[e.toLowerCase()]
                          }
                          return null == t ? null : t
                        },
                        getAllResponseHeaders: function() {
                          return 2 === w ? a : null
                        },
                        setRequestHeader: function(e, t) {
                          var n = e.toLowerCase();
                          return w || (e = $[n] = $[n] || e, _[e] = t), this
                        },
                        overrideMimeType: function(e) {
                          return w || (m.mimeType = e), this
                        },
                        statusCode: function(e) {
                          var t;
                          if (e)
                            if (2 > w)
                              for (t in e) E[t] = [E[t], e[t]];
                            else C.always(e[C.status]);
                          return this
                        },
                        abort: function(e) {
                          var t = e || T;
                          return i && i.abort(t), r(0, t), this
                        }
                      };
                    if (y.promise(C).complete = b.add, C.success = C.done, C.error = C.fail, m.url = ((t || m
                        .url || W.href) + "").replace(ne, "").replace(se, W.protocol + "//"), m.type = n
                      .method || n.type || m.method || m.type, m.dataTypes = bt.trim(m.dataType || "*")
                      .toLowerCase().match(V) || [""], null == m.crossDomain) {
                      u = ut.createElement("a");
                      try {
                        u.href = m.url, u.href = u.href, m.crossDomain = de.protocol + "//" + de.host != u
                          .protocol + "//" + u.host
                      } catch (e) {
                        m.crossDomain = !0
                      }
                    }
                    if (m.data && m.processData && "string" != typeof m.data && (m.data = bt.param(m.data, m
                        .traditional)), l(ce, m, n, C), 2 === w) return C;
                    d = bt.event && m.global, d && 0 === bt.active++ && bt.event.trigger("ajaxStart"), m
                      .type = m.type.toUpperCase(), m.hasContent = !ae.test(m.type), o = m.url, m
                      .hasContent || (m.data && (o = m.url += (K.test(o) ? "&" : "?") + m.data, delete m
                        .data), m.cache === !1 && (m.url = re.test(o) ? o.replace(re, "$1_=" + Y++) : o + (K
                          .test(o) ? "&" : "?") + "_=" + Y++)), m.ifModified && (bt.lastModified[o] && C
                        .setRequestHeader("If-Modified-Since", bt.lastModified[o]), bt.etag[o] && C
                        .setRequestHeader("If-None-Match", bt.etag[o])), (m.data && m.hasContent && m
                        .contentType !== !1 || n.contentType) && C.setRequestHeader("Content-Type", m
                        .contentType), C.setRequestHeader("Accept", m.dataTypes[0] && m.accepts[m.dataTypes[
                        0]] ? m.accepts[m.dataTypes[0]] + ("*" !== m.dataTypes[0] ? ", " + le + "; q=0.01" :
                        "") : m.accepts["*"]);
                    for (p in m.headers) C.setRequestHeader(p, m.headers[p]);
                    if (m.beforeSend && (m.beforeSend.call(v, C, m) === !1 || 2 === w)) return C.abort();
                    T = "abort";
                    for (p in {
                        success: 1,
                        error: 1,
                        complete: 1
                      }) C[p](m[p]);
                    if (i = l(ue, m, n, C)) {
                      if (C.readyState = 1, d && g.trigger("ajaxSend", [C, m]), 2 === w) return C;
                      m.async && m.timeout > 0 && (c = e.setTimeout(function() {
                        C.abort("timeout")
                      }, m.timeout));
                      try {
                        w = 1, i.send(_, r)
                      } catch (e) {
                        if (!(2 > w)) throw e;
                        r(-1, e)
                      }
                    } else r(-1, "No Transport");
                    return C
                  },
                  getJSON: function(e, t, n) {
                    return bt.get(e, t, n, "json")
                  },
                  getScript: function(e, t) {
                    return bt.get(e, void 0, t, "script")
                  }
                }), bt.each(["get", "post"], function(e, t) {
                  bt[t] = function(e, n, r, i) {
                    return bt.isFunction(n) && (i = i || r, r = n, n = void 0), bt.ajax(bt.extend({
                      url: e,
                      type: t,
                      dataType: i,
                      data: n,
                      success: r
                    }, bt.isPlainObject(e) && e))
                  }
                }), fe = [], he = /(=)\?(?=&|$)|\?\?/, bt.ajaxSetup({
                  jsonp: "callback",
                  jsonpCallback: function() {
                    var e = fe.pop() || bt.expando + "_" + Y++;
                    return this[e] = !0, e
                  }
                }), bt.ajaxPrefilter("json jsonp", function(t, n, r) {
                  var i, o, a, s = t.jsonp !== !1 && (he.test(t.url) ? "url" : "string" == typeof t.data &&
                    0 === (t.contentType || "").indexOf("application/x-www-form-urlencoded") && he.test(t
                      .data) && "data");
                  return s || "jsonp" === t.dataTypes[0] ? (i = t.jsonpCallback = bt.isFunction(t
                    .jsonpCallback) ? t.jsonpCallback() : t.jsonpCallback, s ? t[s] = t[s].replace(he, "$1" +
                      i) : t.jsonp !== !1 && (t.url += (K.test(t.url) ? "&" : "?") + t.jsonp + "=" + i), t
                    .converters["script json"] = function() {
                      return a || bt.error(i + " was not called"), a[0]
                    }, t.dataTypes[0] = "json", o = e[i], e[i] = function() {
                      a = arguments
                    }, r.always(function() {
                      void 0 === o ? bt(e).removeProp(i) : e[i] = o, t[i] && (t.jsonpCallback = n
                        .jsonpCallback, fe.push(i)), a && bt.isFunction(o) && o(a[0]), a = o = void 0
                    }), "script") : void 0
                }), bt.ajaxSetup({
                  accepts: {
                    script: "text/javascript, application/javascript, application/ecmascript, application/x-ecmascript"
                  },
                  contents: {
                    script: /\b(?:java|ecma)script\b/
                  },
                  converters: {
                    "text script": function(e) {
                      return bt.globalEval(e), e
                    }
                  }
                }), bt.ajaxPrefilter("script", function(e) {
                  void 0 === e.cache && (e.cache = !1), e.crossDomain && (e.type = "GET")
                }), bt.ajaxTransport("script", function(e) {
                  if (e.crossDomain) {
                    var t, n;
                    return {
                      send: function(r, i) {
                        t = bt("<script>").prop({
                          charset: e.scriptCharset,
                          src: e.url
                        }).on("load error", n = function(e) {
                          t.remove(), n = null, e && i("error" === e.type ? 404 : 200, e.type)
                        }), ut.head.appendChild(t[0])
                      },
                      abort: function() {
                        n && n()
                      }
                    }
                  }
                }), bt.ajaxSettings.xhr = function() {
                  try {
                    return new e.XMLHttpRequest
                  } catch (e) {}
                }, pe = {
                  0: 200,
                  1223: 204
                }, me = bt.ajaxSettings.xhr(), gt.cors = !!me && "withCredentials" in me, gt.ajax = me = !!me, bt
                .ajaxTransport(function(t) {
                  var n, r;
                  return gt.cors || me && !t.crossDomain ? {
                    send: function(i, o) {
                      var a, s = t.xhr();
                      if (s.open(t.type, t.url, t.async, t.username, t.password), t.xhrFields)
                        for (a in t.xhrFields) s[a] = t.xhrFields[a];
                      t.mimeType && s.overrideMimeType && s.overrideMimeType(t.mimeType), t.crossDomain ||
                        i["X-Requested-With"] || (i["X-Requested-With"] = "XMLHttpRequest");
                      for (a in i) s.setRequestHeader(a, i[a]);
                      n = function(e) {
                          return function() {
                            n && (n = r = s.onload = s.onerror = s.onabort = s.onreadystatechange = null,
                              "abort" === e ? s.abort() : "error" === e ? "number" != typeof s.status ?
                              o(0, "error") : o(s.status, s.statusText) : o(pe[s.status] || s.status, s
                                .statusText, "text" !== (s.responseType || "text") || "string" !=
                                typeof s.responseText ? {
                                  binary: s.response
                                } : {
                                  text: s.responseText
                                }, s.getAllResponseHeaders()))
                          }
                        }, s.onload = n(), r = s.onerror = n("error"), void 0 !== s.onabort ? s.onabort =
                        r : s.onreadystatechange = function() {
                          4 === s.readyState && e.setTimeout(function() {
                            n && r()
                          })
                        }, n = n("abort");
                      try {
                        s.send(t.hasContent && t.data || null)
                      } catch (e) {
                        if (n) throw e
                      }
                    },
                    abort: function() {
                      n && n()
                    }
                  } : void 0
                }), ve = function e(t, n, r, i, o, a, s) {
                  var c = 0,
                    u = t.length,
                    l = null == r;
                  if ("object" === bt.type(r)) {
                    o = !0;
                    for (c in r) e(t, n, c, r[c], !0, a, s)
                  } else if (void 0 !== i && (o = !0, bt.isFunction(i) || (s = !0), l && (s ? (n.call(t, i), n =
                      null) : (l = n, n = function(e, t, n) {
                      return l.call(bt(e), n)
                    })), n))
                    for (; u > c; c++) n(t[c], r, s ? i : i.call(t[c], c, n(t[c], r)));
                  return o ? t : l ? n.call(t) : u ? n(t[0], r) : a
                }, ge = /^(?:checkbox|radio)$/i, ye = /<([\w:-]+)/, be = /^$|\/(?:java|ecma)script/i, Ee = {
                  option: [1, "<select multiple='multiple'>", "</select>"],
                  thead: [1, "<table>", "</table>"],
                  col: [2, "<table><colgroup>", "</colgroup></table>"],
                  tr: [2, "<table><tbody>", "</tbody></table>"],
                  td: [3, "<table><tbody><tr>", "</tr></tbody></table>"],
                  _default: [0, "", ""]
                }, Ee.optgroup = Ee.option, Ee.tbody = Ee.tfoot = Ee.colgroup = Ee.caption = Ee.thead, Ee.th = Ee
                .td, _e = /<|&#?\w+;/,
                function() {
                  var e = ut.createDocumentFragment(),
                    t = e.appendChild(ut.createElement("div")),
                    n = ut.createElement("input");
                  n.setAttribute("type", "radio"), n.setAttribute("checked", "checked"), n.setAttribute("name",
                      "t"), t.appendChild(n), gt.checkClone = t.cloneNode(!0).cloneNode(!0).lastChild.checked, t
                    .innerHTML = "<textarea>x</textarea>", gt.noCloneChecked = !!t.cloneNode(!0).lastChild
                    .defaultValue
                }(), $e = new r, we = function(e, t, n) {
                  for (var r = [], i = void 0 !== n;
                    (e = e[t]) && 9 !== e.nodeType;)
                    if (1 === e.nodeType) {
                      if (i && bt(e).is(n)) break;
                      r.push(e)
                    } return r
                }, Te = function(e, t) {
                  for (var n = []; e; e = e.nextSibling) 1 === e.nodeType && e !== t && n.push(e);
                  return n
                }, Ce = /^(?:parents|prev(?:Until|All))/, xe = {
                  children: !0,
                  contents: !0,
                  next: !0,
                  prev: !0
                }, bt.fn.extend({
                  has: function(e) {
                    var t = bt(e, this),
                      n = t.length;
                    return this.filter(function() {
                      for (var e = 0; n > e; e++)
                        if (bt.contains(this, t[e])) return !0
                    })
                  },
                  closest: function(e, t) {
                    for (var n, r = 0, i = this.length, o = [], a = H.test(e) || "string" != typeof e ? bt(e,
                        t || this.context) : 0; i > r; r++)
                      for (n = this[r]; n && n !== t; n = n.parentNode)
                        if (n.nodeType < 11 && (a ? a.index(n) > -1 : 1 === n.nodeType && bt.find
                            .matchesSelector(n, e))) {
                          o.push(n);
                          break
                        } return this.pushStack(o.length > 1 ? bt.uniqueSort(o) : o)
                  },
                  index: function(e) {
                    return e ? "string" == typeof e ? ht.call(bt(e), this[0]) : ht.call(this, e.jquery ? e[
                      0] : e) : this[0] && this[0].parentNode ? this.first().prevAll().length : -1
                  },
                  add: function(e, t) {
                    return this.pushStack(bt.uniqueSort(bt.merge(this.get(), bt(e, t))))
                  },
                  addBack: function(e) {
                    return this.add(null == e ? this.prevObject : this.prevObject.filter(e))
                  }
                }), bt.each({
                  parent: function e(t) {
                    var e = t.parentNode;
                    return e && 11 !== e.nodeType ? e : null
                  },
                  parents: function(e) {
                    return we(e, "parentNode")
                  },
                  parentsUntil: function(e, t, n) {
                    return we(e, "parentNode", n)
                  },
                  next: function(e) {
                    return g(e, "nextSibling")
                  },
                  prev: function(e) {
                    return g(e, "previousSibling")
                  },
                  nextAll: function(e) {
                    return we(e, "nextSibling")
                  },
                  prevAll: function(e) {
                    return we(e, "previousSibling")
                  },
                  nextUntil: function(e, t, n) {
                    return we(e, "nextSibling", n)
                  },
                  prevUntil: function(e, t, n) {
                    return we(e, "previousSibling", n)
                  },
                  siblings: function(e) {
                    return Te((e.parentNode || {}).firstChild, e)
                  },
                  children: function(e) {
                    return Te(e.firstChild)
                  },
                  contents: function(e) {
                    return e.contentDocument || bt.merge([], e.childNodes)
                  }
                }, function(e, t) {
                  bt.fn[e] = function(n, r) {
                    var i = bt.map(this, t, n);
                    return "Until" !== e.slice(-5) && (r = n), r && "string" == typeof r && (i = bt.filter(r,
                        i)), this.length > 1 && (xe[e] || bt.uniqueSort(i), Ce.test(e) && i.reverse()), this
                      .pushStack(i)
                  }
                }), Se = /<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\w:-]+)[^>]*)\/>/gi, Ae =
                /<script|<style|<link/i, Me = /checked\s*(?:[^=]|=\s*.checked.)/i, ke = /^true\/(.*)/, Ne =
                /^\s*<!(?:\[CDATA\[|--)|(?:\]\]|--)>\s*$/g, bt.extend({
                  htmlPrefilter: function(e) {
                    return e.replace(Se, "<$1></$2>")
                  },
                  clone: function e(t, n, r) {
                    var i, o, a, s, e = t.cloneNode(!0),
                      c = bt.contains(t.ownerDocument, t);
                    if (!(gt.noCloneChecked || 1 !== t.nodeType && 11 !== t.nodeType || bt.isXMLDoc(t)))
                      for (s = p(e), a = p(t), i = 0, o = a.length; o > i; i++) $(a[i], s[i]);
                    if (n)
                      if (r)
                        for (a = a || p(t), s = s || p(e), i = 0, o = a.length; o > i; i++) _(a[i], s[i]);
                      else _(t, e);
                    return s = p(e, "script"), s.length > 0 && m(s, !c && p(t, "script")), e
                  },
                  cleanData: function(e) {
                    for (var t, n, r, i = bt.event.special, o = 0; void 0 !== (n = e[o]); o++)
                      if (X(n)) {
                        if (t = n[Q.expando]) {
                          if (t.events)
                            for (r in t.events) i[r] ? bt.event.remove(n, r) : bt.removeEvent(n, r, t.handle);
                          n[Q.expando] = void 0
                        }
                        n[$e.expando] && (n[$e.expando] = void 0)
                      }
                  }
                }), bt.fn.extend({
                  domManip: w,
                  detach: function(e) {
                    return T(this, e, !0)
                  },
                  remove: function(e) {
                    return T(this, e)
                  },
                  text: function(e) {
                    return ve(this, function(e) {
                      return void 0 === e ? bt.text(this) : this.empty().each(function() {
                        (1 === this.nodeType || 11 === this.nodeType || 9 === this.nodeType) && (this
                          .textContent = e)
                      })
                    }, null, e, arguments.length)
                  },
                  append: function() {
                    return w(this, arguments, function(e) {
                      if (1 === this.nodeType || 11 === this.nodeType || 9 === this.nodeType) {
                        var t = y(this, e);
                        t.appendChild(e)
                      }
                    })
                  },
                  prepend: function() {
                    return w(this, arguments, function(e) {
                      if (1 === this.nodeType || 11 === this.nodeType || 9 === this.nodeType) {
                        var t = y(this, e);
                        t.insertBefore(e, t.firstChild)
                      }
                    })
                  },
                  before: function() {
                    return w(this, arguments, function(e) {
                      this.parentNode && this.parentNode.insertBefore(e, this)
                    })
                  },
                  after: function() {
                    return w(this, arguments, function(e) {
                      this.parentNode && this.parentNode.insertBefore(e, this.nextSibling)
                    })
                  },
                  empty: function() {
                    for (var e, t = 0; null != (e = this[t]); t++) 1 === e.nodeType && (bt.cleanData(p(e, !
                      1)), e.textContent = "");
                    return this
                  },
                  clone: function(e, t) {
                    return e = null != e && e, t = null == t ? e : t, this.map(function() {
                      return bt.clone(this, e, t)
                    })
                  },
                  html: function(e) {
                    return ve(this, function(e) {
                      var t = this[0] || {},
                        n = 0,
                        r = this.length;
                      if (void 0 === e && 1 === t.nodeType) return t.innerHTML;
                      if ("string" == typeof e && !Ae.test(e) && !Ee[(ye.exec(e) || ["", ""])[1]
                          .toLowerCase()]) {
                        e = bt.htmlPrefilter(e);
                        try {
                          for (; r > n; n++) t = this[n] || {}, 1 === t.nodeType && (bt.cleanData(p(t, !
                            1)), t.innerHTML = e);
                          t = 0
                        } catch (e) {}
                      }
                      t && this.empty().append(e)
                    }, null, e, arguments.length)
                  },
                  replaceWith: function() {
                    var e = [];
                    return w(this, arguments, function(t) {
                      var n = this.parentNode;
                      bt.inArray(this, e) < 0 && (bt.cleanData(p(this)), n && n.replaceChild(t, this))
                    }, e)
                  }
                }), bt.each({
                  appendTo: "append",
                  prependTo: "prepend",
                  insertBefore: "before",
                  insertAfter: "after",
                  replaceAll: "replaceWith"
                }, function(e, t) {
                  bt.fn[e] = function(e) {
                    for (var n, r = [], i = bt(e), o = i.length - 1, a = 0; o >= a; a++) n = a === o ? this :
                      this.clone(!0), bt(i[a])[t](n), ft.apply(r, n.get());
                    return this.pushStack(r)
                  }
                }), bt.parseHTML = function(e, t, n) {
                  if (!e || "string" != typeof e) return null;
                  "boolean" == typeof t && (n = t, t = !1), t = t || ut;
                  var r = F.exec(e),
                    i = !n && [];
                  return r ? [t.createElement(r[1])] : (r = v([e], t, i), i && i.length && bt(i).remove(), bt
                    .merge([], r.childNodes))
                }, Ie = /^(?:\{[\w\W]*\}|\[[\w\W]*\])$/, Oe = /[A-Z]/g, bt.extend({
                  hasData: function(e) {
                    return $e.hasData(e) || Q.hasData(e)
                  },
                  data: function(e, t, n) {
                    return $e.access(e, t, n)
                  },
                  removeData: function(e, t) {
                    $e.remove(e, t)
                  },
                  _data: function(e, t, n) {
                    return Q.access(e, t, n)
                  },
                  _removeData: function(e, t) {
                    Q.remove(e, t)
                  }
                }), bt.fn.extend({
                  data: function e(t, n) {
                    var r, i, e, o = this[0],
                      a = o && o.attributes;
                    if (void 0 === t) {
                      if (this.length && (e = $e.get(o), 1 === o.nodeType && !Q.get(o, "hasDataAttrs"))) {
                        for (r = a.length; r--;) a[r] && (i = a[r].name, 0 === i.indexOf("data-") && (i = bt
                          .camelCase(i.slice(5)), C(o, i, e[i])));
                        Q.set(o, "hasDataAttrs", !0)
                      }
                      return e
                    }
                    return "object" == typeof t ? this.each(function() {
                      $e.set(this, t)
                    }) : ve(this, function(e) {
                      var n, r;
                      if (o && void 0 === e) {
                        if (n = $e.get(o, t) || $e.get(o, t.replace(Oe, "-$&").toLowerCase()), void 0 !==
                          n) return n;
                        if (r = bt.camelCase(t), n = $e.get(o, r), void 0 !== n) return n;
                        if (n = C(o, r, void 0), void 0 !== n) return n
                      } else r = bt.camelCase(t), this.each(function() {
                        var n = $e.get(this, r);
                        $e.set(this, r, e), t.indexOf("-") > -1 && void 0 !== n && $e.set(this, t, e)
                      })
                    }, null, n, arguments.length > 1, null, !0)
                  },
                  removeData: function(e) {
                    return this.each(function() {
                      $e.remove(this, e)
                    })
                  }
                }), De = /[+-]?(?:\d*\.|)\d+(?:[eE][+-]?\d+|)/.source, Re = /^margin/, Pe = RegExp(
                  "^(?:([+-])=|)(" + De + ")([a-z%]*)$", "i"), Le = RegExp("^(" + De + ")(?!px)[a-z%]+$", "i"),
                Ue = ["Top", "Right", "Bottom", "Left"], Fe = function(e, t) {
                  return e = t || e, "none" === bt.css(e, "display") || !bt.contains(e.ownerDocument, e)
                }, je = function(t) {
                  var n = t.ownerDocument.defaultView;
                  return n && n.opener || (n = e), n.getComputedStyle(t)
                }, He = function(e, t, n, r) {
                  var i, o, a = {};
                  for (o in t) a[o] = e.style[o], e.style[o] = t[o];
                  i = n.apply(e, r || []);
                  for (o in t) e.style[o] = a[o];
                  return i
                }, Be = ut.documentElement,
                function() {
                  function t() {
                    s.style.cssText =
                      "-webkit-box-sizing:border-box;-moz-box-sizing:border-box;box-sizing:border-box;position:relative;display:block;margin:auto;border:1px;padding:1px;top:1%;width:50%",
                      s.innerHTML = "", Be.appendChild(a);
                    var t = e.getComputedStyle(s);
                    n = "1%" !== t.top, o = "2px" === t.marginLeft, r = "4px" === t.width, s.style.marginRight =
                      "50%", i = "4px" === t.marginRight, Be.removeChild(a)
                  }
                  var n, r, i, o, a = ut.createElement("div"),
                    s = ut.createElement("div");
                  s.style && (s.style.backgroundClip = "content-box", s.cloneNode(!0).style.backgroundClip = "",
                    gt.clearCloneStyle = "content-box" === s.style.backgroundClip, a.style.cssText =
                    "border:0;width:8px;height:0;top:0;left:-9999px;padding:0;margin-top:1px;position:absolute",
                    a.appendChild(s), bt.extend(gt, {
                      pixelPosition: function() {
                        return t(), n
                      },
                      boxSizingReliable: function() {
                        return null == r && t(), r
                      },
                      pixelMarginRight: function() {
                        return null == r && t(), i
                      },
                      reliableMarginLeft: function() {
                        return null == r && t(), o
                      },
                      reliableMarginRight: function() {
                        var t, n = s.appendChild(ut.createElement("div"));
                        return n.style.cssText = s.style.cssText =
                          "-webkit-box-sizing:content-box;box-sizing:content-box;display:block;margin:0;border:0;padding:0",
                          n.style.marginRight = n.style.width = "0", s.style.width = "1px", Be.appendChild(
                            a), t = !parseFloat(e.getComputedStyle(n).marginRight), Be.removeChild(a), s
                          .removeChild(n), t
                      }
                    }))
                }(), qe = {
                  HTML: "block",
                  BODY: "block"
                }, bt.fn.ready = function(e) {
                  return bt.ready.promise().done(e), this
                }, bt.extend({
                  isReady: !1,
                  readyWait: 1,
                  holdReady: function(e) {
                    e ? bt.readyWait++ : bt.ready(!0)
                  },
                  ready: function(e) {
                    (e === !0 ? --bt.readyWait : bt.isReady) || (bt.isReady = !0, e !== !0 && --bt.readyWait >
                      0 || (Ge.resolveWith(ut, [bt]), bt.fn.triggerHandler && (bt(ut).triggerHandler("ready"),
                        bt(ut).off("ready"))))
                  }
                }), bt.ready.promise = function(t) {
                  return Ge || (Ge = bt.Deferred(), "complete" === ut.readyState || "loading" !== ut.readyState &&
                    !ut.documentElement.doScroll ? e.setTimeout(bt.ready) : (ut.addEventListener(
                      "DOMContentLoaded", N), e.addEventListener("load", N))), Ge.promise(t)
                }, bt.ready.promise(), Ve = /^(none|table(?!-c[ea]).+)/, We = {
                  position: "absolute",
                  visibility: "hidden",
                  display: "block"
                }, Ye = {
                  letterSpacing: "0",
                  fontWeight: "400"
                }, Ke = ["Webkit", "O", "Moz", "ms"], Xe = ut.createElement("div").style, bt.extend({
                  cssHooks: {
                    opacity: {
                      get: function(e, t) {
                        if (t) {
                          var n = x(e, "opacity");
                          return "" === n ? "1" : n
                        }
                      }
                    }
                  },
                  cssNumber: {
                    animationIterationCount: !0,
                    columnCount: !0,
                    fillOpacity: !0,
                    flexGrow: !0,
                    flexShrink: !0,
                    fontWeight: !0,
                    lineHeight: !0,
                    opacity: !0,
                    order: !0,
                    orphans: !0,
                    widows: !0,
                    zIndex: !0,
                    zoom: !0
                  },
                  cssProps: {
                    float: "cssFloat"
                  },
                  style: function e(t, n, r, i) {
                    if (t && 3 !== t.nodeType && 8 !== t.nodeType && t.style) {
                      var o, a, s, c = bt.camelCase(n),
                        e = t.style;
                      return n = bt.cssProps[c] || (bt.cssProps[c] = I(c) || c), s = bt.cssHooks[n] || bt
                        .cssHooks[c], void 0 === r ? s && "get" in s && void 0 !== (o = s.get(t, !1, i)) ? o :
                        e[n] : (a = typeof r, "string" === a && (o = Pe.exec(r)) && o[1] && (r = S(t, n, o),
                          a = "number"), void(null != r && r === r && ("number" === a && (r += o && o[3] ||
                            (bt.cssNumber[c] ? "" : "px")), gt.clearCloneStyle || "" !== r || 0 !== n
                          .indexOf("background") || (e[n] = "inherit"), s && "set" in s && void 0 === (r =
                            s.set(t, r, i)) || (e[n] = r))))
                    }
                  },
                  css: function(e, t, n, r) {
                    var i, o, a, s = bt.camelCase(t);
                    return t = bt.cssProps[s] || (bt.cssProps[s] = I(s) || s), a = bt.cssHooks[t] || bt
                      .cssHooks[s], a && "get" in a && (i = a.get(e, !0, n)), void 0 === i && (i = x(e, t,
                      r)), "normal" === i && t in Ye && (i = Ye[t]), "" === n || n ? (o = parseFloat(i), n ===
                        !0 || isFinite(o) ? o || 0 : i) : i
                  }
                }), bt.each(["height", "width"], function(e, t) {
                  bt.cssHooks[t] = {
                    get: function(e, n, r) {
                      return n ? Ve.test(bt.css(e, "display")) && 0 === e.offsetWidth ? He(e, We,
                    function() {
                        return R(e, t, r)
                      }) : R(e, t, r) : void 0
                    },
                    set: function(e, n, r) {
                      var i, o = r && je(e),
                        a = r && D(e, t, r, "border-box" === bt.css(e, "boxSizing", !1, o), o);
                      return a && (i = Pe.exec(n)) && "px" !== (i[3] || "px") && (e.style[t] = n, n = bt
                        .css(e, t)), O(e, n, a)
                    }
                  }
                }), bt.cssHooks.marginLeft = k(gt.reliableMarginLeft, function(e, t) {
                  return t ? (parseFloat(x(e, "marginLeft")) || e.getBoundingClientRect().left - He(e, {
                    marginLeft: 0
                  }, function() {
                    return e.getBoundingClientRect().left
                  })) + "px" : void 0
                }), bt.cssHooks.marginRight = k(gt.reliableMarginRight, function(e, t) {
                  return t ? He(e, {
                    display: "inline-block"
                  }, x, [e, "marginRight"]) : void 0
                }), bt.each({
                  margin: "",
                  padding: "",
                  border: "Width"
                }, function(e, t) {
                  bt.cssHooks[e + t] = {
                    expand: function(n) {
                      for (var r = 0, i = {}, o = "string" == typeof n ? n.split(" ") : [n]; 4 > r; r++) i[
                        e + Ue[r] + t] = o[r] || o[r - 2] || o[0];
                      return i
                    }
                  }, Re.test(e) || (bt.cssHooks[e + t].set = O)
                }), bt.fn.extend({
                  css: function(e, t) {
                    return ve(this, function(e, t, n) {
                      var r, i, o = {},
                        a = 0;
                      if (bt.isArray(t)) {
                        for (r = je(e), i = t.length; i > a; a++) o[t[a]] = bt.css(e, t[a], !1, r);
                        return o
                      }
                      return void 0 !== n ? bt.style(e, t, n) : bt.css(e, t)
                    }, e, t, arguments.length > 1)
                  },
                  show: function() {
                    return P(this, !0)
                  },
                  hide: function() {
                    return P(this)
                  },
                  toggle: function(e) {
                    return "boolean" == typeof e ? e ? this.show() : this.hide() : this.each(function() {
                      Fe(this) ? bt(this).show() : bt(this).hide()
                    })
                  }
                }),
                function() {
                  var e = ut.createElement("input"),
                    t = ut.createElement("select"),
                    n = t.appendChild(ut.createElement("option"));
                  e.type = "checkbox", gt.checkOn = "" !== e.value, gt.optSelected = n.selected, t.disabled = !0,
                    gt.optDisabled = !n.disabled, e = ut.createElement("input"), e.value = "t", e.type = "radio",
                    gt.radioValue = "t" === e.value
                }(), Je = bt.expr.attrHandle, bt.fn.extend({
                  attr: function(e, t) {
                    return ve(this, bt.attr, e, t, arguments.length > 1)
                  },
                  removeAttr: function(e) {
                    return this.each(function() {
                      bt.removeAttr(this, e)
                    })
                  }
                }), bt.extend({
                  attr: function(e, t, n) {
                    var r, i, o = e.nodeType;
                    if (3 !== o && 8 !== o && 2 !== o) return void 0 === e.getAttribute ? bt.prop(e, t, n) : (
                      1 === o && bt.isXMLDoc(e) || (t = t.toLowerCase(), i = bt.attrHooks[t] || (bt.expr
                        .match.bool.test(t) ? Qe : void 0)), void 0 !== n ? null === n ? void bt
                      .removeAttr(e, t) : i && "set" in i && void 0 !== (r = i.set(e, n, t)) ? r : (e
                        .setAttribute(t, n + ""), n) : i && "get" in i && null !== (r = i.get(e, t)) ? r :
                      (r = bt.find.attr(e, t), null == r ? void 0 : r))
                  },
                  attrHooks: {
                    type: {
                      set: function(e, t) {
                        if (!gt.radioValue && "radio" === t && bt.nodeName(e, "input")) {
                          var n = e.value;
                          return e.setAttribute("type", t), n && (e.value = n), t
                        }
                      }
                    }
                  },
                  removeAttr: function(e, t) {
                    var n, r, i = 0,
                      o = t && t.match(V);
                    if (o && 1 === e.nodeType)
                      for (; n = o[i++];) r = bt.propFix[n] || n, bt.expr.match.bool.test(n) && (e[r] = !1), e
                        .removeAttribute(n)
                  }
                }), Qe = {
                  set: function(e, t, n) {
                    return t === !1 ? bt.removeAttr(e, n) : e.setAttribute(n, n), n
                  }
                }, bt.each(bt.expr.match.bool.source.match(/\w+/g), function(e, t) {
                  var n = Je[t] || bt.find.attr;
                  Je[t] = function(e, t, r) {
                    var i, o;
                    return r || (o = Je[t], Je[t] = i, i = null != n(e, t, r) ? t.toLowerCase() : null, Je[
                      t] = o), i
                  }
                }), Ze = /^(?:input|select|textarea|button)$/i, et = /^(?:a|area)$/i, bt.fn.extend({
                  prop: function(e, t) {
                    return ve(this, bt.prop, e, t, arguments.length > 1)
                  },
                  removeProp: function(e) {
                    return this.each(function() {
                      delete this[bt.propFix[e] || e]
                    })
                  }
                }), bt.extend({
                  prop: function(e, t, n) {
                    var r, i, o = e.nodeType;
                    if (3 !== o && 8 !== o && 2 !== o) return 1 === o && bt.isXMLDoc(e) || (t = bt.propFix[
                        t] || t, i = bt.propHooks[t]), void 0 !== n ? i && "set" in i && void 0 !== (r = i
                        .set(e, n, t)) ? r : e[t] = n : i && "get" in i && null !== (r = i.get(e, t)) ? r :
                      e[t]
                  },
                  propHooks: {
                    tabIndex: {
                      get: function(e) {
                        var t = bt.find.attr(e, "tabindex");
                        return t ? parseInt(t, 10) : Ze.test(e.nodeName) || et.test(e.nodeName) && e.href ?
                          0 : -1
                      }
                    }
                  },
                  propFix: {
                    for: "htmlFor",
                    class: "className"
                  }
                }), gt.optSelected || (bt.propHooks.selected = {
                  get: function(e) {
                    var t = e.parentNode;
                    return t && t.parentNode && t.parentNode.selectedIndex, null
                  },
                  set: function(e) {
                    var t = e.parentNode;
                    t && (t.selectedIndex, t && t.parentNode && t.parentNode.selectedIndex)
                  }
                }), bt.each(["tabIndex", "readOnly", "maxLength", "cellSpacing", "cellPadding", "rowSpan",
                  "colSpan", "useMap", "frameBorder", "contentEditable"
                ], function() {
                  bt.propFix[this.toLowerCase()] = this
                }), tt = /[\t\r\n\f]/g, bt.fn.extend({
                  addClass: function(e) {
                    var t, n, r, i, o, a, s, c = 0;
                    if (bt.isFunction(e)) return this.each(function(t) {
                      bt(this).addClass(e.call(this, t, L(this)))
                    });
                    if ("string" == typeof e && e)
                      for (t = e.match(V) || []; n = this[c++];)
                        if (i = L(n), r = 1 === n.nodeType && (" " + i + " ").replace(tt, " ")) {
                          for (a = 0; o = t[a++];) r.indexOf(" " + o + " ") < 0 && (r += o + " ");
                          s = bt.trim(r), i !== s && n.setAttribute("class", s)
                        } return this
                  },
                  removeClass: function(e) {
                    var t, n, r, i, o, a, s, c = 0;
                    if (bt.isFunction(e)) return this.each(function(t) {
                      bt(this).removeClass(e.call(this, t, L(this)))
                    });
                    if (!arguments.length) return this.attr("class", "");
                    if ("string" == typeof e && e)
                      for (t = e.match(V) || []; n = this[c++];)
                        if (i = L(n), r = 1 === n.nodeType && (" " + i + " ").replace(tt, " ")) {
                          for (a = 0; o = t[a++];)
                            for (; r.indexOf(" " + o + " ") > -1;) r = r.replace(" " + o + " ", " ");
                          s = bt.trim(r), i !== s && n.setAttribute("class", s)
                        } return this
                  },
                  toggleClass: function(e, t) {
                    var n = typeof e;
                    return "boolean" == typeof t && "string" === n ? t ? this.addClass(e) : this.removeClass(
                      e) : bt.isFunction(e) ? this.each(function(n) {
                      bt(this).toggleClass(e.call(this, n, L(this), t), t)
                    }) : this.each(function() {
                      var t, r, i, o;
                      if ("string" === n)
                        for (r = 0, i = bt(this), o = e.match(V) || []; t = o[r++];) i.hasClass(t) ? i
                          .removeClass(t) : i.addClass(t);
                      else(void 0 === e || "boolean" === n) && (t = L(this), t && Q.set(this,
                        "__className__", t), this.setAttribute && this.setAttribute("class", t ||
                        e === !1 ? "" : Q.get(this, "__className__") || ""))
                    })
                  },
                  hasClass: function(e) {
                    var t, n, r = 0;
                    for (t = " " + e + " "; n = this[r++];)
                      if (1 === n.nodeType && (" " + L(n) + " ").replace(tt, " ").indexOf(t) > -1) return !0;
                    return !1
                  }
                }), nt = /\r/g, bt.fn.extend({
                  val: function(e) {
                    var t, n, r, i = this[0];
                    return arguments.length ? (r = bt.isFunction(e), this.each(function(n) {
                      var i;
                      1 === this.nodeType && (i = r ? e.call(this, n, bt(this).val()) : e, null == i ?
                        i = "" : "number" == typeof i ? i += "" : bt.isArray(i) && (i = bt.map(i,
                          function(e) {
                            return null == e ? "" : e + ""
                          })), t = bt.valHooks[this.type] || bt.valHooks[this.nodeName.toLowerCase()],
                        t && "set" in t && void 0 !== t.set(this, i, "value") || (this.value = i))
                    })) : i ? (t = bt.valHooks[i.type] || bt.valHooks[i.nodeName.toLowerCase()], t &&
                      "get" in t && void 0 !== (n = t.get(i, "value")) ? n : (n = i.value, "string" ==
                        typeof n ? n.replace(nt, "") : null == n ? "" : n)) : void 0
                  }
                }), bt.extend({
                  valHooks: {
                    option: {
                      get: function(e) {
                        return bt.trim(e.value)
                      }
                    },
                    select: {
                      get: function(e) {
                        for (var t, n, r = e.options, i = e.selectedIndex, o = "select-one" === e.type || 0 >
                            i, a = o ? null : [], s = o ? i + 1 : r.length, c = 0 > i ? s : o ? i : 0; s >
                          c; c++)
                          if (n = r[c], (n.selected || c === i) && (gt.optDisabled ? !n.disabled : null === n
                              .getAttribute("disabled")) && (!n.parentNode.disabled || !bt.nodeName(n
                              .parentNode, "optgroup"))) {
                            if (t = bt(n).val(), o) return t;
                            a.push(t)
                          } return a
                      },
                      set: function(e, t) {
                        for (var n, r, i = e.options, o = bt.makeArray(t), a = i.length; a--;) r = i[a], (r
                          .selected = bt.inArray(bt.valHooks.option.get(r), o) > -1) && (n = !0);
                        return n || (e.selectedIndex = -1), o
                      }
                    }
                  }
                }), bt.each(["radio", "checkbox"], function() {
                  bt.valHooks[this] = {
                    set: function(e, t) {
                      return bt.isArray(t) ? e.checked = bt.inArray(bt(e).val(), t) > -1 : void 0
                    }
                  }, gt.checkOn || (bt.valHooks[this].get = function(e) {
                    return null === e.getAttribute("value") ? "on" : e.value
                  })
                }), rt = /%20/g, it = /\[\]$/, ot = /\r?\n/g, at = /^(?:submit|button|image|reset|file)$/i, st =
                /^(?:input|select|textarea|keygen)/i, bt.param = function(e, t) {
                  var n, r = [],
                    i = function(e, t) {
                      t = bt.isFunction(t) ? t() : null == t ? "" : t, r[r.length] = encodeURIComponent(e) + "=" +
                        encodeURIComponent(t)
                    };
                  if (void 0 === t && (t = bt.ajaxSettings && bt.ajaxSettings.traditional), bt.isArray(e) || e
                    .jquery && !bt.isPlainObject(e)) bt.each(e, function() {
                    i(this.name, this.value)
                  });
                  else
                    for (n in e) U(n, e[n], t, i);
                  return r.join("&").replace(rt, "+")
                }, bt.fn.extend({
                  serialize: function() {
                    return bt.param(this.serializeArray())
                  },
                  serializeArray: function() {
                    return this.map(function() {
                      var e = bt.prop(this, "elements");
                      return e ? bt.makeArray(e) : this
                    }).filter(function() {
                      var e = this.type;
                      return this.name && !bt(this).is(":disabled") && st.test(this.nodeName) && !at.test(
                        e) && (this.checked || !ge.test(e))
                    }).map(function(e, t) {
                      var n = bt(this).val();
                      return null == n ? null : bt.isArray(n) ? bt.map(n, function(e) {
                        return {
                          name: t.name,
                          value: e.replace(ot, "\r\n")
                        }
                      }) : {
                        name: t.name,
                        value: n.replace(ot, "\r\n")
                      }
                    }).get()
                  }
                }), bt
            });
          t(Is), n(Is), Qr = 6e4, Jr = "mbox-name-", Zr = "mboxDefault", ei = "/m2/{clientCode}/mbox/json", ti =
            250, ni = "Mbox name is not present or is too long.", ri = "the mbox environment is disabled.", ii =
            "-clicked", oi = "x-only", ai = "disabled", si = "mboxedge", ci = "Content container not found", ui =
            /^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/,
            li = /(?:^|&)([^&=]*)=?([^&]*)/gi, di = ["source", "protocol", "authority", "userInfo", "user",
              "password", "host", "port", "relative", "path", "directory", "file", "query", "anchor"
            ], fi = "at-data-src", hi = "at-script-marker", pi = "at-id-body-style", mi = "at-flicker-control", vi =
            "at-id-default-content-style", gi = "redirect:event", yi = "show:body", bi = "ready:dom", Ei = "script",
            _i = function(e, t) {
              return de(e).find(t)
            }, $i = "mboxMCAVID", wi = "mboxAAMB", Ti = "mboxMCGLH", Ci = "mboxMCGVID", xi = "mboxMCSDID", Si =
            "colorDepth", Ai = "screenHeight", Mi = "screenWidth", ki = "browserHeight", Ni = "browserTimeOffset",
            Ii = "browserWidth", Oi = "mboxCallback", Di = "mboxTarget", Ri = "clickTrackId", Pi = "mboxXDomain",
            Li = "mboxCount", Ui = "mboxHost", Fi = "mbox", ji = "mboxPage", Hi = "mboxSession", Bi =
            "mboxReferrer", zi = "mboxTime", qi = "mboxPC", Gi = "mboxURL", Vi = "mboxVersion", Wi = function() {},
            Yi = "//cdn.tt.omtrdc.net/cdn/target-vec.js", Ki = "success", Xi = "warning", Qi = "error", Ji =
            "optout", Zi = !1, eo = !0, to = "mboxDebug", no = "mboxDisable", ro = "mboxEdit", io = "[Target]", oo =
            "check", ao = "mbox", so = "PC", co = "session", uo = "mboxEdgeServer", lo = "mboxDisabled", fo =
            RegExp("('|\")"), ho = "https://", po = "Visitor Api:", mo = "vst.", vo = mo + "trk", go = mo + "trks",
            yo = "Options argument is required", bo = {
              MBOX_PARAM_VALIDATOR: Et("mbox", Tt, ni),
              OPTIONS_IS_REQUIRED: _t(F, yo),
              URL_PARAM_IS_MANDATORY: Et("url", k, $t("url")),
              SUCCESS_PARAM_IS_MANDATORY: Et("success", P, $t("success")),
              ERROR_PARAM_IS_MANDATORY: Et("error", P, $t("error")),
              MBOX_OPTION_PARAM_VALIDATOR: Et("mbox", Tt, ni)
            }, Eo = "executeAjax():", _o = Eo + " jsonp param requires type param to be jsonp", $o = Eo +
            ' unknown method "{0}"', wo = Eo + ' unknown type "{0}"', To = Eo + " timeout param is not a number",
            Co = Eo + ' invalid method "{0}" for request type "{1}"', xo = Eo + " invalid params, should be object",
            So = "mbox", Ao = "type", Mo = "method", ko = "jsonp", No = "params", Io = "timeout", Oo = [bo
              .OPTIONS_IS_REQUIRED, bo.URL_PARAM_IS_MANDATORY
            ], Do = {
              JSON: "json",
              JSONP: "jsonp",
              SCRIPT: "script"
            }, Ro = {
              GET: "get",
              POST: "post"
            }, Po = {
              valid: function(e) {
                var t = e[Ao];
                return D(Do[t.toUpperCase()]) ? De(wo.replace("{0}", t)) : Pe()
              }
            }, Lo = {
              valid: function(e) {
                var t = e[Mo],
                  n = e[Ao];
                return k(t) ? D(Ro[t.toUpperCase()]) ? De($o.replace("{0}", t)) : t.toUpperCase() !== Ro.GET &&
                  n !== Do.JSON ? De(Co.replace("{0}", t).replace("{1}", n)) : Pe() : Pe()
              }
            }, Uo = {
              valid: function(e) {
                return k(e[ko]) && e[Ao] !== Do.JSONP ? De(_o) : Pe()
              }
            }, Fo = {
              valid: function(e) {
                var t = e[Io];
                return D(t) || O(t) ? Pe() : De(To)
              }
            }, jo = {
              valid: function(e) {
                var t = e[No];
                return F(t) || D(t) ? Pe() : F(t) ? void 0 : De(xo)
              }
            }, Ho = [Po, Lo, Uo, Fo, jo], Bo = "Visitor ID opt-out enabled", zo = "Track Event:", qo =
            "Invalid element: expect object with href attribute.", Go =
            "Invalid initialization. Cannot access document.location.", Vo = "DEFINED-BEHAVIOR-BUILDER:", Wo =
            'cannot preventDefault. Unsupported event: "{0}" for "{1}" element.', Yo =
            "undefined element type or event type.", Ko = 'cannot preventDefault. Unsupported tag: "{0}".', Xo = {},
            Qo = {}, Jo = "fetch()", Zo = "script", ea = "img", ta = {
              SET_CONTENT: "setContent",
              SET_ATTRIBUTE: "setAttribute",
              SET_STYLE: "setStyle",
              REARRANGE: "rearrange",
              RESIZE: "resize",
              MOVE: "move",
              REMOVE: "remove",
              CUSTOM_CODE: "customCode",
              APPEND_CONTENT: "appendContent",
              REDIRECT: "redirect",
              TRACK_CLICK: "trackClick",
              INSERT_BEFORE: "insertBefore",
              INSERT_AFTER: "insertAfter",
              PREPEND_CONTENT: "prependContent",
              REPLACE_CONTENT: "replaceContent"
            }, na = {
              ACTION: "action",
              ATTRIBUTE: "attribute",
              ASSET: "asset",
              CLICK_TRACK_ID: "clickTrackId",
              CONTENT: "content",
              CONTENT_TYPE: "contentType",
              INCLUDE_ALL_URL_PARAMETERS: "includeAllUrlParameters",
              FINAL_HEIGHT: "finalHeight",
              FINAL_LEFT_POSITION: "finalLeftPosition",
              FINAL_TOP_POSITION: "finalTopPosition",
              FINAL_WIDTH: "finalWidth",
              FROM: "from",
              PASS_MBOX_SESSION: "passMboxSession",
              POSITION: "position",
              PRIORITY: "priority",
              PROPERTY: "property",
              SELECTOR: "selector",
              CSS_SELECTOR: "cssSelector",
              TO: "to",
              URL: "url",
              VALUE: "value"
            }, ra = {
              IMPORTANT: "important"
            }, ia = {
              HTML: "html",
              TEXT: "text"
            }, oa = "script,style,link", aa = "click", sa = "a", ca = "at-request-succeeded", ua =
            "at-request-failed", la = "at-content-rendering-succeeded", da = "at-content-rendering-failed", fa =
            /CLKTRK#(\S+)/, ha = /CLKTRK#(\S+)\s/, pa = 50, ma = "applied:", va = "polling:end", ga = "target", ya =
            "traces", ba = "___" + ga + "_" + ya, Ea = 86400, _a = "3rd party cookies disabled", $a =
            "applyOffer():", wa = "Either element or selector is redundant", Ta = "offer parameter is mandatory",
            Ca = [bo.OPTIONS_IS_REQUIRED, Et("offer", R, Ta), _t(Tr, wa)], xa = [bo.OPTIONS_IS_REQUIRED, bo
              .MBOX_OPTION_PARAM_VALIDATOR
            ], Sa = "getOffer():", Aa = [bo.OPTIONS_IS_REQUIRED, bo.MBOX_OPTION_PARAM_VALIDATOR], Ma =
            "Track Event:", ka = [bo.MBOX_PARAM_VALIDATOR], Na = "Classic:", Ia =
            "DOM node ID not provided for mbox:", Oa = "Unable to load target-vec.js for experience creation.", Da =
            "ext", Ra = RegExp("^[a-zA-Z]+$"), Pa = [bo.SUCCESS_PARAM_IS_MANDATORY, bo.ERROR_PARAM_IS_MANDATORY],
            La = "getOffer():", Ua = "success callback throws error", Fa = "error callback throws error", ja = [So,
              Io, No
            ], Ha = window, Ba = Ha.document, za = Se(Ha, e), qa = za.generateId(), Ga = Ne(ke(Ha, Ba), Ba, qa, e),
            Va = je(Ha, Ba, za), Wa = Xe(Ba), Ya = Ze(Wa, e), Ka = rt(Ba, Wa, za, e), Xa = dt(Ba, Ga, Ka, Ya, za,
            e), Qa = vt(Ba, za, e), Ja = gt(Ha, Ba, Wa, za, e, Ka, Ya), Za = yt(), es = bt(Ha, Va, e), ts = Ct(za,
              Va, e), ns = St(xt(Ja, Xa, ts), es), rs = At(ns, za, Va, e), is = Dt(Ba, Va), os = Rt(za, Va), as =
            Gt(os), ss = Vt(), cs = Kt(rs, Wt(Va), is), us = tn(as, ss, cs, Ja), ls = rn(ss, cs), ds = an(us), fs =
            cn(za), hs = Kt(rs, un(e), is), ps = Ht(os, za, hs, Va), ms = Pn(Ha, Ka), vs = Ln(ms), gs = Un(Ha), ys =
            Fn(ps, Va, Ja), bs = jn(ys, Ja, gs, za, Va), Es = qn(ss, vs, ps, ys, za, Va, Ja, bs), _s = [fs, Es, ds,
              ls
            ], $s = ur(Qa), ws = dr($s, _s), Ha.adobe = Ha.adobe || {}, Ha.adobe.target = {}, Ha.adobe.target
            .VERSION = e.version, Ts = {
              tntId: _r(Ba, e, Ya),
              sessionId: vr(Ka),
              error: mr(Ja),
              disabled: pr(Ba, Ja),
              trace: hr(Ha)
            }, Cs = $r(), xs = wr(Ts, za), Ss = xr(_s, Va), As = Ar(Ja, ns, xs, $s, za, Va, e), Ms = Rr(Ja, Ha, rs,
              is, za, Va, e), Ha.adobe.target.applyOffer = Ss, Ha.adobe.target.trackEvent = Ms, ks = Lr(Ja, Za, ns,
              xs, ws, za, Va, Cs), Ha.mboxDefine = ks.createMbox, Ha.mboxUpdate = ks.fetchAndDisplayMbox, Ha
            .mboxCreate = ks.createFetchAndDisplayMbox, Fr(Ja, Ha, ts, Va), jr(Ja, Cs, As, Ss, za, e), Hr(Ha.adobe
              .target), Xr(Ha.adobe.target, As, Cs, Va), za.onDomReady(za.triggerDomReady()), Ja.isEnabled() && Ce()
        }(this.adobe = {})
      }({
        clientCode: "nvidia",
        imsOrgId: "F207D74D549850760A4C98C6@AdobeOrg",
        serverDomain: "nvidia.tt.omtrdc.net",
        crossDomain: "x-only",
        timeout: 15e3,
        globalMboxName: "target-global-mbox",
        globalMboxAutoCreate: !0,
        version: "0.9.3",
        defaultContentHiddenStyle: "visibility:hidden;",
        defaultContentVisibleStyle: "visibility:visible;",
        bodyHiddenStyle: "body{opacity:0}",
        bodyHidingEnabled: !0,
        deviceIdLifetime: 632448e5,
        sessionIdLifetime: 186e4,
        pollingAfterDomReadyTimeout: 18e4,
        visitorApiTimeout: 2e3,
        overrideMboxEdgeServer: !1,
        overrideMboxEdgeServerTimeout: 186e4,
        optoutEnabled: !1
      })
  }).call(t, function() {
    return this
  }())
}
