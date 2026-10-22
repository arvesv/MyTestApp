using Xunit;

namespace MyTestApp.Tests;

public class HtmlRendererTests
{
    private static SystemInfo CreateSampleInfo(string? releaseVersion = null, string branch = "master")
    {
        return new SystemInfo
        {
            OperatingSystem = "Ubuntu 24.04 LTS",
            OsArchitecture = "X64",
            Platform = "Unix",
            DotNetVersion = ".NET 10.0.0",
            ClrVersion = "10.0.0",
            RuntimeIdentifier = "linux-x64",
            TimeZoneId = "Europe/Oslo",
            TimeZoneDisplayName = "(UTC+01:00) Oslo",
            UtcOffset = "+02:00",
            CurrentLocalTime = "2026-10-22 07:00:00 +02:00",
            CurrentUtcTime = "2026-10-22 05:00:00 UTC",
            IsDaylightSavingTime = true,
            HostName = "my-host",
            UserName = "test-user",
            ProcessId = 1234,
            ProcessArchitecture = "X64",
            ProcessorCount = 8,
            MemoryWorkingSet = "64 MB",
            TotalAvailableMemory = "16,384 MB",
            RunningInContainer = true,
            EnvironmentName = "Production",
            AppUptime = "12m 34s",
            AppStartTime = "2026-10-22 04:47:26 UTC",
            GitCommitSha = "abcdef1234567890",
            BuildDate = "2026-10-22 04:45:00 UTC",
            GitBranch = branch,
            ReleaseVersion = releaseVersion
        };
    }

    [Fact]
    public void Render_RendersAllCoreSections()
    {
        var info = CreateSampleInfo();
        var html = HtmlRenderer.Render(info);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("System Diagnostics", html);
        Assert.Contains("Build &amp; Version", html);
        Assert.Contains("Host Operating System", html);
        Assert.Contains(".NET Runtime", html);
        Assert.Contains("Timezone &amp; Clock", html);
        Assert.Contains("Environment &amp; Hardware", html);
        Assert.Contains("Ubuntu 24.04 LTS", html);
        Assert.Contains("my-host", html);
        Assert.Contains("1234", html);
        Assert.Contains("Yes (Docker / OCI)", html);
    }

    [Fact]
    public void Render_WithReleaseVersion_RendersReleaseBadgeAndLink()
    {
        var info = CreateSampleInfo(releaseVersion: "v0.1.2");
        var html = HtmlRenderer.Render(info);

        Assert.Contains("Release Version", html);
        Assert.Contains("v0.1.2", html);
        Assert.Contains("/releases/tag/v0.1.2", html);
    }

    [Fact]
    public void Render_WithoutReleaseVersion_DoesNotRenderReleaseRow()
    {
        var info = CreateSampleInfo(releaseVersion: null);
        var html = HtmlRenderer.Render(info);

        Assert.DoesNotContain("Release Version", html);
        Assert.Contains("pill-branch", html);
    }

    [Fact]
    public void Render_EncodesPotentiallyUnsafeValues()
    {
        var info = new SystemInfo
        {
            OperatingSystem = "<script>alert('xss')</script>",
            OsArchitecture = "X64",
            Platform = "Unix",
            DotNetVersion = ".NET 10",
            ClrVersion = "10.0",
            RuntimeIdentifier = "linux-x64",
            TimeZoneId = "UTC",
            TimeZoneDisplayName = "UTC",
            UtcOffset = "+00:00",
            CurrentLocalTime = "2026-10-22 00:00:00",
            CurrentUtcTime = "2026-10-22 00:00:00 UTC",
            IsDaylightSavingTime = false,
            HostName = "<hostname>",
            UserName = "\"quote\" & 'amp'",
            ProcessId = 1,
            ProcessArchitecture = "X64",
            ProcessorCount = 1,
            MemoryWorkingSet = "10 MB",
            TotalAvailableMemory = "100 MB",
            RunningInContainer = false,
            EnvironmentName = "Test",
            AppUptime = "1s",
            AppStartTime = "2026-10-22 00:00:00 UTC",
            GitCommitSha = "abcdef123456",
            BuildDate = "2026-10-22 00:00:00 UTC",
            GitBranch = "feat/<script>",
            ReleaseVersion = null
        };

        var html = HtmlRenderer.Render(info);

        Assert.DoesNotContain("<script>alert('xss')</script>", html);
        Assert.Contains("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", html);
        Assert.Contains("&lt;hostname&gt;", html);
    }
}
