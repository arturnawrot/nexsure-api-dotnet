using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Inputs;
using Nexsure.Api.Models;
using Nexsure.Api.Services;
using Nexsure.Api.Types;

namespace Nexsure.Api.Services.Policies;

public sealed record AddSinglePolicyResponse
{
    public Policy? Policy { get; init; }
}

/// <summary>Adds a policy to a client. Builds the Nexsure policy XML from typed inputs.</summary>
/// <remarks>
/// Required arguments: <c>client_id</c>, <c>policy_number</c>, <c>assignment</c>
/// (<see cref="AssignmentInput"/>), <c>eff_date</c>, <c>exp_date</c>. Optional:
/// <c>description</c>, <c>mode</c>, <c>stage</c>, <c>policy_type</c>, <c>status</c>.
/// </remarks>
public sealed class AddSinglePolicy : AbstractService<AddSinglePolicyResponse>
{
    public AddSinglePolicy(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/policy/addsinglepolicy";

    private static string Xml(string tag, string text) => $"<{tag}>{text}</{tag}>";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args)
    {
        var clientId = args.GetRaw("client_id");
        var policyNumber = args.Get<string>("policy_number");
        var assignment = args.Get<AssignmentInput>("assignment");
        var effDate = args.Get<string>("eff_date");
        var expDate = args.Get<string>("exp_date");
        var description = args.GetOptional("description", "")!;
        var mode = args.GetOptional("mode", PolicyMode.New);
        var stage = args.GetOptional("stage", PolicyStage.Marketing);
        var policyType = args.GetOptional("policy_type", PolicyType.Monoline);
        var status = args.GetOptional("status", "Quote")!;

        var assignmentXml =
            "<Assignments>" +
            Xml("IsPrimary", assignment.IsPrimary ? "true" : "false") +
            $"<Branch>{Xml("BranchID", assignment.BranchId)}</Branch>" +
            $"<Department>{Xml("DepartmentID", assignment.DepartmentId)}</Department>" +
            "</Assignments>";

        var policyXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<Policy xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            Xml("PolicyNumber", policyNumber) +
            Xml("PolicyMode", mode.ToString()) +
            Xml("PolicyStage", stage.ToString()) +
            Xml("PolicyType", policyType.ToString()) +
            Xml("PolicyStatus", status) +
            Xml("EffDate", effDate) +
            Xml("ExpDate", expDate) +
            (string.IsNullOrEmpty(description) ? "" : Xml("PolicyDescription", description)) +
            "<PolicyDetails/>" +
            assignmentXml +
            "</Policy>";

        return new Dictionary<string, string>
        {
            ["policyXml"] = policyXml,
            ["clientId"] = Convert.ToString(clientId, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ["returnContentType"] = "application/json",
        };
    }

    protected override AddSinglePolicyResponse ParseJson(JsonNode? root)
    {
        var node = root?["Policy"] ?? root;
        return new AddSinglePolicyResponse { Policy = Deserialize<Policy>(node) };
    }
}
