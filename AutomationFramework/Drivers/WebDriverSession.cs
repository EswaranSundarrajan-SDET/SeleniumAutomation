using AutomationFramework.Utilities;
using OpenQA.Selenium;
using Serilog;

namespace AutomationFramework.Drivers;

/// <summary>Owns one scenario's session in a browser provisioned by Selenium Manager.</summary>
public sealed class WebDriverSession : IDisposable
{
    private IWebDriver? driver;
    private ILogger? sessionLog;
    private bool disposed;

    /// <summary>Gets the active driver for this scenario.</summary>
    public IWebDriver Driver => driver ?? throw new InvalidOperationException("This scenario has no active browser.");

    /// <summary>Gets whether this manager has an active browser.</summary>
    public bool IsStarted => driver != null;

    /// <summary>Starts a browser using the configured browser and headless settings.</summary>
    public void Start(string? downloadDirectory = null, ILogger? logger = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (driver != null)
            throw new InvalidOperationException("The browser session has already been started.");

        var browser = ConfigHelper.Browser;
        var log = (logger ?? Log.Logger).ForContext<WebDriverSession>().ForContext("Browser", browser);
        try
        {
            var headless = ConfigHelper.Headless;
            log.Information("Starting WebDriver (headless: {Headless})", headless);
            driver = DriverFactory.Create(browser, headless, downloadDirectory);
            var webDriver = (WebDriver)driver;
            sessionLog = log.ForContext("WebDriverSessionId", webDriver.SessionId.ToString())
                .ForContext("BrowserVersion", webDriver.Capabilities.GetCapability("browserVersion"));
            sessionLog.Information("WebDriver started");
        }
        catch (Exception error)
        {
            log.Error("WebDriver startup failed ({ErrorType})", error.GetType().Name);
            throw;
        }
    }

    /// <summary>Captures the active browser when it supports screenshots.</summary>
    public Screenshot? CaptureScreenshot() => (driver as ITakesScreenshot)?.GetScreenshot();

    /// <summary>Stops the browser and disposes its driver service.</summary>
    public void Dispose()
    {
        disposed = true;
        var activeDriver = driver;
        var log = sessionLog;
        driver = null;
        sessionLog = null;
        if (activeDriver == null)
            return;

        try
        {
            activeDriver.Dispose();
            log?.Information("WebDriver stopped");
        }
        catch (Exception error)
        {
            log?.Error("WebDriver shutdown failed ({ErrorType})", error.GetType().Name);
            throw;
        }
    }
}
