using System.Net;
using System.Net.Http.Headers;
using ApiAutomation.Clients;
using NUnit.Framework;
using RestSharp;

namespace AutomationTests.API;

/// <summary>Verifies retry behavior with a deterministic HTTP handler that sends no network requests.</summary>
[Category("api-retry")]
public sealed class ApiRetryTests
{
    private static readonly Uri BaseUri = new("https://example.test/");

    /// <summary>Verifies GET and PUT recover from a temporary service failure.</summary>
    [TestCase(Method.Get)]
    [TestCase(Method.Put)]
    public async Task RetriesTransientFailureForIdempotentMethods(Method method)
    {
        using var handler = new StubHandler(attempt => new HttpResponseMessage(
            attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
        using var client = new ApiClient(BaseUri, handler);

        var response = await client.ExecuteAsync(new RestRequest("resource", method));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(handler.Attempts, Is.EqualTo(2));
    }

    /// <summary>Verifies that POST and PATCH require an explicit retry policy.</summary>
    [TestCase(Method.Post)]
    [TestCase(Method.Patch)]
    public async Task LeavesWritesSingleAttemptByDefault(Method method)
    {
        using var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadGateway));
        using var client = new ApiClient(BaseUri, handler);

        var response = await client.ExecuteAsync(new RestRequest("resource", method));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
        Assert.That(handler.Attempts, Is.EqualTo(1));
    }

    /// <summary>Verifies an opted-in POST retries a rate-limited response with a zero Retry-After delay.</summary>
    [Test]
    public async Task RetriesReplaySafePostWithServerDelay()
    {
        using var handler = new StubHandler(attempt =>
        {
            if (attempt > 1)
                return new HttpResponseMessage(HttpStatusCode.OK);

            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
            return response;
        });
        using var client = new ApiClient(BaseUri, handler);
        var request = new RestRequest("resource", Method.Post).AddHeader("Idempotency-Key", "test-key");

        var result = await client.ExecuteAsync(request, CancellationToken.None, ApiRetryMode.ReplaySafeRequest);

        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(handler.Attempts, Is.EqualTo(2));
    }

    /// <summary>Verifies an authorization error returns on the first attempt.</summary>
    [Test]
    public async Task PreservesPermanentError()
    {
        using var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        using var client = new ApiClient(BaseUri, handler);

        var response = await client.ExecuteAsync(new RestRequest("resource", Method.Get));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(handler.Attempts, Is.EqualTo(1));
    }

    /// <summary>Verifies the client stops after three attempts and returns the last error.</summary>
    [Test]
    public async Task PreservesFinalErrorAfterTwoRetries()
    {
        using var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.GatewayTimeout));
        using var client = new ApiClient(BaseUri, handler);

        var response = await client.ExecuteAsync(new RestRequest("resource", Method.Get));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.GatewayTimeout));
        Assert.That(handler.Attempts, Is.EqualTo(3));
    }

    /// <summary>Verifies recovery when the handler reports a dropped connection.</summary>
    [Test]
    public async Task RetriesTransportConnectionFailure()
    {
        using var handler = new StubHandler(attempt => attempt == 1
            ? throw new HttpRequestException(HttpRequestError.ConnectionError, "Connection dropped.")
            : new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new ApiClient(BaseUri, handler);

        var response = await client.ExecuteAsync(new RestRequest("resource", Method.Get));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(handler.Attempts, Is.EqualTo(2));
    }

    /// <summary>Verifies a long Retry-After response returns without an early retry.</summary>
    [Test]
    public async Task PreservesRateLimitWhenServerDelayExceedsRetryBudget()
    {
        using var handler = new StubHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMinutes(1));
            return response;
        });
        using var client = new ApiClient(BaseUri, handler);

        var result = await client.ExecuteAsync(new RestRequest("resource", Method.Get));

        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(handler.Attempts, Is.EqualTo(1));
    }

    /// <summary>Verifies the first HTTP failure is available to tests that disable retries.</summary>
    [Test]
    public async Task HonorsRetryOptOutForHttpErrorAssertions()
    {
        using var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var client = new ApiClient(BaseUri, handler);

        var result = await client.ExecuteAsync(new RestRequest("resource", Method.Get),
            CancellationToken.None, ApiRetryMode.Never);

        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(handler.Attempts, Is.EqualTo(1));
    }

    /// <summary>Returns a scripted result for each request attempt.</summary>
    private sealed class StubHandler(Func<int, HttpResponseMessage> reply) : HttpMessageHandler
    {
        /// <summary>Gets the number of requests seen by this handler instance.</summary>
        public int Attempts { get; private set; }

        /// <summary>Checks cancellation and returns the next scripted response.</summary>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(reply(++Attempts));
        }
    }
}
