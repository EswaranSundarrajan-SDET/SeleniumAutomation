using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;

namespace ApiAutomation.Authentication;

/// <summary>Sets authentication on an individual API request. Use an HTTPS service URL.</summary>
public static class ApiAuthentication
{
    /// <summary>Sends a username and password using HTTP Basic authentication.</summary>
    public static RestRequest WithBasic(this RestRequest request, string username, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentNullException.ThrowIfNull(password);

        if (username.Contains(':') || username.Any(char.IsControl))
            throw new ArgumentException("A Basic username cannot contain a colon or control character.", nameof(username));
        if (password.Any(char.IsControl))
            throw new ArgumentException("A Basic password cannot contain a control character.", nameof(password));

        return SetAuthenticator(request, new HttpBasicAuthenticator(username, password));
    }

    /// <summary>Sends an OAuth 2.0 access token in the Authorization header.</summary>
    public static RestRequest WithOAuth2(this RestRequest request, string accessToken)
    {
        ValidateBearerToken(accessToken, nameof(accessToken));
        return SetAuthenticator(request, new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer"));
    }

    /// <summary>Sends a supplied JWT as a Bearer token for the API to validate.</summary>
    public static RestRequest WithJwt(this RestRequest request, string jwt)
    {
        ValidateBearerToken(jwt, nameof(jwt));
        return SetAuthenticator(request, new JwtAuthenticator(jwt));
    }

    internal static bool IsBearerToken(string? token) =>
        token?.TrimEnd('=') is { Length: > 0 } value && value.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '.' or '_' or '~' or '+' or '/');

    private static void ValidateBearerToken(string token, string parameter)
    {
        if (!IsBearerToken(token))
            throw new ArgumentException("A valid Bearer token value is required.", parameter);
    }

    private static RestRequest SetAuthenticator(RestRequest request, IAuthenticator authenticator)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Authenticator = authenticator;
        return request;
    }
}
