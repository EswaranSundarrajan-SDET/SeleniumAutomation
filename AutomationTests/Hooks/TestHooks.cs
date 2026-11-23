using System.Runtime.CompilerServices;
using AutomationReporting;
using AventStack.ExtentReports;
using NUnit.Framework;
using Reqnroll;
using Reqnroll.Events;
using Serilog;
using Serilog.Events;

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
    private ScenarioReport? result;

    /// <summary>Starts the shared logger before any scenario executes.</summary>
    [BeforeTestRun]
    public static void StartLogging()
    {
        AutomationLogger.Start(ReportDirectory.Value);
        Log.Information("Automation run started");
    }

    /// <summary>Starts a report entry before browser setup and includes scenario-outline arguments in its name.</summary>
    [BeforeScenario(Order = 0)]
    public void StartReport()
    {
        var arguments = scenario.ScenarioInfo.Arguments;
        var name = arguments == null || arguments.Count == 0 ? scenario.ScenarioInfo.Title :
            $"{scenario.ScenarioInfo.Title} ({string.Join(", ", arguments.Values.Cast<object>())})";
        var logger = Log.ForContext<TestHooks>()
            .ForContext("ScenarioId", Guid.NewGuid())
            .ForContext("Feature", feature.FeatureInfo.Title)
            .ForContext("Scenario", scenario.ScenarioInfo.Title);
        result = new ScenarioReport(Report.Value.StartScenario(feature.FeatureInfo.Title, name), logger);
        scenario.Set(result, ReportEntryKey);
        logger.Information("Scenario started");
        SubscribeToStepResults();
    }

    /// <summary>Starts the browser for UI scenarios and enables downloads for scenarios tagged accordingly.</summary>
    [BeforeScenario("ui", Order = 10)]
    public void StartBrowser() => session.StartBrowser(scenario.ScenarioInfo.Tags.Contains("download"), result?.Logger);

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
            || !context.TryGetValue<ScenarioReport>(ReportEntryKey, out var entry))
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
        Report.Value.Log(entry.Report, status, detail == null ? message : $"{message} ({detail})");
        entry.Logger.Write(status == Status.Fail ? LogEventLevel.Error : LogEventLevel.Information,
            "Step {Keyword} ended with {StepStatus}", keyword, step.Status);
    }

    /// <summary>Records the outcome and guarantees that the scenario's browser cleanup runs.</summary>
    [AfterScenario]
    public void FinishScenario()
    {
        try
        {
            if (result != null && scenario.TestError is Exception error)
            {
                Report.Value.Log(result.Report, Status.Fail, error.ToString());
                result.Logger.Error("Scenario failed ({ErrorType})", error.GetType().Name);
                CaptureFailure(result);
            }
        }
        finally
        {
            try
            {
                session.StopBrowser();
            }
            catch (Exception error)
            {
                result?.Logger.Error("Scenario cleanup failed ({ErrorType})", error.GetType().Name);
                if (scenario.TestError == null)
                {
                    if (result != null)
                        Report.Value.Log(result.Report, Status.Fail, error.ToString());
                    throw;
                }
            }
        }

        if (result == null || scenario.TestError != null)
            return;

        if (scenario.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
        {
            Report.Value.Log(result.Report, Status.Pass, "Scenario passed");
            result.Logger.Information("Scenario passed");
        }
        else
        {
            Report.Value.Log(result.Report, Status.Skip, scenario.ScenarioExecutionStatus.ToString());
            result.Logger.Information("Scenario ended with {ScenarioStatus}", scenario.ScenarioExecutionStatus);
        }
    }

    /// <summary>Flushes the shared HTML report and logger when the test run finishes.</summary>
    [AfterTestRun]
    public static void WriteReport()
    {
        try
        {
            if (Report.IsValueCreated)
                Report.Value.Flush();
        }
        finally
        {
            Log.Information("Automation run finished");
            Log.CloseAndFlush();
        }
    }

    /// <summary>Attaches a browser screenshot when available and reports capture errors separately from the test failure.</summary>
    private void CaptureFailure(ScenarioReport entry)
    {
        try
        {
            var screenshot = session.CaptureScreenshot();
            if (screenshot == null)
                return;

            var file = Path.Combine(ReportDirectory.Value, $"{Guid.NewGuid():N}.png");
            screenshot.SaveAsFile(file);
            TestContext.AddTestAttachment(file, scenario.ScenarioInfo.Title);
            Report.Value.AttachScreenshot(entry.Report, screenshot.AsBase64EncodedString);
            entry.Logger.Information("Failure screenshot saved to {Screenshot}", Path.GetFileName(file));
        }
        catch (Exception error)
        {
            entry.Logger.Warning("Screenshot failed ({ErrorType})", error.GetType().Name);
            TestContext.Progress.WriteLine($"Screenshot failed: {error.Message}");
        }
    }

    private sealed record ScenarioReport(ExtentTest Report, ILogger Logger);
}
