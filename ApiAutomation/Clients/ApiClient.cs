using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Polly;
using Polly.Retry;
using RestSharp;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace ApiAutomation.Clients;

/// <summary>Executes supported RestSharp requests with bounded retries for transient failures.</summary>
public sealed class ApiClient : IDisposable
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(5);
    private static readonly ResiliencePipeline<RestResponse> Retry = new ResiliencePipelineBuilder<RestResponse>()
        .AddRetry(new RetryStrategyOptions<RestResponse>
        {
            MaxRetryAttempts = MaxRetries,
            Delay = TimeSpan.FromMilliseconds(250),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder<RestResponse>().HandleResult(response =>
                IsTransient(response) && (GetRetryAfter(response) ?? TimeSpan.Zero) <= MaxRetryDelay),
            DelayGenerator = args => ValueTask.FromResult(GetRetryAfter(args.Outcome.Result!)),
            OnRetry = args =>
            {
                var response = args.Outcome.Result!;
                Log.ForContext<ApiClient>().Warning(
                    "Retrying API request ({Attempt}/{MaxRetries}) for {Method} after {StatusCode} ({ResponseStatus})",
                    args.AttemptNumber + 1, MaxRetries, response.Request?.Method, (int)response.StatusCode,
                    response.ResponseStatus);
                return default;
            }
        })
        .Build();

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

    /// <summary>Executes GET, POST, PUT, or PATCH and retries eligible GET and PUT requests with Polly.</summary>
    /// <param name="request">The configured RestSharp request. Use a separate request instance for each concurrent call.</param>
    /// <param name="cancellationToken">Cancels the call or its retry delay.</param>
    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Method is not (Method.Get or Method.Post or Method.Put or Method.Patch))
            throw new NotSupportedException($"HTTP method {request.Method} is not supported.");

        return SendAsync(request, cancellationToken);
    }

    /// <summary>Executes a resource with a supported HTTP method using the same retry policy.</summary>
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

    private async Task<RestResponse> SendAsync(RestRequest request, CancellationToken cancellationToken)
    {
        using var requestScope = LogContext.PushProperty("RequestId", Guid.NewGuid());
        var started = Stopwatch.GetTimestamp();
        var allowRetry = request.Method is (Method.Get or Method.Put) && request.Files.Count == 0;
        var response = allowRetry
            ? await Retry.ExecuteAsync(token => new ValueTask<RestResponse>(client.ExecuteAsync(request, token)),
                cancellationToken).ConfigureAwait(false)
            : await client.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

        var status = (int)response.StatusCode;
        var level = status == 0 || status >= 500 ? LogEventLevel.Warning : LogEventLevel.Information;
        Log.ForContext<ApiClient>().Write(level, "API {Method} returned {StatusCode} ({ResponseStatus}) in {ElapsedMs} ms",
            request.Method, status, response.ResponseStatus, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        return response;
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

    /// <summary>Returns the server's requested delay, or null to use exponential backoff.</summary>
    private static TimeSpan? GetRetryAfter(RestResponse response)
    {
        var header = response.Headers?.FirstOrDefault(parameter =>
            string.Equals(parameter.Name, "Retry-After", StringComparison.OrdinalIgnoreCase));
        if (!RetryConditionHeaderValue.TryParse(header?.Value?.ToString(), out var retryAfter))
            return null;

        var delay = retryAfter.Delta ?? retryAfter.Date.GetValueOrDefault() - DateTimeOffset.UtcNow;
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }
}
