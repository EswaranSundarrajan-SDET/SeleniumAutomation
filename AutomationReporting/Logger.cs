using Serilog;
using Serilog.Context;

namespace AutomationReporting;

/// <summary>Provides the shared logger for UI and API automation.</summary>
public static class Logger
{
    /// <summary>Writes structured events to automation.log in the report directory.</summary>
    public static void Start(string reportDirectory)
    {
        Directory.CreateDirectory(reportDirectory);
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("RunId", Guid.NewGuid())
            .Enrich.FromLogContext()
            .WriteTo.File(Path.Combine(reportDirectory, "automation.log"), shared: true,
                outputTemplate: "{Timestamp:O} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
            .CreateLogger();
    }

    /// <summary>Gets the logger for a source type, retaining any context from the supplied parent.</summary>
    public static ILogger ForContext<TSource>(ILogger? parent = null) =>
        (parent ?? Log.Logger).ForContext<TSource>();

    /// <summary>Gets the logger for a source type, including static classes.</summary>
    public static ILogger ForContext(Type source) => Log.ForContext(source);

    /// <summary>Adds a property to events in the current asynchronous scope.</summary>
    public static IDisposable PushProperty(string name, object? value) => LogContext.PushProperty(name, value);

    /// <summary>Flushes and closes the shared log at the end of the run.</summary>
    public static void CloseAndFlush() => Log.CloseAndFlush();
}
