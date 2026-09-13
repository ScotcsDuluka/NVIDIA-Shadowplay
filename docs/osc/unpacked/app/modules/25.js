// ─────────────────────────────────────────────────────────────
// APP MODULE 25
// role       : service hardwareService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.hardwareService = void 0;
  var o = n(3),
    r = i(o),
    a = n(2);
  n(17);
  var l = a.ngMainCommonModule.service("hardwareService", ["$log", "$q", "$filter", "eventAggregator",
    "hardwareEndpoints", "COMMON_EVENTS",
    function(e, t, n, i, o, a) {
      function l(e, t) {
        return Number(Math.round(e + "e" + t) + "e-" + t)
      }

      function s(e) {
        var t, n;
        try {
          return r.isString(e) ? (n = e / y / y / y, t = l(n, 2), isNaN(t) ? v : t + " GB RAM") : v
        } catch (e) {
          return x.error("failed to format memory", e), v
        }
      }

      function d(e) {
        var t, n, i;
        try {
          return r.isString(e) ? (t = e.split("@"), 2 === t.length && (n = t[0].split("x"), 2 === n.length) ? i = n[
            0] + " x " + n[1] + ", " + t[1] + "Hz" : v) : v
        } catch (e) {
          return x.error("failed to format resolution", e), v
        }
      }

      function c(e) {
        var t;
        if (t = r.isArray(e.GPU) ? e.GPU.length : 0, 0 === t) return v;
        if (1 === t) return e.GPU[0].LongGPUName;
        try {
          var n = "",
            i = r.pluck(e.GPU, "LongGPUName"),
            o = [];
          r.each(i, function(e, t) {
            o.push({
              name: e
            })
          });
          var a = r.countBy(o, "name"),
            l = 0;
          return r.forEach(a, function(e, t) {
            var i = e > 1 ? " x " + e : "",
              o = l < r.size(a) - 1 ? " | " : "";
            n += t + i + o, l++
          }), x.info(n), n
        } catch (e) {
          return x.error("failed to format GPU info", e), v
        }
      }

      function u(e) {
        var t = "";
        return t = n("translate")("l10n.videoDescriptionGpu", {
          arg1: e.gpuName
        }) + "\n" + n("translate")("l10n.videoDescriptionCpu", {
          arg1: e.cpuName
        }) + "\n" + n("translate")("l10n.videoDescriptionMemory", {
          arg1: e.physicalMemory,
          arg2: e.availablePhysicalMemory
        }) + "\n" + n("translate")("l10n.videoDescriptionResolution", {
          arg1: e.currentResolution
        }) + "\n" + n("translate")("l10n.videoDescriptionOS", {
          arg1: e.osName
        }) + "\n", x.info(t), t
      }

      function f(e) {
        return x.info("retrieved system info"), g = null, h = e.data, e.data
      }

      function m() {
        return g && (x.info("canceling gethardware request"), o.get.cancel(b), g = null), p.reloadSystemInfo()
      }
      var g = null,
        p = this,
        h = null,
        b = null,
        x = e.getInstance("main.common/hardwareService"),
        v = "-",
        y = 1024;
      p.getSystemDescription = function() {
        return p.getSystemInfo().then(function(e) {
          var t = {};
          return t.gpuName = c(e), t.cpuName = r.isString(e.CPUName) ? e.CPUName : v, t.driverVersion = r
            .isString(e.DriverVersion) ? e.DriverVersion : v, t.physicalMemory = s(e.PhysicalMemoryCapacity),
            t.availablePhysicalMemory = s(e.TotalPhysicalMemory), t.currentResolution = d(e
            .CurrentResolution), t.osName = e.OSName, u(t)
        })
      }, p.reloadSystemInfo = function() {
        return r.isNull(g) ? (h = null, b = o.get(), g = b.then(f).catch(function(e) {
          return x.error("failed to get system info", e), t.reject(e)
        })) : g
      }, p.getSystemInfo = function() {
        return h ? t.when(h) : p.reloadSystemInfo()
      }, p.init = function() {
        m().then(function(e) {
          i.trigger(a.SYSTEMINFO_UPDATED, e)
        }).catch(function() {
          x.error("failed to reload sys info on focus event")
        })
      }
    }
  ]);
  t.hardwareService = l
}
