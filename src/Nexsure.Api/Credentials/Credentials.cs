namespace Nexsure.Api.Credentials;

/// <summary>
/// Base class for a credential. Each concrete credential knows how to contribute an auth
/// header and/or a JSON body fragment, and a service declares which credential <em>type</em>
/// it needs via <c>AbstractService{T}.CredentialsType</c>.
/// </summary>
public abstract class Credentials
{
    /// <summary>The raw API token, if any.</summary>
    public string? ApiToken { get; }

    /// <param name="apiToken">The API token this credential carries (may be <c>null</c>).</param>
    protected Credentials(string? apiToken = null) => ApiToken = apiToken;

    /// <summary>Headers this credential contributes (e.g. an <c>Authorization</c> header), or <c>null</c>.</summary>
    public abstract IDictionary<string, string>? GetHeader();

    /// <summary>A JSON body fragment this credential contributes, or <c>null</c>.</summary>
    public abstract IDictionary<string, object?>? GetJsonBody();

    /// <summary>Returns the API token.</summary>
    public abstract string? GetApiToken();
}

/// <summary>
/// The "no authentication" credential, used by <c>GetToken</c> before a token exists.
/// A service requesting <see cref="NoAuth"/> still requires a <see cref="NoAuth"/>
/// instance to be present in the client's credential list.
/// </summary>
public sealed class NoAuth : Credentials
{
    public NoAuth() : base(apiToken: null) { }

    public override IDictionary<string, string>? GetHeader() => null;

    public override IDictionary<string, object?>? GetJsonBody() => null;

    public override string? GetApiToken() => null;
}

/// <summary>Bearer-token credential for authenticated Nexsure endpoints.</summary>
public sealed class NexsureCredentials : Credentials
{
    public NexsureCredentials(string? apiToken = null) : base(apiToken) { }

    public override IDictionary<string, string>? GetHeader() =>
        new Dictionary<string, string> { ["Authorization"] = $"Bearer {ApiToken}" };

    public override IDictionary<string, object?>? GetJsonBody() => null;

    public override string? GetApiToken() => ApiToken;
}
