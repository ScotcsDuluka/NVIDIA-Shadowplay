// Program.cs — V5 WASAPI Position Spike (PHASE 13.1)
//
// GOAL (per docs/PHASE-13-SHADOWPLAY-CLOCK.md, phase P13.1):
//   1. Prove per-packet (devicePosition, qpcPosition) is readable for BOTH
//      capture paths (loopback + mic), via two transports:
//        --via interop (default) : direct COM declarations in this spike
//        --via naudio            : NAudio 2.2.1 AudioCaptureClient wrapper
//   2. Measure QPC <-> Stopwatch agreement (same time base? what drift?).
//   3. Empirically determine the UNIT of qpcPosition (QPC ticks vs 100ns)
//      because SDK docs are ambiguous — evidence beats documentation.
//   4. Produce CSV evidence + a summary the OWNER can eyeball in 30 seconds.
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
            public string Via = "interop";       // interop | naudio
            public string OutDir = "out";
        }

        private static int Main(string[] args)
        {
            var cfg = new Config();
            for (int i = 0; i + 1 < args.Length; i += 2)
            {
                string k = args[i], v = args[i + 1];
                if (k == "--duration") cfg.DurationSec = int.Parse(v, CultureInfo.InvariantCulture);
                else if (k == "--source") cfg.Source = v.ToLowerInvariant();
                else if (k == "--via") cfg.Via = v.ToLowerInvariant();
                else if (k == "--out") cfg.OutDir = v;
            }

            Console.WriteLine("=== V5 WASAPI Position Spike (P13.1) ===");
            Console.WriteLine("source=" + cfg.Source + " via=" + cfg.Via +
                              " duration=" + cfg.DurationSec + "s" +
                              " stopwatchFreq=" + Stopwatch.Frequency);

            List<Row> rows;
            string mixInfo;
            if (cfg.Via == "naudio")
            {
                Console.Error.WriteLine(
                    "--via naudio was REMOVED: compile-verified that NAudio 2.2.1 " +
                    "cannot expose positions (wrapper drops them; raw interface is internal). " +
                    "See README 'Findings'. Running --via interop instead.");
                cfg.Via = "interop";
            }
            try
            {
                rows = CaptureViaInterop(cfg, out mixInfo);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CAPTURE FAILED: " + ex.Message);
                return 3;
            }

            if (rows.Count < 10)
            {
                Console.Error.WriteLine("Too few packets (" + rows.Count + ") — play some audio and retry.");
                return 3;
            }

            Directory.CreateDirectory(cfg.OutDir);
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string csvPath = Path.Combine(cfg.OutDir, "position_log_" + stamp + ".csv");
            string sumPath = Path.Combine(cfg.OutDir, "summary_" + stamp + ".txt");
            WriteCsv(csvPath, rows);

            string summary = AnalyzeAndSummarize(cfg, rows, mixInfo);
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
        //      from WasapiDirectInterop.cs (~150 lines, zero deps).
        // ─────────────────────────────────────────────────────────────────
        private static List<Row> CaptureViaInterop(Config cfg, out string mixInfo)
        {
            int flow = cfg.Source == "mic" ? WasapiDirectInterop.eCapture
                                           : WasapiDirectInterop.eRender;
            int flags = cfg.Source == "mic" ? 0
                                            : WasapiDirectInterop.AUDCLNT_STREAMFLAGS_LOOPBACK;

            var device = WasapiDirectInterop.GetDefaultDevice(flow);
            string deviceId;
            device.GetId(out deviceId);
            Console.WriteLine("device : " + deviceId);

            var client = WasapiDirectInterop.ActivateAudioClient(device);
            IntPtr fmtPtr;
            client.GetMixFormat(out fmtPtr);
            var fmt = (WasapiDirectInterop.WAVEFORMATEX)Marshal.PtrToStructure(
                fmtPtr, typeof(WasapiDirectInterop.WAVEFORMATEX));
            mixInfo = WasapiDirectInterop.FormatToString(fmt) +
                      (fmt.wFormatTag == 0xFFFE
                          ? " (EXTENSIBLE cbSize=" + fmt.cbSize + ")"
                          : "");
            Console.WriteLine("mix    : " + mixInfo);

            Guid empty = Guid.Empty;
            // Pass the RAW mix-format pointer — the format is EXTENSIBLE on
            // every modern machine; a struct copy would be undersized and
            // Initialize would return E_INVALIDARG. Free only AFTER Initialize.
            client.Initialize(0 /* shared */, flags, 1_000_000 /* 100ms in 100ns */, 0,
                              fmtPtr, ref empty);
            Marshal.FreeCoTaskMem(fmtPtr);
            var capture = WasapiDirectInterop.GetCaptureClient(client);

            var rows = new List<Row>();
            client.Start();
            long start = Stopwatch.GetTimestamp();
            long limit = (long)((double)cfg.DurationSec * Stopwatch.Frequency);
            while (Stopwatch.GetTimestamp() - start < limit)
            {
                int pending;
                capture.GetNextPacketSize(out pending);
                if (pending == 0) { Thread.Sleep(5); continue; }

                IntPtr data; int frames; int fl; long devPos, qpcPos;
                capture.GetBuffer(out data, out frames, out fl, out devPos, out qpcPos);
                var row = new Row
                {
                    SwTicks = Stopwatch.GetTimestamp(),
                    QpcPosition = qpcPos,
                    DevicePosition = devPos,
                    Frames = frames,
                    Flags = fl
                };
                capture.ReleaseBuffer(0);
                rows.Add(row);
            }
            client.Stop();
            return rows;
        }

        // ─────────────────────────────────────────────────────────────────

        private static void WriteCsv(string path, List<Row> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("index,sw_ticks,qpc_position,device_position,frames,flags");
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                sb.Append(i.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.SwTicks.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.QpcPosition.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.DevicePosition.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.Frames.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.Flags.ToString(CultureInfo.InvariantCulture)).Append('\n');
            }
            File.WriteAllText(path, sb.ToString());
        }

        private static string AnalyzeAndSummarize(Config cfg, List<Row> rows, string mixInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== V5 SUMMARY ===");
            sb.AppendLine("config  : source=" + cfg.Source + " via=" + cfg.Via +
                          " duration=" + cfg.DurationSec + "s packets=" + rows.Count);
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

            // ── 5. Health flags ──────────────────────────────────────────
            int silent = 0, discontinuous = 0, timestampErr = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if ((rows[i].Flags & 0x2) != 0) silent++;         // AUDCLNT_BUFFERFLAGS_SILENT
                if ((rows[i].Flags & 0x1) != 0) discontinuous++;  // AUDCLNT_BUFFERFLAGS_DATA_DISCONTINUITY
                if ((rows[i].Flags & 0x4) != 0) timestampErr++;   // AUDCLNT_BUFFERFLAGS_TIMESTAMP_ERROR
            }
            sb.AppendLine("flags   : silent=" + silent + " discontinuity=" + discontinuous +
                          " timestampError=" + timestampErr);

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
