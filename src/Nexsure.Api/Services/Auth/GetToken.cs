using System.Text.Json.Serialization;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Auth;

/// <summary>Response from the token endpoint (OAuth-style snake_case JSON).</summary>
public sealed record GetTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
    [JsonPropertyName("token_type")] public string TokenType { get; init; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
}

/// <summary>
/// Authenticates and returns a bearer token. Needs a <see cref="NoAuth"/> credential present.
/// </summary>
/// <remarks>Arguments: <c>integration_key</c>, <c>integration_login</c>, <c>integration_pwd</c>.</remarks>
public sealed class GetToken : AbstractService<GetTokenResponse>
{
    public GetToken(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NoAuth);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/auth/gettoken";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args) => new Dictionary<string, string>
    {
        ["IntegrationKey"] = args.Get<string>("integration_key"),
        ["IntegrationLogin"] = args.Get<string>("integration_login"),
        ["IntegrationPwd"] = args.Get<string>("integration_pwd"),
    };
}
