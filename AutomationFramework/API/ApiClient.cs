using RestSharp;

namespace AutomationFramework.API;

public class ApiClient(RestClient client)
{
    public Task<RestResponse> GetStatusCodeExampleAsync(int statusCode)
    {
        var request = new RestRequest("status_codes/{statusCode}", Method.Get)
            .AddUrlSegment("statusCode", statusCode);
        return client.ExecuteAsync(request);
    }
}
