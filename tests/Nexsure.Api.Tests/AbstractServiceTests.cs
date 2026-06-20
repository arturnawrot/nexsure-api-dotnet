using System.Net;
using System.Text.Json;
using Nexsure.Api;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services;
using Xunit;

namespace Nexsure.Api.Tests;

public class AbstractServiceTests
{
    // --- GetCredentials ---

    [Fact]
    public void GetCredentials_ReturnsMatching()
    {
        var cred = new NexsureCredentials("tok");
        var service = new FakeService(TestSupport.ClientWithCredentials(cred));
        Assert.Same(cred, service.GetCredentials());
    }

    [Fact]
    public void GetCredentials_SkipsWrongType()
    {
        var noAuth = new NoAuth();
        var nexsure = new NexsureCredentials("v");
        var service = new FakeService(TestSupport.ClientWithCredentials(noAuth, nexsure));
        Assert.Same(nexsure, service.GetCredentials());
    }

    [Fact]
    public void GetCredentials_ThrowsWhenNoneMatch()
    {
        var service = new FakeService(TestSupport.ClientWithCredentials(new NoAuth()));
        var ex = Assert.Throws<CredentialsNotFoundException>(() => service.GetCredentials());
        Assert.Contains("NexsureCredentials", ex.Message);
    }

    // --- BuildUrl / query string ---

    [Fact]
    public void BuildUrl_PathOnly()
    {
        var service = new FakeService(TestSupport.ClientWithCredentials());
        Assert.Equal("https://resteaiqa0.nexsure.com/fake-endpoint", service.BuildUrl("", null));
    }

    [Fact]
    public void BuildUrl_WithQueryParams()
    {
        var service = new FakeService(TestSupport.ClientWithCredentials());
        var url = service.BuildUrl("", new Dictionary<string, object?> { ["clientId"] = 42, ["q"] = "a b" });
        Assert.Equal("https://resteaiqa0.nexsure.com/fake-endpoint?clientId=42&q=a%20b", url);
    }

    [Fact]
    public void BuildQueryString_SkipsNullValues()
    {
        var query = AbstractService<FakeModel>.BuildQueryString(
            new Dictionary<string, object?> { ["a"] = 1, ["b"] = null });
        Assert.Equal("a=1", query);
    }

    // --- BuildHeaders ---

    [Fact]
    public void BuildHeaders_MergesCredentialHeaders()
    {
        var cred = new NexsureCredentials("tok");
        var service = new FakeService(TestSupport.ClientWithCredentials());
        Assert.Equal("Bearer tok", service.BuildHeaders(cred, ServiceArgs.Empty)["Authorization"]);
    }

    [Fact]
    public void BuildHeaders_NoCredentialHeaders()
    {
        var service = new FakeService(TestSupport.ClientWithCredentials());
        Assert.Empty(service.BuildHeaders(new NoAuth(), ServiceArgs.Empty));
    }

    // --- Execute (full flow over stub transport) ---

    [Fact]
    public async Task Execute_FullFlow()
    {
        var client = TestSupport.ClientWithResponse(
            """{ "name": "result", "value": 99 }""",
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new FakeService(client).ExecuteAsync();

        Assert.Equal("GET", handler.LastRequest!.Method.Method);
        Assert.Equal("https://resteaiqa0.nexsure.com/fake-endpoint", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer tok", handler.LastRequest.Headers.GetValues("Authorization").Single());
        Assert.Equal(new FakeModel { Name = "result", Value = 99 }, result);
    }

    [Fact]
    public async Task Execute_ThrowsWhenNoCredentials()
    {
        var client = TestSupport.ClientWithResponse("{}", out _);
        await Assert.ThrowsAsync<CredentialsNotFoundException>(() => new FakeService(client).ExecuteAsync());
    }

    [Fact]
    public async Task Execute_PropagatesHttpError()
    {
        var handler = new StubHttpMessageHandler("Server Error", HttpStatusCode.InternalServerError);
        var client = new BaseApiClient([new NexsureCredentials("tok")], new HttpClient(handler));
        await Assert.ThrowsAsync<HttpRequestException>(() => new FakeService(client).ExecuteAsync());
    }

    [Fact]
    public async Task Execute_ThrowsOnNonJson()
    {
        var handler = new StubHttpMessageHandler("<html>not json</html>");
        var client = new BaseApiClient([new NexsureCredentials("tok")], new HttpClient(handler));
        await Assert.ThrowsAsync<JsonException>(() => new FakeService(client).ExecuteAsync());
    }
}
