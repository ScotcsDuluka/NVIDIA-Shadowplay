// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 191
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(4),
    i = require(93).set,
    o = r.MutationObserver || r.WebKitMutationObserver,
    a = r.process,
    s = r.Promise,
    c = "process" == require(27)(a);
  module.exports = function() {
    var e,
      t,
      n,
      u = function() {
        var r, i;
        for (c && (r = a.domain) && r.exit(); e;) {
          i = e.fn, e = e.next;
          try {
            i();
          } catch (r) {
            throw e ? n() : t = void 0, r;
          }
        }
        t = void 0, r && r.enter();
      };
    if (c) n = function() {
      a.nextTick(u);
    };
    else if (!o || r.navigator && r.navigator.standalone) {
      if (s && s.resolve) {
        var l = s.resolve(void 0);
        n = function() {
          l.then(u);
        };
      } else n = function() {
        i.call(r, u);
      };
    } else {
      var d = !0,
        f = document.createTextNode("");
      new o(u).observe(f, {
        characterData: !0
      }), n = function() {
        f.data = d = !d;
      };
    }
    return function(r) {
      var i = {
        fn: r,
        next: void 0
      };
      t && (t.next = i), e || (e = i, n()), t = i;
    };
  };
}
