# Phase 12 v3 — Host Audit (Engine-Rebuild-Stabilization)

**Branch:** `Engine-Rebuild-Stabilization` of `ScotcsDuluka/NVIDIA-Shadowplay`
**Architecture spec v3 commit:** `e632a931ee3315cb499d84dc7268cb79fc4dafeb`
**Audit method:** GitHub raw fetches only (no local checkout). All 7 probed files returned HTTP 200 on Engine-Rebuild-Stabilization.

---

## 1. `Engine/NVIDIA Capture.vbproj` — full content + ProjectReferences

### Full XML content

```xml
<Project Sdk="Microsoft.NET.Sdk">

        <PropertyGroup>
                <OutputType>WinExe</OutputType>
                <RootNamespace>NVIDIA_Capture</RootNamespace>
                <StartupObject>NVIDIA_Capture.My.MyApplication</StartupObject>
                <UseWindowsForms>true</UseWindowsForms>
                <MyType>WindowsForms</MyType>
                <TargetFramework>net8.0-windows10.0.26100.0</TargetFramework>
                <SupportedOSPlatformVersion>10.0.26100.0</SupportedOSPlatformVersion>
                <Platforms>AnyCPU;x64</Platforms>
                <OptionExplicit>Off</OptionExplicit>
                <OptionInfer>Off</OptionInfer>
                <ApplicationIcon>NVIDIA ShadowPlay.ico</ApplicationIcon>
                <UserSecretsId>d88fe301-91c6-4beb-a579-370ec911430c</UserSecretsId>
        </PropertyGroup>

        <ItemGroup>
                <Content Include="NVIDIA ShadowPlay.ico" />
        </ItemGroup>

        <ItemGroup>
                <Import Include="System" />
                <Import Include="System.Collections" />
                <Import Include="System.Collections.Generic" />
                <Import Include="System.Data" />
                <Import Include="System.Diagnostics" />
                <Import Include="System.Drawing" />
                <Import Include="System.IO" />
                <Import Include="System.Linq" />
                <Import Include="System.Net" />
                <Import Include="System.Net.Sockets" />
                <Import Include="System.Text" />
                <Import Include="System.Text.Json" />
                <Import Include="System.Threading" />
                <Import Include="System.Threading.Tasks" />
                <Import Include="System.Windows.Forms" />
                <Import Include="Microsoft.VisualBasic" />
        </ItemGroup>

        <ItemGroup>
          <PackageReference Include="NAudio" Version="2.3.0" />
          <PackageReference Include="NAudio.Asio" Version="2.3.0" />
          <PackageReference Include="NAudio.Core" Version="2.3.0" />
          <PackageReference Include="NAudio.Midi" Version="2.3.0" />
          <PackageReference Include="NAudio.Wasapi" Version="2.3.0" />
          <PackageReference Include="NAudio.WinForms" Version="2.3.0" />
          <PackageReference Include="NAudio.WinMM" Version="2.3.0" />
          <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
          <PackageReference Include="System.Text.Json" Version="8.0.5" />
        </ItemGroup>

        <ItemGroup>
                <None Update="API-Core\ffmpeg.exe">
                        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
                </None>
                <None Update="API-Core\ffprobe.exe">
                        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
                </None>
        </ItemGroup>

        <ItemGroup>
                <Compile Update="Engine\[API]\[Engine] Client.vb">
                        <SubType>Form</SubType>
                </Compile>

                <Compile Update="My Project\Application.Designer.vb">
                        <DesignTime>True</DesignTime>
                        <AutoGen>True</AutoGen>
                        <DependentUpon>Application.myapp</DependentUpon>
                </Compile>

                <!-- WinForms Designer files: must be DependentUpon parent form + have resx. -->
                <Compile Update="Engine\Engine\[UI]\AudioSettingsForm.Designer.vb">
                        <DependentUpon>AudioSettingsForm.vb</DependentUpon>
                </Compile>
                <Compile Update="Engine\Engine\[UI]\UI_Engine.Designer.vb">
                        <DependentUpon>UI_Engine.vb</DependentUpon>
                </Compile>

                <EmbeddedResource Update="Engine\Engine\[UI]\AudioSettingsForm.resx">
                        <DependentUpon>AudioSettingsForm.vb</DependentUpon>
                </EmbeddedResource>
                <EmbeddedResource Update="Engine\Engine\[UI]\UI_Engine.resx">
                        <DependentUpon>UI_Engine.vb</DependentUpon>
                </EmbeddedResource>
        </ItemGroup>

        <ItemGroup>
                <None Update="My Project\Application.myapp">
                        <Generator>MyApplicationCodeGenerator</Generator>
                        <LastGenOutput>Application.Designer.vb</LastGenOutput>
                </None>
        </ItemGroup>

</Project>
```

### ProjectReferences

