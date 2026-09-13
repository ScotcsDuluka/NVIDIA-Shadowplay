// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 429
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

    function i(e) {
      return "translate(0," + (e + .5) + ")";
    }

    function o(e) {
      return function(t) {
        return +e(t);
      };
    }

    function r(e) {
      var t = Math.max(0, e.bandwidth() - 1) / 2;
      return e.round() && (t = Math.round(t)),
        function(n) {
          return +e(n) + t;
        };
    }

    function a() {
      return !this.__axis;
    }

    function l(e, l) {
      function s(n) {
        var i = null == c ? l.ticks ? l.ticks.apply(l, d) : l.domain() : c,
          s = null == u ? l.tickFormat ? l.tickFormat.apply(l, d) : t : u,
          f = Math.max(x, 0) + y,
          k = l.range(),
          _ = +k[0] + .5,
          T = +k[k.length - 1] + .5,
          C = (l.bandwidth ? r : o)(l.copy()),
          O = n.selection ? n.selection() : n,
          A = O.selectAll(".domain").data([null]),
          I = O.selectAll(".tick").data(i, l).order(),
          M = I.exit(),
          R = I.enter().append("g").attr("class", "tick"),
          P = I.select("line"),
          D = I.select("text");
        A = A.merge(A.enter().insert("path", ".tick").attr("class", "domain").attr("stroke",
          "currentColor")), I = I.merge(R), P = P.merge(R.append("line").attr("stroke", "currentColor")
          .attr(S + "2", w * x)), D = D.merge(R.append("text").attr("fill", "currentColor").attr(S, w * f)
          .attr("dy", e === m ? "0em" : e === p ? "0.71em" : "0.32em")), n !== O && (A = A.transition(n),
          I = I.transition(n), P = P.transition(n), D = D.transition(n), M = M.transition(n).attr(
            "opacity", b).attr("transform", function(e) {
            return isFinite(e = C(e)) ? E(e) : this.getAttribute("transform");
          }), R.attr("opacity", b).attr("transform", function(e) {
            var t = this.parentNode.__axis;
            return E(t && isFinite(t = t(e)) ? t : C(e));
          })), M.remove(), A.attr("d", e === h || e == g ? v ? "M" + w * v + "," + _ + "H0.5V" + T + "H" +
          w * v : "M0.5," + _ + "V" + T : v ? "M" + _ + "," + w * v + "V0.5H" + T + "V" + w * v : "M" +
          _ + ",0.5H" + T), I.attr("opacity", 1).attr("transform", function(e) {
          return E(C(e));
        }), P.attr(S + "2", w * x), D.attr(S, w * f).text(s), O.filter(a).attr("fill", "none").attr(
          "font-size", 10).attr("font-family", "sans-serif").attr("text-anchor", e === g ? "start" : e ===
          h ? "end" : "middle"), O.each(function() {
          this.__axis = C;
        });
      }
      var d = [],
        c = null,
        u = null,
        x = 6,
        v = 6,
        y = 3,
        w = e === m || e === h ? -1 : 1,
        S = e === h || e === g ? "x" : "y",
        E = e === m || e === p ? n : i;
      return s.scale = function(e) {
        return arguments.length ? (l = e, s) : l;
      }, s.ticks = function() {
        return d = f.call(arguments), s;
      }, s.tickArguments = function(e) {
        return arguments.length ? (d = null == e ? [] : f.call(e), s) : d.slice();
      }, s.tickValues = function(e) {
        return arguments.length ? (c = null == e ? null : f.call(e), s) : c && c.slice();
      }, s.tickFormat = function(e) {
        return arguments.length ? (u = e, s) : u;
      }, s.tickSize = function(e) {
        return arguments.length ? (x = v = +e, s) : x;
      }, s.tickSizeInner = function(e) {
        return arguments.length ? (x = +e, s) : x;
      }, s.tickSizeOuter = function(e) {
        return arguments.length ? (v = +e, s) : v;
      }, s.tickPadding = function(e) {
        return arguments.length ? (y = +e, s) : y;
      }, s;
    }

    function s(e) {
      return l(m, e);
    }

    function d(e) {
      return l(g, e);
    }

    function c(e) {
      return l(p, e);
    }

    function u(e) {
      return l(h, e);
    }
    var f = Array.prototype.slice,
      m = 1,
      g = 2,
      p = 3,
      h = 4,
      b = 1e-6;
    e.axisTop = s, e.axisRight = d, e.axisBottom = c, e.axisLeft = u, Object.defineProperty(e,
    "__esModule", {
      value: !0
    });
  });
}
