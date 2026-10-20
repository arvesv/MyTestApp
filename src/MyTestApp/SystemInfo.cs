using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyTestApp;

public record SystemInfo
{
    // Host Operating System
    public required string OperatingSystem { get; init; }
    public required string OsArchitecture { get; init; }
    public required string Platform { get; init; }

    // .NET Version
    public required string DotNetVersion { get; init; }
    public required string ClrVersion { get; init; }
    public required string RuntimeIdentifier { get; init; }

    // Timezone & Time
    public required string TimeZoneId { get; init; }
    public required string TimeZoneDisplayName { get; init; }
    public required string UtcOffset { get; init; }
    public required string CurrentLocalTime { get; init; }
    public required string CurrentUtcTime { get; init; }
    public required bool IsDaylightSavingTime { get; init; }

    // Other Useful Diagnostics
    public required string HostName { get; init; }
    public required string UserName { get; init; }
    public required int ProcessId { get; init; }
    public required string ProcessArchitecture { get; init; }
    public required int ProcessorCount { get; init; }
    public required string MemoryWorkingSet { get; init; }
    public required string TotalAvailableMemory { get; init; }
    public required bool RunningInContainer { get; init; }
    public required string EnvironmentName { get; init; }
    public required string AppUptime { get; init; }
    public required string AppStartTime { get; init; }

    public static SystemInfo Collect(string environmentName, DateTimeOffset startTime)
    {
        var localZone = TimeZoneInfo.Local;
        var nowLocal = DateTimeOffset.Now;
        var nowUtc = DateTimeOffset.UtcNow;
        var uptime = nowUtc - startTime;

        var gcInfo = GC.GetGCMemoryInfo();
        var totalAvailableMb = gcInfo.TotalAvailableMemoryBytes > 0
            ? $"{gcInfo.TotalAvailableMemoryBytes / (1024 * 1024):N0} MB"
            : "N/A";

        var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                          || File.Exists("/.dockerenv");

        var offset = localZone.GetUtcOffset(nowLocal);
        var formattedOffset = $"{(offset >= TimeSpan.Zero ? "+" : "-")}{offset.Duration():hh\\:mm}";

        return new SystemInfo
        {
            OperatingSystem = RuntimeInformation.OSDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            Platform = Environment.OSVersion.Platform.ToString(),

            DotNetVersion = RuntimeInformation.FrameworkDescription,
            ClrVersion = Environment.Version.ToString(),
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,

            TimeZoneId = localZone.Id,
            TimeZoneDisplayName = localZone.DisplayName,
            UtcOffset = formattedOffset,
            CurrentLocalTime = nowLocal.ToString("yyyy-MM-dd HH:mm:ss zzz"),
            CurrentUtcTime = nowUtc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            IsDaylightSavingTime = localZone.IsDaylightSavingTime(nowLocal),

            HostName = Environment.MachineName,
            UserName = Environment.UserName,
            ProcessId = Environment.ProcessId,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            MemoryWorkingSet = $"{Environment.WorkingSet / (1024 * 1024):N0} MB",
            TotalAvailableMemory = totalAvailableMb,
            RunningInContainer = isContainer,
            EnvironmentName = environmentName,
            AppUptime = FormatTimeSpan(uptime),
            AppStartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
        };
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalHours >= 1)
            return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds}s";
        return $"{ts.Seconds}s";
    }
}
