using System.Reflection;

namespace Nexsure.Api.Services;

/// <summary>
/// A bag of call arguments forwarded into a service's override hooks — the C# stand-in for
/// Python's <c>**kwargs</c>. Build one from an anonymous object
/// (<c>new { client_id = 42 }</c>), a dictionary, or nothing at all.
/// </summary>
public sealed class ServiceArgs
{
    private readonly IReadOnlyDictionary<string, object?> _data;

    private ServiceArgs(IReadOnlyDictionary<string, object?> data) => _data = data;

    /// <summary>An empty argument bag.</summary>
    public static ServiceArgs Empty { get; } = new(new Dictionary<string, object?>());

    /// <summary>
    /// Normalizes any supported argument carrier into a <see cref="ServiceArgs"/>:
    /// <c>null</c>, an existing <see cref="ServiceArgs"/>, an
    /// <see cref="IDictionary{TKey,TValue}"/>, or an arbitrary object whose public
    /// properties become the arguments.
    /// </summary>
    public static ServiceArgs From(object? args)
    {
        switch (args)
        {
            case null:
                return Empty;
            case ServiceArgs existing:
                return existing;
            case IDictionary<string, object?> dict:
                return new ServiceArgs(new Dictionary<string, object?>(dict));
            default:
                var bag = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var prop in args.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetIndexParameters().Length == 0)
                    {
                        bag[prop.Name] = prop.GetValue(args);
                    }
                }
                return new ServiceArgs(bag);
        }
    }

    /// <summary>Whether an argument with the given key was supplied.</summary>
    public bool Has(string key) => _data.ContainsKey(key);

    /// <summary>The raw, untyped value for a key (or <c>null</c> if absent).</summary>
    public object? GetRaw(string key) => _data.TryGetValue(key, out var v) ? v : null;

    /// <summary>
    /// Returns a required argument coerced to <typeparamref name="T"/>.
    /// Throws <see cref="ArgumentException"/> if the key is missing.
    /// </summary>
    public T Get<T>(string key)
    {
        if (!_data.TryGetValue(key, out var value))
        {
            throw new ArgumentException($"Missing required argument '{key}'.");
        }

        return Coerce<T>(value, key);
    }

    /// <summary>
    /// Returns an optional argument coerced to <typeparamref name="T"/>, or
    /// <paramref name="defaultValue"/> when the key is missing or its value is <c>null</c>.
    /// </summary>
    public T? GetOptional<T>(string key, T? defaultValue = default)
    {
        if (!_data.TryGetValue(key, out var value) || value is null)
        {
            return defaultValue;
        }

        return Coerce<T>(value, key);
    }

    private static T Coerce<T>(object? value, string key)
    {
        if (value is null)
        {
            return default!;
        }

        if (value is T typed)
        {
            return typed;
        }

        try
        {
            var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (target.IsEnum && value is string enumName)
            {
                return (T)Enum.Parse(target, enumName, ignoreCase: true);
            }

            return (T)System.Convert.ChangeType(value, target);
        }
        catch (Exception exc) when (exc is InvalidCastException or FormatException or OverflowException or ArgumentException)
        {
            throw new ArgumentException(
                $"Argument '{key}' could not be converted to {typeof(T).Name}.", exc);
        }
    }
}
