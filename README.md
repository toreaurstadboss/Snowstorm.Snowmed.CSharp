# Snowstorm.Snowmed.CSharp

A small .NET 10 minimal API that consumes a local Snowstorm server through a
generated C# client.

## Requirements

- .NET SDK 10
- Docker, to run the Snowstorm server

## Run

Start Snowstorm from the `Server` directory:

```powershell
cd Server
docker compose up
```

In another terminal, run the consumer API from the repository root:

```powershell
dotnet run --project Client\Snowstorm.Snowmed.Csharp.Client
```

The API uses `http://localhost:8080` for Snowstorm and searches the `MAIN`
branch by default. These settings are in
`Client\Snowstorm.Snowmed.Csharp.Client\appsettings.json`.

### Search concepts by name

```http
GET http://localhost:5193/api/concepts?term=diabetes
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

## Add a consumer method

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

## Tests

Integration tests call the running Snowstorm server. Start it by running
`docker compose up` from the `Server` directory, then run:

```powershell
dotnet test Client\Snowstorm.Snowmed.Csharp.Client\Snowstorm.Snowmed.Csharp.Client.slnx
```
