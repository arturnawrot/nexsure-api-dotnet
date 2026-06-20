using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Policies;

public sealed record LoadPolicyByClientIdResponse
{
    public IReadOnlyList<Policy> Policy { get; init; } = [];
}

/// <summary>Gets all policies for a client.</summary>
/// <remarks>Arguments: <c>client_id</c>.</remarks>
public sealed class LoadPolicyByClientId : AbstractService<LoadPolicyByClientIdResponse>
{
    public LoadPolicyByClientId(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/policy/loadpolicybyclientid";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) =>
        new Dictionary<string, object?> { ["clientId"] = args.Get<int>("client_id") };
}
