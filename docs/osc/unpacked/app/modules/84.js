// ─────────────────────────────────────────────────────────────
// APP MODULE 84
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e, n, i, a) {
      function l(t) {
        return e(t = 0 === arguments.length ? new Date : new Date(+t)), t
      }
      return l.floor = function(t) {
        return e(t = new Date(+t)), t
      }, l.ceil = function(t) {
        return e(t = new Date(t - 1)), n(t, 1), e(t), t
      }, l.round = function(e) {
        var t = l(e),
          n = l.ceil(e);
        return e - t < n - e ? t : n
      }, l.offset = function(e, t) {
        return n(e = new Date(+e), null == t ? 1 : Math.floor(t)), e
      }, l.range = function(t, i, o) {
        var r, a = [];
        if (t = l.ceil(t), o = null == o ? 1 : Math.floor(o), !(t < i && o > 0)) return a;
        do a.push(r = new Date(+t)), n(t, o), e(t); while (r < t && t < i);
        return a
      }, l.filter = function(i) {
        return t(function(t) {
          if (t >= t)
            for (; e(t), !i(t);) t.setTime(t - 1)
        }, function(e, t) {
          if (e >= e)
            if (t < 0)
              for (; ++t <= 0;)
                for (; n(e, -1), !i(e););
            else
              for (; --t >= 0;)
                for (; n(e, 1), !i(e););
        })
      }, i && (l.count = function(t, n) {
        return o.setTime(+t), r.setTime(+n), e(o), e(r), Math.floor(i(o, r))
      }, l.every = function(e) {
        return e = Math.floor(e), isFinite(e) && e > 0 ? e > 1 ? l.filter(a ? function(t) {
          return a(t) % e === 0
        } : function(t) {
          return l.count(0, t) % e === 0
        }) : l : null
      }), l
    }

    function n(e) {
      return t(function(t) {
        t.setDate(t.getDate() - (t.getDay() + 7 - e) % 7), t.setHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setDate(e.getDate() + 7 * t)
      }, function(e, t) {
        return (t - e - (t.getTimezoneOffset() - e.getTimezoneOffset()) * d) / f
      })
    }

    function i(e) {
      return t(function(t) {
        t.setUTCDate(t.getUTCDate() - (t.getUTCDay() + 7 - e) % 7), t.setUTCHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setUTCDate(e.getUTCDate() + 7 * t)
      }, function(e, t) {
        return (t - e) / f
      })
    }
    var o = new Date,
      r = new Date,
      a = t(function() {}, function(e, t) {
        e.setTime(+e + t)
      }, function(e, t) {
        return t - e
      });
    a.every = function(e) {
      return e = Math.floor(e), isFinite(e) && e > 0 ? e > 1 ? t(function(t) {
        t.setTime(Math.floor(t / e) * e)
      }, function(t, n) {
        t.setTime(+t + n * e)
      }, function(t, n) {
        return (n - t) / e
      }) : a : null
    };
    var l = a.range,
      s = 1e3,
      d = 6e4,
      c = 36e5,
      u = 864e5,
      f = 6048e5,
      m = t(function(e) {
        e.setTime(e - e.getMilliseconds())
      }, function(e, t) {
        e.setTime(+e + t * s)
      }, function(e, t) {
        return (t - e) / s
      }, function(e) {
        return e.getUTCSeconds()
      }),
      g = m.range,
      p = t(function(e) {
        e.setTime(e - e.getMilliseconds() - e.getSeconds() * s)
      }, function(e, t) {
        e.setTime(+e + t * d)
      }, function(e, t) {
        return (t - e) / d
      }, function(e) {
        return e.getMinutes()
      }),
      h = p.range,
      b = t(function(e) {
        e.setTime(e - e.getMilliseconds() - e.getSeconds() * s - e.getMinutes() * d)
      }, function(e, t) {
        e.setTime(+e + t * c)
      }, function(e, t) {
        return (t - e) / c
      }, function(e) {
        return e.getHours()
      }),
      x = b.range,
      v = t(function(e) {
        e.setHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setDate(e.getDate() + t)
      }, function(e, t) {
        return (t - e - (t.getTimezoneOffset() - e.getTimezoneOffset()) * d) / u
      }, function(e) {
        return e.getDate() - 1
      }),
      y = v.range,
      w = n(0),
      S = n(1),
      E = n(2),
      k = n(3),
      _ = n(4),
      T = n(5),
      C = n(6),
      O = w.range,
      A = S.range,
      I = E.range,
      M = k.range,
      R = _.range,
      P = T.range,
      D = C.range,
      N = t(function(e) {
        e.setDate(1), e.setHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setMonth(e.getMonth() + t)
      }, function(e, t) {
        return t.getMonth() - e.getMonth() + 12 * (t.getFullYear() - e.getFullYear())
      }, function(e) {
        return e.getMonth()
      }),
      L = N.range,
      F = t(function(e) {
        e.setMonth(0, 1), e.setHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setFullYear(e.getFullYear() + t)
      }, function(e, t) {
        return t.getFullYear() - e.getFullYear()
      }, function(e) {
        return e.getFullYear()
      });
    F.every = function(e) {
      return isFinite(e = Math.floor(e)) && e > 0 ? t(function(t) {
        t.setFullYear(Math.floor(t.getFullYear() / e) * e), t.setMonth(0, 1), t.setHours(0, 0, 0, 0)
      }, function(t, n) {
        t.setFullYear(t.getFullYear() + n * e)
      }) : null
    };
    var U = F.range,
      z = t(function(e) {
        e.setUTCSeconds(0, 0)
      }, function(e, t) {
        e.setTime(+e + t * d)
      }, function(e, t) {
        return (t - e) / d
      }, function(e) {
        return e.getUTCMinutes()
      }),
      G = z.range,
      V = t(function(e) {
        e.setUTCMinutes(0, 0, 0)
      }, function(e, t) {
        e.setTime(+e + t * c)
      }, function(e, t) {
        return (t - e) / c
      }, function(e) {
        return e.getUTCHours()
      }),
      H = V.range,
      B = t(function(e) {
        e.setUTCHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setUTCDate(e.getUTCDate() + t)
      }, function(e, t) {
        return (t - e) / u
      }, function(e) {
        return e.getUTCDate() - 1
      }),
      Y = B.range,
      $ = i(0),
      W = i(1),
      j = i(2),
      K = i(3),
      q = i(4),
      X = i(5),
      Z = i(6),
      Q = $.range,
      J = W.range,
      ee = j.range,
      te = K.range,
      ne = q.range,
      ie = X.range,
      oe = Z.range,
      re = t(function(e) {
        e.setUTCDate(1), e.setUTCHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setUTCMonth(e.getUTCMonth() + t)
      }, function(e, t) {
        return t.getUTCMonth() - e.getUTCMonth() + 12 * (t.getUTCFullYear() - e.getUTCFullYear())
      }, function(e) {
        return e.getUTCMonth()
      }),
      ae = re.range,
      le = t(function(e) {
        e.setUTCMonth(0, 1), e.setUTCHours(0, 0, 0, 0)
      }, function(e, t) {
        e.setUTCFullYear(e.getUTCFullYear() + t)
      }, function(e, t) {
        return t.getUTCFullYear() - e.getUTCFullYear()
      }, function(e) {
        return e.getUTCFullYear()
      });
    le.every = function(e) {
      return isFinite(e = Math.floor(e)) && e > 0 ? t(function(t) {
        t.setUTCFullYear(Math.floor(t.getUTCFullYear() / e) * e), t.setUTCMonth(0, 1), t.setUTCHours(0, 0, 0, 0)
      }, function(t, n) {
        t.setUTCFullYear(t.getUTCFullYear() + n * e)
      }) : null
    };
    var se = le.range;
    e.timeDay = v, e.timeDays = y, e.timeFriday = T, e.timeFridays = P, e.timeHour = b, e.timeHours = x, e
      .timeInterval = t, e.timeMillisecond = a, e.timeMilliseconds = l, e.timeMinute = p, e.timeMinutes = h, e
      .timeMonday = S, e.timeMondays = A, e.timeMonth = N, e.timeMonths = L, e.timeSaturday = C, e.timeSaturdays = D,
      e.timeSecond = m, e.timeSeconds = g, e.timeSunday = w, e.timeSundays = O, e.timeThursday = _, e.timeThursdays =
      R, e.timeTuesday = E, e.timeTuesdays = I, e.timeWednesday = k, e.timeWednesdays = M, e.timeWeek = w, e
      .timeWeeks = O, e.timeYear = F, e.timeYears = U, e.utcDay = B, e.utcDays = Y, e.utcFriday = X, e.utcFridays =
      ie, e.utcHour = V, e.utcHours = H, e.utcMillisecond = a, e.utcMilliseconds = l, e.utcMinute = z, e.utcMinutes =
      G, e.utcMonday = W, e.utcMondays = J, e.utcMonth = re, e.utcMonths = ae, e.utcSaturday = Z, e.utcSaturdays = oe,
      e.utcSecond = m, e.utcSeconds = g, e.utcSunday = $, e.utcSundays = Q, e.utcThursday = q, e.utcThursdays = ne, e
      .utcTuesday = j, e.utcTuesdays = ee, e.utcWednesday = K, e.utcWednesdays = te, e.utcWeek = $, e.utcWeeks = Q, e
      .utcYear = le, e.utcYears = se, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
