using System.Net;
using AutomationFramework.Drivers;
using AutomationFramework.Reporting;
using AutomationFramework.UI;
using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;

namespace AutomationTests.Hooks;

public class ScenarioSession
{
    public IWebDriver Driver { get; internal set; } = null!;
    public DirectoryInfo? Downloads { get; internal set; }
}

[Binding]
public class TestHooks(ScenarioSession session, ScenarioContext scenario, FeatureContext feature)
{
    private static readonly Lazy<string> ReportDirectory = new(() => Path.GetFullPath(
        Environment.GetEnvironmentVariable("REPORTS_DIR") ??
        Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "Reports")));
    private static readonly Lazy<ExtentReport> Report = new(() => new ExtentReport(ReportDirectory.Value));
    private ExtentTest? result;

    [BeforeScenario(Order = 0)]
    public void StartReport()
    {
        var arguments = scenario.ScenarioInfo.Arguments;
        var name = arguments == null || arguments.Count == 0 ? scenario.ScenarioInfo.Title :
            $"{scenario.ScenarioInfo.Title} ({string.Join(", ", arguments.Values.Cast<object>())})";
        result = Report.Value.StartScenario(feature.FeatureInfo.Title, name);
    }

    [BeforeScenario("ui", Order = 10)]
    public void StartBrowser()
    {
        if (scenario.ScenarioInfo.Tags.Contains("download"))
            session.Downloads = Directory.CreateTempSubdirectory("selenium-download-");
        session.Driver = DriverFactory.Create(session.Downloads?.FullName);
    }

    [AfterStep]
    public void RecordStep() => result?.Info(WebUtility.HtmlEncode(scenario.StepContext.StepInfo.Text));

    [AfterScenario]
    public void FinishScenario()
    {
        try
        {
            if (result == null)
                return;

            if (scenario.TestError is Exception error)
            {
                result.Fail(WebUtility.HtmlEncode(error.ToString()));
                CaptureFailure();
            }
            else if (scenario.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
            {
                result.Pass("Scenario passed");
            }
            else
            {
                result.Skip(scenario.ScenarioExecutionStatus.ToString());
            }
        }
        finally
        {
            try
            {
                session.Driver?.Dispose();
            }
            finally
            {
                session.Downloads?.Delete(recursive: true);
            }
        }
    }

    [AfterTestRun]
    public static void WriteReport()
    {
        if (Report.IsValueCreated)
            Report.Value.Flush();
    }

    private void CaptureFailure()
    {
        if (session.Driver is not ITakesScreenshot)
            return;

        try
        {
            var screenshot = new BrowserActions(session.Driver).CaptureScreenshot();
            var file = Path.Combine(ReportDirectory.Value, $"{Guid.NewGuid():N}.png");
            screenshot.SaveAsFile(file);
            TestContext.AddTestAttachment(file, scenario.ScenarioInfo.Title);
            result!.AddScreenCaptureFromBase64String(screenshot.AsBase64EncodedString, "Failure screenshot");
        }
        catch (Exception error)
        {
            TestContext.Progress.WriteLine($"Screenshot failed: {error.Message}");
        }
    }
}