**ZERO `<ProjectReference>` entries.** The project is fully standalone legacy — no reference to `CaptureEngine`, `CaptureEngine.Video`, `CaptureEngine.Encoder`, or any new-arch project.

### PackageReferences (9 total)

| Package | Version |
|---|---|
| NAudio | 2.3.0 |
| NAudio.Asio | 2.3.0 |
| NAudio.Core | 2.3.0 |
| NAudio.Midi | 2.3.0 |
| NAudio.Wasapi | 2.3.0 |
| NAudio.WinForms | 2.3.0 |
| NAudio.WinMM | 2.3.0 |
| Newtonsoft.Json | 13.0.4 |
| System.Text.Json | 8.0.5 |

NAudio suite confirmed present (matches architecture spec expectation).

### `<Compile Update="...">` entries (4)

1. **`Engine\[API]\[Engine] Client.vb`** — `<SubType>Form</SubType>` (NOTE: this is a partial class of `UI_Engine` Form, so it gets Form subtype)
2. **`My Project\Application.Designer.vb`** — DesignTime/AutoGen, DependentUpon Application.myapp
3. **`Engine\Engine\[UI]\AudioSettingsForm.Designer.vb`** — DependentUpon AudioSettingsForm.vb
4. **`Engine\Engine\[UI]\UI_Engine.Designer.vb`** — DependentUpon UI_Engine.vb

Plus 2 `EmbeddedResource Update` entries (resx files for the two forms).
Plus 2 `None Update` content items: `API-Core\ffmpeg.exe` and `API-Core\ffprobe.exe` with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`.

**Source files that belong to this project (via SDK auto-glob + explicit Compile Update):** all `.vb` files under `Engine\` and `My Project\` are auto-included by SDK-style globbing; the 4 explicit `<Compile Update>` entries only override Form subtype / Designer linkage. So source files include `[Engine] Client.vb`, `[Capture]/CaptureEngine.vb`, `[Capture]/CaptureSettings.vb`, `[FFmpeg]/FFmpegArgumentBuilder.vb`, `[Audio]/AudioFileWriter.vb`, `[Infrastructure]/JobObjectGuard.vb`, `[UI]/UI_Engine.vb` + `.Designer.vb`, `[UI]/AudioSettingsForm.vb` + `.Designer.vb`, `EncoderDetector.vb`, etc.

### Key project properties

| Property | Value |
|---|---|
| `OutputType` | `WinExe` |
| `TargetFramework` | `net8.0-windows10.0.26100.0` |
| `SupportedOSPlatformVersion` | `10.0.26100.0` |
| `RootNamespace` | `NVIDIA_Capture` |
| `StartupObject` | `NVIDIA_Capture.My.MyApplication` |
| `UseWindowsForms` | `true` |
| `MyType` | `WindowsForms` |
| `Platforms` | `AnyCPU;x64` |
| `OptionExplicit` | `Off` ⚠ |
| `OptionInfer` | `Off` ⚠ |
| `ApplicationIcon` | `NVIDIA ShadowPlay.ico` |

⚠ Option Explicit/Infer are `Off` — legacy VB defaults. New-arch projects use `Strict On / Explicit On / Infer On`.

---

## 2. `Engine/Engine/[API]/[Engine] Client.vb` — full IPC command set

File: 10731 bytes, 247 lines. **Partial Class of `UI_Engine`** (a WinForms Form, not a standalone class).

### OnMessage handler / command dispatcher

The message parser is split across two methods:

**`OnTcpMessage(msg As String)` (parser, lines 105–194):**
```vbnet
Public Sub OnTcpMessage(msg As String)
    Try
        If String.IsNullOrEmpty(msg) OrElse Not msg.Contains("|") Then Return

        Dim parts As String() = msg.Split("|"c)
        If parts.Length < 2 Then Return

        Dim senderSegment As String = parts(0).Trim()
        senderSegment = senderSegment.Replace("[Send] ", "").Replace("[Receive] ", "").Trim()
        If String.Equals(senderSegment, "NVIDIA Engine", StringComparison.Ordinal) Then Return

        Dim data As String = parts(1)

        Dim colonIndex As Integer = data.IndexOf(":"c)
        Dim cmd, value As String
        If colonIndex >= 0 Then
            cmd = data.Substring(0, colonIndex)
            value = data.Substring(colonIndex + 1)
        Else
            cmd = data
            value = ""
        End If

        ' ── Pre-filter commands (handled BEFORE engine_* normalization) ──
        If cmd = "PREWARM_FFMPEG" AndAlso value.Length > 0 Then
            Dim pipeIdx As Integer = value.IndexOf("|"c)
            Dim ffmpegPath As String = If(pipeIdx > 0, value.Substring(0, pipeIdx), value)
            BeginUiInvoke(Sub() HandleEnginePrewarmFFmpeg(ffmpegPath))
            Return
        End If

        If cmd = "engine_config_changed" Then
            BeginUiInvoke(Sub() HandleEngineConfigChanged(value))
            Return
        End If

        ' ── Legacy alias mapping (RECORD_* → engine_*) ──
        Dim canonicalCmd As String = cmd
        Select Case cmd
            Case "RECORD_START" : canonicalCmd = "engine_record_start"
            Case "RECORD_STOP" : canonicalCmd = "engine_record_stop"
            Case "REPLAY_START" : canonicalCmd = "engine_replay_start"
            Case "REPLAY_STOP" : canonicalCmd = "engine_replay_stop"
            Case "REPLAY_SAVE" : canonicalCmd = "engine_replay_save"
        End Select

        If Not canonicalCmd.StartsWith("engine_") Then Return
        cmd = canonicalCmd

        ' ── Optional request ID: req=<token>|<payload> ──
        Dim reqId As String = Nothing
        If value.StartsWith("req=", StringComparison.Ordinal) Then
            Dim sepIdx As Integer = value.IndexOf("|"c)
            If sepIdx > 4 Then
                reqId = value.Substring(4, sepIdx - 4)
                value = value.Substring(sepIdx + 1)
            End If
        End If

        DebugLog($"[Engine] Received: {cmd}={value}" & If(String.IsNullOrEmpty(reqId), "", $" (req={reqId})"))

        Dim cmdCopy As String = cmd
        Dim valueCopy As String = value
        Dim reqIdCopy As String = reqId
        BeginUiInvoke(Sub() DispatchEngineCommand(cmdCopy, valueCopy, reqIdCopy))

    Catch ex As Exception
        Debug.WriteLine($"UI_Engine.OnTcpMessage error: {ex.Message}")
    End Try
