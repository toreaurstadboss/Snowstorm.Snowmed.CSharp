using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Snowstorm.Snowmed.Csharp.Client;

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
                        (searchTerm, token) => ConceptEndpointsHelper.SearchConceptsAsync(
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
                    var concept = await ConceptEndpointsHelper.GetConceptByIdAsync(
                        snowstormClient,
                        branch,
                        conceptId,
                        cancellationToken);

                    return concept is null
                        ? TypedResults.NotFound()
                        : TypedResults.Ok(concept);
                });

        app.MapGet(
            "/api/concepts/{conceptId}/descriptions",
            [EndpointName("GetConceptDescriptionsById")]
            [EndpointSummary("Get concept descriptions by identifier")]
            async Task<Results<Ok<IReadOnlyList<ConceptDescriptionResponse>>, BadRequest<string>>> (string conceptId,
            ISnowstormClient snowstormClient,
            IEndpointUtil endpointUtil,
            CancellationToken cancellationToken) =>
            {
                var concepts = await endpointUtil.SearchByTermAsync(
                    conceptId,
                    (searchTerm, token) => ConceptEndpointsHelper.SearchConceptDescriptionsAsync(
                        snowstormClient,
                        branch,
                        searchTerm,
                        token),
                    cancellationToken);
                return concepts;
            }
        );

        return app;
    }
}
