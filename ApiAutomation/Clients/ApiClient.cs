using RestSharp;
using RestSharp.Interceptors;

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

    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default) =>
        client.ExecuteAsync(request, cancellationToken);

    public Task<RestResponse> ExecuteAsync(
        string resource,
        HttpMethod method,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);

        var request = new RestRequest(resource);
        if (Enum.TryParse<Method>(method.Method, ignoreCase: true, out var namedMethod) &&
            string.Equals(namedMethod.ToString(), method.Method, StringComparison.OrdinalIgnoreCase))
        {
            request.Method = namedMethod;
        }
        else
        {
            request.Interceptors = [new HttpMethodInterceptor(method)];
        }

        return ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => client.Dispose();

    // RestSharp's Method enum has no TRACE or CONNECT values.
    private sealed class HttpMethodInterceptor(HttpMethod method) : Interceptor
    {
        public override ValueTask BeforeHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            requestMessage.Method = method;
            return ValueTask.CompletedTask;
        }
    }
}
