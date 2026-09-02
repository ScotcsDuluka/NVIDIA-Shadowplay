# Ddagrab -> NVENC Integration Spike

Date: 2026-09-02
Hardware: NVIDIA GeForce GTX 1080 Ti
Capture: DXGI Output Duplication / D3D11
Encoder: NVENC H.264 / explicit native NV_ENC_CONFIG
Resource mode: D3D11 shared NT handle

## Run

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
successfully encoded by the native NVENC backend with zero capture/encode
errors and balanced GPU texture lifetime accounting.

This is an integration spike only. It does not by itself close long-duration,
production-session, mux/output, or 144-FPS requirements.
