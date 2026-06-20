using System.Text.Json;
using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;
using Nexsure.Api.Types;

namespace Nexsure.Api.Services.Claims;

public sealed record ClaimDetail
{
    public string? ClaimNo { get; init; }
    public string? DateOfLoss { get; init; }
    public string? DateReported { get; init; }
    public string? DateClosed { get; init; }
    public string? Stage { get; init; }
    public string? Status { get; init; }
    public string? EstimatedAmount { get; init; }
    public string? ReservedAmount { get; init; }
    public string? TotalPaidAmount { get; init; }
    public string? Memo { get; init; }
    public string? Reopened { get; init; }
}

public sealed record Claim
{
    public string? ClaimID { get; init; }
    public JsonElement? PolicyReference { get; init; }
    public ClaimDetail? ClaimDetail { get; init; }
    public JsonElement? Adjustor { get; init; }
    public IReadOnlyList<JsonElement> Claimant { get; init; } = [];
    public IReadOnlyList<JsonElement> ClaimPayment { get; init; } = [];
    public JsonElement? Action { get; init; }
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
}

public sealed record ClaimSearchResponse
{
    public IReadOnlyList<Claim> Claims { get; init; } = [];
    public int TotalPages { get; init; }
}

/// <summary>Searches claims by multiple criteria (paginated).</summary>
/// <remarks>
/// Arguments (any): <c>client_id</c>, <c>client_name</c>, <c>claim_number</c>,
/// <c>policy_number</c>, <c>claimant_name</c>, <c>adjustor_name</c>,
/// <c>date_of_loss_from</c>, <c>date_of_loss_to</c>, <c>search_type</c>, <c>page</c>,
/// <c>results_per_page</c>.
/// </remarks>
public sealed class ClaimSearch : AbstractService<ClaimSearchResponse>
{
    public ClaimSearch(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/claims/claimsearch";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) => new Dictionary<string, object?>
    {
        ["page"] = args.GetOptional("page", 1),
        ["resultsPerPage"] = args.GetOptional("results_per_page", 20),
        ["returnContentType"] = "application/json",
    };

    protected override IDictionary<string, object?>? GetBody(ServiceArgs args)
    {
        var body = new Dictionary<string, object?>
        {
            ["SearchType"] = (int)args.GetOptional("search_type", SearchType.Contains),
        };

        AddIfTruthy(args, body, "client_id", "ClientID");
        AddIfTruthy(args, body, "client_name", "ClientName");
        AddIfTruthy(args, body, "claim_number", "ClaimNumber");
        AddIfTruthy(args, body, "policy_number", "PolicyNumber");
        AddIfTruthy(args, body, "claimant_name", "ClaimantName");
        AddIfTruthy(args, body, "adjustor_name", "AdjustorName");
        AddIfTruthy(args, body, "date_of_loss_from", "DateOfLossFrom");
        AddIfTruthy(args, body, "date_of_loss_to", "DateOfLossTo");
        return body;
    }

    // The original includes a field only when its value is "truthy" (non-zero / non-empty).
    private static void AddIfTruthy(ServiceArgs args, IDictionary<string, object?> body, string argKey, string bodyKey)
    {
        var value = args.GetRaw(argKey);
        if (value is null) return;
        if (value is int i && i == 0) return;
        if (value is string s && s.Length == 0) return;
        body[bodyKey] = value;
    }

    protected override ClaimSearchResponse ParseJson(JsonNode? root)
    {
        var container = root?["Claims"];
        var claims = AsArray(container?["Claim"]).Select(n => Deserialize<Claim>(n)!).ToList();
        // Deserialize (not a direct (int?) cast) so the API's string-encoded ints are tolerated.
        var totalPages = Deserialize<int?>(container?["TotalPages"]) ?? 0;
        return new ClaimSearchResponse { Claims = claims, TotalPages = totalPages };
    }
}
