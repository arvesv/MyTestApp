# Agent Guidelines

This document outlines guidelines, conventions, and operational instructions for AI agents (including Antigravity) working within this repository.

---

## Git Commit Attribution

Any Git commits created by or with the assistance of **Antigravity** must include the following co-author trailer at the end of the commit message:

```text
Co-authored-by: Antigravity <antigravity@users.noreply.github.com>
```

### Commit Message Format

```text
<type>(<scope>): <short description>

<optional body explaining the change>

Co-authored-by: Antigravity <antigravity@users.noreply.github.com>
```

#### Example

```text
feat(diagnostics): add memory telemetry to system diagnostics

Collects total available GC memory alongside process working set.

Co-authored-by: Antigravity <antigravity@users.noreply.github.com>
```

---

## Repository Overview

- **Platform & Language**: .NET 10.0 (C#)
- **Project Type**: ASP.NET Core Minimal API (server-rendered web application)
- **Solution File**: [`MyTestApp.sln`](MyTestApp.sln)
- **Application Source**: [`src/MyTestApp/`](src/MyTestApp/)
  - Entry point: [`Program.cs`](src/MyTestApp/Program.cs)
  - System diagnostics collection: [`SystemInfo.cs`](src/MyTestApp/SystemInfo.cs)
  - Responsive HTML rendering: [`HtmlRenderer.cs`](src/MyTestApp/HtmlRenderer.cs)
- **Containerization**: Multi-stage [`Dockerfile`](Dockerfile) with non-root user (`app`)
- **Orchestration**: [`compose.yaml`](compose.yaml)
- **CI Workflow**: [`.github/workflows/ci.yml`](.github/workflows/ci.yml)

---

## Development & Verification Commands

| Action | Command |
| :--- | :--- |
| **Restore** | `dotnet restore` |
| **Build** | `dotnet build` |
| **Run locally** | `dotnet run --project src/MyTestApp` |
| **Build container** | `docker build -t mytestapp:latest .` |
| **Run container** | `docker run -d -p 8080:8080 --name mytestapp mytestapp:latest` |
| **Health check** | `curl -f http://localhost:8080/healthz` |
| **JSON telemetry** | `curl -f http://localhost:8080/api/info` |

---

## Agent Coding Standards

1. **Lightweight & Self-Contained**: Use built-in .NET BCL and ASP.NET Core capabilities. Avoid unnecessary external package dependencies.
2. **Offline & Air-gapped Friendly**: Do not depend on external CDNs or external web services for styling or scripts in the web application.
3. **Container Security**: Always follow container best practices, such as using multi-stage builds and running with the non-root `$APP_UID` user.
4. **Documentation**: Keep [`README.md`](README.md) and [`agents.md`](agents.md) synchronized with any changes to build steps, endpoints, or environment variables.

