// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 149
// role       : provider $compile | value $rootElement | value $locale
// defines    : angular.module("ngLocale")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  /**
   * @license AngularJS v1.5.5
   * (c) 2010-2016 Google, Inc. http://angularjs.org
   * License: MIT
   */
  ! function(e) {
    "use strict";

    function t(e, t) {
      return t = t || Error,
        function() {
          var n, r, i = 2,
            o = arguments,
            a = o[0],
            s = "[" + (e ? e + ":" : "") + a + "] ",
            c = o[1];
          for (s += c.replace(/\{\d+\}/g, function(e) {
              var t = +e.slice(1, -1),
                n = t + i;
              return n < o.length ? ye(o[n]) : e
            }), s += "\nhttp://errors.angularjs.org/1.5.5/" + (e ? e + "/" : "") + a, r = i, n = "?"; r < o.length; r++,
            n = "&") s += n + "p" + (r - i) + "=" + encodeURIComponent(ye(o[r]));
          return new t(s)
        }
    }

    function n(e) {
      if (null == e || x(e)) return !1;
      if (Vr(e) || _(e) || Rr && e instanceof Rr) return !0;
      var t = "length" in Object(e) && e.length;
      return $(t) && (t >= 0 && (t - 1 in e || e instanceof Array) || "function" == typeof e.item)
    }

    function r(e, t, i) {
      var o, a;
      if (e)
        if (T(e))
          for (o in e) "prototype" == o || "length" == o || "name" == o || e.hasOwnProperty && !e.hasOwnProperty(o) || t
            .call(i, e[o], o, e);
        else if (Vr(e) || n(e)) {
        var s = "object" != typeof e;
        for (o = 0, a = e.length; o < a; o++)(s || o in e) && t.call(i, e[o], o, e)
      } else if (e.forEach && e.forEach !== r) e.forEach(t, i, e);
      else if (E(e))
        for (o in e) t.call(i, e[o], o, e);
      else if ("function" == typeof e.hasOwnProperty)
        for (o in e) e.hasOwnProperty(o) && t.call(i, e[o], o, e);
      else
        for (o in e) Mr.call(e, o) && t.call(i, e[o], o, e);
      return e
    }

    function i(e, t, n) {
      for (var r = Object.keys(e).sort(), i = 0; i < r.length; i++) t.call(n, e[r[i]], r[i]);
      return r
    }

    function o(e) {
      return function(t, n) {
        e(n, t)
      }
    }

    function a() {
      return ++Gr
    }

    function s(e, t) {
      t ? e.$$hashKey = t : delete e.$$hashKey
    }

    function c(e, t, n) {
      for (var r = e.$$hashKey, i = 0, o = t.length; i < o; ++i) {
        var a = t[i];
        if (b(a) || T(a))
          for (var u = Object.keys(a), l = 0, d = u.length; l < d; l++) {
            var f = u[l],
              h = a[f];
            n && b(h) ? w(h) ? e[f] = new Date(h.valueOf()) : C(h) ? e[f] = new RegExp(h) : h.nodeName ? e[f] = h
              .cloneNode(!0) : R(h) ? e[f] = h.clone() : (b(e[f]) || (e[f] = Vr(h) ? [] : {}), c(e[f], [h], !0)) : e[
              f] = h
          }
      }
      return s(e, r), e
    }

    function u(e) {
      return c(e, Ur.call(arguments, 1), !1)
    }

    function l(e) {
      return c(e, Ur.call(arguments, 1), !0)
    }

    function d(e) {
      return parseInt(e, 10)
    }

    function f(e, t) {
      return u(Object.create(e), t)
    }

    function h() {}

    function p(e) {
      return e
    }

    function m(e) {
      return function() {
        return e
      }
    }

    function v(e) {
      return T(e.toString) && e.toString !== Hr
    }

    function g(e) {
      return "undefined" == typeof e
    }

    function y(e) {
      return "undefined" != typeof e
    }

    function b(e) {
      return null !== e && "object" == typeof e
    }

    function E(e) {
      return null !== e && "object" == typeof e && !Br(e)
    }

    function _(e) {
      return "string" == typeof e
    }

    function $(e) {
      return "number" == typeof e
    }

    function w(e) {
      return "[object Date]" === Hr.call(e)
    }

    function T(e) {
      return "function" == typeof e
    }

    function C(e) {
      return "[object RegExp]" === Hr.call(e)
    }

    function x(e) {
      return e && e.window === e
    }

    function S(e) {
      return e && e.$evalAsync && e.$watch
    }

    function A(e) {
      return "[object File]" === Hr.call(e)
    }

    function M(e) {
      return "[object FormData]" === Hr.call(e)
    }

    function k(e) {
      return "[object Blob]" === Hr.call(e)
    }

    function N(e) {
      return "boolean" == typeof e
    }

    function I(e) {
      return e && T(e.then)
    }

    function O(e) {
      return e && $(e.length) && Wr.test(Hr.call(e))
    }

    function D(e) {
      return "[object ArrayBuffer]" === Hr.call(e)
    }

    function R(e) {
      return !(!e || !(e.nodeName || e.prop && e.attr && e.find))
    }

    function P(e) {
      var t, n = {},
        r = e.split(",");
      for (t = 0; t < r.length; t++) n[r[t]] = !0;
      return n
    }

    function L(e) {
      return kr(e.nodeName || e[0] && e[0].nodeName)
    }

    function U(e, t) {
      var n = e.indexOf(t);
      return n >= 0 && e.splice(n, 1), n
    }

    function F(e, t) {
      function n(e, t) {
        var n, r = t.$$hashKey;
        if (Vr(e))
          for (var o = 0, a = e.length; o < a; o++) t.push(i(e[o]));
        else if (E(e))
          for (n in e) t[n] = i(e[n]);
        else if (e && "function" == typeof e.hasOwnProperty)
          for (n in e) e.hasOwnProperty(n) && (t[n] = i(e[n]));
        else
          for (n in e) Mr.call(e, n) && (t[n] = i(e[n]));
        return s(t, r), t
      }

      function i(e) {
        if (!b(e)) return e;
        var t = a.indexOf(e);
        if (t !== -1) return c[t];
        if (x(e) || S(e)) throw zr("cpws", "Can't copy! Making copies of Window or Scope instances is not supported.");
        var r = !1,
          i = o(e);
        return void 0 === i && (i = Vr(e) ? [] : Object.create(Br(e)), r = !0), a.push(e), c.push(i), r ? n(e, i) : i
      }

      function o(e) {
        switch (Hr.call(e)) {
          case "[object Int8Array]":
          case "[object Int16Array]":
          case "[object Int32Array]":
          case "[object Float32Array]":
          case "[object Float64Array]":
          case "[object Uint8Array]":
          case "[object Uint8ClampedArray]":
          case "[object Uint16Array]":
          case "[object Uint32Array]":
            return new e.constructor(i(e.buffer));
          case "[object ArrayBuffer]":
            if (!e.slice) {
              var t = new ArrayBuffer(e.byteLength);
              return new Uint8Array(t).set(new Uint8Array(e)), t
            }
            return e.slice(0);
          case "[object Boolean]":
          case "[object Number]":
          case "[object String]":
          case "[object Date]":
            return new e.constructor(e.valueOf());
          case "[object RegExp]":
            var n = new RegExp(e.source, e.toString().match(/[^\/]*$/)[0]);
            return n.lastIndex = e.lastIndex, n;
          case "[object Blob]":
            return new e.constructor([e], {
              type: e.type
            })
        }
        if (T(e.cloneNode)) return e.cloneNode(!0)
      }
      var a = [],
        c = [];
      if (t) {
        if (O(t) || D(t)) throw zr("cpta", "Can't copy! TypedArray destination cannot be mutated.");
        if (e === t) throw zr("cpi", "Can't copy! Source and destination are identical.");
        return Vr(t) ? t.length = 0 : r(t, function(e, n) {
          "$$hashKey" !== n && delete t[n]
        }), a.push(e), c.push(t), n(e, t)
      }
      return i(e)
    }

    function j(e, t) {
      if (Vr(e)) {
        t = t || [];
        for (var n = 0, r = e.length; n < r; n++) t[n] = e[n]
      } else if (b(e)) {
        t = t || {};
        for (var i in e) "$" === i.charAt(0) && "$" === i.charAt(1) || (t[i] = e[i])
      }
      return t || e
    }

    function H(e, t) {
      if (e === t) return !0;
      if (null === e || null === t) return !1;
      if (e !== e && t !== t) return !0;
      var n, r, i, o = typeof e,
        a = typeof t;
      if (o == a && "object" == o) {
        if (!Vr(e)) {
          if (w(e)) return !!w(t) && H(e.getTime(), t.getTime());
          if (C(e)) return !!C(t) && e.toString() == t.toString();
          if (S(e) || S(t) || x(e) || x(t) || Vr(t) || w(t) || C(t)) return !1;
          i = me();
          for (r in e)
            if ("$" !== r.charAt(0) && !T(e[r])) {
              if (!H(e[r], t[r])) return !1;
              i[r] = !0
            } for (r in t)
            if (!(r in i) && "$" !== r.charAt(0) && y(t[r]) && !T(t[r])) return !1;
          return !0
        }
        if (!Vr(t)) return !1;
        if ((n = e.length) == t.length) {
          for (r = 0; r < n; r++)
            if (!H(e[r], t[r])) return !1;
          return !0
        }
      }
      return !1
    }

    function B(e, t, n) {
      return e.concat(Ur.call(t, n))
    }

    function z(e, t) {
      return Ur.call(e, t || 0)
    }

    function q(e, t) {
      var n = arguments.length > 2 ? z(arguments, 2) : [];
      return !T(t) || t instanceof RegExp ? t : n.length ? function() {
        return arguments.length ? t.apply(e, B(n, arguments, 0)) : t.apply(e, n)
      } : function() {
        return arguments.length ? t.apply(e, arguments) : t.call(e)
      }
    }

    function G(t, n) {
      var r = n;
      return "string" == typeof t && "$" === t.charAt(0) && "$" === t.charAt(1) ? r = void 0 : x(n) ? r = "$WINDOW" :
        n && e.document === n ? r = "$DOCUMENT" : S(n) && (r = "$SCOPE"), r
    }

    function V(e, t) {
      if (!g(e)) return $(t) || (t = t ? 2 : null), JSON.stringify(e, G, t)
    }

    function W(e) {
      return _(e) ? JSON.parse(e) : e
    }

    function Y(e, t) {
      e = e.replace(Jr, "");
      var n = Date.parse("Jan 01, 1970 00:00:00 " + e) / 6e4;
      return isNaN(n) ? t : n
    }

    function K(e, t) {
      return e = new Date(e.getTime()), e.setMinutes(e.getMinutes() + t), e
    }

    function X(e, t, n) {
      n = n ? -1 : 1;
      var r = e.getTimezoneOffset(),
        i = Y(t, r);
      return K(e, n * (i - r))
    }

    function Q(e) {
      e = Rr(e).clone();
      try {
        e.empty()
      } catch (e) {}
      var t = Rr("<div>").append(e).html();
      try {
        return e[0].nodeType === ii ? kr(t) : t.match(/^(<[^>]+>)/)[1].replace(/^<([\w\-]+)/, function(e, t) {
          return "<" + kr(t)
        })
      } catch (e) {
        return kr(t)
      }
    }

    function J(e) {
      try {
        return decodeURIComponent(e)
      } catch (e) {}
    }

    function Z(e) {
      var t = {};
      return r((e || "").split("&"), function(e) {
        var n, r, i;
        e && (r = e = e.replace(/\+/g, "%20"), n = e.indexOf("="), n !== -1 && (r = e.substring(0, n), i = e
          .substring(n + 1)), r = J(r), y(r) && (i = !y(i) || J(i), Mr.call(t, r) ? Vr(t[r]) ? t[r].push(i) : t[
          r] = [t[r], i] : t[r] = i))
      }), t
    }

    function ee(e) {
      var t = [];
      return r(e, function(e, n) {
        Vr(e) ? r(e, function(e) {
          t.push(ne(n, !0) + (e === !0 ? "" : "=" + ne(e, !0)))
        }) : t.push(ne(n, !0) + (e === !0 ? "" : "=" + ne(e, !0)))
      }), t.length ? t.join("&") : ""
    }

    function te(e) {
      return ne(e, !0).replace(/%26/gi, "&").replace(/%3D/gi, "=").replace(/%2B/gi, "+")
    }

    function ne(e, t) {
      return encodeURIComponent(e).replace(/%40/gi, "@").replace(/%3A/gi, ":").replace(/%24/g, "$").replace(/%2C/gi,
        ",").replace(/%3B/gi, ";").replace(/%20/g, t ? "%20" : "+")
    }

    function re(e, t) {
      var n, r, i = Zr.length;
      for (r = 0; r < i; ++r)
        if (n = Zr[r] + t, _(n = e.getAttribute(n))) return n;
      return null
    }

    function ie(e, t) {
      var n, i, o = {};
      r(Zr, function(t) {
        var r = t + "app";
        !n && e.hasAttribute && e.hasAttribute(r) && (n = e, i = e.getAttribute(r))
      }), r(Zr, function(t) {
        var r, o = t + "app";
        !n && (r = e.querySelector("[" + o.replace(":", "\\:") + "]")) && (n = r, i = r.getAttribute(o))
      }), n && (o.strictDi = null !== re(n, "strict-di"), t(n, i ? [i] : [], o))
    }

    function oe(t, n, i) {
      b(i) || (i = {});
      var o = {
        strictDi: !1
      };
      i = u(o, i);
      var a = function() {
          if (t = Rr(t), t.injector()) {
            var r = t[0] === e.document ? "document" : Q(t);
            throw zr("btstrpd", "App already bootstrapped with this element '{0}'", r.replace(/</, "&lt;").replace(/>/,
              "&gt;"))
          }
          n = n || [], n.unshift(["$provide", function(e) {
            e.value("$rootElement", t)
          }]), i.debugInfoEnabled && n.push(["$compileProvider", function(e) {
            e.debugInfoEnabled(!0)
          }]), n.unshift("ng");
          var o = nt(n, i.strictDi);
          return o.invoke(["$rootScope", "$rootElement", "$compile", "$injector", function(e, t, n, r) {
            e.$apply(function() {
              t.data("$injector", r), n(t)(e)
            })
          }]), o
        },
        s = /^NG_ENABLE_DEBUG_INFO!/,
        c = /^NG_DEFER_BOOTSTRAP!/;
      return e && s.test(e.name) && (i.debugInfoEnabled = !0, e.name = e.name.replace(s, "")), e && !c.test(e.name) ?
      a() : (e.name = e.name.replace(c, ""), qr.resumeBootstrap = function(e) {
        return r(e, function(e) {
          n.push(e)
        }), a()
      }, void(T(qr.resumeDeferredBootstrap) && qr.resumeDeferredBootstrap()))
    }

    function ae() {
      e.name = "NG_ENABLE_DEBUG_INFO!" + e.name, e.location.reload()
    }

    function se(e) {
      var t = qr.element(e).injector();
      if (!t) throw zr("test", "no injector found for element argument to getTestability");
      return t.get("$$testability")
    }

    function ce(e, t) {
      return t = t || "_", e.replace(ei, function(e, n) {
        return (n ? t : "") + e.toLowerCase()
      })
    }

    function ue() {
      var t;
      if (!ti) {
        var n = Qr();
        Pr = g(n) ? e.jQuery : n ? e[n] : void 0, Pr && Pr.fn.on ? (Rr = Pr, u(Pr.fn, {
          scope: wi.scope,
          isolateScope: wi.isolateScope,
          controller: wi.controller,
          injector: wi.injector,
          inheritedData: wi.inheritedData
        }), t = Pr.cleanData, Pr.cleanData = function(e) {
          for (var n, r, i = 0; null != (r = e[i]); i++) n = Pr._data(r, "events"), n && n.$destroy && Pr(r)
            .triggerHandler("$destroy");
          t(e)
        }) : Rr = Me, qr.element = Rr, ti = !0
      }
    }

    function le(e, t, n) {
      if (!e) throw zr("areq", "Argument '{0}' is {1}", t || "?", n || "required");
      return e
    }

    function de(e, t, n) {
      return n && Vr(e) && (e = e[e.length - 1]), le(T(e), t, "not a function, got " + (e && "object" == typeof e ? e
        .constructor.name || "Object" : typeof e)), e
    }

    function fe(e, t) {
      if ("hasOwnProperty" === e) throw zr("badname", "hasOwnProperty is not a valid {0} name", t)
    }

    function he(e, t, n) {
      if (!t) return e;
      for (var r, i = t.split("."), o = e, a = i.length, s = 0; s < a; s++) r = i[s], e && (e = (o = e)[r]);
      return !n && T(e) ? q(o, e) : e
    }

    function pe(e) {
      for (var t, n = e[0], r = e[e.length - 1], i = 1; n !== r && (n = n.nextSibling); i++)(t || e[i] !== n) && (t || (
        t = Rr(Ur.call(e, 0, i))), t.push(n));
      return t || e
    }

    function me() {
      return Object.create(null)
    }

    function ve(e) {
      function n(e, t, n) {
        return e[t] || (e[t] = n())
      }
      var r = t("$injector"),
        i = t("ng"),
        o = n(e, "angular", Object);
      return o.$$minErr = o.$$minErr || t, n(o, "module", function() {
        var e = {};
        return function(t, o, a) {
          var s = function(e, t) {
            if ("hasOwnProperty" === e) throw i("badname", "hasOwnProperty is not a valid {0} name", t)
          };
          return s(t, "module"), o && e.hasOwnProperty(t) && (e[t] = null), n(e, t, function() {
            function e(e, t, n, r) {
              return r || (r = i),
                function() {
                  return r[n || "push"]([e, t, arguments]), l
                }
            }

            function n(e, n) {
              return function(r, o) {
                return o && T(o) && (o.$$moduleName = t), i.push([e, n, arguments]), l
              }
            }
            if (!o) throw r("nomod",
              "Module '{0}' is not available! You either misspelled the module name or forgot to load it. If registering a module ensure that you specify the dependencies as the second argument.",
              t);
            var i = [],
              s = [],
              c = [],
              u = e("$injector", "invoke", "push", s),
              l = {
                _invokeQueue: i,
                _configBlocks: s,
                _runBlocks: c,
                requires: o,
                name: t,
                provider: n("$provide", "provider"),
                factory: n("$provide", "factory"),
                service: n("$provide", "service"),
                value: e("$provide", "value"),
                constant: e("$provide", "constant", "unshift"),
                decorator: n("$provide", "decorator"),
                animation: n("$animateProvider", "register"),
                filter: n("$filterProvider", "register"),
                controller: n("$controllerProvider", "register"),
                directive: n("$compileProvider", "directive"),
                component: n("$compileProvider", "component"),
                config: u,
                run: function(e) {
                  return c.push(e), this
                }
              };
            return a && u(a), l
          })
        }
      })
    }

    function ge(e) {
      var t = [];
      return JSON.stringify(e, function(e, n) {
        if (n = G(e, n), b(n)) {
          if (t.indexOf(n) >= 0) return "...";
          t.push(n)
        }
        return n
      })
    }

    function ye(e) {
      return "function" == typeof e ? e.toString().replace(/ \{[\s\S]*$/, "") : g(e) ? "undefined" : "string" !=
        typeof e ? ge(e) : e
    }

    function be(n) {
      u(n, {
        bootstrap: oe,
        copy: F,
        extend: u,
        merge: l,
        equals: H,
        element: Rr,
        forEach: r,
        injector: nt,
        noop: h,
        bind: q,
        toJson: V,
        fromJson: W,
        identity: p,
        isUndefined: g,
        isDefined: y,
        isString: _,
        isFunction: T,
        isObject: b,
        isNumber: $,
        isElement: R,
        isArray: Vr,
        version: ci,
        isDate: w,
        lowercase: kr,
        uppercase: Nr,
        callbacks: {
          counter: 0
        },
        getTestability: se,
        $$minErr: t,
        $$csp: Xr,
        reloadWithDebugInfo: ae
      }), (Lr = ve(e))("ng", ["ngLocale"], ["$provide", function(e) {
        e.provider({
          $$sanitizeUri: wn
        }), e.provider("$compile", ht).directive({
          a: No,
          input: Xo,
          textarea: Xo,
          form: Po,
          script: Ga,
          select: Ya,
          style: Xa,
          option: Ka,
          ngBind: Zo,
          ngBindHtml: ta,
          ngBindTemplate: ea,
          ngClass: ra,
          ngClassEven: oa,
          ngClassOdd: ia,
          ngCloak: aa,
          ngController: sa,
          ngForm: Lo,
          ngHide: Ua,
          ngIf: la,
          ngInclude: da,
          ngInit: ha,
          ngNonBindable: Ma,
          ngPluralize: Oa,
          ngRepeat: Da,
          ngShow: La,
          ngStyle: Fa,
          ngSwitch: ja,
          ngSwitchWhen: Ha,
          ngSwitchDefault: Ba,
          ngOptions: Ia,
          ngTransclude: qa,
          ngModel: xa,
          ngList: pa,
          ngChange: na,
          pattern: Ja,
          ngPattern: Ja,
          required: Qa,
          ngRequired: Qa,
          minlength: es,
          ngMinlength: es,
          maxlength: Za,
          ngMaxlength: Za,
          ngValue: Jo,
          ngModelOptions: Aa
        }).directive({
          ngInclude: fa
        }).directive(Io).directive(ca), e.provider({
          $anchorScroll: rt,
          $animate: Fi,
          $animateCss: Bi,
          $$animateJs: Li,
          $$animateQueue: Ui,
          $$AnimateRunner: Hi,
          $$animateAsyncRun: ji,
          $browser: ut,
          $cacheFactory: lt,
          $controller: bt,
          $document: Et,
          $exceptionHandler: _t,
          $filter: Ln,
          $$forceReflow: Yi,
          $interpolate: Rt,
          $interval: Pt,
          $http: Nt,
          $httpParamSerializer: wt,
          $httpParamSerializerJQLike: Tt,
          $httpBackend: Ot,
          $xhrFactory: It,
          $location: Xt,
          $log: Qt,
          $parse: gn,
          $rootScope: $n,
          $q: yn,
          $$q: bn,
          $sce: Sn,
          $sceDelegate: xn,
          $sniffer: An,
          $templateCache: dt,
          $templateRequest: Mn,
          $$testability: kn,
          $timeout: Nn,
          $window: Dn,
          $$rAF: _n,
          $$jqLite: Xe,
          $$HashMap: Si,
          $$cookieReader: Pn
        })
      }])
    }

    function Ee() {
      return ++li
    }

    function _e(e) {
      return e.replace(hi, function(e, t, n, r) {
        return r ? n.toUpperCase() : n
      }).replace(pi, "Moz$1")
    }

    function $e(e) {
      return !yi.test(e)
    }

    function we(e) {
      var t = e.nodeType;
      return t === ni || !t || t === ai
    }

    function Te(e) {
      for (var t in ui[e.ng339]) return !0;
      return !1
    }

    function Ce(e) {
      for (var t = 0, n = e.length; t < n; t++) Oe(e[t])
    }

    function xe(e, t) {
      var n, i, o, a, s = t.createDocumentFragment(),
        c = [];
      if ($e(e)) c.push(t.createTextNode(e));
      else {
        for (n = n || s.appendChild(t.createElement("div")), i = (bi.exec(e) || ["", ""])[1].toLowerCase(), o = _i[i] ||
          _i._default, n.innerHTML = o[1] + e.replace(Ei, "<$1></$2>") + o[2], a = o[0]; a--;) n = n.lastChild;
        c = B(c, n.childNodes), n = s.firstChild, n.textContent = ""
      }
      return s.textContent = "", s.innerHTML = "", r(c, function(e) {
        s.appendChild(e)
      }), s
    }

    function Se(t, n) {
      n = n || e.document;
      var r;
      return (r = gi.exec(t)) ? [n.createElement(r[1])] : (r = xe(t, n)) ? r.childNodes : []
    }

    function Ae(e, t) {
      var n = e.parentNode;
      n && n.replaceChild(t, e), t.appendChild(e)
    }

    function Me(e) {
      if (e instanceof Me) return e;
      var t;
      if (_(e) && (e = Yr(e), t = !0), !(this instanceof Me)) {
        if (t && "<" != e.charAt(0)) throw vi("nosel",
          "Looking up elements via selectors is not supported by jqLite! See: http://docs.angularjs.org/api/angular.element"
          );
        return new Me(e)
      }
      t ? Fe(this, Se(e)) : Fe(this, e)
    }

    function ke(e) {
      return e.cloneNode(!0)
    }

    function Ne(e, t) {
      if (t || Oe(e), e.querySelectorAll)
        for (var n = e.querySelectorAll("*"), r = 0, i = n.length; r < i; r++) Oe(n[r])
    }

    function Ie(e, t, n, i) {
      if (y(i)) throw vi("offargs", "jqLite#off() does not support the `selector` argument");
      var o = De(e),
        a = o && o.events,
        s = o && o.handle;
      if (s)
        if (t) {
          var c = function(t) {
            var r = a[t];
            y(n) && U(r || [], n), y(n) && r && r.length > 0 || (fi(e, t, s), delete a[t])
          };
          r(t.split(" "), function(e) {
            c(e), mi[e] && c(mi[e])
          })
        } else
          for (t in a) "$destroy" !== t && fi(e, t, s), delete a[t]
    }

    function Oe(e, t) {
      var n = e.ng339,
        r = n && ui[n];
      if (r) {
        if (t) return void delete r.data[t];
        r.handle && (r.events.$destroy && r.handle({}, "$destroy"), Ie(e)), delete ui[n], e.ng339 = void 0
      }
    }

    function De(e, t) {
      var n = e.ng339,
        r = n && ui[n];
      return t && !r && (e.ng339 = n = Ee(), r = ui[n] = {
        events: {},
        data: {},
        handle: void 0
      }), r
    }

    function Re(e, t, n) {
      if (we(e)) {
        var r = y(n),
          i = !r && t && !b(t),
          o = !t,
          a = De(e, !i),
          s = a && a.data;
        if (r) s[t] = n;
        else {
          if (o) return s;
          if (i) return s && s[t];
          u(s, t)
        }
      }
    }

    function Pe(e, t) {
      return !!e.getAttribute && (" " + (e.getAttribute("class") || "") + " ").replace(/[\n\t]/g, " ").indexOf(" " + t +
        " ") > -1
    }

    function Le(e, t) {
      t && e.setAttribute && r(t.split(" "), function(t) {
        e.setAttribute("class", Yr((" " + (e.getAttribute("class") || "") + " ").replace(/[\n\t]/g, " ").replace(
          " " + Yr(t) + " ", " ")))
      })
    }

    function Ue(e, t) {
      if (t && e.setAttribute) {
        var n = (" " + (e.getAttribute("class") || "") + " ").replace(/[\n\t]/g, " ");
        r(t.split(" "), function(e) {
          e = Yr(e), n.indexOf(" " + e + " ") === -1 && (n += e + " ")
        }), e.setAttribute("class", Yr(n))
      }
    }

    function Fe(e, t) {
      if (t)
        if (t.nodeType) e[e.length++] = t;
        else {
          var n = t.length;
          if ("number" == typeof n && t.window !== t) {
            if (n)
              for (var r = 0; r < n; r++) e[e.length++] = t[r]
          } else e[e.length++] = t
        }
    }

    function je(e, t) {
      return He(e, "$" + (t || "ngController") + "Controller")
    }

    function He(e, t, n) {
      e.nodeType == ai && (e = e.documentElement);
      for (var r = Vr(t) ? t : [t]; e;) {
        for (var i = 0, o = r.length; i < o; i++)
          if (y(n = Rr.data(e, r[i]))) return n;
        e = e.parentNode || e.nodeType === si && e.host
      }
    }

    function Be(e) {
      for (Ne(e, !0); e.firstChild;) e.removeChild(e.firstChild)
    }

    function ze(e, t) {
      t || Ne(e);
      var n = e.parentNode;
      n && n.removeChild(e)
    }

    function qe(t, n) {
      n = n || e, "complete" === n.document.readyState ? n.setTimeout(t) : Rr(n).on("load", t)
    }

    function Ge(e, t) {
      var n = Ti[t.toLowerCase()];
      return n && Ci[L(e)] && n
    }

    function Ve(e) {
      return xi[e]
    }

    function We(e, t) {
      var n = function(n, r) {
        n.isDefaultPrevented = function() {
          return n.defaultPrevented
        };
        var i = t[r || n.type],
          o = i ? i.length : 0;
        if (o) {
          if (g(n.immediatePropagationStopped)) {
            var a = n.stopImmediatePropagation;
            n.stopImmediatePropagation = function() {
              n.immediatePropagationStopped = !0, n.stopPropagation && n.stopPropagation(), a && a.call(n)
            }
          }
          n.isImmediatePropagationStopped = function() {
            return n.immediatePropagationStopped === !0
          };
          var s = i.specialHandlerWrapper || Ye;
          o > 1 && (i = j(i));
          for (var c = 0; c < o; c++) n.isImmediatePropagationStopped() || s(e, n, i[c])
        }
      };
      return n.elem = e, n
    }

    function Ye(e, t, n) {
      n.call(e, t)
    }

    function Ke(e, t, n) {
      var r = t.relatedTarget;
      r && (r === e || $i.call(e, r)) || n.call(e, t)
    }

    function Xe() {
      this.$get = function() {
        return u(Me, {
          hasClass: function(e, t) {
            return e.attr && (e = e[0]), Pe(e, t)
          },
          addClass: function(e, t) {
            return e.attr && (e = e[0]), Ue(e, t)
          },
          removeClass: function(e, t) {
            return e.attr && (e = e[0]), Le(e, t)
          }
        })
      }
    }

    function Qe(e, t) {
      var n = e && e.$$hashKey;
      if (n) return "function" == typeof n && (n = e.$$hashKey()), n;
      var r = typeof e;
      return n = "function" == r || "object" == r && null !== e ? e.$$hashKey = r + ":" + (t || a)() : r + ":" + e
    }

    function Je(e, t) {
      if (t) {
        var n = 0;
        this.nextUid = function() {
          return ++n
        }
      }
      r(e, this.put, this)
    }

    function Ze(e) {
      var t = Function.prototype.toString.call(e).replace(Ii, ""),
        n = t.match(Ai) || t.match(Mi);
      return n
    }

    function et(e) {
      var t = Ze(e);
      return t ? "function(" + (t[1] || "").replace(/[\s\r\n]+/, " ") + ")" : "fn"
    }

    function tt(e, t, n) {
      var i, o, a;
      if ("function" == typeof e) {
        if (!(i = e.$inject)) {
          if (i = [], e.length) {
            if (t) throw _(n) && n || (n = e.name || et(e)), Oi("strictdi",
              "{0} is not using explicit annotation and cannot be invoked in strict mode", n);
            o = Ze(e), r(o[1].split(ki), function(e) {
              e.replace(Ni, function(e, t, n) {
                i.push(n)
              })
            })
          }
          e.$inject = i
        }
      } else Vr(e) ? (a = e.length - 1, de(e[a], "fn"), i = e.slice(0, a)) : de(e, "fn", !0);
      return i
    }

    function nt(e, t) {
      function n(e) {
        return function(t, n) {
          return b(t) ? void r(t, o(e)) : e(t, n)
        }
      }

      function i(e, t) {
        if (fe(e, "service"), (T(t) || Vr(t)) && (t = w.instantiate(t)), !t.$get) throw Oi("pget",
          "Provider '{0}' must define $get factory method.", e);
        return $[e + v] = t
      }

      function a(e, t) {
        return function() {
          var n = S.invoke(t, this);
          if (g(n)) throw Oi("undef", "Provider '{0}' must return a value from $get factory method.", e);
          return n
        }
      }

      function s(e, t, n) {
        return i(e, {
          $get: n !== !1 ? a(e, t) : t
        })
      }

      function c(e, t) {
        return s(e, ["$injector", function(e) {
          return e.instantiate(t)
        }])
      }

      function u(e, t) {
        return s(e, m(t), !1)
      }

      function l(e, t) {
        fe(e, "constant"), $[e] = t, C[e] = t
      }

      function d(e, t) {
        var n = w.get(e + v),
          r = n.$get;
        n.$get = function() {
          var e = S.invoke(r, n);
          return S.invoke(t, null, {
            $delegate: e
          })
        }
      }

      function f(e) {
        le(g(e) || Vr(e), "modulesToLoad", "not an array");
        var t, n = [];
        return r(e, function(e) {
          function r(e) {
            var t, n;
            for (t = 0, n = e.length; t < n; t++) {
              var r = e[t],
                i = w.get(r[0]);
              i[r[1]].apply(i, r[2])
            }
          }
          if (!E.get(e)) {
            E.put(e, !0);
            try {
              _(e) ? (t = Lr(e), n = n.concat(f(t.requires)).concat(t._runBlocks), r(t._invokeQueue), r(t
                ._configBlocks)) : T(e) ? n.push(w.invoke(e)) : Vr(e) ? n.push(w.invoke(e)) : de(e, "module")
            } catch (t) {
              throw Vr(e) && (e = e[e.length - 1]), t.message && t.stack && t.stack.indexOf(t.message) == -1 && (t =
                t.message + "\n" + t.stack), Oi("modulerr", "Failed to instantiate module {0} due to:\n{1}", e, t
                .stack || t.message || t)
            }
          }
        }), n
      }

      function h(e, n) {
        function r(t, r) {
          if (e.hasOwnProperty(t)) {
            if (e[t] === p) throw Oi("cdep", "Circular dependency found: {0}", t + " <- " + y.join(" <- "));
            return e[t]
          }
          try {
            return y.unshift(t), e[t] = p, e[t] = n(t, r)
          } catch (n) {
            throw e[t] === p && delete e[t], n
          } finally {
            y.shift()
          }
        }

        function i(e, n, i) {
          for (var o = [], a = nt.$$annotate(e, t, i), s = 0, c = a.length; s < c; s++) {
            var u = a[s];
            if ("string" != typeof u) throw Oi("itkn",
              "Incorrect injection token! Expected service name as string, got {0}", u);
            o.push(n && n.hasOwnProperty(u) ? n[u] : r(u, i))
          }
          return o
        }

        function o(e) {
          return !(Dr <= 11) && ("function" == typeof e && /^(?:class\s|constructor\()/.test(Function.prototype.toString
            .call(e)))
        }

        function a(e, t, n, r) {
          "string" == typeof n && (r = n, n = null);
          var a = i(e, n, r);
          return Vr(e) && (e = e[e.length - 1]), o(e) ? (a.unshift(null), new(Function.prototype.bind.apply(e, a))) : e
            .apply(t, a)
        }

        function s(e, t, n) {
          var r = Vr(e) ? e[e.length - 1] : e,
            o = i(e, t, n);
          return o.unshift(null), new(Function.prototype.bind.apply(r, o))
        }
        return {
          invoke: a,
          instantiate: s,
          get: r,
          annotate: nt.$$annotate,
          has: function(t) {
            return $.hasOwnProperty(t + v) || e.hasOwnProperty(t)
          }
        }
      }
      t = t === !0;
      var p = {},
        v = "Provider",
        y = [],
        E = new Je([], !0),
        $ = {
          $provide: {
            provider: n(i),
            factory: n(s),
            service: n(c),
            value: n(u),
            constant: n(l),
            decorator: d
          }
        },
        w = $.$injector = h($, function(e, t) {
          throw qr.isString(t) && y.push(t), Oi("unpr", "Unknown provider: {0}", y.join(" <- "))
        }),
        C = {},
        x = h(C, function(e, t) {
          var n = w.get(e + v, t);
          return S.invoke(n.$get, n, void 0, e)
        }),
        S = x;
      $["$injector" + v] = {
        $get: m(x)
      };
      var A = f(e);
      return S = x.get("$injector"), S.strictDi = t, r(A, function(e) {
        e && S.invoke(e)
      }), S
    }

    function rt() {
      var e = !0;
      this.disableAutoScrolling = function() {
        e = !1
      }, this.$get = ["$window", "$location", "$rootScope", function(t, n, r) {
        function i(e) {
          var t = null;
          return Array.prototype.some.call(e, function(e) {
            if ("a" === L(e)) return t = e, !0
          }), t
        }

        function o() {
          var e = s.yOffset;
          if (T(e)) e = e();
          else if (R(e)) {
            var n = e[0],
              r = t.getComputedStyle(n);
            e = "fixed" !== r.position ? 0 : n.getBoundingClientRect().bottom
          } else $(e) || (e = 0);
          return e
        }

        function a(e) {
          if (e) {
            e.scrollIntoView();
            var n = o();
            if (n) {
              var r = e.getBoundingClientRect().top;
              t.scrollBy(0, r - n)
            }
          } else t.scrollTo(0, 0)
        }

        function s(e) {
          e = _(e) ? e : n.hash();
          var t;
          e ? (t = c.getElementById(e)) ? a(t) : (t = i(c.getElementsByName(e))) ? a(t) : "top" === e && a(null) :
            a(null)
        }
        var c = t.document;
        return e && r.$watch(function() {
          return n.hash()
        }, function(e, t) {
          e === t && "" === e || qe(function() {
            r.$evalAsync(s)
          })
        }), s
      }]
    }

    function it(e, t) {
      return e || t ? e ? t ? (Vr(e) && (e = e.join(" ")), Vr(t) && (t = t.join(" ")), e + " " + t) : e : t : ""
    }

    function ot(e) {
      for (var t = 0; t < e.length; t++) {
        var n = e[t];
        if (n.nodeType === Ri) return n
      }
    }

    function at(e) {
      _(e) && (e = e.split(" "));
      var t = me();
      return r(e, function(e) {
        e.length && (t[e] = !0)
      }), t
    }

    function st(e) {
      return b(e) ? e : {}
    }

    function ct(e, t, n, i) {
      function o(e) {
        try {
          e.apply(null, z(arguments, 1))
        } finally {
          if (y--, 0 === y)
            for (; b.length;) try {
              b.pop()()
            } catch (e) {
              n.error(e)
            }
        }
      }

      function a(e) {
        var t = e.indexOf("#");
        return t === -1 ? "" : e.substr(t)
      }

      function s() {
        T = null, c(), u()
      }

      function c() {
        E = C(), E = g(E) ? null : E, H(E, A) && (E = A), A = E
      }

      function u() {
        $ === l.url() && _ === E || ($ = l.url(), _ = E, r(x, function(e) {
          e(l.url(), E)
        }))
      }
      var l = this,
        d = e.location,
        f = e.history,
        p = e.setTimeout,
        m = e.clearTimeout,
        v = {};
      l.isMock = !1;
      var y = 0,
        b = [];
      l.$$completeOutstandingRequest = o, l.$$incOutstandingRequestCount = function() {
        y++
      }, l.notifyWhenNoOutstandingRequests = function(e) {
        0 === y ? e() : b.push(e)
      };
      var E, _, $ = d.href,
        w = t.find("base"),
        T = null,
        C = i.history ? function() {
          try {
            return f.state
          } catch (e) {}
        } : h;
      c(), _ = E, l.url = function(t, n, r) {
        if (g(r) && (r = null), d !== e.location && (d = e.location), f !== e.history && (f = e.history), t) {
          var o = _ === r;
          if ($ === t && (!i.history || o)) return l;
          var s = $ && Ht($) === Ht(t);
          return $ = t, _ = r, !i.history || s && o ? (s && !T || (T = t), n ? d.replace(t) : s ? d.hash = a(t) : d
            .href = t, d.href !== t && (T = t)) : (f[n ? "replaceState" : "pushState"](r, "", t), c(), _ = E), l
        }
        return T || d.href.replace(/%27/g, "'")
      }, l.state = function() {
        return E
      };
      var x = [],
        S = !1,
        A = null;
      l.onUrlChange = function(t) {
        return S || (i.history && Rr(e).on("popstate", s), Rr(e).on("hashchange", s), S = !0), x.push(t), t
      }, l.$$applicationDestroyed = function() {
        Rr(e).off("hashchange popstate", s)
      }, l.$$checkUrlChange = u, l.baseHref = function() {
        var e = w.attr("href");
        return e ? e.replace(/^(https?\:)?\/\/[^\/]*/, "") : ""
      }, l.defer = function(e, t) {
        var n;
        return y++, n = p(function() {
          delete v[n], o(e)
        }, t || 0), v[n] = !0, n
      }, l.defer.cancel = function(e) {
        return !!v[e] && (delete v[e], m(e), o(h), !0)
      }
    }

    function ut() {
      this.$get = ["$window", "$log", "$sniffer", "$document", function(e, t, n, r) {
        return new ct(e, r, t, n)
      }]
    }

    function lt() {
      this.$get = function() {
        function e(e, r) {
          function i(e) {
            e != f && (h ? h == e && (h = e.n) : h = e, o(e.n, e.p), o(e, f), f = e, f.n = null)
          }

          function o(e, t) {
            e != t && (e && (e.p = t), t && (t.n = e))
          }
          if (e in n) throw t("$cacheFactory")("iid", "CacheId '{0}' is already taken!", e);
          var a = 0,
            s = u({}, r, {
              id: e
            }),
            c = me(),
            l = r && r.capacity || Number.MAX_VALUE,
            d = me(),
            f = null,
            h = null;
          return n[e] = {
            put: function(e, t) {
              if (!g(t)) {
                if (l < Number.MAX_VALUE) {
                  var n = d[e] || (d[e] = {
                    key: e
                  });
                  i(n)
                }
                return e in c || a++, c[e] = t, a > l && this.remove(h.key), t
              }
            },
            get: function(e) {
              if (l < Number.MAX_VALUE) {
                var t = d[e];
                if (!t) return;
                i(t)
              }
              return c[e]
            },
            remove: function(e) {
              if (l < Number.MAX_VALUE) {
                var t = d[e];
                if (!t) return;
                t == f && (f = t.p), t == h && (h = t.n), o(t.n, t.p), delete d[e]
              }
              e in c && (delete c[e], a--)
            },
            removeAll: function() {
              c = me(), a = 0, d = me(), f = h = null
            },
            destroy: function() {
              c = null, s = null, d = null, delete n[e]
            },
            info: function() {
              return u({}, s, {
                size: a
              })
            }
          }
        }
        var n = {};
        return e.info = function() {
          var e = {};
          return r(n, function(t, n) {
            e[n] = t.info()
          }), e
        }, e.get = function(e) {
          return n[e]
        }, e
      }
    }

    function dt() {
      this.$get = ["$cacheFactory", function(e) {
        return e("templates")
      }]
    }

    function ft() {}

    function ht(t, n) {
      function i(e, t, n) {
        var i = /^\s*([@&<]|=(\*?))(\??)\s*(\w*)\s*$/,
          o = me();
        return r(e, function(e, r) {
          if (e in C) return void(o[r] = C[e]);
          var a = e.match(i);
          if (!a) throw zi("iscp", "Invalid {3} for directive '{0}'. Definition: {... {1}: '{2}' ...}", t, r, e, n ?
            "controller bindings definition" : "isolate scope definition");
          o[r] = {
            mode: a[1][0],
            collection: "*" === a[2],
            optional: "?" === a[3],
            attrName: a[4] || r
          }, a[4] && (C[e] = o[r])
        }), o
      }

      function a(e, t) {
        var n = {
          isolateScope: null,
          bindToController: null
        };
        if (b(e.scope) && (e.bindToController === !0 ? (n.bindToController = i(e.scope, t, !0), n.isolateScope = {}) : n
            .isolateScope = i(e.scope, t, !1)), b(e.bindToController) && (n.bindToController = i(e.bindToController, t,
            !0)), b(n.bindToController)) {
          var r = e.controller,
            o = e.controllerAs;
          if (!r) throw zi("noctrl", "Cannot bind to controller without directive '{0}'s controller.", t);
          if (!yt(r, o)) throw zi("noident", "Cannot bind to controller without identifier for directive '{0}'.", t)
        }
        return n
      }

      function s(e) {
        var t = e.charAt(0);
        if (!t || t !== kr(t)) throw zi("baddir",
          "Directive/Component name '{0}' is invalid. The first character must be a lowercase letter", e);
        if (e !== e.trim()) throw zi("baddir",
          "Directive/Component name '{0}' is invalid. The name should not contain leading or trailing whitespaces",
          e)
      }
      var c = {},
        l = "Directive",
        d = /^\s*directive\:\s*([\w\-]+)\s+(.*)$/,
        v = /(([\w\-]+)(?:\:([^;]+))?;?)/,
        E = P("ngSrc,ngSrcset,src,srcset"),
        $ = /^(?:(\^\^?)?(\?)?(\^\^?)?)?/,
        w = /^(on[a-z]+|formaction)$/,
        C = me();
      this.directive = function e(n, i) {
        return fe(n, "directive"), _(n) ? (s(n), le(i, "directiveFactory"), c.hasOwnProperty(n) || (c[n] = [], t
          .factory(n + l, ["$injector", "$exceptionHandler", function(e, t) {
            var i = [];
            return r(c[n], function(r, o) {
              try {
                var a = e.invoke(r);
                T(a) ? a = {
                    compile: m(a)
                  } : !a.compile && a.link && (a.compile = m(a.link)), a.priority = a.priority || 0, a
                  .index = o, a.name = a.name || n, a.require = a.require || a.controller && a.name, a
                  .restrict = a.restrict || "EA", a.$$moduleName = r.$$moduleName, i.push(a)
              } catch (e) {
                t(e)
              }
            }), i
          }])), c[n].push(i)) : r(n, o(e)), this
      }, this.component = function(e, t) {
        function n(e) {
          function n(t) {
            return T(t) || Vr(t) ? function(n, r) {
              return e.invoke(t, this, {
                $element: n,
                $attrs: r
              })
            } : t
          }
          var o = t.template || t.templateUrl ? t.template : "",
            a = {
              controller: i,
              controllerAs: yt(t.controller) || t.controllerAs || "$ctrl",
              template: n(o),
              templateUrl: n(t.templateUrl),
              transclude: t.transclude,
              scope: {},
              bindToController: t.bindings || {},
              restrict: "E",
              require: t.require
            };
          return r(t, function(e, t) {
            "$" === t.charAt(0) && (a[t] = e)
          }), a
        }
        var i = t.controller || function() {};
        return r(t, function(e, t) {
          "$" === t.charAt(0) && (n[t] = e, T(i) && (i[t] = e))
        }), n.$inject = ["$injector"], this.directive(e, n)
      }, this.aHrefSanitizationWhitelist = function(e) {
        return y(e) ? (n.aHrefSanitizationWhitelist(e), this) : n.aHrefSanitizationWhitelist()
      }, this.imgSrcSanitizationWhitelist = function(e) {
        return y(e) ? (n.imgSrcSanitizationWhitelist(e), this) : n.imgSrcSanitizationWhitelist()
      };
      var x = !0;
      this.debugInfoEnabled = function(e) {
        return y(e) ? (x = e, this) : x
      };
      var A = 10;
      this.onChangesTtl = function(e) {
        return arguments.length ? (A = e, this) : A
      }, this.$get = ["$injector", "$interpolate", "$exceptionHandler", "$templateRequest", "$parse", "$controller",
        "$rootScope", "$sce", "$animate", "$$sanitizeUri",
        function(t, n, i, o, s, m, y, C, M, k) {
          function I() {
            try {
              if (!--be) throw ve = void 0, zi("infchng", "{0} $onChanges() iterations reached. Aborting!\n", A);
              y.$apply(function() {
                for (var e = 0, t = ve.length; e < t; ++e) ve[e]();
                ve = void 0
              })
            } finally {
              be++
            }
          }

          function O(e, t) {
            if (t) {
              var n, r, i, o = Object.keys(t);
              for (n = 0, r = o.length; n < r; n++) i = o[n], this[i] = t[i]
            } else this.$attr = {};
            this.$$element = e
          }

          function D(e, t, n) {
            ye.innerHTML = "<span " + t + ">";
            var r = ye.firstChild.attributes,
              i = r[0];
            r.removeNamedItem(i.name), i.value = n, e.attributes.setNamedItem(i)
          }

          function R(e, t) {
            try {
              e.addClass(t)
            } catch (e) {}
          }

          function P(t, n, r, i, o) {
            t instanceof Rr || (t = Rr(t));
            for (var a = /\S+/, s = 0, c = t.length; s < c; s++) {
              var u = t[s];
              u.nodeType === ii && u.nodeValue.match(a) && Ae(u, t[s] = e.document.createElement("span"))
            }
            var l = j(t, n, t, r, i, o);
            P.$$addScopeClass(t);
            var d = null;
            return function(e, n, r) {
              le(e, "scope"), o && o.needsNewScope && (e = e.$parent.$new()), r = r || {};
              var i = r.parentBoundTranscludeFn,
                a = r.transcludeControllers,
                s = r.futureParentElement;
              i && i.$$boundTransclude && (i = i.$$boundTransclude), d || (d = F(s));
              var c;
              if (c = "html" !== d ? Rr(ae(d, Rr("<div>").append(t).html())) : n ? wi.clone.call(t) : t, a)
                for (var u in a) c.data("$" + u + "Controller", a[u].instance);
              return P.$$addScopeInfo(c, e), n && n(c, e), l && l(e, c, c, i), c
            }
          }

          function F(e) {
            var t = e && e[0];
            return t && "foreignobject" !== L(t) && Hr.call(t).match(/SVG/) ? "svg" : "html"
          }

          function j(e, t, n, r, i, o) {
            function a(e, n, r, i) {
              var o, a, s, c, u, l, d, f, m;
              if (h) {
                var v = n.length;
                for (m = new Array(v), u = 0; u < p.length; u += 3) d = p[u], m[d] = n[d]
              } else m = n;
              for (u = 0, l = p.length; u < l;) s = m[p[u++]], o = p[u++], a = p[u++], o ? (o.scope ? (c = e.$new(), P
                  .$$addScopeInfo(Rr(s), c)) : c = e, f = o.transcludeOnThisElement ? B(e, o.transclude, i) : !o
                .templateOnThisElement && i ? i : !i && t ? B(e, t) : null, o(a, c, s, r, f)) : a && a(e, s
                .childNodes, void 0, i)
            }
            for (var s, c, u, l, d, f, h, p = [], m = 0; m < e.length; m++) s = new O, c = q(e[m], [], s, 0 === m ?
                r : void 0, i), u = c.length ? Y(c, e[m], s, t, n, null, [], [], o) : null, u && u.scope && P
              .$$addScopeClass(s.$$element), d = u && u.terminal || !(l = e[m].childNodes) || !l.length ? null : j(l,
                u ? (u.transcludeOnThisElement || !u.templateOnThisElement) && u.transclude : t), (u || d) && (p.push(
                m, u, d), f = !0, h = h || u), o = null;
            return f ? a : null
          }

          function B(e, t, n) {
            function r(r, i, o, a, s) {
              return r || (r = e.$new(!1, s), r.$$transcluded = !0), t(r, i, {
                parentBoundTranscludeFn: n,
                transcludeControllers: o,
                futureParentElement: a
              })
            }
            var i = r.$$slots = me();
            for (var o in t.$$slots) t.$$slots[o] ? i[o] = B(e, t.$$slots[o], n) : i[o] = null;
            return r
          }

          function q(e, t, n, r, i) {
            var o, a, s = e.nodeType,
              c = n.$attr;
            switch (s) {
              case ni:
                Z(t, mt(L(e)), "E", r, i);
                for (var u, l, f, h, p, m, g = e.attributes, y = 0, E = g && g.length; y < E; y++) {
                  var $ = !1,
                    w = !1;
                  u = g[y], l = u.name, p = Yr(u.value), h = mt(l), (m = Te.test(h)) && (l = l.replace(Gi, "").substr(
                    8).replace(/_(.)/g, function(e, t) {
                    return t.toUpperCase()
                  }));
                  var T = h.match(Ce);
                  T && ee(T[1]) && ($ = l, w = l.substr(0, l.length - 5) + "end", l = l.substr(0, l.length - 6)), f =
                    mt(l.toLowerCase()), c[f] = l, !m && n.hasOwnProperty(f) || (n[f] = p, Ge(e, f) && (n[f] = !0)),
                    ue(e, t, p, f, m), Z(t, f, "A", r, i, $, w)
                }
                if (a = e.className, b(a) && (a = a.animVal), _(a) && "" !== a)
                  for (; o = v.exec(a);) f = mt(o[2]), Z(t, f, "C", r, i) && (n[f] = Yr(o[3])), a = a.substr(o.index +
                    o[0].length);
                break;
              case ii:
                if (11 === Dr)
                  for (; e.parentNode && e.nextSibling && e.nextSibling.nodeType === ii;) e.nodeValue = e.nodeValue +
                    e.nextSibling.nodeValue, e.parentNode.removeChild(e.nextSibling);
                oe(t, e.nodeValue);
                break;
              case oi:
                try {
                  o = d.exec(e.nodeValue), o && (f = mt(o[1]), Z(t, f, "M", r, i) && (n[f] = Yr(o[2])))
                } catch (e) {}
            }
            return t.sort(re), t
          }

          function G(e, t, n) {
            var r = [],
              i = 0;
            if (t && e.hasAttribute && e.hasAttribute(t)) {
              do {
                if (!e) throw zi("uterdir", "Unterminated attribute, found '{0}' but no matching '{1}' found.", t, n);
                e.nodeType == ni && (e.hasAttribute(t) && i++, e.hasAttribute(n) && i--),
                  r.push(e), e = e.nextSibling
              } while (i > 0)
            } else r.push(e);
            return Rr(r)
          }

          function V(e, t, n) {
            return function(r, i, o, a, s) {
              return i = G(i[0], t, n), e(r, i, o, a, s)
            }
          }

          function W(e, t, n, r, i, o) {
            var a;
            return e ? P(t, n, r, i, o) : function() {
              return a || (a = P(t, n, r, i, o), t = n = o = null), a.apply(this, arguments)
            }
          }

          function Y(e, t, n, o, a, s, c, l, d) {
            function f(e, t, n, r) {
              e && (n && (e = V(e, n, r)), e.require = p.require, e.directiveName = m, (C === p || p
                .$$isolateScope) && (e = fe(e, {
                  isolateScope: !0
                })), c.push(e)), t && (n && (t = V(t, n, r)), t.require = p.require, t.directiveName = m, (C ===
                p || p.$$isolateScope) && (t = fe(t, {
                isolateScope: !0
              })), l.push(t))
            }

            function h(e, i, o, a, s) {
              function d(e, t, n, r) {
                var i;
                if (S(e) || (r = n, n = t, t = e, e = void 0), N && (i = y), n || (n = N ? _.parent() : _), !r)
                return s(e, t, i, n, U);
                var o = s.$$slots[r];
                if (o) return o(e, t, i, n, U);
                if (g(o)) throw zi("noslot",
                  'No parent directive that requires a transclusion with slot name "{0}". Element: {1}', r, Q(_))
              }
              var f, h, p, m, v, y, E, _, A, M;
              t === o ? (A = n, _ = n.$$element) : (_ = Rr(o), A = new O(_, n)), v = i, C ? m = i.$new(!0) : $ && (v =
                i.$parent), s && (E = d, E.$$boundTransclude = s, E.isSlotFilled = function(e) {
                return !!s.$$slots[e]
              }), w && (y = X(_, A, E, w, m, i, C)), C && (P.$$addScopeInfo(_, m, !0, !(x && (x === C || x === C
                  .$$originalDirective))), P.$$addScopeClass(_, !0), m.$$isolateBindings = C.$$isolateBindings, M =
                pe(i, A, m, m.$$isolateBindings, C), M.removeWatches && m.$on("$destroy", M.removeWatches));
              for (var k in y) {
                var I = w[k],
                  D = y[k],
                  R = I.$$bindings.bindToController;
                D.identifier && R ? D.bindingInfo = pe(v, A, D.instance, R, I) : D.bindingInfo = {};
                var L = D();
                L !== D.instance && (D.instance = L, _.data("$" + I.name + "Controller", L), D.bindingInfo
                  .removeWatches && D.bindingInfo.removeWatches(), D.bindingInfo = pe(v, A, D.instance, R, I))
              }
              for (r(w, function(e, t) {
                  var n = e.require;
                  e.bindToController && !Vr(n) && b(n) && u(y[t].instance, K(t, n, _, y))
                }), r(y, function(e) {
                  var t = e.instance;
                  T(t.$onChanges) && t.$onChanges(e.bindingInfo.initialChanges), T(t.$onInit) && t.$onInit(), T(t
                    .$onDestroy) && v.$on("$destroy", function() {
                    t.$onDestroy()
                  })
                }), f = 0, h = c.length; f < h; f++) p = c[f], he(p, p.isolateScope ? m : i, _, A, p.require && K(p
                .directiveName, p.require, _, y), E);
              var U = i;
              for (C && (C.template || null === C.templateUrl) && (U = m), e && e(U, o.childNodes, void 0, s), f = l
                .length - 1; f >= 0; f--) p = l[f], he(p, p.isolateScope ? m : i, _, A, p.require && K(p
                .directiveName, p.require, _, y), E);
              r(y, function(e) {
                var t = e.instance;
                T(t.$postLink) && t.$postLink()
              })
            }
            d = d || {};
            for (var p, m, v, y, E, _ = -Number.MAX_VALUE, $ = d.newScopeDirective, w = d.controllerDirectives, C = d
                .newIsolateScopeDirective, x = d.templateDirective, A = d.nonTlbTranscludeDirective, M = !1, k = !1,
                N = d.hasElementTranscludeDirective, I = n.$$element = Rr(t), D = s, R = o, U = !1, F = !1, j = 0, H =
                e.length; j < H; j++) {
              p = e[j];
              var B = p.$$start,
                Y = p.$$end;
              if (B && (I = G(t, B, Y)), v = void 0, _ > p.priority) break;
              if ((E = p.scope) && (p.templateUrl || (b(E) ? (ie("new/isolated scope", C || $, p, I), C = p) : ie(
                  "new/isolated scope", C, p, I)), $ = $ || p), m = p.name, !U && (p.replace && (p.templateUrl || p
                  .template) || p.transclude && !p.$$tlb)) {
                for (var Z, ee = j + 1; Z = e[ee++];)
                  if (Z.transclude && !Z.$$tlb || Z.replace && (Z.templateUrl || Z.template)) {
                    F = !0;
                    break
                  } U = !0
              }
              if (!p.templateUrl && p.controller && (E = p.controller, w = w || me(), ie("'" + m + "' controller", w[
                  m], p, I), w[m] = p), E = p.transclude)
                if (M = !0, p.$$tlb || (ie("transclusion", A, p, I), A = p), "element" == E) N = !0, _ = p.priority,
                  v = I, I = n.$$element = Rr(P.$$createComment(m, n[m])), t = I[0], de(a, z(v), t), v[0]
                  .$$parentNode = v[0].parentNode, R = W(F, v, o, _, D && D.name, {
                    nonTlbTranscludeDirective: A
                  });
                else {
                  var re = me();
                  if (v = Rr(ke(t)).contents(), b(E)) {
                    v = [];
                    var oe = me(),
                      se = me();
                    r(E, function(e, t) {
                      var n = "?" === e.charAt(0);
                      e = n ? e.substring(1) : e, oe[e] = t, re[t] = null, se[t] = n
                    }), r(I.contents(), function(e) {
                      var t = oe[mt(L(e))];
                      t ? (se[t] = !0, re[t] = re[t] || [], re[t].push(e)) : v.push(e)
                    }), r(se, function(e, t) {
                      if (!e) throw zi("reqslot", "Required transclusion slot `{0}` was not filled.", t)
                    });
                    for (var ce in re) re[ce] && (re[ce] = W(F, re[ce], o))
                  }
                  I.empty(), R = W(F, v, o, void 0, void 0, {
                    needsNewScope: p.$$isolateScope || p.$$newScope
                  }), R.$$slots = re
                } if (p.template)
                if (k = !0, ie("template", x, p, I), x = p, E = T(p.template) ? p.template(I, n) : p.template, E = we(
                    E), p.replace) {
                  if (D = p, v = $e(E) ? [] : gt(ae(p.templateNamespace, Yr(E))), t = v[0], 1 != v.length || t
                    .nodeType !== ni) throw zi("tplrt",
                    "Template for directive '{0}' must have exactly one root element. {1}", m, "");
                  de(a, I, t);
                  var ue = {
                      $attr: {}
                    },
                    le = q(t, [], ue),
                    ve = e.splice(j + 1, e.length - (j + 1));
                  (C || $) && J(le, C, $), e = e.concat(le).concat(ve), te(n, ue), H = e.length
                } else I.html(E);
              if (p.templateUrl) k = !0, ie("template", x, p, I), x = p, p.replace && (D = p), h = ne(e.splice(j, e
                .length - j), I, n, a, M && R, c, l, {
                controllerDirectives: w,
                newScopeDirective: $ !== p && $,
                newIsolateScopeDirective: C,
                templateDirective: x,
                nonTlbTranscludeDirective: A
              }), H = e.length;
              else if (p.compile) try {
                y = p.compile(I, n, R), T(y) ? f(null, y, B, Y) : y && f(y.pre, y.post, B, Y)
              } catch (e) {
                i(e, Q(I))
              }
              p.terminal && (h.terminal = !0, _ = Math.max(_, p.priority))
            }
            return h.scope = $ && $.scope === !0, h.transcludeOnThisElement = M, h.templateOnThisElement = k, h
              .transclude = R, d.hasElementTranscludeDirective = N, h
          }

          function K(e, t, n, i) {
            var o;
            if (_(t)) {
              var a = t.match($),
                s = t.substring(a[0].length),
                c = a[1] || a[3],
                u = "?" === a[2];
              if ("^^" === c ? n = n.parent() : (o = i && i[s], o = o && o.instance), !o) {
                var l = "$" + s + "Controller";
                o = c ? n.inheritedData(l) : n.data(l)
              }
              if (!o && !u) throw zi("ctreq", "Controller '{0}', required by directive '{1}', can't be found!", s, e)
            } else if (Vr(t)) {
              o = [];
              for (var d = 0, f = t.length; d < f; d++) o[d] = K(e, t[d], n, i)
            } else b(t) && (o = {}, r(t, function(t, r) {
              o[r] = K(e, t, n, i)
            }));
            return o || null
          }

          function X(e, t, n, r, i, o, a) {
            var s = me();
            for (var c in r) {
              var u = r[c],
                l = {
                  $scope: u === a || u.$$isolateScope ? i : o,
                  $element: e,
                  $attrs: t,
                  $transclude: n
                },
                d = u.controller;
              "@" == d && (d = t[u.name]);
              var f = m(d, l, !0, u.controllerAs);
              s[u.name] = f, e.data("$" + u.name + "Controller", f.instance)
            }
            return s
          }

          function J(e, t, n) {
            for (var r = 0, i = e.length; r < i; r++) e[r] = f(e[r], {
              $$isolateScope: t,
              $$newScope: n
            })
          }

          function Z(e, n, r, o, s, u, d) {
            if (n === s) return null;
            var h = null;
            if (c.hasOwnProperty(n))
              for (var p, m = t.get(n + l), v = 0, y = m.length; v < y; v++) try {
                if (p = m[v], (g(o) || o > p.priority) && p.restrict.indexOf(r) != -1) {
                  if (u && (p = f(p, {
                      $$start: u,
                      $$end: d
                    })), !p.$$bindings) {
                    var E = p.$$bindings = a(p, p.name);
                    b(E.isolateScope) && (p.$$isolateBindings = E.isolateScope)
                  }
                  e.push(p), h = p
                }
              } catch (e) {
                i(e)
              }
            return h
          }

          function ee(e) {
            if (c.hasOwnProperty(e))
              for (var n, r = t.get(e + l), i = 0, o = r.length; i < o; i++)
                if (n = r[i], n.multiElement) return !0;
            return !1
          }

          function te(e, t) {
            var n = t.$attr,
              i = e.$attr,
              o = e.$$element;
            r(e, function(r, i) {
              "$" != i.charAt(0) && (t[i] && t[i] !== r && (r += ("style" === i ? ";" : " ") + t[i]), e.$set(i, r,
                !0, n[i]))
            }), r(t, function(t, r) {
              "class" == r ? (R(o, t), e.class = (e.class ? e.class + " " : "") + t) : "style" == r ? (o.attr(
                  "style", o.attr("style") + ";" + t), e.style = (e.style ? e.style + ";" : "") + t) : "$" == r
                .charAt(0) || e.hasOwnProperty(r) || (e[r] = t, i[r] = n[r])
            })
          }

          function ne(e, t, n, i, a, s, c, u) {
            var l, d, h = [],
              p = t[0],
              m = e.shift(),
              v = f(m, {
                templateUrl: null,
                transclude: null,
                replace: null,
                $$originalDirective: m
              }),
              g = T(m.templateUrl) ? m.templateUrl(t, n) : m.templateUrl,
              y = m.templateNamespace;
            return t.empty(), o(g).then(function(o) {
                var f, E, _, $;
                if (o = we(o), m.replace) {
                  if (_ = $e(o) ? [] : gt(ae(y, Yr(o))), f = _[0], 1 != _.length || f.nodeType !== ni) throw zi(
                    "tplrt", "Template for directive '{0}' must have exactly one root element. {1}", m.name, g);
                  E = {
                    $attr: {}
                  }, de(i, t, f);
                  var w = q(f, [], E);
                  b(m.scope) && J(w, !0), e = w.concat(e), te(n, E)
                } else f = p, t.html(o);
                for (e.unshift(v), l = Y(e, f, n, a, t, m, s, c, u), r(i, function(e, n) {
                    e == f && (i[n] = t[0])
                  }), d = j(t[0].childNodes, a); h.length;) {
                  var T = h.shift(),
                    C = h.shift(),
                    x = h.shift(),
                    S = h.shift(),
                    A = t[0];
                  if (!T.$$destroyed) {
                    if (C !== p) {
                      var M = C.className;
                      u.hasElementTranscludeDirective && m.replace || (A = ke(f)), de(x, Rr(C), A), R(Rr(A), M)
                    }
                    $ = l.transcludeOnThisElement ? B(T, l.transclude, S) : S, l(d, T, A, i, $)
                  }
                }
                h = null
              }),
              function(e, t, n, r, i) {
                var o = i;
                t.$$destroyed || (h ? h.push(t, n, r, o) : (l.transcludeOnThisElement && (o = B(t, l.transclude, i)),
                  l(d, t, n, r, o)))
              }
          }

          function re(e, t) {
            var n = t.priority - e.priority;
            return 0 !== n ? n : e.name !== t.name ? e.name < t.name ? -1 : 1 : e.index - t.index
          }

          function ie(e, t, n, r) {
            function i(e) {
              return e ? " (module: " + e + ")" : ""
            }
            if (t) throw zi("multidir", "Multiple directives [{0}{1}, {2}{3}] asking for {4} on: {5}", t.name, i(t
              .$$moduleName), n.name, i(n.$$moduleName), e, Q(r))
          }

          function oe(e, t) {
            var r = n(t, !0);
            r && e.push({
              priority: 0,
              compile: function(e) {
                var t = e.parent(),
                  n = !!t.length;
                return n && P.$$addBindingClass(t),
                  function(e, t) {
                    var i = t.parent();
                    n || P.$$addBindingClass(i), P.$$addBindingInfo(i, r.expressions), e.$watch(r, function(e) {
                      t[0].nodeValue = e
                    })
                  }
              }
            })
          }

          function ae(t, n) {
            switch (t = kr(t || "html")) {
              case "svg":
              case "math":
                var r = e.document.createElement("div");
                return r.innerHTML = "<" + t + ">" + n + "</" + t + ">", r.childNodes[0].childNodes;
              default:
                return n
            }
          }

          function se(e, t) {
            if ("srcdoc" == t) return C.HTML;
            var n = L(e);
            return "xlinkHref" == t || "form" == n && "action" == t || "img" != n && ("src" == t || "ngSrc" == t) ? C
              .RESOURCE_URL : void 0
          }

          function ue(e, t, r, i, o) {
            var a = se(e, i);
            o = E[i] || o;
            var s = n(r, !0, a, o);
            if (s) {
              if ("multiple" === i && "select" === L(e)) throw zi("selmulti",
                "Binding to the 'multiple' attribute is not supported. Element: {0}", Q(e));
              t.push({
                priority: 100,
                compile: function() {
                  return {
                    pre: function(e, t, c) {
                      var u = c.$$observers || (c.$$observers = me());
                      if (w.test(i)) throw zi("nodomevents",
                        "Interpolations for HTML DOM event attributes are disallowed.  Please use the ng- versions (such as ng-click instead of onclick) instead."
                        );
                      var l = c[i];
                      l !== r && (s = l && n(l, !0, a, o), r = l), s && (c[i] = s(e), (u[i] || (u[i] = []))
                        .$$inter = !0, (c.$$observers && c.$$observers[i].$$scope || e).$watch(s, function(
                          e, t) {
                          "class" === i && e != t ? c.$updateClass(e, t) : c.$set(i, e)
                        }))
                    }
                  }
                }
              })
            }
          }

          function de(t, n, r) {
            var i, o, a = n[0],
              s = n.length,
              c = a.parentNode;
            if (t)
              for (i = 0, o = t.length; i < o; i++)
                if (t[i] == a) {
                  t[i++] = r;
                  for (var u = i, l = u + s - 1, d = t.length; u < d; u++, l++) l < d ? t[u] = t[l] : delete t[u];
                  t.length -= s - 1, t.context === a && (t.context = r);
                  break
                } c && c.replaceChild(r, a);
            var f = e.document.createDocumentFragment();
            for (i = 0; i < s; i++) f.appendChild(n[i]);
            for (Rr.hasData(a) && (Rr.data(r, Rr.data(a)), Rr(a).off("$destroy")), Rr.cleanData(f.querySelectorAll(
                "*")), i = 1; i < s; i++) delete n[i];
            n[0] = r, n.length = 1
          }

          function fe(e, t) {
            return u(function() {
              return e.apply(null, arguments)
            }, e, t)
          }

          function he(e, t, n, r, o, a) {
            try {
              e(t, n, r, o, a)
            } catch (e) {
              i(e, Q(n))
            }
          }

          function pe(e, t, i, o, a) {
            function c(t, n, r) {
              T(i.$onChanges) && n !== r && (ve || (e.$$postDigest(I), ve = []), l || (l = {}, ve.push(u)), l[t] && (
                r = l[t].previousValue), l[t] = new pt(r, n))
            }

            function u() {
              i.$onChanges(l), l = void 0
            }
            var l, d = [],
              f = {};
            return r(o, function(r, o) {
              var u, l, p, m, v, g = r.attrName,
                y = r.optional,
                b = r.mode;
              switch (b) {
                case "@":
                  y || Mr.call(t, g) || (i[o] = t[g] = void 0), t.$observe(g, function(e) {
                      if (_(e) || N(e)) {
                        var t = i[o];
                        c(o, e, t), i[o] = e
                      }
                    }), t.$$observers[g].$$scope = e, u = t[g], _(u) ? i[o] = n(u)(e) : N(u) && (i[o] = u), f[o] =
                    new pt(qi, i[o]);
                  break;
                case "=":
                  if (!Mr.call(t, g)) {
                    if (y) break;
                    t[g] = void 0
                  }
                  if (y && !t[g]) break;
                  l = s(t[g]), m = l.literal ? H : function(e, t) {
                    return e === t || e !== e && t !== t
                  }, p = l.assign || function() {
                    throw u = i[o] = l(e), zi("nonassign",
                      "Expression '{0}' in attribute '{1}' used with directive '{2}' is non-assignable!", t[
                      g], g, a.name)
                  }, u = i[o] = l(e);
                  var E = function(t) {
                    return m(t, i[o]) || (m(t, u) ? p(e, t = i[o]) : i[o] = t), u = t
                  };
                  E.$stateful = !0, v = r.collection ? e.$watchCollection(t[g], E) : e.$watch(s(t[g], E), null, l
                    .literal), d.push(v);
                  break;
                case "<":
                  if (!Mr.call(t, g)) {
                    if (y) break;
                    t[g] = void 0
                  }
                  if (y && !t[g]) break;
                  l = s(t[g]), i[o] = l(e), f[o] = new pt(qi, i[o]), v = e.$watch(l, function(e, t) {
                    e === t && (t = i[o]), c(o, e, t), i[o] = e
                  }, l.literal), d.push(v);
                  break;
                case "&":
                  if (l = t.hasOwnProperty(g) ? s(t[g]) : h, l === h && y) break;
                  i[o] = function(t) {
                    return l(e, t)
                  }
              }
            }), {
              initialChanges: f,
              removeWatches: d.length && function() {
                for (var e = 0, t = d.length; e < t; ++e) d[e]()
              }
            }
          }
          var ve, ge = /^\w/,
            ye = e.document.createElement("div"),
            be = A;
          O.prototype = {
            $normalize: mt,
            $addClass: function(e) {
              e && e.length > 0 && M.addClass(this.$$element, e)
            },
            $removeClass: function(e) {
              e && e.length > 0 && M.removeClass(this.$$element, e)
            },
            $updateClass: function(e, t) {
              var n = vt(e, t);
              n && n.length && M.addClass(this.$$element, n);
              var r = vt(t, e);
              r && r.length && M.removeClass(this.$$element, r)
            },
            $set: function(e, t, n, o) {
              var a, s = this.$$element[0],
                c = Ge(s, e),
                u = Ve(e),
                l = e;
              if (c ? (this.$$element.prop(e, t), o = c) : u && (this[u] = t, l = u), this[e] = t, o ? this.$attr[
                  e] = o : (o = this.$attr[e], o || (this.$attr[e] = o = ce(e, "-"))), a = L(this.$$element),
                "a" === a && ("href" === e || "xlinkHref" === e) || "img" === a && "src" === e) this[e] = t = k(t,
                "src" === e);
              else if ("img" === a && "srcset" === e) {
                for (var d = "", f = Yr(t), h = /(\s+\d+x\s*,|\s+\d+w\s*,|\s+,|,\s+)/, p = /\s/.test(f) ? h :
                    /(,)/, m = f.split(p), v = Math.floor(m.length / 2), y = 0; y < v; y++) {
                  var b = 2 * y;
                  d += k(Yr(m[b]), !0), d += " " + Yr(m[b + 1])
                }
                var E = Yr(m[2 * y]).split(/\s/);
                d += k(Yr(E[0]), !0), 2 === E.length && (d += " " + Yr(E[1])), this[e] = t = d
              }
              n !== !1 && (null === t || g(t) ? this.$$element.removeAttr(o) : ge.test(o) ? this.$$element.attr(o,
                t) : D(this.$$element[0], o, t));
              var _ = this.$$observers;
              _ && r(_[l], function(e) {
                try {
                  e(t)
                } catch (e) {
                  i(e)
                }
              })
            },
            $observe: function(e, t) {
              var n = this,
                r = n.$$observers || (n.$$observers = me()),
                i = r[e] || (r[e] = []);
              return i.push(t), y.$evalAsync(function() {
                  i.$$inter || !n.hasOwnProperty(e) || g(n[e]) || t(n[e])
                }),
                function() {
                  U(i, t)
                }
            }
          };
          var Ee = n.startSymbol(),
            _e = n.endSymbol(),
            we = "{{" == Ee && "}}" == _e ? p : function(e) {
              return e.replace(/\{\{/g, Ee).replace(/}}/g, _e)
            },
            Te = /^ngAttr[A-Z]/,
            Ce = /^(.+)Start$/;
          return P.$$addBindingInfo = x ? function(e, t) {
            var n = e.data("$binding") || [];
            Vr(t) ? n = n.concat(t) : n.push(t), e.data("$binding", n)
          } : h, P.$$addBindingClass = x ? function(e) {
            R(e, "ng-binding")
          } : h, P.$$addScopeInfo = x ? function(e, t, n, r) {
            var i = n ? r ? "$isolateScopeNoTemplate" : "$isolateScope" : "$scope";
            e.data(i, t)
          } : h, P.$$addScopeClass = x ? function(e, t) {
            R(e, t ? "ng-isolate-scope" : "ng-scope")
          } : h, P.$$createComment = function(t, n) {
            var r = "";
            return x && (r = " " + (t || "") + ": " + (n || "") + " "), e.document.createComment(r)
          }, P
        }
      ]
    }

    function pt(e, t) {
      this.previousValue = e, this.currentValue = t
    }

    function mt(e) {
      return _e(e.replace(Gi, ""))
    }

    function vt(e, t) {
      var n = "",
        r = e.split(/\s+/),
        i = t.split(/\s+/);
      e: for (var o = 0; o < r.length; o++) {
        for (var a = r[o], s = 0; s < i.length; s++)
          if (a == i[s]) continue e;
        n += (n.length > 0 ? " " : "") + a
      }
      return n
    }

    function gt(e) {
      e = Rr(e);
      var t = e.length;
      if (t <= 1) return e;
      for (; t--;) {
        var n = e[t];
        n.nodeType === oi && Fr.call(e, t, 1)
      }
      return e
    }

    function yt(e, t) {
      if (t && _(t)) return t;
      if (_(e)) {
        var n = Wi.exec(e);
        if (n) return n[3]
      }
    }

    function bt() {
      var e = {},
        n = !1;
      this.has = function(t) {
        return e.hasOwnProperty(t)
      }, this.register = function(t, n) {
        fe(t, "controller"), b(t) ? u(e, t) : e[t] = n
      }, this.allowGlobals = function() {
        n = !0
      }, this.$get = ["$injector", "$window", function(r, i) {
        function o(e, n, r, i) {
          if (!e || !b(e.$scope)) throw t("$controller")("noscp",
            "Cannot export controller '{0}' as '{1}'! No $scope object provided via `locals`.", i, n);
          e.$scope[n] = r
        }
        return function(t, a, s, c) {
          var l, d, f, h;
          if (s = s === !0, c && _(c) && (h = c), _(t)) {
            if (d = t.match(Wi), !d) throw Vi("ctrlfmt",
              "Badly formed controller string '{0}'. Must match `__name__ as __id__` or `__name__`.", t);
            f = d[1], h = h || d[3], t = e.hasOwnProperty(f) ? e[f] : he(a.$scope, f, !0) || (n ? he(i, f, !0) :
              void 0), de(t, f, !0)
          }
          if (s) {
            var p = (Vr(t) ? t[t.length - 1] : t).prototype;
            l = Object.create(p || null), h && o(a, h, l, f || t.name);
            var m;
            return m = u(function() {
              var e = r.invoke(t, l, a, f);
              return e !== l && (b(e) || T(e)) && (l = e, h && o(a, h, l, f || t.name)), l
            }, {
              instance: l,
              identifier: h
            })
          }
          return l = r.instantiate(t, a, f), h && o(a, h, l, f || t.name), l
        }
      }]
    }

    function Et() {
      this.$get = ["$window", function(e) {
        return Rr(e.document)
      }]
    }

    function _t() {
      this.$get = ["$log", function(e) {
        return function(t, n) {
          e.error.apply(e, arguments)
        }
      }]
    }

    function $t(e) {
      return b(e) ? w(e) ? e.toISOString() : V(e) : e
    }

    function wt() {
      this.$get = function() {
        return function(e) {
          if (!e) return "";
          var t = [];
          return i(e, function(e, n) {
            null === e || g(e) || (Vr(e) ? r(e, function(e) {
              t.push(ne(n) + "=" + ne($t(e)))
            }) : t.push(ne(n) + "=" + ne($t(e))))
          }), t.join("&")
        }
      }
    }

    function Tt() {
      this.$get = function() {
        return function(e) {
          function t(e, o, a) {
            null === e || g(e) || (Vr(e) ? r(e, function(e, n) {
              t(e, o + "[" + (b(e) ? n : "") + "]")
            }) : b(e) && !w(e) ? i(e, function(e, n) {
              t(e, o + (a ? "" : "[") + n + (a ? "" : "]"))
            }) : n.push(ne(o) + "=" + ne($t(e))))
          }
          if (!e) return "";
          var n = [];
          return t(e, "", !0), n.join("&")
        }
      }
    }

    function Ct(e, t) {
      if (_(e)) {
        var n = e.replace(Zi, "").trim();
        if (n) {
          var r = t("Content-Type");
          (r && 0 === r.indexOf(Ki) || xt(n)) && (e = W(n))
        }
      }
      return e
    }

    function xt(e) {
      var t = e.match(Qi);
      return t && Ji[t[0]].test(e)
    }

    function St(e) {
      function t(e, t) {
        e && (i[e] = i[e] ? i[e] + ", " + t : t)
      }
      var n, i = me();
      return _(e) ? r(e.split("\n"), function(e) {
        n = e.indexOf(":"), t(kr(Yr(e.substr(0, n))), Yr(e.substr(n + 1)))
      }) : b(e) && r(e, function(e, n) {
        t(kr(n), Yr(e))
      }), i
    }

    function At(e) {
      var t;
      return function(n) {
        if (t || (t = St(e)), n) {
          var r = t[kr(n)];
          return void 0 === r && (r = null), r
        }
        return t
      }
    }

    function Mt(e, t, n, i) {
      return T(i) ? i(e, t, n) : (r(i, function(r) {
        e = r(e, t, n)
      }), e)
    }

    function kt(e) {
      return 200 <= e && e < 300
    }

    function Nt() {
      var e = this.defaults = {
          transformResponse: [Ct],
          transformRequest: [function(e) {
            return !b(e) || A(e) || k(e) || M(e) ? e : V(e)
          }],
          headers: {
            common: {
              Accept: "application/json, text/plain, */*"
            },
            post: j(Xi),
            put: j(Xi),
            patch: j(Xi)
          },
          xsrfCookieName: "XSRF-TOKEN",
          xsrfHeaderName: "X-XSRF-TOKEN",
          paramSerializer: "$httpParamSerializer"
        },
        n = !1;
      this.useApplyAsync = function(e) {
        return y(e) ? (n = !!e, this) : n
      };
      var i = !0;
      this.useLegacyPromiseExtensions = function(e) {
        return y(e) ? (i = !!e, this) : i
      };
      var o = this.interceptors = [];
      this.$get = ["$httpBackend", "$$cookieReader", "$cacheFactory", "$rootScope", "$q", "$injector", function(a, s, c,
        l, d, f) {
        function h(n) {
          function o(e) {
            var t = u({}, e);
            return t.data = Mt(e.data, e.headers, e.status, c.transformResponse), kt(e.status) ? t : d.reject(t)
          }

          function a(e, t) {
            var n, i = {};
            return r(e, function(e, r) {
              T(e) ? (n = e(t), null != n && (i[r] = n)) : i[r] = e
            }), i
          }

          function s(t) {
            var n, r, i, o = e.headers,
              s = u({}, t.headers);
            o = u({}, o.common, o[kr(t.method)]);
            e: for (n in o) {
              r = kr(n);
              for (i in s)
                if (kr(i) === r) continue e;
              s[n] = o[n]
            }
            return a(s, j(t))
          }
          if (!b(n)) throw t("$http")("badreq", "Http request configuration must be an object.  Received: {0}", n);
          if (!_(n.url)) throw t("$http")("badreq",
            "Http request configuration url must be a string.  Received: {0}", n.url);
          var c = u({
            method: "get",
            transformRequest: e.transformRequest,
            transformResponse: e.transformResponse,
            paramSerializer: e.paramSerializer
          }, n);
          c.headers = s(n), c.method = Nr(c.method), c.paramSerializer = _(c.paramSerializer) ? f.get(c
            .paramSerializer) : c.paramSerializer;
          var l = function(t) {
              var n = t.headers,
                i = Mt(t.data, At(n), void 0, t.transformRequest);
              return g(i) && r(n, function(e, t) {
                "content-type" === kr(t) && delete n[t]
              }), g(t.withCredentials) && !g(e.withCredentials) && (t.withCredentials = e.withCredentials), v(t,
                i).then(o, o)
            },
            h = [l, void 0],
            p = d.when(c);
          for (r(w, function(e) {
              (e.request || e.requestError) && h.unshift(e.request, e.requestError), (e.response || e
                .responseError) && h.push(e.response, e.responseError)
            }); h.length;) {
            var m = h.shift(),
              y = h.shift();
            p = p.then(m, y)
          }
          return i ? (p.success = function(e) {
            return de(e, "fn"), p.then(function(t) {
              e(t.data, t.status, t.headers, c)
            }), p
          }, p.error = function(e) {
            return de(e, "fn"), p.then(null, function(t) {
              e(t.data, t.status, t.headers, c)
            }), p
          }) : (p.success = to("success"), p.error = to("error")), p
        }

        function p(e) {
          r(arguments, function(e) {
            h[e] = function(t, n) {
              return h(u({}, n || {}, {
                method: e,
                url: t
              }))
            }
          })
        }

        function m(e) {
          r(arguments, function(e) {
            h[e] = function(t, n, r) {
              return h(u({}, r || {}, {
                method: e,
                url: t,
                data: n
              }))
            }
          })
        }

        function v(t, i) {
          function o(e) {
            if (e) {
              var t = {};
              return r(e, function(e, r) {
                t[r] = function(t) {
                  function r() {
                    e(t)
                  }
                  n ? l.$applyAsync(r) : l.$$phase ? r() : l.$apply(r)
                }
              }), t
            }
          }

          function c(e, t, r, i) {
            function o() {
              u(t, e, r, i)
            }
            m && (kt(e) ? m.put(C, [e, t, St(r), i]) : m.remove(C)), n ? l.$applyAsync(o) : (o(), l.$$phase || l
              .$apply())
          }

          function u(e, n, r, i) {
            n = n >= -1 ? n : 0, (kt(n) ? _.resolve : _.reject)({
              data: e,
              status: n,
              headers: At(r),
              config: t,
              statusText: i
            })
          }

          function f(e) {
            u(e.data, e.status, j(e.headers()), e.statusText)
          }

          function p() {
            var e = h.pendingRequests.indexOf(t);
            e !== -1 && h.pendingRequests.splice(e, 1)
          }
          var m, v, _ = d.defer(),
            w = _.promise,
            T = t.headers,
            C = E(t.url, t.paramSerializer(t.params));
          if (h.pendingRequests.push(t), w.then(p, p), !t.cache && !e.cache || t.cache === !1 || "GET" !== t
            .method && "JSONP" !== t.method || (m = b(t.cache) ? t.cache : b(e.cache) ? e.cache : $), m && (v = m
              .get(C), y(v) ? I(v) ? v.then(f, f) : Vr(v) ? u(v[1], v[0], j(v[2]), v[3]) : u(v, 200, {}, "OK") : m
              .put(C, w)), g(v)) {
            var x = On(t.url) ? s()[t.xsrfCookieName || e.xsrfCookieName] : void 0;
            x && (T[t.xsrfHeaderName || e.xsrfHeaderName] = x), a(t.method, C, i, c, T, t.timeout, t
              .withCredentials, t.responseType, o(t.eventHandlers), o(t.uploadEventHandlers))
          }
          return w
        }

        function E(e, t) {
          return t.length > 0 && (e += (e.indexOf("?") == -1 ? "?" : "&") + t), e
        }
        var $ = c("$http");
        e.paramSerializer = _(e.paramSerializer) ? f.get(e.paramSerializer) : e.paramSerializer;
        var w = [];
        return r(o, function(e) {
            w.unshift(_(e) ? f.get(e) : f.invoke(e))
          }), h.pendingRequests = [], p("get", "delete", "head", "jsonp"), m("post", "put", "patch"), h.defaults =
          e, h
      }]
    }

    function It() {
      this.$get = function() {
        return function() {
          return new e.XMLHttpRequest
        }
      }
    }

    function Ot() {
      this.$get = ["$browser", "$window", "$document", "$xhrFactory", function(e, t, n, r) {
        return Dt(e, r, e.defer, t.angular.callbacks, n[0])
      }]
    }

    function Dt(e, t, n, i, o) {
      function a(e, t, n) {
        var r = o.createElement("script"),
          a = null;
        return r.type = "text/javascript", r.src = e, r.async = !0, a = function(e) {
          fi(r, "load", a), fi(r, "error", a), o.body.removeChild(r), r = null;
          var s = -1,
            c = "unknown";
          e && ("load" !== e.type || i[t].called || (e = {
            type: "error"
          }), c = e.type, s = "error" === e.type ? 404 : 200), n && n(s, c)
        }, di(r, "load", a), di(r, "error", a), o.body.appendChild(r), a
      }
      return function(o, s, c, u, l, d, f, p, m, v) {
        function b() {
          $ && $(), w && w.abort()
        }

        function E(t, r, i, o, a) {
          y(C) && n.cancel(C), $ = w = null, t(r, i, o, a), e.$$completeOutstandingRequest(h)
        }
        if (e.$$incOutstandingRequestCount(), s = s || e.url(), "jsonp" == kr(o)) {
          var _ = "_" + (i.counter++).toString(36);
          i[_] = function(e) {
            i[_].data = e, i[_].called = !0
          };
          var $ = a(s.replace("JSON_CALLBACK", "angular.callbacks." + _), _, function(e, t) {
            E(u, e, i[_].data, "", t), i[_] = h
          })
        } else {
          var w = t(o, s);
          w.open(o, s, !0), r(l, function(e, t) {
            y(e) && w.setRequestHeader(t, e)
          }), w.onload = function() {
            var e = w.statusText || "",
              t = "response" in w ? w.response : w.responseText,
              n = 1223 === w.status ? 204 : w.status;
            0 === n && (n = t ? 200 : "file" == In(s).protocol ? 404 : 0), E(u, n, t, w.getAllResponseHeaders(), e)
          };
          var T = function() {
            E(u, -1, null, null, "")
          };
          if (w.onerror = T, w.onabort = T, r(m, function(e, t) {
              w.addEventListener(t, e)
            }), r(v, function(e, t) {
              w.upload.addEventListener(t, e)
            }), f && (w.withCredentials = !0), p) try {
            w.responseType = p
          } catch (e) {
            if ("json" !== p) throw e
          }
          w.send(g(c) ? null : c)
        }
        if (d > 0) var C = n(b, d);
        else I(d) && d.then(b)
      }
    }

    function Rt() {
      var e = "{{",
        t = "}}";
      this.startSymbol = function(t) {
        return t ? (e = t, this) : e
      }, this.endSymbol = function(e) {
        return e ? (t = e, this) : t
      }, this.$get = ["$parse", "$exceptionHandler", "$sce", function(n, r, i) {
        function o(e) {
          return "\\\\\\" + e
        }

        function a(n) {
          return n.replace(h, e).replace(p, t)
        }

        function s(e) {
          if (null == e) return "";
          switch (typeof e) {
            case "string":
              break;
            case "number":
              e = "" + e;
              break;
            default:
              e = V(e)
          }
          return e
        }

        function c(e, t, n, r) {
          var i;
          return i = e.$watch(function(e) {
            return i(), r(e)
          }, t, n)
        }

        function l(o, l, h, p) {
          function v(e) {
            try {
              return e = I(e), p && !y(e) ? e : s(e)
            } catch (e) {
              r(no.interr(o, e))
            }
          }
          if (!o.length || o.indexOf(e) === -1) {
            var b;
            if (!l) {
              var E = a(o);
              b = m(E), b.exp = o, b.expressions = [], b.$$watchDelegate = c
            }
            return b
          }
          p = !!p;
          for (var _, $, w, C = 0, x = [], S = [], A = o.length, M = [], k = []; C < A;) {
            if ((_ = o.indexOf(e, C)) == -1 || ($ = o.indexOf(t, _ + d)) == -1) {
              C !== A && M.push(a(o.substring(C)));
              break
            }
            C !== _ && M.push(a(o.substring(C, _))), w = o.substring(_ + d, $), x.push(w), S.push(n(w, v)), C = $ +
              f, k.push(M.length), M.push("")
          }
          if (h && M.length > 1 && no.throwNoconcat(o), !l || x.length) {
            var N = function(e) {
                for (var t = 0, n = x.length; t < n; t++) {
                  if (p && g(e[t])) return;
                  M[k[t]] = e[t]
                }
                return M.join("")
              },
              I = function(e) {
                return h ? i.getTrusted(h, e) : i.valueOf(e)
              };
            return u(function(e) {
              var t = 0,
                n = x.length,
                i = new Array(n);
              try {
                for (; t < n; t++) i[t] = S[t](e);
                return N(i)
              } catch (e) {
                r(no.interr(o, e))
              }
            }, {
              exp: o,
              expressions: x,
              $$watchDelegate: function(e, t) {
                var n;
                return e.$watchGroup(S, function(r, i) {
                  var o = N(r);
                  T(t) && t.call(this, o, r !== i ? n : o, e), n = o
                })
              }
            })
          }
        }
        var d = e.length,
          f = t.length,
          h = new RegExp(e.replace(/./g, o), "g"),
          p = new RegExp(t.replace(/./g, o), "g");
        return l.startSymbol = function() {
          return e
        }, l.endSymbol = function() {
          return t
        }, l
      }]
    }

    function Pt() {
      this.$get = ["$rootScope", "$window", "$q", "$$q", "$browser", function(e, t, n, r, i) {
        function o(o, s, c, u) {
          function l() {
            d ? o.apply(null, f) : o(m)
          }
          var d = arguments.length > 4,
            f = d ? z(arguments, 4) : [],
            h = t.setInterval,
            p = t.clearInterval,
            m = 0,
            v = y(u) && !u,
            g = (v ? r : n).defer(),
            b = g.promise;
          return c = y(c) ? c : 0, b.$$intervalId = h(function() {
            v ? i.defer(l) : e.$evalAsync(l), g.notify(m++), c > 0 && m >= c && (g.resolve(m), p(b
              .$$intervalId), delete a[b.$$intervalId]), v || e.$apply()
          }, s), a[b.$$intervalId] = g, b
        }
        var a = {};
        return o.cancel = function(e) {
          return !!(e && e.$$intervalId in a) && (a[e.$$intervalId].reject("canceled"), t.clearInterval(e
            .$$intervalId), delete a[e.$$intervalId], !0)
        }, o
      }]
    }

    function Lt(e) {
      for (var t = e.split("/"), n = t.length; n--;) t[n] = te(t[n]);
      return t.join("/")
    }

    function Ut(e, t) {
      var n = In(e);
      t.$$protocol = n.protocol, t.$$host = n.hostname, t.$$port = d(n.port) || io[n.protocol] || null
    }

    function Ft(e, t) {
      var n = "/" !== e.charAt(0);
      n && (e = "/" + e);
      var r = In(e);
      t.$$path = decodeURIComponent(n && "/" === r.pathname.charAt(0) ? r.pathname.substring(1) : r.pathname), t
        .$$search = Z(r.search), t.$$hash = decodeURIComponent(r.hash), t.$$path && "/" != t.$$path.charAt(0) && (t
          .$$path = "/" + t.$$path)
    }

    function jt(e, t) {
      if (0 === t.indexOf(e)) return t.substr(e.length)
    }

    function Ht(e) {
      var t = e.indexOf("#");
      return t == -1 ? e : e.substr(0, t)
    }

    function Bt(e) {
      return e.replace(/(#.+)|#$/, "$1")
    }

    function zt(e) {
      return e.substr(0, Ht(e).lastIndexOf("/") + 1)
    }

    function qt(e) {
      return e.substring(0, e.indexOf("/", e.indexOf("//") + 2))
    }

    function Gt(e, t, n) {
      this.$$html5 = !0, n = n || "", Ut(e, this), this.$$parse = function(e) {
        var n = jt(t, e);
        if (!_(n)) throw oo("ipthprfx", 'Invalid url "{0}", missing path prefix "{1}".', e, t);
        Ft(n, this), this.$$path || (this.$$path = "/"), this.$$compose()
      }, this.$$compose = function() {
        var e = ee(this.$$search),
          n = this.$$hash ? "#" + te(this.$$hash) : "";
        this.$$url = Lt(this.$$path) + (e ? "?" + e : "") + n, this.$$absUrl = t + this.$$url.substr(1)
      }, this.$$parseLinkUrl = function(r, i) {
        if (i && "#" === i[0]) return this.hash(i.slice(1)), !0;
        var o, a, s;
        return y(o = jt(e, r)) ? (a = o, s = y(o = jt(n, o)) ? t + (jt("/", o) || o) : e + a) : y(o = jt(t, r)) ? s =
          t + o : t == r + "/" && (s = t), s && this.$$parse(s), !!s
      }
    }

    function Vt(e, t, n) {
      Ut(e, this), this.$$parse = function(r) {
        function i(e, t, n) {
          var r, i = /^\/[A-Z]:(\/.*)/;
          return 0 === t.indexOf(n) && (t = t.replace(n, "")), i.exec(t) ? e : (r = i.exec(e), r ? r[1] : e)
        }
        var o, a = jt(e, r) || jt(t, r);
        g(a) || "#" !== a.charAt(0) ? this.$$html5 ? o = a : (o = "", g(a) && (e = r, this.replace())) : (o = jt(n,
          a), g(o) && (o = a)), Ft(o, this), this.$$path = i(this.$$path, o, e), this.$$compose()
      }, this.$$compose = function() {
        var t = ee(this.$$search),
          r = this.$$hash ? "#" + te(this.$$hash) : "";
        this.$$url = Lt(this.$$path) + (t ? "?" + t : "") + r, this.$$absUrl = e + (this.$$url ? n + this.$$url : "")
      }, this.$$parseLinkUrl = function(t, n) {
        return Ht(e) == Ht(t) && (this.$$parse(t), !0)
      }
    }

    function Wt(e, t, n) {
      this.$$html5 = !0, Vt.apply(this, arguments), this.$$parseLinkUrl = function(r, i) {
        if (i && "#" === i[0]) return this.hash(i.slice(1)), !0;
        var o, a;
        return e == Ht(r) ? o = r : (a = jt(t, r)) ? o = e + n + a : t === r + "/" && (o = t), o && this.$$parse(o), !
          !o
      }, this.$$compose = function() {
        var t = ee(this.$$search),
          r = this.$$hash ? "#" + te(this.$$hash) : "";
        this.$$url = Lt(this.$$path) + (t ? "?" + t : "") + r, this.$$absUrl = e + n + this.$$url
      }
    }

    function Yt(e) {
      return function() {
        return this[e]
      }
    }

    function Kt(e, t) {
      return function(n) {
        return g(n) ? this[e] : (this[e] = t(n), this.$$compose(), this)
      }
    }

    function Xt() {
      var e = "",
        t = {
          enabled: !1,
          requireBase: !0,
          rewriteLinks: !0
        };
      this.hashPrefix = function(t) {
        return y(t) ? (e = t, this) : e
      }, this.html5Mode = function(e) {
        return N(e) ? (t.enabled = e, this) : b(e) ? (N(e.enabled) && (t.enabled = e.enabled), N(e.requireBase) && (t
          .requireBase = e.requireBase), N(e.rewriteLinks) && (t.rewriteLinks = e.rewriteLinks), this) : t
      }, this.$get = ["$rootScope", "$browser", "$sniffer", "$rootElement", "$window", function(n, r, i, o, a) {
        function s(e, t, n) {
          var i = u.url(),
            o = u.$$state;
          try {
            r.url(e, t, n), u.$$state = r.state()
          } catch (e) {
            throw u.url(i), u.$$state = o, e
          }
        }

        function c(e, t) {
          n.$broadcast("$locationChangeSuccess", u.absUrl(), e, u.$$state, t)
        }
        var u, l, d, f = r.baseHref(),
          h = r.url();
        if (t.enabled) {
          if (!f && t.requireBase) throw oo("nobase",
            "$location in HTML5 mode requires a <base> tag to be present!");
          d = qt(h) + (f || "/"), l = i.history ? Gt : Wt
        } else d = Ht(h), l = Vt;
        var p = zt(d);
        u = new l(d, p, "#" + e), u.$$parseLinkUrl(h, h), u.$$state = r.state();
        var m = /^\s*(javascript|mailto):/i;
        o.on("click", function(e) {
          if (t.rewriteLinks && !e.ctrlKey && !e.metaKey && !e.shiftKey && 2 != e.which && 2 != e.button) {
            for (var i = Rr(e.target);
              "a" !== L(i[0]);)
              if (i[0] === o[0] || !(i = i.parent())[0]) return;
            var s = i.prop("href"),
              c = i.attr("href") || i.attr("xlink:href");
            b(s) && "[object SVGAnimatedString]" === s.toString() && (s = In(s.animVal).href), m.test(s) || !
              s || i.attr("target") || e.isDefaultPrevented() || u.$$parseLinkUrl(s, c) && (e.preventDefault(),
                u.absUrl() != r.url() && (n.$apply(), a.angular["ff-684208-preventDefault"] = !0))
          }
        }), Bt(u.absUrl()) != Bt(h) && r.url(u.absUrl(), !0);
        var v = !0;
        return r.onUrlChange(function(e, t) {
          return g(jt(p, e)) ? void(a.location.href = e) : (n.$evalAsync(function() {
            var r, i = u.absUrl(),
              o = u.$$state;
            e = Bt(e), u.$$parse(e), u.$$state = t, r = n.$broadcast("$locationChangeStart", e, i, t, o)
              .defaultPrevented, u.absUrl() === e && (r ? (u.$$parse(i), u.$$state = o, s(i, !1, o)) : (
                v = !1, c(i, o)))
          }), void(n.$$phase || n.$digest()))
        }), n.$watch(function() {
          var e = Bt(r.url()),
            t = Bt(u.absUrl()),
            o = r.state(),
            a = u.$$replace,
            l = e !== t || u.$$html5 && i.history && o !== u.$$state;
          (v || l) && (v = !1, n.$evalAsync(function() {
            var t = u.absUrl(),
              r = n.$broadcast("$locationChangeStart", t, e, u.$$state, o).defaultPrevented;
            u.absUrl() === t && (r ? (u.$$parse(e), u.$$state = o) : (l && s(t, a, o === u.$$state ? null :
              u.$$state), c(e, o)))
          })), u.$$replace = !1
        }), u
      }]
    }

    function Qt() {
      var e = !0,
        t = this;
      this.debugEnabled = function(t) {
        return y(t) ? (e = t, this) : e
      }, this.$get = ["$window", function(n) {
        function i(e) {
          return e instanceof Error && (e.stack ? e = e.message && e.stack.indexOf(e.message) === -1 ? "Error: " + e
            .message + "\n" + e.stack : e.stack : e.sourceURL && (e = e.message + "\n" + e.sourceURL + ":" + e
              .line)), e
        }

        function o(e) {
          var t = n.console || {},
            o = t[e] || t.log || h,
            a = !1;
          try {
            a = !!o.apply
          } catch (e) {}
          return a ? function() {
            var e = [];
            return r(arguments, function(t) {
              e.push(i(t))
            }), o.apply(t, e)
          } : function(e, t) {
            o(e, null == t ? "" : t)
          }
        }
        return {
          log: o("log"),
          info: o("info"),
          warn: o("warn"),
          error: o("error"),
          debug: function() {
            var n = o("debug");
            return function() {
              e && n.apply(t, arguments)
            }
          }()
        }
      }]
    }

    function Jt(e, t) {
      if ("__defineGetter__" === e || "__defineSetter__" === e || "__lookupGetter__" === e || "__lookupSetter__" ===
        e || "__proto__" === e) throw so("isecfld",
        "Attempting to access a disallowed field in Angular expressions! Expression: {0}", t);
      return e
    }

    function Zt(e) {
      return e + ""
    }

    function en(e, t) {
      if (e) {
        if (e.constructor === e) throw so("isecfn",
          "Referencing Function in Angular expressions is disallowed! Expression: {0}", t);
        if (e.window === e) throw so("isecwindow",
          "Referencing the Window in Angular expressions is disallowed! Expression: {0}", t);
        if (e.children && (e.nodeName || e.prop && e.attr && e.find)) throw so("isecdom",
          "Referencing DOM nodes in Angular expressions is disallowed! Expression: {0}", t);
        if (e === Object) throw so("isecobj",
          "Referencing Object in Angular expressions is disallowed! Expression: {0}", t)
      }
      return e
    }

    function tn(e, t) {
      if (e) {
        if (e.constructor === e) throw so("isecfn",
          "Referencing Function in Angular expressions is disallowed! Expression: {0}", t);
        if (e === co || e === uo || e === lo) throw so("isecff",
          "Referencing call, apply or bind in Angular expressions is disallowed! Expression: {0}", t)
      }
    }

    function nn(e, t) {
      if (e && (e === (0).constructor || e === (!1).constructor || e === "".constructor || e === {}.constructor ||
          e === [].constructor || e === Function.constructor)) throw so("isecaf",
        "Assigning to a constructor is disallowed! Expression: {0}", t)
    }

    function rn(e, t) {
      return "undefined" != typeof e ? e : t
    }

    function on(e, t) {
      return "undefined" == typeof e ? t : "undefined" == typeof t ? e : e + t
    }

    function an(e, t) {
      var n = e(t);
      return !n.$stateful
    }

    function sn(e, t) {
      var n, i;
      switch (e.type) {
        case mo.Program:
          n = !0, r(e.body, function(e) {
            sn(e.expression, t), n = n && e.expression.constant
          }), e.constant = n;
          break;
        case mo.Literal:
          e.constant = !0, e.toWatch = [];
          break;
        case mo.UnaryExpression:
          sn(e.argument, t), e.constant = e.argument.constant, e.toWatch = e.argument.toWatch;
          break;
        case mo.BinaryExpression:
          sn(e.left, t), sn(e.right, t), e.constant = e.left.constant && e.right.constant, e.toWatch = e.left.toWatch
            .concat(e.right.toWatch);
          break;
        case mo.LogicalExpression:
          sn(e.left, t), sn(e.right, t), e.constant = e.left.constant && e.right.constant, e.toWatch = e
          .constant ? [] : [e];
          break;
        case mo.ConditionalExpression:
          sn(e.test, t), sn(e.alternate, t), sn(e.consequent, t), e.constant = e.test.constant && e.alternate
            .constant && e.consequent.constant, e.toWatch = e.constant ? [] : [e];
          break;
        case mo.Identifier:
          e.constant = !1, e.toWatch = [e];
          break;
        case mo.MemberExpression:
          sn(e.object, t), e.computed && sn(e.property, t), e.constant = e.object.constant && (!e.computed || e.property
            .constant), e.toWatch = [e];
          break;
        case mo.CallExpression:
          n = !!e.filter && an(t, e.callee.name), i = [], r(e.arguments, function(e) {
            sn(e, t), n = n && e.constant, e.constant || i.push.apply(i, e.toWatch)
          }), e.constant = n, e.toWatch = e.filter && an(t, e.callee.name) ? i : [e];
          break;
        case mo.AssignmentExpression:
          sn(e.left, t), sn(e.right, t), e.constant = e.left.constant && e.right.constant, e.toWatch = [e];
          break;
        case mo.ArrayExpression:
          n = !0, i = [], r(e.elements, function(e) {
            sn(e, t), n = n && e.constant,
              e.constant || i.push.apply(i, e.toWatch)
          }), e.constant = n, e.toWatch = i;
          break;
        case mo.ObjectExpression:
          n = !0, i = [], r(e.properties, function(e) {
            sn(e.value, t), n = n && e.value.constant, e.value.constant || i.push.apply(i, e.value.toWatch)
          }), e.constant = n, e.toWatch = i;
          break;
        case mo.ThisExpression:
          e.constant = !1, e.toWatch = [];
          break;
        case mo.LocalsExpression:
          e.constant = !1, e.toWatch = []
      }
    }

    function cn(e) {
      if (1 == e.length) {
        var t = e[0].expression,
          n = t.toWatch;
        return 1 !== n.length ? n : n[0] !== t ? n : void 0
      }
    }

    function un(e) {
      return e.type === mo.Identifier || e.type === mo.MemberExpression
    }

    function ln(e) {
      if (1 === e.body.length && un(e.body[0].expression)) return {
        type: mo.AssignmentExpression,
        left: e.body[0].expression,
        right: {
          type: mo.NGValueParameter
        },
        operator: "="
      }
    }

    function dn(e) {
      return 0 === e.body.length || 1 === e.body.length && (e.body[0].expression.type === mo.Literal || e.body[0]
        .expression.type === mo.ArrayExpression || e.body[0].expression.type === mo.ObjectExpression)
    }

    function fn(e) {
      return e.constant
    }

    function hn(e, t) {
      this.astBuilder = e, this.$filter = t
    }

    function pn(e, t) {
      this.astBuilder = e, this.$filter = t
    }

    function mn(e) {
      return "constructor" == e
    }

    function vn(e) {
      return T(e.valueOf) ? e.valueOf() : go.call(e)
    }

    function gn() {
      var e, t, n = me(),
        i = me(),
        o = {
          true: !0,
          false: !1,
          null: null,
          undefined: void 0
        };
      this.addLiteral = function(e, t) {
        o[e] = t
      }, this.setIdentifierFns = function(n, r) {
        return e = n, t = r, this
      }, this.$get = ["$filter", function(a) {
        function s(e, t, r) {
          var o, s, u;
          switch (r = r || E, typeof e) {
            case "string":
              e = e.trim(), u = e;
              var v = r ? i : n;
              if (o = v[u], !o) {
                ":" === e.charAt(0) && ":" === e.charAt(1) && (s = !0, e = e.substring(2));
                var y = r ? b : g,
                  _ = new po(y),
                  $ = new vo(_, a, y);
                o = $.parse(e), o.constant ? o.$$watchDelegate = p : s ? o.$$watchDelegate = o.literal ? f : d : o
                  .inputs && (o.$$watchDelegate = l), r && (o = c(o)), v[u] = o
              }
              return m(o, t);
            case "function":
              return m(e, t);
            default:
              return m(h, t)
          }
        }

        function c(e) {
          function t(t, n, r, i) {
            var o = E;
            E = !0;
            try {
              return e(t, n, r, i)
            } finally {
              E = o
            }
          }
          if (!e) return e;
          t.$$watchDelegate = e.$$watchDelegate, t.assign = c(e.assign), t.constant = e.constant, t.literal = e
            .literal;
          for (var n = 0; e.inputs && n < e.inputs.length; ++n) e.inputs[n] = c(e.inputs[n]);
          return t.inputs = e.inputs, t
        }

        function u(e, t) {
          return null == e || null == t ? e === t : ("object" != typeof e || (e = vn(e), "object" != typeof e)) && (
            e === t || e !== e && t !== t)
        }

        function l(e, t, n, r, i) {
          var o, a = r.inputs;
          if (1 === a.length) {
            var s = u;
            return a = a[0], e.$watch(function(e) {
              var t = a(e);
              return u(t, s) || (o = r(e, void 0, void 0, [t]), s = t && vn(t)), o
            }, t, n, i)
          }
          for (var c = [], l = [], d = 0, f = a.length; d < f; d++) c[d] = u, l[d] = null;
          return e.$watch(function(e) {
            for (var t = !1, n = 0, i = a.length; n < i; n++) {
              var s = a[n](e);
              (t || (t = !u(s, c[n]))) && (l[n] = s, c[n] = s && vn(s))
            }
            return t && (o = r(e, void 0, void 0, l)), o
          }, t, n, i)
        }

        function d(e, t, n, r) {
          var i, o;
          return i = e.$watch(function(e) {
            return r(e)
          }, function(e, n, r) {
            o = e, T(t) && t.apply(this, arguments), y(e) && r.$$postDigest(function() {
              y(o) && i()
            })
          }, n)
        }

        function f(e, t, n, i) {
          function o(e) {
            var t = !0;
            return r(e, function(e) {
              y(e) || (t = !1)
            }), t
          }
          var a, s;
          return a = e.$watch(function(e) {
            return i(e)
          }, function(e, n, r) {
            s = e, T(t) && t.call(this, e, n, r), o(e) && r.$$postDigest(function() {
              o(s) && a()
            })
          }, n)
        }

        function p(e, t, n, r) {
          var i;
          return i = e.$watch(function(e) {
            return i(), r(e)
          }, t, n)
        }

        function m(e, t) {
          if (!t) return e;
          var n = e.$$watchDelegate,
            r = !1,
            i = n !== f && n !== d,
            o = i ? function(n, i, o, a) {
              var s = r && a ? a[0] : e(n, i, o, a);
              return t(s, n, i)
            } : function(n, r, i, o) {
              var a = e(n, r, i, o),
                s = t(a, n, r);
              return y(a) ? s : a
            };
          return e.$$watchDelegate && e.$$watchDelegate !== l ? o.$$watchDelegate = e.$$watchDelegate : t
            .$stateful || (o.$$watchDelegate = l, r = !e.inputs, o.inputs = e.inputs ? e.inputs : [e]), o
        }
        var v = Xr().noUnsafeEval,
          g = {
            csp: v,
            expensiveChecks: !1,
            literals: F(o),
            isIdentifierStart: T(e) && e,
            isIdentifierContinue: T(t) && t
          },
          b = {
            csp: v,
            expensiveChecks: !0,
            literals: F(o),
            isIdentifierStart: T(e) && e,
            isIdentifierContinue: T(t) && t
          },
          E = !1;
        return s.$$runningExpensiveChecks = function() {
          return E
        }, s
      }]
    }

    function yn() {
      this.$get = ["$rootScope", "$exceptionHandler", function(e, t) {
        return En(function(t) {
          e.$evalAsync(t)
        }, t)
      }]
    }

    function bn() {
      this.$get = ["$browser", "$exceptionHandler", function(e, t) {
        return En(function(t) {
          e.defer(t)
        }, t)
      }]
    }

    function En(e, n) {
      function i() {
        this.$$state = {
          status: 0
        }
      }

      function o(e, t) {
        return function(n) {
          t.call(e, n)
        }
      }

      function a(e) {
        var t, r, i;
        i = e.pending, e.processScheduled = !1, e.pending = void 0;
        for (var o = 0, a = i.length; o < a; ++o) {
          r = i[o][0], t = i[o][e.status];
          try {
            T(t) ? r.resolve(t(e.value)) : 1 === e.status ? r.resolve(e.value) : r.reject(e.value)
          } catch (e) {
            r.reject(e), n(e)
          }
        }
      }

      function s(t) {
        !t.processScheduled && t.pending && (t.processScheduled = !0, e(function() {
          a(t)
        }))
      }

      function c() {
        this.promise = new i
      }

      function l(e) {
        var t = new c,
          n = 0,
          i = Vr(e) ? [] : {};
        return r(e, function(e, r) {
          n++, v(e).then(function(e) {
            i.hasOwnProperty(r) || (i[r] = e, --n || t.resolve(i))
          }, function(e) {
            i.hasOwnProperty(r) || t.reject(e)
          })
        }), 0 === n && t.resolve(i), t.promise
      }
      var d = t("$q", TypeError),
        f = function() {
          var e = new c;
          return e.resolve = o(e, e.resolve), e.reject = o(e, e.reject), e.notify = o(e, e.notify), e
        };
      u(i.prototype, {
        then: function(e, t, n) {
          if (g(e) && g(t) && g(n)) return this;
          var r = new c;
          return this.$$state.pending = this.$$state.pending || [], this.$$state.pending.push([r, e, t, n]), this
            .$$state.status > 0 && s(this.$$state), r.promise
        },
        catch: function(e) {
          return this.then(null, e)
        },
        finally: function(e, t) {
          return this.then(function(t) {
            return m(t, !0, e)
          }, function(t) {
            return m(t, !1, e)
          }, t)
        }
      }), u(c.prototype, {
        resolve: function(e) {
          this.promise.$$state.status || (e === this.promise ? this.$$reject(d("qcycle",
            "Expected promise to be resolved with value other than itself '{0}'", e)) : this.$$resolve(e))
        },
        $$resolve: function(e) {
          function t(e) {
            c || (c = !0, a.$$resolve(e))
          }

          function r(e) {
            c || (c = !0, a.$$reject(e))
          }
          var i, a = this,
            c = !1;
          try {
            (b(e) || T(e)) && (i = e && e.then), T(i) ? (this.promise.$$state.status = -1, i.call(e, t, r, o(this,
              this.notify))) : (this.promise.$$state.value = e, this.promise.$$state.status = 1, s(this.promise
              .$$state))
          } catch (e) {
            r(e), n(e)
          }
        },
        reject: function(e) {
          this.promise.$$state.status || this.$$reject(e)
        },
        $$reject: function(e) {
          this.promise.$$state.value = e, this.promise.$$state.status = 2, s(this.promise.$$state)
        },
        notify: function(t) {
          var r = this.promise.$$state.pending;
          this.promise.$$state.status <= 0 && r && r.length && e(function() {
            for (var e, i, o = 0, a = r.length; o < a; o++) {
              i = r[o][0], e = r[o][3];
              try {
                i.notify(T(e) ? e(t) : t)
              } catch (e) {
                n(e)
              }
            }
          })
        }
      });
      var h = function(e) {
          var t = new c;
          return t.reject(e), t.promise
        },
        p = function(e, t) {
          var n = new c;
          return t ? n.resolve(e) : n.reject(e), n.promise
        },
        m = function(e, t, n) {
          var r = null;
          try {
            T(n) && (r = n())
          } catch (e) {
            return p(e, !1)
          }
          return I(r) ? r.then(function() {
            return p(e, t)
          }, function(e) {
            return p(e, !1)
          }) : p(e, t)
        },
        v = function(e, t, n, r) {
          var i = new c;
          return i.resolve(e), i.promise.then(t, n, r)
        },
        y = v,
        E = function(e) {
          function t(e) {
            r.resolve(e)
          }

          function n(e) {
            r.reject(e)
          }
          if (!T(e)) throw d("norslvr", "Expected resolverFn, got '{0}'", e);
          var r = new c;
          return e(t, n), r.promise
        };
      return E.prototype = i.prototype, E.defer = f, E.reject = h, E.when = v, E.resolve = y, E.all = l, E
    }

    function _n() {
      this.$get = ["$window", "$timeout", function(e, t) {
        var n = e.requestAnimationFrame || e.webkitRequestAnimationFrame,
          r = e.cancelAnimationFrame || e.webkitCancelAnimationFrame || e.webkitCancelRequestAnimationFrame,
          i = !!n,
          o = i ? function(e) {
            var t = n(e);
            return function() {
              r(t)
            }
          } : function(e) {
            var n = t(e, 16.66, !1);
            return function() {
              t.cancel(n)
            }
          };
        return o.supported = i, o
      }]
    }

    function $n() {
      function e(e) {
        function t() {
          this.$$watchers = this.$$nextSibling = this.$$childHead = this.$$childTail = null, this.$$listeners = {}, this
            .$$listenerCount = {}, this.$$watchersCount = 0, this.$id = a(), this.$$ChildScope = null
        }
        return t.prototype = e, t
      }
      var i = 10,
        o = t("$rootScope"),
        s = null,
        c = null;
      this.digestTtl = function(e) {
        return arguments.length && (i = e), i
      }, this.$get = ["$exceptionHandler", "$parse", "$browser", function(t, u, l) {
        function d(e) {
          e.currentScope.$$destroyed = !0
        }

        function f(e) {
          9 === Dr && (e.$$childHead && f(e.$$childHead), e.$$nextSibling && f(e.$$nextSibling)), e.$parent = e
            .$$nextSibling = e.$$prevSibling = e.$$childHead = e.$$childTail = e.$root = e.$$watchers = null
        }

        function p() {
          this.$id = a(), this.$$phase = this.$parent = this.$$watchers = this.$$nextSibling = this.$$prevSibling =
            this.$$childHead = this.$$childTail = null, this.$root = this, this.$$destroyed = !1, this
            .$$listeners = {}, this.$$listenerCount = {}, this.$$watchersCount = 0, this.$$isolateBindings = null
        }

        function m(e) {
          if (C.$$phase) throw o("inprog", "{0} already in progress", C.$$phase);
          C.$$phase = e
        }

        function v() {
          C.$$phase = null
        }

        function y(e, t) {
          do e.$$watchersCount += t; while (e = e.$parent)
        }

        function E(e, t, n) {
          do e.$$listenerCount[n] -= t, 0 === e.$$listenerCount[n] && delete e.$$listenerCount[n]; while (e = e
            .$parent)
        }

        function _() {}

        function $() {
          for (; A.length;) try {
            A.shift()()
          } catch (e) {
            t(e)
          }
          c = null
        }

        function w() {
          null === c && (c = l.defer(function() {
            C.$apply($)
          }))
        }
        p.prototype = {
          constructor: p,
          $new: function(t, n) {
            var r;
            return n = n || this, t ? (r = new p, r.$root = this.$root) : (this.$$ChildScope || (this
                .$$ChildScope = e(this)), r = new this.$$ChildScope), r.$parent = n, r.$$prevSibling = n
              .$$childTail, n.$$childHead ? (n.$$childTail.$$nextSibling = r, n.$$childTail = r) : n
              .$$childHead = n.$$childTail = r, (t || n != this) && r.$on("$destroy", d), r
          },
          $watch: function(e, t, n, r) {
            var i = u(e);
            if (i.$$watchDelegate) return i.$$watchDelegate(this, t, n, i, e);
            var o = this,
              a = o.$$watchers,
              c = {
                fn: t,
                last: _,
                get: i,
                exp: r || e,
                eq: !!n
              };
            return s = null, T(t) || (c.fn = h), a || (a = o.$$watchers = []), a.unshift(c), y(this, 1),
              function() {
                U(a, c) >= 0 && y(o, -1), s = null
              }
          },
          $watchGroup: function(e, t) {
            function n() {
              c = !1, u ? (u = !1, t(o, o, s)) : t(o, i, s)
            }
            var i = new Array(e.length),
              o = new Array(e.length),
              a = [],
              s = this,
              c = !1,
              u = !0;
            if (!e.length) {
              var l = !0;
              return s.$evalAsync(function() {
                  l && t(o, o, s)
                }),
                function() {
                  l = !1
                }
            }
            return 1 === e.length ? this.$watch(e[0], function(e, n, r) {
              o[0] = e, i[0] = n, t(o, e === n ? o : i, r)
            }) : (r(e, function(e, t) {
              var r = s.$watch(e, function(e, r) {
                o[t] = e, i[t] = r, c || (c = !0, s.$evalAsync(n))
              });
              a.push(r)
            }), function() {
              for (; a.length;) a.shift()()
            })
          },
          $watchCollection: function(e, t) {
            function r(e) {
              o = e;
              var t, r, i, s, c;
              if (!g(o)) {
                if (b(o))
                  if (n(o)) {
                    a !== h && (a = h, v = a.length = 0, d++), t = o.length, v !== t && (d++, a.length = v = t);
                    for (var u = 0; u < t; u++) c = a[u], s = o[u], i = c !== c && s !== s, i || c === s || (
                      d++, a[u] = s)
                  } else {
                    a !== p && (a = p = {}, v = 0, d++), t = 0;
                    for (r in o) Mr.call(o, r) && (t++, s = o[r], c = a[r], r in a ? (i = c !== c && s !== s,
                      i || c === s || (d++, a[r] = s)) : (v++, a[r] = s, d++));
                    if (v > t) {
                      d++;
                      for (r in a) Mr.call(o, r) || (v--, delete a[r])
                    }
                  }
                else a !== o && (a = o, d++);
                return d
              }
            }

            function i() {
              if (m ? (m = !1, t(o, o, c)) : t(o, s, c), l)
                if (b(o))
                  if (n(o)) {
                    s = new Array(o.length);
                    for (var e = 0; e < o.length; e++) s[e] = o[e]
                  } else {
                    s = {};
                    for (var r in o) Mr.call(o, r) && (s[r] = o[r])
                  }
              else s = o
            }
            r.$stateful = !0;
            var o, a, s, c = this,
              l = t.length > 1,
              d = 0,
              f = u(e, r),
              h = [],
              p = {},
              m = !0,
              v = 0;
            return this.$watch(f, i)
          },
          $digest: function() {
            var e, n, r, a, u, d, f, h, p, g, y, b, E = i,
              w = this,
              A = [];
            m("$digest"), l.$$checkUrlChange(), this === C && null !== c && (l.defer.cancel(c), $()), s = null;
            do {
              for (h = !1, g = w; x.length;) {
                try {
                  b = x.shift(), b.scope.$eval(b.expression, b.locals)
                } catch (e) {
                  t(e)
                }
                s = null
              }
              e: do {
                if (d = g.$$watchers)
                  for (f = d.length; f--;) try {
                    if (e = d[f])
                      if (u = e.get, (n = u(g)) === (r = e.last) || (e.eq ? H(n, r) : "number" ==
                          typeof n && "number" == typeof r && isNaN(n) && isNaN(r))) {
                        if (e === s) {
                          h = !1;
                          break e
                        }
                      } else h = !0, s = e, e.last = e.eq ? F(n, null) : n, a = e.fn, a(n, r === _ ? n : r,
                        g), E < 5 && (y = 4 - E, A[y] || (A[y] = []), A[y].push({
                        msg: T(e.exp) ? "fn: " + (e.exp.name || e.exp.toString()) : e.exp,
                        newVal: n,
                        oldVal: r
                      }))
                  } catch (e) {
                    t(e)
                  }
                if (!(p = g.$$watchersCount && g.$$childHead || g !== w && g.$$nextSibling))
                  for (; g !== w && !(p = g.$$nextSibling);) g = g.$parent
              } while (g = p);
              if ((h || x.length) && !E--) throw v(), o("infdig",
                "{0} $digest() iterations reached. Aborting!\nWatchers fired in the last 5 iterations: {1}",
                i, A)
            } while (h || x.length);
            for (v(); S.length;) try {
              S.shift()()
            } catch (e) {
              t(e)
            }
          },
          $destroy: function() {
            if (!this.$$destroyed) {
              var e = this.$parent;
              this.$broadcast("$destroy"), this.$$destroyed = !0, this === C && l.$$applicationDestroyed(), y(
                this, -this.$$watchersCount);
              for (var t in this.$$listenerCount) E(this, this.$$listenerCount[t], t);
              e && e.$$childHead == this && (e.$$childHead = this.$$nextSibling), e && e.$$childTail == this &&
                (e.$$childTail = this.$$prevSibling), this.$$prevSibling && (this.$$prevSibling.$$nextSibling =
                  this.$$nextSibling), this.$$nextSibling && (this.$$nextSibling.$$prevSibling = this
                  .$$prevSibling), this.$destroy = this.$digest = this.$apply = this.$evalAsync = this
                .$applyAsync = h, this.$on = this.$watch = this.$watchGroup = function() {
                  return h
                }, this.$$listeners = {}, this.$$nextSibling = null, f(this)
            }
          },
          $eval: function(e, t) {
            return u(e)(this, t)
          },
          $evalAsync: function(e, t) {
            C.$$phase || x.length || l.defer(function() {
              x.length && C.$digest()
            }), x.push({
              scope: this,
              expression: u(e),
              locals: t
            })
          },
          $$postDigest: function(e) {
            S.push(e)
          },
          $apply: function(e) {
            try {
              m("$apply");
              try {
                return this.$eval(e)
              } finally {
                v()
              }
            } catch (e) {
              t(e)
            } finally {
              try {
                C.$digest()
              } catch (e) {
                throw t(e), e
              }
            }
          },
          $applyAsync: function(e) {
            function t() {
              n.$eval(e)
            }
            var n = this;
            e && A.push(t), e = u(e), w()
          },
          $on: function(e, t) {
            var n = this.$$listeners[e];
            n || (this.$$listeners[e] = n = []), n.push(t);
            var r = this;
            do r.$$listenerCount[e] || (r.$$listenerCount[e] = 0), r.$$listenerCount[e]++; while (r = r
              .$parent);
            var i = this;
            return function() {
              var r = n.indexOf(t);
              r !== -1 && (n[r] = null, E(i, 1, e))
            }
          },
          $emit: function(e, n) {
            var r, i, o, a = [],
              s = this,
              c = !1,
              u = {
                name: e,
                targetScope: s,
                stopPropagation: function() {
                  c = !0
                },
                preventDefault: function() {
                  u.defaultPrevented = !0
                },
                defaultPrevented: !1
              },
              l = B([u], arguments, 1);
            do {
              for (r = s.$$listeners[e] || a, u.currentScope = s, i = 0, o = r.length; i < o; i++)
                if (r[i]) try {
                  r[i].apply(null, l)
                } catch (e) {
                  t(e)
                } else r.splice(i, 1), i--, o--;
              if (c) return u.currentScope = null, u;
              s = s.$parent
            } while (s);
            return u.currentScope = null, u
          },
          $broadcast: function(e, n) {
            var r = this,
              i = r,
              o = r,
              a = {
                name: e,
                targetScope: r,
                preventDefault: function() {
                  a.defaultPrevented = !0
                },
                defaultPrevented: !1
              };
            if (!r.$$listenerCount[e]) return a;
            for (var s, c, u, l = B([a], arguments, 1); i = o;) {
              for (a.currentScope = i, s = i.$$listeners[e] || [], c = 0, u = s.length; c < u; c++)
                if (s[c]) try {
                  s[c].apply(null, l)
                } catch (e) {
                  t(e)
                } else s.splice(c, 1), c--, u--;
              if (!(o = i.$$listenerCount[e] && i.$$childHead || i !== r && i.$$nextSibling))
                for (; i !== r && !(o = i.$$nextSibling);) i = i.$parent
            }
            return a.currentScope = null, a
          }
        };
        var C = new p,
          x = C.$$asyncQueue = [],
          S = C.$$postDigestQueue = [],
          A = C.$$applyAsyncQueue = [];
        return C
      }]
    }

    function wn() {
      var e = /^\s*(https?|ftp|mailto|tel|file):/,
        t = /^\s*((https?|ftp|file|blob):|data:image\/)/;
      this.aHrefSanitizationWhitelist = function(t) {
        return y(t) ? (e = t, this) : e
      }, this.imgSrcSanitizationWhitelist = function(e) {
        return y(e) ? (t = e, this) : t
      }, this.$get = function() {
        return function(n, r) {
          var i, o = r ? t : e;
          return i = In(n).href, "" === i || i.match(o) ? n : "unsafe:" + i
        }
      }
    }

    function Tn(e) {
      if ("self" === e) return e;
      if (_(e)) {
        if (e.indexOf("***") > -1) throw yo("iwcard", "Illegal sequence *** in string matcher.  String: {0}", e);
        return e = Kr(e).replace("\\*\\*", ".*").replace("\\*", "[^:/.?&;]*"), new RegExp("^" + e + "$")
      }
      if (C(e)) return new RegExp("^" + e.source + "$");
      throw yo("imatcher", 'Matchers may only be "self", string patterns or RegExp objects')
    }

    function Cn(e) {
      var t = [];
      return y(e) && r(e, function(e) {
        t.push(Tn(e))
      }), t
    }

    function xn() {
      this.SCE_CONTEXTS = bo;
      var e = ["self"],
        t = [];
      this.resourceUrlWhitelist = function(t) {
        return arguments.length && (e = Cn(t)), e
      }, this.resourceUrlBlacklist = function(e) {
        return arguments.length && (t = Cn(e)), t
      }, this.$get = ["$injector", function(n) {
        function r(e, t) {
          return "self" === e ? On(t) : !!e.exec(t.href)
        }

        function i(n) {
          var i, o, a = In(n.toString()),
            s = !1;
          for (i = 0, o = e.length; i < o; i++)
            if (r(e[i], a)) {
              s = !0;
              break
            } if (s)
            for (i = 0, o = t.length; i < o; i++)
              if (r(t[i], a)) {
                s = !1;
                break
              } return s
        }

        function o(e) {
          var t = function(e) {
            this.$$unwrapTrustedValue = function() {
              return e
            }
          };
          return e && (t.prototype = new e), t.prototype.valueOf = function() {
            return this.$$unwrapTrustedValue()
          }, t.prototype.toString = function() {
            return this.$$unwrapTrustedValue().toString()
          }, t
        }

        function a(e, t) {
          var n = d.hasOwnProperty(e) ? d[e] : null;
          if (!n) throw yo("icontext", "Attempted to trust a value in invalid context. Context: {0}; Value: {1}", e,
            t);
          if (null === t || g(t) || "" === t) return t;
          if ("string" != typeof t) throw yo("itype",
            "Attempted to trust a non-string value in a content requiring a string: Context: {0}", e);
          return new n(t)
        }

        function s(e) {
          return e instanceof l ? e.$$unwrapTrustedValue() : e
        }

        function c(e, t) {
          if (null === t || g(t) || "" === t) return t;
          var n = d.hasOwnProperty(e) ? d[e] : null;
          if (n && t instanceof n) return t.$$unwrapTrustedValue();
          if (e === bo.RESOURCE_URL) {
            if (i(t)) return t;
            throw yo("insecurl", "Blocked loading resource from url not allowed by $sceDelegate policy.  URL: {0}",
              t.toString())
          }
          if (e === bo.HTML) return u(t);
          throw yo("unsafe", "Attempting to use an unsafe value in a safe context.")
        }
        var u = function(e) {
          throw yo("unsafe", "Attempting to use an unsafe value in a safe context.")
        };
        n.has("$sanitize") && (u = n.get("$sanitize"));
        var l = o(),
          d = {};
        return d[bo.HTML] = o(l), d[bo.CSS] = o(l), d[bo.URL] = o(l), d[bo.JS] = o(l), d[bo.RESOURCE_URL] = o(d[bo
          .URL]), {
          trustAs: a,
          getTrusted: c,
          valueOf: s
        }
      }]
    }

    function Sn() {
      var e = !0;
      this.enabled = function(t) {
        return arguments.length && (e = !!t), e
      }, this.$get = ["$parse", "$sceDelegate", function(t, n) {
        if (e && Dr < 8) throw yo("iequirks",
          "Strict Contextual Escaping does not support Internet Explorer version < 11 in quirks mode.  You can fix this by adding the text <!doctype html> to the top of your HTML document.  See http://docs.angularjs.org/api/ng.$sce for more information."
          );
        var i = j(bo);
        i.isEnabled = function() {
          return e
        }, i.trustAs = n.trustAs, i.getTrusted = n.getTrusted, i.valueOf = n.valueOf, e || (i.trustAs = i
          .getTrusted = function(e, t) {
            return t
          }, i.valueOf = p), i.parseAs = function(e, n) {
          var r = t(n);
          return r.literal && r.constant ? r : t(n, function(t) {
            return i.getTrusted(e, t)
          })
        };
        var o = i.parseAs,
          a = i.getTrusted,
          s = i.trustAs;
        return r(bo, function(e, t) {
          var n = kr(t);
          i[_e("parse_as_" + n)] = function(t) {
            return o(e, t)
          }, i[_e("get_trusted_" + n)] = function(t) {
            return a(e, t)
          }, i[_e("trust_as_" + n)] = function(t) {
            return s(e, t)
          }
        }), i
      }]
    }

    function An() {
      this.$get = ["$window", "$document", function(e, t) {
        var n, r, i = {},
          o = e.chrome && e.chrome.app && e.chrome.app.runtime,
          a = !o && e.history && e.history.pushState,
          s = d((/android (\d+)/.exec(kr((e.navigator || {}).userAgent)) || [])[1]),
          c = /Boxee/i.test((e.navigator || {}).userAgent),
          u = t[0] || {},
          l = /^(Moz|webkit|ms)(?=[A-Z])/,
          f = u.body && u.body.style,
          h = !1,
          p = !1;
        if (f) {
          for (var m in f)
            if (r = l.exec(m)) {
              n = r[0], n = n.substr(0, 1).toUpperCase() + n.substr(1);
              break
            } n || (n = "WebkitOpacity" in f && "webkit"), h = !!("transition" in f || n + "Transition" in f), p = !
            !("animation" in f || n + "Animation" in f), !s || h && p || (h = _(f.webkitTransition), p = _(f
              .webkitAnimation))
        }
        return {
          history: !(!a || s < 4 || c),
          hasEvent: function(e) {
            if ("input" === e && Dr <= 11) return !1;
            if (g(i[e])) {
              var t = u.createElement("div");
              i[e] = "on" + e in t
            }
            return i[e]
          },
          csp: Xr(),
          vendorPrefix: n,
          transitions: h,
          animations: p,
          android: s
        }
      }]
    }

    function Mn() {
      var e;
      this.httpOptions = function(t) {
        return t ? (e = t, this) : e
      }, this.$get = ["$templateCache", "$http", "$q", "$sce", function(t, n, r, i) {
        function o(a, s) {
          function c(e) {
            if (!s) throw Eo("tpload", "Failed to load template: {0} (HTTP status: {1} {2})", a, e.status, e
              .statusText);
            return r.reject(e)
          }
          o.totalPendingRequests++, _(a) && t.get(a) || (a = i.getTrustedResourceUrl(a));
          var l = n.defaults && n.defaults.transformResponse;
          return Vr(l) ? l = l.filter(function(e) {
            return e !== Ct
          }) : l === Ct && (l = null), n.get(a, u({
            cache: t,
            transformResponse: l
          }, e)).finally(function() {
            o.totalPendingRequests--
          }).then(function(e) {
            return t.put(a, e.data), e.data
          }, c)
        }
        return o.totalPendingRequests = 0, o
      }]
    }

    function kn() {
      this.$get = ["$rootScope", "$browser", "$location", function(e, t, n) {
        var i = {};
        return i.findBindings = function(e, t, n) {
          var i = e.getElementsByClassName("ng-binding"),
            o = [];
          return r(i, function(e) {
            var i = qr.element(e).data("$binding");
            i && r(i, function(r) {
              if (n) {
                var i = new RegExp("(^|\\s)" + Kr(t) + "(\\s|\\||$)");
                i.test(r) && o.push(e)
              } else r.indexOf(t) != -1 && o.push(e)
            })
          }), o
        }, i.findModels = function(e, t, n) {
          for (var r = ["ng-", "data-ng-", "ng\\:"], i = 0; i < r.length; ++i) {
            var o = n ? "=" : "*=",
              a = "[" + r[i] + "model" + o + '"' + t + '"]',
              s = e.querySelectorAll(a);
            if (s.length) return s
          }
        }, i.getLocation = function() {
          return n.url()
        }, i.setLocation = function(t) {
          t !== n.url() && (n.url(t), e.$digest())
        }, i.whenStable = function(e) {
          t.notifyWhenNoOutstandingRequests(e)
        }, i
      }]
    }

    function Nn() {
      this.$get = ["$rootScope", "$browser", "$q", "$$q", "$exceptionHandler", function(e, t, n, r, i) {
        function o(o, s, c) {
          T(o) || (c = s, s = o, o = h);
          var u, l = z(arguments, 3),
            d = y(c) && !c,
            f = (d ? r : n).defer(),
            p = f.promise;
          return u = t.defer(function() {
            try {
              f.resolve(o.apply(null, l))
            } catch (e) {
              f.reject(e), i(e)
            } finally {
              delete a[p.$$timeoutId]
            }
            d || e.$apply()
          }, s), p.$$timeoutId = u, a[u] = f, p
        }
        var a = {};
        return o.cancel = function(e) {
          return !!(e && e.$$timeoutId in a) && (a[e.$$timeoutId].reject("canceled"), delete a[e.$$timeoutId], t
            .defer.cancel(e.$$timeoutId))
        }, o
      }]
    }

    function In(e) {
      var t = e;
      return Dr && (_o.setAttribute("href", t), t = _o.href), _o.setAttribute("href", t), {
        href: _o.href,
        protocol: _o.protocol ? _o.protocol.replace(/:$/, "") : "",
        host: _o.host,
        search: _o.search ? _o.search.replace(/^\?/, "") : "",
        hash: _o.hash ? _o.hash.replace(/^#/, "") : "",
        hostname: _o.hostname,
        port: _o.port,
        pathname: "/" === _o.pathname.charAt(0) ? _o.pathname : "/" + _o.pathname
      }
    }

    function On(e) {
      var t = _(e) ? In(e) : e;
      return t.protocol === $o.protocol && t.host === $o.host
    }

    function Dn() {
      this.$get = m(e)
    }

    function Rn(e) {
      function t(e) {
        try {
          return decodeURIComponent(e)
        } catch (t) {
          return e
        }
      }
      var n = e[0] || {},
        r = {},
        i = "";
      return function() {
        var e, o, a, s, c, u = n.cookie || "";
        if (u !== i)
          for (i = u, e = i.split("; "), r = {}, a = 0; a < e.length; a++) o = e[a], s = o.indexOf("="), s > 0 && (c =
            t(o.substring(0, s)), g(r[c]) && (r[c] = t(o.substring(s + 1))));
        return r
      }
    }

    function Pn() {
      this.$get = Rn
    }

    function Ln(e) {
      function t(i, o) {
        if (b(i)) {
          var a = {};
          return r(i, function(e, n) {
            a[n] = t(n, e)
          }), a
        }
        return e.factory(i + n, o)
      }
      var n = "Filter";
      this.register = t, this.$get = ["$injector", function(e) {
        return function(t) {
          return e.get(t + n)
        }
      }], t("currency", Bn), t("date", rr), t("filter", Un), t("json", ir), t("limitTo", or), t("lowercase", Mo), t(
        "number", zn), t("orderBy", ar), t("uppercase", ko)
    }

    function Un() {
      return function(e, r, i) {
        if (!n(e)) {
          if (null == e) return e;
          throw t("filter")("notarray", "Expected array but received: {0}", e)
        }
        var o, a, s = Hn(r);
        switch (s) {
          case "function":
            o = r;
            break;
          case "boolean":
          case "null":
          case "number":
          case "string":
            a = !0;
          case "object":
            o = Fn(r, i, a);
            break;
          default:
            return e
        }
        return Array.prototype.filter.call(e, o)
      }
    }

    function Fn(e, t, n) {
      var r, i = b(e) && "$" in e;
      return t === !0 ? t = H : T(t) || (t = function(e, t) {
        return !g(e) && (null === e || null === t ? e === t : !(b(t) || b(e) && !v(e)) && (e = kr("" + e), t = kr(
          "" + t), e.indexOf(t) !== -1))
      }), r = function(r) {
        return i && !b(r) ? jn(r, e.$, t, !1) : jn(r, e, t, n)
      }
    }

    function jn(e, t, n, r, i) {
      var o = Hn(e),
        a = Hn(t);
      if ("string" === a && "!" === t.charAt(0)) return !jn(e, t.substring(1), n, r);
      if (Vr(e)) return e.some(function(e) {
        return jn(e, t, n, r)
      });
      switch (o) {
        case "object":
          var s;
          if (r) {
            for (s in e)
              if ("$" !== s.charAt(0) && jn(e[s], t, n, !0)) return !0;
            return !i && jn(e, t, n, !1)
          }
          if ("object" === a) {
            for (s in t) {
              var c = t[s];
              if (!T(c) && !g(c)) {
                var u = "$" === s,
                  l = u ? e : e[s];
                if (!jn(l, c, n, u, u)) return !1
              }
            }
            return !0
          }
          return n(e, t);
        case "function":
          return !1;
        default:
          return n(e, t)
      }
    }

    function Hn(e) {
      return null === e ? "null" : typeof e
    }

    function Bn(e) {
      var t = e.NUMBER_FORMATS;
      return function(e, n, r) {
        return g(n) && (n = t.CURRENCY_SYM), g(r) && (r = t.PATTERNS[1].maxFrac), null == e ? e : Vn(e, t.PATTERNS[1],
          t.GROUP_SEP, t.DECIMAL_SEP, r).replace(/\u00A4/g, n)
      }
    }

    function zn(e) {
      var t = e.NUMBER_FORMATS;
      return function(e, n) {
        return null == e ? e : Vn(e, t.PATTERNS[0], t.GROUP_SEP, t.DECIMAL_SEP, n)
      }
    }

    function qn(e) {
      var t, n, r, i, o, a = 0;
      for ((n = e.indexOf(To)) > -1 && (e = e.replace(To, "")), (r = e.search(/e/i)) > 0 ? (n < 0 && (n = r), n += +e
          .slice(r + 1), e = e.substring(0, r)) : n < 0 && (n = e.length), r = 0; e.charAt(r) == Co; r++);
      if (r == (o = e.length)) t = [0], n = 1;
      else {
        for (o--; e.charAt(o) == Co;) o--;
        for (n -= r, t = [], i = 0; r <= o; r++, i++) t[i] = +e.charAt(r)
      }
      return n > wo && (t = t.splice(0, wo - 1), a = n - 1, n = 1), {
        d: t,
        e: a,
        i: n
      }
    }

    function Gn(e, t, n, r) {
      var i = e.d,
        o = i.length - e.i;
      t = g(t) ? Math.min(Math.max(n, o), r) : +t;
      var a = t + e.i,
        s = i[a];
      if (a > 0) {
        i.splice(Math.max(e.i, a));
        for (var c = a; c < i.length; c++) i[c] = 0
      } else {
        o = Math.max(0, o), e.i = 1, i.length = Math.max(1, a = t + 1), i[0] = 0;
        for (var u = 1; u < a; u++) i[u] = 0
      }
      if (s >= 5)
        if (a - 1 < 0) {
          for (var l = 0; l > a; l--) i.unshift(0), e.i++;
          i.unshift(1), e.i++
        } else i[a - 1]++;
      for (; o < Math.max(0, t); o++) i.push(0);
      var d = i.reduceRight(function(e, t, n, r) {
        return t += e, r[n] = t % 10, Math.floor(t / 10)
      }, 0);
      d && (i.unshift(d), e.i++)
    }

    function Vn(e, t, n, r, i) {
      if (!_(e) && !$(e) || isNaN(e)) return "";
      var o, a = !isFinite(e),
        s = !1,
        c = Math.abs(e) + "",
        u = "";
      if (a) u = "∞";
      else {
        o = qn(c), Gn(o, i, t.minFrac, t.maxFrac);
        var l = o.d,
          d = o.i,
          f = o.e,
          h = [];
        for (s = l.reduce(function(e, t) {
            return e && !t
          }, !0); d < 0;) l.unshift(0), d++;
        d > 0 ? h = l.splice(d) : (h = l, l = [0]);
        var p = [];
        for (l.length >= t.lgSize && p.unshift(l.splice(-t.lgSize).join("")); l.length > t.gSize;) p.unshift(l.splice(-t
          .gSize).join(""));
        l.length && p.unshift(l.join("")), u = p.join(n), h.length && (u += r + h.join("")), f && (u += "e+" + f)
      }
      return e < 0 && !s ? t.negPre + u + t.negSuf : t.posPre + u + t.posSuf
    }

    function Wn(e, t, n, r) {
      var i = "";
      for ((e < 0 || r && e <= 0) && (r ? e = -e + 1 : (e = -e, i = "-")), e = "" + e; e.length < t;) e = Co + e;
      return n && (e = e.substr(e.length - t)), i + e
    }

    function Yn(e, t, n, r, i) {
      return n = n || 0,
        function(o) {
          var a = o["get" + e]();
          return (n > 0 || a > -n) && (a += n), 0 === a && n == -12 && (a = 12), Wn(a, t, r, i)
        }
    }

    function Kn(e, t, n) {
      return function(r, i) {
        var o = r["get" + e](),
          a = (n ? "STANDALONE" : "") + (t ? "SHORT" : ""),
          s = Nr(a + e);
        return i[s][o]
      }
    }

    function Xn(e, t, n) {
      var r = -1 * n,
        i = r >= 0 ? "+" : "";
      return i += Wn(Math[r > 0 ? "floor" : "ceil"](r / 60), 2) + Wn(Math.abs(r % 60), 2)
    }

    function Qn(e) {
      var t = new Date(e, 0, 1).getDay();
      return new Date(e, 0, (t <= 4 ? 5 : 12) - t)
    }

    function Jn(e) {
      return new Date(e.getFullYear(), e.getMonth(), e.getDate() + (4 - e.getDay()))
    }

    function Zn(e) {
      return function(t) {
        var n = Qn(t.getFullYear()),
          r = Jn(t),
          i = +r - +n,
          o = 1 + Math.round(i / 6048e5);
        return Wn(o, e)
      }
    }

    function er(e, t) {
      return e.getHours() < 12 ? t.AMPMS[0] : t.AMPMS[1]
    }

    function tr(e, t) {
      return e.getFullYear() <= 0 ? t.ERAS[0] : t.ERAS[1]
    }

    function nr(e, t) {
      return e.getFullYear() <= 0 ? t.ERANAMES[0] : t.ERANAMES[1]
    }

    function rr(e) {
      function t(e) {
        var t;
        if (t = e.match(n)) {
          var r = new Date(0),
            i = 0,
            o = 0,
            a = t[8] ? r.setUTCFullYear : r.setFullYear,
            s = t[8] ? r.setUTCHours : r.setHours;
          t[9] && (i = d(t[9] + t[10]), o = d(t[9] + t[11])), a.call(r, d(t[1]), d(t[2]) - 1, d(t[3]));
          var c = d(t[4] || 0) - i,
            u = d(t[5] || 0) - o,
            l = d(t[6] || 0),
            f = Math.round(1e3 * parseFloat("0." + (t[7] || 0)));
          return s.call(r, c, u, l, f), r
        }
        return e
      }
      var n = /^(\d{4})-?(\d\d)-?(\d\d)(?:T(\d\d)(?::?(\d\d)(?::?(\d\d)(?:\.(\d+))?)?)?(Z|([+-])(\d\d):?(\d\d))?)?$/;
      return function(n, i, o) {
        var a, s, c = "",
          u = [];
        if (i = i || "mediumDate", i = e.DATETIME_FORMATS[i] || i, _(n) && (n = Ao.test(n) ? d(n) : t(n)), $(n) && (
            n = new Date(n)), !w(n) || !isFinite(n.getTime())) return n;
        for (; i;) s = So.exec(i), s ? (u = B(u, s, 1), i = u.pop()) : (u.push(i), i = null);
        var l = n.getTimezoneOffset();
        return o && (l = Y(o, l), n = X(n, o, !0)), r(u, function(t) {
          a = xo[t], c += a ? a(n, e.DATETIME_FORMATS, l) : "''" === t ? "'" : t.replace(/(^'|'$)/g, "").replace(
            /''/g, "'")
        }), c
      }
    }

    function ir() {
      return function(e, t) {
        return g(t) && (t = 2), V(e, t)
      }
    }

    function or() {
      return function(e, t, n) {
        return t = Math.abs(Number(t)) === 1 / 0 ? Number(t) : d(t), isNaN(t) ? e : ($(e) && (e = e.toString()), Vr(
          e) || _(e) ? (n = !n || isNaN(n) ? 0 : d(n), n = n < 0 ? Math.max(0, e.length + n) : n, t >= 0 ? e.slice(
            n, n + t) : 0 === n ? e.slice(t, e.length) : e.slice(Math.max(0, n + t), n)) : e)
      }
    }

    function ar(e) {
      function r(t, n) {
        return n = n ? -1 : 1, t.map(function(t) {
          var r = 1,
            i = p;
          if (T(t)) i = t;
          else if (_(t) && ("+" != t.charAt(0) && "-" != t.charAt(0) || (r = "-" == t.charAt(0) ? -1 : 1, t = t
              .substring(1)), "" !== t && (i = e(t), i.constant))) {
            var o = i();
            i = function(e) {
              return e[o]
            }
          }
          return {
            get: i,
            descending: r * n
          }
        })
      }

      function i(e) {
        switch (typeof e) {
          case "number":
          case "boolean":
          case "string":
            return !0;
          default:
            return !1
        }
      }

      function o(e, t) {
        return "function" == typeof e.valueOf && (e = e.valueOf(), i(e)) ? e : v(e) && (e = e.toString(), i(e)) ? e : t
      }

      function a(e, t) {
        var n = typeof e;
        return null === e ? (n = "string", e = "null") : "string" === n ? e = e.toLowerCase() : "object" === n && (e =
          o(e, t)), {
          value: e,
          type: n
        }
      }

      function s(e, t) {
        var n = 0;
        return e.type === t.type ? e.value !== t.value && (n = e.value < t.value ? -1 : 1) : n = e.type < t.type ? -1 :
          1, n
      }
      return function(e, i, o) {
        function c(e, t) {
          return {
            value: e,
            predicateValues: l.map(function(n) {
              return a(n.get(e), t)
            })
          }
        }

        function u(e, t) {
          for (var n = 0, r = 0, i = l.length; r < i && !(n = s(e.predicateValues[r], t.predicateValues[r]) * l[r]
              .descending); ++r);
          return n
        }
        if (null == e) return e;
        if (!n(e)) throw t("orderBy")("notarray", "Expected array but received: {0}", e);
        Vr(i) || (i = [i]), 0 === i.length && (i = ["+"]);
        var l = r(i, o);
        l.push({
          get: function() {
            return {}
          },
          descending: o ? -1 : 1
        });
        var d = Array.prototype.map.call(e, c);
        return d.sort(u), e = d.map(function(e) {
          return e.value
        })
      }
    }

    function sr(e) {
      return T(e) && (e = {
        link: e
      }), e.restrict = e.restrict || "AC", m(e)
    }

    function cr(e, t) {
      e.$name = t
    }

    function ur(e, t, n, i, o) {
      var a = this,
        s = [];
      a.$error = {}, a.$$success = {}, a.$pending = void 0, a.$name = o(t.name || t.ngForm || "")(n), a.$dirty = !1, a
        .$pristine = !0, a.$valid = !0, a.$invalid = !1, a.$submitted = !1, a.$$parentForm = Oo, a.$rollbackViewValue =
        function() {
          r(s, function(e) {
            e.$rollbackViewValue()
          })
        }, a.$commitViewValue = function() {
          r(s, function(e) {
            e.$commitViewValue()
          })
        }, a.$addControl = function(e) {
          fe(e.$name, "input"), s.push(e), e.$name && (a[e.$name] = e), e.$$parentForm = a
        }, a.$$renameControl = function(e, t) {
          var n = e.$name;
          a[n] === e && delete a[n], a[t] = e, e.$name = t
        }, a.$removeControl = function(e) {
          e.$name && a[e.$name] === e && delete a[e.$name], r(a.$pending, function(t, n) {
            a.$setValidity(n, null, e)
          }), r(a.$error, function(t, n) {
            a.$setValidity(n, null, e)
          }), r(a.$$success, function(t, n) {
            a.$setValidity(n, null, e)
          }), U(s, e), e.$$parentForm = Oo
        }, Tr({
          ctrl: this,
          $element: e,
          set: function(e, t, n) {
            var r = e[t];
            if (r) {
              var i = r.indexOf(n);
              i === -1 && r.push(n)
            } else e[t] = [n]
          },
          unset: function(e, t, n) {
            var r = e[t];
            r && (U(r, n), 0 === r.length && delete e[t])
          },
          $animate: i
        }), a.$setDirty = function() {
          i.removeClass(e, ga), i.addClass(e, ya), a.$dirty = !0, a.$pristine = !1, a.$$parentForm.$setDirty()
        }, a.$setPristine = function() {
          i.setClass(e, ga, ya + " " + Do), a.$dirty = !1, a.$pristine = !0, a.$submitted = !1, r(s, function(e) {
            e.$setPristine()
          })
        }, a.$setUntouched = function() {
          r(s, function(e) {
            e.$setUntouched()
          })
        }, a.$setSubmitted = function() {
          i.addClass(e, Do), a.$submitted = !0, a.$$parentForm.$setSubmitted()
        }
    }

    function lr(e) {
      e.$formatters.push(function(t) {
        return e.$isEmpty(t) ? t : t.toString()
      })
    }

    function dr(e, t, n, r, i, o) {
      fr(e, t, n, r, i, o), lr(r)
    }

    function fr(e, t, n, r, i, o) {
      var a = kr(t[0].type);
      if (!i.android) {
        var s = !1;
        t.on("compositionstart", function() {
          s = !0
        }), t.on("compositionend", function() {
          s = !1, u()
        })
      }
      var c, u = function(e) {
        if (c && (o.defer.cancel(c), c = null), !s) {
          var i = t.val(),
            u = e && e.type;
          "password" === a || n.ngTrim && "false" === n.ngTrim || (i = Yr(i)), (r.$viewValue !== i || "" === i && r
            .$$hasNativeValidators) && r.$setViewValue(i, u)
        }
      };
      if (i.hasEvent("input")) t.on("input", u);
      else {
        var l = function(e, t, n) {
          c || (c = o.defer(function() {
            c = null, t && t.value === n || u(e)
          }))
        };
        t.on("keydown", function(e) {
          var t = e.keyCode;
          91 === t || 15 < t && t < 19 || 37 <= t && t <= 40 || l(e, this, this.value)
        }), i.hasEvent("paste") && t.on("paste cut", l)
      }
      t.on("change", u), Yo[a] && r.$$hasNativeValidators && a === n.type && t.on(Wo, function(e) {
        if (!c) {
          var t = this[Ar],
            n = t.badInput,
            r = t.typeMismatch;
          c = o.defer(function() {
            c = null, t.badInput === n && t.typeMismatch === r || u(e)
          })
        }
      }), r.$render = function() {
        var e = r.$isEmpty(r.$viewValue) ? "" : r.$viewValue;
        t.val() !== e && t.val(e)
      }
    }

    function hr(e, t) {
      if (w(e)) return e;
      if (_(e)) {
        qo.lastIndex = 0;
        var n = qo.exec(e);
        if (n) {
          var r = +n[1],
            i = +n[2],
            o = 0,
            a = 0,
            s = 0,
            c = 0,
            u = Qn(r),
            l = 7 * (i - 1);
          return t && (o = t.getHours(), a = t.getMinutes(), s = t.getSeconds(), c = t.getMilliseconds()), new Date(r,
            0, u.getDate() + l, o, a, s, c)
        }
      }
      return NaN
    }

    function pr(e, t) {
      return function(n, i) {
        var o, a;
        if (w(n)) return n;
        if (_(n)) {
          if ('"' == n.charAt(0) && '"' == n.charAt(n.length - 1) && (n = n.substring(1, n.length - 1)), Uo.test(n))
            return new Date(n);
          if (e.lastIndex = 0, o = e.exec(n)) return o.shift(), a = i ? {
            yyyy: i.getFullYear(),
            MM: i.getMonth() + 1,
            dd: i.getDate(),
            HH: i.getHours(),
            mm: i.getMinutes(),
            ss: i.getSeconds(),
            sss: i.getMilliseconds() / 1e3
          } : {
            yyyy: 1970,
            MM: 1,
            dd: 1,
            HH: 0,
            mm: 0,
            ss: 0,
            sss: 0
          }, r(o, function(e, n) {
            n < t.length && (a[t[n]] = +e)
          }), new Date(a.yyyy, a.MM - 1, a.dd, a.HH, a.mm, a.ss || 0, 1e3 * a.sss || 0)
        }
        return NaN
      }
    }

    function mr(e, t, n, r) {
      return function(i, o, a, s, c, u, l) {
        function d(e) {
          return e && !(e.getTime && e.getTime() !== e.getTime())
        }

        function f(e) {
          return y(e) && !w(e) ? n(e) || void 0 : e
        }
        vr(i, o, a, s), fr(i, o, a, s, c, u);
        var h, p = s && s.$options && s.$options.timezone;
        if (s.$$parserName = e, s.$parsers.push(function(e) {
            if (s.$isEmpty(e)) return null;
            if (t.test(e)) {
              var r = n(e, h);
              return p && (r = X(r, p)), r
            }
          }), s.$formatters.push(function(e) {
            if (e && !w(e)) throw Ta("datefmt", "Expected `{0}` to be a date", e);
            return d(e) ? (h = e, h && p && (h = X(h, p, !0)), l("date")(e, r, p)) : (h = null, "")
          }), y(a.min) || a.ngMin) {
          var m;
          s.$validators.min = function(e) {
            return !d(e) || g(m) || n(e) >= m
          }, a.$observe("min", function(e) {
            m = f(e), s.$validate()
          })
        }
        if (y(a.max) || a.ngMax) {
          var v;
          s.$validators.max = function(e) {
            return !d(e) || g(v) || n(e) <= v
          }, a.$observe("max", function(e) {
            v = f(e), s.$validate()
          })
        }
      }
    }

    function vr(e, t, n, r) {
      var i = t[0],
        o = r.$$hasNativeValidators = b(i.validity);
      o && r.$parsers.push(function(e) {
        var n = t.prop(Ar) || {};
        return n.badInput || n.typeMismatch ? void 0 : e
      })
    }

    function gr(e, t, n, r, i, o) {
      if (vr(e, t, n, r), fr(e, t, n, r, i, o), r.$$parserName = "number", r.$parsers.push(function(e) {
          return r.$isEmpty(e) ? null : Ho.test(e) ? parseFloat(e) : void 0
        }), r.$formatters.push(function(e) {
          if (!r.$isEmpty(e)) {
            if (!$(e)) throw Ta("numfmt", "Expected `{0}` to be a number", e);
            e = e.toString()
          }
          return e
        }), y(n.min) || n.ngMin) {
        var a;
        r.$validators.min = function(e) {
          return r.$isEmpty(e) || g(a) || e >= a
        }, n.$observe("min", function(e) {
          y(e) && !$(e) && (e = parseFloat(e, 10)), a = $(e) && !isNaN(e) ? e : void 0, r.$validate()
        })
      }
      if (y(n.max) || n.ngMax) {
        var s;
        r.$validators.max = function(e) {
          return r.$isEmpty(e) || g(s) || e <= s
        }, n.$observe("max", function(e) {
          y(e) && !$(e) && (e = parseFloat(e, 10)), s = $(e) && !isNaN(e) ? e : void 0, r.$validate()
        })
      }
    }

    function yr(e, t, n, r, i, o) {
      fr(e, t, n, r, i, o), lr(r), r.$$parserName = "url", r.$validators.url = function(e, t) {
        var n = e || t;
        return r.$isEmpty(n) || Fo.test(n)
      }
    }

    function br(e, t, n, r, i, o) {
      fr(e, t, n, r, i, o), lr(r), r.$$parserName = "email", r.$validators.email = function(e, t) {
        var n = e || t;
        return r.$isEmpty(n) || jo.test(n)
      }
    }

    function Er(e, t, n, r) {
      g(n.name) && t.attr("name", a());
      var i = function(e) {
        t[0].checked && r.$setViewValue(n.value, e && e.type)
      };
      t.on("click", i), r.$render = function() {
        var e = n.value;
        t[0].checked = e == r.$viewValue
      }, n.$observe("value", r.$render)
    }

    function _r(e, t, n, r, i) {
      var o;
      if (y(r)) {
        if (o = e(r), !o.constant) throw Ta("constexpr", "Expected constant expression for `{0}`, but saw `{1}`.", n,
        r);
        return o(t)
      }
      return i
    }

    function $r(e, t, n, r, i, o, a, s) {
      var c = _r(s, e, "ngTrueValue", n.ngTrueValue, !0),
        u = _r(s, e, "ngFalseValue", n.ngFalseValue, !1),
        l = function(e) {
          r.$setViewValue(t[0].checked, e && e.type)
        };
      t.on("click", l), r.$render = function() {
        t[0].checked = r.$viewValue
      }, r.$isEmpty = function(e) {
        return e === !1
      }, r.$formatters.push(function(e) {
        return H(e, c)
      }), r.$parsers.push(function(e) {
        return e ? c : u
      })
    }

    function wr(e, t) {
      return e = "ngClass" + e, ["$animate", function(n) {
        function i(e, t) {
          var n = [];
          e: for (var r = 0; r < e.length; r++) {
            for (var i = e[r], o = 0; o < t.length; o++)
              if (i == t[o]) continue e;
            n.push(i)
          }
          return n
        }

        function o(e) {
          var t = [];
          return Vr(e) ? (r(e, function(e) {
            t = t.concat(o(e))
          }), t) : _(e) ? e.split(" ") : b(e) ? (r(e, function(e, n) {
            e && (t = t.concat(n.split(" ")))
          }), t) : e
        }
        return {
          restrict: "AC",
          link: function(a, s, c) {
            function u(e) {
              var t = d(e, 1);
              c.$addClass(t)
            }

            function l(e) {
              var t = d(e, -1);
              c.$removeClass(t)
            }

            function d(e, t) {
              var n = s.data("$classCounts") || me(),
                i = [];
              return r(e, function(e) {
                (t > 0 || n[e]) && (n[e] = (n[e] || 0) + t, n[e] === +(t > 0) && i.push(e))
              }), s.data("$classCounts", n), i.join(" ")
            }

            function f(e, t) {
              var r = i(t, e),
                o = i(e, t);
              r = d(r, 1), o = d(o, -1), r && r.length && n.addClass(s, r), o && o.length && n.removeClass(s, o)
            }

            function h(e) {
              if (t === !0 || a.$index % 2 === t) {
                var n = o(e || []);
                if (p) {
                  if (!H(e, p)) {
                    var r = o(p);
                    f(r, n)
                  }
                } else u(n)
              }
              p = Vr(e) ? e.map(function(e) {
                return j(e)
              }) : j(e)
            }
            var p;
            a.$watch(c[e], h, !0), c.$observe("class", function(t) {
              h(a.$eval(c[e]))
            }), "ngClass" !== e && a.$watch("$index", function(n, r) {
              var i = 1 & n;
              if (i !== (1 & r)) {
                var s = o(a.$eval(c[e]));
                i === t ? u(s) : l(s)
              }
            })
          }
        }
      }]
    }

    function Tr(e) {
      function t(e, t, s) {
        g(t) ? n("$pending", e, s) : r("$pending", e, s), N(t) ? t ? (l(a.$error, e, s), u(a.$$success, e, s)) : (u(a
          .$error, e, s), l(a.$$success, e, s)) : (l(a.$error, e, s), l(a.$$success, e, s)), a.$pending ? (i(_a, !0),
          a.$valid = a.$invalid = void 0, o("", null)) : (i(_a, !1), a.$valid = Cr(a.$error), a.$invalid = !a.$valid,
          o("", a.$valid));
        var c;
        c = a.$pending && a.$pending[e] ? void 0 : !a.$error[e] && (!!a.$$success[e] || null), o(e, c), a.$$parentForm
          .$setValidity(e, c, a)
      }

      function n(e, t, n) {
        a[e] || (a[e] = {}), u(a[e], t, n)
      }

      function r(e, t, n) {
        a[e] && l(a[e], t, n), Cr(a[e]) && (a[e] = void 0)
      }

      function i(e, t) {
        t && !c[e] ? (d.addClass(s, e), c[e] = !0) : !t && c[e] && (d.removeClass(s, e), c[e] = !1)
      }

      function o(e, t) {
        e = e ? "-" + ce(e, "-") : "", i(ma + e, t === !0), i(va + e, t === !1)
      }
      var a = e.ctrl,
        s = e.$element,
        c = {},
        u = e.set,
        l = e.unset,
        d = e.$animate;
      c[va] = !(c[ma] = s.hasClass(ma)), a.$setValidity = t
    }

    function Cr(e) {
      if (e)
        for (var t in e)
          if (e.hasOwnProperty(t)) return !1;
      return !0
    }

    function xr(e) {
      e[0].hasAttribute("selected") && (e[0].selected = !0)
    }
    var Sr = /^\/(.+)\/([a-z]*)$/,
      Ar = "validity",
      Mr = Object.prototype.hasOwnProperty,
      kr = function(e) {
        return _(e) ? e.toLowerCase() : e
      },
      Nr = function(e) {
        return _(e) ? e.toUpperCase() : e
      },
      Ir = function(e) {
        return _(e) ? e.replace(/[A-Z]/g, function(e) {
          return String.fromCharCode(32 | e.charCodeAt(0))
        }) : e
      },
      Or = function(e) {
        return _(e) ? e.replace(/[a-z]/g, function(e) {
          return String.fromCharCode(e.charCodeAt(0) & -33)
        }) : e
      };
    "i" !== "I".toLowerCase() && (kr = Ir, Nr = Or);
    var Dr, Rr, Pr, Lr, Ur = [].slice,
      Fr = [].splice,
      jr = [].push,
      Hr = Object.prototype.toString,
      Br = Object.getPrototypeOf,
      zr = t("ng"),
      qr = e.angular || (e.angular = {}),
      Gr = 0;
    Dr = e.document.documentMode, h.$inject = [], p.$inject = [];
    var Vr = Array.isArray,
      Wr = /^\[object (?:Uint8|Uint8Clamped|Uint16|Uint32|Int8|Int16|Int32|Float32|Float64)Array\]$/,
      Yr = function(e) {
        return _(e) ? e.trim() : e
      },
      Kr = function(e) {
        return e.replace(/([-()\[\]{}+?*.$\^|,:#<!\\])/g, "\\$1").replace(/\x08/g, "\\x08")
      },
      Xr = function() {
        function t() {
          try {
            return new Function(""), !1
          } catch (e) {
            return !0
          }
        }
        if (!y(Xr.rules)) {
          var n = e.document.querySelector("[ng-csp]") || e.document.querySelector("[data-ng-csp]");
          if (n) {
            var r = n.getAttribute("ng-csp") || n.getAttribute("data-ng-csp");
            Xr.rules = {
              noUnsafeEval: !r || r.indexOf("no-unsafe-eval") !== -1,
              noInlineStyle: !r || r.indexOf("no-inline-style") !== -1
            }
          } else Xr.rules = {
            noUnsafeEval: t(),
            noInlineStyle: !1
          }
        }
        return Xr.rules
      },
      Qr = function() {
        if (y(Qr.name_)) return Qr.name_;
        var t, n, r, i, o = Zr.length;
        for (n = 0; n < o; ++n)
          if (r = Zr[n], t = e.document.querySelector("[" + r.replace(":", "\\:") + "jq]")) {
            i = t.getAttribute(r + "jq");
            break
          } return Qr.name_ = i
      },
      Jr = /:/g,
      Zr = ["ng-", "data-ng-", "ng:", "x-ng-"],
      ei = /[A-Z]/g,
      ti = !1,
      ni = 1,
      ri = 2,
      ii = 3,
      oi = 8,
      ai = 9,
      si = 11,
      ci = {
        full: "1.5.5",
        major: 1,
        minor: 5,
        dot: 5,
        codeName: "material-conspiration"
      };
    Me.expando = "ng339";
    var ui = Me.cache = {},
      li = 1,
      di = function(e, t, n) {
        e.addEventListener(t, n, !1)
      },
      fi = function(e, t, n) {
        e.removeEventListener(t, n, !1)
      };
    Me._data = function(e) {
      return this.cache[e[this.expando]] || {}
    };
    var hi = /([\:\-\_]+(.))/g,
      pi = /^moz([A-Z])/,
      mi = {
        mouseleave: "mouseout",
        mouseenter: "mouseover"
      },
      vi = t("jqLite"),
      gi = /^<([\w-]+)\s*\/?>(?:<\/\1>|)$/,
      yi = /<|&#?\w+;/,
      bi = /<([\w:-]+)/,
      Ei = /<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\w:-]+)[^>]*)\/>/gi,
      _i = {
        option: [1, '<select multiple="multiple">', "</select>"],
        thead: [1, "<table>", "</table>"],
        col: [2, "<table><colgroup>", "</colgroup></table>"],
        tr: [2, "<table><tbody>", "</tbody></table>"],
        td: [3, "<table><tbody><tr>", "</tr></tbody></table>"],
        _default: [0, "", ""]
      };
    _i.optgroup = _i.option, _i.tbody = _i.tfoot = _i.colgroup = _i.caption = _i.thead, _i.th = _i.td;
    var $i = e.Node.prototype.contains || function(e) {
        return !!(16 & this.compareDocumentPosition(e))
      },
      wi = Me.prototype = {
        ready: function(t) {
          function n() {
            r || (r = !0, t())
          }
          var r = !1;
          "complete" === e.document.readyState ? e.setTimeout(n) : (this.on("DOMContentLoaded", n), Me(e).on("load",
            n))
        },
        toString: function() {
          var e = [];
          return r(this, function(t) {
            e.push("" + t)
          }), "[" + e.join(", ") + "]"
        },
        eq: function(e) {
          return Rr(e >= 0 ? this[e] : this[this.length + e])
        },
        length: 0,
        push: jr,
        sort: [].sort,
        splice: [].splice
      },
      Ti = {};
    r("multiple,selected,checked,disabled,readOnly,required,open".split(","), function(e) {
      Ti[kr(e)] = e
    });
    var Ci = {};
    r("input,select,option,textarea,button,form,details".split(","), function(e) {
      Ci[e] = !0
    });
    var xi = {
      ngMinlength: "minlength",
      ngMaxlength: "maxlength",
      ngMin: "min",
      ngMax: "max",
      ngPattern: "pattern"
    };
    r({
      data: Re,
      removeData: Oe,
      hasData: Te,
      cleanData: Ce
    }, function(e, t) {
      Me[t] = e
    }), r({
      data: Re,
      inheritedData: He,
      scope: function(e) {
        return Rr.data(e, "$scope") || He(e.parentNode || e, ["$isolateScope", "$scope"])
      },
      isolateScope: function(e) {
        return Rr.data(e, "$isolateScope") || Rr.data(e, "$isolateScopeNoTemplate")
      },
      controller: je,
      injector: function(e) {
        return He(e, "$injector")
      },
      removeAttr: function(e, t) {
        e.removeAttribute(t)
      },
      hasClass: Pe,
      css: function(e, t, n) {
        return t = _e(t), y(n) ? void(e.style[t] = n) : e.style[t]
      },
      attr: function(e, t, n) {
        var r = e.nodeType;
        if (r !== ii && r !== ri && r !== oi) {
          var i = kr(t);
          if (Ti[i]) {
            if (!y(n)) return e[t] || (e.attributes.getNamedItem(t) || h).specified ? i : void 0;
            n ? (e[t] = !0, e.setAttribute(t, i)) : (e[t] = !1, e.removeAttribute(i))
          } else if (y(n)) e.setAttribute(t, n);
          else if (e.getAttribute) {
            var o = e.getAttribute(t, 2);
            return null === o ? void 0 : o
          }
        }
      },
      prop: function(e, t, n) {
        return y(n) ? void(e[t] = n) : e[t]
      },
      text: function() {
        function e(e, t) {
          if (g(t)) {
            var n = e.nodeType;
            return n === ni || n === ii ? e.textContent : ""
          }
          e.textContent = t
        }
        return e.$dv = "", e
      }(),
      val: function(e, t) {
        if (g(t)) {
          if (e.multiple && "select" === L(e)) {
            var n = [];
            return r(e.options, function(e) {
              e.selected && n.push(e.value || e.text)
            }), 0 === n.length ? null : n
          }
          return e.value
        }
        e.value = t
      },
      html: function(e, t) {
        return g(t) ? e.innerHTML : (Ne(e, !0), void(e.innerHTML = t))
      },
      empty: Be
    }, function(e, t) {
      Me.prototype[t] = function(t, n) {
        var r, i, o = this.length;
        if (e !== Be && g(2 == e.length && e !== Pe && e !== je ? t : n)) {
          if (b(t)) {
            for (r = 0; r < o; r++)
              if (e === Re) e(this[r], t);
              else
                for (i in t) e(this[r], i, t[i]);
            return this
          }
          for (var a = e.$dv, s = g(a) ? Math.min(o, 1) : o, c = 0; c < s; c++) {
            var u = e(this[c], t, n);
            a = a ? a + u : u
          }
          return a
        }
        for (r = 0; r < o; r++) e(this[r], t, n);
        return this
      }
    }), r({
      removeData: Oe,
      on: function(e, t, n, r) {
        if (y(r)) throw vi("onargs", "jqLite#on() does not support the `selector` or `eventData` parameters");
        if (we(e)) {
          var i = De(e, !0),
            o = i.events,
            a = i.handle;
          a || (a = i.handle = We(e, o));
          for (var s = t.indexOf(" ") >= 0 ? t.split(" ") : [t], c = s.length, u = function(t, r, i) {
              var s = o[t];
              s || (s = o[t] = [], s.specialHandlerWrapper = r, "$destroy" === t || i || di(e, t, a)), s.push(n)
            }; c--;) t = s[c], mi[t] ? (u(mi[t], Ke), u(t, void 0, !0)) : u(t)
        }
      },
      off: Ie,
      one: function(e, t, n) {
        e = Rr(e), e.on(t, function r() {
          e.off(t, n), e.off(t, r)
        }), e.on(t, n)
      },
      replaceWith: function(e, t) {
        var n, i = e.parentNode;
        Ne(e), r(new Me(t), function(t) {
          n ? i.insertBefore(t, n.nextSibling) : i.replaceChild(t, e), n = t
        })
      },
      children: function(e) {
        var t = [];
        return r(e.childNodes, function(e) {
          e.nodeType === ni && t.push(e)
        }), t
      },
      contents: function(e) {
        return e.contentDocument || e.childNodes || []
      },
      append: function(e, t) {
        var n = e.nodeType;
        if (n === ni || n === si) {
          t = new Me(t);
          for (var r = 0, i = t.length; r < i; r++) {
            var o = t[r];
            e.appendChild(o)
          }
        }
      },
      prepend: function(e, t) {
        if (e.nodeType === ni) {
          var n = e.firstChild;
          r(new Me(t), function(t) {
            e.insertBefore(t, n)
          })
        }
      },
      wrap: function(e, t) {
        Ae(e, Rr(t).eq(0).clone()[0])
      },
      remove: ze,
      detach: function(e) {
        ze(e, !0)
      },
      after: function(e, t) {
        var n = e,
          r = e.parentNode;
        t = new Me(t);
        for (var i = 0, o = t.length; i < o; i++) {
          var a = t[i];
          r.insertBefore(a, n.nextSibling), n = a
        }
      },
      addClass: Ue,
      removeClass: Le,
      toggleClass: function(e, t, n) {
        t && r(t.split(" "), function(t) {
          var r = n;
          g(r) && (r = !Pe(e, t)), (r ? Ue : Le)(e, t)
        })
      },
      parent: function(e) {
        var t = e.parentNode;
        return t && t.nodeType !== si ? t : null
      },
      next: function(e) {
        return e.nextElementSibling
      },
      find: function(e, t) {
        return e.getElementsByTagName ? e.getElementsByTagName(t) : []
      },
      clone: ke,
      triggerHandler: function(e, t, n) {
        var i, o, a, s = t.type || t,
          c = De(e),
          l = c && c.events,
          d = l && l[s];
        d && (i = {
          preventDefault: function() {
            this.defaultPrevented = !0
          },
          isDefaultPrevented: function() {
            return this.defaultPrevented === !0
          },
          stopImmediatePropagation: function() {
            this.immediatePropagationStopped = !0
          },
          isImmediatePropagationStopped: function() {
            return this.immediatePropagationStopped === !0
          },
          stopPropagation: h,
          type: s,
          target: e
        }, t.type && (i = u(i, t)), o = j(d), a = n ? [i].concat(n) : [i], r(o, function(t) {
          i.isImmediatePropagationStopped() || t.apply(e, a)
        }))
      }
    }, function(e, t) {
      Me.prototype[t] = function(t, n, r) {
        for (var i, o = 0, a = this.length; o < a; o++) g(i) ? (i = e(this[o], t, n, r), y(i) && (i = Rr(i))) :
          Fe(i, e(this[o], t, n, r));
        return y(i) ? i : this
      }, Me.prototype.bind = Me.prototype.on, Me.prototype.unbind = Me.prototype.off
    }), Je.prototype = {
      put: function(e, t) {
        this[Qe(e, this.nextUid)] = t
      },
      get: function(e) {
        return this[Qe(e, this.nextUid)]
      },
      remove: function(e) {
        var t = this[e = Qe(e, this.nextUid)];
        return delete this[e], t
      }
    };
    var Si = [function() {
        this.$get = [function() {
          return Je
        }]
      }],
      Ai = /^([^\(]+?)=>/,
      Mi = /^[^\(]*\(\s*([^\)]*)\)/m,
      ki = /,/,
      Ni = /^\s*(_?)(\S+?)\1\s*$/,
      Ii = /((\/\/.*$)|(\/\*[\s\S]*?\*\/))/gm,
      Oi = t("$injector");
    nt.$$annotate = tt;
    var Di = t("$animate"),
      Ri = 1,
      Pi = "ng-animate",
      Li = function() {
        this.$get = h
      },
      Ui = function() {
        var e = new Je,
          t = [];
        this.$get = ["$$AnimateRunner", "$rootScope", function(n, i) {
          function o(e, t, n) {
            var i = !1;
            return t && (t = _(t) ? t.split(" ") : Vr(t) ? t : [], r(t, function(t) {
              t && (i = !0, e[t] = n)
            })), i
          }

          function a() {
            r(t, function(t) {
              var n = e.get(t);
              if (n) {
                var i = at(t.attr("class")),
                  o = "",
                  a = "";
                r(n, function(e, t) {
                  var n = !!i[t];
                  e !== n && (e ? o += (o.length ? " " : "") + t : a += (a.length ? " " : "") + t)
                }), r(t, function(e) {
                  o && Ue(e, o), a && Le(e, a)
                }), e.remove(t)
              }
            }), t.length = 0
          }

          function s(n, r, s) {
            var c = e.get(n) || {},
              u = o(c, r, !0),
              l = o(c, s, !1);
            (u || l) && (e.put(n, c), t.push(n), 1 === t.length && i.$$postDigest(a))
          }
          return {
            enabled: h,
            on: h,
            off: h,
            pin: h,
            push: function(e, t, r, i) {
              i && i(), r = r || {}, r.from && e.css(r.from), r.to && e.css(r.to), (r.addClass || r
                .removeClass) && s(e, r.addClass, r.removeClass);
              var o = new n;
              return o.complete(), o
            }
          }
        }]
      },
      Fi = ["$provide", function(e) {
        var t = this;
        this.$$registeredAnimations = Object.create(null), this.register = function(n, r) {
          if (n && "." !== n.charAt(0)) throw Di("notcsel", "Expecting class selector starting with '.' got '{0}'.",
            n);
          var i = n + "-animation";
          t.$$registeredAnimations[n.substr(1)] = i, e.factory(i, r)
        }, this.classNameFilter = function(e) {
          if (1 === arguments.length && (this.$$classNameFilter = e instanceof RegExp ? e : null, this
              .$$classNameFilter)) {
            var t = new RegExp("(\\s+|\\/)" + Pi + "(\\s+|\\/)");
            if (t.test(this.$$classNameFilter.toString())) throw Di("nongcls",
              '$animateProvider.classNameFilter(regex) prohibits accepting a regex value which matches/contains the "{0}" CSS class.',
              Pi)
          }
          return this.$$classNameFilter
        }, this.$get = ["$$animateQueue", function(e) {
          function t(e, t, n) {
            if (n) {
              var r = ot(n);
              !r || r.parentNode || r.previousElementSibling || (n = null)
            }
            n ? n.after(e) : t.prepend(e)
          }
          return {
            on: e.on,
            off: e.off,
            pin: e.pin,
            enabled: e.enabled,
            cancel: function(e) {
              e.end && e.end()
            },
            enter: function(n, r, i, o) {
              return r = r && Rr(r), i = i && Rr(i), r = r || i.parent(), t(n, r, i), e.push(n, "enter", st(o))
            },
            move: function(n, r, i, o) {
              return r = r && Rr(r), i = i && Rr(i), r = r || i.parent(), t(n, r, i), e.push(n, "move", st(o))
            },
            leave: function(t, n) {
              return e.push(t, "leave", st(n), function() {
                t.remove()
              })
            },
            addClass: function(t, n, r) {
              return r = st(r), r.addClass = it(r.addclass, n), e.push(t, "addClass", r)
            },
            removeClass: function(t, n, r) {
              return r = st(r), r.removeClass = it(r.removeClass, n), e.push(t, "removeClass", r)
            },
            setClass: function(t, n, r, i) {
              return i = st(i), i.addClass = it(i.addClass, n), i.removeClass = it(i.removeClass, r), e.push(t,
                "setClass", i)
            },
            animate: function(t, n, r, i, o) {
              return o = st(o), o.from = o.from ? u(o.from, n) : n, o.to = o.to ? u(o.to, r) : r, i = i ||
                "ng-inline-animate", o.tempClasses = it(o.tempClasses, i), e.push(t, "animate", o)
            }
          }
        }]
      }],
      ji = function() {
        this.$get = ["$$rAF", function(e) {
          function t(t) {
            n.push(t), n.length > 1 || e(function() {
              for (var e = 0; e < n.length; e++) n[e]();
              n = []
            })
          }
          var n = [];
          return function() {
            var e = !1;
            return t(function() {
                e = !0
              }),
              function(n) {
                e ? n() : t(n)
              }
          }
        }]
      },
      Hi = function() {
        this.$get = ["$q", "$sniffer", "$$animateAsyncRun", "$document", "$timeout", function(e, t, n, i, o) {
          function a(e) {
            this.setHost(e);
            var t = n(),
              r = function(e) {
                o(e, 0, !1)
              };
            this._doneCallbacks = [], this._tick = function(e) {
              var n = i[0];
              n && n.hidden ? r(e) : t(e)
            }, this._state = 0
          }
          var s = 0,
            c = 1,
            u = 2;
          return a.chain = function(e, t) {
            function n() {
              return r === e.length ? void t(!0) : void e[r](function(e) {
                return e === !1 ? void t(!1) : (r++, void n())
              })
            }
            var r = 0;
            n()
          }, a.all = function(e, t) {
            function n(n) {
              o = o && n, ++i === e.length && t(o)
            }
            var i = 0,
              o = !0;
            r(e, function(e) {
              e.done(n)
            })
          }, a.prototype = {
            setHost: function(e) {
              this.host = e || {}
            },
            done: function(e) {
              this._state === u ? e() : this._doneCallbacks.push(e)
            },
            progress: h,
            getPromise: function() {
              if (!this.promise) {
                var t = this;
                this.promise = e(function(e, n) {
                  t.done(function(t) {
                    t === !1 ? n() : e()
                  })
                })
              }
              return this.promise
            },
            then: function(e, t) {
              return this.getPromise().then(e, t)
            },
            catch: function(e) {
              return this.getPromise().catch(e)
            },
            finally: function(e) {
              return this.getPromise().finally(e)
            },
            pause: function() {
              this.host.pause && this.host.pause()
            },
            resume: function() {
              this.host.resume && this.host.resume()
            },
            end: function() {
              this.host.end && this.host.end(), this._resolve(!0)
            },
            cancel: function() {
              this.host.cancel && this.host.cancel(), this._resolve(!1)
            },
            complete: function(e) {
              var t = this;
              t._state === s && (t._state = c, t._tick(function() {
                t._resolve(e)
              }))
            },
            _resolve: function(e) {
              this._state !== u && (r(this._doneCallbacks, function(t) {
                t(e)
              }), this._doneCallbacks.length = 0, this._state = u)
            }
          }, a
        }]
      },
      Bi = function() {
        this.$get = ["$$rAF", "$q", "$$AnimateRunner", function(e, t, n) {
          return function(t, r) {
            function i() {
              return e(function() {
                o(), s || c.complete(), s = !0
              }), c
            }

            function o() {
              a.addClass && (t.addClass(a.addClass), a.addClass = null), a.removeClass && (t.removeClass(a
                .removeClass), a.removeClass = null), a.to && (t.css(a.to), a.to = null)
            }
            var a = r || {};
            a.$$prepared || (a = F(a)), a.cleanupStyles && (a.from = a.to = null), a.from && (t.css(a.from), a
              .from = null);
            var s, c = new n;
            return {
              start: i,
              end: i
            }
          }
        }]
      },
      zi = t("$compile"),
      qi = new ft;
    ht.$inject = ["$provide", "$$sanitizeUriProvider"], pt.prototype.isFirstChange = function() {
      return this.previousValue === qi
    };
    var Gi = /^((?:x|data)[\:\-_])/i,
      Vi = t("$controller"),
      Wi = /^(\S+)(\s+as\s+([\w$]+))?$/,
      Yi = function() {
        this.$get = ["$document", function(e) {
          return function(t) {
            return t ? !t.nodeType && t instanceof Rr && (t = t[0]) : t = e[0].body, t.offsetWidth + 1
          }
        }]
      },
      Ki = "application/json",
      Xi = {
        "Content-Type": Ki + ";charset=utf-8"
      },
      Qi = /^\[|^\{(?!\{)/,
      Ji = {
        "[": /]$/,
        "{": /}$/
      },
      Zi = /^\)\]\}',?\n/,
      eo = t("$http"),
      to = function(e) {
        return function() {
          throw eo("legacy", "The method `{0}` on the promise returned from `$http` has been disabled.", e)
        }
      },
      no = qr.$interpolateMinErr = t("$interpolate");
    no.throwNoconcat = function(e) {
      throw no("noconcat",
        "Error while interpolating: {0}\nStrict Contextual Escaping disallows interpolations that concatenate multiple expressions when a trusted value is required.  See http://docs.angularjs.org/api/ng.$sce",
        e)
    }, no.interr = function(e, t) {
      return no("interr", "Can't interpolate: {0}\n{1}", e, t.toString())
    };
    var ro = /^([^\?#]*)(\?([^#]*))?(#(.*))?$/,
      io = {
        http: 80,
        https: 443,
        ftp: 21
      },
      oo = t("$location"),
      ao = {
        $$html5: !1,
        $$replace: !1,
        absUrl: Yt("$$absUrl"),
        url: function(e) {
          if (g(e)) return this.$$url;
          var t = ro.exec(e);
          return (t[1] || "" === e) && this.path(decodeURIComponent(t[1])), (t[2] || t[1] || "" === e) && this.search(
            t[3] || ""), this.hash(t[5] || ""), this
        },
        protocol: Yt("$$protocol"),
        host: Yt("$$host"),
        port: Yt("$$port"),
        path: Kt("$$path", function(e) {
          return e = null !== e ? e.toString() : "", "/" == e.charAt(0) ? e : "/" + e
        }),
        search: function(e, t) {
          switch (arguments.length) {
            case 0:
              return this.$$search;
            case 1:
              if (_(e) || $(e)) e = e.toString(), this.$$search = Z(e);
              else {
                if (!b(e)) throw oo("isrcharg",
                  "The first argument of the `$location#search()` call must be a string or an object.");
                e = F(e, {}), r(e, function(t, n) {
                  null == t && delete e[n]
                }), this.$$search = e
              }
              break;
            default:
              g(t) || null === t ? delete this.$$search[e] : this.$$search[e] = t
          }
          return this.$$compose(), this
        },
        hash: Kt("$$hash", function(e) {
          return null !== e ? e.toString() : ""
        }),
        replace: function() {
          return this.$$replace = !0, this
        }
      };
    r([Wt, Vt, Gt], function(e) {
      e.prototype = Object.create(ao), e.prototype.state = function(t) {
        if (!arguments.length) return this.$$state;
        if (e !== Gt || !this.$$html5) throw oo("nostate",
          "History API state support is available only in HTML5 mode and only in browsers supporting HTML5 History API"
          );
        return this.$$state = g(t) ? null : t, this
      }
    });
    var so = t("$parse"),
      co = Function.prototype.call,
      uo = Function.prototype.apply,
      lo = Function.prototype.bind,
      fo = me();
    r("+ - * / % === !== == != < > <= >= && || ! = |".split(" "), function(e) {
      fo[e] = !0
    });
    var ho = {
        n: "\n",
        f: "\f",
        r: "\r",
        t: "\t",
        v: "\v",
        "'": "'",
        '"': '"'
      },
      po = function(e) {
        this.options = e
      };
    po.prototype = {
      constructor: po,
      lex: function(e) {
        for (this.text = e, this.index = 0, this.tokens = []; this.index < this.text.length;) {
          var t = this.text.charAt(this.index);
          if ('"' === t || "'" === t) this.readString(t);
          else if (this.isNumber(t) || "." === t && this.isNumber(this.peek())) this.readNumber();
          else if (this.isIdentifierStart(this.peekMultichar())) this.readIdent();
          else if (this.is(t, "(){}[].,;:?")) this.tokens.push({
            index: this.index,
            text: t
          }), this.index++;
          else if (this.isWhitespace(t)) this.index++;
          else {
            var n = t + this.peek(),
              r = n + this.peek(2),
              i = fo[t],
              o = fo[n],
              a = fo[r];
            if (i || o || a) {
              var s = a ? r : o ? n : t;
              this.tokens.push({
                index: this.index,
                text: s,
                operator: !0
              }), this.index += s.length
            } else this.throwError("Unexpected next character ", this.index, this.index + 1)
          }
        }
        return this.tokens
      },
      is: function(e, t) {
        return t.indexOf(e) !== -1
      },
      peek: function(e) {
        var t = e || 1;
        return this.index + t < this.text.length && this.text.charAt(this.index + t)
      },
      isNumber: function(e) {
        return "0" <= e && e <= "9" && "string" == typeof e
      },
      isWhitespace: function(e) {
        return " " === e || "\r" === e || "\t" === e || "\n" === e || "\v" === e || " " === e
      },
      isIdentifierStart: function(e) {
        return this.options.isIdentifierStart ? this.options.isIdentifierStart(e, this.codePointAt(e)) : this
          .isValidIdentifierStart(e)
      },
      isValidIdentifierStart: function(e) {
        return "a" <= e && e <= "z" || "A" <= e && e <= "Z" || "_" === e || "$" === e
      },
      isIdentifierContinue: function(e) {
        return this.options.isIdentifierContinue ? this.options.isIdentifierContinue(e, this.codePointAt(e)) : this
          .isValidIdentifierContinue(e)
      },
      isValidIdentifierContinue: function(e, t) {
        return this.isValidIdentifierStart(e, t) || this.isNumber(e)
      },
      codePointAt: function(e) {
        return 1 === e.length ? e.charCodeAt(0) : (e.charCodeAt(0) << 10) + e.charCodeAt(1) - 56613888
      },
      peekMultichar: function() {
        var e = this.text.charAt(this.index),
          t = this.peek();
        if (!t) return e;
        var n = e.charCodeAt(0),
          r = t.charCodeAt(0);
        return n >= 55296 && n <= 56319 && r >= 56320 && r <= 57343 ? e + t : e
      },
      isExpOperator: function(e) {
        return "-" === e || "+" === e || this.isNumber(e)
      },
      throwError: function(e, t, n) {
        n = n || this.index;
        var r = y(t) ? "s " + t + "-" + this.index + " [" + this.text.substring(t, n) + "]" : " " + n;
        throw so("lexerr", "Lexer Error: {0} at column{1} in expression [{2}].", e, r, this.text)
      },
      readNumber: function() {
        for (var e = "", t = this.index; this.index < this.text.length;) {
          var n = kr(this.text.charAt(this.index));
          if ("." == n || this.isNumber(n)) e += n;
          else {
            var r = this.peek();
            if ("e" == n && this.isExpOperator(r)) e += n;
            else if (this.isExpOperator(n) && r && this.isNumber(r) && "e" == e.charAt(e.length - 1)) e += n;
            else {
              if (!this.isExpOperator(n) || r && this.isNumber(r) || "e" != e.charAt(e.length - 1)) break;
              this.throwError("Invalid exponent")
            }
          }
          this.index++
        }
        this.tokens.push({
          index: t,
          text: e,
          constant: !0,
          value: Number(e)
        })
      },
      readIdent: function() {
        var e = this.index;
        for (this.index += this.peekMultichar().length; this.index < this.text.length;) {
          var t = this.peekMultichar();
          if (!this.isIdentifierContinue(t)) break;
          this.index += t.length
        }
        this.tokens.push({
          index: e,
          text: this.text.slice(e, this.index),
          identifier: !0
        })
      },
      readString: function(e) {
        var t = this.index;
        this.index++;
        for (var n = "", r = e, i = !1; this.index < this.text.length;) {
          var o = this.text.charAt(this.index);
          if (r += o, i) {
            if ("u" === o) {
              var a = this.text.substring(this.index + 1, this.index + 5);
              a.match(/[\da-f]{4}/i) || this.throwError("Invalid unicode escape [\\u" + a + "]"), this.index += 4,
                n += String.fromCharCode(parseInt(a, 16))
            } else {
              var s = ho[o];
              n += s || o
            }
            i = !1
          } else if ("\\" === o) i = !0;
          else {
            if (o === e) return this.index++, void this.tokens.push({
              index: t,
              text: r,
              constant: !0,
              value: n
            });
            n += o
          }
          this.index++
        }
        this.throwError("Unterminated quote", t)
      }
    };
    var mo = function(e, t) {
      this.lexer = e, this.options = t
    };
    mo.Program = "Program", mo.ExpressionStatement = "ExpressionStatement", mo.AssignmentExpression =
      "AssignmentExpression", mo.ConditionalExpression = "ConditionalExpression", mo.LogicalExpression =
      "LogicalExpression", mo.BinaryExpression = "BinaryExpression", mo.UnaryExpression = "UnaryExpression", mo
      .CallExpression = "CallExpression", mo.MemberExpression = "MemberExpression", mo.Identifier = "Identifier", mo
      .Literal = "Literal", mo.ArrayExpression = "ArrayExpression", mo.Property = "Property", mo.ObjectExpression =
      "ObjectExpression", mo.ThisExpression = "ThisExpression", mo.LocalsExpression = "LocalsExpression", mo
      .NGValueParameter = "NGValueParameter", mo.prototype = {
        ast: function(e) {
          this.text = e, this.tokens = this.lexer.lex(e);
          var t = this.program();
          return 0 !== this.tokens.length && this.throwError("is an unexpected token", this.tokens[0]), t
        },
        program: function() {
          for (var e = [];;)
            if (this.tokens.length > 0 && !this.peek("}", ")", ";", "]") && e.push(this.expressionStatement()), !this
              .expect(";")) return {
              type: mo.Program,
              body: e
            }
        },
        expressionStatement: function() {
          return {
            type: mo.ExpressionStatement,
            expression: this.filterChain()
          }
        },
        filterChain: function() {
          for (var e, t = this.expression(); e = this.expect("|");) t = this.filter(t);
          return t
        },
        expression: function() {
          return this.assignment()
        },
        assignment: function() {
          var e = this.ternary();
          return this.expect("=") && (e = {
            type: mo.AssignmentExpression,
            left: e,
            right: this.assignment(),
            operator: "="
          }), e
        },
        ternary: function() {
          var e, t, n = this.logicalOR();
          return this.expect("?") && (e = this.expression(), this.consume(":")) ? (t = this.expression(), {
            type: mo.ConditionalExpression,
            test: n,
            alternate: e,
            consequent: t
          }) : n
        },
        logicalOR: function() {
          for (var e = this.logicalAND(); this.expect("||");) e = {
            type: mo.LogicalExpression,
            operator: "||",
            left: e,
            right: this.logicalAND()
          };
          return e
        },
        logicalAND: function() {
          for (var e = this.equality(); this.expect("&&");) e = {
            type: mo.LogicalExpression,
            operator: "&&",
            left: e,
            right: this.equality()
          };
          return e
        },
        equality: function() {
          for (var e, t = this.relational(); e = this.expect("==", "!=", "===", "!==");) t = {
            type: mo.BinaryExpression,
            operator: e.text,
            left: t,
            right: this.relational()
          };
          return t
        },
        relational: function() {
          for (var e, t = this.additive(); e = this.expect("<", ">", "<=", ">=");) t = {
            type: mo.BinaryExpression,
            operator: e.text,
            left: t,
            right: this.additive()
          };
          return t
        },
        additive: function() {
          for (var e, t = this.multiplicative(); e = this.expect("+", "-");) t = {
            type: mo.BinaryExpression,
            operator: e.text,
            left: t,
            right: this.multiplicative()
          };
          return t
        },
        multiplicative: function() {
          for (var e, t = this.unary(); e = this.expect("*", "/", "%");) t = {
            type: mo.BinaryExpression,
            operator: e.text,
            left: t,
            right: this.unary()
          };
          return t
        },
        unary: function() {
          var e;
          return (e = this.expect("+", "-", "!")) ? {
            type: mo.UnaryExpression,
            operator: e.text,
            prefix: !0,
            argument: this.unary()
          } : this.primary()
        },
        primary: function() {
          var e;
          this.expect("(") ? (e = this.filterChain(), this.consume(")")) : this.expect("[") ? e = this
            .arrayDeclaration() : this.expect("{") ? e = this.object() : this.selfReferential.hasOwnProperty(this
              .peek().text) ? e = F(this.selfReferential[this.consume().text]) : this.options.literals.hasOwnProperty(
              this.peek().text) ? e = {
              type: mo.Literal,
              value: this.options.literals[this.consume().text]
            } : this.peek().identifier ? e = this.identifier() : this.peek().constant ? e = this.constant() : this
            .throwError("not a primary expression", this.peek());
          for (var t; t = this.expect("(", "[", ".");) "(" === t.text ? (e = {
            type: mo.CallExpression,
            callee: e,
            arguments: this.parseArguments()
          }, this.consume(")")) : "[" === t.text ? (e = {
            type: mo.MemberExpression,
            object: e,
            property: this.expression(),
            computed: !0
          }, this.consume("]")) : "." === t.text ? e = {
            type: mo.MemberExpression,
            object: e,
            property: this.identifier(),
            computed: !1
          } : this.throwError("IMPOSSIBLE");
          return e
        },
        filter: function(e) {
          for (var t = [e], n = {
              type: mo.CallExpression,
              callee: this.identifier(),
              arguments: t,
              filter: !0
            }; this.expect(":");) t.push(this.expression());
          return n
        },
        parseArguments: function() {
          var e = [];
          if (")" !== this.peekToken().text)
            do e.push(this.expression()); while (this.expect(","));
          return e
        },
        identifier: function() {
          var e = this.consume();
          return e.identifier || this.throwError("is not a valid identifier", e), {
            type: mo.Identifier,
            name: e.text
          }
        },
        constant: function() {
          return {
            type: mo.Literal,
            value: this.consume().value
          }
        },
        arrayDeclaration: function() {
          var e = [];
          if ("]" !== this.peekToken().text)
            do {
              if (this.peek("]")) break;
              e.push(this.expression())
            } while (this.expect(","));
          return this.consume("]"), {
            type: mo.ArrayExpression,
            elements: e
          }
        },
        object: function() {
          var e, t = [];
          if ("}" !== this.peekToken().text)
            do {
              if (this.peek("}")) break;
              e = {
                  type: mo.Property,
                  kind: "init"
                }, this.peek().constant ? e.key = this.constant() : this.peek().identifier ? e.key = this
              .identifier() : this.throwError("invalid key", this.peek()), this.consume(":"), e.value = this
                .expression(), t.push(e)
            } while (this.expect(","));
          return this.consume("}"), {
            type: mo.ObjectExpression,
            properties: t
          }
        },
        throwError: function(e, t) {
          throw so("syntax", "Syntax Error: Token '{0}' {1} at column {2} of the expression [{3}] starting at [{4}].",
            t.text, e, t.index + 1, this.text, this.text.substring(t.index))
        },
        consume: function(e) {
          if (0 === this.tokens.length) throw so("ueoe", "Unexpected end of expression: {0}", this.text);
          var t = this.expect(e);
          return t || this.throwError("is unexpected, expecting [" + e + "]", this.peek()), t
        },
        peekToken: function() {
          if (0 === this.tokens.length) throw so("ueoe", "Unexpected end of expression: {0}", this.text);
          return this.tokens[0]
        },
        peek: function(e, t, n, r) {
          return this.peekAhead(0, e, t, n, r)
        },
        peekAhead: function(e, t, n, r, i) {
          if (this.tokens.length > e) {
            var o = this.tokens[e],
              a = o.text;
            if (a === t || a === n || a === r || a === i || !t && !n && !r && !i) return o
          }
          return !1
        },
        expect: function(e, t, n, r) {
          var i = this.peek(e, t, n, r);
          return !!i && (this.tokens.shift(), i)
        },
        selfReferential: {
          this: {
            type: mo.ThisExpression
          },
          $locals: {
            type: mo.LocalsExpression
          }
        }
      }, hn.prototype = {
        compile: function(e, t) {
          var n = this,
            i = this.astBuilder.ast(e);
          this.state = {
            nextId: 0,
            filters: {},
            expensiveChecks: t,
            fn: {
              vars: [],
              body: [],
              own: {}
            },
            assign: {
              vars: [],
              body: [],
              own: {}
            },
            inputs: []
          }, sn(i, n.$filter);
          var o, a = "";
          if (this.stage = "assign", o = ln(i)) {
            this.state.computing = "assign";
            var s = this.nextId();
            this.recurse(o, s), this.return_(s), a = "fn.assign=" + this.generateFunction("assign", "s,v,l")
          }
          var c = cn(i.body);
          n.stage = "inputs", r(c, function(e, t) {
            var r = "fn" + t;
            n.state[r] = {
              vars: [],
              body: [],
              own: {}
            }, n.state.computing = r;
            var i = n.nextId();
            n.recurse(e, i), n.return_(i), n.state.inputs.push(r), e.watchId = t
          }), this.state.computing = "fn", this.stage = "main", this.recurse(i);
          var u = '"' + this.USE + " " + this.STRICT + '";\n' + this.filterPrefix() + "var fn=" + this
            .generateFunction("fn", "s,l,a,i") + a + this.watchFns() + "return fn;",
            l = new Function("$filter", "ensureSafeMemberName", "ensureSafeObject", "ensureSafeFunction",
              "getStringValue", "ensureSafeAssignContext", "ifDefined", "plus", "text", u)(this.$filter, Jt, en, tn,
              Zt, nn, rn, on, e);
          return this.state = this.stage = void 0, l.literal = dn(i), l.constant = fn(i), l
        },
        USE: "use",
        STRICT: "strict",
        watchFns: function() {
          var e = [],
            t = this.state.inputs,
            n = this;
          return r(t, function(t) {
            e.push("var " + t + "=" + n.generateFunction(t, "s"))
          }), t.length && e.push("fn.inputs=[" + t.join(",") + "];"), e.join("")
        },
        generateFunction: function(e, t) {
          return "function(" + t + "){" + this.varsPrefix(e) + this.body(e) + "};"
        },
        filterPrefix: function() {
          var e = [],
            t = this;
          return r(this.state.filters, function(n, r) {
            e.push(n + "=$filter(" + t.escape(r) + ")")
          }), e.length ? "var " + e.join(",") + ";" : ""
        },
        varsPrefix: function(e) {
          return this.state[e].vars.length ? "var " + this.state[e].vars.join(",") + ";" : ""
        },
        body: function(e) {
          return this.state[e].body.join("")
        },
        recurse: function(e, t, n, i, o, a) {
          var s, c, u, l, d = this;
          if (i = i || h, !a && y(e.watchId)) return t = t || this.nextId(), void this.if_("i", this.lazyAssign(t,
            this.computedMember("i", e.watchId)), this.lazyRecurse(e, t, n, i, o, !0));
          switch (e.type) {
            case mo.Program:
              r(e.body, function(t, n) {
                d.recurse(t.expression, void 0, void 0, function(e) {
                  c = e
                }), n !== e.body.length - 1 ? d.current().body.push(c, ";") : d.return_(c)
              });
              break;
            case mo.Literal:
              l = this.escape(e.value), this.assign(t, l), i(l);
              break;
            case mo.UnaryExpression:
              this.recurse(e.argument, void 0, void 0, function(e) {
                c = e
              }), l = e.operator + "(" + this.ifDefined(c, 0) + ")", this.assign(t, l), i(l);
              break;
            case mo.BinaryExpression:
              this.recurse(e.left, void 0, void 0, function(e) {
                  s = e
                }), this.recurse(e.right, void 0, void 0, function(e) {
                  c = e
                }), l = "+" === e.operator ? this.plus(s, c) : "-" === e.operator ? this.ifDefined(s, 0) + e
                .operator + this.ifDefined(c, 0) : "(" + s + ")" + e.operator + "(" + c + ")", this.assign(t, l), i(
                l);
              break;
            case mo.LogicalExpression:
              t = t || this.nextId(), d.recurse(e.left, t), d.if_("&&" === e.operator ? t : d.not(t), d.lazyRecurse(e
                .right, t)), i(t);
              break;
            case mo.ConditionalExpression:
              t = t || this.nextId(), d.recurse(e.test, t), d.if_(t, d.lazyRecurse(e.alternate, t), d.lazyRecurse(e
                .consequent, t)), i(t);
              break;
            case mo.Identifier:
              t = t || this.nextId(), n && (n.context = "inputs" === d.stage ? "s" : this.assign(this.nextId(), this
                  .getHasOwnProperty("l", e.name) + "?l:s"), n.computed = !1, n.name = e.name), Jt(e.name), d.if_(
                  "inputs" === d.stage || d.not(d.getHasOwnProperty("l", e.name)),
                  function() {
                    d.if_("inputs" === d.stage || "s", function() {
                      o && 1 !== o && d.if_(d.not(d.nonComputedMember("s", e.name)), d.lazyAssign(d
                        .nonComputedMember("s", e.name), "{}")), d.assign(t, d.nonComputedMember("s", e.name))
                    })
                  }, t && d.lazyAssign(t, d.nonComputedMember("l", e.name))), (d.state.expensiveChecks || mn(e
                .name)) && d.addEnsureSafeObject(t), i(t);
              break;
            case mo.MemberExpression:
              s = n && (n.context = this.nextId()) || this.nextId(), t = t || this.nextId(), d.recurse(e.object, s,
                void 0,
                function() {
                  d.if_(d.notNull(s), function() {
                    o && 1 !== o && d.addEnsureSafeAssignContext(s), e.computed ? (c = d.nextId(), d.recurse(e
                        .property, c), d.getStringValue(c), d.addEnsureSafeMemberName(c), o && 1 !== o && d
                      .if_(d.not(d.computedMember(s, c)), d.lazyAssign(d.computedMember(s, c), "{}")), l = d
                      .ensureSafeObject(d.computedMember(s, c)), d.assign(t, l), n && (n.computed = !0, n
                        .name = c)) : (Jt(e.property.name), o && 1 !== o && d.if_(d.not(d.nonComputedMember(s,
                        e.property.name)), d.lazyAssign(d.nonComputedMember(s, e.property.name), "{}")), l = d
                      .nonComputedMember(s, e.property.name), (d.state.expensiveChecks || mn(e.property
                      .name)) && (l = d.ensureSafeObject(l)), d.assign(t, l), n && (n.computed = !1, n.name =
                        e.property.name))
                  }, function() {
                    d.assign(t, "undefined")
                  }), i(t)
                }, !!o);
              break;
            case mo.CallExpression:
              t = t || this.nextId(), e.filter ? (c = d.filter(e.callee.name), u = [], r(e.arguments, function(e) {
                var t = d.nextId();
                d.recurse(e, t), u.push(t)
              }), l = c + "(" + u.join(",") + ")", d.assign(t, l), i(t)) : (c = d.nextId(), s = {}, u = [], d
                .recurse(e.callee, c, s, function() {
                  d.if_(d.notNull(c), function() {
                    d.addEnsureSafeFunction(c), r(e.arguments, function(e) {
                        d.recurse(e, d.nextId(), void 0, function(e) {
                          u.push(d.ensureSafeObject(e))
                        })
                      }), s.name ? (d.state.expensiveChecks || d.addEnsureSafeObject(s.context), l = d.member(
                        s.context, s.name, s.computed) + "(" + u.join(",") + ")") : l = c + "(" + u.join(
                      ",") + ")", l = d.ensureSafeObject(l), d.assign(t, l)
                  }, function() {
                    d.assign(t, "undefined")
                  }), i(t)
                }));
              break;
            case mo.AssignmentExpression:
              if (c = this.nextId(), s = {}, !un(e.left)) throw so("lval",
                "Trying to assign a value to a non l-value");
              this.recurse(e.left, void 0, s, function() {
                d.if_(d.notNull(s.context), function() {
                  d.recurse(e.right, c), d.addEnsureSafeObject(d.member(s.context, s.name, s.computed)), d
                    .addEnsureSafeAssignContext(s.context), l = d.member(s.context, s.name, s.computed) + e
                    .operator + c, d.assign(t, l), i(t || l)
                })
              }, 1);
              break;
            case mo.ArrayExpression:
              u = [], r(e.elements, function(e) {
                d.recurse(e, d.nextId(), void 0, function(e) {
                  u.push(e)
                })
              }), l = "[" + u.join(",") + "]", this.assign(t, l), i(l);
              break;
            case mo.ObjectExpression:
              u = [], r(e.properties, function(e) {
                d.recurse(e.value, d.nextId(), void 0, function(t) {
                  u.push(d.escape(e.key.type === mo.Identifier ? e.key.name : "" + e.key.value) + ":" + t)
                })
              }), l = "{" + u.join(",") + "}", this.assign(t, l), i(l);
              break;
            case mo.ThisExpression:
              this.assign(t, "s"), i("s");
              break;
            case mo.LocalsExpression:
              this.assign(t, "l"), i("l");
              break;
            case mo.NGValueParameter:
              this.assign(t, "v"), i("v")
          }
        },
        getHasOwnProperty: function(e, t) {
          var n = e + "." + t,
            r = this.current().own;
          return r.hasOwnProperty(n) || (r[n] = this.nextId(!1, e + "&&(" + this.escape(t) + " in " + e + ")")), r[n]
        },
        assign: function(e, t) {
          if (e) return this.current().body.push(e, "=", t, ";"), e
        },
        filter: function(e) {
          return this.state.filters.hasOwnProperty(e) || (this.state.filters[e] = this.nextId(!0)), this.state
            .filters[e]
        },
        ifDefined: function(e, t) {
          return "ifDefined(" + e + "," + this.escape(t) + ")"
        },
        plus: function(e, t) {
          return "plus(" + e + "," + t + ")"
        },
        return_: function(e) {
          this.current().body.push("return ", e, ";")
        },
        if_: function(e, t, n) {
          if (e === !0) t();
          else {
            var r = this.current().body;
            r.push("if(", e, "){"), t(), r.push("}"), n && (r.push("else{"), n(), r.push("}"))
          }
        },
        not: function(e) {
          return "!(" + e + ")"
        },
        notNull: function(e) {
          return e + "!=null"
        },
        nonComputedMember: function(e, t) {
          var n = /[$_a-zA-Z][$_a-zA-Z0-9]*/,
            r = /[^$_a-zA-Z0-9]/g;
          return n.test(t) ? e + "." + t : e + '["' + t.replace(r, this.stringEscapeFn) + '"]'
        },
        computedMember: function(e, t) {
          return e + "[" + t + "]"
        },
        member: function(e, t, n) {
          return n ? this.computedMember(e, t) : this.nonComputedMember(e, t)
        },
        addEnsureSafeObject: function(e) {
          this.current().body.push(this.ensureSafeObject(e), ";")
        },
        addEnsureSafeMemberName: function(e) {
          this.current().body.push(this.ensureSafeMemberName(e), ";")
        },
        addEnsureSafeFunction: function(e) {
          this.current().body.push(this.ensureSafeFunction(e), ";")
        },
        addEnsureSafeAssignContext: function(e) {
          this.current().body.push(this.ensureSafeAssignContext(e), ";")
        },
        ensureSafeObject: function(e) {
          return "ensureSafeObject(" + e + ",text)"
        },
        ensureSafeMemberName: function(e) {
          return "ensureSafeMemberName(" + e + ",text)"
        },
        ensureSafeFunction: function(e) {
          return "ensureSafeFunction(" + e + ",text)"
        },
        getStringValue: function(e) {
          this.assign(e, "getStringValue(" + e + ")")
        },
        ensureSafeAssignContext: function(e) {
          return "ensureSafeAssignContext(" + e + ",text)"
        },
        lazyRecurse: function(e, t, n, r, i, o) {
          var a = this;
          return function() {
            a.recurse(e, t, n, r, i, o)
          }
        },
        lazyAssign: function(e, t) {
          var n = this;
          return function() {
            n.assign(e, t)
          }
        },
        stringEscapeRegex: /[^ a-zA-Z0-9]/g,
        stringEscapeFn: function(e) {
          return "\\u" + ("0000" + e.charCodeAt(0).toString(16)).slice(-4)
        },
        escape: function(e) {
          if (_(e)) return "'" + e.replace(this.stringEscapeRegex, this.stringEscapeFn) + "'";
          if ($(e)) return e.toString();
          if (e === !0) return "true";
          if (e === !1) return "false";
          if (null === e) return "null";
          if ("undefined" == typeof e) return "undefined";
          throw so("esc", "IMPOSSIBLE")
        },
        nextId: function(e, t) {
          var n = "v" + this.state.nextId++;
          return e || this.current().vars.push(n + (t ? "=" + t : "")), n
        },
        current: function() {
          return this.state[this.state.computing]
        }
      }, pn.prototype = {
        compile: function(e, t) {
          var n = this,
            i = this.astBuilder.ast(e);
          this.expression = e, this.expensiveChecks = t, sn(i, n.$filter);
          var o, a;
          (o = ln(i)) && (a = this.recurse(o));
          var s, c = cn(i.body);
          c && (s = [], r(c, function(e, t) {
            var r = n.recurse(e);
            e.input = r, s.push(r), e.watchId = t
          }));
          var u = [];
          r(i.body, function(e) {
            u.push(n.recurse(e.expression))
          });
          var l = 0 === i.body.length ? h : 1 === i.body.length ? u[0] : function(e, t) {
            var n;
            return r(u, function(r) {
              n = r(e, t)
            }), n
          };
          return a && (l.assign = function(e, t, n) {
            return a(e, n, t)
          }), s && (l.inputs = s), l.literal = dn(i), l.constant = fn(i), l
        },
        recurse: function(e, t, n) {
          var i, o, a, s = this;
          if (e.input) return this.inputs(e.input, e.watchId);
          switch (e.type) {
            case mo.Literal:
              return this.value(e.value, t);
            case mo.UnaryExpression:
              return o = this.recurse(e.argument), this["unary" + e.operator](o, t);
            case mo.BinaryExpression:
              return i = this.recurse(e.left), o = this.recurse(e.right), this["binary" + e.operator](i, o, t);
            case mo.LogicalExpression:
              return i = this.recurse(e.left), o = this.recurse(e.right), this["binary" + e.operator](i, o, t);
            case mo.ConditionalExpression:
              return this["ternary?:"](this.recurse(e.test), this.recurse(e.alternate), this.recurse(e.consequent),
              t);
            case mo.Identifier:
              return Jt(e.name, s.expression), s.identifier(e.name, s.expensiveChecks || mn(e.name), t, n, s
                .expression);
            case mo.MemberExpression:
              return i = this.recurse(e.object, !1, !!n), e.computed || (Jt(e.property.name, s.expression), o = e
                .property.name), e.computed && (o = this.recurse(e.property)), e.computed ? this.computedMember(i,
                o, t, n, s.expression) : this.nonComputedMember(i, o, s.expensiveChecks, t, n, s.expression);
            case mo.CallExpression:
              return a = [], r(e.arguments, function(e) {
                  a.push(s.recurse(e))
                }), e.filter && (o = this.$filter(e.callee.name)), e.filter || (o = this.recurse(e.callee, !0)), e
                .filter ? function(e, n, r, i) {
                  for (var s = [], c = 0; c < a.length; ++c) s.push(a[c](e, n, r, i));
                  var u = o.apply(void 0, s, i);
                  return t ? {
                    context: void 0,
                    name: void 0,
                    value: u
                  } : u
                } : function(e, n, r, i) {
                  var c, u = o(e, n, r, i);
                  if (null != u.value) {
                    en(u.context, s.expression), tn(u.value, s.expression);
                    for (var l = [], d = 0; d < a.length; ++d) l.push(en(a[d](e, n, r, i), s.expression));
                    c = en(u.value.apply(u.context, l), s.expression)
                  }
                  return t ? {
                    value: c
                  } : c
                };
            case mo.AssignmentExpression:
              return i = this.recurse(e.left, !0, 1), o = this.recurse(e.right),
                function(e, n, r, a) {
                  var c = i(e, n, r, a),
                    u = o(e, n, r, a);
                  return en(c.value, s.expression), nn(c.context), c.context[c.name] = u, t ? {
                    value: u
                  } : u
                };
            case mo.ArrayExpression:
              return a = [], r(e.elements, function(e) {
                  a.push(s.recurse(e))
                }),
                function(e, n, r, i) {
                  for (var o = [], s = 0; s < a.length; ++s) o.push(a[s](e, n, r, i));
                  return t ? {
                    value: o
                  } : o
                };
            case mo.ObjectExpression:
              return a = [], r(e.properties, function(e) {
                  a.push({
                    key: e.key.type === mo.Identifier ? e.key.name : "" + e.key.value,
                    value: s.recurse(e.value)
                  })
                }),
                function(e, n, r, i) {
                  for (var o = {}, s = 0; s < a.length; ++s) o[a[s].key] = a[s].value(e, n, r, i);
                  return t ? {
                    value: o
                  } : o
                };
            case mo.ThisExpression:
              return function(e) {
                return t ? {
                  value: e
                } : e
              };
            case mo.LocalsExpression:
              return function(e, n) {
                return t ? {
                  value: n
                } : n
              };
            case mo.NGValueParameter:
              return function(e, n, r) {
                return t ? {
                  value: r
                } : r
              }
          }
        },
        "unary+": function(e, t) {
          return function(n, r, i, o) {
            var a = e(n, r, i, o);
            return a = y(a) ? +a : 0, t ? {
              value: a
            } : a
          }
        },
        "unary-": function(e, t) {
          return function(n, r, i, o) {
            var a = e(n, r, i, o);
            return a = y(a) ? -a : 0, t ? {
              value: a
            } : a
          }
        },
        "unary!": function(e, t) {
          return function(n, r, i, o) {
            var a = !e(n, r, i, o);
            return t ? {
              value: a
            } : a
          }
        },
        "binary+": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a),
              c = t(r, i, o, a),
              u = on(s, c);
            return n ? {
              value: u
            } : u
          }
        },
        "binary-": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a),
              c = t(r, i, o, a),
              u = (y(s) ? s : 0) - (y(c) ? c : 0);
            return n ? {
              value: u
            } : u
          }
        },
        "binary*": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) * t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary/": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) / t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary%": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) % t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary===": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) === t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary!==": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) !== t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary==": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) == t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary!=": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) != t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary<": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) < t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary>": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) > t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary<=": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) <= t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary>=": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) >= t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary&&": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) && t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "binary||": function(e, t, n) {
          return function(r, i, o, a) {
            var s = e(r, i, o, a) || t(r, i, o, a);
            return n ? {
              value: s
            } : s
          }
        },
        "ternary?:": function(e, t, n, r) {
          return function(i, o, a, s) {
            var c = e(i, o, a, s) ? t(i, o, a, s) : n(i, o, a, s);
            return r ? {
              value: c
            } : c
          }
        },
        value: function(e, t) {
          return function() {
            return t ? {
              context: void 0,
              name: void 0,
              value: e
            } : e
          }
        },
        identifier: function(e, t, n, r, i) {
          return function(o, a, s, c) {
            var u = a && e in a ? a : o;
            r && 1 !== r && u && !u[e] && (u[e] = {});
            var l = u ? u[e] : void 0;
            return t && en(l, i), n ? {
              context: u,
              name: e,
              value: l
            } : l
          }
        },
        computedMember: function(e, t, n, r, i) {
          return function(o, a, s, c) {
            var u, l, d = e(o, a, s, c);
            return null != d && (u = t(o, a, s, c), u = Zt(u), Jt(u, i), r && 1 !== r && (nn(d), d && !d[u] && (d[
              u] = {})), l = d[u], en(l, i)), n ? {
              context: d,
              name: u,
              value: l
            } : l
          }
        },
        nonComputedMember: function(e, t, n, r, i, o) {
          return function(a, s, c, u) {
            var l = e(a, s, c, u);
            i && 1 !== i && (nn(l), l && !l[t] && (l[t] = {}));
            var d = null != l ? l[t] : void 0;
            return (n || mn(t)) && en(d, o), r ? {
              context: l,
              name: t,
              value: d
            } : d
          }
        },
        inputs: function(e, t) {
          return function(n, r, i, o) {
            return o ? o[t] : e(n, r, i)
          }
        }
      };
    var vo = function(e, t, n) {
      this.lexer = e, this.$filter = t, this.options = n, this.ast = new mo(e, n), this.astCompiler = n.csp ? new pn(
        this.ast, t) : new hn(this.ast, t)
    };
    vo.prototype = {
      constructor: vo,
      parse: function(e) {
        return this.astCompiler.compile(e, this.options.expensiveChecks)
      }
    };
    var go = Object.prototype.valueOf,
      yo = t("$sce"),
      bo = {
        HTML: "html",
        CSS: "css",
        URL: "url",
        RESOURCE_URL: "resourceUrl",
        JS: "js"
      },
      Eo = t("$compile"),
      _o = e.document.createElement("a"),
      $o = In(e.location.href);
    Rn.$inject = ["$document"], Ln.$inject = ["$provide"];
    var wo = 22,
      To = ".",
      Co = "0";
    Bn.$inject = ["$locale"], zn.$inject = ["$locale"];
    var xo = {
        yyyy: Yn("FullYear", 4, 0, !1, !0),
        yy: Yn("FullYear", 2, 0, !0, !0),
        y: Yn("FullYear", 1, 0, !1, !0),
        MMMM: Kn("Month"),
        MMM: Kn("Month", !0),
        MM: Yn("Month", 2, 1),
        M: Yn("Month", 1, 1),
        LLLL: Kn("Month", !1, !0),
        dd: Yn("Date", 2),
        d: Yn("Date", 1),
        HH: Yn("Hours", 2),
        H: Yn("Hours", 1),
        hh: Yn("Hours", 2, -12),
        h: Yn("Hours", 1, -12),
        mm: Yn("Minutes", 2),
        m: Yn("Minutes", 1),
        ss: Yn("Seconds", 2),
        s: Yn("Seconds", 1),
        sss: Yn("Milliseconds", 3),
        EEEE: Kn("Day"),
        EEE: Kn("Day", !0),
        a: er,
        Z: Xn,
        ww: Zn(2),
        w: Zn(1),
        G: tr,
        GG: tr,
        GGG: tr,
        GGGG: nr
      },
      So = /((?:[^yMLdHhmsaZEwG']+)|(?:'(?:[^']|'')*')|(?:E+|y+|M+|L+|d+|H+|h+|m+|s+|a|Z|G+|w+))(.*)/,
      Ao = /^\-?\d+$/;
    rr.$inject = ["$locale"];
    var Mo = m(kr),
      ko = m(Nr);
    ar.$inject = ["$parse"];
    var No = m({
        restrict: "E",
        compile: function(e, t) {
          if (!t.href && !t.xlinkHref) return function(e, t) {
            if ("a" === t[0].nodeName.toLowerCase()) {
              var n = "[object SVGAnimatedString]" === Hr.call(t.prop("href")) ? "xlink:href" : "href";
              t.on("click", function(e) {
                t.attr(n) || e.preventDefault()
              })
            }
          }
        }
      }),
      Io = {};
    r(Ti, function(e, t) {
      function n(e, n, i) {
        e.$watch(i[r], function(e) {
          i.$set(t, !!e)
        })
      }
      if ("multiple" != e) {
        var r = mt("ng-" + t),
          i = n;
        "checked" === e && (i = function(e, t, i) {
          i.ngModel !== i[r] && n(e, t, i)
        }), Io[r] = function() {
          return {
            restrict: "A",
            priority: 100,
            link: i
          }
        }
      }
    }), r(xi, function(e, t) {
      Io[t] = function() {
        return {
          priority: 100,
          link: function(e, n, r) {
            if ("ngPattern" === t && "/" == r.ngPattern.charAt(0)) {
              var i = r.ngPattern.match(Sr);
              if (i) return void r.$set("ngPattern", new RegExp(i[1], i[2]))
            }
            e.$watch(r[t], function(e) {
              r.$set(t, e)
            })
          }
        }
      }
    }), r(["src", "srcset", "href"], function(e) {
      var t = mt("ng-" + e);
      Io[t] = function() {
        return {
          priority: 99,
          link: function(n, r, i) {
            var o = e,
              a = e;
            "href" === e && "[object SVGAnimatedString]" === Hr.call(r.prop("href")) && (a = "xlinkHref", i
              .$attr[a] = "xlink:href", o = null), i.$observe(t, function(t) {
              return t ? (i.$set(a, t), void(Dr && o && r.prop(o, i[a]))) : void("href" === e && i.$set(a,
                null))
            })
          }
        }
      }
    });
    var Oo = {
        $addControl: h,
        $$renameControl: cr,
        $removeControl: h,
        $setValidity: h,
        $setDirty: h,
        $setPristine: h,
        $setSubmitted: h
      },
      Do = "ng-submitted";
    ur.$inject = ["$element", "$attrs", "$scope", "$animate", "$interpolate"];
    var Ro = function(e) {
        return ["$timeout", "$parse", function(t, n) {
          function r(e) {
            return "" === e ? n('this[""]').assign : n(e).assign || h
          }
          var i = {
            name: "form",
            restrict: e ? "EAC" : "E",
            require: ["form", "^^?form"],
            controller: ur,
            compile: function(n, i) {
              n.addClass(ga).addClass(ma);
              var o = i.name ? "name" : !(!e || !i.ngForm) && "ngForm";
              return {
                pre: function(e, n, i, a) {
                  var s = a[0];
                  if (!("action" in i)) {
                    var c = function(t) {
                      e.$apply(function() {
                        s.$commitViewValue(), s.$setSubmitted()
                      }), t.preventDefault()
                    };
                    di(n[0], "submit", c), n.on("$destroy", function() {
                      t(function() {
                        fi(n[0], "submit", c)
                      }, 0, !1)
                    })
                  }
                  var l = a[1] || s.$$parentForm;
                  l.$addControl(s);
                  var d = o ? r(s.$name) : h;
                  o && (d(e, s), i.$observe(o, function(t) {
                    s.$name !== t && (d(e, void 0), s.$$parentForm.$$renameControl(s, t), (d = r(s
                      .$name))(e, s))
                  })), n.on("$destroy", function() {
                    s.$$parentForm.$removeControl(s), d(e, void 0), u(s, Oo)
                  })
                }
              }
            }
          };
          return i
        }]
      },
      Po = Ro(),
      Lo = Ro(!0),
      Uo = /^\d{4,}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d\.\d+(?:[+-][0-2]\d:[0-5]\d|Z)$/,
      Fo =
      /^[a-z][a-z\d.+-]*:\/*(?:[^:@]+(?::[^@]+)?@)?(?:[^\s:\/?#]+|\[[a-f\d:]+\])(?::\d+)?(?:\/[^?#]*)?(?:\?[^#]*)?(?:#.*)?$/i,
      jo = /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i,
      Ho = /^\s*(\-|\+)?(\d+|(\d*(\.\d*)))([eE][+-]?\d+)?\s*$/,
      Bo = /^(\d{4,})-(\d{2})-(\d{2})$/,
      zo = /^(\d{4,})-(\d\d)-(\d\d)T(\d\d):(\d\d)(?::(\d\d)(\.\d{1,3})?)?$/,
      qo = /^(\d{4,})-W(\d\d)$/,
      Go = /^(\d{4,})-(\d\d)$/,
      Vo = /^(\d\d):(\d\d)(?::(\d\d)(\.\d{1,3})?)?$/,
      Wo = "keydown wheel mousedown",
      Yo = me();
    r("date,datetime-local,month,time,week".split(","), function(e) {
      Yo[e] = !0
    });
    var Ko = {
        text: dr,
        date: mr("date", Bo, pr(Bo, ["yyyy", "MM", "dd"]), "yyyy-MM-dd"),
        "datetime-local": mr("datetimelocal", zo, pr(zo, ["yyyy", "MM", "dd", "HH", "mm", "ss", "sss"]),
          "yyyy-MM-ddTHH:mm:ss.sss"),
        time: mr("time", Vo, pr(Vo, ["HH", "mm", "ss", "sss"]), "HH:mm:ss.sss"),
        week: mr("week", qo, hr, "yyyy-Www"),
        month: mr("month", Go, pr(Go, ["yyyy", "MM"]), "yyyy-MM"),
        number: gr,
        url: yr,
        email: br,
        radio: Er,
        checkbox: $r,
        hidden: h,
        button: h,
        submit: h,
        reset: h,
        file: h
      },
      Xo = ["$browser", "$sniffer", "$filter", "$parse", function(e, t, n, r) {
        return {
          restrict: "E",
          require: ["?ngModel"],
          link: {
            pre: function(i, o, a, s) {
              s[0] && (Ko[kr(a.type)] || Ko.text)(i, o, a, s[0], t, e, n, r)
            }
          }
        }
      }],
      Qo = /^(true|false|\d+)$/,
      Jo = function() {
        return {
          restrict: "A",
          priority: 100,
          compile: function(e, t) {
            return Qo.test(t.ngValue) ? function(e, t, n) {
              n.$set("value", e.$eval(n.ngValue))
            } : function(e, t, n) {
              e.$watch(n.ngValue, function(e) {
                n.$set("value", e)
              })
            }
          }
        }
      },
      Zo = ["$compile", function(e) {
        return {
          restrict: "AC",
          compile: function(t) {
            return e.$$addBindingClass(t),
              function(t, n, r) {
                e.$$addBindingInfo(n, r.ngBind), n = n[0], t.$watch(r.ngBind, function(e) {
                  n.textContent = g(e) ? "" : e
                })
              }
          }
        }
      }],
      ea = ["$interpolate", "$compile", function(e, t) {
        return {
          compile: function(n) {
            return t.$$addBindingClass(n),
              function(n, r, i) {
                var o = e(r.attr(i.$attr.ngBindTemplate));
                t.$$addBindingInfo(r, o.expressions), r = r[0], i.$observe("ngBindTemplate", function(e) {
                  r.textContent = g(e) ? "" : e
                })
              }
          }
        }
      }],
      ta = ["$sce", "$parse", "$compile", function(e, t, n) {
        return {
          restrict: "A",
          compile: function(r, i) {
            var o = t(i.ngBindHtml),
              a = t(i.ngBindHtml, function(e) {
                return (e || "").toString()
              });
            return n.$$addBindingClass(r),
              function(t, r, i) {
                n.$$addBindingInfo(r, i.ngBindHtml), t.$watch(a, function() {
                  r.html(e.getTrustedHtml(o(t)) || "")
                })
              }
          }
        }
      }],
      na = m({
        restrict: "A",
        require: "ngModel",
        link: function(e, t, n, r) {
          r.$viewChangeListeners.push(function() {
            e.$eval(n.ngChange)
          })
        }
      }),
      ra = wr("", !0),
      ia = wr("Odd", 0),
      oa = wr("Even", 1),
      aa = sr({
        compile: function(e, t) {
          t.$set("ngCloak", void 0), e.removeClass("ng-cloak")
        }
      }),
      sa = [function() {
        return {
          restrict: "A",
          scope: !0,
          controller: "@",
          priority: 500
        }
      }],
      ca = {},
      ua = {
        blur: !0,
        focus: !0
      };
    r("click dblclick mousedown mouseup mouseover mouseout mousemove mouseenter mouseleave keydown keyup keypress submit focus blur copy cut paste"
      .split(" "),
      function(e) {
        var t = mt("ng-" + e);
        ca[t] = ["$parse", "$rootScope", function(n, r) {
          return {
            restrict: "A",
            compile: function(i, o) {
              var a = n(o[t], null, !0);
              return function(t, n) {
                n.on(e, function(n) {
                  var i = function() {
                    a(t, {
                      $event: n
                    })
                  };
                  ua[e] && r.$$phase ? t.$evalAsync(i) : t.$apply(i)
                })
              }
            }
          }
        }]
      });
    var la = ["$animate", "$compile", function(e, t) {
        return {
          multiElement: !0,
          transclude: "element",
          priority: 600,
          terminal: !0,
          restrict: "A",
          $$tlb: !0,
          link: function(n, r, i, o, a) {
            var s, c, u;
            n.$watch(i.ngIf, function(n) {
              n ? c || a(function(n, o) {
                c = o, n[n.length++] = t.$$createComment("end ngIf", i.ngIf), s = {
                  clone: n
                }, e.enter(n, r.parent(), r)
              }) : (u && (u.remove(), u = null), c && (c.$destroy(), c = null), s && (u = pe(s.clone), e
                .leave(u).then(function() {
                  u = null
                }), s = null))
            })
          }
        }
      }],
      da = ["$templateRequest", "$anchorScroll", "$animate", function(e, t, n) {
        return {
          restrict: "ECA",
          priority: 400,
          terminal: !0,
          transclude: "element",
          controller: qr.noop,
          compile: function(r, i) {
            var o = i.ngInclude || i.src,
              a = i.onload || "",
              s = i.autoscroll;
            return function(r, i, c, u, l) {
              var d, f, h, p = 0,
                m = function() {
                  f && (f.remove(), f = null), d && (d.$destroy(), d = null), h && (n.leave(h).then(function() {
                    f = null
                  }), f = h, h = null)
                };
              r.$watch(o, function(o) {
                var c = function() {
                    !y(s) || s && !r.$eval(s) || t()
                  },
                  f = ++p;
                o ? (e(o, !0).then(function(e) {
                  if (!r.$$destroyed && f === p) {
                    var t = r.$new();
                    u.template = e;
                    var s = l(t, function(e) {
                      m(), n.enter(e, null, i).then(c)
                    });
                    d = t, h = s, d.$emit("$includeContentLoaded", o), r.$eval(a)
                  }
                }, function() {
                  r.$$destroyed || f === p && (m(), r.$emit("$includeContentError", o))
                }), r.$emit("$includeContentRequested", o)) : (m(), u.template = null)
              })
            }
          }
        }
      }],
      fa = ["$compile", function(t) {
        return {
          restrict: "ECA",
          priority: -400,
          require: "ngInclude",
          link: function(n, r, i, o) {
            return Hr.call(r[0]).match(/SVG/) ? (r.empty(), void t(xe(o.template, e.document).childNodes)(n,
              function(e) {
                r.append(e)
              }, {
                futureParentElement: r
              })) : (r.html(o.template), void t(r.contents())(n))
          }
        }
      }],
      ha = sr({
        priority: 450,
        compile: function() {
          return {
            pre: function(e, t, n) {
              e.$eval(n.ngInit)
            }
          }
        }
      }),
      pa = function() {
        return {
          restrict: "A",
          priority: 100,
          require: "ngModel",
          link: function(e, t, n, i) {
            var o = t.attr(n.$attr.ngList) || ", ",
              a = "false" !== n.ngTrim,
              s = a ? Yr(o) : o,
              c = function(e) {
                if (!g(e)) {
                  var t = [];
                  return e && r(e.split(s), function(e) {
                    e && t.push(a ? Yr(e) : e)
                  }), t
                }
              };
            i.$parsers.push(c), i.$formatters.push(function(e) {
              if (Vr(e)) return e.join(o)
            }), i.$isEmpty = function(e) {
              return !e || !e.length
            }
          }
        }
      },
      ma = "ng-valid",
      va = "ng-invalid",
      ga = "ng-pristine",
      ya = "ng-dirty",
      ba = "ng-untouched",
      Ea = "ng-touched",
      _a = "ng-pending",
      $a = "ng-empty",
      wa = "ng-not-empty",
      Ta = t("ngModel"),
      Ca = ["$scope", "$exceptionHandler", "$attrs", "$element", "$parse", "$animate", "$timeout", "$rootScope", "$q",
        "$interpolate",
        function(e, t, n, i, o, a, s, c, u, l) {
          this.$viewValue = Number.NaN, this.$modelValue = Number.NaN, this.$$rawModelValue = void 0, this
            .$validators = {}, this.$asyncValidators = {}, this.$parsers = [], this.$formatters = [], this
            .$viewChangeListeners = [], this.$untouched = !0, this.$touched = !1, this.$pristine = !0, this.$dirty = !1,
            this.$valid = !0, this.$invalid = !1, this.$error = {}, this.$$success = {}, this.$pending = void 0, this
            .$name = l(n.name || "", !1)(e), this.$$parentForm = Oo;
          var d, f = o(n.ngModel),
            p = f.assign,
            m = f,
            v = p,
            b = null,
            E = this;
          this.$$setOptions = function(e) {
            if (E.$options = e, e && e.getterSetter) {
              var t = o(n.ngModel + "()"),
                r = o(n.ngModel + "($$$p)");
              m = function(e) {
                var n = f(e);
                return T(n) && (n = t(e)), n
              }, v = function(e, t) {
                T(f(e)) ? r(e, {
                  $$$p: t
                }) : p(e, t)
              }
            } else if (!f.assign) throw Ta("nonassign", "Expression '{0}' is non-assignable. Element: {1}", n.ngModel,
              Q(i))
          }, this.$render = h, this.$isEmpty = function(e) {
            return g(e) || "" === e || null === e || e !== e
          }, this.$$updateEmptyClasses = function(e) {
            E.$isEmpty(e) ? (a.removeClass(i, wa), a.addClass(i, $a)) : (a.removeClass(i, $a), a.addClass(i, wa))
          };
          var _ = 0;
          Tr({
            ctrl: this,
            $element: i,
            set: function(e, t) {
              e[t] = !0
            },
            unset: function(e, t) {
              delete e[t]
            },
            $animate: a
          }), this.$setPristine = function() {
            E.$dirty = !1, E.$pristine = !0, a.removeClass(i, ya), a.addClass(i, ga)
          }, this.$setDirty = function() {
            E.$dirty = !0, E.$pristine = !1, a.removeClass(i, ga), a.addClass(i, ya), E.$$parentForm.$setDirty()
          }, this.$setUntouched = function() {
            E.$touched = !1, E.$untouched = !0, a.setClass(i, ba, Ea)
          }, this.$setTouched = function() {
            E.$touched = !0, E.$untouched = !1, a.setClass(i, Ea, ba)
          }, this.$rollbackViewValue = function() {
            s.cancel(b), E.$viewValue = E.$$lastCommittedViewValue, E.$render()
          }, this.$validate = function() {
            if (!$(E.$modelValue) || !isNaN(E.$modelValue)) {
              var e = E.$$lastCommittedViewValue,
                t = E.$$rawModelValue,
                n = E.$valid,
                r = E.$modelValue,
                i = E.$options && E.$options.allowInvalid;
              E.$$runValidators(t, e, function(e) {
                i || n === e || (E.$modelValue = e ? t : void 0, E.$modelValue !== r && E.$$writeModelToScope())
              })
            }
          }, this.$$runValidators = function(e, t, n) {
            function i() {
              var e = E.$$parserName || "parse";
              return g(d) ? (s(e, null), !0) : (d || (r(E.$validators, function(e, t) {
                s(t, null)
              }), r(E.$asyncValidators, function(e, t) {
                s(t, null)
              })), s(e, d), d)
            }

            function o() {
              var n = !0;
              return r(E.$validators, function(r, i) {
                var o = r(e, t);
                n = n && o, s(i, o)
              }), !!n || (r(E.$asyncValidators, function(e, t) {
                s(t, null)
              }), !1)
            }

            function a() {
              var n = [],
                i = !0;
              r(E.$asyncValidators, function(r, o) {
                var a = r(e, t);
                if (!I(a)) throw Ta("nopromise",
                  "Expected asynchronous validator to return a promise but got '{0}' instead.", a);
                s(o, void 0), n.push(a.then(function() {
                  s(o, !0)
                }, function() {
                  i = !1, s(o, !1)
                }))
              }), n.length ? u.all(n).then(function() {
                c(i)
              }, h) : c(!0)
            }

            function s(e, t) {
              l === _ && E.$setValidity(e, t)
            }

            function c(e) {
              l === _ && n(e)
            }
            _++;
            var l = _;
            return i() && o() ? void a() : void c(!1)
          }, this.$commitViewValue = function() {
            var e = E.$viewValue;
            s.cancel(b), (E.$$lastCommittedViewValue !== e || "" === e && E.$$hasNativeValidators) && (E
              .$$updateEmptyClasses(e), E.$$lastCommittedViewValue = e, E.$pristine && this.$setDirty(), this
              .$$parseAndValidate())
          }, this.$$parseAndValidate = function() {
            function t() {
              E.$modelValue !== o && E.$$writeModelToScope()
            }
            var n = E.$$lastCommittedViewValue,
              r = n;
            if (d = !g(r) || void 0)
              for (var i = 0; i < E.$parsers.length; i++)
                if (r = E.$parsers[i](r), g(r)) {
                  d = !1;
                  break
                } $(E.$modelValue) && isNaN(E.$modelValue) && (E.$modelValue = m(e));
            var o = E.$modelValue,
              a = E.$options && E.$options.allowInvalid;
            E.$$rawModelValue = r, a && (E.$modelValue = r, t()), E.$$runValidators(r, E.$$lastCommittedViewValue,
              function(e) {
                a || (E.$modelValue = e ? r : void 0, t())
              })
          }, this.$$writeModelToScope = function() {
            v(e, E.$modelValue), r(E.$viewChangeListeners, function(e) {
              try {
                e()
              } catch (e) {
                t(e)
              }
            })
          }, this.$setViewValue = function(e, t) {
            E.$viewValue = e, E.$options && !E.$options.updateOnDefault || E.$$debounceViewValueCommit(t)
          }, this.$$debounceViewValueCommit = function(t) {
            var n, r = 0,
              i = E.$options;
            i && y(i.debounce) && (n = i.debounce, $(n) ? r = n : $(n[t]) ? r = n[t] : $(n.default) && (r = n
              .default)), s.cancel(b), r ? b = s(function() {
              E.$commitViewValue()
            }, r) : c.$$phase ? E.$commitViewValue() : e.$apply(function() {
              E.$commitViewValue()
            })
          }, e.$watch(function() {
            var t = m(e);
            if (t !== E.$modelValue && (E.$modelValue === E.$modelValue || t === t)) {
              E.$modelValue = E.$$rawModelValue = t, d = void 0;
              for (var n = E.$formatters, r = n.length, i = t; r--;) i = n[r](i);
              E.$viewValue !== i && (E.$$updateEmptyClasses(i), E.$viewValue = E.$$lastCommittedViewValue = i, E
                .$render(), E.$$runValidators(t, i, h))
            }
            return t
          })
        }
      ],
      xa = ["$rootScope", function(e) {
        return {
          restrict: "A",
          require: ["ngModel", "^?form", "^?ngModelOptions"],
          controller: Ca,
          priority: 1,
          compile: function(t) {
            return t.addClass(ga).addClass(ba).addClass(ma), {
              pre: function(e, t, n, r) {
                var i = r[0],
                  o = r[1] || i.$$parentForm;
                i.$$setOptions(r[2] && r[2].$options), o.$addControl(i), n.$observe("name", function(e) {
                  i.$name !== e && i.$$parentForm.$$renameControl(i, e)
                }), e.$on("$destroy", function() {
                  i.$$parentForm.$removeControl(i)
                })
              },
              post: function(t, n, r, i) {
                var o = i[0];
                o.$options && o.$options.updateOn && n.on(o.$options.updateOn, function(e) {
                  o.$$debounceViewValueCommit(e && e.type)
                }), n.on("blur", function() {
                  o.$touched || (e.$$phase ? t.$evalAsync(o.$setTouched) : t.$apply(o.$setTouched))
                })
              }
            }
          }
        }
      }],
      Sa = /(\s+|^)default(\s+|$)/,
      Aa = function() {
        return {
          restrict: "A",
          controller: ["$scope", "$attrs", function(e, t) {
            var n = this;
            this.$options = F(e.$eval(t.ngModelOptions)), y(this.$options.updateOn) ? (this.$options
              .updateOnDefault = !1, this.$options.updateOn = Yr(this.$options.updateOn.replace(Sa, function() {
                return n.$options.updateOnDefault = !0, " "
              }))) : this.$options.updateOnDefault = !0
          }]
        }
      },
      Ma = sr({
        terminal: !0,
        priority: 1e3
      }),
      ka = t("ngOptions"),
      Na =
      /^\s*([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+group\s+by\s+([\s\S]+?))?(?:\s+disable\s+when\s+([\s\S]+?))?\s+for\s+(?:([\$\w][\$\w]*)|(?:\(\s*([\$\w][\$\w]*)\s*,\s*([\$\w][\$\w]*)\s*\)))\s+in\s+([\s\S]+?)(?:\s+track\s+by\s+([\s\S]+?))?$/,
      Ia = ["$compile", "$document", "$parse", function(t, i, o) {
        function a(e, t, r) {
          function i(e, t, n, r, i) {
            this.selectValue = e, this.viewValue = t, this.label = n, this.group = r, this.disabled = i
          }

          function a(e) {
            var t;
            if (!u && n(e)) t = e;
            else {
              t = [];
              for (var r in e) e.hasOwnProperty(r) && "$" !== r.charAt(0) && t.push(r)
            }
            return t
          }
          var s = e.match(Na);
          if (!s) throw ka("iexp",
            "Expected expression in form of '_select_ (as _label_)? for (_key_,)?_value_ in _collection_' but got '{0}'. Element: {1}",
            e, Q(t));
          var c = s[5] || s[7],
            u = s[6],
            l = / as /.test(s[0]) && s[1],
            d = s[9],
            f = o(s[2] ? s[1] : c),
            h = l && o(l),
            p = h || f,
            m = d && o(d),
            v = d ? function(e, t) {
              return m(r, t)
            } : function(e) {
              return Qe(e)
            },
            g = function(e, t) {
              return v(e, w(e, t))
            },
            y = o(s[2] || s[1]),
            b = o(s[3] || ""),
            E = o(s[4] || ""),
            _ = o(s[8]),
            $ = {},
            w = u ? function(e, t) {
              return $[u] = t, $[c] = e, $
            } : function(e) {
              return $[c] = e, $
            };
          return {
            trackBy: d,
            getTrackByValue: g,
            getWatchables: o(_, function(e) {
              var t = [];
              e = e || [];
              for (var n = a(e), i = n.length, o = 0; o < i; o++) {
                var c = e === n ? o : n[o],
                  u = e[c],
                  l = w(u, c),
                  d = v(u, l);
                if (t.push(d), s[2] || s[1]) {
                  var f = y(r, l);
                  t.push(f)
                }
                if (s[4]) {
                  var h = E(r, l);
                  t.push(h)
                }
              }
              return t
            }),
            getOptions: function() {
              for (var e = [], t = {}, n = _(r) || [], o = a(n), s = o.length, c = 0; c < s; c++) {
                var u = n === o ? c : o[c],
                  l = n[u],
                  f = w(l, u),
                  h = p(r, f),
                  m = v(h, f),
                  $ = y(r, f),
                  T = b(r, f),
                  C = E(r, f),
                  x = new i(m, h, $, T, C);
                e.push(x), t[m] = x
              }
              return {
                items: e,
                selectValueMap: t,
                getOptionFromViewValue: function(e) {
                  return t[g(e)]
                },
                getViewValueFromOption: function(e) {
                  return d ? qr.copy(e.viewValue) : e.viewValue
                }
              }
            }
          }
        }

        function s(e, n, o, s) {
          function l(e, t) {
            var n = c.cloneNode(!1);
            t.appendChild(n), d(e, n)
          }

          function d(e, t) {
            e.element = t, t.disabled = e.disabled, e.label !== t.label && (t.label = e.label, t.textContent = e
              .label), e.value !== t.value && (t.value = e.selectValue)
          }

          function f() {
            var e = w && p.readValue();
            if (w)
              for (var t = w.items.length - 1; t >= 0; t--) {
                var r = w.items[t];
                ze(r.group ? r.element.parentNode : r.element)
              }
            w = T.getOptions();
            var i = {};
            if (_ && n.prepend(h), w.items.forEach(function(e) {
                var t;
                y(e.group) ? (t = i[e.group], t || (t = u.cloneNode(!1), C.appendChild(t), t.label = e.group, i[e
                  .group] = t), l(e, t)) : l(e, C)
              }), n[0].appendChild(C), m.$render(), !m.$isEmpty(e)) {
              var o = p.readValue(),
                a = T.trackBy || v;
              (a ? H(e, o) : e === o) || (m.$setViewValue(o), m.$render())
            }
          }
          for (var h, p = s[0], m = s[1], v = o.multiple, g = 0, b = n.children(), E = b.length; g < E; g++)
            if ("" === b[g].value) {
              h = b.eq(g);
              break
            } var _ = !!h,
            $ = Rr(c.cloneNode(!1));
          $.val("?");
          var w, T = a(o.ngOptions, n, e),
            C = i[0].createDocumentFragment(),
            x = function() {
              _ || n.prepend(h), n.val(""), h.prop("selected", !0), h.attr("selected", !0)
            },
            S = function() {
              _ || h.remove()
            },
            A = function() {
              n.prepend($), n.val("?"), $.prop("selected", !0), $.attr("selected", !0)
            },
            M = function() {
              $.remove()
            };
          v ? (m.$isEmpty = function(e) {
              return !e || 0 === e.length
            }, p.writeValue = function(e) {
              w.items.forEach(function(e) {
                e.element.selected = !1
              }), e && e.forEach(function(e) {
                var t = w.getOptionFromViewValue(e);
                t && (t.element.selected = !0)
              })
            }, p.readValue = function() {
              var e = n.val() || [],
                t = [];
              return r(e, function(e) {
                var n = w.selectValueMap[e];
                n && !n.disabled && t.push(w.getViewValueFromOption(n))
              }), t
            }, T.trackBy && e.$watchCollection(function() {
              if (Vr(m.$viewValue)) return m.$viewValue.map(function(e) {
                return T.getTrackByValue(e)
              })
            }, function() {
              m.$render()
            })) : (p.writeValue = function(e) {
              var t = w.getOptionFromViewValue(e);
              t ? (n[0].value !== t.selectValue && (M(), S(), n[0].value = t.selectValue, t.element.selected = !0),
                t.element.setAttribute("selected", "selected")) : null === e || _ ? (M(), x()) : (S(), A())
            }, p.readValue = function() {
              var e = w.selectValueMap[n.val()];
              return e && !e.disabled ? (S(), M(), w.getViewValueFromOption(e)) : null
            }, T.trackBy && e.$watch(function() {
              return T.getTrackByValue(m.$viewValue)
            }, function() {
              m.$render()
            })), _ ? (h.remove(), t(h)(e), h.removeClass("ng-scope")) : h = Rr(c.cloneNode(!1)), n.empty(), f(), e
            .$watchCollection(T.getWatchables, f)
        }
        var c = e.document.createElement("option"),
          u = e.document.createElement("optgroup");
        return {
          restrict: "A",
          terminal: !0,
          require: ["select", "ngModel"],
          link: {
            pre: function(e, t, n, r) {
              r[0].registerOption = h
            },
            post: s
          }
        }
      }],
      Oa = ["$locale", "$interpolate", "$log", function(e, t, n) {
        var i = /{}/g,
          o = /^when(Minus)?(.+)$/;
        return {
          link: function(a, s, c) {
            function u(e) {
              s.text(e || "")
            }
            var l, d = c.count,
              f = c.$attr.when && s.attr(c.$attr.when),
              p = c.offset || 0,
              m = a.$eval(f) || {},
              v = {},
              y = t.startSymbol(),
              b = t.endSymbol(),
              E = y + d + "-" + p + b,
              _ = qr.noop;
            r(c, function(e, t) {
              var n = o.exec(t);
              if (n) {
                var r = (n[1] ? "-" : "") + kr(n[2]);
                m[r] = s.attr(c.$attr[t])
              }
            }), r(m, function(e, n) {
              v[n] = t(e.replace(i, E))
            }), a.$watch(d, function(t) {
              var r = parseFloat(t),
                i = isNaN(r);
              if (i || r in m || (r = e.pluralCat(r - p)), r !== l && !(i && $(l) && isNaN(l))) {
                _();
                var o = v[r];
                g(o) ? (null != t && n.debug("ngPluralize: no rule defined for '" + r + "' in " + f), _ = h,
                u()) : _ = a.$watch(o, u), l = r
              }
            })
          }
        }
      }],
      Da = ["$parse", "$animate", "$compile", function(e, i, o) {
        var a = "$$NG_REMOVED",
          s = t("ngRepeat"),
          c = function(e, t, n, r, i, o, a) {
            e[n] = r, i && (e[i] = o), e.$index = t, e.$first = 0 === t, e.$last = t === a - 1, e.$middle = !(e
              .$first || e.$last), e.$odd = !(e.$even = 0 === (1 & t))
          },
          u = function(e) {
            return e.clone[0]
          },
          l = function(e) {
            return e.clone[e.clone.length - 1]
          };
        return {
          restrict: "A",
          multiElement: !0,
          transclude: "element",
          priority: 1e3,
          terminal: !0,
          $$tlb: !0,
          compile: function(t, d) {
            var f = d.ngRepeat,
              h = o.$$createComment("end ngRepeat", f),
              p = f.match(
                /^\s*([\s\S]+?)\s+in\s+([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+track\s+by\s+([\s\S]+?))?\s*$/);
            if (!p) throw s("iexp",
              "Expected expression in form of '_item_ in _collection_[ track by _id_]' but got '{0}'.", f);
            var m = p[1],
              v = p[2],
              g = p[3],
              y = p[4];
            if (p = m.match(/^(?:(\s*[\$\w]+)|\(\s*([\$\w]+)\s*,\s*([\$\w]+)\s*\))$/), !p) throw s("iidexp",
              "'_item_' in '_item_ in _collection_' should be an identifier or '(_key_, _value_)' expression, but got '{0}'.",
              m);
            var b = p[3] || p[1],
              E = p[2];
            if (g && (!/^[$a-zA-Z_][$a-zA-Z0-9_]*$/.test(g) ||
                /^(null|undefined|this|\$index|\$first|\$middle|\$last|\$even|\$odd|\$parent|\$root|\$id)$/.test(g)
                )) throw s("badident",
              "alias '{0}' is invalid --- must be a valid JS identifier which is not a reserved name.", g);
            var _, $, w, T, C = {
              $id: Qe
            };
            return y ? _ = e(y) : (w = function(e, t) {
                return Qe(t)
              }, T = function(e) {
                return e
              }),
              function(e, t, o, d, p) {
                _ && ($ = function(t, n, r) {
                  return E && (C[E] = t), C[b] = n, C.$index = r, _(e, C)
                });
                var m = me();
                e.$watchCollection(v, function(o) {
                  var d, v, y, _, C, x, S, A, M, k, N, I, O = t[0],
                    D = me();
                  if (g && (e[g] = o), n(o)) M = o, A = $ || w;
                  else {
                    A = $ || T, M = [];
                    for (var R in o) Mr.call(o, R) && "$" !== R.charAt(0) && M.push(R)
                  }
                  for (_ = M.length, N = new Array(_), d = 0; d < _; d++)
                    if (C = o === M ? d : M[d], x = o[C], S = A(C, x, d), m[S]) k = m[S], delete m[S], D[S] = k,
                      N[d] = k;
                    else {
                      if (D[S]) throw r(N, function(e) {
                        e && e.scope && (m[e.id] = e)
                      }), s("dupes",
                        "Duplicates in a repeater are not allowed. Use 'track by' expression to specify unique keys. Repeater: {0}, Duplicate key: {1}, Duplicate value: {2}",
                        f, S, x);
                      N[d] = {
                        id: S,
                        scope: void 0,
                        clone: void 0
                      }, D[S] = !0
                    } for (var P in m) {
                    if (k = m[P], I = pe(k.clone), i.leave(I), I[0].parentNode)
                      for (d = 0, v = I.length; d < v; d++) I[d][a] = !0;
                    k.scope.$destroy()
                  }
                  for (d = 0; d < _; d++)
                    if (C = o === M ? d : M[d], x = o[C], k = N[d], k.scope) {
                      y = O;
                      do y = y.nextSibling; while (y && y[a]);
                      u(k) != y && i.move(pe(k.clone), null, O), O = l(k), c(k.scope, d, b, x, E, C, _)
                    } else p(function(e, t) {
                      k.scope = t;
                      var n = h.cloneNode(!1);
                      e[e.length++] = n, i.enter(e, null, O), O = n, k.clone = e, D[k.id] = k, c(k.scope, d,
                        b, x, E, C, _)
                    });
                  m = D
                })
              }
          }
        }
      }],
      Ra = "ng-hide",
      Pa = "ng-hide-animate",
      La = ["$animate", function(e) {
        return {
          restrict: "A",
          multiElement: !0,
          link: function(t, n, r) {
            t.$watch(r.ngShow, function(t) {
              e[t ? "removeClass" : "addClass"](n, Ra, {
                tempClasses: Pa
              })
            })
          }
        }
      }],
      Ua = ["$animate", function(e) {
        return {
          restrict: "A",
          multiElement: !0,
          link: function(t, n, r) {
            t.$watch(r.ngHide, function(t) {
              e[t ? "addClass" : "removeClass"](n, Ra, {
                tempClasses: Pa
              })
            })
          }
        }
      }],
      Fa = sr(function(e, t, n) {
        e.$watch(n.ngStyle, function(e, n) {
          n && e !== n && r(n, function(e, n) {
            t.css(n, "")
          }), e && t.css(e)
        }, !0)
      }),
      ja = ["$animate", "$compile", function(e, t) {
        return {
          require: "ngSwitch",
          controller: ["$scope", function() {
            this.cases = {}
          }],
          link: function(n, i, o, a) {
            var s = o.ngSwitch || o.on,
              c = [],
              u = [],
              l = [],
              d = [],
              f = function(e, t) {
                return function() {
                  e.splice(t, 1)
                }
              };
            n.$watch(s, function(n) {
              var i, o;
              for (i = 0, o = l.length; i < o; ++i) e.cancel(l[i]);
              for (l.length = 0, i = 0, o = d.length; i < o; ++i) {
                var s = pe(u[i].clone);
                d[i].$destroy();
                var h = l[i] = e.leave(s);
                h.then(f(l, i))
              }
              u.length = 0, d.length = 0, (c = a.cases["!" + n] || a.cases["?"]) && r(c, function(n) {
                n.transclude(function(r, i) {
                  d.push(i);
                  var o = n.element;
                  r[r.length++] = t.$$createComment("end ngSwitchWhen");
                  var a = {
                    clone: r
                  };
                  u.push(a), e.enter(r, o.parent(), o);
                })
              })
            })
          }
        }
      }],
      Ha = sr({
        transclude: "element",
        priority: 1200,
        require: "^ngSwitch",
        multiElement: !0,
        link: function(e, t, n, r, i) {
          r.cases["!" + n.ngSwitchWhen] = r.cases["!" + n.ngSwitchWhen] || [], r.cases["!" + n.ngSwitchWhen].push({
            transclude: i,
            element: t
          })
        }
      }),
      Ba = sr({
        transclude: "element",
        priority: 1200,
        require: "^ngSwitch",
        multiElement: !0,
        link: function(e, t, n, r, i) {
          r.cases["?"] = r.cases["?"] || [], r.cases["?"].push({
            transclude: i,
            element: t
          })
        }
      }),
      za = t("ngTransclude"),
      qa = sr({
        restrict: "EAC",
        link: function(e, t, n, r, i) {
          function o(e) {
            e.length && (t.empty(), t.append(e))
          }
          if (n.ngTransclude === n.$attr.ngTransclude && (n.ngTransclude = ""), !i) throw za("orphan",
            "Illegal use of ngTransclude directive in the template! No parent directive that requires a transclusion found. Element: {0}",
            Q(t));
          var a = n.ngTransclude || n.ngTranscludeSlot;
          i(o, null, a)
        }
      }),
      Ga = ["$templateCache", function(e) {
        return {
          restrict: "E",
          terminal: !0,
          compile: function(t, n) {
            if ("text/ng-template" == n.type) {
              var r = n.id,
                i = t[0].text;
              e.put(r, i)
            }
          }
        }
      }],
      Va = {
        $setViewValue: h,
        $render: h
      },
      Wa = ["$element", "$scope", function(t, n) {
        var r = this,
          i = new Je;
        r.ngModelCtrl = Va, r.unknownOption = Rr(e.document.createElement("option")), r.renderUnknownOption =
          function(e) {
            var n = "? " + Qe(e) + " ?";
            r.unknownOption.val(n), t.prepend(r.unknownOption), t.val(n)
          }, n.$on("$destroy", function() {
            r.renderUnknownOption = h
          }), r.removeUnknownOption = function() {
            r.unknownOption.parent() && r.unknownOption.remove()
          }, r.readValue = function() {
            return r.removeUnknownOption(), t.val()
          }, r.writeValue = function(e) {
            r.hasOption(e) ? (r.removeUnknownOption(), t.val(e), "" === e && r.emptyOption.prop("selected", !0)) :
              null == e && r.emptyOption ? (r.removeUnknownOption(), t.val("")) : r.renderUnknownOption(e)
          }, r.addOption = function(e, t) {
            if (t[0].nodeType !== oi) {
              fe(e, '"option value"'), "" === e && (r.emptyOption = t);
              var n = i.get(e) || 0;
              i.put(e, n + 1), r.ngModelCtrl.$render(), xr(t)
            }
          }, r.removeOption = function(e) {
            var t = i.get(e);
            t && (1 === t ? (i.remove(e), "" === e && (r.emptyOption = void 0)) : i.put(e, t - 1))
          }, r.hasOption = function(e) {
            return !!i.get(e)
          }, r.registerOption = function(e, t, n, i, o) {
            if (i) {
              var a;
              n.$observe("value", function(e) {
                y(a) && r.removeOption(a), a = e, r.addOption(e, t)
              })
            } else o ? e.$watch(o, function(e, i) {
              n.$set("value", e), i !== e && r.removeOption(i), r.addOption(e, t)
            }) : r.addOption(n.value, t);
            t.on("$destroy", function() {
              r.removeOption(n.value), r.ngModelCtrl.$render()
            })
          }
      }],
      Ya = function() {
        function e(e, t, n, i) {
          var o = i[1];
          if (o) {
            var a = i[0];
            if (a.ngModelCtrl = o, t.on("change", function() {
                e.$apply(function() {
                  o.$setViewValue(a.readValue())
                })
              }), n.multiple) {
              a.readValue = function() {
                var e = [];
                return r(t.find("option"), function(t) {
                  t.selected && e.push(t.value)
                }), e
              }, a.writeValue = function(e) {
                var n = new Je(e);
                r(t.find("option"), function(e) {
                  e.selected = y(n.get(e.value))
                })
              };
              var s, c = NaN;
              e.$watch(function() {
                c !== o.$viewValue || H(s, o.$viewValue) || (s = j(o.$viewValue), o.$render()), c = o.$viewValue
              }), o.$isEmpty = function(e) {
                return !e || 0 === e.length
              }
            }
          }
        }

        function t(e, t, n, r) {
          var i = r[1];
          if (i) {
            var o = r[0];
            i.$render = function() {
              o.writeValue(i.$viewValue)
            }
          }
        }
        return {
          restrict: "E",
          require: ["select", "?ngModel"],
          controller: Wa,
          priority: 1,
          link: {
            pre: e,
            post: t
          }
        }
      },
      Ka = ["$interpolate", function(e) {
        return {
          restrict: "E",
          priority: 100,
          compile: function(t, n) {
            if (y(n.value)) var r = e(n.value, !0);
            else {
              var i = e(t.text(), !0);
              i || n.$set("value", t.text())
            }
            return function(e, t, n) {
              var o = "$selectController",
                a = t.parent(),
                s = a.data(o) || a.parent().data(o);
              s && s.registerOption(e, t, n, r, i)
            }
          }
        }
      }],
      Xa = m({
        restrict: "E",
        terminal: !1
      }),
      Qa = function() {
        return {
          restrict: "A",
          require: "?ngModel",
          link: function(e, t, n, r) {
            r && (n.required = !0, r.$validators.required = function(e, t) {
              return !n.required || !r.$isEmpty(t)
            }, n.$observe("required", function() {
              r.$validate()
            }))
          }
        }
      },
      Ja = function() {
        return {
          restrict: "A",
          require: "?ngModel",
          link: function(e, n, r, i) {
            if (i) {
              var o, a = r.ngPattern || r.pattern;
              r.$observe("pattern", function(e) {
                if (_(e) && e.length > 0 && (e = new RegExp("^" + e + "$")), e && !e.test) throw t("ngPattern")(
                  "noregexp", "Expected {0} to be a RegExp but was {1}. Element: {2}", a, e, Q(n));
                o = e || void 0, i.$validate()
              }), i.$validators.pattern = function(e, t) {
                return i.$isEmpty(t) || g(o) || o.test(t)
              }
            }
          }
        }
      },
      Za = function() {
        return {
          restrict: "A",
          require: "?ngModel",
          link: function(e, t, n, r) {
            if (r) {
              var i = -1;
              n.$observe("maxlength", function(e) {
                var t = d(e);
                i = isNaN(t) ? -1 : t, r.$validate()
              }), r.$validators.maxlength = function(e, t) {
                return i < 0 || r.$isEmpty(t) || t.length <= i
              }
            }
          }
        }
      },
      es = function() {
        return {
          restrict: "A",
          require: "?ngModel",
          link: function(e, t, n, r) {
            if (r) {
              var i = 0;
              n.$observe("minlength", function(e) {
                i = d(e) || 0, r.$validate()
              }), r.$validators.minlength = function(e, t) {
                return r.$isEmpty(t) || t.length >= i
              }
            }
          }
        }
      };
    return e.angular.bootstrap ? void(e.console && console.log("WARNING: Tried to load angular more than once.")) : (
    ue(), be(qr), qr.module("ngLocale", [], ["$provide", function(e) {
      function t(e) {
        e += "";
        var t = e.indexOf(".");
        return t == -1 ? 0 : e.length - t - 1
      }

      function n(e, n) {
        var r = n;
        void 0 === r && (r = Math.min(t(e), 3));
        var i = Math.pow(10, r),
          o = (e * i | 0) % i;
        return {
          v: r,
          f: o
        }
      }
      var r = {
        ZERO: "zero",
        ONE: "one",
        TWO: "two",
        FEW: "few",
        MANY: "many",
        OTHER: "other"
      };
      e.value("$locale", {
        DATETIME_FORMATS: {
          AMPMS: ["AM", "PM"],
          DAY: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
          ERANAMES: ["Before Christ", "Anno Domini"],
          ERAS: ["BC", "AD"],
          FIRSTDAYOFWEEK: 6,
          MONTH: ["January", "February", "March", "April", "May", "June", "July", "August", "September",
            "October", "November", "December"
          ],
          SHORTDAY: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
          SHORTMONTH: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
          STANDALONEMONTH: ["January", "February", "March", "April", "May", "June", "July", "August",
            "September", "October", "November", "December"
          ],
          WEEKENDRANGE: [5, 6],
          fullDate: "EEEE, MMMM d, y",
          longDate: "MMMM d, y",
          medium: "MMM d, y h:mm:ss a",
          mediumDate: "MMM d, y",
          mediumTime: "h:mm:ss a",
          short: "M/d/yy h:mm a",
          shortDate: "M/d/yy",
          shortTime: "h:mm a"
        },
        NUMBER_FORMATS: {
          CURRENCY_SYM: "$",
          DECIMAL_SEP: ".",
          GROUP_SEP: ",",
          PATTERNS: [{
            gSize: 3,
            lgSize: 3,
            maxFrac: 3,
            minFrac: 0,
            minInt: 1,
            negPre: "-",
            negSuf: "",
            posPre: "",
            posSuf: ""
          }, {
            gSize: 3,
            lgSize: 3,
            maxFrac: 2,
            minFrac: 2,
            minInt: 1,
            negPre: "-¤",
            negSuf: "",
            posPre: "¤",
            posSuf: ""
          }]
        },
        id: "en-us",
        localeID: "en_US",
        pluralCat: function(e, t) {
          var i = 0 | e,
            o = n(e, t);
          return 1 == i && 0 == o.v ? r.ONE : r.OTHER
        }
      })
    }]), void Rr(e.document).ready(function() {
      ie(e.document, oe)
    }))
  }(window), !window.angular.$$csp().noInlineStyle && window.angular.element(document.head).prepend(
    '<style type="text/css">@charset "UTF-8";[ng\\:cloak],[ng-cloak],[data-ng-cloak],[x-ng-cloak],.ng-cloak,.x-ng-cloak,.ng-hide:not(.ng-hide-animate){display:none !important;}ng\\:form{display:block;}.ng-animate-shim{visibility:hidden;}.ng-anchor{position:absolute;}</style>'
    )
}
