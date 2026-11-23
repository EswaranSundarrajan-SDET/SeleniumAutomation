using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using RestSharp;

namespace ApiAutomation.Clients;

/// <summary>Executes supported RestSharp requests with bounded retries for transient failures.</summary>
public sealed class ApiClient : IDisposable
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(5);
    private readonly RestClient client;

    /// <summary>Creates a client for the service base URL with a 30-second request timeout and redirects disabled.</summary>
    public ApiClient(Uri baseUri)
    {
        client = new RestClient(new RestClientOptions(baseUri)
        {
            Timeout = TimeSpan.FromSeconds(30),
            FollowRedirects = false
        });
    }

    /// <summary>Creates a client backed by the supplied HTTP handler, which the client owns.</summary>
    public ApiClient(Uri baseUri, HttpMessageHandler handler)
    {
        client = new RestClient(handler, configureRestClient: options =>
        {
            options.BaseUrl = baseUri;
            options.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    /// <summary>Executes GET, POST, PUT, or PATCH and retries eligible GET and PUT requests by default.</summary>
    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default) =>
        ExecuteAsync(request, cancellationToken, ApiRetryMode.IdempotentMethods);

    /// <summary>Applies the caller's retry policy to this request. Requests containing files always use one attempt.</summary>
    /// <param name="request">The configured RestSharp request. Use a separate request instance for each concurrent call.</param>
    /// <param name="cancellationToken">Cancels the call or its retry delay.</param>
    /// <param name="retryMode">The replay policy. Opt in for POST or PATCH only when the endpoint guarantees safe replay.</param>
    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken, ApiRetryMode retryMode)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Method is not (Method.Get or Method.Post or Method.Put or Method.Patch))
            throw new NotSupportedException($"HTTP method {request.Method} is not supported.");

        var allowRetry = retryMode switch
        {
            ApiRetryMode.Never => false,
            ApiRetryMode.IdempotentMethods => request.Method is Method.Get or Method.Put,
            ApiRetryMode.ReplaySafeRequest => true,
            _ => throw new ArgumentOutOfRangeException(nameof(retryMode))
        };

        return allowRetry && request.Files.Count == 0
            ? ExecuteWithRetryAsync(request, cancellationToken)
            : client.ExecuteAsync(request, cancellationToken);
    }

    /// <summary>Executes a resource with a supported HTTP method using the default retry policy.</summary>
    public Task<RestResponse> ExecuteAsync(
        string resource,
        HttpMethod method,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);

        var supportedMethod = method.Method switch
        {
            "GET" => Method.Get,
            "POST" => Method.Post,
            "PUT" => Method.Put,
            "PATCH" => Method.Patch,
            _ => throw new NotSupportedException($"HTTP method {method} is not supported.")
        };

        return ExecuteAsync(new RestRequest(resource, supportedMethod), cancellationToken);
    }

    /// <summary>Disposes the underlying RestSharp client and its HTTP resources.</summary>
    public void Dispose() => client.Dispose();

    /// <summary>Retries an eligible request at most twice and returns the final response when its retry budget is exhausted.</summary>
    private async Task<RestResponse> ExecuteWithRetryAsync(RestRequest request, CancellationToken cancellationToken)
    {
        for (var retries = 0; ; retries++)
        {
            var response = await client.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            if (retries == MaxRetries || cancellationToken.IsCancellationRequested || !IsTransient(response))
                return response;

            var delay = RetryDelay(response, retries);
            if (delay > MaxRetryDelay)
                return response;

            Trace.TraceWarning("RestSharp retry {0}/{1} for {2}. HTTP status: {3}. Response status: {4}.",
                retries + 1, MaxRetries, request.Method, (int)response.StatusCode, response.ResponseStatus);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Checks for selected transient HTTP errors, timeouts, and connection failures reported by RestSharp.</summary>
    private static bool IsTransient(RestResponse response) =>
        response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout
        || (response.StatusCode == 0 && (response.ResponseStatus == ResponseStatus.TimedOut
            || response.ErrorException is HttpRequestException
            {
                HttpRequestError: HttpRequestError.ConnectionError or HttpRequestError.ResponseEnded
            }));

    /// <summary>Uses Retry-After when valid or backs off for 250 then 500 milliseconds. The caller stops when the server asks for more than five seconds.</summary>
    private static TimeSpan RetryDelay(RestResponse response, int retries)
    {
        var header = response.Headers?.FirstOrDefault(parameter =>
            string.Equals(parameter.Name, "Retry-After", StringComparison.OrdinalIgnoreCase));
        if (RetryConditionHeaderValue.TryParse(header?.Value?.ToString(), out var retryAfter))
        {
            var delay = retryAfter.Delta ?? retryAfter.Date.GetValueOrDefault() - DateTimeOffset.UtcNow;
            return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
        }

        return TimeSpan.FromMilliseconds(250 * (1 << retries));
    }
}
