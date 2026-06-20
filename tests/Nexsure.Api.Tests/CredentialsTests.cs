using Nexsure.Api.Credentials;
using Xunit;

namespace Nexsure.Api.Tests;

public class CredentialsTests
{
    [Fact]
    public void NexsureCredentials_GetHeader_ReturnsBearer()
    {
        var cred = new NexsureCredentials("my-token");
        Assert.Equal("Bearer my-token", cred.GetHeader()!["Authorization"]);
    }

    [Fact]
    public void NexsureCredentials_GetJsonBody_IsNull()
    {
        Assert.Null(new NexsureCredentials("tok").GetJsonBody());
    }

    [Fact]
    public void NexsureCredentials_GetApiToken_ReturnsToken()
    {
        Assert.Equal("secret", new NexsureCredentials("secret").GetApiToken());
    }

    [Fact]
    public void NoAuth_ContributesNothing()
    {
        var cred = new NoAuth();
        Assert.Null(cred.GetHeader());
        Assert.Null(cred.GetJsonBody());
        Assert.Null(cred.GetApiToken());
    }
}
