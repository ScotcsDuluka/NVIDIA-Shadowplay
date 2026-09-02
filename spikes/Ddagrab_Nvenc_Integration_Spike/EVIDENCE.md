# Ddagrab -> NVENC Integration Spike

Date: 2026-09-02
Hardware: NVIDIA GeForce GTX 1080 Ti
Capture: DXGI Output Duplication / D3D11
Encoder: NVENC H.264 / explicit native NV_ENC_CONFIG
Resource mode: D3D11 shared NT handle

## Run 1 — single-session baseline

Duration: 8.014 s
Display: 1680x1050 @ 75 Hz
Capture emitted: 673
Capture no-frame: 7
Capture dropped: 0
Capture errors: 0
AccessLost: 0
Frames consumed by encoder: 596
Encoded packets: 596
Encode failures: 0
Encoded FPS: 74.37
Input FPS: 74.37

## Run 2 — three-session restart + bitrate characterization

Configuration was unchanged across all sessions. The same Ddagrab and
NVENC backend instances were reused; each session performed Start/Stop on
both backends with a fresh bounded sink.

| Session | Duration | Frames | Encoded | FPS | Measured bitrate | Errors | AccessLost | Textures |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| 1 | 5.005 s | 501 | 501 | 100.10 | 14.24 Mbps | 0 | 0 | 1318/1318 |
| 2 | 5.011 s | 401 | 401 | 80.02 | 11.94 Mbps | 0 | 0 | 2256/2256 |
| 3 | 5.028 s | 250 | 250 | 49.72 | 8.44 Mbps | 0 | 0 | 2776/2776 |
| Aggregate | 15.044 s | 1152 | 1152 | — | 11.57 Mbps | 0 | 0 | 2776/2776 |

Restart result: PASS — all three sessions initialized, captured and encoded
without capture errors, NVENC failures, or AccessLost events.

## Run 3 — ten-session micro-restart characterization

The same backend instances were reused for 10 sessions of 2.0 s each.
All sessions encoded every consumed frame with zero encode failures,
capture errors and AccessLost events.

| Session | Encoded FPS | Measured bitrate |
|---|---:|---:|
| 1 | 73.87 | 9.72 Mbps |
| 2 | 74.19 | 11.02 Mbps |
| 3 | 74.02 | 10.76 Mbps |
| 4 | 74.01 | 10.96 Mbps |
| 5 | 74.14 | 10.89 Mbps |
| 6 | 74.11 | 11.10 Mbps |
| 7 | 73.96 | 11.16 Mbps |
| 8 | 68.91 | 10.27 Mbps |
| 9 | 65.20 | 9.67 Mbps |
| 10 | 64.25 | 9.91 Mbps |
| Aggregate | — | 10.60 Mbps |

Observed throughput remained near 74 FPS for sessions 1–7, then declined
to 68.91 / 65.20 / 64.25 FPS in sessions 8–10. This strengthens the
finding that sustained-load throughput is an open performance investigation,
not a restart correctness failure. GPU throttling or another runtime factor
is not established by this test.

## Timestamp

First PTS: 7500401644562
Last PTS: 7500477804400
PTS span: 7.616 s
PTS source: Ddagrab D3D11VideoFrame Diagnostics (100-ns engine ticks)

## GPU resource lifecycle

Textures created: 1346
Textures disposed: 1346
Resource accounting: PASS

The run used the D3D11 shared-handle path: capture device creates the
shared resource, NVENC opens it on the encoder device with OpenSharedResource1,
and the frame remains the owner of the capture-side resource.

## NVENC initialization

Codec: H.264
Preset: p4
Rate control: CBR
Configured bitrate: 20,000,000 bps
GOP: 75
Explicit config buffer: 3584 bytes
NvEncGetEncodePresetConfig: status 15 / NV_ENC_ERR_INVALID_VERSION
Fallback: explicit native config builder
NvEncInitializeEncoder: PASS

## Verdict

PASS — real Ddagrab frames traversed the bounded video handoff and were
successfully encoded by the native NVENC backend. The three-session restart
run also completed with zero capture errors, zero encode failures and zero
AccessLost events.

Important finding: measured bitrate was below the configured 20 Mbps CBR target
(14.24 / 11.94 / 8.44 Mbps; 11.57 Mbps aggregate), while the displayed desktop
content was not controlled for encoder complexity. Treat bitrate conformance
as OPEN; this result is characterization, not proof of a strict-CBR defect.

Important finding: encoded throughput degraded across consecutive reused
sessions (100.10 -> 80.02 -> 49.72 FPS). No correctness errors were observed,
but sustained multi-session throughput is OPEN and requires longer controlled
investigation before production sign-off.

## Run 4 — controlled dynamic desktop characterization

A deterministic animated canvas was displayed full-window to make the desktop
content substantially more dynamic than the prior static-content baseline.
Configuration remained CBR 20,000,000 bps, p4, GOP 75.

| Session | Duration | Frames | Encoded | FPS | Measured bitrate | Errors | AccessLost | Textures |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| 1 | 5.012 s | 404 | 404 | 80.60 | 6.79 Mbps | 0 | 0 | 1046/1046 |
| 2 | 5.011 s | 335 | 335 | 66.86 | 10.37 Mbps | 0 | 0 | 1794/1794 |
| 3 | 5.014 s | 372 | 372 | 74.19 | 10.73 Mbps | 0 | 0 | 2544/2544 |
| Aggregate | 15.037 s | 1111 | 1111 | — | 9.32 Mbps | 0 | 0 | 2544/2544 |

Interpretation: dynamic content still did not approach the configured 20 Mbps,
but output bitrate varied materially with content/session state. This is not
sufficient evidence of a strict CBR defect because the test does not measure
NVENC VBV behavior against a controlled rate target over a sufficiently long
window. Treat bitrate conformance as OPEN.

Throughput remained within the same broad range as prior runs; no encode or
capture correctness regression was observed.

This is an integration spike only. It does not by itself close long-duration,
production-session, mux/output, or 144-FPS requirements.
