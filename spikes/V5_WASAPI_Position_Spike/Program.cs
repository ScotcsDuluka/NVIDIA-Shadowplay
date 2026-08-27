// Program.cs — V5 WASAPI Position Spike (PHASE 13.1)
//
// GOAL (per docs/PHASE-13-SHADOWPLAY-CLOCK.md, phase P13.1):
//   1. Prove per-packet (devicePosition, qpcPosition) is readable from a
//      direct WASAPI COM interop capture loop (loopback or mic).
//   2. Measure QPC <-> Stopwatch agreement (same time base? what drift?).
//   3. Empirically determine the UNIT of qpcPosition (QPC ticks vs 100ns)
//      because SDK docs are ambiguous — evidence beats documentation.
//   4. Produce CSV evidence + a summary the OWNER can eyeball in 30 seconds.
//
// v3 HARDENING (after "Out of memory." on OWNER's machine):
//   * every WASAPI call is PreserveSig + manual HRESULT check, so failures
//     print call name + hex code + decoded AUDCLNT_E_* (v2 let the CLR's
//     HRESULT map rename E_OUTOFMEMORY into a bare "Out of memory.",
//     hiding which call failed);
//   * Initialize runs a 3-step fallback ladder (100ms -> 0ms -> +NOPERSIST)
//     and logs every attempt's HRESULT;
//   * every init step prints as it happens, so a mid-init failure is
//     locatable from the console alone;
//   * the capture loop records which call broke it and STILL writes
//     partial evidence instead of dying;
//   * fixed v2 analysis bug: flag bit constants were shifted
//     (SILENT=0x1, DATA_DISCONTINUITY=0x4, TIMESTAMP_ERROR=0x2 —
//     v2 had them rotated, mislabeling counts and the verdict gate).
//
// v4 FIX (the real v2/v3 killer, exposed by v3's heartbeats):
//   the loop called ReleaseBuffer(0); a capture-side ReleaseBuffer must be
//   passed the frame count GetBuffer reported, otherwise the engine never
//   advances the read cursor. Result: GetNextPacketSize re-served the same
//   packet forever, the loop spun at ~2.4M iters/s, collected ~120M
//   duplicate rows in 60s, and WriteCsv's single giant StringBuilder (>2.1G
//   chars) threw the OutOfMemoryException. Heartbeats printed 21M/45M/70M
//   "packets" — physically impossible for a ~10ms engine period.
//   v4: ReleaseBuffer(frames) + duplicate-stamp tripwire + hard row cap +
//   streaming CSV writer (no giant string allocation, immune to size) +
//   heartbeat rate display so anomalies are visible at a glance.
//
// v5 ANALYSIS REWORK (driven by OWNER's real 60s evidence run, 6000 pkts):
//   the endpoint drift (first vs last packet) flagged the healthy run FAIL
//   at 5.49 ms. Full-series OLS (see scripts/analyze_v5_evidence.py) proved
//   the truth: clock skew +3.3 ppm, chunk-offset spread 0.27-0.55 ms — the
//   5.49 ms was arrival-phase noise on the two endpoint samples (the first
//   packet even carried the SILENT stream-start transient). v5 reports:
//   fit (OLS skew ppm, gate <200), stable (chunk offset spread, gate <5ms),
//   jitter (our poll residual p95, informational), driftEP (old metric,
//   informational). VERDICT now gates on skew + stability + tsErr + unit.
//
// v6 SILENCE TEST (OWNER question: "what if there is NO sound?"):
//   --silence-test splits the run into 3 equal phases: quiet -> sound ->
//   quiet, tags every row with its phase, and reports per-phase packet
//   counts plus a gap analysis (longest no-packet window, what happened
//   to devicePosition/qpcPosition across it, DISCONTINUITY flag on the
//   resume packet). This measures WHICH silence model the machine uses:
//   Model S (engine renders SILENT packets, timeline keeps advancing)
//   or Model I (endpoint idles, timeline freezes, resume stamped exactly
//   by qpcPosition -> the P13 gap formula fills it without guessing).
//   The gap analysis runs in EVERY mode — silence gaps are always evidence.
//   Also fixes a latent v5 compile bug: local 'n' was declared twice in
//   AnalyzeAndSummarize (CS0128) — the latency counter is now 'nLat'.
//
// This spike never touches audio SAMPLES — only stamps. Safe to run while
// music plays, and equally informative when the room is silent.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace V5_WASAPI_Position_Spike
{
    internal static class Program
    {
        // AUDCLNT_BUFFERFLAGS_* — verified against audioclient.h (v2 had
        // these rotated; THAT bug would have mislabeled the verdict).
        private const int FLAG_SILENT = 0x1;
        private const int FLAG_TIMESTAMP_ERROR = 0x2;
        private const int FLAG_DATA_DISCONTINUITY = 0x4;

        private struct Row
        {
            public long SwTicks;      // Stopwatch.GetTimestamp() at GetBuffer return
            public long QpcPosition;  // hardware stamp from WASAPI
            public long DevicePosition;
            public int Frames;
            public int Flags;
            public int Phase;         // silence-test phase (0 quiet / 1 sound / 2 quiet; 0 when disabled)
        }

        private sealed class Config
        {
            public int DurationSec = 60;
            public string Source = "loopback";   // loopback | mic
            public string Via = "interop";       // interop only (see FINDINGS)
            public string OutDir = "out";
            public bool SilenceTest = false;
        }

        private static int Main(string[] args)
        {
            // Top-level net: nothing may die with an ambiguous message again.
            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FATAL: " + ex.GetType().Name + ": " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 4;
            }
        }

        private static int Run(string[] args)
        {
            var cfg = new Config();
            for (int i = 0; i < args.Length; i++)
            {
                string k = args[i];
                if (k == "--silence-test") { cfg.SilenceTest = true; continue; }
                if (i + 1 >= args.Length) break;
                string v = args[i + 1];
                if (k == "--duration") cfg.DurationSec = int.Parse(v, CultureInfo.InvariantCulture);
                else if (k == "--source") cfg.Source = v.ToLowerInvariant();
                else if (k == "--via") cfg.Via = v.ToLowerInvariant();   // ignored; interop only
                else if (k == "--out") cfg.OutDir = v;
                i++; // consumed the value
            }

            Console.WriteLine("=== V5 WASAPI Position Spike (P13.1, v6) ===");
            Console.WriteLine("source=" + cfg.Source + " via=" + cfg.Via +
                              " duration=" + cfg.DurationSec + "s" +
                              (cfg.SilenceTest ? " SILENCE-TEST(quiet/sound/quiet)" : "") +
                              " stopwatchFreq=" + Stopwatch.Frequency);

            if (cfg.Via != "interop")
            {
                Console.Error.WriteLine(
                    "--via naudio was REMOVED: compile-verified that NAudio 2.2.1 " +
                    "cannot expose positions (wrapper drops them; raw interface is internal). " +
                    "See README 'Findings'. Running --via interop instead.");
            }

            List<Row> rows;
            string mixInfo;
            string initInfo;
            try
            {
                rows = CaptureViaInterop(cfg, out mixInfo, out initInfo);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CAPTURE FAILED: " + ex.Message);
                Console.Error.WriteLine("hint: close apps holding the device exclusively " +
                    "(ASIO, voice chat), switch default device, or test --source mic. " +
                    "If it persists, send the full console output above to the OWNER log.");
                return 3;
            }

            if (rows.Count < 10)
            {
                Console.Error.WriteLine("Too few packets (" + rows.Count + ") — " +
                    "PLAY SOME AUDIO during the run (loopback sees nothing when the " +
                    "endpoint is idle; in --silence-test that means the middle phase), " +
                    "then retry.");
                return 3;
            }

            Directory.CreateDirectory(cfg.OutDir);
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string csvPath = Path.Combine(cfg.OutDir, "position_log_" + stamp + ".csv");
            string sumPath = Path.Combine(cfg.OutDir, "summary_" + stamp + ".txt");
            WriteCsv(csvPath, rows);

            string summary = AnalyzeAndSummarize(cfg, rows, mixInfo, initInfo);
            File.WriteAllText(sumPath, summary);
            Console.WriteLine();
            Console.WriteLine(summary);
            Console.WriteLine("CSV     : " + Path.GetFullPath(csvPath));
            Console.WriteLine("Summary : " + Path.GetFullPath(sumPath));

            bool pass = summary.Contains("VERDICT: PASS", StringComparison.Ordinal);
            Console.WriteLine(pass ? "\nP13.1 GATE: MET — send both files to OWNER for review." :
                                     "\nP13.1 GATE: NOT MET — send both files to OWNER before P13.2.");
            return pass ? 0 : 2;
        }

        // ─────────────────────────────────────────────────────────────────
        // Capture path — direct interop only.
        //
        // ★ FINDING (P13.1, compile-verified on NAudio 2.2.1, .NET 8 SDK):
        //   1. AudioCaptureClient (high-level wrapper) GetBuffer has NO
        //      position overload — CS1501 on first build.
        //   2. NAudio.CoreAudioApi.Interfaces.IAudioCaptureClient is
        //      'internal' (CS0122) and AudioClient.GetService is not
        //      accessible (CS1061).
        //   => NAudio cannot hand over devicePosition/qpcPosition in any
        //      form. P13.2 WasapiPositionCapture ships the direct interop
        //      from WasapiDirectInterop.cs (~200 lines, zero deps).
        // ─────────────────────────────────────────────────────────────────
        private static List<Row> CaptureViaInterop(Config cfg, out string mixInfo, out string initInfo)
        {
            int flow = cfg.Source == "mic" ? WasapiDirectInterop.eCapture
                                           : WasapiDirectInterop.eRender;
            bool loopback = cfg.Source != "mic";

            var device = WasapiDirectInterop.GetDefaultDevice(flow);
            WasapiDirectInterop.Check(device.GetId(out string deviceId), "IMMDevice.GetId");
            Console.WriteLine("[init] device  : " + deviceId);

            var client = WasapiDirectInterop.ActivateAudioClient(device);
            Console.WriteLine("[init] AudioClient activated");

            WasapiDirectInterop.Check(client.GetMixFormat(out IntPtr fmtPtr), "IAudioClient.GetMixFormat");
            var fmt = (WasapiDirectInterop.WAVEFORMATEX)Marshal.PtrToStructure(
                fmtPtr, typeof(WasapiDirectInterop.WAVEFORMATEX));
            mixInfo = WasapiDirectInterop.FormatToString(fmt) +
                      (fmt.wFormatTag == 0xFFFE
                          ? " (EXTENSIBLE cbSize=" + fmt.cbSize + ")"
                          : "");
            Console.WriteLine("[init] mix     : " + mixInfo);

            // ── Initialize fallback ladder ───────────────────────────────
            // Different drivers are picky about buffer duration / flags in
            // loopback. Try the standard request first, then looser ones.
            // Every attempt's HRESULT is printed — no more silent mysteries.
            int[] flagOpts = loopback
                ? new[] { WasapiDirectInterop.AUDCLNT_STREAMFLAGS_LOOPBACK,
                          WasapiDirectInterop.AUDCLNT_STREAMFLAGS_LOOPBACK,
                          WasapiDirectInterop.AUDCLNT_STREAMFLAGS_LOOPBACK |
                          WasapiDirectInterop.AUDCLNT_STREAMFLAGS_NOPERSIST }
                : new[] { 0, 0, 0 };
            long[] durOpts = { 1_000_000, 0, 0 };              // 100ms, engine default, engine default
            string[] durName = { "100ms", "0 (engine default)", "0 (engine default)" };

            int hrInit = -1;
            int usedAttempt = -1;
            for (int attempt = 0; attempt < flagOpts.Length; attempt++)
            {
                Console.Write("[init] Initialize(shared, flags=0x" +
                              flagOpts[attempt].ToString("X") + ", duration=" +
                              durName[attempt] + ") -> ");
                hrInit = client.Initialize(0, flagOpts[attempt], durOpts[attempt], 0,
                                           fmtPtr, IntPtr.Zero);
                Console.WriteLine(WasapiDirectInterop.HrName(hrInit));
                if (hrInit >= 0) { usedAttempt = attempt; break; }
            }
            Marshal.FreeCoTaskMem(fmtPtr);   // engine copied what it needs (or refused)
            if (hrInit < 0)
            {
                throw new InvalidOperationException(
                    "IAudioClient.Initialize failed on ALL fallback attempts (last " +
                    WasapiDirectInterop.HrName(hrInit) + "). The Windows audio engine " +
                    "refused to create this capture stream.");
            }
            initInfo = "Initialize attempt #" + (usedAttempt + 1) +
                       " (flags=0x" + flagOpts[usedAttempt].ToString("X") +
                       ", duration=" + durName[usedAttempt] + ")";

            WasapiDirectInterop.Check(client.GetBufferSize(out uint bufferFrames),
                                      "IAudioClient.GetBufferSize");
            Console.WriteLine("[init] buffer  : " + bufferFrames + " frames");

            var capture = WasapiDirectInterop.GetCaptureClient(client);
            Console.WriteLine("[init] capture service OK");

            WasapiDirectInterop.Check(client.Start(), "IAudioClient.Start");
            int phaseSec = cfg.SilenceTest ? Math.Max(5, cfg.DurationSec / 3) : cfg.DurationSec;
            Console.WriteLine("[capture] started for " + cfg.DurationSec + "s — " +
                (cfg.SilenceTest
                    ? "phases: QUIET " + phaseSec + "s -> SOUND " + phaseSec +
                      "s -> QUIET " + phaseSec + "s"
                    : (loopback ? "PLAY SOME AUDIO (anything)" : "make some noise")));

            var rows = new List<Row>();
            long start = Stopwatch.GetTimestamp();
            long limit = (long)((double)cfg.DurationSec * Stopwatch.Frequency);
            int nextHeartbeat = Math.Min(10, cfg.DurationSec);
            string loopError = null;
            int dupRun = 0;
            int currentPhase = -1;
            // Sane ceiling: real shared-mode engines deliver ~100-500 pkt/s
            // (~10ms period). 10k/s sustained would already be pathological.
            long maxRows = (long)cfg.DurationSec * 10000 + 10000;

            while (true)
            {
                long now = Stopwatch.GetTimestamp();
                if (now - start >= limit) break;
                double elapsed = (now - start) / (double)Stopwatch.Frequency;
                if (elapsed >= nextHeartbeat)
                {
                    Console.WriteLine("[capture] " + nextHeartbeat + "s: " +
                                      rows.Count + " packets (" +
                                      (rows.Count / nextHeartbeat) + "/s)");
                    nextHeartbeat += 10;
                }

                // Silence-test phase tracking (v6): time-based, so banners
                // print even when NO packets arrive (that is the point).
                int ph = cfg.SilenceTest ? Math.Min(2, (int)(elapsed / phaseSec)) : 0;
                if (ph != currentPhase)
                {
                    currentPhase = ph;
                    if (cfg.SilenceTest)
                    {
                        string what = ph == 0 ? "STAY SILENT — close anything playing audio"
                                    : ph == 1 ? "PLAY AUDIO NOW (keep it running)"
                                    : "SILENT AGAIN — stop the audio";
                        Console.WriteLine("[phase] ===== " + (ph * phaseSec) + "-" +
                                          ((ph + 1) * phaseSec) + "s: " + what + " =====");
                    }
                }

                if (rows.Count >= maxRows)
                {
                    loopError = "hard row cap (" + maxRows + ") hit — implausible " +
                                "packet rate; breaking to protect memory";
                    break;
                }

                int hrN = capture.GetNextPacketSize(out int pending);
                if (hrN < 0) { loopError = "GetNextPacketSize: " + WasapiDirectInterop.HrName(hrN); break; }
                if (pending < 0)
                { loopError = "GetNextPacketSize returned negative frames: " + pending; break; }
                if (pending == 0) { Thread.Sleep(5); continue; }

                int hrG = capture.GetBuffer(out IntPtr data, out int frames, out int fl,
                                            out long devPos, out long qpcPos);
                if (hrG < 0) { loopError = "GetBuffer: " + WasapiDirectInterop.HrName(hrG); break; }
                long swNow = Stopwatch.GetTimestamp();

                // v4 THE FIX: report the frames we consumed. Passing 0 (v3)
                // wedged the engine's read cursor and spun this loop forever.
                int hrR = capture.ReleaseBuffer(frames);
                if (hrR < 0)
                {
                    loopError = "ReleaseBuffer(" + frames + "): " +
                                WasapiDirectInterop.HrName(hrR);
                    break;
                }

                // Runaway tripwire: identical consecutive stamps mean the read
                // cursor is not advancing (the v3 pathology). Distinct packets
                // always have increasing devicePosition — 1000 identical in a
                // row is never legitimate.
                if (rows.Count > 0 &&
                    qpcPos == rows[rows.Count - 1].QpcPosition &&
                    devPos == rows[rows.Count - 1].DevicePosition)
                {
                    dupRun++;
                    if (dupRun >= 1000)
                    {
                        loopError = "runaway capture loop: " + dupRun +
                                    " consecutive identical stamps (read cursor not " +
                                    "advancing); breaking to protect memory";
                        break;
                    }
                }
                else dupRun = 0;

                rows.Add(new Row
                {
                    SwTicks = swNow,
                    QpcPosition = qpcPos,
                    DevicePosition = devPos,
                    Frames = frames,
                    Flags = fl,
                    Phase = currentPhase
                });
            }

            int hrStop = client.Stop();
            if (hrStop < 0) Console.WriteLine("[capture] warn: Stop -> " +
                                              WasapiDirectInterop.HrName(hrStop));
            if (loopError != null)
                Console.WriteLine("[capture] stopped early: " + loopError +
                                  " — " + rows.Count + " packets collected (kept as partial evidence)");
            return rows;
        }

        // ─────────────────────────────────────────────────────────────────

        // Streaming writer: one line at a time, no giant in-memory string.
        // (v3 built the whole CSV in a StringBuilder — with a runaway loop
        // that string exceeded StringBuilder's ~2.1G-char limit => OOM.)
        private static void WriteCsv(string path, List<Row> rows)
        {
            using (var w = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                w.WriteLine("index,sw_ticks,qpc_position,device_position,frames,flags,phase");
                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    w.WriteLine(string.Concat(
                        i.ToString(CultureInfo.InvariantCulture), ",",
                        r.SwTicks.ToString(CultureInfo.InvariantCulture), ",",
                        r.QpcPosition.ToString(CultureInfo.InvariantCulture), ",",
                        r.DevicePosition.ToString(CultureInfo.InvariantCulture), ",",
                        r.Frames.ToString(CultureInfo.InvariantCulture), ",",
                        r.Flags.ToString(CultureInfo.InvariantCulture), ",",
                        r.Phase.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        private static string AnalyzeAndSummarize(Config cfg, List<Row> rows,
                                                  string mixInfo, string initInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== V5 SUMMARY ===");
            sb.AppendLine("config  : source=" + cfg.Source + " via=" + cfg.Via +
                          " duration=" + cfg.DurationSec + "s packets=" + rows.Count +
                          (cfg.SilenceTest ? " SILENCE-TEST" : ""));
            sb.AppendLine("init    : " + initInfo);
            sb.AppendLine("mix     : " + mixInfo);
            sb.AppendLine("freq    : Stopwatch.Frequency=" + Stopwatch.Frequency +
                          " (100ns domain=10000000)");

            var first = rows[0];
            var last = rows[rows.Count - 1];

            // ── 1. qpcPosition UNIT detection ────────────────────────────
            // rate = Δqpc / Δstopwatch. ~1.0  => qpc is in QPC ticks.
            //        ~1e7/Frequency => qpc is in 100-ns units.
            double dSw = (double)(last.SwTicks - first.SwTicks);
            double dQpc = (double)(last.QpcPosition - first.QpcPosition);
            double rate = dQpc / dSw;
            double rateAs100ns = rate * Stopwatch.Frequency / 10000000.0;
            string unit;
            if (Math.Abs(rate - 1.0) < 0.02) unit = "QPC ticks (same unit as Stopwatch)";
            else if (Math.Abs(rateAs100ns - 1.0) < 0.02) unit = "100-nanosecond units";
            else unit = "UNRECOGNIZED — inspect CSV (rate=" +
                        rate.ToString("0.0000", CultureInfo.InvariantCulture) + ")";
            sb.AppendLine("unit    : qpcPosition -> " + unit);
            sb.AppendLine("          delta qpc / delta sw = " +
                          rate.ToString("0.000000", CultureInfo.InvariantCulture));

            // Convert qpc into the Stopwatch-tick domain for remaining math.
            double qpcToSw = unit.StartsWith("100", StringComparison.Ordinal)
                ? (double)Stopwatch.Frequency / 10000000.0
                : 1.0;

            // ── 2. devicePosition UNIT check (frames?) ───────────────────
            double dDev = (double)(last.DevicePosition - first.DevicePosition);
            double framesTotal = 0;
            for (int i = 1; i < rows.Count; i++) framesTotal += rows[i].Frames;
            sb.AppendLine("devpos  : delta devicePosition / sum frames = " +
                (framesTotal > 0
                    ? (dDev / framesTotal).ToString("0.000000", CultureInfo.InvariantCulture)
                    : "n/a") +
                " (expected ~1.0 if devicePosition is in sample FRAMES)");

            // ── 3. Read latency: hardware stamp vs arrival ───────────────
            // How far behind hardware time are we reading? (packet end -> now)
            // (v6: counter renamed nLat — v5 had 'int n' here AND in §4 => CS0128.)
            double sumMs = 0; int nLat = 0, late = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                double ticksPerFrame = SwTicksPerFrame(rows, i);
                double hwEndSw = rows[i].QpcPosition * qpcToSw +
                                 rows[i].Frames * ticksPerFrame;
                double lagMs = (rows[i].SwTicks - hwEndSw) * 1000.0 / Stopwatch.Frequency;
                if (lagMs > -50 && lagMs < 5000)
                {
                    sumMs += lagMs; nLat++;
                    if (lagMs > 40) late++;
                }
            }
            sb.AppendLine("latency : mean read-lag behind hardware stamp = " +
                (nLat > 0 ? (sumMs / nLat).ToString("0.00", CultureInfo.InvariantCulture) : "n/a") +
                " ms  (packets >40ms late: " + late + "/" + nLat + ")");
            sb.AppendLine("          (read-lag is read-loop latency, NOT clock drift)");

            // ── 4. Clock relationship — regression over ALL samples (v5) ──
            // Replaces the v4 endpoint drift: both endpoints carry 0-10 ms
            // arrival jitter from our 5 ms polling, so the endpoint metric
            // flagged the healthy P13.1 evidence run FAIL at 5.49 ms. OLS
            // over the full series measures the clocks directly; per-chunk
            // offsets measure STABILITY — the number P13.4 actually consumes.
            double hwElapsedSwTicks = (last.QpcPosition - first.QpcPosition) * qpcToSw;
            double swElapsedTicks = (double)(last.SwTicks - first.SwTicks);
            double driftEndpointMs = (swElapsedTicks - hwElapsedSwTicks) * 1000.0
                                     / Stopwatch.Frequency;

            int n = rows.Count;
            var qpcSw = new double[n];
            var swT = new double[n];
            for (int i = 0; i < n; i++)
            {
                qpcSw[i] = rows[i].QpcPosition * qpcToSw;
                swT[i] = rows[i].SwTicks;
            }
            double mq = 0, ms = 0;
            for (int i = 0; i < n; i++) { mq += qpcSw[i]; ms += swT[i]; }
            mq /= n; ms /= n;
            double cov = 0, varQ = 0;
            for (int i = 0; i < n; i++)
            {
                double dx = qpcSw[i] - mq, dy = swT[i] - ms;
                cov += dx * dy; varQ += dx * dx;
            }
            double slope = cov / varQ;                 // ≈ 1.0 + crystal skew
            double intercept = ms - slope * mq;
            double skewPpm = (slope - 1.0) * 1e6;
            var residMs = new double[n];
            for (int i = 0; i < n; i++)
                residMs[i] = (swT[i] - (slope * qpcSw[i] + intercept)) * 1000.0
                             / Stopwatch.Frequency;
            var absRes = (double[])residMs.Clone();
            Array.Sort(absRes);
            double jitterP95 = absRes[Math.Min(n - 1, (int)(0.95 * n))];
            int chunks = Math.Max(3, Math.Min(10, cfg.DurationSec / 10));
            double offMin = double.MaxValue, offMax = double.MinValue;
            for (int c = 0; c < chunks; c++)
            {
                int lo = c * n / chunks, hi = (c + 1) * n / chunks;
                double s = 0;
                for (int i = lo; i < hi; i++) s += residMs[i];
                double o = s / (hi - lo);
                if (o < offMin) offMin = o;
                if (o > offMax) offMax = o;
            }
            double chunkSpreadMs = offMax - offMin;

            sb.AppendLine("fit     : OLS over " + n + " packets — clock skew = " +
                skewPpm.ToString("+0.0;-0.0", CultureInfo.InvariantCulture) + " ppm" +
                "  (gate: |skew| < 200 ppm)");
            sb.AppendLine("stable  : " + chunks + "-chunk offset spread = " +
                chunkSpreadMs.ToString("0.00", CultureInfo.InvariantCulture) + " ms" +
                "  (gate: < 5.00 ms) — the P13.4 anchoring error budget");
            sb.AppendLine("jitter  : arrival residual p95 = " +
                jitterP95.ToString("0.00", CultureInfo.InvariantCulture) + " ms" +
                " — OUR 5 ms-poll jitter, not stamp noise (informational)");
            sb.AppendLine("driftEP : crude endpoint drift = " +
                driftEndpointMs.ToString("+0.00;-0.00", CultureInfo.InvariantCulture) +
                " ms (v4 metric, arrival-phase sensitive — informational only)");

            // ── 5. Health flags (constants verified against audioclient.h) ─
            int silent = 0, discontinuous = 0, timestampErr = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if ((rows[i].Flags & FLAG_SILENT) != 0) silent++;
                if ((rows[i].Flags & FLAG_DATA_DISCONTINUITY) != 0) discontinuous++;
                if ((rows[i].Flags & FLAG_TIMESTAMP_ERROR) != 0) timestampErr++;
            }
            sb.AppendLine("flags   : silent=" + silent + " discontinuity=" + discontinuous +
                          " timestampError=" + timestampErr +
                          "  (SILENT=0x1 TSERR=0x2 DISCONT=0x4)");

            // ── 6. Gaps & silence model (v6) ─────────────────────────────
            // Longest stretch with NO packets, and what the stamps say
            // happened across it. This is how we learn the machine's
            // silence behavior instead of believing the docs.
            long maxGapSw = 0; int maxGapAt = -1;
            for (int i = 1; i < rows.Count; i++)
            {
                long g = rows[i].SwTicks - rows[i - 1].SwTicks;
                if (g > maxGapSw) { maxGapSw = g; maxGapAt = i; }
            }
            double maxGapMs = maxGapSw * 1000.0 / Stopwatch.Frequency;
            if (maxGapAt > 0 && maxGapMs > 200)
            {
                var pkBefore = rows[maxGapAt - 1];
                var pkAfter = rows[maxGapAt];
                double gapSec = maxGapMs / 1000.0;
                long devJump = pkAfter.DevicePosition - pkBefore.DevicePosition;
                double fpsMeasured = swElapsedTicks > 0 && dDev != 0
                    ? dDev / (swElapsedTicks / Stopwatch.Frequency) : 48000.0;
                sb.AppendLine("gaps    : longest no-packet window = " +
                    maxGapMs.ToString("0", CultureInfo.InvariantCulture) +
                    " ms (packet " + (maxGapAt - 1) + " -> " + maxGapAt + ")");
                sb.AppendLine("          resume: devicePosition jumped " + devJump +
                    " frames (expect ~" + (long)(fpsMeasured * gapSec) +
                    " if the device kept counting through the gap)");
                sb.AppendLine("          resume flags: discont=" +
                    (((pkAfter.Flags & FLAG_DATA_DISCONTINUITY) != 0) ? "1" : "0") +
                    " silent=" + (((pkAfter.Flags & FLAG_SILENT) != 0) ? "1" : "0") +
                    " — qpcPosition pins the resume point exactly either way");
            }
            else
            {
                sb.AppendLine("gaps    : longest no-packet window = " +
                    maxGapMs.ToString("0", CultureInfo.InvariantCulture) + " ms — continuous");
            }

            if (cfg.SilenceTest)
            {
                int p0 = 0, p1 = 0, p2 = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i].Phase == 0) p0++;
                    else if (rows[i].Phase == 1) p1++;
                    else p2++;
                }
                sb.AppendLine("phases  : quiet=" + p0 + " pkts | sound=" + p1 +
                              " pkts | quiet=" + p2 + " pkts");
                if (p0 > 100 && p2 > 100)
                    sb.AppendLine("silence : Model S — engine rendered SILENT packets while quiet " +
                                  "(timeline advanced on its own); emit zeros per packet, " +
                                  "SilenceKeepAlive not required");
                else if (p0 < 50 && p2 < 50)
                    sb.AppendLine("silence : Model I — endpoint idled while quiet (timeline froze); " +
                                  "the P13 gap formula fills from qpcPosition exactly — keep " +
                                  "SilenceKeepAlive OR handle gaps via the formula");
                else
                    sb.AppendLine("silence : MIXED — inspect the gaps above");
            }

            bool pass = Math.Abs(skewPpm) < 200.0 && chunkSpreadMs < 5.0 &&
                        timestampErr == 0 &&
                        !unit.StartsWith("UNRECOGNIZED", StringComparison.Ordinal);
            sb.AppendLine("VERDICT: " + (pass ? "PASS" : "FAIL") +
                          " — " + (pass
                              ? "hardware stamps usable as the single master clock (P13.1 gate met)"
                              : "do NOT proceed to P13.2 before OWNER reviews this summary"));
            return sb.ToString();
        }

        // Stopwatch ticks per audio frame, estimated from consecutive
        // devicePosition deltas (works regardless of declared mix format).
        private static double SwTicksPerFrame(List<Row> rows, int i)
        {
            for (int k = i; k > 0 && k > i - 25; k--)
            {
                long dDev = rows[k].DevicePosition - rows[k - 1].DevicePosition;
                long dSw = rows[k].SwTicks - rows[k - 1].SwTicks;
                if (dDev > 0 && dSw > 0) return (double)dSw / dDev;
            }
            // fallback: nominal 48kHz — only used when no delta exists yet
            return Stopwatch.Frequency / 48000.0;
        }
    }
}
