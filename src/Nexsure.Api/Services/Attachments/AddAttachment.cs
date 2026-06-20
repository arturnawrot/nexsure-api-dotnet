using System.Text;
using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Models;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.Attachments;

public sealed record AddAttachmentResponse
{
    public Attachment? Attachment { get; init; }
}

/// <summary>Uploads a file attachment, assigned to a client or a policy.</summary>
/// <remarks>
/// Arguments: <c>client_id</c> and/or <c>policy_id</c> (policy wins for assignment),
/// <c>file_name</c>, <c>description</c>, <c>file_bytes</c> (<see cref="T:System.Byte[]"/>).
/// </remarks>
public sealed class AddAttachment : AbstractService<AddAttachmentResponse>
{
    public AddAttachment(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/attachments/addattachment";

    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args)
    {
        var clientId = args.GetOptional<int?>("client_id");
        var policyId = args.GetOptional<int?>("policy_id");
        var fileName = args.GetOptional("file_name", "attachment.pdf")!;
        var description = args.GetOptional("description", "")!;
        var fileBytes = args.GetOptional<byte[]>("file_bytes");

        var assignmentType = policyId is not null ? "Policy" : "Client";
        var assignmentId = policyId ?? clientId;

        var attachmentXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<Attachment xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            $"<AssignedTo><AssignmentType>{assignmentType}</AssignmentType>" +
            $"<AssignmentTypeID>{assignmentId}</AssignmentTypeID></AssignedTo>" +
            $"<AttachmentName>{(string.IsNullOrEmpty(description) ? fileName : description)}</AttachmentName>" +
            $"<AttachmentDesc>{description}</AttachmentDesc>" +
            $"<FileName>{fileName}</FileName>" +
            "</Attachment>";

        var content = fileBytes ?? Encoding.UTF8.GetBytes("placeholder");
        var attachmentBase64 = Convert.ToBase64String(content);

        return new Dictionary<string, object?>
        {
            ["xml"] = attachmentXml,
            ["attachmentBase64"] = attachmentBase64,
            ["returnContentType"] = "application/json",
        };
    }

    protected override AddAttachmentResponse ParseJson(JsonNode? root)
    {
        var node = root?["Attachment"] ?? root;
        return new AddAttachmentResponse { Attachment = Deserialize<Attachment>(node) };
    }
}
