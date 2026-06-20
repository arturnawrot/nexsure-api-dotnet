using Nexsure.Api;
using Nexsure.Api.Credentials;
using Nexsure.Api.Enums;
using Nexsure.Api.Services;

namespace Nexsure.Api.Tests;

/// <summary>A minimal response model used by the abstract-service tests.</summary>
public sealed record FakeModel
{
    public string Name { get; init; } = string.Empty;
    public int Value { get; init; }
}

/// <summary>A minimal concrete service used to exercise the base class directly.</summary>
public class FakeService : AbstractService<FakeModel>
{
    public FakeService(BaseApiClient apiClient) : base(apiClient) { }

    public override Type CredentialsType => typeof(NexsureCredentials);

    public override HttpMethod Method => HttpMethod.Get;

    public override string UrlPath => "fake-endpoint";
}

/// <summary>Shared helpers for building clients wired to a stub transport.</summary>
public static class TestSupport
{
    public static BaseApiClient ClientWithResponse(
        string responseBody,
        out StubHttpMessageHandler handler,
        params Credentials.Credentials[] credentials)
    {
        handler = new StubHttpMessageHandler(responseBody);
        return new BaseApiClient(credentials, new HttpClient(handler));
    }

    public static BaseApiClient ClientWithCredentials(params Credentials.Credentials[] credentials)
    {
        var handler = new StubHttpMessageHandler("{}");
        return new BaseApiClient(credentials, new HttpClient(handler));
    }
}
