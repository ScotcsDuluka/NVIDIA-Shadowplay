// ─────────────────────────────────────────────────────────────
// APP MODULE 150
// role       : controller VirtualGridListController
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.virtualGridListController = void 0;
  var i = n(2),
    o = i.ngMainCommonModule.controller("VirtualGridListController", ["$element", "$scope", "eventAggregator",
      "COMMON_EVENTS",
      function(e, t, n, i) {
        function o() {
          return e[0].offsetWidth
        }

        function r(e, t) {
          var n = e || 1;
          return Math.floor(t / n)
        }

        function a(e, t) {
          var n = e || 1;
          return Math.ceil(t / n)
        }

        function l(e, t, n) {
          h[e] || (h[e] = []);
          var i, o, r = h[e];
          for (r.length = 0, i = 0; i < t; i++) {
            if (o = e * t + i, o >= n.getLength()) return r;
            r.push(n.getItemAtIndex(o))
          }
          return r
        }

        function s(e) {
          var t, n;
          if (e > h.length)
            for (n = e - h.length, t = 0; t < n; t++) h.push([]);
          h.length = e;
          for (t in h[h.length - 1]) h[h.length - 1][t] = null
        }

        function d(e, n) {
          var i, o, r = a(n, e.getLength());
          for (s(r), t.nvVirtualGridListItems = h, i = 0; i < r; i++) o = l(i, n, e), t.nvVirtualGridListItems[i] = o
        }

        function c(e, n) {
          var i = a(n, e.getLength());
          s(i), t.nvVirtualGridListItems = {
            getLength: function() {
              return i
            },
            getItemAtIndex: function(t) {
              return l(t, n, e)
            }
          }
        }

        function u(e) {
          var t = !1;
          e.columnCount && e.columnCount !== p.columnCount && (p.columnCount = e.columnCount, t = !0), e.itemWidth &&
            e.itemWidth !== p.itemWidth && (p.itemWidth = e.itemWidth), e.list && e.list !== p.list && (p.list = e
              .list, "[object Array]" === Object.prototype.toString.call(p.list) && (p.list = {
                getLength: function() {
                  return e.list.length
                },
                getItemAtIndex: function(t) {
                  return e.list[t]
                }
              }), t = !0), p.list.getLength() != p.listLength && (p.listLength = p.list.getLength(), t = !0);
          var n = r(p.itemWidth, o());
          n !== p.columnCount && (p.columnCount = n, t = !0), t && this.refreshFn(p.list, p.columnCount)
        }

        function f() {
          n.on(i.WINDOW_RESIZE, g)
        }

        function m() {
          n.off(i.WINDOW_RESIZE, g)
        }
        var g, p = {
            list: {
              getLength: function() {
                return 0
              },
              getItemAtIndex: angular.noop
            },
            columnCount: 0,
            itemWidth: 0,
            listLength: 0
          },
          h = [];
        t.nvVirtualGridListItems = {
          getLength: function() {
            return 0
          },
          getItemAtIndex: function(e) {
            return null
          }
        }, this.initialize = function(e) {
          this.refresh = u.bind({
            refreshFn: e ? c : d
          })
        }, this.refresh = angular.noop, this.resize = function(e) {
          var n = r(p.itemWidth, o());
          n !== p.columnCount && this.refresh({
            columnCount: n
          }), t.$apply()
        }, this.getRowCount = function() {
          return a(p.columnCount, p.list.getLength())
        }, g = this.resize.bind(this), t.$on("$destroy", m), f()
      }
    ]);
  t.virtualGridListController = o
}
