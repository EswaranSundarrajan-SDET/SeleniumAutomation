using AutomationReporting;
using AventStack.ExtentReports;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.Hooks;

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
    public void StartBrowser() => session.StartBrowser(scenario.ScenarioInfo.Tags.Contains("download"));

    [AfterStep]
    public void RecordStep()
    {
        if (result != null)
            Report.Value.Log(result, Status.Info, scenario.StepContext.StepInfo.Text);
    }

    [AfterScenario]
    public void FinishScenario()
    {
        try
        {
            if (result == null)
                return;

            if (scenario.TestError is Exception error)
            {
                Report.Value.Log(result, Status.Fail, error.ToString());
                CaptureFailure(result);
            }
            else if (scenario.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
            {
                Report.Value.Log(result, Status.Pass, "Scenario passed");
            }
            else
            {
                Report.Value.Log(result, Status.Skip, scenario.ScenarioExecutionStatus.ToString());
            }
        }
        finally
        {
            session.StopBrowser();
        }
    }

    [AfterTestRun]
    public static void WriteReport()
    {
        if (Report.IsValueCreated)
            Report.Value.Flush();
    }

    private void CaptureFailure(ExtentTest reportEntry)
    {
        try
        {
            var screenshot = session.CaptureScreenshot();
            if (screenshot == null)
                return;

            var file = Path.Combine(ReportDirectory.Value, $"{Guid.NewGuid():N}.png");
            screenshot.SaveAsFile(file);
            TestContext.AddTestAttachment(file, scenario.ScenarioInfo.Title);
            Report.Value.AttachScreenshot(reportEntry, screenshot.AsBase64EncodedString);
        }
        catch (Exception error)
        {
            TestContext.Progress.WriteLine($"Screenshot failed: {error.Message}");
        }
    }
}
