// LoadGenerator.cs — optional synthetic CPU load for Test 5
// SPDX-License-Identifier: MIT
// Time-bounded, cancellable, no admin rights, no system/power changes.

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal sealed class LoadGenerator : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Thread> _threads = new();
    private readonly int _threadCount;
    private bool _running;

    public LoadGenerator(int threadCount = 4)
    {
        _threadCount = threadCount;
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        for (int i = 0; i < _threadCount; i++)
        {
            var t = new Thread(() =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // Busy-spin CPU work — prevents the thread from yielding
                    _ = Math.Sqrt(i * 12345.6789);
                }
            })
            {
                IsBackground = true,
                Name = $"LoadGen-{i}"
            };
            _threads.Add(t);
            t.Start();
        }
        Console.WriteLine($"  [LoadGen] Started {_threadCount} CPU stress threads.");
    }

    public void Stop()
    {
        if (!_running) return;
        _cts.Cancel();
        _running = false;
        foreach (var t in _threads)
        {
            if (!t.Join(2000))
                Console.WriteLine($"  [LoadGen] WARNING: thread {t.Name} did not join in 2s.");
        }
        _threads.Clear();
        Console.WriteLine("  [LoadGen] Stopped.");
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }
}
