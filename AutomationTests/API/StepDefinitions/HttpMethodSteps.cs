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

    [When("I send HEAD to the echo service")]
    public async Task SendHead() => response = await Echo.ExecuteAsync(new RestRequest("head", Method.Head));

    [When("I send OPTIONS to the echo service for method PATCH")]
    public async Task SendOptions() => response = await Echo.ExecuteAsync(new RestRequest("anything", Method.Options)
        .AddHeader("Origin", "https://example.com")
        .AddHeader("Access-Control-Request-Method", "PATCH"));

    [When("I send {word} through the in-process transport")]
    public async Task SendThroughControlledTransport(string method)
    {
        using var api = new ApiClient(new Uri("https://api.example.test/"), new MethodEchoHandler());
        response = await api.ExecuteAsync("request", new HttpMethod(method));
    }

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

    [Then("the response contains JSON metadata and no body")]
    public void CheckHead()
    {
        CheckSuccess();
        Assert.That(response.ContentType, Does.StartWith("application/json"));
        Assert.That(response.Content, Is.Null.Or.Empty);
    }

    [Then("the response allows PATCH in its CORS headers")]
    public void CheckOptions()
    {
        CheckSuccess();
        var allow = response.Headers?.SingleOrDefault(header =>
            header.Name.Equals("Access-Control-Allow-Methods", StringComparison.OrdinalIgnoreCase));
        var methods = allow?.Value?.ToString()?.Split(',', StringSplitOptions.TrimEntries);
        Assert.That(methods, Does.Contain("PATCH"));
    }

    [Then("the transport receives method {string}")]
    public void CheckTransportMethod(string method)
    {
        CheckSuccess();
        Assert.That(response.Content, Is.EqualTo(method));
    }

    private void CheckSuccess() => Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.ErrorMessage);

    private JsonDocument ReadEcho()
    {
        CheckSuccess();
        return JsonDocument.Parse(response.Content ?? throw new InvalidDataException("The response body was empty."));
    }

    private sealed class MethodEchoHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(request.Method.Method)
            });
    }
}
