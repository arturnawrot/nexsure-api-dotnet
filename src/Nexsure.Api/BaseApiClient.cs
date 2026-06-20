using Microsoft.Extensions.Logging;
using Nexsure.Api.Http;

namespace Nexsure.Api;

/// <summary>Holds the transport and the mutable list of credentials shared by every service.</summary>
public class BaseApiClient : IDisposable
{
    private readonly bool _ownsHttp;

    /// <summary>The <see cref="HttpClient"/> every service request goes through.</summary>
    public HttpClient Http { get; }

    /// <summary>The credentials available to services, searched by type at call time.</summary>
    public List<Credentials.Credentials> Credentials { get; }

    /// <summary>An optional logger factory passed down to services.</summary>
    public ILoggerFactory? LoggerFactory { get; }

    /// <param name="credentials">Initial credentials (may be empty).</param>
    /// <param name="http">An <see cref="HttpClient"/> to use, or <c>null</c> to create a default one. An injected client is never disposed by this object.</param>
    /// <param name="loggerFactory">An optional logger factory.</param>
    public BaseApiClient(
        IEnumerable<Credentials.Credentials> credentials,
        HttpClient? http = null,
        ILoggerFactory? loggerFactory = null)
    {
        LoggerFactory = loggerFactory;

        if (http is null)
        {
            Http = NexsureHttp.CreateClient(loggerFactory);
            _ownsHttp = true;
        }
        else
        {
            Http = http;
            _ownsHttp = false;
        }

        Credentials = credentials.ToList();
    }

    /// <summary>Adds a credential to the client. Later calls pick it up automatically.</summary>
    public void AddCredentials(Credentials.Credentials credentials) => Credentials.Add(credentials);

    /// <summary>Disposes the internally-created <see cref="HttpClient"/> (injected clients are left alone).</summary>
    public void Dispose()
    {
        if (_ownsHttp)
        {
            Http.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
