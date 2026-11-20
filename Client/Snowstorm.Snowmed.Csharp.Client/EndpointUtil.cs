using Microsoft.AspNetCore.Http.HttpResults;

namespace Snowstorm.Snowmed.Csharp.Client;

/// <summary>
/// Provides common behavior for search endpoints.
/// </summary>
public interface IEndpointUtil
{
    /// <summary>
    /// Validates a search term and returns the matching results.
    /// </summary>
    /// <typeparam name="TResponse">The response item type.</typeparam>
    /// <param name="term">The search term supplied by the request.</param>
    /// <param name="searchAsync">The search operation to execute for a valid term.</param>
    /// <param name="cancellationToken">The token used to cancel the search.</param>
    /// <returns>A bad request for a missing term or the search results.</returns>
    Task<Results<Ok<IReadOnlyList<TResponse>>, BadRequest<string>>> GetListAsync<TResponse>(
        string? term,
        Func<string, CancellationToken, Task<IReadOnlyList<TResponse>>> searchAsync,
        CancellationToken cancellationToken);
}

/// <summary>
/// Implements common behavior for search endpoints.
/// </summary>
public sealed class EndpointUtil : IEndpointUtil
{
    public async Task<Results<Ok<IReadOnlyList<TResponse>>, BadRequest<string>>> GetListAsync<TResponse>(
        string? term,
        Func<string, CancellationToken, Task<IReadOnlyList<TResponse>>> searchAsync,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchAsync);

        if (string.IsNullOrWhiteSpace(term))
        {
            return TypedResults.BadRequest("A search term must be provided in the 'term' query parameter.");
        }

        var results = await searchAsync(term, cancellationToken);
        return TypedResults.Ok(results);
    }
}
