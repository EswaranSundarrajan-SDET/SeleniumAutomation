namespace AutomationFramework.Utilities;

public static class ConfigHelper
{
    public static string Browser => Environment.GetEnvironmentVariable("BROWSER")?.Trim().ToLowerInvariant() ?? "chrome";
    public static bool Headless => bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "true");
}
