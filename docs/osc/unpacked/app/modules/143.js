// ─────────────────────────────────────────────────────────────
// APP MODULE 143
// role       : provider exceptionService
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.exceptionService = void 0;
  var o = n(8),
    r = i(o),
    a = n(2);
  n(144);
  var l = n(483),
    s = i(l),
    d = a.ngMainCommonModule.provider("exceptionService", function() {
      var e = !0;
      return {
        setEnabled: function(t) {
          e = t
        },
        $get: ["$log", "telemetryService", "feedbackEndpoints", "TELEMETRY_OSC_EVENT_NAMES", function(t, n, i, o) {
          function a(t, i) {
            if (d.info("OSC UnHandled Exception was caught"), e) {
              l.exception = t, l.cause = i || "Unknown";
              var a = {
                name: t.name,
                message: t.message,
                stackFrame: null
              };
              try {
                a.stackFrame = s.default.parse(t)
              } catch (e) {
                d.error("cannot generate stack trace from error object", e), a.stackFrame = t.toString()
              }
              l.exceptionstr = (0, r.default)(a), l.properties = t.stack ? t.stack.replace(/ at /g,
                  "<br>&nbsp;&nbsp;&nbsp;at ").replace(/([^\/]+):(\d+):(\d+)/g,
                  '<span class="common-window exception-standout">$1</span>: <span class="common-window exception-standout">$2</span>:$3'
                  ) + "<br>" : "", d.info("ExceptionStr: ", l.exceptionstr),
                d.info("Cause: ", l.cause), d.info("Properties: ", l.properties), n.push(o
                  .OSC_UNHANDLED_EXCEPTION, "ExceptionStr: '" + l.exceptionstr + "', Cause: " + l.cause), l
                .postFeedback(), d.debug("Exception stacktrace posted to automated feedback system")
            }
          }
          var l = this,
            d = t.getInstance("osc/exceptionService");
          return l.exception = null, l.cause = "", l.properties = "", l.exceptionstr = "", l.postFeedback =
            function() {
              var e = l.exception.toString(),
                e = l.exceptionstr;
              return i.postFeedback({}, {
                category: "FEEDBACK_AUTOMATIC_OSC_UI_EXCEPTION",
                message: e,
                email: "",
                relatedApplications: [],
                relatedFiles: []
              })
            }, {
              logException: a
            }
        }]
      }
    });
  t.exceptionService = d
}
