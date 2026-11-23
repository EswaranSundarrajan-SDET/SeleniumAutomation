using AutomationFramework.Utilities;
using OpenQA.Selenium;
using Serilog;
using WebDriverManager.DriverConfigs;
using WebDriverManager.DriverConfigs.Impl;
using DriverPackageManager = WebDriverManager.DriverManager;

namespace AutomationFramework.Drivers;

/// <summary>Selects the configured provider for each new Selenium browser session.</summary>
public static class DriverFactory
{
    private static readonly object ProvisionLock = new();
    private static readonly Dictionary<string, BrowserBinaries> Provisioned = new();

    /// <summary>Creates a driver owned by the calling scenario and passes through its optional download directory.</summary>
    public static IWebDriver Create(string? downloadDirectory = null) =>
        Create(ConfigHelper.Browser, ConfigHelper.Headless, downloadDirectory);

    internal static IWebDriver Create(string browser, bool headless, string? downloadDirectory)
    {
        (IBrowserDriverProvider Provider, IDriverConfig DriverConfig) setup = browser switch
        {
            "chrome" => (new ChromeDriverProvider(), new ChromeConfig()),
            "firefox" => (new FirefoxDriverProvider(), new FirefoxConfig()),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };

        var binaries = ResolveLatestStable(browser, setup.DriverConfig);
        return setup.Provider.Create(headless, downloadDirectory, binaries);
    }

    private static BrowserBinaries ResolveLatestStable(string browser, IDriverConfig driverConfig)
    {
        lock (ProvisionLock)
        {
            if (Provisioned.TryGetValue(browser, out var binaries))
                return binaries;

            var log = Log.ForContext(typeof(DriverFactory)).ForContext("Browser", browser);
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
}

internal sealed record BrowserBinaries(string BrowserPath, string DriverPath);
