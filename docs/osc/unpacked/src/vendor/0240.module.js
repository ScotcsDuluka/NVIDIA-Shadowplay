// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 240
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      return e;
    }

    function n(e) {
      return "translate(" + (e + .5) + ",0)";
    }

    function r(e) {
      return "translate(0," + (e + .5) + ")";
    }

    function i(e) {
      return function(t) {
        return +e(t);
      };
    }

    function o(e) {
      var t = Math.max(0, e.bandwidth() - 1) / 2;
      return e.round() && (t = Math.round(t)),
        function(n) {
          return +e(n) + t;
        };
    }

    function a() {
      return !this.__axis;
    }

    function s(e, s) {
      function c(n) {
        var r = null == l ? s.ticks ? s.ticks.apply(s, u) : s.domain() : l,
          c = null == d ? s.tickFormat ? s.tickFormat.apply(s, u) : t : d,
          f = Math.max(y, 0) + E,
          T = s.range(),
          C = +T[0] + .5,
          x = +T[T.length - 1] + .5,
          S = (s.bandwidth ? o : i)(s.copy()),
          A = n.selection ? n.selection() : n,
          M = A.selectAll(".domain").data([null]),
          k = A.selectAll(".tick").data(r, s).order(),
          N = k.exit(),
          I = k.enter().append("g").attr("class", "tick"),
          O = k.select("line"),
          D = k.select("text");
        M = M.merge(M.enter().insert("path", ".tick").attr("class", "domain").attr("stroke",
          "currentColor")), k = k.merge(I), O = O.merge(I.append("line").attr("stroke", "currentColor")
          .attr($ + "2", _ * y)), D = D.merge(I.append("text").attr("fill", "currentColor").attr($, _ * f)
          .attr("dy", e === h ? "0em" : e === m ? "0.71em" : "0.32em")), n !== A && (M = M.transition(n),
          k = k.transition(n), O = O.transition(n), D = D.transition(n), N = N.transition(n).attr(
            "opacity", g).attr("transform", function(e) {
            return isFinite(e = S(e)) ? w(e) : this.getAttribute("transform");
          }), I.attr("opacity", g).attr("transform", function(e) {
            var t = this.parentNode.__axis;
            return w(t && isFinite(t = t(e)) ? t : S(e));
          })), N.remove(), M.attr("d", e === v || e == p ? b ? "M" + _ * b + "," + C + "H0.5V" + x + "H" +
          _ * b : "M0.5," + C + "V" + x : b ? "M" + C + "," + _ * b + "V0.5H" + x + "V" + _ * b : "M" +
          C + ",0.5H" + x), k.attr("opacity", 1).attr("transform", function(e) {
          return w(S(e));
        }), O.attr($ + "2", _ * y), D.attr($, _ * f).text(c), A.filter(a).attr("fill", "none").attr(
          "font-size", 10).attr("font-family", "sans-serif").attr("text-anchor", e === p ? "start" : e ===
          v ? "end" : "middle"), A.each(function() {
          this.__axis = S;
        });
      }
      var u = [],
        l = null,
        d = null,
        y = 6,
        b = 6,
        E = 3,
        _ = e === h || e === v ? -1 : 1,
        $ = e === v || e === p ? "x" : "y",
        w = e === h || e === m ? n : r;
      return c.scale = function(e) {
        return arguments.length ? (s = e, c) : s;
      }, c.ticks = function() {
        return u = f.call(arguments), c;
      }, c.tickArguments = function(e) {
        return arguments.length ? (u = null == e ? [] : f.call(e), c) : u.slice();
      }, c.tickValues = function(e) {
        return arguments.length ? (l = null == e ? null : f.call(e), c) : l && l.slice();
      }, c.tickFormat = function(e) {
        return arguments.length ? (d = e, c) : d;
      }, c.tickSize = function(e) {
        return arguments.length ? (y = b = +e, c) : y;
      }, c.tickSizeInner = function(e) {
        return arguments.length ? (y = +e, c) : y;
      }, c.tickSizeOuter = function(e) {
        return arguments.length ? (b = +e, c) : b;
      }, c.tickPadding = function(e) {
        return arguments.length ? (E = +e, c) : E;
      }, c;
    }

    function c(e) {
      return s(h, e);
    }

    function u(e) {
      return s(p, e);
    }

    function l(e) {
      return s(m, e);
    }

    function d(e) {
      return s(v, e);
    }
    var f = Array.prototype.slice,
      h = 1,
      p = 2,
      m = 3,
      v = 4,
      g = 1e-6;
    e.axisTop = c, e.axisRight = u, e.axisBottom = l, e.axisLeft = d, Object.defineProperty(e,
    "__esModule", {
      value: !0
    });
  });
}
