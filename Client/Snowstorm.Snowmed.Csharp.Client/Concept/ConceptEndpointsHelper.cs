using System.Text.Json;

namespace Snowstorm.Snowmed.Csharp.Client;

/// <summary>
/// Provides helper operations for concept endpoints.
/// </summary>
internal static class ConceptEndpointsHelper
{
    internal static async Task<ConceptResponse?> GetConceptByIdAsync(
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

    private static ConceptDescriptionResponse MapConceptDescription(object item)
    {
        var conceptDescription = item switch
        {
            Description_Component description => description,
            JsonElement jsonElement => jsonElement.Deserialize<Description_Component>()
                ?? throw new InvalidOperationException("Snowstorm returned an empty concept."),
            _ => throw new InvalidOperationException("Snowstorm returned a concept in an unexpected format.")
        };

        return new ConceptDescriptionResponse(conceptDescription.Term, conceptDescription.DescriptionId, conceptDescription.Type, conceptDescription.ConceptId);
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

    private static Task<ItemsPageDescription_Component> QueryDescriptionsAsync(
        ISnowstormClient snowstormClient,
        string branch,
        string conceptId,
        IEnumerable<string>? conceptIds,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        return snowstormClient.FindDescriptionsAsync(
            branch,
            conceptIds: conceptIds,
            conceptId: conceptId,
            limit: limit,
            descriptionIds: null,
            offset: offset,
            cancellationToken: cancellationToken);
    }

    internal static async Task<IReadOnlyList<ConceptDescriptionResponse>> SearchConceptDescriptionsAsync(
        ISnowstormClient snowstormClient,
        string branch,
        string conceptId,
        CancellationToken cancellationToken)
    {
        var searchResult = await QueryDescriptionsAsync(
            snowstormClient,
            branch,
            conceptId: conceptId,    
            conceptIds: null,
            limit: 100,
            offset: 0,
            cancellationToken: cancellationToken);

        return searchResult.Items
            .Select(MapConceptDescription)
            .ToArray();
    }

    internal static async Task<IReadOnlyList<ConceptResponse>> SearchConceptsAsync(
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
}
