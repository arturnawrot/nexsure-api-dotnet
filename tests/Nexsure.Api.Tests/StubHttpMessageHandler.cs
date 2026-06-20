using System.Net;
using System.Text;

namespace Nexsure.Api.Tests;

/// <summary>
/// A fake <see cref="HttpMessageHandler"/> — the idiomatic .NET test double for the HTTP
/// layer. Captures the outgoing request (and its body) and returns a canned response.
/// </summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;
    private readonly string _contentType;

    public StubHttpMessageHandler(
        string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string contentType = "application/json")
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
        _contentType = contentType;
    }

    public HttpRequestMessage? LastRequest { get; private set; }

    public string? LastRequestBody { get; private set; }

    public int CallCount { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = request;
        if (request.Content is not null)
        {
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, _contentType),
        };
    }
}
