using System.Text.Json;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services.Claims;
using Nexsure.Api.Types;
using Xunit;

namespace Nexsure.Api.Tests.Services;

public class ClaimSearchTests
{
    [Fact]
    public async Task Execute_SendsQueryAndBody_AndUnwrapsClaimsWithNestedDetail()
    {
        var client = TestSupport.ClientWithResponse(
            """
            {
              "Claims": {
                "Claim": [
                  {
                    "ClaimID": "c-1",
                    "ClaimDetail": { "ClaimNo": "CLM-1", "Status": "Open", "DateOfLoss": "2024-01-01" }
                  }
                ],
                "TotalPages": "3"
              }
            }
            """,
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new ClaimSearch(client).ExecuteAsync(new
        {
            client_id = 10042,
            claim_status = "Open",
            page = 2,
            results_per_page = 50,
        });

        // Query params
        var uri = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("page=2", uri);
        Assert.Contains("resultsPerPage=50", uri);
        Assert.Contains("returnContentType=application", uri);

        // Body: SearchType default + ClientID included (truthy)
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal((int)SearchType.Contains, doc.RootElement.GetProperty("SearchType").GetInt32());
        Assert.Equal(10042, doc.RootElement.GetProperty("ClientID").GetInt32());

        // Parsed result
        Assert.Equal(3, result.TotalPages);
        var claim = Assert.Single(result.Claims);
        Assert.Equal("c-1", claim.ClaimID);
        Assert.Equal("Open", claim.ClaimDetail!.Status);
        Assert.Equal("CLM-1", claim.ClaimDetail.ClaimNo);
    }

    [Fact]
    public async Task Execute_OmitsZeroClientId()
    {
        var client = TestSupport.ClientWithResponse(
            """{ "Claims": { "Claim": [], "TotalPages": 0 } }""",
            out var handler,
            new NexsureCredentials("tok"));

        await new ClaimSearch(client).ExecuteAsync(new { claim_number = "CLM-9" });

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.False(doc.RootElement.TryGetProperty("ClientID", out _));
        Assert.Equal("CLM-9", doc.RootElement.GetProperty("ClaimNumber").GetString());
    }
}
