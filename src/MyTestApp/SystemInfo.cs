using System.Diagnostics;
using System.Reflection;
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

    // Build & Git Version Telemetry
    public required string GitCommitSha { get; init; }
    public required string BuildDate { get; init; }
    public required string GitBranch { get; init; }
    public string? ReleaseVersion { get; init; }

    public string ShortCommitSha => GitCommitSha.Length > 7 ? GitCommitSha[..7] : GitCommitSha;

    private record StaticMetadata(
        string OperatingSystem,
        string OsArchitecture,
        string Platform,
        string DotNetVersion,
        string ClrVersion,
        string RuntimeIdentifier,
        string HostName,
        string UserName,
        int ProcessId,
        string ProcessArchitecture,
        int ProcessorCount,
        bool RunningInContainer,
        string GitCommitSha,
        string BuildDate,
        string GitBranch,
        string? ReleaseVersion
    );

    private static readonly StaticMetadata StaticData = InitStaticMetadata();

    private static StaticMetadata InitStaticMetadata()
    {
        var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                          || File.Exists("/.dockerenv")
                          || File.Exists("/run/.containerenv");

        static string? GetEnv(string name)
        {
            var val = Environment.GetEnvironmentVariable(name)?.Trim();
            return string.IsNullOrEmpty(val) ? null : val;
        }

        var appVersion = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

        // 1. Commit SHA
        var commitSha = GetEnv("GIT_COMMIT_SHA");
        if (string.IsNullOrEmpty(commitSha))
        {
            var infoVer = typeof(Program).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(infoVer) && infoVer.Contains('+'))
            {
                commitSha = infoVer.Split('+')[1].Trim();
            }
        }
        if (string.IsNullOrEmpty(commitSha))
        {
            commitSha = TryRunGit("rev-parse HEAD") ?? "unknown";
        }

        // 2. Build Date
        var buildTime = GetEnv("BUILD_TIME");
        if (string.IsNullOrEmpty(buildTime))
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath))
            {
                buildTime = File.GetLastWriteTimeUtc(assemblyPath).ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            }
            else
            {
                buildTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            }
        }

        // 3. Branch Name
        var branch = GetEnv("GIT_BRANCH") ?? TryRunGit("rev-parse --abbrev-ref HEAD") ?? "master";
        if (branch.StartsWith("refs/heads/"))
        {
            branch = branch["refs/heads/".Length..];
        }

        // 4. Tag / Release Version
        var tag = GetEnv("GIT_TAG");
        if (string.IsNullOrEmpty(tag))
        {
            tag = TryRunGit("describe --tags --exact-match");
        }

        // Determine if running on master branch:
        // Considered on master if branch is "master" and not built from an explicit release tag
        bool isOnMaster = string.Equals(branch, "master", StringComparison.OrdinalIgnoreCase)
                          && string.IsNullOrWhiteSpace(tag);

        string? releaseVersion = null;
        if (!isOnMaster)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                releaseVersion = tag.StartsWith('v') ? tag : $"v{tag}";
            }
            else
            {
                releaseVersion = $"v{appVersion}";
            }
        }

        return new StaticMetadata(
            OperatingSystem: RuntimeInformation.OSDescription,
            OsArchitecture: RuntimeInformation.OSArchitecture.ToString(),
            Platform: Environment.OSVersion.Platform.ToString(),
            DotNetVersion: RuntimeInformation.FrameworkDescription,
            ClrVersion: Environment.Version.ToString(),
            RuntimeIdentifier: RuntimeInformation.RuntimeIdentifier,
            HostName: Environment.MachineName,
            UserName: Environment.UserName,
            ProcessId: Environment.ProcessId,
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount: Environment.ProcessorCount,
            RunningInContainer: isContainer,
            GitCommitSha: commitSha,
            BuildDate: buildTime,
            GitBranch: branch,
            ReleaseVersion: releaseVersion
        );
    }

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

        var offset = localZone.GetUtcOffset(nowLocal);
        var formattedOffset = $"{(offset >= TimeSpan.Zero ? "+" : "-")}{offset.Duration():hh\\:mm}";

        return new SystemInfo
        {
            OperatingSystem = StaticData.OperatingSystem,
            OsArchitecture = StaticData.OsArchitecture,
            Platform = StaticData.Platform,

            DotNetVersion = StaticData.DotNetVersion,
            ClrVersion = StaticData.ClrVersion,
            RuntimeIdentifier = StaticData.RuntimeIdentifier,

            TimeZoneId = localZone.Id,
            TimeZoneDisplayName = localZone.DisplayName,
            UtcOffset = formattedOffset,
            CurrentLocalTime = nowLocal.ToString("yyyy-MM-dd HH:mm:ss zzz"),
            CurrentUtcTime = nowUtc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            IsDaylightSavingTime = localZone.IsDaylightSavingTime(nowLocal),

            HostName = StaticData.HostName,
            UserName = StaticData.UserName,
            ProcessId = StaticData.ProcessId,
            ProcessArchitecture = StaticData.ProcessArchitecture,
            ProcessorCount = StaticData.ProcessorCount,
            MemoryWorkingSet = $"{Environment.WorkingSet / (1024 * 1024):N0} MB",
            TotalAvailableMemory = totalAvailableMb,
            RunningInContainer = StaticData.RunningInContainer,
            EnvironmentName = environmentName,
            AppUptime = FormatTimeSpan(uptime),
            AppStartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),

            GitCommitSha = StaticData.GitCommitSha,
            BuildDate = StaticData.BuildDate,
            GitBranch = StaticData.GitBranch,
            ReleaseVersion = StaticData.ReleaseVersion
        };
    }

    private static string? TryRunGit(string arguments)
    {
        try
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            if (proc.Start())
            {
                var readTask = proc.StandardOutput.ReadToEndAsync();
                if (proc.WaitForExit(1000))
                {
                    var output = readTask.GetAwaiter().GetResult().Trim();
                    if (proc.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        return output;
                    }
                }
                else
                {
                    try
                    {
                        proc.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                        // Ignore if process already exited or cannot be killed
                    }
                }
            }
        }
        catch
        {
            // Ignore if git command fails or is not available in environment
        }
        return null;
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalSeconds < 0)
            return "0s";
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalHours >= 1)
            return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds}s";
        return $"{ts.Seconds}s";
    }
}
