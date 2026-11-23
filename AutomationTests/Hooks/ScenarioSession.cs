using AutomationFramework.Drivers;
using OpenQA.Selenium;
using Serilog;

namespace AutomationTests.Hooks;

/// <summary>Owns the optional browser and download directory for a single scenario.</summary>
public sealed class ScenarioSession
{
    private readonly WebDriverSession browser = new();
    private DirectoryInfo? downloads;

    /// <summary>Gets the started browser and rejects access before initialization or after cleanup.</summary>
    public IWebDriver Driver => browser.Driver;

    /// <summary>Gets the temporary download directory for a scenario with download support.</summary>
    public string DownloadDirectory => downloads?.FullName
        ?? throw new InvalidOperationException("This scenario has no download directory.");

    /// <summary>Starts the scenario's only browser and allocates a unique download directory when needed.</summary>
    internal void StartBrowser(bool enableDownloads, ILogger? logger)
    {
        if (browser.IsStarted || downloads != null)
            throw new InvalidOperationException("The browser session has already been started.");

        if (enableDownloads)
            downloads = Directory.CreateTempSubdirectory("selenium-download-");

        browser.Start(downloads?.FullName, logger);
    }

    /// <summary>Captures the active browser if screenshots are supported.</summary>
    internal Screenshot? CaptureScreenshot() => browser.CaptureScreenshot();

    /// <summary>Disposes the active driver and deletes temporary downloads even if driver disposal fails.</summary>
    internal void StopBrowser()
    {
        var downloadDirectory = downloads;
        downloads = null;

        try
        {
            browser.Dispose();
        }
        finally
        {
            downloadDirectory?.Delete(recursive: true);
        }
    }
}
