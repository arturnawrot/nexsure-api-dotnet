using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexsure.Api.Json;

/// <summary>
/// Centralized <see cref="JsonSerializerOptions"/> used across the library.
/// </summary>
/// <remarks>
/// The Nexsure API speaks PascalCase (e.g. <c>ClientID</c>, <c>PolicyNumber</c>), which
/// matches the model property names verbatim — so no naming policy is applied. Case
/// insensitivity covers minor casing drift, and request-body dictionary keys are written
/// exactly as authored (e.g. <c>"SearchType"</c>, <c>"returnContentType"</c>). The
/// <c>GetToken</c> endpoint is the lone exception (OAuth snake_case) and uses explicit
/// <see cref="JsonPropertyNameAttribute"/>s on its model.
/// </remarks>
public static class NexsureJson
{
    /// <summary>Shared serializer options.</summary>
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DictionaryKeyPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        // The EAI API is loosely typed: booleans and ids arrive as JSON booleans/numbers
        // OR as strings. These converters tolerate either so one field can't fail a parse.
        options.Converters.Add(new FlexibleBooleanConverter());
        options.Converters.Add(new FlexibleStringConverter());
        return options;
    }
}
