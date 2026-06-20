# Nexsure.Api (.NET)

A .NET 10 client library for the [Nexsure](https://nexsure.com) EAI (Enterprise
Application Integration) API. A faithful C# port of the Python `nexsure-api` library,
built around the same clean, extensible service pattern — adding new endpoints takes minutes.

> **Note:** This library is a work in progress. Not all Nexsure API endpoints are
> implemented yet. See [Available Services](#available-services) for what's ready, and
> [Adding a New Service](#adding-a-new-service) for how to add the rest.

---

## Requirements

- .NET 10 SDK

```bash
dotnet build
dotnet test
```

---

## Quick Start

```csharp
using Nexsure.Api;
using Nexsure.Api.Credentials;

using var client = new NexsureApiClient(new Credentials[] { new NoAuth() });

var token = await client.Services.GetToken.ExecuteAsync(new
{
    integration_key = "your-key",
    integration_login = "your-login",
    integration_pwd = "your-password",
});

client.AddCredentials(new NexsureCredentials((string)token.AccessToken));

var clients = await client.Services.GetClientList.ExecuteAsync(new { client_name = "acme" });
```

`client.Services` is `dynamic` for natural member access, mirroring the Python original.
For full static typing, resolve the service explicitly:

```csharp
using Nexsure.Api.Services.Auth;

var token = await client.ServiceNamespace.Get<GetToken>().ExecuteAsync(new
{
    integration_key = "your-key",
    integration_login = "your-login",
    integration_pwd = "your-password",
}); // token is a strongly-typed GetTokenResponse
```

### Call arguments

Every `ExecuteAsync` takes a single `args` object whose members are forwarded to the
service's hooks (`GetUriParameters` / `GetQueryParams` / `GetBody` / `GetFormData` /
`GetHeaders`) — the C# stand-in for Python's `**kwargs`. Argument names match the Python
originals (snake_case). Pass an anonymous object, an `IDictionary<string, object?>`, or `null`.

---

## Authentication

Nexsure uses a bearer token. Call `GetToken` first — it requires a `NoAuth` credential to
be present — then add the result to your client.

```csharp
using var client = new NexsureApiClient(new Credentials[] { new NoAuth() });

var token = await client.Services.GetToken.ExecuteAsync(new
{
    integration_key = "your-key",
    integration_login = "your-login",
    integration_pwd = "your-password",
});

client.AddCredentials(new NexsureCredentials((string)token.AccessToken));

// All authenticated services are now available
var policy = await client.Services.LoadPolicyByPolicyId.ExecuteAsync(new { policy_id = 12345 });
```

---

## Available Services

Services live under `Services/<Category>/` and are auto-discovered at runtime by
`ServiceFactory` (reflection) — no registration needed. All are exposed flat under
`client.Services.*`.

### Client Services

| Service | Method | Endpoint | Description |
|---|---|---|---|
| `GetToken` | POST | `/auth/gettoken` | Authenticate and get bearer token |
| `GetClientList` | POST | `/clients/getclientlist` | List clients by name |
| `GetClientById` | POST | `/clients/getclientbyid` | Get client details by id |
| `GetClientByName` | POST | `/clients/getclientbyname` | Get client by first/last/company name |
| `ClientSearch` | POST | `/clients/clientsearch` | Search clients by multiple criteria |
| `AddNewClient` | POST | `/clients/addnewclient` | Create a new client |
| `UpdateClient` | POST | `/clients/updateclient` | Update an existing client |

### Policy Services

| Service | Method | Endpoint | Description |
|---|---|---|---|
| `AddSinglePolicy` | POST | `/policy/addsinglepolicy` | Add a policy to a client |
| `LoadPolicyByClientId` | POST | `/policy/loadpolicybyclientid` | Get all policies for a client |
| `LoadPolicyByPolicyId` | POST | `/policy/loadpolicybypolicyid` | Get a policy by id |
| `PolicyLoad` | POST | `/policy/policyload` | Load a policy by number and date range |
| `PolicySearchWithDetails` | POST | `/policy/policysearchwithdetails` | Search policies with full details |

### Claims, Attachments, Organization, Lookup

| Service | Method | Endpoint | Description |
|---|---|---|---|
| `ClaimSearch` | POST | `/claims/claimsearch` | Search claims by multiple criteria |
| `AddAttachment` | POST | `/attachments/addattachment` | Upload a file attachment |
| `GetAttachmentList` | POST | `/attachments/getattachmentlist` | List attachments for a client or policy |
| `SearchBranchByBranchName` | POST | `/organization/searchbranchbybranchname` | Search branches by name |
| `SearchDepartmentByName` | POST | `/organization/searchdepartmentbyname` | Search departments by name |
| `SicNaicsSearch` | POST | `/lookupdata/sicnaicssearch` | Search industry classification codes |
| `ListLookupManagementValues` | POST | `/lookupdata/listlookupmanagementvalues` | Get lookup data categories |

---

## Demo

A console app under [demo/Nexsure.Api.Demo](demo/Nexsure.Api.Demo) walks through the full
flow end-to-end (the C# port of the original `demo.ipynb`): authenticate, discover
branches/departments, create a client, add a policy, attach a file, read lookup data, and
search clients/policies/claims.

```bash
dotnet run --project demo/Nexsure.Api.Demo
```

---

## Adding a New Service

Every service follows the same pattern.

### 1. Create the response model and service

```csharp
using Nexsure.Api;
using Nexsure.Api.Credentials;
using Nexsure.Api.Enums;
using Nexsure.Api.Services;

namespace Nexsure.Api.Services.MyArea;

public sealed record MyEndpointResponse
{
    public int? Id { get; init; }
    public string? Name { get; init; }
}

public sealed class MyEndpoint : AbstractService<MyEndpointResponse>
{
    public MyEndpoint(BaseApiClient apiClient) : base(apiClient) { }

    // --- Required ---
    public override Type CredentialsType => typeof(NexsureCredentials); // or NoAuth
    public override HttpMethod Method => HttpMethod.Post;
    public override string UrlPath => "/my/endpoint";

    // --- Optional hooks (override only what the endpoint uses) ---
    protected override IDictionary<string, object?>? GetQueryParams(ServiceArgs args) =>
        new Dictionary<string, object?> { ["id"] = args.Get<int>("item_id") };

    protected override IDictionary<string, object?>? GetBody(ServiceArgs args) =>
        new Dictionary<string, object?> { ["name"] = args.Get<string>("name") };

    protected override IDictionary<string, string>? GetFormData(ServiceArgs args) =>
        new Dictionary<string, string> { ["formKey"] = "formValue" };

    protected override IDictionary<string, string> GetHeaders(ServiceArgs args) =>
        new Dictionary<string, string> { ["Accept"] = "application/json" };
}
```

There is no `GetResponseType` to implement — the response type is the generic parameter,
carried by C#'s reified generics. For endpoints whose JSON wraps the payload (e.g.
`{ "Clients": { "Client": [...] } }`), override `ParseJson` and use the `AsArray` /
`Deserialize<T>` helpers to navigate.

### 2. That's it

`ServiceFactory` discovers every concrete `AbstractService<T>` in the assembly, so your
new service is immediately available as `client.Services.MyEndpoint`.

### How the service layer works

When you call `client.Services.MyEndpoint.ExecuteAsync(new { item_id = 42 })`:

1. **Credentials lookup** — finds the first credential of the type returned by
   `CredentialsType`. Throws `CredentialsNotFoundException` if none match.
2. **URL construction** — combines `Constants.BaseUrl`, `UrlPath`, and query params.
3. **Headers** — merges the credential's auth header with `GetHeaders(args)`; service wins.
4. **Body / form data** — `GetBody` (JSON) or `GetFormData` (form-url-encoded).
5. **HTTP request** — sent via the shared `HttpClient`, then `EnsureSuccessStatusCode()`.
6. **Deserialization** — `ParseJson` maps the JSON response into the model `T`.

---

## HTTP layer

The library depends directly on `System.Net.Http.HttpClient` — the standard .NET HTTP
abstraction. Cross-cutting concerns use `DelegatingHandler`s (see `LoggingHandler`), the
idiomatic equivalent of Guzzle/PSR-18 middleware. The default client applies a 5s connect
timeout (`SocketsHttpHandler.ConnectTimeout`) and a 30s overall timeout. To supply your
own client (e.g. from `IHttpClientFactory`, with retry/resilience handlers), pass it to
the constructor:

```csharp
using var client = new NexsureApiClient(credentials, httpClientFactory.CreateClient("nexsure"));
```

The EAI API is loosely typed — booleans and ids sometimes arrive as JSON strings rather
than booleans/numbers. The shared `JsonSerializerOptions` include tolerant converters so a
single stringly-typed field can't fail an entire response parse.

---

## Running Tests

```bash
dotnet test
```

---

## License

MIT
