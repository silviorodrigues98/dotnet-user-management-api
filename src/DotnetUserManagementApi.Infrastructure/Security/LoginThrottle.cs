using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace DotnetUserManagementApi.Infrastructure.Security;

public sealed class LoginThrottleOptions
{
    public const string SectionName = "RateLimiting";

    public int MaxFailedAttempts { get; set; } = 5;

    public int WindowMinutes { get; set; } = 15;
}

public interface ILoginThrottle
{
    bool IsBlocked(string key);

    void RecordFailure(string key);

    void Reset(string key);
}

public sealed class InMemoryLoginThrottle(IOptions<LoginThrottleOptions> options) : ILoginThrottle
{
    private sealed record Entry(DateTime WindowStart, int FailedAttempts);

    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public bool IsBlocked(string key)
    {
        if (!_entries.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (IsExpired(entry))
        {
            _entries.TryRemove(key, out _);
            return false;
        }

        return entry.FailedAttempts >= options.Value.MaxFailedAttempts;
    }

    public void RecordFailure(string key)
    {
        var now = DateTime.UtcNow;

        _entries.AddOrUpdate(key,
            _ => new Entry(now, 1),
            (_, existing) => IsExpired(existing)
                ? new Entry(now, 1)
                : existing with { FailedAttempts = existing.FailedAttempts + 1 });
    }

    public void Reset(string key) => _entries.TryRemove(key, out _);

    private bool IsExpired(Entry entry) =>
        entry.WindowStart.AddMinutes(options.Value.WindowMinutes) < DateTime.UtcNow;
}