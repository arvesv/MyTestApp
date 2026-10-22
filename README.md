# MyTestApp

A lightweight, server-rendered ASP.NET Core web application packaged in Docker. Designed to test and demonstrate containerization, environment diagnostics, and CI workflows.

---

## Features

- **Host Diagnostics:** Displays host operating system, architecture, platform, host/machine name, and process details.
- **.NET Runtime Info:** Shows .NET SDK/runtime version, CLR version, and runtime identifier.
- **Build & Version Telemetry:** Displays git commit SHA (with GitHub link), build date/time, branch name, and release version tag (if not on master).
- **Time & Timezone Details:** Shows configured timezone ID, display name, UTC offset, current local time, UTC time, and daylight saving time status.
- **System & Resource Stats:** Shows container status (Docker/OCI), CPU core count, memory working set, and application uptime.
- **Clean Modern UI:** Responsive CSS with dark/light mode support, zero external CDN dependencies (works completely offline).
- **Content Negotiation & JSON API:** Request JSON directly via `Accept: application/json` or visit `/api/info`.
- **Health Check Endpoint:** Includes `/healthz` for container orchestration and load balancers.
- **Container Ready:** Multi-stage `Dockerfile` with non-root security (`app` user) and `compose.yaml` support.
- **Automated CI/CD Pipelines:** GitHub Actions workflows covering build, test, multi-arch/container publishing, releases, and container registry cleanup.

---

## Endpoints

| Endpoint | Method | Format | Description |
| :--- | :--- | :--- | :--- |
| `/` | `GET` | HTML / JSON | Default dashboard showing OS, .NET version, timezone, git telemetry, and diagnostics. Returns JSON if `Accept: application/json` is sent. |
| `/api/info` | `GET` | JSON | Diagnostic and version telemetry information in machine-readable JSON format. |
| `/health` | `GET` | JSON | Health probe endpoint returning `{ "status": "Healthy" }` (Cloud Run & general health checks). |
| `/healthz` | `GET` | JSON | Kubernetes / container standard health probe endpoint returning `{ "status": "Healthy" }`. |

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
  "appStartTime": "2026-10-20 13:16:05 UTC",
  "gitCommitSha": "006420313cae697ce32fdbddcb0319ca03e07cb8",
  "buildDate": "2026-10-20T17:28:43Z",
  "gitBranch": "master",
  "gitTag": null,
  "releaseVersion": null,
  "shortCommitSha": "0064203"
}
```

> [!NOTE]
> `releaseVersion` is populated with the release tag (e.g. `"v0.1.1"`) when running on a release or non-master branch; on the untagged `master` branch, it is `null`.

---

## Running Locally

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### Run the App

```bash
# Build the solution
dotnet build

# Run unit and integration tests
dotnet test

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

