using AutomationFramework.API;
using AutomationFramework.Utilities;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

namespace AutomationFramework.StepDefinitions;

[Binding]
public class StatusCodeSteps
{
    private static readonly RestClient Client = new(new RestClientOptions(ConfigHelper.BaseUri)
    {
        Timeout = TimeSpan.FromSeconds(30),
        FollowRedirects = false
    });
    private readonly ApiClient api = new(Client);
    private RestResponse response = null!;

    [When("I request the status code example for {int}")]
    public async Task RequestAsync(int code) => response = await api.GetStatusCodeExampleAsync(code);

    [Then("the HTTP response status is {int}")]
    public void CheckStatusCode(int expected) =>
        Assert.That((int)response.StatusCode, Is.EqualTo(expected), response.ErrorMessage);
}
