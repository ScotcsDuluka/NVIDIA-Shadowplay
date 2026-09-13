// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 65
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e, n, r, a) {
      function s(t) {
        return e(t = 0 === arguments.length ? new Date() : new Date(+t)), t;
      }
      return s.floor = function(t) {
        return e(t = new Date(+t)), t;
      }, s.ceil = function(t) {
        return e(t = new Date(t - 1)), n(t, 1), e(t), t;
      }, s.round = function(e) {
        var t = s(e),
          n = s.ceil(e);
        return e - t < n - e ? t : n;
      }, s.offset = function(e, t) {
        return n(e = new Date(+e), null == t ? 1 : Math.floor(t)), e;
      }, s.range = function(t, r, i) {
        var o,
          a = [];
        if (t = s.ceil(t), i = null == i ? 1 : Math.floor(i), !(t < r && i > 0)) return a;
        do a.push(o = new Date(+t)), n(t, i), e(t); while (o < t && t < r);
        return a;
      }, s.filter = function(r) {
        return t(function(t) {
          if (t >= t)
            for (; e(t), !r(t);) t.setTime(t - 1);
        }, function(e, t) {
          if (e >= e)
            if (t < 0)
              for (; ++t <= 0;)
                for (; n(e, -1), !r(e););
            else
              for (; --t >= 0;)
                for (; n(e, 1), !r(e););
        });
      }, r && (s.count = function(t, n) {
        return i.setTime(+t), o.setTime(+n), e(i), e(o), Math.floor(r(i, o));
      }, s.every = function(e) {
        return e = Math.floor(e), isFinite(e) && e > 0 ? e > 1 ? s.filter(a ? function(t) {
          return a(t) % e === 0;
        } : function(t) {
          return s.count(0, t) % e === 0;
        }) : s : null;
      }), s;
    }

    function n(e) {
      return t(function(t) {
        t.setDate(t.getDate() - (t.getDay() + 7 - e) % 7), t.setHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setDate(e.getDate() + 7 * t);
      }, function(e, t) {
        return (t - e - (t.getTimezoneOffset() - e.getTimezoneOffset()) * u) / f;
      });
    }

    function r(e) {
      return t(function(t) {
        t.setUTCDate(t.getUTCDate() - (t.getUTCDay() + 7 - e) % 7), t.setUTCHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setUTCDate(e.getUTCDate() + 7 * t);
      }, function(e, t) {
        return (t - e) / f;
      });
    }
    var i = new Date(),
      o = new Date(),
      a = t(function() {}, function(e, t) {
        e.setTime(+e + t);
      }, function(e, t) {
        return t - e;
      });
    a.every = function(e) {
      return e = Math.floor(e), isFinite(e) && e > 0 ? e > 1 ? t(function(t) {
        t.setTime(Math.floor(t / e) * e);
      }, function(t, n) {
        t.setTime(+t + n * e);
      }, function(t, n) {
        return (n - t) / e;
      }) : a : null;
    };
    var s = a.range,
      c = 1e3,
      u = 6e4,
      l = 36e5,
      d = 864e5,
      f = 6048e5,
      h = t(function(e) {
        e.setTime(e - e.getMilliseconds());
      }, function(e, t) {
        e.setTime(+e + t * c);
      }, function(e, t) {
        return (t - e) / c;
      }, function(e) {
        return e.getUTCSeconds();
      }),
      p = h.range,
      m = t(function(e) {
        e.setTime(e - e.getMilliseconds() - e.getSeconds() * c);
      }, function(e, t) {
        e.setTime(+e + t * u);
      }, function(e, t) {
        return (t - e) / u;
      }, function(e) {
        return e.getMinutes();
      }),
      v = m.range,
      g = t(function(e) {
        e.setTime(e - e.getMilliseconds() - e.getSeconds() * c - e.getMinutes() * u);
      }, function(e, t) {
        e.setTime(+e + t * l);
      }, function(e, t) {
        return (t - e) / l;
      }, function(e) {
        return e.getHours();
      }),
      y = g.range,
      b = t(function(e) {
        e.setHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setDate(e.getDate() + t);
      }, function(e, t) {
        return (t - e - (t.getTimezoneOffset() - e.getTimezoneOffset()) * u) / d;
      }, function(e) {
        return e.getDate() - 1;
      }),
      E = b.range,
      _ = n(0),
      $ = n(1),
      w = n(2),
      T = n(3),
      C = n(4),
      x = n(5),
      S = n(6),
      A = _.range,
      M = $.range,
      k = w.range,
      N = T.range,
      I = C.range,
      O = x.range,
      D = S.range,
      R = t(function(e) {
        e.setDate(1), e.setHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setMonth(e.getMonth() + t);
      }, function(e, t) {
        return t.getMonth() - e.getMonth() + 12 * (t.getFullYear() - e.getFullYear());
      }, function(e) {
        return e.getMonth();
      }),
      P = R.range,
      L = t(function(e) {
        e.setMonth(0, 1), e.setHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setFullYear(e.getFullYear() + t);
      }, function(e, t) {
        return t.getFullYear() - e.getFullYear();
      }, function(e) {
        return e.getFullYear();
      });
    L.every = function(e) {
      return isFinite(e = Math.floor(e)) && e > 0 ? t(function(t) {
        t.setFullYear(Math.floor(t.getFullYear() / e) * e), t.setMonth(0, 1), t.setHours(0, 0, 0, 0);
      }, function(t, n) {
        t.setFullYear(t.getFullYear() + n * e);
      }) : null;
    };
    var U = L.range,
      F = t(function(e) {
        e.setUTCSeconds(0, 0);
      }, function(e, t) {
        e.setTime(+e + t * u);
      }, function(e, t) {
        return (t - e) / u;
      }, function(e) {
        return e.getUTCMinutes();
      }),
      j = F.range,
      H = t(function(e) {
        e.setUTCMinutes(0, 0, 0);
      }, function(e, t) {
        e.setTime(+e + t * l);
      }, function(e, t) {
        return (t - e) / l;
      }, function(e) {
        return e.getUTCHours();
      }),
      B = H.range,
      z = t(function(e) {
        e.setUTCHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setUTCDate(e.getUTCDate() + t);
      }, function(e, t) {
        return (t - e) / d;
      }, function(e) {
        return e.getUTCDate() - 1;
      }),
      q = z.range,
      G = r(0),
      V = r(1),
      W = r(2),
      Y = r(3),
      K = r(4),
      X = r(5),
      Q = r(6),
      J = G.range,
      Z = V.range,
      ee = W.range,
      te = Y.range,
      ne = K.range,
      re = X.range,
      ie = Q.range,
      oe = t(function(e) {
        e.setUTCDate(1), e.setUTCHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setUTCMonth(e.getUTCMonth() + t);
      }, function(e, t) {
        return t.getUTCMonth() - e.getUTCMonth() + 12 * (t.getUTCFullYear() - e.getUTCFullYear());
      }, function(e) {
        return e.getUTCMonth();
      }),
      ae = oe.range,
      se = t(function(e) {
        e.setUTCMonth(0, 1), e.setUTCHours(0, 0, 0, 0);
      }, function(e, t) {
        e.setUTCFullYear(e.getUTCFullYear() + t);
      }, function(e, t) {
        return t.getUTCFullYear() - e.getUTCFullYear();
      }, function(e) {
        return e.getUTCFullYear();
      });
    se.every = function(e) {
      return isFinite(e = Math.floor(e)) && e > 0 ? t(function(t) {
        t.setUTCFullYear(Math.floor(t.getUTCFullYear() / e) * e), t.setUTCMonth(0, 1), t.setUTCHours(
          0, 0, 0, 0);
      }, function(t, n) {
        t.setUTCFullYear(t.getUTCFullYear() + n * e);
      }) : null;
    };
    var ce = se.range;
    e.timeDay = b, e.timeDays = E, e.timeFriday = x, e.timeFridays = O, e.timeHour = g, e.timeHours = y, e
      .timeInterval = t, e.timeMillisecond = a, e.timeMilliseconds = s, e.timeMinute = m, e.timeMinutes = v,
      e.timeMonday = $, e.timeMondays = M, e.timeMonth = R, e.timeMonths = P, e.timeSaturday = S, e
      .timeSaturdays = D, e.timeSecond = h, e.timeSeconds = p, e.timeSunday = _, e.timeSundays = A, e
      .timeThursday = C, e.timeThursdays = I, e.timeTuesday = w, e.timeTuesdays = k, e.timeWednesday = T, e
      .timeWednesdays = N, e.timeWeek = _, e.timeWeeks = A, e.timeYear = L, e.timeYears = U, e.utcDay = z, e
      .utcDays = q, e.utcFriday = X, e.utcFridays = re, e.utcHour = H, e.utcHours = B, e.utcMillisecond = a,
      e.utcMilliseconds = s, e.utcMinute = F, e.utcMinutes = j, e.utcMonday = V, e.utcMondays = Z, e
      .utcMonth = oe, e.utcMonths = ae, e.utcSaturday = Q, e.utcSaturdays = ie, e.utcSecond = h, e
      .utcSeconds = p, e.utcSunday = G, e.utcSundays = J, e.utcThursday = K, e.utcThursdays = ne, e
      .utcTuesday = W, e.utcTuesdays = ee, e.utcWednesday = Y, e.utcWednesdays = te, e.utcWeek = G, e
      .utcWeeks = J, e.utcYear = se, e.utcYears = ce, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
