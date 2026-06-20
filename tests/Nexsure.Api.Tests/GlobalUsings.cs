// Match the library: the bare name HttpMethod means our enum. Tests never name the
// System.Net.Http.HttpMethod type directly (they only read request.Method).
global using HttpMethod = Nexsure.Api.Enums.HttpMethod;
