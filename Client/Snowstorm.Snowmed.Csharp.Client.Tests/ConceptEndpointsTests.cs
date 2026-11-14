using System.Net;
using System.Net.Http.Json;
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
