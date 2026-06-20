using System.Reflection;
using Nexsure.Api.Services;

namespace Nexsure.Api;

/// <summary>
/// Discovers every concrete <see cref="IService"/> in an assembly, keyed by class name, so
/// new services light up just by existing — no registration required.
/// </summary>
public sealed class ServiceFactory
{
    /// <summary>Discovered services: <c>serviceName -&gt; serviceType</c>.</summary>
    public IReadOnlyDictionary<string, Type> Services { get; }

    /// <param name="assembly">The assembly to scan, or <c>null</c> for the library assembly.</param>
    public ServiceFactory(Assembly? assembly = null)
    {
        assembly ??= typeof(ServiceFactory).Assembly;
        Services = LoadServices(assembly);
    }

    private static IReadOnlyDictionary<string, Type> LoadServices(Assembly assembly)
    {
        var services = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || !type.IsClass)
            {
                continue;
            }

            if (typeof(IService).IsAssignableFrom(type))
            {
                services[type.Name] = type;
            }
        }

        return services;
    }
}
