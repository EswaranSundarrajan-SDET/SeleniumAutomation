using RestSharp;

namespace AutomationFramework.API;

public sealed class ApiClient(Uri baseUri) : IDisposable
{
    private readonly RestClient client = new(new RestClientOptions(baseUri)
    {
        Timeout = TimeSpan.FromSeconds(30),
        FollowRedirects = false
    });

    public Task<RestResponse> GetAsync(string resource) =>
        client.ExecuteAsync(new RestRequest(resource, Method.Get));

    public void Dispose() => client.Dispose();
}
