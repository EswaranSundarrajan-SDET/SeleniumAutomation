using ApiAutomation.Clients;
using AutomationTests.Configuration;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationTests.API.StepDefinitions;

[Binding]
public class StatusCodeSteps
{
    private static readonly ApiClient Api = new(TestSettings.BaseUri);
    private RestResponse response = null!;

    [When("I request the status code example for {int}")]
    public async Task RequestAsync(int code) => response = await Api.ExecuteAsync(new RestRequest("status_codes/{code}", Method.Get).AddUrlSegment("code", code));

    [Then("the HTTP response status is {int}")]
    public void CheckStatusCode(int expected) =>
        Assert.That((int)response.StatusCode, Is.EqualTo(expected), response.ErrorMessage);
}
