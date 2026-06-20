using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Clients;

public sealed record GetClientByNameResponse
{
    public IReadOnlyList<Client> Client { get; init; } = [];
}

/// <summary>Gets clients by first/last/company name.</summary>
/// <remarks>Arguments (any): <c>first_name</c>, <c>last_name</c>, <c>company_name</c>.</remarks>
public sealed class GetClientByName : AbstractService<GetClientByNameResponse>
{
    public GetClientByName(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/clients/getclientbyname";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args)
    {
        var query = new Dictionary<string, object?>();
        if (args.Has("first_name")) query["firstName"] = args.GetRaw("first_name");
        if (args.Has("last_name")) query["lastName"] = args.GetRaw("last_name");
        if (args.Has("company_name")) query["companyName"] = args.GetRaw("company_name");
        return query;
    }
}
