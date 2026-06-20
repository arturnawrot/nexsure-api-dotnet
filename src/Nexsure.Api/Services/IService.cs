using Nexsure.Api.Enums;

namespace Nexsure.Api.Services;

/// <summary>
/// Non-generic marker for a service. Lets the <c>ServiceFactory</c> discover services by
/// reflection and the <c>ServiceNamespace</c> hand them out without knowing the response type.
/// </summary>
public interface IService
{
    /// <summary>The credential type this service authenticates with.</summary>
    Type CredentialsType { get; }

    /// <summary>The HTTP verb this service uses.</summary>
    HttpMethod Method { get; }

    /// <summary>The URL path (after the base URL) this service targets.</summary>
    string UrlPath { get; }
}
