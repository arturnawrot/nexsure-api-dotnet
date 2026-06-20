using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Organization;

public sealed record Department
{
    public string? DepartmentID { get; init; }
    public string? DepartmentName { get; init; }
    public string? DepartmentCode { get; init; }
    public string? IsActive { get; init; }
}

public sealed record SearchDepartmentByNameResponse
{
    public IReadOnlyList<Department> Department { get; init; } = [];
}

/// <summary>Searches departments by name.</summary>
/// <remarks>Arguments: <c>department_name</c> (optional, defaults to <c>"%%"</c>).</remarks>
public sealed class SearchDepartmentByName : AbstractService<SearchDepartmentByNameResponse>
{
    public SearchDepartmentByName(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/organization/searchdepartmentbyname";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args) => new Dictionary<string, string>
    {
        ["departmentName"] = args.GetOptional("department_name", "%%")!,
        ["returnContentType"] = "application/json",
    };

    protected override SearchDepartmentByNameResponse ParseJson(JsonNode? root)
    {
        var node = root?["Departments"]?["Department"];
        var departments = AsArray(node).Select(n => Deserialize<Department>(n)!).ToList();
        return new SearchDepartmentByNameResponse { Department = departments };
    }
}
