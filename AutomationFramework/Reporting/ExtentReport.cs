using System.Net;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AutomationFramework.Reporting;

public sealed class ExtentReport
{
    private readonly ExtentReports report = new();

    public ExtentReport(string directory)
    {
        Directory.CreateDirectory(directory);
        var reporter = new ExtentSparkReporter(Path.Combine(directory, "ExtentReport.html"));
        report.AttachReporter(reporter);
    }

    public ExtentTest StartScenario(string feature, string name) =>
        report.CreateTest(WebUtility.HtmlEncode(name)).AssignCategory(WebUtility.HtmlEncode(feature));

    public void Flush() => report.Flush();
}
