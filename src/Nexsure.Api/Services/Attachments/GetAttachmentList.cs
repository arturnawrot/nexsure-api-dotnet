using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Attachments;

public sealed record GetAttachmentListResponse
{
    public IReadOnlyList<Attachment> Attachment { get; init; } = [];
}

/// <summary>Lists attachments for a client or policy.</summary>
/// <remarks>Arguments (any): <c>client_id</c>, <c>policy_id</c>.</remarks>
public sealed class GetAttachmentList : AbstractService<GetAttachmentListResponse>
{
    public GetAttachmentList(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/attachments/getattachmentlist";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args)
    {
        var query = new Dictionary<string, object?>();
        if (args.Has("client_id")) query["clientId"] = args.GetRaw("client_id");
        if (args.Has("policy_id")) query["policyId"] = args.GetRaw("policy_id");
        return query;
    }
}
