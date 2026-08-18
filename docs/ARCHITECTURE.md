# ScotcsDuluka Capture Engine Architecture

> Canonical architecture definition for the Engine-Rebuild branch.
>
> This document describes the target architecture. A listed component is not implied to be fully implemented unless separately marked by implementation/status documentation.

## Architecture

```text
ScotcsDuluka Capture Engine
│
├── Capture Backend
│   ├── FFmpeg Backend
│   │   ├── ddagrab
│   │   ├── gdigrab
│   │   └── avfoundation (future)
│   │
│   └── Native Backend
│       ├── DXGI Desktop Duplication
│       ├── Windows Graphics Capture
│       └── NvFBC (future)
│
├── Encoder Backend
│   ├── NVIDIA NVENC
│   │   ├── H.264
│   │   ├── HEVC
│   │   └── AV1
│   │
│   ├── Intel QSV
│   │   ├── H.264
│   │   └── HEVC
│   │
│   ├── AMD AMF
│   │
│   └── Software
│       ├── libx264
│       └── libx265
│
├── Audio Backend
│   └── WASAPI
│       ├── System Audio
│       ├── Microphone
│       └── Audio Mixer
│
├── Frame Pipeline
│   ├── Frame Acquisition
│   ├── Timestamping
│   ├── Queue / Buffer
│   ├── Frame Synchronization
│   └── Backpressure
│
├── Output
│   ├── MP4
│   ├── MKV
│   └── Raw / Pipe
│
└── Core
    ├── Capture Session
    ├── Device Ownership
    ├── Lifecycle
    ├── Error Model
    └── Metrics / Diagnostics
```

## Architectural Rules

1. **Capture Backend** is responsible for acquiring video frames; capture mechanisms are backend implementations, not the pipeline itself.
2. **Encoder Backend** is responsible for video encoding and must remain replaceable independently of frame acquisition.
3. **Audio Backend** owns audio acquisition and mixing concerns. Video/audio synchronization belongs to the Frame Pipeline rather than a capture backend implementation.
4. **Frame Pipeline** owns acquisition handoff, timestamps, buffering, synchronization, and backpressure.
5. **Output** represents the final sink/container/pipe layer and must not own capture-device lifecycle.
6. **Core** owns Capture Session, device ownership, lifecycle, error semantics, and metrics/diagnostics.
7. **Future** components are architectural targets only; their presence here does not claim implementation completeness.
8. Legacy implementation details must not redefine this architecture. Migration work must preserve the boundaries above.

## Status Semantics

Architecture and implementation status are intentionally separate:

- `Implemented` — verified in the current Engine-Rebuild implementation.
- `In Progress` — actively being implemented or validated.
- `Planned` — part of the architecture but not yet implemented.
- `Future` — explicitly reserved for a future platform/vendor capability.

The architecture tree above is the canonical structural definition; implementation status should be recorded separately so that an architectural component is never mistaken for completed code.
