using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;
using Nexsure.Api.Types;

namespace Nexsure.Api.Services.Lookup;

public sealed record NaicSicCode
{
    public string? CodeID { get; init; }
    public string? NaicsCode { get; init; }
    public string? NaicsDescription { get; init; }
    public string? SicCode { get; init; }
    public string? SicDescription { get; init; }
}

public sealed record SicNaicsSearchResponse
{
    public IReadOnlyList<NaicSicCode> Codes { get; init; } = [];
    public int TotalPages { get; init; }
}

/// <summary>Searches SIC/NAICS industry classification codes (paginated).</summary>
/// <remarks>
/// Arguments (any): <c>naics_description</c>, <c>sic_description</c>, <c>naics_code</c>,
/// <c>sic_code</c>, <c>search_type</c>, <c>page</c>, <c>results_per_page</c>.
/// </remarks>
public sealed class SicNaicsSearch : AbstractService<SicNaicsSearchResponse>
{
    public SicNaicsSearch(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/lookupdata/sicnaicssearch";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) => new Dictionary<string, object?>
    {
        ["page"] = args.GetOptional("page", 1),
        ["resultsPerPage"] = args.GetOptional("results_per_page", 20),
        ["returnContentType"] = "application/json",
    };

    protected override IDictionary<string, object?>? GetBody(ServiceArgs args) => new Dictionary<string, object?>
    {
        ["SearchType"] = (int)args.GetOptional("search_type", SearchType.Contains),
        ["NaicsCode"] = args.GetOptional("naics_code", "")!,
        ["NaicsDescription"] = args.GetOptional("naics_description", "")!,
        ["SicCode"] = args.GetOptional("sic_code", "")!,
        ["SicDescription"] = args.GetOptional("sic_description", "")!,
        ["SortField1"] = 0,
        ["SortType1"] = 0,
    };

    protected override SicNaicsSearchResponse ParseJson(JsonNode? root)
    {
        var container = root?["SicNaicsList"];
        var codes = AsArray(container?["NaicSicCode"]).Select(n => Deserialize<NaicSicCode>(n)!).ToList();
        var totalPages = (int?)(container?["TotalPages"]) ?? 0;
        return new SicNaicsSearchResponse { Codes = codes, TotalPages = totalPages };
    }
}
