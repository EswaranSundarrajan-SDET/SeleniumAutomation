using System.Net.Http.Json;

namespace AutomationFramework.API;

public record Settlement(string Reference, decimal Amount);

public class ApiClient(HttpClient client)
{
    public async Task<Settlement> GetSettlementAsync(string reference) =>
        await client.GetFromJsonAsync<Settlement>("api/settlements/" + Uri.EscapeDataString(reference))
        ?? throw new InvalidDataException("The settlement response was empty.");
}
