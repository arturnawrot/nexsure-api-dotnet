using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;
using Nexsure.Api.Types;

namespace Nexsure.Api.Services.Lookup;

public sealed record ListLookupManagementValuesResponse
{
    public IReadOnlyList<LookupCategoryType> Category { get; init; } = [];
}

/// <summary>Gets lookup-data categories and their values.</summary>
/// <remarks>Arguments: <c>category_name</c> (a <see cref="LookupCategory"/>).</remarks>
public sealed class ListLookupManagementValues : AbstractService<ListLookupManagementValuesResponse>
{
    public ListLookupManagementValues(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/lookupdata/listlookupmanagementvalues";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args)
    {
        var category = args.Get<LookupCategory>("category_name");
        return new Dictionary<string, object?>
        {
            ["categoryName"] = category.ToApiValue(),
            ["returnContentType"] = "application/json",
        };
    }

    protected override ListLookupManagementValuesResponse ParseJson(JsonNode? root)
    {
        var container = root?["LookupManagement"] ?? root;
        var categories = AsArray(container?["Category"]).Select(n => Deserialize<LookupCategoryType>(n)!).ToList();
        return new ListLookupManagementValuesResponse { Category = categories };
    }
}
