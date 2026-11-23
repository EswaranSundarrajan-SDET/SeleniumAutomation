using AutomationFramework.API;
using AutomationFramework.Drivers;
using AutomationFramework.Pages;
using AutomationFramework.TestSupport;
using AutomationFramework.Utilities;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;

namespace AutomationFramework.Hooks;

public class ScenarioSession
{
    public TestData Data { get; } = ConfigHelper.LoadData();
    public string Reference { get; } = $"SET-{Guid.NewGuid():N}";
    public Uri BaseUri { get; internal set; } = null!;
    public LoginPage Login { get; internal set; } = null!;
    public SettlementPage Settlement { get; internal set; } = null!;
    public ApiClient Api { get; internal set; } = null!;
}

[Binding]
public class TestHooks(ScenarioSession session, ScenarioContext scenario)
{
    private WebApplication? app;
    private IWebDriver? driver;
    private HttpClient? client;

    [BeforeScenario]
    public async Task StartAsync()
    {
        app = DemoApp.Create();
        await app.StartAsync();
        session.BaseUri = new Uri(app.Urls.Single());
        client = new HttpClient { BaseAddress = session.BaseUri };
        session.Api = new ApiClient(client);
        driver = DriverFactory.Create();
        session.Login = new LoginPage(driver);
        session.Settlement = new SettlementPage(driver);
    }

    [AfterScenario]
    public async Task StopAsync()
    {
        try
        {
            if (scenario.TestError != null && driver is ITakesScreenshot camera)
            {
                var folder = Environment.GetEnvironmentVariable("REPORTS_DIR") ??
                    Path.Combine(TestContext.CurrentContext.WorkDirectory, "Reports");
                Directory.CreateDirectory(folder);
                var file = Path.GetFullPath(Path.Combine(folder, $"{session.Reference}.png"));
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
            try { driver?.Dispose(); }
            finally
            {
                client?.Dispose();
                if (app != null) await app.DisposeAsync();
            }
        }
    }
}
