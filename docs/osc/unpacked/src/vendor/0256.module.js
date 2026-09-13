// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 256
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r, i, o;
  ! function(a, s) {
    "use strict";
    i = [require(286)], r = s, o = "function" == typeof r ? r.apply(exports, i) : r, !(void 0 !== o && (module
      .exports = o));
  }(this, function(e) {
    "use strict";

    function t(e, t, n) {
      if ("function" == typeof Array.prototype.map) return e.map(t, n);
      for (var r = new Array(e.length), i = 0; i < e.length; i++) r[i] = t.call(n, e[i]);
      return r;
    }

    function n(e, t, n) {
      if ("function" == typeof Array.prototype.filter) return e.filter(t, n);
      for (var r = [], i = 0; i < e.length; i++) t.call(n, e[i]) && r.push(e[i]);
      return r;
    }

    function r(e, t) {
      if ("function" == typeof Array.prototype.indexOf) return e.indexOf(t);
      for (var n = 0; n < e.length; n++)
        if (e[n] === t) return n;
      return -1;
    }
    var i = /(^|@)\S+\:\d+/,
      o = /^\s*at .*(\S+\:\d+|\(native\))/m,
      a = /^(eval@)?(\[native code\])?$/;
    return {
      parse: function(e) {
        if ("undefined" != typeof e.stacktrace || "undefined" != typeof e["opera#sourceloc"]) return this
          .parseOpera(e);
        if (e.stack && e.stack.match(o)) return this.parseV8OrIE(e);
        if (e.stack) return this.parseFFOrSafari(e);
        throw new Error("Cannot parse given Error object");
      },
      extractLocation: function(e) {
        if (e.indexOf(":") === -1) return [e];
        var t = /(.+?)(?:\:(\d+))?(?:\:(\d+))?$/,
          n = t.exec(e.replace(/[\(\)]/g, ""));
        return [n[1], n[2] || void 0, n[3] || void 0];
      },
      parseV8OrIE: function(i) {
        var a = n(i.stack.split("\n"), function(e) {
          return !!e.match(o);
        }, this);
        return t(a, function(t) {
          t.indexOf("(eval ") > -1 && (t = t.replace(/eval code/g, "eval").replace(
            /(\(eval at [^\()]*)|(\)\,.*$)/g, ""));
          var n = t.replace(/^\s+/, "").replace(/\(eval code/g, "(").split(/\s+/).slice(1),
            i = this.extractLocation(n.pop()),
            o = n.join(" ") || void 0,
            a = r(["eval", "<anonymous>"], i[0]) > -1 ? void 0 : i[0];
          return new e(o, void 0, a, i[1], i[2], t);
        }, this);
      },
      parseFFOrSafari: function(r) {
        var i = n(r.stack.split("\n"), function(e) {
          return !e.match(a);
        }, this);
        return t(i, function(t) {
          if (t.indexOf(" > eval") > -1 && (t = t.replace(
              / line (\d+)(?: > eval line \d+)* > eval\:\d+\:\d+/g, ":$1")), t.indexOf("@") === -1 &&
            t.indexOf(":") === -1) return new e(t);
          var n = t.split("@"),
            r = this.extractLocation(n.pop()),
            i = n.join("@") || void 0;
          return new e(i, void 0, r[0], r[1], r[2], t);
        }, this);
      },
      parseOpera: function(e) {
        return !e.stacktrace || e.message.indexOf("\n") > -1 && e.message.split("\n").length > e
          .stacktrace.split("\n").length ? this.parseOpera9(e) : e.stack ? this.parseOpera11(e) : this
          .parseOpera10(e);
      },
      parseOpera9: function(t) {
        for (var n = /Line (\d+).*script (?:in )?(\S+)/i, r = t.message.split("\n"), i = [], o = 2, a = r
            .length; o < a; o += 2) {
          var s = n.exec(r[o]);
          s && i.push(new e(void 0, void 0, s[2], s[1], void 0, r[o]));
        }
        return i;
      },
      parseOpera10: function(t) {
        for (var n = /Line (\d+).*script (?:in )?(\S+)(?:: In function (\S+))?$/i, r = t.stacktrace.split(
            "\n"), i = [], o = 0, a = r.length; o < a; o += 2) {
          var s = n.exec(r[o]);
          s && i.push(new e(s[3] || void 0, void 0, s[2], s[1], void 0, r[o]));
        }
        return i;
      },
      parseOpera11: function(r) {
        var o = n(r.stack.split("\n"), function(e) {
          return !!e.match(i) && !e.match(/^Error created at/);
        }, this);
        return t(o, function(t) {
          var n,
            r = t.split("@"),
            i = this.extractLocation(r.pop()),
            o = r.shift() || "",
            a = o.replace(/<anonymous function(: (\w+))?>/, "$2").replace(/\([^\)]*\)/g, "") ||
            void 0;
          o.match(/\(([^\)]*)\)/) && (n = o.replace(/^[^\(]+\(([^\)]*)\)$/, "$1"));
          var s = void 0 === n || "[arguments not available]" === n ? void 0 : n.split(",");
          return new e(a, s, i[0], i[1], i[2], t);
        }, this);
      }
    };
  });
}
