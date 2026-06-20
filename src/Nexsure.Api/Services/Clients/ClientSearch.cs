using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Clients;

public sealed record ClientSearchResponse
{
    public IReadOnlyList<Client> Client { get; init; } = [];
}

/// <summary>Searches clients by multiple criteria.</summary>
/// <remarks>Arguments (any): <c>first_name</c>, <c>last_name</c>, <c>company_name</c>, <c>client_code</c>.</remarks>
public sealed class ClientSearch : AbstractService<ClientSearchResponse>
{
    public ClientSearch(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/clients/clientsearch";

    protected override IDictionary<string, object?>? GetBody(ServiceArgs args)
    {
        var body = new Dictionary<string, object?>();
        if (args.Has("first_name")) body["FirstName"] = args.GetRaw("first_name");
        if (args.Has("last_name")) body["LastName"] = args.GetRaw("last_name");
        if (args.Has("company_name")) body["CompanyName"] = args.GetRaw("company_name");
        if (args.Has("client_code")) body["ClientCode"] = args.GetRaw("client_code");
        return body;
    }
}
