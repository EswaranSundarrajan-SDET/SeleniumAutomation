using Serilog;

namespace AutomationReporting;

/// <summary>Configures the shared file logger for an automation run.</summary>
public static class AutomationLogger
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
}
