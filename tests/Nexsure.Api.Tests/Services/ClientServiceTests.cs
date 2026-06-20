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
