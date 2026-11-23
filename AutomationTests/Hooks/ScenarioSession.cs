using AutomationFramework.Drivers;
using AutomationFramework.UI;
using OpenQA.Selenium;

namespace AutomationTests.Hooks;

/// <summary>Owns the optional browser and download directory for a single scenario.</summary>
public sealed class ScenarioSession
{
    private IWebDriver? driver;
    private DirectoryInfo? downloads;

    /// <summary>Gets the started browser and rejects access before initialization or after cleanup.</summary>
    public IWebDriver Driver => driver
        ?? throw new InvalidOperationException("This scenario has no active browser.");

    /// <summary>Gets the temporary download directory for a scenario with download support.</summary>
    public string DownloadDirectory => downloads?.FullName
        ?? throw new InvalidOperationException("This scenario has no download directory.");

    /// <summary>Starts the scenario's only browser and allocates a unique download directory when needed.</summary>
    internal void StartBrowser(bool enableDownloads)
    {
        if (driver != null || downloads != null)
            throw new InvalidOperationException("The browser session has already been started.");

        if (enableDownloads)
            downloads = Directory.CreateTempSubdirectory("selenium-download-");

        driver = DriverFactory.Create(downloads?.FullName);
    }

    /// <summary>Captures the active browser if screenshots are supported.</summary>
    internal Screenshot? CaptureScreenshot() =>
        driver is ITakesScreenshot ? new BrowserActions(driver).CaptureScreenshot() : null;

    /// <summary>Disposes the active driver and deletes temporary downloads even if driver disposal fails.</summary>
    internal void StopBrowser()
    {
        var activeDriver = driver;
        var downloadDirectory = downloads;
        driver = null;
        downloads = null;

        try
        {
            activeDriver?.Dispose();
        }
        finally
        {
            downloadDirectory?.Delete(recursive: true);
        }
    }
}
