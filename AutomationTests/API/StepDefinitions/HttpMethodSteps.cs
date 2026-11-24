using System.Net;
using System.Text.Json;
using ApiAutomation.Clients;
using AutomationTests.Hooks;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

/// <summary>Exercises the HTTPBingo echo endpoints and keeps the response scoped to the scenario.</summary>
[Binding]
public sealed class HttpMethodSteps(ScenarioContext scenario)
{
    private static readonly ApiClient client = new(new Uri("https://httpbingo.org/"));
    private RestResponse response = null!;

    /// <summary>Sends an encoded query parameter through the RestSharp client and stores the response.</summary>
    [When("I send GET to the echo service with query {string}")]
    public Task SendGet(string query) => SendAsync(
        new RestRequest("get", Method.Get).AddQueryParameter("q", query),
        $"GET /get?q={Uri.EscapeDataString(query)}");

    /// <summary>Sends the supplied method with a JSON name value and stores its response.</summary>
    [When("I send {word} to the echo service with a JSON name of {string}")]
    public Task SendJson(string method, string name)
    {
        var payload = new { name };
        var request = new RestRequest("anything", Enum.Parse<Method>(method, ignoreCase: true)).AddJsonBody(payload);
        return SendAsync(request, $"{request.Method.ToString().ToUpperInvariant()} /anything\nRequest JSON: {JsonSerializer.Serialize(payload)}");
    }

    private async Task SendAsync(RestRequest request, string requestDetails)
    {
        TestHooks.RecordApiDetails(scenario, requestDetails);
        response = await client.ExecuteAsync(request);
        TestHooks.RecordApiDetails(scenario, $"{requestDetails}\nResponse: {(int)response.StatusCode} ({response.ResponseStatus})");
    }

    /// <summary>Verifies the echo service received the expected HTTP method.</summary>
    [Then("the response echoes method {string}")]
    public void CheckMethod(string method)
    {
        using var json = ReadEcho();
        CheckEchoedValue("Method", json.RootElement.GetProperty("method").GetString(), method);
    }

    /// <summary>Verifies the first echoed query value matches the scenario input.</summary>
    [Then("the response echoes query {string}")]
    public void CheckQuery(string query)
    {
        using var json = ReadEcho();
        CheckEchoedValue("Query parameter q", json.RootElement.GetProperty("args").GetProperty("q")[0].GetString(), query);
    }

    /// <summary>Verifies the echoed JSON name matches the submitted value.</summary>
    [Then("the response echoes JSON name {string}")]
    public void CheckJson(string name)
    {
        using var json = ReadEcho();
        CheckEchoedValue("JSON name", json.RootElement.GetProperty("json").GetProperty("name").GetString(), name);
    }

    private void CheckEchoedValue(string field, string? actual, string expected)
    {
        TestHooks.RecordApiDetails(scenario, $"{field}\nExpected: {expected}\nReceived: {actual ?? "null"}");
        Assert.That(actual, Is.EqualTo(expected));
    }

    /// <summary>Checks for HTTP 200 before parsing an echo response.</summary>
    private void CheckSuccess() => Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.ErrorMessage);

    /// <summary>Checks the status, parses the response body, and returns a document for the caller to dispose.</summary>
    private JsonDocument ReadEcho()
    {
        CheckSuccess();
        return JsonDocument.Parse(response.Content ?? throw new InvalidDataException("The response body was empty."));
    }
}
