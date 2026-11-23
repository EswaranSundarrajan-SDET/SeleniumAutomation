using System.Runtime.CompilerServices;
using AutomationReporting;
using AventStack.ExtentReports;
using NUnit.Framework;
using Reqnroll;
using Reqnroll.Events;

namespace AutomationTests.Hooks;

/// <summary>Runs the shared Reqnroll lifecycle and reporting for API and UI scenarios.</summary>
[Binding]
public class TestHooks(ScenarioSession session, ScenarioContext scenario, FeatureContext feature,
    ITestThreadExecutionEventPublisher executionEvents)
{
    private const string ReportEntryKey = nameof(TestHooks) + ".ReportEntry";
    private static readonly ConditionalWeakTable<ITestThreadExecutionEventPublisher, object> StepSubscriptions = new();
    private static readonly Lazy<string> ReportDirectory = new(() => Path.GetFullPath(
        Environment.GetEnvironmentVariable("REPORTS_DIR") ??
        Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "Reports")));
    private static readonly Lazy<ExtentReport> Report = new(() => new ExtentReport(ReportDirectory.Value));
    private ExtentTest? result;

    /// <summary>Starts a report entry before browser setup and includes scenario-outline arguments in its name.</summary>
    [BeforeScenario(Order = 0)]
    public void StartReport()
    {
        var arguments = scenario.ScenarioInfo.Arguments;
        var name = arguments == null || arguments.Count == 0 ? scenario.ScenarioInfo.Title :
            $"{scenario.ScenarioInfo.Title} ({string.Join(", ", arguments.Values.Cast<object>())})";
        result = Report.Value.StartScenario(feature.FeatureInfo.Title, name);
        scenario.Set(result, ReportEntryKey);
        SubscribeToStepResults();
    }

    /// <summary>Starts the browser for UI scenarios and enables downloads for scenarios tagged accordingly.</summary>
    [BeforeScenario("ui", Order = 10)]
    public void StartBrowser() => session.StartBrowser(scenario.ScenarioInfo.Tags.Contains("download"));

    /// <summary>Registers one shared step handler for each Reqnroll execution publisher.</summary>
    private void SubscribeToStepResults()
    {
        lock (StepSubscriptions)
        {
            if (StepSubscriptions.TryGetValue(executionEvents, out _))
                return;

            // Reqnroll publishes step results even when a failure prevents later step hooks from running.
            executionEvents.AddHandler<StepFinishedEvent>(RecordStep);
            StepSubscriptions.Add(executionEvents, new object());
        }
    }

    /// <summary>Logs each step's original keyword, expanded text, and execution status in its scenario entry.</summary>
    private static void RecordStep(StepFinishedEvent completed)
    {
        if (completed.ScenarioContext is not ScenarioContext context
            || !context.TryGetValue<ExtentTest>(ReportEntryKey, out var reportEntry))
            return;

        var step = completed.StepContext;
        var info = step.StepInfo;
        var keyword = info.StepInstance?.Keyword?.TrimEnd() ?? info.StepDefinitionType.ToString();
        var status = step.Status switch
        {
            ScenarioExecutionStatus.OK => Status.Pass,
            ScenarioExecutionStatus.Skipped or ScenarioExecutionStatus.StepDefinitionPending => Status.Skip,
            _ => Status.Fail
        };

        var detail = step.Status switch
        {
            ScenarioExecutionStatus.StepDefinitionPending => "Pending",
            ScenarioExecutionStatus.UndefinedStep => "Undefined step",
            _ => step.StepError?.Message
        };

        var message = $"{keyword} {info.Text}";
        Report.Value.Log(reportEntry, status, detail == null ? message : $"{message} ({detail})");
    }

    /// <summary>Records the outcome and guarantees that the scenario's browser cleanup runs.</summary>
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

    /// <summary>Flushes the shared HTML report when the test run finishes.</summary>
    [AfterTestRun]
    public static void WriteReport()
    {
        if (Report.IsValueCreated)
            Report.Value.Flush();
    }

    /// <summary>Attaches a browser screenshot when available and reports capture errors separately from the test failure.</summary>
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
