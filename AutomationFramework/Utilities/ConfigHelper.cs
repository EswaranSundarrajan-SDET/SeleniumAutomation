namespace AutomationFramework.Utilities;

/// <summary>Reads browser settings from the process environment.</summary>
public static class ConfigHelper
{
    /// <summary>Gets the normalized browser name. Defaults to Chrome when unset.</summary>
    public static string Browser => Environment.GetEnvironmentVariable("BROWSER")?.Trim().ToLowerInvariant() ?? "chrome";
    /// <summary>Gets whether to run without a visible browser window. Defaults to true.</summary>
    public static bool Headless => bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "true");
}
