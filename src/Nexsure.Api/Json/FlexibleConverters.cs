using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexsure.Api.Json;

/// <summary>
/// Reads a boolean that the Nexsure EAI API may send as a real JSON boolean, a string
/// (<c>"true"</c>/<c>"false"</c>/<c>"1"</c>/<c>"0"</c>), or a number. Also covers
/// <see cref="Nullable{Boolean}"/>. Writes a normal JSON boolean.
/// </summary>
public sealed class FlexibleBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.GetDouble() != 0,
            JsonTokenType.String => ParseString(reader.GetString()),
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to bool."),
        };

    private static bool ParseString(string? value) =>
        value is not null &&
        (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1");

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);
}

/// <summary>
/// Reads a 32-bit integer that the Nexsure EAI API may send as a real JSON number or as a
/// string (the API is inconsistent about quoting ids — e.g. <c>"BranchID": "15"</c>). Empty
/// strings parse to <c>0</c>; combined with a registered <see cref="int"/> converter this
/// also covers <see cref="Nullable{Int32}"/>. Writes a normal JSON number.
/// </summary>
public sealed class FlexibleInt32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => ParseString(reader.GetString()),
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to int."),
        };

    private static int ParseString(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? 0
            : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}

/// <summary>
/// Reads a string that the API may send as an actual string, a number, or a boolean (the
/// EAI API is inconsistent about quoting ids and flags). Writes a normal JSON string.
/// </summary>
public sealed class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var l)
                ? l.ToString(CultureInfo.InvariantCulture)
                : reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to string."),
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}

/// <summary>
/// The Nexsure EAI API renders a collection that happens to hold a single element as a lone
/// JSON object instead of a one-item array — e.g. a client with one name comes back as
/// <c>"ClientNames": { ... }</c> rather than <c>"ClientNames": [ { ... } ]</c>. This factory
/// makes every <see cref="IReadOnlyList{T}"/> property tolerate either shape: a JSON array
/// yields its items, a single object/scalar yields a one-item list, and null yields an empty
/// list. It is the deserialization-time equivalent of the <c>AsArray</c> helper that the
/// manual <c>ParseJson</c> overrides already use.
/// </summary>
public sealed class SingleOrArrayListConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType &&
        typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(SingleOrArrayListConverter<>).MakeGenericType(elementType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class SingleOrArrayListConverter<TItem> : JsonConverter<IReadOnlyList<TItem>>
    {
        public override IReadOnlyList<TItem> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.Null => [],
                // List<TItem> isn't an IReadOnlyList<> so this re-enters the built-in
                // collection converter, not this factory — element converters still apply.
                JsonTokenType.StartArray => JsonSerializer.Deserialize<List<TItem>>(ref reader, options) ?? [],
                _ => JsonSerializer.Deserialize<TItem>(ref reader, options) is { } single ? [single] : [],
            };

        public override void Write(Utf8JsonWriter writer, IReadOnlyList<TItem> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }

            writer.WriteEndArray();
        }
    }
}