End Sub
```

**`DispatchEngineCommand(cmd, value, reqId)` (dispatcher, lines 216–245):**
```vbnet
Private Async Sub DispatchEngineCommand(cmd As String, value As String, reqId As String)
    Try
        Select Case cmd
            Case "engine_record_start"
                Await HandleEngineRecordStart(value, reqId)
            Case "engine_record_stop"
                Await HandleEngineRecordStop(reqId)
            Case "engine_replay_start"
                HandleEngineReplayStart(value, reqId)
            Case "engine_replay_stop"
                HandleEngineReplayStop(reqId)
            Case "engine_replay_save"
                HandleEngineReplaySave(value, reqId)
            Case "engine_get_status"
                HandleEngineGetStatus(reqId)
            Case "engine_load_config"
                HandleEngineLoadConfig(reqId)
            Case "engine_set_encoder"
                HandleEngineSetEncoder(value, reqId)
            Case Else
                SendResponse(cmd, "error", "unknown_command", reqId)
        End Select
    Catch ex As Exception
        DebugLog($"DispatchEngineCommand unhandled exception: {ex.Message}")
        Try
            SendResponse(cmd, "error", ex.Message, reqId)
        Catch
        End Try
    End Try
End Sub
```

### Complete command set (10 total)

| # | Command | Source | Handler | Notes |
|---|---|---|---|---|
| 1 | `PREWARM_FFMPEG:<path>[|<encoder>]` | pre-filter | `HandleEnginePrewarmFFmpeg(ffmpegPath)` | value can contain `|`-separated encoder name |
| 2 | `engine_config_changed[:video|config]` | pre-filter | `HandleEngineConfigChanged(value)` | no `reqId` extraction |
| 3 | `engine_record_start:<outputPath>` | canonical (or alias `RECORD_START`) | `Await HandleEngineRecordStart(value, reqId)` | async |
| 4 | `engine_record_stop` | canonical (or alias `RECORD_STOP`) | `Await HandleEngineRecordStop(reqId)` | async |
| 5 | `engine_replay_start:<seconds>` | canonical (or alias `REPLAY_START`) | `HandleEngineReplayStart(value, reqId)` | sync |
| 6 | `engine_replay_stop` | canonical (or alias `REPLAY_STOP`) | `HandleEngineReplayStop(reqId)` | sync |
| 7 | `engine_replay_save:<path>;<duration>` | canonical (or alias `REPLAY_SAVE`) | `HandleEngineReplaySave(value, reqId)` | sync; semicolon-delimited |
| 8 | `engine_get_status` | canonical | `HandleEngineGetStatus(reqId)` | sync |
| 9 | `engine_load_config` | canonical | `HandleEngineLoadConfig(reqId)` | sync |
| 10 | `engine_set_encoder:<value>` | canonical | `HandleEngineSetEncoder(value, reqId)` | sync |

The `HandleEngine*` methods live in other partial-class files of `UI_Engine` (not in this file) — per the prior phase12-audit they call into legacy `CaptureEngine.vb` to start/stop recording.

### Wire format

Documented in the file header (lines 10–13):

```
[Send] <AppName>|<cmd>[:<value>]
[Receive] <AppName>|<msg>
[System]|pong
```

Parser strips the `[Send] `/`[Receive] ` prefix (M9 FIX), then `|`-splits into sender + payload, then `:`-splits payload into cmd + value. Optional `req=<token>|<payload>` is split out before dispatch.

### TcpClientHelper construction

**`tcp` is constructed directly** inside `StartTcpClient()` (line 44):
```vbnet
tcp = New TcpClientHelper("NVIDIA Engine", "127.0.0.1", 5000, autoReconnect:=True)
AddHandler tcp.OnMessageReceived, AddressOf OnTcpMessage
AddHandler tcp.OnDisconnected, AddressOf OnTcpDisconnected
AddHandler tcp.OnReconnecting, AddressOf OnTcpReconnecting
```

The client does NOT receive `TcpClientHelper` via constructor — `UI_Engine` constructs it itself as a `Public Shared tcp As TcpClientHelper` field. Server-side is `API/[Forms - Project Files]/[API]/Server.vb` (per prior audit).

### Outgoing messages (Engine → Hub, broadcast)

- `register:NVIDIA Engine` (on connect)
- `engine_ready` (broadcast after connect)
- `ping` (every 10s, by TcpClientHelper internally)
- `engine_response:<cmd>,<status>[,<data>][,req=<reqId>]` (via `SendResponse` helper)

---

## 3. `Engine/Engine/[Audio]/AudioFileWriter.vb` — public API surface

File: 37306 bytes, 779 lines. `Public Class AudioFileWriter Implements IDisposable` (no namespace).

### Public methods + properties (signatures only)

```vbnet
' ── Constructor ──
Public Sub New(config As AudioConfigValues)

