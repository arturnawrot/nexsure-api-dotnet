using System.Text;
using System.Text.Json.Nodes;
using Nexsure.Api.Credentials;
using Nexsure.Api.Inputs;
using Nexsure.Api.Models;
using Nexsure.Api.Services;
using Nexsure.Api.Types;

namespace Nexsure.Api.Services.Clients;

public sealed record AddNewClientResponse
{
    public Client? Client { get; init; }
}

/// <summary>Creates a new client. Builds the Nexsure client XML from typed inputs.</summary>
/// <remarks>
/// Required arguments: <c>name</c>, <c>assignment</c> (<see cref="AssignmentInput"/>).
/// Optional: <c>client_type</c>, <c>stage</c>, <c>legal_entity</c>, <c>contacts</c>
/// (<see cref="ContactInput"/> list), <c>addresses</c> (<see cref="AddressInput"/> list).
/// </remarks>
public sealed class AddNewClient : AbstractService<AddNewClientResponse>
{
    public AddNewClient(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Post;

    public override string UrlPath => "/clients/addnewclient";

    private static string Xml(string tag, string text) => $"<{tag}>{text}</{tag}>";

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args)
    {
        var name = args.Get<string>("name");
        var assignment = args.Get<AssignmentInput>("assignment");
        var clientType = args.GetOptional("client_type", ClientType.Commercial);
        var stage = args.GetOptional("stage", ClientStage.Prospect);
        var legalEntity = args.GetOptional("legal_entity", LegalEntity.Corporation);
        var contacts = args.GetOptional<IEnumerable<ContactInput>>("contacts") ?? [];
        var addresses = args.GetOptional<IEnumerable<AddressInput>>("addresses") ?? [];

        var namesXml =
            "<ClientNames>" +
            Xml("Name", name) +
            Xml("IsPrimaryName", "true") +
            Xml("IsDBAName", "false") +
            Xml("LegalEntityCd", legalEntity.ToString()) +
            Xml("GrossReceipts", "0") +
            "</ClientNames>";

        var contactsXml = new StringBuilder();
        foreach (var c in contacts)
        {
            contactsXml
                .Append("<Contacts>")
                .Append(Xml("FirstName", c.FirstName))
                .Append(Xml("LastName", c.LastName))
                .Append(Xml("IsPrimary", c.IsPrimary ? "true" : "false"))
                .Append("</Contacts>");
        }

        var locationsXml = new StringBuilder();
        foreach (var addr in addresses)
        {
            locationsXml
                .Append("<Locations><Address>")
                .Append(Xml("AddressType", addr.AddressType.ToString()))
                .Append(Xml("StreetAddress1", addr.Street))
                .Append(Xml("City", addr.City))
                .Append(Xml("State", addr.State))
                .Append(Xml("ZipCode", addr.ZipCode))
                .Append("</Address>")
                .Append(Xml("IsPrimaryLocation", "true"))
                .Append("</Locations>");
        }

        var assignmentXml =
            "<Assignments>" +
            Xml("IsPrimary", assignment.IsPrimary ? "true" : "false") +
            $"<Branch>{Xml("BranchID", assignment.BranchId)}</Branch>" +
            $"<Department>{Xml("DepartmentID", assignment.DepartmentId)}</Department>" +
            "</Assignments>";

        var clientXml =
            "<?xml version=\"1.0\" ?>" +
            "<Client xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
            "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
            Xml("ClientType", clientType.ToString()) +
            Xml("ClientStage", stage.ToString()) +
            namesXml + contactsXml + locationsXml + assignmentXml +
            "</Client>";

        return new Dictionary<string, string>
        {
            ["inputXml"] = clientXml,
            ["returnContentType"] = "application/json",
        };
    }

    protected override AddNewClientResponse ParseJson(JsonNode? root)
    {
        var node = root?["Client"] ?? root;
        return new AddNewClientResponse { Client = Deserialize<Client>(node) };
    }
}
