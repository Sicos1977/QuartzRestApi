// ExportOpenApi – starts a minimal SchedulerHost, fetches /openapi/v1.json
// and writes it to the path given as the first argument (default: openapi.json).

using System.Collections.Specialized;
using System.Net.Http;
using Quartz;
using Quartz.Impl;
using QuartzRestApi;

var output = args.Length > 0 ? args[0] : "openapi.json";
const string address = "http://localhost:44399";

// Minimal in-memory scheduler – no real jobs needed, only the host metadata
var properties = new NameValueCollection
{
    ["quartz.scheduler.instanceName"] = "OpenApiExport",
    ["quartz.jobStore.type"]          = "Quartz.Simpl.RAMJobStore, Quartz",
    ["quartz.threadPool.type"]        = "Quartz.Simpl.SimpleThreadPool, Quartz"
};

var scheduler = new StdSchedulerFactory(properties).GetScheduler().Result;
await scheduler.Start();

var host = new SchedulerHost(address, scheduler, logger: null);
host.Start();

// Give Kestrel a moment to finish binding
await Task.Delay(500);

using var http = new HttpClient();
var json = await http.GetStringAsync($"{address}/openapi/v1.json");

await File.WriteAllTextAsync(output, json);
Console.WriteLine($"OpenAPI spec written to {output}");

host.Stop();
await scheduler.Shutdown();
