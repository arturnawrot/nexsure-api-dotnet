using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Policies;

public sealed record PolicyLoadResponse
{
    public IReadOnlyList<Policy> Policy { get; init; } = [];
}

/// <summary>Loads a policy by number and date range.</summary>
/// <remarks>Arguments (any): <c>policy_number</c>, <c>effective_date</c>, <c>expiration_date</c>.</remarks>
public sealed class PolicyLoad : AbstractService<PolicyLoadResponse>
{
    public PolicyLoad(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/policy/policyload";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args)
    {
        var query = new Dictionary<string, object?>();
        if (args.Has("policy_number")) query["policyNumber"] = args.GetRaw("policy_number");
        if (args.Has("effective_date")) query["effectiveDate"] = args.GetRaw("effective_date");
        if (args.Has("expiration_date")) query["expirationDate"] = args.GetRaw("expiration_date");
        return query;
    }
}
