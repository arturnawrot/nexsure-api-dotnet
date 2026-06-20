using Microsoft.Extensions.Logging;

namespace Nexsure.Api;

/// <summary>
/// The entry point. Holds credentials and transport, discovers all services, and exposes
/// them under <see cref="Services"/>.
/// </summary>
/// <example>
/// <code>
/// using var client = new NexsureApiClient(new Credentials.Credentials[] { new NoAuth() });
///
/// var token = await client.Services.GetToken.ExecuteAsync(new
/// {
///     integration_key = "your-key",
///     integration_login = "your-login",
///     integration_pwd = "your-password",
/// });
///
/// client.AddCredentials(new NexsureCredentials((string)token.AccessToken));
/// var clients = await client.Services.GetClientList.ExecuteAsync(new { client_name = "acme" });
/// </code>
/// </example>
public sealed class NexsureApiClient : BaseApiClient
{
    /// <summary>The factory that discovered the available services.</summary>
    public ServiceFactory ServiceFactory { get; }

    /// <summary>The service namespace — <c>client.Services.GetToken</c>, etc. (dynamic for natural access).</summary>
    public dynamic Services { get; }

    /// <param name="credentials">Initial credentials (start with <c>new NoAuth()</c>, add a token after <c>GetToken</c>).</param>
    /// <param name="http">The <see cref="HttpClient"/> to use, or <c>null</c> for the default.</param>
    /// <param name="serviceFactory">A service factory, or <c>null</c> to discover services in this assembly.</param>
    /// <param name="loggerFactory">An optional logger factory.</param>
    public NexsureApiClient(
        IEnumerable<Credentials.Credentials> credentials,
        HttpClient? http = null,
        ServiceFactory? serviceFactory = null,
        ILoggerFactory? loggerFactory = null)
        : base(credentials, http, loggerFactory)
    {
        ServiceFactory = serviceFactory ?? new ServiceFactory();
        Services = new ServiceNamespace(this, ServiceFactory.Services);
    }

    /// <summary>The service namespace, typed for static <c>Get&lt;TService&gt;()</c> resolution.</summary>
    public ServiceNamespace ServiceNamespace => (ServiceNamespace)Services;
}
