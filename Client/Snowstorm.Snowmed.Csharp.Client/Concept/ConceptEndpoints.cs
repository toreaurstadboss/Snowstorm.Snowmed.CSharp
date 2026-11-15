using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Snowstorm.Snowmed.Csharp.Client;

/// <summary>
/// Represents a concept name and its SNOMED CT identifier.
/// </summary>
public sealed record ConceptResponse(string Name, string ConceptId);

/// <summary>
/// Maps concept search and lookup endpoints.
/// </summary>
public static class ConceptEndpoints
{
    /// <summary>
    /// Maps the concept search and lookup endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="branch">The Snowstorm branch to query.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapConceptEndpoints(this IEndpointRouteBuilder app, string branch = "MAIN")
    {

        //GET: Search concepts by name : /api/concepts
        app.MapGet(
                "/api/concepts",
                [EndpointName("SearchConcepts")]
                [EndpointSummary("Search concepts by name")]
                [EndpointDescription("Returns matching active SNOMED CT concepts with their preferred name and concept identifier.")]
                async Task<Results<Ok<IReadOnlyList<ConceptResponse>>, BadRequest<string>>> (
                    [Description("The concept name or term to search for.")] string? term,
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
                });

        //GET: Search concepts by identifier : /api/concepts/{conceptId}
        app.MapGet(
                "/api/concepts/{conceptId}",
                [EndpointName("GetConceptById")]
                [EndpointSummary("Get a concept by identifier")]
                [EndpointDescription("Returns an active SNOMED CT concept with the requested identifier.")]
                async Task<Results<Ok<ConceptResponse>, NotFound>> (
                    [Description("The SNOMED CT concept identifier.")] string conceptId,
                    ISnowstormClient snowstormClient,
                    CancellationToken cancellationToken) =>
                {
                    var concept = await GetConceptByIdAsync(
                        snowstormClient,
                        branch,
                        conceptId,
                        cancellationToken);

                    return concept is null
                        ? TypedResults.NotFound()
                        : TypedResults.Ok(concept);
                });

        return app;
    }

    private static async Task<IReadOnlyList<ConceptResponse>> SearchConceptsAsync(
        ISnowstormClient snowstormClient,
        string branch,
        string term,
        CancellationToken cancellationToken)
    {
        var searchResult = await QueryConceptsAsync(
            snowstormClient,
            branch,
            term,
            conceptIds: null,
            limit: 100,
            cancellationToken);

        return searchResult.Items
            .Select(MapConcept)
            .ToArray();
    }

    private static async Task<ConceptResponse?> GetConceptByIdAsync(
        ISnowstormClient snowstormClient,
        string branch,
        string conceptId,
        CancellationToken cancellationToken)
    {

        var searchResult = await QueryConceptsAsync(
            snowstormClient,
            branch,
            term: null,
            conceptIds: [conceptId],
            limit: 1,
            cancellationToken);

        var concept = searchResult.Items.FirstOrDefault();
        return concept is null ? null : MapConcept(concept);
    }

    private static Task<ItemsPage> QueryConceptsAsync(
        ISnowstormClient snowstormClient,
        string branch,
        string? term,
        IEnumerable<string>? conceptIds,
        int limit,
        CancellationToken cancellationToken)
    {
        return snowstormClient.FindConceptsAsync(
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
            conceptIds: conceptIds,
            returnIdOnly: false,
            offset: 0,
            limit: limit,
            searchAfter: null,
            accept_Language: "en",
            cancellationToken);
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
