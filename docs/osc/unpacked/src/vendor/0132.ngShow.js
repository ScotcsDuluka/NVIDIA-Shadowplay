// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 132
// directive ngShow | directive ngHide | directive ngValue | directive ngChecked | directive ngReadonly | directive ngRequired | directive ngModel | directive ngDisabled | directive ngMessages | directive ngClick | directive ngDblclick | provider $aria | defines angular.module("ngAria")
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
      function e(e, t, r, i) {
        return function(a, s, c) {
          var u = c.$normalize(t);
          !n[u] || o(s, r) || c[u] || a.$watch(c[e], function(e) {
            e = i ? !e : !!e, s.attr(t, e);
          });
        };
      }
      var n = {
        ariaHidden: !0,
        ariaChecked: !0,
        ariaReadonly: !0,
        ariaDisabled: !0,
        ariaRequired: !0,
        ariaInvalid: !0,
        ariaValue: !0,
        tabindex: !0,
        bindKeypress: !0,
        bindRoleForClick: !0
      };
      this.config = function(e) {
        n = t.extend(n, e);
      }, this.$get = function() {
        return {
          config: function(e) {
            return n[e];
          },
          $$watchExpr: e
        };
      };
    }
    var r = t.module("ngAria", ["ng"]).provider("$aria", n),
      i = ["BUTTON", "A", "INPUT", "TEXTAREA", "SELECT", "DETAILS", "SUMMARY"],
      o = function(e, t) {
        if (t.indexOf(e[0].nodeName) !== -1) return !0;
      };
    r.directive("ngShow", ["$aria", function(e) {
      return e.$$watchExpr("ngShow", "aria-hidden", [], !0);
    }]).directive("ngHide", ["$aria", function(e) {
      return e.$$watchExpr("ngHide", "aria-hidden", [], !1);
    }]).directive("ngValue", ["$aria", function(e) {
      return e.$$watchExpr("ngValue", "aria-checked", i, !1);
    }]).directive("ngChecked", ["$aria", function(e) {
      return e.$$watchExpr("ngChecked", "aria-checked", i, !1);
    }]).directive("ngReadonly", ["$aria", function(e) {
      return e.$$watchExpr("ngReadonly", "aria-readonly", i, !1);
    }]).directive("ngRequired", ["$aria", function(e) {
      return e.$$watchExpr("ngRequired", "aria-required", i, !1);
    }]).directive("ngModel", ["$aria", function(e) {
      function t(t, n, r, a) {
        return e.config(n) && !r.attr(t) && (a || !o(r, i));
      }

      function n(e, t) {
        return !t.attr("role") && t.attr("type") === e && "INPUT" !== t[0].nodeName;
      }

      function r(e, t) {
        var n = e.type,
          r = e.role;
        return "checkbox" === (n || r) || "menuitemcheckbox" === r ? "checkbox" : "radio" === (n ||
          r) || "menuitemradio" === r ? "radio" : "range" === n || "progressbar" === r || "slider" ===
          r ? "range" : "";
      }
      return {
        restrict: "A",
        require: "ngModel",
        priority: 200,
        compile: function(i, o) {
          var a = r(o, i);
          return {
            pre: function(e, t, n, r) {
              "checkbox" === a && (r.$isEmpty = function(e) {
                return e === !1;
              });
            },
            post: function(r, i, o, s) {
              function c() {
                return s.$modelValue;
              }

              function u(e) {
                var t = o.value == s.$viewValue;
                i.attr("aria-checked", t);
              }

              function l() {
                i.attr("aria-checked", !s.$isEmpty(s.$viewValue));
              }
              var d = t("tabindex", "tabindex", i, !1);
              switch (a) {
                case "radio":
                case "checkbox":
                  n(a, i) && i.attr("role", a), t("aria-checked", "ariaChecked", i, !1) && r.$watch(
                    c, "radio" === a ? u : l), d && i.attr("tabindex", 0);
                  break;
                case "range":
                  if (n(a, i) && i.attr("role", "slider"), e.config("ariaValue")) {
                    var f = !i.attr("aria-valuemin") && (o.hasOwnProperty("min") || o
                        .hasOwnProperty("ngMin")),
                      h = !i.attr("aria-valuemax") && (o.hasOwnProperty("max") || o.hasOwnProperty(
                        "ngMax")),
                      p = !i.attr("aria-valuenow");
                    f && o.$observe("min", function(e) {
                      i.attr("aria-valuemin", e);
                    }), h && o.$observe("max", function(e) {
                      i.attr("aria-valuemax", e);
                    }), p && r.$watch(c, function(e) {
                      i.attr("aria-valuenow", e);
                    });
                  }
                  d && i.attr("tabindex", 0);
              }!o.hasOwnProperty("ngRequired") && s.$validators.required && t("aria-required",
                "ariaRequired", i, !1) && o.$observe("required", function() {
                i.attr("aria-required", !!o.required);
              }), t("aria-invalid", "ariaInvalid", i, !0) && r.$watch(function() {
                return s.$invalid;
              }, function(e) {
                i.attr("aria-invalid", !!e);
              });
            }
          };
        }
      };
    }]).directive("ngDisabled", ["$aria", function(e) {
      return e.$$watchExpr("ngDisabled", "aria-disabled", i, !1);
    }]).directive("ngMessages", function() {
      return {
        restrict: "A",
        require: "?ngMessages",
        link: function(e, t, n, r) {
          t.attr("aria-live") || t.attr("aria-live", "assertive");
        }
      };
    }).directive("ngClick", ["$aria", "$parse", function(e, t) {
      return {
        restrict: "A",
        compile: function(n, r) {
          var a = t(r.ngClick, null, !0);
          return function(t, n, r) {
            o(n, i) || (e.config("bindRoleForClick") && !n.attr("role") && n.attr("role", "button"),
              e.config("tabindex") && !n.attr("tabindex") && n.attr("tabindex", 0), e.config(
                "bindKeypress") && !r.ngKeypress && n.on("keypress", function(e) {
                function n() {
                  a(t, {
                    $event: e
                  });
                }
                var r = e.which || e.keyCode;
                32 !== r && 13 !== r || t.$apply(n);
              }));
          };
        }
      };
    }]).directive("ngDblclick", ["$aria", function(e) {
      return function(t, n, r) {
        !e.config("tabindex") || n.attr("tabindex") || o(n, i) || n.attr("tabindex", 0);
      };
    }]);
  }(window, window.angular);
}
