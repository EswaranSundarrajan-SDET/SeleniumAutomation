using System.Text.Json;

namespace AutomationTests.Configuration;

/// <summary>Holds valid and intentionally invalid credentials for the public demo account.</summary>
public record TestData(string User, string Password, string InvalidPassword);

/// <summary>Provides the demo site URL and loads the UI test data shipped with the test assembly.</summary>
public static class TestSettings
{
    /// <summary>Gets the public demo site's base URL used by the web and status-code scenarios.</summary>
    public static Uri BaseUri { get; } = new("https://the-internet.herokuapp.com/");

    /// <summary>Reads the copied JSON test data and rejects an empty data document.</summary>
    public static TestData LoadData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "UI", "TestData", "testdata.json");
        return JsonSerializer.Deserialize<TestData>(File.ReadAllText(path))
            ?? throw new InvalidDataException($"Missing test data in {path}");
    }
}
