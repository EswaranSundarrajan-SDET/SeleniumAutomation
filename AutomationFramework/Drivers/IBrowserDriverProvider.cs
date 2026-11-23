using OpenQA.Selenium;

namespace AutomationFramework.Drivers;

/// <summary>Defines the browser-specific setup used by the driver factory.</summary>
internal interface IBrowserDriverProvider
{
    /// <summary>Starts a session with the requested visibility and optional download directory.</summary>
    IWebDriver Create(bool headless, string? downloadDirectory, BrowserBinaries binaries);
}
