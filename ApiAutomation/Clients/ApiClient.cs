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

    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default) =>
        client.ExecuteAsync(request, cancellationToken);

    public void Dispose() => client.Dispose();
}
