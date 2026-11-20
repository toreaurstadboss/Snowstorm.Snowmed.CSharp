namespace Snowstorm.Snowmed.Csharp.Client;

/// <summary>
/// Represents a concept name and its SNOMED CT identifier.
/// </summary>
public sealed record ConceptResponse(string Name, string ConceptId);

/// <summary>
/// Represents a concept description and its SNOMED CT identifier.
/// </summary>
public sealed record ConceptDescriptionResponse(string Term, string DescriptionId, string Type, string ConceptId);
