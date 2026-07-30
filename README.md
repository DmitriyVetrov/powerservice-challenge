# Coding Challenge: Power Position report

> ### 🚀 Blazor Server web application
>
> A **Blazor Server web application** for this challenge lives in a separate branch:
> **[`feature/blazor-frontend`](https://github.com/DmitriyVetrov/powerservice-challenge/tree/feature/blazor-frontend)**.
> It adds a web UI on top of the worker service — check it out there.

## Overview Requirements 

**Intra-day Day-Ahead Power Position Report** — Developed a .NET 10 Worker Service that generates automated day-ahead power position reports for energy traders. The service runs on a configurable schedule (and once at startup), retrieves day-ahead trades from the trading system, aggregates power volumes by delivery hour, and exports the results to CSV files `PowerPosition_yyyyMMdd_HHmm.csv` using the local extraction time. Implemented configurable scheduling, background processing, dependency injection, logging, and robust error handling to ensure reliable automated report generation. See full [requirements](https://github.com/DmitriyVetrov/powerservice-challenge/blob/main/docs/requirements.md)

## Build

```
dotnet build
```

## Test

```
dotnet test
```

## Run

```
dotnet run --project PowerPosition.Worker
```

Runs an extract at startup, then every `IntervalMinutes`. Ctrl+C to stop.

The commands above work on any OS the .NET 10 SDK supports (Windows, macOS,
Linux). The systemd instructions under [Logs](#logs) are Linux-specific.

## Settings

| Key | Default | Meaning |
|---|---|---|
| `Extract:IntervalMinutes` | `1` | Minutes between extracts. Must be 1–1440. |
| `Extract:OutputPath` | `reports` | Folder for the CSV files. Created if missing; relative to the current directory. |

From `appsettings.json`. Override on the command line (highest priority) or by
environment variable — validated at startup, bad values fail immediately.

```
dotnet run --project PowerPosition.Worker -- --Extract:IntervalMinutes 5 --Extract:OutputPath ./reports-prod
```

```
Extract__IntervalMinutes=5 Extract__OutputPath=./reports-prod dotnet run --project PowerPosition.Worker
```

Published build — same flags, no `--` separator:

```
dotnet publish PowerPosition.Worker -o out
./out/PowerPosition.Worker --Extract:IntervalMinutes 5 --Extract:OutputPath ./reports-prod
```

## Logs

Logging goes to the console (see `Logging` in `appsettings.json`) — there is no
built-in file sink. To keep logs on disk when running manually, redirect
stdout/stderr yourself, e.g.:

```
mkdir -p logs
dotnet run --project PowerPosition.Worker >> logs/power-position-$(date +%Y%m%d).log 2>&1
```

If the worker runs as a systemd service instead (Linux only), systemd captures
stdout/stderr into the journal — there's no log file to redirect, so read it
with `journalctl`:

```
journalctl -u power-position.service -f       # follow live
journalctl -u power-position.service --since today
```

(Replace `power-position.service` with the actual unit name.)

For Windows Service you first need to add the
`Microsoft.Extensions.Hosting.WindowsServices` package and call
`UseWindowsService()` in `Program.cs` — the project doesn't have that yet, so
out of the box it won't run as a Windows service. Until that's added, run it
manually with the stdout redirect above, or wrap it with a third-party process
manager like NSSM.

## Notes

- **DST**: period times are stepped in 1-hour UTC increments from 23:00
  Europe/London on the previous day to 23:00 on the trading day, so a day has 23,
  24, or 25 periods — never a hardcoded 24.
- **Channel**: a bounded producer/consumer channel decouples the schedule timer
  from the extract, so a slow trading-system call delays a tick but never drops
  one (requirement 7).
- **Retry**: the trading-system fetch is retried with a fixed 3-attempt
  exponential backoff (2s then 4s); after that the extract is logged and skipped.
