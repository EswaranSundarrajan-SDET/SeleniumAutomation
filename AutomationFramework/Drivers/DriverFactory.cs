using AutomationFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace AutomationFramework.Drivers;

public static class DriverFactory
{
    public static IWebDriver Create(string? downloadDirectory = null)
    {
        var browser = ConfigHelper.Browser;
        return browser switch
        {
            "chrome" => new ChromeDriver(ChromeSettings(downloadDirectory)),
            "firefox" => new FirefoxDriver(FirefoxSettings(downloadDirectory)),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };
    }

    private static ChromeOptions ChromeSettings(string? downloadDirectory)
    {
        var options = new ChromeOptions();
        if (ConfigHelper.Headless)
            options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1000");
        if (downloadDirectory != null)
        {
            options.AddUserProfilePreference("download.default_directory", Path.GetFullPath(downloadDirectory));
            options.AddUserProfilePreference("download.prompt_for_download", false);
        }
        return options;
    }

    private static FirefoxOptions FirefoxSettings(string? downloadDirectory)
    {
        var options = new FirefoxOptions();
        if (ConfigHelper.Headless)
            options.AddArgument("-headless");
        if (downloadDirectory != null)
        {
            options.SetPreference("browser.download.folderList", 2);
            options.SetPreference("browser.download.dir", Path.GetFullPath(downloadDirectory));
            options.SetPreference("browser.helperApps.neverAsk.saveToDisk", "text/plain,text/csv,application/octet-stream");
        }
        return options;
    }
}
