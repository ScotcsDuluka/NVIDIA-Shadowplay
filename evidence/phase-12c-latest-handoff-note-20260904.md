# Phase 12c — Latest-oriented video handoff

Changed the production CaptureSession video sink policy from DropNewest to DropOldest.

Reason: the CFR scheduler consumes frames in presentation-time order, while a full DropNewest queue can retain stale frames and reject newer desktop states. DropOldest keeps the bounded queue biased toward fresher source frames when the consumer falls behind.

This is an experiment to isolate visual freshness issues; mux/timeline logic is unchanged.

Validation target: build first, then run the real production console validation at 120 FPS/10s and inspect counters/results before making further architectural changes.

Implementation note: production sink is now DropOldest; no mux or timestamp code changed in this step.

Validation command will use the existing real-record console driver. Existing untracked project artifacts are intentionally left untouched.

Next validation is a 10-second real production recording after the policy change.

End of experimental handoff note.

