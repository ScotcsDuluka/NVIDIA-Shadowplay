// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 145
// role       : provider $translatePartialLoader
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r, i;
  /*!
   * angular-translate - v2.7.0 - 2015-05-02
   * http://github.com/angular-translate/angular-translate
   * Copyright (c) 2015 ; Licensed MIT
   */
  ! function(n, o) {
    r = [], i = function() {
      return o()
    }.apply(t, r), !(void 0 !== i && (e.exports = i))
  }(this, function() {
    function e() {
      "use strict";

      function e(e, t) {
        this.name = e, this.isActive = !0, this.tables = {}, this.priority = t || 0
      }

      function t(e) {
        return Object.prototype.hasOwnProperty.call(a, e)
      }

      function n(e) {
        return angular.isString(e) && "" !== e
      }

      function r(e) {
        if (!n(e)) throw new TypeError("Invalid type of a first argument, a non-empty string expected.");
        return t(e) && a[e].isActive
      }

      function i(e, t) {
        for (var n in t) t[n] && t[n].constructor && t[n].constructor === Object ? (e[n] = e[n] || {}, i(e[n], t[
          n])) : e[n] = t[n];
        return e
      }

      function o() {
        var e = [];
        for (var t in a) a[t].isActive && e.push(a[t]);
        return e.sort(function(e, t) {
          return e.priority - t.priority
        }), e
      }
      e.prototype.parseUrl = function(e, t) {
        return angular.isFunction(e) ? e(this.name, t) : e.replace(/\{part\}/g, this.name).replace(/\{lang\}/g, t)
      }, e.prototype.getTable = function(e, t, n, r, i, o) {
        var a = t.defer();
        if (this.tables[e]) a.resolve(this.tables[e]);
        else {
          var s = this;
          n(angular.extend({
            method: "GET",
            url: this.parseUrl(i, e)
          }, r)).success(function(t) {
            s.tables[e] = t, a.resolve(t)
          }).error(function() {
            o ? o(s.name, e).then(function(t) {
              s.tables[e] = t, a.resolve(t)
            }, function() {
              a.reject(s.name)
            }) : a.reject(s.name)
          })
        }
        return a.promise
      };
      var a = {};
      this.addPart = function(r, i) {
        if (!n(r)) throw new TypeError("Couldn't add part, part name has to be a string!");
        return t(r) || (a[r] = new e(r, i)), a[r].isActive = !0, this
      }, this.setPart = function(r, i, o) {
        if (!n(r)) throw new TypeError("Couldn't set part.`lang` parameter has to be a string!");
        if (!n(i)) throw new TypeError("Couldn't set part.`part` parameter has to be a string!");
        if ("object" != typeof o || null === o) throw new TypeError(
          "Couldn't set part. `table` parameter has to be an object!");
        return t(i) || (a[i] = new e(i), a[i].isActive = !1), a[i].tables[r] = o, this
      }, this.deletePart = function(e) {
        if (!n(e)) throw new TypeError("Couldn't delete part, first arg has to be string.");
        return t(e) && (a[e].isActive = !1), this
      }, this.isPartAvailable = r, this.$get = ["$rootScope", "$injector", "$q", "$http", function(s, c, u, l) {
        var d = function(e) {
          if (!n(e.key)) throw new TypeError("Unable to load data, a key is not a non-empty string.");
          if (!n(e.urlTemplate) && !angular.isFunction(e.urlTemplate)) throw new TypeError(
            "Unable to load data, a urlTemplate is not a non-empty string or not a function.");
          var t = e.loadFailureHandler;
          if (void 0 !== t) {
            if (!angular.isString(t)) throw new Error(
              "Unable to load data, a loadFailureHandler is not a string.");
            t = c.get(t)
          }
          var r = [],
            a = u.defer(),
            s = o();
          return angular.forEach(s, function(n) {
            r.push(n.getTable(e.key, u, l, e.$http, e.urlTemplate, t)), n.urlTemplate = e.urlTemplate
          }), u.all(r).then(function() {
            var t = {};
            angular.forEach(s, function(n) {
              i(t, n.tables[e.key])
            }), a.resolve(t)
          }, function() {
            a.reject(e.key)
          }), a.promise
        };
        return d.addPart = function(r, i) {
          if (!n(r)) throw new TypeError("Couldn't add part, first arg has to be a string");
          return t(r) ? a[r].isActive || (a[r].isActive = !0, s.$emit("$translatePartialLoaderStructureChanged",
            r)) : (a[r] = new e(r, i), s.$emit("$translatePartialLoaderStructureChanged", r)), d
        }, d.deletePart = function(e, r) {
          if (!n(e)) throw new TypeError("Couldn't delete part, first arg has to be string");
          if (void 0 === r) r = !1;
          else if ("boolean" != typeof r) throw new TypeError(
            "Invalid type of a second argument, a boolean expected.");
          if (t(e)) {
            var i = a[e].isActive;
            if (r) {
              var o = c.get("$translate"),
                u = o.loaderCache();
              "string" == typeof u && (u = c.get(u)), "object" == typeof u && angular.forEach(a[e].tables,
                function(t, n) {
                  u.remove(a[e].parseUrl(a[e].urlTemplate, n))
                }), delete a[e]
            } else a[e].isActive = !1;
            i && s.$emit("$translatePartialLoaderStructureChanged", e)
          }
          return d
        }, d.isPartLoaded = function(e, t) {
          return angular.isDefined(a[e]) && angular.isDefined(a[e].tables[t])
        }, d.getRegisteredParts = function() {
          var e = [];
          return angular.forEach(a, function(t) {
            t.isActive && e.push(t.name)
          }), e
        }, d.isPartAvailable = r, d
      }]
    }
    return angular.module("pascalprecht.translate").provider("$translatePartialLoader", e), e.displayName =
      "$translatePartialLoader", "pascalprecht.translate"
  })
}
