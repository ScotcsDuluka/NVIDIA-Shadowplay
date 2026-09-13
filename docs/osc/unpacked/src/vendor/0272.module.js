// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 272
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  (function(t) {
    function r(e, n) {
      if (!(this instanceof r)) return new r(e, n);
      if (n = n || {}, e && "object" == typeof e && (n = e, e = null), e && (e = l(e), n.host = e.host, n
          .secure = "https" == e.protocol || "wss" == e.protocol, n.port = e.port, e.query && (n.query = e
            .query)), this.secure = null != n.secure ? n.secure : t.location && "https:" == location
        .protocol, n.host) {
        var i = n.host.split(":");
        n.hostname = i.shift(), i.length ? n.port = i.pop() : n.port || (n.port = this.secure ? "443" :
          "80");
      }
      this.agent = n.agent || !1, this.hostname = n.hostname || (t.location ? location.hostname :
          "localhost"), this.port = n.port || (t.location && location.port ? location.port : this.secure ?
          443 : 80), this.query = n.query || {}, "string" == typeof this.query && (this.query = f.decode(
          this.query)), this.upgrade = !1 !== n.upgrade, this.path = (n.path || "/engine.io").replace(/\/$/,
          "") + "/", this.forceJSONP = !!n.forceJSONP, this.jsonp = !1 !== n.jsonp, this.forceBase64 = !!n
        .forceBase64, this.enablesXDR = !!n.enablesXDR, this.timestampParam = n.timestampParam || "t", this
        .timestampRequests = n.timestampRequests, this.transports = n.transports || ["polling",
        "websocket"], this.readyState = "", this.writeBuffer = [], this.callbackBuffer = [], this
        .policyPort = n.policyPort || 843, this.rememberUpgrade = n.rememberUpgrade || !1, this.binaryType =
        null, this.onlyBinaryUpgrades = n.onlyBinaryUpgrades, this.pfx = n.pfx || null, this.key = n.key ||
        null, this.passphrase = n.passphrase || null, this.cert = n.cert || null, this.ca = n.ca || null,
        this.ciphers = n.ciphers || null, this.rejectUnauthorized = n.rejectUnauthorized || null, this
        .open();
    }

    function i(e) {
      var t = {};
      for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n]);
      return t;
    }
    var o = require(109),
      a = require(22),
      s = require(45)("engine.io-client:socket"),
      c = require(104),
      u = require(23),
      l = require(277),
      d = require(281),
      f = require(71);
    module.exports = r, r.priorWebsocketSuccess = !1, a(r.prototype), r.protocol = u.protocol, r.Socket = r,
      r.Transport = require(69), r.transports = require(109), r.parser = require(23), r.prototype
      .createTransport = function(e) {
        s('creating transport "%s"', e);
        var t = i(this.query);
        t.EIO = u.protocol, t.transport = e, this.id && (t.sid = this.id);
        var n = new o[e]({
          agent: this.agent,
          hostname: this.hostname,
          port: this.port,
          secure: this.secure,
          path: this.path,
          query: t,
          forceJSONP: this.forceJSONP,
          jsonp: this.jsonp,
          forceBase64: this.forceBase64,
          enablesXDR: this.enablesXDR,
          timestampRequests: this.timestampRequests,
          timestampParam: this.timestampParam,
          policyPort: this.policyPort,
          socket: this,
          pfx: this.pfx,
          key: this.key,
          passphrase: this.passphrase,
          cert: this.cert,
          ca: this.ca,
          ciphers: this.ciphers,
          rejectUnauthorized: this.rejectUnauthorized
        });
        return n;
      }, r.prototype.open = function() {
        var e;
        if (this.rememberUpgrade && r.priorWebsocketSuccess && this.transports.indexOf("websocket") != -1)
          e = "websocket";
        else {
          if (0 == this.transports.length) {
            var t = this;
            return void setTimeout(function() {
              t.emit("error", "No transports available");
            }, 0);
          }
          e = this.transports[0];
        }
        this.readyState = "opening";
        var e;
        try {
          e = this.createTransport(e);
        } catch (e) {
          return this.transports.shift(), void this.open();
        }
        e.open(), this.setTransport(e);
      }, r.prototype.setTransport = function(e) {
        s("setting transport %s", e.name);
        var t = this;
        this.transport && (s("clearing existing transport %s", this.transport.name), this.transport
          .removeAllListeners()), this.transport = e, e.on("drain", function() {
          t.onDrain();
        }).on("packet", function(e) {
          t.onPacket(e);
        }).on("error", function(e) {
          t.onError(e);
        }).on("close", function() {
          t.onClose("transport close");
        });
      }, r.prototype.probe = function(e) {
        function t() {
          if (f.onlyBinaryUpgrades) {
            var t = !this.supportsBinary && f.transport.supportsBinary;
            d = d || t;
          }
          d || (s('probe transport "%s" opened', e), l.send([{
            type: "ping",
            data: "probe"
          }]), l.once("packet", function(t) {
            if (!d)
              if ("pong" == t.type && "probe" == t.data) {
                if (s('probe transport "%s" pong', e), f.upgrading = !0, f.emit("upgrading", l), !l)
                  return;
                r.priorWebsocketSuccess = "websocket" == l.name, s('pausing current transport "%s"', f
                  .transport.name), f.transport.pause(function() {
                  d || "closed" != f.readyState && (s(
                      "changing transport and sending upgrade packet"), u(), f.setTransport(l),
                    l.send([{
                      type: "upgrade"
                    }]), f.emit("upgrade", l), l = null, f.upgrading = !1, f.flush());
                });
              } else {
                s('probe transport "%s" failed', e);
                var n = new Error("probe error");
                n.transport = l.name, f.emit("upgradeError", n);
              }
          }));
        }

        function n() {
          d || (d = !0, u(), l.close(), l = null);
        }

        function i(t) {
          var r = new Error("probe error: " + t);
          r.transport = l.name, n(), s('probe transport "%s" failed because of error: %s', e, t), f.emit(
            "upgradeError", r);
        }

        function o() {
          i("transport closed");
        }

        function a() {
          i("socket closed");
        }

        function c(e) {
          l && e.name != l.name && (s('"%s" works - aborting "%s"', e.name, l.name), n());
        }

        function u() {
          l.removeListener("open", t), l.removeListener("error", i), l.removeListener("close", o), f
            .removeListener("close", a), f.removeListener("upgrading", c);
        }
        s('probing transport "%s"', e);
        var l = this.createTransport(e, {
            probe: 1
          }),
          d = !1,
          f = this;
        r.priorWebsocketSuccess = !1, l.once("open", t), l.once("error", i), l.once("close", o), this.once(
          "close", a), this.once("upgrading", c), l.open();
      }, r.prototype.onOpen = function() {
        if (s("socket open"), this.readyState = "open", r.priorWebsocketSuccess = "websocket" == this
          .transport.name, this.emit("open"), this.flush(), "open" == this.readyState && this.upgrade &&
          this.transport.pause) {
          s("starting upgrade probes");
          for (var e = 0, t = this.upgrades.length; e < t; e++) this.probe(this.upgrades[e]);
        }
      }, r.prototype.onPacket = function(e) {
        if ("opening" == this.readyState || "open" == this.readyState) switch (s(
            'socket receive: type "%s", data "%s"', e.type, e.data), this.emit("packet", e), this.emit(
            "heartbeat"), e.type) {
          case "open":
            this.onHandshake(d(e.data));
            break;
          case "pong":
            this.setPing();
            break;
          case "error":
            var t = new Error("server error");
            t.code = e.data, this.emit("error", t);
            break;
          case "message":
            this.emit("data", e.data), this.emit("message", e.data);
        } else s('packet received with socket readyState "%s"', this.readyState);
      }, r.prototype.onHandshake = function(e) {
        this.emit("handshake", e), this.id = e.sid, this.transport.query.sid = e.sid, this.upgrades = this
          .filterUpgrades(e.upgrades), this.pingInterval = e.pingInterval, this.pingTimeout = e.pingTimeout,
          this.onOpen(), "closed" != this.readyState && (this.setPing(), this.removeListener("heartbeat",
            this.onHeartbeat), this.on("heartbeat", this.onHeartbeat));
      }, r.prototype.onHeartbeat = function(e) {
        clearTimeout(this.pingTimeoutTimer);
        var t = this;
        t.pingTimeoutTimer = setTimeout(function() {
          "closed" != t.readyState && t.onClose("ping timeout");
        }, e || t.pingInterval + t.pingTimeout);
      }, r.prototype.setPing = function() {
        var e = this;
        clearTimeout(e.pingIntervalTimer), e.pingIntervalTimer = setTimeout(function() {
          s("writing ping packet - expecting pong within %sms", e.pingTimeout), e.ping(), e.onHeartbeat(
            e.pingTimeout);
        }, e.pingInterval);
      }, r.prototype.ping = function() {
        this.sendPacket("ping");
      }, r.prototype.onDrain = function() {
        for (var e = 0; e < this.prevBufferLen; e++) this.callbackBuffer[e] && this.callbackBuffer[e]();
        this.writeBuffer.splice(0, this.prevBufferLen), this.callbackBuffer.splice(0, this.prevBufferLen),
          this.prevBufferLen = 0, 0 == this.writeBuffer.length ? this.emit("drain") : this.flush();
      }, r.prototype.flush = function() {
        "closed" != this.readyState && this.transport.writable && !this.upgrading && this.writeBuffer
          .length && (s("flushing %d packets in socket", this.writeBuffer.length), this.transport.send(this
            .writeBuffer), this.prevBufferLen = this.writeBuffer.length, this.emit("flush"));
      }, r.prototype.write = r.prototype.send = function(e, t) {
        return this.sendPacket("message", e, t), this;
      }, r.prototype.sendPacket = function(e, t, n) {
        if ("closing" != this.readyState && "closed" != this.readyState) {
          var r = {
            type: e,
            data: t
          };
          this.emit("packetCreate", r), this.writeBuffer.push(r), this.callbackBuffer.push(n), this.flush();
        }
      }, r.prototype.close = function() {
        function e() {
          r.onClose("forced close"), s("socket closing - telling transport to close"), r.transport.close();
        }

        function t() {
          r.removeListener("upgrade", t), r.removeListener("upgradeError", t), e();
        }

        function n() {
          r.once("upgrade", t), r.once("upgradeError", t);
        }
        if ("opening" == this.readyState || "open" == this.readyState) {
          this.readyState = "closing";
          var r = this;
          this.writeBuffer.length ? this.once("drain", function() {
            this.upgrading ? n() : e();
          }) : this.upgrading ? n() : e();
        }
        return this;
      }, r.prototype.onError = function(e) {
        s("socket error %j", e), r.priorWebsocketSuccess = !1, this.emit("error", e), this.onClose(
          "transport error", e);
      }, r.prototype.onClose = function(e, t) {
        if ("opening" == this.readyState || "open" == this.readyState || "closing" == this.readyState) {
          s('socket close with reason: "%s"', e);
          var n = this;
          clearTimeout(this.pingIntervalTimer), clearTimeout(this.pingTimeoutTimer), setTimeout(function() {
              n.writeBuffer = [], n.callbackBuffer = [], n.prevBufferLen = 0;
            }, 0), this.transport.removeAllListeners("close"), this.transport.close(), this.transport
            .removeAllListeners(), this.readyState = "closed", this.id = null, this.emit("close", e, t);
        }
      }, r.prototype.filterUpgrades = function(e) {
        for (var t = [], n = 0, r = e.length; n < r; n++) ~c(this.transports, e[n]) && t.push(e[n]);
        return t;
      };
  }).call(exports, function() {
    return this;
  }());
}
