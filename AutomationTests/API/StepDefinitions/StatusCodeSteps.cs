using ApiAutomation.Clients;
using AutomationTests.Configuration;
using AutomationTests.Hooks;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

/// <summary>Exercises the status-code endpoint through the reusable RestSharp client.</summary>
[Binding]
public class StatusCodeSteps(ScenarioContext scenario)
{
    private static readonly ApiClient Api = new(TestSettings.BaseUri);
    private RestResponse response = null!;

    /// <summary>Sends a request for the supplied status-code route and stores the scenario response.</summary>
    [When("I request the status code example for {int}")]
    public async Task RequestAsync(int code)
    {
        var requestDetails = $"GET /status_codes/{code}";
        TestHooks.RecordApiDetails(scenario, requestDetails);
        response = await Api.ExecuteAsync(new RestRequest("status_codes/{code}", Method.Get).AddUrlSegment("code", code));
        TestHooks.RecordApiDetails(scenario, $"{requestDetails}\nResponse: {(int)response.StatusCode} ({response.ResponseStatus})");
    }

    /// <summary>Compares the received numeric status to the expected scenario value.</summary>
    [Then("the HTTP response status is {int}")]
    public void CheckStatusCode(int expected)
    {
        var actual = (int)response.StatusCode;
        TestHooks.RecordApiDetails(scenario, $"HTTP status\nExpected: {expected}\nReceived: {actual}");
        Assert.That(actual, Is.EqualTo(expected), response.ErrorMessage);
    }
}
