namespace Nexsure.Api.Enums;

/// <summary>The HTTP verbs a service can declare via <c>AbstractService{T}.Method</c>.</summary>
public enum HttpMethod
{
    Get,
    Post,
    Put,
    Patch,
    Delete,
}

/// <summary>Helpers for turning a <see cref="HttpMethod"/> into the wire-format verb string.</summary>
public static class HttpMethodExtensions
{
    /// <summary>Returns the canonical uppercase HTTP verb (e.g. <c>"GET"</c>).</summary>
    public static string ToHttpString(this HttpMethod method) => method switch
    {
        HttpMethod.Get => "GET",
        HttpMethod.Post => "POST",
        HttpMethod.Put => "PUT",
        HttpMethod.Patch => "PATCH",
        HttpMethod.Delete => "DELETE",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown HTTP method"),
    };
}
