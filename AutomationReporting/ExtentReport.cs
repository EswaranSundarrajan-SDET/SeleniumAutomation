using System.Net;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AutomationReporting;

/// <summary>Serializes ExtentReports updates from concurrent scenario workers.</summary>
public sealed class ExtentReport
{
    private readonly object reportLock = new();
    private readonly ExtentReports report = new();

    /// <summary>Creates the output directory and configures its ExtentReport.html reporter.</summary>
    public ExtentReport(string directory)
    {
        Directory.CreateDirectory(directory);
        var reporter = new ExtentSparkReporter(Path.Combine(directory, "ExtentReport.html"));
        report.AttachReporter(reporter);
    }

    /// <summary>Creates an HTML-encoded scenario entry under its feature category.</summary>
    public ExtentTest StartScenario(string feature, string name)
    {
        lock (reportLock)
            return report.CreateTest(WebUtility.HtmlEncode(name)).AssignCategory(WebUtility.HtmlEncode(feature));
    }

    /// <summary>Writes an HTML-encoded message and status to the scenario's entry.</summary>
    public void Log(ExtentTest scenario, Status status, string message)
    {
        lock (reportLock)
            scenario.Log(status, WebUtility.HtmlEncode(message));
    }

    /// <summary>Embeds a Base64-encoded failure screenshot in the scenario entry.</summary>
    public void AttachScreenshot(ExtentTest scenario, string base64)
    {
        lock (reportLock)
            scenario.AddScreenCaptureFromBase64String(base64, "Failure screenshot");
    }

    /// <summary>Writes pending report updates to the HTML file.</summary>
    public void Flush()
    {
        lock (reportLock)
            report.Flush();
    }
}
