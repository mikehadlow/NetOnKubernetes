
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;

var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "yyyy:MM:dd hh:mm:ss ";
    options.JsonWriterOptions = new JsonWriterOptions
    {
        // sometimes useful to change this to true when testing locally.
        // but it needs to be false for Fluent Bit to process log lines correctly
        Indented = false
    };
});
builder.WebHost.UseUrls("http://*:5432/");

var hasStarted = false;
var app = builder.Build();

// configure the Prometheus metrics endpoint, should appear at /metrics
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});

// liveness probe, return HTTP status code 500 if you want the container to be restarted
app.MapGet("/live", () => Results.Ok());
// rediness probe, return 200 OK when the application is ready to respond to requests.
// this can turn on and off if necessary, for example if a backend service is not available.
app.MapGet("/ready", () => hasStarted ? Results.Ok() : Results.StatusCode(500));

// PostStart and PreStop event hooks.
app.MapGet("/postStart", (ILogger<GreetingApp> logger)
    => logger.LogInformation("PostStart event"));
app.MapGet("/preStop", (ILogger<GreetingApp> logger)
    => logger.LogInformation("PreStop event"));

app.MapGet("/", (IConfiguration configuration) =>
{
    var message = configuration["GREETINGS_MESSAGE"]
        ?? "Hello World! env var not found.";
    return new Greeting(message);
});


app.Lifetime.ApplicationStarted.Register(() => hasStarted = true);
await app.RunAsync();



public record Greeting(string Message);
public class GreetingApp { }