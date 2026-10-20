# MyTestApp

A lightweight, server-rendered ASP.NET Core web application packaged in Docker. Designed to test and demonstrate containerization, environment diagnostics, and CI workflows.

---

## Features

- **Host Diagnostics:** Displays host operating system, architecture, platform, host/machine name, and process details.
- **.NET Runtime Info:** Shows .NET SDK/runtime version, CLR version, and runtime identifier.
- **Time & Timezone Details:** Shows configured timezone ID, display name, UTC offset, current local time, UTC time, and daylight saving time status.
- **System & Resource Stats:** Shows container status (Docker/OCI), CPU core count, memory working set, and application uptime.
- **Clean Modern UI:** Responsive CSS with dark/light mode support, zero external CDN dependencies (works completely offline).
- **Content Negotiation & JSON API:** Request JSON directly via `Accept: application/json` or visit `/api/info`.
- **Health Check Endpoint:** Includes `/healthz` for container orchestration and load balancers.
- **Container Ready:** Multi-stage `Dockerfile` with non-root security (`app` user) and `docker-compose.yml` support.
- **Automated CI Pipeline:** GitHub Actions workflow covering restore, build, container image creation, and automated smoke testing.

---

## Endpoints

| Endpoint | Method | Format | Description |
| :--- | :--- | :--- | :--- |
| `/` | `GET` | HTML / JSON | Default dashboard showing OS, .NET version, timezone, and diagnostics. Returns JSON if `Accept: application/json` is sent. |
| `/api/info` | `GET` | JSON | Diagnostic information in machine-readable JSON format. |
| `/healthz` | `GET` | JSON | Health probe endpoint returning `{ "status": "Healthy" }`. |

### Sample JSON Output (`/api/info`)

```json
{
  "operatingSystem": "Ubuntu 24.04.5 LTS",
  "osArchitecture": "X64",
  "platform": "Unix",
  "dotNetVersion": ".NET 10.0.12",
  "clrVersion": "10.0.12",
  "runtimeIdentifier": "linux-x64",
  "timeZoneId": "UTC",
  "timeZoneDisplayName": "(UTC) Coordinated Universal Time",
  "utcOffset": "+00:00",
  "currentLocalTime": "2026-10-20 13:17:37 +00:00",
  "currentUtcTime": "2026-10-20 13:17:37 UTC",
  "isDaylightSavingTime": false,
  "hostName": "mytestapp",
  "userName": "app",
  "processId": 1,
  "processArchitecture": "X64",
  "processorCount": 2,
  "memoryWorkingSet": "71 MB",
  "totalAvailableMemory": "7,941 MB",
  "runningInContainer": true,
  "environmentName": "Production",
  "appUptime": "1m 32s",
  "appStartTime": "2026-10-20 13:16:05 UTC"
}
```

---

## Running Locally

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### Run the App

```bash
# Build the solution
dotnet build

# Run the web application
dotnet run --project src/MyTestApp
```

The application will start on `http://localhost:5037` (or the URL specified in `src/MyTestApp/Properties/launchSettings.json`).

---

## Running with Docker

### Prerequisites

- [Docker](https://www.docker.com/)

### Build and Run Container

```bash
# 1. Build the Docker image
docker build -t mytestapp:latest .

# 2. Run container on port 8080
docker run -d -p 8080:8080 --name mytestapp mytestapp:latest

# 3. View the dashboard
open http://localhost:8080
# Or using curl:
curl http://localhost:8080/api/info
```

### Run with Docker Compose

```bash
docker compose up --build
```

To stop the container:
```bash
docker compose down
```

---

## Continuous Integration & Delivery (CI/CD)

The repository includes GitHub Actions workflows:

1. **CI Pipeline ([`.github/workflows/ci.yml`](.github/workflows/ci.yml))**:
   - Runs on pulls and pushes to validate the .NET build and execute container smoke tests using the shared [container smoke-test action](.github/actions/container-smoke-test/action.yml).
2. **Publish Container ([`.github/workflows/publish.yml`](.github/workflows/publish.yml))**:
   - Triggers on pushes to `master` and version tags (`v*`).
   - Runs container smoke test, extracts OCI metadata, builds the image, and publishes to **GitHub Container Registry (`ghcr.io/arvesv/mytestapp`)**.
   - Reuses the same container smoke-test action as CI before publishing.
   - Tags applied:
     - Push to `master`: `latest`, `sha-<short>`
     - Release tag (e.g. `v1.0.0`): `1.0.0`, `1.0`, `1`, `v1.0.0`, `sha-<short>`

To pull and run the published container from GHCR:
```bash
docker run -d -p 8080:8080 ghcr.io/arvesv/mytestapp:latest
```

---

## Project Structure

```text
MyTestApp/
├── .github/
│   ├── rulesets/
│   │   └── master-protection.json # Branch protection ruleset export
│   ├── workflows/
│   │   ├── ci.yml             # GitHub Actions CI pipeline
│   │   └── publish.yml        # Build & publish container to ghcr.io
│   ├── actions/
│       └── container-smoke-test/
│           └── action.yml     # Shared container build and smoke test
│   └── dependabot.yml         # Automated dependency update configuration
├── src/
│   └── MyTestApp/
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── HtmlRenderer.cs    # Responsive HTML dashboard renderer
│       ├── MyTestApp.csproj   # ASP.NET Core project file
│       ├── Program.cs         # Application entry point & endpoints
│       └── SystemInfo.cs      # Diagnostics and system telemetry collector
├── .dockerignore              # Files excluded from Docker builds
├── .gitattributes             # Git line ending & attribute definitions
├── .gitignore                 # Files excluded from Git version control
├── compose.yaml               # Local multi-container / compose definition
├── Dockerfile                 # Multi-stage container build definition
├── LICENSE
├── MyTestApp.sln              # Solution file
├── README.md                  # Project documentation
└── agents.md                  # Guidelines for AI agents (commit attribution & conventions)
```
