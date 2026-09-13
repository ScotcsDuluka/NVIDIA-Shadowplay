// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 92
// service gfwslService
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.gfwslService = void 0;
  var i = require(2) /* app/2 — WINDOW_STYLES (constant) */;
  require(20) /* app/20 — socketService (provider) */, require(25) /* app/25 — hardwareService (service) */;
  var o = i.ngMainCommonModule.service("gfwslService", ["$log", "$q", "hardwareService", "gfwslEndpoints",
    "OSC_BUILD_INFO",
    function(e, t, n, i, o) {
      function r(e, t) {
        var n = "";
        return e.DeviceId && e.VendorId ? (n = e.DeviceId + "_" + e.VendorId, e.SubSystemId && e
            .SubVendorId ? n = n + "_" + e.SubSystemId + "_" + e.SubVendorId : n += "_FFFF_FFFF") : (c
            .error("GPU info has invalid DeviceId and/or VendorId"), n = "FFFF_FFFF_FFFF_FFFF"), n = n +
          "_" + ++t;
      }

      function a(e) {
        var t = [],
          n = e;
        return n = _.sortBy(n, function(e) {
          return -e.IsPrimary;
        }), _.forEach(n, function(e, n) {
          var i = r(e, n);
          t.push(i);
        }), t;
      }

      function l(e) {
        return "1" === e.SLISupported && "1" === e.HasActiveSLITopology ? e.ActiveTopologyGPUCount : e
          .SLISupported;
      }

      function s(e) {
        if (!e || _.isEmpty(e.GPU)) return void c.error("hardwareInfo has empty GPU list");
        c.info("Hardware Info : ", e);
        var t = _.findWhere(e.GPU, {
          IsPrimary: "1"
        }) || e.GPU[0];
        u = {}, u.gcV = _.last(o.oscPackageVersion.split("-")), u.GFPV = e.DriverVersion, u.sM = (e
            .TotalPhysicalMemory / f).toFixed() + "GB", u.osC = e.OSVersion, u.osB = e.OSBuildNumber, u
          .cSR = e.CurrentResolution, u.dIDa = a(e.GPU), u.cID = e.CPUName, u.is6 = "AMD64" === e
          .ProcessorArchitecture ? "1" : "0", u.IsB = "0", u.gIsB = "0", u.isO = e.IsOptimus, u.IsQ = t
          .IsQuadro, u.iLp = d.isLaptop(e), u.isSLI = l(e), u.glg = "en-US", u.lg = "1033";
      }
      var d = this,
        c = e.getInstance("osc/gfwslService"),
        u = null,
        f = 1073741824,
        m = ["portable", "laptop", "sub notebook", "convertible", "detachable", "notebook"];
      d.isLaptop = function(e) {
        var t = "0",
          n = _.isUndefined(e.MoboType) ? "" : e.MoboType.toLowerCase(),
          i = m.find(function(e) {
            return e === n;
          });
        return i && (t = "1"), t;
      }, d.isSLIDevice = function() {
        return u.isSLI;
      }, d.getFeature = function(e) {
        if (!u) return t.reject("GFWSL Params not available");
        var n = i.get(e, u);
        return n().then(function(e) {
          return c.info(e), e.data.criteria;
        }).catch(function(e) {
          return c.error("GFWSL request failed : ", e), t.reject(e);
        });
      }, d.setServer = function(e) {
        i.setServer(e);
      }, d.init = function() {
        return c.info("Initialize GFWSL service"), n.getSystemInfo().then(function(e) {
          return s(e), !0;
        });
      };
    }
  ]);
  exports.gfwslService = o;
}
