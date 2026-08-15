#!/usr/bin/env python3
"""
P1-B Step 3: Named Pipe + Synthetic PCM Producer

Creates a Windows named pipe, writes continuous 48kHz stereo f32le PCM
(silence) at real-time rate, while FFmpeg reads from it.

This isolates:
  - Named pipe I/O overhead
  - Writer thread overhead (Python, not NAudio, but same pattern)
  - FFmpeg reading from named pipe vs lavfi source

If this test is healthy → named pipe is NOT the cause → NAudio/WASAPI is
If this test is degraded → named pipe I/O itself causes video regression

Usage:
  python3 scripts/p1b_step3_named_pipe.py

Prerequisites:
  - Python 3.10+ with pywin32 (pip install pywin32)
  - ffmpeg.exe at the path below
  - Windows
"""

import subprocess
import time
import sys
import os
import threading
import re


# ============================================================
# Configuration
# ============================================================

FFMPEG_PATH = (
    r"C:\My Project\NVIDIA-Shadowplay"
    r"\Overlay\bin\Release\net8.0-windows10.0.26100.0"
    r"\api-core\ffmpeg.exe"
)

OUTPUT_DIR = r"C:\Users\ScotcsDuluka\Videos\Shadowplay\Gallery"

RECORD_SECONDS = 30

FPS = 144
BITRATE = 17_000_000

PIPE_NAME = "nvidia_shadowplay_test_pipe2"

SAMPLE_RATE = 48_000
CHANNELS = 2

BYTES_PER_SAMPLE = 4  # f32le

BLOCK_ALIGN = CHANNELS * BYTES_PER_SAMPLE

# 10ms of audio per iteration
# 48,000 samples/sec × 0.010 sec = 480 frames
CHUNK_FRAMES = 480

# 480 stereo frames × 8 bytes/frame = 3840 bytes
CHUNK_BYTES = CHUNK_FRAMES * BLOCK_ALIGN


# ============================================================
# Named Pipe
# ============================================================

def create_named_pipe():
    """Create a Windows named pipe server using Win32 API."""

    try:
        import win32pipe
        import win32file
        import pywintypes  # noqa: F401
    except ImportError:
        print("ERROR: pywin32 not installed.")
        print("Run:")
        print("  pip install pywin32")
        sys.exit(1)

    full_pipe_name = f"\\\\.\\pipe\\{PIPE_NAME}"

    pipe_handle = win32pipe.CreateNamedPipe(
        full_pipe_name,

        win32pipe.PIPE_ACCESS_OUTBOUND,

        win32pipe.PIPE_TYPE_BYTE | win32pipe.PIPE_WAIT,

        1,                  # max instances
        64 * 1024,         # out buffer
        64 * 1024,         # in buffer
        0,                  # default timeout
        None                # security attributes
    )

    print(f"[Pipe] Created: {full_pipe_name}")
    print("[Pipe] Waiting for FFmpeg to connect...")

    return pipe_handle, full_pipe_name


# ============================================================
# Synthetic PCM Producer
# ============================================================

