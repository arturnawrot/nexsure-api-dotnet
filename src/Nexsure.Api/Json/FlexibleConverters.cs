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
