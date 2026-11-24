using System.Net;
using System.Text.Json;
using ApiAutomation.Clients;
using AutomationTests.Hooks;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

/// <summary>Checks the request details returned by HTTPBingo and keeps the response scoped to the scenario.</summary>
[Binding]
public sealed class APIHandlingSteps(ScenarioContext scenario)
{
    private static readonly ApiClient client = new(new Uri("https://httpbingo.org/"));
    private RestResponse response = null!;

    /// <summary>Sends an encoded query parameter through the RestSharp client and stores the response.</summary>
    [When("I send GET to HTTPBingo with query {string}")]
    public Task SendGet(string query) => SendAsync(
        new RestRequest("get", Method.Get).AddQueryParameter("q", query),
        $"GET /get?q={Uri.EscapeDataString(query)}");

    /// <summary>Sends the supplied method with a JSON name value and stores its response.</summary>
    [When("I send {word} to HTTPBingo with a JSON name of {string}")]
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

    /// <summary>Verifies HTTPBingo received the expected HTTP method.</summary>
    [Then("the HTTPBingo response contains method {string}")]
    public void CheckMethod(string method)
    {
        using var json = ReadResponseJson();
        CheckResponseValue("Method", json.RootElement.GetProperty("method").GetString(), method);
    }

    /// <summary>Verifies the first returned query value matches the scenario input.</summary>
    [Then("the HTTPBingo response contains query {string}")]
    public void CheckQuery(string query)
    {
        using var json = ReadResponseJson();
        CheckResponseValue("Query parameter q", json.RootElement.GetProperty("args").GetProperty("q")[0].GetString(), query);
    }

    /// <summary>Verifies the returned JSON name matches the submitted value.</summary>
    [Then("the HTTPBingo response contains JSON name {string}")]
    public void CheckJson(string name)
    {
        using var json = ReadResponseJson();
        CheckResponseValue("JSON name", json.RootElement.GetProperty("json").GetProperty("name").GetString(), name);
    }

    private void CheckResponseValue(string field, string? actual, string expected)
    {
        TestHooks.RecordApiDetails(scenario, $"{field}\nExpected: {expected}\nReceived: {actual ?? "null"}");
        Assert.That(actual, Is.EqualTo(expected));
    }

    /// <summary>Checks for HTTP 200 before parsing the HTTPBingo response.</summary>
    private void CheckSuccess() => Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.ErrorMessage);

    /// <summary>Checks the status, parses the response body, and returns a document for the caller to dispose.</summary>
    private JsonDocument ReadResponseJson()
    {
        CheckSuccess();
        return JsonDocument.Parse(response.Content ?? throw new InvalidDataException("The response body was empty."));
    }
}
