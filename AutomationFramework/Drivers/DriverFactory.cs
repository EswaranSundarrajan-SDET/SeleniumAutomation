using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.Drivers;

/// <summary>Selects the configured provider for each new Selenium browser session.</summary>
public static class DriverFactory
{
    /// <summary>Creates a driver owned by the calling scenario and passes through its optional download directory.</summary>
    public static IWebDriver Create(string? downloadDirectory = null)
    {
        var browser = ConfigHelper.Browser;
        IBrowserDriverProvider provider = browser switch
        {
            "chrome" => new ChromeDriverProvider(),
            "firefox" => new FirefoxDriverProvider(),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };

        return provider.Create(ConfigHelper.Headless, downloadDirectory);
    }
}
