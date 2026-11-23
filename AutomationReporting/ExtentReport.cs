using System.Net;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AutomationReporting;

public sealed class ExtentReport
{
    private readonly object reportLock = new();
    private readonly ExtentReports report = new();

    public ExtentReport(string directory)
    {
        Directory.CreateDirectory(directory);
        var reporter = new ExtentSparkReporter(Path.Combine(directory, "ExtentReport.html"));
        report.AttachReporter(reporter);
    }

    public ExtentTest StartScenario(string feature, string name)
    {
        lock (reportLock)
            return report.CreateTest(WebUtility.HtmlEncode(name)).AssignCategory(WebUtility.HtmlEncode(feature));
    }

    public void Log(ExtentTest scenario, Status status, string message)
    {
        lock (reportLock)
            scenario.Log(status, WebUtility.HtmlEncode(message));
    }

    public void AttachScreenshot(ExtentTest scenario, string base64)
    {
        lock (reportLock)
            scenario.AddScreenCaptureFromBase64String(base64, "Failure screenshot");
    }

    public void Flush()
    {
        lock (reportLock)
            report.Flush();
    }
}
