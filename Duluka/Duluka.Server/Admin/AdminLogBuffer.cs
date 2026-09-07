using System.Collections.Concurrent;

namespace Duluka.Server.Admin;

/// <summary>A live in-memory log tail for the /admin operator console.
/// Every log record that passes the minimum level is appended to a bounded
/// ring buffer; the dashboard polls Since(lastSeq) for incremental streaming.
/// Purely additive — the regular console logging is untouched.</summary>
public sealed class AdminLogBuffer
{
    private readonly object _gate = new();
    private readonly List<AdminLogEntry> _ring = new();
    private long _seq;

    /// <summary>Maximum entries kept in memory (oldest are evicted).</summary>
    public int Capacity { get; }

    public AdminLogBuffer(int capacity = 4000) => Capacity = capacity;

    public void Add(LogLevel level, string category, string message, Exception? exception)
    {
        var text = exception is null
            ? message
            : $"{message} :: {exception.GetType().Name}: {exception.Message}";
        var entry = new AdminLogEntry(
            Seq: Interlocked.Increment(ref _seq),
            Time: DateTimeOffset.UtcNow.ToString("HH:mm:ss.fff"),
            Level: LevelName(level),
            Logger: ShortCategory(category),
            Message: text);
        lock (_gate)
        {
            _ring.Add(entry);
            if (_ring.Count > Capacity)
                _ring.RemoveRange(0, _ring.Count - Capacity);
        }
    }

    /// <summary>All entries with Seq greater than afterSeq, in order, plus
    /// the newest Seq at call time (the cursor for the next poll).</summary>
    public (IReadOnlyList<AdminLogEntry> Entries, long Last) Since(long afterSeq)
    {
        lock (_gate)
        {
            List<AdminLogEntry> entries = _ring.Count == 0 || _ring[^1].Seq <= afterSeq
                ? []
                : _ring.SkipWhile(e => e.Seq <= afterSeq).ToList();
            return (entries, _seq);
        }
    }

    private static string LevelName(LogLevel level) => level switch
    {
        LogLevel.Trace => "trace",
        LogLevel.Debug => "debug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "error",
        LogLevel.Critical => "critical",
        _ => "none",
    };

    private static string ShortCategory(string category)
    {
        var dot = category.LastIndexOf('.');
        return dot < 0 ? category : category[(dot + 1)..];
    }
}

/// <summary>ILoggerProvider that mirrors every enabled log record into the
/// AdminLogBuffer. Minimum level Information — Kestrel/ASP.NET debug noise
/// would otherwise flood the ring within seconds.</summary>
public sealed class AdminLogBufferProvider(AdminLogBuffer buffer) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new BufferLogger(buffer, categoryName);

    public void Dispose() { }

    private sealed class BufferLogger(AdminLogBuffer buffer, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            buffer.Add(logLevel, category, formatter(state, exception), exception);
        }
    }
}

public sealed record AdminLogEntry(long Seq, string Time, string Level, string Logger, string Message);
