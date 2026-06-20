using System.Dynamic;
using Nexsure.Api.Services;

namespace Nexsure.Api;

/// <summary>
/// The service namespace (<c>client.Services</c>) that resolves a service by name.
/// Implemented as a <see cref="DynamicObject"/> so member access reads naturally —
/// <c>client.Services.GetToken</c> — mirroring the original's attribute-based dispatch.
/// For statically-typed access, use <see cref="Get{TService}"/>.
/// </summary>
public sealed class ServiceNamespace : DynamicObject
{
    private readonly BaseApiClient _client;
    private readonly IReadOnlyDictionary<string, Type> _services;

    internal ServiceNamespace(BaseApiClient client, IReadOnlyDictionary<string, Type> services)
    {
        _client = client;
        _services = services;
    }

    /// <summary>Creates a fresh instance of the named service.</summary>
    /// <exception cref="MissingMemberException">No service with that name exists.</exception>
    public object Resolve(string serviceName)
    {
        if (!_services.TryGetValue(serviceName, out var serviceType))
        {
            throw new MissingMemberException($"Service '{serviceName}' not found");
        }

        return Activator.CreateInstance(serviceType, _client)!;
    }

    /// <summary>Creates a fresh, statically-typed instance of <typeparamref name="TService"/>.</summary>
    public TService Get<TService>() where TService : IService =>
        (TService)Activator.CreateInstance(typeof(TService), _client)!;

    /// <inheritdoc />
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = Resolve(binder.Name);
        return true;
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetDynamicMemberNames() => _services.Keys;
}
