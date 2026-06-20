using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Organization;

public sealed record Branch
{
    public int? BranchID { get; init; }
    public string? BranchName { get; init; }
    public string? BranchCode { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record SearchBranchByBranchNameResponse
{
    public IReadOnlyList<Branch> Branch { get; init; } = [];
}

/// <summary>Searches branches by name.</summary>
/// <remarks>Arguments: <c>branch_name</c> (optional).</remarks>
public sealed class SearchBranchByBranchName : AbstractService<SearchBranchByBranchNameResponse>
{
    public SearchBranchByBranchName(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/organization/searchbranchbybranchname";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args)
    {
        var data = new Dictionary<string, string> { ["returnContentType"] = "application/json" };
        var branchName = args.GetOptional("branch_name", "")!;
        if (!string.IsNullOrEmpty(branchName))
        {
            data["branchName"] = branchName;
        }
        return data;
    }

    protected override SearchBranchByBranchNameResponse ParseJson(JsonNode? root)
    {
        var node = root?["Branches"]?["Branch"];
        var branches = AsArray(node).Select(n => Deserialize<Branch>(n)!).ToList();
        return new SearchBranchByBranchNameResponse { Branch = branches };
    }
}
