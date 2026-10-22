using MyTestApp;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var appStartTime = DateTimeOffset.UtcNow;

// Security and caching headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    context.Response.Headers.Append("Pragma", "no-cache");
    await next();
});

// Root endpoint: renders server-side HTML system diagnostics or JSON if requested
app.MapGet("/", (HttpContext context) =>
{
    var info = SystemInfo.Collect(app.Environment.EnvironmentName, appStartTime);

    // Support content negotiation for JSON clients (e.g. curl -H "Accept: application/json")
    var accept = context.Request.Headers.Accept.ToString();
    if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase) &&
        !accept.Contains("text/html", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(info);
    }

    return Results.Content(HtmlRenderer.Render(info), "text/html; charset=utf-8");
});

// Dedicated JSON API endpoint
app.MapGet("/api/info", () =>
{
    var info = SystemInfo.Collect(app.Environment.EnvironmentName, appStartTime);
    return Results.Json(info);
});

// Container health check endpoints (supports both /health and /healthz; Cloud Run infrastructure reserves /healthz)
var healthCheck = () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
});

app.MapGet("/health", healthCheck);
app.MapGet("/healthz", healthCheck);

app.Run();

public partial class Program { }
