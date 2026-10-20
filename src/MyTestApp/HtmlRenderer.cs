using System.Net;

namespace MyTestApp;

public static class HtmlRenderer
{
    public static string Render(SystemInfo info)
    {
        string Encode(object? val) => WebUtility.HtmlEncode(val?.ToString() ?? string.Empty);

        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>System Information - MyTestApp</title>
            <style>
                :root {
                    --bg-gradient: linear-gradient(135deg, #f0f4f8 0%, #d9e2ec 100%);
                    --card-bg: #ffffff;
                    --text-primary: #102a43;
                    --text-secondary: #627d98;
                    --border-color: #e2e8f0;
                    --accent: #2563eb;
                    --accent-light: #dbeafe;
                    --badge-green-bg: #dcfce7;
                    --badge-green-text: #15803d;
                    --shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
                    --radius: 12px;
                }

                @media (prefers-color-scheme: dark) {
                    :root {
                        --bg-gradient: linear-gradient(135deg, #0b0f19 0%, #111827 100%);
                        --card-bg: #1f2937;
                        --text-primary: #f3f4f6;
                        --text-secondary: #9ca3af;
                        --border-color: #374151;
                        --accent: #3b82f6;
                        --accent-light: #1e3a8a;
                        --badge-green-bg: #064e3b;
                        --badge-green-text: #6ee7b7;
                        --shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.5);
                    }
                }

                * {
                    box-sizing: border-box;
                    margin: 0;
                    padding: 0;
                }

                body {
                    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
                    background: var(--bg-gradient);
                    color: var(--text-primary);
                    min-height: 100vh;
                    padding: 2rem 1rem;
                    display: flex;
                    flex-direction: column;
                    align-items: center;
                }

                .container {
                    width: 100%;
                    max-width: 900px;
                }

                header {
                    margin-bottom: 2rem;
                    text-align: center;
                }

                .header-badge {
                    display: inline-flex;
                    align-items: center;
                    gap: 0.5rem;
                    background-color: var(--badge-green-bg);
                    color: var(--badge-green-text);
                    padding: 0.25rem 0.75rem;
                    border-radius: 9999px;
                    font-size: 0.875rem;
                    font-weight: 600;
                    margin-bottom: 0.75rem;
                }

                .status-dot {
                    width: 8px;
                    height: 8px;
                    border-radius: 50%;
                    background-color: currentColor;
                }

                h1 {
                    font-size: 2rem;
                    font-weight: 700;
                    margin-bottom: 0.5rem;
                }

                p.subtitle {
                    color: var(--text-secondary);
                    font-size: 1rem;
                }

                .grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
                    gap: 1.5rem;
                    margin-bottom: 2rem;
                }

                @media (max-width: 500px) {
                    .grid {
                        grid-template-columns: 1fr;
                    }
                }

                .card {
                    background: var(--card-bg);
                    border: 1px solid var(--border-color);
                    border-radius: var(--radius);
                    padding: 1.5rem;
                    box-shadow: var(--shadow);
                }

                .card-title {
                    display: flex;
                    align-items: center;
                    gap: 0.5rem;
                    font-size: 1.15rem;
                    font-weight: 600;
                    margin-bottom: 1.25rem;
                    padding-bottom: 0.75rem;
                    border-bottom: 1px solid var(--border-color);
                }

                .card-icon {
                    font-size: 1.25rem;
                }

                .row {
                    display: flex;
                    justify-content: space-between;
                    align-items: flex-start;
                    padding: 0.5rem 0;
                    font-size: 0.925rem;
                    border-bottom: 1px dashed var(--border-color);
                }

                .row:last-child {
                    border-bottom: none;
                }

                .label {
                    color: var(--text-secondary);
                    font-weight: 500;
                }

                .value {
                    font-weight: 600;
                    text-align: right;
                    word-break: break-word;
                    max-width: 60%;
                }

                .pill {
                    display: inline-block;
                    background: var(--accent-light);
                    color: var(--accent);
                    padding: 0.15rem 0.5rem;
                    border-radius: 6px;
                    font-size: 0.825rem;
                    font-weight: 600;
                }

                footer {
                    text-align: center;
                    color: var(--text-secondary);
                    font-size: 0.875rem;
                    padding-top: 1rem;
                }

                footer a {
                    color: var(--accent);
                    text-decoration: none;
                    font-weight: 500;
                    margin: 0 0.5rem;
                }

                footer a:hover {
                    text-decoration: underline;
                }

                .actions {
                    margin-top: 1rem;
                    display: flex;
                    justify-content: center;
                    gap: 0.75rem;
                }

                button.refresh-btn {
                    background: var(--accent);
                    color: white;
                    border: none;
                    border-radius: 6px;
                    padding: 0.45rem 1rem;
                    font-size: 0.875rem;
                    font-weight: 500;
                    cursor: pointer;
                    transition: opacity 0.2s;
                }

                button.refresh-btn:hover {
                    opacity: 0.9;
                }
            </style>
        </head>
        <body>
            <div class="container">
                <header>
                    <div class="header-badge">
                        <span class="status-dot"></span>
                        <span>Service Online</span>
                    </div>
                    <h1>System Diagnostics</h1>
                    <p class="subtitle">Host environment, runtime platform &amp; timezone details</p>
                </header>

                <div class="grid">
                    <!-- Operating System -->
                    <div class="card">
                        <div class="card-title">
                            <span class="card-icon">&#x1F4BB;</span>
                            <span>Host Operating System</span>
                        </div>
                        <div class="row">
                            <span class="label">Operating System</span>
                            <span class="value">{{Encode(info.OperatingSystem)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">OS Architecture</span>
                            <span class="value"><span class="pill">{{Encode(info.OsArchitecture)}}</span></span>
                        </div>
                        <div class="row">
                            <span class="label">Platform</span>
                            <span class="value">{{Encode(info.Platform)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Host / Machine Name</span>
                            <span class="value">{{Encode(info.HostName)}}</span>
                        </div>
                    </div>

                    <!-- .NET Version -->
                    <div class="card">
                        <div class="card-title">
                            <span class="card-icon">&#x2699;&#xFE0F;</span>
                            <span>.NET Runtime</span>
                        </div>
                        <div class="row">
                            <span class="label">.NET Framework</span>
                            <span class="value"><span class="pill">{{Encode(info.DotNetVersion)}}</span></span>
                        </div>
                        <div class="row">
                            <span class="label">CLR Version</span>
                            <span class="value">{{Encode(info.ClrVersion)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Runtime Identifier</span>
                            <span class="value">{{Encode(info.RuntimeIdentifier)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Environment</span>
                            <span class="value">{{Encode(info.EnvironmentName)}}</span>
                        </div>
                    </div>

                    <!-- Timezone & Time -->
                    <div class="card">
                        <div class="card-title">
                            <span class="card-icon">&#x1F551;</span>
                            <span>Timezone &amp; Clock</span>
                        </div>
                        <div class="row">
                            <span class="label">Timezone ID</span>
                            <span class="value"><span class="pill">{{Encode(info.TimeZoneId)}}</span></span>
                        </div>
                        <div class="row">
                            <span class="label">Display Name</span>
                            <span class="value">{{Encode(info.TimeZoneDisplayName)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">UTC Offset</span>
                            <span class="value">{{Encode(info.UtcOffset)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Current Local Time</span>
                            <span class="value">{{Encode(info.CurrentLocalTime)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Current UTC Time</span>
                            <span class="value">{{Encode(info.CurrentUtcTime)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Daylight Saving</span>
                            <span class="value">{{(info.IsDaylightSavingTime ? "Active" : "Inactive")}}</span>
                        </div>
                    </div>

                    <!-- Host & Resources -->
                    <div class="card">
                        <div class="card-title">
                            <span class="card-icon">&#x1F6E1;&#xFE0F;</span>
                            <span>Environment &amp; Hardware</span>
                        </div>
                        <div class="row">
                            <span class="label">Containerized</span>
                            <span class="value"><span class="pill">{{(info.RunningInContainer ? "Yes (Docker / OCI)" : "No")}}</span></span>
                        </div>
                        <div class="row">
                            <span class="label">CPU Cores</span>
                            <span class="value">{{Encode(info.ProcessorCount)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Memory Working Set</span>
                            <span class="value">{{Encode(info.MemoryWorkingSet)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Available System Memory</span>
                            <span class="value">{{Encode(info.TotalAvailableMemory)}}</span>
                        </div>
                        <div class="row">
                            <span class="label">Process ID / Arch</span>
                            <span class="value">{{Encode(info.ProcessId)}} ({{Encode(info.ProcessArchitecture)}})</span>
                        </div>
                        <div class="row">
                            <span class="label">App Uptime</span>
                            <span class="value">{{Encode(info.AppUptime)}}</span>
                        </div>
                    </div>
                </div>

                <footer>
                    <p>MyTestApp &bull; Rendered at {{Encode(info.CurrentUtcTime)}}</p>
                    <div class="actions">
                        <button class="refresh-btn" onclick="window.location.reload()">Refresh</button>
                        <a href="/api/info" target="_blank">View JSON</a>
                        <a href="/healthz" target="_blank">Health Check</a>
                    </div>
                </footer>
            </div>
        </body>
        </html>
        """;
    }
}

