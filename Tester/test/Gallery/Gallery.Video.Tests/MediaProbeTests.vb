Option Strict On
Option Explicit On
Option Infer On

' MediaProbeTests.vb — REAL ffprobe on REAL synthetic files (Tier 2).
'
' OWNER RULE: never assume codecs — every claim below comes from what
' ffprobe says about files whose codecs we controlled at generation time.
' Synthetic generation mirrors the product matrix validated in HANDOFF §3
' (MP4 / H.264 / AAC / start_time≈0).

Imports System
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class MediaProbeTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("PROBE: av60 synthetic → h264/yuv420p/60fps + aac/48000/2ch",
                   AddressOf Test_Av60)
            runner("PROBE: real_shape synthetic 1680×1050 → matches pinned ShadowPlay format (§5)",
                   AddressOf Test_RealShape)
            runner("PROBE: wrong_codec synthetic mpeg4 → probed outside supported set",
                   AddressOf Test_WrongCodec)
            runner("PROBE: video_only synthetic → HasAudio=False (video-only mode basis)",
                   AddressOf Test_VideoOnly)
            runner("PROBE: audio_only synthetic → HasVideo=False (NoVideoStream basis)",
                   AddressOf Test_AudioOnly)
            runner("PROBE: corrupt/truncated → probe returns Nothing",
                   AddressOf Test_Corrupt)
            runner("PROBE: not-media bytes → probe returns Nothing",
                   AddressOf Test_NotMedia)
            runner("PROBE: missing file → probe returns Nothing",
                   AddressOf Test_Missing)
            runner("PROBE: parser tolerant to missing optional fields",
                   AddressOf Test_ParseTolerant)
        End Sub

        Private Shared Sub Test_Av60()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim info = MediaProbe.Probe(path, TestMedia.FfprobePath)

            TestRunner.Assert(info IsNot Nothing, "probe succeeded")
            TestRunner.Assert(info.HasVideo, "has video stream")
            TestRunner.Assert(info.HasAudio, "has audio stream")

            Dim v = info.Video
            TestRunner.AssertEqual("h264", v.CodecName, "video codec (probed, not assumed)")
            TestRunner.AssertEqual("yuv420p", v.PixFmt, "pixel format")
            TestRunner.AssertEqual(320, v.Width, "width")
            TestRunner.AssertEqual(240, v.Height, "height")

            Dim fps = MediaInfo.ParseFrameRate(v.AvgFrameRate)
            TestRunner.Assert(fps > 59.0 AndAlso fps < 61.0, $"fps ≈ 60 (got {fps})")

            Dim a = info.Audio
            TestRunner.AssertEqual("aac", a.CodecName, "audio codec")
            TestRunner.AssertEqual(48000, a.SampleRate, "sample rate")
            TestRunner.AssertEqual(2, a.Channels, "channels")

            TestRunner.Assert(info.Format.DurationSec > 3.5 AndAlso info.Format.DurationSec < 4.5,
                              $"duration ≈ 4s (got {info.Format.DurationSec})")
        End Sub

        Private Shared Sub Test_RealShape()
            ' §5 pin: every assertion mirrors the REAL owner-machine ffprobe of
            ' Record_2026-09-10_21-18-52.mp4 (2026-09-10): mov/mp4, H.264
            ' yuv420p 1680×1050 (16:10, both even), ~60 fps rational, AAC LC
            ' 48k stereo. The synthetic reproduces the SHAPE; the real file
            ' remains the format authority.
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_real_shape.mp4", "real_shape")
            Dim info = MediaProbe.Probe(path, TestMedia.FfprobePath)

            TestRunner.Assert(info IsNot Nothing, "probe succeeded")
            TestRunner.Assert(info.HasVideo AndAlso info.HasAudio, "both streams")

            Dim v = info.Video
            TestRunner.AssertEqual("h264", v.CodecName, "video codec = h264 (real: h264 High)")
            TestRunner.AssertEqual("yuv420p", v.PixFmt, "pix_fmt = yuv420p")
            TestRunner.AssertEqual(1680, v.Width, "width = 1680 (16:10)")
            TestRunner.AssertEqual(1050, v.Height, "height = 1050 (16:10)")
            TestRunner.Assert(v.Width Mod 2 = 0 AndAlso v.Height Mod 2 = 0,
                              "both dims even (yuv420p safe — real file also even)")

            Dim fps = MediaInfo.ParseFrameRate(v.AvgFrameRate)
            TestRunner.Assert(fps > 59.0 AndAlso fps < 61.0,
                              $"fps parses from rational ≈ 60 (got {fps}; real file avg ≈ 59.88 — parser accepts 'N/M' and decimals, PTS-driven clock is VFR-safe)")

            Dim a = info.Audio
            TestRunner.AssertEqual("aac", a.CodecName, "audio codec = aac")
            TestRunner.AssertEqual(48000, a.SampleRate, "sample rate = 48000")
            TestRunner.AssertEqual(2, a.Channels, "channels = 2")

            TestRunner.Assert(info.Format.FormatName.Contains("mp4"),
                              $"container mov/mp4 family (got {info.Format.FormatName})")
        End Sub

        Private Shared Sub Test_WrongCodec()
            ' Proves the probe layer reports a codec OUTSIDE the supported set
            ' (mpeg4), which the session maps to Faulted.UnsupportedFormat
            ' (§7 — asserted session-side in SessionTests.Test_FaultMapping).
            TestMedia.RequireBinaries()
            Dim info = MediaProbe.Probe(TestMedia.Synthetic("synth_wrong_codec.mp4", "wrong_codec"),
                                        TestMedia.FfprobePath)
            TestRunner.Assert(info IsNot Nothing, "probe parses (file is valid media)")
            TestRunner.AssertEqual("mpeg4", info.Video.CodecName, "probed codec = mpeg4")
            TestRunner.Assert(info.Video.CodecName <> "h264" OrElse info.Video.PixFmt <> "yuv420p",
                              "outside the session's supported set")
        End Sub

        Private Shared Sub Test_VideoOnly()
            TestMedia.RequireBinaries()
            Dim info = MediaProbe.Probe(TestMedia.Synthetic("synth_video_only.mp4", "video_only"), TestMedia.FfprobePath)
            TestRunner.Assert(info IsNot Nothing, "probe succeeded")
            TestRunner.Assert(info.HasVideo, "has video")
            TestRunner.Assert(Not info.HasAudio, "no audio → video-only playback mode")
        End Sub

        Private Shared Sub Test_AudioOnly()
            TestMedia.RequireBinaries()
            Dim info = MediaProbe.Probe(TestMedia.Synthetic("synth_audio_only.mp4", "audio_only"), TestMedia.FfprobePath)
            TestRunner.Assert(info IsNot Nothing, "probe succeeded")
            TestRunner.Assert(Not info.HasVideo, "no video stream")
            TestRunner.Assert(info.HasAudio, "has audio")
        End Sub

        Private Shared Sub Test_Corrupt()
            TestMedia.RequireBinaries()
            Dim info = MediaProbe.Probe(TestMedia.CorruptFile(), TestMedia.FfprobePath)
            TestRunner.Assert(info Is Nothing, "corrupt → Nothing (fault mapping upstream)")
        End Sub

        Private Shared Sub Test_NotMedia()
            TestMedia.RequireBinaries()
            Dim info = MediaProbe.Probe(TestMedia.NotMediaFile(), TestMedia.FfprobePath)
            TestRunner.Assert(info Is Nothing, "not-media → Nothing")
        End Sub

        Private Shared Sub Test_Missing()
            TestMedia.RequireBinaries()
            Dim info = MediaProbe.Probe(System.IO.Path.Combine(TestMedia.Sandbox, "definitely_missing.mp4"), TestMedia.FfprobePath)
            TestRunner.Assert(info Is Nothing, "missing → Nothing")
        End Sub

        Private Shared Sub Test_ParseTolerant()
            ' Minimal JSON: no optional fields at all — parser must not throw.
            Dim json = "{""streams"":[{""index"":0,""codec_type"":""video""}]," &
                       """format"":{""format_name"":""mov,mp4""}}"
            Dim info = MediaProbe.ParseJson("x.mp4", json)
            TestRunner.AssertEqual("mov,mp4", info.Format.FormatName, "format name")
            TestRunner.AssertEqual(1, info.Streams.Count, "one stream")
            TestRunner.AssertEqual(0.0, info.Format.DurationSec, "missing duration → 0")
        End Sub

    End Class

End Namespace
