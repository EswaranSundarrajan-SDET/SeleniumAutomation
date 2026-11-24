using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiAutomation.Clients;
using RestSharp;

namespace ApiAutomation.Authentication;

/// <summary>Requests OAuth 2.0 access tokens from an HTTPS token endpoint.</summary>
public sealed class OAuth2TokenClient(ApiClient client)
{
    private readonly ApiClient tokenEndpoint = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>Requests a Bearer access token for the client credentials grant and optional scope.</summary>
    public async Task<string> RequestClientCredentialsTokenAsync(string resource, string clientId, string clientSecret,
        string? scope = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentException.ThrowIfNullOrEmpty(clientId);
        ArgumentNullException.ThrowIfNull(clientSecret);

        // OAuth 2.0 form-encodes each credential before putting it in the Basic header.
        var request = new RestRequest(resource, Method.Post)
            .WithBasic(WebUtility.UrlEncode(clientId), WebUtility.UrlEncode(clientSecret))
            .AddParameter("grant_type", "client_credentials");
        if (!string.IsNullOrWhiteSpace(scope))
            request.AddParameter("scope", scope);

        var response = await tokenEndpoint.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (!response.IsSuccessful)
        {
            HttpStatusCode? status = response.StatusCode == 0 ? null : response.StatusCode;
            throw new HttpRequestException("The OAuth 2.0 token request failed.", null, status);
        }

        return ReadBearerToken(response.Content)
            ?? throw new HttpRequestException("The OAuth 2.0 response did not contain a usable Bearer token.",
                null, response.StatusCode);
    }

    private static string? ReadBearerToken(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            var response = JsonSerializer.Deserialize<TokenResponse>(content);
            return response != null && string.Equals(response.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase)
                && ApiAuthentication.IsBearerToken(response.AccessToken)
                ? response.AccessToken
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType);
}
