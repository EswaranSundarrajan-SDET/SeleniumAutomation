using System.Text.Json;

namespace AutomationTests.Configuration;

public record TestData(string User, string Password, string InvalidPassword);

public static class TestSettings
{
    public static Uri BaseUri { get; } = new("https://the-internet.herokuapp.com/");

    public static TestData LoadData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "UI", "TestData", "testdata.json");
        return JsonSerializer.Deserialize<TestData>(File.ReadAllText(path))
            ?? throw new InvalidDataException($"Missing test data in {path}");
    }
}
