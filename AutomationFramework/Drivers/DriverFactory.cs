using AutomationFramework.Utilities;
using AutomationReporting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using WebDriverManager.DriverConfigs;
using WebDriverManager.DriverConfigs.Impl;
using DriverPackageManager = WebDriverManager.DriverManager;

namespace AutomationFramework.Drivers;

/// <summary>Provisions managed browsers and creates Selenium driver sessions.</summary>
public static class DriverFactory
{
    private const int BlockPermission = 2;
    private static readonly object ProvisionLock = new();
    private static readonly Dictionary<string, BrowserBinaries> Provisioned = new();

    /// <summary>Creates a driver owned by the calling scenario and passes through its optional download directory.</summary>
    public static IWebDriver Create(string? downloadDirectory = null) =>
        Create(ConfigHelper.Browser, ConfigHelper.Headless, downloadDirectory);

    /// <summary>Creates a Chrome or Firefox session with the supplied settings.</summary>
    public static WebDriver Create(string browser, bool headless, string? downloadDirectory) =>
        Create(browser, headless, downloadDirectory, blockGeolocation: false);

    /// <summary>Creates a browser session with optional geolocation blocking in its test profile.</summary>
    public static WebDriver Create(string browser, bool headless, string? downloadDirectory, bool blockGeolocation) => browser switch
    {
        "chrome" => CreateChrome(ResolveLatestStable(browser, new ChromeConfig()), headless, downloadDirectory, blockGeolocation),
        "firefox" => CreateFirefox(ResolveLatestStable(browser, new FirefoxConfig()), headless, downloadDirectory, blockGeolocation),
        _ => throw new ArgumentException($"Unsupported browser: {browser}")
    };

    private static ChromeDriver CreateChrome(BrowserBinaries binaries, bool headless, string? downloadDirectory, bool blockGeolocation)
    {
        var options = new ChromeOptions { BinaryLocation = binaries.BrowserPath };
        if (headless)
            options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1000");
        // Chrome's password prompts can interrupt demo logins in its temporary test profile.
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
        if (blockGeolocation)
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", BlockPermission);
        if (downloadDirectory != null)
        {
            options.AddUserProfilePreference("download.default_directory", Path.GetFullPath(downloadDirectory));
            options.AddUserProfilePreference("download.prompt_for_download", false);
        }

        var service = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(binaries.DriverPath),
            Path.GetFileName(binaries.DriverPath));
        return new ChromeDriver(service, options);
    }

    private static FirefoxDriver CreateFirefox(BrowserBinaries binaries, bool headless, string? downloadDirectory, bool blockGeolocation)
    {
        var options = new FirefoxOptions { BinaryLocation = binaries.BrowserPath };
        if (headless)
            options.AddArgument("-headless");
        if (blockGeolocation)
            options.SetPreference("permissions.default.geo", BlockPermission);
        if (downloadDirectory != null)
        {
            options.SetPreference("browser.download.folderList", 2);
            options.SetPreference("browser.download.dir", Path.GetFullPath(downloadDirectory));
            options.SetPreference("browser.helperApps.neverAsk.saveToDisk", "text/plain,text/csv,application/octet-stream");
        }

        var service = FirefoxDriverService.CreateDefaultService(Path.GetDirectoryName(binaries.DriverPath),
            Path.GetFileName(binaries.DriverPath));
        return new FirefoxDriver(service, options);
    }

    private static BrowserBinaries ResolveLatestStable(string browser, IDriverConfig driverConfig)
    {
        lock (ProvisionLock)
        {
            if (Provisioned.TryGetValue(browser, out var binaries))
                return binaries;

            var log = Logger.ForContext(typeof(DriverFactory)).ForContext("Browser", browser);
            log.Information("Checking for the latest stable browser and matching WebDriver");
            var paths = SeleniumManager.BinaryPaths($"--browser {browser} --browser-version stable " +
                "--force-browser-download --skip-browser-in-path --skip-driver-in-path --ttl 0");
            var browserPath = paths["browser_path"];
            var driverVersion = Path.GetFileName(Path.GetDirectoryName(paths["driver_path"]));
            if (!File.Exists(browserPath) || !Version.TryParse(driverVersion, out _))
                throw new NoSuchDriverException($"Selenium Manager did not resolve a browser and compatible driver version for {browser}.");

            // The matching versions come from the managed browser, which may differ from the system installation.
            var driverCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".cache", "seleniumautomation", "drivers");
            var driverPath = new DriverPackageManager(driverCache).SetUpDriver(driverConfig, driverVersion!);
            if (!File.Exists(driverPath))
                throw new NoSuchDriverException($"WebDriverManager did not set up {browser} WebDriver {driverVersion}.");

            binaries = new BrowserBinaries(browserPath, driverPath);

            Provisioned.Add(browser, binaries);
            log.Information("Using managed browser at {BrowserPath} with WebDriverManager driver {DriverVersion} at {DriverPath}",
                binaries.BrowserPath, driverVersion, binaries.DriverPath);
            return binaries;
        }
    }

    private sealed record BrowserBinaries(string BrowserPath, string DriverPath);
}