def synthetic_producer_thread(pipe_handle, stop_event, stats):
    """
    Write continuous f32le silence PCM to the named pipe
    at real-time rate.
    """

    import win32file

    # Pre-allocate silence buffer.
    # All zero bytes = 0.0f for f32le.
    chunk = b"\x00" * CHUNK_BYTES

    frames_written = 0
    bytes_written = 0

    start_time = time.perf_counter()
    next_write_time = start_time

    print(
        f"[Producer] Starting synthetic PCM: "
        f"{SAMPLE_RATE}Hz, {CHANNELS}ch, f32le"
    )

    print(
        f"[Producer] Chunk size: "
        f"{CHUNK_BYTES} bytes "
        f"({CHUNK_FRAMES} frames = 10ms)"
    )

    while not stop_event.is_set():

        # ----------------------------------------------------
        # Real-time pacing
        # ----------------------------------------------------

        now = time.perf_counter()

        if now < next_write_time:
            time.sleep(next_write_time - now)

        next_write_time += 0.010

        # ----------------------------------------------------
        # Write PCM chunk
        # ----------------------------------------------------

        try:
            win32file.WriteFile(
                pipe_handle,
                chunk
            )

            frames_written += CHUNK_FRAMES
            bytes_written += CHUNK_BYTES

            # ------------------------------------------------
            # Update stats every 100 chunks = ~1 second
            # ------------------------------------------------

            if frames_written % (CHUNK_FRAMES * 100) == 0:

                elapsed = time.perf_counter() - start_time

                stats["frames_written"] = frames_written
                stats["bytes_written"] = bytes_written
                stats["elapsed"] = elapsed

                stats["rate"] = (
                    frames_written / elapsed
                    if elapsed > 0
                    else 0
                )

                print(
                    f"[Producer] "
                    f"{frames_written} frames, "
                    f"{bytes_written} bytes, "
                    f"{elapsed:.1f}s, "
                    f"{frames_written / elapsed:.0f} fps"
                )

        except Exception as e:

            print(f"[Producer] Write error: {e}")

            break

    # --------------------------------------------------------
    # Final stats
    # --------------------------------------------------------

    elapsed = time.perf_counter() - start_time

    stats["frames_written"] = frames_written
    stats["bytes_written"] = bytes_written
    stats["elapsed"] = elapsed

    stats["rate"] = (
        frames_written / elapsed
        if elapsed > 0
        else 0
    )

    print(
        f"[Producer] Stopped. "
        f"Total: {frames_written} frames, "
        f"{bytes_written} bytes"
    )


# ============================================================
# FFmpeg Progress Parser
# ============================================================

def parse_progress(line):
    """Parse FFmpeg progress output."""

    metrics = {}

    patterns = {
        "frame": r"frame=\s*(\d+)",
        "fps": r"fps=\s*(\d+)",
        "dup": r"dup=\s*(\d+)",
        "drop": r"drop=\s*(\d+)",
        "speed": r"speed=\s*([\d.]+)x",
        "time": r"time=(\d{2}:\d{2}:\d{2}\.\d{2})",
        "size": r"size=\s*(\d+)",
    }

    for key, pattern in patterns.items():

        match = re.search(pattern, line)

        if match:
            metrics[key] = match.group(1)

    return metrics if metrics else None


# ============================================================
# Main
# ============================================================