The repository includes GitHub Actions workflows modeled after the [Nexus](https://github.com/arvesv/Nexus) project:

1. **CI Pipeline ([`.github/workflows/ci.yml`](.github/workflows/ci.yml))**:
   - Runs on pulls and pushes to `master` to validate the .NET build and execute container smoke tests using the shared [container smoke-test action](.github/actions/container-smoke-test/action.yml).
2. **Publish Container ([`.github/workflows/publish.yml`](.github/workflows/publish.yml))**:
   - Triggers on pushes to `master`, manual `workflow_dispatch`, or when invoked by the Release pipeline (`workflow_call`).
   - Collects Git commit SHA, branch, release tag, and build timestamp into container build args.
   - Runs container smoke test, extracts OCI metadata, builds the image, and publishes to **GitHub Container Registry (`ghcr.io/arvesv/mytestapp`)**.
   - Tags applied:
     - Push to `master`: branch `:master` and short `:sha-<commit>` (without overwriting `:latest`).
     - Release tag (e.g. `v0.1.0`): `:latest`, `:0.1.0`, `:0.1`, `:0`, `:v0.1.0`.
3. **Release Pipeline ([`.github/workflows/release.yml`](.github/workflows/release.yml))**:
   - Manual trigger via `workflow_dispatch` with semantic version bump (`patch`, `minor`, `major`, or `custom`).
   - Restores dependencies and builds the solution in `Release` mode.
   - Automatically bumps version in `src/MyTestApp/MyTestApp.csproj`, `compose.yaml`, and `README.md` using `scripts/bump-version.js`.
   - Tags the repository with `vX.Y.Z`, updates the `latest` branch to the release commit, creates a GitHub Release with auto-generated release notes, and triggers container publication via `publish.yml`.
4. **Deploy Pipeline ([`.github/workflows/deploy.yml`](.github/workflows/deploy.yml))**:
   - Deploys container image to **Google Cloud Run** in a region close to Oslo (`europe-north2` Stockholm or `europe-north1` Finland) using a GitHub Environment (`production` or `oslo`).
   - Configured via environment variables (`GCP_REGION`, `GCP_SERVICE_NAME`, `GCP_PROJECT_ID`).
   - Supports keyless Workload Identity Federation (`GCP_WORKLOAD_IDENTITY_PROVIDER` + `GCP_SERVICE_ACCOUNT`) and Service Account keys (`GCP_SA_KEY`).
   - Triggerable manually (`workflow_dispatch`), as a reusable step in the release pipeline (`workflow_call`), or automatically following successful container publishing on `master` (`workflow_run`).
   - Includes automatic post-deployment health check against `/health` (with `/api/info` fallback).
5. **Cleanup Untagged Images ([`.github/workflows/cleanup-untagged-images.yml`](.github/workflows/cleanup-untagged-images.yml))**:
   - Scheduled daily (01:17 UTC) or manual cleanup of untagged and stale `sha-*` images from GHCR using `dataaxiom/ghcr-cleanup-action@v1`.
   - Strictly excludes and preserves images that have release or branch tags (`latest,master,v*,*.*`).
6. **Dependabot ([`.github/dependabot.yml`](.github/dependabot.yml))**:
   - Weekly automated security and version updates for GitHub Actions, NuGet packages, and Dockerfile base images.

To pull and run the published container from GHCR:
```bash
docker run -d -p 8080:8080 ghcr.io/arvesv/mytestapp:0.1.3
```

---

## Project Structure

```text
MyTestApp/
├── .github/
│   ├── rulesets/
│   │   └── master-protection.json        # Branch protection ruleset export
│   ├── workflows/
│   │   ├── ci.yml                        # GitHub Actions CI pipeline
│   │   ├── publish.yml                   # Build & publish container to ghcr.io
│   │   ├── release.yml                   # Release workflow with version bump & GH release
│   │   ├── deploy.yml                    # Deploy container to GCP Cloud Run (near Oslo)
│   │   └── cleanup-untagged-images.yml   # Scheduled/manual GHCR untagged image cleanup
│   ├── actions/
│   │   └── container-smoke-test/
│   │       └── action.yml                # Shared container build and smoke test
│   └── dependabot.yml                    # Automated dependency update configuration
├── scripts/
│   └── bump-version.js                   # Semver bump script for csproj and doc references
├── src/
│   └── MyTestApp/
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── HtmlRenderer.cs               # Responsive HTML dashboard renderer
│       ├── MyTestApp.csproj              # ASP.NET Core project file
│       ├── Program.cs                    # Application entry point & endpoints
│       └── SystemInfo.cs                 # Diagnostics and system telemetry collector
├── tests/
│   └── MyTestApp.Tests/
│       ├── EndpointTests.cs              # Integration tests for HTTP & health endpoints
│       ├── HtmlRendererTests.cs          # Unit tests for HTML rendering & XSS safety
│       ├── MyTestApp.Tests.csproj        # xUnit test project
│       └── SystemInfoTests.cs            # Unit tests for diagnostics telemetry
├── .dockerignore                         # Files excluded from Docker builds
├── .gitattributes                        # Git line ending & attribute definitions
├── .gitignore                            # Files excluded from Git version control
├── compose.yaml                          # Local multi-container / compose definition
├── Dockerfile                            # Multi-stage container build definition
├── LICENSE
├── MyTestApp.sln                         # Solution file
├── README.md                             # Project documentation
└── agents.md                             # Guidelines for AI agents (commit attribution & conventions)
```
