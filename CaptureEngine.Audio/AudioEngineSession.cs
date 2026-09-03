using System;
using System.Collections.Generic;
using System.Threading;
using CaptureEngine.Audio.Wasapi;

namespace CaptureEngine.Audio
{
    public sealed class AudioEngineSession : IDisposable
    {
        private sealed class TrackRuntime : IDisposable
        {
            public readonly AudioTrackKind Kind;
            public readonly WasapiPositionCapture Capture;
            public readonly AudioPositionTracker Tracker;
            public readonly List<IAudioSink> Sinks = new List<IAudioSink>();
            public int OutputChannels;
            public int OutputBits = 16;
            public long DataBytes;
            public long SilenceBytes;
            public long LastEnd100ns;
            public long FirstQpc100ns;
            public bool Started;

            public TrackRuntime(AudioTrackKind kind, WasapiPositionCapture capture)
            {
                Kind = kind;
                Capture = capture;
                Tracker = new AudioPositionTracker(capture.SampleRate);
                OutputChannels = Math.Max(1, capture.Channels);
            }

            public void Dispose()
            {
                try { Capture.Dispose(); } catch { }
            }
        }

        private readonly AudioEngineConfig _config;
        private readonly Action<string> _log;
        private readonly object _sync = new object();
        private readonly List<TrackRuntime> _tracks = new List<TrackRuntime>();
        private readonly Dictionary<AudioTrackKind, List<IAudioSink>> _pendingSinks = new Dictionary<AudioTrackKind, List<IAudioSink>>();
        private AudioEngineDiagnostics _diagnostics = new AudioEngineDiagnostics();
        private long _sessionStartQpc100ns;
        private long _videoStartQpc100ns;
        private bool _started;
        private bool _stopped;

        public bool IsRunning => _started && !_stopped;
        public long SessionStartQpc100ns => Interlocked.Read(ref _sessionStartQpc100ns);
        public long VideoStartQpc100ns => Interlocked.Read(ref _videoStartQpc100ns);
        public AudioEngineDiagnostics Diagnostics => _diagnostics;

        public bool TryGetTrackFormat(AudioTrackKind kind, out int sampleRate, out int channels)
        {
            lock (_sync)
            {
                foreach (var t in _tracks)
                {
                    if (t.Kind == kind)
                    {
                        sampleRate = t.Capture.SampleRate;
                        channels = t.OutputChannels;
                        return true;
                    }
                }
            }
            sampleRate = 0;
            channels = 0;
            return false;
        }

        public AudioEngineSession(AudioEngineConfig config, Action<string> log = null)
        {
            _config = config ?? new AudioEngineConfig();
            _log = log;
        }

        public void AddSink(AudioTrackKind track, IAudioSink sink)
        {
            if (sink == null) throw new ArgumentNullException(nameof(sink));
            lock (_sync)
            {
                foreach (var t in _tracks)
                {
                    if (t.Kind == track)
                    {
                        t.Sinks.Add(sink);
                        return;
                    }
                }
                if (!_pendingSinks.TryGetValue(track, out var list))
                {
                    list = new List<IAudioSink>();
                    _pendingSinks[track] = list;
                }
                list.Add(sink);
            }
        }

        public void Start()
        {
            lock (_sync)
            {
                if (_started) return;
                _sessionStartQpc100ns = WasapiPositionCapture.QpcTicksTo100ns(System.Diagnostics.Stopwatch.GetTimestamp());
                _diagnostics.SessionStartQpc100ns = _sessionStartQpc100ns;

                if (_config.SystemEnabled) TryStartTrack(AudioTrackKind.System, true);
                if (_config.MicrophoneEnabled) TryStartTrack(AudioTrackKind.Microphone, false);
                _started = true;
                _stopped = false;
            }
            Log($"[AudioEngine] started: system={_config.SystemEnabled} mic={_config.MicrophoneEnabled}");
        }

        public void SetVideoStartQpc100ns(long videoStartQpc100ns)
        {
            if (videoStartQpc100ns <= 0) return;
            Interlocked.Exchange(ref _videoStartQpc100ns, videoStartQpc100ns);
            _diagnostics.VideoStartQpc100ns = videoStartQpc100ns;
            lock (_sync)
            {
                long delta = videoStartQpc100ns - _sessionStartQpc100ns;
                _diagnostics.VideoAlignmentOffset100ns = delta;
                _diagnostics.VideoAligned = true;
            }
        }

        public void Stop(long sessionEndQpc100ns = 0)
        {
            lock (_sync)
            {
                if (!_started || _stopped) return;
                if (sessionEndQpc100ns <= 0)
                    sessionEndQpc100ns = WasapiPositionCapture.QpcTicksTo100ns(System.Diagnostics.Stopwatch.GetTimestamp());
                foreach (var t in _tracks)
                {
                    try { t.Capture.Stop(); } catch { }
                    FinalizeTrack(t, sessionEndQpc100ns);
                }
                _stopped = true;
                RebuildDiagnostics();
            }
            Log(_diagnostics.ToString());
        }

