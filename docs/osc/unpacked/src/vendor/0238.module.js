// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 238
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(43), require(98));
  }(this, function(e) {
    return function() {
      var t = e,
        n = t.x64,
        r = n.Word,
        i = n.WordArray,
        o = t.algo,
        a = o.SHA512,
        s = o.SHA384 = a.extend({
          _doReset: function() {
            this._hash = new i.init([new r.init(3418070365, 3238371032), new r.init(1654270250,
                914150663), new r.init(2438529370, 812702999), new r.init(355462360, 4144912697),
              new r.init(1731405415, 4290775857), new r.init(2394180231, 1750603025), new r.init(
                3675008525, 1694076839), new r.init(1203062813, 3204075428)
            ]);
          },
          _doFinalize: function() {
            var e = a._doFinalize.call(this);
            return e.sigBytes -= 16, e;
          }
        });
      t.SHA384 = a._createHelper(s), t.HmacSHA384 = a._createHmacHelper(s);
    }(), e.SHA384;
  });
}
