// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 137
// directive ngMessages | directive ngMessagesInclude | directive ngMessage | directive ngMessageExp | defines angular.module("ngMessages")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  /**
   * @license AngularJS v1.5.5
   * (c) 2010-2016 Google, Inc. http://angularjs.org
   * License: MIT
   */
  ! function(e, t) {
    "use strict";

    function n() {
      function e(e, t) {
        if (e) return r(e) ? e.indexOf(t) >= 0 : e.hasOwnProperty(t);
      }
      return ["$animate", function(t) {
        return {
          restrict: "AE",
          transclude: "element",
          priority: 1,
          terminal: !0,
          require: "^^ngMessages",
          link: function(n, i, o, a, s) {
            var c,
              u = i[0],
              l = o.ngMessage || o.when,
              d = o.ngMessageExp || o.whenExp,
              f = function(e) {
                c = e ? r(e) ? e : e.split(/[\s,]+/) : null, a.reRender();
              };
            d ? (f(n.$eval(d)), n.$watchCollection(d, f)) : f(l);
            var h, p;
            a.register(u, p = {
              test: function(t) {
                return e(c, t);
              },
              attach: function() {
                h || s(n, function(e) {
                  t.enter(e, null, i), h = e;
                  var n = h.$$attachId = a.getAttachId();
                  h.on("$destroy", function() {
                    h && h.$$attachId === n && (a.deregister(u), p.detach());
                  });
                });
              },
              detach: function() {
                if (h) {
                  var e = h;
                  h = null, t.leave(e);
                }
              }
            });
          }
        };
      }];
    }
    var r = t.isArray,
      i = t.forEach,
      o = t.isString,
      a = t.element;
    t.module("ngMessages", []).directive("ngMessages", ["$animate", function(e) {
      function t(e, t) {
        return o(t) && 0 === t.length || n(e.$eval(t));
      }

      function n(e) {
        return o(e) ? e.length : !!e;
      }
      var r = "ng-active",
        a = "ng-inactive";
      return {
        require: "ngMessages",
        restrict: "AE",
        controller: ["$element", "$scope", "$attrs", function(o, s, c) {
          function u(e, t) {
            for (var n = t, r = []; n && n !== e;) {
              var i = n.$$ngMessageNode;
              if (i && i.length) return g[i];
              n.childNodes.length && r.indexOf(n) == -1 ? (r.push(n), n = n.childNodes[n.childNodes
                .length - 1]) : n.previousSibling ? n = n.previousSibling : (n = n.parentNode, r
                .push(n));
            }
          }

          function l(e, t, n) {
            var r = g[n];
            if (f.head) {
              var i = u(e, t);
              i ? (r.next = i.next, i.next = r) : (r.next = f.head, f.head = r);
            } else f.head = r;
          }

          function d(e, t, n) {
            var r = g[n],
              i = u(e, t);
            i ? i.next = r.next : f.head = r.next;
          }
          var f = this,
            h = 0,
            p = 0;
          this.getAttachId = function() {
            return p++;
          };
          var m,
            v,
            g = this.messages = {};
          this.render = function(u) {
            u = u || {}, m = !1, v = u;
            for (var l = t(s, c.ngMessagesMultiple) || t(s, c.multiple), d = [], h = {}, p = f
                .head, g = !1, y = 0; null != p;) {
              y++;
              var b = p.message,
                E = !1;
              g || i(u, function(e, t) {
                if (!E && n(e) && b.test(t)) {
                  if (h[t]) return;
                  h[t] = !0, E = !0, b.attach();
                }
              }), E ? g = !l : d.push(b), p = p.next;
            }
            i(d, function(e) {
              e.detach();
            }), d.length !== y ? e.setClass(o, r, a) : e.setClass(o, a, r);
          }, s.$watchCollection(c.ngMessages || c.for, f.render), o.on("$destroy", function() {
            i(g, function(e) {
              e.message.detach();
            });
          }), this.reRender = function() {
            m || (m = !0, s.$evalAsync(function() {
              m && v && f.render(v);
            }));
          }, this.register = function(e, t) {
            var n = h.toString();
            g[n] = {
              message: t
            }, l(o[0], e, n), e.$$ngMessageNode = n, h++, f.reRender();
          }, this.deregister = function(e) {
            var t = e.$$ngMessageNode;
            delete e.$$ngMessageNode, d(o[0], e, t), delete g[t], f.reRender();
          };
        }]
      };
    }]).directive("ngMessagesInclude", ["$templateRequest", "$document", "$compile", function(e, t, n) {
      return {
        restrict: "AE",
        require: "^^ngMessages",
        link: function(r, i, o) {
          var s = o.ngMessagesInclude || o.src;
          e(s).then(function(e) {
            n(e)(r, function(e) {
              i.after(e);
              var r = n.$$createComment ? n.$$createComment("ngMessagesInclude", s) : t[0]
                .createComment(" ngMessagesInclude: " + s + " "),
                o = a(r);
              i.after(o), i.remove();
            });
          });
        }
      };
    }]).directive("ngMessage", n()).directive("ngMessageExp", n());
  }(window, window.angular);
}
