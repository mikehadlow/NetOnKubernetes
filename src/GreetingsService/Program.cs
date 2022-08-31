using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Prometheus;
using Microsoft.AspNetCore.Http;

namespace GreetingsService;

public static class Program
{
    public static void Main()
    {
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

        var hasStarted = false;
        var app = builder.Build();
        
        // Listen to 5432
        app.Urls.Add("http://*:5432");

        // configure the Prometheus metrics endpoint, should appear at /metrics
        app.MapMetrics();
        
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

        app.Start();

        // mark as true when complex startup logic has completed and the service is ready to take requests.
        hasStarted = true;
        
        app.WaitForShutdown();
    }
}

public record Greeting(string Message);

public class GreetingApp { }
