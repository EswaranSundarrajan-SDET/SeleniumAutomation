using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AutomationTests.API.Support;

/// <summary>Provides an isolated token endpoint and protected reports for authentication scenarios.</summary>
internal sealed class AuthenticationTestHandler : HttpMessageHandler
{
    internal static readonly Uri BaseUri = new("https://authentication.test/");
    internal const string BasicReader = "fixture-reader";
    internal const string BasicLimitedUser = "fixture-writer";
    internal const string BasicPassword = "fixture-password";
    internal const string OAuthClientId = "fixture-client";
    internal const string OAuthClientSecret = "fixture+secret: with space%";
    internal const string ReadScope = "reports.read";
    internal const string WriteScope = "reports.write";

    private const string JwtAudience = "reports-fixture";
    private const int OAuthLifetimeSeconds = 300;
    private readonly ConcurrentDictionary<string, OAuthGrant> issuedTokens = new(StringComparer.Ordinal);
    private readonly SymmetricSecurityKey signingKey = NewSigningKey();
    private readonly JwtSecurityTokenHandler jwtHandler = new() { MapInboundClaims = false };

    internal string CreateJwt(string scope, bool expired = false, bool invalidSignature = false)
    {
        var now = DateTime.UtcNow;
        var credentials = new SigningCredentials(invalidSignature ? NewSigningKey() : signingKey,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(BaseUri.AbsoluteUri, JwtAudience, [new Claim("scope", scope)],
            notBefore: now.AddMinutes(-2), expires: expired ? now.AddMinutes(-1) : now.AddMinutes(5),
            signingCredentials: credentials);
        return jwtHandler.WriteToken(token);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = request.RequestUri?.AbsolutePath;
        if (request.Method == HttpMethod.Post && path == "/oauth/token")
            return await IssueTokenAsync(request, cancellationToken).ConfigureAwait(false);

        if (request.Method != HttpMethod.Get)
            return Json(HttpStatusCode.MethodNotAllowed, new { error = "method_not_allowed" });

        var authorization = request.Headers.Authorization;
        return path switch
        {
            "/basic/reports" => ReadReport(BasicScope(authorization), "Basic"),
            "/oauth/reports" => ReadReport(OAuthScope(authorization), "Bearer"),
            "/jwt/reports" => ReadReport(JwtScope(authorization), "Bearer"),
            _ => Json(HttpStatusCode.NotFound, new { error = "not_found" })
        };
    }

    private async Task<HttpResponseMessage> IssueTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var credentials = BasicCredentials(request.Headers.Authorization);
        if (credentials is not { } pair || WebUtility.UrlDecode(pair.Username) != OAuthClientId
            || WebUtility.UrlDecode(pair.Password) != OAuthClientSecret)
        {
            var invalidClient = Json(HttpStatusCode.Unauthorized, new { error = "invalid_client" });
            invalidClient.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", "realm=\"oauth\""));
            return invalidClient;
        }

        if (request.Content is not { } content || !string.Equals(content.Headers.ContentType?.MediaType,
                "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            return Json(HttpStatusCode.BadRequest, new { error = "invalid_request" });

        var form = ReadForm(await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        if (form == null || !form.TryGetValue("grant_type", out var grantType))
            return Json(HttpStatusCode.BadRequest, new { error = "invalid_request" });
        if (grantType != "client_credentials")
            return Json(HttpStatusCode.BadRequest, new { error = "unsupported_grant_type" });
        if (!form.TryGetValue("scope", out var scope) || scope is not (ReadScope or WriteScope))
            return Json(HttpStatusCode.BadRequest, new { error = "invalid_scope" });

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        issuedTokens[token] = new OAuthGrant(scope, DateTimeOffset.UtcNow.AddSeconds(OAuthLifetimeSeconds));
        var response = Json(HttpStatusCode.OK,
            new { access_token = token, token_type = "Bearer", expires_in = OAuthLifetimeSeconds, scope });
        response.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };
        response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
        return response;
    }

    private static string? BasicScope(AuthenticationHeaderValue? authorization)
    {
        if (BasicCredentials(authorization) is not { Password: BasicPassword } pair)
            return null;

        return pair.Username switch
        {
            BasicReader => ReadScope,
            BasicLimitedUser => WriteScope,
            _ => null
        };
    }

    private string? OAuthScope(AuthenticationHeaderValue? authorization)
    {
        var token = Credential(authorization, "Bearer");
        return token != null && issuedTokens.TryGetValue(token, out var grant) && grant.ExpiresAt > DateTimeOffset.UtcNow
            ? grant.Scope
            : null;
    }

    private string? JwtScope(AuthenticationHeaderValue? authorization)
    {
        var token = Credential(authorization, "Bearer");
        if (token == null)
            return null;

        try
        {
            var principal = jwtHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = BaseUri.AbsoluteUri,
                ValidateAudience = true,
                ValidAudience = JwtAudience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
            }, out _);
            return principal.FindFirst("scope")?.Value ?? string.Empty;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private static HttpResponseMessage ReadReport(string? scope, string scheme)
    {
        if (scope == ReadScope)
            return Json(HttpStatusCode.OK, new { result = "authorized" });

        if (scope == null)
        {
            var unauthorized = Json(HttpStatusCode.Unauthorized, new { error = "unauthorized" });
            unauthorized.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(scheme, "realm=\"reports\""));
            return unauthorized;
        }

        var forbidden = Json(HttpStatusCode.Forbidden, new { error = "insufficient_scope" });
        if (scheme == "Bearer")
            forbidden.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer",
                $"realm=\"reports\", error=\"insufficient_scope\", scope=\"{ReadScope}\""));
        return forbidden;
    }

    private static (string Username, string Password)? BasicCredentials(AuthenticationHeaderValue? authorization)
    {
        var encoded = Credential(authorization, "Basic");
        if (encoded == null)
            return null;

        try
        {
            var pair = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var separator = pair.IndexOf(':');
            return separator >= 0 ? (pair[..separator], pair[(separator + 1)..]) : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? Credential(AuthenticationHeaderValue? authorization, string scheme) =>
        authorization != null && authorization.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(authorization.Parameter)
            ? authorization.Parameter
            : null;

    private static Dictionary<string, string>? ReadForm(string body)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var field in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = field.Split('=', 2);
            if (pair.Length != 2 || !fields.TryAdd(WebUtility.UrlDecode(pair[0]), WebUtility.UrlDecode(pair[1])))
                return null;
        }

        return fields;
    }

    private static HttpResponseMessage Json(HttpStatusCode status, object content) =>
        new(status) { Content = JsonContent.Create(content) };

    private static SymmetricSecurityKey NewSigningKey() => new(RandomNumberGenerator.GetBytes(32));

    private sealed record OAuthGrant(string Scope, DateTimeOffset ExpiresAt);
}
