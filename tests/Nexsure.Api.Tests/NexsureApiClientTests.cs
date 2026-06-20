using Nexsure.Api;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services.Auth;
using Nexsure.Api.Services.Clients;
using Xunit;

namespace Nexsure.Api.Tests;

public class NexsureApiClientTests
{
    private static NexsureApiClient NewClient(params Credentials.Credentials[] credentials) =>
        new(credentials, new HttpClient(new StubHttpMessageHandler("{}")));

    [Fact]
    public void Services_ResolvesServiceInstance()
    {
        using var client = NewClient(new NoAuth());
        object service = client.Services.GetToken;
        Assert.IsType<GetToken>(service);
    }

    [Fact]
    public void UnknownService_Throws()
    {
        using var client = NewClient();
        Assert.Throws<MissingMemberException>(() =>
        {
            object _ = client.Services.NoSuchService;
        });
    }

    [Fact]
    public void EachAccess_CreatesNewInstance()
    {
        using var client = NewClient(new NexsureCredentials("tok"));
        object first = client.Services.GetClientList;
        object second = client.Services.GetClientList;
        Assert.NotSame(first, second);
    }

    [Fact]
    public void TypedGet_ReturnsService()
    {
        using var client = NewClient(new NexsureCredentials("tok"));
        var service = client.ServiceNamespace.Get<GetClientList>();
        Assert.IsType<GetClientList>(service);
    }

    [Fact]
    public void InheritsCredentials()
    {
        var cred = new NexsureCredentials("tok");
        using var client = NewClient(cred);
        Assert.Contains(cred, client.Credentials);
    }
}
