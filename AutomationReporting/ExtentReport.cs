using System.Globalization;
using System.Net;
using System.Text;
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

    /// <summary>Writes a BDD step with its timing, binding, and expandable details.</summary>
    public void LogStep(ExtentTest scenario, Status status, string step, TimeSpan? duration, string? binding,
        string? dataTable, string? docString, string? diagnostic)
    {
        var html = new StringBuilder("<strong>").Append(WebUtility.HtmlEncode(step)).Append("</strong>");
        if (duration is TimeSpan elapsed)
        {
            html.Append("<div class='small'>Duration: ")
                .Append(elapsed.TotalMilliseconds.ToString("0.##", CultureInfo.InvariantCulture))
                .Append(" ms</div>");
        }
        if (binding != null)
            html.Append("<div class='small'>Step definition: ").Append(WebUtility.HtmlEncode(binding)).Append("</div>");

        AppendStepDetail(html, "Data table", dataTable);
        AppendStepDetail(html, "Doc string", docString);
        AppendStepDetail(html, "Result details", diagnostic);

        lock (reportLock)
            scenario.Log(status, html.ToString());
    }

    private static void AppendStepDetail(StringBuilder html, string label, string? content)
    {
        if (content != null)
        {
            html.Append("<details class='mt-1'><summary>").Append(label).Append("</summary><pre class='mb-1'>")
                .Append(WebUtility.HtmlEncode(content)).Append("</pre></details>");
        }
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