def main():

    # --------------------------------------------------------
    # Import pywin32
    # --------------------------------------------------------

    try:
        import win32pipe
        import win32file
    except ImportError:

        print("ERROR: pywin32 not installed.")
        print("Run:")
        print("  pip install pywin32")

        sys.exit(1)

    # --------------------------------------------------------
    # Validate FFmpeg
    # --------------------------------------------------------

    if not os.path.isfile(FFMPEG_PATH):

        print("[ERROR] FFmpeg not found:")
        print(f"        {FFMPEG_PATH}")

        sys.exit(1)

    # --------------------------------------------------------
    # Create output directory
    # --------------------------------------------------------

    os.makedirs(
        OUTPUT_DIR,
        exist_ok=True
    )

    output_file = os.path.join(
        OUTPUT_DIR,
        "p1b_step3_named_pipe.mp4"
    )

    # --------------------------------------------------------
    # Create Named Pipe
    # --------------------------------------------------------

    pipe_handle, full_pipe_name = create_named_pipe()

    # --------------------------------------------------------
    # Build FFmpeg command
    # --------------------------------------------------------

    ffmpeg_args = [
        FFMPEG_PATH,

        "-hide_banner",
        "-loglevel",
        "info",

        # ----------------------------------------------------
        # Video input
        # ----------------------------------------------------

        "-f",
        "lavfi",

        "-i",
        f"ddagrab=output_idx=0:framerate={FPS}",

        # ----------------------------------------------------
        # Audio queue
        # ----------------------------------------------------

        "-thread_queue_size",
        "1024",

        # ----------------------------------------------------
        # Named pipe audio input
        # ----------------------------------------------------

        "-f",
        "f32le",

        "-ar",
        str(SAMPLE_RATE),

        "-ac",
        str(CHANNELS),

        "-i",
        full_pipe_name,

        # ----------------------------------------------------
        # NVIDIA NVENC
        # ----------------------------------------------------

        "-c:v",
        "h264_nvenc",

        "-preset",
        "p4",

        "-tune",
        "ll",

        "-rc",
        "cbr",

        "-b:v",
        str(BITRATE),

        "-minrate",
        str(BITRATE),

        "-maxrate",
        str(BITRATE),

        "-bufsize",
        str(BITRATE),

        # ----------------------------------------------------
        # GOP / FPS
        # ----------------------------------------------------

        "-g",
        str(FPS),

        "-fps_mode",
        "cfr",

        # ----------------------------------------------------
        # NVENC AQ
        # ----------------------------------------------------

        "-spatial-aq",
        "1",

        "-temporal-aq",
        "1",

        # ----------------------------------------------------
        # Audio encoding
        # ----------------------------------------------------

        "-c:a",
        "aac",

        "-b:a",
        "320k",

        "-ar",
        str(SAMPLE_RATE),

        # ----------------------------------------------------
        # MP4
        # ----------------------------------------------------

        "-movflags",
        "+faststart",

        "-y",

        output_file,
    ]

    print()
    print("=" * 70)
    print("P1-B STEP 3")
    print("Named Pipe + Synthetic PCM")
    print("=" * 70)

    print()
    print("[FFmpeg] Command:")
    print(" ".join(ffmpeg_args))

    print()
    print("[FFmpeg] Starting...")

    # --------------------------------------------------------
    # Start FFmpeg
    # --------------------------------------------------------

    proc = subprocess.Popen(
        ffmpeg_args,

        stderr=subprocess.PIPE,
        stdout=subprocess.DEVNULL,

        stdin=subprocess.PIPE,

        text=True,
        bufsize=1,
    )

    # --------------------------------------------------------
    # Wait for FFmpeg to connect
    # --------------------------------------------------------

    print("[Pipe] Waiting for connection...")

    try:

        win32pipe.ConnectNamedPipe(
            pipe_handle,
            None
        )

    except Exception as e:

        # ERROR_PIPE_CONNECTED = 535
        if getattr(e, "winerror", None) != 535:
            print(f"[Pipe] Connect error: {e}")

            try:
                proc.terminate()
            except Exception:
                pass

            raise

    print("[Pipe] Connected!")

    # --------------------------------------------------------
    # Start producer thread
    # --------------------------------------------------------

    stop_event = threading.Event()

    stats = {}

    producer = threading.Thread(
        target=synthetic_producer_thread,

        args=(
            pipe_handle,
            stop_event,
            stats,
        ),

        daemon=True,
    )

    producer.start()

    # --------------------------------------------------------
    # Read FFmpeg progress
    # --------------------------------------------------------

    start_time = time.time()

    last_progress = {}

    while True:

        elapsed = time.time() - start_time

        # ----------------------------------------------------
        # Recording duration reached
        # ----------------------------------------------------

        if elapsed >= RECORD_SECONDS:

            print()
            print(
                f"[FFmpeg] "
                f"Recording duration reached: "
                f"{RECORD_SECONDS}s"
            )

            try:

                proc.stdin.write("q\n")
                proc.stdin.flush()

            except Exception:

                try:
                    proc.terminate()
                except Exception:
                    pass

            break

        # ----------------------------------------------------
        # Read FFmpeg stderr
        # ----------------------------------------------------

        line = proc.stderr.readline()

        if not line:

            if proc.poll() is not None:
                break

            continue

        line = line.strip()

        # ----------------------------------------------------
        # FFmpeg progress line
        # ----------------------------------------------------

        if "frame=" in line:

            metrics = parse_progress(line)

            if metrics:

                last_progress = metrics

                print(
                    f"  [{elapsed:.1f}s] "
                    f"frame={metrics.get('frame', '?')} "
                    f"fps={metrics.get('fps', '?')} "
                    f"dup={metrics.get('dup', '?')} "
                    f"drop={metrics.get('drop', '?')} "
                    f"speed={metrics.get('speed', '?')} "
                    f"time={metrics.get('time', '?')}"
                )

    # --------------------------------------------------------
    # Stop producer
    # --------------------------------------------------------

    print()
    print("[Producer] Stopping...")

    stop_event.set()

    producer.join(
        timeout=5
    )

    # --------------------------------------------------------
    # Flush pipe
    # --------------------------------------------------------

    try:

        win32file.FlushFileBuffers(
            pipe_handle
        )

    except Exception:
        pass

    # --------------------------------------------------------
    # Disconnect pipe
    # --------------------------------------------------------

    try:

        win32pipe.DisconnectNamedPipe(
            pipe_handle
        )

    except Exception:
        pass

    # --------------------------------------------------------
    # Close pipe
    # --------------------------------------------------------

    try:

        win32file.CloseHandle(
            pipe_handle
        )

    except Exception:
        pass

    # --------------------------------------------------------
    # Wait for FFmpeg
    # --------------------------------------------------------

    print("[FFmpeg] Waiting for process to exit...")

    try:

        proc.wait(
            timeout=15
        )

    except Exception:

        print("[FFmpeg] Timeout. Killing process...")

        try:
            proc.kill()
        except Exception:
            pass

        proc.wait()

    exit_code = proc.returncode

    last_progress["exit_code"] = exit_code

    # --------------------------------------------------------
    # Read remaining FFmpeg output
    # --------------------------------------------------------

    try:

        remaining = proc.stderr.read()

    except Exception:

        remaining = ""

    for line in remaining.split("\n"):

        line = line.strip()

        if (
            "frame=" in line
            and "Lsize" in line
        ):

            metrics = parse_progress(line)

            if metrics:
                last_progress.update(metrics)

        if (
            "Qavg" in line
            or "exited" in line
            or "Exiting" in line
        ):

            print(f"  {line}")

    # ========================================================
    # Results
    # ========================================================

    print()
    print("=" * 60)
    print("STEP 3 RESULTS: Named Pipe + Synthetic PCM")
    print("=" * 60)

    print(
        f"  Final frame: "
        f"{last_progress.get('frame', 'N/A')}"
    )

    print(
        f"  FPS: "
        f"{last_progress.get('fps', 'N/A')}"
    )

    print(
        f"  dup: "
        f"{last_progress.get('dup', 'N/A')}"
    )

    print(
        f"  drop: "
        f"{last_progress.get('drop', 'N/A')}"
    )

    print(
        f"  speed: "
        f"{last_progress.get('speed', 'N/A')}x"
    )

    print(
        f"  time: "
        f"{last_progress.get('time', 'N/A')}"
    )

    print(
        f"  exit code: "
        f"{exit_code}"
    )

    print(
        f"  Producer frames: "
        f"{stats.get('frames_written', 'N/A')}"
    )

    print(
        f"  Producer bytes: "
        f"{stats.get('bytes_written', 'N/A')}"
    )

    producer_elapsed = stats.get(
        "elapsed",
        None
    )

    if producer_elapsed is not None:

        print(
            f"  Producer elapsed: "
            f"{producer_elapsed:.1f}s"
        )

    producer_rate = stats.get(
        "rate",
        None
    )

    if producer_rate is not None:

        print(
            f"  Producer rate: "
            f"{producer_rate:.0f} frames/sec"
        )

    print(
        f"  Output: "
        f"{output_file}"
    )

    # ========================================================
    # Comparison
    # ========================================================

    print()
    print("=" * 60)
    print("COMPARISON")
    print("=" * 60)

    print(
        f"{'Test':<30} "
        f"{'fps':<8} "
        f"{'dup':<8} "
        f"{'drop':<8} "
        f"{'speed':<8} "
        f"{'exit':<6}"
    )

    print("-" * 68)

    print(
        f"{'Video Only':<30} "
        f"{'143-144':<8} "
        f"{'1563':<8} "
        f"{'1':<8} "
        f"{'~1.0':<8} "
        f"{'0':<6}"
    )

    print(
        f"{'Video + Synth (lavfi)':<30} "
        f"{'143-144':<8} "
        f"{'1318':<8} "
        f"{'0':<8} "
        f"{'~1.0':<8} "
        f"{'0':<6}"
    )

    print(
        f"{'Video + Named Pipe':<30} "
        f"{str(last_progress.get('fps', '?')):<8} "
        f"{str(last_progress.get('dup', '?')):<8} "
        f"{str(last_progress.get('drop', '?')):<8} "
        f"{str(last_progress.get('speed', '?')):<8} "
        f"{str(exit_code):<6}"
    )

    print(
        f"{'Video + Live WASAPI':<30} "
        f"{'36-ish':<8} "
        f"{'3151':<8} "
        f"{'7':<8} "
        f"{'0.976':<8} "
        f"{'0':<6}"
    )

    print()
    print("=" * 60)
    print("TEST COMPLETE")
    print("=" * 60)


# ============================================================
# Entry Point
# ============================================================

if __name__ == "__main__":
    main()