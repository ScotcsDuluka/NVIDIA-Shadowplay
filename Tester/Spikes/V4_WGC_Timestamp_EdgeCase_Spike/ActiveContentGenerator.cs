// ActiveContentGenerator.cs — deterministic active content for V5 controlled tests
// SPDX-License-Identifier: MIT
// Spawns a small console window that updates its title bar text at ~60 Hz.
// This causes the desktop to change deterministically, which should trigger
// WGC to deliver frames. No external dependencies, no video files, no GPU load.
// The window itself is a simple .NET Console window whose title changes rapidly.
// This is a SPIKE-ONLY helper — not production code.

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal sealed class ActiveContentGenerator : IDisposable
{
    private Thread? _animationThread;
    private CancellationTokenSource _cts = new();
    private bool _running;

    /// <summary>
    /// Starts a deterministic animation loop that changes the console title
    /// at ~60 Hz. This forces the desktop to update, which WGC should detect
    /// and deliver as new frames.
    /// </summary>
    public void Start()
    {
        if (_running) return;
        _running = true;

        _animationThread = new Thread(() =>
        {
            long frame = 0;
            while (!_cts.Token.IsCancellationRequested)
            {
                // Change console title rapidly — this triggers desktop redraw
                Console.Title = $"V5 ACTIVE CONTENT — Frame {frame++} — {DateTime.UtcNow:HH:mm:ss.fff}";
                // Target ~60 Hz title update
                Thread.Sleep(16);
            }
        })
        {
            IsBackground = true,
            Name = "ActiveContentGen"
        };
        _animationThread.Start();
    }

    public void Stop()
    {
        if (!_running) return;
        _cts.Cancel();
        _running = false;
        _animationThread?.Join(2000);
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }
}
