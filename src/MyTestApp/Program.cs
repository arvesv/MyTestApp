using MyTestApp;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var appStartTime = DateTimeOffset.UtcNow;

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

// Container health check endpoint
app.MapGet("/healthz", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.Run();
