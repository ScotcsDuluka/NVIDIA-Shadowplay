// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 72
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r() {}

  function i(e) {
    var n = "",
      r = !1;
    return n += e.type, exports.BINARY_EVENT != e.type && exports.BINARY_ACK != e.type || (n += e.attachments,
        n += "-"), e.nsp && "/" != e.nsp && (r = !0, n += e.nsp), null != e.id && (r && (n += ",", r = !1),
        n += e.id), null != e.data && (r && (n += ","), n += d.stringify(e.data)), l("encoded %j as %s", e,
      n), n;
  }

  function o(e, t) {
    function n(e) {
      var n = h.deconstructPacket(e),
        r = i(n.packet),
        o = n.buffers;
      o.unshift(r), t(o);
    }
    h.removeBlobs(e, n);
  }

  function a() {
    this.reconstructor = null;
  }

  function s(e) {
    var n = {},
      r = 0;
    if (n.type = Number(e.charAt(0)), null == exports.types[n.type]) return u();
    if (exports.BINARY_EVENT == n.type || exports.BINARY_ACK == n.type) {
      for (var i = "";
        "-" != e.charAt(++r) && (i += e.charAt(r), r != e.length););
      if (i != Number(i) || "-" != e.charAt(r)) throw new Error("Illegal attachments");
      n.attachments = Number(i);
    }
    if ("/" == e.charAt(r + 1))
      for (n.nsp = ""; ++r;) {
        var o = e.charAt(r);
        if ("," == o) break;
        if (n.nsp += o, r == e.length) break;
      } else n.nsp = "/";
    var a = e.charAt(r + 1);
    if ("" !== a && Number(a) == a) {
      for (n.id = ""; ++r;) {
        var o = e.charAt(r);
        if (null == o || Number(o) != o) {
          --r;
          break;
        }
        if (n.id += e.charAt(r), r == e.length) break;
      }
      n.id = Number(n.id);
    }
    if (e.charAt(++r)) try {
      n.data = d.parse(e.substr(r));
    } catch (e) {
      return u();
    }
    return l("decoded %s as %j", e, n), n;
  }

  function c(e) {
    this.reconPack = e, this.buffers = [];
  }

  function u(e) {
    return {
      type: exports.ERROR,
      data: "parser error"
    };
  }
  var l = require(32)("socket.io-parser"),
    d = require(280),
    f = (require(68), require(22)),
    h = require(283),
    p = require(112);
  exports.protocol = 4, exports.types = ["CONNECT", "DISCONNECT", "EVENT", "BINARY_EVENT", "ACK",
      "BINARY_ACK", "ERROR"
    ], exports.CONNECT = 0, exports.DISCONNECT = 1, exports.EVENT = 2, exports.ACK = 3, exports.ERROR = 4,
    exports.BINARY_EVENT = 5, exports.BINARY_ACK = 6, exports.Encoder = r, exports.Decoder = a, r.prototype
    .encode = function(e, n) {
      if (l("encoding packet %j", e), exports.BINARY_EVENT == e.type || exports.BINARY_ACK == e.type) o(e, n);
      else {
        var r = i(e);
        n([r]);
      }
    }, f(a.prototype), a.prototype.add = function(e) {
      var n;
      if ("string" == typeof e) n = s(e), exports.BINARY_EVENT == n.type || exports.BINARY_ACK == n.type ? (
        this.reconstructor = new c(n), 0 === this.reconstructor.reconPack.attachments && this.emit(
          "decoded", n)) : this.emit("decoded", n);
      else {
        if (!p(e) && !e.base64) throw new Error("Unknown type: " + e);
        if (!this.reconstructor) throw new Error("got binary data when not reconstructing a packet");
        n = this.reconstructor.takeBinaryData(e), n && (this.reconstructor = null, this.emit("decoded", n));
      }
    }, a.prototype.destroy = function() {
      this.reconstructor && this.reconstructor.finishedReconstruction();
    }, c.prototype.takeBinaryData = function(e) {
      if (this.buffers.push(e), this.buffers.length == this.reconPack.attachments) {
        var t = h.reconstructPacket(this.reconPack, this.buffers);
        return this.finishedReconstruction(), t;
      }
      return null;
    }, c.prototype.finishedReconstruction = function() {
      this.reconPack = null, this.buffers = [];
    };
}
