using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;
using Nexsure.Api.Types;

namespace Nexsure.Api.Services.Policies;

public sealed record PolicySearchWithDetailsResponse
{
    public IReadOnlyList<Policy> Policies { get; init; } = [];
    public int TotalPages { get; init; }
}

/// <summary>Searches policies with full details (paginated).</summary>
/// <remarks>
/// Arguments (any): <c>client_name</c>, <c>client_id</c>, <c>policy_number</c>,
/// <c>include_history</c>, <c>search_type</c>, <c>page</c>, <c>results_per_page</c>.
/// </remarks>
public sealed class PolicySearchWithDetails : AbstractService<PolicySearchWithDetailsResponse>
{
    public PolicySearchWithDetails(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/policy/policysearchwithdetails";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) => new Dictionary<string, object?>
    {
        ["page"] = args.GetOptional("page", 1),
        ["resultsPerPage"] = args.GetOptional("results_per_page", 20),
        ["returnContentType"] = "application/json",
    };

    protected override IDictionary<string, object?>? GetBody(ServiceArgs args) => new Dictionary<string, object?>
    {
        ["SearchType"] = (int)args.GetOptional("search_type", SearchType.Contains),
        ["ClientID"] = args.GetOptional("client_id", 0),
        ["ClientName"] = args.GetOptional("client_name", "")!,
        ["PolicyMode"] = 0,
        ["PolicyStage"] = 0,
        ["PolicyStatus"] = 0,
        ["PolicyType"] = 0,
        ["IncludeHistory"] = args.GetOptional("include_history", false),
        ["PolicyNumber"] = args.GetOptional("policy_number", "")!,
    };

    protected override PolicySearchWithDetailsResponse ParseJson(JsonNode? root)
    {
        var container = root?["Policies"] ?? root;
        var policies = AsArray(container?["Policy"]).Select(n => Deserialize<Policy>(n)!).ToList();
        // Deserialize (not a direct (int?) cast) so the API's string-encoded ints are tolerated.
        var totalPages = Deserialize<int?>(container?["TotalPages"]) ?? 0;
        return new PolicySearchWithDetailsResponse { Policies = policies, TotalPages = totalPages };
    }
}