' ── Properties ──
Public ReadOnly Property IsRunning As Boolean
Public ReadOnly Property SystemStartTicks As Long
Public ReadOnly Property MicStartTicks As Long

' ── Methods ──
Public Function Start(systemPath As String, micPath As String) As Boolean
Public Sub [Stop]()
Public Function GetDiagnostics() As String
Public Shared Function HasAudioData(wavPath As String) As Boolean
Public Shared Function ListMicDevices() As List(Of Tuple(Of String, String))
Public Sub Dispose() Implements IDisposable.Dispose

' ── Events ──
Public Event SystemStartFailed(reason As String)
Public Event MicStartFailed(reason As String)
Public Event SystemFormatDetected(format As AudioFormat)
Public Event MicFormatDetected(format As AudioFormat)
```

### Nested public class

```vbnet
Public Class AudioConfigValues
    Public Property SystemAudioCapture As Boolean = False
    Public Property MicCapture As Boolean = False
    Public Property SystemAudioVolume As Single = 1.0F
    Public Property MicVolume As Single = 1.0F
    Public Property MicDeviceId As String = ""
    Public Property MicDeviceName As String = ""
End Class
```

### Constructor signature

`Public Sub New(config As AudioConfigValues)` — accepts ONLY the config object. **NO path**, **NO D3D11 device**, **NO logger**.

### D3D11 device?

**NONE.** Standalone WASAPI via NAudio (`Imports NAudio.CoreAudioApi`, `Imports NAudio.Wave`). Uses `WasapiCapture` for capture, `WaveFileWriter` for .wav output. Zero GPU/D3D11 dependency.

### Temp .wav path management

Paths are **NOT** passed in constructor — they are passed to `Start(systemPath As String, micPath As String)`. The class opens `WaveFileWriter` per track using the path the caller provides. Caller has full control of where temp .wav files live (e.g., `%TEMP%\temp_system.wav`).

### QPC timestamp capture

**Yes — per-track, on `StartRecording()` (NOT first callback).** Implementation:
```vbnet
Dim startRecordingTicks As Long = Stopwatch.GetTimestamp()
track.StartRecordingTicks = startRecordingTicks
If track.Config.IsSystem Then
    _systemStartTicks = startRecordingTicks
Else
    _micStartTicks = startRecordingTicks
End If
```

`Stopwatch.GetTimestamp()` is QPC under the hood on Windows. Exposed publicly as `SystemStartTicks` and `MicStartTicks` (Long, ticks). These are the sync anchors used by the muxer's `-ss`/`adelay` filters.

Additionally tracks `FirstCallbackDispatchTicks` per track (QPC at first WASAPI callback) — used for diagnostics only (computing `FirstCallbackDelayMs`).

### Dispose pattern

**Idempotent and try/finally safe.** Pattern:
```vbnet
Public Sub Dispose() Implements IDisposable.Dispose
    If _disposed Then Return
    _disposed = True
    [Stop]()