        private void TryStartTrack(AudioTrackKind kind, bool loopback)
        {
            try
            {
                var options = new WasapiCaptureOptions
                {
                    Loopback = loopback,
                    DeviceId = loopback ? "" : (_config.MicrophoneDeviceId ?? ""),
                    IncludePcm = true,
                    PollIntervalMs = _config.PollIntervalMs,
                    BufferDuration100ns = _config.BufferDuration100ns
                };
                var capture = new WasapiPositionCapture(options);
                var runtime = new TrackRuntime(kind, capture);
                capture.PacketReady += packet => OnPacket(runtime, packet);
                capture.StoppedWithError += error => Log($"[AudioEngine] {kind} capture error: {error}");
                if (_pendingSinks.TryGetValue(kind, out var list))
                {
                    runtime.Sinks.AddRange(list);
                    _pendingSinks.Remove(kind);
                }
                _tracks.Add(runtime);
                capture.Start();
                Log($"[AudioEngine] {kind} endpoint started: {capture.DeviceId} {capture.SampleRate}Hz/{capture.Channels}ch");
            }
            catch (Exception ex)
            {
                Log($"[AudioEngine] {kind} endpoint FAILED (track disabled): {ex.Message}");
            }
        }

        private void OnPacket(TrackRuntime t, WasapiPacket packet)
        {
            if (packet.Data == null || packet.Frames <= 0) return;
            byte[] pcm = AudioPcm16.Convert(packet.Data, packet.Data.Length,
                                           Math.Max(1, t.Capture.Channels), t.Capture.BitsPerSample);
            int frames = pcm.Length / Math.Max(1, t.OutputChannels * 2);
            if (frames <= 0) return;

            var report = t.Tracker.Feed(frames, packet.DevicePositionFrames,
                                        packet.QpcPosition100ns, packet.Flags);
            long dataEnd = report.LastEnd100ns;
            long dataStart = dataEnd - report.BufferDur100ns;

            if (report.Hole100ns > 0)
            {
                long silenceFrames = report.Hole100ns * t.Capture.SampleRate / 10_000_000L;
                if (silenceFrames > 0)
                {
                    var silence = new byte[silenceFrames * t.OutputChannels * 2L];
                    var sp = new AudioPacket(t.Kind, dataStart - report.Hole100ns,
                                             checked((int)silenceFrames), silence, true,
                                             packet.QpcPosition100ns, packet.DevicePositionFrames, packet.Flags,
                                             t.Capture.SampleRate, t.OutputChannels);
                    Dispatch(t, sp);
                    t.SilenceBytes += silence.Length;
                }
            }

            var dp = new AudioPacket(t.Kind, dataStart, frames, pcm, false,
                                     packet.QpcPosition100ns, packet.DevicePositionFrames, packet.Flags,
                                     t.Capture.SampleRate, t.OutputChannels);
            Dispatch(t, dp);
            t.DataBytes += pcm.Length;
            t.LastEnd100ns = dataEnd;
            if (!t.Started)
            {
                t.Started = true;
                t.FirstQpc100ns = packet.QpcPosition100ns;
            }
        }

        private static void Dispatch(TrackRuntime t, in AudioPacket packet)
        {
            foreach (var sink in t.Sinks)
            {
                try { sink.Write(packet); } catch { }
            }
        }

        private void FinalizeTrack(TrackRuntime t, long sessionEndQpc100ns)
        {
            if (!t.Started)
            {
                long span = sessionEndQpc100ns - _sessionStartQpc100ns;
                if (span > 0)
                    DispatchSilence(t, _sessionStartQpc100ns, span);
                return;
            }

            long tail = sessionEndQpc100ns - t.LastEnd100ns;
            if (tail > 0) DispatchSilence(t, t.LastEnd100ns, tail);
        }

        private void DispatchSilence(TrackRuntime t, long pts100ns, long duration100ns)
        {
            long framesLong = duration100ns * t.Capture.SampleRate / 10_000_000L;
            if (framesLong <= 0) return;
            var bytes = new byte[checked((int)(framesLong * t.OutputChannels * 2L))];
            var packet = new AudioPacket(t.Kind, pts100ns, checked((int)framesLong), bytes, true, 0, 0, 0,
                                         t.Capture.SampleRate, t.OutputChannels);
            Dispatch(t, packet);
            t.SilenceBytes += bytes.Length;
            t.LastEnd100ns = pts100ns + duration100ns;
        }

        private void RebuildDiagnostics()
        {
            _diagnostics = new AudioEngineDiagnostics
            {
                SessionStartQpc100ns = _sessionStartQpc100ns,
                VideoStartQpc100ns = _videoStartQpc100ns,
                VideoAlignmentOffset100ns = _videoStartQpc100ns - _sessionStartQpc100ns,
                VideoAligned = _videoStartQpc100ns > 0
            };
            foreach (var t in _tracks)
            {
                _diagnostics.Tracks.Add(new AudioTrackDiagnostics
                {
                    Track = t.Kind,
                    DeviceId = t.Capture.DeviceId,
                    SampleRate = t.Capture.SampleRate,
                    Channels = t.OutputChannels,
                    Frames = t.Tracker.Frames,
                    Packets = t.Tracker.Packets,
                    DataBytes = t.DataBytes,
                    SilenceBytes = t.SilenceBytes,
                    FirstQpc100ns = t.FirstQpc100ns,
                    LastEnd100ns = t.LastEnd100ns,
                    GapPackets = t.Tracker.GapPackets,
                    IdleGapPackets = t.Tracker.IdleGapPackets,
                    QpcAnomalies = t.Tracker.QpcAnomalies,
                    CursorViolations = t.Tracker.MonotonicViolations,
                    TimestampErrorPackets = t.Tracker.TimestampErrorPackets
                });
            }
        }

        private void Log(string message) { try { _log?.Invoke(message); } catch { } }

        public void Dispose() { try { Stop(); } catch { } foreach (var t in _tracks) t.Dispose(); }
    }
}
