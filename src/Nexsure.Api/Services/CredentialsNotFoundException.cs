namespace Nexsure.Api.Services;

/// <summary>Thrown when a service runs but the client holds no credential of the type it requires.</summary>
public sealed class CredentialsNotFoundException : InvalidOperationException
{
    public CredentialsNotFoundException(Type credentialsType)
        : base($"No credentials of type '{credentialsType.Name}' found")
    {
        CredentialsType = credentialsType;
    }

    /// <summary>The credential type that was being looked for.</summary>
    public Type CredentialsType { get; }
}
