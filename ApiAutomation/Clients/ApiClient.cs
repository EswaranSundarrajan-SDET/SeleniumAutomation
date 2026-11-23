using RestSharp;

namespace ApiAutomation.Clients;

public sealed class ApiClient : IDisposable
{
    private readonly RestClient client;

    public ApiClient(Uri baseUri)
    {
        client = new RestClient(new RestClientOptions(baseUri)
        {
            Timeout = TimeSpan.FromSeconds(30),
            FollowRedirects = false
        });
    }

    public ApiClient(Uri baseUri, HttpMessageHandler handler)
    {
        client = new RestClient(handler, configureRestClient: options =>
        {
            options.BaseUrl = baseUri;
            options.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Method is not (Method.Get or Method.Post or Method.Put or Method.Patch))
            throw new NotSupportedException($"HTTP method {request.Method} is not supported.");

        return client.ExecuteAsync(request, cancellationToken);
    }

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

    public void Dispose() => client.Dispose();
}
