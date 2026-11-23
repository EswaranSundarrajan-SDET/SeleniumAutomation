using System.Net;
using System.Text.Json;
using ApiAutomation.Clients;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

[Binding]
public sealed class HttpMethodSteps
{
    private static readonly ApiClient Echo = new(new Uri("https://httpbingo.org/"));
    private RestResponse response = null!;

    [When("I send GET to the echo service with query {string}")]
    public async Task SendGet(string query) => response = await Echo.ExecuteAsync(
        new RestRequest("get", Method.Get).AddQueryParameter("q", query));

    [When("I send {word} to the echo service with a JSON name of {string}")]
    public async Task SendJson(string method, string name) => response = await Echo.ExecuteAsync(
        new RestRequest("anything", Enum.Parse<Method>(method, ignoreCase: true)).AddJsonBody(new { name }));

    [Then("the response echoes method {string}")]
    public void CheckMethod(string method)
    {
        using var json = ReadEcho();
        Assert.That(json.RootElement.GetProperty("method").GetString(), Is.EqualTo(method));
    }

    [Then("the response echoes query {string}")]
    public void CheckQuery(string query)
    {
        using var json = ReadEcho();
        Assert.That(json.RootElement.GetProperty("args").GetProperty("q")[0].GetString(), Is.EqualTo(query));
    }

    [Then("the response echoes JSON name {string}")]
    public void CheckJson(string name)
    {
        using var json = ReadEcho();
        Assert.That(json.RootElement.GetProperty("json").GetProperty("name").GetString(), Is.EqualTo(name));
    }

    private void CheckSuccess() => Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.ErrorMessage);

    private JsonDocument ReadEcho()
    {
        CheckSuccess();
        return JsonDocument.Parse(response.Content ?? throw new InvalidDataException("The response body was empty."));
    }
}
