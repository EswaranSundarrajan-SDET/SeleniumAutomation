using AutomationFramework.Drivers;
using AutomationFramework.Utilities;
using AutomationReporting;
using OpenQA.Selenium;
using Serilog;

namespace AutomationTests.Hooks;

/// <summary>Owns the optional browser and download directory for a single scenario.</summary>
public sealed class ScenarioSession
{
    private IWebDriver? driver;
    private DirectoryInfo? downloads;
    private ILogger? browserLog;
    private bool stopped;

    /// <summary>Gets the started browser and rejects access before initialization or after cleanup.</summary>
    public IWebDriver Driver => driver ?? throw new InvalidOperationException("This scenario has no active browser.");

    /// <summary>Gets the temporary download directory for a scenario with download support.</summary>
    public string DownloadDirectory => downloads?.FullName
        ?? throw new InvalidOperationException("This scenario has no download directory.");

    /// <summary>Starts an isolated browser with the settings requested by this scenario.</summary>
    internal void StartBrowser(bool enableDownloads, bool blockGeolocation, ILogger? logger)
    {
        ObjectDisposedException.ThrowIf(stopped, this);
        if (driver != null || downloads != null)
            throw new InvalidOperationException("The browser session has already been started.");

        var browser = ConfigHelper.Browser;
        var log = Logger.ForContext<ScenarioSession>(logger).ForContext("Browser", browser);
        try
        {
            var headless = ConfigHelper.Headless;
            log.Information("Starting WebDriver (headless: {Headless})", headless);
            if (enableDownloads)
                downloads = Directory.CreateTempSubdirectory("selenium-download-");

            var activeDriver = DriverFactory.Create(browser, headless, downloads?.FullName, blockGeolocation);
            driver = activeDriver;
            browserLog = log.ForContext("WebDriverSessionId", activeDriver.SessionId.ToString())
                .ForContext("BrowserVersion", activeDriver.Capabilities.GetCapability("browserVersion"));
            browserLog.Information("WebDriver started");
        }
        catch (Exception error)
        {
            log.Error("WebDriver startup failed ({ErrorType})", error.GetType().Name);
            throw;
        }
    }

    /// <summary>Captures the viewport or returns the text of an open browser alert.</summary>
    internal Screenshot? CaptureScreenshot(out string? alertText)
    {
        alertText = null;
        if (driver is not ITakesScreenshot camera)
            return null;

        try
        {
            // A screenshot command can dismiss a native alert that the next step needs to verify.
            alertText = driver.SwitchTo().Alert().Text;
            return null;
        }
        catch (NoAlertPresentException)
        {
            return camera.GetScreenshot();
        }
    }

    /// <summary>Disposes the active driver and deletes temporary downloads even if driver disposal fails.</summary>
    internal void StopBrowser()
    {
        stopped = true;
        var activeDriver = driver;
        var log = browserLog;
        var downloadDirectory = downloads;
        driver = null;
        browserLog = null;
        downloads = null;

        try
        {
            activeDriver?.Dispose();
            log?.Information("WebDriver stopped");
        }
        catch (Exception error)
        {
            log?.Error("WebDriver shutdown failed ({ErrorType})", error.GetType().Name);
            throw;
        }
        finally
        {
            downloadDirectory?.Delete(recursive: true);
        }
    }
}
