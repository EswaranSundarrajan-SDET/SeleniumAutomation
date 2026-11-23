using AutomationFramework.Drivers;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;

namespace AutomationFramework.Hooks;

public class ScenarioSession
{
    public IWebDriver Driver { get; internal set; } = null!;
}

[Binding]
public class TestHooks(ScenarioSession session, ScenarioContext scenario)
{
    [BeforeScenario("ui")]
    public void Start() => session.Driver = DriverFactory.Create();

    [AfterScenario("ui")]
    public void Stop()
    {
        try
        {
            if (scenario.TestError != null && session.Driver is ITakesScreenshot camera)
            {
                var folder = Environment.GetEnvironmentVariable("REPORTS_DIR") ??
                    Path.Combine(TestContext.CurrentContext.WorkDirectory, "Reports");
                Directory.CreateDirectory(folder);
                var file = Path.GetFullPath(Path.Combine(folder, $"{Guid.NewGuid():N}.png"));
                camera.GetScreenshot().SaveAsFile(file);
                TestContext.AddTestAttachment(file, scenario.ScenarioInfo.Title);
            }
        }
        catch (Exception error)
        {
            TestContext.Progress.WriteLine($"Screenshot failed: {error.Message}");
        }
        finally
        {
            session.Driver?.Dispose();
        }
    }
}
