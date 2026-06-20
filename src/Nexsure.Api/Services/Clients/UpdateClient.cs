using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Clients;

public sealed record UpdateClientResponse
{
    public Client? Client { get; init; }
}

/// <summary>Updates an existing client from a raw client XML payload.</summary>
/// <remarks>Arguments: <c>client_xml</c>.</remarks>
public sealed class UpdateClient : AbstractService<UpdateClientResponse>
{
    public UpdateClient(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/clients/updateclient";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args) =>
        new Dictionary<string, string> { ["inputXml"] = args.Get<string>("client_xml") };
}
