using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace AutomationFramework.Drivers;

/// <summary>Builds Firefox sessions using Gecko-specific options.</summary>
internal sealed class FirefoxDriverProvider : IBrowserDriverProvider
{
    /// <summary>Starts Firefox and applies its download preferences when the scenario needs a download directory.</summary>
    public IWebDriver Create(bool headless, string? downloadDirectory)
    {
        var options = new FirefoxOptions();
        if (headless)
            options.AddArgument("-headless");
        if (downloadDirectory != null)
        {
            options.SetPreference("browser.download.folderList", 2);
            options.SetPreference("browser.download.dir", Path.GetFullPath(downloadDirectory));
            options.SetPreference("browser.helperApps.neverAsk.saveToDisk", "text/plain,text/csv,application/octet-stream");
        }
        return new FirefoxDriver(options);
    }
}
