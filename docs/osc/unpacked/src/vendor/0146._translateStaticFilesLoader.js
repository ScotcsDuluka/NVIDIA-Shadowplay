// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 146
// factory $translateStaticFilesLoader
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r, i;
  ! function(n, o) {
    r = [], i = function() {
      return o();
    }.apply(exports, r), !(void 0 !== i && (module.exports = i));
  }(this, function() {
    function e(e, t) {
      "use strict";
      return function(n) {
        if (!(n && (angular.isArray(n.files) || angular.isString(n.prefix) && angular.isString(n
          .suffix)))) throw new Error(
          "Couldn't load static files, no files and prefix or suffix specified!");
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
              return e.data;
            }, function() {
              return e.reject(n.key);
            });
          }, i = [], o = n.files.length, a = 0; a < o; a++) i.push(r({
          prefix: n.files[a].prefix,
          key: n.key,
          suffix: n.files[a].suffix
        }));
        return e.all(i).then(function(e) {
          for (var t = e.length, n = {}, r = 0; r < t; r++)
            for (var i in e[r]) n[i] = e[r][i];
          return n;
        });
      };
    }
    return angular.module("pascalprecht.translate").factory("$translateStaticFilesLoader", e), e.$inject = [
      "$q", "$http"
    ], e.displayName = "$translateStaticFilesLoader", "pascalprecht.translate";
  });
}
