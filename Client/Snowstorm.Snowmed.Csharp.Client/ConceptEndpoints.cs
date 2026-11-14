using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Snowstorm.Snowmed.Csharp.Client;

/// <summary>
/// Represents a concept name and its SNOMED CT identifier.
/// </summary>
public sealed record ConceptResponse(string Name, string ConceptId);

/// <summary>
/// Maps concept search endpoints.
/// </summary>
public static class ConceptEndpoints
{
    /// <summary>
    /// Maps the concept search endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="branch">The Snowstorm branch to search.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapConceptEndpoints(this IEndpointRouteBuilder app, string branch = "MAIN")
    {
        app.MapGet(
                "/api/concepts",
                async Task<Results<Ok<IReadOnlyList<ConceptResponse>>, BadRequest<string>>> (
                    string? term,
                    ISnowstormClient snowstormClient,
                    IEndpointUtil endpointUtil,
                    CancellationToken cancellationToken) =>
                {
                    return await endpointUtil.SearchByTermAsync(
                        term,
                        (searchTerm, token) => SearchConceptsAsync(
                            snowstormClient,
                            branch,
                            searchTerm,
                            token),
                        cancellationToken);
                })
            .WithName("SearchConcepts")
            .WithSummary("Search concepts by name")
            .WithDescription("Returns matching active SNOMED CT concepts with their preferred name and concept identifier.")
            .Produces<IReadOnlyList<ConceptResponse>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IReadOnlyList<ConceptResponse>> SearchConceptsAsync(
        ISnowstormClient snowstormClient,
        string branch,
        string term,
        CancellationToken cancellationToken)
    {
        var searchResult = await snowstormClient.FindConceptsAsync(
            branch,
            activeFilter: true,
            definitionStatusFilter: null,
            module: null,
            term: term,
            termActive: true,
            descriptionType: null,
            language: ["en"],
            preferredIn: null,
            acceptableIn: null,
            preferredOrAcceptableIn: null,
            ecl: null,
            effectiveTime: null,
            isNullEffectiveTime: null,
            isPublished: null,
            statedEcl: null,
            includeLeafFlag: null,
            form: null,
            conceptIds: null,
            returnIdOnly: false,
            offset: 0,
            limit: 100,
            searchAfter: null,
            accept_Language: "en",
            cancellationToken);

        return searchResult.Items
            .Select(MapConcept)
            .ToArray();
    }

    private static ConceptResponse MapConcept(object item)
    {
        var concept = item switch
        {
            ConceptMini conceptMini => conceptMini,
            JsonElement jsonElement => jsonElement.Deserialize<ConceptMini>()
                ?? throw new InvalidOperationException("Snowstorm returned an empty concept."),
            _ => throw new InvalidOperationException("Snowstorm returned a concept in an unexpected format.")
        };

        return new ConceptResponse(concept.Pt.Term, concept.ConceptId);
    }
}