End Sub
```

`_disposed` is a simple Boolean guard (not Interlocked). The internal `[Stop]()` method has its own per-step `Try/Catch` blocks (6 steps: lifecycle=Draining → stop captures → wait for in-flight callbacks with 2s timeout → lifecycle=Stopped → CompleteAdding on queues → Join writer threads with 10s timeout → flush+dispose WaveFileWriter+WasapiCapture). Double-Dispose is safe (guard returns early). `ObjectDisposedException` is raised from `Start()` if called after Dispose.

### Accounting fields (architecture doc invariant)

**Present and exposed via `GetDiagnostics()`.** All on the private nested `TrackState` class. Surfaced as formatted strings (NOT public properties — caller must parse `GetDiagnostics()` output or extend the class):

| Field | Type | Notes |
|---|---|---|
| `CallbackCount` | Long | Total WASAPI callbacks received |
| `BytesEnqueued` | Long | Bytes pushed into per-track BlockingCollection |
| `WrittenBytes` | Long | Bytes actually written to disk by writer thread |
| `WriteLagBytes` | Long (derived) | `BytesEnqueued - WrittenBytes` |
| `SamplesEnqueued` | Long | |
| `DroppedChunks` | Long | Queue full → chunk discarded |
| `DroppedBytes` | Long | Real PCM bytes dropped (data loss) |
| `DroppedSamples` | Long | |
| `DroppedDurationSec` | Double | |
| `DroppedSilenceBytes` | Long | Synthetic silence dropped to make room for real PCM |
| `InitialSilenceBytes` | Long | Pre-fill silence to align WAV with capture timeline |
| `BytesAccountingResidual` | Long (derived) | `BytesEnqueued - WrittenBytes - DroppedBytes - DroppedSilenceBytes` — should be ≈0 after Stop() |

**Invariant documented in code (lines 722–729):**
> After writer drains completely (WriteLagBytes ≈ 0):
> `BytesEnqueued ≈ WrittenBytes + DroppedBytes + DroppedSilenceBytes`
> Any non-zero residual after Stop() indicates writer did not drain properly OR a counting bug.

**Architecture spec §13 Q2 expectation: CONFIRMED.** All three accounting fields (`BytesEnqueued`, `WrittenBytes`, `DroppedBytes`) are present, plus extras (`DroppedSilenceBytes`, `DroppedChunks`, `BytesAccountingResidual`, `WriteLagBytes`).

---

## 4. `CaptureEngine.Encoder/Contract/EncodedPacket.vb` — Payload type + ownership

File: 6747 bytes, 142 lines. `Public NotInheritable Class EncodedPacket Implements IDisposable` in namespace `CaptureEngine.Encoder`.

### Full class definition (compact)

```vbnet
Option Strict On
Option Explicit On

Imports System.Threading

Namespace CaptureEngine.Encoder
    Public NotInheritable Class EncodedPacket
        Implements IDisposable

        Private ReadOnly _metadata As PacketMetadata
        Private ReadOnly _payload As Byte()
        Private ReadOnly _payloadLength As Integer
        Private _disposeCount As Integer = 0

        Public Sub New(metadata As PacketMetadata, payload As Byte(), payloadLength As Integer)
            If payload Is Nothing Then Throw New ArgumentNullException(NameOf(payload))
            If payloadLength < 0 OrElse payloadLength > payload.Length Then
                Throw New ArgumentOutOfRangeException(NameOf(payloadLength),
                    "payloadLength must be in [0, payload.Length].")
            End If
            _metadata = metadata
            _payload = payload
            _payloadLength = payloadLength
        End Sub

        Public ReadOnly Property Metadata As PacketMetadata
            Get
                Return _metadata
            End Get
        End Property

        Public ReadOnly Property Payload As Byte()
            Get
                Return _payload
            End Get
        End Property

        Public ReadOnly Property PayloadLength As Integer
            Get
                Return _payloadLength
            End Get
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return Thread.VolatileRead(_disposeCount) > 0
            End Get
        End Property

        Public ReadOnly Property DisposeCount As Integer
            Get
                Return Thread.VolatileRead(_disposeCount)
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            Interlocked.Increment(_disposeCount)
        End Sub
    End Class
