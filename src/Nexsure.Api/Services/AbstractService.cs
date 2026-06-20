using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nexsure.Api.Enums;
using Nexsure.Api.Json;

namespace Nexsure.Api.Services;

/// <summary>
/// Base class for every Nexsure service. Subclasses declare three things —
/// <see cref="CredentialsType"/>, <see cref="Method"/>, and <see cref="UrlPath"/> — and
/// optionally override the request hooks (<see cref="GetUriParameters"/>,
/// <see cref="GetQueryParams"/>, <see cref="GetBody"/>, <see cref="GetFormData"/>,
/// <see cref="GetHeaders"/>) and the response hook (<see cref="ParseJson"/>). The base
/// class wires those into a single request and deserializes the JSON response into
/// <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// The response type is the generic parameter <typeparamref name="T"/>, so unlike the
/// original Python design there is no separate <c>get_response_type</c> to implement.
/// Endpoints whose JSON wraps the payload (e.g. <c>{ "Clients": { "Client": [...] } }</c>)
/// override <see cref="ParseJson"/> and use the <see cref="AsArray"/> / <see cref="Deserialize{TItem}"/>
/// helpers to navigate.
/// </remarks>
/// <typeparam name="T">The response model this service deserializes into.</typeparam>
public abstract class AbstractService<T> : IService
{
    /// <summary>The owning client (for credential lookup and transport).</summary>
    protected BaseApiClient ApiClient { get; }

    /// <summary>The <see cref="HttpClient"/> shared with the owning client.</summary>
    protected HttpClient Http { get; }

    private readonly ILogger _logger;

    /// <param name="apiClient">The client this service runs against.</param>
    protected AbstractService(BaseApiClient apiClient)
    {
        ApiClient = apiClient;
        Http = apiClient.Http;
        _logger = apiClient.LoggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
    }

    // --- Required declarations ---

    /// <inheritdoc />
    public abstract Type CredentialsType { get; }

    /// <inheritdoc />
    public abstract HttpMethod Method { get; }

    /// <inheritdoc />
    public abstract string UrlPath { get; }

    // --- Optional overrides ---

    /// <summary>URI segment appended to the path (e.g. an id). Empty by default.</summary>
    protected virtual string GetUriParameters(ServiceArgs args) => string.Empty;

    /// <summary>Query-string parameters appended to the URL, or <c>null</c>.</summary>
    protected virtual IDictionary<string, object?>? GetQueryParams(ServiceArgs args) => null;

    /// <summary>The JSON request body, or <c>null</c>.</summary>
    protected virtual IDictionary<string, object?>? GetBody(ServiceArgs args) => null;

    /// <summary>Form-url-encoded request data, or <c>null</c>.</summary>
    protected virtual IDictionary<string, string>? GetFormData(ServiceArgs args) => null;

    /// <summary>Headers merged on top of the credential's auth header. Empty by default.</summary>
    protected virtual IDictionary<string, string> GetHeaders(ServiceArgs args) =>
        new Dictionary<string, string>();

    /// <summary>
    /// Maps the parsed JSON response into <typeparamref name="T"/>. The default deserializes
    /// the whole document; override for endpoints that wrap their payload.
    /// </summary>
    protected virtual T ParseJson(JsonNode? root)
    {
        if (root is null)
        {
            throw new JsonException($"Empty response from {UrlPath}.");
        }

        return root.Deserialize<T>(NexsureJson.Options)
            ?? throw new JsonException($"Response from {UrlPath} deserialized to null for {typeof(T).Name}.");
    }

    // --- Helpers for ParseJson overrides ---

    /// <summary>Treats a node as a list: a JSON array yields its items, a single object yields one item, null yields none.</summary>
    protected static IEnumerable<JsonNode?> AsArray(JsonNode? node) => node switch
    {
        JsonArray array => array,
        null => Array.Empty<JsonNode?>(),
        _ => new[] { node },
    };

    /// <summary>Deserializes a node into <typeparamref name="TItem"/> using the shared options.</summary>
    protected static TItem? Deserialize<TItem>(JsonNode? node) =>
        node is null ? default : node.Deserialize<TItem>(NexsureJson.Options);

    // --- Pipeline ---

    /// <summary>Finds the first credential matching <see cref="CredentialsType"/> in the client's list.</summary>
    /// <exception cref="CredentialsNotFoundException">No matching credential is present.</exception>
    public Credentials.Credentials GetCredentials()
    {
        foreach (var credential in ApiClient.Credentials)
        {
            if (CredentialsType.IsInstanceOfType(credential))
            {
                return credential;
            }
        }

        throw new CredentialsNotFoundException(CredentialsType);
    }

