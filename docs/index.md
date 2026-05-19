---
_layout: landing
---

# QuartzRestApi

A self-hosted REST API library for [Quartz.NET](https://www.quartz-scheduler.net/), built on **.NET 10** with **ASP.NET Core / Kestrel**.

QuartzRestApi wraps your existing `IScheduler` instance behind an HTTP endpoint so other processes — on the same machine or across a network — can manage jobs and triggers without needing a direct Quartz.NET reference.

## Features

- **Zero-config hosting** — one line to expose your scheduler over HTTP
- **Typed C# client** — `SchedulerConnector` covers the full endpoint surface
- **Optional API key authentication** — opt-in, with named profiles and per-route whitelisting
- **Interactive docs** — built-in Scalar / OpenAPI reference at `/scalar/v1`
- **Full Quartz.NET parity** — jobs, triggers, calendars, pause/resume, interrupt, error reset and more

## Quick start

```csharp
// Start the API host
var host = new SchedulerHost("http://localhost:44344", scheduler, logger);
host.Start();

// Connect from another process
var connector = new SchedulerConnector("http://localhost:44344");
var name = await connector.GetSchedulerName();
```

## Secured quick start

```csharp
// Host with a single API key — grants full access
var host = new SchedulerHost("http://localhost:44344", scheduler, logger,
    apiKey: "my-secret-key");

// Client that sends the key automatically
var connector = new SchedulerConnector("http://localhost:44344",
    apiKey: "my-secret-key");

// Or use named profiles with factory methods
var admin    = ApiKeyProfile.AllowAll("Admin", "key-admin");

var readOnly = ApiKeyProfile.DenyAll("ReadOnly", "key-readonly");
readOnly.GetMetaData   = true;
readOnly.GetJobKeys    = true;
readOnly.GetJobDetail  = true;
// ...enable any other read endpoints as needed

var hostWithProfiles = new SchedulerHost("http://localhost:44344", scheduler, logger,
    profiles: [admin, readOnly]);
```

## Links

- [NuGet package](https://www.nuget.org/packages/QuartzRestApi)
- [GitHub repository](https://github.com/Sicos1977/QuartzRestApi)
- [Full README](https://github.com/Sicos1977/QuartzRestApi/blob/main/README.md)
- [API Reference](api/)