End Namespace
```

### `Payload` type

**`Byte()`** — plain byte array. NOT `MemoryStream`, NOT `Span(Of Byte)`, NOT `Memory(Of Byte)`.

Accompanied by `PayloadLength As Integer` (must be `>= 0` and `<= payload.Length`). Caller MUST use `(Payload, 0, PayloadLength)` for writes — never `Payload.Length` directly (the underlying array may be over-allocated for pool reuse).

### Ownership model — caller-owned

**YES — caller-owned.** Extensively documented in the class XML comments:

> 1. EncodedPacket is owned by EXACTLY ONE caller at a time.
> 2. The caller receives ownership from `IEncoderBackend.Encode()` return value OR from `Flush(sink)` callback.
> 3. The owner MUST call Dispose() when done — this releases the underlying byte[] buffer back to the encoder's pool (future) OR leaves it for GC (current default).
> 4. After Dispose(), accessing Payload OR Metadata is UNDEFINED BEHAVIOR.
> 5. Dispose() is IDEMPOTENT — safe to call multiple times (per P1-B.1 FIX lesson #3).
> 6. Dispose() is THREAD-SAFE — uses `Interlocked.Increment` to atomically update `_disposeCount`.

Current Dispose is **release-only** (just increments `_disposeCount`; does NOT clear `_payload` to avoid racing with concurrent `Payload` reads). Future pooled-buffer variant would return the byte[] to the pool on first Dispose via `Interlocked.CompareExchange(_disposeCount, 1, 0)`.

### D3D11/NVENC types leaking through

**NONE.** The class contains ONLY:
- `Byte()` payload
- `Integer` payloadLength
- `PacketMetadata` struct (also free of D3D11/NVENC types — see below)
- `Integer` _disposeCount

No NVENC handles, no `ID3D11Device`, no DXGI types, no Silk.NET/SharpDX/Vortice references. **Contract surface is clean.**

### `PacketMetadata` field (sibling file `PacketMetadata.vb`, 4497 bytes)

**YES — present as an immutable struct** in `CaptureEngine.Encoder` namespace (separate file). `EncodedPacket._metadata` is of type `PacketMetadata`.

```vbnet
Public Structure PacketMetadata
    Public ReadOnly Property Sequence As Long              ' monotonic per-encoder counter
    Public ReadOnly Property PresentationTimestampTicks As Long   ' PTS (Engine ticks)
    Public ReadOnly Property DecodingTimestampTicks As Long       ' DTS (Engine ticks)
    Public ReadOnly Property DurationTicks As Long                ' frame duration
    Public ReadOnly Property IsKeyFrame As Boolean                ' I-frame flag
    Public ReadOnly Property IsReferenceFrame As Boolean          ' P/B reference flag
    Public ReadOnly Property CodecKey As String                   ' e.g. "NVENC_H264"
    Public ReadOnly Property CodecSpecificFlags As Integer        ' bit field for future use
End Structure
```

Constructor:
```vbnet
Public Sub New(sequence As Long,
               presentationTimeTicks As Long,
               decodingTimeTicks As Long,
               durationTicks As Long,
               isKeyFrame As Boolean,
               isReferenceFrame As Boolean,
               codecKey As String,
               codecSpecificFlags As Integer)
```

Timestamps are in **Engine PTS units** (same domain as `FrameDiagnostics` per P1-A v1.3.1 §3.6.1) — guarantees encoded packets can be correlated with source `IVideoFrame.Diagnostics.PresentationTimestampTicks` without unit conversion at the muxer.

---

## 5. `CaptureEngine.Video/Contract/IVideoBackendContext.vb` + `EncoderConfig.vb`

### 5a. IVideoBackendContext — full interface definition

File: 635 bytes, 18 lines.

```vbnet
Option Strict On
Option Explicit On

Imports CaptureEngine.Diagnostics

