using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Policies;

public sealed record LoadPolicyByPolicyIdResponse
{
    public IReadOnlyList<Policy> Policy { get; init; } = [];
}

/// <summary>Gets a policy by id.</summary>
/// <remarks>Arguments: <c>policy_id</c>.</remarks>
public sealed class LoadPolicyByPolicyId : AbstractService<LoadPolicyByPolicyIdResponse>
{
    public LoadPolicyByPolicyId(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/policy/loadpolicybypolicyid";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) =>
        new Dictionary<string, object?> { ["policyId"] = args.Get<int>("policy_id") };
}
