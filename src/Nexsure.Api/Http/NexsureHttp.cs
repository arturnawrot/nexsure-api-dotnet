using Microsoft.Extensions.Logging;

namespace Nexsure.Api.Http;

/// <summary>
/// Helpers for building the default <see cref="HttpClient"/> the library uses when the
/// caller doesn't supply their own. In an application you'd typically register an
/// <c>IHttpClientFactory</c>-managed client instead and pass it into the client constructor.
/// </summary>
public static class NexsureHttp
{
    /// <summary>Default connect timeout, in seconds (mirrors the original 5s connect timeout).</summary>
    public const int DefaultConnectTimeoutSeconds = 5;

    /// <summary>Default overall request timeout, in seconds (mirrors the original 30s read timeout).</summary>
    public const int DefaultReadTimeoutSeconds = 30;

    /// <summary>
    /// Builds a default <see cref="HttpClient"/> with a <see cref="LoggingHandler"/> over a
    /// <see cref="SocketsHttpHandler"/> configured with the standard connect/read timeouts.
    /// </summary>
    public static HttpClient CreateClient(
        ILoggerFactory? loggerFactory = null,
        int connectTimeoutSeconds = DefaultConnectTimeoutSeconds,
        int readTimeoutSeconds = DefaultReadTimeoutSeconds)
    {
        var sockets = new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(connectTimeoutSeconds),
        };

        var handler = new LoggingHandler(sockets, loggerFactory?.CreateLogger<LoggingHandler>());

        return new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(readTimeoutSeconds),
        };
    }
}
