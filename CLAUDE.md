# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
# Build (debug)
dotnet build

# Run GUI in development mode (recommended for local iteration)
dotnet run -- --dev

# Publish single-file EXE (framework-dependent, ~206KB)
dotnet publish .\PhoenixToolkit.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish

# Run GUI
dotnet run

# Run CLI modes (used by scheduled tasks)
dotnet run -- --dev --fetch
dotnet run -- --dev --cleanup
```

Requires .NET 8 SDK. Target: `net8.0-windows10.0.17763.0`. The repo includes a local test harness project under `tests/PhoenixToolkit.Tests/`.

## Architecture

PhoenixToolkit is a Windows Forms app that automates fetching Phoenix installer packages from a network share and cleaning up old ones on a schedule.

**Runtime entry point** (`Program.cs`):
- **Release mode** (default): Published runtime behavior with protocol registration and scheduled-task support
- **Development mode** (`--dev`): Source-run workflow for fast local iteration with isolated config/state storage
- **CLI mode** (`--fetch` / `--cleanup`): Silent execution invoked by Windows Task Scheduler, and also available in dev mode for quick validation

**Service layer** (all static classes in `Services/`):
- `ConfigService` — JSON persistence via source-generated serializer (`AppConfigJsonContext`). Config file lives next to the EXE as `config.json`.
- `FetchService` — Copies latest Designer (`Phoenix-Windows-*.exe`) and Server (`Phoenix-Server-Windows-*.exe`) installers from network share to local dirs. Uses file stability check (5s size comparison) to avoid copying in-progress uploads. Version extracted from `FileVersionInfo.FilePrivatePart` (Designer) or filename timestamp regex (Server).
- `CleanupService` — Three-tier retention: keep all (weeks 0–3) → keep first build per day (weeks 3–6) → delete (weeks 6–9). Also cleans logs older than 30 days.
- `SchedulerService` — Creates/removes Windows scheduled tasks via `schtasks.exe` with XML task definitions. Task names: `PhoenixToolkit_Fetch`, `PhoenixToolkit_Cleanup`.
- `NotificationService` — Windows toast notifications when new packages are fetched.

**Configuration model** (`Models/AppConfig.cs`): Immutable C# record with defaults. Derived paths (`DesignerDir`, `ServerDir`, `LogDir`) computed from `LocalBaseDir`.

**Key constraint**: Release builds require Administrator elevation (`app.manifest` → `requireAdministrator`) because `schtasks.exe` needs it. Dev mode is intended for fast iteration and avoids release-only side effects such as task installation and automatic protocol registration.

## Conventions

- Immutable records for data models (C# `record` types)
- Source-generated JSON serialization (no reflection)
- All services are static — no DI container
- Namespace: `PhoenixToolkit` (root), `PhoenixToolkit.Services`, `PhoenixToolkit.Models`
- UI text is in Chinese (简体中文)
- Single NuGet dependency: `Microsoft.Toolkit.Uwp.Notifications` v7.1.3
