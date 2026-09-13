// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 265
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n() {
    throw new Error("setTimeout has not been defined");
  }

  function r() {
    throw new Error("clearTimeout has not been defined");
  }

  function i(e) {
    if (l === setTimeout) return setTimeout(e, 0);
    if ((l === n || !l) && setTimeout) return l = setTimeout, setTimeout(e, 0);
    try {
      return l(e, 0);
    } catch (t) {
      try {
        return l.call(null, e, 0);
      } catch (t) {
        return l.call(this, e, 0);
      }
    }
  }

  function o(e) {
    if (d === clearTimeout) return clearTimeout(e);
    if ((d === r || !d) && clearTimeout) return d = clearTimeout, clearTimeout(e);
    try {
      return d(e);
    } catch (t) {
      try {
        return d.call(null, e);
      } catch (t) {
        return d.call(this, e);
      }
    }
  }

  function a() {
    m && h && (m = !1, h.length ? p = h.concat(p) : v = -1, p.length && s());
  }

  function s() {
    if (!m) {
      var e = i(a);
      m = !0;
      for (var t = p.length; t;) {
        for (h = p, p = []; ++v < t;) h && h[v].run();
        v = -1, t = p.length;
      }
      h = null, m = !1, o(e);
    }
  }

  function c(e, t) {
    this.fun = e, this.array = t;
  }

  function u() {}
  var l,
    d,
    f = module.exports = {};
  ! function() {
    try {
      l = "function" == typeof setTimeout ? setTimeout : n;
    } catch (e) {
      l = n;
    }
    try {
      d = "function" == typeof clearTimeout ? clearTimeout : r;
    } catch (e) {
      d = r;
    }
  }();
  var h,
    p = [],
    m = !1,
    v = -1;
  f.nextTick = function(e) {
      var t = new Array(arguments.length - 1);
      if (arguments.length > 1)
        for (var n = 1; n < arguments.length; n++) t[n - 1] = arguments[n];
      p.push(new c(e, t)), 1 !== p.length || m || i(s);
    }, c.prototype.run = function() {
      this.fun.apply(null, this.array);
    }, f.title = "browser", f.browser = !0, f.env = {}, f.argv = [], f.version = "", f.versions = {}, f.on =
    u, f.addListener = u, f.once = u, f.off = u, f.removeListener = u, f.removeAllListeners = u, f.emit = u, f
    .prependListener = u, f.prependOnceListener = u, f.listeners = function(e) {
      return [];
    }, f.binding = function(e) {
      throw new Error("process.binding is not supported");
    }, f.cwd = function() {
      return "/";
    }, f.chdir = function(e) {
      throw new Error("process.chdir is not supported");
    }, f.umask = function() {
      return 0;
    };
}