Namespace CaptureEngine.Video
    ''' <summary>
    ''' Object passed to a backend at Initialize time. Gives the backend
    ''' ONLY what it needs — a logger, the BackendKind, and (optionally,
    ''' per §5 device-ownership decision) the D3D11 device to use.
    ''' Does NOT expose CaptureEngine's state machine or its sync lock.
    ''' (P1-A v1.3.1 §11)
    ''' </summary>
    Public Interface IVideoBackendContext
        ReadOnly Property Logger As EngineLogger
        ReadOnly Property BackendKind As VideoBackendKind
    End Interface
End Namespace
```

### Members

- `ReadOnly Property Logger As EngineLogger`
- `ReadOnly Property BackendKind As VideoBackendKind`

**That's it.** Two properties.

### D3D11 device on context?

**NO** — currently the interface does NOT carry a D3D11 device. The comment hints that this is per the §5 device-ownership decision (Option A: each backend owns its own device; Option B: device shared via context). Today it's Option A: the D3D11 device property is absent.

### Other context fields

**NONE.** No adapter LUID, no output index, no format requirements, no capture parameters. Just Logger + BackendKind. Backend-specific config (e.g., `DdagrabConfig` with monitor index, timeout, BGRA8 flag) is passed separately via the backend's own Initialize(config) overload.

### Is this the contract that NvencEncoderBackend would receive via Initialize(config As EncoderConfig)?

**NO.** `IVideoBackendContext` is for VIDEO CAPTURE backends (DdagrabBackend, GfxCaptureBackend) — passed at `IVideoCaptureBackend.Initialize(context)`. Encoders receive `EncoderConfig` instead (a separate type, see 5b below). The two contracts serve different stages of the pipeline:

```
CaptureEngine ──IVideoBackendContext──► IVideoCaptureBackend (DdagrabBackend)
                                         │
                                         ▼ produces IVideoFrame
CaptureEngine ──EncoderConfig──────────► IEncoderBackend (NvencEncoderBackend)
                                         │
                                         ▼ produces EncodedPacket
```

### 5b. EncoderConfig — full class definition

File: 5440 bytes, 126 lines. `Public NotInheritable Class EncoderConfig Implements ICloneable` in namespace `CaptureEngine.Encoder`.

```vbnet
Option Strict On
Option Explicit On

Namespace CaptureEngine.Encoder
    Public NotInheritable Class EncoderConfig
        Implements ICloneable

        ' ---- codec identity ----
        Public Property CodecKey As String = "NVENC_H264"
        Public Property FFmpegCodec As String = "h264_nvenc"

        ' ---- encoding parameters ----
        Public Property BitrateBps As Long = 20_000_000L
        Public Property MinrateBps As Long = 20_000_000L
        Public Property MaxrateBps As Long = 20_000_000L
        Public Property BufsizeBps As Long = 40_000_000L
        Public Property GopSize As Integer = 60
        Public Property RateControl As String = "cbr"
        Public Property Cq As Integer = 0
        Public Property Preset As String = "p4"
        Public Property OutputPixelFormat As String = "nv12"

        ' ---- frame I/O contract ----
        Public Property ExpectedWidth As Integer = 0
        Public Property ExpectedHeight As Integer = 0
        Public Property ExpectedInputFormat As CaptureEngine.Video.VideoPixelFormat =
            CaptureEngine.Video.VideoPixelFormat.Bgra8

        ' ---- threading / latency ----
        Public Property MaxInFlightFrames As Integer = 4
        Public Property FlushTimeoutMs As Integer = 5000
        Public Property StopTimeoutMs As Integer = 10000

        Public Function Clone() As EncoderConfig
            Return CType(Me.MemberwiseClone(), EncoderConfig)
        End Function

        Private Function CloneObject() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
    End Class
End Namespace
```

### EncoderConfig carries D3D11 device?

**NO — EncoderConfig has NO device field.** Properties cover only:
- Codec identity (`CodecKey`, `FFmpegCodec`)
- Encoding parameters (BitrateBps/MinrateBps/MaxrateBps/BufsizeBps/GopSize/RateControl/Cq/Preset/OutputPixelFormat)
- Frame I/O contract (ExpectedWidth/ExpectedHeight/ExpectedInputFormat — defaults to Bgra8 per Phase 1 baseline)
- Threading/latency (MaxInFlightFrames/FlushTimeoutMs/StopTimeoutMs)

No `Device As ID3D11Device`, no `DeviceContext`, no adapter LUID, no NVENC session pointer. The class doc explicitly states:

> "This is the contract-level config — it contains only fields that are meaningful to ALL encoder implementations (NVENC, QSV, AMF, Software). Implementation-specific options (e.g. NVENC's SpatialAQ / TemporalAQ / Tune) live in implementation-specific config types defined by the concrete encoder's own project."

**Therefore NvencEncoderBackend must either:**
- **Create its own D3D11 device internally** (Option A — matches IVideoBackendContext's current shape, simplest)
- **Receive one out-of-band** via a separate `Initialize(device, config)` overload or constructor (Option B — requires extending the encoder contract surface; future work)

The most likely Phase 12a path is **Option A**: NvencEncoderBackend creates its own D3D11 device at `Initialize(config)` time, queries NVENC capability, allocates an NVENC encoder session, and tears down all of it at `Dispose()`. This is consistent with `IVideoBackendContext` having no device today.

---

## 6. `Engine/Engine/[Infrastructure]/JobObjectGuard.vb` — public API

File: 7382 bytes, 173 lines. `Public Notinheritable Class JobObjectGuard Implements IDisposable` (no namespace — root-level class).

### Public constructor signature(s)

**One constructor:**
```vbnet
Public Sub New()
```

Parameterless. Creates a Job Object via `CreateJobObject(IntPtr.Zero, Nothing)`, sets `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`, and wraps the raw handle in `New SafeFileHandle(rawHandle, ownsHandle:=True)` for finalization safety. Throws `Win32Exception` if either P/Invoke fails (and `CloseHandle`s the raw handle before throwing to avoid leaking the OS resource).

### Public methods

| Method | Signature | Notes |
|---|---|---|
| `New` | `Public Sub New()` | Parameterless; creates + configures the Job Object |
| `Assign` | `Public Sub Assign(process As Process)` | Must be called AFTER `process.Start()` and BEFORE process spawns children (FFmpeg doesn't spawn children, so order non-critical). Uses `DangerousAddRef`/`Release` pattern (M11 FIX) to prevent handle-close race. Also sets `process.PriorityClass = ProcessPriorityClass.AboveNormal` (best-effort). |
| `Dispose` | `Public Sub Dispose() Implements IDisposable.Dispose` | Idempotent guard on `_disposed`; closes `_handle` (SafeFileHandle) |

### Lifetime model

**PER-CAPTUREENGINE INSTANCE (per engine), NOT per-FFmpeg-subprocess.** From the file header doc (lines 10–12):

> Fix: Create one Job Object per CaptureEngine instance, with `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`. Assign every spawned ffmpeg to it. When the Engine process dies for any reason, Windows automatically kills all processes in the Job → ffmpeg dies with it.

So **one `JobObjectGuard` per engine lifetime**. Every FFmpeg subprocess (and there may be multiple: per-session capture FFmpeg + per-session mux FFmpeg) is `.Assign()`-ed to that same JobObjectGuard. The JobObjectGuard outlives any individual FFmpeg subprocess — when the engine/CaptureEngine is Disposed, the JobObjectGuard is Disposed, the SafeFileHandle finalizes, the OS handle closes, and **all** assigned FFmpeg subprocesses get killed by the Windows kernel.

`JOB_OBJECT_LIMIT_BREAKAWAY_OK` is intentionally NOT set — so FFmpeg cannot escape the job even if it tried.

### IDisposable + idempotent?

- **Implements IDisposable: YES**
- **Idempotent: YES** — `If _disposed Then Return; _disposed = True; ...`
- `_disposed` is a plain Boolean (not Interlocked — Dispose is expected to be called once from a single owner thread, not concurrently).
- The `Assign(process)` method has its own internal Try/Catch/Finally for the `DangerousAddRef`/`DangerousRelease` pattern and the priority-class set (M11 FIX comment explains the race it prevents).
- Note: there is no `Protected Overrides Sub Dispose(disposing As Boolean)` — this is the simpler non-standard Dispose pattern (no finalizer, no managed-resource disposal distinction). Acceptable here because the only resource is the SafeFileHandle, which has its own finalizer.

---

## Implementation Impact

**1. NvencEncoderBackend must create its own D3D11 device: YES.** Both `EncoderConfig` (no `Device` field — only codec/bitrate/preset/timing/format properties) and `IVideoBackendContext` (only `Logger` + `BackendKind`) are device-free. Architecture Option A applies — `NvencEncoderBackend.Initialize(config)` must create its own D3D11 device + NVENC session internally and Dispose them on `Dispose()`.

**2. AudioFileWriter can be constructed standalone: YES.** No D3D11 dependency, takes `AudioConfigValues` in constructor, takes .wav paths in `Start(systemPath, micPath)`. QPC timestamp capture and accounting invariants (`BytesEnqueued`/`WrittenBytes`/`DroppedBytes`/`BytesAccountingResidual`) already match architecture spec §13 Q2. The `AudioSidecar.vb` stub in CaptureEngine.FFmpegBackend needs only to instantiate AudioFileWriter + manage temp paths + expose `HasAudioData` — no adapter wrapping required beyond that thin orchestration.

**3. `[Engine] Client.vb` can be reused with rewiring only: YES, with caveats.** Wire format + dispatcher are REUSE-VERBATIM. But the class is a `Partial Class of UI_Engine` (a WinForms Form) and `tcp` is a `Public Shared` field on that form. If `RecordingEngine` is non-UI, either (a) keep `UI_Engine` as the IPC host and have it call into `RecordingEngine`, or (b) extract `OnTcpMessage`+`DispatchEngineCommand`+`SendResponse` into a standalone `RecordingEngineIpc` class. Handlers (`HandleEngineRecordStart` etc.) currently call legacy `CaptureEngine.vb` — these need rewiring to new `RecordingEngine` (Phase 12a scope).

**4. JobObjectGuard is per-engine: PER-ENGINE.** One JobObjectGuard per `RecordingEngine` (or `CaptureEngine`) instance. Every FFmpeg subprocess (capture FFmpeg + mux FFmpeg) is `.Assign()`-ed to the same guard. Not per-FFmpeg-subprocess. Dispose is idempotent + SafeFileHandle-backed finalizer ensures orphans die even on engine crash.

**5. EncodedPacket.Payload is directly writable to FFmpeg stdin: YES.** Payload is `Byte()` with `PayloadLength As Integer`. Consumer code: `stdin.Write(packet.Payload, 0, packet.PayloadLength); packet.Dispose();`. No MemoryStream copy needed. Caller MUST respect `PayloadLength` (never use `Payload.Length` — array may be over-allocated) and MUST call `Dispose()` after consuming (caller-owned per contract).
