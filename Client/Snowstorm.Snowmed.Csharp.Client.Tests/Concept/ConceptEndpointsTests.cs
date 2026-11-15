using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Snowstorm.Snowmed.Csharp.Client;

namespace Snowstorm.Snowmed.Csharp.Client.Tests;

[TestFixture]
public sealed class ConceptEndpointsTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestContext.Progress.WriteLine(
            "Snowstorm must be running before these integration tests. From the Server folder, run \"docker compose up\".");

        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetConceptById_ReturnsMatchingNameAndIdentifier()
    {
        var response = await _client.GetAsync("/api/concepts/73211009");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var concept = await response.Content.ReadFromJsonAsync<ConceptResponse>();

        concept.Should().Be(new ConceptResponse("Diabetes mellitus", "73211009"));
    }

    [Test]
    public async Task GetConceptById_ReturnsNotFound_WhenConceptDoesNotExist()
    {
        var response = await _client.GetAsync("/api/concepts/999999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task OpenApiDocument_IncludesConceptEndpointAttributes()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");
        var searchOperation = paths
            .GetProperty("/api/concepts")
            .GetProperty("get");
        var lookupOperation = paths
            .GetProperty("/api/concepts/{conceptId}")
            .GetProperty("get");

        searchOperation.GetProperty("operationId").GetString().Should().Be("SearchConcepts");
        searchOperation.GetProperty("summary").GetString().Should().Be("Search concepts by name");
        searchOperation.GetProperty("description").GetString().Should().Contain("matching active SNOMED CT concepts");
        searchOperation.GetProperty("parameters")[0]
            .GetProperty("description")
            .GetString()
            .Should()
            .Be("The concept name or term to search for.");

        lookupOperation.GetProperty("operationId").GetString().Should().Be("GetConceptById");
        lookupOperation.GetProperty("summary").GetString().Should().Be("Get a concept by identifier");
        lookupOperation.GetProperty("description").GetString().Should().Contain("active SNOMED CT concept");
        lookupOperation.GetProperty("parameters")[0]
            .GetProperty("description")
            .GetString()
            .Should()
            .Be("The SNOMED CT concept identifier.");
    }

    [TestCase("diabetes", "73211009", "Diabetes mellitus")]
    [TestCase("hypertension", "38341003", "Hypertensive disorder")]
    [TestCase("asthma", "195967001", "Asthma")]
    [TestCase("myocardial infarction", "22298006", "Myocardial infarction")]
    public async Task SearchConcepts_ReturnsMatchingNameAndIdentifier(
        string term,
        string expectedConceptId,
        string expectedName)
    {
        var response = await _client.GetAsync($"/api/concepts?term={Uri.EscapeDataString(term)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var concepts = await response.Content.ReadFromJsonAsync<ConceptResponse[]>();

        concepts.Should().NotBeNull();
        concepts.Should().Contain(concept =>
            concept.ConceptId == expectedConceptId
            && concept.Name == expectedName);
    }

    [Test]
    public async Task SearchConcepts_ReturnsBadRequest_WhenTermIsMissing()
    {
        var response = await _client.GetAsync("/api/concepts");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
