using Nexsure.Api.Credentials;
using Nexsure.Api.Enums;
using Nexsure.Api.Services.Auth;
using Xunit;

namespace Nexsure.Api.Tests.Services;

public class GetTokenTests
{
    [Fact]
    public void Configuration()
    {
        var service = new GetToken(TestSupport.ClientWithCredentials());
        Assert.Equal(typeof(NoAuth), service.CredentialsType);
        Assert.Equal(HttpMethod.Post, service.Method);
        Assert.Equal("/auth/gettoken", service.UrlPath);
    }

    [Fact]
    public async Task Execute_SendsFormDataAndParsesSnakeCaseResponse()
    {
        var client = TestSupport.ClientWithResponse(
            """
            { "access_token": "abc.def", "token_type": "Bearer", "expires_in": 3600, "refresh_token": "r1" }
            """,
            out var handler,
            new NoAuth());

        var result = await new GetToken(client).ExecuteAsync(new
        {
            integration_key = "key",
            integration_login = "login",
            integration_pwd = "pwd",
        });

        Assert.Equal("POST", handler.LastRequest!.Method.Method);
        Assert.Equal("https://resteaiqa0.nexsure.com/auth/gettoken", handler.LastRequest.RequestUri!.ToString());

        // Form-url-encoded body
        Assert.Equal("application/x-www-form-urlencoded", handler.LastRequest.Content!.Headers.ContentType!.MediaType);
        Assert.Contains("IntegrationKey=key", handler.LastRequestBody);
        Assert.Contains("IntegrationLogin=login", handler.LastRequestBody);
        Assert.Contains("IntegrationPwd=pwd", handler.LastRequestBody);

        Assert.Equal("abc.def", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("r1", result.RefreshToken);
    }
}
