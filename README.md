# Snowstorm.Snowmed.CSharp

A small .NET 10 minimal API that consumes a local Snowstorm server through a
generated C# client.

For details on the generated client for this API and how to regenerate it with
NSwag, see the [API client README](./Client/Snowstorm.Snowmed.Csharp.ApiClient/README.md).

## Requirements

- .NET SDK 10
- Docker, to run the Snowstorm server

## Running the Snowstorm Snomed CT Terminology server

Start Snowstorm _Server_ from the `Server` directory:

```powershell
cd Server
docker compose up
```

This launches the Snowstorm server running at :
http://localhost:8080 

Note that Docker engine / service must be running for this to work.


## Running the Snowwstorm Snomed CT Terminology consumer client
In another terminal, run the consumer API client from the repository root:

```powershell
dotnet run --project Client\Snowstorm.Snowmed.Csharp.Client
```

The client is running at port 7130 over HTTPS and Swagger page here:

https://localhost:7130/swagger/index.html

The API uses the Snomed Ct server running `http://localhost:8080` for Snowstorm and searches the `MAIN`
branch by default. These settings are in
`Client\Snowstorm.Snowmed.Csharp.Client\appsettings.json`.

### Search concepts by name

```http
GET https://localhost:7139/api/concepts?term=diabetes
```

The response is a list of matching concept names and identifiers:

```json
[
  {
    "name": "Diabetes mellitus",
    "conceptId": "73211009"
  }
]
```

The `.http` file in the client project contains more example requests.

## Generated Snowstorm C# client using NSwag studio

1. Find the appropriate operation on the generated `ISnowstormClient` interface
   in `Snowstormclient.cs`. That client contains the Snowstorm API details; avoid
   duplicating its HTTP requests.
2. Add a route to the relevant endpoint mapping class (currently
   `ConceptEndpoints`). Call the generated client method there and map its result
   to a small response record for this API.
3. Keep reusable endpoint behavior in `IEndpointUtil` / `EndpointUtil`, rather
   than adding concept-specific logic there.
4. Add a request example to the `.http` file and an integration test in the
   separate test project, `Client\Snowstorm.Snowmed.Csharp.Client.Tests`.

## Generated Snowstorm client

`Snowstormclient.cs` was generated with
[NSwagStudio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio) from the
Snowstorm OpenAPI definition. Treat it as generated code; add consumer behavior
to the endpoint mappings rather than editing generated operations by hand.

## Generated client for this API itself

The Snowstorm client that was generated got all the functionality of Snowstorm Snomed CT. We have provided tailored consumer client in the  Snowstorm.Snowmed.Csharp.Client generated via `NSWag.MSbuild` inside the `Showstorm.Snowmed.Csharp.ApiClient`. 

The reusable C# client for this Minimal API is in
`Client\Snowstorm.Snowmed.Csharp.ApiClient`. It is generated from this API's
OpenAPI document and kept separate from the upstream Snowstorm client.

Start this API with the HTTPS launch profile:

```powershell
dotnet run --project Client\Snowstorm.Snowmed.Csharp.Client --launch-profile https
```

In another terminal, generate the client:

```powershell
dotnet msbuild Client\Snowstorm.Snowmed.Csharp.ApiClient\Snowstorm.Snowmed.Csharp.ApiClient.csproj /t:GenerateApiClient
```

The default document URL is `https://localhost:7130/openapi/v1.json`. Override
it with `/p:OpenApiUrl=...` when the API is hosted elsewhere. The generated
source is checked in, so building or consuming the API client library does not
require the API to be running.

To consume it from another .NET project, add a project reference and register
the generated client with an `HttpClient` configured for the API's base URL:

```powershell
dotnet add <consumer-project.csproj> reference Client\Snowstorm.Snowmed.Csharp.ApiClient\Snowstorm.Snowmed.Csharp.ApiClient.csproj
```

```csharp
builder.Services.AddHttpClient<ISnowstormApiClient, SnowstormApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7130/");
});
```

The generated client exposes operations such as `SearchConceptsAsync` and
`GetConceptByIdAsync`.

## Tests

Integration tests call the running Snowstorm server. Start it by running
`docker compose up` from the `Server` directory, then run:

```powershell
dotnet test Client\Snowstorm.Snowmed.Csharp.Client\Snowstorm.Snowmed.Csharp.Client.slnx
```
