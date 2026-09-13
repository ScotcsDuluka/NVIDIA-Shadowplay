// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 146
// role       : factory $translateStaticFilesLoader
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r, i;
  /*!
   * angular-translate - v2.9.0 - 2016-01-24
   * 
   * Copyright (c) 2016 The angular-translate team, Pascal Precht; Licensed MIT
   */
  ! function(n, o) {
    r = [], i = function() {
      return o()
    }.apply(t, r), !(void 0 !== i && (e.exports = i))
  }(this, function() {
    function e(e, t) {
      "use strict";
      return function(n) {
        if (!(n && (angular.isArray(n.files) || angular.isString(n.prefix) && angular.isString(n.suffix))))
        throw new Error("Couldn't load static files, no files and prefix or suffix specified!");
        n.files || (n.files = [{
          prefix: n.prefix,
          suffix: n.suffix
        }]);
        for (var r = function(r) {
            if (!r || !angular.isString(r.prefix) || !angular.isString(r.suffix)) throw new Error(
              "Couldn't load static file, no prefix or suffix specified!");
            return t(angular.extend({
              url: [r.prefix, n.key, r.suffix].join(""),
              method: "GET",
              params: ""
            }, n.$http)).then(function(e) {
              return e.data
            }, function() {
              return e.reject(n.key)
            })
          }, i = [], o = n.files.length, a = 0; a < o; a++) i.push(r({
          prefix: n.files[a].prefix,
          key: n.key,
          suffix: n.files[a].suffix
        }));
        return e.all(i).then(function(e) {
          for (var t = e.length, n = {}, r = 0; r < t; r++)
            for (var i in e[r]) n[i] = e[r][i];
          return n
        })
      }
    }
    return angular.module("pascalprecht.translate").factory("$translateStaticFilesLoader", e), e.$inject = ["$q",
      "$http"
    ], e.displayName = "$translateStaticFilesLoader", "pascalprecht.translate"
  })
}
