using Nexsure.Api.Credentials;
using Nexsure.Api.Services.Clients;
using Xunit;

namespace Nexsure.Api.Tests.Services;

public class ClientServiceTests
{
    [Fact]
    public async Task GetClientList_UnwrapsNestedClientsClientArray()
    {
        var client = TestSupport.ClientWithResponse(
            """
            {
              "Clients": {
                "Client": [
                  { "ClientId": "10042", "ClientName": "Acme", "ClientType": "Commercial", "LocCity": "Reno" },
                  { "ClientId": "10043", "ClientName": "Acme West", "LocCity": "Tahoe" }
                ]
              }
            }
            """,
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new GetClientList(client).ExecuteAsync(new { client_name = "acme" });

        Assert.Contains("clientName=acme", handler.LastRequestBody);
        Assert.Equal(2, result.Clients.Count);
        Assert.Equal("Acme", result.Clients[0].ClientName);
        Assert.Equal("Reno", result.Clients[0].LocCity);
    }

    [Fact]
    public async Task GetClientList_HandlesSingleObjectInsteadOfArray()
    {
        var client = TestSupport.ClientWithResponse(
            """{ "Clients": { "Client": { "ClientId": "1", "ClientName": "Solo" } } }""",
            out _,
            new NexsureCredentials("tok"));

        var result = await new GetClientList(client).ExecuteAsync(new { client_name = "solo" });
        Assert.Single(result.Clients);
        Assert.Equal("Solo", result.Clients[0].ClientName);
    }

    [Fact]
    public async Task GetClientById_SendsQueryParamAndParsesSharedClientModel()
    {
        var client = TestSupport.ClientWithResponse(
            """
            { "Client": [ { "ClientID": 10042, "ClientType": "Commercial", "IsActive": true,
              "ClientNames": [ { "Name": "Acme Inc", "IsPrimaryName": true } ] } ] }
            """,
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new GetClientById(client).ExecuteAsync(new { client_id = 10042 });

        Assert.Equal("https://resteaiqa0.nexsure.com/clients/getclientbyid?clientId=10042",
            handler.LastRequest!.RequestUri!.ToString());
        var single = Assert.Single(result.Client);
        Assert.Equal(10042, single.ClientID);
        Assert.Equal("Acme Inc", single.ClientNames[0].Name);
    }

    [Fact]
    public async Task AddNewClient_ParsesSingleObjectCollectionsAsOneItemLists()
    {
        // The live API returns single-element collections as a lone object, not a one-item
        // array: ClientNames/Contacts/Locations/Assignments each come back as { ... }.
        var client = TestSupport.ClientWithResponse(
            """
            {
              "Client": {
                "ClientID": "6864",
                "IsActive": "true",
                "ClientType": "Commercial",
                "ClientNames": { "ClientNameID": "8148", "Name": "Acme Shape Probe", "IsPrimaryName": "true" },
                "Contacts": { "PersonID": "10686", "FirstName": "John" },
                "Locations": {
                  "LocationID": "8461",
                  "Address": [
                    { "AddressType": "Mailing", "City": "Chicago" },
                    { "AddressType": "Physical", "City": "Chicago" }
                  ]
                },
                "Assignments": { "AssignmentID": "10633", "IsPrimary": "true" }
              }
            }
            """,
            out _,
            new NexsureCredentials("tok"));

        var result = await new AddNewClient(client).ExecuteAsync(new
        {
            name = "Acme Shape Probe",
            assignment = new Nexsure.Api.Inputs.AssignmentInput("1", "1"),
        });

        var created = result.Client!;
        Assert.Equal(6864, created.ClientID);
        var name = Assert.Single(created.ClientNames);
        Assert.Equal("Acme Shape Probe", name.Name);
        Assert.Single(created.Contacts);
        var location = Assert.Single(created.Locations);
        Assert.Equal(2, location.Address.Count); // Address is already an array on the wire
        Assert.Single(created.Assignments);
    }

    [Fact]
    public async Task ClientSearch_OmitsUnsuppliedCriteria()
    {
        var client = TestSupport.ClientWithResponse(
            """{ "Client": [] }""",
            out var handler,
            new NexsureCredentials("tok"));

        await new ClientSearch(client).ExecuteAsync(new { company_name = "acme" });

        using var doc = System.Text.Json.JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("acme", doc.RootElement.GetProperty("CompanyName").GetString());
        Assert.False(doc.RootElement.TryGetProperty("FirstName", out _));
    }
}
