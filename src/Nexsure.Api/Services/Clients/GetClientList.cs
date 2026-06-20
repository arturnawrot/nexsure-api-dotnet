using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Clients;

/// <summary>A lightweight client row from the list endpoint.</summary>
public sealed record ClientListItem
{
    public string? ClientId { get; init; }
    public string? ClientName { get; init; }
    public string? ClientType { get; init; }
    public string? ClientStage { get; init; }
    public string? ClientLocationName { get; init; }
    public string? LocAddress1 { get; init; }
    public string? LocAddress2 { get; init; }
    public string? LocCity { get; init; }
    public string? LocState { get; init; }
    public string? LocZipCode { get; init; }
}

public sealed record GetClientListResponse
{
    public IReadOnlyList<ClientListItem> Clients { get; init; } = [];
}

/// <summary>Lists clients by name.</summary>
/// <remarks>Arguments: <c>client_name</c>.</remarks>
public sealed class GetClientList : AbstractService<GetClientListResponse>
{
    public GetClientList(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/clients/getclientlist";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args) => new Dictionary<string, string>
    {
        ["clientName"] = args.Get<string>("client_name"),
        ["returnContentType"] = "application/json",
    };

    protected override GetClientListResponse ParseJson(JsonNode? root)
    {
        var node = root?["Clients"]?["Client"];
        var clients = AsArray(node).Select(n => Deserialize<ClientListItem>(n)!).ToList();
        return new GetClientListResponse { Clients = clients };
    }
}
