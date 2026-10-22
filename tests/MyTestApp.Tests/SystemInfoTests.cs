using Xunit;

namespace MyTestApp.Tests;

public class SystemInfoTests
{
    [Fact]
    public void Collect_ReturnsValidDiagnostics()
    {
        var startTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        var info = SystemInfo.Collect("Testing", startTime);

        Assert.NotNull(info);
        Assert.Equal("Testing", info.EnvironmentName);
        Assert.False(string.IsNullOrWhiteSpace(info.OperatingSystem));
        Assert.False(string.IsNullOrWhiteSpace(info.DotNetVersion));
        Assert.False(string.IsNullOrWhiteSpace(info.ClrVersion));
        Assert.False(string.IsNullOrWhiteSpace(info.HostName));
        Assert.False(string.IsNullOrWhiteSpace(info.TimeZoneId));
        Assert.False(string.IsNullOrWhiteSpace(info.GitCommitSha));
        Assert.False(string.IsNullOrWhiteSpace(info.GitBranch));
        Assert.True(info.ProcessorCount > 0);
        Assert.True(info.ProcessId > 0);
        Assert.Contains("5m", info.AppUptime);
    }

    [Theory]
    [InlineData("1234567890abcdef", "1234567")]
    [InlineData("1234567", "1234567")]
    [InlineData("1234", "1234")]
    public void ShortCommitSha_FormatsCorrectly(string commitSha, string expected)
    {
        var info = new SystemInfo
        {
            OperatingSystem = "Linux",
            OsArchitecture = "X64",
            Platform = "Unix",
            DotNetVersion = ".NET 10",
            ClrVersion = "10.0.0",
            RuntimeIdentifier = "linux-x64",
            TimeZoneId = "UTC",
            TimeZoneDisplayName = "UTC",
            UtcOffset = "+00:00",
            CurrentLocalTime = "2026-10-22 00:00:00",
            CurrentUtcTime = "2026-10-22 00:00:00 UTC",
            IsDaylightSavingTime = false,
            HostName = "host",
            UserName = "user",
            ProcessId = 1,
            ProcessArchitecture = "X64",
            ProcessorCount = 4,
            MemoryWorkingSet = "50 MB",
            TotalAvailableMemory = "1000 MB",
            RunningInContainer = true,
            EnvironmentName = "Testing",
            AppUptime = "1m 0s",
            AppStartTime = "2026-10-22 00:00:00 UTC",
            GitCommitSha = commitSha,
            BuildDate = "2026-10-22 00:00:00 UTC",
            GitBranch = "master",
            ReleaseVersion = null
        };

        Assert.Equal(expected, info.ShortCommitSha);
    }
}
