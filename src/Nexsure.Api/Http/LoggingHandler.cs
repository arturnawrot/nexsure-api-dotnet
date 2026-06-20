using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nexsure.Api.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that logs each request and any non-success response.
/// The idiomatic .NET seam for cross-cutting HTTP concerns — chain more handlers (retry,
/// auth, metrics) the same way.
/// </summary>
public sealed class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler>? logger = null)
        => _logger = logger ?? NullLogger<LoggingHandler>.Instance;

    public LoggingHandler(HttpMessageHandler innerHandler, ILogger<LoggingHandler>? logger = null)
        : base(innerHandler)
        => _logger = logger ?? NullLogger<LoggingHandler>.Instance;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("HTTP {Method} {Url}", request.Method, request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(
                "HTTP {StatusCode} for {Method} {Url}: {Body}",
                (int)response.StatusCode, request.Method, request.RequestUri, body);
        }

        return response;
    }
}
