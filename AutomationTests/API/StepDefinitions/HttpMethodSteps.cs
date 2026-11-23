using System.Net;
using System.Text.Json;
using ApiAutomation.Clients;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

/// <summary>Exercises the HTTPBingo echo endpoints and keeps the response scoped to the scenario.</summary>
[Binding]
public sealed class HttpMethodSteps
{
    private static readonly ApiClient Echo = new(new Uri("https://httpbingo.org/"));
    private RestResponse response = null!;

    /// <summary>Sends an encoded query parameter through the RestSharp client and stores the response.</summary>
    [When("I send GET to the echo service with query {string}")]
    public async Task SendGet(string query) => response = await Echo.ExecuteAsync(
        new RestRequest("get", Method.Get).AddQueryParameter("q", query));

    /// <summary>Sends the supplied method with a JSON name value and stores its response.</summary>
    [When("I send {word} to the echo service with a JSON name of {string}")]
    public async Task SendJson(string method, string name) => response = await Echo.ExecuteAsync(
        new RestRequest("anything", Enum.Parse<Method>(method, ignoreCase: true)).AddJsonBody(new { name }));

    /// <summary>Verifies the echo service received the expected HTTP method.</summary>
    [Then("the response echoes method {string}")]
    public void CheckMethod(string method)
    {
        using var json = ReadEcho();
        Assert.That(json.RootElement.GetProperty("method").GetString(), Is.EqualTo(method));
    }

    /// <summary>Verifies the first echoed query value matches the scenario input.</summary>
    [Then("the response echoes query {string}")]
    public void CheckQuery(string query)
    {
        using var json = ReadEcho();
        Assert.That(json.RootElement.GetProperty("args").GetProperty("q")[0].GetString(), Is.EqualTo(query));
    }

    /// <summary>Verifies the echoed JSON name matches the submitted value.</summary>
    [Then("the response echoes JSON name {string}")]
    public void CheckJson(string name)
    {
        using var json = ReadEcho();
        Assert.That(json.RootElement.GetProperty("json").GetProperty("name").GetString(), Is.EqualTo(name));
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