    internal string BuildUrl(string uriParameters, IDictionary<string, object?>? queryParams)
    {
        var path = UrlPath.Trim('/');
        if (!string.IsNullOrEmpty(uriParameters))
        {
            path = $"{path}/{uriParameters.Trim('/')}";
        }

        var url = $"{Constants.BaseUrl.TrimEnd('/')}/{path}";

        var query = BuildQueryString(queryParams);
        return query.Length == 0 ? url : $"{url}?{query}";
    }

    internal static string BuildQueryString(IDictionary<string, object?>? queryParams)
    {
        if (queryParams is null || queryParams.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var (key, value) in queryParams)
        {
            if (value is null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty));
        }

        return builder.ToString();
    }

    internal IDictionary<string, string> BuildHeaders(Credentials.Credentials credentials, ServiceArgs args)
    {
        var headers = GetHeaders(args);
        var credentialHeaders = credentials.GetHeader();
        if (credentialHeaders is null)
        {
            return headers;
        }

        var merged = new Dictionary<string, string>(credentialHeaders);
        foreach (var (key, value) in headers)
        {
            merged[key] = value;
        }

        return merged;
    }

    internal IDictionary<string, object?>? BuildBody(Credentials.Credentials credentials, ServiceArgs args)
    {
        var serviceBody = GetBody(args);
        var credentialBody = credentials.GetJsonBody();

        var serviceHasBody = serviceBody is { Count: > 0 };
        var credentialHasBody = credentialBody is { Count: > 0 };

        if (serviceHasBody && credentialHasBody)
        {
            var merged = new Dictionary<string, object?>(credentialBody!);
            foreach (var (key, value) in serviceBody!)
            {
                merged[key] = value;
            }

            return merged;
        }

        return serviceHasBody ? serviceBody : credentialBody;
    }

    internal HttpRequestMessage BuildRequest(
        string url,
        IDictionary<string, string> headers,
        IDictionary<string, object?>? body,
        IDictionary<string, string>? formData)
    {
        var request = new HttpRequestMessage(new System.Net.Http.HttpMethod(Method.ToHttpString()), url);

        // JSON body takes precedence; otherwise form data. (No service sets both.)
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, mediaType: null, options: NexsureJson.Options);
        }
        else if (formData is not null)
        {
            request.Content = new FormUrlEncodedContent(formData);
        }

        foreach (var (key, value) in headers)
        {
            if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            request.Headers.TryAddWithoutValidation(key, value);
        }

        return request;
    }

    internal async Task<T> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        JsonNode? root;
        try
        {
            root = await response.Content.ReadFromJsonAsync<JsonNode>(NexsureJson.Options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exc) when (exc is JsonException or NotSupportedException)
        {
            throw new JsonException(
                $"Expected JSON response from {UrlPath}, got Content-Type: {response.Content.Headers.ContentType}", exc);
        }

        try
        {
            return ParseJson(root);
        }
        catch (Exception exc) when (exc is JsonException or NotSupportedException or InvalidOperationException)
        {
            throw new JsonException(
                $"Failed to deserialize response from {UrlPath} into {typeof(T).Name}: {exc.Message}", exc);
        }
    }

    /// <summary>
    /// Runs the service: resolves credentials, builds the URL/query/headers/body, sends the
    /// request, and deserializes the response into <typeparamref name="T"/>.
    /// </summary>
    /// <param name="args">
    /// Call arguments forwarded to the override methods. Pass an anonymous object
    /// (<c>new { client_id = 42 }</c>), an <see cref="IDictionary{TKey,TValue}"/>, a
    /// <see cref="ServiceArgs"/>, or <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task<T> ExecuteAsync(object? args = null, CancellationToken cancellationToken = default)
    {
        var resolvedArgs = ServiceArgs.From(args);
        var credentials = GetCredentials();

        var headers = BuildHeaders(credentials, resolvedArgs);
        var body = BuildBody(credentials, resolvedArgs);
        var formData = GetFormData(resolvedArgs);
        var queryParams = GetQueryParams(resolvedArgs);
        var uriParameters = GetUriParameters(resolvedArgs);
        var url = BuildUrl(uriParameters, queryParams);

        _logger.LogDebug("Executing {Method} {Url}", Method.ToHttpString(), url);

        using var request = BuildRequest(url, headers, body, formData);
        using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await ParseResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
}
