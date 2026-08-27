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
        }

        private sealed class Config
        {
            public int DurationSec = 60;
            public string Source = "loopback";   // loopback | mic
            public string Via = "interop";       // interop only (see FINDINGS)
            public string OutDir = "out";
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
            for (int i = 0; i + 1 < args.Length; i += 2)
            {
                string k = args[i], v = args[i + 1];
                if (k == "--duration") cfg.DurationSec = int.Parse(v, CultureInfo.InvariantCulture);
                else if (k == "--source") cfg.Source = v.ToLowerInvariant();
                else if (k == "--via") cfg.Via = v.ToLowerInvariant();   // ignored; interop only
                else if (k == "--out") cfg.OutDir = v;
            }

            Console.WriteLine("=== V5 WASAPI Position Spike (P13.1, v4) ===");
            Console.WriteLine("source=" + cfg.Source + " via=" + cfg.Via +
                              " duration=" + cfg.DurationSec + "s" +
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
                    "endpoint is idle), then retry.");
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
            Console.WriteLine("[capture] started for " + cfg.DurationSec + "s — " +
                              (loopback ? "PLAY SOME AUDIO (anything)" : "make some noise"));

            var rows = new List<Row>();
            long start = Stopwatch.GetTimestamp();
            long limit = (long)((double)cfg.DurationSec * Stopwatch.Frequency);
            int nextHeartbeat = Math.Min(10, cfg.DurationSec);
            string loopError = null;
            int dupRun = 0;
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
                    Flags = fl
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
                w.WriteLine("index,sw_ticks,qpc_position,device_position,frames,flags");
                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    w.WriteLine(string.Concat(
                        i.ToString(CultureInfo.InvariantCulture), ",",
                        r.SwTicks.ToString(CultureInfo.InvariantCulture), ",",
                        r.QpcPosition.ToString(CultureInfo.InvariantCulture), ",",
                        r.DevicePosition.ToString(CultureInfo.InvariantCulture), ",",
                        r.Frames.ToString(CultureInfo.InvariantCulture), ",",
                        r.Flags.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        private static string AnalyzeAndSummarize(Config cfg, List<Row> rows,
                                                  string mixInfo, string initInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== V5 SUMMARY ===");
            sb.AppendLine("config  : source=" + cfg.Source + " via=" + cfg.Via +
                          " duration=" + cfg.DurationSec + "s packets=" + rows.Count);
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
            double sumMs = 0; int n = 0, late = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                double ticksPerFrame = SwTicksPerFrame(rows, i);
                double hwEndSw = rows[i].QpcPosition * qpcToSw +
                                 rows[i].Frames * ticksPerFrame;
                double lagMs = (rows[i].SwTicks - hwEndSw) * 1000.0 / Stopwatch.Frequency;
                if (lagMs > -50 && lagMs < 5000)
                {
                    sumMs += lagMs; n++;
                    if (lagMs > 40) late++;
                }
            }
            sb.AppendLine("latency : mean read-lag behind hardware stamp = " +
                (n > 0 ? (sumMs / n).ToString("0.00", CultureInfo.InvariantCulture) : "n/a") +
                " ms  (packets >40ms late: " + late + "/" + n + ")");
            sb.AppendLine("          (read-lag is read-loop latency, NOT clock drift)");

            // ── 4. Clock drift over the run (the P13.1 gate) ─────────────
            double hwElapsedSwTicks = (last.QpcPosition - first.QpcPosition) * qpcToSw;
            double swElapsedTicks = (double)(last.SwTicks - first.SwTicks);
            double driftMs = (swElapsedTicks - hwElapsedSwTicks) * 1000.0 / Stopwatch.Frequency;
            sb.AppendLine("drift   : Stopwatch vs hardware over run = " +
                driftMs.ToString("0.00", CultureInfo.InvariantCulture) + " ms" +
                "  (gate: |drift| < 5.00 ms)");

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

            bool pass = Math.Abs(driftMs) < 5.0 && timestampErr == 0 &&
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
