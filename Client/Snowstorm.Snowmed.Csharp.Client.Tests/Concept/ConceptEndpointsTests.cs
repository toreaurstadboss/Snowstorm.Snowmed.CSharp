using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using Snowstorm.Snowmed.Csharp.ApiClient;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Snowstorm.Snowmed.Csharp.Client.Tests;

[TestFixture]
public sealed class ConceptEndpointsTests
{

    private HttpClient _client = null!;
    private SnowstormApiClient _apiClient = null!;
    private WebApplicationFactory<Program> _factory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestContext.Out.WriteLine(
            "Snowstorm must be running before these integration tests. From the Server folder, run \"docker compose up\".");

        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        _apiClient = new SnowstormApiClient(_client);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _apiClient = null!;
    }

    [SetUp]
    public void Setup()
    {
        TestContext.Out.WriteLine(
         """
         Snowstorm - LocalDEV - Integration test
         snowstorm-1    |          ____                          _
         snowstorm-1    |  __/\__ / ___| _ __   _____      _____| |_ ___  _ __ _ __ ___
         snowstorm-1    |  \_\/_/ \___ \| '_ \ / _ \ \ /\ / / __| __/ _ \| '__| '_ ` _ \
         snowstorm-1    |  /_/\_\  ___) | | | | (_) \ V  V /\__ \ || (_) | |  | | | | | |
         snowstorm-1    |    \/   |____/|_| |_|\___/ \_/\_/ |___/\__\___/|_|  |_| |_| |_|
         snowstorm-1    | ===============================================================

         """);
    }

    [Test]
    public async Task GetConceptById_ReturnsMatchingNameAndIdentifier()
    {
        var concept = await _apiClient.GetConceptByIdAsync("73211009");
        concept.Should().BeEquivalentTo(new ConceptResponse("Diabetes mellitus", "73211009"));
    }

    [Test]
    [TestCase("Diabetes")]
    public async Task SearchConcepts_ReturnsResultContainingMatchingNameAndIdentifier(string term)
    {
        var concepts = await _apiClient.SearchConceptsAsync(term);
        concepts.Should().NotBeNullOrEmpty();
        concepts.Should().Contain(concept =>
            concept.ConceptId == "73211009"
            && concept.Name == "Diabetes mellitus");
        TestContext.Out.WriteLine("Found {0} concepts matching term '{1}'", concepts.Count, term);
    }

    [Test]
    public async Task GetConceptById_ReturnsNotFound_WhenConceptDoesNotExist()
    {
        var act = async () => await _apiClient.GetConceptByIdAsync("999999999");
        var exception = await act.Should().ThrowAsync<Snowstorm.Snowmed.Csharp.ApiClient.ApiException>();
        exception.Which.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task GetConceptDescriptions_ReturnsExpected_WhenConceptDoesExist()
    {
        var conceptDescriptions = await _apiClient.GetConceptDescriptionsByIdAsync("73211009");    
        conceptDescriptions.Should().NotBeNull();
        conceptDescriptions.Should().Contain(description =>
            description.ConceptId == "73211009"
            && description.DescriptionId == "813575016"
            && description.Term == "Diabetes mellitus (disorder)"
            && description.Type == "FSN");
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
