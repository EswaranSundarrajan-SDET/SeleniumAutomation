using System.Net;
using System.Net.Http.Headers;
using ApiAutomation.Authentication;
using ApiAutomation.Clients;
using AutomationTests.API.Support;
using AutomationTests.Hooks;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

/// <summary>Exercises authentication and report permissions against a private HTTP fixture.</summary>
[Binding]
public sealed class ApiAuthenticationSteps
{
    private const string TokenResource = "oauth/token";
    private readonly ScenarioContext scenario;
    private readonly AuthenticationTestHandler handler;
    private readonly ApiClient client;
    private RestResponse? response;
    private string? accessToken;
    private HttpStatusCode? rejectedTokenStatus;

    public ApiAuthenticationSteps(ScenarioContext scenario)
    {
        this.scenario = scenario;
        handler = new AuthenticationTestHandler();
        client = new ApiClient(AuthenticationTestHandler.BaseUri, handler);
    }

    [AfterScenario("authentication")]
    public void DisposeClient() => client.Dispose();

    [When("I request protected reports with {word} Basic credentials")]
    public Task RequestWithBasicAsync(string credentials)
    {
        var request = new RestRequest("basic/reports", Method.Get);
        var authenticatedRequest = credentials switch
        {
            "reader" => request.WithBasic(AuthenticationTestHandler.BasicReader, AuthenticationTestHandler.BasicPassword),
            "limited" => request.WithBasic(AuthenticationTestHandler.BasicLimitedUser, AuthenticationTestHandler.BasicPassword),
            "wrong-password" => request.WithBasic(AuthenticationTestHandler.BasicReader, "incorrect-fixture-password"),
            "unknown-user" => request.WithBasic("unknown-fixture-user", AuthenticationTestHandler.BasicPassword),
            "missing" => request,
            _ => throw new ArgumentException("Unknown Basic credential test case.", nameof(credentials))
        };

        return SendAsync(authenticatedRequest, $"Basic credential case {credentials}");
    }

    [Given("I obtain an OAuth 2.0 access token for scope {string}")]
    public async Task ObtainAccessTokenAsync(string scope)
    {
        var detail = $"POST /{TokenResource}\nOAuth 2.0 client credentials grant\nRequested scope {scope}";
        TestHooks.RecordApiDetails(scenario, detail);
        accessToken = await new OAuth2TokenClient(client).RequestClientCredentialsTokenAsync(
            TokenResource, AuthenticationTestHandler.OAuthClientId, AuthenticationTestHandler.OAuthClientSecret, scope);
        TestHooks.RecordApiDetails(scenario, $"{detail}\nReceived a Bearer access token.");
    }

    [When("I request protected reports with OAuth 2.0 token state {word}")]
    public Task RequestWithOAuth2Async(string token)
    {
        var request = new RestRequest("oauth/reports", Method.Get);
        var authenticatedRequest = token switch
        {
            "issued" => request.WithOAuth2(accessToken ?? throw new InvalidOperationException("Obtain a token before making this request.")),
            "invalid" => request.WithOAuth2("unissued-fixture-token"),
            "missing" => request,
            _ => throw new ArgumentException("Unknown OAuth 2.0 token test case.", nameof(token))
        };

        return SendAsync(authenticatedRequest, $"OAuth 2.0 access token case {token}");
    }

    [When("I request an OAuth 2.0 access token with incorrect client credentials")]
    public void RequestWithIncorrectClientCredentials()
    {
        TestHooks.RecordApiDetails(scenario, $"POST /{TokenResource}\nInvalid OAuth 2.0 client credentials.");
        var error = Assert.ThrowsAsync<HttpRequestException>(() =>
            new OAuth2TokenClient(client).RequestClientCredentialsTokenAsync(
                TokenResource, AuthenticationTestHandler.OAuthClientId, "incorrect-fixture-client-secret",
                AuthenticationTestHandler.ReadScope));
        rejectedTokenStatus = error!.StatusCode;
    }

    [Then("the OAuth 2.0 token endpoint returns HTTP {int}")]
    public void CheckTokenEndpointStatus(int expected)
    {
        TestHooks.RecordApiDetails(scenario, $"Expected HTTP {expected}\nReceived HTTP {(int?)rejectedTokenStatus}");
        Assert.That(rejectedTokenStatus, Is.EqualTo((HttpStatusCode)expected));
    }

    [When("I request protected reports with JWT state {word}")]
    public Task RequestWithJwtAsync(string token)
    {
        var request = new RestRequest("jwt/reports", Method.Get);
        var authenticatedRequest = token switch
        {
            "reader" => request.WithJwt(handler.CreateJwt(AuthenticationTestHandler.ReadScope)),
            "writer" => request.WithJwt(handler.CreateJwt(AuthenticationTestHandler.WriteScope)),
            "wrong-signature" => request.WithJwt(handler.CreateJwt(AuthenticationTestHandler.ReadScope, invalidSignature: true)),
            "expired" => request.WithJwt(handler.CreateJwt(AuthenticationTestHandler.ReadScope, expired: true)),
            "missing" => request,
            _ => throw new ArgumentException("Unknown JWT test case.", nameof(token))
        };

        return SendAsync(authenticatedRequest, $"JWT case {token}");
    }

    [Then("the authentication API returns HTTP {int}")]
    public void CheckResponseStatus(int expected)
    {
        var actual = (int)Response.StatusCode;
        TestHooks.RecordApiDetails(scenario, $"Expected HTTP {expected}\nReceived HTTP {actual}");
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Then("the response offers a {word} authentication challenge")]
    public void CheckAuthenticationChallenge(string expected)
    {
        var offered = Response.Headers?.Any(header =>
            string.Equals(header.Name, "WWW-Authenticate", StringComparison.OrdinalIgnoreCase)
            && AuthenticationHeaderValue.TryParse(header.Value?.ToString(), out var challenge)
            && string.Equals(challenge.Scheme, expected, StringComparison.OrdinalIgnoreCase)) == true;
        TestHooks.RecordApiDetails(scenario, $"Expected {expected} challenge\nPresent in response {offered}");
        Assert.That(offered, Is.True, $"Expected a {expected} authentication challenge.");
    }

    private RestResponse Response => response ?? throw new InvalidOperationException("Send a protected API request first.");

    private async Task SendAsync(RestRequest request, string authenticationCase)
    {
        var detail = $"GET /{request.Resource}\n{authenticationCase}";
        TestHooks.RecordApiDetails(scenario, detail);
        response = await client.ExecuteAsync(request);
        TestHooks.RecordApiDetails(scenario, $"{detail}\nReceived HTTP {(int)response.StatusCode} ({response.ResponseStatus})");
    }
}
