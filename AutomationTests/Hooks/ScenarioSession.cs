using AutomationFramework.Drivers;
using AutomationFramework.UI;
using OpenQA.Selenium;

namespace AutomationTests.Hooks;

public sealed class ScenarioSession
{
    private IWebDriver? driver;
    private DirectoryInfo? downloads;

    public IWebDriver Driver => driver
        ?? throw new InvalidOperationException("This scenario has no active browser.");

    public string DownloadDirectory => downloads?.FullName
        ?? throw new InvalidOperationException("This scenario has no download directory.");

    internal void StartBrowser(bool enableDownloads)
    {
        if (driver != null || downloads != null)
            throw new InvalidOperationException("The browser session has already been started.");

        if (enableDownloads)
            downloads = Directory.CreateTempSubdirectory("selenium-download-");

        driver = DriverFactory.Create(downloads?.FullName);
    }

    internal Screenshot? CaptureScreenshot() =>
        driver is ITakesScreenshot ? new BrowserActions(driver).CaptureScreenshot() : null;

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
