// WasapiPacket.cs — P13.2: one WASAPI capture packet with its hardware stamps.
//
// This is the currency of the P13 clock design: every packet carries the
// device's OWN stamp of when its content is playing, so downstream code
// (AudioTap v3 gap math, SyncMath v2 anchors) subtracts stamps instead of
// guessing with Stopwatch arrival times.
//
// Stamp units — normalized here, at the source:
//   WASAPI reports qpcPosition in QPC ticks (P13.1 evidence: slope vs .NET
//   Stopwatch = 1.000003..1.000006, i.e. the SAME counter domain). Stopwatch
//   ticks are only 100ns ticks when Stopwatch.Frequency == 10,000,000 (true
//   on OWNER's machine). To keep every downstream formula in the doc's
//   100ns domain on ANY machine, the capture class normalizes:
//       QpcPosition100ns = RawQpcTicks * 10^7 / Stopwatch.Frequency
//   (split division — the naive product overflows long at ~29 days of QPC).

using System;

namespace CaptureEngine.Audio.Wasapi
{
    /// <summary>AUDCLNT_BUFFERFLAGS bits (audioclient.h — verified in P13.1).</summary>
    public static class WasapiPacketFlags
    {
        /// <summary>Packet content is all silence (stream start typically).
        /// P13.1 evidence: quiet-phase packets are genuine zero buffers
        /// WITHOUT this flag — never key silence math off it.</summary>
        public const int Discontinuity = 0x1;
        /// <summary>Timestamps derived from a known-error source.</summary>
        public const int Silent = 0x2;
        /// <summary>Packet not contiguous with the previous one.</summary>
        public const int TimestampError = 0x4;
    }

    /// <summary>One captured WASAPI packet: PCM bytes + hardware stamps.</summary>
    public struct WasapiPacket
    {
        /// <summary>Hardware stamp of the packet's FIRST frame, in 100ns
        /// units (normalized from raw QPC ticks — see file header).</summary>
        public long QpcPosition100ns;

        /// <summary>The stamp exactly as WASAPI reported it (QPC ticks).
        /// Diagnostics only — engine math uses QpcPosition100ns.</summary>
        public long RawQpcTicks;

        /// <summary>Device sample-clock position of the first frame, in
        /// sample FRAMES since stream start (P13.1: ratio 1.000000 vs
        /// summed packet frames).</summary>
        public long DevicePositionFrames;

        /// <summary>Frames in this packet (P13.1 evidence: 480 = 10ms @ 48kHz
        /// on OWNER's machine, 100% of packets).</summary>
        public int Frames;

        /// <summary>AUDCLNT_BUFFERFLAGS bitmask (see WasapiPacketFlags).</summary>
        public int Flags;

        /// <summary>PCM copy of the packet in the device MIX FORMAT
        /// (float32 stereo on typical machines — see
        /// WasapiPositionCapture.MixFormatInfo). Null when
        /// WasapiCaptureOptions.IncludePcm == false.</summary>
        public byte[] Data;
    }
}

