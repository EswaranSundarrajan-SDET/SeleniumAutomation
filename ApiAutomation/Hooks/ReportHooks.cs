using System.Net;
using AutomationReporting;
using AventStack.ExtentReports;
using NUnit.Framework;
using Reqnroll;

namespace ApiAutomation.Hooks;

[Binding]
public class ReportHooks(ScenarioContext scenario, FeatureContext feature)
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

    [AfterStep]
    public void RecordStep() => result?.Info(WebUtility.HtmlEncode(scenario.StepContext.StepInfo.Text));

    [AfterScenario]
    public void FinishScenario()
    {
        if (result == null)
            return;

        if (scenario.TestError is Exception error)
            result.Fail(WebUtility.HtmlEncode(error.ToString()));
        else if (scenario.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
            result.Pass("Scenario passed");
        else
            result.Skip(scenario.ScenarioExecutionStatus.ToString());
    }

    [AfterTestRun]
    public static void WriteReport()
    {
        if (Report.IsValueCreated)
            Report.Value.Flush();
    }
}
