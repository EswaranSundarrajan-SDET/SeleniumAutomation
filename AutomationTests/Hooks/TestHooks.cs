using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutomationReporting;
using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;
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
    private const string StepStartedKey = nameof(TestHooks) + ".StepStarted";
    private const string ApiDetailsKey = nameof(TestHooks) + ".ApiDetails";
    private static readonly ConditionalWeakTable<ITestThreadExecutionEventPublisher, object> StepSubscriptions = new();
    private static readonly Lazy<string> ReportDirectory = new(ResolveReportDirectory);
    private static readonly Lazy<ExtentReport> Report = new(() => new ExtentReport(ReportDirectory.Value));
    private ScenarioReport? result;

    private static string ResolveReportDirectory()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable("REPORTS_DIR");
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
            return Path.GetFullPath(configuredDirectory);

        var projectDirectory = typeof(TestHooks).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "AutomationTestsProjectDirectory")?.Value;
        var parentDirectory = projectDirectory != null && Directory.Exists(projectDirectory)
            ? projectDirectory
            : TestContext.CurrentContext.TestDirectory;

        return Path.Combine(parentDirectory, "Reports");
    }

    /// <summary>Starts the shared logger before any scenario executes.</summary>
    [BeforeTestRun]
    public static void StartLogging()
    {
        Logger.Start(ReportDirectory.Value);
        Logger.ForContext<TestHooks>().Information("Automation run started");
    }

    /// <summary>Starts a categorized report entry before browser setup and includes scenario-outline arguments.</summary>
    [BeforeScenario(Order = 0)]
    public void StartReport()
    {
        var arguments = scenario.ScenarioInfo.Arguments;
        var name = arguments == null || arguments.Count == 0 ? scenario.ScenarioInfo.Title :
            $"{scenario.ScenarioInfo.Title} ({string.Join(", ", arguments.Values.Cast<object>())})";
        var logger = Logger.ForContext<TestHooks>()
            .ForContext("ScenarioId", Guid.NewGuid())
            .ForContext("Feature", feature.FeatureInfo.Title)
            .ForContext("Scenario", scenario.ScenarioInfo.Title);
        var category = GetReportCategory(scenario.ScenarioInfo.CombinedTags);
        result = new ScenarioReport(Report.Value.StartScenario(feature.FeatureInfo.Title, name, category), logger,
            category == "UI" ? session : null);
        scenario.Set(result, ReportEntryKey);
        logger.Information("Scenario started");
        SubscribeToStepResults();
    }

    private static string? GetReportCategory(string[] tags)
    {
        if (tags.Contains("ui", StringComparer.OrdinalIgnoreCase))
            return "UI";

        return tags.Contains("API", StringComparer.OrdinalIgnoreCase) ? "API" : null;
    }

    /// <summary>Starts the browser with the download and permission settings requested by the scenario.</summary>
    [BeforeScenario("ui", Order = 10)]
    public void StartBrowser()
    {
        var tags = scenario.ScenarioInfo.Tags;
        session.StartBrowser(tags.Contains("download"), tags.Contains("block-geolocation"), result?.Logger);
    }

    /// <summary>Registers the shared step handlers once for each Reqnroll execution publisher.</summary>
    private void SubscribeToStepResults()
    {
        lock (StepSubscriptions)
        {
            if (StepSubscriptions.TryGetValue(executionEvents, out _))
                return;

            // Reqnroll publishes step results even when a failure prevents later step hooks from running.
            executionEvents.AddHandler<StepStartedEvent>(RecordStepStart);
            executionEvents.AddHandler<StepFinishedEvent>(RecordStep);
            StepSubscriptions.Add(executionEvents, new object());
        }
    }

    private static void RecordStepStart(StepStartedEvent started)
    {
        if (started.StepContext is ScenarioStepContext step)
            step.Set(new StepStart(Stopwatch.GetTimestamp(), step.StepInfo.Table?.ToString()), StepStartedKey);
    }

    internal static void RecordApiDetails(ScenarioContext context, string details) =>
        context.StepContext.Set(details, ApiDetailsKey);

    /// <summary>Logs each completed step with its timing, binding, Gherkin arguments, and result.</summary>
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

        var diagnostic = step.Status switch
        {
            ScenarioExecutionStatus.Skipped when step.StepError == null => "A previous step or hook did not complete successfully.",
            ScenarioExecutionStatus.StepDefinitionPending when step.StepError == null => "The step definition is pending.",
            ScenarioExecutionStatus.UndefinedStep => "No matching step definition was found.",
            _ => step.StepError?.ToString()
        };

        var start = step is ScenarioStepContext stepContext && stepContext.TryGetValue<StepStart>(StepStartedKey, out var recorded)
            ? recorded
            : null;
        var didNotRun = step.Status == ScenarioExecutionStatus.UndefinedStep
            || (step.Status == ScenarioExecutionStatus.Skipped && step.StepError == null);
        TimeSpan? duration = start != null && !didNotRun ? Stopwatch.GetElapsedTime(start.Timestamp) : null;
        var (screenshot, screenshotNote) = CaptureStepScreenshot(entry);
        var apiDetails = step is ScenarioStepContext apiStep && apiStep.TryGetValue<string>(ApiDetailsKey, out var details)
            ? details
            : null;

        Report.Value.LogStep(entry.Report, status, $"{keyword} {info.Text}", duration,
            info.BindingMatch?.StepBinding?.Method.ToString(), start?.DataTable, info.MultilineText, diagnostic,
            screenshot?.AsBase64EncodedString, screenshotNote, apiDetails);
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
            Logger.ForContext<TestHooks>().Information("Automation run finished");
            Logger.CloseAndFlush();
        }
    }

    /// <summary>Attaches a browser screenshot when available and reports capture errors separately from the test failure.</summary>
    private void CaptureFailure(ScenarioReport entry)
    {
        try
        {
            var screenshot = session.CaptureScreenshot(out _);
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
            entry.Logger.Warning("Screenshot capture failed with {ErrorType} ({ErrorMessage})",
                error.GetType().Name, error.Message);
        }
    }

    private static (Screenshot? Image, string? Note) CaptureStepScreenshot(ScenarioReport entry)
    {
        if (entry.BrowserSession == null)
            return (null, null);

        try
        {
            var screenshot = entry.BrowserSession.CaptureScreenshot(out var alertText);
            if (screenshot != null)
            {
                entry.LastScreenshot = screenshot;
                return (screenshot, null);
            }

            if (alertText == null)
                return (null, "The browser was unavailable for this step.");

            var note = $"Browser alert: {alertText}";
            if (entry.LastScreenshot != null)
                note += " (Viewport captured before the alert.)";

            return (entry.LastScreenshot, note);
        }
        catch (Exception error)
        {
            entry.Logger.Warning("Step screenshot capture failed with {ErrorType} ({ErrorMessage})",
                error.GetType().Name, error.Message);
            return (null, "The browser could not capture a screenshot for this step.");
        }
    }

    private sealed record ScenarioReport(ExtentTest Report, ILogger Logger, ScenarioSession? BrowserSession)
    {
        public Screenshot? LastScreenshot { get; set; }
    }
    private sealed record StepStart(long Timestamp, string? DataTable);
}
