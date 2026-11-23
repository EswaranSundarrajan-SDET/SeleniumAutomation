using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutomationFramework.Drivers;

/// <summary>Builds Chrome sessions using Chromium-specific options.</summary>
internal sealed class ChromeDriverProvider : IBrowserDriverProvider
{
    /// <summary>Starts Chrome with a fixed viewport and applies a scenario download preference when supplied.</summary>
    public IWebDriver Create(bool headless, string? downloadDirectory)
    {
        var options = new ChromeOptions();
        if (headless)
            options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1000");
        if (downloadDirectory != null)
        {
            options.AddUserProfilePreference("download.default_directory", Path.GetFullPath(downloadDirectory));
            options.AddUserProfilePreference("download.prompt_for_download", false);
        }
        return new ChromeDriver(options);
    }
}
