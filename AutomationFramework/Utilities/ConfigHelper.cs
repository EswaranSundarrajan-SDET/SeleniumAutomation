using System.Text.Json;

namespace AutomationFramework.Utilities;

public record TestData(string User, string Password, decimal Amount);

public static class ConfigHelper
{
    public static string Browser => Environment.GetEnvironmentVariable("BROWSER")?.Trim().ToLowerInvariant() ?? "chrome";
    public static bool Headless => bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "true");

    public static TestData LoadData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "testdata.json");
        return JsonSerializer.Deserialize<TestData>(File.ReadAllText(path))
            ?? throw new InvalidDataException($"Missing test data in {path}");
    }
}
