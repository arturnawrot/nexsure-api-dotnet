using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Clients;

public sealed record GetClientByIdResponse
{
    public IReadOnlyList<Client> Client { get; init; } = [];
}

/// <summary>Gets full client details by id.</summary>
/// <remarks>Arguments: <c>client_id</c>.</remarks>
public sealed class GetClientById : AbstractService<GetClientByIdResponse>
{
    public GetClientById(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/clients/getclientbyid";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) =>
        new Dictionary<string, object?> { ["clientId"] = args.Get<int>("client_id") };
}
